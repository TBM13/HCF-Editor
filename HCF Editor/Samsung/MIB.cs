using HCF_Editor.UI.Output;
using System;
using System.Buffers.Binary;

namespace HCF_Editor.Samsung
{
    /// <summary>
    /// Implementation of drivers/net/wireless/scsc/mib.c
    /// </summary>
    internal static class MIB
    {
        public const byte SLSI_MIB_MORE_MASK = 0x80,
                          SLSI_MIB_SIGN_MASK = 0x40,
                          SLSI_MIB_TYPE_MASK = 0x20,
                          SLSI_MIB_LENGTH_MASK = 0x1F;

        public const byte SLSI_MIB_MAX_INDEXES = 2;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8605 // Unboxing a possibly null value.
        public static void Encode(MIBEntry entry, out byte[] encodedOutput)
        {
            int requiredSize = 5 + (5 * SLSI_MIB_MAX_INDEXES) +
                               (entry.Type == MIBValueType.Octet ? ((byte[])entry.Value).Length : 5);

            ushort encodedLength = 4;

            encodedOutput = new byte[requiredSize];
            Span<byte> encodedOutputSpan = new(encodedOutput);

            BinaryPrimitives.WriteUInt16LittleEndian(encodedOutputSpan, entry.Psid);

            for (int i = 0; i < SLSI_MIB_MAX_INDEXES && entry.Index[i] != 0; i++)
                encodedOutput[2] += (byte)EncodeUInt32(encodedOutputSpan[(4 + encodedOutput[2])..], entry.Index[i]);

            encodedLength += encodedOutput[2];

            switch (entry.Type)
            {
                case MIBValueType.UInt:
                    encodedLength += (ushort)EncodeUInt32(encodedOutputSpan[encodedLength..], (uint)entry.Value);
                    break;
                case MIBValueType.Int:
                    encodedLength += (ushort)EncodeInt32(encodedOutputSpan[encodedLength..], (int)entry.Value);
                    break;
                case MIBValueType.Octet:
                    encodedLength += (ushort)EncodeOctetStr(encodedOutputSpan[encodedLength..], (byte[])entry.Value);
                    break;
                case MIBValueType.Bool:
                    encodedLength += (ushort)EncodeUInt32(encodedOutputSpan[encodedLength..], (bool)entry.Value ? 1u : 0u);
                    break;
                case MIBValueType.None:
                    break;
                default:
                    throw new("Invalid Entry Type");
            }

            BinaryPrimitives.WriteUInt16LittleEndian(encodedOutputSpan[2..], (ushort)(encodedLength - 4));

            if (encodedLength % 2 == 1)
            {
                /* Add a padding byte "0x00" to the encoded buffer. The Length
		         * value is NOT updated to account for this pad value. If the
		         * length is an Odd number the Pad values MUST be there if it
		         * is Even it will not be.
		         */

                encodedOutput[encodedLength] = 0x00;
                encodedLength++;
            }
        }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8605 // Unboxing a possibly null value.

        public static int Decode(Span<byte> encodedData, out MIBEntry entry)
        {
            if (encodedData.Length < 4)
                throw new("Encoded Data Length must be greater than 4");

            int decodedLength = 4;
            ushort length = BinaryPrimitives.ReadUInt16LittleEndian(encodedData[2..]);

            if (encodedData.Length < decodedLength + length)
                throw new("Encoded Data Length must be >= than decoded length");

            ushort psid = BinaryPrimitives.ReadUInt16LittleEndian(encodedData);
            entry = new(psid, null);

            int indexCount = 0;
            while (decodedLength < 4 + length)
            {
                MIBValueType type = DecodeTypeLength(encodedData[decodedLength..], out int nextValueLength);

                if (encodedData.Length < decodedLength + nextValueLength)
                    throw new("Encoded Data Length must be >= than decoded length");

                switch (type)
                {
                    case MIBValueType.UInt:
                        decodedLength += DecodeUInt32(encodedData[decodedLength..], out uint u);

                        /* If this is that last value then it is the "unitValue"
			             * if other values follow it is an Index Value
			             */
                        if ((decodedLength < 4 + length) && (indexCount != SLSI_MIB_MAX_INDEXES))
                        {
                            entry.Index[indexCount] = (ushort)u;
                            indexCount++;
                        }
                        else
                        {
                            entry.Value = u;
                            if (decodedLength != 4 + length)
                                OutputViewer.Log("WARN: Decoded length didn't match expected length (4 + length)", OutputEntryType.Warn);
                        }

                        break;

                    case MIBValueType.Int:
                        decodedLength += DecodeInt32(encodedData[decodedLength..], out int i);
                        if (decodedLength != 4 + length)
                            OutputViewer.Log("WARN: Decoded length didn't match expected length (4 + length)", OutputEntryType.Warn);

                        entry.Value = i;
                        break;

                    case MIBValueType.Octet:
                        decodedLength += DecodeOctetStr(encodedData[decodedLength..], out byte[] o);
                        if (decodedLength != 4 + length)
                            OutputViewer.Log("WARN: Decoded length didn't match expected length (4 + length)", OutputEntryType.Warn);

                        entry.Value = o;
                        break;

                    default:
                        throw new("Invalid MIB data type");
                }
            }

            if (length % 2 == 1)
            {
                /* Remove the padding byte "0x00" in the encoded buffer.
                 * The Length value does NOT account for this pad value
                 * If the length is an Odd number the Pad values MUST be
                 * there if it is Even it will not be.
                 */

                if (encodedData[decodedLength] != 0x00)
                    OutputViewer.Log("WARN: Padding not detected", OutputEntryType.Warn);

                length++;
            }

            return 4 + length;
        }

        public static MIBValueType DecodeTypeLength(Span<byte> buffer, out int length)
        {
            length = 1;
            if ((buffer[0] & SLSI_MIB_MORE_MASK) > 0)
                length = buffer[0] & SLSI_MIB_LENGTH_MASK;

            if ((buffer[0] & SLSI_MIB_SIGN_MASK) > 0)
                return MIBValueType.Int;

            if ((buffer[0] & SLSI_MIB_MORE_MASK) > 0 && 
                (buffer[0] & SLSI_MIB_TYPE_MASK) > 0)
            {
                int oldLengthValue = 0;

                for (int i = 0; i < length; i++)
                {
                    oldLengthValue <<= 8;
                    oldLengthValue |= buffer[1 + i];
                }

                length += oldLengthValue;
                return MIBValueType.Octet;
            }

            return MIBValueType.UInt;
        }

        #region Low Level Encode/Decode
        public static int EncodeUInt32(Span<byte> buffer, uint value)
        {
            if (value < 64)
            {
                buffer[0] = (byte)value;
                return 1;
            }

            int writeCount = 0;

            /* Encode the Integer
	         *  0xABFF0055 = [0xAB, 0xFF, 0x00, 0x55]
	         *    0xAB0055 = [0xAB, 0x00, 0x55]
	         *      0xAB55 = [0xAB, 0x55]
	         *        0x55 = [0x55]
	         */

            for (int i = 0; i < 4; i++)
            {
                uint bValue = (value & 0xFF000000) >> 24;

                if (bValue > 0 || writeCount > 0)
                {
                    writeCount++;
                    Span<byte> span = buffer[writeCount..];
                    BinaryPrimitives.WriteUInt32LittleEndian(span, bValue);
                }

                value <<= 8;
            }

            /* vldata Length | more bit */
            buffer[0] = (byte)((byte)writeCount | SLSI_MIB_MORE_MASK);

            return 1 + writeCount;
        }

        public static int EncodeInt32(Span<byte> buffer, int value)
        {
            if ((value & 0x10000000) == 0)
                return EncodeUInt32(buffer, (uint)value);

            if (value >= -64 && value < 0)
            {
                buffer[0] = (byte)(value & 0x7F); /* vldata Length | more bit */
                return 1;
            }

            int writeCount = 0;

            /* Encode the Negative Integer */
            for (int i = 0; i < 4; i++)
            {
                int bValue = (int)((value & 0xFF000000) >> 24);

                if (!((bValue == 0xFF) && ((value & 0x800000) > 0)) || writeCount > 0)
                {
                    writeCount++;
                    Span<byte> span = buffer[writeCount..];
                    BinaryPrimitives.WriteInt32LittleEndian(span, bValue);
                }

                value <<= 8;
            }

            /* vldata Length | more bit | sign bit*/
            buffer[0] = (byte)((byte)writeCount | SLSI_MIB_MORE_MASK | SLSI_MIB_SIGN_MASK);

            return 1 + writeCount;
        }

        public static int EncodeOctetStr(Span<byte> buffer, Span<byte> value)
        {
            /* Encode the Length (Up to 4 bytes 32 bits worth)
             *  0xABFF0000 = [0xAB, 0xFF, 0x00, 0x00]
             *    0xAB0000 = [0xAB, 0x00, 0x00]
             *      0xAB00 = [0xAB, 0x00]
             *        0x00 = [0x00]
             */

            int length = value.Length;
            int writeCount = 0;

            for (int i = 0; i < 3; i++)
            {
                int bValue = (int)((length & 0xFF000000) >> 24);

                if (bValue > 0 || writeCount > 0)
                {
                    writeCount++;
                    Span<byte> span = buffer[writeCount..];
                    BinaryPrimitives.WriteInt32LittleEndian(span, bValue);
                }

                length <<= 8;
            }

            buffer[0] = (byte)((byte)(1 + writeCount) | SLSI_MIB_MORE_MASK | SLSI_MIB_TYPE_MASK);
            buffer[1 + writeCount] = (byte)((byte)value.Length & 0xFF);

            for (int i = 0; i < value.Length; i++)
                buffer[2 + writeCount + i] = value[i];

            return 2 + writeCount + value.Length;
        }

        public static int DecodeUInt32(Span<byte> buffer, out uint output)
        {
            if ((buffer[0] & SLSI_MIB_MORE_MASK) == 0)
            {
                output = (uint)(buffer[0] & 0x7F);
                return 1;
            }

            int length = buffer[0] & SLSI_MIB_LENGTH_MASK;

            uint v = 0;
            for (int i = 0; i < length; i++)
            {
                v <<= 8;
                v |= buffer[1 + i];
            }

            output = v;
            return 1 + length;
        }

        public static int DecodeUInt64(Span<byte> buffer, out ulong output)
        {
            if ((buffer[0] & SLSI_MIB_MORE_MASK) == 0)
            {
                output = (ulong)(buffer[0] & 0x7F);
                return 1;
            }

            int length = buffer[0] & SLSI_MIB_LENGTH_MASK;

            ulong v = 0;
            for (int i = 0; i < length; i++)
            {
                v <<= 8;
                v |= buffer[1 + i];
            }

            output = v;
            return 1 + length;
        }

        public static int DecodeInt32(Span<byte> buffer, out int output)
        {
            if ((buffer[0] & SLSI_MIB_SIGN_MASK) == 0)
            {
                /* just use the Unsigned Decoder */
                int readCount = DecodeUInt32(buffer, out uint u);

                output = (int)u;
                return readCount;
            }

            if ((buffer[0] & SLSI_MIB_MORE_MASK) == 0)
            {
                output = (int)(0xFFFFFF80 | buffer[0]);
                return 1;
            }

            int length = buffer[0] & SLSI_MIB_LENGTH_MASK;

            uint v = 0xFFFFFFFF;
            for (int i = 0; i < length; i++)
            {
                v <<= 8;
                v |= buffer[1 + i];
            }

            output = (int)v;
            return 1 + length;
        }

        public static int DecodeInt64(Span<byte> buffer, out long output)
        {
            if ((buffer[0] & SLSI_MIB_SIGN_MASK) == 0)
            {
                /* just use the Unsigned Decoder */
                int readCount = DecodeUInt64(buffer, out ulong u);

                output = (long)u;
                return readCount;
            }

            if ((buffer[0] & SLSI_MIB_MORE_MASK) == 0)
            {
                output = (long)(0xFFFFFFFFFFFFFF80UL| buffer[0]);
                return 1;
            }

            int length = buffer[0] & SLSI_MIB_LENGTH_MASK;

            ulong v = 0xFFFFFFFFFFFFFFFFUL;
            for (int i = 0; i < length; i++)
            {
                v <<= 8;
                v |= buffer[1 + i];
            }

            output = (long)v; // WARNING: Big values like 999999999 may not be parsed correctly!
            return 1 + length;
        }

        public static int DecodeOctetStr(Span<byte> buffer, Span<byte> output)
        {
            int length = buffer[0] & SLSI_MIB_LENGTH_MASK;
            int decodedLengthValue = 0;

            for (int i = 0; i < length; i++)
            {
                decodedLengthValue <<= 8;
                decodedLengthValue |= buffer[1 + i];
            }

            if (decodedLengthValue > 0)
            {
                if (output.Length < decodedLengthValue)
                    throw new("Out Span is too small");

                for (int i = 0; i < decodedLengthValue; i++)
                    output[i] = buffer[1 + length + i];
            }

            return 1 + length + decodedLengthValue;
        }

        public static int DecodeOctetStr(Span<byte> buffer, out byte[] output)
        {
            int length = buffer[0] & SLSI_MIB_LENGTH_MASK;
            int decodedLengthValue = 0;

            for (int i = 0; i < length; i++)
            {
                decodedLengthValue <<= 8;
                decodedLengthValue |= buffer[1 + i];
            }

            output = new byte[decodedLengthValue];
            return DecodeOctetStr(buffer, output);
        }

        #endregion
    }

    public enum MIBValueType
    {
        Invalid = -1,
        Bool,
        UInt,
        Int,
        Octet,
        None
    }

    public class MIBEntry
    {
        public ushort Psid { get; set; }
        public ushort[] Index { get; } = new ushort[MIB.SLSI_MIB_MAX_INDEXES];
        public object? Value { get; set; }
        public MIBValueType Type 
        { 
            get
            {
                return Value switch
                {
                    bool => MIBValueType.Bool,
                    uint => MIBValueType.UInt,
                    int => MIBValueType.Int,
                    byte[] => MIBValueType.Octet,
                    null => MIBValueType.None,
                    _ => MIBValueType.Invalid,
                };
            }
        }

        public MIBEntry() { }

        public MIBEntry(ushort Psid, object? Value)
        {
            this.Psid = Psid;
            this.Value = Value;
        }
    }
}

using System;
using System.Collections.Generic;

namespace HCF_Editor
{
    internal class HcfFile
    {
        private class Data
        {
            public Span<byte> Bytes => bytes.AsSpan()[ReadPosition..];
            public int Length => bytes.Length - ReadPosition;
            public int ReadPosition { get; private set; }

            private readonly byte[] bytes;

            public Data(byte[] data) => 
                bytes = data;

            public byte Read(int index = 0) =>
                bytes[index + ReadPosition];

            public void Advance(int amount) =>
                ReadPosition += amount;
        }

        public int Hash { get; private set; }

        private readonly Data data;

        public HcfFile(byte[] bytes)
        {
            data = new(bytes);
            ValidateHeader();
        }

        private void ValidateHeader()
        {
            // Validate header
            if (!(data.Length >= 8 && data.Read(7) == 1))
                throw new("Invalid Header");

            // Read Hash
            int MGT_HASH_SIZE_BYTES = 2;
            int MGT_HASH_OFFSET = 4;

            for (int i = 0; i < MGT_HASH_SIZE_BYTES; i++)
                Hash = (Hash << 8) | data.Read(i + MGT_HASH_OFFSET);

            // Discard header
            data.Advance(8);
        }

        public List<MIBEntry> DecodeMibEntries()
        {
            List<MIBEntry> list = new();

            while (data.Length > 0)
            {
                data.Advance(MIB.Decode(data.Bytes, out MIBEntry entry));
                list.Add(entry);
            }

            return list;
        }
    }
}

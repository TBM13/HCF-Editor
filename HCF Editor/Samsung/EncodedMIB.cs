using HCF_Editor.UI.Output;
using System;
using System.Collections.Generic;

namespace HCF_Editor.Samsung
{
    public class EncodedMIB
    {
        private class Data
        {
            public Span<byte> Bytes => bytes.AsSpan()[ReadPosition..];
            public int Length => bytes.Length - ReadPosition;
            public int ReadPosition { get; private set; }

            private readonly byte[] bytes;

            public Data(byte[] data) => 
                bytes = data;

            public void Advance(int amount) =>
                ReadPosition += amount;
        }

        public string FilePath { get; init; }
        public string FileName { get; init; }
        public int Hash { get; private set; }

        private readonly Data data;

        public EncodedMIB(byte[] bytes, string filePath, string fileName)
        {
            data = new(bytes);
            FilePath = filePath;
            FileName = fileName;

            ValidateHeader();
        }

        private void ValidateHeader()
        {
            if (data.Bytes.Length < 8)
            {
                OutputViewer.Log("Encoded MIB file is too small. Aborting", OutputEntryType.Error);
                data.Advance(data.Length);
                return;
            }

            // Validate header
            if (data.Bytes[7] != 1)
            {
                OutputViewer.Log("Encoded MIB file has an invalid Header. Aborting", OutputEntryType.Error);
                data.Advance(data.Length);
                return;
            }

            // Read Hash
            int MGT_HASH_SIZE_BYTES = 2;
            int MGT_HASH_OFFSET = 4;

            for (int i = 0; i < MGT_HASH_SIZE_BYTES; i++)
                Hash = (Hash << 8) | data.Bytes[i + MGT_HASH_OFFSET];

            // Discard header
            data.Advance(8);
        }

        public List<MIBEntry> DecodeMibEntries()
        {
            List<MIBEntry> list = new();

            try
            {
                while (data.Length > 0)
                {
                    data.Advance(MIB.Decode(data.Bytes, out MIBEntry entry));
                    list.Add(entry);
                }
            }
            catch (Exception ex)
            {
                OutputViewer.Log($"MIB Decode: {ex.Message}", OutputEntryType.Error);
            }

            if (data.Length > 0)
                OutputViewer.Log($"{data.Length} bytes weren't read", OutputEntryType.Warn);

            return list;
        }
    }
}

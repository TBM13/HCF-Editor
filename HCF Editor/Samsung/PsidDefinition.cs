namespace HCF_Editor.Samsung
{

    public class PsidDefinition
    {
        public string Name { get; set; }
        public ushort Psid { get; set; }
        public bool? PerInterface { get; set; }
        public PsidDefinitionType? Type { get; set; }
        public string? Units { get; set; }
        public long? Min { get; set; }
        public long? Max { get; set; }
        public object? Default { get; set; }
        public string? Description { get; set; }

        public PsidDefinition(ushort psid, string name)
        {
            Psid = psid;
            Name = name;
        }
    }

    public enum PsidDefinitionType
    {
        UInt8,
        UInt16,
        UInt32,
        UInt64,

        Int8,
        Int16,
        Int32,
        Int64,

        Bool,
    }
}

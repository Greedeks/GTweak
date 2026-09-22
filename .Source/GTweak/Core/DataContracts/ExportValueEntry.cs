namespace GTweak.Core.DataContracts
{
    internal sealed class ExportValueEntry
    {
        public uint Value { get; }

        public ExportValueEntry(uint value)
        {
            Value = value;
        }
    }
}

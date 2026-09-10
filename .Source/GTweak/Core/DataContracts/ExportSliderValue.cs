namespace GTweak.Core.DataContracts
{
    internal sealed class ExportSliderValue
    {
        public uint Value { get; }

        public ExportSliderValue(uint value)
        {
            Value = value;
        }
    }
}

namespace GTweak.Core.DataContracts
{
    internal sealed class ExportToggleValue
    {
        public bool State { get; }

        public ExportToggleValue(bool state)
        {
            State = state;
        }
    }
}

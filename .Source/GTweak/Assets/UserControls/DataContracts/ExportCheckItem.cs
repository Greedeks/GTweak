namespace GTweak.Assets.UserControls.DataContracts
{
    internal sealed class ExportCheckItem
    {
        public string Label { get; }
        public bool IsChecked { get; }

        public ExportCheckItem(string label, bool isChecked)
        {
            Label = label;
            IsChecked = isChecked;
        }
    }
}

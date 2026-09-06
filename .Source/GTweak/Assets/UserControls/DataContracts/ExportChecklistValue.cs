using System.Collections.Generic;

namespace GTweak.Assets.UserControls.DataContracts
{
    internal sealed class ExportChecklistValue
    {
        public IReadOnlyList<ExportCheckItem> Items { get; }

        public ExportChecklistValue(IReadOnlyList<ExportCheckItem> items)
        {
            Items = items;
        }
    }
}

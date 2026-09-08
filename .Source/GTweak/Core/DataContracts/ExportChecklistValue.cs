using System;
using System.Collections.ObjectModel;
using GTweak.Core.DataContracts.Items;

namespace GTweak.Core.DataContracts
{
    internal sealed class ExportChecklistValue
    {
        public ObservableCollection<ExportCheckItem> Items { get; }

        public ExportChecklistValue(ObservableCollection<ExportCheckItem> items, Action onEmpty = null)
        {
            Items = items;

            foreach (ExportCheckItem item in Items)
            {
                item.RemoveCommand = new RelayCommand(_ =>
                {
                    Items.Remove(item);
                    if (Items.Count == 0)
                    {
                        onEmpty?.Invoke();
                    }
                });
            }
        }
    }
}
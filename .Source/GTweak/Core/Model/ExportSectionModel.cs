using System.Collections.ObjectModel;
using GTweak.Core.Base;
using GTweak.Core.DataContracts;
using GTweak.Core.Items;
using GTweak.Modules.Managers;

namespace GTweak.Core.Model
{
    internal sealed class ExportSectionModel : PropertyChangedBase
    {
        public JsonConfigManager.Section Section { get; }
        public ObservableCollection<ExportEntryItem> Items { get; } = new ObservableCollection<ExportEntryItem>();

        public int Count
        {
            get
            {
                int total = 0;

                for (int i = 0; i < Items.Count; i++)
                {
                    object val = Items[i].Value;

                    if (val is ExportChecklistValue checklist)
                    {
                        total += checklist.Items.Count;
                    }
                    else if (val is ExportPackagesValue packages)
                    {
                        total += packages.Packages.Count;
                    }
                    else
                    {
                        total++;
                    }
                }

                return total;
            }
        }

        public bool IsVisible => Items.Count > 0;

        public ExportSectionModel(JsonConfigManager.Section section)
        {
            Section = section;
            Items.CollectionChanged += (_, __) => RefreshState();
        }

        public void RefreshState()
        {
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(nameof(IsVisible));
        }
    }
}

using System;
using System.Collections.ObjectModel;
using GTweak.Core.DataContracts.Items;

namespace GTweak.Core.DataContracts
{
    internal sealed class ExportPackagesValue
    {
        public ObservableCollection<ExportPackageItem> Packages { get; }

        public ExportPackagesValue(ObservableCollection<ExportPackageItem> packages, Action onEmpty = null)
        {
            Packages = packages;

            foreach (ExportPackageItem item in Packages)
            {
                item.RemoveCommand = new RelayCommand(_ =>
                {
                    Packages.Remove(item);
                    if (Packages.Count == 0)
                    {
                        onEmpty?.Invoke();
                    }
                });
            }
        }
    }
}

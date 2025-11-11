using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GTweak.Core.Base
{
    internal interface IBasePageItem
    {
        string Name { get; set; }
        bool State { get; set; }
    }

    internal interface ITypedPageItem<T> : IBasePageItem
    {
        T Value { get; set; }
    }

    internal abstract class ViewModelPageBase<TModel, TTweaksClass> : ViewModelBase
        where TModel : IBasePageItem, new()
        where TTweaksClass : new()
    {
        public ObservableCollection<TModel> Toggles { get; private set; }

        public TModel this[string name] => Toggles.FirstOrDefault(d => d.Name == name);
        protected abstract Dictionary<string, object> GetControlStates();
        protected abstract void Analyze(TTweaksClass tweaks);

        protected ViewModelPageBase()
        {
            TTweaksClass tweaks = new TTweaksClass();
            Analyze(tweaks);
            Toggles = new ObservableCollection<TModel>(GetControlStates().Select(kvp => CreateModelFromState(kvp.Key, kvp.Value)));

            OnPropertyChanged(nameof(Toggles));
        }

        private TModel CreateModelFromState(string name, object parameter)
        {
            TModel model = new TModel { Name = name };

            switch (parameter)
            {
                case bool b:
                    model.State = b;
                    break;
                case double d when model is ITypedPageItem<double> doubleItem:
                    doubleItem.Value = d;
                    break;
                case string s when model is ITypedPageItem<string> stringItem:
                    stringItem.Value = s;
                    break;
                default:
                    break;
            }

            return model;
        }
    }
}

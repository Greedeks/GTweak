using System.Collections.Generic;
using System.Collections.ObjectModel;

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
        private ObservableCollection<TModel> _toggles;

        protected abstract IReadOnlyDictionary<string, object> GetControlStates();
        protected abstract void Analyze(TTweaksClass tweaks);

        public TModel this[string name]
        {
            get
            {
                foreach (TModel toggle in Toggles)
                {
                    if (toggle.Name == name)
                    {
                        return toggle;
                    }
                }
                return default;
            }
        }

        public ObservableCollection<TModel> Toggles
        {
            get
            {
                if (_toggles == null)
                {
                    InitializeData();
                }
                return _toggles;
            }
            set
            {
                _toggles = value;
                OnPropertyChanged(nameof(Toggles));
            }
        }

        protected ViewModelPageBase() { }

        private void InitializeData()
        {
            TTweaksClass tweaks = new TTweaksClass();
            Analyze(tweaks);

            IReadOnlyDictionary<string, object> states = GetControlStates();
            List<TModel> modelsList = new List<TModel>(states.Count);

            foreach (var kvp in states)
            {
                modelsList.Add(CreateModelFromState(kvp.Key, kvp.Value));
            }

            _toggles = new ObservableCollection<TModel>(modelsList);
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

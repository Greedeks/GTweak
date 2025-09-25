using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace GTweak.Core
{
    internal interface IToggleModel
    {
        string Name { get; set; }
        bool State { get; set; }
    }

    internal interface ISliderModel : IToggleModel
    {
        double Value { get; set; }
    }

    internal abstract class BasePageVM<TModel, TTweaksClass> : ViewModelBase
        where TModel : IToggleModel, new()
        where TTweaksClass : new()
    {
        public ObservableCollection<TModel> Toggles { get; private set; }

        public TModel this[string name] => Toggles.FirstOrDefault(d => d.Name == name);

        protected abstract Dictionary<string, object> GetControlStates();

        protected abstract void Analyze(TTweaksClass tweaks);

        protected BasePageVM()
        {
            var tweaks = new TTweaksClass();
            Parallel.Invoke(() => Analyze(tweaks));

            Toggles = new ObservableCollection<TModel>(
                GetControlStates().Select(kvp =>
                {
                    var model = new TModel { Name = kvp.Key };

                    if (kvp.Value is bool b)
                        model.State = b;

                    if (kvp.Value is double d && model is ISliderModel sliderModel)
                        sliderModel.Value = d;

                    return model;
                }));

            OnPropertyChanged(nameof(Toggles));
        }

    }
}

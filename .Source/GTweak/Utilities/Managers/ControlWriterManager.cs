using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GTweak.Utilities.Managers
{
    internal sealed class ControlWriterManager
    {
        private readonly Dictionary<string, object> _controlStates;

        internal GenericCollection<bool> Button { get; }
        internal GenericCollection<bool> Checkbox { get; }
        internal GenericCollection<object> Slider { get; }
        internal GenericCollection<object> ColorPicker { get; }

        internal ControlWriterManager(Dictionary<string, object> controlStates)
        {
            _controlStates = controlStates ?? new Dictionary<string, object>();

            Button = new GenericCollection<bool>(_controlStates, "TglButton");
            Checkbox = new GenericCollection<bool>(_controlStates, "Checkbox");
            Slider = new GenericCollection<object>(_controlStates, "Slider");
            ColorPicker = new GenericCollection<object>(_controlStates, "ColorPicker");
        }
        internal class GenericCollection<T>
        {
            private readonly string _prefix;
            private readonly Dictionary<string, object> _controlStates;
            private readonly ConcurrentDictionary<int, string> _keyCache = new ConcurrentDictionary<int, string>();

            internal GenericCollection(Dictionary<string, object> controlStates, string prefix)
            {
                _controlStates = controlStates;
                _prefix = prefix;
            }

            internal T this[int index]
            {
                set
                {
                    string key = _keyCache.GetOrAdd(index, i => $"{_prefix}{i}");

                    if (_controlStates != null)
                    {
                        _controlStates[key] = value;
                    }
                }
            }
        }
    }
}
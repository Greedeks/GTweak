using System.Collections.Generic;

namespace GTweak.Modules.Managers
{
    internal sealed class ControlWriterManager
    {
        private readonly Dictionary<string, object> _controlStates;

        internal GenericCollection<bool> ToggleButton { get; }
        internal GenericCollection<bool> Checkbox { get; }
        internal GenericCollection<object> Slider { get; }
        internal GenericCollection<object> ColorPicker { get; }

        internal ControlWriterManager(Dictionary<string, object> controlStates)
        {
            _controlStates = controlStates ?? new Dictionary<string, object>();

            ToggleButton = new GenericCollection<bool>(_controlStates, "TglButton");
            Checkbox = new GenericCollection<bool>(_controlStates, "Checkbox");
            Slider = new GenericCollection<object>(_controlStates, "Slider");
            ColorPicker = new GenericCollection<object>(_controlStates, "ColorPicker");
        }

        internal class GenericCollection<T>
        {
            private readonly string _prefix;
            private readonly Dictionary<string, object> _controlStates;
            private readonly string[] _keyCache;

            internal GenericCollection(Dictionary<string, object> controlStates, string prefix, int capacity = 64)
            {
                _controlStates = controlStates;
                _prefix = prefix;
                _keyCache = new string[capacity];
            }

            internal T this[int index]
            {
                set
                {
                    if ((uint)index < (uint)_keyCache.Length)
                    {
                        _keyCache[index] ??= $"{_prefix}{index}";
                        _controlStates[_keyCache[index]] = value;
                    }
                }
            }
        }
    }
}
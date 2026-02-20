using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GTweak.Utilities.Managers
{
    internal sealed class ControlWriterManager
    {
        private readonly Dictionary<string, object> _controlStates;

        internal ButtonCollection Button { get; }
        internal SliderCollection Slider { get; }

        internal ControlWriterManager(Dictionary<string, object> controlStates)
        {
            _controlStates = controlStates ?? new Dictionary<string, object>();
            Button = new ButtonCollection(_controlStates);
            Slider = new SliderCollection(_controlStates);
        }

        internal class ButtonCollection
        {
            private readonly Dictionary<string, object> _controlStates;
            private static readonly ConcurrentDictionary<int, string> KeyCache = new ConcurrentDictionary<int, string>();

            internal ButtonCollection(Dictionary<string, object> controlStates) => _controlStates = controlStates;

            internal bool this[int index]
            {
                set
                {
                    string key = KeyCache.GetOrAdd(index, i => $"TglButton{i}");

                    if (_controlStates != null)
                    {
                        _controlStates[key] = value;
                    }
                }
            }
        }

        internal class SliderCollection
        {
            private readonly Dictionary<string, object> _controlStates;
            private static readonly ConcurrentDictionary<int, string> KeyCache = new ConcurrentDictionary<int, string>();

            internal SliderCollection(Dictionary<string, object> controlStates) => _controlStates = controlStates;

            internal object this[int index]
            {
                set
                {
                    string key = KeyCache.GetOrAdd(index, i => $"Slider{i}");

                    if (_controlStates != null)
                    {
                        _controlStates[key] = value;
                    }
                }
            }
        }
    }
}
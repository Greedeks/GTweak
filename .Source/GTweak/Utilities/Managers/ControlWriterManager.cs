using System.Collections.Generic;

namespace GTweak.Utilities.Managers
{
    internal sealed class ControlWriterManager
    {
        private readonly Dictionary<string, object> _controlStates;

        internal ControlWriterManager(Dictionary<string, object> controlStates)
        {
            _controlStates = controlStates;
        }

        internal ButtonCollection Button => new ButtonCollection(_controlStates);

        internal SliderCollection Slider => new SliderCollection(_controlStates);

        internal class ButtonCollection
        {
            private readonly Dictionary<string, object> _controlStates;

            internal ButtonCollection(Dictionary<string, object> controlStates)
            {
                _controlStates = controlStates;
            }

            internal bool this[int index]
            {
                set => _controlStates[$"TglButton{index}"] = value;
            }
        }

        internal class SliderCollection
        {
            private readonly Dictionary<string, object> _controlStates;

            internal SliderCollection(Dictionary<string, object> controlStates)
            {
                _controlStates = controlStates;
            }

            internal object this[int index]
            {
                set => _controlStates[$"Slider{index}"] = value;
            }
        }
    }
}
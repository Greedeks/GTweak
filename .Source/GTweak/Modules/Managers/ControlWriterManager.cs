using System;
using System.Collections.Generic;

namespace GTweak.Modules.Managers
{
    internal sealed class ControlWriterManager
    {
        private readonly Dictionary<string, object> _controlStates;

        internal ControlWriterManager(Dictionary<string, object> controlStates)
        {
            _controlStates = controlStates ?? new Dictionary<string, object>();
        }

        internal object this[string key]
        {
            set => _controlStates[key] = value;
        }

        internal object this[Enum key]
        {
            set => _controlStates[key.ToString()] = value;
        }
    }
}
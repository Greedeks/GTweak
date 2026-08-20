using GTweak.Core.Interfaces;

namespace GTweak.Core.Model
{
    internal sealed class InterfaceModel : ITypedPageItem<string>
    {
        public string Name { get; set; }
        public bool State { get; set; }
        public string Value { get; set; }
    }
}

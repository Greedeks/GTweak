using GTweak.Core.Base;

namespace GTweak.Core.Model
{
    internal sealed class SystemModel : ITypedPageItem<double>
    {
        public string Name { get; set; }
        public bool State { get; set; }
        public double Value { get; set; }
    }
}

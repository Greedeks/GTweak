namespace GTweak.Core.Interfaces
{
    internal interface IPageItem
    {
        string Name { get; set; }
        bool State { get; set; }
    }

    internal interface ITypedPageItem<T> : IPageItem
    {
        T Value { get; set; }
    }
}

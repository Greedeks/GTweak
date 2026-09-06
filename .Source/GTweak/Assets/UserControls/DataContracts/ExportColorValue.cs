namespace GTweak.Assets.UserControls.DataContracts
{
    internal sealed class ExportColorValue
    {
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }

        public ExportColorValue(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }
    }
}

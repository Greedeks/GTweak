using System.Windows.Media;

namespace GTweak.Assets.UserControl
{
    public partial class ProggressRing
    {
        internal Brush ChangeBackground  { get => Back.Stroke; set { Back.Stroke = value; } }
        internal double ChangeStrokeThickness { get => Back.StrokeThickness; set { Main.StrokeThickness = Back.StrokeThickness = value; } }
        public ProggressRing()
        {
            InitializeComponent();
        }
    }
}

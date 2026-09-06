using System.Windows;
using System.Windows.Controls;

namespace GTweak.Assets.UserControls
{
    public partial class ExportItem : UserControl
    {
        public static readonly DependencyProperty LabelProperty =
              DependencyProperty.Register(nameof(Label), typeof(string), typeof(ExportItem), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(object), typeof(ExportItem), new PropertyMetadata(null));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public object Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public ExportItem()
        {
            InitializeComponent();
        }
    }
}

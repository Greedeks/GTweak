using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GTweak.Assets.UserControls
{
    public partial class ExportItem : UserControl
    {
        public static readonly DependencyProperty LabelProperty =
             DependencyProperty.Register(nameof(Label), typeof(string), typeof(ExportItem), new PropertyMetadata(null));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(object), typeof(ExportItem), new PropertyMetadata(null));

        public static readonly DependencyProperty RemoveCommandProperty =
            DependencyProperty.Register(nameof(RemoveCommand), typeof(ICommand), typeof(ExportItem), new PropertyMetadata(null));

        public static readonly DependencyProperty RemoveCommandParameterProperty =
            DependencyProperty.Register(nameof(RemoveCommandParameter), typeof(object), typeof(ExportItem), new PropertyMetadata(null));

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

        public ICommand RemoveCommand
        {
            get => (ICommand)GetValue(RemoveCommandProperty);
            set => SetValue(RemoveCommandProperty, value);
        }

        public object RemoveCommandParameter
        {
            get => GetValue(RemoveCommandParameterProperty);
            set => SetValue(RemoveCommandParameterProperty, value);
        }

        public ExportItem()
        {
            InitializeComponent();
        }
    }
}
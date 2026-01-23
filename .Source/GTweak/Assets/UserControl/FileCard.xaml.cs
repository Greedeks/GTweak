using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using GTweak.Utilities.Animation;

namespace GTweak.Assets.UserControl
{
    public partial class FileCard
    {
        public static readonly DependencyProperty FileNameProperty = DependencyProperty.Register(nameof(FileName), typeof(string), typeof(FileCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IconSourceProperty = DependencyProperty.Register(nameof(IconSource), typeof(ImageSource), typeof(FileCard), new PropertyMetadata(null));

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(FileCard), new PropertyMetadata(null));

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(FileCard), new PropertyMetadata(null));

        public string FileName
        {
            get => (string)GetValue(FileNameProperty);
            set => SetValue(FileNameProperty, value);
        }

        public ImageSource IconSource
        {
            get => (ImageSource)GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public FileCard()
        {
            InitializeComponent();
        }

        private void Card_MouseEnter(object sender, MouseEventArgs e) => CardBody.BeginAnimation(OpacityProperty, FactoryAnimation.CreateIn(1.0, 0.7, 0.2));


        private void Card_MouseLeave(object sender, MouseEventArgs e) => CardBody.BeginAnimation(OpacityProperty, FactoryAnimation.CreateIn(0.7, 1.0, 0.2));



        private void Card_MouseClick(object sender, MouseButtonEventArgs e)
        {
            ProgressBar.Visibility = Visibility.Visible;

            if (Command != null && Command.CanExecute(CommandParameter))
            {
                Command.Execute(CommandParameter);
            }

            ProgressBar.BeginAnimation(RangeBase.ValueProperty, FactoryAnimation.CreateIn(0, 100, 0.2, () => { ProgressBar.Visibility = Visibility.Collapsed; }, reverse: true));
        }

    }
}
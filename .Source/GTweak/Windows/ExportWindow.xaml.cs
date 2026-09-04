using System.Windows.Input;
using Wpf.Ui.Controls;

namespace GTweak.Windows
{
    public partial class ExportWindow : FluentWindow
    {
        public ExportWindow()
        {
            InitializeComponent();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e?.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void ButtonClose_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => Close();
    }
}

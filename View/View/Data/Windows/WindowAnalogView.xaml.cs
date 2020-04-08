using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Controller.Data.Channels;
using Controller.Data.Windows;
using CustomElements.EditableLabel;
using CustomElements.SizeSavedWindow;

namespace View.Data.Windows
{
    /// <summary>
    /// Interaction logic for WindowAnalog.xaml
    /// </summary>
    public partial class WindowAnalogView : Window
    {
        #region Disable Minimize Button
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;
        private const int WS_MINIMIZE = -131073;

        public WindowAnalogView()
        {
            InitializeComponent();
            this.SourceInitialized += (s, e) =>
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                var value = GetWindowLong(hwnd, GWL_STYLE);
                SetWindowLong(hwnd, GWL_STYLE, (int)(value & WS_MINIMIZE));

            };
        }
        #endregion

        private readonly WindowBasicController _controller;

        public WindowAnalogView(WindowBasicController controller)
            :this()
        {
            _controller = controller;
            DataContext = _controller;
            InitializeComponent();
            SizeSavedWindow.addToSizeSavedWindows(this);
        }

        private void Label_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("!!!!!!!" + ((EditableLabel)sender).Focus() + "#####");
            var realSender = (Label)sender;
            var textBox = (TextBox)realSender.Template.FindName("textBox", realSender);
            textBox.Opacity = 1;
            textBox.IsEnabled = true;
        }

        private void Label_LostFocus_1(object sender, RoutedEventArgs e)
        {
            var realSender = (Label)sender;
            var textBox = (TextBox)realSender.Template.FindName("textBox", realSender);
            textBox.IsEnabled = false;
            textBox.Opacity = 0;
        }

        private void AddItem(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                //Console.WriteLine(sender.GetType());
                var realSender = (Border)sender;
                var dataContext = (ChannelBasicController)realSender.DataContext;
                dataContext.AddStep();
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

    }
}
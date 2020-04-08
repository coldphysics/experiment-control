using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Controller.Data.Channels;
using Controller.Data.Windows;
using CustomElements.SizeSavedWindow;

namespace View.Data.Windows
{
    /// <summary>
    /// Interaction logic for WindowDigitalView.xaml
    /// </summary>
    public partial class WindowDigitalView : Window
    {
        #region Disable Minimize Button
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;
        private const int WS_MINIMIZE = -131073;

        public WindowDigitalView()
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
        public WindowDigitalView(WindowBasicController controller)
            :this()
        {
            _controller = controller;
            DataContext = _controller;
            InitializeComponent();
            SizeSavedWindow.addToSizeSavedWindows(this);
        }
        private void AddItem(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2 )
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
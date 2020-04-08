using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Controller
{
    /// <summary>
    /// Custom Window that has no minimize button.
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    public partial class  CustomWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;

        private const int WS_MAXIMIZEBOX = 0x10000; //maximize button
        private const int WS_MINIMIZEBOX = 0x20000; //minimize button

        public CustomWindow()
        {
            //InitializeComponent();
            this.SourceInitialized += MainWindow_SourceInitialized;
        }

        private IntPtr _windowHandle;
        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            _windowHandle = new WindowInteropHelper(this).Handle;

            //disable minimize button
            DisableMinimizeButton();
        }

        public void DisableMinimizeButton()
        {
            if (_windowHandle == IntPtr.Zero)
                throw new InvalidOperationException("The window has not yet been completely initialized");

            SetWindowLong(_windowHandle, GWL_STYLE, GetWindowLong(_windowHandle, GWL_STYLE) & ~WS_MINIMIZEBOX);
        }
    }
}


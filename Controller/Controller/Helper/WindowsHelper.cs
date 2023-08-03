using CustomElements.SizeSavedWindow;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace Controller.Helper
{
    public class WindowsHelper
    {
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);


        //RECO create a common type for view models and pass it here
        /// <summary>
        /// Creates a window to host a View Model. This allows the View Models (controller) to be unaware of the specific representation they will have.
        /// The mapping between the VM and the View happens elsewhere.
        /// </summary>
        /// <typeparam name="T">The specific type of window to be instantiated (PLEASE only use with general purpose window types)</typeparam>
        /// <param name="viewModel">The view model that you want to host</param>
        /// <param name="sizeToContent">Whether the window should be sized to its contents</param>
        /// <param name="isFixedSize">Whether the window can be resized or not (if not, no minimize or maximize buttons will appear)</param>
        /// <param name="persistSizeAndPosition">Whether to persist the size and location of this window for future runs of the applicaiton</param>
        /// <param name="disableMinimizeButton">Whether to disable the resize button (this does not affect the sizability of the window)</param>
        /// <returns>The window object with the desired content inside it (do not forget to give it a title!)</returns>
        public static Window CreateWindowToHostViewModel<T>(
            object viewModel,
            bool sizeToContent,
            bool isFixedSize,
            bool persistSizeAndPosition,
            bool disableMinimizeButton,
            DataTemplate contentTemplate) where T : Window, new()
        {
            ContentControl contentUI = new ContentControl();
            if(contentTemplate != null)
            {
                contentUI.ContentTemplate = contentTemplate;
            }
            contentUI.Content = viewModel;
            DockPanel dockPanel = new DockPanel();
            dockPanel.Children.Add(contentUI);
            T hostWindow = new T
            {
                Content = dockPanel
            };

            InitializeWindow(hostWindow, persistSizeAndPosition, disableMinimizeButton);

            if (sizeToContent)
            {
                hostWindow.SizeToContent = SizeToContent.WidthAndHeight;
            }

            if (isFixedSize)
            {
                hostWindow.ResizeMode = ResizeMode.NoResize;
            }

            return hostWindow;
        }

        public static Window CreateWindowToHostViewModel(
            object viewModel,
            bool sizeToContent,
            bool isFixedSize = false,
            bool persistSizeAndPosition = false,
            bool disableMinimizeButton = false,
            DataTemplate contentTemplate = null)
        {
            return CreateWindowToHostViewModel<Window>(viewModel, sizeToContent, isFixedSize, persistSizeAndPosition, disableMinimizeButton, contentTemplate);
        }

        // ********* Helper Methods ************
        // *************************************
        private static void InitializeWindow(Window window, bool persistSizeAndPosition, bool disableMinimizeButton)
        {
            if (disableMinimizeButton)
            {
                window.SourceInitialized += OnSourceInitialized;
            }

            if (persistSizeAndPosition)
            {
                SizeSavedWindowManager.AddToSizeSavedWindows(window);
            }
        }

        private static void OnSourceInitialized(object sender, EventArgs e)
        {
            IntPtr _windowHandle = new WindowInteropHelper((Window)sender).Handle;

            //disable minimize button
            DisableMinimizeButton(_windowHandle);
        }

        private static void DisableMinimizeButton(IntPtr _windowHandle)
        {
            const int GWL_STYLE = -16;
            const int WS_MINIMIZEBOX = 0x20000; //minimize button

            if (_windowHandle == IntPtr.Zero)
                throw new InvalidOperationException("The window has not yet been completely initialized");

            SetWindowLong(_windowHandle, GWL_STYLE, GetWindowLong(_windowHandle, GWL_STYLE) & ~WS_MINIMIZEBOX);
        }
    }
}

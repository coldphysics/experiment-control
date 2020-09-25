using System.Windows;
using System.Windows.Controls;

namespace Controller.Helper
{
    public class WindowsHelper
    {
        //RECO create a common type for view models and pass it here
        /// <summary>
        /// Creates a window to host a View Model. This allows the View Models (controller) to be unaware of the specific representation they will have.
        /// The mapping between the VM and the View happens elsewhere.
        /// </summary>
        /// <typeparam name="T">The specific type of window to be instantiated (PLEASE only use with general purpose window types)</typeparam>
        /// <param name="viewModel">The view model that you want to host</param>
        /// <param name="sizeToContent">Whether the window should be sized to its contents</param>
        /// <param name="isFixedSize">Whether the window can be resized or not (if not no minize or maximize buttons will appear)</param>
        /// <returns>The window object with the desired content inside it (do not forget to give it a title!)</returns>
        public static Window CreateWindowToHostViewModel<T>(object viewModel, bool sizeToContent, bool isFixedSize = false) where T : Window, new()
        {
            ContentControl contentUI = new ContentControl();
            contentUI.Content = viewModel;
            DockPanel dockPanel = new DockPanel();
            dockPanel.Children.Add(contentUI);
            T hostWindow = new T
            {
                Content = dockPanel
            };

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

        public static Window CreateWindowToHostViewModel(object viewModel, bool sizeToContent, bool isFixedSize = false)
        {
            return CreateWindowToHostViewModel<Window>(viewModel, sizeToContent, isFixedSize);
        }
    }
}

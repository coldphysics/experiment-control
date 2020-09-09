using System.Windows;
using System.Windows.Controls;

namespace Controller
{
    public class WindowsHelper
    {
        //RECO create a common type for view models and pass it here
        public static Window CreateWindowToHostViewModel(object viewModel, bool sizeToContent, bool isFixedSize=false)
        {
            ContentControl contentUI = new ContentControl();
            contentUI.Content = viewModel;
            //contentUI.ClipToBounds = true;
            DockPanel dockPanel = new DockPanel();
            dockPanel.Children.Add(contentUI);
            Window hostWindow = new Window();
            hostWindow.Content = dockPanel;

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

        public static CustomWindow CreateCustomWindowToHostViewModel(object viewModel, bool sizeToContent)
        {
            ContentControl contentUI = new ContentControl();
            contentUI.Content = viewModel;
            //contentUI.ClipToBounds = true;
            DockPanel dockPanel = new DockPanel();
            dockPanel.Children.Add(contentUI);
            CustomWindow hostWindow = new CustomWindow();
            // hostWindow.DisableMinimizeButton();
            hostWindow.Content = dockPanel;

            if (sizeToContent)
                hostWindow.SizeToContent = SizeToContent.WidthAndHeight;

            return hostWindow;
        }
    }
}

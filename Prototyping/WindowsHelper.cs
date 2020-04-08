using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Prototyping
{
    class WindowsHelper
    {
        //RECO create a common type for view models and pass it here
        public static Window CreateWindowHostingUserControl(object viewModel, bool sizeToContent)
        {
            ContentControl contentUI = new ContentControl();
            contentUI.Content = viewModel;
            //contentUI.ClipToBounds = true;
            DockPanel dockPanel = new DockPanel();
            dockPanel.Children.Add(contentUI);
            Window hostWindow = new Window();
            hostWindow.Content = dockPanel;

            if (sizeToContent)
                hostWindow.SizeToContent = SizeToContent.WidthAndHeight;

            return hostWindow;
        }
    }
}

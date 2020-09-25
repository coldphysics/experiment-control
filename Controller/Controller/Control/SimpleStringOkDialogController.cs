using Communication.Commands;
using Controller.Common;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Controller.Control
{
    public class SimpleStringOkDialogController : SimpleMessageController
    {
        public RelayCommand OkClickedCommand { private set; get; }   

        public SimpleStringOkDialogController(string message)
            :base(message)
        {
            OkClickedCommand = new RelayCommand(CloseWindow);
        }

        private void CloseWindow(object uc)
        {
            if (uc != null)
            {
                Window w = Window.GetWindow((UserControl)uc);
                w.Close();
            }
        }

    }
}

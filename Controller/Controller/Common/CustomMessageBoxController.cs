using Communication.Commands;
using Controller.MainWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Controller.Common
{
    public class CustomMessageBoxController : ChildController
    {
        private ICommand okClicked;

        public bool DontShowAgain { set; get; }

        public string Message { set; get; }

        public RelayCommand OkClicked
        {
            get
            {
                if (okClicked == null)
                {
                    okClicked = new RelayCommand((parameter) => Close(parameter));
                }

                return okClicked;
            }
        }

        public CustomMessageBoxController(BaseController parent) : base(parent)
        {
        }

        private void Close(object parameter)
        {
            UserControl uc = (UserControl)parameter;
            Window w = Window.GetWindow(uc);
            w.Close();
        }
    }
}

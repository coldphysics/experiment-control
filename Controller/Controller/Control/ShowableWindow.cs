using System.Windows;
using System.Windows.Input;
using Communication.Commands;

namespace Controller.Control
{
    public class ShowableWindow
    {
        public Window window;
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        private string _Name;

        public ICommand SelectWindow { get; private set; }

        public ShowableWindow()
        {
            SelectWindow = new RelayCommand(DoSelectWindow);
        }
        public void DoSelectWindow(object parameter)
        {
            if (window.IsVisible)
            {
                window.Activate();
            }
            else
            {
                window.Show();
            }
        }
    }
}

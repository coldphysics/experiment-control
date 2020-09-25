using Controller.Control.Compare;

using System.ComponentModel;


namespace Controller.Common
{
    public class SimpleMessageController:ICompareItemController, INotifyPropertyChanged
    {
        private string _message;

        public string Message
        {
            get
            {
                return _message;
            }

            set
            {
                _message = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Message"));
            
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public SimpleMessageController(string message)
        {
            Message = message;
        }
    }
}

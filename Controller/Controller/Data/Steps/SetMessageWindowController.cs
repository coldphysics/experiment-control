using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Communication.Commands;

namespace Controller.Data.Steps
{
    public class SetMessageWindowController : INotifyPropertyChanged
    {

        private Tuple<bool, string> oldMessageState;
        private string message;
        private RelayCommand clearText;
        private RelayCommand abort;
        private RelayCommand setAsNormal;
        private RelayCommand setAsCritical;

        public RelayCommand ClearText
        {
            get
            {
                if (clearText == null)
                {
                    clearText = new RelayCommand(_ => Message = "");
                }

                return clearText;
            }
            set { clearText = value; }
        }


        public RelayCommand Abort
        {
            get
            {
                if (abort == null)
                {
                    Message = oldMessageState.Item2;
                    CrirticalState = oldMessageState.Item1;
                    abort = new RelayCommand(window => CloseWindow(false, window));
                }

                return abort;
            }
            set { abort = value; }
        }


        public RelayCommand SetAsNormal
        {
            get
            {
                if (setAsNormal == null)
                {
                    setAsNormal = new RelayCommand(window => ChangeCriticality(false, window));
                }
                return setAsNormal;
            }
            set { setAsNormal = value; }
        }


        public RelayCommand SetAsCritical
        {
            get
            {
                if (setAsCritical == null)
                {
                    setAsCritical = new RelayCommand(window => ChangeCriticality(true, window));
                }
                return setAsCritical;
            }
            set { setAsCritical = value; }
        }
        public string Message
        {
            set
            {
                this.message = value;
                OnPropertyChanged("Message");
            }
            get
            {
                return message;
            }
        }

        public bool CrirticalState
        {
            set;
            get;
        }

        public SetMessageWindowController(Tuple<bool, string> oldMessageState)
        {
            this.oldMessageState = oldMessageState;
            message = oldMessageState.Item2;
            CrirticalState = oldMessageState.Item1;
        }

        private void ChangeCriticality(bool newLevel, object window)
        {
            CrirticalState = newLevel;
            CloseWindow(true, window);
        }

        private void CloseWindow(bool dialogResult, object uc)
        {
            UserControl u = (UserControl)uc;
           
            Window w = Window.GetWindow(u);
            w.DialogResult = dialogResult;
            w.Close();
        }
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}

using Communication.Commands;
using Controller.MainWindow;
using Errors.Error;
using Errors.Error.ErrorItems;
using System;
using System.Windows.Input;

namespace Controller.Error.ErrorItems
{
    public abstract class AbstractErrorItemController : ChildController
    {
        private AbstractErrorItem _errorItem;

        public AbstractErrorItem ErrorItem
        {
            set
            {
                _errorItem = value;
                // if we change the whole error item, we notify the change of all properties
                OnPropertyChanged(string.Empty);
            }
            get
            {
                return _errorItem;
            }
        }

        public virtual  bool IsExpanded
        {
            get
            {
                return ((ErrorsWindowController)parent).GetOpenedState(Category);
            }
        }

        public string Message
        {
            set
            {
                ErrorItem.Message = value;
                OnPropertyChanged("Message");
            }

            get
            {
                return ErrorItem.Message;
            }
        }

        public ErrorCategory Category
        {
            set
            {
                ErrorItem.ErrorCategory = value;
                OnPropertyChanged("Category");
            }

            get
            {
                return ErrorItem.ErrorCategory;
            }
        }

        public ICommand DeleteThisErrorCommand { set; get; }

        public AbstractErrorItemController(ErrorsWindowController errorController, AbstractErrorItem errorItem)
            : base(errorController)
        {
            DeleteThisErrorCommand = new RelayCommand(DeleteThisErrorClicked);
            ErrorItem = errorItem;
        }

        protected abstract void DeleteThisErrorClicked(object parameter);
    }
}

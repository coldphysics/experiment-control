using Communication.Commands;
using Controller.MainWindow;
using Errors.Error;
using Errors.Error.ErrorItems;
using System;
using System.Windows.Input;

namespace Controller.Error.ErrorItems
{
    /// <summary>
    /// The parent class for all error item controllers
    /// </summary>
    public abstract class AbstractErrorItemController : ChildController
    {
        /// <summary>
        /// The underlying error item
        /// </summary>
        private AbstractErrorItem _errorItem;

        /// <summary>
        /// Provides access to the underlying error item. Changing the error item triggers property changes for all exposed properties
        /// </summary>
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

        /// <summary>
        /// Indicates whether the category this item belongs to is opened (expanded) or not
        /// </summary>
        public virtual  bool IsExpanded
        {
            get
            {
                return ((ErrorsWindowController)parent).GetOpenedState(Category);
            }
        }

        /// <summary>
        ///  The error messag, or the error header title
        /// </summary>
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

        /// <summary>
        /// The category this error item belongs to.
        /// </summary>
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

        /// <summary>
        /// A command to handle the event that the user tries to delete a specific error item
        /// </summary>
        public ICommand DeleteThisErrorCommand { set; get; }

        /// <summary>
        /// Creates a new instance of this class 
        /// </summary>
        /// <param name="errorController">The parent controller</param>
        /// <param name="errorItem">The underlying error item.</param>
        public AbstractErrorItemController(ErrorsWindowController errorController, AbstractErrorItem errorItem)
            : base(errorController)
        {
            DeleteThisErrorCommand = new RelayCommand(DeleteThisErrorClicked);
            ErrorItem = errorItem;
        }

        /// <summary>
        /// Handles the event of a user tries to delete a specific error item
        /// </summary>
        /// <param name="parameter">not used</param>
        protected abstract void DeleteThisErrorClicked(object parameter);
    }
}

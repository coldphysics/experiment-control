
using Communication.Commands;
using Errors.Error;
using Errors.Error.ErrorItems;
using System.Windows.Input;

namespace Controller.Error.ErrorItems
{
    public class ErrorHeaderController : AbstractErrorItemController
    {
        /// <summary>
        /// A command to handle the button click to toggle the visibility of an error category
        /// </summary>
        public ICommand ToggleCategoryCommand { set; get; }

        /// <summary>
        /// A command to handle the button click to clear the errors of an error category
        /// </summary>
        public ICommand ClearCategoryCommand { set; get; }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="errorController">The parent controller</param>
        /// <param name="errorItem">The underlying <see cref="ErrorHeader"/> </param>
        public ErrorHeaderController(ErrorsWindowController errorController, ErrorHeader errorItem) : base(errorController, errorItem)
        {
            ToggleCategoryCommand = new RelayCommand((parameter) => {
                if (IsExpanded)
                {
                    CloseCategoryClick();
                }
                else
                {
                    OpenCategoryClick();
                }
            });

            ClearCategoryCommand = new RelayCommand(ClearCategoryClick);
        }

        /// <param name="parameter"></param>
        private void ClearCategoryClick(object parameter)
        {
            ErrorCollector.Instance.RemoveErrorsOfWindow(ErrorItem.ErrorCategory);
        }

        private void OpenCategoryClick()
        {
            ((ErrorsWindowController)parent).OpenCategory(ErrorItem.ErrorCategory);
        }

        private void CloseCategoryClick()
        {
            ((ErrorsWindowController)parent).CloseCategory(ErrorItem.ErrorCategory);
        }

        protected override void DeleteThisErrorClicked(object parameter)
        {
            ErrorCollector.Instance.RemoveErrorsOfWindowEvenStayOnDelete(ErrorItem.ErrorCategory);
        }
    }
}

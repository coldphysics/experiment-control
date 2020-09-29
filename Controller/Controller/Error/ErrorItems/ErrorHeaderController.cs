
using Communication.Commands;
using Errors.Error;
using Errors.Error.ErrorItems;
using System.Windows.Input;

namespace Controller.Error.ErrorItems
{
    public class ErrorHeaderController : AbstractErrorItemController
    {
        public bool IsExpanded
        {
            set
            {
                ((ErrorsWindowController)parent).SetOpenedState(Category, value);
                // notification that the property has chaged happens through the parent (no need to worry about it here)
            }

            get
            {
                return ((ErrorsWindowController)parent).GetOpenedState(Category);
            }
        }
        public ICommand ToggleCategoryCommand { set; get; }
        public ICommand ClearCategoryCommand { set; get; }

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

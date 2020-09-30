using Communication.Interfaces.Controller;
using Controller.Error.ErrorItems;
using Controller.MainWindow;
using Errors.Error;
using Errors.Error.ErrorItems;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Controller.Error
{
    public class ErrorsWindowController : ChildController, IWindowController
    {
        private ObservableCollection<AbstractErrorItemController> errors = new ObservableCollection<AbstractErrorItemController>();
        private readonly Dictionary<ErrorCategory, bool> openedState = new Dictionary<ErrorCategory, bool>();

        public ObservableCollection<AbstractErrorItemController> Errors
        {
            set
            {
                errors = value;
                OnPropertyChanged("Errors");
            }

            get
            {
                return errors;
            }
        }

        public ErrorsWindowController(BaseController parent) : base(parent)
        {
            // We start with all categories opened
            foreach (ErrorCategory category in Enum.GetValues(typeof(ErrorCategory)).Cast<ErrorCategory>())
            {
                openedState.Add(category, true);
            }

            ErrorCollector.Instance.PropertyChanged += ErrorCollector_PropertyChanged;
        }

        private void ErrorCollector_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Errors")
            {
                UpdateErrorList();
            }
        }

        public void OpenCategory(ErrorCategory category)
        {
            SetOpenedState(category, true);
        }

        public void CloseCategory(ErrorCategory category)
        {
            SetOpenedState(category, false);
        }

        private void SetOpenedState(ErrorCategory category, bool state)
        {
            if (openedState[category] != state)
            {
                openedState[category] = state;
                List<AbstractErrorItemController> controllers = Errors
                    .Where(item => item.Category == category)
                    .ToList();

                foreach(AbstractErrorItemController controller in controllers)
                    controller.NotifyPropertyChanged("IsExpanded");
            }
        }

        public bool GetOpenedState(ErrorCategory category)
        {
            return openedState[category];
        }

        private static string GetCategoryName(ErrorCategory category)
        {
            switch (category)
            {
                case ErrorCategory.Basic:
                    return "Basic";
                case ErrorCategory.MainHardware:
                    return "Main Hardware";
                case ErrorCategory.Messages:
                    return "Messages";
                case ErrorCategory.Pulseblaster:
                    return "Pulseblaster";
                case ErrorCategory.Python:
                    return "Python / External";
                case ErrorCategory.Variables:
                    return "Variables";
            }

            return "Unknown";
        }

        private void UpdateErrorList()
        {
            List<ConcreteErrorItem> errorsCopy = ErrorCollector.Instance.GetErrorsSnapshot();
            var currentErrors = errors
                .Where(e => e is ConcreteErrorItemController)
                .Cast<ConcreteErrorItemController>()
                .Select(e => e.ConcreteErrorItem).ToList();

            // this includes the same error with a new date/GC...
            int newErrorsCount = errorsCopy.Except(currentErrors).Count();
            int deletedErrorsCount = currentErrors.Except(errorsCopy).Count();

            // only update views if there are some changes in errors
            if (newErrorsCount > 0 || deletedErrorsCount > 0)
            {
                List<AbstractErrorItemController> _sortedList = new List<AbstractErrorItemController>();

                CreateErrorListForSingleCategory(errorsCopy, _sortedList, ErrorCategory.Basic);
                CreateErrorListForSingleCategory(errorsCopy, _sortedList, ErrorCategory.MainHardware);
                CreateErrorListForSingleCategory(errorsCopy, _sortedList, ErrorCategory.Pulseblaster);
                CreateErrorListForSingleCategory(errorsCopy, _sortedList, ErrorCategory.Variables);
                CreateErrorListForSingleCategory(errorsCopy, _sortedList, ErrorCategory.Python);
                Errors = new ObservableCollection<AbstractErrorItemController>(_sortedList);
            }

            if (errorsCopy.Count != 0)
            {
                // only blink again if we have new errors!
                if (newErrorsCount > 0)
                    BlinkManager.GetInstance().BlinkErrorAsync();
            }
            else
            {
                BlinkManager.GetInstance().StopBlinkingAsync();
            }
        }

        private void CreateErrorListForSingleCategory(List<ConcreteErrorItem> newErrors, List<AbstractErrorItemController> _sortedList, ErrorCategory category)
        {
            bool isHeaderAdded = false;
            ErrorHeader header;
            ErrorHeaderController headerController;
            ConcreteErrorItemController itemController;

            for (int i = 0; i < newErrors.Count(); i++)
            {
                if (newErrors[i].ErrorCategory == category)
                {
                    if (!isHeaderAdded)
                    {
                        header = new ErrorHeader();
                        header.Message = "--- " + GetCategoryName(category) + " ---";
                        header.ErrorCategory = category;
                        headerController = new ErrorHeaderController(this, header);
                        _sortedList.Add(headerController);
                        isHeaderAdded = true;
                    }
                    if (openedState[category])
                    {
                        itemController = new ConcreteErrorItemController(this, newErrors[i]);
                        _sortedList.Add(itemController);
                    }
                }
            }
        }
    }
}

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
            openedState[category] = true;
            UpdateErrorList();
        }

        public void CloseCategory(ErrorCategory category)
        {
            openedState[category] = false;
            UpdateErrorList();
        }

        public void SetOpenedState(ErrorCategory category, bool state)
        {
            if (openedState[category] != state)
            {
                openedState[category] = state;
                ErrorHeaderController controller = Errors
                    .Where(item => item is ErrorHeaderController && ((ErrorHeaderController)item).Category == category)
                    .Cast<ErrorHeaderController>()
                    .FirstOrDefault(null);
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
            List<AbstractErrorItemController> _sortedList = new List<AbstractErrorItemController>();

            CreateErrorListForSingleCategory(errorsCopy, _sortedList, ErrorCategory.Basic);
            CreateErrorListForSingleCategory(errorsCopy, _sortedList, ErrorCategory.MainHardware);
            CreateErrorListForSingleCategory(errorsCopy, _sortedList, ErrorCategory.Pulseblaster);
            CreateErrorListForSingleCategory(errorsCopy, _sortedList, ErrorCategory.Variables);
            CreateErrorListForSingleCategory(errorsCopy, _sortedList, ErrorCategory.Python);

            if (_sortedList.Count != 0)
            {
                BlinkManager.GetInstance().BlinkErrorAsync();
            }
            else
            {
                BlinkManager.GetInstance().StopBlinkingAsync();
            }


            Errors = new ObservableCollection<AbstractErrorItemController>(_sortedList);
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

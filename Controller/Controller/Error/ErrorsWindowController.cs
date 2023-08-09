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
    /// <summary>
    /// A controller for the ErrorWindowView
    /// </summary>
    public class ErrorsWindowController : ChildController, IWindowController
    {
        /// <summary>
        /// A list of all child <see cref="AbstractErrorItemController"/>s (both headers and concrete items)
        /// </summary>
        private ObservableCollection<AbstractErrorItemController> errors = new ObservableCollection<AbstractErrorItemController>();
        /// <summary>
        /// The opened state for every error category
        /// </summary>
        private readonly Dictionary<ErrorCategory, bool> openedState = new Dictionary<ErrorCategory, bool>();

        /// <summary>
        /// This event is fired when the user should be made aware of an error in the error window.
        /// </summary>
        public event EventHandler IndicateErrorOnTaskbarEvent;
        /// <summary>
        /// This event is fired when the user should not be made aware of an error in the error window anymore.
        /// </summary>
        public event EventHandler StopIndicatingErrorOnTaskbarEvent;

        /// <summary>
        /// Provides access for the errors list
        /// </summary>
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

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="parent">The parent controller (most likely the <see cref="MainWindowController"/>)</param>
        public ErrorsWindowController(BaseController parent) : base(parent)
        {
            // We start with all categories opened
            foreach (ErrorCategory category in Enum.GetValues(typeof(ErrorCategory)).Cast<ErrorCategory>())
            {
                openedState.Add(category, true);
            }

            ErrorCollector.Instance.PropertyChanged += ErrorCollector_PropertyChanged;
        }

        /// <summary>
        /// Handles the event emitted by the underlying <see cref="ErrorCollector"/> indicating that the list of errors has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ErrorCollector_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Errors")
            {
                UpdateErrorList();
            }
        }

        /// <summary>
        /// Expands a specific error category
        /// </summary>
        /// <param name="category"></param>
        public void OpenCategory(ErrorCategory category)
        {
            SetOpenedState(category, true);
        }

        /// <summary>
        /// Collapses a specific error category
        /// </summary>
        /// <param name="category"></param>
        public void CloseCategory(ErrorCategory category)
        {
            SetOpenedState(category, false);
        }

        /// <summary>
        /// Handles changing the opened state of a given category with the help of the openedState dictionary
        /// </summary>
        /// <param name="category">The category to operate on</param>
        /// <param name="state">The new state</param>
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

        /// <summary>
        /// Gets the current opened state for a given category
        /// </summary>
        /// <param name="category">The category</param>
        /// <returns>true if the category is opened; otherwise, false</returns>
        public bool GetOpenedState(ErrorCategory category)
        {
            return openedState[category];
        }

        /// <summary>
        /// Maps a <see cref="ErrorCategory"/> to a human-friendly name
        /// </summary>
        /// <param name="category">The category to map</param>
        /// <returns>The resulting human-friendly name</returns>
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

        /// <summary>
        /// Handles the event that the error list managed by the <see cref="ErrorCollector"/> has changed
        /// </summary>
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
                    IndicateErrorOnTaskbarEvent(this, null);
            }
            else
            {
                StopIndicatingErrorOnTaskbarEvent(this, null);
            }
        }

        /// <summary>
        /// Creates the child controllers (<see cref="AbstractErrorItemController"/>) for a given <see cref="ErrorCategory"/>.
        /// </summary>
        /// <param name="newErrors">The set of all errors retrieved from the <see cref="ErrorCollector"/></param>
        /// <param name="_sortedList">The resulting list of controllers</param>
        /// <param name="category">The error category for which to create controllers</param>
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
                    // there is one header per category. Ensure to create it
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

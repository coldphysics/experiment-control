
using Errors.Error;
using Errors.Error.ErrorItems;

namespace Controller.Error.ErrorItems
{
    /// <summary>
    /// An error item represnting a concrete error
    /// </summary>
    public class ConcreteErrorItemController : AbstractErrorItemController
    {
        /// <summary>
        /// Casts the underlying <see cref="AbstractErrorItem"/> into a <see cref="ConcreteErrorItem"/>
        /// </summary>
        public ConcreteErrorItem ConcreteErrorItem
        {
            get => (ConcreteErrorItem) base.ErrorItem;
            set => base.ErrorItem = value;
        }

        /// <summary>
        /// A string that represets the current global counter and date time of this error item.
        /// </summary>
        public string DateTime
        {
            set
            {
                ConcreteErrorItem.DataTime = value;
                OnPropertyChanged("DateTime");
            }

            get => ConcreteErrorItem.DataTime;
        }

        /// <summary>
        /// The detailed type of the error item
        /// </summary>
        public ErrorTypes Type
        {
            set
            {
                ConcreteErrorItem.ErrorType = value;
                OnPropertyChanged("Type");
            }

            get => ConcreteErrorItem.ErrorType;
        }

        /// <summary>
        /// Specifies whether the error item should remain even if the delete button is clicked
        /// </summary>
        public bool StayOnDelete
        {
            set
            {
                ConcreteErrorItem.StayOnDelete = value;
                OnPropertyChanged("StayOnDelete");
            }

            get => ConcreteErrorItem.StayOnDelete;
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="errorController">The parent controller</param>
        /// <param name="errorItem">The underlying error item</param>
        public ConcreteErrorItemController(ErrorsWindowController errorController, ConcreteErrorItem errorItem) 
            : base(errorController, errorItem)
        {
        }

        /// <summary>
        /// Handles clicking on the delete button
        /// </summary>
        /// <param name="parameter"></param>
        protected override void DeleteThisErrorClicked(object parameter)
        {
            ErrorCollector.Instance.RemoveSingleError(ConcreteErrorItem);
        }
    }
}

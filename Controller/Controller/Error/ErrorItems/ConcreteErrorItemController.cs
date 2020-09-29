using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Errors.Error;
using Errors.Error.ErrorItems;

namespace Controller.Error.ErrorItems
{
    public class ConcreteErrorItemController : AbstractErrorItemController
    {
        public ConcreteErrorItem ConcreteErrorItem
        {
            get => (ConcreteErrorItem) base.ErrorItem;
            set => base.ErrorItem = value;
        }

        public string DateTime
        {
            set
            {
                ConcreteErrorItem.DataTime = value;
                OnPropertyChanged("DateTime");
            }

            get => ConcreteErrorItem.DataTime;
        }

        public ErrorTypes Type
        {
            set
            {
                ConcreteErrorItem.ErrorType = value;
                OnPropertyChanged("Type");
            }

            get => ConcreteErrorItem.ErrorType;
        }

        public bool StayOnDelete
        {
            set
            {
                ConcreteErrorItem.StayOnDelete = value;
                OnPropertyChanged("StayOnDelete");
            }

            get => ConcreteErrorItem.StayOnDelete;
        }


        public ConcreteErrorItemController(ErrorsWindowController errorController, ConcreteErrorItem errorItem) 
            : base(errorController, errorItem)
        {
        }

        protected override void DeleteThisErrorClicked(object parameter)
        {
            ErrorCollector.Instance.RemoveSingleError(ConcreteErrorItem);
        }
    }
}

using System.Collections.Generic;

namespace Errors.Error.ErrorItems
{
    public class ConcreteErrorItem : AbstractErrorItem
    {
        /// <summary>
        /// The time the error occurred
        /// </summary>
        public string DataTime
        {
            get;
            set;
        }

        /// <summary>
        /// true if error should not be deleted even if setError is called for the actual errorType
        /// </summary>
        public bool StayOnDelete
        {
            get;
            set;
        }

        /// <summary>
        /// type (group) for an error, setError will wipe out all other errors of this type
        /// </summary>
        public ErrorTypes ErrorType
        {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            ConcreteErrorItem item = obj as ConcreteErrorItem;
            return item != null &&
                   DataTime == item.DataTime &&
                   StayOnDelete == item.StayOnDelete &&
                   ErrorType == item.ErrorType &&
                   Message == item.Message &&
                   ErrorCategory == item.ErrorCategory;
        }

        public override int GetHashCode()
        {
            var hashCode = -740192389;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DataTime);
            hashCode = hashCode * -1521134295 + StayOnDelete.GetHashCode();
            hashCode = hashCode * -1521134295 + ErrorType.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Message);
            hashCode = hashCode * -1521134295 + ErrorCategory.GetHashCode();

            return hashCode;
        }
    }
}

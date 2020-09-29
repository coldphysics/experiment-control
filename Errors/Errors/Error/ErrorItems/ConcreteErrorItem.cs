using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}

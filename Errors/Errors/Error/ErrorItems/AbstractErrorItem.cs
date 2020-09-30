using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Errors.Error.ErrorItems
{
    public abstract class AbstractErrorItem
    {
        /// <summary>
        /// String containing the Message text of the Error item
        /// </summary>
        public string Message
        {
            get;
            set;
        }

        /// <summary>
        /// enum which describes on which Card the error occurred
        /// </summary>
        public ErrorCategory ErrorCategory
        {
            get;
            set;
        }

    }
}

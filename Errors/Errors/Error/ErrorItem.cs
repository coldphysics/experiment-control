namespace Errors.Error
{
    /// <summary>
    /// An error Item containing all information about one error message
    /// </summary>
    public class ErrorItem
    {
        /// <summary>
        /// The time the error occurred
        /// don't know yet what data type to use, so is is string with a specified format in ErrorClass
        /// </summary>
        public string DataTime
        {
            get;
            set;
        }

        public bool isHeader { get; set; } = false;

        /// <summary>
        /// String containing the Message text of the Error
        /// </summary>
        public string ErrorMessage
        {
            get;
            set;
        }

        /// <summary>
        /// enum which describes on which Card the error occurred
        /// </summary>
        public ErrorWindow ErrorWindow
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

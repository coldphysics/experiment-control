using Errors.Error.ErrorItems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Errors.Error
{
    /// <summary>
    /// A singleton class that manages the collection of errors emitted by the various parts of the application
    /// </summary>
    public sealed class ErrorCollector : INotifyPropertyChanged
    {
        /// <summary>
        /// a list containing all errors as ErrorItems
        /// </summary>
        private List<ConcreteErrorItem> errors = new List<ConcreteErrorItem>();
        /// <summary>
        /// A flag that indicates whether changing the contents of the error list causes properychanged events to be emitted or not
        /// </summary>
        private bool _notificationsEnabled = true;
        /// <summary>
        /// A flag to indicate that the error list has changed during the period in which notifications were disabled
        /// </summary>
        private bool _pendingNotifications = false;
        /// <summary>
        /// A lock that ensures that only the first invoker of the StartBulkUpdate can reenable notifications via the EndBulkUpdate method
        /// </summary>
        private object notificationLock;

        /// <summary>
        /// The port at which this class listens for incoming status messages over TCP
        /// </summary>
        private static readonly int PORT_FOR_NETWORK_STATUS_MESSAGES = 7205;
        /// <summary>
        /// The port at which this class listens for incoming error messages over TCP
        /// </summary>
        private static readonly int PORT_FOR_NETWORK_ERROR_MESSAGES = 7200;
        /// <summary>
        /// A string all network status messages MUST start with
        /// </summary>
        private static readonly string STATUS = "STATUS";
        /// <summary>
        /// A string all network error messages MUST start with
        /// </summary>
        private static readonly string ERROR = "ERROR";

        // ******************** events ********************

        /// <summary>
        /// Indicates when the errors list or status list have changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        // ******************** lock stuff ********************
        /// <summary>
        /// lock variable
        /// </summary>
        private static readonly object _singletonLockObj = new object();
        /// <summary>
        /// A synchronization object that ensures that invocations to this class methods are thread-safe
        /// </summary>
        private readonly object _lockObj = new object();

        // ******************** properties ********************
        /// <summary>
        /// Maps an error category to an status message
        /// </summary>
        public Dictionary<ErrorCategory, string> Status { get; set; } = new Dictionary<ErrorCategory, string>();

        /// <summary>
        /// The latest known value for the global counter (setting the value here does not really change the actual global counter!)
        /// </summary>
        public int GlobalCounter { get; set; } = 0;

        // ******************** constructor and instance constructor ********************
        /// <summary>
        /// the singleton instance of this class
        /// </summary>
        private static volatile ErrorCollector _instance;

        /// <summary>
        /// the singleton instance of this class (thread-safe)
        /// </summary>
        public static ErrorCollector Instance
        {
            get
            {
                if (_instance == null) // first check
                {
                    lock (_singletonLockObj)
                    {
                        if (_instance == null) // second check for thread safety
                        {
                            _instance = new ErrorCollector();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Instantiate a new instance of this class. Private to force callers to use the Instance property
        /// </summary>
        private ErrorCollector()
        {
            Thread waitForNetworkErrors = new Thread(AwaitNetworkErrors);
            waitForNetworkErrors.IsBackground = true;
            waitForNetworkErrors.Start();

            Thread waitForNetworkStatusReports = new Thread(AwaitNetworkStatusReports);
            waitForNetworkStatusReports.IsBackground = true;
            waitForNetworkStatusReports.Start();
        }

        /// <summary>
        /// Called when the contents of the error list get changed
        /// Will not emit an event if the _notificationsEnabled flag is false. In this case it well set the _pendingNotifications flag to true.
        /// </summary>
        private void OnUpdateErrorList()
        {
            lock (_lockObj)
            {
                if (_notificationsEnabled)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Errors"));
                }
                else
                {
                    _pendingNotifications = true;
                }
            }

        }

        /// <summary>
        /// Blocks the notifications emitted from this class regarding changes in the error list. Useful before starting an operation known for the ability to detect
        /// multiple errors at once, e.g., RootModel.Verify()
        /// </summary>
        /// <returns>An object that can be used to unblock the notifications (only if the caller is the caller actually causing the blocking to happen)</returns>
        public object StartBulkUpdate()
        {
            object candidateLock = new object();

            lock (_lockObj)
            {
                if (notificationLock == null)
                {
                    notificationLock = candidateLock;
                }

                _notificationsEnabled = false;
            }

            return candidateLock;
        }

        /// <summary>
        /// Unblocks the notifications emitted from this class regarding changes in the error list.
        /// </summary>
        /// <param name="candidateLock">The lock returned from the corresponding StartBulkUpdate method call</param>
        /// <returns>true if the notifications were unblocked, false otherwise (either not the first blocker, or wrong lock object)</returns>
        public bool EndBulkUpdate(object candidateLock)
        {
            lock (_lockObj)
            {
                if (candidateLock == notificationLock)
                {
                    notificationLock = null;
                    _notificationsEnabled = true;

                    if (_pendingNotifications)
                    {
                        _pendingNotifications = false;
                        OnUpdateErrorList();
                    }

                    return true;
                }
            }

            return false;
        }

        // ******************** delete Errors ********************
        /// <summary>
        /// Deletes a specifc concrete error item (forcefully)
        /// </summary>
        /// <param name="error"></param>
        public void RemoveSingleError(ConcreteErrorItem error)
        {
            bool removed = false;

            lock (_lockObj)
            {
                if (errors.Contains(error))
                {
                    errors.Remove(error);
                    removed = true;
                }
            }

            if (removed)
                OnUpdateErrorList();
        }

        /// <summary>
        /// Deletes all Errors of the specified Window and Type without the errors with the StayOnDeleteErrors flag 
        /// or the <see cref="StickyErrorItem"/> errors
        /// </summary>
        /// <param name="errorWindow">Window the errors belong to</param>
        /// <param name="errorType">Type of the errors to delete</param>
        public void RemoveErrorsOfWindowAndType(ErrorCategory errorWindow, ErrorTypes errorType)
        {
            int removed;

            lock (_lockObj)
            {
                removed = errors.RemoveAll(error =>
                {
                    return error.ErrorCategory == errorWindow && !error.StayOnDelete && error.ErrorType == errorType && !(error is StickyErrorItem);
                });
            }

            if (removed > 0)
                OnUpdateErrorList();
        }

        /// <summary>
        /// Deletes all Errors of the specified Window without the errors with the StayOnDeleteErrors flag 
        /// or the <see cref="StickyErrorItem"/> errors
        /// </summary>
        /// <param name="errorCategory">Category the errors belong to</param>
        public void RemoveErrorsOfWindow(ErrorCategory errorCategory)
        {
            int removed;

            lock (_lockObj)
            {
                removed = errors.RemoveAll(error =>
                {
                    return error.ErrorCategory == errorCategory && !error.StayOnDelete && !(error is StickyErrorItem);
                });
            }

            if (removed > 0)
                OnUpdateErrorList();
        }

        /// <summary>
        /// Deletes all Errors of the specified category, even the StayOnDeleteErrors
        /// </summary>
        /// <param name="errorCategory">Category the errors belong to</param>
        public void RemoveErrorsOfWindowEvenStayOnDelete(ErrorCategory errorCategory)
        {
            int removed;

            lock (_lockObj)
            {
                removed = errors.RemoveAll(error =>
                {
                    return error.ErrorCategory == errorCategory && !(error is StickyErrorItem);
                });

            }

            if (removed > 0)
                OnUpdateErrorList();
        }

        // ******************** Networking ********************
        /// <summary>
        /// Waits for a TCP message at a specific port, and executes an action that processes it
        /// </summary>
        /// <param name="port">The port at which the message will be received</param>
        /// <param name="action">The action that will process the message</param>
        private void AwaitNetworkMessages(int port, Action<string> action)
        {
            TcpListener serverSocket = new TcpListener(IPAddress.Any, port);
            TcpClient clientSocket;
            NetworkStream networkStream;
            serverSocket.Start();

            while (true)
            {
                using (clientSocket = serverSocket.AcceptTcpClient())
                {
                    using (networkStream = clientSocket.GetStream())
                    {
                        byte[] bytesFrom = new byte[10025];
                        Array.Resize(ref bytesFrom, clientSocket.ReceiveBufferSize);
                        networkStream.Read(bytesFrom, 0, clientSocket.ReceiveBufferSize);
                        string dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                        dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf('\0'));
                        action(dataFromClient);
                    }

                }
            }
        }

        /// <summary>
        /// waits for errors being sent via the network (port 7200)
        /// You have to send "ERROR" + ErrorMessage if the error shall be displayed. If the message does not start with "ERROR", it will be ignored.
        /// You should also terminate the ErrorMessage with "\0" because for some reason the network communication does not transmit or receive string terminating characters.
        /// </summary>
        private void AwaitNetworkErrors()
        {
            AwaitNetworkMessages(PORT_FOR_NETWORK_ERROR_MESSAGES, (dataFromClient) =>
            {
                if (dataFromClient.Length > ERROR.Length)
                {
                    if (dataFromClient.Substring(0, ERROR.Length).Equals(ERROR))
                    {
                        string message = dataFromClient.Substring(ERROR.Length, dataFromClient.Length - ERROR.Length);
                        AddError(message, ErrorCategory.Python, true, ErrorTypes.ExternalError);
                    }
                }
            });

        }

        /// <summary>
        /// The usage of status reports over the network is not clear??
        /// </summary>
        private void AwaitNetworkStatusReports()
        {
            AwaitNetworkMessages(PORT_FOR_NETWORK_STATUS_MESSAGES, (dataFromClient) =>
            {
                if (dataFromClient.Length > STATUS.Length)
                {
                    if (dataFromClient.Substring(0, STATUS.Length).Equals(STATUS))
                    {
                        SetStatus(dataFromClient.Substring(STATUS.Length, dataFromClient.Length - STATUS.Length), ErrorCategory.Python);
                    }
                }
            });

        }

        // ******************** add Errors ********************
        /// <summary>
        /// Adds an error to the errorList (if an error with the same message exists, it is replaced)
        /// </summary>
        /// <param name="errorMsg">string error message</param>
        /// <param name="errorCategory">category of error</param>
        /// <param name="stayOnDelete">true if error should stay even if the category is cleaned up</param>
        /// <param name="errorType">specific error type</param>
        public ConcreteErrorItem AddError(string errorMsg, ErrorCategory errorCategory, bool stayOnDelete, ErrorTypes errorType)
        {
            return AddError(errorMsg, errorCategory, stayOnDelete, errorType, false);
        }

        public StickyErrorItem AddStickyError(string errorMsg, ErrorCategory errorCard, bool stayOnDelete, ErrorTypes errorType)
        {
            return (StickyErrorItem)AddError(errorMsg, errorCard, stayOnDelete, errorType, true);
        }

        private ConcreteErrorItem AddError(string errorMsg, ErrorCategory errorCard, bool stayOnDelete, ErrorTypes errorType, bool isSticky)
        {
            ConcreteErrorItem error = null;

            if (errorMsg != null)
            {
                ConcreteErrorItem alreadyExistingError = errors.Where(param => param.Message.Equals(errorMsg)).FirstOrDefault();

                if (alreadyExistingError != null)
                    RemoveSingleError(alreadyExistingError);//This guarantees a new datetime information for the error

                if (!isSticky)
                {
                    error = new ConcreteErrorItem
                    {
                        DataTime = "GC " + GlobalCounter + " " + DateTime.Now.ToString("ddd, dd.MM., HH:mm:ss"),
                        Message = errorMsg,
                        ErrorCategory = errorCard,
                        StayOnDelete = stayOnDelete,
                        ErrorType = errorType
                    };
                }
                else
                {
                    error = new StickyErrorItem
                    {
                        DataTime = "GC " + GlobalCounter + " " + DateTime.Now.ToString("ddd, dd.MM., HH:mm:ss"),
                        Message = errorMsg,
                        ErrorCategory = errorCard,
                        StayOnDelete = stayOnDelete,
                        ErrorType = errorType
                    };
                }

                lock (_lockObj)
                {
                    if (Status.ContainsKey(errorCard))
                    {
                        // mostly used for external python errors
                        error.Message += " at " + Status[errorCard];
                    }

                    errors.Add(error);
                }
            }

            OnUpdateErrorList();

            return error;
        }

        public void SetStatus(string statusMessage, ErrorCategory category)
        {
            if (statusMessage != null)
            {
                string outString = "GC " + GlobalCounter + " " + DateTime.Now.ToString("ddd, dd.MM., HH:mm:ss") + " - " +
                                   statusMessage;

                lock (_lockObj)
                {
                    if (Status.ContainsKey(category))
                    {
                        Status[category] = outString;
                    }
                    else
                    {
                        Status.Add(category, outString);
                    }
                }
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Status"));
        }

        /// <summary>
        /// Gets a snapshot of the current state of the error list
        /// </summary>
        /// <returns></returns>
        public List<ConcreteErrorItem> GetErrorsSnapshot()
        {
            List<ConcreteErrorItem> errorsCopy;

            lock (_lockObj)
            {
                errorsCopy = new List<ConcreteErrorItem>(errors);
            }

            return errorsCopy;
        }

    }
}
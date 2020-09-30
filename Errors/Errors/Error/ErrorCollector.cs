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
    /// Error Class responsible for the whole error handling
    /// </summary>
    public sealed class ErrorCollector : INotifyPropertyChanged
    {
        /// <summary>
        /// a list containing all errors as ErrorItems
        /// </summary>
        private List<ConcreteErrorItem> errors = new List<ConcreteErrorItem>();
        private bool _notificationsEnabled = true;
        private bool _pendingNotifications = false;
        private object notificationLock;

        private static readonly int PORT_FOR_NETWORK_STATUS_MESSAGES = 7205;
        private static readonly int PORT_FOR_NETWORK_ERROR_MESSAGES = 7200;
        private static readonly string STATUS = "STATUS";
        private static readonly string ERROR = "ERROR";

        // ******************** events ********************

        public event PropertyChangedEventHandler PropertyChanged;

        // ******************** lock stuff ********************
        /// <summary>
        /// lock variable
        /// </summary>
        private static readonly object _singletonLockObj = new object();

        private readonly object _lockObj = new object();

        // ******************** properties ********************
        public Dictionary<ErrorCategory, string> Status { get; set; } = new Dictionary<ErrorCategory, string>();

        public int GlobalCounter { get; set; } = 0;

        // ******************** constructor and instance constructor ********************
        /// <summary>
        /// the instance return value for creating a new ErrorClass instance
        /// </summary>
        private static volatile ErrorCollector _instance;

        /// <summary>
        /// instance Constructor, for this being a Singleton Class (thread-safe)
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


        private ErrorCollector()
        {
            Thread waitForNetworkErrors = new Thread(AwaitNetworkErrors);
            waitForNetworkErrors.IsBackground = true;
            waitForNetworkErrors.Start();

            Thread waitForNetworkStatusReports = new Thread(AwaitNetworkStatusReports);
            waitForNetworkStatusReports.IsBackground = true;
            waitForNetworkStatusReports.Start();
        }

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
        /// Deletes all Errors of the specified Window and Type without the StayOnDeleteErrors
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
        /// Deletes all Errors of the specified Window without the StayOnDeleteErrors
        /// </summary>
        /// <param name="errorWindow">Window the errors belong to</param>
        public void RemoveErrorsOfWindow(ErrorCategory errorWindow)
        {
            int removed;

            lock (_lockObj)
            {
                removed = errors.RemoveAll(error =>
                {
                    return error.ErrorCategory == errorWindow && !error.StayOnDelete && !(error is StickyErrorItem);
                });
            }

            if (removed > 0)
                OnUpdateErrorList();
        }

        /// <summary>
        /// Deletes all Errors of the specified Window, even the StayOnDeleteErrors
        /// </summary>
        /// <param name="errorWindow">Window the errors belong to</param>
        public void RemoveErrorsOfWindowEvenStayOnDelete(ErrorCategory errorWindow)
        {
            int removed;

            lock (_lockObj)
            {
                removed = errors.RemoveAll(error =>
                {
                    return error.ErrorCategory == errorWindow && !(error is StickyErrorItem);
                });

            }

            if (removed > 0)
                OnUpdateErrorList();
        }

        // ******************** add Errors ********************

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

        /// <summary>
        /// a function adding an error to the errorList
        /// </summary>
        /// <param name="errorMsg">string error message</param>
        /// <param name="errorCard">ErrorCards enum</param>
        /// <param name="stayOnDelete">true if error should stay even if the category is cleaned up</param>
        /// <param name="errorType">ErrorTypes enum</param>
        public ConcreteErrorItem AddError(string errorMsg, ErrorCategory errorCard, bool stayOnDelete, ErrorTypes errorType)
        {
            return AddError(errorMsg, errorCard, stayOnDelete, errorType, false);
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
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
        private Errors.ErrorWindow _parent;
        /// <summary>
        /// a list containing all errors as ErrorItems
        /// </summary>
        private List<ConcreteErrorItem> errors = new List<ConcreteErrorItem>();
        private bool _blinkstate = false;
        private bool showEmptyCategories = false;
        private bool _showBasic = true;
        private bool _showPulseblaster = true;
        private bool _showMainHardware = true;
        private bool _showVariables = true;
        private bool _showPython = true;

        private static readonly int PORT_FOR_NETWORK_STATUS_MESSAGES = 7205;
        private static readonly int PORT_FOR_NETWORK_ERROR_MESSAGES = 7200;

        // ******************** events ********************

        public event PropertyChangedEventHandler PropertyChanged;

        // ******************** lock stuff ********************
        /// <summary>
        /// lock variable
        /// </summary>
        private static readonly object _singletonLockObj = new object();

        private readonly object _lockObj = new object();

        // ******************** properties ********************
        public Dictionary<string, string> Status { get; set; } = new Dictionary<string, string>();

        public int GlobalCounter { get; set; } = 0;

        public bool ShowBasic
        {
            get { return _showBasic; }
            set
            {
                _showBasic = value;
                OnUpdateErrorList();
            }
        }

        public bool ShowPulseblaster
        {
            get { return _showPulseblaster; }
            set
            {
                _showPulseblaster = value;
                OnUpdateErrorList();
            }
        }

        public bool ShowMainHardware
        {
            get { return _showMainHardware; }
            set
            {
                _showMainHardware = value;
                OnUpdateErrorList();
            }
        }

        public bool ShowVariables
        {
            get { return _showVariables; }
            set
            {
                _showVariables = value;
                OnUpdateErrorList();
            }
        }

        public bool ShowPython
        {
            get { return _showPython; }
            set
            {
                _showPython = value;
                OnUpdateErrorList();
            }
        }

        public List<AbstractErrorItem> SortedList
        {
            get { return CreateErrorList(); }
        }

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

        public void SetParent(Errors.ErrorWindow parentWindow)
        {
            _parent = parentWindow;
        }

        private void Blink()
        {
            if (_blinkstate == false)
            {
                _blinkstate = true;
                _parent.blink();
            }
        }

        private void StopBlink()
        {
            if (_blinkstate == true)
            {
                _blinkstate = false;
                _parent.stopBlink();
            }
        }

        private void CreateErrorListForSingleCategory(List<ConcreteErrorItem> newErrors, List<AbstractErrorItem> _sortedList, ErrorCategory errorWindow, string errorWindowName, bool showThisCategory)
        {
            bool isHeaderAdded = false;
            AbstractErrorItem header;

            for (int i = 0; i < newErrors.Count(); i++)
            {
                if (newErrors[i].ErrorCategory == errorWindow)
                {
                    if (!isHeaderAdded)
                    {
                        header = new ErrorHeader();
                        header.Message = "--- " + errorWindowName + " ---";
                        header.ErrorCategory = errorWindow;
                        _sortedList.Add(header);
                        isHeaderAdded = true;
                    }
                    if (showThisCategory)
                    {
                        _sortedList.Add(newErrors[i]);
                    }
                }
            }
            if (!isHeaderAdded && showEmptyCategories)
            {
                header = new ErrorHeader();
                header.Message = errorWindowName + " OK";
                header.ErrorCategory = errorWindow;
                _sortedList.Add(header);
            }
        }

        /// <summary>
        /// a method returning an error list
        /// </summary>
        /// <returns>an ErrorItem list containing all errors</returns>
        private List<AbstractErrorItem> CreateErrorList()
        {
            List<ConcreteErrorItem> errorsCopy;
            //Lock, deepClone of "newErrors"
            lock (_lockObj)
            {
                errorsCopy = new List<ConcreteErrorItem>(errors);
            }

            List<AbstractErrorItem> _sortedList = new List<AbstractErrorItem>();

            CreateErrorListForSingleCategory(errorsCopy, _sortedList, ErrorCategory.Basic, "Basic", ShowBasic);
            CreateErrorListForSingleCategory(errorsCopy, _sortedList, ErrorCategory.MainHardware, "Main Hardware", ShowMainHardware);
            CreateErrorListForSingleCategory(errorsCopy, _sortedList, ErrorCategory.Pulseblaster, "Pulseblaster", ShowPulseblaster);
            CreateErrorListForSingleCategory(errorsCopy, _sortedList, ErrorCategory.Variables, "Variables", ShowVariables);
            CreateErrorListForSingleCategory(errorsCopy, _sortedList, ErrorCategory.Python, "Python / External", ShowPython);

            if (_sortedList.Count != 0)
            {
                Blink();
            }
            else
            {
                StopBlink();
            }

            return _sortedList;
        }

        private void OnUpdateErrorList()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SortedList"));
        }

        // ******************** delete Errors ********************
        public void RemoveSingleError(ConcreteErrorItem error)
        {
            lock (_lockObj)
            {
                if (errors.Contains(error))
                {
                    errors.Remove(error);
                }
            }
            OnUpdateErrorList();
        }

        /// <summary>
        /// Deletes all Errors of the specified Window and Type without the StayOnDeleteErrors
        /// </summary>
        /// <param name="errorWindow">Window the errors belong to</param>
        /// <param name="errorType">Type of the errors to delete</param>
        public void RemoveErrorsOfWindowAndType(ErrorCategory errorWindow, ErrorTypes errorType)
        {
            lock (_lockObj)
            {
                errors.RemoveAll(error => {
                    return error.ErrorCategory == errorWindow && !error.StayOnDelete && error.ErrorType == errorType && !(error is StickyErrorItem);
                });
            }
            OnUpdateErrorList();
        }

        /// <summary>
        /// Deletes all Errors of the specified Window without the StayOnDeleteErrors
        /// </summary>
        /// <param name="errorWindow">Window the errors belong to</param>
        public void RemoveErrorsOfWindow(ErrorCategory errorWindow)
        {
            lock (_lockObj)
            {
                errors.RemoveAll(error => {
                    return error.ErrorCategory == errorWindow && !error.StayOnDelete && !(error is StickyErrorItem);
                });
            }
            OnUpdateErrorList();
        }

        /// <summary>
        /// Deletes all Errors of the specified Window, even the StayOnDeleteErrors
        /// </summary>
        /// <param name="errorWindow">Window the errors belong to</param>
        public void RemoveErrorsOfWindowEvenStayOnDelete(ErrorCategory errorWindow)
        {
            lock (_lockObj)
            {
                errors.RemoveAll(error => {
                    return error.ErrorCategory == errorWindow && !(error is StickyErrorItem);
                });

            }
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
                if (dataFromClient.Length > 5)
                {
                    if (dataFromClient.Substring(0, 5).Equals("ERROR"))
                    {
                        ErrorCollector errorCollector = ErrorCollector.Instance;
                        errorCollector.AddError(dataFromClient.Substring(5, dataFromClient.Length - 5), ErrorCategory.Python, true, ErrorTypes.ExternalError);
                    }
                }
            });

        }

        private void AwaitNetworkStatusReports()
        {
            AwaitNetworkMessages(PORT_FOR_NETWORK_STATUS_MESSAGES, (dataFromClient) =>
            {
                if (dataFromClient.Length > 6)
                {
                    if (dataFromClient.Substring(0, 6).Equals("STATUS"))
                    {
                        SetStatus(dataFromClient.Substring(6, dataFromClient.Length - 6), "Python");
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
                    if (Status.ContainsKey("Python"))
                    {
                        error.Message += " at " + Status["Python"];
                    }
                    errors.Add(error);
                }
            }

            OnUpdateErrorList();

            return error;
        }

        public void SetStatus(string statusMessage, string Group)
        {
            if (statusMessage != null)
            {
                string outString = "GC " + GlobalCounter + " " + DateTime.Now.ToString("ddd, dd.MM., HH:mm:ss") + " - " +
                                   statusMessage;

                lock (_lockObj)
                {
                    if (Status.ContainsKey(Group))
                    {
                        Status[Group] = outString;
                    }
                    else
                    {
                        Status.Add(Group, outString);
                    }
                }
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Status"));
        }

    }
}
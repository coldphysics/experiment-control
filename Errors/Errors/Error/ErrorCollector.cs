using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Errors.Error
{
    // ******************** enums ********************
    public enum ErrorWindow
    {
        Pulseblaster,
        MainHardware,
        Variables,
        Basic,
        Python,
        Messages
    };

    public enum ErrorTypes
    {
        ProgramError,
        OutOfRange,
        NegativeTime,
        DynamicCompileError,
        StrangeStephanError,
        FileNameEmpty,
        FileNotFound,
        ExternalError,
        Other
    };

    /// <summary>
    /// Error Class responsible for the whole error handling
    /// </summary>
    public sealed class ErrorCollector : INotifyPropertyChanged
    {
        private Errors.ErrorWindow _parent;
        /// <summary>
        /// a list containing all errors as ErrorItems
        /// </summary>
        private List<ErrorItem> errors = new List<ErrorItem>();
        private bool _blinkstate = false;
        private bool showEmptyCategories = false;
        private bool _showBasic = true;
        private bool _showPulseblaster = true;
        private bool _showMainHardware = true;
        private bool _showVariables = true;
        private bool _showPython = true;

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

        public List<ErrorItem> SortedList
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

        private void CreateErrorListForSingleCategory(List<ErrorItem> newErrors, List<ErrorItem> _sortedList, ErrorWindow errorWindow, string errorWindowName, bool showThisCategory)
        {
            bool isHeaderAdded = false;
            ErrorItem header;

            for (int i = 0; i < newErrors.Count(); i++)
            {
                if (newErrors[i].ErrorWindow == errorWindow)
                {
                    if (!isHeaderAdded)
                    {
                        header = new ErrorItem();
                        header.DataTime = "";
                        header.ErrorMessage = "--- " + errorWindowName + " ---";
                        header.isHeader = true;
                        header.ErrorWindow = errorWindow;
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
                header = new ErrorItem();
                header.DataTime = "";
                header.ErrorMessage = errorWindowName + " OK";
                header.isHeader = true;
                header.ErrorWindow = errorWindow;
                _sortedList.Add(header);
            }
        }

        /// <summary>
        /// a method returning an error list
        /// </summary>
        /// <returns>an ErrorItem list containing all errors</returns>
        private List<ErrorItem> CreateErrorList()
        {
            List<ErrorItem> errorsCopy;
            //Lock, deepClone of "newErrors"
            lock (_lockObj)
            {
                errorsCopy = new List<ErrorItem>(errors);
            }

            List<ErrorItem> _sortedList = new List<ErrorItem>();

            CreateErrorListForSingleCategory(errorsCopy, _sortedList, ErrorWindow.Basic, "Basic", ShowBasic);
            CreateErrorListForSingleCategory(errorsCopy, _sortedList, ErrorWindow.MainHardware, "Main Hardware", ShowMainHardware);
            CreateErrorListForSingleCategory(errorsCopy, _sortedList, ErrorWindow.Pulseblaster, "Pulseblaster", ShowPulseblaster);
            CreateErrorListForSingleCategory(errorsCopy, _sortedList, ErrorWindow.Variables, "Variables", ShowVariables);
            CreateErrorListForSingleCategory(errorsCopy, _sortedList, ErrorWindow.Python, "Python / External", ShowPython);

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
        public void RemoveSingleError(ErrorItem error)
        {
            lock (_lockObj)
            {
                if (errors.Contains(error))
                {
                    errors.Remove(error);
                }
            }
        }

        /// <summary>
        /// Deletes all Errors of the specified Window and Type without the StayOnDeleteErrors
        /// </summary>
        /// <param name="errorWindow">Window the errors belong to</param>
        /// <param name="errorType">Type of the errors to delete</param>
        public void RemoveErrorsOfWindowAndType(ErrorWindow errorWindow, ErrorTypes errorType)
        {
            lock (_lockObj)
            {
                errors.RemoveAll(error => {
                    return error.ErrorWindow == errorWindow && !error.StayOnDelete && error.ErrorType == errorType && !(error is StickyErrorItem);
                });
            }
            OnUpdateErrorList();
        }

        /// <summary>
        /// Deletes all Errors of the specified Window without the StayOnDeleteErrors
        /// </summary>
        /// <param name="errorWindow">Window the errors belong to</param>
        public void RemoveErrorsOfWindow(ErrorWindow errorWindow)
        {
            lock (_lockObj)
            {
                errors.RemoveAll(error => {
                    return error.ErrorWindow == errorWindow && !error.StayOnDelete && !(error is StickyErrorItem);
                });
            }
            OnUpdateErrorList();
        }

        /// <summary>
        /// Deletes all Errors of the specified Window, even the StayOnDeleteErrors
        /// </summary>
        /// <param name="errorWindow">Window the errors belong to</param>
        public void RemoveErrorsOfWindowEvenStayOnDelete(ErrorWindow errorWindow)
        {
            lock (_lockObj)
            {
                errors.RemoveAll(error => {
                    return error.ErrorWindow == errorWindow && !(error is StickyErrorItem);
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
            AwaitNetworkMessages(7200, (dataFromClient) =>
            {
                if (dataFromClient.Length > 5)
                {
                    if (dataFromClient.Substring(0, 5).Equals("ERROR"))
                    {
                        ErrorCollector errorCollector = ErrorCollector.Instance;
                        errorCollector.AddError(dataFromClient.Substring(5, dataFromClient.Length - 5), ErrorWindow.Python, true, ErrorTypes.ExternalError);
                    }
                }
            });

        }

        private void AwaitNetworkStatusReports()
        {
            AwaitNetworkMessages(7205, (dataFromClient) =>
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
        public ErrorItem AddError(string errorMsg, ErrorWindow errorCard, bool stayOnDelete, ErrorTypes errorType)
        {
            return AddError(errorMsg, errorCard, stayOnDelete, errorType, false);
        }

        public StickyErrorItem AddStickyError(string errorMsg, ErrorWindow errorCard, bool stayOnDelete, ErrorTypes errorType)
        {
            return (StickyErrorItem)AddError(errorMsg, errorCard, stayOnDelete, errorType, true);
        }

        private ErrorItem AddError(string errorMsg, ErrorWindow errorCard, bool stayOnDelete, ErrorTypes errorType, bool isSticky)
        {
            ErrorItem error = null;

            if (errorMsg != null)
            {
                ErrorItem alreadyExistingError = errors.Where(param => param.ErrorMessage.Equals(errorMsg)).FirstOrDefault();

                if (alreadyExistingError != null)
                    RemoveSingleError(alreadyExistingError);//This guarantees a new datetime information for the error

                if (!isSticky)
                {
                    error = new ErrorItem
                    {
                        DataTime = "GC " + GlobalCounter + " " + DateTime.Now.ToString("ddd, dd.MM., HH:mm:ss"),
                        ErrorMessage = errorMsg,
                        ErrorWindow = errorCard,
                        StayOnDelete = stayOnDelete,
                        ErrorType = errorType
                    };
                }
                else
                {
                    error = new StickyErrorItem
                    {
                        DataTime = "GC " + GlobalCounter + " " + DateTime.Now.ToString("ddd, dd.MM., HH:mm:ss"),
                        ErrorMessage = errorMsg,
                        ErrorWindow = errorCard,
                        StayOnDelete = stayOnDelete,
                        ErrorType = errorType
                    };
                }

                lock (_lockObj)
                {
                    if (Status.ContainsKey("Python"))
                    {
                        error.ErrorMessage += " at " + Status["Python"];
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

        /// <summary>
        /// sets a List of strings as errors
        /// </summary>
        /// <param name="errorMsg">string List error message</param>
        /// <param name="errorCard">ErrorCards enum</param>
        /// <param name="stayOnDelete">true if error should stay even if setError or reset is called on this errorType</param>
        /// <param name="errorType">ErrorTypes enum</param>
        /*public void SetError(List<string> errorMsg, ErrorWindow errorCard, bool stayOnDelete, ErrorTypes errorType)
        {
            ErrorItem error;
            lock (_lockObj)
            {
                if (Errors != null)
                {
                    for (int i = 0; i < Errors.Count; i++)
                    {
                        if (Errors[i].ErrorWindow == errorCard && !Errors[i].StayOnDelete && Errors[i].ErrorType == errorType)
                        {
                            Errors.Remove(Errors[i]);
                            i = -1;
                        }
                    }
                }
            }
            if (errorMsg != null)
            {
                for (int i = 0; i < errorMsg.Count(); i++)
                {
                    error = new ErrorItem();
                    error.DataTime = DateTime.Now.ToString("ddd, dd.MM.yyyy HH:mm:ss UTCK");
                    error.ErrorMessage = errorMsg[i];
                    error.ErrorWindow = errorCard;
                    error.StayOnDelete = stayOnDelete;
                    error.ErrorType = errorType;
                    lock (_lockObj)
                    {
                        Errors.Add(error);
                    }
                }
            }
            if (null != this.PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("SortedList"));
            }
            //this._parent.blink();
        }*/

        /// <summary>
        /// Deletes all Errors of the specified Window and Type, even the StayOnDeleteErrors
        /// </summary>
        /// <param name="errorWindow">Window the errors belong to</param>
        /// <param name="errorType">Type of the errors to delete</param>
        /*public void ResetEvenStayOnDelete(ErrorWindow errorWindow, ErrorTypes errorType)
        {
            lock (_lockObj)
            {
                for (int i = 0; i < Errors.Count; i++)
                {
                    if (Errors[i].ErrorWindow == errorWindow && Errors[i].ErrorType == errorType)
                    {
                        Errors.Remove(Errors[i]);
                        i = -1;
                    }
                }
            }
            if (null != this.PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("SortedList"));
            }
        }*/

        /// <summary>
        /// Sets an error (and deletes all other errors of this window and type)
        /// </summary>
        /// <param name="errorMsg">string List error message</param>
        /// <param name="errorWindow">ErrorWindow enum</param>
        /// <param name="stayOnDelete">true if error should stay even if setError or reset is called on this errorType</param>
        /// <param name="errorType">ErrorTypes enum</param>
        /*public void SetSingleError(string errorMsg, ErrorWindow errorWindow, bool stayOnDelete, ErrorTypes errorType)
        {
            ErrorItem error;
            lock (_lockObj)
            {
                if (Errors != null)
                {
                    for (int i = 0; i < Errors.Count; i++)
                    {
                        if (Errors[i].ErrorWindow == errorWindow && !Errors[i].StayOnDelete && Errors[i].ErrorType == errorType)
                        {
                            Errors.Remove(Errors[i]);
                            i = -1;
                        }
                    }
                }
            }
            if (errorMsg != null)
            {
                    error = new ErrorItem();
                    error.DataTime = DateTime.Now.ToString("ddd, dd.MM.yyyy HH:mm:ss UTCK");
                    error.ErrorMessage = errorMsg;
                    error.ErrorWindow = errorWindow;
                    error.StayOnDelete = stayOnDelete;
                    error.ErrorType = errorType;
                    lock (_lockObj)
                    {
                        Errors.Add(error);
                    }
            }
            if (null != this.PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("SortedList"));
            }
            //this._parent.blink();
        }*/
    }
}
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
        // ******************** variables ********************

        public int GlobalCounter = 0;
        private bool _blinkstate = false;
        public Errors.ErrorWindow _parent;
        private bool showEmptyCategories = false;
        /// <summary>
        /// a list containing all errors as ErrorItem
        /// </summary>
        public List<ErrorItem> Errors = new List<ErrorItem>();
        public Dictionary<string, string> Status = new Dictionary<string, string>();

        // ******************** events ********************
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion


        // ******************** lock stuff ********************
        /// <summary>
        /// lock variable
        /// </summary>
        private static readonly object SyncRoot = new Object();

        private readonly object _lockObj = new object();

        // ******************** properties ********************
        //Bool values which indicate whether 
        public bool ShowBasic
        {
            get { return _showBasic; }
            set
            {
                _showBasic = value;
                if (null != this.PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SortedList"));
                }
            }
        }
        private bool _showBasic = true;

        public bool ShowPulseblaster
        {
            get { return _showPulseblaster; }
            set
            {
                _showPulseblaster = value;
                if (null != this.PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SortedList"));
                }
            }
        }
        private bool _showPulseblaster = true;

        public bool ShowMainHardware
        {
            get { return _showMainHardware; }
            set
            {
                _showMainHardware = value;
                if (null != this.PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SortedList"));
                }
            }
        }
        private bool _showMainHardware = true;

        public bool ShowVariables
        {
            get { return _showVariables; }
            set
            {
                _showVariables = value;
                if (null != this.PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SortedList"));
                }
            }
        }
        private bool _showVariables = true;

        public bool ShowPython
        {
            get { return _showPython; }
            set
            {
                _showPython = value;
                if (null != this.PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SortedList"));
                }
            }
        }
        private bool _showPython = true;

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
        /// instance Constructor, for this being a Singleton Class
        /// </summary>
        public static ErrorCollector Instance
        {
            get
            {
                if (_instance == null) // first check
                {
                    lock (SyncRoot)
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
        /// Constructor for ErrorClass
        /// </summary>
        private ErrorCollector()
        {
            Thread waitForNetworkErrors = new Thread(awaitNetworkErrors);
            waitForNetworkErrors.IsBackground = true;
            waitForNetworkErrors.Start();

            Thread waitForNetworkStatusReports = new Thread(awaitNetworkStatusReports);
            waitForNetworkStatusReports.IsBackground = true;
            waitForNetworkStatusReports.Start();
        }

        public void SetParent(Errors.ErrorWindow parentWindow)
        {
            _parent = parentWindow;
        }

        public void blink()
        {
            if (_blinkstate == false)
            {
                _blinkstate = true;
                _parent.blink();
            }
        }

        public void stopBlink()
        {
            if (_blinkstate == true)
            {
                _blinkstate = false;
                _parent.stopBlink();
            }
        }

        private void createErrorListForSingleCategory(List<ErrorItem> newErrors, ref List<string> tempList, ref List<ErrorItem> _sortedList, ErrorWindow errorWindow, string errorWindowName, bool showThisCategory)
        {
            bool occurednewErrors = false;
            ErrorItem header;
            for (int i = 0; i < newErrors.Count(); i++)
            {
                if (newErrors[i].ErrorWindow == errorWindow)
                {
                    if (!occurednewErrors)
                    {
                        tempList.Add("--- " + errorWindowName + " ---");
                        header = new ErrorItem();
                        header.DataTime = "";
                        header.ErrorMessage = "--- " + errorWindowName + " ---";
                        header.isHeader = true;
                        header.ErrorWindow = errorWindow;
                        _sortedList.Add(header);
                        occurednewErrors = true;
                    }
                    if (showThisCategory)
                    {
                        tempList.Add(newErrors[i].DataTime + "\t" + newErrors[i].ErrorMessage);
                        _sortedList.Add(newErrors[i]);
                    }
                }
            }
            if (!occurednewErrors && showEmptyCategories)
            {
                tempList.Add(errorWindowName + " OK");
                header = new ErrorItem();
                header.DataTime = "";
                header.ErrorMessage = errorWindowName + " OK";
                header.isHeader = true;
                header.ErrorWindow = errorWindow;
                _sortedList.Add(header);
            }
        }

        /// <summary>
        /// a method returning a string error list
        /// </summary>
        /// <returns>a string list containing all errors</returns>
        private List<ErrorItem> CreateErrorList()
        {
            List<ErrorItem> newErrors;
            lock (_lockObj)
            {
                newErrors = new List<ErrorItem>(Errors);
            }
            //Lock, deepClone of "newErrors"
            var tempList = new List<string>();
            var _sortedList = new List<ErrorItem>();
            _sortedList.Clear();

            if (newErrors != null)
            {
                createErrorListForSingleCategory(newErrors, ref tempList, ref _sortedList, ErrorWindow.Basic, "Basic", ShowBasic);
                createErrorListForSingleCategory(newErrors, ref tempList, ref _sortedList, ErrorWindow.MainHardware, "Main Hardware", ShowMainHardware);
                createErrorListForSingleCategory(newErrors, ref tempList, ref _sortedList, ErrorWindow.Pulseblaster, "Pulseblaster", ShowPulseblaster);
                createErrorListForSingleCategory(newErrors, ref tempList, ref _sortedList, ErrorWindow.Variables, "Variables", ShowVariables);
                createErrorListForSingleCategory(newErrors, ref tempList, ref _sortedList, ErrorWindow.Python, "Python / External", ShowPython);
                if (_sortedList.Count != 0)
                {
                    blink();
                }
                else
                {
                    stopBlink();
                }
            }
            return _sortedList;

        }



        // ******************** delete Errors ********************
        public void RemoveSingleError(ErrorItem error)
        {
            lock (_lockObj)
            {
                if (Errors.Contains(error))
                {
                    Errors.Remove(error);
                }
            }
            if (null != this.PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("SortedList"));
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
                for (int i = 0; i < Errors.Count; i++)
                {
                    if (Errors[i].ErrorWindow == errorWindow && !Errors[i].StayOnDelete && Errors[i].ErrorType == errorType && !(Errors[i] is StickyErrorItem))
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
        }

        /// <summary>
        /// Deletes all Errors of the specified Window without the StayOnDeleteErrors
        /// </summary>
        /// <param name="errorWindow">Window the errors belong to</param>
        public void RemoveErrorsOfWindow(ErrorWindow errorWindow)
        {
            lock (_lockObj)
            {
                for (int i = 0; i < Errors.Count; i++)
                {
                    if (Errors[i].ErrorWindow == errorWindow && !Errors[i].StayOnDelete && !(Errors[i] is StickyErrorItem))
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
        }

        /// <summary>
        /// Deletes all Errors of the specified Window, even the StayOnDeleteErrors
        /// </summary>
        /// <param name="errorWindow">Window the errors belong to</param>
        public void RemoveErrorsOfWindowEvenStayOnDelete(ErrorWindow errorWindow)
        {
            lock (_lockObj)
            {
                for (int i = 0; i < Errors.Count; i++)
                {
                    if (Errors[i].ErrorWindow == errorWindow && !(Errors[i] is StickyErrorItem))
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
        }

        // ******************** add Errors ********************

        /// <summary>
        /// waits for errors being sent via the network (port 7200)
        /// You have to send "ERROR" + ErrorMessage if the error shall be displayed. If the message does not start with "ERROR", it will be ignored.
        /// You should also terminate the ErrorMessage with "\0" because for some reason the network communication does not transmit or receive string terminating characters.
        /// </summary>
        void awaitNetworkErrors()
        {
            TcpListener serverSocket = new TcpListener(IPAddress.Any ,7200);
            TcpClient clientSocket = default(TcpClient);
            serverSocket.Start();
            while (true)
            {
                clientSocket = serverSocket.AcceptTcpClient();
                NetworkStream networkStream = clientSocket.GetStream();
                byte[] bytesFrom = new byte[10025];
                Array.Resize(ref bytesFrom, clientSocket.ReceiveBufferSize);
                networkStream.Read(bytesFrom, 0, clientSocket.ReceiveBufferSize);
                string dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf('\0'));
                //System.Console.Write("Message from Client: {0}\n", dataFromClient);
                clientSocket.Close();
                if (dataFromClient.Length > 5)
                {
                    if (dataFromClient.Substring(0, 5).Equals("ERROR"))
                    {
                        ErrorCollector errorCollector = ErrorCollector.Instance;
                        //System.Console.Write("Calling AddError!\n");
                        errorCollector.AddError(dataFromClient.Substring(5, dataFromClient.Length - 5), ErrorWindow.Python, true, ErrorTypes.ExternalError);
                    }

                }
            }
        }

        //RECO start network listening safely
        void awaitNetworkStatusReports()
        {
            TcpListener serverSocket = new TcpListener(IPAddress.Any, 7205);
            TcpClient clientSocket = default(TcpClient);
            serverSocket.Start();
            while (true)
            {
                clientSocket = serverSocket.AcceptTcpClient();
                NetworkStream networkStream = clientSocket.GetStream();
                byte[] bytesFrom = new byte[10025];
                Array.Resize(ref bytesFrom, clientSocket.ReceiveBufferSize);
                networkStream.Read(bytesFrom, 0, clientSocket.ReceiveBufferSize);
                string dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf('\0'));
                //System.Console.Write("Message from Client: {0}\n", dataFromClient);
                clientSocket.Close();
                if (dataFromClient.Length > 6)
                {
                    if (dataFromClient.Substring(0, 6).Equals("STATUS"))
                    {
                        ErrorCollector errorCollector = ErrorCollector.Instance;
                        //System.Console.Write("Calling AddError!\n");
                        errorCollector.SetStatus(dataFromClient.Substring(6, dataFromClient.Length - 6), "Python");
                    }

                }
            }
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
            //System.Console.Write("ADD ERROR!\n");
            if (errorMsg != null)
            {
                ErrorItem alreadyExistingError = Errors.Where(param => param.ErrorMessage.Equals(errorMsg)).FirstOrDefault();

                if(alreadyExistingError != null)
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
                //_parent.blink();


                lock (_lockObj)
                {
                    if (Status.ContainsKey("Python"))
                    {
                        error.ErrorMessage += " at " + Status["Python"];
                    }
                    Errors.Add(error);
                }

            }

            if (null != this.PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("SortedList"));
            }

            return error;
        }



        public void SetStatus(string statusMessage, string Group)
        {
            //System.Console.Write("ADD ERROR!\n");
            if (statusMessage != null)
            {
                string outString = "GC " + GlobalCounter + " " + DateTime.Now.ToString("ddd, dd.MM., HH:mm:ss") + " - " +
                                   statusMessage;
                //_parent.blink();
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
            if (null != this.PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Status"));
            }
            //this._parent.blink();
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

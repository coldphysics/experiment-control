using ADwin.Driver;
using Communication.Enums;

namespace HardwareAdWin.Driver
{
    //CHANGED Ghareeb 04.10.2016 implementation of the IAdWinDriver interface
    //CHANGED Ghareeb 20.10.2016 to use an enum for the state of the AdWin system instead of an integer value.


    /// <summary>
    /// Provides functionalities to start, stop and query the status of a process in the AdWin system.
    /// </summary>
    public class AdWinFunctions : IAdWinDriver
    {
        /// <summary>
        /// An instance of the AdWin system driver.
        /// </summary>
        private readonly ADwin.Driver.ADwinSystem _adwin = new ADwinSystem();

        //RECO remove the constructor since device number is assigned by default to 1
        /// <summary> 
        /// Initializes a new instance of the <see cref="AdWinFunctions"/> class. It initializes the DeviceNumber to 1.
        /// </summary>
        public AdWinFunctions()
        {
            _adwin.DeviceNumber = 1;
        }

        /// <summary>
        /// Initializes the AdWin system (sets Parameter1 to 1) and then waits until the initialization is done, afterwards ADwin processor becomes idle 
        /// </summary>
        public void InitializeAdWin()
        {
            _adwin.Pars[1].Value = (int)(AdWinStateEnum.INITIALIZING);

            while (_adwin.Pars[1].Value == (int)AdWinStateEnum.INITIALIZING)
            {
                // wait for initialization
            }
        }

        /// <summary>
        /// Starts the AdWin system (sets Parameter1 to 2).
        /// </summary>
        public void StartAdWin()
        {
            _adwin.Pars[1].Value = (int)AdWinStateEnum.PROCESSING_FIFOS;
        }



        /// <summary>
        /// Asks the state of the AdWin system.
        /// </summary>
        /// <returns>
        /// (<see cref=" AdWinStateEnum.IDLE"/>, <see cref=" AdWinStateEnum.INITIALIZING"/>, or <see cref=" AdWinStateEnum.PROCESSING_FIFOS"/>) if the hardware system has a regular state,
        /// <see cref=" AdWinStateEnum.UNKNOWN"/> if the hardware system has an invalid state.
        /// </returns>
        public AdWinStateEnum AskAdWinState()
        {
            try
            {
                return (AdWinStateEnum)_adwin.Pars[1].Value;
            }
            catch 
            {
                return AdWinStateEnum.UNKNOWN;
            }
        }


        #region IAdWinDriver Members

        /// <summary>
        /// Determines whether the state of the communication channel is OK
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the communication state is OK, <c>false</c> otherwise.
        /// </returns>
        public bool IsCommunicationOK()
        {
            //A weird exception could be thrown here if the build architecture of the main project is not x86
            return (_adwin.TestCommunication() == CommunicationState.Ok);
        }

        /// <summary>
        /// Sets the device number.
        /// </summary>
        /// <param name="newDeviceNumber">The new device number.</param>
        public void SetDeviceNumber(int newDeviceNumber)
        {
            _adwin.DeviceNumber = 1;
        }

        /// <summary>
        /// Checks whether a certain FIFO exists.
        /// </summary>
        /// <param name="fifoNumber">The 1-based index of the FIFO to check.</param>
        /// <returns>
        ///   <c>true</c> if the FIFO exists, <c>false</c> otherwise.
        /// </returns>
        public bool DoesFIFOExist(int fifoNumber)
        {
            return _adwin.Fifos[fifoNumber].Exists;
        }

        /// <summary>
        /// Gets the free number of elements in a certain FIFO.
        /// </summary>
        /// <param name="fifoNumber">The 1-based index of the FIFO</param>
        /// <returns>
        /// The free number of elements in the specified FIFO
        /// </returns>
        public int GetFreeSpaceInFIFO(int fifoNumber)
        {
            return _adwin.Fifos[fifoNumber].Free;
        }

        /// <summary>
        /// Gets the length of a certain FIFO.
        /// </summary>
        /// <param name="fifoNumber">The 1-based index of the FIFO</param>
        /// <returns>
        /// The length of the specified FIFO
        /// </returns>
        public int GetFIFOLength(int fifoNumber)
        {
            return _adwin.Fifos[fifoNumber].Length;
        }


        /// <summary>
        /// Clears the specified FIFO.
        /// </summary>
        /// <param name="fifoNumber">The 1-based index of the FIFO</param>
        public void ClearFIFO(int fifoNumber)
        {
            _adwin.Fifos[fifoNumber].Clear();
        }


        /// <summary>
        /// Determines whether the specified FIFO is full.
        /// </summary>
        /// <param name="fifoNumber">The 1-based index of the FIFO</param>
        /// <returns>
        ///   <c>true</c> if the specified FIFO is full, <c>false</c> otherwise.
        /// </returns>
        public bool IsFIFOFull(int fifoNumber)
        {
            return _adwin.Fifos[fifoNumber].IsFull();
        }

        /// <summary>
        /// Transfers count values of type Int32 from array into the specified FIFO array
        /// </summary>
        /// <param name="fifoNumber">The 1-based index of the FIFO</param>
        /// <param name="count">Number (≥1) of integer values to be transferred.</param>
        /// <param name="array">A zero-based array of type System.Int32 from which data are transferred to the ADwin system.</param>
        public void SetAsInteger(int fifoNumber, int count, int[] array)
        {
            _adwin.Fifos[fifoNumber].SetAsInteger(count, array);

        }

        /// <summary>
        /// Boots an AdWin System with a T11 hardware
        /// </summary>
        public void BootAdWinT11()
        {
            _adwin.Boot(ProcessorType.T11);
        }

        /// <summary>
        /// Boots an AdWin System with a T12 hardware
        /// </summary>
        public void BootAdWinT12()
        {
            //RECO the enumeration of T12 is not available in the version of AdWin software used in the 5th floor
            _adwin.Boot("C:\\ADwin\\ADwin12.btl");
        }

        /// <summary>
        /// Loads a specific process to the AdWin system.
        /// </summary>
        /// <param name="processNumber">The 1-based process number.</param>
        /// <param name="processFilePath">The process file path.</param>
        public void LoadProcess(int processNumber, string processFilePath)
        {
            _adwin.Processes[processNumber].Load(processFilePath);
        }

        /// <summary>
        /// Starts a process.
        /// </summary>
        /// <param name="processNumber">The 1-based process number.</param>
        public void StartProcess(int processNumber)
        {
            _adwin.Processes[processNumber].Start();
        }

        /// <summary>
        /// Stops a process.
        /// </summary>
        /// <param name="processNumber">The 1-based process number.</param>
        public void StopProcess(int processNumber)
        {
            _adwin.Processes[processNumber].Stop();
        }

        #endregion
    }
}

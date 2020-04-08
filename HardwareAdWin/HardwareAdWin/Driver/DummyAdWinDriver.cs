using Communication.Enums;
using HardwareAdWin.Simulator;

namespace HardwareAdWin.Driver
{
    /// <summary>
    /// A driver for the <see cref=" DummyAdWinHW"/> class. 
    /// It provides functionality to make it interchangeable with the real AdWin hardware.
    /// </summary>
    /// <seealso cref="IAdWinDriver" />
    class DummyAdWinDriver:IAdWinDriver
    {
        /// <summary>
        /// A reference to the dummy implementation of the AdWin hardware 
        /// </summary>
        private DummyAdWinHW adwin;

        /// <summary>
        /// Initializes the <see cref=" DummyAdWinHW"/> instance.
        /// </summary>
        public void InitializeAdWin()
        {
            adwin.Initialize();
        }

        /// <summary>
        /// Starts the consuming procedure of the <see cref=" DummyAdWinHW"/> instance.
        /// </summary>
        public void StartAdWin()
        {
            adwin.StartConsuming();
        }

        /// <summary>
        /// Asks the state of the <see cref=" DummyAdWinHW"/> instance.
        /// </summary>
        /// <returns>
        ///(<see cref=" AdWinStateEnum.IDLE"/>, <see cref=" AdWinStateEnum.INITIALIZING"/>, or <see cref=" AdWinStateEnum.PROCESSING_FIFOS"/>) if the hardware system has a regular state,
        /// <see cref=" AdWinStateEnum.UNKNOWN"/> if the hardware system has an invalid state.
        /// </returns>
        public AdWinStateEnum AskAdWinState()
        {
            return adwin.CurrentAdWinState;
        }

        /// <summary>
        /// Always returns <c>true</c>.
        /// </summary>
        /// <returns>
        ///   <c>true</c>
        /// </returns>
        public bool IsCommunicationOK()
        {
            return true;
        }

        /// <summary>
        /// Does nothing!
        /// </summary>
        /// <param name="newDeviceNumber">The new device number.</param>
        public void SetDeviceNumber(int newDeviceNumber)
        {
            
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
            return adwin.DoesFIFOExist(fifoNumber);
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
            return adwin.GetFreeSpaceInFIFO(fifoNumber);
        }

        /// <summary>
        /// Gets the length of a certain FIFO.
        /// </summary>
        /// <param name="fifoNumber">The 1-based index of the FIFO</param>
        /// <returns>
        /// The length of the specified FIFO
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public int GetFIFOLength(int fifoNumber)
        {
            return adwin.GetFIFOLength(fifoNumber);
        }

        /// <summary>
        /// Clears the specified FIFO.
        /// </summary>
        /// <param name="fifoNumber">The 1-based index of the FIFO</param>
        public void ClearFIFO(int fifoNumber)
        {
            adwin.ClearFIFO(fifoNumber);
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
            return adwin.IsFIFOFull(fifoNumber);
        }

        /// <summary>
        /// Transfers <c>count</c> values of type Int32 from array into the specified FIFO array
        /// </summary>
        /// <param name="fifoNumber">The 1-based index of the FIFO</param>
        /// <param name="count">Number (≥1) of integer values to be transferred.</param>
        /// <param name="array">A zero-based array of type System.Int32 from which data are transferred to the ADwin system.</param>
        public void SetAsInteger(int fifoNumber, int count, int[] array)
        {
            adwin.EnqueueElements(fifoNumber, array);
        }

        /// <summary>
        /// Does nothing!
        /// </summary>
        public void BootAdWinT11()
        {
            
        }

        /// <summary>
        /// Does nothing!
        /// </summary>
        public void BootAdWinT12()
        {
            
        }

        /// <summary>
        /// Does nothing!
        /// </summary>
        /// <param name="processNumber">The 1-based process number.</param>
        /// <param name="processFilePath">The process file path.</param>
        public void LoadProcess(int processNumber, string processFilePath)
        {
            
        }


        /// <summary>
        /// Creates the <see cref=" DummyAdWinHW"/> instance.
        /// </summary>
        /// <param name="processNumber">The 1-based process number.</param>
        public void StartProcess(int processNumber)
        {
            adwin = DummyAdWinHW.GetInstance();
        }

        /// <summary>
        /// Stops the <see cref=" DummyAdWinHW"/> instance if it were running.
        /// </summary>
        /// <param name="processNumber">The 1-based process number.</param>
        public void StopProcess(int processNumber)
        {
            adwin.StopConsumer();
        }
    }
}

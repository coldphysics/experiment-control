using Communication.Enums;

namespace HardwareAdWin.Driver
{
    //ADDED Ghareeb 04.10.2016

    /// <summary>
    /// Abstracts the needed functionality of an ADWin Driver
    /// </summary>
    interface IAdWinDriver
    {
        /// <summary>
        /// Synchronously initializes the ADWin System
        /// </summary>
        void InitializeAdWin();

        /// <summary>
        /// Starts the AdWin system.
        /// </summary>
        void StartAdWin();

        /// <summary>
        /// Asks the state of the AdWin system
        /// </summary>
        /// <returns>
        /// (<see cref=" AdWinStateEnum.IDLE"/>, <see cref=" AdWinStateEnum.INITIALIZING"/>, or <see cref=" AdWinStateEnum.PROCESSING_FIFOS"/>) if the hardware system has a regular state,
        /// <see cref=" AdWinStateEnum.UNKNOWN"/> if the hardware system has an invalid state.
        /// </returns>
        AdWinStateEnum AskAdWinState();

        /// <summary>
        /// Determines whether the state of the communication channel is OK
        /// </summary>
        /// <returns><c>true</c> if the communication state is OK, <c>false</c> otherwise.</returns>
        bool IsCommunicationOK();

        /// <summary>
        /// Sets the device number.
        /// </summary>
        /// <param name="newDeviceNumber">The new device number.</param>
        void SetDeviceNumber(int newDeviceNumber);

        /// <summary>
        /// Checks whether a certain FIFO exists.
        /// </summary>
        /// <param name="fifoNumber">The 1-based index of the FIFO to check.</param>
        /// <returns><c>true</c> if the FIFO exists, <c>false</c> otherwise.</returns>
        bool DoesFIFOExist(int fifoNumber);

        /// <summary>
        /// Gets the free number of elements in a certain FIFO.
        /// </summary>
        /// <param name="fifoNumber">The 1-based index of the FIFO</param>
        /// <returns>The free number of elements in the specified FIFO</returns>
        int GetFreeSpaceInFIFO(int fifoNumber);

        /// <summary>
        /// Gets the length of a certain FIFO.
        /// </summary>
        /// <param name="fifoNumber">The 1-based index of the FIFO</param>
        /// <returns>The length of the specified FIFO</returns>
        int GetFIFOLength(int fifoNumber);

        /// <summary>
        /// Clears the specified FIFO.
        /// </summary>
        /// <param name="fifoNumber">The 1-based index of the FIFO</param>
        void ClearFIFO(int fifoNumber);

        /// <summary>
        /// Determines whether the specified FIFO is full.
        /// </summary>
        /// <param name="fifoNumber">The 1-based index of the FIFO</param>
        /// <returns><c>true</c> if the specified FIFO is full, <c>false</c> otherwise.</returns>
        bool IsFIFOFull(int fifoNumber);

        /// <summary>
        /// Transfers count values of type Int32 from array into the specified FIFO array
        /// </summary>
        /// <param name="fifoNumber">The 1-based index of the FIFO</param>
        /// <param name="count">Number (≥1) of integer values to be transferred.</param>
        /// <param name="array">A zero-based array of type System.Int32 from which data are transferred to the ADwin system.</param>
        void SetAsInteger(int fifoNumber, int count, int[] array);

        #region Booting and Processes        
        /// <summary>
        /// Boots an AdWin System with a T11 hardware
        /// </summary>
        void BootAdWinT11();

        /// <summary>
        /// Boots an AdWin System with a T12 hardware
        /// </summary>
        void BootAdWinT12();

        /// <summary>
        /// Loads a specific process to the AdWin system.
        /// </summary>
        /// <param name="processNumber">The 1-based process number.</param>
        /// <param name="processFilePath">The process file path.</param>
        void LoadProcess(int processNumber, string processFilePath);

        /// <summary>
        /// Starts a process.
        /// </summary>
        /// <param name="processNumber">The 1-based process number.</param>
        void StartProcess(int processNumber);

        /// <summary>
        /// Stops a process.
        /// </summary>
        /// <param name="processNumber">The 1-based process number.</param>
        void StopProcess(int processNumber);
        #endregion
    }
}

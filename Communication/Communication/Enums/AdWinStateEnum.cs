namespace Communication.Enums
{
    //ADDED Ghareeb 20.10.2016

    /// <summary>
    /// Describes the current state of the AdWin hardware system (which is running the computer-control process)
    /// </summary>
    public enum AdWinStateEnum
    {
        /// <summary>
        /// The AdWin system is waiting for the next command. 
        /// (Waiting to be ordered to initialize, or to start processing the already-filled FIFOs)
        /// </summary>
        IDLE = 0,

        /// <summary>
        /// The AdWin system is initializing its internal state (e.g., its FIFOs) to be able to receive data. 
        /// (The next state is IDLE)
        /// </summary>
        INITIALIZING = 1,

        /// <summary>
        /// The AdWin system is processing the sequences which are being filled within its FIFOs. 
        /// (The next state is IDLE)
        /// </summary>
        PROCESSING_FIFOS = 2,

        /// <summary>
        /// The state of the AdWinSystem is unknown! 
        /// (Should not happen)
        /// </summary>
        UNKNOWN = -1
    }
}

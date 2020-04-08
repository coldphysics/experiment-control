namespace Communication.Interfaces.Hardware
{
    /// <summary>
    /// Queries the length and the free space of a specific FIFO array in the AdWin system.
    /// </summary>
    public interface IAdWinDebug
    {
        /// <summary>
        /// Gets free space available in the current FIFO array.
        /// </summary>
        /// <returns>The free space available in the current FIFO array, or <c>0</c> if there was an error while trying to read this value.</returns>
        int GetFifoStatus();
        /// <summary>
        /// Gets the total length of the current FIFO array.
        /// </summary>
        /// <returns>The total length of the current FIFO array, or <c>1</c> if there was an error while trying to read this value.</returns>
        int GetTotalFifo();
    }
}

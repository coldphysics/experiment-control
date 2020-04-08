using System.Collections.Generic;

namespace HardwareAdWin.HardwareAdWin
{
    /// <summary>
    /// The parameters that will be passed to the thread that writes the output to one FIFO in the AdWin system.
    /// </summary>
    public class FIFOWriterThreadState
    {
        /// <summary>
        /// The number of the FIFO (1-based) to write the output to.
        /// </summary>
        public int FifoNum;
        /// <summary>
        /// The data to write to the FIFO
        /// </summary>
        public List<uint> FifoArray;
    }
}

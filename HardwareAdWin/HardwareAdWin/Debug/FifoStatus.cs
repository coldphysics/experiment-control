using System;
using Communication.Interfaces.Hardware;
using HardwareAdWin.Driver;

namespace HardwareAdWin.Debug
{
    //CHANGED Ghareeb 04.10.2016 use of IAdWinDriver instead of directly accessing the ADwin.Driver
    //A Small Debug Window (not necessary for the program) that shows the Load of all Fifos    

    /// <summary>
    /// Queries the length and the free space of a specific FIFO array in the AdWin system.
    /// </summary>
    /// <seealso cref="Communication.Interfaces.Hardware.IAdWinDebug" />
    public class FifoStatus:IAdWinDebug
    {
        /// <summary>
        /// The 0-based active FIFO number.
        /// </summary>
        private int _fifoNumber;

        /// <summary>
        /// The driver of the AdWin system.
        /// </summary>
        private IAdWinDriver adwin;

        /// <summary>
        /// Initializes a new instance of the <see cref="FifoStatus"/> class.
        /// </summary>
        public FifoStatus()
        {
            adwin = AdWinDriverFactory.GetAdWinDriver();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FifoStatus"/> class and ensures that the communication with the AdWin system is functioning well. 
        /// </summary>
        /// <param name="FifoNumber">The 0-based number for the active FIFO.</param>
        /// <exception cref="System.Exception">Occurs when a communication problem is detected.</exception>
        public FifoStatus(int FifoNumber)
            :this()
        {
            this._fifoNumber = FifoNumber + 1;
            adwin.SetDeviceNumber(1);

            if (!adwin.IsCommunicationOK())
            {
                throw new Exception("ADWIN: Communication Problem!");
            }
        }
        /// <summary>
        /// Gets free space available in the current FIFO array.
        /// </summary>
        /// <returns>
        /// The free space available in the current FIFO array, or <c>0</c> if there was an error while trying to read this value.
        /// </returns>
        public int GetFifoStatus()
        {
            try
            {
                if (!adwin.DoesFIFOExist(_fifoNumber))
                {
                    return 0;
                    //throw new Exception("ADWIN: Fifo does not exist (Maybe the Adwin program is not started yet)");
                }

                return adwin.GetFreeSpaceInFIFO(_fifoNumber);
            }
            catch (Exception)
            {
                Console.WriteLine("Error in HardwareAdWin.Debug.FifoStatus.GetFifoStatus.");
                return 0;
            }
        }

        /// <summary>
        /// Gets the total length of the current FIFO array.
        /// </summary>
        /// <returns>
        /// The total length of the current FIFO array, or <c>1</c> if there was an error while trying to read this value.
        /// </returns>
        public int GetTotalFifo()
        {
            try
            {
                if (!adwin.DoesFIFOExist(_fifoNumber))
                {
                    return 1;
                    //throw new Exception("ADWIN: Fifo does not exist (Maybe the Adwin program is not started yet)");
                }

                return adwin.GetFIFOLength(_fifoNumber);
            }
            catch (Exception)
            {
                Console.WriteLine("Error in HardwareAdWin.Debug.FifoStatus.GetTotalFifo.");
                return 1;
            }
            
        }
    }
}

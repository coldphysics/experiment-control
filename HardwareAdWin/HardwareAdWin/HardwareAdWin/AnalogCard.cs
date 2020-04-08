using System;
using System.Collections.Generic;
using System.Threading;
using Buffer.OutputProcessors.Quantization;
using Communication.Interfaces.Generator;
using HardwareAdWin.Driver;

namespace HardwareAdWin.HardwareAdWin
{
    //CHANGED Ghareeb 04.10.2016 use of AdWinDriver instead of directly accessing ADwin.Driver
    //RECO have a common parent for both Analog and Digital cards
    /// <summary>
    /// Represent the mechanism to send the output data of a single analog channel to the corresponding FIFO in the AdWin system
    /// </summary>
    public class AnalogCard
    {
        private IAdWinDriver adwin;

        /// <summary>
        /// Indicates the index of the card starting from zero.
        /// </summary>
        private int _cardNum;

        /// <summary>
        /// Guarantees the mutual exclusion of different threads when trying accessing <see cref=" started"/> array.
        /// </summary>
        private object adWinLock;

        /// <summary>
        /// A reference to the array that is used to detect that all FIFO-threads are running for the current cycle
        /// </summary>
        private bool[] started;

        /// <summary>
        /// The maximum number of elements that can be written to the FIFO at once.
        /// </summary>
        private const int BLOCK_SIZE = 1000;

        /// <summary>
        /// The number of milliseconds that the thread that is writing to the FIFO would sleep before it tries sending again.
        /// </summary>
        private const int WAIT_FOR_FULL_FIFO_MILLIS = 10;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalogCard"/> class.
        /// </summary>
        /// <param name="cardNum">The number of the card.</param>
        public AnalogCard(int cardNum)
        {
            this._cardNum = cardNum;
            adwin = AdWinDriverFactory.GetAdWinDriver();
            adwin.SetDeviceNumber(1);
        }

       
        /// <summary>
        /// Starts writing the output of all channels of this card to the FIFOs of the AdWin system.
        /// </summary>
        /// <param name="_adWinLock">A lock object that guarantees that the started flag of the FIFO of this card is being read and changed atomically.</param>
        /// <param name="_started">The array of all started-flags of all FIFOs.</param>
        /// <param name="cardOutput">The output of all channels of this card.</param>
        /// <remarks>
        /// It calls the method <see cref=" startWriteToFifo"/> for each channel.
        /// </remarks>
        public void Initialize(object _adWinLock, bool[] _started, ICardOutput cardOutput)
        {
            int CHANNELS_PER_ANALOG_CARD = Global.GetNumAnalogChannelsPerCard();
            QuantizedAnalogCardOutput output = (QuantizedAnalogCardOutput)cardOutput;
            List<List<uint>> outputTotal = output.Output;

            //RECO discuss usefulness
            if (outputTotal.Count == 0)
            {
                return;
            }

            this.adWinLock = _adWinLock;
            this.started = _started;

            //start writing the output of all channels of this card.
            for (int i = 0; i < CHANNELS_PER_ANALOG_CARD; i++)
            {
                int channelNumber = i + (this._cardNum - 1) * CHANNELS_PER_ANALOG_CARD + 1; //The channel number that we are working on (1-based, counting is done among all channels of all cards)
                startWriteToFifo(channelNumber, outputTotal[i]); // first parameter here is not zero-based value
            }
        }

        ///// <summary>
        ///// Casts the output values from <see cref=" System.Double"/> to <see cref=" System.UInt32"/>. The output is supposed to be already quantized.
        ///// </summary>
        ///// <param name="output">The output in double format.</param>
        ///// <returns>The output in uint format.</returns>
        //private List<List<uint>> CastOutputValues(double[,] output)
        //{
        //    List<List<uint>> result = new List<List<uint>>();
        //    List<uint> current;

        //    for (int i = 0; i < output.GetLength(0); i++)
        //    {
        //        current = new List<uint>();
        //        result.Add(current);

        //        for (int j = 0; j < output.GetLength(1); j++)
        //        {
        //            current.Add((uint)output[i, j]);
        //        }
        //    }

        //    return result;
        //}


        /// <summary>
        /// Starts writing the converted output of one of this card's channels to the corresponding FIFO.
        /// </summary>
        /// <param name="analogCardChannelFIFONumber">The channel's FIFO number.</param>
        /// <param name="output">The converted output of the specified channel.</param>
        public void startWriteToFifo(int analogCardChannelFIFONumber, List<uint> output)
        {
            FIFOWriterThreadState threadState = new FIFOWriterThreadState();
            threadState.FifoNum = analogCardChannelFIFONumber;
            threadState.FifoArray = output;// new List<uint>();
            ParameterizedThreadStart pts = new ParameterizedThreadStart(writeToFifo);
            //RECO threads should not be declared as local variables! There should be a field for this thread.
            Thread thread = new Thread(pts);
            thread.Name = "Writing_to_FIFO_" + threadState.FifoNum;
            thread.Priority = ThreadPriority.Highest;
            thread.IsBackground = true;
            thread.Start(threadState);
        }

        /// <summary>
        /// This method is executed by threads which write the output data to the corresponding FIFOs.
        /// </summary>
        /// <param name="obj">
        /// An instance of the type <see cref=" FIFOWriterThreadState"/> which holds the data to be written to the FIFO and the number of FIFO, 
        /// </param>
        /// <remarks>
        /// Sending is done in blocks of 1000 integers = 1000 * 4 Bytes, and whenever the FIFO is found to be full, the thread sleeps for 10 ms.
        /// When the first block of data is sent to the FIFO, the shared "started" array is changed to indicate this event.
        /// </remarks>
        private void writeToFifo(object obj)
        {

            //DateTime start = DateTime.Now;
            FIFOWriterThreadState threadState = (FIFOWriterThreadState)obj;

            int[] channelData = new int[threadState.FifoArray.Count];
            int bytesWritten = 0;
            int bytesToSend;
            int[] dataToSend;
            int free;


            // copy all the output data to an internal array 
            for (int i = 0; i < channelData.Length; i++)
            {
                channelData[i] = (int)threadState.FifoArray[i];//uints are converted to ints! values larger than Int32.MaxValue =  are overflown to negative values!!
            }
            // RECO no need to clear FIFOs since it is already done by initialization function within Adwin code at each cycle
            adwin.ClearFIFO(threadState.FifoNum);//Clears (initializes) the AdWin FIFO array to start copying on it.
            bool endThread = false;// A boolean that indicates to the end of data processing.

            while (!endThread)
            {
                if (!adwin.IsFIFOFull(threadState.FifoNum))//this FIFO isn't completely full, contains free space
                {
                    if (bytesWritten >= channelData.Length) //we have finished writing data
                    {
                        endThread = true;
                        break; //this statement already guarantees the exit from the while loop threadEnd is useless
                    }

                    free = adwin.GetFreeSpaceInFIFO(threadState.FifoNum); //The number of free cells in the FIFO

                    if (free >= BLOCK_SIZE)//Do not send until a size at least the size of a block is free in the FIFO - but why?
                    {
                        //Either send a block-size of data or if the remaining data is less than a block-size, send the remaining data
                        //RECO this should be determined before the if statement and should be used instead of BLOCK_SIZE as the condition of the if statement.
                        bytesToSend = Math.Min(channelData.Length - bytesWritten, BLOCK_SIZE);

                        //RECO use the method that specifies the index to start sending the data instead of copying the part to send into a sub-array. (i.e. SetAsInteger(int count, int[] pcArray, int pcArrayIndex) )
                        dataToSend = new int[bytesToSend];

                        for (int i = 0; i < bytesToSend; i++)
                        {
                            dataToSend[i] = channelData[i + bytesWritten]; // copy the channel data to an internal array(data to send) starting from the last writing index(bytesWritten)
                        }

                        adwin.SetAsInteger(threadState.FifoNum, bytesToSend, dataToSend); // transfer data written in datatosend with length bytestosend to the adwin FIFO array Data(x)
                        bytesWritten += bytesToSend;// increase the size of already written data
                    }

                    //Thread-safe access to the started array.. not meant for synchronization
                    //RECO envelop the started array in a class and put the locking mechanism there.
                    //RECO access the started array only the first time this loop executes.
                    //RECO use AutoResetEvent to communicate between the output sending threads and AdwinHwGroup
                    lock (adWinLock) // this part of code isn't interrupted by other threads.
                    {
                        if (!started[threadState.FifoNum - 1])
                        {
                            started[threadState.FifoNum - 1] = true;
                        }
                    }
                }

                Thread.Sleep(WAIT_FOR_FULL_FIFO_MILLIS);
 
            }


        }
    }
}

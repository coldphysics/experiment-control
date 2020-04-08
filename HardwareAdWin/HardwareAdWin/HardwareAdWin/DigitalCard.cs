using System;
using System.Collections.Generic;
using System.Threading;
using Buffer.OutputProcessors.Quantization;
using Communication.Interfaces.Generator;
using HardwareAdWin.Driver;


namespace HardwareAdWin.HardwareAdWin
{
    //CHANGED Ghareeb 04.10.2016 use of IAdWinDriver

    /// <summary>
    /// Represent the mechanism to send the output data of a single digital card to the corresponding FIFO in the AdWin system
    /// </summary>
    public class DigitalCard
    {

        /// <summary>
        /// An instance of the AdWin system driver.
        /// </summary>
        private readonly IAdWinDriver adwin;

        /// <summary>
        /// Indicates the index of the card starting from zero, digital cards come after analog ones
        /// </summary>
        private readonly int _cardNo;

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

        //RECO remove the name parameter as it is not used
        /// <summary>
        /// Initializes a new instance of the <see cref="DigitalCard"/> class.
        /// </summary>
        /// <param name="cardNo">The number of the card.</param>
        public DigitalCard(int cardNo)// adding a new parameter to refer to the order of the digital card
        {
            this._cardNo = cardNo;
            adwin = AdWinDriverFactory.GetAdWinDriver();
            adwin.SetDeviceNumber(1);
        }


        /// <summary>
        /// Starts writing the converted (compressed) output of this card to the FIFOs of the AdWin system. 
        /// </summary>
        /// <param name="_adWinLock">A lock object that guarantees that the started flag of the FIFO of this card is being read and changed atomically.</param>
        /// <param name="_started">The array of all started-flags of all FIFOs.</param>
        /// <param name="output">The converted (compressed) output of all channels and cards (analog and digital).</param>
        public void Initialize(object _adWinLock, bool[] _started, ICardOutput output)
        {
            this.adWinLock = _adWinLock;
            this.started = _started;

            QuantizedDigitalCardOutput cardOutput = (QuantizedDigitalCardOutput)output;
            //CHANGED Ghareeb 09.02.2017 this error should be useless
            //if (outputTotal.Count == Global.NumberOfBuffers)//check whether the number of buffers is right to go further

            //For digital cards, we don't have separate outputs for separate channels, we rather have a single output for the entire digital card.
            int cardFIFOIndex = Global.GetNumAnalogCards() * Global.GetNumAnalogChannelsPerCard() + _cardNo + 1;// digital channels' order comes after analog ones,first digital card number is zero
            startWriteToFifo(cardFIFOIndex, cardOutput.Output);
        }


        /// <summary>
        /// Starts writing the converted output of this digital card to the corresponding FIFO.
        /// </summary>
        /// <param name="digitalCardFIFONumber">The digital card FIFO number.</param>
        /// <param name="output">The converted output of this digital card.</param>
        public void startWriteToFifo(int digitalCardFIFONumber, List<uint> output)
        {
            FIFOWriterThreadState threadState = new FIFOWriterThreadState();
            threadState.FifoNum = digitalCardFIFONumber;
            threadState.FifoArray = output;
            ParameterizedThreadStart pts = new ParameterizedThreadStart(writeToFifo);
            //RECO threads should not be declared as local variables! There should be a field for this thread.
            Thread thread = new Thread(pts);
            thread.Name = "Writing_to_FIFO_" + threadState.FifoNum;
            thread.IsBackground = true;
            thread.Priority = ThreadPriority.Highest;
            thread.Start(threadState);
        }

        /// <summary>
        /// This method is executed by threads which write the output data to the corresponding FIFOs.
        /// </summary>
        /// <param name="obj">
        /// An instance of the type <see cref=" FIFOWriterThreadState"/> which holds the data to be written to the FIFO, 
        /// and specifies the number of the FIFO.
        /// </param>
        /// <remarks>
        /// Sending is done in blocks of 1000 integers = 1000 * 4 Bytes, and whenever the FIFO is found to be full, the thread sleeps for 10 ms.
        /// When the first block of data is sent to the FIFO, the shared "started" array is changed to indicate this event.
        /// </remarks>
        private void writeToFifo(object obj)
        {
            FIFOWriterThreadState threadState = (FIFOWriterThreadState)obj;
            int bytesWritten = 0;
            int[] channelData = new int[threadState.FifoArray.Count];
            //RECO use following statement instead: int[] channelData = (int[])threadState.FifoArray.ToArray();
            //What is the benefit of having a local copy of the data?
            for (int i = 0; i < channelData.Length; i++)
            {
                channelData[i] = (int)threadState.FifoArray[i];//uints are converted to ints! values larger than Int32.MaxValue =  are overflown to negative values!!
            }

            adwin.ClearFIFO(threadState.FifoNum);//Clears (initializes) the AdWin FIFO array to start copying on it.
            bool threadEnd = false; // A boolean that indicates the end of data processing.

            while (!threadEnd)
            {
                if (!adwin.IsFIFOFull(threadState.FifoNum)) // this FIFO is not full
                {
                    if (bytesWritten >= channelData.Length) //we have finished writing data
                    {
                        threadEnd = true;
                        break;//this statement already guarantees the exit from the while loop threadEnd is useless
                    }

                    //Does this account for the data being transferred in the network? Yes, because TCP is used!
                    int free = adwin.GetFreeSpaceInFIFO(threadState.FifoNum);//The number of free cells in the FIFO

                    if (free >= BLOCK_SIZE)//Do not send until a size at least the size of a block is free in the FIFO - but why?
                    {
                        //Either send a block-size of data or if the remaining data is less than a block-size, send the remaining data
                        //RECO this should be determined before the if statement and should be used instead of BLOCK_SIZE as the condition of the if statement.
                        int bytesToSend = Math.Min(channelData.Length - bytesWritten, BLOCK_SIZE);

                        //RECO use the method that specifies the index to start sending the data instead of copying the part to send into a sub-array. (i.e. SetAsInteger(int count, int[] pcArray, int pcArrayIndex) )
                        int[] dataToSend = new int[bytesToSend];

                        for (int i = 0; i < bytesToSend; i++)
                        {
                            dataToSend[i] = channelData[i + bytesWritten];
                        }

                        adwin.SetAsInteger(threadState.FifoNum, bytesToSend, dataToSend); // transfer data written in dataToSend with length bytesToSend to the adwin FIFO array Data(x)
                        bytesWritten += bytesToSend;

                    }

                    //Thread-safe access to the started array.. not meant for synchronization
                    //RECO contain the started array in a class and put the locking mechanism there.
                    lock (adWinLock) // this part of code isn't interrupted by other threads
                    {
                        if (!started[threadState.FifoNum - 1])
                        {
                            started[threadState.FifoNum - 1] = true;
                        }
                    }
                }
                //CHANGED Ghareeb Falazi 24.06.2016
                //else//if the FIFO is full, back-off for 10 ms
                //{
                Thread.Sleep(WAIT_FOR_FULL_FIFO_MILLIS);
                //}
                /*
                object nonBusyWaitLockObj = new object();
                lock (nonBusyWaitLockObj)
                {
                    Monitor.Wait(nonBusyWaitLockObj,10);// Thread.Sleep(10); //t in ms
                }
                 * */


            }

        }
    }
}

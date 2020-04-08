using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Communication.Enums;
using HardwareAdWin.Simulator.Decompression;


namespace HardwareAdWin.Simulator
{
    /// <summary>
    /// A specific-purpose-simulation of the AdWin hardware that can only interact with this software control program.
    /// It implements the decompression mechanism provided by the real-world AdWin system being used in the experiments,
    /// but the smallest period is WAY smaller than that of the real AdWin hardware (20 ms ~ 200 ms),
    /// and it is not guaranteed to produce output with exact periods (as Windows operating system is not a real-time system). 
    /// </summary>
    public class DummyAdWinHW:IDisposable
    {


        /// <summary>
        /// The single instance of type <see cref=" DummyAdWinHW"/>.
        /// </summary>
        private static DummyAdWinHW singleton;

        /// <summary>
        /// Gets the single instance of type <see cref=" DummyAdWinHW"/>.
        /// It instantiates the instance if necessary.
        /// </summary>
        /// <param name="analogFifoCount">The number of analog FIFOs.</param>
        /// <param name="digitalFifosCount">The number of digital FIFOs.</param>
        /// <param name="fifoSize">Size of a FIFO.</param>
        /// <param name="consumptionPeriod">The consumption period (in milliseconds).</param>
        /// <returns>The single instance of type <see cref=" DummyAdWinHW"/></returns>
        public static DummyAdWinHW GetInstance(int analogFifoCount, int digitalFifosCount, int fifoSize, int consumptionPeriod)
        {
            if (singleton == null)
                singleton = new DummyAdWinHW(analogFifoCount, digitalFifosCount, fifoSize, consumptionPeriod);

            return singleton;
        }

        /// <summary>
        /// Gets the single instance of type <see cref=" DummyAdWinHW"/>.
        /// It instantiates the instance if necessary.
        /// </summary>
        /// <returns>The single instance of type <see cref=" DummyAdWinHW"/></returns>
        public static DummyAdWinHW GetInstance()
        {
            if (singleton == null)
            {
                int ANALOG_CARDS_COUNT = Global.GetNumAnalogCards();
                int ANALOG_CHANNELS_COUNT = Global.GetNumAnalogChannelsPerCard();
                int ANALOG_FIFOS = ANALOG_CARDS_COUNT * ANALOG_CHANNELS_COUNT;
                int DIGITAL_CARDS_COUNT = Global.GetNumDigitalCards();
                int DEFAULT_FIFO_SIZE = 20000;
                int DEFAULT_CONSUMPTION_PERIOD = 50;

                singleton = new DummyAdWinHW(ANALOG_FIFOS, DIGITAL_CARDS_COUNT, DEFAULT_FIFO_SIZE, DEFAULT_CONSUMPTION_PERIOD);
            }

            return singleton;
        }


        /// <summary>
        /// The total number of analog FIFOs hosted by the emulator
        /// </summary>
        public readonly int NUMBER_OF_ANALOG_FIFOS;

        /// <summary>
        /// The total number of digital FIFOs hosted by the emulator
        /// </summary>
        public readonly int NUMBER_OF_DIGITAL_FIFOS;

        /// <summary>
        /// The common size of all FIFOs
        /// </summary>
        private readonly int FIFO_SIZE;

        /// <summary>
        /// The delay between one output and the next (in milliseconds)
        /// </summary>
        public readonly int CONSUMPTION_PERIOD;

        /// <summary>
        /// The current state of the AdWin system.
        /// </summary>
        private AdWinStateEnum currentAdWinState = AdWinStateEnum.IDLE;

        /// <summary>
        /// A timer that ticks once per period and initiates a thread that processes the FIFOs once,
        /// and prepares a (possibly) new output value.
        /// </summary>
        private Timer consumer;

        /// <summary>
        /// Insures that the threads created by the timer do not execute at the same time (in case the period is too short)
        /// </summary>
        private object locker = new object();

        /// <summary>
        /// The FIFOs hosted by the AdWin system.
        /// </summary>
        private ConcurrentQueue<int>[] fifos;

        /// <summary>
        /// The decompression unit.
        /// </summary>
        private DecompressionUnit decompressionUnit;

        /// <summary>
        /// A snapshot of the current output of all channels
        /// </summary>
        double[] currentOutput;

        /// <summary>
        /// Gets the current state of the AdWin system
        /// </summary>
        /// <value>
        /// The current state of the AdWin system
        /// </value>
        public AdWinStateEnum CurrentAdWinState
        {
            get { return currentAdWinState; }
        }

        /// <summary>
        /// Gets or sets the size of the notification batch.
        /// </summary>
        /// <value>
        /// The size of the notification batch.
        /// </value>
        /// <remarks>
        /// The notification batch size is the number of channel outputs accumulated before notifying the event listeners of the <see cref=" ChannelOutputAvailable"/> event.
        /// </remarks>
        public int NotificationBatchSize
        {
            set;
            get;
        }

        
        /// <summary>
        /// Occurs when <see cref=" NotificationBatchSize"/> consecutive outputs have been accumulated.
        /// </summary>
        public event ChannelsOutputAvailableEventHandler ChannelOutputAvailable;

        /// <summary>
        /// The current batch of channel outputs.
        /// </summary>
        private List<double[]> notificationsBatch = new List<double[]>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DummyAdWinHW" /> class.
        /// </summary>
        /// <param name="analogFifoCount">The number of analog FIFOs.</param>
        /// <param name="digitalFifosCount">The number of digital FIFOs.</param>
        /// <param name="fifoSize">Size of a FIFO.</param>
        /// <param name="consumptionPeriod">The consumption period (in milliseconds).</param>
        /// <remarks>The constructor starts the timer responsible for the processing and output immediately.</remarks>
        protected DummyAdWinHW(int analogFifoCount, int digitalFifosCount, int fifoSize, int consumptionPeriod)
        {
            NUMBER_OF_ANALOG_FIFOS = analogFifoCount;
            NUMBER_OF_DIGITAL_FIFOS = digitalFifosCount;
            FIFO_SIZE = fifoSize;
            CONSUMPTION_PERIOD = consumptionPeriod;
            fifos = new ConcurrentQueue<int>[NUMBER_OF_ANALOG_FIFOS + NUMBER_OF_DIGITAL_FIFOS];

            for (int i = 0; i < fifos.Length; i++)
                fifos[i] = new ConcurrentQueue<int>();

            decompressionUnit = new DecompressionUnit(fifos, NUMBER_OF_ANALOG_FIFOS);

            //Initializes the output on all channels (for output purposes only)
            this.currentOutput = decompressionUnit.GetCurrentOutput();
            //Timer keeps running the whole lifecycle of the simulator
            consumer = new Timer(DoConsumerWork, null, CONSUMPTION_PERIOD, CONSUMPTION_PERIOD);

        }


        /// <summary>
        /// Indicates whether the specified FIFO exists or not.
        /// </summary>
        /// <param name="fifoNumber">The 1-based FIFO number.</param>
        /// <returns></returns>
        public bool DoesFIFOExist(int fifoNumber)
        {
            --fifoNumber;

            return fifoNumber < fifos.Length && fifos[fifoNumber] != null;
        }


        /// <summary>
        /// Initializes the AdWin system.
        /// </summary>
        public void Initialize()
        {
            currentAdWinState = AdWinStateEnum.INITIALIZING;
            decompressionUnit.Init();
            currentAdWinState = AdWinStateEnum.IDLE;
        }

        /// <summary>
        /// Stops processing the FIFOs.
        /// </summary>
        public void StopConsumer()
        {
            consumer.Dispose();
        }

        /// <summary>
        /// Changes the state of the AdWin system to <see cref=" AdWinStateEnum.PROCESSING_FIFOS"/>.
        /// </summary>
        public void StartConsuming()
        {
            currentAdWinState = AdWinStateEnum.PROCESSING_FIFOS;
        }

        /// <summary>
        /// Gets the free space in the specified FIFO.
        /// </summary>
        /// <param name="fifoNumber">The 1-based FIFO number.</param>
        /// <returns>The free space in the specified FIFO</returns>
        /// <exception cref="System.IndexOutOfRangeException">Thrown when fifoNumber does not refer to a valid FIFO.</exception>
        public int GetFreeSpaceInFIFO(int fifoNumber)
        {
            --fifoNumber;

            return FIFO_SIZE - fifos[fifoNumber].Count;
        }

        /// <summary>
        /// Gets the length (capacity) of the specified FIFO.
        /// </summary>
        /// <param name="fifoNumber">The 1-based FIFO number.</param>
        /// <returns>The length (capacity) of the specified FIFO</returns>
        /// <exception cref="System.IndexOutOfRangeException">Thrown when fifoNumber does not refer to a valid FIFO.</exception>
        public int GetFIFOLength(int fifoNumber)
        {
            if (!DoesFIFOExist(fifoNumber))
                throw new IndexOutOfRangeException(String.Format("FIFO {0} does not exist!", fifoNumber));

            return FIFO_SIZE;
        }

        /// <summary>
        /// Clears the specified FIFO.
        /// </summary>
        /// <param name="fifoNumber">The 1-based FIFO number.</param>
        /// <exception cref="System.IndexOutOfRangeException">Thrown when fifoNumber does not refer to a valid FIFO.</exception>
        public void ClearFIFO(int fifoNumber)
        {
            --fifoNumber;
            int currentElement;

            while (fifos[fifoNumber].TryDequeue(out currentElement)) ;
        }

        /// <summary>
        /// Determines whether the specified FIFO is full.
        /// </summary>
        /// <param name="fifoNumber">The 1-based FIFO number.</param>
        /// <returns><c>true</c> if the specified FIFO is full, <c>false</c> otherwise.</returns>
        /// <exception cref="System.IndexOutOfRangeException">Thrown when fifoNumber does not refer to a valid FIFO.</exception>
        public bool IsFIFOFull(int fifoNumber)
        {
            --fifoNumber;

            return fifos[fifoNumber].Count >= FIFO_SIZE;//>= instead of == for safety!
        }

        /// <summary>
        /// Enqueues the specified elements in the specified FIFO.
        /// </summary>
        /// <param name="fifoNumber">The 1-based FIFO number.</param>
        /// <param name="array">The array of elements to enqueue.</param>
        /// <exception cref="System.IndexOutOfRangeException">Thrown when fifoNumber does not refer to a valid FIFO.</exception>
        public void EnqueueElements(int fifoNumber, int[] array)
        {
            --fifoNumber;
            ConcurrentQueue<int> selectedQueue = fifos[fifoNumber];

            foreach (int element in array)
                if (selectedQueue.Count < FIFO_SIZE)
                    selectedQueue.Enqueue(element);
                else
                    break;
        }

        /// <summary>
        /// Executes a single iteration of processing/output
        /// </summary>
        /// <param name="param">Not used.</param>
        private void DoConsumerWork(object param)
        {
            lock (locker)
            {
                DateTime start = DateTime.Now;
                currentOutput = decompressionUnit.GetCurrentOutput();
                OnChannelOutputAvailable(currentOutput);

                if (!decompressionUnit.ProcessFIFOs())
                    currentAdWinState = AdWinStateEnum.IDLE;


                #region Debug Output
                //DateTime end = DateTime.Now;
                //int[] SHOW_CHANNELS = { 0, 1, 32, 33 };
                //StringBuilder builder = new StringBuilder();

                //builder.AppendFormat("{0}-{1}\n( ", (start.Ticks % 100000000) / 10000, (end.Ticks - start.Ticks) / 10000);

                //for (int i = 0; i < currentOutput.Length; i++)
                //{
                //    if (SHOW_CHANNELS.ToList().Contains(i))
                //    {
                //        builder.AppendFormat(new System.Globalization.CultureInfo("en-US"), "{0}: {1,6:00.00}, ", i, currentOutput[i]);
                //    }
                //}

                //builder.Append(")\n");

                //System.Diagnostics.Debug.Write(builder.ToString());
                #endregion
            }

        }


        /// <summary>
        /// Called after each processing execution of the AdWin simulator (whenever the <see cref=" consumer"/> Timer ticks).
        /// </summary>
        /// <param name="currentOutput">The current output.</param>
        /// <remarks>This method will cause the <see cref=" ChannelOutputAvailable"/> event to fire only if the size of the current batch has reached <see cref=" NotificationBatchSize"/>.</remarks>
        protected void OnChannelOutputAvailable(double[] currentOutput)
        {
            if (ChannelOutputAvailable != null)
            {
                notificationsBatch.Add(currentOutput);

                if (notificationsBatch.Count == NotificationBatchSize)
                {
                    ChannelOutputAvailable(null, new ChannelsOutputAvailableEventArgs { CurrentOutput = notificationsBatch.ToArray() });
                    notificationsBatch.Clear();
                }
                
            }
            else if (notificationsBatch.Count > 0)
                notificationsBatch.Clear();//Get rid of old batches
        }


        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            try
            {
                consumer.Dispose();
            }
            catch
            {  }
        }

        #endregion
    }
}

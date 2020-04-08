using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using HardwareAdWin.Simulator.Decompression.DecompressionResult;
using HardwareAdWin.Simulator.Dequeue;

namespace HardwareAdWin.Simulator.Decompression
{
    /// <summary>
    /// Decompresses values from the hosted analog and digital FIFOs, and provides a consistent (synchronized) output in terms of voltages.
    /// </summary>
    class DecompressionUnit
    {
        #region Constants
        /* Analog-Related Constants */
        /****************************/
        /// <summary>
        /// The initial output value for analog FIFOs measured in volts.
        /// </summary>
        private const double INITIAL_ANALOG_OUTPUT_VALUE_VOLTS = 0.0;
        /// <summary>
        /// The minimum voltage of an analog channel.
        /// </summary>
        private const double MINIMUM_ANALOG_VOLTAGE = -10.0;
        /// <summary>
        /// The maximum voltage of an analog channel,
        /// </summary>
        private const double MAXIMUM_ANALOG_VOLTAGE = +10.0;
        /// <summary>
        /// The number of analog FIFOs
        /// </summary>
        /// 
        private readonly int ANALOGS_COUNT;
 

        /* Digital-Related Constants */
        /*****************************/
        /// <summary>
        /// The initial encoded output value for digital FIFOs (one bit per channel).
        /// </summary>  
        private const uint INITIAL_DIGITAL_OUTPUT_VALUE_ENCODED = 0x0;
        /// <summary>
        /// The number of channels hosted by a digital card.
        /// </summary>
        private readonly int CHANNELS_PER_DIGITAL_CARD;
        /// <summary>
        /// The voltage level corresponding to a logical true.
        /// </summary>
        public const double DIGITAL_HIGH_VOLTAGE = 5.0;
        /// <summary>
        /// The voltage level corresponding to a logical false.
        /// </summary>
        public const double DIGITAL_LOW_VOLTAGE = 0.0;
        /// <summary>
        /// The number of digital FIFOs.
        /// </summary>
        private readonly int DIGITALS_COUNT;
        #endregion

        /// <summary>
        /// The array of wrapped FIFOs.
        /// </summary>
        private AbstractDequeue[] fifos;
        /// <summary>
        /// The array of last decompression results (one per FIFO).
        /// </summary>
        private IDecompressionResult[] decompressionResults;
        /// <summary>
        /// The decompression results that should be used in the (near) future. (Gets filled in special cases of decompression)
        /// </summary>
        private Queue<IDecompressionResult>[] nextDecompressionResults;

        /// <summary>
        /// Initializes a new instance of the <see cref="DecompressionUnit"/> class.
        /// </summary>
        /// <param name="fifos">The FIFOs.</param>
        /// <param name="numberOfAnalogs">The number of analog FIFOs.</param>
        public DecompressionUnit(ConcurrentQueue<int>[] fifos, int numberOfAnalogs)
        {

            this.CHANNELS_PER_DIGITAL_CARD = Global.GetNumDigitalChannelsPerCard();
            this.ANALOGS_COUNT = numberOfAnalogs;
            this.DIGITALS_COUNT = fifos.Length - numberOfAnalogs;
            this.decompressionResults = new IDecompressionResult[fifos.Length];
            this.nextDecompressionResults = new Queue<IDecompressionResult>[fifos.Length];

            this.fifos = new AbstractDequeue[fifos.Length];
            AnalogNumericDecompressionResult dummyPrevAnalog = new AnalogNumericDecompressionResult(INITIAL_ANALOG_OUTPUT_VALUE_VOLTS, 0);
            DigitalNumericDecompressionResult dummyPrevDigital = new DigitalNumericDecompressionResult(INITIAL_DIGITAL_OUTPUT_VALUE_ENCODED, 0);

            for (int i = 0; i < fifos.Length; i++)
            {
                this.nextDecompressionResults[i] = new Queue<IDecompressionResult>();

                if (i < numberOfAnalogs)
                {
                    this.fifos[i] = new AnalogDequeue(fifos[i]);
                    this.decompressionResults[i] = new AnalogFinishReachedDecompressionResult(dummyPrevAnalog);
                }
                else
                {
                    this.fifos[i] = new DigitalDequeue(fifos[i]);
                    this.decompressionResults[i] = new DigitalFinishReachedDecompressionResult(dummyPrevDigital);
                }
            }
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public void Init()
        {
            foreach (AbstractDequeue dequeue in fifos)
                dequeue.Init();
        }

        /// <summary>
        /// Gets the current output.
        /// </summary>
        /// <returns>A double array that describes the current output (one value per channel).</returns>
        public double[] GetCurrentOutput()
        {
            double[] resultArray = new double[ANALOGS_COUNT + DIGITALS_COUNT * CHANNELS_PER_DIGITAL_CARD];

            //Build output for analog channels
            for (int i = 0; i < ANALOGS_COUNT; i++)
            {
                //numeric value
                if (decompressionResults[i] is AnalogNumericDecompressionResult)
                {
                    //use the value (decreases repetitions)
                    (decompressionResults[i] as AnalogNumericDecompressionResult).UseValue();
                    resultArray[i] = (decompressionResults[i] as AnalogNumericDecompressionResult).VALUE;
                }
                else if (decompressionResults[i] is AnalogFinishReachedDecompressionResult)//sequence has been finished normally
                {
                    //Keep using the last output value
                    resultArray[i] = (decompressionResults[i] as AnalogFinishReachedDecompressionResult).LastOutputValue;
                }
                else
                {
                    //The sequence was ended abnormally, use the initial value.
                    resultArray[i] = INITIAL_ANALOG_OUTPUT_VALUE_VOLTS;
                }
            }

            //Build output for digital channels
            for (int i = ANALOGS_COUNT, k = 0; i < fifos.Length; i++, k++)
            {
                double[] cardOutput;

                if (decompressionResults[i] is DigitalNumericDecompressionResult)
                {
                    (decompressionResults[i] as DigitalNumericDecompressionResult).UseValue();
                    cardOutput = (decompressionResults[i] as DigitalNumericDecompressionResult).VALUES;
                }
                else if (decompressionResults[i] is DigitalFinishReachedDecompressionResult)
                {
                    cardOutput = (decompressionResults[i] as DigitalFinishReachedDecompressionResult).LastOutputValues;
                }
                else
                {
                    //Create an instance with the initial values, and use the decoded result.
                    cardOutput = (new DigitalNumericDecompressionResult(INITIAL_DIGITAL_OUTPUT_VALUE_ENCODED, 0)).VALUES;
                }

                for (int j = 0; j < CHANNELS_PER_DIGITAL_CARD; j++)
                    resultArray[ANALOGS_COUNT + k * CHANNELS_PER_DIGITAL_CARD + j] = cardOutput[j];

            }

            return resultArray;

        }

        private void GenerateCurrentOutput()
        { 

        }

        /// <summary>
        /// Processes the FIFOs once.
        /// </summary>
        /// <returns>
        /// <c>true</c> if at least one FIFO produced a regular value, 
        /// <c>false</c> if each of the FIFOs either has reached the end-of-stream sequence or is empty.
        /// </returns>
        public bool ProcessFIFOs()
        {
            AbstractNumericDecompressionResult temp;
            int regularsCounter = 0;//counts how many numeric values will be in the output.

            for (int i = 0; i < fifos.Length; i++)
            {
                //The last process of this FIFO lead to a regular value (not empty nor finish sequence was found).
                if (decompressionResults[i] is AbstractNumericDecompressionResult)
                {
                    temp = decompressionResults[i] as AbstractNumericDecompressionResult;

                    //Are there still some repetitions in this numeric value, so we can use it?
                    if (temp.Repetitions > 0)
                    {
                        ++regularsCounter;
                    }
                    else
                    {
                        //No more repetitions for the last value, so we should read a new value.
                        decompressionResults[i] = ReadNextValueFromFIFO(i);

                        //Is the new value a numeric value?
                        if (decompressionResults[i] is AbstractNumericDecompressionResult)
                        {
                            ++regularsCounter;
                        }
                    }
                }
                else//Is it the beginning of a new sequence?
                    if (decompressionResults[i] is AbstractFinishReachedDecompressionResult && !fifos[i].IsEmpty())
                    {
                        decompressionResults[i] = ReadNextValueFromFIFO(i);

                        //Is the new value a numeric value?
                        if (decompressionResults[i] is AbstractNumericDecompressionResult)
                        {
                            ++regularsCounter;
                        }
                    }

            }

            return regularsCounter > 0;
        }

        /// <summary>
        /// Converts the normalized and quantized value back into voltage value.
        /// </summary>
        /// <param name="valueInBits">The normalized and quantized value.</param>
        /// <returns>The value in Volts.</returns>
        private double ConvertBitsToVoltage(uint valueInBits)
        {
            const double STEPS = 65535.0;//2^16 (16-bits)
            const double MIN = MINIMUM_ANALOG_VOLTAGE;
            const double MAX = MAXIMUM_ANALOG_VOLTAGE;

            double unQuantized = valueInBits / STEPS;
            double unNormalized = unQuantized * (MAX - MIN) + MIN;

            return unNormalized;
        }

        /// <summary>
        /// Reads the next value from the specified FIFO.
        /// </summary>
        /// <param name="fifoIndex">Index of the FIFO.</param>
        /// <returns>An instance of type <see cref=" EmptyFIFODecompressionResult"/> if the FIFO is empty, 
        /// or an instance of type <see cref=" AbstractFinishReachedDecompressionResult"/> if the value read indicates a normal end-of-transmission, 
        /// or an instance of type <see cref=" AbstractNumericDecompressionResult"/> if a regular value was read.</returns>
        private IDecompressionResult ReadNextValueFromFIFO(int fifoIndex)
        {
            //If the current value was specified in a previous iteration, then use it!
            if (nextDecompressionResults[fifoIndex].Count > 0)
            {
                return nextDecompressionResults[fifoIndex].Dequeue();
            }

            const uint ESCAPE_VALUE = 0x0001;
            const uint ESCAPE_LITERAL_PADDING_VAL = 0x0000;
            const uint FINISHED_VAL = 0xFFFF;

            AbstractDequeue fifo = fifos[fifoIndex];
            uint currentValue;

            //The sequence should be 
            //<ESC><PAD> (1*1)
            //<ESC><PAD><ESC><FIN> (1*1 then FIN)
            //<ESC><PAD><ESC><rep>, (1*rep)
            //<ESC><FINISHED>, (FIN)
            //<val>, (val*1)
            //<val><ESC><rep>, (val*rep)
            //<val><ESC><FIN>, (val*1 then FIN)
            //<val><ESC><PAD>, (val*1 then 1*1)
            //<val><ESC><PAD><ESC><repetition>, (val*1 then 1*rep)
            //<val><ESC><PAD><ESC><FIN> (val*1 then 1*1 then FIN)!!!
            //or an error!
            if (!fifo.Dequeue(out currentValue))
                return new EmptyFIFODecompressionResult();

            //It should be <ESC><PAD>, <ESC><PAD><ESC><FIN>, <ESC><PAD><ESC><rep> ,or <ESC><FIN>
            if (currentValue == ESCAPE_VALUE)
            {
                if (!fifo.Dequeue(out currentValue))
                    return new EmptyFIFODecompressionResult();

                //It should be <ESC><PAD> , <ESC><PAD><ESC><rep>, or <ESC><PAD><ESC><FIN>
                if (currentValue == ESCAPE_LITERAL_PADDING_VAL)
                {
                    //<ESC><PAD><ESC><rep>, or <ESC><PAD><ESC><FIN>
                    if (fifo.Peek(out currentValue) && currentValue == ESCAPE_VALUE)
                    {
                        //Get rid of ESC
                        fifo.Dequeue(out currentValue);

                        if (!fifo.Dequeue(out currentValue))
                            return new EmptyFIFODecompressionResult();

                        //<ESC><PAD><ESC><FIN> 
                        if (currentValue == FINISHED_VAL)
                        {
                            //(1*1 then FIN)
                            AbstractNumericDecompressionResult currentResult = CreateNumericResult(ESCAPE_VALUE, 1, fifoIndex);
                            nextDecompressionResults[fifoIndex].Enqueue(CreateFinishedResult(currentResult));

                            return currentResult;
                        }
                        else//<ESC><PAD><ESC><rep>
                        {
                            //(1*rep)
                            return CreateNumericResult(ESCAPE_VALUE, currentValue, fifoIndex);
                        }
                    }
                    else//<ESC><PAD>
                    {
                        return CreateNumericResult(ESCAPE_VALUE, 1, fifoIndex);
                    }

                }
                else//It should be <ESC><FIN>
                {
                    if (currentValue == FINISHED_VAL)
                    {
                        //Read the last value output on this channel and return it too!
                        //Is the last value already of type FinishedReached?
                        if (decompressionResults[fifoIndex] is AbstractFinishReachedDecompressionResult)
                            return decompressionResults[fifoIndex];

                        return CreateFinishedResult((decompressionResults[fifoIndex] as AbstractNumericDecompressionResult));
                    }
                    else
                        throw new Exception(String.Format("AdWinDummy: Invalid sequence found in FIFO {0} <ESC>,{1} was found while <ESC>,<FIN> was expected!", fifoIndex, currentValue));
                }

            }
            else
            {
                //<val>, (val*1)
                //<val><ESC><rep>, (val*rep)
                //<val><ESC><FIN>, (val*1 then FIN)
                //<val><ESC><PAD>, (val*1 then 1*1)
                //<val><ESC><PAD><ESC><repetition>, (val*1 then 1*rep)
                //<val><ESC><PAD><ESC><FIN> (val*1 then 1*1 then FIN)!!!

                uint value = currentValue;

                //<val><ESC><rep>, <val><ESC><FIN>, <val><ESC><PAD>, <val><ESC><PAD><ESC><FIN>, or <val><ESC><PAD><ESC><rep>
                if (fifo.Peek(out currentValue) && currentValue == ESCAPE_VALUE)
                {
                    fifo.Dequeue(out currentValue);//Get rid of the <ESC>

                    //if nothing could be read then the sequence is finished abnormally
                    if (!fifo.Dequeue(out currentValue))
                        return new EmptyFIFODecompressionResult();

                    //<val><ESC><PAD>, <val><ESC><PAD><ESC><FIN>, or <val><ESC><PAD><ESC><rep>
                    if (currentValue == ESCAPE_LITERAL_PADDING_VAL)
                    {
                        // <val><ESC><PAD><ESC><FIN>, or <val><ESC><PAD><ESC><rep>
                        if (fifo.Peek(out currentValue) && currentValue == ESCAPE_VALUE)
                        {
                            //Get rid of ESC
                            fifo.Dequeue(out currentValue);

                            //<val> then <empty>!
                            if (!fifo.Dequeue(out currentValue))
                            {
                                AbstractNumericDecompressionResult currentResult = CreateNumericResult(value, 1, fifoIndex);
                                nextDecompressionResults[fifoIndex].Enqueue(new EmptyFIFODecompressionResult());

                                return currentResult;
                            }

                            //<val><ESC><PAD><ESC><FIN> 
                            if (currentValue == FINISHED_VAL)
                            {
                                //(val*1)then (1*1) then (FIN)
                                AbstractNumericDecompressionResult next = CreateNumericResult(ESCAPE_VALUE, 1, fifoIndex);
                                nextDecompressionResults[fifoIndex].Enqueue(next);
                                nextDecompressionResults[fifoIndex].Enqueue(CreateFinishedResult(next));

                                return CreateNumericResult(value, 1, fifoIndex);
                            }
                            else//<val><ESC><PAD><ESC><rep>
                            {
                                //(val*1) then (1*rep)
                                AbstractNumericDecompressionResult currentResult = CreateNumericResult(value, 1, fifoIndex);//val*1
                                nextDecompressionResults[fifoIndex].Enqueue(CreateNumericResult(ESCAPE_VALUE, currentValue, fifoIndex));//1*rep

                                return currentResult;
                            }
                        }
                        else//<val><ESC><PAD>
                        {
                            nextDecompressionResults[fifoIndex].Enqueue(CreateNumericResult(ESCAPE_VALUE, 1, fifoIndex));

                            return CreateNumericResult(value, 1, fifoIndex);
                        }
                    }
                    else
                    {//<val><ESC><FIN> pr <val><ESC><rep>

                        //<val><ESC><FIN>
                        if (currentValue == FINISHED_VAL)
                        {
                            AbstractNumericDecompressionResult currentResult = CreateNumericResult(value, 1, fifoIndex);
                            nextDecompressionResults[fifoIndex].Enqueue(CreateFinishedResult(currentResult));

                            return currentResult;
                        }
                        else//<val><ESC><rep>
                        {
                            return CreateNumericResult(value, currentValue, fifoIndex);
                        }
                    }
                }
                else// <val>
                    return CreateNumericResult(value, 1, fifoIndex);

            }


        }

        /// <summary>
        /// Creates a concrete instance of type <see cref=" AbstractNumericDecompressionResult"/>, based on the type of the FIFO corresponding to the passed <paramref name="fifoIndex"/> value.
        /// </summary>
        /// <param name="readValue">The value read from the FIFO.</param>
        /// <param name="repetitons">The number of repetition corresponding to the read value.</param>
        /// <param name="fifoIndex">Index of the FIFO we have read the value from.</param>
        /// <returns>A concrete instance of type <see cref=" AbstractNumericDecompressionResult"/></returns>
        private AbstractNumericDecompressionResult CreateNumericResult(uint readValue, uint repetitons, int fifoIndex)
        {
            if (fifoIndex < ANALOGS_COUNT)
            {
                return new AnalogNumericDecompressionResult(ConvertBitsToVoltage(readValue), repetitons);
            }
            else
            {
                return new DigitalNumericDecompressionResult(readValue, repetitons);
            }
        }

        /// <summary>
        /// Creates a concrete instance of type <see cref=" AbstractFinishReachedDecompressionResult"/>, based on the type of the FIFO corresponding to the passed <paramref name="previousValue"/> value.
        /// </summary>
        /// <param name="previousValue">The previous numeric value.</param>
        /// <returns>A concrete instance of type <see cref=" AbstractFinishReachedDecompressionResult"/></returns>
        private AbstractFinishReachedDecompressionResult CreateFinishedResult(AbstractNumericDecompressionResult previousValue)
        {
            if (previousValue is AnalogNumericDecompressionResult)
            {
                return new AnalogFinishReachedDecompressionResult(previousValue as AnalogNumericDecompressionResult);
            }
            else
            {
                return new DigitalFinishReachedDecompressionResult(previousValue as DigitalNumericDecompressionResult);
            }
        }


    }
}

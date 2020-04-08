using System.Collections.Generic;
using System.Linq;
using Buffer.OutputProcessors.Quantization;
using Communication.Interfaces.Generator;
using Model.Data;


namespace Buffer.OutputProcessors.Compression
{
    /// <summary>
    /// Compresses the output. It assumes the output of analog cards has already been quantized with 16-bit quantization.
    /// </summary>
    /// <seealso cref="Buffer.OutputProcessors.OutputProcessor" />
    public class OutputCompressor : OutputProcessor
    {
        private IModelOutput output;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputCompressor"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        public OutputCompressor(DataModel model)
            : base(model)
        { }

        /// <summary>
        /// Compresses the output.
        /// </summary>
        /// <param name="output">The output to be compressed.</param>
        private void CompressOutput(IModelOutput output)
        {
            foreach (KeyValuePair<string, ICardOutput> card in output.Output)
            {
                CompressOutputOfCard(card.Value);
            }
        }

        /// <summary>
        /// Compresses the output of a card.
        /// </summary>
        /// <param name="cardOutput">The card's output.</param>
        private void CompressOutputOfCard(ICardOutput cardOutput)
        {
            if (cardOutput is QuantizedAnalogCardOutput)
                CompressOutputOfAnalogCard((QuantizedAnalogCardOutput)cardOutput);
            else
                CompressOutputOfDigitalCard((QuantizedDigitalCardOutput)cardOutput);
        }

        /// <summary>
        /// Compresses the output of an analog card and packs it (two 16-bit values in a single 32-bit variable).
        /// </summary>
        /// <param name="output">The output.</param>
        private void CompressOutputOfAnalogCard(QuantizedAnalogCardOutput output)
        {
            List<uint> currentChannelOutput;
            uint[] conversionResult;
            uint[] packedOutput;

            for (int channel = 0; channel < output.Output.Count; channel++)
            {
                currentChannelOutput = output.Output[channel];
                //Compress output
                conversionResult = CompressOutputOfChannel(currentChannelOutput);
                //As the quantization depth is 16 bits. We fit two values together in a single uint (32 bits)
                packedOutput = PackAnalogChannelOutput(conversionResult);
                output.Output[channel] = new List<uint>(packedOutput);
            }
        }

        /// <summary>
        /// Compresses the output of a digital card.
        /// </summary>
        /// <param name="output">The output.</param>
        private void CompressOutputOfDigitalCard(QuantizedDigitalCardOutput output)
        {
            uint[] result = CompressOutputOfChannel(output.Output);
            output.Output = new List<uint>(result);
        }

        /// <summary>
        /// Packs the output of an analog channel
        /// </summary>
        /// <param name="unpackedOutput">The unpacked output.</param>
        /// <returns>The packed output of the analog channel</returns>
        /// <remarks>Assumes the quantization that was already performed has a depth of 16 bits.</remarks>
        private uint[] PackAnalogChannelOutput(uint[] unpackedOutput)
        {
            List<uint> result = new List<uint>();

            for (int step = 0; step < unpackedOutput.Length; step++)
            {
                //As the quantization depth is 16 bits. We fit two values together in a single uint (32 bits)
                result.Add(unpackedOutput[step] << 16);//Add the first value
                step++;

                if (step < unpackedOutput.Length)
                {
                    result[result.Count() - 1] += unpackedOutput[step];//Add the second value
                }

                //If the output.Length is odd, the last value (i.e. FINISHED_VAL = 0x0000FFFF) will be shifted to the upper 2 bytes of the last uint, and the lower 2 bytes will be 0x0000
                //so the last uint will be then 0xFFFF0000
            }

            return result.ToArray();
        }


        //RECO potential enhancement: don't compress unless the number of repetitions is more than 2
        //RECO potential enhancement: combine the escape value and the value after it in a single value (uint)

        /// <summary>
        /// Converts an uint array to the compressed format that the AdWin system understands.
        /// </summary>
        /// <param name="output">The unconverted (compressed) array of <c>uint</c>'s.</param>
        /// <returns>The converted (compressed) array of <c>uint</c>'s.</returns>
        /// <remarks>
        /// This method compresses the input array so that repeated integers are represented only once.
        /// For example, if we have the input sequence: 2,4,13,2,5,5,5,6,7,7,100,1,1,1,1,3,3 then the converted sequence will be: 
        /// 2,4,13,2,5,1,3,6,7,1,2,100,1,0,1,4,3,1,2,1,65.536 .
        /// After each repetition block the escape value (1) is inserted followed by the number of repetitions within the block.
        /// If the value 1 itself is in the data, it is followed by the value 0 to indicate that it is an actual value rather than an escape character.
        /// At the end of the data the escape value (1) is inserted followed by the finish value (0x0000FFFF = 65.536).
        /// The number of repetitions cannot exceed 30000, otherwise the repetition block would be broken up into 2 or more parts.
        /// </remarks>
        private uint[] CompressOutputOfChannel(List<uint> output)
        {
            List<uint> outputList = new List<uint>();//holds intermediate results
            uint[] realOut;//holds the final result
            uint lastVal = 0;//the last value read
            uint reps = 0;//number of consecutive repetitions for the last value
            const uint ESC_VAL = 0x00000001;//the escape bit
            const uint ESC_LITERAL_PAD = 0x00000000;//added after the escape character when the escape character represents real data not repetition
            const uint FINISHED_VAL = 0x0000FFFF;
            const uint MAX_REPS = 30000;//the largest value allowed after the escape bit to indicate repetitions

            if (output.Count > 0)
            {
                if (output[0] == ESC_VAL)//this if-else is needed to populate lastVal before entering the loop
                {
                    outputList.Add(output[0]);
                    outputList.Add(ESC_LITERAL_PAD);
                }
                else
                {
                    outputList.Add(output[0]);
                }

                lastVal = output[0];
                reps = 0;

                for (int j = 1; j < output.Count; j++)
                {
                    if (output[j] == lastVal)//If the last value is repeated
                    {
                        reps++;//count repetitions

                        if (reps >= MAX_REPS)//if the number of repetitions exceeds the threshold
                        {
                            //insert the number of repetitions in the result
                            outputList.Add(ESC_VAL);
                            outputList.Add(reps);// do not count the current value (not reps + 1)
                            reps = 0;

                            //add the current value to the result
                            if (output[j] == ESC_VAL)
                            {
                                outputList.Add(output[j]);
                                outputList.Add(ESC_LITERAL_PAD);
                            }
                            else
                            {
                                outputList.Add(output[j]);
                            }

                            lastVal = output[j];//redundant
                        }
                    }
                    else//we have a new value
                    {
                        if (reps == 0)//the previous value only occurred once in a row
                        {
                            //add the current value
                            outputList.Add(output[j]);

                            if (output[j] == ESC_VAL)
                            {
                                outputList.Add(ESC_LITERAL_PAD);
                            }
                        }
                        else//the previous value occurred more than once
                        {
                            //add the number of repetitions of the previous value
                            outputList.Add(ESC_VAL);
                            outputList.Add(reps + 1);//take the last occurrence of the previous value into account
                            reps = 0;

                            //add the current value
                            if (output[j] == ESC_VAL)
                            {
                                outputList.Add(output[j]);
                                outputList.Add(ESC_LITERAL_PAD);
                            }
                            else
                            {
                                outputList.Add(output[j]);
                            }
                            //lastVal = output[j];
                        }

                        lastVal = output[j];
                    }
                }

                if (reps > 0)//if input data ends with a repetition block
                {
                    //add the number of repetitions
                    outputList.Add(ESC_VAL);
                    outputList.Add(reps + 1);
                }

                realOut = new uint[outputList.Count + 2];//CHANGED it was outputList.Count Ghareeb Falazi 27.06.2016

                for (int i = 0; i < outputList.Count; i++)
                {
                    realOut[i] = outputList[i];
                }
                // CHANGED: adding finished_val at the end to distinguish the end of sequence, useful for Totman approach Majd 
                realOut[realOut.Length - 2] = ESC_VAL;
                realOut[realOut.Length - 1] = FINISHED_VAL;

                return realOut;

            }
            else
            {
                realOut = new uint[2];
                realOut[0] = ESC_VAL;
                realOut[1] = FINISHED_VAL;

                return realOut;
            }
        }


        /// <summary>
        /// Processes the specified output by compressing it
        /// </summary>
        /// <param name="output">The output to compress.</param>
        /// <remarks>Assumes that the output of analog cards have already been quantized with a 16-bit quantization</remarks>
        public override void Process(IModelOutput output)
        {
            this.output = output;
            CompressOutput(output);
        }
    }
}

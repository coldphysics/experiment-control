using System;
using System.Collections.Generic;
using Communication.Interfaces.Generator;
using Generator.Generator.Concatenator;
using Model.Data;

namespace Buffer.OutputProcessors.Quantization
{
    /// <summary>
    /// Quantizes the output according to:
    /// normalized voltage = (voltage - min) / (max - min) where min = -10, max = +10
    /// quantized voltage = normalized voltage * quantization steps; where quantization steps = 2^16 = 65535.0
    /// </summary>
    /// <seealso cref="Buffer.OutputProcessors.OutputProcessor" />
    public class OutputQuantizer:OutputProcessor
    {
        private readonly double MINIMUM_VOLTAGE = -10.0;
        private readonly double MAXIMUM_VOLTAGE = +10.0;
        private readonly int QUANTIZATION_STEPS = 65535;//2^16

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputQuantizer"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        public OutputQuantizer(DataModel model)
            :base(model)
        {
        }
        /// <summary>
        /// Quantizes the voltage value (16 bits quantization).
        /// </summary>
        /// <param name="input">The input voltage in the range [-10.0, +10.0]</param>
        /// <returns>The quantized voltage, which takes values {0,1,..., 2^16=65535}</returns>
        private UInt32 ConvertVoltageToBits(double input)
        {
            //normalized voltage = (voltage - min) / (max - min) where min = -10, max = +10
            //quantized voltage = normalized voltage * quantization steps; where quantization steps = 2^16 = 65535.0
            return ((UInt32)Math.Round((input - MINIMUM_VOLTAGE) / (MAXIMUM_VOLTAGE - MINIMUM_VOLTAGE) * QUANTIZATION_STEPS));
        }


        /// <summary>
        /// Processes the specified output by quantizing 
        /// </summary>
        /// <param name="output">The output.</param>
        /// <remarks> This method changes the type of the output of the cards from <see cref=" AnalogCardOutput"/> or <see cref=" DigitalCardOutput"/> to either <see cref=" QuantizedAnalogCardOutput"/> or <see cref=" QuantizedDigitalCardOutput"/> </remarks>
        public override void Process(IModelOutput output)
        {
            ICardOutput currentCard;
            Dictionary<string, ICardOutput> newCards = new Dictionary<string, ICardOutput>();

            foreach (KeyValuePair<string, ICardOutput> card in output.Output)
            {
                if (card.Value is AnalogCardOutput)
                {
                    AnalogCardOutput analogCard = (AnalogCardOutput)card.Value;

                    currentCard = new QuantizedAnalogCardOutput();
                    for (int channel = 0; channel < analogCard.Output.GetLength(0); channel++)
                    {
                        ((QuantizedAnalogCardOutput)currentCard).Output.Add(new List<uint>());
                        for (int step = 0; step < analogCard.Output.GetLength(1); step++)
                        {
                            ((QuantizedAnalogCardOutput)currentCard).Output[channel].Add(ConvertVoltageToBits(analogCard.Output[channel, step]));
                        }
                    }
                }
                else
                {
                    currentCard = new QuantizedDigitalCardOutput();

                    ((QuantizedDigitalCardOutput)currentCard).Output = new List<uint>(((DigitalCardOutput)card.Value).Output);
                }

                newCards[card.Key] = currentCard;
            }

            foreach (KeyValuePair<string, ICardOutput> card in newCards)
            {
                output.Output[card.Key] = card.Value;
            }
        }
    }
}

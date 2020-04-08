using System;
using Communication.Interfaces.Generator;
using Generator.Generator.Concatenator;
using Model.Data;
using Model.Data.Cards;
using Model.Data.Channels;
using Model.Settings;

namespace Buffer.OutputProcessors.ValidationUnit
{
    [Serializable]
    public class ValidationException : Exception
    {
        public ValidationException() { }
        public ValidationException(string message) : base(message) { }
        public ValidationException(string message, Exception inner) : base(message, inner) { }
        protected ValidationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    /// <summary>
    /// Provides functionality to validate the output sequence in its final form against the upper and lower output limits .
    /// </summary>
    class OutputValidator:OutputProcessor
    {
        public OutputValidator(DataModel model)
            : base(model)
        { }
        /// <summary>
        /// Validates that the output of all analog cards against the upper and lower limits specified for each the output of each channel.
        /// </summary>

        /// <exception cref="ValidationException">Thrown when the output is not valid.</exception>
        public override void Process(IModelOutput output)
        {
            int cardIndex = 0;


            foreach (string cardName in output.Output.Keys)
            {
                ICardOutput card = output.Output[cardName];

                if (card is AnalogCardOutput)
                {
                    double[,] cardOutput = (card as AnalogCardOutput).Output;

                    if (cardOutput != null)
                    {
                        CardBasicModel cardModel = dataModel.group.Cards[cardIndex];

                        for (int channelIndex = 0; channelIndex < cardOutput.GetLength(0); channelIndex++)
                        {
                            ChannelSettingsModel channelSettings = cardModel.Settings[channelIndex];

                            for (int step = 0; step < cardOutput.GetLength(1); step++)
                            {
                                if (AreLimitsViolated(cardOutput[channelIndex, step], channelSettings.UpperLimit, channelSettings.LowerLimit))
                                {
                                    string time = String.Format("{0}{1}", step * TimeSettingsInfo.GetInstance().SmallestTimeStep, TimeSettingsInfo.GetInstance().TimeUnit);
                                    string errorMessage = String.Format("Value outside the allowed Output limits (Value = {0}) at (Card: {1}, Channel: {2}, Time: {3})", cardOutput[channelIndex, step], cardModel.Name, channelSettings.Channel, time);
                                    throw new ValidationException(errorMessage);
                                }
                            }
                        }
                    }
                }

                cardIndex++;
            }

        }

        /// <summary>
        /// Checks whether the upper or lower limits of a specific time step are violated
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="upperLimit">The upper limit.</param>
        /// <param name="lowerLimit">The lower limit.</param>
        /// <returns><c>true</c> if a violation is detected.</returns>
        /// <remarks>
        /// This method accepts a violation of at most 1 microvolt due to floating-point imprecision.
        /// </remarks>
        private bool AreLimitsViolated(double value, double upperLimit, double lowerLimit)
        {
            const double PRECISION = 0.000001;//Microvolt

            return (value - PRECISION > upperLimit || value + PRECISION < lowerLimit);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Communication.Interfaces.Generator;
using Generator.Generator.Concatenator;
using Model.Data;
using Model.Data.Cards;
using Model.Data.Channels;

namespace Buffer.ValidationUnit
{
    /// <summary>
    /// Provides functionality to validate the output sequence in its final form against the upper and lower output limits .
    /// </summary>
    class ValidationUnit
    {
        /// <summary>
        /// Validates that the output of all analog cards against the upper and lower limits specified for each the output of each channel.
        /// </summary>
        /// <param name="output">The output to validate.</param>
        /// <param name="model">The model that contains the output limits for each channel.</param>
        /// <param name="errorMessage">The possible error message.</param>
        /// <returns></returns>
        public static bool ValidateLimits(Dictionary<string, ICardOutput> output, DataModel model, out string errorMessage)
        {
            int cardIndex = 0;
            errorMessage = "Output valid!";

            foreach (string cardName in output.Keys)
            {
                ICardOutput card = output[cardName];

                if (card is AnalogCardOutput)
                {
                    double[,] cardOutput = (card as AnalogCardOutput).Output;

                    if (cardOutput != null)
                    {
                        CardBasicModel cardModel = model.Groups.Single().Value.Cards[cardIndex];

                        for (int channelIndex = 0; channelIndex < cardOutput.GetLength(0); channelIndex++)
                        {
                            ChannelSettingsModel channelSettings = cardModel.Settings[channelIndex];

                            for (int step = 0; step < cardOutput.GetLength(1); step++)
                            {
                                if (cardOutput[channelIndex, step] > channelSettings.UpperLimit || cardOutput[channelIndex, step] < channelSettings.LowerLimit)
                                {
                                    string time = String.Format("{0}{1}", step * model.Groups.Single().Value.Settings.SmallestTimeStep, model.Groups.Single().Value.Settings.Unit);
                                    errorMessage = String.Format("Value outside the allowed output limits (value = {0}) at (Card: {1}, Channel: {2}, Time: {3})", cardOutput[channelIndex, step], cardModel.Name, channelSettings.Channel, time);
                                    return false;
                                }
                            }
                        }
                    }
                }

                cardIndex++;
            }

            return true;
        }


    }
}

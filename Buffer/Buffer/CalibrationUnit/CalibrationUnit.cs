using System;
using System.Collections.Generic;
using System.Linq;
using Communication.Interfaces.Generator;
using Generator.Generator.Concatenator;
using Model.Data;
using Model.Data.Cards;
using Model.Data.Channels;
using Model.Data.SequenceGroups;
using Model.Variables;

namespace Buffer.CalibrationUnit
{
    /// <summary>
    /// Manages the calibration of all analog cards
    /// </summary>
    public class CalibrationUnit
    {
        /// <summary>
        /// The number of channels in a digital card.
        /// </summary>
        private const int CHANNELS_PER_ANALOG_CARD = 8;

        /// <summary>
        /// The set of channel calibrators used by this instance.
        /// </summary>
        private List<ChannelCalibrator> calibrators = new List<ChannelCalibrator>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CalibrationUnit"/> class.
        /// </summary>
        /// <param name="model">The data model.</param>
        public CalibrationUnit(DataModel model)
        {
            VariablesModel variablesModel = model.variablesModel;
            SequenceGroupModel sequenceGroup = model.Groups.Single().Value;//Breaks if sequencegroup does not have exactly 1 element

            foreach (CardBasicModel card in sequenceGroup.Cards)//Loop through all analog cards
            {
                if (card.Type == CardBasicModel.CardType.Analog)
                {
                    foreach (ChannelSettingsModel setting in card.Settings)//Loop through all channels of the card
                    {
                        if (setting.UseCalibration)
                            //Assumes that the index of the channel is the preserved when traversing in this way
                            calibrators.Add(new ChannelCalibrator(setting.CalibrationScript, variablesModel));
                        else
                            calibrators.Add(null);
                    }
                }
            }
        }

        /// <summary>
        /// Generates the error message.
        /// </summary>
        /// <param name="cardIndex">Index of the card.</param>
        /// <param name="channelIndex">Index of the channel.</param>
        /// <param name="e">The exception.</param>
        /// <returns></returns>
        private static string GenerateErrorMessage(int cardIndex, int channelIndex, Exception e, bool includeChannelInfo)
        {
            string errorMessage = e.Message;


            if (e is MissingMemberException)
                errorMessage = string.Format("The special python variable \"{0}\" must be assigned to in the script!", ChannelCalibrator.PYTHON_VARIABLE_NAME_FOR_CALIBRATED_OUPTUT);
            else if (e is FormatException)
                errorMessage = string.Format("The value of the special variable \"{0}\" should be numeric.", ChannelCalibrator.PYTHON_VARIABLE_NAME_FOR_CALIBRATED_OUPTUT);
            else
                errorMessage = e.Message;

            if (includeChannelInfo)
                errorMessage = String.Format("Error while evaluating the python calibration script for (Card: A{0}, Channel: {1}).\nDetails: {2}", cardIndex, channelIndex, errorMessage);

            return errorMessage;
        }

        /// <summary>
        /// Checks whether a script is a valid Python script and ensures that the special output python variable is assigned a value.
        /// </summary>
        /// <param name="script">The script.</param>
        /// <param name="variablesModel">The variables model.</param>
        /// <param name="errorMessage">The error message returned if the script is invalid.</param>
        /// <returns><c>true</c> if the script is valid, <c>false</c> otherwise.</returns>
        public static bool ValidatePythonScript(int cardIndex, int channelIndex, string script, VariablesModel variablesModel, out string errorMessage, bool includeChannelInfoInErrorMsg)
        {
            try
            {
                ChannelCalibrator calibrator = new ChannelCalibrator(script, variablesModel);

                double result = calibrator.CalibrateValue(5);//Dummy test
                errorMessage = "Script is fine!";

                return true;



            }
            catch (InvalidCastException e)
            {
                errorMessage = string.Format("The special python variable \"{0}\" must be used in the script!", ChannelCalibrator.PYTHON_VARIABLE_NAME_FOR_UNCALIBRATED_OUPTUT);

                return false;
            }
            catch (Exception e)
            {
                errorMessage = GenerateErrorMessage(cardIndex, channelIndex, e, includeChannelInfoInErrorMsg);

                return false;
            }
        }

        /// <summary>
        /// Evaluates the specified script.
        /// </summary>
        /// <param name="script">The script.</param>
        /// <param name="variablesModel">The variables model.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static double Evaluate(string script, VariablesModel variablesModel, double input)
        {
            ChannelCalibrator calibrator = new ChannelCalibrator(script, variablesModel);
            double result = calibrator.CalibrateValue(input);

            return result;
        }

        /// <summary>
        /// Calibrates the output.
        /// </summary>
        /// <param name="output">The output to calibrate in the format:(cardName, cardOutput).</param>
        /// <param name="errorMessage">The error message returned if the calibration process fails.</param>
        /// <returns><c>true</c> when the calibration process succeeds, <c>false</c> otherwise.</returns>
        public bool CalibrateOutput(Dictionary<string, ICardOutput> output, out string errorMessage)
        {
            int cardIndex = 0;
            errorMessage = "Calibration Successful!";
            try
            {
                foreach (string cardName in output.Keys)
                {
                    ICardOutput card = output[cardName];

                    if (card is AnalogCardOutput)
                    {
                        double[,] cardOutput = (card as Generator.Generator.Concatenator.AnalogCardOutput).Output;
                        CalibrateAnalogCard(cardOutput, cardIndex);
                    }

                    cardIndex++;
                }

                return true;
            }
            catch (Exception e)
            {
                errorMessage = e.Message;

                return false;
            }
        }

        /// <summary>
        /// Calibrates the output of an analog card.
        /// </summary>
        /// <param name="cardOutput">The channel output.</param>
        /// <exception cref="System.Exception">Thrown when an error occurs while evaluating the Python script used for calibration.</exception>
        private void CalibrateAnalogCard(double[,] cardOutput, int cardIndex)
        {

            if (cardOutput != null)
            {
                for (int i = 0; i < cardOutput.GetLength(0); i++)
                {
                    try
                    {
                        CalibrateChannel(cardOutput, cardIndex, i);
                    }
                    catch (Exception e)
                    {
                        string errorMessage = GenerateErrorMessage(cardIndex, i, e, true);
                        throw new Exception(errorMessage, e);
                    }
                }
            }
        }

        /// <summary>
        /// Calibrates the output of a single channel. The channel will not be calibrated if its settings does not ask for it.
        /// </summary>
        /// <param name="channelOutput">The channel output.</param>
        /// <param name="channelIndex">Index of the channel.</param>
        private void CalibrateChannel(double[,] channelOutput, int cardIndex, int channelIndex)
        {
            ChannelCalibrator calibrator = calibrators[cardIndex * CHANNELS_PER_ANALOG_CARD + channelIndex];

            if (calibrator != null)
            {
                for (int i = 0; i < channelOutput.GetLength(1); i++)
                {
                    channelOutput[channelIndex, i] = calibrator.CalibrateValue(channelOutput[channelIndex, i]);
                }
            }
        }
    }
}

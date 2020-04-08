using System;
using System.Collections.Generic;
using Communication.Interfaces.Generator;
using Generator.Generator.Concatenator;
using Model.Data;
using Model.Data.Cards;
using Model.Data.Channels;
using Model.Data.SequenceGroups;
using PythonUtils.ScriptAnalysis;

namespace Buffer.OutputProcessors.CalibrationUnit
{
    /// <summary>
    /// An exception that is thrown when a calibration error is detected.
    /// </summary>
    [Serializable]
    public class CalibrationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalibrationException"/> class.
        /// </summary>
        public CalibrationException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalibrationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CalibrationException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="CalibrationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner exception.</param>
        public CalibrationException(string message, Exception inner) : base(message, inner) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="CalibrationException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected CalibrationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    /// <summary>
    /// Manages the calibration of all analog cards
    /// </summary>
    public class OutputCalibrator : OutputProcessor
    {
        /// <summary>
        /// The analyzer. (singleton)
        /// </summary>
        private static CalibrationScriptAnalyzer analyzer;

        /// <summary>
        /// Gets the analyzer.
        /// </summary>
        /// <value>
        /// The analyzer.
        /// </value>
        /// <remarks>This getter instantiates a new analyzer instance only if it has not already been instantiated (singleton)</remarks>
        public static CalibrationScriptAnalyzer Analyzer
        {
            get 
            {
                if (analyzer == null)
                    analyzer = new CalibrationScriptAnalyzer(ChannelCalibrator.PYTHON_VARIABLE_NAME_FOR_CALIBRATED_OUPTUT, 
                        new string[] { ChannelCalibrator.PYTHON_VARIABLE_NAME_FOR_UNCALIBRATED_OUPTUT });

                return analyzer; 
            }

        }
        /// <summary>
        /// The number of channels in a digital card.
        /// </summary>
        private readonly int CHANNELS_PER_ANALOG_CARD;

        /// <summary>
        /// The set of channel calibrators used by this instance.
        /// </summary>
        private List<ChannelCalibrator> calibrators = new List<ChannelCalibrator>();

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputCalibrator"/> class.
        /// </summary>
        /// <param name="model">The data model.</param>
        public OutputCalibrator(DataModel model)
            : base(model)
        {
            CHANNELS_PER_ANALOG_CARD = Global.GetNumAnalogChannelsPerCard();

            SequenceGroupModel sequenceGroup = model.group;//Breaks if sequencegroup does not have exactly 1 element

            foreach (CardBasicModel card in sequenceGroup.Cards)//Loop through all analog cards
            {
                if (card.Type == CardBasicModel.CardType.Analog)
                {
                    foreach (ChannelSettingsModel setting in card.Settings)//Loop through all channels of the card
                    {
                        if (setting.UseCalibration)
                            //Assumes that the index of the channel is the preserved when traversing in this way
                            calibrators.Add(new ChannelCalibrator(setting.CalibrationScript, model));
                        else
                            calibrators.Add(null);
                    }
                }
            }
        }


        /// <summary>
        /// Checks whether a script is a valid Python script and ensures that the special output python variable is assigned a value.
        /// </summary>
        /// <param name="cardName">Name of the card.</param>
        /// <param name="channelIndex">Index of the channel.</param>
        /// <param name="script">The script.</param>
        /// <param name="dataModel">The variables model.</param>
        /// <param name="errorMessage">The error message returned if the script is invalid.</param>
        /// <param name="includeChannelInfoInErrorMsg">if set to <c>true</c> then the script location information will be included in error MSG].</param>
        /// <returns>
        ///   <c>true</c> if the script is valid, <c>false</c> otherwise.
        /// </returns>
        public static bool ValidatePythonScript(string cardName, int channelIndex, string script, DataModel dataModel, out string errorMessage, bool includeChannelInfoInErrorMsg)
        {
            ChannelBasedScriptLocation location = null;

            if(includeChannelInfoInErrorMsg)
                location = new ChannelBasedScriptLocation(){CardName = cardName, ChannelIndex=channelIndex};

            return Analyzer.ValidatePythonScript(location, script, dataModel, out errorMessage);
        }


        /// <summary>
        /// Calibrates the output
        /// </summary>
        /// <exception cref="CalibrationException">Thrown when an error occurs while calibration the output</exception>
        public override void Process(IModelOutput output)
        {
            try
            {
                CalibrateOutput(output);
            }
            catch (Exception e)
            {
                throw new CalibrationException(e.Message, e);
            }
        }

        /// <summary>
        /// Calibrates the output.
        /// </summary> 
        private void CalibrateOutput(IModelOutput output)
        {
            int cardIndex = 0;

            foreach (string cardName in output.Output.Keys)
            {
                ICardOutput card = output.Output[cardName];

                if (card is AnalogCardOutput)
                {
                    double[,] cardOutput = (card as Generator.Generator.Concatenator.AnalogCardOutput).Output;
                    CalibrateAnalogCard(cardOutput, cardIndex, cardName);
                }

                cardIndex++;
            }
        }

        /// <summary>
        /// Calibrates the output of an analog card.
        /// </summary>
        /// <param name="cardOutput">The channel output.</param>
        /// <exception cref="System.Exception">Thrown when an error occurs while evaluating the Python script used for calibration.</exception>
        private void CalibrateAnalogCard(double[,] cardOutput, int cardIndex, string cardName)
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
                        ChannelBasedScriptLocation location = new ChannelBasedScriptLocation() { CardName = cardName, ChannelIndex = i };
                        string errorMessage = Analyzer.GenerateErrorMessage(location, e);
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
                Dictionary<double, double> calibrationMap = new Dictionary<double, double>();//Used to boost performance
                double current;

                for (int i = 0; i < channelOutput.GetLength(1); i++)
                {
                    current = channelOutput[channelIndex, i];//The current uncalibrated value

                    if (calibrationMap.ContainsKey(current))//Check if we have seen this value before
                        channelOutput[channelIndex, i] = calibrationMap[current];//No need to run the script, just get it from the dict
                    else//It's new; calculate it and store it in the dict.
                    {
                        channelOutput[channelIndex, i] = calibrator.CalibrateValue(current);
                        calibrationMap[current] = channelOutput[channelIndex, i];
                    }

                }
            }
        }
    }
}

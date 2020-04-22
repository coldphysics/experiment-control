using Model.BaseTypes;
using Model.Data;
using Model.Data.Cards;
using Model.Data.Channels;
using Model.Data.SequenceGroups;
using Model.Data.Sequences;
using Model.Data.Steps;
using Model.Root;
using Model.Variables;
using System;
using System.Text.RegularExpressions;

namespace Model.V1
{
    /// <summary>
    /// Converts a model of version 1 to a model of the current version
    /// </summary>
    public class ModelConverter
    {

        public RootModel ConvertToCurrentVersion(Model.V1.Root.RootModel root)
        {
            RootModel result = new RootModel();
            result.EstimatedStartTime = root.EstimatedStartTime;
            result.GlobalCounter = root.GlobalCounter;
            result.IsItererating = root.IsItererating;
            result.VariablesList = root.VariablesList;
            result.EveryCycleNonTimeCriticalPythonPath = "";
            result.EveryCycleTimeCriticalPythonFilePath = "";
            result.StartofScanNonTimeCriticalPythonFilePath = "";
            result.StartofScanTimeCriticalPythonFilePath = "";
            ConvertDataModel(result.Data, root.Data);

            return result;
        }

        #region Converting DataModel
        private void ConvertDataModel(DataModel newData, V1.Data.DataModel oldData)
        {
            //Discard group name
            newData.variablesModel = ConvertVariablesModel(oldData.variablesModel);

            foreach (string groupName in oldData.Groups.Keys)
            {
                newData.group = ConvertSequenceGroup(newData, oldData.Groups[groupName]);
            }

        }
        #endregion

        #region Converting Variables
        //Copying Variables
        private VariablesModel ConvertVariablesModel(V1.Variables.VariablesModel variables)
        {
            VariablesModel result = new VariablesModel();
            result.GroupNames = variables.GroupNames;

            foreach (V1.Variables.VariableModel variable in variables.VariablesList)
            {
                result.VariablesList.Add(ConvertVariableModel(variable));
            }

            return result;
        }

        private VariableModel ConvertVariableModel(V1.Variables.VariableModel variable)
        {
            VariableModel result = new VariableModel();
            result.groupIndex = variable.groupIndex;
            result.TypeOfVariable = variable.TypeOfVariable;
            result.VariableCode = variable.VariableCode;
            result.VariableEndValue = variable.VariableEndValue;
            result.VariableName = variable.VariableName;
            result.VariableStartValue = variable.VariableStartValue;
            result.VariableStepValue = variable.VariableStepValue;
            result.VariableValue = variable.VariableValue;

            return result;
        }
        #endregion

        #region Converting SequenceGroup
        private SequenceGroupModel ConvertSequenceGroup(DataModel parent, V1.Data.SequenceGroups.SequenceGroupModel group)
        {
            //Discard group name
            SequenceGroupModel result = new SequenceGroupModel(parent);

            foreach (V1.Data.Cards.CardBasicModel card in group.Cards)
            {
                result.Cards.Add(ConvertCard(result, card));
            }
            return result;
        }
        #endregion

        #region Converting Time-Based Settings
        private void ConvertSettings(V1.Data.SequenceGroups.Settings settings)
        {
            System.Diagnostics.Debug.Write("TimeSettings from model version V1 was discarded");
        }
        #endregion

        #region Converting Cards
        private CardBasicModel ConvertCard(SequenceGroupModel parent, V1.Data.Cards.CardBasicModel card)
        {
            //Choose a suitable name for the card
            string expectedNamePattern = string.Format("({0}|{1})\\d+", CardBasicModel.ANALOG_CARD_BASE_NAME, CardBasicModel.DIGITAL_CARD_BASE_NAME);
            string name;

            if (Regex.IsMatch(card.Name, expectedNamePattern))
                name = card.Name;
            else
            {
                string findNumberPattern = "\\w*(\\d+)\\w*";
                Match match = Regex.Match(card.Name, findNumberPattern);
                int number = 0;

                if (match.Success)//if not success it is a not expected situation
                {
                    number = Int32.Parse(match.Groups[1].Value);
                }

                name = string.Format("{0}{1}", (card.Type == Data.Cards.CardBasicModel.CardType.Analog) ?
                    CardBasicModel.ANALOG_CARD_BASE_NAME : CardBasicModel.DIGITAL_CARD_BASE_NAME, number);

            }

            //startIndex is discarded
            CardBasicModel result = new CardModel(name, card.NumberOfChannels,  (CardBasicModel.CardType)card.Type, parent);
            int channel = 0;

            foreach (V1.Data.Channels.ChannelSettingsModel settings in card.Settings)
            {
                result.Settings.Add(ConvertChannelSettings(result, channel, settings));
                channel++;
            }

            foreach (V1.Data.Sequences.SequenceModel sequence in card.Sequences)
            {
                result.Sequences.Add(ConvertSequence(result, sequence));
            }
            return result;
        }
        #endregion

        #region Converting ChannelSettings
        private ChannelSettingsModel ConvertChannelSettings(CardBasicModel parent, int channel, V1.Data.Channels.ChannelSettingsModel channelSettings)
        {
            //Discard the channel number stored within settings
            ChannelSettingsModel result = new ChannelSettingsModel(channel, parent);
            result.CalibrationScript = "";
            //result.InitValue = channelSettings.InitValue;
            // result.InitValue = 0;
            if (channelSettings.InitValue != 0)
            {
                Console.WriteLine("Warning! Ignoring a non-zero init value for channel {0}", channel);
            }

            result.Invert = channelSettings.Invert;
            result.LowerLimit = channelSettings.LowerLimit;
            result.Name = channelSettings.Name;
            result.UpperLimit = channelSettings.UpperLimit;
            result.InputUnit = channelSettings.VoltageUnit;

            //result.UseCalibrationFile = channelSettings.UseCalibrationFile;
            //result.OffsetValue = channelSettings.OffsetValue;
            //result.MultiplicatorValue = channelSettings.MultiplicatorValue;
            //result.CalibrationFilePath = channelSettings.CalibrationFilePath;

            //If the original model uses the old way of calibration
            if (channelSettings.MultiplicatorValue != 1 || channelSettings.OffsetValue != 0)
            {
                result.UseCalibration = true;
                result.CalibrationScript = string.Format("cal = uncal * {0} + {1}", channelSettings.MultiplicatorValue, channelSettings.OffsetValue);
            }

            return result;
        }
        #endregion

        #region Converting Sequence
        private SequenceModel ConvertSequence(CardBasicModel parent, V1.Data.Sequences.SequenceModel sequence)
        {
            SequenceModel result = new SequenceModel(parent);
            result.Name = sequence.Name;
            result.startTime = sequence.startTime;

            foreach (V1.Data.Channels.ChannelBasicModel channel in sequence.Channels)
            {
                result.Channels.Add(ConvertChannel(result, channel));
            }

            return result;
        }
        #endregion

        #region Converting Channel
        private ChannelBasicModel ConvertChannel(SequenceModel parent, V1.Data.Channels.ChannelBasicModel channel)
        {
            ChannelBasicModel result = new ChannelModel(parent);

            foreach (V1.Data.Steps.StepBasicModel step in channel.Steps)
            {
                result.Steps.Add(ConvertStep(result, step));
            }

            return result;
        }
        #endregion

        #region Converting Steps
        private StepBasicModel ConvertStep(ChannelBasicModel parent, V1.Data.Steps.StepBasicModel step)
        {
            StepBasicModel result = null;

            if (step is V1.Data.Steps.StepFileModel)
            {
                V1.Data.Steps.StepFileModel fileStep = (V1.Data.Steps.StepFileModel)step;

                result = new StepFileModel(parent, (StepFileModel.StoreType)fileStep.Store);
                (result as StepFileModel).FileName = fileStep.FileName;
            }
            else
            {
                V1.Data.Steps.StepRampModel rampStep = (V1.Data.Steps.StepRampModel)step;

                result = new StepRampModel(parent, (StepRampModel.StoreType)rampStep.Store);
            }

            //Common
            result.Duration = ConvertValueDoubleModel(step.Duration);
            result.DurationVariableName = step.DurationVariableName;
            result.MessageState = step.MessageState;
            result.MessageString = step.MessageString;
            result.StartTime = step.StartTime;
            result.Value = ConvertValueDoubleModel(step.Value);
            result.ValueVariableName = step.ValueVariableName;

            return result;
        }
        #endregion

        #region Converting ValueDoubleModel
        private ValueDoubleModel ConvertValueDoubleModel(V1.BaseTypes.ValueDoubleModel valueModel)
        {
            ValueDoubleModel result = new ValueDoubleModel
            {
                Name = valueModel.Name,
                Value = valueModel.Value
            };

            return result;
        }
        #endregion

    }
}

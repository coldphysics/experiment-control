using Model.Data.SequenceGroups;
using Model.Data.Steps;
using Model.Root;
using System.Text;

namespace Model.Utilities
{
    public class ModelChangeChecker
    {
        /// <summary>
        /// Checks the structural and content differences between two Models
        /// </summary>
        /// <param name="oldModel">The old model</param>
        /// <param name="newModel">The new model</param>
        /// <returns>
        /// A StringBuilder that contains a textual description of the differences between the two models.
        /// Will contain a string of length 0 if no changes are detected, or if the oldModel is null.
        /// </returns>
        public static StringBuilder DescribeModelChanges(RootModel oldModel, RootModel newModel)
        {
            StringBuilder LogString = new StringBuilder("");

            if (oldModel != null)
            {
                //Step 1: Compare single Steps.
                SequenceGroupModel group = oldModel.Data.group;
                SequenceGroupModel groupNew = newModel.Data.group;

                if (group.Cards.Count != groupNew.Cards.Count)
                {
                    //System.Console.WriteLine("Number of cards not equal! In group {0}", group.Name);
                    LogString.Append("Number of cards not equal");
                }
                else
                {
                    for (int j = 0; j < group.Cards.Count; j++)
                    {
                        Data.Cards.CardBasicModel card = group.Cards[j];
                        Data.Cards.CardBasicModel cardNew = groupNew.Cards[j];
                        if (card.Name != cardNew.Name)
                        {
                            //System.Console.WriteLine("Card names not equal! old: {0} new: {1}", card.Name, cardNew.Name);
                            LogString.Append("Card names not equal \t" + card.Name +
                                             " --> " + cardNew.Name + "\n");
                        }
                        if (card.Sequences.Count != cardNew.Sequences.Count)
                        {
                            //System.Console.WriteLine("Number of sequences not equal! In card {0}", card.Name);
                            LogString.Append("Number of sequences not in card \"" +
                                             card.Name + "\"\n");
                        }
                        else
                        {
                            for (int k = 0; k < card.Sequences.Count; k++)
                            {
                                Data.Sequences.SequenceModel sequence = card.Sequences[k];
                                Data.Sequences.SequenceModel sequenceNew = cardNew.Sequences[k];

                                if (sequence.Name != sequenceNew.Name)
                                {
                                    //System.Console.WriteLine("Sequence names not equal! old: {0} new: {1}", sequence.Name, sequenceNew.Name);
                                    LogString.Append("Sequence names not equal in \"" + card.Name + "\"\t" +
                                                     sequence.Name + " --> " +
                                                     sequenceNew.Name + "\n");
                                }

                                if (sequence.Channels.Count != sequenceNew.Channels.Count)
                                {
                                    //System.Console.WriteLine("Number of channels not equal! In sequence {0}", sequence.Name);
                                    LogString.Append("Number of channels not equal in \"" + card.Name + "\", \"" + sequence.Name + "\"\n");
                                }
                                else
                                {
                                    for (int l = 0; l < sequence.Channels.Count; l++)
                                    {
                                        Data.Channels.ChannelBasicModel channel = sequence.Channels[l];
                                        Data.Channels.ChannelBasicModel channelNew = sequenceNew.Channels[l];

                                        if (k == 0)
                                        {
                                            if (channel.Setting.Name != channelNew.Setting.Name)
                                            {
                                                //System.Console.WriteLine("Channel names not equal! old: {0} new: {1}", channel.Setting.Name, channelNew.Setting.Name);
                                                LogString.Append("Channel names not equal in \"" + card.Name + "\"\t" + channel.Setting.Name +
                                                                 " --> " + channelNew.Setting.Name + "\n");
                                            }

                                            if (channel.Setting.LowerLimit != channelNew.Setting.LowerLimit)
                                            {
                                                LogString.Append("Channel lower limit not equal in \"" + card.Name + "\", \"" + channel.Setting.Name +
                                                                 "\"\t" +
                                                                 channel.Setting.LowerLimit +
                                                                 " --> " +
                                                                 channelNew.Setting.LowerLimit +
                                                                 "\n");
                                            }

                                            if (channel.Setting.UpperLimit != channelNew.Setting.UpperLimit)
                                            {
                                                LogString.Append("Channel lower limit not equal in \"" + card.Name + "\", \"" + channel.Setting.Name +
                                                                 "\"\t" +
                                                                 channel.Setting.UpperLimit +
                                                                 " --> " +
                                                                 channelNew.Setting.UpperLimit +
                                                                 "\n");
                                            }

                                            if (channel.Setting.Invert != channelNew.Setting.Invert)
                                            {
                                                LogString.Append("Channel inverting not equal in \"" + card.Name + "\", \"" + channel.Setting.Name +
                                                                 "\"\t" +
                                                                 channel.Setting.Invert +
                                                                 " --> " +
                                                                 channelNew.Setting.Invert +
                                                                 "\n");
                                            }

                                            if (channel.Setting.UseCalibration != channelNew.Setting.UseCalibration)
                                            {
                                                LogString.Append(
                                                        "Channel settings usage of \"use calibration\" in \"" + card.Name + "\", \"" +
                                                        channel.Setting.Name + "\"\t" + channel.Setting.UseCalibration +
                                                        " --> " + channelNew.Setting.UseCalibration + "\n");
                                            }
                                            else
                                            {
                                                if (channel.Setting.UseCalibration)
                                                {
                                                    if (channel.Setting.InputUnit != channelNew.Setting.InputUnit)
                                                    {
                                                        LogString.Append("Channel input unit not equal in \"" + card.Name + "\", \"" +
                                                            channel.Setting.Name + "\"\t" +
                                                            channel.Setting.InputUnit +
                                                            " --> " +
                                                            channelNew.Setting.InputUnit +
                                                            "\n");
                                                    }
                                                }
                                            }

                                            //Compare the scripts even if the calibration is disabled! (Ask Felix)
                                            if (channel.Setting.CalibrationScript != channelNew.Setting.CalibrationScript)
                                            {
                                                LogString.Append("Channel calibration Script not equal in \"" + card.Name + "\", \"" +
                                                    channel.Setting.Name + "\"\t" +
                                                    channel.Setting.CalibrationScript +
                                                    " --> " +
                                                    channelNew.Setting.CalibrationScript +
                                                    "\n");
                                            }
                                        }

                                        if (channel.Steps.Count != channelNew.Steps.Count)
                                        {
                                            //System.Console.WriteLine("Number of steps not equal! In channel {0}", channel.Setting.Name);
                                            LogString.Append("Number of steps not equal in \"" + card.Name + "\",  \"" + sequence.Name +
                                                             "\", \"" + channel.Setting.Name + "\"\n");
                                        }
                                        else
                                        {
                                            for (int m = 0; m < channel.Steps.Count; m++)
                                            {
                                                StepBasicModel step = channel.Steps[m];
                                                StepBasicModel stepNew = channelNew.Steps[m];

                                                if (step.GetType() != stepNew.GetType())
                                                {
                                                    //System.Console.WriteLine("Step type changed in channel {2} at step {3}! old: {0} new: {1}",step.GetType(), stepNew.GetType(),channel.Setting.Name, m);
                                                    LogString.Append("Step types not equal in \"" + card.Name + "\",  \"" +
                                                                     sequence.Name + "\", \"" +
                                                                     channel.Setting.Name + "\", step \"" +
                                                                     (m + 1) + "\"\t" + step.GetType() + " --> " +
                                                                     stepNew.GetType() + "\n");
                                                }
                                                else
                                                {

                                                    if (step.GetType() == typeof(StepFileModel))
                                                    {
                                                        if (((StepFileModel)step).FileName != ((StepFileModel)step).FileName)
                                                        {
                                                            //System.Console.WriteLine("Step filename changed in channel {2} at step {3}! old: {0} new: {1}",((StepFileModel)step).FileName, ((StepFileModel)step).FileName,channel.Setting.Name, m);
                                                            LogString.Append("Step filenames not equal in \"" + card.Name +
                                                                             "\",  \"" + sequence.Name +
                                                                             "\", \"" + channel.Setting.Name +
                                                                             "\", step \"" + (m + 1) + "\"\t" +
                                                                             ((StepFileModel)step).FileName +
                                                                             " --> " +
                                                                             ((StepFileModel)stepNew).FileName +
                                                                             "\n");
                                                        }
                                                    }
                                                    if (step.GetType() == typeof(StepRampModel))
                                                    {
                                                        //System.Console.WriteLine("Ramp {0} {1}\n{2} - {3}!", step.Duration.Value, stepNew.Duration.Value, step.DurationVariableName, stepNew.DurationVariableName);
                                                        if (step.DurationVariableName != stepNew.DurationVariableName)
                                                        {
                                                            //System.Console.WriteLine("Duration variable changed in channel {2} at step {3}! old: {0} new: {1}",step.DurationVariableName,stepNew.DurationVariableName,channel.Setting.Name, m);
                                                            LogString.Append(
                                                                "Duration Variables not equal in \"" + card.Name + "\",  \"" +
                                                                sequence.Name + "\", \"" + channel.Setting.Name +
                                                                "\", step \"" + (m + 1) + "\"\t" +
                                                                StringOrDefault(step.DurationVariableName,
                                                                    "user input") + " --> " +
                                                                StringOrDefault(stepNew.DurationVariableName,
                                                                    "user input") + "\n");
                                                        }

                                                        if (step.DurationVariableName == "" || step.DurationVariableName == null)
                                                        {
                                                            if (step.Duration.Value != stepNew.Duration.Value)
                                                            {
                                                                LogString.Append(
                                                                    "Duration values not equal in \"" +
                                                                    card.Name +
                                                                    "\",  \"" + sequence.Name + "\", \"" +
                                                                    channel.Setting.Name + "\", step \"" +
                                                                    (m + 1) + "\"\t" + step.Duration.Value +
                                                                    " --> " + stepNew.Duration.Value + "\n");
                                                            }
                                                        }

                                                        if (step.ValueVariableName != stepNew.ValueVariableName)
                                                        {
                                                            LogString.Append("Value Variables not equal in \"" + card.Name +
                                                                             "\",  \"" + sequence.Name +
                                                                             "\", \"" + channel.Setting.Name +
                                                                             "\", step \"" + (m + 1) + "\"\t" +
                                                                             StringOrDefault(
                                                                                 step.ValueVariableName,
                                                                                 "user input") + " --> " +
                                                                             StringOrDefault(
                                                                                 stepNew.ValueVariableName,
                                                                                 "user input") + "\n");
                                                        }

                                                        if (step.ValueVariableName == "" || step.ValueVariableName == null)
                                                        {
                                                            if (step.Value.Value != stepNew.Value.Value)
                                                            {
                                                                // System.Console.WriteLine("Value changed in channel {2} at step {3}! old: {0} new: {1}",step.Value.Value,stepNew.Value.Value,channel.Setting.Name, m);
                                                                LogString.Append(
                                                                    "Value values not equal in \"" + card.Name + "\",  \"" +
                                                                    sequence.Name + "\", \"" +
                                                                    channel.Setting.Name + "\", step \"" +
                                                                    (m + 1) + "\"\t" + step.Value.Value +
                                                                    " --> " + stepNew.Value.Value + "\n");
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return LogString;
        }

        private static string StringOrDefault(string str, string defaultStr)
        {
            if (string.IsNullOrEmpty(str))
            {
                return defaultStr;
            }

            return str;
        }
    }
}

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Model.Data.Cards;
using Model.Data.Channels;
using Model.Data.Sequences;
using Model.Data.Steps;
using Model.Root;

namespace Controller.Control
{
    public class SwitchWindowController : INotifyPropertyChanged
    {
        private Dictionary<string, List<string>> switches;
        private RootModel rootModel;


        public Dictionary<string, List<string>> Switches
        {
            get
            {
                if (switches == null)
                {
                    Dictionary<string, Dictionary<double, List<KeyValuePair<string, double>>>> totalArray = new Dictionary<string, Dictionary<double, List<KeyValuePair<string, double>>>>();
                    //Sequence Name, Start Time, Channel name + voltage
                    int stepCtr = 0;

                    foreach (CardBasicModel card in rootModel.Data.group.Cards)
                    {
                        foreach (SequenceModel sequence in card.Sequences)
                        {
                            if (!totalArray.ContainsKey(sequence.Name))
                            {
                                totalArray.Add(sequence.Name, new Dictionary<double, List<KeyValuePair<string, double>>>());
                            }

                            //double startTime = sequence.startTime;
                            //System.Console.WriteLine("StartTime: {0}\n", startTime);
                            foreach (ChannelBasicModel channel in sequence.Channels)
                            {
                                double startTime = 0;
                                double lastVal = 0;

                                foreach (StepBasicModel step in channel.Steps)
                                {

                                    System.Console.WriteLine("StepStartTime: {0}\n", startTime);
                                    double val = 0;

                                    if (step.GetType().Equals(typeof(StepRampModel)))
                                    {
                                        if (((StepRampModel)step).Store != StepRampModel.StoreType.Constant)
                                        {
                                            val = -1000000 - stepCtr;
                                            stepCtr--;
                                        }
                                    }

                                    if (val == 0)
                                    {
                                        if (startTime != 0)
                                        {
                                            if (step.Value.Value == lastVal)
                                            {
                                                startTime += step.Duration.Value;
                                                continue;
                                            }
                                        }

                                        val = step.Value.Value;
                                    }

                                    if (totalArray[sequence.Name].ContainsKey(startTime))
                                    {
                                        totalArray[sequence.Name][startTime].Add(new KeyValuePair<string, double>(card.Name + "-" + channel.Setting.Name, val));
                                    }
                                    else
                                    {
                                        totalArray[sequence.Name].Add(startTime, new List<KeyValuePair<string, double>> { new KeyValuePair<string, double>(card.Name + "-" + channel.Setting.Name, val) });
                                    }

                                    lastVal = val;
                                    startTime += step.Duration.Value;
                                }
                            }
                        }
                    }


                    List<string> switchChannels;
                    switches = new Dictionary<string, List<string>>();
                    foreach (KeyValuePair<string, Dictionary<double, List<KeyValuePair<string, double>>>> sequenceKeyValuePair in totalArray)
                    {
                        switchChannels = new List<string>();
                        var switchTimes = sequenceKeyValuePair.Value.Keys.ToList();
                        switchTimes.Sort();

                        foreach (double switchTime in switchTimes)
                        {
                            List<KeyValuePair<string, double>> Value = sequenceKeyValuePair.Value[switchTime];
                            if (switchTime == 0)
                            {
                                continue;
                            }
                            string str = switchTime.ToString() + "\n-----";
                            foreach (KeyValuePair<string, double> channelSwitchKeyValuePair in Value)
                            {
                                str += "\n" + channelSwitchKeyValuePair.Key;
                            }
                            switchChannels.Add(str);
                        }

                        switches.Add(sequenceKeyValuePair.Key, switchChannels);
                    }
                }

                return switches;
            }
        }

        public SwitchWindowController(RootModel root)
        {
            this.rootModel = root;
        }
        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        /// <summary>
        /// Called when a property belonging to this class changes.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

}

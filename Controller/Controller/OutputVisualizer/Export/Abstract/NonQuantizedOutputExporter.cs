using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AbstractController.Data.Sequence;
using Communication.Interfaces.Generator;
using Model.Settings;

namespace Controller.OutputVisualizer.Export.Abstract
{
    /// <summary>
    /// Represents an exporter that deals with non-quantized non-compressed output
    /// </summary>
    public abstract class NonQuantizedOutputExporter : OutputExporter
    {
        private List<SequenceInfo> _allSequences;

        private List<SequenceInfo> _includedSequences;


        public NonQuantizedOutputExporter(IModelOutput modelOutput) : base(modelOutput)
        {
        }

        public void SetAllSequences(IList<AbstractSequenceController> allSequences, decimal stepTime)
        {
            _allSequences = allSequences.Select(sequenceC =>
            {
                decimal startTime = (decimal)sequenceC.ActualStartTime();
                decimal duration = (decimal)sequenceC.LongestDurationAllSequences();
                SequenceInfo result = new SequenceInfo()
                {
                    SequenceIndex = sequenceC.Index(),
                    SequenceName = sequenceC.GetModel().Name,
                    NumberOfSteps = (int)(duration / stepTime),
                    StartIndexOfSteps = (int)(startTime / stepTime)
                };

                return result;
            }).ToList();
        }

        public override async Task<bool> ExportOutput(ExportOptions options)
        {
            if (options.SequenceIndices != null)
            {
                _includedSequences = _allSequences.Where(item => options.SequenceIndices.Contains(item.SequenceIndex)).ToList();
            }
            else
            {
                _includedSequences = _allSequences;
            }

            List<DataPoint> dataPoints = new List<DataPoint>();
            List<DataPoint> current;

            foreach (string card in options.Channels.Keys)
            {
                foreach (int channel in options.Channels[card])
                {
                    current = ConvertChannelToDataPoints(card, channel);
                    dataPoints.AddRange(current);
                }
            }

            return await PerformExport(dataPoints, options);
        }

        protected abstract Task<bool> PerformExport(List<DataPoint> dataPoints, ExportOptions options);


        private List<DataPoint> ConvertChannelToDataPoints(string cardName, int channelNumber)
        {
            List<DataPoint> result = null;
            // a KeyNotFoundException might be thrown here if the cardName is not found
            ICardOutput card = modelOutput.Output[cardName];

            if (card is INonQuantizedCardOutput)
            {
                double[] channelOutput = ((INonQuantizedCardOutput)card).GetChannelOutput(channelNumber);
                decimal TIME_STEP_MILLIS = TimeSettingsInfo.GetInstance().SmallestTimeStepDecimal;
                DataPoint currentDP;
                result = new List<DataPoint>();
                int dpIndex;

                foreach (SequenceInfo sequence in _includedSequences)
                {
                    dpIndex = sequence.StartIndexOfSteps;

                    for (int i = 0; i < sequence.NumberOfSteps; i++)
                    {
                        currentDP = new DataPoint()
                        {
                            CardName = cardName,
                            ChannelIndex = channelNumber,
                            OutputValue = channelOutput[dpIndex],
                            TimeMillis = dpIndex * TIME_STEP_MILLIS,
                            SequenceIndex = sequence.SequenceIndex,
                            SequenceName = sequence.SequenceName
                        };
                        ++dpIndex;

                        result.Add(currentDP);
                    }
                }
            }

            return result;
        }


        class SequenceInfo
        {
            public int SequenceIndex { set; get; }

            public string SequenceName { set; get; }

            public int NumberOfSteps { set; get; }

            public int StartIndexOfSteps { set; get; }

        }

    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AbstractController.Data.Sequence;
using Communication.Interfaces.Generator;
using Model.Data.Sequences;
using Model.Settings;

namespace Controller.OutputVisualizer.Export.Abstract
{
    public abstract class NonQuantizedOutputExporter : OutputExporter
    {
        private List<SequenceInfo> _allSequences;

        private List<SequenceInfo> _includedSequences;


        public NonQuantizedOutputExporter(IModelOutput modelOutput) : base(modelOutput)
        {
        }

        public void SetAllSequences(IList<AbstractSequenceController> allSequences)
        {
            _allSequences = allSequences.Select(sequenceC =>
            {
                return new SequenceInfo()
                {
                    SequenceIndex = sequenceC.Index(),
                    SequenceName = sequenceC.GetModel().Name,
                    StartTimeInclusive = (decimal)sequenceC.ActualStartTime(),
                    EndTimeExclusive = (decimal)sequenceC.ActualStartTime() + (decimal)sequenceC.ActualDuration()
                };
            }).ToList();
        }

        public override async Task ExportOutput(ExportOptions options)
        {
            if (options.SequenceIndices != null)
            {
                _includedSequences = _allSequences.Where(item => options.SequenceIndices.IndexOf(item.SequenceIndex) > 0).ToList();
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

            await PerformExport(dataPoints, options);
        }

        protected abstract Task PerformExport(List<DataPoint> dataPoints, ExportOptions options);


        private List<DataPoint> ConvertChannelToDataPoints(string cardName, int channelNumber)
        {
            List<DataPoint> result = null;
            // a KeyNotFoundException might be thrown here if the cardName is not found
            ICardOutput card = modelOutput.Output[cardName];

            if (card is INonQuantizedCardOutput)
            {
                double[] channelOutput = ((INonQuantizedCardOutput)card).GetChannelOutput(channelNumber);
                decimal TIME_STEP_MILLIS = TimeSettingsInfo.GetInstance().SmallestTimeStepDecimal;
                decimal currentTimeMillis = 0.0M;
                DataPoint currentDP;
                SequenceInfo currentSI;
                result = new List<DataPoint>();

                foreach (double ouputPoint in channelOutput)
                {
                    currentSI = FindSequenceOfDataPoint(currentTimeMillis);

                    if (currentSI != null)
                    {
                        currentDP = new DataPoint()
                        {
                            CardName = cardName,
                            ChannelIndex = channelNumber,
                            OutputValue = ouputPoint,
                            TimeMillis = currentTimeMillis,
                            SequenceIndex = currentSI.SequenceIndex,
                            SequenceName = currentSI.SequenceName
                        };

                        result.Add(currentDP);
                    }

                    currentTimeMillis += TIME_STEP_MILLIS;

                }
            }

            return result;
        }

        private SequenceInfo FindSequenceOfDataPoint(decimal dataPointTime)
        {
            if (_includedSequences != null)
            {
                return FindSequenceOfDataPoint(dataPointTime, _includedSequences);
            }

            return FindSequenceOfDataPoint(dataPointTime, _allSequences);
        }


        private SequenceInfo FindSequenceOfDataPoint(decimal dataPointTime, IList<SequenceInfo> sequences)
        {
            foreach (SequenceInfo sequence in sequences)
            {
                if (sequence.IsInside(dataPointTime))
                    return sequence;
            }

            return null;
        }


        class SequenceInfo
        {
            public int SequenceIndex { set; get; }

            public string SequenceName { set; get; }

            public decimal StartTimeInclusive { set; get; }

            public decimal EndTimeExclusive { set; get; }

            public bool IsInside(decimal dataPointTime)
            {
                return dataPointTime >= StartTimeInclusive && dataPointTime < EndTimeExclusive;
            }
        }

    }
}

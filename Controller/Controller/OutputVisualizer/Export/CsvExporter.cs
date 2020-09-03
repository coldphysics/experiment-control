using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Communication.Interfaces.Generator;
using CsvHelper;
using Model.Settings;

namespace Controller.OutputVisualizer.Export
{
    public class CsvExporter : OutputExporter
    {
        public string OutputPath
        {
            set;
            get;
        }

        public CsvExporter(IModelOutput modelOutput) : base(modelOutput)
        {
            
        }

        public override async Task ExportMultiChannelOutputs(Dictionary<string, List<int>> channels)
        {
            List<DataPoint> dataPoints = new List<DataPoint>();
            List<DataPoint> current;

            foreach(string card in channels.Keys)
            {
                foreach(int channel in channels[card])
                {
                    current = ConvertChannelToDataPoints(card, channel);
                    dataPoints.AddRange(current);
                }
            }

            await WriteDataPointsToFile(dataPoints);
        }

        public override async Task ExportSingleChannelOutput(string cardName, int channelNumber)
        {
            List<DataPoint> dataPoints = ConvertChannelToDataPoints(cardName, channelNumber);

            await WriteDataPointsToFile(dataPoints);
        }

        private async Task WriteDataPointsToFile(List<DataPoint> dataPoints)
        {
            using (var writer = new StreamWriter(OutputPath))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    foreach (DataPoint dp in dataPoints)
                    {
                        csv.WriteRecord(dp);
                        await csv.NextRecordAsync();
                    }

                    await csv.FlushAsync();
                }
            }

        }

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
                DataPoint current;
                result = new List<DataPoint>();

                foreach (double ouputPoint in channelOutput)
                {
                    current = new DataPoint()
                    {
                        CardName = cardName,
                        ChannelIndex = channelNumber,
                        OutputValue = ouputPoint,
                        TimeMillis = currentTimeMillis
                    };

                    result.Add(current);
                    currentTimeMillis += TIME_STEP_MILLIS;

                }
            }

            return result;
        }

        class DataPoint
        {
            public string CardName { set; get; }

            public int ChannelIndex { set; get; }

            public double OutputValue { set; get; }

            public decimal TimeMillis { set; get; }
        }
    }
}

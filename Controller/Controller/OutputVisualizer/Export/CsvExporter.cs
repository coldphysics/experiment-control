using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Communication.Interfaces.Generator;
using Controller.OutputVisualizer.Export.Abstract;
using CsvHelper;
using CsvHelper.Configuration;


namespace Controller.OutputVisualizer.Export
{
    public class CsvExporter : NonQuantizedOutputExporter
    {
        public string OutputPath
        {
            set;
            get;
        }

        public CsvExporter(IModelOutput modelOutput) : base(modelOutput)
        {

        }



        protected override async Task PerformExport(List<DataPoint> dataPoints, ExportOptions options)
        {
            using (var writer = new StreamWriter(OutputPath))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.Configuration.RegisterClassMap(new DataPointMap(options));
                    csv.WriteHeader<DataPoint>();
                    await csv.NextRecordAsync();

                    foreach (DataPoint dp in dataPoints)
                    {
                        csv.WriteRecord(dp);
                        await csv.NextRecordAsync();
                    }

                    await csv.FlushAsync();
                }
            }

        }

    }
    public class DataPointMap : ClassMap<DataPoint>
    {
        public DataPointMap()
        {
            AutoMap(CultureInfo.CurrentCulture);
        }

        public DataPointMap(ExportOptions options) : this()
        {
            if (options.OutputFields != null)
            {
                if (options.OutputFields.Contains(OutputField.CardName))
                {
                    Map(m => m.CardName).Index(0);
                }
                else
                {
                    Map(m => m.CardName).Ignore();
                }

                if (options.OutputFields.Contains(OutputField.ChannelIndex))
                {
                    Map(m => m.ChannelIndex).Index(1);
                }
                else
                {
                    Map(m => m.ChannelIndex).Ignore();
                }

                if (options.OutputFields.Contains(OutputField.SequenceIndex))
                {
                    Map(m => m.SequenceIndex).Index(2);
                }
                else
                {
                    Map(m => m.SequenceIndex).Ignore();
                }

                if (options.OutputFields.Contains(OutputField.SequenceName))
                {
                    Map(m => m.SequenceName).Index(3);
                }
                else
                {
                    Map(m => m.SequenceName).Ignore();
                }

                if (options.OutputFields.Contains(OutputField.TimeMillis))
                {
                    Map(m => m.TimeMillis).Index(4);
                }
                else
                {
                    Map(m => m.TimeMillis).Ignore();
                }

                if (options.OutputFields.Contains(OutputField.OutputValue))
                {
                    Map(m => m.OutputValue).Index(5);
                }
                else
                {
                    Map(m => m.OutputValue).Ignore();
                }
            }
        }
    }
}

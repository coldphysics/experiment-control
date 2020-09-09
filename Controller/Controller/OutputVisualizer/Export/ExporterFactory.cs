using Communication.Interfaces.Generator;
using Controller.OutputVisualizer.Export.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controller.OutputVisualizer.Export
{
    public class ExporterFactory
    {
        private static ExporterFactory instance;

        private ExporterFactory()
        {}

        public static ExporterFactory GetInstance()
        {
            if (instance == null)
            {
                instance = new ExporterFactory();
            }

            return instance;
        }

        public OutputExporter GetNewExporter(string exporterName, IModelOutput modelOuput)
        {
            switch (exporterName)
            {
                case "CSV":
                default:
                    return new CsvExporter(modelOuput);
            }
        }
    }
}

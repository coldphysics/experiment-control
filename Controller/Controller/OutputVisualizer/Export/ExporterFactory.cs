using AbstractController.Data.Sequence;
using Communication.Interfaces.Generator;
using Controller.OutputVisualizer.Export.Abstract;
using Model.Settings;
using System.Collections.Generic;

namespace Controller.OutputVisualizer.Export
{
    /// <summary>
    /// Creates an instance of one of the implementations of the <see cref="OutputExporter"/> class based on a string provided by the user.
    /// </summary>
    public class ExporterFactory
    {
        private static ExporterFactory instance;

        private ExporterFactory()
        {}

        /// <summary>
        /// Get an instance of this class
        /// </summary>
        /// <returns>The one and only instance of this class.</returns>
        public static ExporterFactory GetInstance()
        {
            if (instance == null)
            {
                instance = new ExporterFactory();
            }

            return instance;
        }

        /// <summary>
        /// Creates a new instance of the exporter specified by the user
        /// </summary>
        /// <param name="exporterName">the name of the exporter. Currently, only "CSV" is supported</param>
        /// <param name="modelOuput">"the snapshot of the output to be exported"</param>
        /// <param name="allSequences">a set of <see cref="AbstractSequenceController"/> that represent all existing sequences in the output</param>
        /// <returns>A new instance of the specified exporter. The instance might need further initialization based on its type.</returns>
        public OutputExporter GetNewExporter(string exporterName, IModelOutput modelOuput, IList<AbstractSequenceController> allSequences)
        {
            switch (exporterName)
            {
                case "CSV":
                default:
                    CsvExporter exporter = new CsvExporter(modelOuput);
                    exporter.SetAllSequences(allSequences, TimeSettingsInfo.GetInstance().SmallestTimeStepDecimal);
                    return exporter;
            }
        }
    }
}

using AbstractController.Data.Sequence;
using Buffer.OutputProcessors.Compression;
using Buffer.OutputProcessors.Quantization;
using Communication.Interfaces.Generator;
using Controller;
using Controller.MainWindow;
using Controller.MainWindow.MeasurementRoutine;
using Controller.OutputVisualizer.Export;
using Controller.OutputVisualizer.Export.Abstract;
using CsvHelper;
using Generator.Cookbook;
using Generator.Generator;
using MainProject.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model.Root;
using Model.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GeneratorUT
{
    [TestClass]
    public class ModelOutputUT : ParentUT
    {
        [DataRow("Resources\\0.2ms.xml.gz", "Dy4thFloor")]
        [DataRow("Resources\\0.0ms.xml.gz", "Dy4thFloor")]
        [DataTestMethod]
        public void TestRegularOutputDurationWithReplication(string modelName, string profileName)
        {
            DataOutputGenerator outputGenerator = CreateOutputGenerator(modelName, profileName);
            IModelOutput output = outputGenerator.Generate();
            Assert.IsTrue(DoubleEquals(outputGenerator.Duration() * 1000.0, output.OutputDurationMillis));
            output.ReplicateOutput(3);
            Assert.IsTrue(DoubleEquals(outputGenerator.Duration() * 1000.0 * 3, output.OutputDurationMillis));
        }

        [DataRow("Resources\\0.2ms.xml.gz", "Dy4thFloor")]
        [DataRow("Resources\\0.0ms.xml.gz", "Dy4thFloor")]
        [DataTestMethod]
        public void TestCompressedOutputDurationWithReplication(string modelName, string profileName)
        {
            SelectProfile(profileName);
            RootModel model = LoadModel(modelName);
            GeneratorRecipe recipe = new GeneratorRecipe(new SequenceGroupGeneratorRecipe());
            DataOutputGenerator outputGenerator = (DataOutputGenerator)recipe.Cook(model);

            IModelOutput output = outputGenerator.Generate();
            OutputQuantizer oq = new OutputQuantizer(model.Data);
            OutputCompressor oc = new OutputCompressor(model.Data);
            oq.Process(output);
            oc.Process(output);

            Assert.IsTrue(DoubleEquals(outputGenerator.Duration() * 1000.0, output.OutputDurationMillis));
            output.ReplicateOutput(3);
            Assert.IsTrue(DoubleEquals(outputGenerator.Duration() * 1000.0 * 3, output.OutputDurationMillis));
        }

        [DataRow("Resources\\1 khz-3 sequences-1s 1s 2s.xml.gz", "NoOutput", "AO1", 0)]
        [DataTestMethod]
        public void TestExportToCsv(string modelName, string profileName, string cardName, int channelIndex)
        {
            SelectProfile(profileName);
            MasterBuilder builder = new MasterBuilder();
            builder.Build();
            MainWindowController mainWindowController = builder.GetMainController();
            MeasurementRoutineManagerController manager = new MeasurementRoutineManagerController(mainWindowController,
                mainWindowController.GetRootController().returnModel);

            Task<RootModel> loadTask = manager.LoadModelAsync(modelName, true, true);
            loadTask.Wait();
            RootModel model = loadTask.Result;
            GeneratorRecipe recipe = new GeneratorRecipe(new SequenceGroupGeneratorRecipe());
            DataOutputGenerator outputGenerator = (DataOutputGenerator)recipe.Cook(model);
            IModelOutput output = outputGenerator.Generate();
            int stepsCount = ((INonQuantizedCardOutput)output.Output[cardName]).GetChannelOutput(channelIndex).Length;
            decimal stepTime = TimeSettingsInfo.GetInstance().SmallestTimeStepDecimal;

            ObservableCollection<AbstractSequenceController> sequences = mainWindowController.GetRootController().DataController.SequenceGroup.Windows.First().Tabs;

            // We test including and excluding headers
            // First, we try all passing null to exported columns
            string filePath = ExportCsv(cardName, channelIndex, output, sequences, null, null);

            List<OutputField> headers = new List<OutputField>() {
                OutputField.CardName,
                OutputField.ChannelIndex,
                OutputField.SequenceName,
                OutputField.SequenceIndex,
                OutputField.OutputValue,
                OutputField.TimeMillis
            };

            EvaluateCsvFileValidity(filePath, headers, stepTime, stepsCount);

            // Now, we try passing no header fields
            headers = new List<OutputField>
            {

            };

            filePath = ExportCsv(cardName, channelIndex, output, sequences, headers, null);
            EvaluateCsvFileValidity(filePath, headers, stepTime, stepsCount);

            // Now, we try passing some of the output fields
            headers = new List<OutputField>
            {
                OutputField.TimeMillis,
                OutputField.OutputValue
            };

            filePath = ExportCsv(cardName, channelIndex, output, sequences, headers, null);
            EvaluateCsvFileValidity(filePath, headers, stepTime, stepsCount);


            // Now, we try passing all of the output fields
            headers = new List<OutputField>
            {
                OutputField.CardName,
                OutputField.ChannelIndex,
                OutputField.SequenceName,
                OutputField.SequenceIndex,
                OutputField.OutputValue,
                OutputField.TimeMillis
            };

            filePath = ExportCsv(cardName, channelIndex, output, sequences, headers, null);
            EvaluateCsvFileValidity(filePath, headers, stepTime, stepsCount);
        }

        private string ExportCsv(string cardName, int channelIndex, IModelOutput output, IList<AbstractSequenceController> sequences, List<OutputField> includedHeaders, List<int> includedSequences)
        {
            CsvExporter exporter = new CsvExporter(output);
            exporter.SetAllSequences(sequences);
            ExportOptions options = ExportOptionsBuilder
                .NewInstance(cardName, channelIndex)
                .SetOutputFields(includedHeaders)
                .SetSequenceIndices(includedSequences)
                .Build();
            string filePath = FileHelper.GenerateTemporaryFilePath(".csv");
            exporter.OutputPath = filePath;
            exporter.ExportOutput(options).Wait();

            return filePath;
        }

        private void EvaluateCsvFileValidity(string filePath, List<OutputField> expectedHeaders, decimal stepTime, int stepCount)
        {
            Assert.IsTrue(File.Exists(filePath));

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                if (expectedHeaders.Count == 0)
                {
                    Assert.AreEqual(0, csv.GetRecords<DataPoint>().Count());
                }
                else
                {
                    var records = new List<DataPoint>();
                    csv.Read();
                    csv.ReadHeader();
                    Assert.IsNotNull(csv.Context.HeaderRecord);
                    Assert.AreEqual(expectedHeaders.Count, csv.Context.HeaderRecord.Length);

                    foreach (OutputField headerField in expectedHeaders)
                    {
                        Assert.IsTrue(csv.Context.HeaderRecord.Contains(headerField.ToString()));
                    }

                    int count = 0;
                    decimal lastReadTime = -1M;

                    while (csv.Read())
                    {
                        dynamic record = csv.GetRecord<dynamic>();

                        if (expectedHeaders.Contains(OutputField.TimeMillis))
                        {
                            // check correct time
                            if (lastReadTime >= 0)
                            {
                                Assert.AreEqual(lastReadTime + stepTime, decimal.Parse(record.TimeMillis));
                            }
                        }

                        lastReadTime = decimal.Parse(record.TimeMillis);
                        ++count;
                    }

                    Assert.AreEqual(stepCount, count);
                }
            }

            File.Delete(filePath);
        }
    }
}
using AbstractController.Data.Channels;
using AbstractController.Data.Sequence;
using Buffer.OutputProcessors.Compression;
using Buffer.OutputProcessors.Quantization;
using Communication.Interfaces.Generator;
using Controller;
using Controller.MainWindow;
using Controller.MainWindow.MeasurementRoutine;
using Controller.OutputVisualizer;
using Controller.OutputVisualizer.Export;
using Controller.OutputVisualizer.Export.Abstract;
using CsvHelper;
using CustomElements.CheckableTreeView;
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
    public class ModelExportUT: ParentUT
    {
        [DataRow("Resources\\3 sequences-1ms 1ms 2ms.xml.gz", "NoOutput", "AO1", 0, "AO2", 1)]
        [TestMethod]
        public void TestControllerInitialization(string modelName, string profileName, string card1Name,  int channel1Index, string card2Name, int channel2Index)
        {
            SelectProfile(profileName);
            MasterBuilder builder = new MasterBuilder();
            builder.Build();
            MainWindowController mainWindowController = builder.GetMainController();
            MeasurementRoutineManagerController manager = new MeasurementRoutineManagerController(mainWindowController,
                mainWindowController.GetRootController().returnModel);
            Task<RootModel> loadTask = manager.LoadModelAsync(modelName, true, true);
            loadTask.Wait();
            RootModel rootModel = loadTask.Result;
        
            // First. we test the channels TV
            Dictionary<string, List<int>> channels = new Dictionary<string, List<int>>();
            channels.Add(card1Name, new List<int>() { channel1Index });
            channels.Add(card2Name, new List<int>() { channel2Index });
            VisualizationWindowManager.Initialize(mainWindowController);
            ExportWindowController controller = new ExportWindowController(VisualizationWindowManager.GetInstance().OutputVisualizationController, channels);
            var checkedElements = controller.SelectedChannelsTV.GetCheckedLeaves();
            Assert.AreEqual(2, checkedElements.Count);
            List<AbstractChannelController> checkedChannels = checkedElements
                .Select(element => (element as CheckableTVItemController).Item as AbstractChannelController)
                .ToList();
            Assert.IsTrue(checkedChannels.Select(cc=>cc.Index()).Contains(channel1Index));
            Assert.IsTrue(checkedChannels.Select(cc => cc.Index()).Contains(channel2Index));
            Assert.IsTrue(checkedChannels.Where(cc => cc.Index() == channel1Index).First().Parent.Parent.Model.Name == card1Name);
            Assert.IsTrue(checkedChannels.Where(cc => cc.Index() == channel2Index).First().Parent.Parent.Model.Name == card2Name);

            // Now, we test the sequences TV
            Assert.IsNotNull(controller.SelectedSequencesTV);
            Assert.AreEqual(1, controller.SelectedSequencesTV.Count);
            Assert.AreEqual(rootModel.Data.group.Cards.First().Sequences.Count, 
                controller.SelectedSequencesTV.First().Children.Count);


            // Now, we test the columns TV
            Assert.IsNotNull(controller.SelectedColumnsTV);
            Assert.AreEqual(1, controller.SelectedColumnsTV.Count);
            Assert.AreEqual(Enum.GetValues(typeof(OutputField)).Length,
                controller.SelectedColumnsTV.First().Children.Count);

        }

        [DataRow("Resources\\3 sequences-1ms 1ms 2ms.xml.gz", "NoOutput", "AO1", 0)]
        [DataTestMethod]
        public void TestExportToCsv_LargeFile(string modelName, string profileName, string cardName, int channelIndex)
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

            string filePath = ExportCsv(cardName, channelIndex, output, sequences, null, null);

            List<OutputField> headers = new List<OutputField>() {
                OutputField.CardName,
                OutputField.ChannelIndex,
                OutputField.SequenceName,
                OutputField.SequenceIndex,
                OutputField.OutputValue,
                OutputField.TimeMillis
            };

            EvaluateCsvFileValidity_Columns_Time(filePath, headers, stepTime, stepsCount);
        }

        [DataRow("Resources\\3 sequences-1ms 1ms 2ms.xml.gz", "NoOutput", "AO1", 0)]
        [DataTestMethod]
        public void TestExportToCsv_Sequences_Columns(string modelName, string profileName, string cardName, int channelIndex)
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

            EvaluateCsvFileValidity_Columns_Time(filePath, headers, stepTime, stepsCount);

            // Now, we try passing no header fields
            headers = new List<OutputField>
            {

            };

            filePath = ExportCsv(cardName, channelIndex, output, sequences, headers, null);
            EvaluateCsvFileValidity_Columns_Time(filePath, headers, stepTime, stepsCount);

            // Now, we try passing some of the output fields
            headers = new List<OutputField>
            {
                OutputField.TimeMillis,
                OutputField.OutputValue
            };

            filePath = ExportCsv(cardName, channelIndex, output, sequences, headers, null);
            EvaluateCsvFileValidity_Columns_Time(filePath, headers, stepTime, stepsCount);


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
            EvaluateCsvFileValidity_Columns_Time(filePath, headers, stepTime, stepsCount);


            // We test including and excluding sequences
            List<int> allSeqIndices = new List<int>();

            for (int i = 0; i < sequences.Count; i++)
            {
                allSeqIndices.Add(i);
            }

            List<int> sequenceIndices = allSeqIndices;
            // We try passing null
            filePath = ExportCsv(cardName, channelIndex, output, sequences, headers, null);
            EvaluateCsvFileValidity_Sequences(filePath, sequenceIndices, sequences, stepTime);

            // We try including no sequences at all
            sequenceIndices = new List<int>
            {

            };
            filePath = ExportCsv(cardName, channelIndex, output, sequences, headers, sequenceIndices);
            EvaluateCsvFileValidity_Sequences(filePath, sequenceIndices, sequences, stepTime);


            //We try including 1 sequence only
            sequenceIndices = new List<int>
            {
                1
            };
            filePath = ExportCsv(cardName, channelIndex, output, sequences, headers, sequenceIndices);
            EvaluateCsvFileValidity_Sequences(filePath, sequenceIndices, sequences, stepTime);

            //We try including 2 non-consequitive sequence only
            sequenceIndices = new List<int>
            {
                0,2
            };
            filePath = ExportCsv(cardName, channelIndex, output, sequences, headers, sequenceIndices);
            EvaluateCsvFileValidity_Sequences(filePath, sequenceIndices, sequences, stepTime);

            //We try including all sequences
            sequenceIndices = allSeqIndices;
            filePath = ExportCsv(cardName, channelIndex, output, sequences, headers, sequenceIndices);
            EvaluateCsvFileValidity_Sequences(filePath, sequenceIndices, sequences, stepTime);
        }

        [DataRow("Resources\\2 seq - 1ms 1ms_2 cards - AO1,ch0 AO2,ch1 - different durations.xml.gz", "NoOutput", "AO1", "AO2", 0, 1)]
        [DataTestMethod]
        public void TestExportToCsv_2Channels(string modelName, string profileName, string card1Name, string card2Name, int channel1Index, int channel2Index)
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
            int stepsCount = ((INonQuantizedCardOutput)output.Output[card1Name]).GetChannelOutput(channel1Index).Length;
            decimal stepTime = TimeSettingsInfo.GetInstance().SmallestTimeStepDecimal;

            ObservableCollection<AbstractSequenceController> sequences = mainWindowController.GetRootController().DataController.SequenceGroup.Windows.First().Tabs;
            Dictionary<string, List<int>> channels = new Dictionary<string, List<int>>();
            channels.Add(card1Name, new List<int>() { channel1Index });
            channels.Add(card2Name, new List<int>() { channel2Index });

            string filePath = ExportCsv(channels, output, sequences, null, null);

            List<OutputField> headers = new List<OutputField>() {
                OutputField.CardName,
                OutputField.ChannelIndex,
                OutputField.SequenceName,
                OutputField.SequenceIndex,
                OutputField.OutputValue,
                OutputField.TimeMillis
            };

            EvaluateCsvFileValidity_Channels(filePath, channels, stepsCount);
        }
        private string ExportCsv(string cardName, int channelIndex, IModelOutput output, IList<AbstractSequenceController> sequences, List<OutputField> includedHeaders, List<int> includedSequences)
        {
            Dictionary<string, List<int>> channels = new Dictionary<string, List<int>>();
            channels.Add(cardName, new List<int>() { channelIndex });

            return ExportCsv(channels, output, sequences, includedHeaders, includedSequences);
        }

        private string ExportCsv(Dictionary<string, List<int>> channels, IModelOutput output, IList<AbstractSequenceController> sequences, List<OutputField> includedHeaders, List<int> includedSequences)
        {
            CsvExporter exporter = new CsvExporter(output);
            exporter.SetAllSequences(sequences, TimeSettingsInfo.GetInstance().SmallestTimeStepDecimal);
            ExportOptions options = ExportOptionsBuilder
                .NewInstance(channels)
                .SetOutputFields(includedHeaders)
                .SetSequenceIndices(includedSequences)
                .Build();
            string filePath = FileHelper.GenerateTemporaryFilePath(".csv");
            exporter.OutputPath = filePath;
            exporter.ExportOutput(options).Wait();

            return filePath;
        }


        /// <summary>
        /// This method evaluates the correct sequencing and timing of records in the exported csv file.
        /// It also tests the inclusion of a selective set of output columns.
        /// The method removes the csv file.
        /// </summary>
        /// <param name="filePath">The path to the csv file</param>
        /// <param name="expectedHeaders">A list of the columns that are expected to be in the csv file</param>
        /// <param name="stepTime">The time (in millis) of a single time step</param>
        /// <param name="stepsCount">The expected number of timesteps (records) in the csv file</param>
        private void EvaluateCsvFileValidity_Columns_Time(string filePath, List<OutputField> expectedHeaders, decimal stepTime, int stepsCount)
        {
            File.Exists(filePath);

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                if (expectedHeaders.Count == 0)
                {
                    Assert.AreEqual(0, csv.GetRecords<DataPoint>().Count());
                }
                else
                {
                    csv.Read();
                    csv.ReadHeader();
                    Assert.IsNotNull(csv.Context.HeaderRecord);
                    Assert.AreEqual(expectedHeaders.Count, csv.Context.HeaderRecord.Length);

                    foreach (OutputField headerField in expectedHeaders)
                    {
                        Assert.IsTrue(csv.Context.HeaderRecord.Contains(headerField.ToString()));
                    }

                    decimal lastReadTime = -1M;
                    List<dynamic> all = new List<dynamic>();

                    while (csv.Read())
                    {
                        dynamic record = csv.GetRecord<dynamic>();
                        all.Add(record);

                        if (expectedHeaders.Contains(OutputField.TimeMillis))
                        {
                            // check correct time
                            if (lastReadTime >= 0)
                            {
                                Assert.AreEqual(lastReadTime + stepTime, decimal.Parse(record.TimeMillis));
                            }

                            lastReadTime = decimal.Parse(record.TimeMillis);
                        }
                    }

                    Assert.AreEqual(stepsCount, all.Count);
                }
            }

            File.Delete(filePath);
        }

        /// <summary>
        /// This method evaluates the inclusion of a selective set of sequences.
        /// The method removes the csv file.
        /// </summary>
        /// <param name="filePath">The path to the csv file</param>
        /// <param name="expectedSequences">The expected set of sequence indices to be included in the csv file</param>
        /// <param name="allSequences">The set of all sequences (to get information from them)</param>
        /// <param name="stepTime">The time (in millis) of a single time step</param>
        private void EvaluateCsvFileValidity_Sequences(string filePath, List<int> expectedSequences, IList<AbstractSequenceController> allSequences, decimal stepTime)
        {
            File.Exists(filePath);

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                if (expectedSequences.Count == 0)
                {
                    Assert.AreEqual(0, csv.GetRecords<DataPoint>().Count());
                }
                else
                {
                    decimal expectedRecordsCount = allSequences
                        .Where(aSeq => expectedSequences.Contains(aSeq.Index()))
                        .Select(aSeq => (decimal)aSeq.ActualDuration() / stepTime)
                        .Sum();
                    var records = csv.GetRecords<DataPoint>().ToList();

                    Assert.AreEqual(expectedRecordsCount, records.Count());
                    List<int> actual = records.Select(record => record.SequenceIndex).Distinct().ToList();
                    Assert.AreEqual(expectedSequences.Count, actual.Count);

                    foreach (int sequenceIndex in expectedSequences)
                    {
                        Assert.IsTrue(actual.Contains(sequenceIndex));
                    }

                }
            }

            File.Delete(filePath);
        }

        /// <summary>
        /// This method evaluates the inclusion of a selective set of sequences.
        /// The method removes the csv file.
        /// </summary>
        /// <param name="filePath">The path to the csv file</param>
        /// <param name="expectedSequences">The expected set of sequence indices to be included in the csv file</param>
        /// <param name="allSequences">The set of all sequences (to get information from them)</param>
        /// <param name="stepTime">The time (in millis) of a single time step</param>
        private void EvaluateCsvFileValidity_Channels(string filePath, Dictionary<string, List<int>> channels, decimal totalSteps)
        {
            File.Exists(filePath);

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {

                var records = csv.GetRecords<DataPoint>().ToList();
                int totalChannelsCount = 0;

                // check that all expected channels are there, and that each of them has the same number of dp's,
                // which is equal to the expected number of dp's
                foreach (string cardName in channels.Keys)
                {
                    foreach (int channelIndex in channels[cardName])
                    {
                        Assert.AreEqual(totalSteps, records.Where(dp => dp.ChannelIndex == channelIndex && dp.CardName == cardName).Count());
                        ++totalChannelsCount;
                    }
                }

                // check no extra dp's exist
                Assert.AreEqual(totalSteps * totalChannelsCount, records.Count());

            }

            File.Delete(filePath);
        }


    }
}

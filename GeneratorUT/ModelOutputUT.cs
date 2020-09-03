using Buffer.OutputProcessors.Compression;
using Buffer.OutputProcessors.Quantization;
using Communication.Interfaces.Generator;
using Controller;
using Controller.OutputVisualizer.Export;
using Generator.Cookbook;
using Generator.Generator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model.Root;
using System.IO;

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

        //[DataRow("Resources\\0.2ms.xml.gz", "Dy4thFloor", "AO1", 0)]
        //[DataRow("Resources\\0.0ms.xml.gz", "Dy4thFloor", "AO1", 0)]
        [DataRow("Resources\\1715_Excite_on_P_branch_ARBs_ODT_lowDensity.xml.gz", "Dy4thFloor", "AO1", 0)]
        [DataTestMethod]
        public void TestExportToCsv(string modelName, string profileName, string cardName, int channelIndex)
        {
            SelectProfile(profileName);
            RootModel model = LoadModel(modelName);
            GeneratorRecipe recipe = new GeneratorRecipe(new SequenceGroupGeneratorRecipe());
            DataOutputGenerator outputGenerator = (DataOutputGenerator)recipe.Cook(model);

            IModelOutput output = outputGenerator.Generate();
            CsvExporter exporter = new CsvExporter(output);
            string filePath = FileHelper.GenerateTemporaryFilePath(".csv");
            exporter.OutputPath = filePath;
            exporter.ExportSingleChannelOutput(cardName, channelIndex).Wait() ;
            Assert.IsTrue(File.Exists(filePath));
        }
    }
}
using Buffer.OutputProcessors.Compression;
using Buffer.OutputProcessors.Quantization;
using Communication.Interfaces.Generator;
using Generator.Cookbook;
using Generator.Generator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model.Data;
using Model.Root;
using System;

namespace GeneratorUT
{
    [TestClass]
    public class ModelOutputUT: ParentUT
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

        private bool DoubleEquals(double value1, double value2)
        {
            const double TOLERATED_DIFFERENCE = 1.0E-15;
            return Math.Abs(value1 - value2) <= TOLERATED_DIFFERENCE;
        }
    }
}

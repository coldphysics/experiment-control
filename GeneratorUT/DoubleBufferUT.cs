using System.Linq;
using Buffer.Basic;
using Communication.Interfaces.Generator;
using Generator.Generator;
using MainProject.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model.Builder;
using Model.Root;

namespace GeneratorUT
{
    [TestClass]
    public class DoubleBufferUT : ParentUT
    {
        [DataRow("Resources\\0.2ms.xml.gz", "Dy4thFloor")]
        [DataRow("Resources\\0.0ms.xml.gz", "Dy4thFloor")]
        [DataTestMethod]
        public void TestRegularOutputDurationWithReplication(string modelName, string profileName)
        {
            SelectProfile(profileName);
            const int timesToReplicate = 3;
            RootModel model = base.LoadModel(modelName);
            ModelBuilder modelBuilder = new ModelBuilder(); 
            modelBuilder.Build();
            DoubleBufferBuilder builder = new DoubleBufferBuilder();
            builder.Build(model, modelBuilder.GetCardTypes());
            DoubleBuffer buffer = builder.GetDoubleBuffer();
            DataOutputGenerator outputGenerator = CreateOutputGenerator(modelName, profileName);
            IModelOutput output = outputGenerator.Generate();

            buffer.FinishedModelGeneration += (sender, args) =>
            {
                Assert.IsTrue(args.IsSuccessful);
                Assert.IsTrue(DoubleEquals(output.OutputDurationMillis * timesToReplicate / 1000.0, buffer.DurationSeconds));
            };

            buffer.CopyData(model, timesToReplicate);            
        }
    }
}
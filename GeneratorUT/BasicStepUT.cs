using System.Collections.ObjectModel;
using AbstractController.Data.Sequence;
using Generator.Cookbook;
using Generator.Generator;
using Generator.Generator.Step.Abstract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model.Root;

namespace GeneratorUT
{
    [TestClass]
    public class BasicStepUT : ParentUT
    {
        [DataRow("Resources\\disabled-first-sequence.xml.gz", "Dy4thFloor")]
        [DataRow("Resources\\disabled-mid-sequence.xml.gz", "Dy4thFloor")]
        [DataTestMethod]
        public void TestDisabledSequenceDoesNotAffectLastStepValue(string modelName, string profileName)
        {
            DataOutputGenerator outputGenerator = CreateOutputGenerator(modelName, profileName);
            Collection<AbstractSequenceController> seqs = outputGenerator.SequenceGroup.Windows[0].Tabs;
            AbstractSequenceController seq = seqs[seqs.Count - 1];
            double value = ((BasicStepOutputGenerator)seq.Channels[0].Steps[0]).GetLastValueOfPreviousStep();
            Assert.AreEqual(0, value);
        }

    }
}

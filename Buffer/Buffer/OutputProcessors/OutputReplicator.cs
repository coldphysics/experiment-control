using Communication.Interfaces.Generator;
using Model.Data;

namespace Buffer.OutputProcessors
{
    public class OutputReplicator:OutputProcessor
    {
        public int TimesToReplicate { set; get; }

        public OutputReplicator(DataModel dataModel, int timesToReplicate)
            :base(dataModel)
        {
            TimesToReplicate = timesToReplicate;
        }

        public override void Process(IModelOutput output)
        {
            output.ReplicateOutput(TimesToReplicate);
        }
    }
}

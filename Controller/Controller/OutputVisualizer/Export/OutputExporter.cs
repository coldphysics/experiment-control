using Communication.Interfaces.Generator;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Controller.OutputVisualizer.Export
{
    public abstract class OutputExporter
    {
        protected IModelOutput modelOutput;

        public OutputExporter(IModelOutput modelOutput)
        {
            this.modelOutput = modelOutput;
        }

        public abstract Task ExportSingleChannelOutput(string cardName, int channelNumber);

        public abstract Task ExportMultiChannelOutputs(Dictionary<string, List<int>> channels);
        
    }
}

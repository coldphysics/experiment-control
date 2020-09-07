using Communication.Interfaces.Generator;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Controller.OutputVisualizer.Export.Abstract
{
    public abstract class OutputExporter
    {
        protected IModelOutput modelOutput;

        public OutputExporter(IModelOutput modelOutput)
        {
            this.modelOutput = modelOutput;
        }

        public abstract Task ExportOutput(ExportOptions options);
        
    }
}

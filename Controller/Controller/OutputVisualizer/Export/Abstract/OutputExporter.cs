using Communication.Interfaces.Generator;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Controller.OutputVisualizer.Export.Abstract
{
    /// <summary>
    /// An abstract class to represent the functionality of exporting the output to some target format
    /// </summary>
    public abstract class OutputExporter
    {
        protected IModelOutput modelOutput;

        public OutputExporter(IModelOutput modelOutput)
        {
            this.modelOutput = modelOutput;
        }

        public abstract Task<bool> ExportOutput(ExportOptions options);
        
    }
}

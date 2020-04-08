using Communication.Interfaces.Generator;
using Model.Data;

namespace Buffer.OutputProcessors
{
    public abstract class OutputProcessor
    {
        protected DataModel dataModel;

        public OutputProcessor(DataModel dataModel)
        {
            this.dataModel = dataModel;

        }

        public abstract void Process(IModelOutput output);
        

    }
}

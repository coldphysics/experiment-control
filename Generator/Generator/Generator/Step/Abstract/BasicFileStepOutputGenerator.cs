using Generator.Generator.Channel;
using Model.Data.Steps;

namespace Generator.Generator.Step.Abstract
{
    public class BasicFileStepOutputGenerator:BasicStepOutputGenerator
    {
        /// <summary>
        /// The model that describes the current step.
        /// </summary>
        protected StepFileModel Model
        {
            get
            {
                return (StepFileModel)_model;
            }
        }

        public BasicFileStepOutputGenerator(StepBasicModel model, ChannelOutputGenerator parent)
            : base(model, parent)
        { }

        //TODO put file reading functionality here
    }
}

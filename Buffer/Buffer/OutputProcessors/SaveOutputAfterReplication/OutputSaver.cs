using Communication.Interfaces.Generator;
using Model.Data;

namespace Buffer.OutputProcessors.SaveOutputAfterReplication
{
    public class OutputSaver : OutputProcessor
    {
        //Ebaa        
        /// <summary>
        /// Gets or sets the visualizer output.
        /// </summary>
        /// <value>
        /// The visualizer output.
        /// </value>
        /// /// <remarks>
        /// To save the output for the Outputvisualizer before quantization and compression.
        /// </remarks>
        private IModelOutput VisualizerOutput;



        public OutputSaver(DataModel model)
            : base(model)
        {
        }


        /// <summary>
        /// Processes the specified model output. DeepClone is needed here to have another object of the output (not just a reference).
        /// </summary>
        /// <param name="modelOutput">The model output.</param>
        public override void Process(IModelOutput modelOutput)
        {
            VisualizerOutput = modelOutput.DeepClone();

        }

        public IModelOutput GetVisualizerOutput()
        {
            return VisualizerOutput;
        }


    }
}

using System.Collections.Generic;

namespace Communication.Interfaces.Generator
{
    /// <summary>
    /// Represents the generated output of a model
    /// </summary>
    public interface IModelOutput
    {
        /// <summary>
        /// Gets the dictionary that maps the output of all cards to their card names
        /// </summary>
        /// <value>
        /// The output.
        /// </value>
        Dictionary<string, ICardOutput> Output
        {
            get;
        }

        /// <summary>
        /// Replicates the output.
        /// </summary>
        /// <param name="timesToReplicate">The times to replicate.</param>
        void ReplicateOutput(int timesToReplicate);


        IModelOutput DeepClone();

    }
}

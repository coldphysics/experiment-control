using Communication.Interfaces.Generator;
using System.Collections.Generic;

namespace Generator.Generator.Concatenator
{
    /// <summary>
    /// The generated output of the whole model
    /// </summary>
    /// <seealso cref="Communication.Interfaces.Generator.IModelOutput" />
    public class ModelOutput : IModelOutput
    {
        /// <summary>
        /// The output
        /// </summary>
        private Dictionary<string, ICardOutput> output;

        /// <summary>
        /// Gets the dictionary that maps the output of all cards to their card names
        /// </summary>
        /// <value>
        /// The output.
        /// </value>
        public Dictionary<string, ICardOutput> Output
        {
            get { return output; }

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelOutput"/> class.
        /// </summary>
        /// <param name="allCards">All cards.</param>
        public ModelOutput(Dictionary<string, ICardOutput> allCards)
        {
            this.output = allCards;
        }



        #region IModelOutput Members

        /// <summary>
        /// Replicates the output.
        /// </summary>
        /// <param name="timesToReplicate">The times to replicate.</param>
        public void ReplicateOutput(int timesToReplicate)
        {
            foreach (ICardOutput card in output.Values)
            {
                card.ReplicateOutput(timesToReplicate);
            }
        }

        public IModelOutput DeepClone()
        {
            Dictionary<string, ICardOutput> dictCopy = new Dictionary<string, ICardOutput>();

            foreach (string cardName in output.Keys)
            {
                dictCopy.Add(cardName, output[cardName].DeepClone());
            }

            ModelOutput result = new ModelOutput(dictCopy);

            return result;
        }

        #endregion
    }
}

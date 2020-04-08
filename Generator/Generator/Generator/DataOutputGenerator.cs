using AbstractController.Data;
using Communication.Interfaces.Generator;
using Model.Data;

namespace Generator.Generator
{
    /// <summary>
    /// Represents a generator that is capable of generating the output of all <c>SequenceGroup</c>s.
    /// </summary>
    /// <seealso cref="AbstractController.Data.AbstractDataController" />
    /// <seealso cref="Communication.Interfaces.Generator.IDataGenerator" />
    public class DataOutputGenerator : AbstractDataController, IDataGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataOutputGenerator"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="parent">The parent.</param>
        public DataOutputGenerator(DataModel model, RootOutputGenerator parent) : base(model, parent)
        {
        }

        #region IGenerator Members

        /// <summary>
        /// Generates the final output. 
        /// It adds one entry per <c>SequenceGroup</c> (usually, only one), and uses its child <see cref=" IGenerator"/>'s to generate the output of the <c>SequenceGroup</c>'s.
        /// </summary>
        /// <returns>
        /// A Dictionary of key-value pairs.
        /// The keys are the <c>SequenceGroup</c>'s names, i.e., the experiment names.
        /// The values are themselves dictionaries that map each card's name (as a <c>string</c>) to an object of a card type.
        /// </returns>
        public IModelOutput Generate()
        {
            IModelOutput result = ((IGenerator)SequenceGroup).Generate();

            return result;
        }

        #endregion
    }
}
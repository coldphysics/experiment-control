using Communication.Interfaces.Generator;
using Communication.Interfaces.Model;
using Model.Root;

namespace Generator.Cookbook
{
    /// <summary>
    /// Cooks the entire hierarchy of output generators.
    /// </summary>
    /// <seealso cref="IGeneratorRecipe" />
    public class GeneratorRecipe : IGeneratorRecipe
    {
        /// <summary>
        /// A dictionary mapping the name of a sequence group to the output generator of this sequence group (usually, there is only one entry)
        /// </summary>
        private readonly IWrapSequenceGroupGenerator _sequenceGroupWrapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratorRecipe"/> class.
        /// </summary>
        /// <param name="sequenceGroupWrapper">A dictionary mapping the name of a sequence group to the output generator of this sequence group (usually, there is only one entry)</param>
        public GeneratorRecipe(IWrapSequenceGroupGenerator sequenceGroupWrapper)
        {
            _sequenceGroupWrapper = sequenceGroupWrapper;
        }

        #region IWrapGenerator Members

        /// <summary>
        /// Cooks the <see cref=" IDataGenerator" /> based on a specific <see cref=" Communication.Interfaces.Model.IModel" />.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>
        /// The cooked data output generator.
        /// </returns>
        /// <exception cref=" System.Exception">
        /// Could not find a recipe for:  + entry.Key
        /// </exception>
        public IDataGenerator Cook(IModel model)
        {
            RootModel realModel = (RootModel)model;
            RootOutputGenerator rootVertex = new RootOutputGenerator(realModel);
            Generator.DataOutputGenerator dataOutputGenerator = new Generator.DataOutputGenerator(realModel.Data, rootVertex);
            rootVertex.DataController = dataOutputGenerator;
            _sequenceGroupWrapper.CookSequenceGroup(realModel.Data.group, dataOutputGenerator);

            return dataOutputGenerator;


        }

        #endregion
    }
}
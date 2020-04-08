using AbstractController;
using Model.Root;

namespace Generator
{
    /// <summary>
    /// The parent class for the hierarchy of generators. 
    /// It serves as a basic implementation for the <see cref="AbstractRootController"/> class.
    /// </summary>
    public class RootOutputGenerator : AbstractRootController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RootOutputGenerator"/> class.
        /// </summary>
        /// <param name="model">The <see cref=" RootModel" /> this instance will be associated to.</param>
        public RootOutputGenerator(RootModel model) : base(model)
        {}


    }
}
using AbstractController.Data.Channels;
using AbstractController.Data.SequenceGroup;
using Model.Data.Cards;

namespace AbstractController.Data.Steps
{
    /// <summary>
    /// The controller at the step level of the hierarchy. It represents part of the ModelView of a single step.
    /// </summary>
    public abstract class AbstractStepController
    {
        /// <summary>
        /// The parent controller of this controller.
        /// </summary>
        protected AbstractChannelController Parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractStepController"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public AbstractStepController(AbstractChannelController parent)
        {
            Parent = parent;
        }

        /// <summary>
        /// Returns the type of the grand-grandpa <c>CardController</c>
        /// </summary>
        /// <returns>The type of the grand-grandpa <c>CardController</c></returns>
        public CardBasicModel.CardType CardType()
        {
            return Parent.CardType();
        }

        /// <summary>
        /// Returns a reference to the grand-grand-grandpa <c>SequenceGroupController</c>.
        /// </summary>
        /// <returns>A reference to the grand-grand-grandpa <c>SequenceGroupController</c>.</returns>
        public AbstractSequenceGroupController Group()
        {
            return Parent.Group();
        }
    }
}
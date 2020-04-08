using AbstractController.Data.Card;
using Model.Data.SequenceGroups;
using System.Collections.ObjectModel;
using System.Linq;

namespace AbstractController.Data.SequenceGroup
{
    /// <summary>
    /// The controller at the sequence group level of the hierarchy
    /// </summary>
    public class AbstractSequenceGroupController
    {
        /// <summary>
        /// The <see cref=" SequenceGroupModel"/> this controller is associated to.
        /// </summary>
        protected readonly SequenceGroupModel Model;
        /// <summary>
        /// The direct parent of this controller
        /// </summary>
        protected readonly AbstractDataController Parent;
        /// <summary>
        /// A collection of <see cref=" AbstractCardController"/>'s which this controller has. Each of these controllers (ModelViews) would be associated with a Window of a card.
        /// </summary>
        public ObservableCollection<AbstractCardController> Windows;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractSequenceGroupController"/> class. Creates an empty collection of <see cref=" AbstractCardController"/>'s .
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="parent">The parent.</param>
        public AbstractSequenceGroupController(SequenceGroupModel model, AbstractDataController parent)
        {
            Model = model;
            Parent = parent;
            Windows = new ObservableCollection<AbstractCardController>();
        }


        /// <summary>
        /// Returns the duration measured in input units. It is calculated as the duration of the first card controller this sequence group has.
        /// </summary>
        /// <returns>The duration measured in the same time units used for the values which are put into the user interface.</returns>
        public double Duration()
        {
            if (Windows != null)
            {
                if (Windows.Count != 0)
                {
                    return Windows.First().Duration();
                }
            }
            return 0;
        }
    }
}
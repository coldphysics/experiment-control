using AbstractController.Data.Sequence;
using AbstractController.Data.SequenceGroup;
using Model.Data.Cards;
using Model.Data.Channels;
using Model.Data.Steps;
using System.Collections.ObjectModel;

namespace AbstractController.Data.Channels
{
    /// <summary>
    /// The controller at the channel level of the hierarchy. It represents part of the ModelView of a row in a tab.
    /// </summary>
    public abstract class AbstractChannelController
    {
        /// <summary>
        /// The model this controller is associated to.
        /// </summary>
        protected readonly ChannelModel Model;
        /// <summary>
        /// The parent controller of this controller.
        /// </summary>
        public readonly AbstractSequenceController Parent;
        /// <summary>
        /// A collection of all step controllers this channel has.
        /// </summary>
        public ObservableCollection<object> Steps { get; private set; }//RECO make the elements of type AbstractStepController


        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractChannelController"/> class. It also creates an empty collection of step controllers.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="parent">The parent.</param>
        protected AbstractChannelController(ChannelModel model, AbstractSequenceController parent)
        {
            Steps = new ObservableCollection<object>();
            Model = model;
            Parent = parent;
        }

        //RECO remove
        /// <summary>
        /// Gets the <c>ChannelController</c> of index (1) of the previous <c>AbstractSequenceController</c>.
        /// </summary>
        /// <returns>The <c>ChannelController</c> of index (1) of the previous <c>AbstractSequenceController</c></returns>
        /// <exception cref="System.Exception"></exception>
        /// <exception cref=" System.IndexOutOfRangeException"></exception>
        public AbstractChannelController SameChannelOfPreviousTab()
        {
            AbstractSequenceController previousTab = Parent.PreviousTab();
            AbstractChannelController predecessorChannel = previousTab.Channels[1];

            return predecessorChannel;
        }


        /// <summary>
        /// Returns the grand-grandpa
        /// </summary>
        /// <returns>The grand-grandpa</returns>
        public AbstractSequenceGroupController Group()
        {
            return Parent.Group();
        }

        /// <summary>
        /// Returns the index of this controller in the list of all <c>ChannelController</c>s the parent has.
        /// </summary>
        /// <returns>The index of this controller in the list of all <c>ChannelController</c>s the parent has</returns>
        public int Index()
        {
            int index = Parent.Channels.IndexOf(this);

            return index;
        }

        /// <summary>
        /// Returns the type of the parent card
        /// </summary>
        /// <returns>The type of the parent card</returns>
        internal CardBasicModel.CardType CardType()
        {
            return Parent.CardType();
        }


        /// <summary>
        /// Calculates the duration of the current channel which belongs to one sequence by summing up the durations of all its steps
        /// </summary>
        /// <returns>The duration of the current channel</returns>
        public double Duration()
        {
            double duration = 0;

            foreach (StepBasicModel step in Model.Steps)
            {
                duration += step.Duration.Value;
            }

            return duration;
        }
    }
}
using AbstractController.Data.Card;
using AbstractController.Data.Channels;
using AbstractController.Data.SequenceGroup;
using Model.Data.Cards;
using Model.Data.Sequences;
using System.Collections.ObjectModel;

namespace AbstractController.Data.Sequence
{
    /// <summary>
    /// The controller at the sequence level in the hierarchy. It represents part of the ModelView of a Tab in a window.
    /// </summary>
    public abstract class AbstractSequenceController
    {
        /// <summary>
        /// The model this controller is associated to.
        /// </summary>
        protected readonly SequenceModel Model;
        /// <summary>
        /// The parent controller.
        /// </summary>
        public readonly AbstractCardController Parent;
        /// <summary>
        /// A collection of all child <see cref=" AbstractChannelController"/>'s associated with this controller.
        /// </summary>
        public ObservableCollection<AbstractChannelController> Channels { get; protected set; }



        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractSequenceController"/> class.  It also creates an empty list of child <see cref=" AbstractChannelController"/>'s.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="parent">The parent.</param>
        public AbstractSequenceController(SequenceModel model, AbstractCardController parent)
        {
            Channels = new ObservableCollection<AbstractChannelController>();
            Model = model;
            Parent = parent;
        }

        /// <summary>
        /// Returns a reference to the model.
        /// </summary>
        /// <returns>A reference to the model</returns>
        public SequenceModel GetModel()
        {
            return Model;
        }

        /// <summary>
        /// Returns the previous SequenceController in the same card.
        /// </summary>
        /// <returns>The previous <see cref="AbstractSequenceController"/>.</returns>
        /// <exception cref=" System.Exception"></exception>
        /// <exception cref=" System.IndexOutOfRangeException"></exception>
        public AbstractSequenceController PreviousTab()
        {
            return Parent.PreviousTab(this);
        }

        /// <summary>
        /// Returns the type of the card this controller's parent is associated to.
        /// </summary>
        /// <returns>The type of the card this controller's parent is associated to</returns>
        internal CardBasicModel.CardType CardType()
        {
            return Parent.CardType();
        }

        /// <summary>
        /// Returns the grandpa.
        /// </summary>
        /// <returns>The grandpa</returns>
        public AbstractSequenceGroupController Group()
        {
            return Parent.GetGroup();
        }

        /// <summary>
        /// Returns the index of this controller in the list of all controllers the parent has.
        /// </summary>
        /// <returns>The index if the controller is found in the list. -1 otherwise.</returns>
        public int Index()
        {
            return Parent.Tabs.IndexOf(this);
        }

        /// <summary>
        /// Searches all the <see cref=" AbstractCardController"/>'s for the Sequence whose duration is maximal and whose index is the same as this one.
        /// It throws an exception if the index of the current sequence controller is not present in all other cards.
        /// </summary>
        /// <returns>The maximum duration</returns>
        /// <exception cref=" System.IndexOutOfRangeException"></exception>
        public double LongestDurationAllSequences()//RECO do this with a class that sees the entire hierarchy of abstract controllers
        {
            double duration = 0;
            int index = Index();
            

            foreach (AbstractCardController window in Group().Windows)
            {
                double runTime = window.Tabs[index].ActualDuration();

                if (runTime > duration)
                    duration = runTime;
            }
            
            return duration;
        }
        /// <summary>
        /// Gets the starting time of the sequence taking all cards into consideration. This method also sets the startTime attribute of the corresponding <see cref=" SequenceModel"/> to this value.
        /// </summary>
        /// <returns>The starting time of the sequence</returns>
        public double ActualStartTime()
        {
            double startTime = Parent.StartTimeOf(this);
            Model.startTime = startTime;

            return startTime;
        }

        /// <summary>
        /// Gets the duration of this sequence which is the maximum duration of channels this sequence has
        /// </summary>
        /// <returns>The duration of the sequence.</returns>
        public double ActualDuration()
        {
            double duration = 0;

            if (Model.IsEnabled)
            {
                foreach (AbstractChannelController channel in Channels)
                {
                    double channelDuration = channel.Duration();

                    if (duration < channelDuration)
                    {
                        duration = channelDuration;
                    }
                }
            }

            return duration;
        }
    }
}
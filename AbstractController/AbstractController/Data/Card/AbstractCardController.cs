using AbstractController.Data.Sequence;
using AbstractController.Data.SequenceGroup;
using Model.Data.Cards;
using System;
using System.Collections.ObjectModel;

namespace AbstractController.Data.Card
{
    /// <summary>
    /// The abstract controller at the card level in the hierarchy. It represents part of the ModelView of a Window (of a Card).
    /// </summary>
    public abstract class AbstractCardController
    {
        //look
        /// <summary>
        /// The <see cref=" CardBasicModel"/> this controller is associated to.
        /// </summary>
        public readonly CardBasicModel Model;
        /// <summary>
        /// The direct parent of this controller.
        /// </summary>
        protected readonly AbstractSequenceGroupController Parent;
        /// <summary>
        /// A collection of <see cref=" AbstractSequenceController"/>'s which are children to this controller. 
        /// Each of which would be represented as a tab in the window.
        /// </summary>
        public ObservableCollection<AbstractSequenceController> Tabs
        { get; protected set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractCardController"/> class. It creates an empty collection of <see cref=" AbstractSequenceController"/>'s.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="parent">The parent.</param>

        protected AbstractCardController(CardBasicModel model, AbstractSequenceGroupController parent)
        {
            Tabs = new ObservableCollection<AbstractSequenceController>();
            Model = model;
            Parent = parent;
        }

        /// <summary>
        /// Returns the type of the current card.
        /// </summary>
        /// <returns>The type of the current card</returns>
        public CardBasicModel.CardType CardType()
        {
            return Model.Type;
        }

        /// <summary>
        /// Returns the parent controller
        /// </summary>
        /// <returns>The parent controller</returns>
        internal AbstractSequenceGroupController GetGroup()
        {
            return Parent;
        }

        /// <summary>
        /// Gets the previous <see cref="AbstractSequenceController"/> from the list.
        /// </summary>
        /// <param name="tab">The sequence controller which we want to get its predecessor.</param>
        /// <returns>The previous element. Doesn't return a null but throws an exception.</returns>
        /// <exception cref="System.Exception">Thrown when the provided element is already the first sequence controller in the list</exception>
        /// <exception cref="System.IndexOutOfRangeException">Thrown when the provided element is not present in the list</exception>
        internal AbstractSequenceController PreviousTab(AbstractSequenceController tab)
        {
            if (Tabs.IndexOf(tab) == 0)
                throw new Exception("There is no previous Sequence");

            int index = Tabs.IndexOf(tab);

            return Tabs[index - 1];
        }

        //RECO make it recursive
        //RECO investigate usefulness

        /// <summary>
        /// Gets the starting time of a specific sequence which is in the list. It calculates the starting time considering only the current card
        /// </summary>
        /// <param name="sequenceController">The controller of the sequence that we want to get its starting time.</param>
        /// <returns>The starting time. If the specified <c>SequenceController</c> is not present in the list, it returns 0.0 .</returns>
        public double GetStartTime(AbstractSequenceController sequenceController)
        {
            int index = Tabs.IndexOf(sequenceController);

            if (index == 0)
                return 0;

            double startTime = 0;

            for (int iTab = 0; iTab < index; iTab++)
            {
                startTime += Tabs[iTab].ActualDuration();
            }

            return startTime;
        }

        /// <summary>
        /// Gets the starting time of a specific sequence which is in the list. It calculates the starting time considering all cards.
        /// </summary>
        /// <param name="sequence">The controller of the sequence that we want to get its starting time.</param>
        /// <returns>The starting time.  If the specified <c>SequenceController</c> is not present in the list, it returns 0.0 .</returns>
        public double StartTimeOf(AbstractSequenceController sequence)
        {
            var index = Tabs.IndexOf(sequence);
            double startTime = 0;

            for (var i = 0; i < index; i++)
            {
                startTime += Tabs[i].LongestDurationAllSequences();
            }

            return startTime;
        }

        //RECO just call the StartTimeOf method for the last sequence and add its duration
        /// <summary>
        /// Returns the duration of the card as the sum of the all the durations of the sequences this card has. Remember: the duration of a sequence is calculated relative to all cards not just this one.
        /// </summary>
        /// <returns>The duration of the card.</returns>
        public double Duration()
        {
            double duration = 0;

            foreach (AbstractSequenceController sequence in Tabs)
            {
                duration += sequence.LongestDurationAllSequences();
            }

            return duration;
        }




    }
}
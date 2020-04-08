using Model.V4.Data.Channels;
using Model.V4.Data.Cookbook;
using Model.V4.Data.SequenceGroups;
using Model.V4.Data.Sequences;
using Model.V4.Data.Steps;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace Model.V4.Data.Cards
{
    /// <summary>
    /// Represents a card (digital or analog) in the model hierarchy. It contains a list of all <see cref=" SequenceModel"/>'s for this card.
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    [KnownType(typeof (CardModel))]
    public abstract class CardBasicModel
    {
        /// <summary>
        /// The base name of analog cards
        /// </summary>
        public const string ANALOG_CARD_BASE_NAME = "AO";

        /// <summary>
        /// The base name for digital cards
        /// </summary>
        public const string DIGITAL_CARD_BASE_NAME = "DO";

        //RECO The type of a card should be represented as class type rather than an enum
        /// <summary>
        /// An enumeration type representing the two card types (digital and analog)
        /// </summary>
        public enum CardType
        {
            Digital,
            Analog            
        }

        #region fields and properties
        /// <summary>
        /// A readonly field representing the name of card. Usually something like 'AO1'
        /// </summary>
        /// 
        [DataMember] public readonly string Name;
        /// <summary>
        /// A readonly field representing the number of channels associated to this card
        /// </summary>
        /// 
        [DataMember] public readonly uint NumberOfChannels;
        /// <summary>
        /// The collection of <see cref=" SequenceModel"/>s belonging to this card 
        /// </summary>
        /// 
        [DataMember] public readonly ObservableCollection<SequenceModel> Sequences;
        /// <summary>
        /// A collection of all <see cref=" ChannelSettingsModel"/> of the channels this card has (in all <see cref="Model.Data.Sequences.SequenceModel"/>s)
        /// </summary>
        [DataMember] public readonly ObservableCollection<ChannelSettingsModel> Settings;
        /// <summary>
        /// Determines if this card is a digital or analog
        /// </summary>
        /// 
        [DataMember] public readonly CardType Type;

        /// <summary>
        /// Used to create child <see cref=" SequenceModel"/>s to this card
        /// </summary>
        /// 
        [DataMember] private readonly ModelRecipe _recipe = new ModelRecipe();

        /// <summary>
        /// The parent of this object
        /// </summary>
        [DataMember] public readonly SequenceGroupModel _parent;
        

        #endregion

        /// <summary>
        /// Initializes a new instance of this class. It creates an empty collection of <see cref=" SequenceModel" />s
        /// and an empty collection of <see cref=" ChannelSettingsModel" />s
        /// </summary>
        /// <param name="name">The name of the card</param>
        /// <param name="numberOfChannels">The number of channels the card has</param>
        /// <param name="startIndex">The starting index of numbering the channels of this card</param>
        /// <param name="type">The type of the card (digital/analog)</param>
        /// <param name="parent">The parent of this card</param>
        protected CardBasicModel(string name, uint numberOfChannels, CardType type, SequenceGroupModel parent)
        {
            Settings = new ObservableCollection<ChannelSettingsModel>();
            NumberOfChannels = numberOfChannels;
            _parent = parent;
            Name = name;
            Type = type;
            Sequences = new ObservableCollection<SequenceModel>();
        }

        /// <summary>
        /// Adds a new <see cref="SequenceModel"/> and attaches it to this card
        /// </summary>
        /// <returns>The newly created <see cref="SequenceModel"/></returns>
        public SequenceModel SequenceAdd()
        {
            return _recipe.CookSequence(NumberOfChannels, this);
        }

        //RECO have a better name for the method, and get rid of the unused parameter (step), and unify the behavior of returning null or throwing an Exception on failure

        /// <summary>
        /// Searches the previous sequences backwards for the same channel, and if the channel is not empty it returns its last step, otherwise it continues searching the sequences.
        /// If the specified sequence is the first then this method throws an exception
        /// </summary>
        /// <param name="sequence">The current sequence</param>
        /// <param name="channel">The current channel</param>
        /// <param name="step">Not used!</param>
        /// <returns>The found step, or <c>null</c> if no step if found</returns>
        /// <exception cref="System.Exception"></exception>
        internal StepBasicModel PreviousStep(SequenceModel sequence, ChannelBasicModel channel, StepBasicModel step)
        {
            SequenceModel previousSequence = PreviousSequence(sequence);

            while (previousSequence.Channels[channel.Index()].Steps.Count == 0)
            {
                if (previousSequence.Index() == 0)
                {
                    return null;
                }

                previousSequence = PreviousSequence(previousSequence);
            }

            return previousSequence.Channels[channel.Index()].Steps.Last();
        }

        /// <summary>
        /// Returns the previous <see cref="SequenceModel"/> of the specified sequence in the collection of sequences this card has.
        /// If the specified sequence is the first then this method throws an exception
        /// </summary>
        /// <param name="sequence">The specified <see cref="SequenceModel"/></param>
        /// <returns>The sequence before the one specified</returns>
        /// <exception cref="System.Exception"></exception>
        internal SequenceModel PreviousSequence(SequenceModel sequence)
        {
            int index = Sequences.IndexOf(sequence);

            if (index > 0)
            {
                return Sequences[index - 1];
            }

            throw new Exception("Already first sequence.");
        }

        /// <summary>
        /// Moves a <see cref="SequenceModel"/> from one index to another in the collection of sequences this card has.
        /// </summary>
        /// <param name="oldIndex">The old index of the sequence</param>
        /// <param name="newIndex">The new index of the sequence</param>
        internal void SequenceMove(int oldIndex, int newIndex)
        {
            Sequences.Move(oldIndex, newIndex);
        }

        /// <summary>
        /// Returns the index of the specified sequence in the collection of all sequences this card has.
        /// </summary>
        /// <param name="sequence">The specified sequence</param>
        /// <returns>The index of the specified sequence or -1 if the sequence is not found</returns>
        internal int IndexOf(SequenceModel sequence)
        {
            return Sequences.IndexOf(sequence);
        }

        /// <summary>
        /// Returns the last sequence of this card. Throws an exception if the card has no sequences.
        /// </summary>
        /// <returns>The last sequence of this card</returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        internal SequenceModel LastSequence()
        {
            return Sequences.Last();
        }

        /// <summary>
        /// Delegates the validation to the child <see cref=" SequenceModel"/>s
        /// </summary>
        /// <returns><c>true</c> if the <see cref=" CardBasicModel"/> is valid, <c>false</c> otherwise</returns>
        public bool Verify()
        {
            bool flag = true;
            foreach (SequenceModel sequence in Sequences)
            {
                if (!sequence.Verify())
                    flag = false;
            }
            return flag;
        }
    }
}
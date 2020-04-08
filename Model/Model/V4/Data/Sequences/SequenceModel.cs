using Model.V4.Data.Cards;
using Model.V4.Data.Channels;
using Model.V4.Data.Steps;
using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Model.V4.Data.Sequences
{
    /// <summary>
    /// Contains a set of steps of all channels of one card
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class SequenceModel
    {
        /// <summary>
        /// The card this sequence belongs to.
        /// </summary>
        [DataMember] private readonly CardBasicModel _parent;
        /// <summary>
        /// The name of this sequence. The default value is "Sequence"
        /// </summary>
        [DataMember] public string Name = "Sequence";
        /// <summary>
        /// A collection of all channels this sequence has.
        /// </summary>
        [DataMember] public ObservableCollection<ChannelBasicModel> Channels { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this sequence is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this sequence is enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsEnabled { set; get; }

        /// <summary>
        /// The starting time of the of this sequence. It gets calculated by <see cref=" AbstractSequenceController"/>
        /// </summary>
        public double startTime;

        /// <summary>
        /// Initializes a new instance of <see cref="SequenceModel"/>. Creates a new collection of <see cref=" ChannelBasicModel"/>s
        /// </summary>
        /// <param name="parent">The card this sequence belongs to</param>
        public SequenceModel(CardBasicModel parent)
        {
            _parent = parent;
            Channels = new ObservableCollection<ChannelBasicModel>();
            IsEnabled = true;
        }

        /// <summary>
        /// Returns the collection of all <see cref=" ChannelSettingsModel"/>'s
        /// </summary>
        /// <returns>The collection of all <see cref=" ChannelSettingsModel"/>'s</returns>
        public ObservableCollection<ChannelSettingsModel> ChannelSettings()
        {
            return _parent.Settings;
        }

        /// <summary>
        /// Returns the index of the specified channel. Returns -1 if the channel is not found.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns>The index of the specified channel. Returns -1 if the channel is not found.</returns>
        internal int IndexOf(ChannelBasicModel channel)
        {
            return Channels.IndexOf(channel);
        }

        //RECO have a better name for the method, and get rid of the unused parameter (step), and unify the behavior of returning null or throwing an Exception on failure
        /// <summary>
        /// Delegates the task to the parent <see cref="CardBasicModel"/> 
        /// </summary>
        /// <param name="channel">The current channel</param>
        /// <param name="step">Not used!</param>
        /// <returns>The found step, or <c>null</c> if no step if found</returns>
        /// <exception cref="System.Exception"></exception>
        internal StepBasicModel PreviousStep(ChannelBasicModel channel, StepBasicModel step)
        {
            return _parent.PreviousStep(this, channel, step);
        }

        /// <summary>
        /// Delegates the task to the parent <see cref="CardBasicModel"/> 
        /// </summary>
        /// <returns>The sequence before this one in the same card.</returns>
        /// <exception cref="System.Exception"></exception>
        public SequenceModel PreviousSequence()
        {
            return _parent.PreviousSequence(this);
        }

        /// <summary>
        /// Delegates the task to the parent <see cref="CardBasicModel"/> 
        /// </summary>
        /// <returns>The result of executing <c>_parent.IndexOf(this);</c></returns>
        public int Index()
        {
            return _parent.IndexOf(this);
        }

        /// <summary>
        /// Delegates the validation to the child <see cref=" ChannelBasicModel"/>s
        /// </summary>
        /// <returns><c>true</c> if the <see cref=" SequenceModel"/> is valid, <c>false</c> otherwise</returns>
        public bool Verify()
        {
            bool flag = true;
            foreach (ChannelBasicModel channel in Channels)
            {
                if (!channel.Verify())
                    flag = false;
            }
            return flag;
        }

        /// <summary>
        /// Returns the parent.
        /// </summary>
        /// <returns>The parent.</returns>
        public CardBasicModel Card()
        {
            return _parent;
        }

        
    }
}
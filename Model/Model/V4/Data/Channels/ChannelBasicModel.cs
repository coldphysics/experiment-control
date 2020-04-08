using Model.V4.Data.Cards;
using Model.V4.Data.Sequences;
using Model.V4.Data.Steps;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace Model.V4.Data.Channels
{
    /// <summary>
    /// Contains the steps of a channel of a specific sequence
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    [KnownType(typeof (ChannelModel))]
    public abstract class ChannelBasicModel
    {
        #region fields and properties
        /// <summary>
        /// The sequence this channel belongs to
        /// </summary>
        [DataMember] protected readonly SequenceModel Parent;
        /// <summary>
        /// A collection of all steps this channel has.
        /// </summary>
        [DataMember] public readonly ObservableCollection<StepBasicModel> Steps;
        /// <summary>
        /// Gets the settings of this channel. By going up to the card (grandpa)
        /// </summary>
        public ChannelSettingsModel Setting
        {
            get
            {
                return Parent.ChannelSettings()[Index()];
            }
        }
        #endregion


        /// <summary>
        /// Initializes a new instance of <see cref="ChannelBasicModel"/>. Creates a new collection of <see cref=" StepBasicModel"/>s
        /// </summary>
        /// <param name="parent">The sequence this channel belongs to</param>
        protected ChannelBasicModel(SequenceModel parent)
        {
            Steps = new ObservableCollection<StepBasicModel>();
            Parent = parent;
        }

        #region methods
        /// <summary>
        /// Delegates the validation to the child <see cref=" StepBasicModel"/>'s
        /// </summary>
        /// <returns><c>true</c> if the <see cref=" ChannelBasicModel"/> is valid, <c>false</c> otherwise</returns>
        public bool Verify()
        {
            bool flag = true;

            foreach (StepBasicModel step in Steps)
            {
                if (!step.Verify())
                    flag = false;
            }
            return flag;
        }

        /// <summary>
        /// Adds a new step at the end of this channel's <see cref="StepBasicModel"/> collection.
        /// </summary>
        /// <param name="newStep">The specified step</param>
        public void StepAdd(StepBasicModel newStep)
        {
            Steps.Add(newStep);
        }


        /// <summary>
        /// Delegates the task to the parent <see cref="SequenceModel" />
        /// </summary>
        /// <returns>The result of <c>Parent.IndexOf(this);</c></returns>
        public int Index()
        {
            return Parent.IndexOf(this);
        }

        /// <summary>
        /// Returns the previous step if the current step isn't the first one among this channel's steps, otherwise delegates the task to the parent <see cref="SequenceModel"/> 
        /// </summary>
        /// <param name="step">The current step</param>
        /// <returns>The previous step. if there is no steps previous to the current one, it could return<c>null</c> or raise an exception (in the case that this step belongs to the first sequence) </returns>
        /// <exception cref="System.Exception"></exception>
        public StepBasicModel PreviousStep(StepBasicModel step)
        {
            return Steps.IndexOf(step) > 0
                       ? Steps.ElementAt(Steps.IndexOf(step) - 1)
                       : Parent.PreviousStep(this, step);
        }

        /// <summary>
        /// Returns the sequence this channel belongs to.
        /// </summary>
        /// <returns>The sequence this channel belongs to.</returns>
        internal SequenceModel Sequence()
        {
            return Parent;
        }

        /// <summary>
        /// Returns the UpperLimit value of this channel's settings which is stored in <see cref="ChannelSettingsModel" />
        /// </summary>
        /// <returns>The UpperLimit value of this channel's settings which is stored in <see cref="ChannelSettingsModel" /></returns>
        public double UpperLimit()
        {
            return Setting.UpperLimit;
        }

        /// <summary>
        /// Reuters the LowerLimit value of this channel's settings which is stored in <see cref="ChannelSettingsModel"/>
        /// </summary>
        /// <returns>The LowerLimit value of this channel's settings which is stored in <see cref="ChannelSettingsModel"/></returns>
        public double LowerLimit()
        {
            return Setting.LowerLimit;
        }

        /// <summary>
        /// Returns a reference to grandpa <see cref="CardBasicModel"/>
        /// </summary>
        /// <returns>A reference to grandpa <see cref="CardBasicModel"/></returns>
        public CardBasicModel Card()
        {
            return Parent.Card();
        }


        #endregion
    }
}
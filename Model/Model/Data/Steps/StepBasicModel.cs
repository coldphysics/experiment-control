using Errors.Error;
using Model.BaseTypes;
using Model.Data.Cards;
using Model.Data.Channels;
using Model.Data.Sequences;
using Model.Settings;
using System;
using System.Runtime.Serialization;

namespace Model.Data.Steps
{


    /// <summary>
    /// Represents the method that will handle the Error event of a step's non-verified condition (e.g. a value of a step is bigger than the Upperlimit)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ErrorEventHandler(object sender, EventArgs e);

    //RECO not all derived classes use the value (the StepFileMode class doesn't need them), so a separate level of the hierarchy should be made for steps needing the duration and value
    /// <summary>
    /// An abstract class describing the properties of a step which belongs to a channel,it has two derived classes (StepFileModel, StepRampModel) 
    /// </summary>
    [Serializable]
    [DataContract]
    [KnownType(typeof (StepRampModel))]
    [KnownType(typeof (StepFileModel))]
    [KnownType(typeof (StepPythonModel))]
    public abstract class StepBasicModel
    {
        #region fields and properties
        /// <summary>
        /// Represents the duration this step needs to be accomplished 
        /// </summary>
        [DataMember] public ValueDoubleModel Duration;

        /// <summary>
        /// For constant steps it represents the constant value, for the linear and exponential steps it represents the final value to reach,
        /// for file-based steps, it represents the first value in the file (seems useless here). And for steps associated with a variable, represent's the variable's value for the current cycle.
        /// </summary>
        [DataMember] public ValueDoubleModel Value;

        /// <summary>
        /// A reference to the <see cref="ChannelBasicModel"/> this step belongs to.
        /// </summary>
        [DataMember] protected ChannelBasicModel Parent;

        //RECO the value is being calculated in the view, so investigate the usefulness of keeping it in the model
        /// <summary>
        /// The start running time of this step
        /// </summary>
        [DataMember] public double StartTime;

        //RECO the association between the Step and the Variable should be done by a reference rather than a string
        /// <summary>
        /// The name of the variable that is associated with the duration of this step. If this step is not associated to a variable this field is null.
        /// </summary>
        [DataMember] public string DurationVariableName;

        /// <summary>
        /// The name of the variable that is associated with the value of this step. If this step is not associated to a variable this field is null.
        /// </summary>
        [DataMember] public string ValueVariableName;

        /// <summary>
        /// Indicates whether the comment message associated with this step is critical <c>true</c> or normal <c>false</c>. 
        /// If it is critical then a description of this step would be present in the error window
        /// </summary>
        [DataMember] public bool MessageState = false;

        /// <summary>
        /// The description message associated with this step. 
        /// If the state of this message is <c>true</c> (critical) this message will appear in the description 
        /// that will be shown in the error window.
        /// </summary>
        [DataMember] public string MessageString = "";


        /// <summary>
        /// Gets the settings of the parent <see cref="ChannelBasicModel"/> 
        /// </summary>
        public ChannelSettingsModel Setting
        {
            get { return Parent.Setting; }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="StepBasicModel"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public StepBasicModel(ChannelBasicModel parent)
        {
            this.Parent = parent;
            Duration = new ValueDoubleModel();
            Value = new ValueDoubleModel();
        }

        /// <summary>
        /// Set the value of StartTime.
        /// </summary>
        /// <param name="startTime">The new value of start time</param>
        public void SetStartTime(double startTime)
        {
            StartTime = startTime;
        }

        /// <summary>
        /// Verifies the validity of the step. Implemented by the derived classes.
        /// </summary>
        /// <returns><c>true</c> if the <see cref=" StepBasicModel"/> is valid, <c>false</c> otherwise</returns>
        public virtual bool Verify()
        {
            ErrorCollector errorCollector = ErrorCollector.Instance;

            if (MessageState == true)
            {
                if (MessageString == "")
                {
                    errorCollector.AddError(
                        "Critical state at " + this.Parent.Card().Name + ", " + this.Parent.Sequence().Name + ", " +
                        this.Parent.Setting.Name + ", Step " + this.Index(), ErrorCategory.Messages, false,
                        ErrorTypes.Other);
                }
                else
                {
                    errorCollector.AddError(
                        "Critical state (" + MessageString + ") at " + this.Parent.Card().Name + ", " + this.Parent.Sequence().Name + ", " +
                        this.Parent.Setting.Name + ", Step " + this.Index(), ErrorCategory.Messages, false,
                        ErrorTypes.Other);
                }
            }

            return true;
        }

        /// <summary>
        /// Returns the index of this step among the collection of steps which its parent <see cref="ChannelBasicModel"/> has
        /// </summary>
        /// <returns>The index of the step, or (-1) if the step is not found. </returns>
        public int Index()
        {
            return Parent.Steps.IndexOf(this);
        }

        /// <summary>
        /// Returns the direct parent <see cref="ChannelBasicModel"/>
        /// </summary>
        /// <returns>The direct parent <see cref="ChannelBasicModel"/></returns>
        public ChannelBasicModel Channel()
        {
            return Parent;
        }

        /// <summary>
        /// Returns a reference to the grand-grand parent <see cref="CardBasicModel"/> this step belongs to.
        /// </summary>
        /// <returns>A reference to the grand-grand parent <see cref="CardBasicModel"/> this step belongs to</returns>
        public CardBasicModel Card()
        {
            return Parent.Card();
        }

        /// <summary>
        /// Returns a reference to the grand parent <see cref="SequenceModel"/> this step belongs to.
        /// </summary>
        /// <returns>A reference to the grand parent <see cref="SequenceModel"/> this step belongs to</returns>
        public SequenceModel Sequence()
        {
            return Parent.Sequence();
        }

        /// <summary>
        /// Returns the upper limit value taken from  <see cref="Model.Data.Channels.ChannelSettingsModel"/> of the channel this step belongs to.
        /// </summary>
        /// <returns>The upper limit value taken from  <see cref="Model.Data.Channels.ChannelSettingsModel"/> of the channel this step belongs to</returns>
        public double UpperLimit()
        {
            return Parent.UpperLimit();
        }

        /// <summary>
        /// Returns the lower limit value taken from  <see cref="Model.Data.Channels.ChannelSettingsModel"/> of the channel this step belongs to.
        /// </summary>
        /// <returns>The lower limit value taken from  <see cref="Model.Data.Channels.ChannelSettingsModel"/> of the channel this step belongs to</returns>
        public double LowerLimit()
        {
            return Parent.LowerLimit();
        }

        /// <summary>
        /// Returns the smallest indivisible time step for each minor cycle.
        /// </summary>
        /// <returns>The smallest indivisible time step for each minor cycle.</returns>
        public double SmallestStepSize()
        {
            return TimeSettingsInfo.GetInstance().SmallestTimeStep;
        }


        /// <summary>
        /// Calls the (PreviousStep) method of its parent.
        /// </summary>
        /// <returns>A reference to its preceding step</returns>
        public StepBasicModel PreviousStep()
        {
            return Parent.PreviousStep(this);
        }
    }
}
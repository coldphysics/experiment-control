using Errors.Error;
using Model.V2.BaseTypes;
using Model.V2.Data.Channels;
using System;
using System.Runtime.Serialization;

namespace Model.V2.Data.Steps
{

    /// <summary>
    /// An implementation of <see cref="StepBasicModel"/> defining the step which has a constant or a ramp function (linear, exponential) as a value
    /// </summary>
    [Serializable]
    [DataContract]
    public class StepRampModel : StepBasicModel
    {
        /// <summary>
        /// An enumeration type defining the different types of step values (Constant, Linear, Exponential)
        /// </summary>
        public enum StoreType
        {
            /// <summary>
            /// Holds the value on the output for the specified duration
            /// </summary>
            Constant,
            /// <summary>
            /// Goes linearly from the previous value to the specified value and reaches it exactly when the specified duration passes
            /// </summary>
            Linear,
            /// <summary>
            /// Goes exponentially from the previous value up to the absolute of the specified value and reaches it exactly when the specified duration passes
            /// </summary>
            Exponential
        }

        /// <summary>
        /// Represents the step's value type which is defined in (StoreType) enumeration
        /// </summary>
        [DataMember] public StoreType Store;

        //RECO this field is present in all derived classes of StepBasicModel so it should be in the superclass
        /// <summary>
        /// The event that will be triggered when an error occurs
        /// </summary>
        [field: NonSerialized] public event ErrorEventHandler Valid;

        //RECO split the class into multiple sub-classes, because the validation operation has some differences for each type of steps
        /// <summary>
        /// Initializes a new instance of StepRampModel class which contains the duration and the value of this step
        /// </summary>
        /// <param name="parent">The channel which this step belongs to</param>
        /// <param name="store">The type of the value</param>
        public StepRampModel(ChannelBasicModel parent, StoreType store)
        {
            Parent = parent;
            Value = new ValueDoubleModel();
            Duration = new ValueDoubleModel();
            Store = store;
        }


        //RECO this method is present in all derived classes of StepBasicModel so it should be in the superclass
        /// <summary>
        /// Returns the index of this step among the collection of steps which its parent <see cref="ChannelBasicModel"/> has
        /// </summary>
        /// <returns>The index of the step, or (-1) if the step is not found. </returns>
        public int Index()
        {
            return Parent.Steps.IndexOf(this);
        }

       
        /// <summary>
        /// The implementation of the abstract method of <see cref="StepBasicModel"/>.
        /// It verifies that the duration of this step is positive, and 
        /// verifies that the duration of this step is a multiple of the SmallestStepSize
        /// It adds an error to the errorCollector in case any condition is not correctly verified.
        /// This method also checks if the step is critical through the field <c>MessageState</c>, and if so, it adds a message describing 
        /// this step in the <c>errorCollector</c> (even if no errors are detected)
        /// </summary>
        /// <returns><c>true</c> if all conditions are met, <c>false</c> otherwise </returns>
        public override bool Verify()
        {
            ErrorCollector errorCollector = ErrorCollector.Instance;
            //errorCollector.reset(ErrorWindow.Variables, ErrorTypes.DynamicCompileError);
            bool flag = true;
            double steps = Duration.Value / SmallestStepSize();

            
            //Validation of the upper and lower limits is done in the ValidationUnit of the Buffer project
            //********************************************************************************************

            //System.Console.Write("Verifying! {0} {1}\n", Value.Value, UpperLimit());
            //if (Value.Value > UpperLimit())
            //{
            //    if (Valid != null)
            //        Valid(this, new EventArgs());
            //    flag = false;
            //    errorCollector.AddError("Value > UpperLimit at " + this.Parent.Card().Name + ", " + this.Parent.Sequence().Name + ", " + this.Parent.Setting.Name + ", Step " + this.Index(), ErrorWindow.MainHardware, false, ErrorTypes.OutOfRange);
            //}

            //if (Value.Value < LowerLimit())
            //{
            //    if (Valid != null)
            //        Valid(this, new EventArgs());
            //    flag = false;
            //    errorCollector.AddError("Value < LowerLimit at " + this.Parent.Card().Name + ", " + this.Parent.Sequence().Name + ", " + this.Parent.Setting.Name + ", Step " + this.Index(), ErrorWindow.MainHardware, false, ErrorTypes.OutOfRange);
            //}

            if (Duration.Value < 0)
            {
                if (Valid != null)
                    Valid(this, new EventArgs());
                flag = false;
                errorCollector.AddError("Duration < 0 at " + this.Parent.Card().Name + ", " + this.Parent.Sequence().Name + ", " + this.Parent.Setting.Name + ", Step " + this.Index(), ErrorCategory.MainHardware, false, ErrorTypes.NegativeTime);
            }

            if (Math.Abs(steps - Math.Round(steps)) >= 0.00001) //steps = Duration.Value/SmallestStepSize(); , should be an integer, if not, Value is not a multiple of SmallestStepSize
            {
                if (Valid != null)
                    Valid(this, new EventArgs());
                flag = false;
                errorCollector.AddError("Duration not a multiple of the SampleRate at " + this.Parent.Card().Name + ", " + this.Parent.Sequence().Name + ", " + this.Parent.Setting.Name + ", Step " + this.Index(), ErrorCategory.MainHardware, false, ErrorTypes.StrangeStephanError);
            }

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

            return flag;
        }
    }
}
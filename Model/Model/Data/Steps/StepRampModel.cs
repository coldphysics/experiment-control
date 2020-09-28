using Errors.Error;
using Model.Data.Channels;
using System;
using System.Runtime.Serialization;

namespace Model.Data.Steps
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
            [Obsolete("The exponential step type is obsolete. Use the Python step type instead.")]
            Exponential
        }

        /// <summary>
        /// Represents the step's value type which is defined in (StoreType) enumeration
        /// </summary>
        [DataMember] public StoreType Store;


        //RECO split the class into multiple sub-classes, because the validation operation has some differences for each type of steps
        /// <summary>
        /// Initializes a new instance of StepRampModel class which contains the duration and the value of this step
        /// </summary>
        /// <param name="parent">The channel which this step belongs to</param>
        /// <param name="store">The type of the value</param>
        public StepRampModel(ChannelBasicModel parent, StoreType store)
            :base(parent)
        {
            Store = store;
        }

        /// <summary>
        /// Verifies that the duration of this step is positive, and 
        /// verifies that the duration of this step is a multiple of the SmallestStepSize
        /// It adds an error to the errorCollector in case any condition is not correctly verified.
        /// This method also checks if the step is critical through the field <c>MessageState</c>, and if so, it adds a message describing 
        /// this step in the <c>errorCollector</c> (even if no errors are detected)
        /// </summary>
        /// <returns><c>true</c> if all conditions are met, <c>false</c> otherwise </returns>
        public override bool Verify()
        {
            ErrorCollector errorCollector = ErrorCollector.Instance;
            bool flag = true;
            double steps = Duration.Value / SmallestStepSize();


            if (Duration.Value < 0)
            {
                flag = false;
                errorCollector.AddError("Duration < 0 at " + this.Parent.Card().Name + ", " + this.Parent.Sequence().Name + ", " + this.Parent.Setting.Name + ", Step " + this.Index(), ErrorCategory.MainHardware, false, ErrorTypes.NegativeTime);
            }

            if (Math.Abs(steps - Math.Round(steps)) >= 0.00001) //steps = Duration.Value/SmallestStepSize(); , should be an integer, if not, Value is not a multiple of SmallestStepSize
            {
                flag = false;
                errorCollector.AddError("Duration not a multiple of the SampleRate at " + this.Parent.Card().Name + ", " + this.Parent.Sequence().Name + ", " + this.Parent.Setting.Name + ", Step " + this.Index(), ErrorCategory.MainHardware, false, ErrorTypes.StrangeStephanError);
            }


            return flag & base.Verify();
        }
    }
}
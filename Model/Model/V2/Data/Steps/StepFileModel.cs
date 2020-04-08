using Errors.Error;
using Model.V2.BaseTypes;
using Model.V2.Data.Channels;
using System;
using System.IO;
using System.Runtime.Serialization;

namespace Model.V2.Data.Steps
{
    /// <summary>
    /// An implementation of <see cref="StepBasicModel"/> defining a step which gets its value from written file
    /// </summary>
    [Serializable]
    [DataContract]
    public class StepFileModel : StepBasicModel
    {
        /// <summary>
        /// Enumeration type defining the two possible types of files(Csv,Binary)
        /// </summary>
        public enum StoreType
        {
            Csv,
            Binary
        }

        /// <summary>
        /// Represents the step's file type which is defined in (StoreType) enumeration
        /// </summary>
        [DataMember] public StoreType Store;

        /// <summary>
        /// The path + the name of the file
        /// </summary>
        [DataMember] public string FileName;

        /// <summary>
        /// The event that will be triggered when an error occurs
        /// </summary>
        [field: NonSerialized] public event ErrorEventHandler Valid;


        /// <summary>
        /// Initializes a new instance of <c>StepFileModel</c> class which contains the name and the value of this step
        /// </summary>
        /// <param name="parent">the channel which this step belongs to</param>
        /// <param name="store">the type of the file</param>
        public StepFileModel(ChannelBasicModel parent, StoreType store)
        {
            Parent = parent;
            Store = store;
            Value = new ValueDoubleModel();
            Duration = new ValueDoubleModel();
        }


        /// <summary>
        /// Returns the index of this step among the collection of steps which the parent <see cref="ChannelBasicModel"/> has
        /// </summary>
        /// <returns>The index of the step, or (-1) if the step is not found. </returns>
        public int Index()
        {
            return Parent.Steps.IndexOf(this);
        }

        //RECO the output of files should be validated (preferably in an online mode)

        /// <summary>
        /// The implementation of the abstract method <see cref="StepBasicModel"/>.
        /// It verifies that the filename is not empty, and
        /// verifies that the file exists.
        /// It adds an error to the errorCollector in case any condition is not correctly verified.
        /// This method also checks if the step is critical through the field <c>MessageState</c>, and if so, it adds a message describing 
        /// this step in the <c>errorCollector</c> (even if no errors are detected)
        /// Notice that no verification is being made here about the actual values stored in the file.
        /// </summary>
        /// <returns><c>true</c> if all conditions are met, <c>false</c> otherwise </returns>
        public override bool Verify()
        {
            ErrorCollector errorCollector = ErrorCollector.Instance;
            bool flag = true;

            if (FileName == "")
            {
                if (Valid != null)
                    Valid(this, new EventArgs());
                flag = false;
                errorCollector.AddError("FileName == \"\" at " + this.Parent.Card().Name + ", " + this.Parent.Sequence().Name + ", " + this.Parent.Setting.Name + ", Step " + this.Index(), ErrorWindow.MainHardware, false, ErrorTypes.FileNameEmpty);
            }

            if (!File.Exists(FileName))
            {
                if (Valid != null)
                    Valid(this, new EventArgs());
                flag = false;
                errorCollector.AddError("FileName not found at " + this.Parent.Card().Name + ", " + this.Parent.Sequence().Name + ", " + this.Parent.Setting.Name + ", Step " + this.Index(), ErrorWindow.MainHardware, false, ErrorTypes.FileNotFound);
            }

            if (MessageState == true)//The step is critical (Notice that this does not change the value of th flag)
            {
                if (MessageString == "")//A messages string is not specified, so we use a generic message
                {
                    errorCollector.AddError(
                        "Critical state at " + this.Parent.Card().Name + ", " + this.Parent.Sequence().Name + ", " +
                        this.Parent.Setting.Name + ", Step " + this.Index(), ErrorWindow.Messages, false,
                        ErrorTypes.Other);
                }
                else//A message is specified, so we include it in the resulting message
                {
                    errorCollector.AddError(
                        "Critical state (" + MessageString + ") at " + this.Parent.Card().Name + ", " + this.Parent.Sequence().Name + ", " +
                        this.Parent.Setting.Name + ", Step " + this.Index(), ErrorWindow.Messages, false,
                        ErrorTypes.Other);
                }
            }

            return flag;
        }
    }
}
using Errors.Error;
using Model.BaseTypes;
using Model.Data.Channels;
using System;
using System.IO;
using System.Runtime.Serialization;

namespace Model.Data.Steps
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
        /// Initializes a new instance of <c>StepFileModel</c> class which contains the name and the value of this step
        /// </summary>
        /// <param name="parent">the channel which this step belongs to</param>
        /// <param name="store">the type of the file</param>
        public StepFileModel(ChannelBasicModel parent, StoreType store)
            :base(parent)
        {
            Duration = new ValueDoubleModel();
            Store = store;
        }


        /// <summary>
        /// Verifies that the filename is not empty, and
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
                flag = false;
                errorCollector.AddError("FileName == \"\" at " + this.Parent.Card().Name + ", " + this.Parent.Sequence().Name + ", " + this.Parent.Setting.Name + ", Step " + this.Index(), ErrorCategory.MainHardware, false, ErrorTypes.FileNameEmpty);
            }

            if (!File.Exists(FileName))
            {
                flag = false;
                errorCollector.AddError("FileName not found at " + this.Parent.Card().Name + ", " + this.Parent.Sequence().Name + ", " + this.Parent.Setting.Name + ", Step " + this.Index(), ErrorCategory.MainHardware, false, ErrorTypes.FileNotFound);
            }

            return flag & base.Verify();
        }
    }
}
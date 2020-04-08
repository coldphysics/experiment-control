using Errors.Error;
using Model.Data.Channels;
using System;

namespace Model.Data.Steps
{
    /// <summary>
    /// A step that uses a python script to produce the output.
    /// </summary>
    /// <seealso cref="Model.Data.Steps.StepBasicModel" />
    [Serializable]
    public class StepPythonModel:StepBasicModel
    {
        /// <summary>
        /// The possible sub-types for this step (used to be consistent with the other 
        /// </summary>
        public enum StoreType
        {
            Python
        }

        /// <summary>
        /// Gets or sets the store.
        /// </summary>
        /// <value>
        /// The store.
        /// </value>
        public StoreType Store
        {
            set;
            get;
        }

        /// <summary>
        /// Gets or sets the script.
        /// </summary>
        /// <value>
        /// The script.
        /// </value>
        public string Script { set; get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepPythonModel"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public StepPythonModel(ChannelBasicModel parent)
            :base(parent)
        {
            Store = StoreType.Python;
        }
        
        /// <summary>
        /// Verifies this instance by ensuring that a script exists and executing the general verification of a step.
        /// </summary>
        /// <returns><c>true</c> if there is an error in the step</returns>
        public override bool Verify()
        {
            ErrorCollector errorCollector = ErrorCollector.Instance;
            bool flag = true;

            if (Script == null || Script.Trim().Length == 0)
            {
                errorCollector.AddError("Pyhton script is not specified at " + this.Parent.Card().Name + ", " + this.Parent.Sequence().Name + ", " + this.Parent.Setting.Name + ", Step " + this.Index(), ErrorWindow.MainHardware, false, ErrorTypes.DynamicCompileError);
                flag = false;
            }

            return flag & base.Verify();
        }
        
    }
}

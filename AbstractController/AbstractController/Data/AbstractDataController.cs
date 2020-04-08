using AbstractController.Data.SequenceGroup;
using Model.Data;
using Model.Settings;

namespace AbstractController.Data
{
    /// <summary>
    /// The abstract controller at the data level of the model hierarchy
    /// </summary>
    public class AbstractDataController
    {
        /// <summary>
        /// The <see cref=" DataModel"/> this controller is associated with.
        /// </summary>
        protected readonly DataModel Model;
        /// <summary>
        /// The direct parent of this controller
        /// </summary>
        protected readonly AbstractRootController Parent;
        /// <summary>
        /// The set of <see cref=" AbstractSequenceGroupController"/>'s this controller has. 
        /// </summary>
        public AbstractSequenceGroupController SequenceGroup;
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractDataController"/> class. Does not add any initial <see cref=" AbstractSequenceGroupController"/>!
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="parent">The parent.</param>
        public AbstractDataController(DataModel model, AbstractRootController parent)
        {
            Model = model;
            Parent = parent;
        }

        /// <summary>
        /// Returns duration of the entire model in seconds as the duration of the sequence group divided by the input time base 
        /// </summary>
        /// <returns>The duration in seconds</returns>
        
        public double Duration()
        {
            return SequenceGroup.Duration() / TimeSettingsInfo.GetInstance().InputTimeBase;

        }

      
        
    }
}
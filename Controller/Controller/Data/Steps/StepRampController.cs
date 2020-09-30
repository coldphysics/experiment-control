using System;
using Controller.Data.Channels;
using Errors.Error;
using Model.Data.Steps;

namespace Controller.Data.Steps
{
    /// <summary>
    /// A controller for the steps that use a single value to determine the output
    /// </summary>
    /// <seealso cref="Controller.Data.Steps.StepBasicController" />
    public abstract class StepRampController : StepBasicController
    {
        // ******************** properties ********************        
        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        public StepRampModel Model
        {
            get
            {
                return (StepRampModel)_model;
            }
        }

        /// <summary>
        /// Gets or sets the analog type selected.
        /// </summary>
        /// <value>
        /// The analog type selected.
        /// </value>
        public override ChannelBasicController.AnalogTypes AnalogTypeSelected
        {
            get
            {
                return
                    (ChannelBasicController.AnalogTypes)
                    Enum.Parse(typeof(ChannelBasicController.AnalogTypes), Model.Store.ToString());
            }
            set
            {
                StepRampModel.StoreType store;
                object errorNotificationLock = ErrorCollector.Instance.StartBulkUpdate();
                object token = _rootController.BulkUpdateStart();

                if (Enum.TryParse(value.ToString(), out store))
                {
                    Model.Store = store;
                }
                else
                {
                    ((ChannelBasicController)Parent).ChangeStep(this, value.ToString());
                }

                ((ChannelBasicController)Parent).CopyToBuffer();
                _rootController.BulkUpdateEnd(token);
                ErrorCollector.Instance.EndBulkUpdate(errorNotificationLock);
            }
        }

        /// <summary>
        /// Gets or sets the digital type selected.
        /// </summary>
        /// <value>
        /// The digital type selected.
        /// </value>
        public override ChannelBasicController.DigitalTypes DigitalTypeSelected
        {
            get
            {
                return
                    (ChannelBasicController.DigitalTypes)
                    Enum.Parse(typeof(ChannelBasicController.DigitalTypes), Model.Store.ToString());
            }
            set
            {
                StepRampModel.StoreType store;
                object errorNotificationLock = ErrorCollector.Instance.StartBulkUpdate();
                object token = _rootController.BulkUpdateStart();
                if (Enum.TryParse(value.ToString(), out store))
                {
                    Model.Store = store;
                }
                else
                {
                    ((ChannelBasicController)Parent).ChangeStep(this, value.ToString());
                }
                ((ChannelBasicController)Parent).CopyToBuffer();
                _rootController.BulkUpdateEnd(token);
                ErrorCollector.Instance.EndBulkUpdate(errorNotificationLock);
            }
        }


        // ******************** constructor ********************        
        /// <summary>
        /// Initializes a new instance of the <see cref="StepRampController"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="parent">The parent.</param>
        public StepRampController(StepRampModel model, ChannelBasicController parent)
            : base(parent, model)
        {
            ReattachVariable();
        }

    }
}
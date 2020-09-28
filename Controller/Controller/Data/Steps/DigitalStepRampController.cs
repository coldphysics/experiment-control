using Controller.Data.Channels;
using Model.Data.Steps;

namespace Controller.Data.Steps
{
    /// <summary>
    /// This class is necessary to correctly create DataTemplates for this type of steps (map VMs to Vs)
    /// </summary>
    public class DigitalStepRampController : StepRampController
    {
        public DigitalStepRampController(StepRampModel model, ChannelBasicController parent) : base(model, parent)
        {
        }
    }
}

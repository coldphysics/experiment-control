using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Controller.Data.Channels;
using Model.Data.Steps;

namespace Controller.Data.Steps
{
    /// <summary>
    /// This class is necessary to correctly create DataTemplates for this type of steps (map VMs to Vs)
    /// </summary>
    public class AnalogStepFileController : StepFileController
    {
        public AnalogStepFileController(StepFileModel model, ChannelBasicController parent) : base(model, parent)
        {
        }
    }
}

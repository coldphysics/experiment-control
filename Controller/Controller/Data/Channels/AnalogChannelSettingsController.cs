using Controller.Data.Tabs;
using Model.Data.Channels;

namespace Controller.Data.Channels
{
    /// <summary>
    /// This class is necessary to correctly create DataTemplates for this type of steps (map VMs to Vs)
    /// </summary>
    public class AnalogChannelSettingsController : ChannelSettingsController
    {
        public AnalogChannelSettingsController(ChannelSettingsModel model, TabController parent, int numOfChannel) : base(model, parent, numOfChannel)
        {
        }
    }
}

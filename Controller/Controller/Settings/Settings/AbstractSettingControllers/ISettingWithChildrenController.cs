using System.Collections.Generic;

namespace Controller.Settings.Settings.AbstractSettingControllers
{
    /// <summary>
    /// Traverses the (sub-) tree of children of this setting controller
    /// </summary>
    public interface ISettingWithChildrenController
    {
        void TraverseChildren(ICollection<SettingController> settings);
    }
}

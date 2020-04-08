using System.Collections.Generic;
using Controller.Settings.Settings.AbstractSettingControllers;
using Model.Settings.Settings;

namespace Controller.Settings.Settings
{
    /// <summary>
    /// A base controller for controllers whose model is a setting with child settings
    /// </summary>
    /// <typeparam name="T">The type of values of the model setting</typeparam>
    /// <seealso cref="Prototyping.Controller.Settings.SettingController" />
    public abstract class SettingWithChildrenController<T> : SettingController, ISettingWithChildrenController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingWithChildrenController{T}"/> class.
        /// </summary>
        /// <param name="setting">The setting.</param>
        public SettingWithChildrenController(SettingWithDynamicChildren<T> setting)
            : base(setting)
        { }

        /// <summary>
        /// Gets the collection of controllers for the child settings.
        /// </summary>
        /// <value>
        /// The child settings.
        /// </value>
        /// <remarks>This getter creates a new collection of controllers based on the current child settings whenever it is invoked</remarks>
        public ICollection<SettingController> ChildSettings
        {
            get
            {
                ICollection<BasicSetting> childSettings = ((SettingWithDynamicChildren<T>)setting).RelatedOptions;
                ICollection<SettingController> result = ControllersBuilder.BuildSettingControllers(childSettings);

                return result;
            }
        }

        /// <summary>
        /// Restores the default values for this setting.
        /// </summary>
        public override void RestoreDefaults()
        {
            base.RestoreDefaults();
            OnPropertyChanged("ChildSettings");
        }


        /// <summary>
        /// Returns true if the setting and all of its children are valid.
        /// </summary>
        /// <returns>
        ///   <c>true</c> when the setting is valid, otherwise <c>false</c>.
        /// </returns>
        public override bool IsValid()
        {
            foreach (SettingController setting in ChildSettings)
                if (!setting.IsValid())
                    return false;

            return true;
        }

        /// <summary>
        /// Traverses the (sub-) tree of children of this setting controller
        /// </summary>
        public void TraverseChildren(ICollection<SettingController> settings)
        {
            foreach (SettingController current in ChildSettings)
            {
                settings.Add(current);

                if (current is ISettingWithChildrenController)
                {
                    ((ISettingWithChildrenController)current).TraverseChildren(settings);
                }
            }
        }

    }
}

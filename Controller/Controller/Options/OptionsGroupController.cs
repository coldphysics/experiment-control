using System.Collections.Generic;
using System.Collections.ObjectModel;
using Controller.Settings.Settings;
using Model.Options;

namespace Controller.Options
{
    public class OptionsGroupController:BaseController
    {
        private OptionsGroup optionsGroup;

        public OptionsGroup OptionsGroup
        {
            get { return optionsGroup; }
        }
        /// <summary>
        /// Gets or sets the collection of associated <see cref="SettingController"/>.
        /// </summary>
        /// <value>
        /// The collection of associated <see cref="SettingController"/>..
        /// </value>
        public ICollection<SettingController> DirectOptions
        {
            get;
            set;
        }

        public ICollection<OptionsGroupController> ChildOptionGroups
        {
            set;
            get;
        }

        public string Name
        {
            get
            {
                return optionsGroup.Name;
            }

        }


        public bool IsSelected
        {
            set;
            get;
        }



        public OptionsGroupController(OptionsGroup optionsGroup)
        {
            this.optionsGroup = optionsGroup;
            DirectOptions = new ObservableCollection<SettingController>();
            ChildOptionGroups = new ObservableCollection<OptionsGroupController>();
        }

        /// <summary>
        /// Restores the default values for the direct options of the group.
        /// </summary>
        public void RestoreDefaultValues()
        {
            foreach (SettingController controller in DirectOptions)
                controller.RestoreDefaults();

            foreach (OptionsGroupController group in ChildOptionGroups)
                group.RestoreDefaultValues();
        }

        public bool IsValid()
        {
            foreach (SettingController childOption in DirectOptions)
                if (!childOption.IsValid())
                    return false;

            foreach (OptionsGroupController childGroup in ChildOptionGroups)
                if (!childGroup.IsValid())
                    return false;

            return true;
        }

    }
}

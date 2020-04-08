using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Model.Options;

namespace Controller.Options
{
    class OptionsManagerBuilder
    {
        /// <summary>
        /// The root of the controllers hierarchy to be built
        /// </summary>
        private OptionsManagerController root;


        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsManagerBuilder"/> class.
        /// </summary>
        public OptionsManagerBuilder()
        {
            OptionsManager profilesManager = OptionsManager.GetInstance();
            //profilesManager.LoadProfiles();
            root = new OptionsManagerController(profilesManager);
        }



        public static OptionsGroupController BuildOptionsGroupController(OptionsGroup model)
        {
            OptionsGroupController result = new OptionsGroupController(model);
            result.DirectOptions = Settings.ControllersBuilder.BuildSettingControllers(model.RelatedOptions);
            result.ChildOptionGroups = new ObservableCollection<OptionsGroupController>();

            foreach (OptionsGroup group in model.ChildOptionGroups)
                result.ChildOptionGroups.Add(BuildOptionsGroupController(group));

            return result;
        }

        /// <summary>
        /// Builds the <see cref="root"/> based on the hierarchy of settings
        /// </summary>
        public void Build()
        {
            ICollection<OptionsGroup> groups = root.OptionGroupsModel;
            OptionsGroupController currentOGController = null;

            foreach (OptionsGroup group in groups)
            {
                currentOGController = BuildOptionsGroupController(group);
                
                root.OptionGroups.Add(currentOGController);
            }

            root.OptionGroups.First().IsSelected = true;
        }

        /// <summary>
        /// Gets the result of the building process.
        /// </summary>
        /// <returns>The build <see cref="ProfileManagerController"/></returns>
        public OptionsManagerController GetResult()
        {
            return root;
        }
    }
}

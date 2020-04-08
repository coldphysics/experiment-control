using System.Collections.Generic;
using System.Linq;
using Controller.Settings.Settings;
using Model.Settings;
using Model.Settings.Settings;

namespace Controller.Settings
{
    /// <summary>
    /// The builder of controllers
    /// </summary>
    public class ControllersBuilder
    {
        /// <summary>
        /// The root of the controllers hierarchy to be built
        /// </summary>
        private ProfileManagerController root;


        /// <summary>
        /// Initializes a new instance of the <see cref="ControllersBuilder"/> class.
        /// </summary>
        public ControllersBuilder()
        {
            ProfilesManager profilesManager = ProfilesManager.GetInstance();
            //profilesManager.LoadProfiles();
            root = new ProfileManagerController(profilesManager);
        }

        /// <summary>
        /// Builds collection of <see cref="SettingController"/> based on a collection of <see cref="BasicSetting"/>.
        /// </summary>
        /// <param name="settingsModel">The settings model.</param>
        /// <returns>A collection of <see cref="SettingController"/></returns>
        public static ICollection<SettingController> BuildSettingControllers(ICollection<BasicSetting> settingsModel)
        {
            ICollection<SettingController> result = new List<SettingController>();
            SettingController currentSettingController;

            if (settingsModel != null)
            {
                foreach (BasicSetting setting in settingsModel)
                {
                    if (setting is FileSetting)
                    {
                        currentSettingController = new FileSettingController(setting as FileSetting);
                    }
                    else if (setting is FolderSetting)
                    {
                        currentSettingController = new FolderSettingController(setting as FolderSetting);
                    }
                    else if (setting is StringSetting)
                    {
                        currentSettingController = new StringSettingController(setting as StringSetting);
                    }
                    else if (setting is SampleRateSetting)
                    {
                        //Unavoidable tight-coupling
                        StringMultiOptionSettingController hwTypeSettingController = (StringMultiOptionSettingController)result.Where(param => param.Name == ((SampleRateSetting)setting).HardwareTypeSetting.Name ).FirstOrDefault();
                        currentSettingController = new SampleRateSettingController(setting as SampleRateSetting, hwTypeSettingController);
                    }
                    else if (setting is IntegerSetting)
                    {
                        currentSettingController = new IntegerSettingController(setting as IntegerSetting);
                    }
                    else if (setting is DecimalSetting)
                    {
                        currentSettingController = new DecimalSettingController(setting as DecimalSetting);
                    }
                    else if (setting is BooleanSetting)
                    {
                        currentSettingController = new BooleanSettingController(setting as BooleanSetting);
                    }
                    else if (setting is StringMultiOptionSetting)
                    {
                        currentSettingController = new StringMultiOptionSettingController(setting as StringMultiOptionSetting);
                    }
                    else if (setting is DatabaseConnectionSetting)
                    {
                        currentSettingController = new DatabaseConnectionSettingController(setting as DatabaseConnectionSetting);
                    }
                    else
                    {
                        currentSettingController = new SettingController(setting);
                    }


                    result.Add(currentSettingController);
                }
            }


            return result;
        }

        /// <summary>
        /// Builds the controller for the specified profile.
        /// </summary>
        /// <param name="model">The model of the profile.</param>
        /// <returns>The controller for the specified profile</returns>
        public static ProfileController BuildProfileController(Profile model)
        {
            ProfileController result = new ProfileController(model);
            result.Settings = BuildSettingControllers(model.Settings);

            return result;
        }

        /// <summary>
        /// Builds the <see cref="root"/> based on the hierarchy of settings
        /// </summary>
        public void Build()
        {
            ICollection<Profile> profiles = root.ProfilesModel;
            ProfileController currentProfileController = null;

            foreach (Profile profile in profiles)
            {
                currentProfileController = BuildProfileController(profile);
                
                root.Profiles.Add(currentProfileController);
            }
        }

        /// <summary>
        /// Gets the result of the building process.
        /// </summary>
        /// <returns>The build <see cref="ProfileManagerController"/></returns>
        public ProfileManagerController GetResult()
        {
            return root;
        }
    }

}

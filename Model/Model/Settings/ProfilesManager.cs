using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace Model.Settings
{
    /// <summary>
    /// Manages all profiles
    /// </summary>
    /// <remarks>This class implements the Singleton design pattern</remarks>
    public class ProfilesManager
    {
        private const string PROFILE_SAVE_PATH = @"\Computer Control Profiles\";
        /// <summary>
        /// The singleton instance
        /// </summary>
        private static ProfilesManager instance;
        /// <summary>
        /// The manager of the default profiles
        /// </summary>
        private DefaultProfilesManager defaultProfilesManager = new DefaultProfilesManager();
        /// <summary>
        /// The collection of profiles managed by this instance
        /// </summary>
        private ICollection<Profile> profiles;
        /// <summary>
        /// The currently active profile
        /// </summary>
        private Profile activeProfile = null;

        /// <summary>
        /// Gets the profile saving folder.
        /// </summary>
        /// <value>
        /// The profile saving folder.
        /// </value>
        public static string SaveFolder
        {
            get
            {
                string saveFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + PROFILE_SAVE_PATH;

                return saveFolder;
            }
 
        }
        /// <summary>
        /// Gets or sets the active profile.
        /// </summary>
        /// <value>
        /// The active profile.
        /// </value>
        /// <remarks>This method saves the name of the active profile on the HDD</remarks>
        public Profile ActiveProfile
        {
            get { return activeProfile; }
            set 
            {
                if (value != null)
                {
                    Properties.Settings.Default.ActiveProfileName = value.Name;
                    Properties.Settings.Default.Save();
                }

                activeProfile = value;
            }
        }

        /// <summary>
        /// Gets the collection of profiles managed by this class.
        /// </summary>
        /// <value>
        /// The profiles.
        /// </value>
        public ICollection<Profile> Profiles
        {
            get { return profiles; }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="ProfilesManager"/> class from being created.
        /// </summary>
        private ProfilesManager()
        { }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        /// <returns>The singleton instance</returns>
        public static ProfilesManager GetInstance()
        {
            if (instance == null)
            {
                ProfilesManager temp = new ProfilesManager();

                if (temp.LoadProfiles())//Loading of profiles from disk is successful
                {
                    string activeProfileName = Properties.Settings.Default.ActiveProfileName;

                    foreach (Profile profile in temp.Profiles)
                    {
                        if (profile.Name == activeProfileName)
                        {
                            temp.ActiveProfile = profile;
                            break;
                        }
                    }
                }
                else//Loading from disk was not successful (default profiles were created)
                {
                    Properties.Settings.Default.ActiveProfileName = "";
                    Properties.Settings.Default.Save();
                    temp.ActiveProfile = null;
                }

                instance = temp;//only assign it after successful creation!
            }

            return instance;
        }

        /// <summary>
        /// Creates the set of default profiles.
        /// </summary>
        /// <returns></returns>
        private ICollection<Profile> CreateDefaultProfiles()
        {
            return new ObservableCollection<Profile>(defaultProfilesManager.CreateDefaultProfiles());
        }

        /// <summary>
        /// Creates a new profile with default values.
        /// </summary>
        /// <param name="profileName">Name of the profile.</param>
        /// <returns></returns>
        public Profile CreateNewProfile(string profileName)
        {
            Profile result = defaultProfilesManager.CreateDefaultProfile();
            result.Name = profileName;

            return result;
        }

        /// <summary>
        /// Loads the profiles from the permanent storage. If no profiles are (yet) saved on permanent storage, it loads the default profiles instead.
        /// </summary>
        /// <returns><c>true</c> if profiles where loaded from disk, <c>false</c> if default profiles had to be created.</returns>
        private bool LoadProfiles()
        {

            if (!Directory.Exists(SaveFolder))
            {
                profiles = CreateDefaultProfiles();
                SaveAllProfiles();

                return false;
            }
            else
            {
                profiles = new ObservableCollection<Profile>();
                IEnumerable<string> files = Directory.EnumerateFiles(SaveFolder, "*.profile");
                Profile currentProfile = null;

                foreach (string file in files)
                {
                    currentProfile = LoadProfile(file);

                    if (currentProfile != null)
                        Profiles.Add(currentProfile);

                }

                if (profiles.Count == 0)
                {
                    profiles = CreateDefaultProfiles();
                    SaveAllProfiles();

                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public Profile LoadProfile(string filesPath)
        {
            Profile result = null;
            using (var reader = new FileStream(filesPath, FileMode.Open))
            {
                var deserializer = new DataContractSerializer(typeof(Profile));
                result = (Profile)deserializer.ReadObject(reader);
            }

            return result;
        }
        /// <summary>
        /// Saves all profiles to stable storage.
        /// </summary>
        public void SaveAllProfiles()
        {
            DeleteProfilesFromDisk();

            foreach (Profile profile in Profiles)
                SaveProfile(profile);
        }

        /// <summary>
        /// Deletes the profiles from disk.
        /// </summary>
        private static void DeleteProfilesFromDisk()
        {
            try
            {
                if (Directory.Exists(SaveFolder))
                {
                    DirectoryInfo di = new DirectoryInfo(SaveFolder);
                    di.Delete(true);
                    di.Refresh();
                }
            }
            catch (Exception)
            {
                //This might need to be handled better!
            }
        }
        
        /// <summary>
        /// Saves the profile to stable storage.
        /// </summary>
        /// <param name="profileToSave">The profile to save.</param>
        public void SaveProfile(Profile profileToSave)
        {
            string saveFolder = SaveFolder;

            // save variables
            if (!Directory.Exists(saveFolder))
            {
                Directory.CreateDirectory(saveFolder);
            }

            string filePath = saveFolder + profileToSave.Name + ".profile";
            SaveProfileToPath(profileToSave, filePath);
        }

        /// <summary>
        /// Saves the profile to a specific path.
        /// </summary>
        /// <param name="profile">The profile to save.</param>
        /// <param name="filePath">The file path.</param>
        public void SaveProfileToPath(Profile profile, string filePath)
        {
            // xml data
            using (var writer = new FileStream(filePath, FileMode.Create))
            {
                Type type = profile.GetType();
                var serializer = new DataContractSerializer(type);
                serializer.WriteObject(writer, profile);
            }
        }


        /// <summary>
        /// Creates a copy of the profiles manager
        /// </summary>
        /// <returns>A copy of the profiles manager.</returns>
        private ProfilesManager DeepCopyProfilesManager()
        {
                    
            var stream = new MemoryStream();
            var formatter = new BinaryFormatter { Context = new StreamingContext(StreamingContextStates.Clone) };
            formatter.Serialize(stream, profiles);
            stream.Position = 0;
            ICollection<Profile> copyOfProfiles = (ICollection<Profile>)formatter.Deserialize(stream);

            ProfilesManager copy = new ProfilesManager();
            copy.profiles = copyOfProfiles;

            if (activeProfile != null)
            {
                string name = activeProfile.Name;

                foreach(Profile current in copy.profiles)
                {
                    if (current.Name == name)
                    {
                        copy.activeProfile = current;
                        break;
                    }
                }
            }

            return copy;
        
        }


        /// <summary>
        /// Determines whether a restart is required due to a change in specific settings in the active profile made in the snapshot.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns><c>true</c> if a restart is required.</returns>
        public bool IsRestartReuqired(ProfilesManagerSnapshot snapshot)
        {
            if (activeProfile != null)
            {
                return snapshot.ActiveProfile.IsRestartRequired(activeProfile);
            }

            return false;
        }

        /// <summary>
        /// Copies the profiles that are maintained by a modified snapshot of the profiles manager.
        /// </summary>
        /// <param name="snapshot">The modified snapshot.</param>
        /// <returns><c>true</c> when the changes require restarting the application. <c>false</c> otherwise.</returns>
        public void GetValuesFromSnapshot(ProfilesManagerSnapshot snapshot)
        {
            profiles = snapshot.Profiles;
            ActiveProfile = snapshot.ActiveProfile;
        }

        /// <summary>
        /// Creates a snapshot of the profiles manager.
        /// </summary>
        /// <returns>A snapshot of the profiles manager.</returns>
        public ProfilesManagerSnapshot CreateSnapshot()
        {
            ProfilesManager copy = DeepCopyProfilesManager();
            ProfilesManagerSnapshot snapshot = new ProfilesManagerSnapshot(copy);

            return snapshot;
        }


        /// <summary>
        /// Restores the original profiles.
        /// </summary>
        public static ProfilesManager RestoreOriginalProfiles()
        {
            DeleteProfilesFromDisk();
            instance = GetInstance();//This saves the profiles on disk

            return instance;
        }
    }
}

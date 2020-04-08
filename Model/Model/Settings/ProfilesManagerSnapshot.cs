using System.Collections.Generic;

namespace Model.Settings
{
    /// <summary>
    /// This class is a facade to a snapshot of the profiles manager. It is intended to be used by the profiles manager controller in order
    /// to have a temporal copy of the profiles so that the changes made by the user are not directed to the original profiles but rather to
    /// this copy. The changes are sent back to the original profiles manager when the user saves the changes and the changes are validated against
    /// errors.
    /// </summary>
    public class ProfilesManagerSnapshot
    {
        /// <summary>
        /// A snapshot of the profiles manager
        /// </summary>
        private ProfilesManager snapshot;

        /// <summary>
        /// Gets or sets the active profile.
        /// </summary>
        /// <value>
        /// The active profile.
        /// </value>
        public Profile ActiveProfile
        {
            get 
            { 
                return snapshot.ActiveProfile; 
            }
            set 
            {
                snapshot.ActiveProfile = value;
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
            get { return snapshot.Profiles; }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="ProfilesManager"/> class from being created.
        /// </summary>
        public ProfilesManagerSnapshot(ProfilesManager copiedProfilesManager)
        {
            snapshot = copiedProfilesManager;
        }

        

        /// <summary>
        /// Creates a new profile with default values.
        /// </summary>
        /// <param name="profileName">Name of the profile.</param>
        /// <returns></returns>
        public Profile CreateNewProfile(string profileName)
        {
            return snapshot.CreateNewProfile(profileName);
        }

        
        
    }
}

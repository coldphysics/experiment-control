namespace Model.Settings.Builders
{
    /// <summary>
    /// Builds an instance of <see cref="Profile"/>
    /// </summary>
    /// <seealso cref="Prototyping.Model.Builders.SettingsCollectionBuilder" />
    class ProfileBuilder : SettingsCollectionBuilder
    {
        /// <summary>
        /// The name of the profile
        /// </summary>
        private string name;

        /// <summary>
        /// Sets the name of the profile.
        /// </summary>
        /// <param name="name">The name.</param>
        public void SetProfileName(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Gets the result of the building process.
        /// </summary>
        /// <returns>The built instance.</returns>
        public Profile GetResult()
        {
            Profile result = new Profile(settingsCollection, name);
            return result;
        }
    }
}

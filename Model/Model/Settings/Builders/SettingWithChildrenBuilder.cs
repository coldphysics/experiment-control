using Model.Settings.Settings;
using System.Collections.Generic;

namespace Model.Settings.Builders
{
    /// <summary>
    /// Base class for builders of settings that have child settings
    /// </summary>
    /// <typeparam name="T">The type of values held by the setting</typeparam>
    abstract class SettingWithChildrenBuilder<T>
    {
        /// <summary>
        /// The name of the setting
        /// </summary>
        protected string name;
        /// <summary>
        /// The default value
        /// </summary>
        protected T defaultValue;
        /// <summary>
        /// A collection of <see cref="SettingsCollectionBuilder"/>, one for each value of the current setting to be built
        /// </summary>
        protected Dictionary<T, SettingsCollectionBuilder> childSettingsBuilders;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingWithChildrenBuilder{T}"/> class.
        /// </summary>
        public SettingWithChildrenBuilder()
        {
            childSettingsBuilders = new Dictionary<T, SettingsCollectionBuilder>();
        }

        /// <summary>
        /// Sets the name of the setting.
        /// </summary>
        /// <param name="name">The name.</param>
        public void SetSettingName(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Sets the setting default value.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        public void SetSettingDefaultValue(T defaultValue)
        {
            this.defaultValue = defaultValue;
        }

        /// <summary>
        /// Gets the collection builder that is associated to a specific value for the setting.
        /// </summary>
        /// <param name="key">The value.</param>
        /// <returns>A collection builder that is associated to the specified value</returns>
        private SettingsCollectionBuilder GetCollectionBuilder(T key)
        {
            if (!childSettingsBuilders.ContainsKey(key))
                childSettingsBuilders[key] = new SettingsCollectionBuilder();

            return childSettingsBuilders[key];
        }

        /// <summary>
        /// Adds a child string setting.
        /// </summary>
        /// <param name="key">The key to associate the setting with.</param>
        /// <param name="name">The name of the setting.</param>
        /// <param name="defaultValue">The default value.</param>
        public void AddChildStringSetting(T key, string name, string defaultValue)
        {
            SettingsCollectionBuilder builder = GetCollectionBuilder(key);
            builder.AddStringSetting(name, defaultValue);
        }

        /// <summary>
        /// Adds a child integer setting.
        /// </summary>
        /// <param name="key">The key to associate the setting with.</param>
        /// <param name="name">The name of the setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="minimumValue">The minimum value.</param>
        /// <param name="maximumValue">The maximum value.</param>
        /// <param name="unit">The unit.</param>
        /// <returns>A reference to the added setting</returns>
        public IntegerSetting AddChildIntegerSetting(T key, string name, int defaultValue, int minimumValue, int maximumValue, string unit)
        {
            SettingsCollectionBuilder builder = GetCollectionBuilder(key);
            IntegerSetting setting = builder.AddIntegerSetting(name, defaultValue, minimumValue, maximumValue, unit);

            return setting;
        }

        /// <summary>
        /// Adds a child file setting.
        /// </summary>
        /// <param name="key">The key to associate the setting with.</param>
        /// <param name="name">The name of the setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="fileExtensions">The file extensions.</param>
        /// <returns>A reference to the added setting</returns>
        public FileSetting AddChildFileSetting(T key, string name, string defaultValue, ICollection<string> fileExtensions)
        {
            SettingsCollectionBuilder builder = GetCollectionBuilder(key);
            FileSetting setting = builder.AddFileSetting(name, defaultValue, fileExtensions);

            return setting;
        }

        /// <summary>
        /// Adds a child boolean setting.
        /// </summary>
        /// <param name="key">The key to associate the setting with.</param>
        /// <param name="name">The name of the setting.</param>
        /// <param name="defaultValue">if set to <c>true</c> [default value].</param>
        /// <param name="relatedSettingsDictionary">The related settings dictionary.</param>
        /// <returns>A reference to the added setting</returns>
        public BooleanSetting AddChildBooleanSetting(T key, string name, bool defaultValue, Dictionary<bool, ICollection<BasicSetting>> relatedSettingsDictionary)
        {
            SettingsCollectionBuilder builder = GetCollectionBuilder(key);
            BooleanSetting setting = builder.AddBooleanSetting(name, defaultValue, relatedSettingsDictionary);

            return setting;
        }

        /// <summary>
        /// Adds a child folder setting.
        /// </summary>
        /// <param name="key">The key to associate the setting with.</param>
        /// <param name="name">The name of the setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>A reference to the added setting</returns>
        public FolderSetting AddChildFolderSetting(T key, string name, string defaultValue)
        {
            SettingsCollectionBuilder builder = GetCollectionBuilder(key);
            FolderSetting setting = builder.AddFolderSetting(name, defaultValue);

            return setting;
        }

        /// <summary>
        /// Adds a child string multi option setting.
        /// </summary>
        /// <param name="key">The key to associate the setting with.</param>
        /// <param name="name">The name of the setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="options">The options.</param>
        /// <param name="relatedSettingsDictionary">The related settings dictionary.</param>
        /// <returns>A reference to the added setting</returns>
        public StringMultiOptionSetting AddChildStringMultiOptionSetting(T key, string name, string defaultValue, string[] options, Dictionary<string, ICollection<BasicSetting>> relatedSettingsDictionary)
        {
            SettingsCollectionBuilder builder = GetCollectionBuilder(key);
            StringMultiOptionSetting setting = builder.AddStringMultiOptionSetting(name, defaultValue, options, relatedSettingsDictionary);

            return setting;
        }

        /// <summary>
        /// Adds a child database connection setting.
        /// </summary>
        /// <param name="key">The key to associate the setting with.</param>
        /// <param name="name">The name of the setting.</param>
        /// <param name="defServer">The default server address.</param>
        /// <param name="defDatabase">The default database name.</param>
        /// <param name="defUserName">Name of the default user-name.</param>
        /// <param name="defPassword">The default password.</param>
        /// <param name="defTable">The default table name.</param>
        /// <returns>A reference to the added setting</returns>
        public DatabaseConnectionSetting AddChildDatabaseConnectionSetting(T key, string name, string defServer, int defPort, string defDatabase, string defUserName, string defPassword, string defTable)
        {
            SettingsCollectionBuilder builder = GetCollectionBuilder(key);
            DatabaseConnectionSetting setting = builder.AddDatabaseConnectionSetting(name, defServer, defPort, defDatabase, defUserName, defPassword, defTable);

            return setting;
        }

        /// <summary>
        /// Adds a child setting.
        /// </summary>
        /// <param name="key">The key to associate the setting with.</param>
        /// <param name="name">The name of the setting.</param>
        public void AddChildSetting(T key, BasicSetting setting)
        {
            SettingsCollectionBuilder builder = GetCollectionBuilder(key);
            builder.AddSetting(setting);
        }


        /// <summary>
        /// Gets the dictionary that maps each possible value of the current setting to a collection of child settings.
        /// </summary>
        /// <returns>The dictionary that maps each possible value of the current setting to a collection of child settings.</returns>
        protected Dictionary<T, ICollection<BasicSetting>> GetRelatedSettingsDictionary()
        {
            Dictionary<T, ICollection<BasicSetting>> result = new Dictionary<T, ICollection<BasicSetting>>();

            foreach (T key in childSettingsBuilders.Keys)
            {
                result[key] = childSettingsBuilders[key].GetSettingCollection();
            }

            return result;
        }

    }
}

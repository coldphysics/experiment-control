using Model.Settings.Settings;
using System.Collections.Generic;

namespace Model.Settings.Builders
{
    /// <summary>
    /// Builds a collection of settings
    /// </summary>
    class SettingsCollectionBuilder
    {
        /// <summary>
        /// The settings collection
        /// </summary>
        protected ICollection<BasicSetting> settingsCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsCollectionBuilder"/> class.
        /// </summary>
        public SettingsCollectionBuilder()
        {
            settingsCollection = new List<BasicSetting>();
        }

        /// <summary>
        /// Adds a string setting.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>A reference to the added setting</returns>
        public StringSetting AddStringSetting(string name, string defaultValue)
        {
            StringSetting setting = new StringSetting(name, defaultValue);
            AddSetting(setting);

            return setting;
        }


        /// <summary>
        /// Adds an integer setting.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="minimumValue">The minimum value.</param>
        /// <param name="maximumValue">The maximum value.</param>
        /// <param name="unit">The unit.</param>
        /// <returns>A reference to the added setting</returns>
        public IntegerSetting AddIntegerSetting(string name, int defaultValue, int minimumValue, int maximumValue, string unit)
        {
            IntegerSetting setting = new IntegerSetting(name, defaultValue, minimumValue, maximumValue, unit);
            AddSetting(setting);

            return setting;
        }

        /// <summary>
        /// Adds a decimal setting.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="minimumValue">The minimum value.</param>
        /// <param name="maximumValue">The maximum value.</param>
        /// <param name="unit">The unit.</param>
        /// <returns>A reference to the added setting</returns>
        public DecimalSetting AddDecimalSetting(string name, decimal defaultValue, decimal minimumValue, decimal maximumValue, string unit)
        {
            DecimalSetting setting = new DecimalSetting(name, defaultValue, minimumValue, maximumValue, unit);
            AddSetting(setting);

            return setting;
        }
        /// <summary>
        /// Adds a file setting.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="fileExtensions">The allowed file extensions.</param>
        /// <returns>A reference to the added setting</returns>
        public FileSetting AddFileSetting(string name, string defaultValue, ICollection<string> fileExtensions)
        {
            FileSetting setting = new FileSetting(name, defaultValue, fileExtensions);
            AddSetting(setting);

            return setting;
        }

        /// <summary>
        /// Adds a boolean setting.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="defaultValue">the default value</param>
        /// <param name="relatedSettingsDictionary">The related settings dictionary.</param>
        /// <returns>A reference to the added setting</returns>
        public BooleanSetting AddBooleanSetting(string name, bool defaultValue, Dictionary<bool, ICollection<BasicSetting>> relatedSettingsDictionary)
        {
            BooleanSetting setting = new BooleanSetting(name, defaultValue, relatedSettingsDictionary);
            AddSetting(setting);

            return setting;
        }

        /// <summary>
        /// Adds a folder setting.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>A reference to the added setting</returns>
        public FolderSetting AddFolderSetting(string name, string defaultValue)
        {
            FolderSetting setting = new FolderSetting(name, defaultValue);
            AddSetting(setting);

            return setting;
        }

        /// <summary>
        /// Adds a string multi option setting.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="options">The allowed options for the value.</param>
        /// <param name="relatedSettingsDictionary">The related settings dictionary.</param>
        /// <returns>A reference to the added setting</returns>
        public StringMultiOptionSetting AddStringMultiOptionSetting(string name, string defaultValue, string[] options, Dictionary<string, ICollection<BasicSetting>> relatedSettingsDictionary)
        {
            StringMultiOptionSetting setting = new StringMultiOptionSetting(name, defaultValue, options, relatedSettingsDictionary);
            AddSetting(setting);

            return setting;
        }

        /// <summary>
        /// Adds a database connection setting.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="defServer">The default server address.</param>
        /// <param name="defDatabase">The default database name.</param>
        /// <param name="defUserName">The default user-name.</param>
        /// <param name="defPassword">The default password.</param>
        /// <param name="defTable">The default table name.</param>
        /// <returns>A reference to the added setting</returns>
        public DatabaseConnectionSetting AddDatabaseConnectionSetting(string name, string defServer, int defPort, string defDatabase, string defUserName, string defPassword, string defTable)
        {
            DatabaseConnectionSetting setting = new DatabaseConnectionSetting(name, defServer, defPort, defDatabase, defUserName, defPassword, defTable);
            AddSetting(setting);

            return setting;
        }

        /// <summary>
        /// Adds a sample rate setting.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="minimumValue">The minimum value.</param>
        /// <param name="maximumValue">The maximum value.</param>
        /// <param name="unit">The unit.</param>
        /// <param name="masterSetting">The setting that specifies the hardware type.</param>
        /// <returns>A reference to the added setting</returns>
        public SampleRateSetting AddSampleRateSetting(string name, int defaultValue, int minimumValue, int maximumValue, SampleRateUnit unit, StringMultiOptionSetting masterSetting)
        {
            SampleRateSetting setting = new SampleRateSetting(name, defaultValue, minimumValue, maximumValue, unit, masterSetting);
            AddSetting(setting);

            return setting;
        }

        /// <summary>
        /// Adds a setting.
        /// </summary>
        /// <param name="setting">The setting to add.</param>
        public void AddSetting(BasicSetting setting)
        {
            settingsCollection.Add(setting);
        }

        /// <summary>
        /// Gets the resulting collection of settings
        /// </summary>
        /// <returns>The resulting collection of settings</returns>
        public ICollection<BasicSetting> GetSettingCollection()
        {
            return settingsCollection;
        }

    }
}

using System;
using System.Runtime.Serialization;

namespace Model.Settings.Settings
{
    /// <summary>
    /// The base class for all settings
    /// </summary>
    [Serializable]
    [KnownType(typeof(BooleanSetting))]
    [KnownType(typeof(DatabaseConnectionSetting))]
    [KnownType(typeof(StringSetting))]
    [KnownType(typeof(IntegerSetting))]
    [KnownType(typeof(DecimalSetting))]
    [KnownType(typeof(SampleRateSetting))]
    [KnownType(typeof(StringMultiOptionSetting))]
    [KnownType(typeof(FileSetting))]
    [KnownType(typeof(FolderSetting))]
    public abstract class BasicSetting
    {
        /// <summary>
        /// The name of the setting
        /// </summary>
        public readonly string NAME;


        private bool requiresRestart;

        /// <summary>
        /// Indicates whether changing the value of this setting requires restarting the application
        /// </summary>
        public bool RequiresRestart
        {
            set
            {
                this.requiresRestart = value;
            }
            get
            {
                return requiresRestart;
            }
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSetting"/> class.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        public BasicSetting(string name)
        {
            this.NAME = name;
            this.requiresRestart = false;
        }



        /// <summary>
        /// Restores the default value for the setting.
        /// </summary>
        public abstract void RestoreDefaults();

        /// <summary>
        /// Determines whether the value is different from another setting.
        /// </summary>
        /// <param name="other">The other setting.</param>
        /// <returns><c>true</c> when the value is different between the two settings.</returns>
        public abstract bool HasValueChanged(BasicSetting other);
    }
}

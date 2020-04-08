using System;

namespace Model.Settings.Settings
{
    /// <summary>
    /// A setting that allows specifying a folder
    /// </summary>
    /// <seealso cref="Model.Settings.Settings.StringSetting" />
    [Serializable]
    public class FolderSetting : StringSetting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FolderSetting"/> class.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="defaultValue">The default value of the setting.</param>
        public FolderSetting(string name, string defaultValue)
            : base(name, defaultValue)
        {
        }
    }
}

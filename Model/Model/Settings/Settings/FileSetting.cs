using System;
using System.Collections.Generic;

namespace Model.Settings.Settings
{
    /// <summary>
    /// A setting that allows specifying a file to open
    /// </summary>
    /// <seealso cref="Model.Settings.Settings.StringSetting" />
    [Serializable]
    public class FileSetting : StringSetting
    {
        /// <summary>
        /// The accepted file extensions
        /// </summary>
        private ICollection<string> acceptedFileExtensions;

        /// <summary>
        /// Gets the accepted file extensions.
        /// </summary>
        /// <value>
        /// The accepted file extensions.
        /// </value>
        public ICollection<string> AcceptedFileExtensions
        {
            get { return acceptedFileExtensions; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSetting"/> class.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="acceptedFileExtensions">The accepted file extensions. Each of the extensions should have the format "explanation|extension(s)" e.g, "JPEG files|*.jpg;*.jpeg"</param>
        public FileSetting(string name, string defaultValue, ICollection<string> acceptedFileExtensions)
            : base(name, defaultValue)
        {
            this.acceptedFileExtensions = acceptedFileExtensions;
        }
    }
}

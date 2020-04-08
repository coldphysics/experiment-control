using System;
using System.Text;
using Microsoft.Win32;
using Model.Settings.Settings;

namespace Controller.Settings.Settings
{
    /// <summary>
    /// The controller for a <see cref="FileSetting"/>
    /// </summary>
    /// <seealso cref="Prototyping.Controller.Settings.ChooserSettingController" />
    public class FileSettingController : ChooserSettingController
    {
        /// <summary>
        /// Gets the setting.
        /// </summary>
        /// <value>
        /// The setting.
        /// </value>
        private FileSetting Setting
        {
            get
            {
                return (FileSetting)setting;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSettingController"/> class.
        /// </summary>
        /// <param name="setting">The setting.</param>
        public FileSettingController(FileSetting setting)
            : base(setting)
        { }


        /// <summary>
        /// The action to be performed to choose a value
        /// </summary>
        /// <param name="obj">A parameter to pass to the action</param>
        protected override void OpenCommandAction(object obj)
        {
            StringBuilder filterBuilder = new StringBuilder();
            int counter = 0;

            foreach (string fileType in Setting.AcceptedFileExtensions)
            {
                filterBuilder.Append(fileType);

                if (counter < Setting.AcceptedFileExtensions.Count - 1)
                    filterBuilder.Append("|");

                counter++;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = filterBuilder.ToString();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (openFileDialog.ShowDialog() == true)
                Value = openFileDialog.FileName;
        }
    }
}

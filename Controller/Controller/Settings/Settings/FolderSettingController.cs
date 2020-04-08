using Microsoft.WindowsAPICodePack.Dialogs;
using Model.Settings.Settings;

namespace Controller.Settings.Settings
{
    /// <summary>
    /// The controller for a <see cref="FolderSetting"/>
    /// </summary>
    /// <seealso cref="Prototyping.Controller.Settings.ChooserSettingController" />
    public class FolderSettingController : ChooserSettingController
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderSettingController"/> class.
        /// </summary>
        /// <param name="setting">The setting.</param>
        public FolderSettingController(FolderSetting setting)
            : base(setting)
        { }

        /// <summary>
        /// The action to be performed to choose a value
        /// </summary>
        /// <param name="obj">A parameter to pass to the action</param>
        protected override void OpenCommandAction(object obj)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.EnsurePathExists = false;
            CommonFileDialogResult result = dialog.ShowDialog();


            if (result == CommonFileDialogResult.Ok)
                Value = dialog.FileName;
        }

    }
}

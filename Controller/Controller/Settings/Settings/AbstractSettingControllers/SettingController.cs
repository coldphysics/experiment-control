using System.ComponentModel;
using Model.Settings.Settings;

namespace Controller.Settings.Settings
{
    /// <summary>
    /// A base class for all setting controllers
    /// </summary>
    /// <seealso cref="Prototyping.Controller.BaseController" />
    public class SettingController:BaseController,IDataErrorInfo
    {
        /// <summary>
        /// The model associated with this controller
        /// </summary>
        protected BasicSetting setting;

        /// <summary>
        /// Gets the name of the setting.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get
            {
                return setting.NAME;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingController"/> class.
        /// </summary>
        /// <param name="setting">The setting.</param>
        public SettingController(BasicSetting setting)
        {
            this.setting = setting;
        }


        /// <summary>
        /// Restores the default values for this setting.
        /// </summary>
        public virtual void RestoreDefaults()
        {
            setting.RestoreDefaults();
            OnPropertyChanged("Value");
        }



        #region IDataErrorInfo Members

        /// <summary>
        /// Returns true if the value of the setting is valid.
        /// </summary>
        /// <returns><c>true</c> when the setting is valid, otherwise <c>false</c>.</returns>
        public virtual bool IsValid()
        {
            return true;
        }

        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        public virtual string Error
        {
            get
            {
                return "";
            }
        }

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public virtual string this[string columnName]
        {
            get
            {
                return "";
            }
        }

        #endregion
    }
}

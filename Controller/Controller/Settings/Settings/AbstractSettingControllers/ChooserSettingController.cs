using System;
using Model.Settings.Settings;
using Communication.Commands;

namespace Controller.Settings.Settings
{
    /// <summary>
    /// A base class for controllers that allow advanced choosing of the value (through dialogs)
    /// </summary>
    /// <seealso cref="Prototyping.Controller.Settings.SettingController" />
    public abstract class ChooserSettingController : SettingController
    {


        /// <summary>
        /// Gets the command that will be executed to choose the value.
        /// </summary>
        /// <value>
        /// The open command.
        /// </value>
        public RelayCommand OpenCommand { get; private set; }
        /// <summary>
        /// Gets the command that will be executed to clear the chosen value.
        /// </summary>
        /// <value>
        /// The clear command.
        /// </value>
        public RelayCommand ClearCommand { get; private set; }

        /// <summary>
        /// Gets or sets the chosen value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value
        {
            set
            {
                ((StringSetting)setting).Value = value;
                OnPropertyChanged("Value");
                OnPropertyChanged("HasValue");
            }
            get
            {
                return ((StringSetting)setting).Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has value.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has value; otherwise, <c>false</c>.
        /// </value>
        public bool HasValue
        {
            get
            {
                return !(String.IsNullOrEmpty(Value));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChooserSettingController"/> class.
        /// </summary>
        /// <param name="setting">The setting.</param>
        public ChooserSettingController(StringSetting setting)
            : base(setting)
        {
            OpenCommand = new RelayCommand(OpenCommandAction);
            ClearCommand = new RelayCommand(param => Value = "");
        }

        /// <summary>
        /// The action to be performed to choose a value
        /// </summary>
        /// <param name="obj">A parameter to pass to the action</param>
        protected abstract void OpenCommandAction(object obj);


        /// <summary>
        /// Restores the defaults.
        /// </summary>
        public override void RestoreDefaults()
        {
            base.RestoreDefaults();
            OnPropertyChanged("HasValue");
        }

        /// <summary>
        /// Returns true if the value of the setting is valid.
        /// </summary>
        /// <returns><c>true</c> when the setting is valid, otherwise <c>false</c>.</returns>
        public override bool IsValid()
        {
            return this["Value"] == "";
        }

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public override string this[string columnName]
        {
            get
            {
                string result = "";

                switch (columnName)
                {
                    case "Value":
                        if (String.IsNullOrEmpty(Value) && !((StringSetting)setting).CanValueBeEmpty)
                            result = "The Value cannot be empty";
                        break;
                }

                return result;
            }
        }

    }


}

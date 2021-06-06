using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Communication.Commands;
using Model.Settings.Settings;
using MySql.Data.MySqlClient;

namespace Controller.Settings.Settings
{
    /// <summary>
    /// A controller for a <see cref="DatabaseConnectionSetting"/>
    /// </summary>
    /// <seealso cref="Prototyping.Controller.Settings.SettingController" />
    public class DatabaseConnectionSettingController : SettingController
    {
        /// <summary>
        /// The verify connection command
        /// </summary>
        private RelayCommand verifyConnectionCommand;

        private bool isVerifyingConnection;

        private string portText;

        /// <summary>
        /// Gets the setting.
        /// </summary>
        /// <value>
        /// The setting.
        /// </value>
        DatabaseConnectionSetting Setting
        {
            get
            {
                return (DatabaseConnectionSetting)setting;
            }
        }
  

        /// <summary>
        /// Gets the verify connection command.
        /// </summary>
        /// <value>
        /// The verify connection command.
        /// </value>
        public RelayCommand VerifyConnectionCommand
        {
            get 
            {
                if (verifyConnectionCommand == null)
                    verifyConnectionCommand = new RelayCommand(PerformConnectionValidationCommand, param => IsValid());

                return verifyConnectionCommand; 
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is verifyin connection.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is verifying connection; otherwise, <c>false</c>.
        /// </value>
        public bool IsVerifyingConnection
        {
            set
            {
                isVerifyingConnection = value;
                OnPropertyChanged("IsVerifyingConnection");
            }
            get
            {
                return isVerifyingConnection;
            }
        }

        /// <summary>
        /// Sets the table name.
        /// </summary>
        /// <value>
        /// The table name.
        /// </value>
        public string Table
        {
            set
            {
                Setting.Table = value;
                OnPropertyChanged("Table");
            }

            get
            {
                return Setting.Table;
            }
        }

        /// <summary>
        /// Gets or sets the server address.
        /// </summary>
        /// <value>
        /// The server address.
        /// </value>
        public string Server
        {
            set
            {
                Setting.Server = value;
                OnPropertyChanged("Server");
            }

            get
            {
                return Setting.Server;
            }
        }

        /// <summary>
        /// Gets or sets the database name.
        /// </summary>
        /// <value>
        /// The database name.
        /// </value>
        public string Database
        {
            set
            {
                Setting.Database = value;
                OnPropertyChanged("Database");
            }

            get
            {
                return Setting.Database;
            }
        }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>
        /// The username.
        /// </value>
        public string Username
        {
            set
            {
                Setting.UserName = value;
                OnPropertyChanged("Username");
            }

            get
            {
                return Setting.UserName;
            }
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password
        {
            set
            {
                Setting.Password = value;
                OnPropertyChanged("Password");
            }

            get
            {
                return Setting.Password;
            }
        }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>
        /// The port.
        /// </value>
        public int Port
        {
            set
            {
                Setting.Port = value;
                OnPropertyChanged("Port");
                OnPropertyChanged("PortText");
            }

            get
            {
                return Setting.Port;
            }
        }

        /// <summary>
        /// Gets or sets the port text.
        /// </summary>
        /// <value>
        /// The port text.
        /// </value>
        public string PortText {
            set
            {
                this.portText = value;
            }
            get
            {
                return portText;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConnectionSettingController"/> class.
        /// </summary>
        /// <param name="setting">The setting.</param>
        public DatabaseConnectionSettingController(DatabaseConnectionSetting setting)
            : base(setting)
        {
            PortText = setting.Port.ToString();
        }

        /// <summary>
        /// Restores the default values for this setting.
        /// </summary>
        public override void RestoreDefaults()
        {
            base.RestoreDefaults();
            OnPropertyChanged("Password");
            OnPropertyChanged("Username");
            OnPropertyChanged("Database");
            OnPropertyChanged("Server");
            OnPropertyChanged("Table");
            OnPropertyChanged("Port");
        }

              
        /// <summary>
        /// Performs the connection validation command.
        /// </summary>
        /// <param name="param">The parameter.</param>
        public void PerformConnectionValidationCommand(object param)
        {
            BackgroundWorker worker = new BackgroundWorker();
            //this is where the long running process should go
            worker.DoWork += (o, ea) =>
            {
                bool valid;
                string message = ValidateDatabaseConnection(out valid);
                ea.Result = new List<object> { valid, message };
            };
            worker.RunWorkerCompleted += (o, ea) =>
            {
                bool isValid = (bool)((List<object>)ea.Result)[0];
                string result = (string)((List<object>)ea.Result)[1];
                //work has completed. you can now interact with the UI
                IsVerifyingConnection = false;

                if (isValid)
                {
                    MessageBox.Show("The connection with the database is valid!", "Valid Connection", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(result, "Invalid Connection!", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            };
            //set the IsBusy before you start the thread
            IsVerifyingConnection = true;
            worker.RunWorkerAsync();
        }

        //RECO better, create a data access layer that both the output handler and this method can access.
        //RECO better check the entire schema of the table  
        /// <summary>
        /// Validates the database connection.
        /// </summary>
        /// <param name="isValid">if set to <c>true</c> the connection is valid.</param>
        /// <returns>A string that represents the potential error message.</returns>
        /// <remarks>This method also checks for the existence of the globalCounter column in the specified table.</remarks>
        public string ValidateDatabaseConnection(out bool isValid)
        {
            string server = Server;
            string database = Database;
            string uid = Username;
            string password = Password;
            string port = Port.ToString();

            string connectionString = "SERVER=" + server +  ";PORT=" + Port + ";DATABASE=" +
                                      database + ";UID=" + uid + ";PASSWORD=" + password + ";SslMode=none;";
            string table = Table;
            string query = "select globalCounter FROM " + table + "  LIMIT 1;";

            
            string result = "The database connection is valid!";
            isValid = true;

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.ExecuteScalar();
                }
            }
            catch (Exception e)
            {
                result = string.Format("The database connection is invalid: {0}", e.Message);
                isValid = false;
            }
            
            return result;
        }

        /// <summary>
        /// Always returns true
        /// </summary>
        /// <returns>
        ///   <c>true</c> when the setting is valid, otherwise <c>false</c>.
        /// </returns>
        /// <remarks>This method always returns <c>true</c>. To validate the database connection, please use the <see cref=" ValidateDatabaseConnection"/> method explicitly.</remarks>
        public override bool IsValid()
        {
            string[] properties = { "Server", "Table", "Database", "Username", "Password", "Port"};

            foreach (string name in properties)
                if (this[name] != "")
                    return false;

            return true;
        }

        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        /// <remarks>Always returns an empty string</remarks>
        public override string Error
        {
            get { return ""; }
        }

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>Always returns an empty string</returns>
        public override string this[string columnName]
        {
            get 
            {
                string error = "";
                bool hasError = false;
                switch (columnName)
                {
                    case "Server":
                        if (String.IsNullOrEmpty(Server))
                            hasError = true;
                        break;

                    case "Table":
                        if (String.IsNullOrEmpty(Table))
                            hasError = true;
                        break;

                    case "Database":
                        if (String.IsNullOrEmpty(Database))
                            hasError = true;
                        break;

                    case "Username":
                        if (String.IsNullOrEmpty(Username))
                            hasError = true;
                        break;

                    case "Password":
                        if (String.IsNullOrEmpty(Password))
                            hasError = true;
                        break;
                    case "Port":
                        int dummy;
                        if(!Int32.TryParse(PortText, out dummy))
                            hasError = true;
                        break;
                }

                if (hasError)
                    error = string.Format("The Value for the {0} field is invalid.", columnName);

                return error;
            }
        }
    }
}

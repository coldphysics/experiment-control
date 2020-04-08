using System;

namespace Model.Settings.Settings
{
    /// <summary>
    /// A setting that allows the specification of all information necessary to connect to a specific table in a specific database.
    /// </summary>
    /// <seealso cref="Model.Settings.Settings.BasicSetting" />
    [Serializable]
    public class DatabaseConnectionSetting:BasicSetting
    {
        /// <summary>
        /// The default value for the server address
        /// </summary>
        private readonly string DEFAULT_SERVER;
        /// <summary>
        /// The default value for the database name
        /// </summary>
        private readonly string DEFAULT_DATABASE;
        /// <summary>
        /// The default value for the user-name
        /// </summary>
        private readonly string DEFAULT_USERNAME;
        /// <summary>
        /// The default value for the password
        /// </summary>
        private readonly string DEFAULT_PASSWORD;
        /// <summary>
        /// The default value for the table name
        /// </summary>
        private readonly string DEFAULT_TABLE;
        
        /// <summary>
        /// The default value for the port
        /// </summary>
        private readonly int DEFAULT_PORT;

        /// <summary>
        /// The server address
        /// </summary>
        private string server;
        /// <summary>
        /// The database name
        /// </summary>
        private string database;
        /// <summary>
        /// The user-name
        /// </summary>
        private string userName;
        /// <summary>
        /// The password
        /// </summary>
        private string password;
        /// <summary>
        /// The table name
        /// </summary>
        private string table;

        /// <summary>
        /// The port
        /// </summary>
        private int port;

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>
        /// The port.
        /// </value>
        public int Port
        {
            get { return port; }
            set { port = value; }
        }

        /// <summary>
        /// Gets or sets the table name.
        /// </summary>
        /// <value>
        /// The table name.
        /// </value>
        public string Table
        {
            get { return table; }
            set { table = value; }
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        /// <summary>
        /// Gets or sets the user-name.
        /// </summary>
        /// <value>
        /// The user-name.
        /// </value>
        public string UserName
        {
            get { return userName; }
            set { userName = value; }
        }

        /// <summary>
        /// Gets or sets the database name.
        /// </summary>
        /// <value>
        /// The database name.
        /// </value>
        public string Database
        {
            get { return database; }
            set { database = value; }
        }

        /// <summary>
        /// Gets or sets the server address.
        /// </summary>
        /// <value>
        /// The server address.
        /// </value>
        public string Server
        {
            get { return server; }
            set { server = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConnectionSetting"/> class.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="defServer">The default server address.</param>
        /// <param name="defDatabase">The default database name.</param>
        /// <param name="defUserName">The default user-name.</param>
        /// <param name="defPassword">The default password.</param>
        /// <param name="defTable">The default table name.</param>
        public DatabaseConnectionSetting(string name, string defServer, int defPort, string defDatabase, string defUserName, string defPassword, string defTable)
            :base(name)
        {
            this.DEFAULT_PORT = defPort;
            this.DEFAULT_TABLE = table;
            this.DEFAULT_SERVER = defServer;
            this.DEFAULT_DATABASE = defDatabase;
            this.DEFAULT_USERNAME = defUserName;
            this.DEFAULT_PASSWORD = defPassword;
            RestoreDefaults();
        }

        /// <summary>
        /// Restores the default values for the setting.
        /// </summary>
        public override void RestoreDefaults()
        {
            Table = DEFAULT_TABLE;
            Server = DEFAULT_SERVER;
            Database = DEFAULT_DATABASE;
            UserName = DEFAULT_USERNAME;
            Password = DEFAULT_PASSWORD;
            Port = DEFAULT_PORT;
        }

        /// <summary>
        /// Determines whether the value is different from another setting.
        /// </summary>
        /// <param name="other">The other setting.</param>
        /// <returns>
        ///   <c>true</c> when the value is different between the two settings.
        /// </returns>
        public override bool HasValueChanged(BasicSetting other)
        {
            DatabaseConnectionSetting otherSetting = (DatabaseConnectionSetting)other;

            if (otherSetting.Port != Port)
                return true;
            if (otherSetting.Database != Database)
                return true;
            if (otherSetting.Server != Server)
                return true;
            if (otherSetting.Table != Table)
                return true;
            if (otherSetting.UserName != UserName)
                return true;
            if (otherSetting.Password != Password)
                return true;

            return false;
        }
    }
}

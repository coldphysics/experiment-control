using Errors.Error;
using Model.MeasurementRoutine.GlobalVariables;
using Model.Root;
using Model.Settings;
using Model.Settings.Settings;
using Model.Variables;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Buffer.Basic.OutputHandler;

namespace Buffer.DatabaseAccess
{
    class DatabaseManager
    {
        private const char KEY_VAL_SEP = '\t';

        //  *********************************** database **********************************************************        
        /// <summary>
        /// Opens a connection with the MySql database.
        /// </summary>
        /// <param name="setting">The setting that holds the connection parameters.</param>
        /// <returns>An open connection to the database.</returns>
        private MySqlConnection ConnectDatabase(DatabaseConnectionSetting setting)//TODO probably should use the "using" model of connection; safer!
        {
            string server = setting.Server;
            string database = setting.Database;
            string uid = setting.UserName;
            string password = setting.Password;
            string port = setting.Port.ToString();

            string connectionString = "SERVER=" + server + ";PORT=" + port + ";DATABASE=" +
                                     database + ";UID=" + uid + ";PASSWORD=" + password + ";SslMode=none;";
            MySqlConnection connection = null;

            try
            {
                connection = new MySqlConnection(connectionString);
                connection.Open();
            }
            catch (Exception e)
            {
                var errorCollector = ErrorCollector.Instance;
                errorCollector.AddError("Could not connect to SQL database!" + e, ErrorCategory.Basic, true,
                    ErrorTypes.FileNameEmpty);
            }

            return connection;
        }

        /// <summary>
        /// Saves the variables and the scan progress/settings to the database
        /// </summary>
        /// <param name="model">the model</param>
        public void CreateDatabaseEntry(EntryPOCO entry)
        {
            List<string> variablesList = new List<string>();
            List<string> iteratorList = new List<string>();

            // create list 
            foreach (VariableModel variable in entry.VariableList)
            {
                if (variable.VariableName.Trim() != "")
                {
                    variablesList.Add($"{variable.VariableName}{KEY_VAL_SEP}{variable.VariableValue.ToString(CultureInfo.InvariantCulture)}");

                    if (variable.IsIterator())
                    {
                        iteratorList.Add($"{variable.VariableName}{KEY_VAL_SEP}{variable.VariableValue.ToString(CultureInfo.InvariantCulture)}{KEY_VAL_SEP}{variable.VariableStartValue.ToString(CultureInfo.InvariantCulture)}{KEY_VAL_SEP}{variable.VariableEndValue.ToString(CultureInfo.InvariantCulture)}{KEY_VAL_SEP}{variable.VariableStepValue.ToString(CultureInfo.InvariantCulture)}");
                    }
                }
            }


            // add additional variables
            variablesList.Add($"cntGlobal{KEY_VAL_SEP}{entry.GlobalCounter}");
            variablesList.Add($"StartCounterOfScans{KEY_VAL_SEP}{entry.StartCounterOfScansOfCurrentModel}");
            variablesList.Add($"StartCounterOfRoutine{KEY_VAL_SEP}{entry.StartGlobalCounterOfMeasurementRoutine}");

            // variablesList.Add("StartCounterOfScans" + "\t" + StartCounterOfScans);
            variablesList.Add($"IterationOfScan{KEY_VAL_SEP}{entry.IterationOfScan}");
            variablesList.Add($"CompletedScans{KEY_VAL_SEP}{entry.CompletedScans}");
            variablesList.Add($"NumberOfIterations{KEY_VAL_SEP}{entry.NumberOfIterations}");
            variablesList.Add($"CycleDuration{KEY_VAL_SEP}{entry.CycleDuration.ToString(CultureInfo.InvariantCulture)}");

            //ADDED Ghareeb 05.10.2016 do not try any access to DB if not allowed
            if (Global.CanAccessDatabase())
            {
                DatabaseConnectionSetting setting = (DatabaseConnectionSetting)ProfilesManager.GetInstance().ActiveProfile.GetSettingByName(SettingNames.DATABASE_CONNECTION);
                //save to sql database
                string sqlVariables = string.Join("\n", variablesList);
                string sqlIterators = string.Join("\n", iteratorList);

                using (MySqlConnection connection = ConnectDatabase(setting))
                {
                    string table = setting.Table;
                    DateTime estimatedStartTime = entry.EstimatedStartTime;
                    string query = "";

                    if (!ProfilesManager.GetInstance().ActiveProfile.GetSettingValueByName<bool>(SettingNames.USE_LEGACY_DATABASE))//We need to use new schema
                    {
                        OperatingMode currentMode;

                        if (entry.IsMeasurementRoutineMode)
                        {
                            currentMode = OperatingMode.MEASUREMENT_ROUTINE;
                        }
                        else if (entry.IsIterating)
                        {
                            currentMode = OperatingMode.ITERATION;
                        }
                        else
                        {
                            currentMode = OperatingMode.STATIC;
                        }

                        int modelNumber = entry.ModelIndex;

                        StringBuilder routineArrayBuilder = new StringBuilder();
                        List<double> array = GlobalVariablesManager.GetInstance().GetVariableByName<List<double>>(GlobalVariableNames.ROUTINE_ARRAY).VariableValue;

                        for (int i = 0; i < array.Count; i++)
                        {
                            routineArrayBuilder.Append(array[i]);

                            if (i < array.Count - 1)
                                routineArrayBuilder.Append(", ");
                        }

                        string routineArray = routineArrayBuilder.ToString();


                        query = string.Format(
                            "replace into {0} " +
                            "(globalCounter, startTime, startCounterOfScans, iterationOfScan, completedScans, numberOfIterations, " +
                            "Variables, iterators, operatingMode, startCounterOfRoutine, modelNumber, routineArray)" +
                            "VALUES ('{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}')",
                            table, entry.GlobalCounter, estimatedStartTime.ToString("yyyy-MM-dd HH:mm:ss"), entry.StartCounterOfScansOfCurrentModel, entry.IterationOfScan,
                            entry.CompletedScans, entry.NumberOfIterations, sqlVariables, sqlIterators, currentMode.ToString(), entry.StartGlobalCounterOfMeasurementRoutine, modelNumber, routineArray
                            );

                    }
                    else // this should be removed when all experiments start using the new schema.
                    {
                        query = "replace into " + table +
                                       " (globalCounter, startTime, startCounterOfScans, iterationOfScan, completedScans, numberOfIterations, Variables, iterators) VALUES('" +
                                       entry.GlobalCounter + "', '" + estimatedStartTime.ToString("yyyy-MM-dd HH:mm:ss") + "', '" +
                                       entry.StartCounterOfScansOfCurrentModel + "', '" +
                                       entry.IterationOfScan + "', '" + entry.CompletedScans + "', '" + entry.NumberOfIterations + "', '" + sqlVariables + "', '" + sqlIterators + "')";
                    }
                    try
                    {
                        using (var cmd = new MySqlCommand(query, connection))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception e)
                    {
                        var errorCollector = ErrorCollector.Instance;
                        errorCollector.AddError("Could not save to SQL database!" + e, ErrorCategory.Basic, true,
                            ErrorTypes.FileNameEmpty);
                        Console.WriteLine(query);
                        Console.WriteLine(e.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Queries the database for the last global counter if access to the database is allowed, otherwise returns 1
        /// </summary>
        /// <returns>last global counter if access to DB is allowed, 0 otherwise.</returns>
        public int LoadGlobalCounter()
        {
            if (!Global.CanAccessDatabase())
                return 0;

            int globalCounter = -1;
            DatabaseConnectionSetting setting = (DatabaseConnectionSetting)ProfilesManager.GetInstance().ActiveProfile.GetSettingByName(SettingNames.DATABASE_CONNECTION);
            MySqlConnection connection = null;


            using (connection = ConnectDatabase(setting))
            {
                string table = setting.Table;
                string query = "select globalCounter FROM " + table + " ORDER BY globalCounter DESC LIMIT 0,1";

                try
                {
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        globalCounter = int.Parse(cmd.ExecuteScalar() + "");
                    }
                }
                catch (Exception e)
                {
                    ErrorCollector errorCollector = ErrorCollector.Instance;
                    errorCollector.AddError("Could not load the global counter out of the SQL database!", ErrorCategory.Basic,
                        true, ErrorTypes.FileNameEmpty);
                    Console.WriteLine(query);
                    Console.WriteLine(e.ToString());
                }
            }

            return globalCounter;
        }
    }
}

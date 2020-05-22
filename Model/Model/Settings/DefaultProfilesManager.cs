using Model.Settings.Builders;
using Model.Settings.Settings;
using System;
using System.Collections.Generic;

namespace Model.Settings
{
    /// <summary>
    /// The types of hardware
    /// </summary>
    public enum HW_TYPES
    {
        /// <summary>
        /// AdWin with the T11 processor
        /// </summary>
        AdWin_T11,
        /// <summary>
        /// AdWin with the T12 processor
        /// </summary>
        AdWin_T12,
        /// <summary>
        /// AdWin simulator
        /// </summary>
        AdWin_Simulator,
        /// <summary>
        /// National Instruments that operates on PCI slots (one card per slot)
        /// </summary>
        NI_PCI,
        /// <summary>
        /// National Instruments chassis
        /// </summary>
        NI_CHASSIS,
        /// <summary>
        /// Dummy hardware that only simulates the cycle duration
        /// </summary>
        NO_OUTPUT
    }

    /// <summary>
    /// The units of the sample rate
    /// </summary>
    public enum SampleRateUnit
    {
        Hz = 0,
        kHz,
        MHz,
        GHz
    }

    /// <summary>
    /// The units of time
    /// </summary>
    public enum TimeUnit
    {
        s = 0,
        ms,
        us,
        ns
    }

    /// <summary>
    /// All setting names
    /// </summary>
    public class SettingNames
    {
        // "r" means that this setting requires a restart of the application

        public const string NUM_DIGITAL_CARDS = "Number of Digital Cards";//r
        public const string NUM_ANALOG_CARDS = "Number of Analog Cards";//r
        public const string COMPRESSION = "Apply Compression";
        public const string NUM_DIGITAL_CHANNELS_PER_CARD = "Number of Channels per Digital Card";//r
        public const string NUM_ANALOG_CHANNELS_PER_CARD = "Number of Channels per Analog Card";//r
        public const string HW_TYPE = "Hardware Type";//r
        public const string SAVE_VARIABLES_TO_TXT = "Automatically Save the Variable Values to a *.txt file";
        
        public const string SAVE_FIRST_MODEL_TO_XML = "Save only the first model file (.xml) of iterating sequences";
        public const string SAVE_MODEL_TO_XML = "Automatically Save Changed Models to a File";
        public const string SAVE_MODEL_FOLDER_PATH = "Root Folder for Automatic Model Saving";
        public const string TIME_CRITICAL_PYTHON_CYCLE = "Every Cycle Time-Critical Python File Path";
        public const string NON_TIME_CRITICAL_PYTHON_CYCLE = "Every Cycle Non-Time-Critical Python Path";
        public const string TIME_CRITICAL_PYTHON_START = "Start of Scan Time-Critical Python File Path";
        public const string NON_TIME_CRITICAL_PYTHON_START = "Start of Scan Non-Time-Critical Python File Path";
        //public const string EXPERIMENT_NAME = "Experiment Name";
        public const string ALLOW_ACCESS_DATABASE = "Save to Database / Read GC from the Database";//r
        public const string USE_LEGACY_DATABASE = "Use Legacy Database Structure";//r
        public const string DATABASE_CONNECTION = "Database Connection";//r
        //public const string SAMPLE_RATE_UNIT = "Sample Rate Unit";
        public const string SAMPLE_RATE = "Sample Rate";
        //public const string TIME_UNIT = "Time Unit";
        //public const string TIME_BASE_MULTIPLICATOR = "Time Base Multiplicator";
        public const string ADBASIC_CODE_PATH = "Compiled AdBasic Code File Path";
        public const string CONSTANT_WAIT_TIME = "Wait Time Between Cycles";
        public const string PYTHON_INTERPRETER = "Path of Python Interpreter";
        public const string LECROY_VB_SCRIPT = "Control Lecroy VB Script Path";
       
       
    }

    /// <summary>
    /// Creates and manages the default profiles
    /// </summary>
    class DefaultProfilesManager
    {
        /// <summary>
        /// The names of all default profiles
        /// </summary>
        private string[] default_profile_names = 
        {
            "SuperAtoms5thFloor",
            "ColdRydberg4thFloor",
            "RQO5thFloor",
            "Dy4thFloor",
            "AdWinSimulation",
            "NoOutput",
            "Default"
        };

        /// <summary>
        /// Creates the set of default profiles.
        /// </summary>
        /// <returns></returns>
        public Profile[] CreateDefaultProfiles()
        {
            Profile[] result = 
            { 
                CreateColdRydberg4thFloorProfile(),
                CreateRQO5thFloorProfile(),
                CreateSuperAtoms5thFloorProfile(),
                CreateDy4thFloorProfile(),
                CreateSimulatorProfile(),
                CreateNoOutputProfile()
            };

            return result;
        }


        /// <summary>
        /// Creates profile with default values
        /// </summary>
        /// <returns>A new profile with default values</returns>
        public Profile CreateDefaultProfile()
        {
            ProfileBuilder builder = new ProfileBuilder();
            builder.SetProfileName(default_profile_names[6]);

            //builder.AddStringSetting(SettingNames.EXPERIMENT_NAME, "");

            //Building of HW Type setting and its child
            StringMultiOptionSettingBuilder hwTypeBuilder = new StringMultiOptionSettingBuilder();
            hwTypeBuilder.SetSettingName(SettingNames.HW_TYPE);
            hwTypeBuilder.SetSettingDefaultValue(HW_TYPES.AdWin_T12.ToString());
            hwTypeBuilder.SetOptions(Enum.GetNames(typeof(HW_TYPES)));
            FileSetting adBasicPath = hwTypeBuilder.AddChildFileSetting(HW_TYPES.AdWin_T12.ToString(),
                SettingNames.ADBASIC_CODE_PATH, "", new List<string> { "Compiled AdBasic (*.TC1)|*.TC1|Compiled AdBasic (*.TB1)|*.TB1" });
            adBasicPath.CanValueBeEmpty = false;//This value has to be provided
            hwTypeBuilder.AddChildSetting(HW_TYPES.AdWin_T11.ToString(), adBasicPath);
            BasicSetting allowCompression = hwTypeBuilder.AddChildBooleanSetting(HW_TYPES.AdWin_T12.ToString(),
                SettingNames.COMPRESSION, true, null);
            hwTypeBuilder.AddChildSetting(HW_TYPES.AdWin_T11.ToString(), allowCompression);
            hwTypeBuilder.AddChildSetting(HW_TYPES.AdWin_Simulator.ToString(), allowCompression);
            StringMultiOptionSetting hwTypeSetting = hwTypeBuilder.GetResult();
            hwTypeSetting.RequiresRestart = true;
            builder.AddSetting(hwTypeSetting);


            IntegerSetting numSetting = builder.AddIntegerSetting(SettingNames.NUM_ANALOG_CARDS, 4, 0, Int32.MaxValue, "Cards");
            numSetting.RequiresRestart = true;
            numSetting = builder.AddIntegerSetting(SettingNames.NUM_ANALOG_CHANNELS_PER_CARD, 8, 1, Int32.MaxValue, "Channels");
            numSetting.RequiresRestart = true;
            numSetting = builder.AddIntegerSetting(SettingNames.NUM_DIGITAL_CARDS, 2, 0, Int32.MaxValue, "Cards");
            numSetting.RequiresRestart = true;
            numSetting = builder.AddIntegerSetting(SettingNames.NUM_DIGITAL_CHANNELS_PER_CARD, 32, 1, Int32.MaxValue, "Channels");
            numSetting.RequiresRestart = true;

            builder.AddSampleRateSetting(SettingNames.SAMPLE_RATE, 50, 1, Int32.MaxValue, SampleRateUnit.kHz, hwTypeSetting);
            //builder.AddStringMultiOptionSetting(SettingNames.SAMPLE_RATE_UNIT, SampleRateUnit.kHz.ToString(), Enum.GetNames(typeof(SampleRateUnit)), null);
            //builder.AddIntegerSetting(SettingNames.TIME_BASE_MULTIPLICATOR, 1, 1, Int32.MaxValue, "Time Unit");
            //builder.AddStringMultiOptionSetting(SettingNames.TIME_UNIT, TimeUnit.ms.ToString(), Enum.GetNames(typeof(TimeUnit)), null);

            builder.AddDecimalSetting(SettingNames.CONSTANT_WAIT_TIME, 2, 0, Decimal.MaxValue, "Seconds");

            //Building of save_to_xml setting and its child
            BooleanSettingBuilder saveModelBuilder = new BooleanSettingBuilder();
            saveModelBuilder.SetSettingName(SettingNames.SAVE_MODEL_TO_XML);
            saveModelBuilder.SetSettingDefaultValue(true);
            FolderSetting savePath = saveModelBuilder.AddChildFolderSetting(true, SettingNames.SAVE_MODEL_FOLDER_PATH, "");
            savePath.CanValueBeEmpty = false;//This value has to be provided
            saveModelBuilder.AddChildBooleanSetting(true, SettingNames.SAVE_VARIABLES_TO_TXT, true, null);
            //Ebaa 8.05.2018
            saveModelBuilder.AddChildBooleanSetting(true, SettingNames.SAVE_FIRST_MODEL_TO_XML, true, null);
            BooleanSetting saveToXMLSetting = saveModelBuilder.GetResult();
            //builder.AddSetting(saveToXMLSetting);

            //Building of database_access setting
            BooleanSettingBuilder accessDatabase = new BooleanSettingBuilder();
            accessDatabase.SetSettingName(SettingNames.ALLOW_ACCESS_DATABASE);
            accessDatabase.SetSettingDefaultValue(true);

            accessDatabase.AddChildBooleanSetting(true, SettingNames.USE_LEGACY_DATABASE, true, null).RequiresRestart = true;
            
            DatabaseConnectionSetting dbConnectionSetting = accessDatabase.AddChildDatabaseConnectionSetting(true, SettingNames.DATABASE_CONNECTION, "", 3306, "", "", "", "");
            dbConnectionSetting.RequiresRestart = true;
            accessDatabase.AddChildSetting(true, saveToXMLSetting);
            BooleanSetting accessDB = accessDatabase.GetResult();
            accessDB.RequiresRestart = true;
            builder.AddSetting(accessDB);

            FileSetting interpreter = builder.AddFileSetting(SettingNames.PYTHON_INTERPRETER, "", new List<string> { "Executable Files (*.exe)|*.exe" });
            interpreter.CanValueBeEmpty = false;//This value has to be provided

            return builder.GetResult();
        }

        /// <summary>
        /// Creates the profile for the ColdRydberg4thFloor experiment.
        /// </summary>
        /// <returns>A new profile</returns>
        private Profile CreateColdRydberg4thFloorProfile()
        {
            Profile result = CreateDefaultProfile();
            result.Name = default_profile_names[1];
            ((BooleanSetting)result.GetSettingByName(SettingNames.USE_LEGACY_DATABASE)).Value = false;

            return result;
        }

        /// <summary>
        /// Creates the profile for the RQO5thFloor experiment.
        /// </summary>
        /// <returns>A new profile</returns>
        private Profile CreateRQO5thFloorProfile()
        {
            Profile result = CreateDefaultProfile();
            ((StringMultiOptionSetting)result.GetSettingByName(SettingNames.HW_TYPE)).Value = HW_TYPES.AdWin_T11.ToString();
            ((StringMultiOptionSetting)result.GetSettingByName(SettingNames.HW_TYPE)).DefaultValue = HW_TYPES.AdWin_T11.ToString();
            ((IntegerSetting)result.GetSettingByName(SettingNames.NUM_ANALOG_CARDS)).Value = 2;
            ((IntegerSetting)result.GetSettingByName(SettingNames.NUM_ANALOG_CARDS)).DefaultValue = 2;
            ((IntegerSetting)result.GetSettingByName(SettingNames.NUM_DIGITAL_CARDS)).Value = 1;
            ((IntegerSetting)result.GetSettingByName(SettingNames.NUM_DIGITAL_CARDS)).DefaultValue = 1;
            ((DecimalSetting)result.GetSettingByName(SettingNames.CONSTANT_WAIT_TIME)).Value = 1;
            ((DecimalSetting)result.GetSettingByName(SettingNames.CONSTANT_WAIT_TIME)).DefaultValue = 1;
            ((BooleanSetting)result.GetSettingByName(SettingNames.SAVE_MODEL_TO_XML)).Value = false;
            ((BooleanSetting)result.GetSettingByName(SettingNames.SAVE_MODEL_TO_XML)).DefaultValue = false;

            result.Name = default_profile_names[2];

            return result;
        }

        /// <summary>
        /// Creates the profile for the SuperAtoms5thFloor experiment.
        /// </summary>
        /// <returns>A new profile</returns>
        private Profile CreateSuperAtoms5thFloorProfile()
        {
            Profile result = CreateDefaultProfile();
            ((StringMultiOptionSetting)result.GetSettingByName(SettingNames.HW_TYPE)).Value = HW_TYPES.NI_CHASSIS.ToString();
            ((StringMultiOptionSetting)result.GetSettingByName(SettingNames.HW_TYPE)).DefaultValue = HW_TYPES.NI_CHASSIS.ToString();
            ((IntegerSetting)result.GetSettingByName(SettingNames.NUM_ANALOG_CARDS)).Value = 3;
            ((IntegerSetting)result.GetSettingByName(SettingNames.NUM_ANALOG_CARDS)).DefaultValue = 3;
            ((IntegerSetting)result.GetSettingByName(SettingNames.NUM_DIGITAL_CARDS)).Value = 2;
            ((IntegerSetting)result.GetSettingByName(SettingNames.NUM_DIGITAL_CARDS)).DefaultValue = 2;
            ((IntegerSetting)result.GetSettingByName(SettingNames.SAMPLE_RATE)).Value = 100;
            ((IntegerSetting)result.GetSettingByName(SettingNames.SAMPLE_RATE)).DefaultValue = 100;
            result.Name = default_profile_names[0];

            return result;
        }

        /// <summary>
        /// Creates the profile for the Dy4thFloor experiment.
        /// </summary>
        /// <returns>A new profile</returns>
        private Profile CreateDy4thFloorProfile()
        {
            Profile result = CreateDefaultProfile();
            ((StringMultiOptionSetting)result.GetSettingByName(SettingNames.HW_TYPE)).Value = HW_TYPES.NI_PCI.ToString();
            ((StringMultiOptionSetting)result.GetSettingByName(SettingNames.HW_TYPE)).DefaultValue = HW_TYPES.NI_PCI.ToString();
            ((IntegerSetting)result.GetSettingByName(SettingNames.NUM_ANALOG_CARDS)).Value = 3;
            ((IntegerSetting)result.GetSettingByName(SettingNames.NUM_ANALOG_CARDS)).DefaultValue = 3;
            ((IntegerSetting)result.GetSettingByName(SettingNames.NUM_DIGITAL_CARDS)).Value = 1;
            ((IntegerSetting)result.GetSettingByName(SettingNames.NUM_DIGITAL_CARDS)).DefaultValue = 1;
            ((IntegerSetting)result.GetSettingByName(SettingNames.SAMPLE_RATE)).Value = 100;
            ((IntegerSetting)result.GetSettingByName(SettingNames.SAMPLE_RATE)).DefaultValue = 100;
            result.Name = default_profile_names[3];

            return result;
        }

        /// <summary>
        /// Creates the simulator profile.
        /// </summary>
        /// <returns>A new profile</returns>
        private Profile CreateSimulatorProfile()
        {
            Profile result = CreateDefaultProfile();
            ((StringMultiOptionSetting)result.GetSettingByName(SettingNames.HW_TYPE)).Value = HW_TYPES.AdWin_Simulator.ToString();
            ((StringMultiOptionSetting)result.GetSettingByName(SettingNames.HW_TYPE)).DefaultValue = HW_TYPES.AdWin_Simulator.ToString();
            ((BooleanSetting)result.GetSettingByName(SettingNames.ALLOW_ACCESS_DATABASE)).Value = false;
            ((BooleanSetting)result.GetSettingByName(SettingNames.ALLOW_ACCESS_DATABASE)).DefaultValue = false;
            ((SampleRateSetting)result.GetSettingByName(SettingNames.SAMPLE_RATE)).UnitOfSampleRate = SampleRateUnit.Hz;
            ((SampleRateSetting)result.GetSettingByName(SettingNames.SAMPLE_RATE)).Value = 20;

            result.Name = default_profile_names[4];

            return result;
        }

        private Profile CreateNoOutputProfile()
        {
            Profile result = CreateDefaultProfile();
            ((StringMultiOptionSetting)result.GetSettingByName(SettingNames.HW_TYPE)).Value = HW_TYPES.NO_OUTPUT.ToString();
            ((StringMultiOptionSetting)result.GetSettingByName(SettingNames.HW_TYPE)).DefaultValue = HW_TYPES.NO_OUTPUT.ToString();
            ((BooleanSetting)result.GetSettingByName(SettingNames.ALLOW_ACCESS_DATABASE)).Value = false;
            ((BooleanSetting)result.GetSettingByName(SettingNames.ALLOW_ACCESS_DATABASE)).DefaultValue = false;

            result.Name = default_profile_names[5];

            return result;
        }
    }
}

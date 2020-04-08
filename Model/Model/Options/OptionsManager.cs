using Model.Options.Builders;
using Model.Settings.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Model.Options
{
    /// <summary>
    /// Stores and manages the set of all options
    /// </summary>
    [Serializable]
    public class OptionsManager
    {
        /// <summary>
        /// The path of the folder that saves the options
        /// </summary>
        private const string OPTIONS_SAVE_PATH = @"\Computer Control Options\";
        /// <summary>
        /// The name of the file that saves the options
        /// </summary>
        private const string SAVE_FILE_NAME = "ProgramOptions.options";
        /// <summary>
        /// The singleton instance of this class
        /// </summary>
        private static OptionsManager instance;

        /// <summary>
        /// The option groups of the first level of the options hierarchy.
        /// </summary>
        private ICollection<OptionsGroup> optionGroups = new ObservableCollection<OptionsGroup>();

        /// <summary>
        /// Gets or sets the option groups of the first level of the options hierarchy.
        /// </summary>
        /// <value>
        /// The option groups.
        /// </value>
        public ICollection<OptionsGroup> OptionGroups
        {
            get { return optionGroups; }
            set { optionGroups = value; }
        }

        /// <summary>
        /// Gets the save folder path.
        /// </summary>
        /// <value>
        /// The save folder.
        /// </value>
        public static string SaveFolder
        {
            get
            {
                string saveFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + OPTIONS_SAVE_PATH;

                return saveFolder;
            }
        }

        /// <summary>
        /// Gets the save file path.
        /// </summary>
        /// <value>
        /// The save file.
        /// </value>
        public static string SaveFile
        {
            get
            {
                string result = SaveFolder + SAVE_FILE_NAME;

                return result;
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="OptionsManager"/> class from being created.
        /// </summary>
        private OptionsManager()
        { }

        /// <summary>
        /// Gets the singleton instance of the <see cref=" OptionsManager"/> class.
        /// </summary>
        /// <returns>The singleton instance of the <see cref=" OptionsManager"/> class.</returns>
        public static OptionsManager GetInstance()
        {
            if (instance == null)
            {
                instance = new OptionsManager();
                instance.LoadOptions();
            }

            return instance;
        }

        /// <summary>
        /// Loads the options.
        /// </summary>
        /// <returns><c>true</c> if the options were read from the options file. <c>false</c> if the default options were used.</returns>
        private bool LoadOptions()
        {
            bool hasLoadedOptions = false;
            optionGroups = DefaultOptionsFactory.CreateDefaultOptions();

            if (File.Exists(SaveFile))
            {
                ICollection<OptionsGroup> savedOptions = ReadOptionsFromFile(SaveFile);
                ICollection<BasicSetting> savedOptionsFlat = TraverseOptions(savedOptions);
                string[] allCurrentNames = OptionNames.GetAllNames();

                foreach (string optionName in allCurrentNames)
                {
                    foreach (BasicSetting savedOption in savedOptionsFlat)
                    {
                        if (savedOption.NAME.Equals(optionName))
                        {
                            ReplaceOptionByName(optionName, savedOption);
                        }
                    }
                }

                hasLoadedOptions = true;

            }
            else
            {
                hasLoadedOptions = false;
            }

            SaveOptions();

            return hasLoadedOptions;
        }

        /// <summary>
        /// Reads the options from file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>A collection of the <see cref=" OptionsGroup"/>s countained in the specified file.</returns>
        private ICollection<OptionsGroup> ReadOptionsFromFile(string filePath)
        {
            ICollection<OptionsGroup> result = null;
            using (var reader = new FileStream(filePath, FileMode.Open))
            {
                var deserializer = new DataContractSerializer(typeof(ObservableCollection<OptionsGroup>));
                result = (ICollection<OptionsGroup>)deserializer.ReadObject(reader);
            }

            return result;
        }

        /// <summary>
        /// Saves the options to the save file.
        /// </summary>
        public void SaveOptions()
        {
            string saveFolder = SaveFolder;

            // save variables
            if (!Directory.Exists(saveFolder))
            {
                Directory.CreateDirectory(saveFolder);
            }

            string filePath = SaveFile;
            SaveOptionsToPath(filePath);
        }

        /// <summary>
        /// Saves the options to a specific path.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        private void SaveOptionsToPath(string filePath)
        {
            // xml data
            using (var writer = new FileStream(filePath, FileMode.Create))
            {
                Type type = optionGroups.GetType();
                var serializer = new DataContractSerializer(type);
                serializer.WriteObject(writer, optionGroups);
            }
        }

        /// <summary>
        /// Deletes the options from disk.
        /// </summary>
        private static void DeleteOptionsFromDisk()
        {
            try
            {
                if (Directory.Exists(SaveFolder))
                {
                    DirectoryInfo di = new DirectoryInfo(SaveFolder);
                    di.Delete(true);
                    di.Refresh();
                }
            }
            catch (Exception)
            {
                //This might need to be handled better!
            }
        }

        /// <summary>
        /// Deeply copies this instance.
        /// </summary>
        /// <returns>A deep copy of the instance.</returns>
        public OptionsManager DeepCopyOptionsManager()
        {
            var stream = new MemoryStream();
            var formatter = new BinaryFormatter { Context = new StreamingContext(StreamingContextStates.Clone) };
            formatter.Serialize(stream, optionGroups);
            stream.Position = 0;
            ICollection<OptionsGroup> copyOfOptions = (ICollection<OptionsGroup>)formatter.Deserialize(stream);

            OptionsManager copy = new OptionsManager();
            copy.optionGroups = copyOfOptions;

            return copy;
        }

        /// <summary>
        /// Reads a previously copied instance that were altered and stores the changes in this instance.
        /// </summary>
        /// <param name="copy">The copy.</param>
        public void GetValuesFromCopy(OptionsManager copy)
        {
            optionGroups = copy.optionGroups;
        }

        /// <summary>
        /// Restores the original options.
        /// </summary>
        /// <returns></returns>
        public static OptionsManager RestoreOriginalOptions()
        {
            DeleteOptionsFromDisk();
            instance = GetInstance();

            return instance;
        }


        /// <summary>
        /// Replaces an existing option with a new one
        /// </summary>
        /// <param name="optionName">Name of the option.</param>
        /// <param name="newOption">The new option.</param>
        /// <returns><c>true</c> if the option was found and replaced.</returns>
        private bool ReplaceOptionByName(string optionName, BasicSetting newOption)
        {
            foreach (OptionsGroup group in optionGroups)
            {
                if (group.ReplaceChildSettingByName(optionName, newOption))
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Gets an option by its name.
        /// </summary>
        /// <param name="optionName">Name of the option.</param>
        /// <returns>The option whose name is specified.</returns>
        public BasicSetting GetOptionByName(string optionName)
        {
            BasicSetting result = null;

            foreach(OptionsGroup group in optionGroups)
            {
                result = group.GetChildOptionByName(optionName);

                if(result != null)
                    break;
            }

            return result;
        }

        

        /// <summary>
        /// Gets the value of the specified option.
        /// </summary>
        /// <typeparam name="T">The type of the value of the option</typeparam>
        /// <param name="optionName">Name of the option.</param>
        /// <returns>The value of the option</returns>
        public T GetOptionValueByName<T>(string optionName)
        {
            Setting<T> option = (Setting<T>)GetOptionByName(optionName);

            return option.Value;
        }

        /// <summary>
        /// Traverses the set of all options.
        /// </summary>
        /// <param name="collectionOfOptionsGroups">The collection of options groups.</param>
        /// <returns></returns>
        private ICollection<BasicSetting> TraverseOptions(ICollection<OptionsGroup> collectionOfOptionsGroups)
        {
            ICollection<BasicSetting> result = new ObservableCollection<BasicSetting>();

            foreach (OptionsGroup group in collectionOfOptionsGroups)
            {
                group.TraverseChildren(result);
            }

            return result;
        }

        //An assumption here is that the same set of options exists in both of the two groups        
        /// <summary>
        /// Determines whether a restart of the application is required to changes in options.
        /// </summary>
        /// <param name="otherOptions">The other options.</param>
        /// <returns><c>true</c> if a restart is required to be performed</returns>
        public bool IsRestartRequired(ICollection<OptionsGroup> otherOptions)
        {
            ICollection<BasicSetting> otherFlat = TraverseOptions(optionGroups);
            BasicSetting myOption;

            foreach (BasicSetting option in otherFlat)
            {
                if(option.RequiresRestart)
                {
                    myOption = GetOptionByName(option.NAME);

                    if (myOption.HasValueChanged(option))
                        return true;
                }
            }

            return false;

        }

    }
}

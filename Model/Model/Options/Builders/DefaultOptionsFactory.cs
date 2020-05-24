using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Model.Options.Builders
{
    /// <summary>
    /// Creates the default options
    /// </summary>
    class DefaultOptionsFactory
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="DefaultOptionsFactory"/> class from being created.
        /// </summary>
        private DefaultOptionsFactory()
        { }

        /// <summary>
        /// Creates the default options.
        /// </summary>
        /// <returns>A collection of the option groups of the highest level of the hierarchy.</returns>
        public static ICollection<OptionsGroup> CreateDefaultOptions()
        {
            ICollection<OptionsGroup> result = new ObservableCollection<OptionsGroup>();

            //Display options
            //Variables Window Options
            OptionsGroupBuilder variablesWindowOGB = new OptionsGroupBuilder();
            variablesWindowOGB.SetName("Variables Window");
            variablesWindowOGB.AddIntegerSetting(OptionNames.VARIABLES_STATIC_GROUP_HEIGHT, 10, 1, 100, "Variables");
            OptionsGroup variablesWindowOG = variablesWindowOGB.GetResult();

            OptionsGroupBuilder visualizationOGB = new OptionsGroupBuilder();
            visualizationOGB.SetName("Output Visualizer Window");
            visualizationOGB.AddIntegerSetting(OptionNames.VISUALIZED_SAMPLES, 400, 1, 10000, "Number of Samples");
            OptionsGroup visualizationOG = visualizationOGB.GetResult();

            //General
            OptionsGroupBuilder displayGeneralOGB = new OptionsGroupBuilder();
            displayGeneralOGB.SetName("Display");
            Uri iconUri = new Uri("pack://application:,,,/View;component/Resources/cr.png", UriKind.RelativeOrAbsolute);
            displayGeneralOGB.AddFileSetting(OptionNames.ICON_PATH, iconUri.AbsolutePath, new List<string>() { "PNG files|*.png", "JPEG files|*.jpg;*.jpeg", "ICON files|*.ico" });
            displayGeneralOGB.AddBooleanSetting(OptionNames.AUTOMATICALLY_OPEN_WINDOWS, true, null);
            displayGeneralOGB.AddChildOptionsGroup(variablesWindowOG);
            displayGeneralOGB.AddChildOptionsGroup(visualizationOG);
            result.Add(displayGeneralOGB.GetResult());




            return result;
        }
    }
}

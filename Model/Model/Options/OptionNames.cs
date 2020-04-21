namespace Model.Options
{
    /// <summary>
    /// Holds a set of constants representing the names of all existing options.
    /// </summary>
    public sealed class OptionNames
    {
        /// <summary>
        /// The constant name for the icon path option
        /// </summary>
        public const string ICON_PATH = "Icon file path";
        /// <summary>
        /// The constant name for the option that specifies the maximum number of variables that fit in a single column of a static variables group.
        /// </summary>
        public const string VARIABLES_STATIC_GROUP_HEIGHT = "Max # of static variables per group column";
        /// <summary>
        /// The constant name for the option that specifies whether the program should open all windows (variables, errors, and cards) automatically at startup.
        /// </summary>
        public const string AUTOMATICALLY_OPEN_WINDOWS = "Automatically open all windows at startup";

        public const string VISUALIZED_SAMPLES = "The number of output values that are visualized";

        /// <summary>
        /// Gets a collection of all option names
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllNames()
        {
            return new string[] { 
                ICON_PATH,
                VARIABLES_STATIC_GROUP_HEIGHT,
                AUTOMATICALLY_OPEN_WINDOWS,
                VISUALIZED_SAMPLES
            };
        }
    }
}

using Model.Settings.Builders;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Model.Options.Builders
{
    /// <summary>
    /// Builds a group of options
    /// </summary>
    /// <seealso cref="Model.Settings.Builders.SettingsCollectionBuilder" />
    class OptionsGroupBuilder:SettingsCollectionBuilder
    {
        /// <summary>
        /// The name of the group
        /// </summary>
        private string name;
        /// <summary>
        /// The child groups contained within the group
        /// </summary>
        private ICollection<OptionsGroup> childGroups = new ObservableCollection<OptionsGroup>();

        /// <summary>
        /// Sets the name of the group.
        /// </summary>
        /// <param name="name">The name.</param>
        public void SetName(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Adds the child option groups.
        /// </summary>
        /// <param name="child">The child.</param>
        public void AddChildOptionsGroup(OptionsGroup child)
        {
            childGroups.Add(child);
        }

        /// <summary>
        /// Gets the resulting options group.
        /// </summary>
        /// <returns>The resulting options group.</returns>
        public OptionsGroup GetResult()
        {
            OptionsGroup result = new OptionsGroup(name, GetSettingCollection(), childGroups);

            return result;
        }
        

    }
}

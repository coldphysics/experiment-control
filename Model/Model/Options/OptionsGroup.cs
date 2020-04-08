using Model.Settings;
using Model.Settings.Settings;
using System;
using System.Collections.Generic;

namespace Model.Options
{
    /// <summary>
    /// A set of options semantically related to each other
    /// </summary>
    /// <seealso cref="Model.Settings.Settings.ISettingWithChildren" />
    [Serializable]
    public class OptionsGroup:ISettingWithChildren
    {
        /// <summary>
        /// The options directly contained in this options group
        /// </summary>
        private ICollection<BasicSetting> relatedOptions;
        /// <summary>
        /// The potential child options groups directly contained in this options group
        /// </summary>
        private ICollection<OptionsGroup> childOptionGroups;
        /// <summary>
        /// The name of the options group
        /// </summary>
        private string name;

        /// <summary>
        /// Gets the name of the options group.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get { return name; }

        }

        /// <summary>
        /// Gets the child option groups.
        /// </summary>
        /// <value>
        /// The child option groups.
        /// </value>
        public ICollection<OptionsGroup> ChildOptionGroups
        {
            get { return childOptionGroups; }

        }
        
        /// <summary>
        /// Gets the related options.
        /// </summary>
        /// <value>
        /// The related options.
        /// </value>
        public ICollection<BasicSetting> RelatedOptions
        {
            get { return relatedOptions; }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsGroup"/> class.
        /// </summary>
        /// <param name="name">The name of the group.</param>
        /// <param name="relatedOptions">The related options.</param>
        /// <param name="childOptionGroups">The child option groups.</param>
        public OptionsGroup(string name, ICollection<BasicSetting> relatedOptions, ICollection<OptionsGroup> childOptionGroups)
        {
            this.relatedOptions = relatedOptions;
            this.childOptionGroups = childOptionGroups;
            this.name = name;
        }

        /// <summary>
        /// Finds a child option based on its name.
        /// </summary>
        /// <param name="optionName">Name of the option.</param>
        /// <returns>The found option or <c>null</c></returns>
        public BasicSetting GetChildOptionByName(string optionName)
        {
            BasicSetting result = SettingsCollectionSearcher.FindSettingByName(optionName, RelatedOptions);

            if (result == null)
            {
                foreach (OptionsGroup childGroup in childOptionGroups)
                {
                    result = childGroup.GetChildOptionByName(optionName);

                    if (result != null)
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Replaces the option with the given name with a new option
        /// </summary>
        /// <param name="settingName">Name of the option.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns><c>true</c> if the option was found and replaced.</returns>
        public bool ReplaceChildSettingByName(string optionName, BasicSetting newValue)
        {
            if (!SettingsCollectionSearcher.ReplaceSettingByName(optionName, newValue, RelatedOptions))
            {
                foreach (OptionsGroup childGroup in childOptionGroups)
                {
                    if (childGroup.ReplaceChildSettingByName(optionName, newValue))
                        return true;
                }

                return false;
            }

            return true;
        }
                
        /// <summary>
        /// Traverses the children.
        /// </summary>
        /// <param name="options">The result of the traversal</param>
        public void TraverseChildren(ICollection<BasicSetting> options)
        {
            SettingsCollectionSearcher.TraversAllSettings(options, RelatedOptions);

            foreach (OptionsGroup childGroup in childOptionGroups)
            {
                childGroup.TraverseChildren(options);
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }



        #region ISettingWithChildren Members




        #endregion
    }
}

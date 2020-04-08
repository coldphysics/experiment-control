using System.Collections.Generic;

namespace CustomElements.CheckableTreeView
{
    /// <summary>
    /// A view model for the checked tree view.
    /// </summary>
    /// <seealso cref="System.Collections.Generic.List{CustomElements.CheckableTreeView.CTVItemViewModel}" />
    public class CTVViewModel:List<CTVItemViewModel>
    {
        /// <summary>
        /// Gets the checked items at the leaf level.
        /// </summary>
        /// <returns></returns>
        public ICollection<CTVItemViewModel> GetCheckedLeaves()
        {
            ICollection<CTVItemViewModel> result = new List<CTVItemViewModel>();
            CTVItemViewModel root = this[0];
            root.GetCheckedLeaves(result);

            return result;
        }
    }
}

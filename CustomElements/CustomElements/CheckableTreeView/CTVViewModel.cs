using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace CustomElements.CheckableTreeView
{
    /// <summary>
    /// A view model for the checked tree view.
    /// </summary>
    /// <seealso cref="System.Collections.Generic.List{CustomElements.CheckableTreeView.CTVItemViewModel}" />
    public class CTVViewModel : ObservableCollection<CTVItemViewModel>
    {
        public event EventHandler CheckStateChanged;

        public CTVViewModel()
        {
            CollectionChanged += CTVViewModel_CollectionChanged;
        }

        private void CTVViewModel_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (CTVItemViewModel subitem in e.NewItems)
                {
                    subitem.HierarchyCheckStateChanged += Subitem_HierarchyCheckStateChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (CTVItemViewModel subitem in e.OldItems)
                {
                    subitem.HierarchyCheckStateChanged -= Subitem_HierarchyCheckStateChanged;
                }
            }
        }

        private void Subitem_HierarchyCheckStateChanged(object sender, EventArgs e)
        {
            CheckStateChanged?.Invoke(sender, e);
        }

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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace CustomElements.CheckableTreeView
{
    /// <summary>
    /// The view model for the checkable tree view item
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public abstract class CTVItemViewModel : INotifyPropertyChanged
    {
        public event EventHandler HierarchyCheckStateChanged;
        #region Data

        /// <summary>
        /// Indicates whether the item is checked.
        /// </summary>
        /// <remarks>
        /// The <c>null</c> state means that the checked state is <c>undefined</c>.
        /// </remarks>
        private bool? _isChecked = false;
        /// <summary>
        /// The parent item.
        /// </summary>
        private CTVItemViewModel _parent;
        /// <summary>
        /// The child items.
        /// </summary>
        private ObservableCollection<CTVItemViewModel> children;

        #endregion // Data

        #region Properties

        /// <summary>
        /// Gets or sets the child items.
        /// </summary>
        /// <value>
        /// The children.
        /// </value>
        public ObservableCollection<CTVItemViewModel> Children
        {
            get
            {
                return children;
            }
            set
            {
                this.children = value;

                foreach (CTVItemViewModel child in this.children)
                    child._parent = this;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this item is initially selected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this item is initially selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsInitiallySelected { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this item is initially expanded.
        /// </summary>
        /// <value>
        /// <c>true</c> if this item is initially expanded; otherwise, <c>false</c>.
        /// </value>
        public bool IsInitiallyExpanded { get; set; }

        /// <summary>
        /// Gets the name shown as the header for the item.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public abstract string Name { get; }

        #region IsChecked

        /// <summary>
        /// Gets/sets the state of the associated UI toggle (ex. CheckBox).
        /// The return value is calculated based on the check state of all
        /// child FooViewModels.  Setting this property to true or false
        /// will set all children to the same check state, and setting it 
        /// to any value will cause the parent to verify its check state.
        /// </summary>
        public bool? IsChecked
        {
            get { return _isChecked; }
            set
            {
                this.SetIsChecked(value, true, true);

            }
        }

        /// <summary>
        /// Sets the is checked property
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="updateChildren">if set to <c>true</c> the children's state should be updated.</param>
        /// <param name="updateParent">if set to <c>true</c> the parent's state should be updated.</param>
        void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked)
                return;

            _isChecked = value;

            if (updateChildren && _isChecked.HasValue && children != null)
            {
                foreach (CTVItemViewModel child in this.children)
                    child.SetIsChecked(_isChecked, true, false);
            }

            if (updateParent && _parent != null)
                _parent.VerifyCheckState();

            this.OnPropertyChanged("IsChecked");
            Subitem_HierarchyCheckStateChanged(this, new EventArgs());
        }

        /// <summary>
        /// Verifies the state of the check based on the states of the children.
        /// </summary>
        void VerifyCheckState()
        {
            bool? state = null;
            for (int i = 0; i < this.Children.Count; ++i)
            {
                bool? current = this.Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            this.SetIsChecked(state, false, true);
        }

        #endregion // IsChecked

        #endregion // Properties

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="prop">The property.</param>
        void OnPropertyChanged(string prop)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        /// <summary>
        /// Adds a child item
        /// </summary>
        /// <param name="child">The child.</param>
        public void AddChild(CTVItemViewModel child)
        {
            if (children == null)
            {
                children = new ObservableCollection<CTVItemViewModel>();
                children.CollectionChanged += Children_CollectionChanged;
            }

            children.Add(child);
            child._parent = this;
        }

        private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
            HierarchyCheckStateChanged?.Invoke(sender, e);
        }


        /// <summary>
        /// Gets the checked child items (including the item itself) at the leaf level.
        /// </summary>
        /// <param name="result">The result.</param>
        public void GetCheckedLeaves(ICollection<CTVItemViewModel> result)
        {
            if (Children != null && Children.Count > 0)
            {
                if (IsChecked == null || IsChecked == true)
                {
                    foreach (CTVItemViewModel child in Children)
                        child.GetCheckedLeaves(result);
                }//else no children are checked
            }
            else//It is a leaf
            {
                if (IsChecked != null && IsChecked.Value)
                    result.Add(this);
                //else it is not checked
            }
        }

    }
}
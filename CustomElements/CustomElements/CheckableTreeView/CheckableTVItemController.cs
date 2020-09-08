

namespace CustomElements.CheckableTreeView
{
    /// <summary>
    /// A model view for an item of the checkable tree view that holds a 
    /// reference to an associated object (a channel or card controller in this use-case)
    /// </summary>
    /// <seealso cref="CTVItemViewModel" />
    public class CheckableTVItemController : CTVItemViewModel
    {
        /// <summary>
        /// Gets or sets the (real) item associated to this checkable item
        /// </summary>
        /// <value>
        /// The item.
        /// </value>
        public object Item { get; set; }


        /// <summary>
        /// Gets or sets the name (the Item.ToString() is the default value)
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public override string Name { set; get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckableTVItemController"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        public CheckableTVItemController(object item)
        {
            Item = item;

            if (Name == null)
                Name = Item.ToString();
        }
    }
}

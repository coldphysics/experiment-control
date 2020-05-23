using System;
using System.Collections.Generic;
using AbstractController.Data.Card;
using AbstractController.Data.Channels;
using Controller.Root;
using CustomElements.CheckableTreeView;
using Model.Data.Cards;

namespace Controller.Control.StepBatchAddition
{
    /// <summary>
    /// A model view for an item of the checkable tree view that holds a 
    /// reference to an associated object (a channel or card controller in this use-case)
    /// </summary>
    /// <seealso cref="CustomElements.CheckableTreeView.CTVItemViewModel" />
    public class CheckableTVItemController : CTVItemViewModel
    {
        /// <summary>
        /// The (real) item associated to this checkable item
        /// </summary>
        private object item;

        /// <summary>
        /// Gets or sets the (real) item associated to this checkable item
        /// </summary>
        /// <value>
        /// The item.
        /// </value>
        public object Item
        {
            get { return item; }
            set { item = value; }
        }


        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public override string Name
        {
            get
            {
                return item.ToString();
            }
        }



        /// <summary>
        /// Initializes a new instance of the <see cref="CheckableTVItemController"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        public CheckableTVItemController(object item)
        {
            this.item = item;
        }
    }

    /// <summary>
    /// Builds <see cref=" CTVViewModel"/>'s
    /// </summary>
    public class ModelBasedCTVBuilder
    {
        /// <summary>
        /// Builds a new instance of <see cref=" CTVViewModel"/> based on a <see cref=" RootController"/> and for either digital cards or analog cards.
        /// </summary>
        /// <param name="controller">The root controller.</param>
        /// <param name="isDigital">if set to <c>true</c> then the resulting <see cref=" CTVViewModel"/> shows digital cards only, 
        /// otherwise it will show analog cards only</param>
        /// <returns>The resulting <see cref=" CTVViewModel"/></returns>
        public static CTVViewModel Build(RootController controller, bool isDigital)
        {
            ICollection<AbstractCardController> cards = controller.DataController.SequenceGroup.Windows;
            string rootName = String.Format("All {0} Cards", (isDigital ? "Digital" : "Analog"));
            CTVItemViewModel root = new CheckableTVItemController(rootName) { IsInitiallyExpanded = true };
            CTVItemViewModel currentCard;
            CTVItemViewModel currentChannel;

            foreach (AbstractCardController card in cards)
            {

                currentCard = new CheckableTVItemController(card);

                if (isDigital == (card.CardType() == CardBasicModel.CardType.Digital))
                {

                    foreach (AbstractChannelController channel in card.Tabs[0].Channels)
                    {
                        currentChannel = new CheckableTVItemController(channel);
                        currentCard.AddChild(currentChannel);
                    }
                    root.AddChild(currentCard);
                }
            }

            return new CTVViewModel() { root };
        }

        //Ebaa
        public static CTVViewModel BuildCheckableTree(RootController controller)
        {
            ICollection<AbstractCardController> cards = controller.DataController.SequenceGroup.Windows;
            string rootName = String.Format("Cards:");
            CTVItemViewModel root = new CheckableTVItemController(rootName) { IsInitiallyExpanded = true };
            CTVItemViewModel currentCard;
            CTVItemViewModel currentChannel;

            foreach (AbstractCardController card in cards)
            {

                currentCard = new CheckableTVItemController(card);
                CardBasicModel cardModel = card.Model;



                //if (isDigital == (card.CardType() == CardBasicModel.CardType.Digital))
                // {


                foreach (AbstractChannelController channel in card.Tabs[0].Channels)
                {
                    currentChannel = new CheckableTVItemController(channel);
                    string name = channel.ToString();
                    // currentChannel.UserdefinedChannelName = cardModel.Settings[channel.Index()].Name;
                    currentCard.AddChild(currentChannel);
                }
                root.AddChild(currentCard);
                // }
            }
            CTVViewModel result = new CTVViewModel();
            result.Add(root);

            return result ;
        }
    }
}

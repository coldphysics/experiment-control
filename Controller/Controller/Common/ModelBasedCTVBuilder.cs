using System;
using System.Collections.Generic;
using AbstractController.Data.Card;
using AbstractController.Data.Channels;
using Controller.Root;
using CustomElements.CheckableTreeView;
using Model.Data.Cards;

namespace Controller.Common
{

    /// <summary>
    /// Builds a view model for a (Checkable Tree View) <see cref=" CTVViewModel"/> based on the structure of a data model.
    /// </summary>
    public class ModelBasedCTVBuilder
    {
        /// <summary>
        /// Builds a new instance of <see cref=" CTVViewModel"/> based on a <see cref=" RootController"/> and for either digital cards or analog cards.
        /// </summary>
        /// <param name="controller">The root controller.</param>
        /// <param name="isDigital">if set to <c>true</c> then the resulting <see cref=" CTVViewModel"/> shows digital cards only, 
        /// otherwise it will show analog cards only</param>
        /// <returns>The resulting <see cref=" CTVViewModel"/> representing the root of a tree</returns>
        public static CTVViewModel Build(RootController controller, bool isDigital)
        {
            ICollection<AbstractCardController> cards = controller.DataController.SequenceGroup.Windows;
            string rootName = string.Format("All {0} Cards", (isDigital ? "Digital" : "Analog"));
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

        /// <summary>
        ///  Builds a new instance of <see cref=" CTVViewModel"/> based on a <see cref=" RootController"/> and for all cards.
        /// </summary>
        /// <param name="controller"> The root controller</param>
        /// <returns>The resulting <see cref=" CTVViewModel"/> representing the root of a tree</returns>
        public static CTVViewModel BuildCheckableTree(RootController controller)
        {
            ICollection<AbstractCardController> cards = controller.DataController.SequenceGroup.Windows;
            string rootName = string.Format("Cards:");
            CTVItemViewModel root = new CheckableTVItemController(rootName) { IsInitiallyExpanded = true };
            CTVItemViewModel currentCard;
            CTVItemViewModel currentChannel;

            foreach (AbstractCardController card in cards)
            {
                currentCard = new CheckableTVItemController(card);
                CardBasicModel cardModel = card.Model;

                foreach (AbstractChannelController channel in card.Tabs[0].Channels)
                {
                    currentChannel = new CheckableTVItemController(channel);
                    string name = channel.ToString();
                    // currentChannel.UserdefinedChannelName = cardModel.Settings[channel.Index()].Name;
                    currentCard.AddChild(currentChannel);
                }

                root.AddChild(currentCard);
            }

            CTVViewModel result = new CTVViewModel();
            result.Add(root);

            return result ;
        }
    }
}

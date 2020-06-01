using Model.Data.Cards;
using Model.Data.Cookbook;
using Model.Data.SequenceGroups;
using Model.Root;
using Model.Settings;
using System.Collections.Generic;

namespace Model.Builder
{
    /// <summary>
    /// Builds a blank (empty from sequences) model hierarchy based on the values of the active settings profile.
    /// </summary>
    public class ModelBuilder
    {
        /// <summary>
        /// The root of the resulting hierarchy
        /// </summary>
        private RootModel root;
        /// <summary>
        /// A dictionary that associates the name of a card with its type (analog/digital)
        /// </summary>
        private Dictionary<string, CardBasicModel.CardType> cardTypes;
        /// <summary>
        /// The naming base for digital cards
        /// </summary>
        private const string DIGITAL_CARD_NAME_BASE = CardBasicModel.DIGITAL_CARD_BASE_NAME;
        /// <summary>
        /// The naming base for analog cards
        /// </summary>
        private const string ANALOG_CARD_NAME_BASE = CardBasicModel.ANALOG_CARD_BASE_NAME;

        /// <summary>
        /// Builds the hierarchy.
        /// </summary>
        /// <remarks>The result of the building process can be accessed using the <see cref=" GetRootModel"/> method.</remarks>
        public void Build()
        {
            //Creating new RootModel
            RootModel model = new RootModel();

            model.Data.variablesModel = new Variables.VariablesModel();
            //Create the Model for the AdWin SequenceGroup
            SequenceGroupModel sequenceGroup = new SequenceGroupModel(model.Data);

            //Adds the SequenceGroup to the RootModel
            model.Data.group  = sequenceGroup;

            //Creates the Generator  for each Card
            ModelRecipe cardModelRecipe = new ModelRecipe();
            
            int analogCards = Global.GetNumAnalogCards();
            int analogChannels = Global.GetNumAnalogChannelsPerCard();
            int digitalCards = Global.GetNumDigitalCards();
            int digitalChannels = Global.GetNumDigitalChannelsPerCard();
            string currentCardName;

            for (int aCard = 0; aCard < analogCards; aCard++)
            {
                currentCardName = string.Format("{0}{1}", ANALOG_CARD_NAME_BASE, aCard + 1);

                cardModelRecipe.CookCard((uint)analogChannels, currentCardName, CardBasicModel.CardType.Analog, sequenceGroup);
            }

            for (int dCard = 0; dCard < digitalCards; dCard++)
            {
                currentCardName = string.Format("{0}{1}", DIGITAL_CARD_NAME_BASE, dCard + 1);

                cardModelRecipe.CookCard((uint)digitalChannels, currentCardName, CardBasicModel.CardType.Digital, sequenceGroup);
            }

            this.root = model;
            this.cardTypes = cardModelRecipe.dict;
        }

        /// <summary>
        /// Gets the root of model hierarchy.
        /// </summary>
        /// <returns>The root of model hierarchy.</returns>
        public RootModel GetRootModel()
        {
            return root;
        }

        /// <summary>
        /// Gets a dictionary that associates the name of a card with its type (analog/digital)
        /// </summary>
        /// <returns>a dictionary that associates the name of a card with its type (analog/digital)</returns>
        public Dictionary<string, CardBasicModel.CardType> GetCardTypes()
        {
            return cardTypes;
        }
    }
}

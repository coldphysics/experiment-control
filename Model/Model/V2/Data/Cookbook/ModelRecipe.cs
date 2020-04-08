using Model.V2.Data.Cards;
using Model.V2.Data.Channels;
using Model.V2.Data.SequenceGroups;
using Model.V2.Data.Sequences;
using System;
using System.Collections.Generic;

namespace Model.V2.Data.Cookbook
{
    //RECO This class should be a singleton (stateless)because the dictionary is redundant
    /// <summary>
    /// Cooks Cards, Sequences and Channels (Factory Design Pattern)
    /// </summary>
    [Serializable]
    public class ModelRecipe
    {
        //TODO seems illogical to have this list here
        /// <summary>
        /// A dictionary that maps the name of card to its <see cref=" CardBasicModel.CardType"/>.
        /// </summary>
        [NonSerialized()]
        public Dictionary<string, CardBasicModel.CardType> dict = new Dictionary<string, CardBasicModel.CardType> { }; //to save infos about the name of the card with its type 

        /// <summary>
        /// Cooks a card (digital or analog) and adds it to the <see cref=" SequenceGroupModel"/>. 
        /// The card will contain one <see cref=" SequenceModel"/> with the specified number of channels.
        /// </summary>
        /// <param name="numberOfChannels">The number of channels the card has</param>
        /// <param name="startIndex">The index of the first channel in the card, usually set to 0</param>
        /// <param name="name">The name of the card</param>
        /// <param name="type">The type of the card (Digital or Analog)</param>
        /// <param name="sequenceGroup">The sequence group that the card belongs to. The card will automatically be added to this <see cref=" SequenceGroupModel"/></param>
        /// <returns>The newly created card</returns>
        public CardModel CookCard(uint numberOfChannels, int startIndex, string name, CardBasicModel.CardType type,
                              SequenceGroupModel sequenceGroup)
        {
            if (type == CardBasicModel.CardType.Analog)
            {
                //update the number of buffers based on this rule: analog--> 8 buffers, digital --> 1 buffer)
                Global.NumberOfBuffers += numberOfChannels; // analog card
            }
            else
            {
                Global.NumberOfBuffers += 1; // digital card#
            }

            dict.Add(name, type);

            CardModel card = new CardModel(name, numberOfChannels, startIndex, type, sequenceGroup);
            CookSequence(numberOfChannels, card);
            //CookSequence(numberOfChannels, card);
            sequenceGroup.Cards.Add(card);
            //System.Console.WriteLine("startIndex: {0}\n", startIndex);
            for (int iChannel = startIndex; iChannel < numberOfChannels + startIndex; iChannel++)
            {
                var setting = new ChannelSettingsModel(iChannel, card);
                card.Settings.Add(setting);
            }

            return card;
        }

        /// <summary>
        /// Cooks a SequenceModel. The Sequence will contain the specified number of <see cref=" ChannelModel"/>s with no steps.
        /// The <c>SequenceModel</c> is added to the <c>CardBasicModel</c>.
        /// </summary>
        /// <param name="numberOfChannels">the number of channels this <c>SequenceModel</c> has.</param>
        /// <param name="card">the <see cref=" CardBasicModel"/> this <c>SequenceModel</c> belongs to.</param>
        /// <returns>The newly created <c>SequenceModel</c></returns>
        internal SequenceModel CookSequence(uint numberOfChannels, CardBasicModel card)
        {
            var sequence = new SequenceModel(card);

            for (int channelIndex = 1; channelIndex <= numberOfChannels; channelIndex++)
            {
                var newChannel = new ChannelModel(sequence);

                //var newStep = new StepRampModel(newChannel, StepRampModel.StoreType.Constant);
                //newChannel.Steps.Add(newStep);

                sequence.Channels.Add(newChannel);
            }

            card.Sequences.Add(sequence);

            return sequence;
        }
    }
}
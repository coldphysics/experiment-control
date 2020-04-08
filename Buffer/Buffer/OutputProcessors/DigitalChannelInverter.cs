using Communication.Interfaces.Generator;
using Generator.Generator.Concatenator;
using Model.Data;
using Model.Data.Cards;
using Model.Data.Channels;
using Model.Data.SequenceGroups;
using Model.Data.Sequences;

namespace Buffer.OutputProcessors
{
    class DigitalChannelInverter : OutputProcessor
    {

        public DigitalChannelInverter(DataModel model)
            : base(model)
        { }

        /// <summary>
        /// Inverts the output of the digital channels that have the "invert" setting set to true.
        /// </summary>
        public override void Process(IModelOutput output)
        {
            InvertDigitalChannels(dataModel, output);
        }

        /// <summary>
        /// Inverts the output of the digital channels that have the "invert" setting set to true.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="output">The raw output.</param>
        private void InvertDigitalChannels(DataModel model, IModelOutput output)
        {
            SequenceGroupModel group = model.group;

            foreach (CardBasicModel card in group.Cards)
            {
                if (card.Type == CardBasicModel.CardType.Digital)
                {
                    foreach (SequenceModel sequence in card.Sequences)
                    {
                        uint chanMask = 0;

                        foreach (ChannelBasicModel channel in sequence.Channels)
                        {
                            if (channel.Setting.Invert)
                            {
                                chanMask |= (uint)(1 << (int)channel.Index());
                            }
                        }

                        InvertDigitalChannel((DigitalCardOutput)output.Output[card.Name], chanMask);
                    }
                }
            }

        }


        /// <summary>
        /// Applies an XOR-mask to the output of a digital card (inverter).
        /// </summary>
        /// <param name="d">The digital card.</param>
        /// <param name="mask">The XOR-mask to apply.</param>
        private void InvertDigitalChannel(DigitalCardOutput d, uint mask)
        {
            uint[] cardOutput = d.Output;
            long elements = -1;


            elements = cardOutput.Length;

            for (long i = 0; i < elements; i++)
            {
                cardOutput[i] ^= mask;
            }
        }
    }
}

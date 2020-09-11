using System.Collections.Generic;


namespace Controller.OutputVisualizer.Export.Abstract
{
    /// <summary>
    /// Represents the set of options that configure how the export operation should happen
    /// Use <see cref="ExportOptionsBuilder"/> to build instances of this class
    /// </summary>
    public class ExportOptions
    {
        /// <summary>
        /// A dictionary mapping card names to the channel indices that has to be exported.
        /// This Property must be omitted.
        /// </summary>
        public Dictionary<string, List<int>> Channels { set; get; }

        /// <summary>
        /// A list of indices of the sequences that has to be exported.
        /// If this property is null, the entire output for the selected channels will be exported.
        /// </summary>
        public List<int> SequenceIndices { set; get; }

        /// <summary>
        /// A list of output fields that will be considered in the export operation.
        /// If this property is null, all possible output fields will be included.
        /// </summary>
        public List<OutputField> OutputFields { set; get; }

    }

    /// <summary>
    /// Builds an instance of the <see cref="ExportOptions"/> class 
    /// </summary>
    public class ExportOptionsBuilder
    {
        private Dictionary<string, List<int>> _channels;
        private List<int> _sequenceIndices;
        private List<OutputField> _outputFields;

        private ExportOptionsBuilder(Dictionary<string, List<int>> channels)
        {
            _channels = channels;
        }

        public static ExportOptionsBuilder NewInstance(Dictionary<string, List<int>> channels)
        {
            ExportOptionsBuilder instance = new ExportOptionsBuilder(channels);

            return instance;
        }

        public static ExportOptionsBuilder NewInstance(string cardName, int channelIndex)
        {
            Dictionary<string, List<int>> channels = new Dictionary<string, List<int>>();
            channels.Add(cardName, new List<int>() { channelIndex });

            return NewInstance(channels);
        }

        public ExportOptionsBuilder SetSequenceIndices(List<int> sequenceIndices)
        {
            _sequenceIndices = sequenceIndices;
            return this;
        }

        public ExportOptionsBuilder AddSequenceIndex(int sequenceIndex)
        {
            if (_sequenceIndices == null)
            {
                _sequenceIndices = new List<int>();
            }

            _sequenceIndices.Add(sequenceIndex);

            return this;
        }

        public ExportOptionsBuilder SetOutputFields(List<OutputField> outputFields)
        {
            _outputFields = outputFields;

            return this;
        }

        public ExportOptionsBuilder AddOutputField(OutputField outputField)
        {
            if (_outputFields == null)
            {
                _outputFields = new List<OutputField>();
            }

            _outputFields.Add(outputField);

            return this;
        }

        public ExportOptions Build()
        {
            ExportOptions result = new ExportOptions()
            {
                Channels = _channels,
                OutputFields = _outputFields,
                SequenceIndices = _sequenceIndices
            };

            return result;
        }
    }

    /// <summary>
    /// Enumerates the fields that can be included in the output
    /// </summary>
    public enum OutputField
    {
        TimeMillis,
        OutputValue,
        CardName,
        ChannelIndex,
        SequenceName,
        SequenceIndex
    }
}

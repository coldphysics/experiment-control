using AbstractController.Data.Card;
using AbstractController.Data.Channels;
using AbstractController.Data.Sequence;
using Controller.Common;
using Controller.MainWindow;
using Controller.OutputVisualizer.Export.Abstract;
using CustomElements.CheckableTreeView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Controller.OutputVisualizer.Export
{
    public class ExportWindowController : ChildController
    {
        public CTVViewModel SelectedChannelsTV { set; get; }

        public CTVViewModel SelectedSequencesTV { set; get; }

        public CTVViewModel SelectedColumnsTV { set; get; }

        public List<string> ExportFormats { set; get; } = new List<string>
        {
            "CSV"
        };

        public string SelectedExportFormat { set; get; }

        public ExportWindowController(OutputVisualizationWindowController parent)
            : base(parent)
        {
            SelectedChannelsTV = ModelBasedCTVBuilder.BuildCheckableTree(parent.GetRootController());
            CreateSequencesTV(parent);
            CreateColumnsTV();
            SelectedExportFormat = ExportFormats[0];
        }

        public ExportWindowController(OutputVisualizationWindowController parent, Dictionary<string, List<int>> initialSelectedChannels)
            : this(parent)
        {
            InitilizeSelectedChannelsTV(initialSelectedChannels);
        }

        /// <summary>
        /// Ensures that the set of channesl passed as argument is checked in the corresponding checkable tree view
        /// </summary>
        /// <param name="initialSelectedChannels"></param>
        private void InitilizeSelectedChannelsTV(Dictionary<string, List<int>> initialSelectedChannels)
        {
            // the tree starts with a virtual node
            // then all cards
            // then all channels
            List<List<CTVItemViewModel>> cardRootElements = SelectedChannelsTV
                .First()
                .Children
                .Select(tvi => tvi as CheckableTVItemController)
                .Where(tvi => initialSelectedChannels.Keys.Contains((tvi.Item as AbstractCardController).Model.Name))
                .Select(tvi => tvi.Children
                    .Where(childTvi =>
                    {
                        CheckableTVItemController ctviController = (CheckableTVItemController)childTvi;
                        AbstractChannelController channelController = (AbstractChannelController)ctviController.Item;
                        string cardName = channelController.Parent.Parent.Model.Name;
                        return (initialSelectedChannels[cardName].Contains(channelController.Index()));
                    })
                    .ToList())
                .ToList();

            foreach (List<CTVItemViewModel> channels in cardRootElements)
            {
                foreach (CTVItemViewModel channelElement in channels)
                {
                    channelElement.IsChecked = true;
                }
            }
        }

        /// <summary>
        /// Creates a checkable tree view containing the names (and indices) of the sequences of this model
        /// </summary>
        /// <param name="parent"></param>
        private void CreateSequencesTV(OutputVisualizationWindowController parent)
        {
            ICollection<AbstractSequenceController> sequences = parent.GetRootController().DataController.SequenceGroup.Windows.First().Tabs;
            string rootName = string.Format("Sequences:");
            CTVItemViewModel root = new CheckableTVItemController(rootName) { IsInitiallyExpanded = true };
            CTVItemViewModel currentSequence;

            foreach (AbstractSequenceController sequence in sequences)
            {
                currentSequence = new CheckableTVItemController(sequence);
                currentSequence.IsChecked = true;
                root.AddChild(currentSequence);
            }

            SelectedSequencesTV = new CTVViewModel
            {
                root
            };
        }

        /// <summary>
        /// Creates a checkable tree view containing the names of the columns of that will be exported
        /// </summary>
        private void CreateColumnsTV()
        {
            IEnumerable<OutputField> outputField = Enum
                .GetValues(typeof(OutputField))
                .Cast<OutputField>();

            string rootName = string.Format("Columns:");
            CTVItemViewModel root = new CheckableTVItemController(rootName) { IsInitiallyExpanded = true };
            CTVItemViewModel currentColumn;

            foreach (OutputField field in outputField)
            {
                currentColumn = new CheckableTVItemController(field);
                // add spaces to the enum name (see: https://stackoverflow.com/questions/5796383/insert-spaces-between-words-on-a-camel-cased-token)
                currentColumn.Name = Regex.Replace(field.ToString(), "(\\B[A-Z])", " $1");

                if (field == OutputField.CardName || field == OutputField.ChannelIndex)
                    currentColumn.IsChecked = true;

                root.AddChild(currentColumn);
            }

            SelectedColumnsTV = new CTVViewModel
            {
                root
            };
        }
    }
}

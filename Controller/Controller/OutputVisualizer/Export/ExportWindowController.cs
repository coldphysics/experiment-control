using AbstractController.Data.Card;
using AbstractController.Data.Channels;
using AbstractController.Data.Sequence;
using Communication.Commands;
using Communication.Interfaces.Generator;
using Controller.Common;
using Controller.MainWindow;
using Controller.OutputVisualizer.Export.Abstract;
using CustomElements.CheckableTreeView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shell;

namespace Controller.OutputVisualizer.Export
{
    /// <summary>
    /// The view model for the output export window
    /// </summary>
    public class ExportWindowController : ChildController
    {
        private bool _isExporting;
        public bool IsExporting
        {
            set
            {
                _isExporting = value;
                NotifyPropertyChanged("IsExporting");
            }
            get
            { return _isExporting; }
        }

        public CTVViewModel SelectedChannelsTV { set; get; }

        public CTVViewModel SelectedSequencesTV { set; get; }

        public CTVViewModel SelectedColumnsTV { set; get; }

        public List<string> ExportFormats { set; get; } = new List<string>
        {
            "CSV"
        };

        public RelayCommand ExportClickedCommand { private set; get; }

        public RelayCommand CancelClickedCommand { private set; get; }

        public string SelectedExportFormat { set; get; }

        public ExportWindowController(OutputVisualizationWindowController parent)
            : base(parent)
        {
            SelectedChannelsTV = ModelBasedCTVBuilder.BuildCheckableTree(parent.GetRootController());
            CreateSequencesTV(parent);
            CreateColumnsTV();
            SelectedExportFormat = ExportFormats[0];
            ExportClickedCommand = new RelayCommand(async (parameter) =>
            {
                await PerformExport(parameter);
            });
            CancelClickedCommand = new RelayCommand(Cancel);
        }

        public ExportWindowController(OutputVisualizationWindowController parent, Dictionary<string, List<int>> initialSelectedChannels)
            : this(parent)
        {
            InitilizeSelectedChannelsTV(initialSelectedChannels);
        }

        /// <summary>
        /// This constructor can be used if the set of selected channels is represented via a Checkable Tree View instance (<see cref="CTVViewModel"/>)
        /// </summary>
        /// <param name="parent">The parent controller</param>
        /// <param name="initialSelectedChannelTreeRoot"> The checkable tree view instance</param>
        public ExportWindowController(OutputVisualizationWindowController parent, CTVViewModel initialSelectedChannelTreeRoot)
            : this(parent, GetSelectedChannels(initialSelectedChannelTreeRoot))
        {
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

        private static Dictionary<string, List<int>> GetSelectedChannels(CTVViewModel root)
        {
            Dictionary<string, List<int>> result = new Dictionary<string, List<int>>();
            var leaves = root
                .GetCheckedLeaves()
                .Cast<CheckableTVItemController>()
                .Select(item => item.Item as AbstractChannelController);

            foreach (var cc in leaves)
            {
                string cardName = cc.Parent.Parent.Model.Name;

                if (!result.ContainsKey(cardName))
                {
                    result[cardName] = new List<int>();
                }

                result[cardName].Add(cc.Index());
            }

            return result;
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

                if (field == OutputField.TimeMillis || field == OutputField.OutputValue)
                    currentColumn.IsChecked = true;

                root.AddChild(currentColumn);
            }

            SelectedColumnsTV = new CTVViewModel
            {
                root
            };
        }


        private async Task PerformExport(object parameter)
        {
            // this prevents the export window from closing via the 'x' button while the export is running
            UserControl uc = (UserControl)parameter;
            Window w = Window.GetWindow(uc);
            w.Closing += (sender, args) =>
            {
                if (IsExporting)
                    args.Cancel = true;
            };


            // show a warning message
            if (Model.Properties.Settings.Default.ShowStopOutputOnExportHint)
            {
                CustomMessageBoxController customMessageBoxController = new CustomMessageBoxController(this);
                customMessageBoxController.Message = "The export operation may take a while. It is recommended to perform the export while the output is stopped";
                Window modalWindow = WindowsHelper.CreateWindowToHostViewModel(customMessageBoxController, true, true);
                modalWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                bool? dialogResult = modalWindow.ShowDialog();

                if (dialogResult.GetValueOrDefault(false))
                {
                    if (customMessageBoxController.DontShowAgain)
                    {
                        Model.Properties.Settings.Default.ShowStopOutputOnExportHint = false;
                        Model.Properties.Settings.Default.Save();
                    }
                }
                else
                {
                    return;
                }
            }

            // prepare configuration for output
            Dictionary<string, List<int>> channels = GetSelectedChannels(SelectedChannelsTV);

            List<OutputField> outputFields = SelectedColumnsTV
                .GetCheckedLeaves()
                .Cast<CheckableTVItemController>()
                .Select(leaf => (OutputField)leaf.Item)
                .ToList();
            List<int> sequenceIndices = SelectedSequencesTV
                .GetCheckedLeaves()
                .Cast<CheckableTVItemController>()
                .Select(leaf => (AbstractSequenceController)leaf.Item)
                .Select(c => c.Index())
                .ToList();

            ExportOptions options = ExportOptionsBuilder
                .NewInstance(channels)
                .SetOutputFields(outputFields)
                .SetSequenceIndices(sequenceIndices)
                .Build();

            IModelOutput modelOutput = ((OutputVisualizationWindowController)parent).LastKnownOutput;
            IList<AbstractSequenceController> allSequences = ((OutputVisualizationWindowController)parent)
                .GetRootController()
                .DataController
                .SequenceGroup
                .Windows
                .First()
                .Tabs;
            OutputExporter outputExporter = ExporterFactory.GetInstance().GetNewExporter(SelectedExportFormat, modelOutput, allSequences);
            bool performExport = false;

            // if the desired exporter is file-based, we open a save file dialog
            // other types of exporters should have other dialogs if necessary
            if (outputExporter is IFileBasedExporter)
            {
                string filter = string.Format("{0} Files (.{1})| *.{1}", SelectedExportFormat.ToUpper(), SelectedExportFormat.ToLower());
                string filePath = FileHelper.PickFilePath(SelectedExportFormat, filter);

                if (filePath != null)
                {
                    ((IFileBasedExporter)outputExporter).OutputPath = filePath;
                    performExport = true;
                }
            }

            if (performExport)
            {
                // asynchronously perform the output
                IsExporting = true;
                bool outcome = await outputExporter.ExportOutput(options);
                IsExporting = false;

                if (outcome)
                {
                    MessageBox.Show("The export operation was successful", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("The export operation failed (is the file open?)", "Failure", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            w.Close();
        }


        private void Cancel(object parameter)
        {
            UserControl uc = (UserControl)parameter;
            Window w = Window.GetWindow(uc);
            w.DialogResult = false;
            w.Close();
        }

    }
}

using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Communication.Commands;
using Controller.Data.Channels;
using Microsoft.Win32;
using Model.Data.Cards;
using Model.Data.Steps;
using Model.Settings;

namespace Controller.Data.Steps
{
    /// <summary>
    /// Controller for the steps that get their values from a file.
    /// </summary>
    /// <seealso cref="Controller.Data.Steps.StepBasicController" />
    public abstract class StepFileController : StepBasicController
    {
        // ******************** properties ********************        
        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        public StepFileModel Model
        {
            get
            {
                return (StepFileModel)_model;
            }
        }

        /// <summary>
        /// Gets or sets the analog type selected.
        /// </summary>
        /// <value>
        /// The analog type selected.
        /// </value>
        public override ChannelBasicController.AnalogTypes AnalogTypeSelected
        {
            get
            {
                return
                    (ChannelBasicController.AnalogTypes)
                    Enum.Parse(typeof(ChannelBasicController.AnalogTypes), Model.Store.ToString());
            }
            set
            {
                object token = _rootController.BulkUpdateStart();
                StepFileModel.StoreType store;

                if (Enum.TryParse(value.ToString(), out store))
                {
                    Model.Store = store;
                }
                else
                {
                    ((ChannelBasicController)Parent).ChangeStep(this, value.ToString());
                }

                ((ChannelBasicController)Parent).CopyToBuffer();
                _rootController.BulkUpdateEnd(token);
            }
        }

        /// <summary>
        /// Gets or sets the digital type selected.
        /// </summary>
        /// <value>
        /// The digital type selected.
        /// </value>
        public override ChannelBasicController.DigitalTypes DigitalTypeSelected
        {
            get
            {
                return
                    (ChannelBasicController.DigitalTypes)
                    Enum.Parse(typeof(ChannelBasicController.DigitalTypes), Model.Store.ToString());
            }
            set
            {
                StepFileModel.StoreType store;
                object token = _rootController.BulkUpdateStart();

                if (Enum.TryParse(value.ToString(), out store))
                {
                    Model.Store = store;
                }
                else
                {
                    ((ChannelBasicController)Parent).ChangeStep(this, value.ToString());
                }

                ((ChannelBasicController)Parent).CopyToBuffer();
                _rootController.BulkUpdateEnd(token);
            }
        }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName
        {
            get { return Model.FileName; }
            set { Model.FileName = value; }
        }


        // ******************** events ********************        
        /// <summary>
        /// Gets the open file dialog command
        /// </summary>
        /// <value>
        /// The open file dialog command.
        /// </value>
        public ICommand OpenFileDialog { get; private set; }



        // ******************** constructor ********************        
        /// <summary>
        /// Initializes a new instance of the <see cref="StepFileController"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="parent">The parent.</param>
        public StepFileController(StepFileModel model, ChannelBasicController parent) : base(parent, model)
        {
            OpenFileDialog = new RelayCommand(OpenFile);
        }

        /// <summary>
        /// Executed when the open file command is triggerd
        /// </summary>
        /// <param name="sender">The sender.</param>
        private void OpenFile(object sender)
        {
            OpenFileDialog fileDialog = null;
            bool csv = false;
            bool binary = false;
            if (DigitalTypeSelected == ChannelBasicController.DigitalTypes.Csv ||
                AnalogTypeSelected == ChannelBasicController.AnalogTypes.Csv)
            {
                fileDialog = new OpenFileDialog {DefaultExt = ".csv", Filter = "Comma separated (.csv)|*.csv"};
                csv = true;
            }

            if (DigitalTypeSelected == ChannelBasicController.DigitalTypes.Binary ||
                AnalogTypeSelected == ChannelBasicController.AnalogTypes.Binary)
            {
                fileDialog = new OpenFileDialog {DefaultExt = ".bin", Filter = "Binary (.bin)|*.bin"};
                binary = true;
            }

            bool? result = fileDialog.ShowDialog();
            if (result == true)
            {
                string fileName = fileDialog.FileName;
                int length = 0;
                double value = 0;

                if (csv)
                {
                    length = File.ReadAllLines(fileName).Length;
                    value = Convert.ToDouble(File.ReadLines(fileName).Last());
                }

                if (binary)
                {
                    var stream = new FileStream(fileName, FileMode.Open);
                    var byteLength = (int) stream.Length;
                    if (CardType() == CardBasicModel.CardType.Analog)
                    {
                        length = (byteLength / sizeof(float));
                        stream.Seek(-sizeof (float), SeekOrigin.End);
                        var br = new BinaryReader(stream);
                        value = br.ReadSingle();
                        br.Close();
                    }

                    if (CardType() == CardBasicModel.CardType.Digital)
                    {
                        length = (byteLength/sizeof(byte));
                        stream.Seek(-sizeof(byte), SeekOrigin.End);
                        var br = new BinaryReader(stream);
                        value = br.ReadByte();
                        br.Close();
                    }
                    stream.Close();
                }

                Model.FileName = fileName;
                Duration = length * TimeSettingsInfo.GetInstance().SmallestTimeStep;
                Value = value;
                UpdateProperty("StartTime");
                UpdateProperty("Duration");
                UpdateProperty("FileName");
                UpdateProperty("Value");
                ((ChannelBasicController)Parent).CopyToBuffer();
            }
        }


        /// <summary>
        /// Gets the name of the duration variable.
        /// </summary>
        /// <returns>
        /// An empty string
        /// </returns>
        protected override string GetDurationVariableName()
        {
            return "";
            //throw new Exception("There are no Varialbes allowed in StepFileController! (Controller-Data-Steps-StepFileController.cs)");
            //return _model.DurationVariableName;
        }

        /// <summary>
        /// Sets the name of the duration variable.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <exception cref="System.Exception">There are no Variables allowed in StepFileController! (Controller-Data-Steps-StepFileController.cs)</exception>
        protected override void SetDurationVariableName(string value)
        {
            throw new Exception("There are no Variables allowed in StepFileController! (Controller-Data-Steps-StepFileController.cs)");
            //_model.DurationVariableName = value;
            //((ChannelBasicController)Parent).CopyToBuffer();
        }

        /// <summary>
        /// Gets the name of the value variable.
        /// </summary>
        /// <returns>
        /// An empty string
        /// </returns>
        protected override string GetValueVariableName()
        {
            return "";
            //throw new Exception("There are no Varialbes allowed in StepFileController! (Controller-Data-Steps-StepFileController.cs)");
            //return _model.ValueVariableName;
        }
        /// <summary>
        /// Sets the name of the value variable.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <exception cref="System.Exception">There are no Variables allowed in StepFileController! (Controller-Data-Steps-StepFileController.cs)</exception>
        protected override void SetValueVariableName(string value)
        {
            throw new Exception("There are no Variables allowed in StepFileController! (Controller-Data-Steps-StepFileController.cs)");
            //_model.ValueVariableName = value;
            //((ChannelBasicController)Parent).CopyToBuffer();
        }
    }
}
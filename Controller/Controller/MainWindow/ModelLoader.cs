using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Model.Data.Cards;
using Model.Data.Channels;
using Model.Data.SequenceGroups;
using Model.Data.Sequences;
using Model.Root;

namespace Controller.MainWindow
{
    /// <summary>
    /// Arguments for the <see cref=" ModelStructureMismatchDelegate"/> delegate
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ModelStructureMismatchEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the number of analog cards.
        /// </summary>
        /// <value>
        /// The model analog cards.
        /// </value>
        public int ModelAnalogCards { set; get; }

        /// <summary>
        /// Gets or sets the number of digital cards.
        /// </summary>
        /// <value>
        /// The model digital cards.
        /// </value>
        public int ModelDigitalCards { set; get; }

        /// <summary>
        /// Gets or sets the number of analog channels per card.
        /// </summary>
        /// <value>
        /// The model analog channels per card.
        /// </value>
        public int ModelAnalogChannelsPerCard { set; get; }

        /// <summary>
        /// Gets or sets the the number of channels per card.
        /// </summary>
        /// <value>
        /// The model digital channels per card.
        /// </value>
        public int ModelDigitalChannelsPerCard { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether the loading operation should be canceled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if cancel; otherwise, <c>false</c>.
        /// </value>
        public bool Cancel { set; get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelStructureMismatchEventArgs"/> class.
        /// </summary>
        public ModelStructureMismatchEventArgs()
        {
            Cancel = false;
        }
    }

    /// <summary>
    /// A delegate for the event that gets triggered when the <see cref=" ModelLoader"/> class detects that the structure of model to be loaded does
    /// not match with the expected structure specified by the settings.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="ModelStructureMismatchEventArgs"/> instance containing the event data.</param>
    public delegate void ModelStructureMismatchDelegate(object sender, ModelStructureMismatchEventArgs e);

    /// <summary>
    /// Arguments for the <see cref=" ModelVersionMismatchDelegate"/> delegate
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ModelVersionMismatchEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the current version.
        /// </summary>
        /// <value>
        /// The current version.
        /// </value>
        public int CurrentVersion { set; get; }

        /// <summary>
        /// Gets or sets the model version.
        /// </summary>
        /// <value>
        /// The model version.
        /// </value>
        public int ModelVersion { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether the loading operation should be canceled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if cancel; otherwise, <c>false</c>.
        /// </value>
        public bool Cancel { set; get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelVersionMismatchEventArgs"/> class.
        /// </summary>
        public ModelVersionMismatchEventArgs()
        {
            Cancel = false;
        }
    }


    //Ebaa 1.10.2018
    //public class PythonScriptMismatchEventargs: EventArgs
    //{
    //    public bool mustSpecifyPyhtonScripts
    //    {
    //        set;
    //        get;
    //    }

    //    public PythonScriptMismatchEventargs()
    //    {
    //        mustSpecifyPyhtonScripts= false;
    //    }

    //}
    /// <summary>
    /// A delegate for the event that gets triggered when the <see cref=" ModelLoader"/> class detects that the version of model to be loaded does
    /// not match with current model version.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="ModelVersionMismatchEventArgs"/> instance containing the event data.</param>
    public delegate void ModelVersionMismatchDelegate(object sender, ModelVersionMismatchEventArgs e);

    //Ebaa 1.10.2018
    public delegate void PythonScriptMismatchDelegate(object sender, ModelVersionMismatchEventArgs e);

    /// <summary>
    /// Loads a model file into memory
    /// </summary>
    public class ModelLoader
    {
        private const string ACTIVE_MODEL_XML_SCHEMA = "http://schemas.datacontract.org/2004/07/Model";
        private const string V1_MODEL_XML_SCHEMA = "http://schemas.datacontract.org/2004/07/Model.V1";
        private const string V2_MODEL_XML_SCHEMA = "http://schemas.datacontract.org/2004/07/Model.V2";
        private const string V3_MODEL_XML_SCHEMA = "http://schemas.datacontract.org/2004/07/Model.V3";
        private const string V4_MODEL_XML_SCHEMA = "http://schemas.datacontract.org/2004/07/Model.V4";
        // The first two versions had a map of sequence groups. When this map is serialized, for some reason,
        // its type contains a string that depends (somehow) on the namespace of the key and value.
        // Since these are changed due to versioning, e.g., V1. is added to all types, the old serialized models
        // cannot be deserialized into version specific models (nor the active model). Therefore, this had to be
        // fixed manually. However, since version 3, the map is dropped from the model, so this became irrelevant.
        private const string ACTIVE_MODEL_CRYPT = "KeyValueOfstringSequenceGroupModel3tbQ1peO";
        private const string V1_MODEL_CRYPT = "KeyValueOfstringSequenceGroupModelEC1jtckR";
        private const string V2_MODEL_CRYPT = "KeyValueOfstringSequenceGroupModeluFmvqGmx";
        private const string MODEL_VERSION_PATTERN = "<MODEL_VERSION>(\\d+)</MODEL_VERSION>";

        /// <summary>
        /// Occurs when the structure of the model does not match with the settings.
        /// </summary>
        public event ModelStructureMismatchDelegate ModelStructureMismatchDetected;

        /// <summary>
        /// Occurs when the version of the model does not match with the current model version.
        /// </summary>
        public event ModelVersionMismatchDelegate ModelVersionMismatchDetected;

        //Ebaa 1.10.2018
        public event PythonScriptMismatchDelegate PythonScriptMismatchDetected;

        /// <summary>
        /// Raises the <see cref="E:ModelStructureMismatch" /> event.
        /// </summary>
        /// <param name="args">The <see cref="ModelStructureMismatchEventArgs"/> instance containing the event data.</param>
        protected void OnModelStructureMismatch(ModelStructureMismatchEventArgs args)
        {
            if (ModelStructureMismatchDetected != null)
                ModelStructureMismatchDetected(this, args);
        }

        /// <summary>
        /// Raises the <see cref="E:ModelVersionMismatch" /> event.
        /// </summary>
        /// <param name="args">The <see cref="ModelVersionMismatchEventArgs"/> instance containing the event data.</param>
        protected void OnModelVersionMismatch(ModelVersionMismatchEventArgs args)
        {
            if (ModelVersionMismatchDetected != null)
                ModelVersionMismatchDetected(this, args);
        }


        //Ebaa 1.10.2018
        protected void OnPythonScriptsMismatch(ModelVersionMismatchEventArgs args)
        {
            if (PythonScriptMismatchDetected != null)
                PythonScriptMismatchDetected(this, args);
            //  PythonScriptMismatchDetected?.Invoke(this, args);
        }

        #region Structure Conversion

        private RootModel ConvertModelStructureIfNecessary(RootModel loadedModel)
        {
            int intendedAnalogCardsCount = Global.GetNumAnalogCards();
            int intendedDigitalCardsCount = Global.GetNumDigitalCards();
            int intendedAnalogChannelsCount = Global.GetNumAnalogChannelsPerCard();
            int intendedDigitalChannelsCount = Global.GetNumDigitalChannelsPerCard();

            int loadedModelAnalogCards =
                loadedModel.Data.group.Cards.Count(param =>
                    param.Type == Model.Data.Cards.CardBasicModel.CardType.Analog);
            int loadedModelDigitalCards = loadedModel.Data.group.Cards.Count(param =>
                param.Type == Model.Data.Cards.CardBasicModel.CardType.Digital);
            int loadedModelAnalogChannels = 0;
            int loadedModelDigitalChannels = 0;

            CardBasicModel firstAnalogCard = loadedModel.Data.group.Cards.FirstOrDefault(param =>
                param.Type == Model.Data.Cards.CardBasicModel.CardType.Analog);

            if (firstAnalogCard != null)
            {
                loadedModelAnalogChannels = (int) firstAnalogCard.NumberOfChannels;
            }

            CardBasicModel firstDigitalCard = loadedModel.Data.group.Cards.FirstOrDefault(param =>
                param.Type == Model.Data.Cards.CardBasicModel.CardType.Digital);

            if (firstDigitalCard != null)
            {
                loadedModelDigitalChannels = (int) firstDigitalCard.NumberOfChannels;
            }


            if (intendedAnalogCardsCount != loadedModelAnalogCards ||
                intendedAnalogChannelsCount != loadedModelAnalogChannels ||
                intendedDigitalCardsCount != loadedModelDigitalCards ||
                intendedDigitalChannelsCount != loadedModelDigitalChannels)
            {
                ModelStructureMismatchEventArgs args = new ModelStructureMismatchEventArgs
                {
                    ModelAnalogCards = loadedModelAnalogCards,
                    ModelAnalogChannelsPerCard = loadedModelAnalogChannels,
                    ModelDigitalCards = loadedModelDigitalCards,
                    ModelDigitalChannelsPerCard = loadedModelDigitalChannels
                };

                OnModelStructureMismatch(args);

                if (args.Cancel)
                    return null;

                FixNumberOfCards(loadedModel, intendedAnalogCardsCount, loadedModelAnalogCards,
                    CardBasicModel.CardType.Analog, CardBasicModel.ANALOG_CARD_BASE_NAME);

                FixNumberOfCards(loadedModel, intendedDigitalCardsCount, loadedModelDigitalCards,
                    CardBasicModel.CardType.Digital, CardBasicModel.DIGITAL_CARD_BASE_NAME);

                foreach (CardBasicModel currentCard in loadedModel.Data.group.Cards)
                {
                    if (currentCard.Type == CardBasicModel.CardType.Analog)
                        FixNumberOfChannels(currentCard, intendedAnalogChannelsCount, loadedModelAnalogChannels);
                    else
                        FixNumberOfChannels(currentCard, intendedDigitalChannelsCount, loadedModelDigitalChannels);
                }
            }

            return loadedModel;
        }

        private void FixNumberOfChannels(CardBasicModel currentCard, int intendedChannelsCount, int loadedChannelsCount)
        {
            //We need to add channels
            if (loadedChannelsCount < intendedChannelsCount)
            {
                //Add channel settings
                for (int i = 1; i <= intendedChannelsCount - loadedChannelsCount; i++)
                    currentCard.Settings.Add(
                        new Model.Data.Channels.ChannelSettingsModel(loadedChannelsCount + i, currentCard));

                //Add channels
                foreach (SequenceModel currentSequnce in currentCard.Sequences)
                {
                    for (int i = 1; i <= intendedChannelsCount - loadedChannelsCount; i++)
                        currentSequnce.Channels.Add(new ChannelModel(currentSequnce));
                }
            }
            else if (loadedChannelsCount > intendedChannelsCount) //We need to remove channels
            {
                int TO_REMOVE = loadedChannelsCount - intendedChannelsCount;
                int removedCounter = TO_REMOVE;

                //Remove channel settings
                for (int i = currentCard.Settings.Count - 1; i >= 0; i--)
                {
                    currentCard.Settings.RemoveAt(i);
                    removedCounter--;

                    if (removedCounter == 0)
                        break;
                }

                //Remove channels
                foreach (SequenceModel currnetSequence in currentCard.Sequences)
                {
                    removedCounter = TO_REMOVE;

                    for (int i = currnetSequence.Channels.Count - 1; i >= 0; i--)
                    {
                        currnetSequence.Channels.RemoveAt(i);

                        removedCounter--;

                        if (removedCounter == 0)
                            break;
                    }
                }
            }
        }

        private void FixNumberOfCards(RootModel loadedModel, int intendedCardCount, int loadedCardCount,
            CardBasicModel.CardType type, string cardBasicName)
        {
            CardBasicModel currentCard;
            SequenceGroupModel sg = loadedModel.Data.group;
            ObservableCollection<CardBasicModel> cards = loadedModel.Data.group.Cards;
            int channelsCount = 0;
            CardBasicModel firstCard = cards.FirstOrDefault(param => param.Type == type);

            if (firstCard != null)
                channelsCount = (int) firstCard.NumberOfChannels;

            //We need to add cards!
            if (loadedCardCount < intendedCardCount)
            {
                int position;

                for (int i = 1; i <= intendedCardCount - loadedCardCount; i++)
                {
                    currentCard = CreateNewCard(loadedModel, intendedCardCount, loadedCardCount, channelsCount, type,
                        cardBasicName, i);

                    if (type == CardBasicModel.CardType.Analog)
                        position = loadedCardCount + i - 1;
                    else
                        position = cards.Count;

                    cards.Insert(position, currentCard);
                }
            }
            else if (loadedCardCount > intendedCardCount) //We need to remove cards
            {
                int toRemove = loadedCardCount - intendedCardCount;

                for (int i = cards.Count - 1; i >= 0; i--)
                {
                    if (cards[i].Type == type)
                    {
                        cards.RemoveAt(i);
                        toRemove--;
                    }

                    if (toRemove == 0)
                        break;
                }
            }
        }

        private CardBasicModel CreateNewCard(RootModel loadedModel, int intendedCardCount, int loadedCardCount,
            int intendedChannelCount, CardBasicModel.CardType type, string cardBasicName, int cardPosition)
        {
            ObservableCollection<SequenceModel> firstCardSequences = loadedModel.Data.group.Cards.First().Sequences;


            //Create a new card
            string currentName =
                String.Format("{0}{1}", cardBasicName,
                    loadedCardCount + cardPosition); //RECO find out whether starting index is being used
            CardBasicModel currentCard =
                new CardModel(currentName, (uint) intendedChannelCount, type, loadedModel.Data.group);

            for (int iChannel = 0; iChannel < intendedChannelCount; iChannel++)
            {
                ChannelSettingsModel setting = new ChannelSettingsModel(iChannel, currentCard);
                currentCard.Settings.Add(setting);
            }

            SequenceModel currentSequence;
            //Build empty sequences for the new card (based on the sequences of the first card)
            for (int i = 0; i < firstCardSequences.Count; i++)
            {
                currentSequence = CreateNewSequence(currentCard, firstCardSequences[i].Name,
                    firstCardSequences[i].startTime, intendedChannelCount);
                currentCard.Sequences.Add(currentSequence);
            }

            return currentCard;
        }

        private SequenceModel CreateNewSequence(CardBasicModel currentCard, string name, double startTime,
            int intendedChannelCount)
        {
            SequenceModel currentSequence = new SequenceModel(currentCard);

            for (int channelIndex = 1; channelIndex <= intendedChannelCount; channelIndex++)
            {
                ChannelModel newChannel = new ChannelModel(currentSequence);

                currentSequence.Channels.Add(newChannel);
            }

            currentSequence.Name = name;
            currentSequence.startTime = startTime;

            return currentSequence;
        }

        #endregion

        #region Version Conversion

        /// <summary>
        /// Detects the version of the loaded model, and if it needs an "upgrade" prepares it and upgrades it to the current model version.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        private RootModel ConvertModelVersionIfNecessary(string fileName)
        {
            RootModel result = null;

            using (FileStream stream = new FileStream(fileName, FileMode.Open))
            {
                using (GZipStream gz = new GZipStream(stream, CompressionMode.Decompress, false))
                {
                    StreamReader stringReader = new StreamReader(gz);
                    string xml = stringReader.ReadToEnd();
                    XmlDictionaryReader reader = null;
                    Regex matcher = new Regex(MODEL_VERSION_PATTERN, RegexOptions.Compiled);
                    Match match = matcher.Match(xml);

                    //It is version 1!
                    if (!match.Success)
                    {
                        ModelVersionMismatchEventArgs args = new ModelVersionMismatchEventArgs
                        {
                            CurrentVersion = (new RootModel()).MODEL_VERSION,
                            ModelVersion = 1
                        };

                        //Ebaa 1.10.2018
                        //PythonScriptMismatchEventargs pythonAgs = new PythonScriptMismatchEventargs
                        //{
                        //    mustSpecifyPyhtonScripts = true
                        //};


                        OnModelVersionMismatch(args);

                        if (args.Cancel)
                            return null;

                        xml = xml.Replace(ACTIVE_MODEL_XML_SCHEMA, V1_MODEL_XML_SCHEMA);

                        //RECO find more dynamic way of figuring this out!
                        xml = xml.Replace(ACTIVE_MODEL_CRYPT, V1_MODEL_CRYPT);
                        reader = XmlDictionaryReader.CreateTextReader(GenerateStreamFromString(xml),
                            new XmlDictionaryReaderQuotas());

                        var deserializer = new DataContractSerializer(typeof(Model.V1.Root.RootModel));
                        Model.V1.Root.RootModel model = (Model.V1.Root.RootModel) deserializer.ReadObject(reader, true);
                        Model.V1.ModelConverter converter = new Model.V1.ModelConverter();
                        result = converter.ConvertToCurrentVersion(model);
                    }
                    else
                    {
                        int modelVersion = Int32.Parse(match.Groups[1].Value);

                        ModelVersionMismatchEventArgs args = new ModelVersionMismatchEventArgs
                        {
                            CurrentVersion = (new RootModel()).MODEL_VERSION,
                            ModelVersion = modelVersion
                        };
                        OnModelVersionMismatch(args);

                        if (args.Cancel)
                            return null;

                        //It is version 2!
                        if (modelVersion == 2)
                        {
                            xml = xml.Replace(ACTIVE_MODEL_XML_SCHEMA, V2_MODEL_XML_SCHEMA);

                            //RECO find more dynamic way of figuring this out!
                            xml = xml.Replace(ACTIVE_MODEL_CRYPT, V2_MODEL_CRYPT);
                            reader = XmlDictionaryReader.CreateTextReader(GenerateStreamFromString(xml),
                                new XmlDictionaryReaderQuotas());

                            var deserializer = new DataContractSerializer(typeof(Model.V2.Root.RootModel));
                            Model.V2.Root.RootModel model =
                                (Model.V2.Root.RootModel) deserializer.ReadObject(reader, true);
                            Model.V2.ModelConverter converter = new Model.V2.ModelConverter();
                            result = converter.ConvertToCurrentVersion(model);
                        }
                        else if (modelVersion == 3) //It is version 3
                        {
                            xml = xml.Replace(ACTIVE_MODEL_XML_SCHEMA, V3_MODEL_XML_SCHEMA);

                            reader = XmlDictionaryReader.CreateTextReader(GenerateStreamFromString(xml),
                                new XmlDictionaryReaderQuotas());

                            var deserializer = new DataContractSerializer(typeof(Model.V3.Root.RootModel));
                            Model.V3.Root.RootModel model =
                                (Model.V3.Root.RootModel) deserializer.ReadObject(reader, true);
                            Model.V3.ModelConverter converter = new Model.V3.ModelConverter();
                            result = converter.ConvertToCurrentVersion(model);
                        }
                        else if (modelVersion == 4) // It is version 4
                        {
                            xml = xml.Replace(ACTIVE_MODEL_XML_SCHEMA, V4_MODEL_XML_SCHEMA);

                            reader = XmlDictionaryReader.CreateTextReader(GenerateStreamFromString(xml),
                                new XmlDictionaryReaderQuotas());

                            var deserializer = new DataContractSerializer(typeof(Model.V4.Root.RootModel));
                            Model.V4.Root.RootModel model =
                                (Model.V4.Root.RootModel) deserializer.ReadObject(reader, true);
                            Model.V4.ModelConverter converter = new Model.V4.ModelConverter();
                            result = converter.ConvertToCurrentVersion(model);
                            // MessageBox.Show("You are using an old version of the model"+"\n"+"In this version python scripts are model-specific"+"\n"+"You can add pzthon scripts from (python scripts) button"+"\n"+"Note: In order to modify python scripts, load the model as a primary model","Warning", MessageBoxButton.OK);
                        }
                        else //It is the current version!
                        {
                            reader = XmlDictionaryReader.CreateTextReader(GenerateStreamFromString(xml),
                                new XmlDictionaryReaderQuotas());
                            var deserializer = new DataContractSerializer(typeof(RootModel));
                            result = (RootModel) deserializer.ReadObject(reader, true);
                        }

                        if (modelVersion < 5)
                        {
                            OnPythonScriptsMismatch(args);
                        }
                    }
                }
            }

            return result;
        }

        private static MemoryStream GenerateStreamFromString(string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));
        }

        //private void SaveDummy()
        //{
        //    Model.V2.Root.RootModel result = new Model.V2.Root.RootModel(Global.Experiment);
        //    result.Data.Groups.Add("test", null);
        //    var writer = new FileStream("d:\\tessst.xml", FileMode.Create);
        //    Type type = result.GetType();
        //    var serializer = new DataContractSerializer(type);
        //    serializer.WriteObject(writer, result);
        //    writer.Close();
        //}

        #endregion

        /// <summary>
        /// Loads the model.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public RootModel LoadModel(string fileName)
        {
            RootModel model = ConvertModelVersionIfNecessary(fileName);

            if (model != null)
            {
                model = ConvertModelStructureIfNecessary(model);
            }

            return model;
        }
    }
}
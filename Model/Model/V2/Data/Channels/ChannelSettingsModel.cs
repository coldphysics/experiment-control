using Model.V2.Data.Cards;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Model.V2.Data.Channels
{
    //RECO split into a parent settings (Name) digital settings (Invert) and analog settings (Rest)

    /// <summary>
    /// Represents the Model part of the MVVM pattern for digital and analog channel settings.
    /// The pattern contains this class as the Model, the <see cref=" Controller.Data.Channels.ChannelSettingsController"/> as the ModelView and
    /// both <see cref=" Controller.Data.Channels.AnalogSettingsWindow"/> and <see cref=" Controller.Data.Channels.DigitalSettingsWindow"/> as the View
    /// </summary>
    [Serializable]
    [DataContract]
    public class ChannelSettingsModel
    {
        #region fields and properties
        /// <summary>
        /// A reference to the <see cref=" CardBasicModel"/> which holds the collection of all <c>ChannelSettingsModel</c>.
        /// It is used to find the index of this object in the collection of all <c>ChannelSettingsModel</c> objects
        /// </summary>
        [DataMember] private readonly CardBasicModel _parent;
         //RECO remove Card field; it is not used nor useful        
        /// <summary>
        /// Unused
        /// </summary>
        [DataMember] public int Card;
        /// <summary>
        /// The initial value of the channel in an analog card for all <see cref=" Model.Data.Sequences.SequenceModel"/>s
        /// </summary>
        [DataMember] public double InitValue;
        /// <summary>
        /// The lower limit value of the channel in an analog card for all <see cref=" Model.Data.Sequences.SequenceModel"/>s
        /// </summary>
        [DataMember] public double LowerLimit = -10;
        /// <summary>
        /// The name of the channel (for all <see cref=" Model.Data.Sequences.SequenceModel"/>s)
        /// </summary>
        [DataMember] public string Name;
        /// <summary>
        /// The upper limit value of the channel in an analog card for all <see cref=" Model.Data.Sequences.SequenceModel"/>s
        /// </summary>
        [DataMember] public double UpperLimit = 10;
        /// <summary>
        /// The offset value of the channel in an analog card for all <see cref=" Model.Data.Sequences.SequenceModel"/>s
        /// </summary>
        //[DataMember] public double OffsetValue = 0;
        /// <summary>
        /// The multiplicator value of the channel in an analog card for all <see cref=" Model.Data.Sequences.SequenceModel"/>s
        /// </summary>
        //[DataMember] public double MultiplicatorValue = 1;
        /// <summary>
        /// Indicates whether the channel's signal should be inverted (in a digital card in for all <see cref=" Model.Data.Sequences.SequenceModel"/>s)
        /// </summary>
        [DataMember] public bool Invert = false;
        /// <summary>
        /// The voltage unit value of the channel in an analog card for all <see cref=" Model.Data.Sequences.SequenceModel"/>s
        /// </summary>
        [DataMember]
        public string InputUnit = "V";


        /// <summary>
        /// The index of the channel this settings object is associated to. (measured starting from the startIndex which could be more than 0)
        /// </summary>
        [DataMember] public int Channel { get; private set; }


        //ADDED Ghareeb 18.11.2016 THIS CHANGES THE MODEL!        
        /// <summary>
        /// Gets or sets the pyhton calibration script.
        /// </summary>
        /// <value>
        /// The python calibration script.
        /// </value>
        [DataMember]
        public string CalibrationScript { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether  to use calibration.
        /// </summary>
        /// <value>
        ///   <c>true</c> if calibration is used; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool UseCalibration { set; get; }
        #endregion


        //RECO remove the usage of defaultValues.xml
        //RECO this constructor contains logical errors which can lead to unexpected results
        /// <summary>
        /// Initializes a new instance of this class.
        /// It tries to read the default values for this channel from an XML file and if the file doesn't exist it creates it and adds a record to it.
        /// </summary>
        /// <param name="channel">The index of the <see cref=" ChannelBasicModel"/> this object is associated to</param>
        /// <param name="parent">The <see cref=" CardBasicModel"/> that holds the collection of all <c>ChannelSettingsModel</c>s </param>
        public ChannelSettingsModel(int channel, CardBasicModel parent)
        {
            _parent = parent;
            Name = "Channel " + Convert.ToString(channel) + ":";
            Channel = channel;

            //load default Values

            string stringIdentifier = _parent.Name + "-" + this.Channel;//e.g. A3-5
            
            //The key is the stringIdentifier of the settings object (e.g. A3-5) and the value is the default values for some fields
            Dictionary<string, ChannelSettingsSaveValues> defaultValues = new Dictionary<string, ChannelSettingsSaveValues>();
            
            //RECO file paths should not be hard-coded. At least use a constant. Better use a resource
            if (File.Exists("defaultValues.xml"))//If the default settings file is found read it entirely (this will read settings for all channels)!!
            {
                var stream = new FileStream("defaultValues.xml", FileMode.Open);
                var reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas());
                var deserializer = new DataContractSerializer(typeof(Dictionary<string, ChannelSettingsSaveValues>));
                //System.Console.Write("a\n");
                defaultValues = (Dictionary<string, ChannelSettingsSaveValues>)deserializer.ReadObject(reader, true);
                stream.Close();
            }
            else//If the file is not found then create it with no values
            {
                var writer = new FileStream("defaultValues.xml", FileMode.Create);
                Type type = defaultValues.GetType();
                var serializer = new DataContractSerializer(type);
                serializer.WriteObject(writer, defaultValues);
                writer.Close();
            }

            //RECO read more values from the default XML file
            //If the defaultValues read from previous step contains the stringIdentifier of the settings read the stored values
            if (defaultValues.ContainsKey(stringIdentifier))
            {
                UpperLimit = defaultValues[stringIdentifier].Max;
                LowerLimit = defaultValues[stringIdentifier].Min;
                Name = defaultValues[stringIdentifier].Name;
            }
            else//If stringIdentifier is not found then store the default values assigned in this class declaration in the xml file
            {
                ChannelSettingsSaveValues channelSettingsSaveValues = new ChannelSettingsSaveValues();
                channelSettingsSaveValues.Max = UpperLimit;
                channelSettingsSaveValues.Min = LowerLimit;
                channelSettingsSaveValues.Name = Name;
                defaultValues.Add(stringIdentifier, channelSettingsSaveValues);

                var writer = new FileStream("defaultValues.xml", FileMode.Create);//RECO This might remove the file and create a new one.. better append!
                Type type = defaultValues.GetType();
                var serializer = new DataContractSerializer(type);
                serializer.WriteObject(writer, defaultValues);
                writer.Close();
            }
            
        }

        /// <summary>
        /// Gets the index of this settings object in the collection of all channel settings objects which is stored in the <see cref=" Model.Data.Cards.CardBasicModel"/>
        /// </summary>
        /// <returns>The index of this settings object in the collection of all channel settings objects which is stored in the <see cref=" Model.Data.Cards.CardBasicModel"/></returns>
        public int Index()
        {
            return _parent.Settings.IndexOf(this);
        }

        /// <summary>
        /// Makes a shallow copy of this object.
        /// </summary>
        /// <returns>A shallow copy of this object.</returns>
        public ChannelSettingsModel ShallowCopy()
        {
            return (ChannelSettingsModel)MemberwiseClone();
        }
    }
}
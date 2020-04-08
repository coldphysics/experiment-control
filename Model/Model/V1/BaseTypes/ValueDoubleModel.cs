using System;
using System.Runtime.Serialization;

namespace Model.V1.BaseTypes
{
    //RECO investigate the usefulness of this class

    /// <summary>
    /// Defines the signature of methods used to handle the changed events.
    /// </summary>
    /// <param name="sender">The PostingEvents object which fires the event.</param>
    /// <param name="e">Event data identifying the object that was changed</param>
    public delegate void ChangedEventHandler(object sender, EventArgs e);

    /// <summary>
    /// A container class for a string name and a double value, with an event triggered when any value gets changed
    /// </summary>
    [Serializable]
    [DataContract]
    public class ValueDoubleModel
    {
        /// <summary>
        /// The name of this value. Not used in practice
        /// </summary>
        private string _name;
        /// <summary>
        /// The value of this variable
        /// </summary>
        [DataMember]
        private double _value;
        /// <summary>
        /// An event which gets triggered whenever the name or the value of this class get changed.
        /// Not used in practice
        /// </summary>
        [field: NonSerialized]
        public event ChangedEventHandler Changed;

        /// <summary>
        /// Sets or gets the name.
        /// An event is triggered whenever the name is set.
        /// Not used in practice
        /// </summary>
        [DataMember]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;

                if (Changed != null)
                    Changed(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Sets or gets the value.
        /// An event is triggered whenever the value is set.
        /// </summary>
        public double Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;

                if (Changed != null)
                    Changed(this, EventArgs.Empty);
            }
        }


        //public Type Type
        //{
        //    get { return _type; }
        //    set
        //    {
        //        _type = value;
        //        if (Changed != null)
        //        {
        //            Changed(this, EventArgs.Empty);
        //        }
        //    }
        //}
    }
}
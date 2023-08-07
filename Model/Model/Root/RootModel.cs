using Communication.Interfaces.Model;
using Model.Data;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;


namespace Model.Root
{
    //RECO encapsulate fields into properties
    //RECO use Composite Design Pattern instead of the explicit has-a relationship
    //RECO could be useless;integrate with DataModel
    //CONSEQ take care of serializable hierarchy

    /// <summary>
    /// General container for the entire data model, also contains the global counter
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class RootModel : IModel
    {
      
        //ADDED Ghareeb 18.11.2016 to distinguish versions    
        //Modified Ebaa 18.09.2018 to a new version (version 5)
        /// <summary>
        /// The version of the model
        /// </summary>
        [DataMember]
        public readonly int MODEL_VERSION = 5;

        /// <summary>
        /// Represents the child model of the root model in the hierarchy
        /// </summary>
        [DataMember] public readonly DataModel Data;//RECO read-only variable is preferred to be capitalized
        /// <summary>
        /// Theoretical deadline of the beginning of the sequence cycle (estimated by the previous cycle)
        /// Is maintained and used by the Buffer.Basic.OutputHandler/>
        /// </summary>
        public DateTime EstimatedStartTime;
        /// <summary>
        /// Specifies whether the iteration mode is enabled
        /// Is maintained and used by the Buffer.Basic.OutputHandler/>
        /// </summary>
        public bool IsItererating;
        /// <summary>
        /// Unique key triggered by a variable (iterator, static, dynamic) change. It 
        /// is maintained and used by the Buffer.Basic.OutputHandler/>
        /// </summary>
        public int GlobalCounter;
        /// <summary>
        /// Contains the names and values of all variables used to be written to the database and to a text file.
        /// Is maintained and used by the Buffer.Basic.OutputHandler/>
        /// </summary>
        [ObsoleteAttribute("This property is obsolete and should never be used", false)]
        public List<string> VariablesList;


        //model-specific python scripts
        [DataMember]
        public string EveryCycleTimeCriticalPythonFilePath = "";
        [DataMember]
        public string EveryCycleNonTimeCriticalPythonPath= "";
        [DataMember]
        public string StartofScanTimeCriticalPythonFilePath= "";
        [DataMember]
        public string StartofScanNonTimeCriticalPythonFilePath= "";
        [DataMember]
        public string ControlLecroyVBScriptPath= "";



        /// <summary>
        /// Constructs a new RootModel.
        /// Creates an empty instance of <see cref="DataModel"/> as a child
        /// </summary>
        /// <param name="experiment">The type of the experiment</param>
        public RootModel()
        {  
            Data = new DataModel(this);
        }

        /// <summary>
        /// Verifies the validity of the user-input model (before applying it).
        /// Delegates the process to its child <see cref="DataModel"/>
        /// </summary>
        /// <returns><c>true</c> if the model is valid, <c>false</c> otherwise</returns>
        public bool Verify()
        {
            return Data.Verify();
        }
    }
}
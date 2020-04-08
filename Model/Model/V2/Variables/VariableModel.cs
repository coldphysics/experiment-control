using Communication;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace Model.V2.Variables
{
    /// <summary>
    /// Describes a single variable model (could be used for any kind of variables -static, dynamic, or iterator-)
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class VariableModel
    {
        //RECO create a hierarchy of variable models (static, iterator, dynamic)
        /// <summary>
        /// Specifies the type of variable
        /// </summary>
        [DataMember] public VariableType TypeOfVariable = VariableType.VariableTypeStatic;
        /// <summary>
        /// Python code. Used when the type of the variable is dynamic
        /// </summary>
        [DataMember] public String VariableCode = "";
        /// <summary>
        /// Used for iterator variables
        /// </summary>
        [DataMember] public double VariableStepValue = 0;
        /// <summary>
        /// Used for iterator variables
        /// </summary>
        [DataMember] public double VariableEndValue = 0;
        /// <summary>
        /// Used for iterator variables
        /// </summary>
        [DataMember] public double VariableStartValue = 0;
        /// <summary>
        /// The current value of the variable
        /// </summary>
        [DataMember] public double VariableValue = 0;
        /// <summary>
        /// The name of the variable
        /// </summary>
        [DataMember] public String VariableName = "";
        /// <summary>
        /// Used with static variables to identify the group they belong to
        /// </summary>
        [DataMember] public int groupIndex = 0;

        /// <summary>
        /// Determines whether the type of the variable is iterator
        /// </summary>
        /// <returns>True if the type of the variable is iterator</returns>
        public bool IsIterator()
        {
            return TypeOfVariable == VariableType.VariableTypeIterator;
        }

        //RECO unify the deep cloning procedure        
        /// <summary>
        /// Duplicates the current <see cref=" VariableModel"/> object by using a <see cref=" System.IO.MemoryStream"/>.
        /// </summary>
        /// <returns>An exact copy of the current <see cref=" VariableModel"/> object.</returns>
        public VariableModel DeepClone()
        {
            object objResult = null;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, this);

                ms.Position = 0;
                objResult = bf.Deserialize(ms);
            }
            return (VariableModel)objResult;
        }
    }

}

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Model.V3.Variables
{
    /// <summary>
    /// Manages the list of <see cref=" VariableModel"/> by providing basic operations to manipulate it and retrieve variables from it. It also holds a Dictionary of the variable group names
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class VariablesModel
    {
        /// <summary>
        /// A list of all the <see cref=" VariableModel"/>'s 
        /// </summary>
        [DataMember] 
        public List<VariableModel> VariablesList = new List<VariableModel>();

        /// <summary>
        /// A dictionary (group id, group name). Describes the groups of static variables only
        /// </summary>
        [DataMember]
        public Dictionary<int, string> GroupNames = new Dictionary<int, string>();

        /// <summary>
        /// Adds a new empty <see cref=" VariableModel"/> to the list
        /// </summary>
        /// <returns>A reference to the newly added <see cref=" VariableModel"/></returns>
        public VariableModel addVariable()
        {
            VariableModel variable = new VariableModel();
            VariablesList.Add(variable);
            return variable;
        }

        /// <summary>
        /// Removes a variable from the list
        /// </summary>
        /// <param name="variable">a reference to the variable which is to be removed from the list</param>
        public void deleteVariable(VariableModel variable)
        {
            VariablesList.Remove(variable);
        }

        /// <summary>
        /// Searches the list for a variable with the exact specified name
        /// </summary>
        /// <param name="name">The name of the variable to look for in the list</param>
        /// <returns>The found variable. Doesn't return null.</returns>
        /// <exception cref=" System.Exception">If the variable specified by name is not found in the list</exception>
        public VariableModel GetByName(String name)
        {
            foreach (VariableModel variable in VariablesList)
            {
                if (name.Equals(variable.VariableName))
                {
                    return variable;
                }
            }
            throw new Exception("Variable not found! Name: " + name);
        }

    }
}

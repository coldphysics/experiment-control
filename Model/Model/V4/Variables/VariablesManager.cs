using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Variables.Variables;

namespace Model.Variables
{
    public class VariablesManager
    {
        private ICollection<BasicVariableModel> variables;
        private ICollection<VariableGroupModel> groups;
        private const string DEFAULT_STATIC_VARIABLES_GROUP_NAME = "default static variables group";

        public ICollection<VariableGroupModel> Groups
        {
            get { return groups; }
            set { groups = value; }
        }

        public ICollection<BasicVariableModel> Variables
        {
            get { return variables; }
            set { variables = value; }
        }

        public VariablesManager()
        {
            variables = new ObservableCollection<BasicVariableModel>();
            groups = new ObservableCollection<VariableGroupModel>();
            groups.Add(new VariableGroupModel(DEFAULT_STATIC_VARIABLES_GROUP_NAME, false));
        }

        #region Group Management
        public bool RemoveGroup(VariableGroupModel toRemove)
        {
            if (toRemove.CanBeDeleted)
            {
                groups.Remove(toRemove);
                return true;
            }
            else
                return false;
        }

        public VariableGroupModel CreateGroup()
        {
            return CreateGroup("");
        }

        public VariableGroupModel CreateGroup(string name)
        {
            foreach(VariableGroupModel group in Groups)
                if(group.Name.Equals(name))
                    return null;

            VariableGroupModel newGroup = new VariableGroupModel(name, true);
            groups.Add(newGroup);

            return newGroup;
        }
        #endregion

        public BasicVariableModel FindVariable(string name)
        {
            return Variables.Where(variable => variable.Name.Equals(name)).FirstOrDefault();
        }
        
        private BasicVariableModel AddVariable(BasicVariableModel variable)
        {
            BasicVariableModel existing = FindVariable(variable.Name);

            if(existing == null)
            {
                Variables.Add(variable);
                return variable;
            }
            else
                return null;
        }

        public StaticVariableModel CreateStaticVariable(string name, VariableGroupModel group)
        {
            StaticVariableModel newVariable = new StaticVariableModel() { Name = name, Parent = group };
            BasicVariableModel additionResult = AddVariable(newVariable);

            if(additionResult == null)
                return null;
            else
                return newVariable;

        }


        
    }
}

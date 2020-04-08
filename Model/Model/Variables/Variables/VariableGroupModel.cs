using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Variables.Variables
{
    public class VariableGroupModel
    {
        private string name = "";
        private ICollection<GroupableVariableModel> children;
        private bool canBeDeleted;

        public bool CanBeDeleted
        {
            get { return canBeDeleted; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public ICollection<GroupableVariableModel> Children
        {
            get { return children; }
            set { children = value; }
        }

        public VariableGroupModel()
        {
            canBeDeleted = true;
        }

        public VariableGroupModel(string name, bool canBeDeleted)
        {
            this.Name = name;
            this.canBeDeleted = canBeDeleted;
        }
    }
}

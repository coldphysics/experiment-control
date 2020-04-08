using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Variables.Variables
{
    public abstract class GroupableVariableModel:BasicVariableModel
    {
        private VariableGroupModel parent;


        public VariableGroupModel Parent
        {
            get { return parent; }
            set { parent = value; }
        }
    }
}

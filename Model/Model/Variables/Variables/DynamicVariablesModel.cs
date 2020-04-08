using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Variables.Variables
{
    public class DynamicVariablesModel:BasicVariableModel
    {
        private string pythonScript;

        public string PythonScript
        {
            get { return pythonScript; }
            set { pythonScript = value; }
        }
    }
}

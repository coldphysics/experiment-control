using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Controller.Variables;
using Model.Variables;

namespace Controller.Control.Compare
{
    public class CompareStringItemController : AbstractCompareItemController<string>
    {
        public CompareStringItemController(string oldData, string newData, VariableModel newVariable, bool editable, VariablesController variablesController)
            : base(oldData, newData, newVariable, editable, variablesController)
        {
        }

        protected override void TakeNewValue()
        {
            _newVariable.VariableCode = NewValue;
        }

        protected override void TakeOldValue()
        {
            _newVariable.VariableCode = OldValue;
        }
    }
}

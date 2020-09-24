using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Controller.Variables;
using Model.Variables;

namespace Controller.Control.Compare
{
    public class CompareDoubleItemController : AbstractCompareItemController<double>
    {
        public CompareDoubleItemController(double oldData, double newData, VariableModel newVariable, bool editable, VariablesController variablesController)
            : base(oldData, newData, newVariable, editable, variablesController)
        {
        }

        protected override void TakeNewValue()
        {
            _newVariable.VariableValue = NewValue;
        }

        protected override void TakeOldValue()
        {
            _newVariable.VariableValue = OldValue;
        }
    }
}

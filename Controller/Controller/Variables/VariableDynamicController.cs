
using Model.Variables;

namespace Controller.Variables
{
    public class VariableDynamicController : VariableController
    {
        public VariableDynamicController(VariableModel variableModel, VariablesController parent) : base(variableModel, parent)
        {
        }

        public VariableDynamicController(VariableController basis)
            : base(basis)
        { }
    }
}

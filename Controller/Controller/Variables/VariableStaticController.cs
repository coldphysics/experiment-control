using Model.Variables;

namespace Controller.Variables
{
    public class VariableStaticController : VariableController
    {
        public VariableStaticController(VariableModel variableModel, VariablesController parent) : base(variableModel, parent)
        {
        }

        public VariableStaticController(VariableController basis)
            : base(basis)
        { }
    }
}

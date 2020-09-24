using Controller.Variables;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.Linq;

using Model.Variables;
using Communication;

namespace Controller.Control.Compare
{
    public class VariableComparisonController
    {
        public ObservableCollection<ICompareItemController> Items { set; get; } = new ObservableCollection<ICompareItemController>();

        public VariableComparisonController(VariablesController variablesController, VariablesModel newVariables)
        {
            var oldVariables = variablesController._variablesModel;

            var newVariableNames = newVariables.VariablesList.Select(varM => varM.VariableName).ToList();
            var oldVariableNames = oldVariables.VariablesList.Select(varM => varM.VariableName).ToList();

            var notInNew = oldVariableNames.Except(newVariableNames);
            var notInOld = newVariableNames.Except(oldVariableNames);
            var inBoth = oldVariableNames.Except(notInNew);

            foreach (var commonVariableName in inBoth)
            {
                var oldVariable = oldVariables.GetByName(commonVariableName);
                var newVariable = newVariables.GetByName(commonVariableName);

                if (newVariable.VariableCode != oldVariable.VariableCode)
                {
                    Items.Add(new CompareStringItemController(oldVariable.VariableCode, newVariable.VariableCode, newVariable, true, variablesController));
                } else if (newVariable.VariableValue != oldVariable.VariableValue)
                {
                    if (newVariable.TypeOfVariable == VariableType.VariableTypeStatic)
                    {
                        Items.Add(new CompareDoubleItemController(oldVariable.VariableValue,
                            newVariable.VariableValue, newVariable, true, variablesController));
                    }
                    else if (!((oldVariable.TypeOfVariable == VariableType.VariableTypeDynamic) &&
                            (newVariable.TypeOfVariable == VariableType.VariableTypeDynamic)))
                    {
                        Items.Add(new CompareDoubleItemController(oldVariable.VariableValue,
                        newVariable.VariableValue, newVariable, false, variablesController));
                    }
                }

            }
        }
    }
}

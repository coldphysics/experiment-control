using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Communication;
using Model.Variables;

namespace Controller.Variables.Compare
{
    /// <summary>
    /// Interaction logic for VariableComparison.xaml
    /// </summary>
    public partial class VariableComparison : Window
    {
        public static void ShowNewWindow(VariablesController variablesController, VariablesModel newVariables)
        {
            if (window != null)
            {
                window.Close();
            }
            window = new VariableComparison(variablesController, newVariables);
            window.Show();
        }
        private static VariableComparison window = null;
        public VariableComparison(VariablesController variablesController, VariablesModel newVariables)
        {
            InitializeComponent();


            var oldVariables = variablesController._variablesModel;

            var newVariablesList = newVariables.VariablesList;
            var oldVariablesList = oldVariables.VariablesList;

            var newVariableNames = new List<string>();
            var oldVariableNames = new List<string>();

            int cntSame = 0;
            foreach (var newVariable in newVariablesList)
            {
                newVariableNames.Add(newVariable.VariableName);
            }
            foreach (var oldVariable in oldVariablesList)
            {
                oldVariableNames.Add(oldVariable.VariableName);
            }
            var notInNew = oldVariableNames.Except(newVariableNames);

            foreach (var missingNew in notInNew)
            {
                var missing = oldVariables.GetByName(missingNew);
/*                output.Append(missing.VariableName + "\t" + missing.VariableValue + "\t" +
                                  missing.VariableCode + "\n");*/
            }
            var notInOld = newVariableNames.Except(oldVariableNames);
//            output.Append("not in old: " + notInNew.Count() + "\n");
            foreach (var missingOld in notInOld)
            {
                var missing = newVariables.GetByName(missingOld);
/*                output.Append(missing.VariableName + "\t" + missing.VariableValue + "\t" +
                                  missing.VariableCode + "\n");*/
            }
            var inBoth = oldVariableNames.Except(notInNew);


            foreach (var newVariable in newVariablesList)
            {
                bool same = true;
                if (!inBoth.Contains(newVariable.VariableName))
                {
                    continue;
                }

                var oldVariable = oldVariables.GetByName(newVariable.VariableName);
                if (newVariable.VariableCode != oldVariable.VariableCode)
                {
                    same = false;
/*                    output.Append(newVariable.VariableName + "\told: " + oldVariable.VariableCode +
                                      "\tnew: " + newVariable.VariableCode + "\n");*/
                    spMain.Children.Add(new CompareItem(CompareItem.EnumType.Code, oldVariable.VariableCode, newVariable.VariableCode, newVariable, true, variablesController));
                }
                if (newVariable.VariableValue != oldVariable.VariableValue) 
                {
                    same = false;
                    if (newVariable.TypeOfVariable == VariableType.VariableTypeStatic)
                    {
                        var item = new CompareItem(CompareItem.EnumType.Value, oldVariable.VariableValue,
                            newVariable.VariableValue, newVariable, true, variablesController);
                        spMain.Children.Add(item);
                    }
                    else
                    {
                        if (!((oldVariable.TypeOfVariable == VariableType.VariableTypeDynamic) &&
                            (newVariable.TypeOfVariable == VariableType.VariableTypeDynamic)))
                        {
                        var item = new CompareItem(CompareItem.EnumType.Value, oldVariable.VariableValue,
                            newVariable.VariableValue, newVariable, false, variablesController);
                        spMain.Children.Add(item);
                            
                        }
                        
                    }
                    /*                    output.Append(newVariable.VariableName + "\told: " + oldVariable.VariableValue +
                                                          "\tnew: " + newVariable.VariableValue + "\n");*/
                }

                if (same)
                {
                    cntSame++;
                }
            }
//            output.Append(cntSame + " variables are the same." + "\n");

    //            spMain.Children.Add(new CompareItem());
        }
    }
}

using System.Collections.Generic;

namespace Model.MeasurementRoutine.GlobalVariables
{

    public class GlobalVariablesFactory
    {
        private static GlobalVariablesFactory instance = null;

        private GlobalVariablesFactory()
        { }

        public static GlobalVariablesFactory GetInstance()
        {
            if (instance == null)
            {
                instance = new GlobalVariablesFactory();
            }

            return instance;
        }


        public ICollection<AbstractGlobalVariable> BuildVariables()
        {
            ICollection<AbstractGlobalVariable> result = new List<AbstractGlobalVariable>();

            result.Add(
                new GlobalVariable<List<double>>()
                    { 
                        VariableName = GlobalVariableNames.ROUTINE_ARRAY, 
                        VariableValue = new List<double>()
                    }
                );

            return result;
        }

    }
}

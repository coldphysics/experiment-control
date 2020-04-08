using Communication;
using Microsoft.Scripting.Hosting;
using Model.Data;
using Model.Data.Sequences;
using Model.MeasurementRoutine.GlobalVariables;
using Model.Variables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PythonUtils
{
    /// <summary>
    /// Manages a python scope and facilitates adding variables defined in the program to it.
    /// </summary>
    public class PythonScopeManager
    {
        /// <summary>
        /// The scope to manage.
        /// </summary>
        private ScriptScope scope;

        /// <summary>
        /// The names of the variables that were added to the scope
        /// </summary>
        private List<string> variables = new List<string>();

        /// <summary>
        /// Gets the scope.
        /// </summary>
        /// <value>
        /// The scope.
        /// </value>
        public ScriptScope Scope
        {
            get { return scope; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PythonScopeManager"/> class.
        /// </summary>
        /// <param name="scope">The scope.</param>
        public PythonScopeManager(ScriptScope scope)
        {
            this.scope = scope;
        }

        /// <summary>
        /// Adds a variable to the scope if not already existing, otherwise, changes its value.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The value of the variable.</param>
        public void SetPyhtonVariableValue(string name, object value)
        {
            scope.SetVariable(name, value);
        }

        /// <summary>
        /// Gets the value of a variable from the variable scope associated with this instance.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <returns>The value of the specified variable.</returns>\
        /// <exception cref="System.MissingMemberException">Thrown when the specified variable is not found in the scope.</exception>
        public object GetPythonVariableValue(string variableName)
        {
            dynamic result = scope.GetVariable(variableName);//Throws System.MissingMemberException
            return result;
        }


        /// <summary>
        /// Gets the value of a variable from the variable scope associated with this instance, and casts it to <c>double</c>.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <returns>The <c>double</c> value of the specified variable</returns>
        /// <exception cref="System.MissingMemberException">Thrown when the specified variable is not found in the scope.</exception>
        /// <exception cref="System.InvalidCastException">Thrown when the type of the variable cannot be treated as a <c>double</c>.</exception>
        public double GetPythonVariableValueAsDouble(string variableName)
        {
            object value = GetPythonVariableValue(variableName);
            double result = Double.Parse(string.Format("{0}", value));//This enables converting all numeric types to double

            return result;
        }

        /// <summary>
        /// Gets the value of a variable from the variable scope associated with this instance, and casts it to <c>int</c>.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <returns>The <c>int</c> value of the specified variable</returns>
        /// <exception cref="System.MissingMemberException">Thrown when the specified variable is not found in the scope.</exception>
        /// <exception cref="System.InvalidCastException">Thrown when the type of the variable cannot be treated as a <c>int</c>.</exception>
        public int GetPythonVariableValueAsInteger(string variableName)
        {
            object value = GetPythonVariableValue(variableName);
            return (int)value;
        }

        /// <summary>
        /// Gets the value of a variable from the variable scope associated with this instance, and casts it to <c>List_double</c>.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <returns>The <c>List_double</double></c> value of the specified variable</returns>
        /// <exception cref="System.MissingMemberException">Thrown when the specified variable is not found in the scope.</exception>
        /// <exception cref="System.InvalidCastException">Thrown when the type of the variable cannot be treated as a <c>List_double</c>.</exception>
        public List<double> GetPythonVariableAsCollectionOfDoubles(string variableName)
        {
            object value = GetPythonVariableValue(variableName);
            return (List<double>)value;
        }

        /// <summary>
        /// Gets all variable names.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllPythonVariableNames()
        {
            IEnumerable<string> result = scope.GetVariableNames();

            return result;
        }

        /// <summary>
        /// Adds the variables of a specific type to the scope.
        /// </summary>
        /// <param name="variablesModel">The variables model.</param>
        /// <param name="typeToAdd">The type to add.</param>
        private void AddVariablesToScope(VariablesModel variablesModel, VariableType typeToAdd)
        {
            foreach (VariableModel variable in variablesModel.VariablesList)
            {
                if (variable.TypeOfVariable == typeToAdd)
                {
                    SetPyhtonVariableValue(variable.VariableName, variable.VariableValue);
                    variables.Add(variable.VariableName);
                }
            }

        }

        /// <summary>
        /// Adds the static variables to the scope.
        /// </summary>
        /// <param name="variablesModel">The variables model.</param>
        public void AddStaticVariablesToScope(VariablesModel variablesModel)
        {
            AddVariablesToScope(variablesModel, VariableType.VariableTypeStatic);
        }

        /// <summary>
        /// Adds the iterator variables to the scope.
        /// </summary>
        /// <param name="variablesModel">The variables model.</param>
        public void AddIteratorVariablesToScope(VariablesModel variablesModel)
        {

            foreach (VariableModel variable in variablesModel.VariablesList)
            {
                if (variable.TypeOfVariable == VariableType.VariableTypeIterator)
                {
                    SetPyhtonVariableValue(variable.VariableName, variable.VariableValue);
                    SetPyhtonVariableValue(variable.VariableName + "EndValue", variable.VariableEndValue);
                    SetPyhtonVariableValue(variable.VariableName + "StartValue", variable.VariableStartValue);

                    variables.Add(variable.VariableName);
                    variables.Add(variable.VariableName + "EndValue");
                    variables.Add(variable.VariableName + "StartValue");
                }
            }
        }

        /// <summary>
        /// Adds the dynamic variables to the scope.
        /// </summary>
        /// <param name="variablesModel">The variables model.</param>
        public void AddDynamicVariablesToScope(VariablesModel variablesModel)
        {
            AddVariablesToScope(variablesModel, VariableType.VariableTypeDynamic);
        }

        /// <summary>
        /// Adds variables to the scope indicating whether each sequence is enabled or disabled
        /// </summary>
        /// <param name="model">The model.</param>
        public void AddSequenceEnabledStateVariables(DataModel model)
        {
            foreach (SequenceModel sequence in model.group.Cards.First().Sequences)
            {
                SetPyhtonVariableValue("seq_" + sequence.Index(), sequence.IsEnabled);
            }
        }

        /// <summary>
        /// Adds the global variables to the scope.
        /// </summary>
        public void AddGlobalVariables()
        {
            foreach (AbstractGlobalVariable variable in GlobalVariablesManager.GetInstance().GetAllGlobalVariables())
            {
                SetPyhtonVariableValue(variable.VariableName, variable.GetValue());
            }
        }

        /// <summary>
        /// Checks whether a program variable is defined within the scope
        /// </summary>
        /// <param name="variableName">The name of the variable</param>
        /// <returns><c>true</c> if the variable is defined in the scope</returns>
        public bool IsVariableUsed(string variableName)
        {
            return variables.Contains(variableName);
        }


        /// <summary>
        /// Clears all python variables (including all defined names such as imported things!) from the scope.
        /// </summary>
        public void ClearScope()
        {
            variables.Clear();
            foreach (string name in variables)
                scope.RemoveVariable(name);
        }


    }
}

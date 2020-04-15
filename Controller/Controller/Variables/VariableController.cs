using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Communication;
using Communication.Commands;
using Model.Variables;
using PythonUtils;
using Model.Data.Channels;
using Model.Data.Steps;
using System.Windows;
using PythonUtils.ScriptExecution;

namespace Controller.Variables
{
    /// <summary>
    /// A controller for a single variable
    /// </summary>
    public class VariableController : BaseController
    {
        // ******************** variables ********************
        private VariablesController _parent;



        public VariableModel _model;
        /// <summary>
        /// A list of string representations of the locations in which this variable is being used.
        /// </summary>
        private List<string> usages = new List<string>();


        public delegate void LoseFocusOnIterators(object sender, EventArgs e);
        public event LoseFocusOnIterators LoseFocus;


        public ICommand DeleteVariable { get; private set; }
        public ICommand SwitchToStatic { get; private set; }
        public ICommand SwitchToIterator { get; private set; }
        public ICommand SwitchToDynamic { get; private set; }
        public ICommand MoveUp { get; private set; }
        public ICommand MoveDown { get; private set; }
        public ICommand RemoveGroup { get; private set; }
        // ******************** properties ********************

        /// <summary>
        /// Gets a value indicating whether this <see cref="VariableController"/> is used.
        /// </summary>
        /// <value>
        ///   <c>true</c> if used; otherwise, <c>false</c>.
        /// </value>
        public bool Used
        {
            get
            {
                if (usages != null && usages.Count > 0)
                    return true;

                return false;
            }
        }


        /// <summary>
        /// Gets the list of usages of this variable as a single string
        /// </summary>
        /// <value>
        /// The usages as string or <c>null</c> if there are no usages.
        /// </value>
        public string UsagesAsString
        {
            get
            {
                if (usages.Count > 0)
                {
                    StringBuilder builder = new StringBuilder();

                    for (int i = 0; i < usages.Count; i++)
                    {
                        builder.Append(usages[i]);

                        if (i < usages.Count - 1)
                            builder.AppendLine();
                    }

                    return builder.ToString();
                }

                return null;
            }

        }

        private bool _isGroupHeader = false;

        public bool IsGroupHeader
        {
            get { return _isGroupHeader; }
            set { _isGroupHeader = value; }
        }

        public int getModelIndex
        {
            get { return _parent._variablesModel.VariablesList.IndexOf(_model); }
        }

        public string GroupName
        {
            get
            {
                if (_parent.GroupNames.ContainsKey(GroupIndex))
                {
                    return _parent.GroupNames[GroupIndex];
                }
                return "";
            }
            set
            {
                if (_parent.GroupNames.ContainsKey(GroupIndex))//This should always be true!
                {
                    _parent.RenameGroup(GroupIndex, value);
                    OnPropertyChanged("GroupName");
                }
            }
        }

        public bool GroupExpandState
        {
            get
            {
                if (_parent.GroupsExpandState.ContainsKey(GroupIndex))
                {
                    return _parent.GroupsExpandState[GroupIndex];
                }
                return true;
            }

            set
            {
                if (_parent.GroupsExpandState.ContainsKey(GroupIndex))
                {
                    _parent.GroupsExpandState[GroupIndex] = value;
                    OnPropertyChanged("GroupExpandState");
                }
            }
        }

        public static string NOVARIABLE
        {
            get { return ""; }
        }

        public int GroupIndex
        {
            get { return _model.groupIndex; }
            set
            {
                _model.groupIndex = value;
                OnPropertyChanged("GroupIndex");
            }
        }
        public void DoLoseFocus()
        {
            this.LoseFocus?.Invoke(null, null);
        }

        public SolidColorBrush VariableUsage
        {
            get
            {
                if (this.Used)
                {
                    return Brushes.LightGreen;
                }
                return Brushes.LightGray;
            }
        }

        /// <summary>
        /// Name of the variable
        /// </summary>
        public String VariableName
        {
            get { return _model.VariableName; }
            set
            {
                if (IsNameUsed(value))
                {
                    MessageBox.Show("The variable name (" + value + ") is already in use by another variable!", "Cannot Rename Variable", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    _parent.CheckAllVariablesUsage(null);
                    if (Used)
                    {
                        MessageBoxResult result = MessageBox.Show("The variable is used in the following places:\n" + UsagesAsString + "\nIf you continue, it will be renamed everywhere?", "Rename Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

                        if (result == MessageBoxResult.OK)
                        {
                            RenameVariableInPythonScripts(value);

                            _model.VariableName = value;
                            updateVariablesListFromParent();
                            _parent.DoVariablesValueChanged(this);
                        }

                    }
                    else
                    {
                        _model.VariableName = value;
                        updateVariablesListFromParent();
                        _parent.DoVariablesValueChanged(this);
                    }
                }
            }
        }

        public void ClearUsages()
        {
            usages.Clear();

            OnPropertyChanged("UsagesAsString");
            OnPropertyChanged("VariableUsage");
        }
        public void AddUsage(string usage)
        {
            usages.Add(usage);
            OnPropertyChanged("UsagesAsString");
            OnPropertyChanged("VariableUsage");

        }

        /// <summary>
        /// Value of the variable
        /// </summary>
        public double VariableValue
        {
            get { return _model.VariableValue; }
            set
            {
                if (_model.VariableValue == value)
                {
                    return;
                }

                // prevent inconsistencies and multiple updates on the buffer
                object lockObject = _parent.VariableUpdateStart();

                System.Console.Write("variable {0}: {1}\n", _model.VariableName, value);
                _model.VariableValue = value;
                _parent.DoVariablesValueChanged(this);
                OnPropertyChanged("VariableValue");

                //TODO this could be an error
                if (this.TypeOfVariable != VariableType.VariableTypeDynamic)
                {
                    _parent.evaluate(null);
                }
                // re-enable the Buffer updateVariablesListFromParent
                _parent.VariableUpdateDone(lockObject);
            }
        }

        /// <summary>
        /// Start value of the variable
        /// </summary>
        public double VariableStartValue
        {
            get { return _model.VariableStartValue; }
            set
            {
                _model.VariableStartValue = value;          
                _parent.ResetIteratorValues();
                _parent.countTotalNumberOfIterations();
            }
        }

        public bool VariableLocked
        {
            get { return _variableLocked; }
            set
            {
                _variableLocked = value;
                OnPropertyChanged("VariableLocked");
                OnPropertyChanged("VariableLockedColor");
            }
        }
        private bool _variableLocked = false;

        public SolidColorBrush VariableLockedColor
        {
            get
            {
                if (this.VariableLocked)
                {
                    return Brushes.LightGray;
                }
                return Brushes.White;
            }
        }

        /// <summary>
        /// End value of the variable
        /// </summary>
        public double VariableEndValue
        {
            get { return _model.VariableEndValue; }
            set
            {

                _model.VariableEndValue = value;
                _parent.DoVariablesValueChanged(this);
                _parent.countTotalNumberOfIterations();
            }
        }

        /// <summary>
        /// Step value for the variable
        /// </summary>
        public double VariableStepValue
        {
            get { return _model.VariableStepValue; }
            set
            {

                _model.VariableStepValue = value;
                _parent.DoVariablesValueChanged(this);
                _parent.countTotalNumberOfIterations();
            }
        }

        /// <summary>
        /// Code for the variable
        /// </summary>
        public string VariableCode
        {
            get { return _model.VariableCode; }
            set
            {
                Object bufferUpdateLock = _parent.VariableUpdateStart();
                _model.VariableCode = value;
                _parent.evaluate(null);
                _parent.VariableUpdateDone(bufferUpdateLock);
            }
        }

        /// <summary>
        /// Type of the variable (static, iterator, dynamic)
        /// </summary>
        public VariableType TypeOfVariable
        {
            get { return _model.TypeOfVariable; }
            set
            {
                Object bufferUpdateLock = _parent.VariableUpdateStart();
                VariableType oldType = _model.TypeOfVariable;
                _model.TypeOfVariable = value;
                // we need to newly calculate the number of iterations when we have a new iterator, or we remove one
                if (value == VariableType.VariableTypeIterator || oldType == VariableType.VariableTypeIterator)
                {
                    _parent.countTotalNumberOfIterations();
                }
                _parent.DoVariablesValueChanged(this);
                if (this.VariableName != NOVARIABLE)
                {
                    foreach (VariableController variable in _parent.VariablesDynamic)
                    {
                        if (variable.VariableCode.Contains(this.VariableName))
                        {
                            System.Console.Write("{0} depends on {1}\n", variable.VariableName, this.VariableName);
                            _parent.DoVariablesValueChanged(variable);
                        }
                    }
                }
                _parent.VariableUpdateDone(bufferUpdateLock);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="VariableController"/> changes in the iterating mode.
        /// </summary>
        /// <value>
        ///   <c>true</c> if changes; otherwise, <c>false</c>.
        /// </value>
        public bool changes
        {
            get
            {
                if (TypeOfVariable == VariableType.VariableTypeIterator)
                {
                    return true;
                }
                if (TypeOfVariable == VariableType.VariableTypeStatic)
                {
                    return false;
                }

                foreach (VariableController variable in _parent.Variables)
                {
                    if (PythonScriptVariablesAnalyzer.IsVariableUsedInScript(variable.VariableName, VariableCode))
                    {
                        if (variable.changes)//Potential infite call!!
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public List<MenuItem> staticGroups
        {
            get { return _parent.constructStaticGroups(this); }
        }


        // ******************** constructor ********************
        /// <summary>
        /// Public constructor for a variable controller
        /// </summary>
        /// <param name="variableModel">The model of the variable</param>
        /// <param name="parent">The parent variables controller</param>
        public VariableController(VariableModel variableModel, VariablesController parent)
        {
            _model = variableModel;
            _parent = parent;
            SwitchToStatic = new RelayCommand(switchToStatic);
            SwitchToIterator = new RelayCommand(switchToIterator);
            SwitchToDynamic = new RelayCommand(switchToDynamic);
            DeleteVariable = new RelayCommand(deleteVariable);
            MoveUp = new RelayCommand(moveUp, CanMoveUpOrDown);
            MoveDown = new RelayCommand(moveDown, CanMoveUpOrDown);
            RemoveGroup = new RelayCommand(DoRemoveGroup);
        }

        /// <summary>
        /// Determines whether this instance can move up or down.
        /// </summary>
        /// <param name="parameter">not used</param>
        /// <returns><c>true</c> if the type of the variable is iterator or dynamic.</returns>
        private bool CanMoveUpOrDown(object parameter)
        {
            return (TypeOfVariable != VariableType.VariableTypeStatic);
        }

        /// <summary>
        /// Removes the group.
        /// </summary>
        /// <param name="parameter">not used</param>
        private void DoRemoveGroup(object parameter)
        {
            if (IsGroupHeader)
            {
                _parent.RemoveGroup(GroupIndex);
            }
        }
        /// <summary>
        /// Deletes the current variable.
        /// </summary>
        /// <param name="parameter">not used</param>
        public void deleteVariable(object parameter)
        {
            _parent.RemoveVariable(this);
        }

        /// <summary>
        /// Switches the type of this variable to static
        /// </summary>
        /// <param name="parameter"></param>
        public void switchToStatic(object parameter)
        {
            this.TypeOfVariable = VariableType.VariableTypeStatic;
            updateVariablesListFromParent();
        }

        /// <summary>
        /// Switches the type of this variable to iterator
        /// </summary>
        /// <param name="parameter"></param>
        public void switchToIterator(object parameter)
        {
            this.TypeOfVariable = VariableType.VariableTypeIterator;
            updateVariablesListFromParent();
        }

        /// <summary>
        /// Switches the variable to dynamic
        /// </summary>
        /// <param name="parameter"></param>
        public void switchToDynamic(object parameter)
        {
            this.TypeOfVariable = VariableType.VariableTypeDynamic;
            updateVariablesListFromParent();
        }

        /// <summary>
        /// Move the Variable 1 up
        /// </summary>
        /// <param name="parameter"></param>
        public void moveUp(object parameter)
        {
            _parent.moveUp(this);
        }

        /// <summary>
        /// Move the Variable 1 down
        /// </summary>
        /// <param name="parameter"></param>
        public void moveDown(object parameter)
        {
            _parent.moveDown(this);
        }


        /// <summary>
        /// Calls an updateVariablesListFromParent of the variable List (e.g. if the type of this variable is changed so it will be assigned to another variable type list.
        /// </summary>
        public void updateVariablesListFromParent()
        {
            System.Console.WriteLine("Update variable list");
            _parent.UpdateVariablesList();
        }

        //RECO make this also check the special variable names such as (cal, uncal, val, out, t, T, t0, and the imported math symbols)
        /// <summary>
        /// Checks whether the new variable name is used by some other variable
        /// </summary>
        /// <param name="variableName">The new variable name</param>
        /// <returns><c>true</c> if the name is already in use; <c>false</c> therwise.</returns>
        private bool IsNameUsed(string variableName)
        {
            foreach (VariableController variable in _parent.Variables)
            {
                if (variable.VariableName.Equals(variableName) && !variable.Equals(this))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Renames the current variable in all its occurrences in python scripts.
        /// </summary>
        /// <param name="newName">The new name of the variable</param>
        private void RenameVariableInPythonScripts(string newName)
        {
            VariableUsageChecker checker = new VariableUsageChecker(_parent._rootModel);

            foreach (VariableUsage usage in checker.GetUsagesOfVariable(VariableName))
            {
                switch (usage.UsageType)
                {
                    case VariableUsageType.CalibrationScript:
                        ChannelSettingsModel setting = (ChannelSettingsModel)usage.UsageContext;
                        setting.CalibrationScript = PythonScriptVariablesAnalyzer.RenameVariableInScript(VariableName, newName, setting.CalibrationScript);
                        break;
                    case VariableUsageType.DynamicVariable:
                        VariableModel dynamicVariable = (VariableModel)usage.UsageContext;
                        dynamicVariable.VariableCode = PythonScriptVariablesAnalyzer.RenameVariableInScript(VariableName, newName, dynamicVariable.VariableCode);
                        break;
                    case VariableUsageType.PythonStep:
                        StepPythonModel pyStep = (StepPythonModel)usage.UsageContext;
                        pyStep.Script = PythonScriptVariablesAnalyzer.RenameVariableInScript(VariableName, newName, pyStep.Script);
                        break;
                }
            }
        }


    }


}

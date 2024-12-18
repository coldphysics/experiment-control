﻿using System;
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
using Controller.Helper;
using Errors.Error;

namespace Controller.Variables
{
    /// <summary>
    /// A controller for a single variable
    /// </summary>
    public abstract class VariableController : BaseController
    {
        public readonly static string NO_VARIABLE = "";

        // ******************** variables ********************
        private VariablesController _parent;
        public VariableModel _model;
        /// <summary>
        /// A list of string representations of the locations in which this variable is being used.
        /// </summary>
        private List<string> usages = new List<string>();

        private bool _variableLocked = false;

        public ICommand DeleteVariableCommand { get; private set; }
        public ICommand SwitchToStaticCommand { get; private set; }
        public ICommand SwitchToIteratorCommand { get; private set; }
        public ICommand SwitchToDynamicCommand { get; private set; }
        public ICommand MoveUpCommand { get; private set; }
        public ICommand MoveDownCommand { get; private set; }
        public ICommand RemoveGroupCommand { get; private set; }
        public ICommand MouseDownCommand { get; private set; }
        // ******************** properties ********************

        /// <summary>
        /// Indicates whether the current variable can be moved to a group (only returns true for static variables)
        /// </summary>
        public bool CanMoveVariableToGroup
        {
            get
            {
                return TypeOfVariable == VariableType.VariableTypeStatic;
            }
        }

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

        public bool IsGroupHeader
        {
            get;
            set;
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

        public int GroupIndex
        {
            get { return _model.groupIndex; }
            set
            {
                _model.groupIndex = value;
                OnPropertyChanged("GroupIndex");
            }
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
        public string VariableName
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
                            UpdateVariablesListFromParent();
                            _parent.SignalVariableValueChanged(this);
                        }

                    }
                    else
                    {
                        _model.VariableName = value;
                        UpdateVariablesListFromParent();
                        _parent.SignalVariableValueChanged(this);
                    }
                }
            }
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
                object notificationLock = ErrorCollector.Instance.StartBulkUpdate();
                object copyLock = _parent.GetRootController().BulkUpdateStart();
              
                System.Console.Write("variable {0}: {1}\n", _model.VariableName, value);
                _model.VariableValue = value;
                _parent.SignalVariableValueChanged(this);
                OnPropertyChanged("VariableValue");

                //TODO this could be an error
                if (this.TypeOfVariable != VariableType.VariableTypeDynamic)
                {
                    _parent.Evaluate(null);
                }
                // re-enable the Buffer updateVariablesListFromParent
                _parent.VariableUpdateDone(copyLock, notificationLock);
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
                _parent.CountTotalNumberOfIterations();
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
                _parent.SignalVariableValueChanged(this);
                _parent.CountTotalNumberOfIterations();
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
                _parent.SignalVariableValueChanged(this);
                _parent.CountTotalNumberOfIterations();
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
                object errorNotificationsLock = ErrorCollector.Instance.StartBulkUpdate();
                object bufferUpdateLock = _parent.GetRootController().BulkUpdateStart();
                _model.VariableCode = value;
                _parent.Evaluate(null);
                _parent.VariableUpdateDone(bufferUpdateLock, errorNotificationsLock);
            }
        }

        /// <summary>
        /// Type of the variable (static, iterator, dynamic)
        /// </summary>
        public VariableType TypeOfVariable
        {
            get { return _model.TypeOfVariable; }
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
            get { return _parent.ConstructStaticGroups(this); }
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
            SwitchToStaticCommand = new RelayCommand(SwitchToStatic);
            SwitchToIteratorCommand = new RelayCommand(SwitchToIterator);
            SwitchToDynamicCommand = new RelayCommand(SwitchToDynamic);
            DeleteVariableCommand = new RelayCommand(DeleteVariable);
            MoveUpCommand = new RelayCommand(MoveUp, CanMoveUpOrDown);
            MoveDownCommand = new RelayCommand(MoveDown, CanMoveUpOrDown);
            RemoveGroupCommand = new RelayCommand(DoRemoveGroup);
            MouseDownCommand = new RelayCommand(OnMouseDown);
        }

        /// <summary>
        /// Creates a new instance of this class based on an exiting instance;
        /// </summary>
        /// <param name="controller">The controller on whihc we want to base the new instance</param>
        public VariableController(VariableController controller)
            : this(controller._model, controller._parent)
        {
            IsGroupHeader = controller.IsGroupHeader;
            VariableLocked = controller.VariableLocked;
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
        /// Determines whether this instance can move up or down.
        /// </summary>
        /// <param name="parameter">not used</param>
        /// <returns><c>true</c> if the type of the variable is iterator or dynamic.</returns>
        private bool CanMoveUpOrDown(object parameter)
        {
            if (TypeOfVariable == VariableType.VariableTypeStatic)
                return false;

            if (TypeOfVariable == VariableType.VariableTypeIterator && _variableLocked)
                return false;

            return true;
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
        public void DeleteVariable(object parameter)
        {
            _parent.RemoveVariable(this);
        }

        /// <summary>
        /// Switches the type of this variable to static
        /// </summary>
        /// <param name="parameter"></param>
        public void SwitchToStatic(object parameter)
        {
            _parent.ChangeVariableType(this, VariableType.VariableTypeStatic);
        }

        /// <summary>
        /// Switches the type of this variable to iterator
        /// </summary>
        /// <param name="parameter"></param>
        public void SwitchToIterator(object parameter)
        {
            _parent.ChangeVariableType(this, VariableType.VariableTypeIterator);
        }

        /// <summary>
        /// Switches the variable to dynamic
        /// </summary>
        /// <param name="parameter"></param>
        public void SwitchToDynamic(object parameter)
        {
            _parent.ChangeVariableType(this, VariableType.VariableTypeDynamic);
        }

        /// <summary>
        /// Move the Variable 1 up
        /// </summary>
        /// <param name="parameter"></param>
        public void MoveUp(object parameter)
        {
            _parent.MoveUp(this);
        }

        /// <summary>
        /// Move the Variable 1 down
        /// </summary>
        /// <param name="parameter"></param>
        public void MoveDown(object parameter)
        {
            _parent.MoveDown(this);
        }


        /// <summary>
        /// Calls an updateVariablesListFromParent of the variable List (e.g. if the type of this variable is changed so it will be assigned to another variable type list.)
        /// </summary>
        public void UpdateVariablesListFromParent()
        {
            Console.WriteLine("Update variable list");
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
            foreach (VariableModel variable in _parent._variablesModel.VariablesList)
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

        private void OnMouseDown(object parameter)
        {
            ListBoxItem item = ViewsHelper.FindParentByType<ListBoxItem>((UserControl)parameter);
            item.IsSelected = true;
        }


    }


}

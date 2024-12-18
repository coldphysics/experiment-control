﻿using AbstractController.Data.Steps;
using Communication.Commands;
using Communication.Events;
using Controller.Data.Channels;
using Controller.Data.WindowGroups;
using Controller.Helper;
using Controller.Variables;
using Model.Data.Steps;
using Model.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Controller.Data.Steps
{
    /// <summary>
    /// A general controller for steps
    /// </summary>
    /// <seealso cref="AbstractController.Data.Steps.AbstractStepController" />
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public abstract class StepBasicController : AbstractStepController, INotifyPropertyChanged
    {

        // ******************** Fields ******************** 
        // ************************************************
        /// <summary>
        /// The model of this step
        /// </summary>
        internal readonly StepBasicModel _model;

        /// <summary>
        /// The controller for the channel that contains this step
        /// </summary>
        internal ChannelBasicController _parent;

        /// <summary>
        /// The variable that is attached to this step's duration
        /// </summary>
        private VariableController _durationVariable;

        /// <summary>
        /// The variable that is attached to this step's value
        /// </summary>
        private VariableController _valueVariable;

        /// <summary>
        /// The controller for the variables
        /// </summary>
        private VariablesController _variables;

        // ******************** Constructor ********************
        // *****************************************************
        /// <summary>
        /// Initializes a new instance of the <see cref="StepBasicController"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="model">The model.</param>
        public StepBasicController(ChannelBasicController parent, StepBasicModel model)
            : base(parent)
        {
            _parent = parent;
            _model = model;
            RemoveItem = new RelayCommand(Remove);
            DigitalButton = new RelayCommand(SetDigitalValue);
            UserInputValue = new RelayCommand(SetUserInputValue);
            UserInputDuration = new RelayCommand(SetUserInputDuration);
            VariableInputValue = new RelayCommand(SetVariableInputValue);
            VariableInputDuration = new RelayCommand(SetVariableInputDuration);
            InsertCommand = new RelayCommand(DoInsert);
            MoveLeftCommand = new RelayCommand(DoMoveLeft);
            MoveRightCommand = new RelayCommand(DoMoveRight);
            SetMessage = new RelayCommand(DoSetMessage);
            KeyDownPressedCommand = new RelayCommand(OnPreviewKeyDown);

        }

        // ******************** Events ******************** 
        // ************************************************
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        // ******************* Properties **********************
        // *****************************************************
        /// <summary>
        /// Gets the _root controller.
        /// </summary>
        /// <value>
        /// The _root controller.
        /// </value>
        public Root.RootController _rootController
        {
            get { return _parent._rootController; }
        }

        /// <summary>
        /// Gets the possible names for a step in an analog card.
        /// </summary>
        /// <value>
        /// An array of all possible names of steps in an analog card.
        /// </value>
        public Array AnalogTypeList
        {
            get { return Enum.GetValues(typeof(ChannelBasicController.AnalogTypes)); }
        }

        /// <summary>
        /// Gets or sets the analog type selected.
        /// </summary>
        /// <value>
        /// The analog type selected.
        /// </value>
        public abstract ChannelBasicController.AnalogTypes AnalogTypeSelected
        {
            set;
            get;
        }

        /// <summary>
        /// Gets the color of the button of the value of the digital step.
        /// </summary>
        /// <value>
        /// The color of the button.
        /// </value>
        public SolidColorBrush ButtonColor
        {
            get
            {
                if (Math.Abs(GetValue() - 1) < 0.001)
                {
                    return Brushes.LawnGreen;
                }

                return Brushes.Red;
            }
        }

        /// <summary>
        /// Gets or sets the command that is triggered when the digital value button is clicked
        /// </summary>
        /// <value>
        /// The digital button command.
        /// </value>
        public ICommand DigitalButton { get; private set; }

        /// <summary>
        /// Gets the possible names for a step in an digital card.
        /// </summary>
        /// <value>
        /// An array of all possible names of steps in an digital card.
        /// </value>
        public Array DigitalTypeList
        {
            get { return Enum.GetValues(typeof(ChannelBasicController.DigitalTypes)); }
        }

        /// <summary>
        /// Gets or sets the digital type selected.
        /// </summary>
        /// <value>
        /// The digital type selected.
        /// </value>
        public abstract ChannelBasicController.DigitalTypes DigitalTypeSelected
        {
            set;
            get;

        }

        /// <summary>
        /// Gets or sets the duration of the step.
        /// </summary>
        /// <value>
        /// The duration.
        /// </value>
        /// <remarks>Automatically uses the variable if one is attached</remarks>
        public double Duration
        {
            get
            {
                if (GetDurationVariableName() == "" || GetDurationVariableName() == null)
                {
                    return GetDuration();
                }
                else
                {
                    return DurationVariable.VariableValue;
                }
            }
            set
            {
                if (GetDurationVariableName() == "" || GetDurationVariableName() == null)
                {
                    SetDuration(value);
                }
                else
                {
                    SetDuration(DurationVariable.VariableValue);
                    UpdateProperty("Duration");
                }
            }
        }

        /// <summary>
        /// Gets the color of the duration of the step
        /// </summary>
        /// <value>
        /// The color of the duration.
        /// </value>
        public SolidColorBrush DurationColor
        {
            get
            {
                if (GetDurationVariableName() == "" || GetDurationVariableName() == null)
                {
                    return Brushes.White;
                }
                if (!DurationVariable.changes)
                {
                    return Brushes.LightGreen;
                }

                return Brushes.Orange;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the duration of the step is read-only.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the duration of the step is read-only; otherwise, <c>false</c>.
        /// </value>
        public bool DurationIsReadOnly
        {
            get
            {

                if (GetDurationVariableName() == "" || GetDurationVariableName() == null)
                {
                    //System.Console.Write("Duration: false\n");
                    return false;
                }
                //System.Console.Write("Duration: true\n");
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the variable that is attached to this step's duration
        /// </summary>
        /// <value>
        /// The duration variable.
        /// </value>
        public VariableController DurationVariable
        {
            get
            {
                if (_durationVariable == null && GetDurationVariableName() != null)
                {
                    if (!GetDurationVariableName().Equals(VariableController.NO_VARIABLE))
                    {
                        ReattachVariable();
                    }
                }

                return _durationVariable;
            }
            set { _durationVariable = value; }
        }

        /// <summary>
        /// Gets the name of the duration variable.
        /// </summary>
        /// <value>
        /// The name of the duration variable.
        /// </value>
        public string DurationVariableName
        {
            get
            {
                //System.Console.Write("DVN: {0}\n", GetDurationVariableName());
                return GetDurationVariableName();
            }
        }

        /// <summary>
        /// Gets the names of all iterator variables which can be used as durations for steps
        /// </summary>
        /// <value>
        /// The names of all iterator variables as simple menu items.
        /// </value>
        public List<MenuItem> dynamicNamesDuration
        {
            get
            {
                return ConstructVariableMenuItems(VariableInputDuration, Variables.VariablesDynamic);
            }
        }

        /// <summary>
        /// Gets the names of all dynamic variables which can be used as values for steps
        /// </summary>
        /// <value>
        /// The names of all dynamic variables as simple menu items.
        /// </value>
        public List<MenuItem> dynamicNamesValue
        {
            get
            {
                return ConstructVariableMenuItems(VariableInputValue, Variables.VariablesDynamic);

            }
        }

        /// <summary>
        /// Gets the input unit.
        /// </summary>
        /// <value>
        /// The input unit.
        /// </value>
        public string InputUnit
        {
            get
            {
                if (_model.Setting.UseCalibration)
                    return _model.Setting.InputUnit;
                else
                    return ChannelSettingsController.DEFAULT_INPUT_UNIT;
            }
        }


        /// <summary>
        /// Gets or sets the command that is triggered when the insert button is clicked
        /// </summary>
        /// <value>
        /// The insert command.
        /// </value>
        public ICommand InsertCommand { get; set; }

        /// <summary>
        /// Gets the names of all iterator variables which can be used as durations for steps
        /// </summary>
        /// <value>
        /// The names of all iterator variables as simple menu items.
        /// </value>
        public List<MenuItem> iteratorNamesDuration
        {
            get
            {
                return ConstructVariableMenuItems(VariableInputDuration, Variables.VariablesIterator);
            }
        }

        /// <summary>
        /// Gets the names of all iterator variables which can be used as values for steps
        /// </summary>
        /// <value>
        /// The names of all iterator variables as simple menu items.
        /// </value>
        public List<MenuItem> iteratorNamesValue
        {
            get
            {
                return ConstructVariableMenuItems(VariableInputValue, Variables.VariablesIterator);
            }
        }
        /// <summary>
        /// This command is triggered is triggered when the user presses a keyboard button while the control is in focus
        /// </summary>
        public ICommand KeyDownPressedCommand { private set; get; }

        /// <summary>
        /// Gets or sets the command that is triggered when the move left button is clicked
        /// </summary>
        /// <value>
        /// The move left command.
        /// </value>
        public ICommand MoveLeftCommand { get; private set; }

        /// <summary>
        /// Gets or sets the command that is triggered when the move right button is clicked
        /// </summary>
        /// <value>
        /// The move right command.
        /// </value>
        public ICommand MoveRightCommand { get; private set; }

        /// <summary>
        /// Gets or sets the command that is triggered when the remove item button is clicked
        /// </summary>
        /// <value>
        /// The remove item command.
        /// </value>
        public ICommand RemoveItem { get; private set; }

        /// <summary>
        /// Gets or sets the command that is triggered when the set message button is clicked
        /// </summary>
        /// <value>
        /// The set message command.
        /// </value>
        public ICommand SetMessage { get; private set; }

        /// <summary>
        /// Gets the color of the set message button.
        /// </summary>
        /// <value>
        /// The color of the set message button.
        /// </value>
        public SolidColorBrush SetMessageColor
        {
            get
            {
                if (GetMessageState())
                {
                    return new SolidColorBrush(Colors.Red);
                }
                else
                {
                    if (GetMessageString() == null)
                    {
                        SetMessageString("");
                    }
                    if (GetMessageString().Length == 0)
                    {
                        return new SolidColorBrush(Colors.White);
                    }
                    return new SolidColorBrush(Colors.GreenYellow);
                }
            }
        }

        /// <summary>
        /// Gets the set message value.
        /// </summary>
        /// <value>
        /// The set message value.
        /// </value>
        public string SetMessageMessage
        {
            get { return GetMessageString(); }
        }

        /// <summary>
        /// Gets a value indicating whether a tool-tip has to be shown for the message.
        /// </summary>
        /// <value>
        /// <c>true</c> if [set message show tooltip]; otherwise, <c>false</c>.
        /// </value>
        public bool SetMessageShowTooltip
        {
            get
            {
                if (GetMessageString() == null)
                {
                    SetMessageString("");
                }
                if (GetMessageString().Length == 0)
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Gets the start time.
        /// </summary>
        /// <value>
        /// The start time.
        /// </value>
        /// <remarks>
        /// This getter has a side effect of setting the start time of the underlying step model
        /// </remarks>
        public double StartTime
        {
            get
            {
                double startTime = ((ChannelBasicController)Parent).StartTimeOf(this);

                return startTime;
            }
        }

        /// <summary>
        /// Gets the names of all static variables which can be used as durations for steps
        /// </summary>
        /// <value>
        /// The names of all static variables as simple menu items.
        /// </value>
        public List<MenuItem> staticNamesDuration
        {
            get
            {
                return ConstructStaticVariablesMenuItems(VariableInputDuration);
            }
        }

        //The following menu items will be used as context menu items when right-clicking on the value or the duration of a step.
        /// <summary>
        /// Gets the names of all static variables which can be used as values for steps
        /// </summary>
        /// <value>
        /// The names of all static variables as simple menu items.
        /// </value>
        public List<MenuItem> staticNamesValue
        {
            get
            {
                return ConstructStaticVariablesMenuItems(VariableInputValue);
            }
        }

        /// <summary>
        /// Gets the time unit.
        /// </summary>
        /// <value>
        /// The time unit.
        /// </value>
        public string Unit
        {
            get { return TimeSettingsInfo.GetInstance().TimeUnit; }
        }

        /// <summary>
        /// Gets or sets the command that is triggered when the user sets the duration manually
        /// </summary>
        /// <value>
        /// The duration of the user input command.
        /// </value>
        public ICommand UserInputDuration { get; private set; }

        /// <summary>
        /// Gets or sets the command that is triggered when the user sets the value manually
        /// </summary>
        /// <value>
        /// The user input value command.
        /// </value>
        public ICommand UserInputValue { get; private set; }

        /// <summary>
        /// Gets or sets the value of the step
        /// </summary>
        /// <value>
        /// The value of the step
        /// </value>
        /// <remarks>Automatically uses the variable if one is attached</remarks>
        public double Value
        {
            get
            {
                if (GetValueVariableName() == "" || GetValueVariableName() == null)
                {
                    return GetValue();
                }
                else
                {
                    return ValueVariable.VariableValue;
                }
            }
            set
            {
                if (GetValueVariableName() == "" || GetValueVariableName() == null)
                {
                    SetValue(value);
                }
                else
                {
                    SetValue(ValueVariable.VariableValue);
                    UpdateProperty("Value");
                }
            }
        }

        /// <summary>
        /// Gets the color of the value of the analog step.
        /// </summary>
        /// <value>
        /// The color of the value.
        /// </value>
        public SolidColorBrush ValueColor
        {
            get
            {
                if (GetValueVariableName() == "" || GetValueVariableName() == null)
                {
                    return Brushes.White;
                }
                if (!ValueVariable.changes)
                {
                    return Brushes.LightGreen;
                }

                return Brushes.Orange;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the value of the step is read-only.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the value of the step is read-only; otherwise, <c>false</c>.
        /// </value>
        public bool ValueIsReadOnly
        {
            get
            {
                if (GetValueVariableName() == "" || GetValueVariableName() == null)
                {
                    //System.Console.Write("Value: false\n");
                    return false;
                }
                //System.Console.Write("Value: true\n");
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the variable that is attached to this step's value
        /// </summary>
        /// <value>
        /// The value variable.
        /// </value>
        public VariableController ValueVariable
        {
            get
            {
                if (_valueVariable == null && GetValueVariableName() != null)
                {
                    if (!GetValueVariableName().Equals(VariableController.NO_VARIABLE))
                    {
                        ReattachVariable();
                    }
                }

                return _valueVariable;
            }
            set { _valueVariable = value; }
        }

        /// <summary>
        /// Gets the name of the value variable.
        /// </summary>
        /// <value>
        /// The name of the value variable.
        /// </value>
        public string ValueVariableName
        {
            get
            {
                //System.Console.Write("VVN: {0}\n", GetValueVariableName());
                return GetValueVariableName();
            }
        }

        /// <summary>
        /// Gets or sets the command that is triggered when the duration of the step is associated to a variable
        /// </summary>
        /// <value>
        /// The variable input value command.
        /// </value>
        public ICommand VariableInputDuration { get; private set; }

        /// <summary>
        /// Gets or sets the command that is triggered when the value of the step is associated to a variable
        /// </summary>
        /// <value>
        /// The variable input value command.
        /// </value>
        public ICommand VariableInputValue { get; private set; }
        /// <summary>
        /// Gets the variables' controller.
        /// </summary>
        /// <value>
        /// The variables' controller.
        /// </value>
        /// <exception cref="System.Exception">Variables == null!?!</exception>
        private VariablesController Variables
        {
            get
            {
                //_variables = _rootController.Variables;
                if (_variables == null)
                {
                    _variables = _rootController.Variables;

                    if (_variables == null)
                    {
                        throw new Exception("Variables == null!?!");
                    }

                    _variables.VariablesListChanged += VariablesListChanged;
                    _variables.VariablesValueChanged += VariablesValueChanged;
                    _variables.VariableTypeChanged += VariableTypeChanged;
                }

                return _variables;
            }
        }


        // ***************** Methods ********************
        // **********************************************

        // ************* Public Methods *****************
        // **********************************************
        /// <summary>
        /// Executed when the insert command is triggered.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        public void DoInsert(object parameter)
        {
            _parent.InsertStep(this);
        }

        /// <summary>
        /// Executes when the move left command is triggered.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        public void DoMoveLeft(object parameter)
        {
            _parent.MoveStep(this, ChannelBasicController.LeftRightEnum.Left);
        }

        /// <summary>
        /// Executed when the move right command is triggered.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        public void DoMoveRight(object parameter)
        {
            _parent.MoveStep(this, ChannelBasicController.LeftRightEnum.Right);
        }

        /// <summary>
        /// Performs the set message command
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        public void DoSetMessage(object parameter)
        {
            SetMessageWindowController controller = new SetMessageWindowController(new Tuple<bool, string>(GetMessageState(), GetMessageString()));
            Window window = WindowsHelper.CreateWindowToHostViewModel(controller, false);
            window.Width = 300;
            window.Height = 300;
            window.Title = "Step Message";

            bool? dialogResult = window.ShowDialog();

            if (dialogResult.HasValue && dialogResult.Value)
            {
                if (controller.Message != GetMessageString())
                {
                    SetMessageString(controller.Message);

                    if (null != this.PropertyChanged)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("SetMessageMessage"));
                        PropertyChanged(this, new PropertyChangedEventArgs("SetMessageShowTooltip"));
                        PropertyChanged(this, new PropertyChangedEventArgs("SetMessageColor"));
                    }
                }

                if (controller.CrirticalState != GetMessageState())
                {
                    SetMessageState(controller.CrirticalState);
                    if (null != this.PropertyChanged)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("SetMessageColor"));

                    }
                }
            }

        }

        //This function re-attaches variables after a file has been loaded.        
        /// <summary>
        /// Reattaches the variables after a file has been loaded.
        /// </summary>
        /// <exception cref="System.Exception">
        /// Variable is null
        /// </exception>
        public void ReattachVariable()
        {
            VariableController variable;

            if (GetValueVariableName() != null)
            {
                if (!GetValueVariableName().Equals(VariableController.NO_VARIABLE))
                {
                    variable = Variables.GetByName(GetValueVariableName());

                    if (variable != null)
                    {
                        SetValueVariableName(variable.VariableName);
                        ValueVariable = variable;
                        Value = ValueVariable.VariableValue;
                    }
                    else
                    {
                        throw new Exception("Variable is null");
                    }
                }
            }

            if (GetDurationVariableName() != null)
            {
                if (!GetDurationVariableName().Equals(VariableController.NO_VARIABLE))
                {
                    variable = Variables.GetByName(GetDurationVariableName());

                    if (variable != null)
                    {
                        SetDurationVariableName(variable.VariableName);
                        DurationVariable = variable;
                        Duration = DurationVariable.VariableValue;
                    }
                    else
                    {
                        throw new Exception("Variable is null");
                    }
                }
            }
        }

        /// <summary>
        /// Executed when the user sets the duration
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        public void SetUserInputDuration(object parameter)
        {
            SetDurationVariableName(VariableController.NO_VARIABLE);
            DurationVariable = null;
            if (null != this.PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("DurationColor"));
                PropertyChanged(this, new PropertyChangedEventArgs("DurationIsReadOnly"));
            }
        }

        /// <summary>
        /// Executed when the user sets the value.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        public void SetUserInputValue(object parameter)
        {
            SetValueVariableName(VariableController.NO_VARIABLE);
            ValueVariable = null;
            if (null != this.PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ValueColor"));
                PropertyChanged(this, new PropertyChangedEventArgs("ValueIsReadOnly"));
            }
        }

        /// <summary>
        /// Executed when the duration is set to a variable.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        public void SetVariableInputDuration(object parameter)
        {
            VariableController variable = (VariableController)parameter;

            SetDurationVariableName(variable.VariableName);
            DurationVariable = variable;
            Duration = DurationVariable.VariableValue;

            if (null != this.PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("DurationColor"));
                PropertyChanged(this, new PropertyChangedEventArgs("DurationIsReadOnly"));
            }
            UpdateProperty("DurationVariableName");
        }

        /// <summary>
        /// Executed when the value is set to a variable.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        public void SetVariableInputValue(object parameter)
        {
            VariableController variable = (VariableController)parameter;

            SetValueVariableName(variable.VariableName);
            ValueVariable = variable;
            Value = ValueVariable.VariableValue;

            if (null != this.PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ValueColor"));
                PropertyChanged(this, new PropertyChangedEventArgs("ValueIsReadOnly"));
            }

            UpdateProperty("ValueVariableName");
        }

        /// <summary>
        /// Updates the property.
        /// </summary>
        /// <param name="property">The property.</param>
        public void UpdateProperty(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        /// <summary>
        /// Executed when any of the variable lists change
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void VariablesListChanged(object sender, VariablesChangedEventArgs e)
        {
            if (null != PropertyChanged)
            {
                if (e.RefreshStatics)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("staticNamesDuration"));
                    PropertyChanged(this, new PropertyChangedEventArgs("staticNamesValue"));
                }

                if (e.RefreshDynamics)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("dynamicNamesValue"));
                    PropertyChanged(this, new PropertyChangedEventArgs("dynamicNamesDuration"));
                }

                if (e.RefreshIterators)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("iteratorNamesValue"));
                    PropertyChanged(this, new PropertyChangedEventArgs("iteratorNamesDuration"));
                }

            }
        }

        /// <summary>
        /// Executed when a specific variable changes its value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="changedVariable">The changed variable information.</param>
        private void VariablesValueChanged(object sender, VariableController changedVariable)
        {

            if (changedVariable.Equals(ValueVariable))
            {
                //System.Console.Write("VVC1! -{0}-\n", changedVariable.VariableName);
                Value = ValueVariable.VariableValue;
                SetValueVariableName(ValueVariable.VariableName);
                UpdateProperty("ValueVariableName");
                UpdateProperty("ValueColor");
            }
            if (changedVariable.Equals(DurationVariable))
            {
                //System.Console.Write("VVC2! -{0}-\n", changedVariable.VariableName);
                Duration = DurationVariable.VariableValue;
                SetDurationVariableName(DurationVariable.VariableName);
                UpdateProperty("DurationVariableName");
                UpdateProperty("DurationColor");
            }
        }

        /// <summary>
        /// Handles the evet that a variable has changed its type
        /// </summary>
        /// <param name="sender">The VariablesController</param>
        /// <param name="newController">The VariableController created after the change (it is differet from the current one!)</param>
        private void VariableTypeChanged(object sender, VariableController newController)
        {
            if (newController.VariableName == VariableController.NO_VARIABLE)
                throw new Exception("Changing the type of a variable that has no name is not supported!");

            if ((ValueVariable != null && newController.VariableName == ValueVariable.VariableName) ||
                (DurationVariable != null && newController.VariableName == DurationVariable.VariableName))
            {
                ReattachVariable();

                if (newController == ValueVariable)
                {
                    UpdateProperty("ValueColor");
                }

                if (newController == DurationVariable)
                {
                    UpdateProperty("DurationColor");
                }
            }
        }

        // *********** Non-public Methods ***************
        // **********************************************

        /// <summary>
        /// Gets the index the of step.
        /// </summary>
        /// <returns>The index of the step</returns>
        internal int IndexOfModel()
        {
            return _model.Index();
        }

        /// <summary>
        /// Gets the duration.
        /// </summary>
        /// <returns>The duration of the step in millis</returns>
        protected double GetDuration()
        {
            return _model.Duration.Value;
        }

        /// <summary>
        /// Gets the name of the duration variable.
        /// </summary>
        /// <returns>The name of the duration variable</returns>
        protected virtual string GetDurationVariableName()
        {
            return _model.DurationVariableName;
        }

        /// <summary>
        /// Gets the state of the message.
        /// </summary>
        /// <returns>The state of the message.</returns>
        protected bool GetMessageState()
        {
            return _model.MessageState;
        }

        /// <summary>
        /// Gets the message string.
        /// </summary>
        /// <returns>The string of the message</returns>
        protected string GetMessageString()
        {
            return _model.MessageString;
        }

        /// <summary>
        /// Gets the value of the step
        /// </summary>
        /// <returns></returns>
        protected double GetValue()
        {
            return _model.Value.Value;
        }

        /// <summary>
        /// Gets the name of the value variable.
        /// </summary>
        /// <returns>The name of the value variable.</returns>
        protected virtual string GetValueVariableName()
        {
            return _model.ValueVariableName;
        }

        /// <summary>
        /// Sets the duration and initiates a copy to buffer.
        /// </summary>
        /// <param name="duration">The duration.</param>
        protected void SetDuration(double duration)
        {

            if (duration == _model.Duration.Value)
            {
                return;
            }

            object token = _rootController.BulkUpdateStart();
            _model.Duration.Value = duration;

            var parent = ((ChannelBasicController)Parent);
            parent.CopyToBuffer();
            UpdateGroupDuration();
            parent.UpdateSteps(this);
            _rootController.BulkUpdateEnd(token);

        }

        /// <summary>
        /// Sets the name of the duration variable.
        /// </summary>
        /// <param name="value">The value.</param>
        protected virtual void SetDurationVariableName(string value)
        {
            _model.DurationVariableName = value;
            ((ChannelBasicController)Parent).CopyToBuffer();
        }

        /// <summary>
        /// Sets the state of the message.
        /// </summary>
        /// <param name="newState">if set to <c>true</c> then the step is critical.</param>
        protected void SetMessageState(bool newState)
        {
            _model.MessageState = newState;
        }

        /// <summary>
        /// Sets the message string.
        /// </summary>
        /// <param name="newString">The new string.</param>
        protected void SetMessageString(string newString)
        {
            _model.MessageString = newString;
        }

        /// <summary>
        /// Sets the value of the step and initiates a copy to buffer.
        /// </summary>
        /// <param name="value">The value.</param>
        protected void SetValue(double value)
        {
            _model.Value.Value = value;
            ((ChannelBasicController)Parent).CopyToBuffer();
        }

        /// <summary>
        /// Sets the name of the value variable.
        /// </summary>
        /// <param name="value">The value.</param>
        protected virtual void SetValueVariableName(string value)
        {
            _model.ValueVariableName = value;
            ((ChannelBasicController)Parent).CopyToBuffer();
        }

        /// <summary>
        /// Updates the duration of the whole model
        /// </summary>
        protected void UpdateGroupDuration()
        {
            var payload = new EventPayload(EventPayload.DestinationType.Group, EventPayload.ActionType.UpdateTime);
            ((WindowGroupController)Group()).OnGroupUpdate(this, payload);
        }

        /// <summary>
        /// Constructs the static variables menu items.
        /// </summary>
        /// <returns>A collection of menu items that represents the groups of static variables and the contained variables as well.</returns>
        private List<MenuItem> ConstructStaticVariablesMenuItems(ICommand commandOfSelection)
        {
            List<MenuItem> result = new List<MenuItem>();
            List<MenuItem> currentSubList;
            MenuItem current;
            List<KeyValuePair<int, string>> dictionaryAsList = new List<KeyValuePair<int, string>>(Variables.GroupNames);
            dictionaryAsList.Sort(
                (item1, item2) =>
                {
                    return item1.Value.CompareTo(item2.Value);
                }
            );


            foreach (KeyValuePair<int, string> group in dictionaryAsList)
            {
                ObservableCollection<VariableController> variablesOfGroup = Variables.GetVariablesOfGroup(group.Key);

                if (variablesOfGroup.Count > 0)
                {
                    current = new MenuItem();
                    current.Header = group.Value;
                    currentSubList = ConstructVariableMenuItems(commandOfSelection, variablesOfGroup);
                    current.HorizontalAlignment = HorizontalAlignment.Stretch;

                    foreach (MenuItem subItem in currentSubList)
                        current.Items.Add(subItem);

                    result.Add(current);
                }
            }
            return result;
        }

        /// <summary>
        /// Constructs the menu items corresponding to a collection of variables.
        /// </summary>
        /// <param name="variableInput">The variable input.</param>
        /// <param name="variableControllerCollection">The variable controller collection.</param>
        /// <returns></returns>
        private List<MenuItem> ConstructVariableMenuItems(ICommand variableInput, ObservableCollection<VariableController> variableControllerCollection)
        {
            //checkVariablesDefined();

            List<MenuItem> menuList = new List<MenuItem>();
            MenuItem item;
            foreach (VariableController variable in variableControllerCollection)
            {
                item = new MenuItem();
                item.Header = variable.VariableName;
                item.Command = variableInput;
                item.CommandParameter = variable;
                item.HorizontalAlignment = HorizontalAlignment.Stretch;

                if (!variable.VariableName.Equals(""))
                {
                    menuList.Add(item);
                }
            }

            return menuList;

        }

        private void OnPreviewKeyDown(object parameter)
        {
            KeyEventArgs e = (KeyEventArgs)parameter;

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.Left)
                {
                    DoMoveLeft(null);
                    e.Handled = true;
                }
                else if (e.Key == Key.Right)
                {
                    DoMoveRight(null);
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Executed when the remove command is triggered.
        /// </summary>
        /// <param name="delete">The parameter.</param>
        private void Remove(object delete)
        {
            System.Console.WriteLine("Remove!");
            object token = _rootController.BulkUpdateStart();
            ((ChannelBasicController)Parent).RemoveStep(this);
            UpdateGroupDuration();
            _rootController.BulkUpdateEnd(token);
        }

        /// <summary>
        /// Executed when the digital value button is pressed.
        /// </summary>
        /// <param name="caller">The caller.</param>
        private void SetDigitalValue(object caller)
        {
            if (Math.Abs(GetValue() - 1) < 0.001)
            {
                SetValue(0);
            }
            else
            {
                if (Math.Abs(GetValue()) < 0.001)
                    SetValue(1);
            }

            UpdateProperty("ButtonColor");
        }

    }
}
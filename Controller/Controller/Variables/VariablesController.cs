﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Buffer.Basic;
using Communication;
using Communication.Commands;
using Communication.Interfaces.Controller;
using Controller.Control;
using Controller.MainWindow;
using Errors.Error;
using Model.Options;
using Model.Root;
using Model.Variables;
using PythonUtils;
using PythonUtils.ScriptExecution;

namespace Controller.Variables
{
    public class VariablesChangedEventArgs : EventArgs
    {
        public bool RefreshStatics { set; get; }
        public bool RefreshIterators { set; get; }
        public bool RefreshDynamics { set; get; }
    }
    public class VariablesController : INotifyPropertyChanged, IWindowController, IController
    {
        // ******************** variables ********************
        const double FLOATMARGIN = 1E-7;

        /// <summary>
        /// The height (in pixels) of a single variable in the variables window
        /// </summary>
        private const int VARIABLE_HEIGHT = 28;
        /// <summary>
        /// The maximum number of static variables per one column in a group
        /// </summary>
        private int staticVariablesPerGroupColumn;

        /// <summary>
        /// Gets or sets the static variables per group column.
        /// </summary>
        /// <value>
        /// The static variables per group column.
        /// </value>
        public int StaticVariablesPerGroupColumn
        {
            get { return staticVariablesPerGroupColumn; }
            set
            {
                staticVariablesPerGroupColumn = value;
                OnPropertyChanged("StaticVariablesPerGroupColumn");
                OnPropertyChanged("StaticGroupHeight");
            }
        }

        /// <summary>
        /// Gets the maximum height of a static variables group measured in pixels
        /// </summary>
        /// <value>
        /// The height of the static group.
        /// </value>
        public double StaticGroupHeight
        {
            get
            {
                return (double)VARIABLE_HEIGHT * StaticVariablesPerGroupColumn;
            }
        }

        private Root.RootController _parentController;

        //The Variables Model containing all Variables.
        public RootModel _rootModel;
        public VariablesModel _variablesModel;
        private readonly OutputHandler _outputHandler;

        public int numberOfIterations;

        private Dictionary<int, bool> groupsExpandState;


        // ******************** events ********************
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion


        //Events that occur when the Variables are changed (Values) or the order and Variable Types are changed (List).
        public event VariablesValueChangedEventHandler VariablesValueChanged;
        public event VariablesListChangedEventHandler VariablesListChanged;
        public event LoseFocusOnIterators LoseFocus;

        public event EventHandler RefreshWindows;

        public void DoRefreshWindows()
        {
            if (RefreshWindows != null)
            {
                RefreshWindows(this, new EventArgs());
            }
        }

        public delegate void VariablesListChangedEventHandler(object sender, VariablesChangedEventArgs e);
        public delegate void VariablesValueChangedEventHandler(object sender, VariableController changedVar);

        public delegate void LoseFocusOnIterators(object sender, EventArgs e);

        public ICommand Check { get; private set; }
        public ICommand Iterate { get; private set; }
        public ICommand Evaluate { get; private set; }
        public ICommand AddStatic { get; private set; }
        public ICommand AddIterator { get; private set; }
        public ICommand AddDynamic { get; private set; }
        public ICommand StaticGroupSelect { get; set; }
        private ICommand MoveVariableToNewGroupCommand { set; get; }


        // ******************** properties ********************

        public ObservableCollection<ShowableWindow> WindowsList
        {
            get
            {
                return MainWindowController.WindowsList;
            }
        }


        public Root.RootController GetRootController()
        {
            return _parentController;
        }

        public Dictionary<int, string> GroupNames
        {
            get { return _variablesModel.GroupNames; }
            set { _variablesModel.GroupNames = value; }
        }

        public Dictionary<int, bool> GroupsExpandState
        {
            get
            {
                if (groupsExpandState == null)
                {
                    groupsExpandState = new Dictionary<int, bool>();

                    foreach (int key in GroupNames.Keys)
                        groupsExpandState.Add(key, true);
                }

                return groupsExpandState;
            }
        }

        public List<MenuItem> constructStaticGroups(VariableController variable)
        {
            List<MenuItem> menuList = new List<MenuItem>();
            MenuItem item;
            List<KeyValuePair<int, string>> dictionaryAsList = new List<KeyValuePair<int, string>>(GroupNames);
            dictionaryAsList.Sort(
                (item1, item2) =>
                {
                    return item1.Value.CompareTo(item2.Value);
                }
            );


            foreach (KeyValuePair<int, string> group in dictionaryAsList)
            {
                item = new MenuItem();
                item.Header = group.Value;
                item.Command = StaticGroupSelect;
                item.CommandParameter = new Tuple<VariableController, int>(variable, group.Key);
                menuList.Add(item);
            }


            item = new MenuItem();
            item.Header = "Create New Group ...";
            item.Command = MoveVariableToNewGroupCommand;
            item.CommandParameter = variable;
            item.Background = SystemColors.HighlightBrush;
            item.Foreground = SystemColors.HighlightTextBrush;
            menuList.Add(item);

            return menuList;
        }

        /// <summary>
        /// Moves the variable to a new group with a default name.
        /// </summary>
        /// <param name="parameter">The <see cref=" VariableController"/> of the corresponding variable.</param>
        private void MoveVariableToNewGroup(object parameter)
        {
            VariableController variable = (VariableController)parameter;
            int groupKey = -1;

            foreach (int key in GroupNames.Keys)
                if (key > groupKey)
                    groupKey = key;

            groupKey++;
            GroupNames.Add(groupKey, "New Group " + groupKey);
            GroupsExpandState.Add(groupKey, true);
            variable.GroupIndex = groupKey;
            updateStatics();
        }

        public void DoStaticGroupSelect(object parameter)
        {
            Tuple<VariableController, int> realPar = parameter as Tuple<VariableController, int>;
            realPar.Item1.GroupIndex = realPar.Item2;
            updateStatics();
        }

        //ObservableCollections containing all Variables and only show specified Variables if needed.
        public ObservableCollection<VariableController> Variables = new ObservableCollection<VariableController>();

        /// <summary>
        /// Gets a sorted collection of the controllers of static variables
        /// </summary>
        /// <value>
        /// The static variables' controllers
        /// </value>
        /// <remarks>The collection is sorted based on the group name, then on the fact whether the variable is a 
        /// group header or a regular variable, and finally based on the variable name.</remarks>
        public ObservableCollection<VariableController> VariablesStatic
        {
            get
            {
                ObservableCollection<VariableController> returnCollection =
                    new ObservableCollection<VariableController>(Variables);

                foreach (KeyValuePair<int, string> group in GroupNames)//Add group headers as variable controllelrs
                {
                    VariableModel vM = new VariableModel();
                    VariableController vC = new VariableController(vM, this);
                    vM.TypeOfVariable = VariableType.VariableTypeStatic;
                    vM.groupIndex = group.Key;
                    vC.IsGroupHeader = true;
                    returnCollection.Add(vC);
                }

                return
                    new ObservableCollection<VariableController>(from tempVarController in new
                    ObservableCollection
                        <VariableController>(
                    returnCollection.Where(//filter out non-static variable controllers
                        w =>
                        w.TypeOfVariable.Equals(
                            VariableType.
                                VariableTypeStatic)))//order the result
                                                                 orderby tempVarController.GroupName ascending, tempVarController.IsGroupHeader descending, tempVarController.VariableName ascending
                                                                 select tempVarController);
            }
        }
        public ObservableCollection<VariableController> VariablesIterator
        {
            get
            {
                return new ObservableCollection<VariableController>(from varCtrl in
                                                                        new
                                                                        ObservableCollection
                                                                            <VariableController>(
                                                                        Variables.Where(
                                                                            w =>
                                                                            w.TypeOfVariable.Equals(
                                                                                VariableType.
                                                                                    VariableTypeIterator)))
                                                                    orderby varCtrl.getModelIndex ascending
                                                                    select varCtrl);
            }
        }
        public ObservableCollection<VariableController> VariablesDynamic
        {
            get
            {
                return new ObservableCollection<VariableController>(from varCtrl in
                                                                        new
                                                                        ObservableCollection
                                                                            <VariableController>(
                                                                        Variables.Where(
                                                                            w =>
                                                                            w.TypeOfVariable.Equals(
                                                                                VariableType.
                                                                                    VariableTypeDynamic)))
                                                                    orderby varCtrl.getModelIndex ascending
                                                                    select varCtrl);
            }
        }

        /// <summary>
        /// Returns a variable by its name.
        /// </summary>
        /// <param name="name">Name of the variable to find</param>
        /// <returns>the variable specified by name or null if it cannot be found</returns>
        public VariableController GetByName(String name)
        {
            foreach (VariableController variable in Variables)
            {
                if (name.Equals(variable.VariableName))
                {
                    return variable;
                }
            }
            throw new Exception("Variable not found! Name: " + name);
            //return null;
        }

        // ******************** lock stuff ********************
        /// <summary>
        /// avoid inconsistency the data should not be copied to the buffer until all updates are done. In _variableUpdateLockObject the very first lock object is stored, all objects from sub updates are not stored. The updateVariablesListFromParent is prevented until the lock is released. 
        /// </summary>
        //private Object _variableUpdateLockObject = null;

        /// <summary>
        /// disables the CopyToBuffer function until a VariablesUpdateDone with the object _variableUpdateLockObject is called 
        /// </summary>
        public Object VariableUpdateStart()
        {
            //var lockObject = new object();
            //if (_variableUpdateLockObject == null)//Only who has the first lock wins!!
            //{
            //    _variableUpdateLockObject = lockObject;
            //}
            //_parentController.DisableCopyToBuffer();
            //return lockObject;

            return _parentController.BulkUpdateStart();
        }

        /// <summary>
        /// re-enables and triggers the CopyToBuffer function if the object in the argument matches the _variableUpdateLockObject
        /// </summary>
        /// <param name="lockObject">lock/unlock object</param>
        public bool VariableUpdateDone(Object lockObject)
        {
            //if (_variableUpdateLockObject == lockObject)
            //{
            //    _variableUpdateLockObject = null;
            //    _parentController.EnableCopyToBufferAndCopyChanges();
            //}

            return _parentController.BulkUpdateEnd(lockObject);
        }

        void LockIterators(object sender, EventArgs e)
        {
            this.LoseFocus(null, null);

            // ResetIteratorValues();
            evaluate(null);
            _iteratorsLocked = true;
            System.Console.WriteLine("Locked iterators");
            foreach (VariableController iterator in VariablesIterator)
            {
                iterator.VariableLocked = true;
            }
        }

        void UnlockIterators(object sender, EventArgs e)
        {
            _iteratorsLocked = false;
            foreach (VariableController iterator in VariablesIterator)
            {
                iterator.VariableLocked = false;
            }
            System.Console.WriteLine("Unlocked iterators");
        }

        private bool _iteratorsLocked = false;

        public bool IsIteratorsLocked()
        {
            return _iteratorsLocked;
        }


        // ******************** constructor ********************
        /// <summary>
        /// Public constructor for the Variables Controller.
        /// </summary>
        /// <param name="variablesModel">The variables model which is within the complete data model.</param>
        public VariablesController(VariablesModel variablesModel, OutputHandler outputHandler, RootModel rootModel)
        {
            _outputHandler = outputHandler;
            SetNewRootModel(rootModel);
            SetNewVariablesModel(variablesModel);
            _outputHandler.LockIterators += LockIterators;
            _outputHandler.UnlockIterators += UnlockIterators;
            AddStatic = new RelayCommand(addStatic);
            AddIterator = new RelayCommand(addIterator);
            AddDynamic = new RelayCommand(addDynamic);
            Evaluate = new RelayCommand(evaluate);
            Iterate = new RelayCommand(iterate);
            Check = new RelayCommand(CheckAllVariablesUsage);
            StaticGroupSelect = new RelayCommand(DoStaticGroupSelect);
            MoveVariableToNewGroupCommand = new RelayCommand(MoveVariableToNewGroup);

            ReadOptions();
        }


        public void countTotalNumberOfIterations()
        {
            int count = 1;
            foreach (var variable in VariablesIterator)
            {
                double currentVal = variable.VariableStartValue;
                int localCount = 1;
                while (true)
                {
                    currentVal += variable.VariableStepValue;
                    if (((currentVal > variable.VariableEndValue + FLOATMARGIN && variable.VariableStepValue >= 0) || (currentVal < variable.VariableEndValue - FLOATMARGIN && variable.VariableStepValue < 0)) || variable.VariableStepValue == 0)
                    {
                        break;
                    }
                    localCount++;
                }
                count = count * localCount;
            }
            numberOfIterations = count;
            _outputHandler.NumberOfIterations = count;

        }

        /// <summary>
        /// Sets a new model for the variables
        /// </summary>
        /// <param name="variablesModel">the model which will replace the old one</param>
        public void SetNewVariablesModel(VariablesModel variablesModel)
        {
            this._variablesModel = variablesModel;
            Variables.Clear();
            foreach (VariableModel variable in variablesModel.VariablesList)
            {
                Variables.Add(new VariableController(variable, this));
            }
            UpdateVariablesList();
            countTotalNumberOfIterations();
        }

        /// <summary>
        /// Sets a new RootModel for the variables
        /// </summary>
        /// <param name="variablesModel">the model which will replace the old one</param>
        public void SetNewRootModel(RootModel rootModel)
        {
            this._rootModel = rootModel;
            groupsExpandState = null;
        }



        /// <summary>
        /// A function that will be called if the Buffer wants to iterate(and evaluate) the Variables
        /// </summary>
        /// <param name="sender">unused</param>
        /// <param name="e">unused</param>
        public void IterateVariablesFromBuffer(object sender, EventArgs e)
        {
            iterate(null);
        }

        /// <summary>
        /// A function that will be called if the Buffer wants to evaluate the Variables
        /// </summary>
        /// <param name="sender">unused</param>
        /// <param name="e">unused</param>
        public void EvaluateVariablesFromBuffer(object sender, EventArgs e)
        {
            evaluate(null);
        }

        public void ResetIteratorValuesFromBuffer(object sender, EventArgs e)
        {
            ResetIteratorValues();
            evaluate(null);
        }

        public void moveUp(VariableController variableController)
        {
            if (variableController.TypeOfVariable == VariableType.VariableTypeIterator
                || variableController.TypeOfVariable == VariableType.VariableTypeDynamic)
            {
                int currentModelIndex = _variablesModel.VariablesList.IndexOf(variableController._model);

                if (currentModelIndex > 0)
                {
                    // find the variable to swap with   
                    VariableModel previousVariable = null;
                    int previousModelIndex = currentModelIndex;

                    do
                    {
                        --previousModelIndex;
                        previousVariable = _variablesModel.VariablesList[previousModelIndex];
                    }
                    while (!previousVariable.TypeOfVariable.Equals(variableController.TypeOfVariable) && previousModelIndex > 0);

                    // if we managed to find a suitable step, swap it with the current one
                    if (previousModelIndex > 0)
                    {
                        VariableModel currentVariable = _variablesModel.VariablesList[currentModelIndex];
                        _variablesModel.VariablesList[currentModelIndex] = previousVariable;
                        _variablesModel.VariablesList[previousModelIndex] = currentVariable;
                        updatIterators();
                        updateDynamics();
                        countTotalNumberOfIterations();
                    }
                }
            }
            else if (variableController.TypeOfVariable == VariableType.VariableTypeStatic)
            {
                if (variableController.GroupIndex > 0)
                {
                    variableController.GroupIndex--;
                    updateStatics();
                }
            }
        }

        public void moveDown(VariableController variableController)
        {
            if (variableController.TypeOfVariable == VariableType.VariableTypeIterator
                || variableController.TypeOfVariable == VariableType.VariableTypeDynamic)
            {
                int currentModelIndex = _variablesModel.VariablesList.IndexOf(variableController._model);

                if (currentModelIndex < _variablesModel.VariablesList.Count - 1)
                {
                    // find the variable to swap with   
                    VariableModel nextVariable = null;
                    int nextModelIndex = currentModelIndex;

                    do
                    {
                        ++nextModelIndex;
                        nextVariable = _variablesModel.VariablesList[nextModelIndex];
                    }
                    while (!nextVariable.TypeOfVariable.Equals(variableController.TypeOfVariable)
                        && nextModelIndex < _variablesModel.VariablesList.Count - 1);

                    // if we managed to find a suitable step, swap it with the current one
                    if (nextModelIndex < _variablesModel.VariablesList.Count - 1)
                    {
                        VariableModel currentVariable = _variablesModel.VariablesList[currentModelIndex];
                        _variablesModel.VariablesList[currentModelIndex] = nextVariable;
                        _variablesModel.VariablesList[nextModelIndex] = currentVariable;
                        updatIterators();
                        updateDynamics();
                        countTotalNumberOfIterations();
                    }
                }
            }
            if (variableController.TypeOfVariable == VariableType.VariableTypeStatic)
            {
                variableController.GroupIndex++;
                updateStatics();
            }
        }

        /*public void UpdateSpecificVarialbesCollection(CustomElements.VariableType varType)
        {
            switch (varType)
            {
                case VariableType.VariableTypeStatic: 
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("VariablesStatic"));
                        }
                        break;
                case VariableType.VariableTypeIterator:
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("VariablesIterator"));
                        }
                        break;
                case VariableType.VariableTypeDynamic:
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("VariablesDynamic"));
                        }
                        break;
        }
            //System.Console.Write("b\n");
            if (VariablesListChanged != null)
            {
                VariablesListChanged(this, null);
            }
        }*/

        /// <summary>
        /// This function gets called when the Lists of Variables are changed.
        /// </summary>
        public void UpdateVariablesList()//This function deletes the focus of an input field
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("VariablesStatic"));
                PropertyChanged(this, new PropertyChangedEventArgs("VariablesIterator"));
                PropertyChanged(this, new PropertyChangedEventArgs("VariablesDynamic"));
            }
            if (VariablesListChanged != null)
            {
                VariablesListChanged(this, new VariablesChangedEventArgs() { RefreshStatics = true, RefreshIterators = true, RefreshDynamics = true });
            }
        }

        public void UpdateVariablesListNoValues()//This function deletes the focus of an input field
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("VariablesStatic"));
                PropertyChanged(this, new PropertyChangedEventArgs("VariablesIterator"));
                PropertyChanged(this, new PropertyChangedEventArgs("VariablesDynamic"));
            }
        }

        public void updateStatics()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VariablesStatic"));
        }
        public void updatIterators()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VariablesIterator"));
        }
        public void updateDynamics()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VariablesDynamic"));
        }



        /* refresh the gui */
        public void RefreshVariableValuesInGUI(bool refreshStatics, bool refreshIterators, bool refreshDynamics)
        {
            VariablesListChanged?.Invoke(this, new VariablesChangedEventArgs() { RefreshDynamics = refreshDynamics, RefreshIterators = refreshIterators, RefreshStatics = refreshStatics });
        }

        public void DoVariablesValueChanged(VariableController variable)
        {
            if (VariablesValueChanged != null)
            {
                VariablesValueChanged(this, variable);
            }
            _parentController.CopyToBuffer();
        }

        public void DoVariablesValueChangedByName(string name)
        {
            foreach (VariableController var in Variables)
            {
                if (var.VariableName == name)
                {
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(var, new PropertyChangedEventArgs("VariableValue"));
                    }
                    if (VariablesValueChanged != null)
                    {
                        VariablesValueChanged(this, var);
                    }
                    _parentController.CopyToBuffer();
                }
            }
        }

        public void RemoveVariable(VariableController variable)
        {
            this.CheckAllVariablesUsage(null);

            if (variable.Used)
            {
                MessageBoxResult result = MessageBox.Show(String.Format("This Variable is used in \n{0}, you cannot delete it!", variable.UsagesAsString));
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("Do you really want to delete this Variable?\n" + variable.VariableName, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    VariableModel localModel = variable._model;
                    Variables.Remove(variable);
                    _variablesModel.deleteVariable(localModel);
                    UpdateVariablesList();
                }

            }
        }

        public void RemoveGroup(int groupKey)
        {
            if (GetVariablesOfGroup(groupKey).Count > 0)
            {
                MessageBox.Show("The group you are trying to remove contains variables. Please ensure that the group is empty before trying to remove it.",
                    "Unable to Remove Group", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                GroupNames.Remove(groupKey);
                GroupsExpandState.Remove(groupKey);
                updateStatics();
            }
        }

        public void RenameGroup(int groupKey, string groupName)
        {
            bool isNameUsed = false;
            foreach (string name in GroupNames.Values)
            {
                if (groupName.Equals(name))
                {
                    isNameUsed = true;
                    break;
                }
            }

            if (isNameUsed)
                MessageBox.Show("This group name is already in use please choose another one!", "Cannot Rename Group", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                GroupNames[groupKey] = groupName;
                updateStatics();
            }
        }

        public ObservableCollection<VariableController> GetVariablesOfGroup(int groupKey)
        {
            ObservableCollection<VariableController> result = new ObservableCollection<VariableController>();

            foreach (VariableController vController in Variables)
            {
                if (vController.TypeOfVariable == VariableType.VariableTypeStatic && vController.GroupIndex == groupKey)
                    result.Add(vController);

            }

            return result;
        }

        /// <summary>
        /// Adds a static Variable
        /// </summary>
        /// <param name="parameter">unused</param>
        public void addStatic(object parameter)
        {
            VariableModel variableModel = _variablesModel.addVariable();
            VariableController variable = new VariableController(variableModel, this);
            variable.TypeOfVariable = VariableType.VariableTypeStatic;
            Variables.Add(variable);

            if (!GroupNames.ContainsKey(0))
            {
                GroupNames.Add(0, "Default Group");

                if (!GroupsExpandState.ContainsKey(0))
                    GroupsExpandState.Add(0, true);
            }
            //UpdateSpecificVarialbesCollection(VariableType.VariableTypeStatic);
            UpdateVariablesList();
        }

        /// <summary>
        /// Adds an iterator variable
        /// </summary>
        /// <param name="parameter">unused</param>
        public void addIterator(object parameter)
        {
            VariableModel variableModel = _variablesModel.addVariable();

            VariableController variable = new VariableController(variableModel, this);
            variable.TypeOfVariable = VariableType.VariableTypeIterator;
            Variables.Add(variable);
            //UpdateSpecificVarialbesCollection(VariableType.VariableTypeIterator);
            UpdateVariablesList();

            countTotalNumberOfIterations();
        }

        /// <summary>
        /// Adds a dynamic variable
        /// </summary>
        /// <param name="parameter">unused</param>
        public void addDynamic(object parameter)
        {
            VariableModel variableModel = _variablesModel.addVariable();

            VariableController variable = new VariableController(variableModel, this);
            variable.TypeOfVariable = VariableType.VariableTypeDynamic;
            Variables.Add(variable);
            //UpdateSpecificVarialbesCollection(VariableType.VariableTypeDynamic);
            UpdateVariablesList();
        }


        public void ResetIteratorValues()
        {
            // Ebaa 18.06.2018
            // prevent inconsistencies and multiple updates on the buffer
            // Object bufferUpdateLock = VariableUpdateStart();
            foreach (VariableController iterator in VariablesIterator)
            {

                iterator.VariableValue = iterator.VariableStartValue;

                // //test 11.06
                // VariableModel VariableModelToBeReset = _outputHandler.Model.Data.variablesModel.VariablesList.Find(x => x.VariableName == iterator._model.VariableName);


                ////test 11.06
                // if (VariableModelToBeReset != null)
                //  {

                //      VariableModelToBeReset.VariableValue = iterator.VariableStartValue;
                //  }
                ////// MessageBox.Show("Iterator:"+ iterator._model.VariableName+"\n Value:"+ iterator.VariableValue+ "\n Model Vlaue:"+iterator._model.VariableValue);
            }
            RefreshVariableValuesInGUI(false, true, true);
            // Ebaa 18.06.2018
            // reenable buffer updates
            // VariableUpdateDone(bufferUpdateLock);
        }
        List<List<VariableModel>> iterationPattern = new List<List<VariableModel>>();
        public void createIterationPattern()
        {
            List<VariableModel> localIterators = new List<VariableModel>();
            iterationPattern = new List<List<VariableModel>>();

            foreach (VariableController ctrl in VariablesIterator)
            {
                localIterators.Add(ctrl._model.DeepClone());
            }


            int ctr = 0;
            bool lastVariableOverflowed = false;
            while (!lastVariableOverflowed)
            {
                List<VariableModel> toAddIterators = new List<VariableModel>();


                foreach (VariableModel ctrl in localIterators)
                {
                    toAddIterators.Add(ctrl.DeepClone());
                }


                iterationPattern.Add(new List<VariableModel>(new List<VariableModel>(toAddIterators)));
                ctr++;
                lastVariableOverflowed = true;
                foreach (VariableModel iterator in localIterators)
                {
                    // only increase variable if the one before had an overflow
                    if (lastVariableOverflowed)
                    {
                        // first CheckAllVariablesUsage whether is this variable is not changing at all
                        if (iterator.VariableStepValue == 0)
                        {
                            lastVariableOverflowed = true;
                            iterator.VariableValue = iterator.VariableStartValue;
                            continue;
                        }

                        double nextValue = iterator.VariableValue + iterator.VariableStepValue;
                        const double FLOATMARGIN = 1E-7;
                        if ((nextValue <= iterator.VariableEndValue + FLOATMARGIN && iterator.VariableStepValue > 0) ||
                            (nextValue >= iterator.VariableEndValue - FLOATMARGIN && iterator.VariableStepValue < 0))
                        {
                            lastVariableOverflowed = false;
                            iterator.VariableValue = nextValue;
                        }
                        else
                        {
                            lastVariableOverflowed = true;
                            iterator.VariableValue = iterator.VariableStartValue;
                        }
                    }

                }
            }
            iterationPattern = Shuffle(iterationPattern);
            System.Console.Write("Everything should be saved into localIterators and localDynamics\n");

        }

        public List<TValue> Shuffle<TValue>(
           List<TValue> source)
        {
            Random r = new Random();
            return source.OrderBy(x => r.Next())
               .ToList();
        }

        //private bool shuffleIterations = true;
        //private bool pause = false;
        private int shuffleIterationsCounter = 0;


        /// <summary>
        /// Iterates the variables
        /// </summary>
        /// <param name="parameter">null</param>
        public void iterate(object parameter)
        {

            // prevent inconsistencies and multiple updates on the buffer
            Object bufferUpdateLock = VariableUpdateStart();

            //createIterationPattern();
            if (!_outputHandler.shuffleIterations)
            {
                shuffleIterationsCounter = 0;
            }
            if (shuffleIterationsCounter == 0 && _outputHandler.shuffleIterations)
            {
                createIterationPattern();
            }


            if (_outputHandler.shuffleIterations)
            {
                if (shuffleIterationsCounter >= iterationPattern.Count)
                {
                    shuffleIterationsCounter = 0;
                }
                for (int i = 0; i < VariablesIterator.Count; i++)
                {
                    VariablesIterator[i].VariableValue = iterationPattern[shuffleIterationsCounter][i].VariableValue;
                }
                shuffleIterationsCounter++;
            }

            if (!_outputHandler.shuffleIterations)
            {
                bool lastVariableOverflowed = true;


                foreach (VariableController iterator in VariablesIterator)
                {
                    //  to be deleted
                    //  VariableModel VariableModelToBeReset = _outputHandler.Model.Data.variablesModel.VariablesList.Find(x => x.VariableName == iterator._model.VariableName);
                    // only increase variable if the one before had an overflow
                    if (lastVariableOverflowed)
                    {
                        // first CheckAllVariablesUsage whether is this variable is not changing at all
                        if (iterator.VariableStepValue == 0)
                        {
                            lastVariableOverflowed = true;
                            iterator.VariableValue = iterator.VariableStartValue;
                            //to be deleted
                            //  VariableModelToBeReset.VariableValue = VariableModelToBeReset.VariableStartValue;
                            continue;
                        }

                        double nextValue = iterator.VariableValue + iterator.VariableStepValue;
                        //to be deleted
                        //  nextValue = VariableModelToBeReset.VariableValue + VariableModelToBeReset.VariableStepValue;
                        const double FLOATMARGIN = 1E-7;
                        if ((nextValue <= iterator.VariableEndValue + FLOATMARGIN && iterator.VariableStepValue > 0) ||
                            (nextValue >= iterator.VariableEndValue - FLOATMARGIN && iterator.VariableStepValue < 0))
                        {
                            lastVariableOverflowed = false;
                            iterator.VariableValue = nextValue;
                            //  _outputHandler.Model.Data.variablesModel.VariablesList.ElementAt(0).VariableValue = nextValue;
                            //to be deleted
                            //     VariableModelToBeReset.VariableValue = nextValue;
                            //   MessageBox.Show("inside iterate method"+iterator.VariableValue.ToString() + "\n");
                            //  MessageBox.Show( "inside iterate method variableModel: " + _rootModel.Data.variablesModel.VariablesList.ElementAt(0).VariableValue.ToString());

                        }
                        else
                        {
                            lastVariableOverflowed = true;
                            iterator.VariableValue = iterator.VariableStartValue;
                        }
                    }

                }
                // RefreshVariableValuesInGUI(); not required -> done in evaluate
            }

            evaluate(null);

            // reenable buffer updates
            VariableUpdateDone(bufferUpdateLock);
        }

        /// <summary>
        /// evaluates the Variables
        /// </summary>
        /// <param name="parameter">null</param>
        public void evaluate(object parameter)
        {
            // prevent inconsistencies and multiple updates on the buffer
            Object bufferUpdateLock = VariableUpdateStart();
            ErrorCollector errorCollector = ErrorCollector.Instance;
            errorCollector.RemoveErrorsOfWindowAndType(ErrorWindow.Variables, ErrorTypes.DynamicCompileError);

            HighPerformancePythonScriptExecutorBuilder builder = new HighPerformancePythonScriptExecutorBuilder();
            builder.SetOutputVariableName("val");
            builder.SetInputVariableNames(null);
            builder.SetModelVariableValues(_parentController.returnModel.Data, false);
            builder.AddGlobalVariablesToScope();
            builder.SetScript("pass");
            HighPerformancePythonScriptExecutor executer = (HighPerformancePythonScriptExecutor)builder.Build();//One executer for all variables

            foreach (VariableController dynamicVariable in VariablesDynamic)
            {
                try
                {
                    double result = 0;
                    executer.Script = dynamicVariable.VariableCode;//Change the script to the code of the current variable (syntax errors are discovered here)
                    result = executer.Execute();
                    dynamicVariable.VariableValue = result;
                    executer.SetVariableValue(dynamicVariable.VariableName, dynamicVariable.VariableValue);
                }
                catch (FormatException e)
                {
                    errorCollector.AddError(string.Format("Python val = expression does not return a Int32, Int64 or Double! Details: {0}\nAt dynamic variable: {1}", e.Message, dynamicVariable.VariableName), ErrorWindow.Variables, false, ErrorTypes.DynamicCompileError);
                }
                catch (Exception e)
                {
                    errorCollector.AddError(e.Message + "\nAt dynamic variable: " + dynamicVariable.VariableName, ErrorWindow.Variables, false, ErrorTypes.DynamicCompileError);
                }

            }

            // reenable buffer updates
            VariableUpdateDone(bufferUpdateLock);
            RefreshVariableValuesInGUI(false, true, true);
            System.Console.Write("Eval done!\n");
        }

        /// <summary>
        /// Checks the usage of all variables
        /// </summary>
        /// <param name="parameter">null</param>
        public void CheckAllVariablesUsage(object parameter)
        {
            VariableUsageChecker checker = new VariableUsageChecker(_rootModel);

            foreach (VariableController variable in Variables)
            {
                variable.ClearUsages();
                string currentUsage = "";

                foreach (VariableUsage usage in checker.GetUsagesOfVariable(variable.VariableName))
                {
                    switch (usage.UsageType)
                    {
                        case VariableUsageType.CalibrationScript:
                            currentUsage = String.Format("Calibration Script at ({0})", usage.ScriptLocation.GetLocationAsString());
                            break;
                        case VariableUsageType.Duration:
                            currentUsage = String.Format("Step Duration at ({0})", usage.ScriptLocation.GetLocationAsString());
                            break;
                        case VariableUsageType.Value:
                            currentUsage = String.Format("Step Value at ({0})", usage.ScriptLocation.GetLocationAsString());
                            break;
                        case VariableUsageType.DynamicVariable:
                            currentUsage = String.Format("Dynamic Variable at ({0})", usage.ScriptLocation.GetLocationAsString());
                            break;
                        case VariableUsageType.PythonStep:
                            currentUsage = String.Format("Python Step at ({0})", usage.ScriptLocation.GetLocationAsString());
                            break;
                    }

                    variable.AddUsage(currentUsage);
                }

            }
        }

        /// <summary>
        /// Sets the parent controller.
        /// </summary>
        /// <param name="parentController">The parent controller.</param>
        internal void SetParentController(Root.RootController parentController)
        {
            _parentController = parentController;
        }

        /// <summary>
        /// Reads the related options
        /// </summary>
        private void ReadOptions()
        {
            StaticVariablesPerGroupColumn = OptionsManager.GetInstance().GetOptionValueByName<int>(OptionNames.VARIABLES_STATIC_GROUP_HEIGHT);
        }

        /// <summary>
        /// Called when some options that require the attention of this window change.
        /// </summary>
        public void NotifyOptionsChanged()
        {
            ReadOptions();
        }

        public void UpdateWindowsList()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WindowsList"));
        }

        #region IController Members

        /// <summary>
        /// Requests the buffer to accept the current model which will be read to generate the output of future cycles.
        /// </summary>
        public void CopyToBuffer()
        {
            _parentController.CopyToBuffer();
        }

        #endregion
    }
}

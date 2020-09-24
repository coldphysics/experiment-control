using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Communication.Commands;
using Controller.Variables;
using Model.Variables;

namespace Controller.Control.Compare
{
    public abstract class AbstractCompareItemController<T> : INotifyPropertyChanged, ICompareItemController
    {
        protected readonly VariableModel _newVariable;
        protected readonly VariablesController _variablesController;

        private string _variableName;
        private T _oldValue;
        private T _newValue;
        private bool _isTakeNewSelected;
        private Visibility _buttonsVisibility;

        public event PropertyChangedEventHandler PropertyChanged;

        public string VariableName
        {
            set
            {
                _variableName = value;
                DeclarePropertyChanged("VariableName");
            }

            get { return _variableName; }
        }

        public T OldValue
        {
            set
            {
                _oldValue = value;
                DeclarePropertyChanged("OldValue");
            }

            get { return _oldValue; }
        }

        public T NewValue
        {
            set
            {
                _newValue = value;
                DeclarePropertyChanged("NewValue");
            }

            get { return _newValue; }
        }

        public bool IsTakeNewSelected
        {
            set
            {
                _isTakeNewSelected = value;
                DeclarePropertyChanged("IsTakeNewSelected");
                DeclarePropertyChanged("IsTakeOldSelected");
            }

            get { return _isTakeNewSelected; }
        }

        public bool IsTakeOldSelected
        {
            get
            {
                return !IsTakeNewSelected;
            }
        }

        public Visibility ButtonsVisibility
        {
            set
            {
                _buttonsVisibility = value;
                DeclarePropertyChanged("ButtonsVisibility");
            }

            get
            {
                return _buttonsVisibility;
            }

        }

        public RelayCommand TakeOldValueCommand { set; get; }
        public RelayCommand TakeNewValueCommand { set; get; }

        public enum EnumType
        {
            Value, Code
        }

        public AbstractCompareItemController(T oldData, T newData, VariableModel newVariable, bool editable, VariablesController variablesController)
        {
            TakeOldValueCommand = new RelayCommand(TakeOldClicked);
            TakeNewValueCommand = new RelayCommand(TakeNewClicked);
            OldValue = oldData;
            NewValue = newData;
            _newVariable = newVariable;
            _variablesController = variablesController;
            VariableName = newVariable.VariableName;
            ButtonsVisibility = editable ? Visibility.Visible : Visibility.Hidden;
            IsTakeNewSelected = true;
        }

        protected abstract void TakeNewValue();

        protected abstract void TakeOldValue();

        private void TakeNewClicked(object parameter)
        {
            TakeNewValue();
            IsTakeNewSelected = true;
            _variablesController.evaluate(null);
            _variablesController.DoVariablesValueChangedByName(_newVariable.VariableName);
        }

        private void TakeOldClicked(object parameter)
        {
            TakeOldValue();
            IsTakeNewSelected = false;
            _variablesController.evaluate(null);
            _variablesController.DoVariablesValueChangedByName(_newVariable.VariableName);
        }

        private void DeclarePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}

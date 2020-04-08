using System;
using System.Windows;
using System.Windows.Controls;
using Model.Variables;

namespace Controller.Variables.Compare
{
    /// <summary>
    /// Interaction logic for CompareItem.xaml
    /// </summary>
    public partial class CompareItem : UserControl
    {
        private readonly EnumType _type;
        private readonly Object _oldValue;
        private readonly Object _newValue;
        private readonly VariableModel _newVariable;
        private readonly VariablesController _variablesController;
        public enum EnumType
        {
            Value, Code
        }

        public CompareItem(EnumType type, Object oldData, Object newData, VariableModel newVariable, bool editable, VariablesController variablesController)
        {
            InitializeComponent();
            this._type = type;
            this._oldValue = oldData;
            this._newValue = newData;
            this._newVariable = newVariable;
            this._variablesController = variablesController;
            VariableName.Content = newVariable.VariableName;
            OldValue.Content = oldData;
            NewValue.Content = newData;

            if (!editable)
            {
                ButtonOld.Visibility = Visibility.Hidden;
                ButtonNew.Visibility = Visibility.Hidden;
            }
        }

        private void ButtonTakeNewClick(object sender, RoutedEventArgs e)
        {
            switch (_type)
            {
                    case EnumType.Value:
                        _newVariable.VariableValue = (Double) _newValue;
                    break;
                    case EnumType.Code:
                        _newVariable.VariableCode = (String) _newValue;
                    break;

            }
            ButtonNew.IsEnabled = false;
            ButtonOld.IsEnabled = true;
            _variablesController.evaluate(null);
            _variablesController.DoVariablesValueChangedByName(_newVariable.VariableName);
        }
        
        private void ButtonTakeOldClick(object sender, RoutedEventArgs e)
        {
            switch (_type)
            {
                case EnumType.Value:
                    _newVariable.VariableValue = (Double) _oldValue;
                    break;
                case EnumType.Code:
                    _newVariable.VariableCode = (String) _oldValue;
                    break;
            }

            ButtonNew.IsEnabled = true;
            ButtonOld.IsEnabled = false;
            _variablesController.evaluate(null);
            _variablesController.DoVariablesValueChangedByName(_newVariable.VariableName);
        }
    }
}

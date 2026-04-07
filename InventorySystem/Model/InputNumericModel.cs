using InventorySystem.BaseClass;
using InventorySystem.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventorySystem.Model
{
    public class InputNumericModel : ViewModelBase, IInputModel
    {
        private Type TargetType { get; set; }
        public string Key { get; set; }
        public string Label { get; set; }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        private bool _isReady;
        public bool IsReady
        {
            get { return _isReady; }
            set
            {
                _isReady = value;
                OnPropertyChanged();
            }
        }

        private string _value;
        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                Validate();
                OnPropertyChanged();
            }
        }

        public InputNumericModel(string key, string label, Type type, string value = "")
        {
            TargetType = type;
            Key = key;
            Label = label;
            Value = value;
            Validate();
        }

        private void Validate()
        {
            if (string.IsNullOrWhiteSpace(Value))
            {
                IsReady = false;
                ErrorMessage = $"{Label} is required";
                return;
            }

            try
            {
                // ChangeType attempts to force the string into the TargetType
                Convert.ChangeType(Value, TargetType);
                IsReady = true;
                ErrorMessage = null;
            }
            catch
            {
                IsReady = false;
                ErrorMessage = $"Please enter a valid {TargetType.Name}";
            }
        }
    }
}

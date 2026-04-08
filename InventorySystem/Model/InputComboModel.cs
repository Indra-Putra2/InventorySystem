using InventorySystem.BaseClass;
using InventorySystem.Interface;

namespace InventorySystem.Model
{
    public class InputComboModel : ViewModelBase, IInputModel
    {
        public string Key { get; set; }
        public string Label { get; set; }

        private List<string> _option;
        public List<string> Options
        {
            get { return _option; }
            set 
            { 
                _option = value;
                OnPropertyChanged();
            }
        }


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

        public InputComboModel(string key, string label, List<string> options, string value = "")
        {
            Key = key;
            Label = label;
            Options = options;
            Value = value;
            Validate();
        }

        private void Validate()
        {
            if (string.IsNullOrEmpty(Value))
            {
                IsReady = false;
                ErrorMessage = $"{Label} is required";
            }
            else
            {
                IsReady = true;
                ErrorMessage = null;
            }
        }
    }
}

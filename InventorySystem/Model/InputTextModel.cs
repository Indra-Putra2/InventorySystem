using InventorySystem.BaseClass;
using InventorySystem.Interface;
namespace InventorySystem.Model
{
    public class InputTextModel : ViewModelBase, IInputModel
    {
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

        public InputTextModel(string key, string label, string value = "")
        {
            Key = key;
            Label = label;
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
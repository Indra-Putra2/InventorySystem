using InventorySystem.BaseClass;

namespace InventorySystem.Model
{
    public class InputComboModel : ViewModelBase
    {
        public string Label { get; set; }
        public List<string> Options { get; set; }

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

        public InputComboModel(string label, List<string> options)
        {
            Label = label;
            Options = options;
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

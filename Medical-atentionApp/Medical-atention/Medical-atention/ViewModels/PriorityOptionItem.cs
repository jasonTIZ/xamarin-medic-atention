using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;

namespace Medical_atention.ViewModels
{
    public class PriorityOptionItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        public PriorityOptionItem(string key, ICommand selectCommand)
        {
            Key = key;
            SelectCommand = selectCommand;
        }

        public string Key { get; }

        public ICommand SelectCommand { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BorderColor));
            }
        }

        public Color BorderColor => _isSelected ? Color.FromHex("#2C7BE5") : Color.FromHex("#E2E8F0");

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

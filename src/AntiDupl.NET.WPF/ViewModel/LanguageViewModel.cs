namespace AntiDupl.NET.WPF.ViewModel
{
    public enum LanguageEnum
    {
        English = 1,
        Russian = 2,
        TraditionalChinese = 3
    }

    public class LanguageViewModel : PropertyChangedBase
    {
        private bool _isChecked;

        public LanguageEnum Enum { get; set; }

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked == value) return;
                _isChecked = value;
                RaisePropertyChangedEvent();
            }
        }

        public string Title { get; set; }
    }
}

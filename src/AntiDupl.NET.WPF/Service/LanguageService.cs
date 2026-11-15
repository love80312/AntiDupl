
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AntiDupl.NET.WPF.Command;
using AntiDupl.NET.WPF.Model;
using AntiDupl.NET.WPF.ObjectModel;
using AntiDupl.NET.WPF.ViewModel;

namespace AntiDupl.NET.WPF.Service
{
    class LanguageService : ILanguageService
    {
        public event EventHandler LanguageChanged;

        private readonly IConfigurationModel _config;

        public LanguageService(IConfigurationModel config)
        {
            _config = config;

            foreach (LanguageEnum value in Enum.GetValues(typeof(LanguageEnum)))
            {
                _languageCollection.Add(new LanguageViewModel
                {
                    Enum = value,
                    IsChecked = value == LanguageEnum.English,
                    Title = GetLanguageTitle(value)
                });
            }

            if (_config != null)
            {
                var culture = _config.Language?.ToCultureInfo()
                    ?? new CultureInfo("en-US");

                Language = culture;

                SetCheckedForLanguageCollection(
                    GetLanguageEnumFromCulture(culture),
                    LanguageCollection);
            }
            else
            {
                Language = new CultureInfo("en-US");
            }
        }

        private readonly ObservableCollection<LanguageViewModel>
            _languageCollection = new();

        public ObservableCollection<LanguageViewModel> LanguageCollection
            => _languageCollection;

        private ICommand _languageSelectedCommand;

        public ICommand LanguageSelectedCommand
        {
            get
            {
                return _languageSelectedCommand ??= new RelayCommand(arg =>
                {
                    if (arg is not LanguageEnum language)
                        return;

                    SetCheckedForLanguageCollection(
                        language,
                        LanguageCollection);

                    CultureInfo newLanguage =
                        GetCultureFromLanguageEnum(language);

                    if (_config != null)
                    {
                        _config.Language =
                            SerializableCultureInfo.FromCultureInfo(newLanguage);
                    }

                    Language = newLanguage;
                });
            }
        }

        private static string GetLanguageTitle(LanguageEnum language)
        {
            return language switch
            {
                LanguageEnum.English => "English",
                LanguageEnum.Russian => "Русский",
                LanguageEnum.TraditionalChinese => "繁體中文",
                _ => language.ToString()
            };
        }

        private static void SetCheckedForLanguageCollection(
            LanguageEnum language,
            ObservableCollection<LanguageViewModel> collection)
        {
            foreach (var item in collection)
            {
                item.IsChecked = item.Enum == language;
            }
        }

        private static CultureInfo GetCultureFromLanguageEnum(
            LanguageEnum language)
        {
            return language switch
            {
                LanguageEnum.English => new CultureInfo("en-US"),
                LanguageEnum.Russian => new CultureInfo("ru-RU"),
                LanguageEnum.TraditionalChinese => new CultureInfo("zh-TW"),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(language),
                    language,
                    "Unsupported language")
            };
        }

        private static LanguageEnum GetLanguageEnumFromCulture(
            CultureInfo culture)
        {
            return culture.Name switch
            {
                "en-US" => LanguageEnum.English,
                "ru-RU" => LanguageEnum.Russian,
                "zh-TW" => LanguageEnum.TraditionalChinese,
                _ => LanguageEnum.English
            };
        }

        public CultureInfo Language
        {
            get => System.Threading.Thread.CurrentThread.CurrentUICulture;

            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                System.Threading.Thread.CurrentThread.CurrentUICulture = value;

                var dictionaries =
                    Application.Current.Resources.MergedDictionaries;

                // 主要語系字典。
                string dictionaryName = value.Name switch
                {
                    "ru-RU" => "Resources/lang.ru-RU.xaml",
                    "zh-TW" => "Resources/lang.zh-TW.xaml",
                    _ => "Resources/lang.xaml"
                };

                ResourceDictionary dict = new ResourceDictionary
                {
                    Source = new Uri(dictionaryName, UriKind.Relative)
                };

                // 不要將 .extra.xaml 誤認為主要語系字典。
                ResourceDictionary oldDict = dictionaries.FirstOrDefault(d =>
                    d.Source != null &&
                    d.Source.OriginalString.StartsWith(
                        "Resources/lang.",
                        StringComparison.Ordinal) &&
                    !d.Source.OriginalString.EndsWith(
                        ".extra.xaml",
                        StringComparison.OrdinalIgnoreCase));

                if (oldDict != null)
                {
                    int index = dictionaries.IndexOf(oldDict);
                    dictionaries.Remove(oldDict);
                    dictionaries.Insert(index, dict);
                }
                else
                {
                    dictionaries.Add(dict);
                }

                // 補充語系字典。
                string extraDictionaryName = value.Name switch
                {
                    "ru-RU" => "Resources/lang.ru-RU.extra.xaml",
                    "zh-TW" => "Resources/lang.zh-TW.extra.xaml",
                    _ => "Resources/lang.extra.xaml"
                };

                ResourceDictionary extraDict = new ResourceDictionary
                {
                    Source = new Uri(extraDictionaryName, UriKind.Relative)
                };

                ResourceDictionary oldExtraDict =
                    dictionaries.FirstOrDefault(d =>
                        d.Source != null &&
                        d.Source.OriginalString.EndsWith(
                            ".extra.xaml",
                            StringComparison.OrdinalIgnoreCase));

                if (oldExtraDict != null)
                {
                    int index = dictionaries.IndexOf(oldExtraDict);
                    dictionaries.Remove(oldExtraDict);
                    dictionaries.Insert(index, extraDict);
                }
                else
                {
                    dictionaries.Add(extraDict);
                }

                LanguageChanged?.Invoke(
                    Application.Current,
                    EventArgs.Empty);
            }
        }
    }
}

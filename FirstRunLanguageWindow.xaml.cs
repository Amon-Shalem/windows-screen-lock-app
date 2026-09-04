using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CustomScreenLocker
{
    public partial class FirstRunLanguageWindow : Window
    {
        public string SelectedLanguageCode { get; private set; } = "en-US";

        public FirstRunLanguageWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 確保首次開啟時一律預設為英文 (Default: en-US)
            string defaultCode = "en-US";

            LanguageListContainer.Children.Clear();

            foreach (var lang in I18nManager.GetAvailableLanguages())
            {
                var rb = new RadioButton
                {
                    Content = lang.DisplayName,
                    Tag = lang.Code,
                    FontSize = 14,
                    FontWeight = FontWeights.Medium,
                    Foreground = new SolidColorBrush(Color.FromRgb(243, 244, 246)),
                    Margin = new Thickness(6, 8, 6, 8),
                    IsChecked = (lang.Code == defaultCode)
                };

                rb.Checked += (s, ev) =>
                {
                    if (rb.Tag is string code)
                    {
                        SelectedLanguageCode = code;
                    }
                };

                LanguageListContainer.Children.Add(rb);

                if (lang.Code == defaultCode)
                {
                    SelectedLanguageCode = lang.Code;
                }
            }
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            I18nManager.SetLanguage(SelectedLanguageCode);
            this.DialogResult = true;
            this.Close();
        }
    }
}

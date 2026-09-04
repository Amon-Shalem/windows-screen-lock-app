using System.Windows;
using System.Windows.Input;

namespace CustomScreenLocker
{
    public partial class PromptDialog : Window
    {
        public string InputText { get; private set; } = string.Empty;

        public PromptDialog(string title, string defaultText = "")
        {
            InitializeComponent();
            TxtPromptTitle.Text = title;
            TxtInput.Text = defaultText;
            TxtInput.Focus();
            TxtInput.SelectAll();
        }

        private void TxtInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Submit();
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Submit();
        }

        private void Submit()
        {
            if (!string.IsNullOrWhiteSpace(TxtInput.Text))
            {
                InputText = TxtInput.Text.Trim();
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                TxtInput.Focus();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}

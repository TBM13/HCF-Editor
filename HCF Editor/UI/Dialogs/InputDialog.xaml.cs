using System.Windows;

namespace HCF_Editor.UI.Dialogs
{
    public partial class InputDialog : Window
    {
        public string Text => MainTextBox.Text;

        public InputDialog(string title, string text = "")
        {
            InitializeComponent();

            Title = title;
            MainTextBox.Text = text;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}

using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HCF_Editor.UI
{
    public partial class ListValueEditor : UserControl, IValueEditor
    {
        public IValueEditor.ObjectEvent? OnValueEdited { get; set; }

        public string Text
        {
            get => (string)Expander.Header;
            set => Expander.Header = value;
        }

        public object? Value
        {
            get => value;
            set
            {
                if (value is null)
                    throw new("Can't edit a null value");

                if (value is not IEnumerable)
                    throw new("Can't edit a non-IEnumerable value, use a ValueEditor");

                this.value = (IEnumerable)value;
                Expander.ToolTip = value.GetType();
                PopulateStackPanel();
            }
        }

        private object? value;
        private object? editedValue;

        private bool unsavedChanges;

        public ListValueEditor() =>
            InitializeComponent();

        private void PopulateStackPanel()
        {
            StackPanel.Children.Clear();
            Grid.Background = Brushes.White;
            if (value == null)
                return;

            switch (value)
            {
                case IList:
                    int i = 0;
                    foreach (object obj in (IEnumerable)value)
                    {
                        ValueEditor editor = new()
                        {
                            Margin = new Thickness(2)
                        };

                        editor.Value = obj;
                        editor.Text = i.ToString();
                        int i2 = i;
                        editor.OnValueEdited += (editedObj) =>
                            EditValue(i2, obj, editedObj);

                        StackPanel.Children.Add(editor);

                        i++;
                    }

                    break;

                case IDictionary d:
                    foreach (dynamic pair in d)
                    {
                        ValueEditor editor = new()
                        {
                            Margin = new Thickness(2)
                        };

                        editor.Value = pair.Value;
                        editor.Text = pair.Key.ToString();
                        editor.OnValueEdited += (editedObj) =>
                            EditValue(pair.Key, pair.Value, editedObj);

                        StackPanel.Children.Add(editor);
                    }

                    break;

                default:
                    throw new($"Unsupported value type {value?.GetType()}");
            }
        }

        private void EditValue(int index, object original, object edited)
        {
            editedValue = null;

            if (edited != null && !edited.Equals(original))
            {
                editedValue = value;
                if (editedValue != null)
                {
                    ((IList)editedValue)[index] = edited;
                    unsavedChanges = true;
                }
            }

            SaveButton.IsEnabled = editedValue != null || unsavedChanges;
            Grid.Background = SaveButton.IsEnabled ? Brushes.LightBlue : Brushes.White;
        }

        private void EditValue(object key, object original, object edited)
        {
            editedValue = null;

            if (edited != null && !edited.Equals(original))
            {
                editedValue = value;
                if (editedValue != null)
                {
                    ((IDictionary)editedValue)[key] = edited;
                    unsavedChanges = true;
                }
            }

            SaveButton.IsEnabled = editedValue != null || unsavedChanges;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (editedValue == null)
                return;

            OnValueEdited?.Invoke(editedValue);
            value = editedValue;
            SaveButton.IsEnabled = false;
            unsavedChanges = false;

            // We need to re-populate the stack panel in order to update the original
            // value of the elements
            PopulateStackPanel();
        }
    }
}

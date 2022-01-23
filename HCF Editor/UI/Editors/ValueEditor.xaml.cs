using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HCF_Editor.UI.Editors
{
    public partial class ValueEditor : UserControl, IValueEditor
    {
        public Events.ObjectEvent? OnValueEdited { get; set; }

        public string Text
        {
            get => (string)Label.Content;
            set
            {
                Label.Content = value;

                // Calculate text width
                FormattedText ft = new(value,
                                       CultureInfo.CurrentCulture,
                                       FlowDirection.LeftToRight,
                                       new(Label.FontFamily.Source),
                                       Label.FontSize,
                                       Brushes.Black,
                                       VisualTreeHelper.GetDpi(this).PixelsPerDip);

                Label.Width = 10 + ft.Width;
                ValueTextbox.Margin = new(3 + Label.Width,
                                          ValueTextbox.Margin.Top,
                                          ValueTextbox.Margin.Right,
                                          ValueTextbox.Margin.Bottom);
            }
        }

        public object? Value
        {
            get => value;
            set
            {
                if (value is null)
                    throw new("Can't edit a null value");

                this.value = value;
                ValueTextbox.Text = value.ToString();
                ValueTextbox.IsEnabled = true;
                ValueTextbox.ToolTip = value.GetType();
            }
        }

        private object? value;
        private object? editedValue;

        public ValueEditor() =>
            InitializeComponent();

        private void ValueTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            editedValue = null;

            switch (value)
            {
                case null:
                    break;

                case string:
                    editedValue = ValueTextbox.Text;
                    break;

                case ushort:
                    if (ushort.TryParse(ValueTextbox.Text, out ushort us))
                        editedValue = us;
                    break;

                case uint:
                    if (uint.TryParse(ValueTextbox.Text, out uint ui))
                        editedValue = ui;
                    break;

                case ulong:
                    if (ulong.TryParse(ValueTextbox.Text,out ulong ul))
                        editedValue = ul;
                    break;

                case short:
                    if (short.TryParse(ValueTextbox.Text, out short s))
                        editedValue = s;
                    break;

                case int:
                    if (int.TryParse(ValueTextbox.Text, out int i))
                        editedValue = i;
                    break;

                case long:
                    if (long.TryParse(ValueTextbox.Text, out long l))
                        editedValue = l;
                    break;

                case float:
                    if (float.TryParse(ValueTextbox.Text, out float f))
                        editedValue = f;
                    break;

                case double:
                    if (double.TryParse(ValueTextbox.Text, out double d))
                        editedValue = d;
                    break;

                case decimal:
                    if (decimal.TryParse(ValueTextbox.Text, out decimal de))
                        editedValue = de;
                    break;

                case bool:
                    if (bool.TryParse(ValueTextbox.Text, out bool b))
                        editedValue = b;
                    break;

                case byte:
                    if (byte.TryParse(ValueTextbox.Text, out byte by))
                        editedValue = by;
                    break;

                default:
                    throw new($"Unsupported value type {value?.GetType()}");
            }

            SaveButton.IsEnabled = editedValue != null && !editedValue.Equals(value);
            Grid.Background = SaveButton.IsEnabled ? Brushes.LightBlue : Brushes.White;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (editedValue == null)
                return;

            OnValueEdited?.Invoke(editedValue);
            value = editedValue;
            SaveButton.IsEnabled = false;

            Grid.Background = Brushes.White;
        }
    }
}

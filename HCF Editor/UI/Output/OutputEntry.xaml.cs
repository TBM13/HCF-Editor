using System.Windows.Controls;
using System.Windows.Media;

namespace HCF_Editor.UI.Output
{
    public partial class OutputEntry : UserControl
    {
        public string Message
        {
            get => (string)MessageLabel.Content;
            set => MessageLabel.Content = value;
        }
        
        public OutputEntryType Type
        {
            get => type;
            set
            {
                type = value;
                IconEllipse.Fill = type switch
                {
                    OutputEntryType.Debug => Brushes.LightSeaGreen,
                    OutputEntryType.Info => Brushes.MediumSlateBlue,
                    OutputEntryType.Warn => Brushes.Orange,
                    OutputEntryType.Error => Brushes.Red,
                    _ => Brushes.White
                };
            }
        }

        private OutputEntryType type = OutputEntryType.Debug;

        public OutputEntry() => 
            InitializeComponent();
    }

    public enum OutputEntryType
    {
        Debug,
        Info,
        Warn,
        Error
    }
}

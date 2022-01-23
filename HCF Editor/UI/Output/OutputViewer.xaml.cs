using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace HCF_Editor.UI.Output
{
    public partial class OutputViewer : UserControl
    {
        public static OutputViewer? Instance 
        {
            get => instance;
            set
            {
                instance = value;

                if (instance != null)
                {
                    // Display Pending Entries
                    for (int i = 0; i < pendingEntries.Count; i++)
                    {
                        instance.Visibility = Visibility.Visible;
                        instance.MainStackPanel.Children.Add(pendingEntries[i]);

                        pendingEntries.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        private static OutputViewer? instance;
        private static readonly List<OutputEntry> pendingEntries = new();

        public OutputViewer()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;

            for (int i = 0; i < MainStackPanel.Children.Count; i++)
            {
                if (MainStackPanel.Children[i] is not OutputEntry)
                    continue;

                MainStackPanel.Children.RemoveAt(i);
                i--;
            }
        }

        public static void Log(string message, OutputEntryType type)
        {
            OutputEntry entry = new()
            {
                Type = type,
                Message = message
            };

            if (Instance == null)
            {
                pendingEntries.Add(entry);
                return;
            }

            Instance.Visibility = Visibility.Visible;
            Instance.MainStackPanel.Children.Add(entry);
        }
    }
}

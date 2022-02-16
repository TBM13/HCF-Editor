using HCF_Editor.Samsung;
using HCF_Editor.UI.Tabs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HCF_Editor
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddTab(TabItem tab)
        {
            MainTabControl.Items.Add(tab);
            MainTabControl.SelectedItem = tab;
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MIBMenuItem.IsEnabled = MainTabControl.SelectedItem is MibEditorTab;
        }

        private void File_OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new()
            {
                Filter = "Encoded MIB Files (*.hcf)|*.hcf|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                byte[] bytes = File.ReadAllBytes(dialog.FileName);
                EncodedMIB file = new(bytes, dialog.FileName, dialog.SafeFileName);

                MibEditorTab tab = new()
                {
                    Header = dialog.SafeFileName,
                    ToolTip = dialog.FileName
                };

                AddTab(tab);
                tab.LoadEncodedMIB(file);
            }
        }

        private void MIB_EditPsidDefinitions_Click(object sender, RoutedEventArgs e)
        {
            MibEditorTab editor = (MibEditorTab)MainTabControl.SelectedItem;
            editor.EditPsidDefinitions();
        }
    }
}

using HCF_Editor.Samsung;
using HCF_Editor.UI.Editors;
using HCF_Editor.UI.Output;
using System;
using System.Collections.Generic;
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

namespace HCF_Editor.UI.Tabs
{
    public partial class MibEditorTab : TabItem
    {
        public MibEditorTab() => 
            InitializeComponent();

        public void LoadEncodedMIB(EncodedMIB mib)
        {
            OutputViewer.Instance = OutputView;

            foreach(MIBEntry entry in mib.DecodeMibEntries())
                EntriesDataGrid.Items.Add(entry);

            OutputViewer.Instance = null;
        }

        private void EntriesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EntryEditor.IsEnabled = false;

            if (EntriesDataGrid.SelectedItems.Count == 1)
            {
                EntryEditor.IsEnabled = true;
                EntryEditor.Entry = (MIBEntry)EntriesDataGrid.SelectedItem;
            }
        }
    }
}
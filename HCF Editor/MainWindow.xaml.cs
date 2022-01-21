using HCF_Editor.UI;
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
            ReadFile();
        }

        private void ReadFile()
        {
            byte[] bytes = File.ReadAllBytes(@"D:\Android\Devices\A20\vendor-R\etc\wifi\mx140_wlan.hcf");
            HcfFile hcf = new(bytes);

            foreach(MIBEntry entry in hcf.DecodeMibEntries())
            {
                MIBEntryEditor editor = new()
                {
                    Entry = entry
                };

                StackPanel.Children.Add(editor);
            }
        }
    }
}

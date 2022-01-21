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

namespace HCF_Editor.UI
{
    public partial class MIBEntryEditor : UserControl
    {
        public delegate void MIBEntryEvent(MIBEntry entry);
        public MIBEntryEvent? OnEntryEdited { get; set; }

        public MIBEntry? Entry 
        {
            get => entry;
            set
            {
                if (value is null)
                    throw new("Can't edit a null entry");


                entry = value;
                PsidLabel.Content = $"PSID: {entry.Psid}";

                IndexLabel.Content = $"Indexes: ";
                for (int i = 0; i < entry.Index.Length; i++)
                {
                    if (i != 0)
                        IndexLabel.Content += ", ";

                    IndexLabel.Content += entry.Index[i].ToString();
                }

                ValueTypeLabel.Content = "Value Type: ";
                ValueTypeLabel.Content += entry.Type switch
                {
                    MIBValueType.Bool => "Boolean",
                    MIBValueType.UInt => "UInt32",
                    MIBValueType.Int => "Int32",
                    MIBValueType.Octet => "Octet",
                    MIBValueType.None => "None",
                    _ => "Invalid"
                };

                if (valueEditor != null)
                    StackPanel.Children.Remove((UserControl)valueEditor);

                if (entry.Type == MIBValueType.Octet)
                    valueEditor = new ListValueEditor();
                else
                    valueEditor = new ValueEditor();

                valueEditor.Text = "Entry Value";
                valueEditor.Value = entry.Value;
                StackPanel.Children.Add((UserControl)valueEditor);
            }
        }

        private MIBEntry? entry;
        private IValueEditor? valueEditor;

        public MIBEntryEditor() => 
            InitializeComponent();
    }
}

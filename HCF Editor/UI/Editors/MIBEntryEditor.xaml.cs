using HCF_Editor.Samsung;
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

namespace HCF_Editor.UI.Editors
{
    public partial class MIBEntryEditor : UserControl
    {
        public Events.MIBEntryEvent? OnEntryEdited { get; set; }

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
                    MIBValueType.Invalid => "Invalid",
                    MIBValueType.Bool => "Boolean",
                    MIBValueType.UInt => "UInt32",
                    MIBValueType.Int => "Int32",
                    MIBValueType.Octet => "Octet",
                    MIBValueType.None => "None",
                    _ => "Unknown"
                };

                if (ValueEditor != null)
                    StackPanel.Children.Remove((UserControl)ValueEditor);

                if (entry.Type == MIBValueType.Octet)
                    ValueEditor = new ListValueEditor();
                else
                    ValueEditor = new ValueEditor();

                ValueEditor.Text = "Entry Value";
                ValueEditor.Value = entry.Value;
                StackPanel.Children.Add((UserControl)ValueEditor);
            }
        }

        public PsidDefinition? PsidDefinition
        {
            get => psidDefinition;
            set
            {
                psidDefinition = value;

                foreach (UIElement elem in PsidDef_StackPanel.Children)
                    elem.Visibility = Visibility.Collapsed;

                if (value == null)
                    return;

                PsidDef_Name.Content = $"Name: {value.Name}";
                PsidDef_Name.Visibility = Visibility.Visible;

                if (value.PerInterface != null)
                {
                    PsidDef_PerInterface.Content = $"Per Interface?: {value.PerInterface}";
                    PsidDef_PerInterface.Visibility = Visibility.Visible;
                }

                if (value.Type != null)
                {
                    PsidDef_Type.Content = $"Type: {value.Type}";
                    PsidDef_Type.Visibility = Visibility.Visible;
                }

                if (value.Units != null)
                {
                    PsidDef_Units.Content = $"Units: {value.Units}";
                    PsidDef_Units.Visibility = Visibility.Visible;
                }

                if (value.Min != null)
                {
                    PsidDef_Min.Content = $"Min: {value.Min}";
                    PsidDef_Min.Visibility = Visibility.Visible;
                }

                if (value.Max != null)
                {
                    PsidDef_Max.Content = $"Max: {value.Max}";
                    PsidDef_Max.Visibility = Visibility.Visible;
                }

                if (value.Default != null)
                {
                    PsidDef_Default.Content = $"Default: {value.Default}";
                    PsidDef_Default.Visibility = Visibility.Visible;
                }

                if (value.Description != null)
                {
                    PsidDef_Description.Content = $"Description: {value.Description}";
                    PsidDef_Description.Visibility = Visibility.Visible;
                }
            }
        }

        public IValueEditor? ValueEditor { get; private set; }

        private MIBEntry? entry;
        private PsidDefinition? psidDefinition;

        public MIBEntryEditor() => 
            InitializeComponent();
    }
}

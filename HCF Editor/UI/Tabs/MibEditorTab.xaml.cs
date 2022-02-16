using HCF_Editor.Samsung;
using HCF_Editor.UI.Dialogs;
using HCF_Editor.UI.Editors;
using HCF_Editor.UI.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        public EncodedMIB? MIBFile { get; private set; }

        private readonly Dictionary<ushort, PsidDefinition> psidDefinitions = new();

        private const string psidDefPattern = 
            @"\/\*+?\n*? *?\* NAME[ \t]*?:( ([^\n]*?))?\n *?\* PSID[ \t]*?:( ([^\n]*?) [^\n]*?)?\n( *?\* PER INTERFACE\?[ \t]*?:( ([^\n]*?))?\n)?( *?\* TYPE[ \t]*?:( ([^\n]*?))?\n)?( *?\* UNITS[ \t]*?:( ([^\n]*?))?\n)?( *?\* MIN[ \t]*?:( ([^\n]*?))?\n)?( *?\* MAX[ \t]*?:( ([^\n]*?))?\n)?( *?\* DEFAULT[ \t]*?:( ([^\n]*?))?\n)?( *?\* DESCRIPTION *?:(\n(.*?))?)*? *?\*+?\/";

        public MibEditorTab() => 
            InitializeComponent();

        public void LoadEncodedMIB(EncodedMIB mib)
        {
            MIBFile = mib;
            OutputViewer.Instance = OutputView;

            foreach(MIBEntry entry in mib.DecodeMibEntries())
                EntriesDataGrid.Items.Add(entry);

            OutputViewer.Instance = null;
        }

        public void EditPsidDefinitions()
        {
            if (MIBFile == null)
                return;

            InputDialog dialog = new($"PSID Definitions for {MIBFile.FileName}");

            if (dialog.ShowDialog() == true)
            {
                psidDefinitions.Clear();
                foreach (Match match in Regex.Matches(dialog.MainTextBox.Text.Replace("\r", ""), psidDefPattern, RegexOptions.Singleline)) 
                {
                    string name = match.Groups[2].Value;

                    if (!ushort.TryParse(match.Groups[4].Value, out ushort psid))
                    {
                        Log($"Invalid PSID: {match.Groups[4].Value}", OutputEntryType.Error);
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(name))
                        Log($"PSID Definition '{psid}' has no name", OutputEntryType.Warn);

                    PsidDefinition def = new(psid, name);

                    string perInterface = match.Groups[7].Value;
                    if (perInterface.Length > 0)
                    {
                        switch (perInterface.ToUpperInvariant())
                        {
                            case "NO":
                                def.PerInterface = false;
                                break;
                            case "YES":
                                def.PerInterface = true;
                                break;
                            default:
                                Log($"PSID Definition '{psid}' ({name}) has an unrecognized 'Per Interface' value: '{perInterface}'", OutputEntryType.Warn);
                                break;
                        }
                    }

                    string type = match.Groups[10].Value;
                    if (type.Length > 0)
                    {
                        def.Type = type.ToUpperInvariant() switch
                        {
                            "SLSIUINT8" => PsidDefinitionType.UInt8,
                            "SLSIUINT16" => PsidDefinitionType.UInt16,
                            "SLSIUINT32" => PsidDefinitionType.UInt32,
                            "SLSIUINT64" => PsidDefinitionType.UInt64,

                            "SLSIINT8" => PsidDefinitionType.Int8,
                            "SLSIINT16" => PsidDefinitionType.Int16,
                            "SLSIINT32" => PsidDefinitionType.Int32,
                            "SLSIINT64" => PsidDefinitionType.Int64,
                            //"INT64" => PsidDefinitionType.Int64,

                            "SLSIBOOL" => PsidDefinitionType.Bool,

                            _ => null
                        };

                        if (def.Type == null)
                            Log($"PSID Definition '{psid}' ({name}) has an unrecognized 'Type' value: '{type}'", OutputEntryType.Warn);
                    }

                    string units = match.Groups[13].Value;
                    if (units.Length > 0)
                        def.Units = units;

                    string min = match.Groups[16].Value;
                    if (min.Length > 0)
                    {
                        if (!long.TryParse(min, out long minI))
                            Log($"PSID Definition '{psid}' ({name}) has an invalid 'Min' value: '{min}'", OutputEntryType.Warn);
                        else
                            def.Min = minI;
                    }

                    string max = match.Groups[19].Value;
                    if (max.Length > 0)
                    {
                        if (!long.TryParse(max, out long maxI))
                            Log($"PSID Definition '{psid}' ({name}) has an invalid 'Max' value: '{max}'", OutputEntryType.Warn);
                        else
                            def.Max = maxI;
                    }

                    string defaultValue = match.Groups[22].Value;
                    if (defaultValue.Length > 0)
                        def.Default = defaultValue;

                    string description = match.Groups[25].Value;
                    if (description.Length > 0)
                    {
                        description = description.Replace(" * ", "");
                        def.Description = description;
                    }

                    psidDefinitions[def.Psid] = def;
                }

                Log($"Loaded {psidDefinitions.Count} PSID definitions", OutputEntryType.Info);
            }
        }

        private void EntriesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EntryEditor.IsEnabled = false;

            if (EntriesDataGrid.SelectedItems.Count == 1)
            {
                EntryEditor.IsEnabled = true;
                EntryEditor.Entry = (MIBEntry)EntriesDataGrid.SelectedItem;

                if (psidDefinitions.ContainsKey(EntryEditor.Entry.Psid))
                    EntryEditor.PsidDefinition = psidDefinitions[EntryEditor.Entry.Psid];
            }
        }

        private void Log(string message, OutputEntryType type)
        {
            OutputViewer.Instance = OutputView;
            OutputViewer.Log(message, type);
            OutputViewer.Instance = null;
        }
    }
}
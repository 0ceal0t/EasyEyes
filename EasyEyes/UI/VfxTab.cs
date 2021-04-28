using Dalamud.Interface;
using Dalamud.Plugin;
using EasyEyes.Util;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VFXSelect.UI;

namespace EasyEyes.UI {
    public class VfxTab {
        public Plugin _plugin;

        public VfxTab( Plugin plugin ) {
            _plugin = plugin;
        }

        public SavedItem SelectedVfx = null;
        public string AddVfxPath = "";
        public void Draw() {
            var ret = ImGui.BeginTabItem( "Blacklist##MainInterfaceTabs" );
            if( !ret ) return;

            var Id = "##VFXs";
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );
            ImGui.Columns( 2, Id + "Columns", true );

            ImGui.InputText( Id + "-Path", ref AddVfxPath, 255 );
            ImGui.SameLine();
            if( ImGui.Button( "Add Path" + Id ) ) {
                if( _plugin.PluginInterface.Data.FileExists( AddVfxPath ) ) {
                    _plugin.Configuration.AddPath( AddVfxPath, out var newItem );
                    SelectedVfx = newItem;
                }
                AddVfxPath = "";
            }
            ImGui.SameLine();
            ImGui.PushFont( UiBuilder.IconFont );
            if( ImGui.Button( $"{( char )FontAwesomeIcon.Search}", new Vector2( 30, 23 ) ) ) {
                _plugin.MainUI.SelectUI.Show(showLocal: false);
            }
            ImGui.PopFont();

            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );

            ImGui.BeginChild( Id + "Tree", new Vector2(-1, ImGui.GetContentRegionAvail().Y - 22), true );
            VFXSelectDialog.DisplayVisible( _plugin.Configuration.Items.Count, out int preItems, out int showItems, out int postItems, out float itemHeight );
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + preItems * itemHeight );
            int idx = 0;
            foreach( var item in _plugin.Configuration.Items ) {
                if( idx < preItems || idx > ( preItems + showItems ) ) { idx++; continue; }
                if( ImGui.Selectable( item.AVFXPath + Id + idx, SelectedVfx == item ) ) {
                    SelectedVfx = item;
                }
                idx++;
            }
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + postItems * itemHeight );
            ImGui.EndChild();

            if(ImGui.SmallButton("Export" + Id ) ) {
                ExportDialog();
            }
            ImGui.SameLine();
            if( ImGui.SmallButton( "Import" + Id ) ) {
                ImportDialog();
            }

            ImGui.NextColumn();
            // ========================
            if( SelectedVfx == null ) {
                ImGui.Text( "Select an item..." );
            }
            else {
                ImGui.TextColored( new Vector4(0.1f, 0.8f, 0.1f, 1.0f), SelectedVfx.AVFXPath );
                if(ImGui.SmallButton("Copy Path" + Id ) ) {
                    ImGui.SetClipboardText( SelectedVfx.AVFXPath );
                }
                ImGui.Checkbox( "Disabled" + Id, ref SelectedVfx.Disabled );

                ImGui.SameLine();
                _plugin.MainUI.DrawSpawnButton( "Spawn", Id, SelectedVfx.AVFXPath, SelectedVfx.Disabled );

                ImGui.Text( "Notes:" );
                ImGui.InputTextMultiline( Id + "-Description", ref SelectedVfx.Notes, 400, new Vector2( ImGui.GetContentRegionAvail().X, 200 ) );
                if( ImGui.Button( "Save" + Id ) ) {
                    _plugin.Configuration.Save();
                }
                ImGui.SameLine();
                if( MainInterface.RemoveButton( "Delete" ) ) {
                    _plugin.Configuration.RemoveItem( SelectedVfx );
                    SelectedVfx = null;
                }
            }
            ImGui.Columns( 1 );
            ImGui.EndTabItem();
        }

        public void ExportDialog() {
            Task.Run( async () => {
                var picker = new SaveFileDialog {
                    Filter = "Text File (*.txt)|*.txt*|All files (*.*)|*.*",
                    Title = "Select a Save Location.",
                    DefaultExt = "txt",
                    AddExtension = true
                };
                var result = await picker.ShowDialogAsync();
                if( result == DialogResult.OK ) {
                    try {
                        if(_plugin.Configuration.Items.Count > 0 ) {
                            var paths = _plugin.Configuration.Items.ConvertAll( x => x.AVFXPath ).ToArray();
                            File.WriteAllLines( picker.FileName, paths );
                        }
                    }
                    catch( Exception ex ) {
                        PluginLog.LogError( ex, "Could not select a file" );
                    }
                }
            } );
        }

        public void ImportDialog() {
            Task.Run( async () => {
                var picker = new OpenFileDialog {
                    Filter = "Text File (*.txt)|*.txt*|All files (*.*)|*.*",
                    Title = "Select a File Location.",
                    CheckFileExists = true
                };
                var result = await picker.ShowDialogAsync();
                if( result == DialogResult.OK ) {
                    try {
                        var paths = File.ReadAllLines( picker.FileName );
                        foreach(var path in paths ) {
                            if( !string.IsNullOrEmpty( path ) ) {
                                _plugin.Configuration.AddPath( path, out var newItem );
                            }
                        }
                    }
                    catch( Exception ex ) {
                        PluginLog.LogError( ex, "Could not select a file" );
                    }
                }
            } );
        }
    }
}
using Dalamud.Interface;
using Dalamud.Logging;
using ImGuiNET;
using System;
using System.IO;
using System.Numerics;
using VFXSelect.UI;

namespace EasyEyes.UI {
    public class VfxTab {
        public Plugin Plugin;

        public VfxTab( Plugin plugin ) {
            Plugin = plugin;
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
                if( Plugin.DataManager.FileExists( AddVfxPath ) ) {
                    Plugin.Config.AddPath( AddVfxPath, out var newItem );
                    SelectedVfx = newItem;
                }
                AddVfxPath = "";
            }
            ImGui.SameLine();
            ImGui.PushFont( UiBuilder.IconFont );
            if( ImGui.Button( $"{( char )FontAwesomeIcon.Search}", new Vector2( 30, 23 ) ) ) {
                Plugin.MainUI.SelectUI.Show(showLocal: false);
            }
            ImGui.PopFont();

            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );

            ImGui.BeginChild( Id + "Tree", new Vector2(-1, ImGui.GetContentRegionAvail().Y - 22), true );
            VFXSelectDialog.DisplayVisible( Plugin.Config.Items.Count, out var preItems, out var showItems, out var postItems, out var itemHeight );
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + preItems * itemHeight );
            var idx = 0;
            foreach( var item in Plugin.Config.Items ) {
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
                Plugin.MainUI.DrawSpawnButton( "Spawn", Id, SelectedVfx.AVFXPath, SelectedVfx.Disabled );

                ImGui.Text( "Notes:" );
                ImGui.InputTextMultiline( Id + "-Description", ref SelectedVfx.Notes, 400, new Vector2( ImGui.GetContentRegionAvail().X, 200 ) );
                if( ImGui.Button( "Save" + Id ) ) {
                    Plugin.Config.Save();
                }
                ImGui.SameLine();
                if( MainInterface.RemoveButton( "Delete" ) ) {
                    Plugin.Config.RemoveItem( SelectedVfx );
                    SelectedVfx = null;
                }
            }
            ImGui.Columns( 1 );
            ImGui.EndTabItem();
        }

        public void ExportDialog() {
            Plugin.DialogManager.SaveFileDialog( "Select a Save Location", ".txt,.*", "exported_vfx", "txt", ( bool ok, string res ) => {
                if ( ok ) {
                    try {
                        if( Plugin.Config.Items.Count > 0 ) {
                            var paths = Plugin.Config.Items.ConvertAll( x => x.AVFXPath ).ToArray();
                            File.WriteAllLines( res, paths );
                        }
                    }
                    catch( Exception ex ) {
                        PluginLog.LogError( ex, "Could not select a file" );
                    }
                }
            } );
        }

        public void ImportDialog() {
            Plugin.DialogManager.OpenFileDialog( "Select a File Location", ".txt,.*", ( bool ok, string res ) => {
                if( ok ) {
                    try {
                        var paths = File.ReadAllLines( res );
                        foreach( var path in paths ) {
                            if( !string.IsNullOrEmpty( path ) ) {
                                Plugin.Config.AddPath( path, out var newItem );
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

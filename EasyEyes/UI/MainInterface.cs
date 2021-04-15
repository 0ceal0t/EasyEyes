using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Interface;
using Dalamud.Plugin;
using ImGuiNET;
using EasyEyes.Structs.Vfx;

namespace EasyEyes.UI
{
    public class MainInterface {
        private readonly Plugin _plugin;
        public bool Visible = false;
        public bool ShowDebugBar = false;
        public BaseVfx SpawnVfx = null;

        public MainInterface( Plugin plugin )  {
            _plugin = plugin;
#if DEBUG
            Visible = true;
#endif
        }

        public void Draw() {
            if( Visible ) {
                DrawMainInterface();
            }
        }

        public void DrawMainInterface() {
            ImGui.SetNextWindowSize( new Vector2( 400, 500 ), ImGuiCond.FirstUseEver );
            var ret = ImGui.Begin( _plugin.Name, ref Visible );
            if( !ret ) return;

            ImGui.BeginTabBar( "MainInterfaceTabs" );
            DrawVfxs();
            DrawLog();
            ImGui.EndTabBar();


            ImGui.End();
        }

        public SavedItem SelectedVfx = null;
        public string AddVfxPath = "";
        public void DrawVfxs() {
            var ret = ImGui.BeginTabItem( "VFXs##MainInterfaceTabs" );
            if( !ret ) return;

            var Id = "##VFXs";
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );
            ImGui.Columns( 2, Id + "Columns", true );
            ImGui.BeginChild( Id + "Tree" );
            DisplayVisible( _plugin.Configuration.Items.Count, out int preItems, out int showItems, out int postItems, out float itemHeight );
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
            ImGui.NextColumn();
            // ========================
            ImGui.InputText( "Path" + Id, ref AddVfxPath, 255 );
            ImGui.SameLine();
            if(ImGui.Button("Add" + Id ) ) {
                _plugin.Configuration.AddPath( AddVfxPath, out var newItem );
                SelectedVfx = newItem;
                AddVfxPath = "";
            }
            if( SelectedVfx == null ) {
                ImGui.Text( "Select an item..." );
            }
            else {
                ImGui.Text( SelectedVfx.AVFXPath );
                ImGui.SameLine();
                if( ImGui.Button( "Delete" ) ) {
                    _plugin.Configuration.RemoveItem( SelectedVfx );
                    SelectedVfx = null;
                }
                ImGui.Checkbox( "Disabled" + Id, ref SelectedVfx.Disabled );
                ImGui.InputTextMultiline( "Description" + Id, ref SelectedVfx.Notes, 400, new Vector2(200, 200) );
                if(ImGui.Button("Save" + Id ) ) {
                    _plugin.Configuration.Save();
                }
            }
            ImGui.Columns( 1 );
            ImGui.EndTabItem();
        }

        public string SelectedLogPath = "";
        public void DrawLog() {
            var ret = ImGui.BeginTabItem( "Log##MainInterfaceTabs" );
            if( !ret ) return;

            var Id = "##Log";
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );
            if( ImGui.Button((_plugin.DoRecord ? "Stop" : "Record") + Id) ) {
                _plugin.DoRecord = !_plugin.DoRecord;
            }
            ImGui.SameLine();
            if(ImGui.Button("RESET" + Id ) ) {
                _plugin.ClearRecord();
                SelectedLogPath = "";
            }
            var disabled = string.IsNullOrEmpty( SelectedLogPath );
            // ========= ADD ========
            if( disabled ) ImGui.PushStyleVar( ImGuiStyleVar.Alpha, ImGui.GetStyle().Alpha * 0.5f );
            ImGui.SameLine();
            if( ImGui.Button( "Add" + Id ) && !disabled ) {
                _plugin.Configuration.AddPath( SelectedLogPath, out var newItem );
                SelectedVfx = newItem;
            }
            if( disabled ) ImGui.PopStyleVar();
            // ======== SPAWN / REMOVE =========
            if( SpawnVfx == null ) {
                if( disabled ) ImGui.PushStyleVar( ImGuiStyleVar.Alpha, ImGui.GetStyle().Alpha * 0.5f );
                ImGui.SameLine();
                if( ImGui.Button( "Spawn" + Id ) && !disabled ) {
                    ImGui.OpenPopup( "Spawn_Popup" );
                }
                if( disabled ) ImGui.PopStyleVar();
            }
            else {
                ImGui.SameLine();
                if( ImGui.Button( "Remove" + Id ) ) {
                    SpawnVfx?.Remove();
                    SpawnVfx = null;
                }
            }
            if( ImGui.BeginPopup( "Spawn_Popup" ) ) {
                if( ImGui.Selectable( "On Ground" ) ) {
                    SpawnVfx = new StaticVfx( _plugin, SelectedLogPath, _plugin.PluginInterface.ClientState.LocalPlayer.Position );
                }
                if( ImGui.Selectable( "On Self" ) ) {
                    SpawnVfx = new ActorVfx( _plugin, _plugin.PluginInterface.ClientState.LocalPlayer, _plugin.PluginInterface.ClientState.LocalPlayer, SelectedLogPath );
                }
                ImGui.EndPopup();
            }

            ImGui.BeginChild( Id + "Tree" );
            DisplayVisible( _plugin.Recorded.Count, out int preItems, out int showItems, out int postItems, out float itemHeight );
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + preItems * itemHeight );
            int idx = 0;
            foreach( var item in _plugin.Recorded ) {
                if( idx < preItems || idx > ( preItems + showItems ) ) { idx++; continue; }

                var wasDisabled = item.removed;

                if( ImGui.Selectable( item.path + Id + idx, SelectedLogPath == item.path ) ) {
                    SelectedLogPath = item.path;
                }
                idx++;
            }
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + postItems * itemHeight );
            ImGui.EndChild();
            ImGui.EndTabItem();
        }

        public void Dispose() {

        }

        public static void DisplayVisible( int count, out int preItems, out int showItems, out int postItems, out float itemHeight ) {
            //float childHeight = ImGui.GetWindowSize().Y - ImGui.GetCursorPosY();
            float childHeight = ImGui.GetContentRegionAvail().Y;
            var scrollY = ImGui.GetScrollY();
            var style = ImGui.GetStyle();
            itemHeight = ImGui.GetTextLineHeight() + style.ItemSpacing.Y;
            preItems = ( int )Math.Floor( scrollY / itemHeight );
            showItems = ( int )Math.Ceiling( childHeight / itemHeight );
            postItems = count - showItems - preItems;

        }

        /*
         * if( ImGui.BeginPopup( "Spawn_Popup" ) ) {
                if( ImGui.Selectable( "On Ground" ) ) {
                    SpawnVfx = new StaticVfx( _plugin, previewSpawn, _plugin.PluginInterface.ClientState.LocalPlayer.Position);
                }
                if( ImGui.Selectable( "On Self" ) ) {
                    SpawnVfx = new ActorVfx( _plugin, _plugin.PluginInterface.ClientState.LocalPlayer, _plugin.PluginInterface.ClientState.LocalPlayer, previewSpawn );
                }
                if (ImGui.Selectable("On Taget" ) ) {
                    var t = _plugin.PluginInterface.ClientState.Targets.CurrentTarget;
                    if(t != null ) {
                        SpawnVfx = new ActorVfx( _plugin, t, t, previewSpawn );
                    }
                }
                ImGui.EndPopup();
            }
         */
    }
}
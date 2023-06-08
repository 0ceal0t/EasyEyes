using ImGuiNET;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using VFXSelect.UI;

namespace EasyEyes.UI {
    public class LogTab {
        public Plugin Plugin;
        public string SelectedLogPath = "";
        private string SearchInput = "";

        public LogTab( Plugin plugin ) {
            Plugin = plugin;
        }

        public void Draw() {
            var ret = ImGui.BeginTabItem( "Recent VFXs##MainInterfaceTabs" );
            if( !ret ) return;

            var Id = "##Log";
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );
            if( Plugin.DoRecord ) {
                if( MainInterface.RemoveButton( "Stop" + Id ) ) {
                    Plugin.DoRecord = false;
                }
            }
            else {
                if( MainInterface.OkButton( "Record" + Id ) ) {
                    Plugin.DoRecord = true;
                }
            }
            ImGui.SameLine();
            if( ImGui.Button( "Reset" + Id ) ) {
                Plugin.ClearRecord();
                SelectedLogPath = "";
            }

            var disabled = string.IsNullOrEmpty( SelectedLogPath );

            // ========= ADD ========
            if( disabled ) ImGui.PushStyleVar( ImGuiStyleVar.Alpha, ImGui.GetStyle().Alpha * 0.5f );

            ImGui.SameLine();
            if( ImGui.Button( "Add To Blacklist" + Id ) && !disabled ) {
                Plugin.Config.AddPath( SelectedLogPath, out var _ );
            }

            if( disabled ) ImGui.PopStyleVar();

            // ======== SPAWN / REMOVE =========
            Plugin.MainUI.DrawSpawnButton( "Spawn", Id, SelectedLogPath, disabled );
            var resetScroll = false;

            if( ImGui.InputTextWithHint( "##Search", "Search", ref SearchInput, 255 ) ) {
                resetScroll = true;
            }

            var searched = new List<string>();
            if( string.IsNullOrEmpty( SearchInput ) ) {
                searched.AddRange( Plugin.Recorded );
            }
            else {
                searched.AddRange( Plugin.Recorded.Where( x => x.ToLower().Contains( SearchInput.ToLower() ) ) );
            }

            //=======================
            ImGui.BeginChild( Id + "Tree", new Vector2( -1, -1 ), true );

            if( searched.Count > 0 ) {
                VFXSelectDialog.DisplayVisible( searched.Count, out var preItems, out var showItems, out var postItems, out var itemHeight );
                ImGui.SetCursorPosY( ImGui.GetCursorPosY() + preItems * itemHeight );
                if( resetScroll ) { ImGui.SetScrollHereY(); };

                var idx = 0;
                foreach( var item in searched ) {
                    if( idx < preItems || idx > ( preItems + showItems ) ) { idx++; continue; }

                    if( ImGui.Selectable( item + Id + idx, SelectedLogPath == item ) ) {
                        SelectedLogPath = item;
                    }
                    idx++;
                }

                ImGui.SetCursorPosY( ImGui.GetCursorPosY() + postItems * itemHeight );
            }
            else {
                ImGui.Text( "Press [Record] to view recent VFXs..." );
            }

            ImGui.EndChild();
            ImGui.EndTabItem();
        }
    }
}

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
using EasyEyes.Structs.Vfx;
using ImGuiNET;
using VFXSelect.UI;

namespace EasyEyes.UI
{
    public class MainInterface {
        private readonly Plugin _plugin;
        public bool Visible = false;
        public bool ShowDebugBar = false;

        public LogTab _Log;
        public VfxTab _Vfx;
        public SettingsTab _Settings;
        public VFXSelectDialog SelectUI;

        public MainInterface( Plugin plugin )  {
            _plugin = plugin;
            _Log = new LogTab( plugin );
            _Vfx = new VfxTab( plugin );
            _Settings = new SettingsTab( plugin );
            SelectUI = new VFXSelectDialog( _plugin._Sheets, "File Select", _plugin.Configuration.RecentSelects );
            SelectUI.OnSelect += _plugin.AddVfx;
            SelectUI.OnAddRecent += _plugin.Configuration.AddRecent;

#if DEBUG
            Visible = true;
#endif
        }

        public void Draw() {
            if( !Visible ) return;

            SelectUI.Draw();

            ImGui.SetNextWindowSize( new Vector2( 400, 500 ), ImGuiCond.FirstUseEver );
            var ret = ImGui.Begin( _plugin.Name, ref Visible );
            if( !ret ) return;

            ImGui.BeginTabBar( "MainInterfaceTabs" );
            _Vfx.Draw();
            _Log.Draw();
            ImGui.EndTabBar();

            ImGui.End();
        }

        public void Dispose() {
        }

        public static void DisplayVisible( int count, out int preItems, out int showItems, out int postItems, out float itemHeight ) {
            float childHeight = ImGui.GetContentRegionAvail().Y;
            var scrollY = ImGui.GetScrollY();
            var style = ImGui.GetStyle();
            itemHeight = ImGui.GetTextLineHeight() + style.ItemSpacing.Y;
            preItems = ( int )Math.Floor( scrollY / itemHeight );
            showItems = ( int )Math.Ceiling( childHeight / itemHeight );
            postItems = count - showItems - preItems;
        }

        public static bool OkButton( string label, bool small = false ) {
            return ColoredButton( label, new Vector4( 0.10f, 0.80f, 0.10f, 1.0f ), small );
        }
        public static bool RemoveButton(string label, bool small = false ) {
            return ColoredButton( label, new Vector4( 0.80f, 0.10f, 0.10f, 1.0f ), small );
        }
        public static bool ColoredButton( string label, Vector4 color, bool small) {
            bool ret = false;
            ImGui.PushStyleColor( ImGuiCol.Button, color );
            if( small ) {
                if( ImGui.SmallButton( label ) ) {
                    ret = true;
                }
            }
            else {
                if( ImGui.Button( label ) ) {
                    ret = true;
                }
            }
            ImGui.PopStyleColor();
            return ret;
        }

        public void DrawSpawnButton(string text, string Id, string path, bool disabled) {
            if( _plugin.SpawnVfx == null ) {
                if( disabled ) ImGui.PushStyleVar( ImGuiStyleVar.Alpha, ImGui.GetStyle().Alpha * 0.5f );
                ImGui.SameLine();
                if( ImGui.Button( text + Id ) && !disabled ) {
                    ImGui.OpenPopup( "Spawn_Popup" );
                }
                if( disabled ) ImGui.PopStyleVar();
            }
            else {
                ImGui.SameLine();
                if( ImGui.Button( "Remove" + Id ) ) {
                    _plugin.SpawnVfx?.Remove();
                    _plugin.SpawnVfx = null;
                }
            }
            if( ImGui.BeginPopup( "Spawn_Popup" ) ) {
                if( ImGui.Selectable( "On Ground" ) ) {
                    _plugin.SpawnVfx = new StaticVfx( _plugin, path, _plugin.PluginInterface.ClientState.LocalPlayer.Position );
                }
                if( ImGui.Selectable( "On Self" ) ) {
                    _plugin.SpawnVfx = new ActorVfx( _plugin, _plugin.PluginInterface.ClientState.LocalPlayer, _plugin.PluginInterface.ClientState.LocalPlayer, path );
                }
                ImGui.EndPopup();
            }
        }
    }
}
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

namespace EasyEyes.UI
{
    public class MainInterface {
        private readonly Plugin _plugin;
        public bool Visible = false;
        public bool ShowDebugBar = false;

        public LogTab _Log;
        public VfxTab _Vfx;
        public SettingsTab _Settings;

        public BaseVfx SpawnVfx {
            get { return _Log.SpawnVfx; }
            set { _Log.SpawnVfx = value; }
        }

        public MainInterface( Plugin plugin )  {
            _plugin = plugin;
            _Log = new LogTab( plugin );
            _Vfx = new VfxTab( plugin );
            _Settings = new SettingsTab( plugin );

#if DEBUG
            Visible = true;
#endif
        }

        public void Draw() {
            if( !Visible ) return;

            ImGui.SetNextWindowSize( new Vector2( 400, 500 ), ImGuiCond.FirstUseEver );
            var ret = ImGui.Begin( _plugin.Name, ref Visible );
            if( !ret ) return;

            ImGui.BeginTabBar( "MainInterfaceTabs" );
            _Vfx.Draw();
            _Log.Draw();
            //_Settings.Draw(); // DONT NEED THIS YET
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
    }
}
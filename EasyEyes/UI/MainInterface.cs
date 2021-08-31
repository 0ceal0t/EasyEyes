using System.Numerics;
using EasyEyes.Structs.Vfx;
using ImGuiNET;
using VFXSelect.UI;

namespace EasyEyes.UI
{
    public class MainInterface {
        private readonly Plugin Plugin;
        public bool Visible = false;
        public bool ShowDebugBar = false;

        public LogTab Log;
        public VfxTab Vfx;
        public VFXSelectDialog SelectUI;

        public MainInterface( Plugin plugin )  {
            Plugin = plugin;
            Log = new LogTab( plugin );
            Vfx = new VfxTab( plugin );

            SelectUI = new VFXSelectDialog(
                "File Select",
                null,
                showSpawn: true,
                spawnVfxExists: () => SpawnExists(),
                removeSpawnVfx: () => RemoveSpawnVfx(),
                spawnOnGround: ( string path ) => SpawnOnGround( path ),
                spawnOnSelf: ( string path ) => SpawnOnSelf( path ),
                spawnOnTarget: ( string path ) => SpawnOnTarget( path )
            );
            SelectUI.OnSelect += Plugin.AddVfx;
        }

        public bool SpawnExists() {
            return Plugin.SpawnVfx != null;
        }

        public void RemoveSpawnVfx() {
            Plugin.SpawnVfx?.Remove();
            Plugin.SpawnVfx = null;
        }

        public void SpawnOnGround( string path ) {
            Plugin.SpawnVfx = new StaticVfx( Plugin, path, Plugin.ClientState.LocalPlayer.Position );
        }

        public void SpawnOnSelf( string path ) {
            Plugin.SpawnVfx = new ActorVfx( Plugin, Plugin.ClientState.LocalPlayer, Plugin.ClientState.LocalPlayer, path );
        }

        public void SpawnOnTarget( string path ) {
            var t = Plugin.TargetManager.Target;
            if( t != null ) {
                Plugin.SpawnVfx = new ActorVfx( Plugin, t, t, path );
            }
        }

        public void Draw() {
            if( !Visible ) return;

            SelectUI.Draw();

            // =================
            ImGui.SetNextWindowSize( new Vector2( 400, 500 ), ImGuiCond.FirstUseEver );
            var ret = ImGui.Begin( Plugin.Name, ref Visible );
            if( !ret ) return;

            ImGui.BeginTabBar( "MainInterfaceTabs" );
            Vfx.Draw();
            Log.Draw();
            ImGui.EndTabBar();

            ImGui.End();
        }

        public void Dispose() {
        }

        public static bool OkButton( string label, bool small = false ) {
            return ColoredButton( label, new Vector4( 0.10f, 0.80f, 0.10f, 1.0f ), small );
        }
        public static bool RemoveButton(string label, bool small = false ) {
            return ColoredButton( label, new Vector4( 0.80f, 0.10f, 0.10f, 1.0f ), small );
        }
        public static bool ColoredButton( string label, Vector4 color, bool small) {
            var ret = false;
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
            if( Plugin.SpawnVfx == null ) {
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
                    RemoveSpawnVfx();
                }
            }
            if( ImGui.BeginPopup( "Spawn_Popup" ) ) {
                if( ImGui.Selectable( "On Ground" ) ) {
                    SpawnOnGround( path );
                }
                if( ImGui.Selectable( "On Self" ) ) {
                    SpawnOnSelf( path );
                }
                ImGui.EndPopup();
            }
        }
    }
}
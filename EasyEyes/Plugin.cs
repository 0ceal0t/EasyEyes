using Dalamud.Game.Command;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Plugin;
using EasyEyes.Structs.Vfx;
using EasyEyes.UI;
using System.Collections.Generic;
using System.IO;
using VFXSelect;
using VFXSelect.UI;

namespace EasyEyes {
    public class Plugin : IDalamudPlugin {
        private const string CommandName = "/easy";

        public static FileDialogManager DialogManager { get; private set; }

        public static ResourceLoader ResourceLoader { get; private set; }

        public BaseVfx SpawnVfx = null;
        public Configuration Config;
        public MainInterface MainUI;

        public string PluginDebugTitleStr;
        public string RootLocation;
        public string FileLocation;

        public Plugin( IDalamudPluginInterface pluginInterface ) {
            pluginInterface.Create<Services>();
            DialogManager = new();

            Config = Services.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Config.Initialize( Services.PluginInterface );

            ResourceLoader = new ResourceLoader( this );

            Services.CommandManager.AddHandler( CommandName, new CommandInfo( OnCommand ) {
                HelpMessage = "toggle ui"
            } );

            RootLocation = Services.PluginInterface.AssemblyLocation.DirectoryName;

            FileLocation = Path.Combine( RootLocation, "does_not_exist.avfx" );

            SheetManager.Initialize(
                Path.Combine( RootLocation, "Files", "npc.csv" ),
                Path.Combine( RootLocation, "Files", "monster_vfx.json" ),
                Services.DataManager,
                Services.PluginInterface
            );

            MainUI = new MainInterface( this );

            ResourceLoader.Init();
            ResourceLoader.Enable();

            Services.PluginInterface.UiBuilder.Draw += MainUI.Draw;
            Services.PluginInterface.UiBuilder.OpenConfigUi += OpenMainUi;
            Services.PluginInterface.UiBuilder.OpenMainUi += OpenMainUi;
        }

        public void ClearSpawnVfx() {
            SpawnVfx = null;
        }

        public void Dispose() {
            Services.PluginInterface.UiBuilder.Draw -= MainUI.Draw;
            Services.PluginInterface.UiBuilder.OpenConfigUi -= OpenMainUi;
            Services.PluginInterface.UiBuilder.OpenMainUi -= OpenMainUi;

            ResourceLoader?.Dispose();

            SpawnVfx?.Remove();
            SpawnVfx = null;

            Services.CommandManager.RemoveHandler( CommandName );
            MainUI?.Dispose();
        }

        private void OpenMainUi() {
            MainUI.Visible = true;
        }

        private void OnCommand( string command, string rawArgs ) {
            MainUI.Visible = !MainUI.Visible;
        }

        public void AddVfx( VFXSelectResult result ) {
            Config.AddPath( result.Path, out var _ );
        }

        public List<string> Recorded = [];
        public bool DoRecord = false;
        public void AddRecord( string path ) {
            if( !DoRecord ) return;
            if( Recorded.Contains( path ) ) return;
            Recorded.Add( path );
        }

        public void ClearRecord() {
            Recorded.Clear();
        }
    }
}
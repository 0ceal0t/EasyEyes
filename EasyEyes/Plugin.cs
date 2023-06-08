using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
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
        public string Name => "EasyEyes";
        private const string CommandName = "/easy";

        public static DalamudPluginInterface PluginInterface { get; private set; }
        public static ClientState ClientState { get; private set; }
        public static CommandManager CommandManager { get; private set; }
        public static SigScanner SigScanner { get; private set; }
        public static DataManager DataManager { get; private set; }
        public static TargetManager TargetManager { get; private set; }
        public static FileDialogManager DialogManager { get; private set; }

        public static ResourceLoader ResourceLoader { get; private set; }

        public BaseVfx SpawnVfx = null;
        public Configuration Config;
        public MainInterface MainUI;

        public string PluginDebugTitleStr;
        public string RootLocation;
        public string FileLocation;

        public Plugin(
                DalamudPluginInterface pluginInterface,
                ClientState clientState,
                CommandManager commandManager,
                SigScanner sigScanner,
                DataManager dataManager,
                TargetManager targetManager
            ) {
            PluginInterface = pluginInterface;
            ClientState = clientState;
            CommandManager = commandManager;
            SigScanner = sigScanner;
            DataManager = dataManager;
            TargetManager = targetManager;
            DialogManager = new();

            Config = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Config.Initialize( PluginInterface );

            ResourceLoader = new ResourceLoader( this );

            CommandManager.AddHandler( CommandName, new CommandInfo( OnCommand ) {
                HelpMessage = "toggle ui"
            } );

            RootLocation = PluginInterface.AssemblyLocation.DirectoryName;

            FileLocation = Path.Combine( RootLocation, "does_not_exist.avfx" );

            SheetManager.Initialize(
                Path.Combine( RootLocation, "Files", "npc.csv" ),
                Path.Combine( RootLocation, "Files", "monster_vfx.json" ),
                DataManager,
                PluginInterface
            );

            MainUI = new MainInterface( this );

            ResourceLoader.Init();
            ResourceLoader.Enable();

            PluginInterface.UiBuilder.Draw += MainUI.Draw;
        }

        public void ClearSpawnVfx() {
            SpawnVfx = null;
        }

        public void Dispose() {
            PluginInterface.UiBuilder.Draw -= MainUI.Draw;

            ResourceLoader?.Dispose();

            SpawnVfx?.Remove();
            SpawnVfx = null;

            CommandManager.RemoveHandler( CommandName );
            MainUI?.Dispose();
        }

        private void OnCommand( string command, string rawArgs ) {
            MainUI.Visible = !MainUI.Visible;
        }

        public void AddVfx( VFXSelectResult result ) {
            Config.AddPath( result.Path, out var _ );
        }

        public List<string> Recorded = new();
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
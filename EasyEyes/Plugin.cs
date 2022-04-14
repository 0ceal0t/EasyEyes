using System;

using System.Collections.Generic;
using System.IO;
using Dalamud.Game.Command;
using Dalamud.Plugin;

using System.Reflection;

using EasyEyes.UI;
using EasyEyes.Structs.Vfx;

using VFXSelect;
using VFXSelect.UI;

using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game;
using Dalamud.Data;

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

        public static ResourceLoader ResourceLoader { get; private set; }

        public BaseVfx SpawnVfx = null;
        public Configuration Config;

        public MainInterface MainUI;

        public string PluginDebugTitleStr;
        public string AssemblyLocation { get; set; } = Assembly.GetExecutingAssembly().Location;
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

            Config = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Config.Initialize( PluginInterface );

            ResourceLoader = new ResourceLoader( this );

            CommandManager.AddHandler( CommandName, new CommandInfo( OnCommand ) {
                HelpMessage = "toggle ui"
            } );

            FileLocation = Path.Combine( Path.GetDirectoryName( AssemblyLocation ), "does_not_exist.avfx" );

            SheetManager.Initialize(
                Path.Combine( Path.GetDirectoryName( AssemblyLocation ), "Files", "npc.csv" ),
                Path.Combine( Path.GetDirectoryName( AssemblyLocation ), "Files", "monster_vfx.json" ),
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

        public struct RecordedItem {
            public string path;
        }

        public void AddVfx( VFXSelectResult result ) {
            Config.AddPath( result.Path, out var _ );
        }

        public List<RecordedItem> Recorded = new();
        public bool DoRecord = false;
        public void AddRecord( string path ) {
            if( !DoRecord ) return;

            var item = new RecordedItem {
                path = path
            };
            Recorded.Add( item );
        }

        public void ClearRecord() {
            Recorded.Clear();
        }
    }
}
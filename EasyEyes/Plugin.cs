using System;
using System.Collections.Generic;
using System.IO;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using ImGuiNET;
using System.Reflection;
using EasyEyes.UI;

namespace EasyEyes {
    public class Plugin : IDalamudPlugin {
        public string Name => "EasyEyes";
        private const string CommandName = "/easy";

        public DalamudPluginInterface PluginInterface;
        public Configuration Configuration;
        public ResourceLoader ResourceLoader;

        public MainInterface MainUI;

        public string PluginDebugTitleStr;
        public string AssemblyLocation { get; set; } = Assembly.GetExecutingAssembly().Location;

        public void Initialize( DalamudPluginInterface pluginInterface ) {
            PluginInterface = pluginInterface;

            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize( PluginInterface );
            ResourceLoader = new ResourceLoader( this );
            PluginInterface.CommandManager.AddHandler( CommandName, new CommandInfo( OnCommand ) {
                HelpMessage = "/easy - toggle ui"
            } );

            PluginDebugTitleStr = $"{Name} - Debug Build";

            ResourceLoader.Init();
            ResourceLoader.Enable();
            MainUI = new MainInterface( this );
            PluginInterface.UiBuilder.OnBuildUi += MainUI.Draw;
        }

        public void Dispose() {
            PluginInterface.UiBuilder.OnBuildUi -= MainUI.Draw;

            PluginInterface.CommandManager.RemoveHandler( CommandName );
            PluginInterface?.Dispose();
            MainUI?.Dispose();
            ResourceLoader?.Dispose();
        }

        private void OnCommand( string command, string rawArgs ) {
            MainUI.Visible = !MainUI.Visible;
        }

        public struct RecordedItem {
            public string path;
            public bool removed;

            //public override int GetHashCode() => ( path ).GetHashCode();
            //public override bool Equals( object obj ) => obj is RecordedItem other && this.Equals( other );
            //public bool Equals( RecordedItem p ) => path == p.path;
        }
        public List<RecordedItem> Recorded = new List<RecordedItem>();
        public bool DoRecord = false;
        public void Record(string path, bool removed ) {
            if( !DoRecord ) return;

            var item = new RecordedItem {
                path = path,
                removed = removed
            };
            Recorded.Add( item );
        }

        public void ClearRecord() {
            Recorded.Clear();
        }
    }
}
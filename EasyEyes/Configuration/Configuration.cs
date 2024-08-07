using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace EasyEyes {

    [Serializable]
    public class SavedItem {
        public string AVFXPath;
        public string Notes = "";
        public bool Disabled = true;

        public SavedItem( string path ) {
            AVFXPath = path;
        }
    }

    [Serializable]
    public class Configuration : IPluginConfiguration {
        public int Version { get; set; } = 0;
        public List<SavedItem> Items = [];

        [NonSerialized]
        private IDalamudPluginInterface _pluginInterface;
        [NonSerialized]
        public static Configuration Config;

        public void Initialize( IDalamudPluginInterface pluginInterface ) {
            _pluginInterface = pluginInterface;
            Config = this;
        }

        public bool IsDisabled( string path ) {
            foreach( var item in Items ) {
                if( item.AVFXPath == path && item.Disabled ) {
                    return true;
                }
            }
            return false;
        }

        // ============
        public bool AddPath( string path, out SavedItem newItem ) {
            newItem = null;
            foreach( var item in Items ) {
                if( item.AVFXPath == path ) {
                    return false;
                }
            }
            newItem = new SavedItem( path );
            Items.Add( newItem );
            Save();
            return true;
        }

        public void RemoveItem( SavedItem item ) {
            Items.Remove( item );
            Save();
        }

        public void Save() {
            _pluginInterface.SavePluginConfig( this );
        }
    }
}
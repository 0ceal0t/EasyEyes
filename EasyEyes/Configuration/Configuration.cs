using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using VFXSelect.UI;

namespace EasyEyes {

    [Serializable]
    public class SavedItem {
        public string AVFXPath;
        public string Notes = "";
        public bool Disabled = true;

        public SavedItem(string _path ) {
            AVFXPath = _path;
        }
    }

    [Serializable]
    public class Configuration : IPluginConfiguration {
        public int Version { get; set; } = 0;
        public List<SavedItem> Items = new List<SavedItem>();
        public List<VFXSelectResult> RecentSelects = new List<VFXSelectResult>();

        [NonSerialized]
        private DalamudPluginInterface _pluginInterface;
        [NonSerialized]
        public static Configuration Config;

        public void Initialize( DalamudPluginInterface pluginInterface ) {
            _pluginInterface = pluginInterface;
            Config = this;
        }

        public bool IsDisabled(string path ) {
            foreach(var item in Items ) {
                if(item.AVFXPath == path && item.Disabled ) {
                    return true;
                }
            }
            return false;
        }

        // ============
        public bool AddPath(string path, out SavedItem newItem ) {
            newItem = null;
            foreach(var item in Items ) {
                if(item.AVFXPath == path ) {
                    return false;
                }
            }
            newItem = new SavedItem( path );
            Items.Add( newItem );
            Save();
            return true;
        }

        public void RemoveItem(SavedItem item ) {
            Items.Remove( item );
            Save();
        }
        // ==============
        public void AddRecent( VFXSelectResult result ) {
            if( RecentSelects.Contains( result ) ) {
                RecentSelects.Remove( result ); // want to move it to the top
            }
            RecentSelects.Add( result );
            if( RecentSelects.Count > 10 ) {
                RecentSelects.RemoveRange( 0, RecentSelects.Count - 10 );
            }
            Save();
        }

        public void Save() {
            _pluginInterface.SavePluginConfig( this );
        }
    }
}
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyEyes.UI {
    public class PresetTab {
        public Plugin _plugin;

        public PresetTab( Plugin plugin ) {
            _plugin = plugin;
        }

        public void Draw() {
            var ret = ImGui.BeginTabItem( "Presets##MainInterfaceTabs" );
            if( !ret ) return;

            var Id = "##Preset";
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );

            if( ImGui.Button( "Save" ) ) {
                _plugin.Configuration.Save();
            }

            ImGui.EndTabItem();
        }
    }
}
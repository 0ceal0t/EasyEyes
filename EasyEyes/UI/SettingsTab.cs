using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyEyes.UI {
    public class SettingsTab {
        public Plugin _plugin;
        public string DummyInput;

        public SettingsTab( Plugin plugin ) {
            _plugin = plugin;
            DummyInput = _plugin.Configuration.DUMMY_VFX;
        }

        public void Draw() {
            var ret = ImGui.BeginTabItem( "Settings##MainInterfaceTabs" );
            if( !ret ) return;

            var Id = "##Settings";
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );

            if( ImGui.Button( "Save" ) ) {
                _plugin.Configuration.Save();
            }

            ImGui.EndTabItem();
        }
    }
}

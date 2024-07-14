using Dalamud.Interface.Textures;
using ImGuiNET;
using System.Numerics;
using VFXSelect.Data.Rows;

namespace VFXSelect.UI {
    public class VFXCommonSelect : VFXSelectTab<XivCommon, XivCommon> {
        private ISharedImmediateTexture Icon;

        public VFXCommonSelect( string parentId, string tabId, VFXSelectDialog dialog ) :
            base( parentId, tabId, SheetManager.Misc, dialog ) {
        }

        public override bool CheckMatch( XivCommon item, string searchInput ) {
            return VFXSelectDialog.Matches( item.Name, searchInput );
        }

        public override void OnSelect() {
            LoadIcon( Selected.Icon, ref Icon );
        }

        public override void DrawSelected( XivCommon loadedItem ) {
            if( loadedItem == null ) { return; }
            ImGui.Text( loadedItem.Name );
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );

            if( Icon != null && Icon.GetWrapOrDefault() != null ) {
                ImGui.Image( Icon.GetWrapOrDefault().ImGuiHandle, new Vector2( Icon.GetWrapOrDefault().Width, Icon.GetWrapOrDefault().Height ) );
            }

            ImGui.Text( "VFX Path: " );
            ImGui.SameLine();
            VFXSelectDialog.DisplayPath( loadedItem.VfxPath );

            if( ImGui.Button( "SELECT" + Id ) ) {
                Dialog.Invoke( new VFXSelectResult( VFXSelectType.GameStatus, "[COMMON] " + loadedItem.Name, loadedItem.VfxPath ) );
            }
            ImGui.SameLine();
            VFXSelectDialog.Copy( loadedItem.VfxPath, id: Id + "Copy" );
            Dialog.Spawn( loadedItem.VfxPath, id: Id + "Spawn" );
        }

        public override string UniqueRowTitle( XivCommon item ) {
            return item.Name + "##" + item.RowId;
        }
    }
}
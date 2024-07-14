using Dalamud.Interface.Textures;
using ImGuiNET;
using System.Numerics;
using VFXSelect.Data.Rows;

namespace VFXSelect.UI {
    public class VFXHousingSelect : VFXSelectTab<XivHousing, XivHousingSelected> {
        private ISharedImmediateTexture Icon;

        public VFXHousingSelect( string parentId, string tabId, VFXSelectDialog dialog ) :
            base( parentId, tabId, SheetManager.Housing, dialog ) {
        }

        public override void OnSelect() {
            LoadIcon( Selected.Icon, ref Icon );
        }

        public override bool CheckMatch( XivHousing item, string searchInput ) {
            return VFXSelectDialog.Matches( item.Name, searchInput );
        }

        public override void DrawSelected( XivHousingSelected loadedItem ) {
            if( loadedItem == null ) { return; }
            ImGui.Text( loadedItem.Housing.Name );
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );

            if( Icon != null && Icon.GetWrapOrDefault() != null ) {
                ImGui.Image( Icon.GetWrapOrDefault().ImGuiHandle, new Vector2( Icon.GetWrapOrDefault().Width, Icon.GetWrapOrDefault().Height ) );
            }

            ImGui.Text( "SGB Path: " );
            ImGui.SameLine();
            VFXSelectDialog.DisplayPath( loadedItem.Housing.sgbPath );

            var vfxIdx = 0;
            foreach( var path in loadedItem.VfxPaths ) {
                ImGui.Text( "VFX #" + vfxIdx + ": " );
                ImGui.SameLine();
                VFXSelectDialog.DisplayPath( path );
                if( ImGui.Button( "SELECT" + Id + vfxIdx ) ) {
                    Dialog.Invoke( new VFXSelectResult( VFXSelectType.GameItem, "[HOUSING] " + loadedItem.Housing.Name + " #" + vfxIdx, path ) );
                }
                ImGui.SameLine();
                VFXSelectDialog.Copy( path, id: Id + "Copy" + vfxIdx );
                Dialog.Spawn( path, id: Id + "Spawn" + vfxIdx );
                vfxIdx++;
            }
        }

        public override string UniqueRowTitle( XivHousing item ) {
            return item.Name + Id;
        }
    }
}
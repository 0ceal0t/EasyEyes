using Dalamud.Interface.Textures;
using EasyEyes;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VFXSelect.Data.Sheets;

namespace VFXSelect.UI {
    public abstract class VFXSelectTab {
        public abstract void Draw();
    }

    public abstract class VFXSelectTab<T, S> : VFXSelectTab {
        protected SheetLoader<T, S> Loader;
        protected VFXSelectDialog Dialog;

        public string Id;

        public string Name;
        public string ParentId;

        public string SearchInput = "";
        public T Selected = default;
        public S Loaded = default;

        public VFXSelectTab( string parentId, string tabId, SheetLoader<T, S> loader, VFXSelectDialog dialog ) {
            Loader = loader;
            Dialog = dialog;
            Name = tabId;
            ParentId = parentId;
            Id = "##Select/" + tabId + "/" + parentId;
        }

        public abstract bool CheckMatch( T item, string searchInput );
        public abstract string UniqueRowTitle( T item );
        public abstract void DrawSelected( S loadedItem );
        public virtual void DrawExtra() { }
        public virtual void OnSelect() { }

        public List<T> Searched;
        public override void Draw() {
            var ret = ImGui.BeginTabItem( Name + "##Select/" + ParentId );
            if( !ret )
                return;
            Loader.Load();
            if( !Loader.Loaded ) {
                ImGui.EndTabItem();
                return;
            }
            //
            Searched ??= [.. Loader.Items];
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );
            var ResetScroll = false;
            DrawExtra();
            if( ImGui.InputText( "Search" + Id, ref SearchInput, 255 ) ) {
                Searched = Loader.Items.Where( x => CheckMatch( x, SearchInput ) ).ToList();
                ResetScroll = true;
            }
            ImGui.Columns( 2, Id + "Columns", true );
            ImGui.BeginChild( Id + "Tree" );
            VFXSelectDialog.DisplayVisible( Searched.Count, out var preItems, out var showItems, out var postItems, out var itemHeight );
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + preItems * itemHeight );
            if( ResetScroll ) { ImGui.SetScrollHereY(); };
            var idx = 0;
            foreach( var item in Searched ) {
                if( idx < preItems || idx > ( preItems + showItems ) ) { idx++; continue; }
                if( ImGui.Selectable( UniqueRowTitle( item ), EqualityComparer<T>.Default.Equals( Selected, item ) ) ) {
                    if( !EqualityComparer<T>.Default.Equals( Selected, item ) ) {
                        Task.Run( async () => {
                            var result = Loader.SelectItem( item, out Loaded );
                        } );
                        Selected = item;
                        OnSelect();
                    }
                }
                idx++;
            }
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + postItems * itemHeight );
            ImGui.EndChild();
            ImGui.NextColumn();
            // ========================
            if( Selected == null ) {
                ImGui.Text( "Select an item..." );
            }
            else {
                if( Loaded != null ) {
                    ImGui.BeginChild( Id + "Selected" );

                    DrawSelected( Loaded );

                    ImGui.EndChild();
                }
                else {
                    ImGui.Text( "No data found" );
                }
            }
            ImGui.Columns( 1 );
            //
            ImGui.EndTabItem();
        }

        public void LoadIcon( uint iconId, ref ISharedImmediateTexture wrap ) {
            try {
                wrap = Services.TextureProvider.GetFromGameIcon( iconId < 0 ? 0 : iconId );
            }
            catch( Exception ) {
                wrap = Services.TextureProvider.GetFromGameIcon( 0 );
            }
        }
    }
}
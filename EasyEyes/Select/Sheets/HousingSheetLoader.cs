using EasyEyes;
using Lumina.Excel.Sheets;
using System;
using System.Linq;
using VFXSelect.Data.Rows;

namespace VFXSelect.Data.Sheets {
    public class HousingSheetLoader : SheetLoader<XivHousing, XivHousingSelected> {
        public override void OnLoad() {
            var sheet = SheetManager.DataManager.GetExcelSheet<HousingFurniture>().Where( x => x.ModelKey > 0 );
            foreach( var item in sheet ) {
                Items.Add( new XivHousing( item ) );
            }

            var sheet2 = SheetManager.DataManager.GetExcelSheet<HousingYardObject>().Where( x => x.ModelKey > 0 );
            foreach( var item in sheet2 ) {
                Items.Add( new XivHousing( item ) );
            }
        }

        public override bool SelectItem( XivHousing item, out XivHousingSelected selectedItem ) {
            selectedItem = null;
            var sgbPath = item.GetSbgPath();
            var result = SheetManager.DataManager.FileExists( sgbPath );
            if( result ) {
                try {
                    var file = SheetManager.DataManager.GetFile( sgbPath );
                    selectedItem = new XivHousingSelected( item, file );
                }
                catch( Exception e ) {
                    Services.Error( e, "Error loading SGB file " + sgbPath );
                    return false;
                }
            }
            return result;
        }
    }
}

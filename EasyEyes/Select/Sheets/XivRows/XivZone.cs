namespace VFXSelect.Data.Rows {

    public class XivZone {
        public string Name;
        public int RowId;

        public string LgbPath;

        // ffxiv/fst_f1/bah/f1bz/level/f1bz
        // bg/ffxiv/fst_f1/bah/f1bz/level/vfx.lgb

        public XivZone( Lumina.Excel.Sheets.TerritoryType zone ) {
            Name = zone.PlaceName.ValueNullable?.Name.ToString();
            RowId = ( int )zone.RowId;

            var bg = zone.Bg.ToString().Split( '/' );
            bg[^1] = "vfx.lgb";
            LgbPath = "bg/" + string.Join( "/", bg );
        }

        public string GetLgbPath() {
            return LgbPath;
        }
    }
}

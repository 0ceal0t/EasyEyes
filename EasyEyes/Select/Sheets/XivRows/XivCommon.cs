namespace VFXSelect.Data.Rows {
    public class XivCommon {
        public string Name;
        public ushort Icon;
        public string VfxPath;
        public int RowId;

        public XivCommon( int rowId, string vfxPath, string name, ushort icon ) {
            RowId = rowId;
            VfxPath = vfxPath;
            Name = name;
            Icon = icon;
        }

        public XivCommon( Lumina.Excel.Sheets.VFX vfx ) {
            RowId = ( int )vfx.RowId;
            Icon = 0;
            Name = vfx.Location.ToString();
            VfxPath = $"vfx/common/eff/{Name}.avfx";
        }
    }
}

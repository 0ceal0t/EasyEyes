namespace VFXSelect.Data.Rows {
    public class XivMount : XivNpcBase {
        public ushort Icon;

        public XivMount( Lumina.Excel.Sheets.Mount mount ) : base( mount.ModelChara.Value ) {
            Icon = mount.Icon;
            Name = mount.Singular.ToString();
        }
    }
}

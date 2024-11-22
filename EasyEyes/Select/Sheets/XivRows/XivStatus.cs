using System.Collections.Generic;

namespace VFXSelect.Data.Rows {
    public class XivStatus {
        public bool VfxExists = false;

        public string Name;
        public int RowId;
        public uint Icon;

        public List<string> LoopPaths;

        public static readonly string statusPrefix = "vfx/common/eff/";

        public XivStatus( Lumina.Excel.Sheets.Status status ) {
            Name = status.Name.ToString();
            RowId = ( int )status.RowId;
            Icon = status.Icon;

            //HitVFXPath = status.HitEffect.Value?.Location.Value?.Location;

            foreach( var vfx in status.VFX.ValueNullable?.VFX ?? [] ) {
                var path = vfx.ValueNullable?.Location.ExtractText();
                if( !string.IsNullOrEmpty( path ) ) LoopPaths.Add( path );
            }


            VfxExists = LoopPaths.Count > 0;
        }
    }
}

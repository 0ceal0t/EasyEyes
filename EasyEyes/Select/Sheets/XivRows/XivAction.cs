using System.Text;

namespace VFXSelect.Data.Rows {
    public class XivAction : XivActionBase {
        public XivAction( Lumina.Excel.Sheets.Action action, bool justSelf = false ) {
            Name = action.Name.ToString();
            RowId = ( int )action.RowId;
            Icon = action.Icon;

            SelfVFXKey = action.AnimationEnd.ValueNullable?.Key.ToString();
            SelfVFXExists = !string.IsNullOrEmpty( SelfVFXKey );

            if( !justSelf ) {
                CastVFX = action.VFX.ValueNullable?.VFX.ValueNullable?.Location.ExtractText();
                CastVFXExists = !string.IsNullOrEmpty( CastVFX );

                //startVfx = action.AnimationStart.Value?.VFX.Value?.Location;

                // split this off into its own item
                HitVFXKey = action.ActionTimelineHit.ValueNullable?.Key.ToString();
                HitVFXExists = !string.IsNullOrEmpty( HitVFXKey );
                if( HitVFXExists ) {
                    var sAction = new Lumina.Excel.Sheets.Action {
                        Icon = action.Icon,
                        Name = new Lumina.Text.SeString( Encoding.UTF8.GetBytes( Name + " / Target" ) ),
                        IsPlayerAction = action.IsPlayerAction,
                        RowId = action.RowId,
                        AnimationEnd = action.ActionTimelineHit
                    };
                    HitAction = new XivAction( sAction, justSelf: true );
                }
            }

            VfxExists = ( CastVFXExists || SelfVFXExists );
        }
    }
}

using FFXIVClientStructs.FFXIV.Client.System.String;
using System;
using System.Numerics;

namespace EasyEyes.Structs.Vfx {
    public unsafe class StaticVfx : BaseVfx {

        public StaticVfx( Plugin plugin, string path, Vector3 position ) : base( plugin, path ) {
            Vfx = Plugin.ResourceLoader.StaticVfxCreate(
                ( new Utf8String( path ) ).StringPtr,
                ( new Utf8String( "Client.System.Scheduler.Instance.VfxObject" ) ).StringPtr
            );
            Plugin.ResourceLoader.StaticVfxRun( Vfx, 0.0f, 0xFFFFFFFF );

            UpdatePosition( position );
            Update();
        }

        public override void Remove() {
            Plugin.ResourceLoader.StaticVfxRemove( Vfx );
        }
    }
}
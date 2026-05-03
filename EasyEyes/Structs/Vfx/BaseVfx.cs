using Dalamud.Game.ClientState.Objects.Types;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;

namespace EasyEyes.Structs.Vfx {
    public abstract unsafe class BaseVfx {
        public Plugin Plugin;
        public VfxObject* Vfx;
        public string Path;

        public BaseVfx( Plugin plugin, string path ) {
            Plugin = plugin;
            Path = path;
        }

        public abstract void Remove();

        public void UpdatePosition( Vector3 position ) {
            if( Vfx == null ) return;
            Vfx->Position = new Vector3 {
                X = position.X,
                Y = position.Y,
                Z = position.Z
            };
        }

        public void UpdatePosition( IGameObject actor ) {
            if( Vfx == null ) return;
            Vfx->Position = actor.Position;
        }

        public void UpdateScale( Vector3 scale ) {
            if( Vfx == null ) return;
            Vfx->Scale = new Vector3 {
                X = scale.X,
                Y = scale.Y,
                Z = scale.Z
            };
        }

        protected void Update() {
            if( Vfx == null ) return;
            Vfx->UpdateTransforms( true );
        }
    }
}
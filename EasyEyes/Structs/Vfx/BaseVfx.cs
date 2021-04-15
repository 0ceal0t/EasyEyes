using Dalamud.Game.ClientState.Actors.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EasyEyes.Structs.Vfx {
    public abstract class BaseVfx {
        public Plugin _Plugin;
        public IntPtr Vfx;
        public string Path;

        public BaseVfx( Plugin plugin, string path) {
            _Plugin = plugin;
            Path = path;
        }

        public abstract void Remove();

        public void Update() {
            if( Vfx == IntPtr.Zero ) return;
            var flagAddr = IntPtr.Add( Vfx, 0x38 );
            byte currentFlag = Marshal.ReadByte( flagAddr );
            currentFlag |= 0x2;
            Marshal.WriteByte( flagAddr, currentFlag );
        }

        public void UpdatePosition( Vector3 position ) {
            if( Vfx == IntPtr.Zero ) return;
            IntPtr addr = IntPtr.Add( Vfx, 0x50 );
            var x = BitConverter.GetBytes( position.X );
            var y = BitConverter.GetBytes( position.Y );
            var z = BitConverter.GetBytes( position.Z );
            Marshal.Copy( x, 0, addr, 4 );
            Marshal.Copy( z, 0, addr + 0x4, 4 );
            Marshal.Copy( y, 0, addr + 0x8, 4 );
        }

        public void UpdatePosition( Actor actor ) {
            UpdatePosition( new Vector3( actor.Position.X, actor.Position.Y, actor.Position.Z ) );
        }
    }
}
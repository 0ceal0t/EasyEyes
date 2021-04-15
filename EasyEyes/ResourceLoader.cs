using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Dalamud.Plugin;
using Reloaded.Hooks;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X64;
using System.Threading.Tasks;
using System.Threading;

namespace EasyEyes {
    public class ResourceLoader : IDisposable
    {
        public Plugin _plugin { get; set; }
        public bool IsEnabled { get; set; }

        //====== STATIC ===========
        [UnmanagedFunctionPointer( CallingConvention.Cdecl, CharSet = CharSet.Ansi )]
        public delegate IntPtr VfxCreateDelegate( string path, string pool );
        public VfxCreateDelegate VfxCreate;
        [UnmanagedFunctionPointer( CallingConvention.Cdecl, CharSet = CharSet.Ansi )]
        public delegate IntPtr VfxRunDelegate( IntPtr vfx, float a1, uint a2 );
        public VfxRunDelegate VfxRun;
        [UnmanagedFunctionPointer( CallingConvention.Cdecl, CharSet = CharSet.Ansi )]
        public delegate IntPtr VfxRemoveDelegate( IntPtr vfx );
        public VfxRemoveDelegate VfxRemove;
        // ======= STATIC HOOKS ========
        [Function( CallingConventions.Microsoft )]
        public unsafe delegate IntPtr VfxCreateHook( char* path, char* pool );
        public IHook<VfxCreateHook> StaticVfxNewHook { get; private set; }
        [Function( CallingConventions.Microsoft )]
        public unsafe delegate IntPtr VfxRemoveHook( IntPtr vfx );
        public IHook<VfxRemoveHook> StaticVfxRemoveHook { get; private set; }

        // ======== ACTOR =============
        [UnmanagedFunctionPointer( CallingConvention.Cdecl, CharSet = CharSet.Ansi )]
        public delegate IntPtr StatusAddDelegate( string a1, IntPtr a2, IntPtr a3, float a4, char a5, UInt16 a6, char a7 );
        public StatusAddDelegate StatusAdd;
        [UnmanagedFunctionPointer( CallingConvention.Cdecl, CharSet = CharSet.Ansi )]
        public delegate IntPtr StatusRemoveDelegate( IntPtr vfx, char a2 );
        public StatusRemoveDelegate StatusRemove;
        // ======== ACTOR HOOKS =============
        [Function( CallingConventions.Microsoft )]
        public unsafe delegate IntPtr VfxStatusAddHook( char* a1, IntPtr a2, IntPtr a3, float a4, char a5, UInt16 a6, char a7 );
        public IHook<VfxStatusAddHook> ActorVfxNewHook { get; private set; }
        [Function( CallingConventions.Microsoft )]
        public unsafe delegate IntPtr VfxStatusRemoveHook( IntPtr vfx, char a2 );
        public IHook<VfxStatusRemoveHook> ActorVfxRemoveHook { get; private set; }

#if !DEBUG
        public bool EnableHooks = true;
#else
        public bool EnableHooks = false;
#endif


        public ResourceLoader( Plugin plugin ) {
            _plugin = plugin;
        }

        public unsafe void Init()
        {
            var scanner = _plugin.PluginInterface.TargetModuleScanner;

            // https://github.com/0ceal0t/Dalamud-VFXEditor/blob/main/VFXEditor/ResourceLoader.cs
            var vfxCreateAddress = scanner.ScanText( "E8 ?? ?? ?? ?? F3 0F 10 35 ?? ?? ?? ?? 48 89 43 08" );
            var vfxRunAddress = scanner.ScanText( "E8 ?? ?? ?? ?? 0F 28 B4 24 ?? ?? ?? ?? 48 8B 8C 24 ?? ?? ?? ?? 48 33 CC E8 ?? ?? ?? ?? 48 8B 9C 24 ?? ?? ?? ?? 48 81 C4 ?? ?? ?? ?? 5F" );
            var vfxRemoveAddress = scanner.ScanText( "40 53 48 83 EC 20 48 8B D9 48 8B 89 ?? ?? ?? ?? 48 85 C9 74 28 33 D2 E8 ?? ?? ?? ?? 48 8B 8B ?? ?? ?? ?? 48 85 C9" );
            var statusAddAddr = scanner.ScanText( "40 53 55 56 57 48 81 EC ?? ?? ?? ?? 0F 29 B4 24 ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 0F B6 AC 24 ?? ?? ?? ?? 0F 28 F3 49 8B F8" );
            var statusRemoveAddr2 = scanner.ScanText( "0F 11 48 10 48 8D 05" ) + 7;
            var statusRemove2 = Marshal.ReadIntPtr( statusRemoveAddr2 + Marshal.ReadInt32( statusRemoveAddr2 ) + 4 );
            StatusAdd = Marshal.GetDelegateForFunctionPointer<StatusAddDelegate>( statusAddAddr );
            StatusRemove = Marshal.GetDelegateForFunctionPointer<StatusRemoveDelegate>( statusRemove2 );
            VfxRemove = Marshal.GetDelegateForFunctionPointer<VfxRemoveDelegate>( vfxRemoveAddress );
            VfxRun = Marshal.GetDelegateForFunctionPointer<VfxRunDelegate>( vfxRunAddress );
            VfxCreate = Marshal.GetDelegateForFunctionPointer<VfxCreateDelegate>( vfxCreateAddress );
            if( EnableHooks ) {
                StaticVfxNewHook = new Hook<VfxCreateHook>( StaticVfxNewHandler, ( long )vfxCreateAddress );
                StaticVfxRemoveHook = new Hook<VfxRemoveHook>( StaticVfxRemoveHandler, ( long )vfxRemoveAddress );
                ActorVfxNewHook = new Hook<VfxStatusAddHook>( ActorVfxNewHandler, ( long )statusAddAddr );
                ActorVfxRemoveHook = new Hook<VfxStatusRemoveHook>( ActorVfxRemoveHandler, ( long )statusRemove2 );
            }
        }

        private unsafe IntPtr StaticVfxNewHandler( char* path, char* pool ) {
            if( Process( path ) ) {
                var p = Encoding.ASCII.GetBytes( "vfx/common/eff/foot_001.avfx" );
                var bPath = stackalloc byte[p.Length + 1];
                Marshal.Copy( p, 0, new IntPtr( bPath ), p.Length );
                return StaticVfxNewHook.OriginalFunction( (char*) bPath, pool );
            }
            return StaticVfxNewHook.OriginalFunction( path, pool );
        }
        private unsafe IntPtr StaticVfxRemoveHandler( IntPtr vfx ) {
            if( _plugin.MainUI?.SpawnVfx != null && vfx == _plugin.MainUI.SpawnVfx.Vfx ) {
                _plugin.MainUI.SpawnVfx = null;
            }
            return StaticVfxRemoveHook.OriginalFunction( vfx );
        }

        private unsafe IntPtr ActorVfxNewHandler( char* a1, IntPtr a2, IntPtr a3, float a4, char a5, UInt16 a6, char a7 ) {
            if( Process( a1 ) ) {
                var p = Encoding.ASCII.GetBytes( "vfx/common/eff/cmma_shoot1c.avfx" );
                var bPath = stackalloc byte[p.Length + 1];
                Marshal.Copy( p, 0, new IntPtr( bPath ), p.Length );
                return ActorVfxNewHook.OriginalFunction( ( char* )bPath, a2, a3, a4, a5, a6, a7 );
            }
            return ActorVfxNewHook.OriginalFunction( a1, a2, a3, a4, a5, a6, a7 );
        }
        private unsafe IntPtr ActorVfxRemoveHandler( IntPtr vfx, char a2 ) {
            if( _plugin.MainUI?.SpawnVfx != null && vfx == _plugin.MainUI.SpawnVfx.Vfx ) {
                _plugin.MainUI.SpawnVfx = null;
            }
            return ActorVfxRemoveHook.OriginalFunction( vfx, a2 );
        }

        private unsafe bool Process(char* path ) {

            var gameFsPath = Marshal.PtrToStringAnsi( new IntPtr( path ) );
            if( _plugin.Configuration.IsDisabled( gameFsPath ) ) {
                return true;
            }
            _plugin.Record( gameFsPath, false );
            return false;
        }

        public void Enable() {
            if( IsEnabled ) return;
            if( EnableHooks ) {
                StaticVfxNewHook.Activate();
                StaticVfxRemoveHook.Activate();
                ActorVfxNewHook.Activate();
                ActorVfxRemoveHook.Activate();

                StaticVfxNewHook.Enable();
                StaticVfxRemoveHook.Enable();
                ActorVfxNewHook.Enable();
                ActorVfxRemoveHook.Enable();
            }
            IsEnabled = true;
        }

        public void Disable() {
            if( !IsEnabled ) return;
            if( EnableHooks ) {
                StaticVfxNewHook.Disable();
                StaticVfxRemoveHook.Disable();
                ActorVfxNewHook.Disable();
                ActorVfxRemoveHook.Disable();
            }
            IsEnabled = false;
        }

        public void Dispose() {
            if( IsEnabled )
                Disable();
        }
    }
}
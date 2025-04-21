using Dalamud.Hooking;
using EasyEyes.Util;
using Penumbra.String.Classes;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace EasyEyes {
    public unsafe class ResourceLoader {
        private Plugin Plugin { get; set; }
        private bool IsEnabled { get; set; }
        private Crc32 Crc32 { get; }

        public delegate void* GetResourceSyncPrototype( IntPtr resourceManager, uint* categoryId, char* resourceType,
            uint* resourceHash, byte* path, void* resParams );

        public delegate void* GetResourceAsyncPrototype( IntPtr resourceManager, uint* categoryId, char* resourceType,
            uint* resourceHash, byte* path, void* resParams, bool isUnknown );

        // ====== FILES HOOKS ========

        public Hook<GetResourceSyncPrototype> GetResourceSyncHook { get; private set; }

        public Hook<GetResourceAsyncPrototype> GetResourceAsyncHook { get; private set; }

        //====== STATIC ===========
        public delegate IntPtr StaticVfxCreateDelegate( string path, string pool );

        public StaticVfxCreateDelegate StaticVfxCreate;

        public delegate IntPtr StaticVfxRunDelegate( IntPtr vfx, float a1, uint a2 );

        public StaticVfxRunDelegate StaticVfxRun;

        public delegate IntPtr StaticVfxRemoveDelegate( IntPtr vfx );

        public StaticVfxRemoveDelegate StaticVfxRemove;

        // ======= STATIC HOOKS ========
        public Hook<StaticVfxCreateDelegate> StaticVfxCreateHook { get; private set; }

        public Hook<StaticVfxRemoveDelegate> StaticVfxRemoveHook { get; private set; }

        // ======== ACTOR =============
        public delegate IntPtr ActorVfxCreateDelegate( string path, IntPtr a2, IntPtr a3, float a4, char a5, ushort a6, char a7 );

        public ActorVfxCreateDelegate ActorVfxCreate;

        public delegate IntPtr ActorVfxRemoveDelegate( IntPtr vfx, char a2 );

        public ActorVfxRemoveDelegate ActorVfxRemove;

        // ======== ACTOR HOOKS =============
        public Hook<ActorVfxCreateDelegate> ActorVfxCreateHook { get; private set; }

        public Hook<ActorVfxRemoveDelegate> ActorVfxRemoveHook { get; private set; }

        public ResourceLoader( Plugin plugin ) {
            Plugin = plugin;
            Crc32 = new Crc32();
        }

        public unsafe void Init() {
            var scanner = Services.SigScanner;

            var getResourceSyncAddress = scanner.ScanText( "E8 ?? ?? ?? ?? 48 8B D8 8B C7" );
            var getResourceAsyncAddress = scanner.ScanText( "E8 ?? ?? ?? 00 48 8B D8 EB ?? F0 FF 83 ?? ?? 00 00" );

            GetResourceSyncHook = Services.Hooks.HookFromAddress<GetResourceSyncPrototype>( getResourceSyncAddress, GetResourceSyncHandler );
            GetResourceAsyncHook = Services.Hooks.HookFromAddress<GetResourceAsyncPrototype>( getResourceAsyncAddress, GetResourceAsyncHandler );

            var staticVfxCreateAddress = scanner.ScanText( "E8 ?? ?? ?? ?? F3 0F 10 35 ?? ?? ?? ?? 48 89 43 08" );
            var staticVfxRunAddress = scanner.ScanText( "E8 ?? ?? ?? ?? 8B 4B 7C 85 C9" );
            var staticVfxRemoveAddress = scanner.ScanText( "40 53 48 83 EC 20 48 8B D9 48 8B 89 ?? ?? ?? ?? 48 85 C9 74 28 33 D2 E8 ?? ?? ?? ?? 48 8B 8B ?? ?? ?? ?? 48 85 C9" );

            var actorVfxCreateAddress = scanner.ScanText( "40 53 55 56 57 48 81 EC ?? ?? ?? ?? 0F 29 B4 24 ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 0F B6 AC 24 ?? ?? ?? ?? 0F 28 F3 49 8B F8" );
            var actorVfxRemoveAddress_1 = scanner.ScanText( "0F 11 48 10 48 8D 05" ) + 7;
            var actorVfxRemoveAddress = Marshal.ReadIntPtr( actorVfxRemoveAddress_1 + Marshal.ReadInt32( actorVfxRemoveAddress_1 ) + 4 );

            ActorVfxCreate = Marshal.GetDelegateForFunctionPointer<ActorVfxCreateDelegate>( actorVfxCreateAddress );
            ActorVfxRemove = Marshal.GetDelegateForFunctionPointer<ActorVfxRemoveDelegate>( actorVfxRemoveAddress );

            StaticVfxRemove = Marshal.GetDelegateForFunctionPointer<StaticVfxRemoveDelegate>( staticVfxRemoveAddress );
            StaticVfxRun = Marshal.GetDelegateForFunctionPointer<StaticVfxRunDelegate>( staticVfxRunAddress );
            StaticVfxCreate = Marshal.GetDelegateForFunctionPointer<StaticVfxCreateDelegate>( staticVfxCreateAddress );

            StaticVfxCreateHook = Services.Hooks.HookFromAddress<StaticVfxCreateDelegate>( staticVfxCreateAddress, StaticVfxNewHandler );
            StaticVfxRemoveHook = Services.Hooks.HookFromAddress<StaticVfxRemoveDelegate>( staticVfxRemoveAddress, StaticVfxRemoveHandler );

            ActorVfxCreateHook = Services.Hooks.HookFromAddress<ActorVfxCreateDelegate>( actorVfxCreateAddress, ActorVfxNewHandler );
            ActorVfxRemoveHook = Services.Hooks.HookFromAddress<ActorVfxRemoveDelegate>( actorVfxRemoveAddress, ActorVfxRemoveHandler );

        }

        private unsafe IntPtr StaticVfxNewHandler( string path, string pool ) {
            var vfx = StaticVfxCreateHook.Original( path, pool );
            Plugin.AddRecord( path );
            return vfx;
        }

        private unsafe IntPtr StaticVfxRemoveHandler( IntPtr vfx ) {
            if( Plugin.SpawnVfx != null && vfx == ( IntPtr )Plugin.SpawnVfx.Vfx ) {
                Plugin.ClearSpawnVfx();
            }
            return StaticVfxRemoveHook.Original( vfx );
        }

        private unsafe IntPtr ActorVfxNewHandler( string a1, IntPtr a2, IntPtr a3, float a4, char a5, ushort a6, char a7 ) {
            var vfx = ActorVfxCreateHook.Original( a1, a2, a3, a4, a5, a6, a7 );
            Plugin.AddRecord( a1 );
            return vfx;
        }

        private unsafe IntPtr ActorVfxRemoveHandler( IntPtr vfx, char a2 ) {
            if( Plugin.SpawnVfx != null && vfx == ( IntPtr )Plugin.SpawnVfx.Vfx ) {
                Plugin.ClearSpawnVfx();
            }
            return ActorVfxRemoveHook.Original( vfx, a2 );
        }

        public void Enable() {
            if( IsEnabled ) return;
            GetResourceSyncHook.Enable();
            GetResourceAsyncHook.Enable();

            StaticVfxCreateHook.Enable();
            StaticVfxRemoveHook.Enable();
            ActorVfxCreateHook.Enable();
            ActorVfxRemoveHook.Enable();

            IsEnabled = true;
        }

        public void Dispose() {
            if( IsEnabled ) Disable();
        }

        public void Disable() {
            if( !IsEnabled ) return;
            GetResourceSyncHook.Disable();
            GetResourceAsyncHook.Disable();
            StaticVfxCreateHook.Disable();
            StaticVfxRemoveHook.Disable();
            ActorVfxCreateHook.Disable();
            ActorVfxRemoveHook.Disable();

            Thread.Sleep( 500 );

            GetResourceSyncHook.Dispose();
            GetResourceAsyncHook.Dispose();
            StaticVfxCreateHook.Dispose();
            StaticVfxRemoveHook.Dispose();
            ActorVfxCreateHook.Dispose();
            ActorVfxRemoveHook.Dispose();

            GetResourceSyncHook = null;
            GetResourceAsyncHook = null;
            StaticVfxCreateHook = null;
            StaticVfxRemoveHook = null;
            ActorVfxCreateHook = null;
            ActorVfxRemoveHook = null;

            IsEnabled = false;
            Plugin = null;
        }

        private unsafe void* GetResourceSyncHandler(
            IntPtr pFileManager,
            uint* pCategoryId,
            char* pResourceType,
            uint* pResourceHash,
            byte* pPath,
            void* pUnknown
        ) => GetResourceHandler( true, pFileManager, pCategoryId, pResourceType, pResourceHash, pPath, pUnknown, false );

        private unsafe void* GetResourceAsyncHandler(
            IntPtr pFileManager,
            uint* pCategoryId,
            char* pResourceType,
            uint* pResourceHash,
            byte* pPath,
            void* pUnknown,
            bool isUnknown
        ) => GetResourceHandler( false, pFileManager, pCategoryId, pResourceType, pResourceHash, pPath, pUnknown, isUnknown );

        private unsafe void* CallOriginalHandler(
            bool isSync,
            IntPtr pFileManager,
            uint* pCategoryId,
            char* pResourceType,
            uint* pResourceHash,
            byte* pPath,
            void* pUnknown,
            bool isUnknown
        ) => isSync
            ? GetResourceSyncHook.Original( pFileManager, pCategoryId, pResourceType, pResourceHash, pPath, pUnknown )
            : GetResourceAsyncHook.Original( pFileManager, pCategoryId, pResourceType, pResourceHash, pPath, pUnknown, isUnknown );

        private unsafe void* GetResourceHandler(
            bool isSync, IntPtr pFileManager,
            uint* pCategoryId,
            char* pResourceType,
            uint* pResourceHash,
            byte* pPath,
            void* pUnknown,
            bool isUnknown
        ) {
            if( !Utf8GamePath.FromPointer( pPath, out var gamePath ) ) {
                return CallOriginalHandler( isSync, pFileManager, pCategoryId, pResourceType, pResourceHash, pPath, pUnknown, isUnknown );
            }

            var gamePathString = gamePath.ToString();

            if( Plugin?.Config == null || !Plugin.Config.IsDisabled( gamePathString ) ) {
                return CallOriginalHandler( isSync, pFileManager, pCategoryId, pResourceType, pResourceHash, pPath, pUnknown, isUnknown );
            }

            var path = Encoding.ASCII.GetBytes( "vfx/path/nothing.avfx" );
            var bPath = stackalloc byte[path.Length + 1];
            Marshal.Copy( path, 0, new IntPtr( bPath ), path.Length );
            pPath = bPath;
            Crc32.Init();
            Crc32.Update( path );
            *pResourceHash = Crc32.Checksum;
            return CallOriginalHandler( isSync, pFileManager, pCategoryId, pResourceType, pResourceHash, pPath, pUnknown, isUnknown );
        }
    }
}
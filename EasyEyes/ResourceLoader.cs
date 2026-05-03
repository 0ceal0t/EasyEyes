using Dalamud.Hooking;
using EasyEyes.Util;
using Penumbra.String;
using Penumbra.String.Classes;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using FFXIVClientStructs.FFXIV.Client.System.Resource;
using InteropGenerator.Runtime;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using Dalamud.Utility;

namespace EasyEyes {
    public unsafe class ResourceLoader {
        private Plugin Plugin { get; set; }
        private bool IsEnabled { get; set; }
        private Crc32 Crc32 { get; }

        public delegate void* GetResourceSyncPrototype( ResourceManager* resourceManager, ResourceCategory* category, uint* type, uint* hash, CStringPointer path,
            void* unknown, void* unkDebugPtr, uint unkDebugInt );

        public delegate void* GetResourceAsyncPrototype( ResourceManager* resourceManager, ResourceCategory* category, uint* type, uint* hash, CStringPointer path,
            void* unknown, bool isUnknown, void* unkDebugPtr, uint unkDebugInt );

        // ====== FILES HOOKS ========

        public Hook<GetResourceSyncPrototype> GetResourceSyncHook { get; private set; }

        public Hook<GetResourceAsyncPrototype> GetResourceAsyncHook { get; private set; }

        //====== STATIC ===========

        public VfxObject.Delegates.Create StaticVfxCreate;

        public delegate IntPtr StaticVfxRunDelegate( VfxObject* vfx, float a1, uint a2 );

        public StaticVfxRunDelegate StaticVfxRun;

        public delegate IntPtr StaticVfxRemoveDelegate( VfxObject* vfx );

        public StaticVfxRemoveDelegate StaticVfxRemove;

        // ======= STATIC HOOKS ========
        public Hook<VfxObject.Delegates.Create> StaticVfxCreateHook { get; private set; }

        public Hook<StaticVfxRemoveDelegate> StaticVfxRemoveHook { get; private set; }

        // ======== ACTOR =============
        public delegate VfxObject* ActorVfxCreateDelegate( string path, IntPtr a2, IntPtr a3, float a4, char a5, ushort a6, char a7 );

        public ActorVfxCreateDelegate ActorVfxCreate;

        public delegate IntPtr ActorVfxRemoveDelegate( VfxObject* vfx, char a2 );

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

            var getResourceSyncAddress = scanner.ScanText( "E8 ?? ?? ?? ?? 48 8B C8 8B C3 F0 0F C0 81" );
            var getResourceAsyncAddress = scanner.ScanText( "E8 ?? ?? ?? 00 48 8B D8 EB ?? F0 FF 83 ?? ?? 00 00" );

            GetResourceSyncHook = Services.Hooks.HookFromAddress<GetResourceSyncPrototype>( getResourceSyncAddress, GetResourceSyncHandler );
            GetResourceAsyncHook = Services.Hooks.HookFromAddress<GetResourceAsyncPrototype>( getResourceAsyncAddress, GetResourceAsyncHandler );

            var staticVfxCreateAddress = scanner.ScanText( VfxObject.Addresses.Create.String );
            var staticVfxRunAddress = scanner.ScanText( "E8 ?? ?? ?? ?? B0 02 EB 02" );
            var staticVfxRemoveAddress = scanner.ScanText( "40 53 48 83 EC 20 48 8B D9 48 8B 89 ?? ?? ?? ?? 48 85 C9 74 28 33 D2 E8 ?? ?? ?? ?? 48 8B 8B ?? ?? ?? ?? 48 85 C9" );

            var actorVfxCreateAddress = scanner.ScanText( "40 53 55 56 57 48 81 EC ?? ?? ?? ?? 0F 29 B4 24 ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 0F B6 AC 24 ?? ?? ?? ?? 0F 28 F3 49 8B F8" );
            var actorVfxRemoveAddress_1 = scanner.ScanText( "0F 11 48 10 48 8D 05" ) + 7;
            var actorVfxRemoveAddress = Marshal.ReadIntPtr( actorVfxRemoveAddress_1 + Marshal.ReadInt32( actorVfxRemoveAddress_1 ) + 4 );

            ActorVfxCreate = Marshal.GetDelegateForFunctionPointer<ActorVfxCreateDelegate>( actorVfxCreateAddress );
            ActorVfxRemove = Marshal.GetDelegateForFunctionPointer<ActorVfxRemoveDelegate>( actorVfxRemoveAddress );

            StaticVfxRemove = Marshal.GetDelegateForFunctionPointer<StaticVfxRemoveDelegate>( staticVfxRemoveAddress );
            StaticVfxRun = Marshal.GetDelegateForFunctionPointer<StaticVfxRunDelegate>( staticVfxRunAddress );
            StaticVfxCreate = Marshal.GetDelegateForFunctionPointer<VfxObject.Delegates.Create>( staticVfxCreateAddress );

            StaticVfxCreateHook = Services.Hooks.HookFromAddress<VfxObject.Delegates.Create>( staticVfxCreateAddress, StaticVfxNewHandler );
            StaticVfxRemoveHook = Services.Hooks.HookFromAddress<StaticVfxRemoveDelegate>( staticVfxRemoveAddress, StaticVfxRemoveHandler );

            ActorVfxCreateHook = Services.Hooks.HookFromAddress<ActorVfxCreateDelegate>( actorVfxCreateAddress, ActorVfxNewHandler );
            ActorVfxRemoveHook = Services.Hooks.HookFromAddress<ActorVfxRemoveDelegate>( actorVfxRemoveAddress, ActorVfxRemoveHandler );

        }

        private unsafe VfxObject* StaticVfxNewHandler( CStringPointer path, CStringPointer pool ) {
            var vfx = StaticVfxCreateHook.Original( path, pool );
            Plugin.AddRecord( path.ExtractText() );
            return vfx;
        }

        private unsafe IntPtr StaticVfxRemoveHandler( VfxObject* vfx ) {
            if( Plugin.SpawnVfx != null && (nint)vfx == ( nint )Plugin.SpawnVfx.Vfx ) {
                Plugin.ClearSpawnVfx();
            }
            return StaticVfxRemoveHook.Original( vfx );
        }

        private unsafe VfxObject* ActorVfxNewHandler( string a1, IntPtr a2, IntPtr a3, float a4, char a5, ushort a6, char a7 ) {
            var vfx = ActorVfxCreateHook.Original( a1, a2, a3, a4, a5, a6, a7 );
            Plugin.AddRecord( a1 );
            return vfx;
        }

        private unsafe IntPtr ActorVfxRemoveHandler( VfxObject* vfx, char a2 ) {
            if( Plugin.SpawnVfx != null && (nint) vfx == ( nint )Plugin.SpawnVfx.Vfx ) {
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
            ResourceManager* resourceManager, ResourceCategory* category, uint* type, uint* hash, CStringPointer path,
            void* unknown, void* unkDebugPtr, uint unkDebugInt
        ) => GetResourceHandler( true, resourceManager, category, type, hash, path, unknown, false, unkDebugPtr, unkDebugInt );

        private unsafe void* GetResourceAsyncHandler(
             ResourceManager* resourceManager, ResourceCategory* category, uint* type, uint* hash, CStringPointer path,
            void* unknown, bool isUnknown, void* unkDebugPtr, uint unkDebugInt
        ) => GetResourceHandler( false, resourceManager, category, type, hash, path, unknown, isUnknown, unkDebugPtr, unkDebugInt );

        private unsafe void* CallOriginalHandler(
            bool isSync,
            ResourceManager* resourceManager, ResourceCategory* category, uint* type, uint* hash, CStringPointer path,
            void* unknown, bool isUnknown, void* unkDebugPtr, uint unkDebugInt
        ) => isSync
            ? GetResourceSyncHook.Original( resourceManager, category, type, hash, path, unknown, unkDebugPtr, unkDebugInt )
            : GetResourceAsyncHook.Original( resourceManager, category, type, hash, path, unknown, isUnknown, unkDebugPtr, unkDebugInt );

        private unsafe void* GetResourceHandler(
            bool isSync,
            ResourceManager* resourceManager, ResourceCategory* category, uint* type, uint* hash, CStringPointer pPath,
            void* unknown, bool isUnknown, void* unkDebugPtr, uint unkDebugInt
        ) {
            if( !Utf8GamePath.FromPointer( pPath, MetaDataComputation.None, out var gamePath ) ) {
                return CallOriginalHandler( isSync, resourceManager, category, type, hash, pPath, unknown, isUnknown, unkDebugPtr, unkDebugInt );
            }

            var gamePathString = gamePath.ToString();

            if( Plugin?.Config == null || !Plugin.Config.IsDisabled( gamePathString ) ) {
                return CallOriginalHandler( isSync, resourceManager, category, type, hash, pPath, unknown, isUnknown, unkDebugPtr, unkDebugInt );
            }

            var path = Encoding.ASCII.GetBytes( "vfx/path/nothing.avfx" );
            var bPath = stackalloc byte[path.Length + 1];
            Marshal.Copy( path, 0, new IntPtr( bPath ), path.Length );
            pPath = bPath;
            Crc32.Init();
            Crc32.Update( path );
            *hash = Crc32.Checksum;
            return CallOriginalHandler( isSync, resourceManager, category, type, hash, pPath, unknown, isUnknown, unkDebugPtr, unkDebugInt );
        }
    }
}
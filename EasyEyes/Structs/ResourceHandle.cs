using System.Runtime.InteropServices;

namespace EasyEyes.Structs
{
    [StructLayout( LayoutKind.Explicit )]
    public unsafe struct ResourceHandle
    {
        [FieldOffset( 0x48 )]
        public byte* FileName;
    }
}
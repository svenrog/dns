using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace DNS.Protocol.Marshalling
{
    public static class Struct
    {
        public static T PinStruct<[DynamicallyAccessedMembers(
           DynamicallyAccessedMemberTypes.PublicConstructors |
           DynamicallyAccessedMemberTypes.NonPublicConstructors
       )] T>(byte[] data) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);

            try
            {
                return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
        }
    }
}

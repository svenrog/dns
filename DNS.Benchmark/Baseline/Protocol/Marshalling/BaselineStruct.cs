using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace DNS.Benchmark.Baseline.Protocol.Marshalling
{
    public static class BaselineStruct
    {
        public static T GetStruct<[DynamicallyAccessedMembers(
           DynamicallyAccessedMemberTypes.PublicConstructors |
           DynamicallyAccessedMemberTypes.NonPublicConstructors
       )] T>(byte[] data, int offset, int length) where T : struct
        {
            byte[] buffer = new byte[length];
            Array.Copy(data, offset, buffer, 0, buffer.Length);
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

namespace GIGXR.Platform.Utilities
{
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;

    /// <summary>
    /// Helper class that holds extension methods related only to the Color32Class.
    /// </summary>
    public static class Color32Extensions
    {
        /// <summary>
        /// Converts an array of Color32 to its byte array representation.
        /// </summary>
        /// <param name="colors"></param>
        /// <returns></returns>
        // https://stackoverflow.com/questions/21512259/fast-copy-of-color32-array-to-byte-array
        public static byte[] ToByteArray(this Color32[] colors)
        {
            if (colors == null || colors.Length == 0)
                return null;

            int lengthOfColor32 = Marshal.SizeOf(typeof(Color32));
            int length = lengthOfColor32 * colors.Length;
            byte[] bytes = new byte[length];

            GCHandle handle = default(GCHandle);
            try
            {
                handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
                IntPtr ptr = handle.AddrOfPinnedObject();
                Marshal.Copy(ptr, bytes, 0, length);
            }
            finally
            {
                if (handle != default(GCHandle))
                    handle.Free();
            }

            return bytes;
        }
    }
}
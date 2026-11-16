using System.Runtime.InteropServices;

namespace RmlUiNet.Native {
    internal static class Box {
        // The values are written into caller-provided floats. An out parameter replaced the
        // previous pointer-returning form, which allocated a struct per call on the native
        // side that the managed side never freed.

        /// <summary>Reads a box-area position, relative to the border box.</summary>
        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Box_GetPosition")]
        public static extern void GetPosition(System.IntPtr box, int area, out float x, out float y);

        /// <summary>Reads the content-area size.</summary>
        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Box_GetSize")]
        public static extern void GetSize(System.IntPtr box, out float x, out float y);

        /// <summary>Reads the size of one of the box's areas.</summary>
        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Box_GetSizeBoxArea")]
        public static extern void GetSize(System.IntPtr box, int area, out float x, out float y);
    }
}

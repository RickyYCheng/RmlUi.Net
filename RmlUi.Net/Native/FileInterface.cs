using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RmlUiNet.Native
{
    internal static class FileInterface
    {
        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_FileInterface_Free")]
        public static extern void Free(IntPtr instance);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_FileInterface_New")]
        public static extern IntPtr Create(
            OnOpen onOpen,
            OnClose onClose,
            OnLoadFile onLoadFile,
            OnRead onRead,
            OnSeek onSeek,
            OnTell onTell,
            OnLength onLength
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr OnOpen([MarshalAs(UnmanagedType.LPUTF8Str)] string path);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void OnClose(IntPtr file);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate nuint OnRead(byte* buffer, nuint size, IntPtr file);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal delegate bool OnSeek(IntPtr file, long offset, int origin);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate nuint OnTell(IntPtr file);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate nuint OnLength(IntPtr file);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal delegate bool OnLoadFile(IntPtr output, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    }
}

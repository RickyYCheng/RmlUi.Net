using System;
using System.Runtime.InteropServices;
using System.Text;

namespace RmlUiNet.Native
{
    internal static class SystemInterface
    {
        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_SystemInterface_Free")]
        public static extern void Free(IntPtr instance);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_SystemInterface_New")]
        public static extern IntPtr Create(OnGetElapsedTime onGetElapsedTime, OnTranslateString onTranslateString, OnLogMessage onLogMessage, OnJoinPath onJoinPath, OnSetMouseCursor onSetMouseCursor, OnSetClipboardText onSetClipboardText, OnGetClipboardText onGetClipboardText, OnActivateKeyboard onActivateKeyboard, OnDeactivateKeyboard onDeactivateKeyboard);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate double OnGetElapsedTime();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int OnTranslateString(IntPtr output, [MarshalAs(UnmanagedType.LPUTF8Str)] string input);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal delegate bool OnLogMessage(LogType type, [MarshalAs(UnmanagedType.LPUTF8Str)] string message);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void OnJoinPath(IntPtr output, [MarshalAs(UnmanagedType.LPUTF8Str)] string path, [MarshalAs(UnmanagedType.LPUTF8Str)] string file);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void OnSetMouseCursor([MarshalAs(UnmanagedType.LPUTF8Str)] string message);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void OnSetClipboardText([MarshalAs(UnmanagedType.LPUTF8Str)] string text);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void OnGetClipboardText(IntPtr output);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void OnActivateKeyboard(float caret_x, float caret_y, float line_height);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void OnDeactivateKeyboard();
    }
}

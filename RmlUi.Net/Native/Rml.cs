using System;
using System.Runtime.InteropServices;

namespace RmlUiNet.Native
{
    internal static class Rml
    {
        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_BindingAbiVersion")]
        public static extern int BindingAbiVersion();

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_ObserveLifetime")]
        public static extern void ObserveLifetime(OnDestroyed contextDestroyed, OnDestroyed elementDestroyed);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void OnDestroyed(IntPtr pointer);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Initialise")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Initialise();

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Shutdown")]
        public static extern void Shutdown();

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_SetRenderInterface")]
        public static extern void SetRenderInterface(IntPtr renderInterface);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_SetSystemInterface")]
        public static extern void SetSystemInterface(IntPtr systemInterface);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_SetFileInterface")]
        public static extern void SetFileInterface(IntPtr fileInterface);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_LoadFontFace")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool LoadFontFace([MarshalAs(UnmanagedType.LPUTF8Str)] string fileName, [MarshalAs(UnmanagedType.I1)] bool fallbackFace, FontWeight weight);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_LoadFontFaceEx")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool LoadFontFaceEx([MarshalAs(UnmanagedType.LPUTF8Str)] string fileName, [MarshalAs(UnmanagedType.LPUTF8Str)] string family, FontStyle style, FontWeight weight, [MarshalAs(UnmanagedType.I1)] bool fallbackFace, int faceIndex);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_CreateEventListener")]
        public static extern IntPtr CreateEventListener(OnProcessEvent onProcessEvent, OnAttachEvent onAttachEvent, OnDetachEvent onDetachEvent);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_ReleaseEventListener")]
        public static extern void ReleaseEventListener(IntPtr eventListener);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_RemoveContext")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RemoveContext([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_RegisterPlugin")]
        public static extern void RegisterPlugin(IntPtr pluginPtr);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_GetRenderInterface")]
        public static extern IntPtr GetRenderInterface();

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_GetSystemInterface")]
        public static extern IntPtr GetSystemInterface();

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_CreateString")]
        public static extern IntPtr CreateString([MarshalAs(UnmanagedType.LPUTF8Str)] string str);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Log")]
        public static extern void Log(LogType type, [MarshalAs(UnmanagedType.LPUTF8Str)] string str);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_UpdateString")]
        public static extern void UpdateString(IntPtr str, [MarshalAs(UnmanagedType.LPUTF8Str)] string newContent);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_FreeString")]
        public static extern void FreeString(IntPtr str);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_String_Free")]
        public static extern void FreeStringBuffer(IntPtr str);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_RegisterEventType")]
        public static extern EventId RegisterEventType([MarshalAs(UnmanagedType.LPUTF8Str)] string type, [MarshalAs(UnmanagedType.I1)] bool interruptible, [MarshalAs(UnmanagedType.I1)] bool bubbles, DefaultActionPhase default_action_phase);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void OnProcessEvent(IntPtr eventPtr);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void OnAttachEvent(IntPtr elementPtr, [MarshalAs(UnmanagedType.LPUTF8Str)] string elementType);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void OnDetachEvent(IntPtr elementPtr, [MarshalAs(UnmanagedType.LPUTF8Str)] string elementType);
    }
}

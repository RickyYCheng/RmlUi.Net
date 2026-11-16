using System;

namespace RmlUiNet
{
    public static class Rml
    {
        public static bool Initialise()
        {
            return Native.Rml.Initialise();
        }

        public static void Shutdown()
        {
            RmlInstanceCache.Instance?.Clear();
            Native.Rml.Shutdown();
        }

        public static void SetSystemInterface(SystemInterface systemInterface)
        {
            Native.Rml.SetSystemInterface(systemInterface.NativePtr);
        }

        public static void SetRenderInterface(RenderInterface renderInterface)
        {
            Native.Rml.SetRenderInterface(renderInterface.NativePtr);
        }

        public static void SetFileInterface(FileInterface fileInterface)
        {
            Native.Rml.SetFileInterface(fileInterface.NativePtr);
        }

        /// <summary>
        /// Adds a new font face to the font engine. The face's family, style and weight will be determined from the face itself.
        /// </summary>
        /// <param name="fileName">The file to load the face from.</param>
        /// <param name="fallbackFace">True to use this font face for unknown characters in other font faces.</param>
        /// <param name="weight">The weight to load when the font face contains multiple weights, otherwise the weight to register the font as. By default it loads all found font weights.</param>
        /// <returns>True if the face was loaded successfully, false otherwise.</returns>
        public static bool LoadFontFace(string fileName, bool fallbackFace = false, FontWeight weight = FontWeight.Auto)
        {
            return Native.Rml.LoadFontFace(fileName, fallbackFace, weight);
        }

        /// <summary>
        /// Adds a new font face to the font engine and registers it under an explicit family,
        /// style and weight instead of the ones read from the face itself.
        /// </summary>
        /// <param name="fileName">The file to load the face from.</param>
        /// <param name="family">The family to register the font as.</param>
        /// <param name="style">The style to register the font as.</param>
        /// <param name="weight">The weight to load, or to register the face as. <see cref="FontWeight.Auto"/> loads all found weights.</param>
        /// <param name="fallbackFace">True to use this font face for unknown characters in other font faces.</param>
        /// <param name="faceIndex">The index of the font face within a font collection.</param>
        /// <returns>True if the face was loaded successfully, false otherwise.</returns>
        public static bool LoadFontFace(string fileName, string family, FontStyle style,
            FontWeight weight = FontWeight.Auto, bool fallbackFace = false, int faceIndex = 0)
        {
            return Native.Rml.LoadFontFaceEx(fileName, family, style, weight, fallbackFace, faceIndex);
        }

        public static EventId RegisterEventType(string type, bool interruptible, bool bubbles, DefaultActionPhase defaultActionPhase) {
            return Native.Rml.RegisterEventType(type, interruptible, bubbles, defaultActionPhase);
        }

        public static Context? CreateContext(string name, Vector2i dimensions, RenderInterface? renderInterface = null)
        {
            return Context.Create(name, dimensions, renderInterface);
        }

        public static bool RemoveContext(string name) {
            bool removed = Native.Rml.RemoveContext(name);
            if (removed) {
                // Every element and document in that context has just been destroyed, so the
                // wrappers cached for their addresses are dangling. Dropping them keeps a later
                // allocation that reuses an address from being mistaken for the old element.
                RmlInstanceCache.Instance.Clear();
            }
            return removed;
        }

        public static void RegisterPlugin(Plugin plugin) {
            Native.Rml.RegisterPlugin(plugin.NativePtr);
        }

        public static void Log(string message, LogType type = LogType.Always) {
            Native.Rml.Log(type, message);
        }

        public static RenderInterface GetRenderInterface() {
            return RenderInterface.Create(Native.Rml.GetRenderInterface()) ?? throw new Exception("Failed to create render interface wrapper");
        }
    }
}

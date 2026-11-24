using System;
using System.Collections.Generic;
using System.Text;

namespace RmlUiNet
{
    public static class Debugger
    {
        public static void Initialise(Context context)
        {
            Native.Debugger.Initialise(context.NativePtr);
        }

        public static void SetContext(Context context)
        {
            Native.Debugger.SetContext(context.NativePtr);
        }

        public static bool IsVisible()
        {
            Rml.RequireInitialised();
            return Native.Debugger.IsVisible();
        }

        public static void SetVisible(bool visible)
        {
            Rml.RequireInitialised();
            Native.Debugger.SetVisible(visible);
        }

        public static void Shutdown()
        {
            Rml.VerifyOutsideCallback();
            Rml.RequireInitialised();
            Native.Debugger.Shutdown();
        }
    }
}

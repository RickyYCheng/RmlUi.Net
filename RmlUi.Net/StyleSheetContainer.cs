using RmlUiNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RmlUiNet
{

    public class StyleSheetContainer : RmlBase<StyleSheetContainer>
    {
        private readonly bool _ownsContainer;
        public StyleSheetContainer(IntPtr ptr, bool automaticallyRegisterInCache = false) : base(ptr, automaticallyRegisterInCache)
        {

        }

        public StyleSheetContainer(string filename) : base(IntPtr.Zero, false) {
            Rml.RequireInitialised();
            ArgumentNullException.ThrowIfNull(filename);
            NativePtr = Native.StyleSheetContainer.Create();
            _ownsContainer = true;
            try { LoadStyleSheetContainer(filename); }
            catch { Dispose(); throw; }
        }

        public void LoadStyleSheetContainer(string filename)
        {
            ArgumentNullException.ThrowIfNull(filename);
            if (!Native.StyleSheetContainer.LoadStyleSheetContainer(NativePtr, filename))
                throw new System.IO.IOException($"Could not load stylesheet '{filename}'.");
        }

        public override void Dispose()
        {
            Rml.VerifyOutsideCallback();
            if (IsDisposed)
                return;
            if (_ownsContainer)
                Native.StyleSheetContainer.Free(NativePtr);
            base.Dispose();
        }
    }
}

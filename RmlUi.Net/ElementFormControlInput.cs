using System;
using System.Runtime.InteropServices;

namespace RmlUiNet
{
    public class ElementFormControlInput : Element<ElementFormControlInput>
    {
        #region Methods

        protected ElementFormControlInput(IntPtr ptr, bool automaticallyRegisterInCache)
            : base(ptr, automaticallyRegisterInCache)
        {
        }

        internal static ElementFormControlInput? Create(IntPtr ptr)
        {
            return GetOrCreateCache(ptr, address => new ElementFormControlInput(address, false));
        }

        public string GetValue()
        {
            return Util.ReadOwnedUtf8(Native.ElementFormControlInput.GetValue(NativePtr));
        }

        public void SetValue(string value) => Native.ElementFormControlInput.SetValue(NativePtr, value ?? throw new ArgumentNullException(nameof(value)));

        #endregion
    }
}

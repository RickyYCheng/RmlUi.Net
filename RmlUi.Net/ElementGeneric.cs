using System;

namespace RmlUiNet
{
    public class ElementGeneric : Element<ElementGeneric>
    {
        internal ElementGeneric(IntPtr ptr, bool automaticallyRegisterInCache, bool track)
            : base(ptr, automaticallyRegisterInCache, track) { }

        #region Methods

        public ElementGeneric(IntPtr ptr, bool automaticallyRegisterInCache)
            : base(ptr, automaticallyRegisterInCache)
        {
        }

        internal static ElementGeneric? Create(IntPtr ptr)
        {
            return GetOrCreateCache(ptr, ptr => new ElementGeneric(ptr, false));
        }

        #endregion
    }
}

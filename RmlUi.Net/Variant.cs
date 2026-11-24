using RmlUiNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RmlUiNet {

    public class Variant : RmlBase<Event> {
        private sealed class OwnedVariant : SafeHandle
        {
            public OwnedVariant(IntPtr pointer) : base(IntPtr.Zero, ownsHandle: true) => SetHandle(pointer);
            public override bool IsInvalid => handle == IntPtr.Zero;
            protected override bool ReleaseHandle()
            {
                Native.Variant.Free(handle);
                return true;
            }
        }

        private readonly OwnedVariant? _owned;
        /// <summary>
        /// The variant data type
        /// </summary>
        public VariantType Type {
            get
            {
                try { return (VariantType)Native.Variant.GetType(NativePtr); }
                finally { GC.KeepAlive(this); }
            }
        }

        /// <summary>
        /// The variant data
        /// </summary>
        public object? Value {
            get {
                try
                {
                switch (Type) {
                    case VariantType.DOUBLE:
                        return Native.Variant.GetAsDouble(NativePtr, 0);
                    case VariantType.INT:
                        return Native.Variant.GetAsInt(NativePtr, 0);
                    case VariantType.FLOAT:
                        return Native.Variant.GetAsFloat(NativePtr, 0f);
                    case VariantType.UINT:
                        return Native.Variant.GetAsUInt(NativePtr, 0u);
                    case VariantType.BOOL:
                        return Native.Variant.GetAsBool(NativePtr, false);
                    case VariantType.STRING:
                        var strPtr = Native.Variant.GetAsString(NativePtr, "");
                        return Util.ReadOwnedUtf8(strPtr);
                    default:
                        return null;
                }
                }
                finally { GC.KeepAlive(this); }
            }
        }

        public Variant(int value) : this(new OwnedVariant(Native.Variant.CreateInt(value))) { }
        public Variant(string value) : this(new OwnedVariant(Native.Variant.CreateString(value ?? throw new ArgumentNullException(nameof(value))))) { }
        public Variant(uint value) : this(new OwnedVariant(Native.Variant.CreateUInt(value))) { }
        public Variant(float value) : this(new OwnedVariant(Native.Variant.CreateFloat(value))) { }
        public Variant(double value) : this(new OwnedVariant(Native.Variant.CreateDouble(value))) { }
        public Variant(bool value) : this(new OwnedVariant(Native.Variant.CreateBool(value))) { }

        private Variant(OwnedVariant owner) : base(owner.DangerousGetHandle())
        {
            _owned = owner;
        }

        public Variant(IntPtr ptr, bool automaticallyRegisterInCache = false) : base(ptr, automaticallyRegisterInCache) {

        }

        protected override void Dispose(bool disposing)
        {
            _owned?.Dispose();
            base.Dispose(disposing);
        }

        public override string ToString() {
            return $"Variant{{{Type}: {Value}}}";
        }
    }
}

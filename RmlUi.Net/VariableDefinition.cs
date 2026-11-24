using System;
using System.Text;

namespace RmlUiNet {
    public abstract class VariableDefinition : RmlBase<VariableDefinition> {
        private Native.VariableDefinition.OnGet _onGet;
        private Native.VariableDefinition.OnSet _onSet;
        private Native.VariableDefinition.OnSize _onSize;
        private Native.VariableDefinition.OnChild _onChild;
        private int _references;

        public VariableDefinition(DataVariableType type) : base(IntPtr.Zero) {
            _onGet = OnGet;
            _onSet = OnSet;
            _onSize = ptr => Rml.InvokeCallback(() => Size(ptr), 0);
            _onChild = (ptr, name, index) => Rml.InvokeCallback(() => Child(ptr, name, index)?.NativePtr ?? IntPtr.Zero, IntPtr.Zero);

            NativePtr = Native.VariableDefinition.Create(
                type,
                _onGet,
                _onSet,
                _onSize,
                _onChild
            );

            ManuallyRegisterCache(NativePtr, this);
        }

        private bool OnGet(IntPtr ptr, IntPtr variantPtr)
        {
            return Rml.InvokeCallback(() =>
            {
                if (!Get(ptr, out var variant) || variant is null)
                    return false;
                Native.Variant.Copy(variantPtr, variant.NativePtr);
                GC.KeepAlive(variant);
                return true;
            }, false);
        }

        private bool OnSet(IntPtr ptr, IntPtr variantPtr)
        {
            using var variant = new Variant(variantPtr);
            return Rml.InvokeCallback(() => Set(ptr, variant), false);
        }

        internal void Retain() => _references++;
        internal void Release() => _references--;

        protected override void Dispose(bool disposing)
        {
            Rml.VerifyOutsideCallback();
            if (IsDisposed)
                return;
            if (_references != 0)
                throw new InvalidOperationException("Release the variables and models using this definition first.");
            Native.VariableDefinition.Free(NativePtr);
            base.Dispose(disposing);
        }

        public abstract DataVariable Child(IntPtr ptr, string name, int index);
        public abstract int Size(IntPtr ptr);
        public abstract bool Set(IntPtr ptr, Variant variant);
        public abstract bool Get(IntPtr ptr, out Variant variant);
    }

    public enum DataVariableType : int {
        Scalar = 0,
        Array,
        Struct
    }
}

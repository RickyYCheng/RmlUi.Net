using RmlUiNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static RmlUiNet.Native.SystemInterface;

namespace RmlUiNet {
    public unsafe class DataVariable : RmlBase<DataVariable>, IDisposable {
        public VariableDefinition Definition { get; protected set; } = null!;

        public DataVariable() : base(IntPtr.Zero) {
        }

        public DataVariable(VariableDefinition definition, IntPtr data = 0) : base(IntPtr.Zero) {
            Initialise(definition, data);
        }

        protected void CreateNative(VariableDefinition definition) {
            Initialise(definition, IntPtr.Zero);
        }

        private void Initialise(VariableDefinition definition, IntPtr data)
        {
            Rml.VerifyAccess();
            ThrowExceptionIfDisposed();
            ArgumentNullException.ThrowIfNull(definition);
            if (Identity != IntPtr.Zero)
                throw new InvalidOperationException("The variable already has a native definition.");
            NativePtr = Native.DataVariable.Create(definition.NativePtr, data);
            Definition = definition;
            Definition.Retain();
        }

        public override void Dispose() {
            Rml.VerifyOutsideCallback();
            if (IsDisposed)
                return;
            if (Identity != IntPtr.Zero)
            {
                Native.DataVariable.Free(NativePtr);
                Definition.Release();
            }
            base.Dispose();
        }
    }
}

using System;
using System.Runtime.InteropServices;

namespace RmlUiNet
{
    /// <summary>
    /// Handle to a struct type registered on a data model, used to bind its members so RmlUi
    /// can read them. Members are addressed by their byte offset inside the managed struct,
    /// which the native side converts into the member pointer RmlUi expects.
    /// </summary>
    /// <remarks>
    /// The handle is owned by the data model's type register, not by this wrapper: it stays
    /// valid as long as the data model exists, so <see cref="RmlBase{T}.Dispose()"/> does not
    /// free the native handle.
    /// </remarks>
    public sealed class StructHandle : RmlBase<StructHandle>
    {
        internal StructHandle(IntPtr ptr)
            : base(ptr)
        {
        }

        /// <summary>Binds a <see cref="float"/> member at the given byte offset.</summary>
        public bool RegisterMember(string name, int offset) => RegisterMember(name, offset, StructMemberType.Float);

        /// <summary>Binds an <see cref="int"/> member at the given byte offset.</summary>
        public bool RegisterMemberInt(string name, int offset) => RegisterMember(name, offset, StructMemberType.Int);

        /// <summary>Binds a <see cref="string"/> member at the given byte offset.</summary>
        public bool RegisterMemberString(string name, int offset) => RegisterMember(name, offset, StructMemberType.String);

        /// <summary>Binds a member of the given type at the given byte offset.</summary>
        public bool RegisterMember(string name, int offset, StructMemberType type)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            return type switch
            {
                StructMemberType.Float => Native.DataModelConstructor.RegisterStructMemberFloat(NativePtr, name, offset),
                StructMemberType.Int => Native.DataModelConstructor.RegisterStructMemberInt(NativePtr, name, offset),
                StructMemberType.String => Native.DataModelConstructor.RegisterStructMemberString(NativePtr, name, offset),
                _ => throw new ArgumentOutOfRangeException(nameof(type)),
            };
        }
    }

    /// <summary>Member types RmlUi can read from a registered struct.</summary>
    public enum StructMemberType
    {
        Float,
        Int,
        String
    }
}

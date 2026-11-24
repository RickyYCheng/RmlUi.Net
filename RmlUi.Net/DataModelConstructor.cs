using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RmlUiNet;

public unsafe class DataModelConstructor : RmlBase<DataModelConstructor>
{
    private readonly Context _context;
    private readonly string _name;
    private readonly Dictionary<string, (Type Type, IntPtr Storage)> _bindings = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Native.DataModelConstructor.DataEventFunc> _events = new(StringComparer.Ordinal);
    private readonly NativeLifetime _modelLifetime = new();
    private readonly List<VariableDefinition> _definitions = new();

    public DataModelHandle Handle { get; }

    internal DataModelConstructor(Context context, string name, IntPtr ptr) : base(ptr)
    {
        _context = context;
        _name = name;
        Handle = new DataModelHandle(Native.DataModelConstructor.GetModelHandle(NativePtr));
    }

    public bool BindVariable(string name, DataVariable variable)
    {
        ArgumentNullException.ThrowIfNull(variable);
        if (!BindVariable(name, variable.NativePtr))
            return false;
        variable.Definition.Retain();
        _definitions.Add(variable.Definition);
        return true;
    }

    public bool BindVariable(string name, IntPtr variable)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        if (variable == IntPtr.Zero)
            throw new ArgumentException("A data variable is required.", nameof(variable));
        return Native.DataModelConstructor.BindVariable(NativePtr, name, variable);
    }

    public bool BindEventCallback(string name, Action<Event, IEnumerable<Variant>> callback)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(callback);
        if (_events.ContainsKey(name))
            return false;

        Native.DataModelConstructor.DataEventFunc binding = (_, evt, values, count) =>
        {
            using var scope = Rml.EnterCallback();
            var lifetime = new NativeLifetime();
            try
            {
                var variants = new Variant[count];
                for (int i = 0; i < count; i++)
                    variants[i] = new Variant(values[i]) { Lifetime = lifetime };
                callback(new Event(evt) { Lifetime = lifetime }, variants);
            }
            catch (Exception error) { Rml.ReportCallbackError(error); }
            finally { lifetime.IsAlive = false; }
        };

        if (!Native.DataModelConstructor.BindEventCallback(NativePtr, name, binding))
            return false;
        _events.Add(name, binding);
        return true;
    }

    public bool BindFloat(string name, float value) => BindScalar(name, value, Native.DataModelConstructor.BindFloat);
    public bool BindInt(string name, int value) => BindScalar(name, value, Native.DataModelConstructor.BindInt);
    public bool BindUInt(string name, uint value) => BindScalar(name, value, Native.DataModelConstructor.BindUInt);
    public bool BindBool(string name, bool value) => BindScalar(name, value, Native.DataModelConstructor.BindBool);

    private bool BindScalar<T>(string name, T value, Func<IntPtr, string, IntPtr, bool> bind) where T : unmanaged
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        IntPtr model = NativePtr;
        if (_bindings.TryGetValue(name, out var existing))
        {
            if (existing.Type != typeof(T))
                return false;
            *(T*)existing.Storage = value;
            return true;
        }

        IntPtr storage = Marshal.AllocHGlobal(sizeof(T));
        bool bound = false;
        try
        {
            *(T*)storage = value;
            bound = bind(model, name, storage);
            if (bound)
                _bindings.Add(name, (typeof(T), storage));
            return bound;
        }
        finally
        {
            if (!bound)
                Marshal.FreeHGlobal(storage);
        }
    }

    public bool BindString(string name, string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(value);
        IntPtr model = NativePtr;
        if (_bindings.TryGetValue(name, out var existing))
        {
            if (existing.Type != typeof(string))
                return false;
            Native.Rml.UpdateString(existing.Storage, value);
            return true;
        }

        IntPtr storage = Native.Rml.CreateString(value);
        bool bound = false;
        try
        {
            bound = Native.DataModelConstructor.BindString(model, name, storage);
            if (bound)
                _bindings.Add(name, (typeof(string), storage));
            return bound;
        }
        finally
        {
            if (!bound)
                Native.Rml.FreeString(storage);
        }
    }

    public StructHandle? RegisterStructHandle(string name)
    {
        IntPtr handle = Native.DataModelConstructor.RegisterStruct(NativePtr, name);
        return handle == IntPtr.Zero ? null : new StructHandle(handle) { Lifetime = _modelLifetime };
    }

    [Obsolete("Use RegisterStructHandle instead.")]
    public int RegisterStruct(string name) => RegisterStructHandle(name) is null ? 0 : 1;

    public override void Dispose()
    {
        Rml.VerifyOutsideCallback();
        if (!IsDisposed)
            _context.RemoveDataModel(_name);
    }

    internal void Release()
    {
        if (IsDisposed)
            return;
        Handle.Dispose();
        Native.DataModelConstructor.Free(NativePtr);
        foreach (var binding in _bindings.Values)
        {
            if (binding.Type == typeof(string))
                Native.Rml.FreeString(binding.Storage);
            else
                Marshal.FreeHGlobal(binding.Storage);
        }
        _bindings.Clear();
        _events.Clear();
        foreach (var definition in _definitions)
            definition.Release();
        _definitions.Clear();
        _modelLifetime.IsAlive = false;
        base.Dispose();
    }
}

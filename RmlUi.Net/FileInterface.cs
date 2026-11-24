using System;
using System.Runtime.InteropServices;

namespace RmlUiNet
{
    public abstract class FileInterface : RmlBase<FileInterface>
    {
        private Native.FileInterface.OnOpen _onOpen;
        private Native.FileInterface.OnClose _onClose;
        private Native.FileInterface.OnRead _onRead;
        private Native.FileInterface.OnSeek _onSeek;
        private Native.FileInterface.OnTell _onTell;
        private Native.FileInterface.OnLength _onLength;
        private Native.FileInterface.OnLoadFile _onLoadFile;

        public unsafe FileInterface() : base(IntPtr.Zero)
        {
            Rml.CheckAbi();
            _onOpen = path => Rml.InvokeCallback(() => Open(path), IntPtr.Zero);
            _onClose = file => Rml.InvokeCallback(() => Close(file));
            _onRead = _Read;
            _onSeek = (file, offset, origin) => Rml.InvokeCallback(() => SeekSigned(file, offset, origin), false);
            _onTell = file => Rml.InvokeCallback(() => checked((nuint)Tell(file)), (nuint)0);
            _onLength = file => Rml.InvokeCallback(() => checked((nuint)Length(file)), (nuint)0);
            _onLoadFile = (output, path) => Rml.InvokeCallback(() =>
            {
                string data = LoadFile(path);
                if (string.IsNullOrEmpty(data))
                    return false;
                Native.Rml.UpdateString(output, data);
                return true;
            }, false);

            NativePtr = Native.FileInterface.Create(
                _onOpen,
                _onClose,
                _onLoadFile,
                _onRead,
                _onSeek,
                _onTell,
                _onLength
            );

            ManuallyRegisterCache(NativePtr, this);
        }

        internal unsafe nuint _Read(byte* buffer, nuint size, IntPtr file)
        {
            using var scope = Rml.EnterCallback();
            try
            {
                var len = Read(size, file, out byte[] bytes);
                if (len == 0)
                    return 0;
                if (buffer == null || bytes is null || len > size || len > (ulong)bytes.Length)
                    throw new InvalidOperationException("The file reader returned an invalid byte count.");
                Marshal.Copy(bytes, 0, (IntPtr)buffer, (int)len);
                return (nuint)len;
            }
            catch (Exception error) { Rml.ReportCallbackError(error); return 0; }
        }

        protected virtual bool SeekSigned(IntPtr file, long offset, int origin) =>
            offset >= 0 && offset <= uint.MaxValue && Seek(file, (uint)offset, origin);

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            Rml.VerifyOutsideCallback();
            if (Rml.IsRegistered(this))
                throw new InvalidOperationException("Shut down RmlUi or replace the installed file interface before disposing it.");
            Native.FileInterface.Free(NativePtr);
            base.Dispose(disposing);
        }

        public abstract IntPtr Open(string path);
        public abstract void Close(IntPtr file);
        public abstract ulong Read(ulong size, IntPtr file, out byte[] bytes);
        public abstract bool Seek(IntPtr file, uint offset, int origin);
        public abstract ulong Tell(IntPtr file);
        public abstract ulong Length(IntPtr file);
        public abstract string LoadFile(string path);
    }
}

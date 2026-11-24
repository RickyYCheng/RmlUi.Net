using System;
using System.Text;
using static RmlUiNet.Native.SystemInterface;

namespace RmlUiNet
{
    public abstract class SystemInterface : RmlBase<SystemInterface>
    {
        private Native.SystemInterface.OnGetElapsedTime _onGetElapsedTime;
        private Native.SystemInterface.OnTranslateString _onTranslateString;
        private Native.SystemInterface.OnLogMessage _onLogMessage;
        private Native.SystemInterface.OnJoinPath _onJoinPath;
        private Native.SystemInterface.OnSetMouseCursor _onSetMouseCursor;
        private Native.SystemInterface.OnSetClipboardText _onSetClipboardText;
        private Native.SystemInterface.OnGetClipboardText _onGetClipboardText;
        private Native.SystemInterface.OnActivateKeyboard _onActivateKeyboard;
        private Native.SystemInterface.OnDeactivateKeyboard _onDeactivateKeyboard;

        public SystemInterface() : base(IntPtr.Zero)
        {
            Rml.CheckAbi();
            _onGetElapsedTime = () => Rml.InvokeCallback(() => ElapsedTime, 0d);
            _onTranslateString = (output, input) => Rml.InvokeCallback(() =>
            {
                string result = TranslateString(out int changes, input);
                Native.Rml.UpdateString(output, result ?? input);
                return changes;
            }, 0);
            _onLogMessage = (type, message) => Rml.InvokeCallback(() => LogMessage(type, message), true);
            _onJoinPath = (output, path, file) => Rml.InvokeCallback(() => Native.Rml.UpdateString(output, JoinPath(path, file) ?? file));
            _onSetMouseCursor = cursor => Rml.InvokeCallback(() => SetMouseCursor(cursor));
            _onSetClipboardText = text => Rml.InvokeCallback(() => SetClipboardText(text));
            _onGetClipboardText = output => Rml.InvokeCallback(() => Native.Rml.UpdateString(output, GetClipboardText() ?? string.Empty));
            _onActivateKeyboard = (x, y, height) => Rml.InvokeCallback(() => ActivateKeyboard(x, y, height));
            _onDeactivateKeyboard = () => Rml.InvokeCallback(DeactivateKeyboard);

            NativePtr = Native.SystemInterface.Create(
                _onGetElapsedTime,
                _onTranslateString,
                _onLogMessage,
                _onJoinPath,
                _onSetMouseCursor,
                _onSetClipboardText,
                _onGetClipboardText,
                _onActivateKeyboard,
                _onDeactivateKeyboard
            );

            ManuallyRegisterCache(NativePtr, this);
        }

        public virtual string JoinPath(string path, string file) {
            return System.IO.Path.Combine(path, file);
        }

        /// <summary>
        /// Get the number of seconds elapsed since the start of the application.
        /// </summary>
        public abstract double ElapsedTime { get; }

        /// <summary>
        /// Translate the input string into the translated string.
        /// </summary>
        /// <param name="changeCount">Number of translations that occurred.</param>
        /// <param name="input">String as received from XML.</param>
        /// <returns>Translated string ready for display.</returns>
        public virtual string TranslateString(out int changeCount, string input)
        {
            changeCount = 0;
            return input;
        }

        public virtual bool LogMessage(LogType type, string message)
        {
            Console.WriteLine("[{0}] {1}", type, message);

            return true;
        }

        public virtual void SetMouseCursor(string cursor) {

        }

        public abstract void SetClipboardText(string text);

        public abstract string GetClipboardText();

        public abstract void ActivateKeyboard(float caretX, float caretY, float lineHeight);

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            Rml.VerifyOutsideCallback();
            if (Rml.IsRegistered(this))
                throw new InvalidOperationException("Shut down RmlUi or replace the installed system interface before disposing it.");
            Native.SystemInterface.Free(NativePtr);
            base.Dispose(disposing);
        }

        public abstract void DeactivateKeyboard();
    }

    public enum LogType
    {
        Always = 0,
        Error,
        Assert,
        Warning,
        Info,
        Debug,
    }
}

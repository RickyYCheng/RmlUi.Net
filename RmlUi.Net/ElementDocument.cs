using System;
using System.Runtime.InteropServices;

namespace RmlUiNet
{
    public class ElementDocument : Element<ElementDocument>
    {
        private bool _isCustomElement;
        private Native.ElementDocument.OnLoadInlineScript? _onLoadInlineScript;
        private Native.ElementDocument.OnLoadExternalScript? _onLoadExternalScript;

        #region Properties

        #endregion

        #region Methods
        public ElementDocument() : base(IntPtr.Zero, false) {
            Rml.RequireInitialised();
            Rml.VerifyOutsideCallback();
            _isCustomElement = true;
            _onLoadInlineScript = (text, path, line) => Rml.InvokeCallback(() => OnLoadInlineScript(text, path, line));
            _onLoadExternalScript = path => Rml.InvokeCallback(() => OnLoadExternalScript(path));

            NativePtr = Native.ElementDocument.Create(_onLoadInlineScript, _onLoadExternalScript);
            Lifetime = RmlInstanceCache.Instance.TrackElement(NativePtr);
            RmlInstanceCache.Instance.RetainElementOwner(NativePtr, this);
        }

        protected ElementDocument(IntPtr ptr, bool automaticallyRegisterInCache)
            : base(ptr, automaticallyRegisterInCache)
        {
        }

        public virtual void OnLoadInlineScript(string context, string source_path, int source_line)
        {

        }

        public virtual void OnLoadExternalScript(string source_path)
        {

        }

        /// <summary>
        /// Show the document.
        /// </summary>
        /// <param name="modalFlag">Flags controlling the modal state of the document, see the 'ModalFlag' description for details.</param>
        /// <param name="focusFlag">Flags controlling the focus, see the 'FocusFlag' description for details.</param>
        /// <param name="scrollFlag">Flags controlling whether the focused element is scrolled into view.</param>
        public void Show(ModalFlag modalFlag = ModalFlag.None, FocusFlag focusFlag = FocusFlag.Auto, ScrollFlag scrollFlag = ScrollFlag.Auto)
        {
            Native.ElementDocument.Show(NativePtr, modalFlag, focusFlag, scrollFlag);
        }

        public void PullToFront() {
            Native.ElementDocument.PullToFront(NativePtr);
        }

        /// <summary>
        /// Hide the document.
        /// </summary>
        public void Hide()
        {
            Native.ElementDocument.Hide(NativePtr);
        }

        /// <summary>
        /// Merge a style sheet container into the document.
        /// </summary>
        /// <param name="container"></param>
        public void AddStyleSheetContainer(StyleSheetContainer container)
        {
            Native.ElementDocument.AddStyleSheetContainer(NativePtr, container.NativePtr);
        }

        /// <summary>
        /// Set the title of the document.
        /// </summary>
        /// <param name="title"></param>
        public void SetTitle(string title) {
            Native.ElementDocument.SetTitle(NativePtr, title);
        }

        /// <summary>
        /// Get the title of the document.
        /// </summary>
        /// <returns></returns>
        public string GetTitle()
        {
            return Util.ReadOwnedUtf8(Native.ElementDocument.GetTitle(NativePtr));
        }

        public string GetSourceURL()
        {
            return Util.ReadOwnedUtf8(Native.ElementDocument.GetSourceURL(NativePtr));
        }

        /*
        /// <summary>
        /// Create an element with the given tag name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IntPtr CreateElement(string tagName)
        {
            return Native.ElementDocument.CreateElement(NativePtr, tagName);
        }
        */

        /// <summary>
        /// Close the document.
        /// </summary>
        /// <remarks>
        /// The destruction of the document is deferred until the next call to Context::Update().
        /// </remarks>
        public void Close()
        {
            Native.ElementDocument.Close(NativePtr);
        }

        internal static ElementDocument? Create(IntPtr ptr)
        {
            if (RmlInstanceCache.Instance.GetElementOwner(ptr) is ElementDocument custom)
                return custom;
            return GetOrCreateCache(ptr, ptr => new ElementDocument(ptr, false));
        }

        public override void Dispose()
        {
            Rml.VerifyOutsideCallback();
            if (IsDisposed)
                return;
            if (_isCustomElement)
            {
                if (GetParentNode() is not null)
                    throw new InvalidOperationException("Do not dispose a document owned by RmlUi.");
                Native.ElementDocument.Free(NativePtr);
            }
            base.Dispose();
        }

        #endregion
    }
}

#include "RmlNative.h"
#include "RmlUi/Core/ElementDocument.h"
#include "RmlUi/Core/StyleSheet.h"
#include "RmlUi/Core/StyleSheetContainer.h"
#include "RmlUi/Core/StyleSheetTypes.h"
#include "ElementPool.h"
#include "Pool.h"

// ScrollFlag is new in RmlUi 6.3: showing a document can now scroll the focused element
// into view. All three flags are passed explicitly so the managed side owns the defaults.
RMLUI_CAPI void rml_ElementDocument_Show(Rml::ElementDocument* document, Rml::ModalFlag modal_flag, Rml::FocusFlag focus_flag, Rml::ScrollFlag scroll_flag) {
    if (document == nullptr) {
        return;
    }
    document->Show(modal_flag, focus_flag, scroll_flag);
}

RMLUI_CAPI void rml_ElementDocument_Hide(Rml::ElementDocument* document) {
    document->Hide();
}

RMLUI_CAPI void rml_ElementDocument_PullToFront(Rml::ElementDocument* document) {
    document->PullToFront();
}

RMLUI_CAPI void rml_ElementDocument_SetTitle(Rml::ElementDocument* document, const char* title) {
    document->SetTitle(title);
}

RMLUI_CAPI const char* rml_ElementDocument_GetTitle(Rml::ElementDocument* document) {
    return RMLUI_DUP_STR(document->GetTitle().c_str());
}

RMLUI_CAPI const char* rml_ElementDocument_GetSourceURL(Rml::ElementDocument* document) {
    return RMLUI_DUP_STR(document->GetSourceURL().c_str());
}

RMLUI_CAPI Rml::Element* rml_ElementDocument_CreateElement(Rml::ElementDocument* document, const char* name) {
    if (document == nullptr || name == nullptr) {
        return nullptr;
    }

    return RmlNative::StoreDetachedElement(document->CreateElement(name));
}

RMLUI_CAPI Rml::Element* rml_ElementDocument_CreateTextNode(Rml::ElementDocument* document, const char* text) {
    if (document == nullptr || text == nullptr) {
        return nullptr;
    }

    return RmlNative::StoreDetachedElement(document->CreateTextNode(text));
}

RMLUI_CAPI void rml_ElementDocument_AddStyleSheetContainer(Rml::ElementDocument* document, Rml::StyleSheetContainer* container) {
    if (document == nullptr || container == nullptr) {
        return;
    }

    // Merge the supplied container with the document's current one. The document owns the
    // combined container; the caller keeps ownership of the one it passed in.
    const Rml::StyleSheetContainer* current = document->GetStyleSheetContainer();
    Rml::SharedPtr<Rml::StyleSheetContainer> combined;
    if (current != nullptr) {
        combined = current->CombineStyleSheetContainer(*container);
    }
    else {
        Rml::StyleSheetContainer empty;
        combined = empty.CombineStyleSheetContainer(*container);
    }

    document->SetStyleSheetContainer(combined);
}

RMLUI_CAPI void rml_ElementDocument_Close(Rml::ElementDocument* document) {
    document->Close();
}

typedef void(*onLoadInlineScript)(const char *context, const char *source_path, int source_line);
typedef void(*onLoadExternalScript)(const char *source_path);

class ElementDocumentCustom : public Rml::ElementDocument {
private:
    ::onLoadInlineScript m_onLoadInlineScript;
    ::onLoadExternalScript m_onLoadExternalScript;

public:
    explicit ElementDocumentCustom(const Rml::String& tag, ::onLoadInlineScript onLoadInlineScript, ::onLoadExternalScript onLoadExternalScript) : ElementDocument(tag) {
        m_onLoadInlineScript = onLoadInlineScript;
        m_onLoadExternalScript = onLoadExternalScript;
    }

    void LoadInlineScript(const Rml::String& context, const Rml::String& source_path, int source_line) override {
        if (m_onLoadInlineScript != NULL) {
            (*m_onLoadInlineScript)(context.c_str(), source_path.c_str(), source_line);
        }
    }

    void LoadExternalScript(const Rml::String& source_path) override {
        if (m_onLoadExternalScript != NULL) {
            (*m_onLoadExternalScript)(source_path.c_str());
        }
    }
};

RMLUI_CAPI void *rml_ElementDocument_New(::onLoadInlineScript onLoadInlineScript, ::onLoadExternalScript onLoadExternalScript) {
    return RmlNative::OwnBindingElement(new ElementDocumentCustom("body", onLoadInlineScript, onLoadExternalScript));
}

RMLUI_CAPI void rml_ElementDocument_Free(ElementDocumentCustom *element) {
    RmlNative::TakeBindingElement(element);
}

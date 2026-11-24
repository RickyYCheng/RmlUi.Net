#include "RmlNative.h"
#include "RmlUi/Core.h"
#include "RmlUi/Core/Element.h"
#include "Util.h"
#include "ElementPool.h"

typedef void(*onRender)();

class ElementCustom : public Rml::Element {
private:
    ::onRender m_onRender;

public:
    explicit ElementCustom(const Rml::String& tag, ::onRender onRender) : Element(tag) {
        m_onRender = onRender;
    }

    void OnRender() override {
        if (m_onRender != NULL) {
            (*m_onRender)();
        }
        else {
            Rml::Element::OnRender();
        }
    }
};

RMLUI_CAPI void *rml_ElementCustom_New(char *tag, ::onRender onRender) {
    return RmlNative::OwnBindingElement(new ElementCustom(tag, onRender));
}

RMLUI_CAPI void rml_ElementCustom_Free(ElementCustom *element) {
    RmlNative::TakeBindingElement(element);
}

#include "RmlNative.h"
#include "RmlUi/Core.h"
#include "RmlUi/Core/Box.h"
#include "EventListener.h"

struct V2F {
    float X;
    float Y;
};

// The previous versions of these exports returned a newly allocated V2F that the managed
// side never freed, leaking on every call. They now write into a caller-provided struct,
// which matches the managed 'out Vector2f' signature.

RMLUI_CAPI void rml_Box_GetPosition(Rml::Box* box, Rml::BoxArea area, V2F* out_position) {
    if (box == nullptr || out_position == nullptr) {
        return;
    }
    const Rml::Vector2f position = box->GetPosition(area);
    out_position->X = position.x;
    out_position->Y = position.y;
}

RMLUI_CAPI void rml_Box_GetSize(Rml::Box* box, V2F* out_size) {
    if (box == nullptr || out_size == nullptr) {
        return;
    }
    const Rml::Vector2f size = box->GetSize();
    out_size->X = size.x;
    out_size->Y = size.y;
}

RMLUI_CAPI void rml_Box_GetSizeBoxArea(Rml::Box* box, Rml::BoxArea area, V2F* out_size) {
    if (box == nullptr || out_size == nullptr) {
        return;
    }
    const Rml::Vector2f size = box->GetSize(area);
    out_size->X = size.x;
    out_size->Y = size.y;
}

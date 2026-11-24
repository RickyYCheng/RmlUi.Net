#include "RmlNative.h"
#include "RmlUi/Core.h"
#include "ElementPool.h"
#include "EventListener.h"
#include <cstdlib>
#include <string>

using OnDestroyed = void(*)(void*);

class LifetimeObserver final : public Rml::Plugin {
public:
    OnDestroyed context_destroyed = nullptr;
    OnDestroyed element_destroyed = nullptr;

    int GetEventClasses() override { return EVT_BASIC | EVT_ELEMENT; }
    void OnContextDestroy(Rml::Context* context) override { context_destroyed(context); }
    void OnElementDestroy(Rml::Element* element) override { element_destroyed(element); }
};

static LifetimeObserver lifetime_observer;

RMLUI_CAPI int rml_BindingAbiVersion() { return 2; }

RMLUI_CAPI void rml_ObserveLifetime(OnDestroyed context_destroyed, OnDestroyed element_destroyed) {
    lifetime_observer.context_destroyed = context_destroyed;
    lifetime_observer.element_destroyed = element_destroyed;
    Rml::RegisterPlugin(&lifetime_observer);
}

RMLUI_CAPI bool rml_Initialise() {
    return Rml::Initialise();
}

RMLUI_CAPI void rml_Shutdown() {
    // Release detached elements first: their release callbacks may call back into
    // managed code and must run while both the managed side and RmlUi are alive.
    RmlNative::ClearDetachedElements();
    Rml::Shutdown();
}

RMLUI_CAPI void rml_SetRenderInterface(Rml::RenderInterface *renderInterface) {
    Rml::SetRenderInterface(renderInterface);
}

RMLUI_CAPI void rml_SetSystemInterface(Rml::SystemInterface *systemInterface) {
    Rml::SetSystemInterface(systemInterface);
}

RMLUI_CAPI void rml_SetFileInterface(Rml::FileInterface* fileInterface) {
    Rml::SetFileInterface(fileInterface);
}

RMLUI_CAPI bool rml_LoadFontFace(const char *fileName, bool fallbackFace, Rml::Style::FontWeight weight) {
    if (fileName == nullptr) {
        return false;
    }
    return Rml::LoadFontFace(fileName, fallbackFace, weight);
}

// Loads a font face and registers it under an explicit family/style/weight, the 6.3
// overload that backs the RCSS @font-face rule. Returns false when RmlUi rejects it.
RMLUI_CAPI bool rml_LoadFontFaceEx(const char *fileName, const char *family, Rml::Style::FontStyle style,
                                   Rml::Style::FontWeight weight, bool fallbackFace, int faceIndex) {
    if (fileName == nullptr || family == nullptr) {
        return false;
    }
    return Rml::LoadFontFace(fileName, family, style, weight, fallbackFace, faceIndex);
}

RMLUI_CAPI Rml::EventId rml_RegisterEventType(const char* type, bool interruptible, bool bubbles, Rml::DefaultActionPhase default_action_phase) {
    return Rml::RegisterEventType(type, interruptible, bubbles, default_action_phase);
}

RMLUI_CAPI Rml::EventListener *rml_CreateEventListener(::onProcessEvent onProcessEvent, ::onAttachEvent onAttachEvent, ::onDetachEvent onDetachEvent) {
    return new EventListenerProxy(onProcessEvent, onAttachEvent, onDetachEvent);
}

RMLUI_CAPI void rml_ReleaseEventListener(Rml::EventListener *eventListener) {
    delete eventListener;
}

RMLUI_CAPI bool rml_RemoveContext(const char *name) {
    if (name == nullptr) {
        return false;
    }
    return Rml::RemoveContext(name);
}

RMLUI_CAPI void rml_RegisterPlugin(Rml::Plugin *plugin) {
    Rml::RegisterPlugin(plugin);
}

RMLUI_CAPI Rml::RenderInterface* rml_GetRenderInterface() {
    return Rml::GetRenderInterface();
}

RMLUI_CAPI Rml::SystemInterface* rml_GetSystemInterface() {
    return Rml::GetSystemInterface();
}

RMLUI_CAPI std::string* rml_CreateString(const char* value) {
    return value ? new Rml::String(value) : nullptr;
}

RMLUI_CAPI void rml_Log(Rml::Log::Type type,  char* message) {
    // Pass the message as an argument: Log::Message is printf-style and GCC's
    // -Wformat-security rejects a non-literal format string.
    Rml::Log::Message(type, "%s", message);
}

RMLUI_CAPI void rml_UpdateString(std::string* str, const char* new_value) {
    *str = new_value;
}

RMLUI_CAPI void rml_FreeString(std::string* str) {
    delete str;
}

// Frees a string buffer previously returned by this library. String-returning
// exports allocate with malloc (via strdup/_strdup); the managed side must not
// free those buffers with a mismatched allocator.
RMLUI_CAPI void rml_String_Free(char* str) {
        free(str);
}

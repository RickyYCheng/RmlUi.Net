#pragma once

#include "RmlUi/Core/Element.h"
#include "RmlUi/Core/ElementInstancer.h"

#include <unordered_map>
#include <unordered_set>

// Keeps elements created by rml_ElementDocument_CreateElement / rml_ElementDocument_CreateTextNode
// alive until they are attached to the document tree. RmlUi hands out owning pointers
// (Rml::ElementPtr); the binding stores them here so that a raw pointer can cross the
// P/Invoke boundary without the element being destroyed when the C++ function returns.
namespace RmlNative {

inline std::unordered_map<Rml::Element*, Rml::ElementPtr>& DetachedElementPool() {
    static std::unordered_map<Rml::Element*, Rml::ElementPtr> pool;
    return pool;
}

// Takes ownership of a freshly created element and returns its raw pointer.
inline Rml::Element* StoreDetachedElement(Rml::ElementPtr element) {
    Rml::Element* raw = element.get();
    if (raw != nullptr) {
        DetachedElementPool().emplace(raw, std::move(element));
    }
    return raw;
}

// Releases ownership of an element previously stored by StoreDetachedElement.
// Returns nullptr if the element is not in the pool.
inline Rml::ElementPtr TakeDetachedElement(Rml::Element* element) {
    if (element == nullptr) {
        return nullptr;
    }

    auto& pool = DetachedElementPool();
    auto it = pool.find(element);
    if (it == pool.end()) {
        return nullptr;
    }

    Rml::ElementPtr result = std::move(it->second);
    pool.erase(it);
    return result;
}

inline auto& BindingElements() {
    static std::unordered_set<Rml::Element*> elements;
    return elements;
}

inline void ReleaseBindingElement(Rml::Element* element) {
    BindingElements().erase(element);
    delete element;
}

class BindingElementOwner final : public Rml::ElementInstancer {
public:
    Rml::ElementPtr InstanceElement(Rml::Element*, const Rml::String&, const Rml::XMLAttributes&) override {
        return nullptr;
    }

    void ReleaseElement(Rml::Element* element) override { ReleaseBindingElement(element); }
};

inline Rml::Element* OwnBindingElement(Rml::Element* element) {
    static BindingElementOwner owner;
    element->SetInstancer(&owner);
    BindingElements().insert(element);
    return StoreDetachedElement(Rml::ElementPtr(element));
}

inline Rml::ElementPtr TakeBindingElement(Rml::Element* element) {
    return BindingElements().count(element) ? TakeDetachedElement(element) : Rml::ElementPtr();
}

// Releases every detached element still in the pool. Must be called before
// Rml::Shutdown so that elements instanced through custom (managed-backed)
// instancers are released while their callbacks are still alive.
inline void ClearDetachedElements() {
    DetachedElementPool().clear();
}

} // namespace RmlNative

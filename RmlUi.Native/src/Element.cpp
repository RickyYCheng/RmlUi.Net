#include "RmlNative.h"
#include "RmlUi/Core/Element.h"
#include "RmlUi/Core/Types.h"
#include "ElementPool.h"
#include "Util.h"

RMLUI_CAPI const char *rml_Element_GetTagName(Rml::Element *element) {
    return element->GetTagName().c_str();
}

RMLUI_CAPI void
rml_Element_AddEventListener(Rml::Element *element, const char *event, Rml::EventListener *eventListener,
                             bool inCapturePhase) {
    element->AddEventListener(event, eventListener, inCapturePhase);
}

RMLUI_CAPI void
rml_Element_RemoveEventListener(Rml::Element *element, const char *event, Rml::EventListener *eventListener,
                                bool inCapturePhase) {
    element->RemoveEventListener(event, eventListener, inCapturePhase);
}

RMLUI_CAPI const char *rml_Element_GetElementByID(Rml::Element *element, const char *id, Rml::Element **foundElement) {
    if (foundElement != nullptr) {
        *foundElement = nullptr;
    }
    if (element == nullptr || id == nullptr || foundElement == nullptr) {
        return nullptr;
    }

    *foundElement = element->GetElementById(id);

    if (*foundElement) {
        return Util::GetElementMarshalId(*foundElement);
    }

    return nullptr;
}

RMLUI_CAPI const char *rml_Element_GetElementTypeName(Rml::Element *element) {
    return Util::GetElementMarshalId(element);
}

RMLUI_CAPI const char *rml_Element_GetOwnerDocument(Rml::Element *element, Rml::Element **foundElement) {
    // Validate input pointers
    if (element == nullptr) {
        if (foundElement) *foundElement = nullptr;
        return nullptr;
    }

    // Safely get owner document
    try {
        Rml::Element* ownerDocument = element->GetOwnerDocument();
        
        // Check if document was found
        if (ownerDocument == nullptr) {
            if (foundElement) *foundElement = nullptr;
            return nullptr;
        }

        // Set the output parameter if provided
        if (foundElement) {
            *foundElement = ownerDocument;
        }

        // Safely get type name
        return ownerDocument ? Util::GetElementMarshalId(ownerDocument) : nullptr;
    }
    catch (...) {
        // Handle any unexpected exceptions
        if (foundElement) *foundElement = nullptr;
        return nullptr;
    }
}

RMLUI_CAPI const char* rml_Element_GetInnerRml(Rml::Element *element) {
    return RMLUI_DUP_STR(element->GetInnerRML().c_str());
}

RMLUI_CAPI void rml_Element_SetInnerRml(Rml::Element *element, const char* rml) {
    element->SetInnerRML(rml);
}

RMLUI_CAPI bool rml_Element_Focus(Rml::Element *element) {
    if (element == nullptr) {
        return false;
    }
    return element->Focus();
}

RMLUI_CAPI void rml_Element_Blur(Rml::Element *element) {
    element->Blur();
}

RMLUI_CAPI void rml_Element_Click(Rml::Element *element) {
    element->Click();
}

RMLUI_CAPI float rml_Element_GetOffsetTop(Rml::Element *element) {
    return element->GetOffsetTop();
}

RMLUI_CAPI float rml_Element_GetAbsoluteLeft(Rml::Element *element) {
    return element->GetAbsoluteLeft();
}

RMLUI_CAPI float rml_Element_GetAbsoluteTop(Rml::Element *element) {
    return element->GetAbsoluteTop();
}

RMLUI_CAPI void rml_Element_SetAttributeString(Rml::Element *element, const char *attr, const char *value) {
    if (element == nullptr || attr == nullptr || value == nullptr) {
        return;
    }
    element->SetAttribute(attr, value);
}

RMLUI_CAPI const char* rml_Element_GetAttributeString(Rml::Element *element, const char *attr, const char *default_value) {
    return RMLUI_DUP_STR(element->GetAttribute<Rml::String>(attr, default_value).c_str());
}

RMLUI_CAPI bool rml_Element_SetProperty(Rml::Element *element, const char *name, const char *value) {
    if (element == nullptr || name == nullptr || value == nullptr) {
        return false;
    }
    return element->SetProperty(name, value);
}

RMLUI_CAPI const Rml::Property* rml_Element_GetProperty(Rml::Element *element, const char *name) {
    return element->GetLocalProperty(name);
}

RMLUI_CAPI const Rml::Property* rml_Element_GetPropertyById(Rml::Element *element, Rml::PropertyId property_id) {
    return element->GetLocalProperty(property_id);
}

RMLUI_CAPI const char* rml_Element_GetPropertyString(Rml::Element *element, const char *name) {
    if (element == nullptr || name == nullptr) {
        return RMLUI_DUP_STR("");
    }

    const Rml::Property* property = element->GetProperty(name);
    if (property == nullptr) {
        return RMLUI_DUP_STR("");
    }

    // Property::ToString keeps the unit (e.g. "50px") unlike the raw variant string.
    return RMLUI_DUP_STR(property->ToString().c_str());
}

RMLUI_CAPI Rml::Element* rml_Element_GetChild(Rml::Element *element, int index) {
    return element->GetChild(index);
}

RMLUI_CAPI int rml_Element_GetNumChildren(Rml::Element *element, bool include_non_dom_elements) {
    return element->GetNumChildren(include_non_dom_elements);
}

RMLUI_CAPI float rml_Element_GetClientHeight(Rml::Element *element) {
    return element->GetClientHeight();
}

RMLUI_CAPI bool rml_Element_IsClassSet(Rml::Element *element, const char* class_name) {
    return element->IsClassSet(class_name);
}

RMLUI_CAPI void rml_Element_SetClass(Rml::Element *element, const char* class_name, bool activate) {
    if (element == nullptr || class_name == nullptr) {
        return;
    }
    element->SetClass(class_name, activate);
}

RMLUI_CAPI float rml_Element_GetClientLeft(Rml::Element *element) {
    return element->GetClientLeft();
}

RMLUI_CAPI float rml_Element_GetClientTop(Rml::Element *element) {
    return element->GetClientTop();
}

RMLUI_CAPI float rml_Element_GetClientWidth(Rml::Element *element) {
    return element->GetClientWidth();
}

RMLUI_CAPI Rml::Element* rml_Element_GetFirstChild(Rml::Element *element) {
    return element->GetFirstChild();
}

RMLUI_CAPI const char* rml_Element_GetId(Rml::Element *element) {
    return element->GetId().c_str();
}

RMLUI_CAPI void rml_Element_SetId(Rml::Element *element, const char* id) {
    element->SetId(id);
}

RMLUI_CAPI Rml::Element* rml_Element_GetLastChild(Rml::Element *element) {
    return element->GetLastChild();
}

RMLUI_CAPI Rml::Element* rml_Element_GetNextSibling(Rml::Element *element) {
    return element->GetNextSibling();
}

RMLUI_CAPI float rml_Element_GetOffsetHeight(Rml::Element *element) {
    return element->GetOffsetHeight();
}

RMLUI_CAPI float rml_Element_GetOffsetLeft(Rml::Element *element) {
    return element->GetOffsetLeft();
}

RMLUI_CAPI Rml::Element* rml_Element_GetOffsetParent(Rml::Element *element) {
    return element->GetOffsetParent();
}

RMLUI_CAPI float rml_Element_GetOffsetWidth(Rml::Element *element) {
    return element->GetOffsetWidth();
}

RMLUI_CAPI Rml::Element* rml_Element_GetParentNode(Rml::Element *element) {
    return element->GetParentNode();
}

static bool CanAdopt(Rml::Element* parent, Rml::Element* child) {
    if (!parent || !child || dynamic_cast<Rml::ElementDocument*>(child))
        return false;
    for (auto* ancestor = parent; ancestor; ancestor = ancestor->GetParentNode())
        if (ancestor == child)
            return false;
    return true;
}

RMLUI_CAPI void rml_Element_ReplaceChild(Rml::Element *element, Rml::Element *inserted_element, Rml::Element *replaced_element) {
    if (!CanAdopt(element, inserted_element) || !replaced_element ||
            replaced_element->GetParentNode() != element || inserted_element == replaced_element) {
        return;
    }

    // The inserted element is either a detached element created through the binding,
    // or already part of a document tree. In both cases take ownership before use.
    Rml::ElementPtr inserted = RmlNative::TakeDetachedElement(inserted_element);
    if (!inserted) {
        Rml::Element* parent = inserted_element->GetParentNode();
        if (parent == nullptr) {
            return;
        }
        inserted = parent->RemoveChild(inserted_element);
    }

    if (inserted)
        element->ReplaceChild(std::move(inserted), replaced_element);
}

RMLUI_CAPI Rml::Element* rml_Element_GetPreviousSibling(Rml::Element *element) {
    return element->GetPreviousSibling();
}

RMLUI_CAPI void rml_Element_RemoveChild(Rml::Element *element, Rml::Element *element_to_remove) {
    if (element == nullptr || element_to_remove == nullptr || element_to_remove->GetParentNode() != element ||
            dynamic_cast<Rml::ElementDocument*>(element_to_remove)) {
        return;
    }

    // RmlUi 6.0 transfers ownership of the removed element to the caller; keep it
    // alive so the managed side can re-attach it later (rml_Element_AppendChild).
    Rml::ElementPtr removed = element->RemoveChild(element_to_remove);
    if (removed) {
        RmlNative::StoreDetachedElement(std::move(removed));
    }
}

RMLUI_CAPI bool rml_Element_DestroyDetached(Rml::Element* element) {
    return bool(RmlNative::TakeDetachedElement(element));
}

RMLUI_CAPI float rml_Element_GetScrollHeight(Rml::Element *element) {
    return element->GetScrollHeight();
}

RMLUI_CAPI float rml_Element_GetScrollTop(Rml::Element *element) {
    return element->GetScrollTop();
}

RMLUI_CAPI float rml_Element_GetScrollWidth(Rml::Element *element) {
    return element->GetScrollWidth();
}

RMLUI_CAPI Rml::Element* rml_Element_AppendChild(Rml::Element *element, Rml::Element *element_to_append, bool add_to_dom) {
    if (!CanAdopt(element, element_to_append)) {
        return nullptr;
    }

    // The appended element is either a detached element created through the binding,
    // or already part of a document tree. In both cases take ownership before use.
    Rml::ElementPtr appended = RmlNative::TakeDetachedElement(element_to_append);
    if (!appended) {
        Rml::Element* parent = element_to_append->GetParentNode();
        if (parent == nullptr) {
            return nullptr;
        }
        appended = parent->RemoveChild(element_to_append);
    }

    return appended ? element->AppendChild(std::move(appended), add_to_dom) : nullptr;
}

RMLUI_CAPI Rml::Element* rml_Element_AppendChildTag(Rml::Element *element, const char* tagName, bool add_to_dom) {
    if (element == nullptr || tagName == nullptr) {
        return nullptr;
    }

    Rml::ElementDocument* document = element->GetOwnerDocument();
    if (document == nullptr) {
        return nullptr;
    }

    return element->AppendChild(document->CreateElement(tagName), add_to_dom);
}

RMLUI_CAPI Rml::Element* rml_Element_Closest(Rml::Element *element, const char* selectors) {
    return element->Closest(selectors);
}

RMLUI_CAPI bool rml_Element_DispatchEvent(Rml::Element *element, const char* event_id, Rml::Dictionary& parameters, bool interruptible, bool bubbles) {
    if (element == nullptr || event_id == nullptr) {
        return false;
    }

    return element->DispatchEvent(event_id, parameters, interruptible, bubbles);
}

RMLUI_CAPI bool rml_Element_HasAttribute(Rml::Element *element, const char* attr) {
	return element->HasAttribute(attr);
}

RMLUI_CAPI bool rml_Element_HasChildNodes(Rml::Element *element) {
	return element->HasChildNodes();
}

RMLUI_CAPI bool rml_Element_HasClass(Rml::Element *element, const char* class_name) {
	return element->IsClassSet(class_name);
}

RMLUI_CAPI bool rml_Element_InsertBefore(Rml::Element *element, Rml::Element *element_to_add, Rml::Element *adjacent_element) {
    if (!CanAdopt(element, element_to_add) || !adjacent_element ||
            adjacent_element->GetParentNode() != element || element_to_add == adjacent_element) {
        return false;
    }

    // Element::InsertBefore takes an owning ElementPtr. Constructing one directly from a raw
    // pointer made a second owner for an element that was still attached to its tree, so the
    // temporary deleted it on return and left the document with a dangling child. Take the
    // element out of its parent (or the detached-element pool) the same way AppendChild and
    // ReplaceChild do.
    Rml::ElementPtr inserted = RmlNative::TakeDetachedElement(element_to_add);
    if (!inserted) {
        Rml::Element* parent = element_to_add->GetParentNode();
        if (parent == nullptr) {
            return false;
        }
        inserted = parent->RemoveChild(element_to_add);
        if (!inserted) {
            return false;
        }
    }

    // InsertBefore returns the inserted element, or nullptr when the adjacent element is not
    // a child of this element.
    return element->InsertBefore(std::move(inserted), adjacent_element) != nullptr;
}

RMLUI_CAPI Rml::Element* rml_Element_QuerySelector(Rml::Element *element, const char* selector) {
	return element->QuerySelector(selector);
}

// Element-list queries. RmlUi fills a Vector<Element*> that the caller owns, so the list is
// heap-allocated and handed back as an opaque handle; the managed side reads it through
// rml_ElementList_* and releases it with rml_ElementList_Free. The previous implementation
// returned a pointer to a stack local, which the caller then read after the frame was gone.
RMLUI_CAPI Rml::ElementList* rml_Element_QuerySelectorAll(Rml::Element *element, const char* selector) {
	if (element == nullptr || selector == nullptr) {
		return nullptr;
	}
	auto* list = new Rml::ElementList();
	element->QuerySelectorAll(*list, selector);
	return list;
}

RMLUI_CAPI Rml::ElementList* rml_Element_GetElementsByTagName(Rml::Element *element, const char* tag) {
	if (element == nullptr || tag == nullptr) {
		return nullptr;
	}
	auto* list = new Rml::ElementList();
	element->GetElementsByTagName(*list, tag);
	return list;
}

RMLUI_CAPI Rml::ElementList* rml_Element_GetElementsByClassName(Rml::Element *element, const char* class_name) {
	if (element == nullptr || class_name == nullptr) {
		return nullptr;
	}
	auto* list = new Rml::ElementList();
	element->GetElementsByClassName(*list, class_name);
	return list;
}

RMLUI_CAPI int rml_ElementList_Size(Rml::ElementList* list) {
	return list == nullptr ? 0 : static_cast<int>(list->size());
}

RMLUI_CAPI Rml::Element* rml_ElementList_Get(Rml::ElementList* list, int index) {
	if (list == nullptr || index < 0 || index >= static_cast<int>(list->size())) {
		return nullptr;
	}
	return (*list)[static_cast<size_t>(index)];
}

RMLUI_CAPI void rml_ElementList_Free(Rml::ElementList* list) {
	delete list;
}

RMLUI_CAPI bool rml_Element_Matches(Rml::Element *element, const char* selector) {
	return element->Matches(selector);
}

RMLUI_CAPI void rml_Element_RemoveAttribute(Rml::Element *element, const char* attr) {
	element->RemoveAttribute(attr);
}

RMLUI_CAPI void rml_Element_SetInnerText(Rml::Element *element, const char* text) {
    if (element == nullptr || text == nullptr) {
        return;
    }

    // Drop every child, not just text nodes: the contract is "the element's children become
    // a single text node". Iterating downwards keeps the indices of the children still to be
    // visited valid after each removal.
    for (int i = element->GetNumChildren() - 1; i >= 0; --i) {
        element->RemoveChild(element->GetChild(i));
    }

    Rml::ElementDocument* document = element->GetOwnerDocument();
    if (document == nullptr) {
        return;
    }

    element->AppendChild(document->CreateTextNode(text));
}

RMLUI_CAPI void rml_Element_ScrollIntoView(Rml::Element *element) {
	element->ScrollIntoView();
}

RMLUI_CAPI void rml_Element_ScrollTo(Rml::Element *element, float x, float y, Rml::ScrollBehavior behavior) {
	element->ScrollTo(Rml::Vector2f(x, y), behavior);
}

RMLUI_CAPI const Rml::Box* rml_Element_GetBox(Rml::Element *element) {
    return &(element->GetBox());
}

RMLUI_CAPI Rml::RenderManager* rml_Element_GetRenderManager(Rml::Element *element) {
	return element->GetRenderManager();
}

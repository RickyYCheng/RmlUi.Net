#include "RmlNative.h"
#include "RmlUi/Core.h"
#include "RmlUi/Core/Property.h"
#include <string>

RMLUI_CAPI Rml::Variant* rml_Property_Get(Rml::Property* property) {
  if (property == nullptr) {
    return nullptr;
  }

  return &property->value;
}

RMLUI_CAPI const char* rml_Property_GetString(Rml::Property* property, const char *default_value) {
  if (property == nullptr) {
    return RMLUI_DUP_STR(default_value != nullptr ? default_value : "");
  }

  // Property::ToString keeps the unit (e.g. "50px") unlike the raw variant string.
  return RMLUI_DUP_STR(property->ToString().c_str());
}
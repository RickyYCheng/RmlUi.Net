#include "RmlNative.h"
#include "RmlUi/Core.h"
#include "RmlUi/Core/StyleSheetSpecification.h"
#include "Pool.h"

RMLUI_CAPI Rml::PropertyDefinition* rml_StyleSheetSpecification_RegisterProperty(const char* property_name, const char* default_value, bool inherited, bool forces_layout) {
  if (property_name == nullptr || default_value == nullptr) {
    return nullptr;
  }

  return &Rml::StyleSheetSpecification::RegisterProperty(property_name, default_value, inherited, forces_layout);
}
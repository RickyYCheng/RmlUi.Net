#include "RmlNative.h"
#include "RmlUi/Core.h"
#include "RmlUi/Core/StyleSheetSpecification.h"
#include <string>

RMLUI_CAPI Rml::PropertyDefinition* rml_PropertyDefinition_AddParser(Rml::PropertyDefinition* property_definition, const char* parser_name, const char* parser_params) {
  if (property_definition == nullptr || parser_name == nullptr || parser_params == nullptr) {
    return nullptr;
  }

  return &property_definition->AddParser(parser_name, parser_params);
}

RMLUI_CAPI Rml::PropertyId rml_PropertyDefinition_GetId(Rml::PropertyDefinition* property_definition) {
  if (property_definition == nullptr) {
    return Rml::PropertyId::Invalid;
  }

  return property_definition->GetId();
}

RMLUI_CAPI const char* rml_PropertyDefinition_GetValueString(Rml::PropertyDefinition* property_definition, Rml::Property* property, const char* default_value) {
  if (property_definition == nullptr || property == nullptr) {
    return RMLUI_DUP_STR(default_value != nullptr ? default_value : "");
  }

  auto str = new Rml::String();
  if (property_definition->GetValue(*str, *property)) {
    return RMLUI_DUP_STR(str->c_str());
  }
  return RMLUI_DUP_STR(default_value != nullptr ? default_value : "");
}
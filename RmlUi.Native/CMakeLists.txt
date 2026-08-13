cmake_minimum_required(VERSION 3.14...3.23)

project(RmlUiNative VERSION 1.0 LANGUAGES CXX)

# Keep build artifacts inside the build tree so that multiple configurations
# (x86/x64, Debug/Release) never overwrite each other. Every artifact type lands
# in the same bin/ directory on every platform for uniform CI handling.
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)

if(WIN32)
    set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded")
endif()

# RmlUi and freetype are built as static libraries and linked into the shared
# RmlUiNative library: position-independent code is required on Linux/macOS.
set(CMAKE_POSITION_INDEPENDENT_CODE ON)

# add external libraries
set(BUILD_SHARED_LIBS OFF)
add_subdirectory("ext/freetype")

# Expose the vendored freetype build to RmlUi's find_package(Freetype) call.
if(NOT TARGET Freetype::Freetype)
    add_library(Freetype::Freetype ALIAS freetype)
endif()
set(FREETYPE_INCLUDE_DIR_ft2build "${CMAKE_CURRENT_SOURCE_DIR}/ext/freetype/include" CACHE PATH "Path to ft2build.h" FORCE)
set(FREETYPE_INCLUDE_DIR_freetype2 "${CMAKE_CURRENT_SOURCE_DIR}/ext/freetype/include" CACHE PATH "Path to freetype include dir" FORCE)
if(MSVC)
    if(CMAKE_BUILD_TYPE STREQUAL "Debug")
        set(FREETYPE_LIBRARY "${CMAKE_BINARY_DIR}/bin/freetyped.lib" CACHE FILEPATH "Path to freetype library" FORCE)
    else()
        set(FREETYPE_LIBRARY "${CMAKE_BINARY_DIR}/bin/freetype.lib" CACHE FILEPATH "Path to freetype library" FORCE)
    endif()
elseif(WIN32)
    set(FREETYPE_LIBRARY "${CMAKE_BINARY_DIR}/bin/freetype.lib" CACHE FILEPATH "Path to freetype library" FORCE)
else()
    set(FREETYPE_LIBRARY "${CMAKE_BINARY_DIR}/bin/libfreetype.a" CACHE FILEPATH "Path to freetype library" FORCE)
endif()

add_subdirectory("ext/RmlUi")

add_library(${PROJECT_NAME} SHARED
    "src/Context.cpp"

    "src/Element.cpp"
    "src/ElementDocument.cpp"
    "src/ElementFormControlInput.cpp"
    "src/ElementCustom.cpp"
    "src/ElementInstancer.cpp"

    "src/Variant.cpp"
    "src/VariableDefinition.cpp"
    "src/StructHandle.cpp"
    "src/Dictionary.cpp"
    "src/DataVariable.cpp"
    "src/DataModelHandle.cpp"
    "src/DataModelConstructor.cpp"
    "src/Event.cpp"
    "src/EventInstancer.cpp"
    "src/EventListener.h"
    "src/EventListenerInstancer.cpp"

    "src/Box.cpp"
    "src/RenderInterface.cpp"
    "src/SystemInterface.cpp"
    "src/FileInterface.cpp"
    "src/Plugin.cpp"

    "src/StyleSheetContainer.cpp"
    "src/StyleSheetSpecification.cpp"
    
    "src/XMLAttributes.cpp"
    "src/PropertyDefinition.cpp"
    "src/Property.cpp"
    "src/Rml.cpp"
    "src/Debugger.cpp"
    "src/Util.h"
    "src/Buffer.cpp"
    "include/Buffer.h"
)

include_directories(${PROJECT_NAME}
    PRIVATE include
)

target_compile_definitions(${PROJECT_NAME} PUBLIC "RMLUI_STATIC_LIB")
target_compile_features(${PROJECT_NAME} PRIVATE cxx_std_17)

target_link_libraries(${PROJECT_NAME}
    freetype
    RmlUi::Core
    RmlUi::Debugger
)

# Keep the expected binary names for the .NET runtime on each platform:
#   Windows: RmlUiNative.dll
#   Linux:   libRmlUiNative.so
#   macOS:   RmlUiNative.dylib
if(APPLE)
    set_target_properties(${PROJECT_NAME} PROPERTIES PREFIX "")
endif()

# Warnings as errors for the binding sources only (vendored libraries are built with their own settings).
if(MSVC)
    target_compile_options(${PROJECT_NAME} PRIVATE /W4 /WX)
else()
    target_compile_options(${PROJECT_NAME} PRIVATE -Wall -Wextra -Werror)
endif()

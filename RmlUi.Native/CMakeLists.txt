cmake_minimum_required(VERSION 3.21...3.31)

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
# FreeType is vendored and linked statically into RmlUiNative. Disable its
# optional system dependencies: on CI runners they may be absent, and on macOS
# the Homebrew builds are arm64-only and cannot satisfy the x86_64 slice of our
# universal binary. FreeType uses its bundled zlib for compressed fonts.
# The options must be seeded as CACHE variables with FORCE: freetype's own
# cmake_minimum_required predates CMake 3.13, so policy CMP0077 is OLD there
# and its option() calls wipe plain normal variables.
set(FT_DISABLE_PNG ON CACHE BOOL "Disable PNG support" FORCE)
set(FT_DISABLE_HARFBUZZ ON CACHE BOOL "Disable HarfBuzz" FORCE)
set(FT_DISABLE_BROTLI ON CACHE BOOL "Disable Brotli" FORCE)
set(FT_DISABLE_BZIP2 ON CACHE BOOL "Disable bzip2" FORCE)
set(FT_DISABLE_ZLIB ON CACHE BOOL "Use FreeType's bundled zlib" FORCE)
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

# Apply the patch in the build tree, not in the shared submodule checkout.
find_package(Git REQUIRED)

set(RMLUI_SOURCE "${CMAKE_CURRENT_BINARY_DIR}/rmlui-source")
file(COPY "${CMAKE_CURRENT_SOURCE_DIR}/ext/RmlUi/" DESTINATION "${RMLUI_SOURCE}" PATTERN ".git" EXCLUDE)
file(GLOB RMLUI_PATCHES CONFIGURE_DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/patches/*.patch")
list(SORT RMLUI_PATCHES)
# Read the pinned version so older working copies already patched in place
# still produce the same source. Only files touched by patches are replaced.
set(RMLUI_PATCHED_PATH "Source/Core/Elements/WidgetTextInput.cpp")
execute_process(COMMAND "${GIT_EXECUTABLE}" show "HEAD:${RMLUI_PATCHED_PATH}"
    WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/ext/RmlUi"
    OUTPUT_FILE "${RMLUI_SOURCE}/${RMLUI_PATCHED_PATH}" COMMAND_ERROR_IS_FATAL ANY)
foreach(rmlui_patch IN LISTS RMLUI_PATCHES)
    execute_process(
        COMMAND "${CMAKE_COMMAND}" -E env "GIT_DIR=${RMLUI_SOURCE}/.patch-only" "${GIT_EXECUTABLE}" apply --ignore-space-change "${rmlui_patch}"
        WORKING_DIRECTORY "${RMLUI_SOURCE}" COMMAND_ERROR_IS_FATAL ANY)
endforeach()

add_subdirectory("${RMLUI_SOURCE}" "${CMAKE_CURRENT_BINARY_DIR}/rmlui")

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

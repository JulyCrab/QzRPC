#!/bin/bash
# QzRPC Build Script (Linux/macOS)
#   ./build.sh            -> Windows + macOS
#   ./build.sh --win      -> Windows only -> dist/QzRPC.exe
#   ./build.sh --mac      -> macOS only   -> dist/QzRPC-macos.zip
#   ./build.sh --clean    -> clean first (combine with --win / --mac)

CONFIG="Release"
CLEAN=false
MAC=false
WIN=false

# parse args
while [[ $# -gt 0 ]]; do
    case $1 in
        --clean)
            CLEAN=true
            shift
            ;;
        --debug)
            CONFIG="Debug"
            shift
            ;;
        --mac)
            MAC=true
            shift
            ;;
        --win)
            WIN=true
            shift
            ;;
        *)
            shift
            ;;
    esac
done

echo "=== QzRPC Build Script ==="
echo ""

# load config (portable: works with BSD grep on macOS and GNU grep on Linux)
VERSION=$(grep '"version"' build-config.json | head -1 | sed -E 's/.*"version"[[:space:]]*:[[:space:]]*"([^"]+)".*/\1/')

# pick targets (no platform flag = build both)
TARGETS=""
if [ "$WIN" = true ] || { [ "$MAC" = false ] && [ "$WIN" = false ]; }; then TARGETS="$TARGETS win-x64"; fi
if [ "$MAC" = true ] || { [ "$MAC" = false ] && [ "$WIN" = false ]; }; then TARGETS="$TARGETS osx-x64"; fi

# clean if requested
if [ "$CLEAN" = true ]; then
    echo "Cleaning previous builds..."
    rm -rf bin obj dist
    echo "Clean complete!"
    echo ""
fi

package_mac() {
    # assemble the .app bundle (icon is the prebuilt build/macos/AppIcon.icns)
    bash build/macos/make-app-bundle.sh dist/QzRPC dist

    # zip it preserving the executable bit (ditto on macOS, zip elsewhere)
    rm -f dist/QzRPC-macos.zip
    if command -v ditto >/dev/null 2>&1; then
        ( cd dist && ditto -c -k --keepParent QzRPC.app QzRPC-macos.zip )
    elif command -v zip >/dev/null 2>&1; then
        ( cd dist && zip -ry QzRPC-macos.zip QzRPC.app >/dev/null )
    fi

    if [ -f dist/QzRPC-macos.zip ]; then
        echo "Output: dist/QzRPC-macos.zip (and unzipped dist/QzRPC.app)"
    else
        echo "Output: dist/QzRPC.app"
    fi
}

# build each requested target
for RT in $TARGETS; do
    echo "Building QzRPC v$VERSION for $RT ($CONFIG)..."
    echo ""

    dotnet publish QzRPC.csproj \
        -c "$CONFIG" \
        -r "$RT" \
        -p:PublishSingleFile=true \
        -p:SelfContained=true \
        -p:PublishTrimmed=false \
        -p:EnableCompressionInSingleFile=true \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        -p:Version="$VERSION" \
        --output dist

    if [ $? -ne 0 ]; then
        echo ""
        echo "=== Build Failed ($RT) ==="
        exit 1
    fi

    echo ""
    echo "=== $RT Build Successful! ==="
    if [ "$RT" = "osx-x64" ]; then
        package_mac
    else
        echo "Output: dist/QzRPC.exe"
        if [ -f "dist/QzRPC.exe" ]; then
            echo "File Size: $(du -h dist/QzRPC.exe | cut -f1)"
        fi
    fi
    echo ""
done

echo "=== All builds complete ==="

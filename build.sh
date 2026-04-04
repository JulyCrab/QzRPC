#!/bin/bash
# QzRPC Build Script (Linux/Mac)

CONFIG="Release"
CLEAN=false

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
        *)
            shift
            ;;
    esac
done

echo "=== QzRPC Build Script ==="
echo ""

# load config
VERSION=$(grep -Po '"version":\s*"\K[^"]*' build-config.json)
RUNTIME="win-x64"

# clean if requested
if [ "$CLEAN" = true ]; then
    echo "Cleaning previous builds..."
    rm -rf bin obj dist
    echo "Clean complete!"
    echo ""
fi

# build
echo "Building QzRPC v$VERSION..."
echo "Configuration: $CONFIG"
echo "Runtime: $RUNTIME"
echo ""

dotnet publish QzRPC.csproj \
    -c "$CONFIG" \
    -r "$RUNTIME" \
    -p:PublishSingleFile=true \
    -p:SelfContained=true \
    -p:PublishTrimmed=false \
    -p:EnableCompressionInSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:Version="$VERSION" \
    --output dist

if [ $? -eq 0 ]; then
    echo ""
    echo "=== Build Successful! ==="
    echo ""
    echo "Output: dist/QzRPC.exe"
    
    if [ -f "dist/QzRPC.exe" ]; then
        SIZE=$(du -h dist/QzRPC.exe | cut -f1)
        echo "File Size: $SIZE"
    fi
else
    echo ""
    echo "=== Build Failed ==="
    exit 1
fi

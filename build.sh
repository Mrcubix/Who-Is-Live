#!/bin/bash

## Build

# Windows

dotnet publish ./WhoIsLive.UX.Desktop/ -c Release -r win-x64 -o build/win-x64 --no-self-contained
dotnet publish ./WhoIsLive.UX.Desktop/ -c Release -r win-x86 -o build/win-x86 --no-self-contained
dotnet publish ./WhoIsLive.UX.Desktop/ -c Release -r win-arm64 -o build/win-arm64 --no-self-contained

# Linux

dotnet publish ./WhoIsLive.UX.Desktop/ -c Release -r linux-x64 -o build/linux-x64 --no-self-contained
dotnet publish ./WhoIsLive.UX.Desktop/ -c Release -r linux-arm -o build/linux-arm --no-self-contained
dotnet publish ./WhoIsLive.UX.Desktop/ -c Release -r linux-arm64 -o build/linux-arm64 --no-self-contained

# Mac

dotnet publish ./WhoIsLive.UX.Desktop/ -c Release -r osx-x64 -o build/osx-x64 --no-self-contained
dotnet publish ./WhoIsLive.UX.Desktop/ -c Release -r osx-arm64 -o build/osx-arm64 --no-self-contained

## Zip

# Windows

zip -r build/WhoIsLive-win-x64.zip build/win-x64/*
zip -r build/WhoIsLive-win-x86.zip build/win-x86/*
zip -r build/WhoIsLive-win-arm64.zip build/win-arm64/*

# Linux

zip -r build/WhoIsLive-linux-x64.zip build/linux-x64/*
zip -r build/WhoIsLive-linux-arm.zip build/linux-arm/*
zip -r build/WhoIsLive-linux-arm64.zip build/linux-arm64/*

# Mac

zip -r build/WhoIsLive-osx-x64.zip build/osx-x64/*
zip -r build/WhoIsLive-osx-arm64.zip build/osx-arm64/*

# Re-create hashes.txt
> "./build/hashes.txt"

# Append all hashes to hashes.txt
(
    cd ./build

    echo "" >> hashes.txt

    # Compute all UX Hashes
    for os in win linux osx; do
        for arch in x64 x86 arm64; do

            name="WhoIsLive-$os-$arch.zip"

            if [ -f "$name" ]; then
                echo "Computing $name"
                sha256sum $name >> "hashes.txt"
            fi
        done
    done
)
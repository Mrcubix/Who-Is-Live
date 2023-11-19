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

Compress-Archive -Path build/win-x64/* -DestinationPath build/WhoIsLive-win-x64.zip -Update
Compress-Archive -Path build/win-x86/* -DestinationPath build/WhoIsLive-win-x86.zip -Update
Compress-Archive -Path build/win-arm64/* -DestinationPath build/WhoIsLive-win-arm64.zip -Update

# Linux

Compress-Archive -Path build/linux-x64/* -DestinationPath build/WhoIsLive-linux-x64.zip -Update
Compress-Archive -Path build/linux-arm/* -DestinationPath build/WhoIsLive-linux-arm.zip -Update
Compress-Archive -Path build/linux-arm64/* -DestinationPath build/WhoIsLive-linux-arm64.zip -Update

# Mac

Compress-Archive -Path build/osx-x64/* -DestinationPath build/WhoIsLive-osx-x64.zip -Update
Compress-Archive -Path build/osx-arm64/* -DestinationPath build/WhoIsLive-osx-arm64.zip -Update
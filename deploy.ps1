# Publishing Native AOT

Set-Location (Join-Path $PSScriptRoot "Parameter")

dotnet publish `
	-c Release `
	-r win-x64 `
	-p:PublishAot=true `
	-p:DebugSymbols=false `
	-p:DebugType=None `
	-o ./bin/native/win-x64

dotnet publish `
	-c Release `
	-r win-x86 `
	-p:PublishAot=true `
	-p:DebugSymbols=false `
	-p:DebugType=None `
	-o ./bin/native/win-x86

# Publishing Framework dependent

dotnet publish `
	-c Release `
	-r win-x64 `
	-p:DebugSymbols=false `
	-p:DebugType=None `
	-p:PublishSingleFile=true `
	--self-contained false `
	-p:IncludeNativeLibrariesForSelfExtract=true

dotnet publish `
	-c Release `
	-r win-x86 `
	-p:DebugSymbols=false `
	-p:DebugType=None `
	-p:PublishSingleFile=true `
	--self-contained false `
	-p:IncludeNativeLibrariesForSelfExtract=true
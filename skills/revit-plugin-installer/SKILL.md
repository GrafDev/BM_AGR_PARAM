---
name: revit-plugin-installer
description: Guide for creating .exe installers for Revit plugins using Inno Setup. Includes standardized build scripts and installer configuration. Use when a plugin is ready for distribution and needs a professional installer.
---

# Revit Plugin Installer (.exe)

This skill provides the standard "BuroMoscow" workflow for packaging Revit plugins into executable installers.

## 1. Prerequisites
- **Inno Setup 6** or later installed.
- **dotnet CLI** or MSBuild for compilation.

## 2. Build Script (`build_installer.bat`)

Create this file in your project root to automate the process:

```batch
@echo off
set VERSION_FILE=version.txt
set ISS_FILE=installer.iss
set INNO="C:\Path\To\iscc.exe"

:: Read version
set /p VERSION=<%VERSION_FILE%
set VERSION=%VERSION: =%

:: Build Release
dotnet build -c Release

:: Compile Installer
%INNO% /DAppVersion="%VERSION%" "%ISS_FILE%"
```

## 3. Inno Setup Config (`installer.iss`)

Use the following template for `installer.iss`. It handles registry creation and `.addin` file generation.

```pascal
#define AppName      "My Plugin Name"
#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif
#define AddinTarget  "{userappdata}\Autodesk\Revit\Addins\2023"

[Setup]
AppId={{GENERATE-NEW-GUID}}
AppName={#AppName}
AppVersion={#AppVersion}
DefaultDirName={#AddinTarget}
OutputDir=Build\Installer
OutputBaseFilename=MyPlugin_v{#AppVersion}_Setup
PrivilegesRequired=lowest

[Files]
Source: "bin\Release\MyPlugin.dll"; DestDir: "{app}"; Flags: ignoreversion

[Code]
procedure CurStepChanged(CurStep: TSetupStep);
var Lines : TArrayOfString;
begin
  if CurStep <> ssPostInstall then Exit;
  // Generate .addin file logic here...
end;
```

## 4. Key Priorities
1. **Per-User Install**: Target `{userappdata}` to avoid requiring admin rights.
2. **Pathing**: Ensure the `.addin` file correctly points to the installed DLL path.
3. **Naming**: Use the `PluginName_vX.X.X_Setup.exe` format for distribution files.

; ============================================================
;  BM_AGR_PARAM Inno Setup Script
;  SEAMLESS HOT-RELOAD - Version 5.2.1
; ============================================================

#define AppName      "BM AGR Parameter Manager"
#ifndef AppVersion
  #define AppVersion "5.2.1"
#endif
#define AppPublisher "BuroMoscow"
#define AddinTarget  "{userappdata}\Autodesk\Revit\Addins\2023"
#define BuildDir      SourcePath + "bin\Release"

[Setup]
AppId={{C3D4E5F6-0123-4567-89AB-CDEF01234567}
AppName={#AppName}
AppVersion={#AppVersion}
DefaultDirName={#AddinTarget}
OutputDir={#SourcePath}Build\Installer
OutputBaseFilename=BM_AGR_PARAM_v{#AppVersion}_Setup
Compression=lzma2
SolidCompression=yes
ArchitecturesAllowed=x64compatible
PrivilegesRequired=lowest

; ГОРЯЧЕЕ ОБНОВЛЕНИЕ БЕЗ ОШИБОК
CloseApplications=no

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Files]
; Загрузчик: если занят - просто планируем замену (без ошибки), логика все равно обновится
Source: "{#BuildDir}\BM_AGR_Loader.dll"; DestDir: "{app}"; Flags: ignoreversion restartreplace uninsrestartdelete
; Логика: обновляется всегда мгновенно
Source: "{#BuildDir}\BM_AGR_PARAM.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\version.txt"; DestDir: "{app}"; Flags: ignoreversion

[Code]
procedure CurStepChanged(TSetupStep: TSetupStep);
var
  AddinFile : String;
  DllPath   : String;
  Lines     : TArrayOfString;
begin
  if TSetupStep <> ssPostInstall then Exit;

  AddinFile := ExpandConstant('{app}\BM_AGR_PARAM.addin');
  DllPath := ExpandConstant('{app}\BM_AGR_Loader.dll');

  if not FileExists(AddinFile) then
  begin
    SetArrayLength(Lines, 11);
    Lines[0] := '<?xml version="1.0" encoding="utf-8"?>';
    Lines[1] := '<RevitAddIns>';
    Lines[2] := '  <AddIn Type="Application">';
    Lines[3] := '    <Name>BM AGR Parameter Manager</Name>';
    Lines[4] := '    <Assembly>' + DllPath + '</Assembly>';
    Lines[5] := '    <AddInId>A1B2C3D4-E5F6-4789-BCDE-F0123456789A</AddInId>';
    Lines[6] := '    <FullClassName>BM_AGR_Loader.LoaderApp</FullClassName>';
    Lines[7] := '    <VendorId>BM</VendorId>';
    Lines[8] := '    <VendorDescription>BuroMoscow</VendorDescription>';
    Lines[9] := '  </AddIn>';
    Lines[10] := '</RevitAddIns>';
    SaveStringsToFile(AddinFile, Lines, False);
  end;
end;

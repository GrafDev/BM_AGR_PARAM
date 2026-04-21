; ============================================================
;  BM_AGR_PARAM Inno Setup Script
;  SEAMLESS HOT-RELOAD - Version 5.2.0
; ============================================================

#define AppName      "BM AGR Parameter Manager"
#ifndef AppVersion
  #define AppVersion "5.2.0"
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

; ПОЛНОСТЬЮ БЕСШОВНЫЙ РЕЖИМ
CloseApplications=no

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Files]
; Загрузчик: ставим только если он не заблокирован (т.е. Ревит закрыт или его еще нет)
Source: "{#BuildDir}\BM_AGR_Loader.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CanInstallLoader
; Логика: ставим ВСЕГДА (она никогда не блокируется)
Source: "{#BuildDir}\BM_AGR_PARAM.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\version.txt"; DestDir: "{app}"; Flags: ignoreversion

[Code]
// Проверка: можно ли обновить лоадер (не занят ли он Ревитом)
function CanInstallLoader(): Boolean;
var
  FileName: String;
  FileHandle: Longint;
begin
  FileName := ExpandConstant('{app}\BM_AGR_Loader.dll');
  if not FileExists(FileName) then
  begin
    Result := True;
    Exit;
  end;

  // Пытаемся открыть файл для записи. Если не вышло - значит он заблокирован.
  FileHandle := FileOpen(FileName, fmOpenWrite or fmShareDenyWrite);
  if FileHandle <> -1 then
  begin
    FileClose(FileHandle);
    Result := True;
  end
  else
  begin
    Log('Loader is locked, skipping update. This is normal for Hot-Reload.');
    Result := False;
  end;
end;

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

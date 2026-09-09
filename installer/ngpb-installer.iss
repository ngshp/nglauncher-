; ngpb-installer.iss - NGPB Launcher Installer
#define MyAppName "NGPB Launcher"
#define MyAppVersion "1.2.0"
#define MyAppPublisher "NGSH"

[Setup]
AppName={#MyAppName}
AppVersion={#MyAppVersion}
DefaultDirName={pf}\{#MyAppName}
OutputDir={#SourceFolder}\..\InstallerOutput
OutputBaseFilename=NGPB-Launcher-v1.2.0-Setup
Compression=lzma2/max
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64

[Files]
; PERBAIKAN: Gunakan SourceFolder yang dikirim dari workflow
Source: "{#SourceFolder}\NgpbLauncher.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceFolder}\*"; DestDir: "{app}"; Excludes: "*.pdb,*.xml"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\NgpbLauncher.exe"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\NgpbLauncher.exe"

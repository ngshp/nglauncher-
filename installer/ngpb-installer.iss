; ngpb-installer.iss - NGPB Launcher Installer (CI Optimized)
#define MyAppName "NGPB Launcher"
#define MyAppVersion "1.2.0"
#define MyAppPublisher "NGSH"

[Setup]
AppName={#MyAppName}
AppVersion={#MyAppVersion}
DefaultDirName={pf}\{#MyAppName}
; HAPUS OutputDir di sini, biarkan dikontrol penuh oleh parameter /O dari workflow
; OutputDir=... 
OutputBaseFilename=NGPB-Launcher-v1.2.0-Setup
Compression=lzma2/max
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin

[Files]
; Copy executable utama
Source: "{#SourceFolder}\NgpbLauncher.exe"; DestDir: "{app}"; Flags: ignoreversion
; Copy semua file pendukung kecuali debug symbols
Source: "{#SourceFolder}\*"; DestDir: "{app}"; Excludes: "*.pdb,*.xml,*.config"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\NgpbLauncher.exe"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\NgpbLauncher.exe"

[Run]
; Opsional: Jalankan aplikasi setelah install jika user memilih
; Filename: "{app}\NgpbLauncher.exe"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

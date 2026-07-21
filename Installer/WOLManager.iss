; Skrypt instalatora Inno Setup dla WOLManager (Wake On LAN Manager)
; Kompilacja:  "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" WOLManager.iss

#define MyAppName "WOL Manager"
; Must be the full release number: it drives the ARP DisplayVersion and the
; installer filename. Keeping it short (e.g. "1.1" for release v1.1.1) made the
; installed version disagree with the package version and produced two different
; releases carrying an identically named WOLManager-Setup-1.1.exe.
#define MyAppVersion "1.1.2"
#define MyAppPublisher "tmfgroup"
#define MyAppExeName "WOLManager.exe"

[Setup]
; Unikalny identyfikator aplikacji (nie zmieniać przy aktualizacjach)
AppId={{8C58025E-20F7-4AAC-A2F3-9619A113089D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
; Wymagane uprawnienia administratora (instalacja do Program Files)
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

; Code signing - ACTIVE only when ISCC is invoked with /DSIGN (private Installer\build.ps1,
; which defines the "wolmanagersign" tool via /S and signs with the tmfgroup cert). Without
; /DSIGN (e.g. anyone building from the repo) the installer is produced UNSIGNED - by design.
#ifdef SIGN
SignTool=wolmanagersign
SignedUninstaller=yes
#endif

; --- Ikony ---
; Ikona pliku instalatora (setup.exe)
SetupIconFile=..\ico.ico
; Ikona widoczna w "Programy i funkcje" / Panelu sterowania (odinstalowanie)
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}

; --- Wyjście ---
OutputDir=.\Output
OutputBaseFilename=WOLManager-Setup-{#MyAppVersion}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "polish"; MessagesFile: "compiler:Languages\Polish.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
; Checked by default, so silent installs (winget) create the desktop icon too.
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "..\publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
; Ikona kopiowana do folderu aplikacji (dla skrótów)
Source: "..\ico.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
; Skrót w menu Start
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\ico.ico"
; Skrót do odinstalowania w menu Start
Name: "{group}\Odinstaluj {#MyAppName}"; Filename: "{uninstallexe}"
; Skrót na pulpicie (opcjonalny)
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\ico.ico"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

; Skrypt instalatora Inno Setup dla WOLMenager (Wake On LAN Manager)
; Kompilacja:  "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" WOLMenager.iss

#define MyAppName "WOL Menager"
#define MyAppVersion "1.0"
#define MyAppPublisher "tmfgroup"
#define MyAppExeName "WOLMenager.exe"

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

; --- Ikony ---
; Ikona pliku instalatora (setup.exe)
SetupIconFile=..\ico.ico
; Ikona widoczna w "Programy i funkcje" / Panelu sterowania (odinstalowanie)
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}

; --- Wyjście ---
OutputDir=.\Output
OutputBaseFilename=WOLMenager-Setup-{#MyAppVersion}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "polish"; MessagesFile: "compiler:Languages\Polish.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

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

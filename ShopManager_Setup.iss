; Inno Setup Script for Shop Manager
; This script packages ShopManager.App.exe and MariaDB MSI installer together.

[Setup]
AppName=Shop Manager
AppVersion=1.0
AppPublisher=Shop Manager
DefaultDirName={autopf}\ShopManager
DefaultGroupName=Shop Manager
AllowNoIcons=yes
OutputDir=setup_output
OutputBaseFilename=ShopManager_Setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; 1. Copy the published self-contained WPF App
Source: "src\ShopManager.App\bin\Release\net9.0-windows\win-x64\publish\ShopManager.App.exe"; DestDir: "{app}"; Flags: ignoreversion

; 2. Copy the MariaDB MSI Installer (User needs to download this and place it next to this .iss file)
Source: "mariadb-10.11.8-winx64.msi"; DestDir: "{tmp}"; Flags: deleteafterinstall

[Icons]
Name: "{group}\Shop Manager"; Filename: "{app}\ShopManager.App.exe"
Name: "{autodesktop}\Shop Manager"; Filename: "{app}\ShopManager.App.exe"; Tasks: desktopicon

[Run]
; 3. Run MariaDB MSI Installer silently in the background
; This registers a Windows Service named "MySQL" running on port 3306 with password "02062003"
Filename: "msiexec.exe"; Parameters: "/i ""{tmp}\mariadb-10.11.8-winx64.msi"" /qn SERVICENAME=""MySQL"" PORT=3306 PASSWORD=""02062003"""; StatusMsg: "Đang tự động cài đặt và cấu hình cơ sở dữ liệu ngầm (vui lòng chờ)..."; Flags: runhidden

; 4. Run the application after setup finishes
Filename: "{app}\ShopManager.App.exe"; Description: "{cm:LaunchProgram,Shop Manager}"; Flags: postinstall nowait skipifsilent

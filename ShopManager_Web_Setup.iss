; Inno Setup Script for Shop Manager Web
; This script packages ShopManager.Web and MariaDB MSI installer together.

[Setup]
AppName=Shop Manager Web
AppVersion=1.0
AppPublisher=Shop Manager
DefaultDirName={autopf}\ShopManagerWeb
DefaultGroupName=Shop Manager Web
AllowNoIcons=yes
OutputDir=setup_output
OutputBaseFilename=ShopManager_Web_Setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; 1. Copy everything from the Web publish output (includes exe, wwwroot, appsettings)
Source: "src\ShopManager.Web\bin\Release\net10.0\win-x64\publish\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion

; 2. Copy the MariaDB MSI Installer (User needs to download this and place it next to this .iss file)
Source: "mariadb-10.11.8-winx64.msi"; DestDir: "{tmp}"; Flags: deleteafterinstall

[Icons]
Name: "{group}\Shop Manager Web"; Filename: "{app}\ShopManager.Web.exe"
Name: "{autodesktop}\Shop Manager Web"; Filename: "{app}\ShopManager.Web.exe"; Tasks: desktopicon

[Run]
; 3. Run MariaDB MSI Installer silently in the background
Filename: "msiexec.exe"; Parameters: "/i ""{tmp}\mariadb-10.11.8-winx64.msi"" /qn SERVICENAME=""MySQL"" PORT=3306 PASSWORD=""02062003"""; StatusMsg: "Đang tự động cài đặt và cấu hình cơ sở dữ liệu ngầm (vui lòng chờ)..."; Flags: runhidden

; 4. Run the Web Server application in the background
Filename: "{app}\ShopManager.Web.exe"; Description: "Khởi chạy máy chủ Shop Manager Web"; Flags: postinstall nowait skipifsilent

; 5. Automatically open the browser to the web app URL
Filename: "http://localhost:5000"; Flags: postinstall shellexec nowait; Description: "Mở trang quản trị bán hàng trên trình duyệt"

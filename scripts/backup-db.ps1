# MySQL Database Backup Script for Fashion Store POS
# Usage: powershell.exe -File scripts/backup-db.ps1

$DbName = "main"
$DbUser = "root"
$DbPass = "" # Set password if applicable
$BackupDir = "C:\FashionStoreBackups"
$KeepDays = 7

# Ensure backup directory exists
if (-not (Test-Path $BackupDir)) {
    New-Item -ItemType Directory -Force -Path $BackupDir | Out-Null
}

$Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$BackupFile = Join-Path $BackupDir "$($DbName)_$($Timestamp).sql"
$LogFile = Join-Path $BackupDir "backup_log.txt"

Write-Host "Starting backup of database '$DbName'..." -ForegroundColor Cyan

# Check if mysqldump is available
if (-not (Get-Command "mysqldump" -ErrorAction SilentlyContinue)) {
    $ErrMsg = "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] ERROR: mysqldump not found in PATH.`n"
    $ErrMsg | Out-File $LogFile -Append
    Write-Host "ERROR: mysqldump not found in system PATH. Cannot perform backup." -ForegroundColor Red
    exit 1
}

try {
    # Run mysqldump
    $DumpArgs = "--user=$DbUser --result-file=$BackupFile $DbName"
    if ($DbPass -ne "") { $DumpArgs = "--password=$DbPass $DumpArgs" }
    
    Start-Process "mysqldump" -ArgumentList $DumpArgs -NoNewWindow -Wait
    
    if (Test-Path $BackupFile) {
        $Msg = "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] SUCCESS: Backup created at $BackupFile`n"
        $Msg | Out-File $LogFile -Append
        Write-Host "✅ Backup successfully created at $BackupFile" -ForegroundColor Green
    } else {
        throw "mysqldump failed to create file."
    }
} catch {
    $ErrMsg = "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] ERROR: Backup failed: $_.Exception.Message`n"
    $ErrMsg | Out-File $LogFile -Append
    Write-Host "❌ ERROR: Backup failed. Check log for details." -ForegroundColor Red
}

# Cleanup old backups
Write-Host "Cleaning up backups older than $KeepDays days..." -ForegroundColor Gray
Get-ChildItem $BackupDir -Filter "*.sql" | Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-$KeepDays) } | Remove-Item -Force
Write-Host "Cleanup complete."

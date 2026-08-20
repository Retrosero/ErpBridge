$path = 'C:\Users\retro\Documents\GitHub\ErpBridge\saha-satış\app\src\main\java\com\example\ui\screens\BridgeSyncHelper.kt'
$bytes = [System.IO.File]::ReadAllBytes($path)
$text = [System.Text.Encoding]::UTF8.GetString($bytes)
$lines = $text -split "`r?`n"
for ($i = 53; $i -lt 80; $i++) {
    $line = $lines[$i]
    Write-Host ('{0,5}|{1}' -f $line.Length, $line)
}

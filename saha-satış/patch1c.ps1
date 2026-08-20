# Patch1c: Replace the broken isUnsupportedEndpoint with the working version
$ErrorActionPreference = 'Stop'

$dirName = "saha-sat" + [char]0x131 + [char]0x15F
$path = "C:\Users\retro\Documents\GitHub\ErpBridge\$dirName\app\src\main\java\com\example\ui\screens\BridgeSyncHelper.kt"
if (-not (Test-Path -LiteralPath $path)) { Write-Host "FAIL"; exit 1 }

$bytes = [System.IO.File]::ReadAllBytes($path)
$text = [System.Text.Encoding]::UTF8.GetString($bytes)

$nl = "`n"
$apos = [char]0x2019
$warn = [char]0x26A0

# Build the current (broken) block. Use single-quoted here-strings to avoid PS variable expansion.
$old = @'
    /**
     * Merkezi API bu entity için endpoint sunmuyorsa (404) veya API anahtarının
     * bu uç noktaya erişim yetkisi yoksa (403) sync fonksiyonu bilgilendirici
     * log düşüerek başarıyla dönsün. Tüm sync zincirini kırmasın; sadece o tablo
     * boş kalsın. UI tarafında "bu özellik tenant'ta yok" şeklinde gösterilir.
     */
    private fun isUnsupportedEndpoint(
        response: retrofit2.Response<*>,
        entity: String,
        log: (String) -> Unit
    ): Boolean {
        val code = response.code()
        if (code == 404) {
            log("⚠ '' endpoint’i merkezi API’de mevcut değil (HTTP 404). Bu tablo için sync atlanıyor.")
            return true
        }
        if (code == 403) {
            log("⚠ '' endpoint’ine bu API anahtarıyla erişim yok (HTTP 403). Bu tablo için sync atlanıyor.")
            return true
        }
        return false
    }
    suspend fun syncCariler(
'@

# Verify the old block is present exactly once
$count = ([regex]::Matches($text, [regex]::Escape($old))).Count
Write-Host "Old block count: $count"
if ($count -ne 1) { exit 1 }

# Build the new block. Use simple string concatenation for the log lines so $entity survives
# Construct the warning log line: "⚠ '$entity' endpoint'i ..."
$log404 = '"' + $warn + " '`$entity' endpoint" + $apos + "i merkezi API" + $apos + "de mevcut de"
$c_g = [char]0x11F
$c_c = [char]0xE7
$c_i = [char]0x131
$log404 = $log404 + $c_g + "il (HTTP 404). Bu tablo i" + $c_c + "in sync atlan" + $c_i + "yor.`""
$log403 = '"' + $warn + " '`$entity' endpoint" + $apos + "ine bu API anahtar" + $c_i + "yla eri"
$c_s = [char]0x15F
$log403 = $log403 + $c_s + "im yok (HTTP 403). Bu tablo i" + $c_c + "in sync atlan" + $c_i + "yor.`" + '"'

# Build the rest of the new block via single-quoted here-string
$newBlock = @"

    /**
     * Merkezi API bu entity için endpoint sunmuyorsa (404) veya API anahtarının
     * bu uç noktaya erişim yetkisi yoksa (403) sync fonksiyonu bilgilendirici
     * log düşerek başarıyla dönsün. Tüm sync zincirini kırmasın; sadece o tablo
     * boş kalsın. UI tarafında "bu özellik tenant'ta yok" şeklinde gösterilir.
     */
    private fun isUnsupportedEndpoint(
        response: retrofit2.Response<*>,
        entity: String,
        log: (String) -> Unit
    ): Boolean {
        val code = response.code()
        if (code == 404) {
            log($log404)
            return true
        }
        if (code == 403) {
            log($log403)
            return true
        }
        return false
    }

    suspend fun syncCariler(
"@

# Use IndexOf + Substring
$idx = $text.IndexOf($old)
if ($idx -lt 0) { Write-Host "Old not found"; exit 1 }
$newText = $text.Substring(0, $idx) + $newBlock + $text.Substring($idx + $old.Length)

$utf8 = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllText($path, $newText, $utf8)
Write-Host "OK"

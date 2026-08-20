# Patch1b: Fix the isUnsupportedEndpoint helper - replace broken version with proper one
$ErrorActionPreference = 'Stop'

$dirName = "saha-sat" + [char]0x131 + [char]0x15F
$path = "C:\Users\retro\Documents\GitHub\ErpBridge\$dirName\app\src\main\java\com\example\ui\screens\BridgeSyncHelper.kt"

if (-not (Test-Path -LiteralPath $path)) { Write-Host "FAIL"; exit 1 }

$bytes = [System.IO.File]::ReadAllBytes($path)
$text = [System.Text.Encoding]::UTF8.GetString($bytes)

# Build the broken block (what's currently in the file)
$nl = "`n"
$old = "    }" + $nl +
"    /**" + $nl +
"     * Merkezi API bu entity i" + [char]0xE7 + "in endpoint sunmuyorsa (404) veya API anahtar" + [char]0x131 + "n" + [char]0x131 + "n" + $nl +
"     * bu u" + [char]0xE7 + " noktaya eri" + [char]0x15F + "im yetkisi yoksa (403) sync fonksiyonu bilgilendirici" + $nl +
"     * log d" + [char]0xFC + [char]0x15F + [char]0xFC + "erek ba" + [char]0x15F + "ar" + [char]0x131 + "yla d" + [char]0xF6 + "ns" + [char]0xFC + "n. T" + [char]0xFC + "m sync zincirini k" + [char]0x131 + "rmas" + [char]0x131 + "n; sadece o tablo" + $nl +
"     * bo" + [char]0x15F + " kals" + [char]0x131 + "n. UI taraf" + [char]0x131 + "nda `" + "bu " + [char]0xF6 + "zellik tenant'ta yok`" + " " + [char]0x15F + "eklinde g" + [char]0xF6 + "sterilir." + $nl +
"     */" + $nl +
"    private fun isUnsupportedEndpoint(" + $nl +
"        response: retrofit2.Response<*>," + $nl +
"        entity: String," + $nl +
"        log: (String) -> Unit" + $nl +
"    ): Boolean {" + $nl +
"        val code = response.code()" + $nl +
"        if (code == 404) {" + $nl +
"            log(`"` + [char]0x26A0 + " '' endpoint" + [char]0x2019 + "i merkezi API" + [char]0x2019 + "de mevcut de" + [char]0x11F + "il (HTTP 404). Bu tablo i" + [char]0xE7 + "in sync atlan" + [char]0x131 + "yor.`")" + $nl +
"            return true" + $nl +
"        }" + $nl +
"        if (code == 403) {" + $nl +
"            log(`"` + [char]0x26A0 + " '' endpoint" + [char]0x2019 + "ine bu API anahtar" + [char]0x131 + "yla eri" + [char]0x15F + "im yok (HTTP 403). Bu tablo i" + [char]0xE7 + "in sync atlan" + [char]0x131 + "yor.`")" + $nl +
"            return true" + $nl +
"        }" + $nl +
"        return false" + $nl +
"    }" + $nl +
"    suspend fun syncCariler("

# Count matches
$count = ([regex]::Matches($text, [regex]::Escape($old))).Count
Write-Host "Old block match count: $count"
if ($count -ne 1) { exit 1 }

# Build the new block - escape $entity properly
$new = "    }" + $nl +
"" + $nl +
"    /**" + $nl +
"     * Merkezi API bu entity i" + [char]0xE7 + "in endpoint sunmuyorsa (404) veya API anahtar" + [char]0x131 + "n" + [char]0x131 + "n" + $nl +
"     * bu u" + [char]0xE7 + " noktaya eri" + [char]0x15F + "im yetkisi yoksa (403) sync fonksiyonu bilgilendirici" + $nl +
"     * log d" + [char]0xFC + [char]0x15F + [char]0xFC + "erek ba" + [char]0x15F + "ar" + [char]0x131 + "yla d" + [char]0xF6 + "ns" + [char]0xFC + "n. T" + [char]0xFC + "m sync zincirini k" + [char]0x131 + "rmas" + [char]0x131 + "n; sadece o tablo" + $nl +
"     * bo" + [char]0x15F + " kals" + [char]0x131 + "n. UI taraf" + [char]0x131 + "nda `" + "bu " + [char]0xF6 + "zellik tenant'ta yok`" + " " + [char]0x15F + "eklinde g" + [char]0xF6 + "sterilir." + $nl +
"     */" + $nl +
"    private fun isUnsupportedEndpoint(" + $nl +
"        response: retrofit2.Response<*>," + $nl +
"        entity: String," + $nl +
"        log: (String) -> Unit" + $nl +
"    ): Boolean {" + $nl +
"        val code = response.code()" + $nl +
"        if (code == 404) {" + $nl +
"            log(`"` + [char]0x26A0 + " ` + chr(39) + entity + chr(39) + ` endpoint" + [char]0x2019 + "i merkezi API" + [char]0x2019 + "de mevcut de" + [char]0x11F + "il (HTTP 404). Bu tablo i" + [char]0xE7 + "in sync atlan" + [char]0x131 + "yor.`")" + $nl +
"            return true" + $nl +
"        }" + $nl +
"        if (code == 403) {" + $nl +
"            log(`"` + [char]0x26A0 + " ` + chr(39) + entity + chr(39) + ` endpoint" + [char]0x2019 + "ine bu API anahtar" + [char]0x131 + "yla eri" + [char]0x15F + "im yok (HTTP 403). Bu tablo i" + [char]0xE7 + "in sync atlan" + [char]0x131 + "yor.`")" + $nl +
"            return true" + $nl +
"        }" + $nl +
"        return false" + $nl +
"    }" + $nl +
"" + $nl +
"    suspend fun syncCariler("

# Use IndexOf + Substring for single replacement
$idx = $text.IndexOf($old)
if ($idx -lt 0) { Write-Host "Old not found in text"; exit 1 }

$newText = $text.Substring(0, $idx) + $new + $text.Substring($idx + $old.Length)

# Write back
$utf8 = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllText($path, $newText, $utf8)
Write-Host "OK: patch fixed"

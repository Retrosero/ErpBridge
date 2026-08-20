# Patch1: Add isUnsupportedEndpoint helper before syncCariler
$ErrorActionPreference = 'Stop'

$dirName = "saha-sat" + [char]0x131 + [char]0x15F
$path = "C:\Users\retro\Documents\GitHub\ErpBridge\$dirName\app\src\main\java\com\example\ui\screens\BridgeSyncHelper.kt"

if (-not (Test-Path -LiteralPath $path)) { Write-Host "FAIL: path not found"; exit 1 }

$bytes = [System.IO.File]::ReadAllBytes($path)
$text = [System.Text.Encoding]::UTF8.GetString($bytes)

$c_c = [char]0xE7
$c_i = [char]0x131
$c_s = [char]0x15F
$c_u = [char]0xFC
$c_o = [char]0xF6
$c_g = [char]0x11F
$warn = [char]0x26A0
$apos = [char]0x2019

# Build the insert content (everything after the closing brace of handleApiError)
$insertBlock = @"

    /**
     * Merkezi API bu entity i${c_c}in endpoint sunmuyorsa (404) veya API anahtar${c_i}n${c_i}n
     * bu u${c_c} noktaya eri${c_s}im yetkisi yoksa (403) sync fonksiyonu bilgilendirici
     * log d${c_u}${c_s}${c_u}erek ba${c_s}ar${c_i}yla d${c_o}ns${c_u}n. T${c_u}m sync zincirini k${c_i}rmas${c_i}n; sadece o tablo
     * bo${c_s} kals${c_i}n. UI taraf${c_i}nda "bu ${c_o}zellik tenant'ta yok" ${c_s}eklinde g${c_o}sterilir.
     */
    private fun isUnsupportedEndpoint(
        response: retrofit2.Response<*>,
        entity: String,
        log: (String) -> Unit
    ): Boolean {
        val code = response.code()
        if (code == 404) {
            log("${warn} '$entity' endpoint${apos}i merkezi API${apos}de mevcut de${c_g}il (HTTP 404). Bu tablo i${c_c}in sync atlan${c_i}yor.")
            return true
        }
        if (code == 403) {
            log("${warn} '$entity' endpoint${apos}ine bu API anahtar${c_i}yla eri${c_s}im yok (HTTP 403). Bu tablo i${c_c}in sync atlan${c_i}yor.")
            return true
        }
        return false
    }
"@

# Find the marker: the line "    }" that closes handleApiError, immediately followed by "suspend fun syncCariler("
$nl = "`n"
$marker = "    }" + $nl + "    suspend fun syncCariler("
$count = ([regex]::Matches($text, [regex]::Escape($marker))).Count
Write-Host "Marker match count: $count"
if ($count -ne 1) { exit 1 }

# The marker is preceded by the closing brace, but we need to add our helper BEFORE that brace
# Better approach: find "suspend fun syncCariler(" and prepend the helper before it
$target = "    suspend fun syncCariler("
$idx = $text.IndexOf($target)
if ($idx -lt 0) { Write-Host "Target not found"; exit 1 }
$newText = $text.Substring(0, $idx) + $insertBlock + $nl + $text.Substring($idx)

if ($newText -eq $text) { Write-Host "No change!"; exit 1 }

# Write back as UTF-8 WITHOUT BOM
$utf8 = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllText($path, $newText, $utf8)
Write-Host "OK: isUnsupportedEndpoint helper added before syncCariler"

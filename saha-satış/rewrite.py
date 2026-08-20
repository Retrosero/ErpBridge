import re

with open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "r") as f:
    code = f.read()

# 1. Fix fingerprints
code = re.sub(
    r'val currentFingerprint = cariler.joinToString\(\",\"\) \{ it.hashCode\(\).toString\(\) \}',
    r'val currentFingerprint = cariler.joinToString(",") { (it.actualCariKod ?: it.id ?: "").toString() }',
    code
)

code = re.sub(
    r'val currentFingerprint = urunler.joinToString\(\",\"\) \{ it.hashCode\(\).toString\(\) \}',
    r'val currentFingerprint = urunler.joinToString(",") { (it.actualUrunKod ?: it.id ?: "").toString() }',
    code
)

code = re.sub(
    r'val currentFingerprint = items.joinToString\(\",\"\) \{ it.hashCode\(\).toString\(\) \}',
    r'val currentFingerprint = items.joinToString(",") { (it.id ?: "").toString() }',
    code
)

# 2. Fix repeating page log
code = code.replace(
    'log("Tekrarlayan sayfa algılandı, sayfalama durduruluyor.")',
    'log("Tekrarlayan sayfa algılandı, senkronizasyon tamamlandı.")'
)

# 3. Fix size < pageSize termination logic
# It appears like:
# if (cariler.size < pageSize || ((syncRes.total ?: 0) > 0 && totalFetched >= (syncRes.total ?: 0))) {
#     hasMore = false
# } else {
#     currentPage++
# }
code = re.sub(
    r'if \(([a-zA-Z0-9_]+)\.size < pageSize \|\| \(\(.*?total \?: 0\) > 0 && totalFetched >= \(.*?total \?: 0\)\)\) \{\s*hasMore = false\s*\} else \{\s*currentPage\+\+\s*\}',
    r'if (\1.size < pageSize) {\n                            hasMore = false\n                        } else {\n                            currentPage++\n                        }',
    code
)

with open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "w") as f:
    f.write(code)


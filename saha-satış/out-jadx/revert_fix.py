import re
with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace(
    "if (cariler.isEmpty() || cariler.size < pageSize || ((syncRes.total ?: 0) > 0 && totalFetched >= (syncRes.total ?: 0))) {",
    "if (cariler.isEmpty() || ((syncRes.total ?: 0) > 0 && totalFetched >= (syncRes.total ?: 0))) {"
)

content = content.replace(
    "if (cariler.isEmpty() || cariler.size < pageSize || ((syncRes.total ?: 0) > 0 && allMappedCustomers.size >= (syncRes.total ?: 0))) {",
    "if (cariler.isEmpty() || ((syncRes.total ?: 0) > 0 && allMappedCustomers.size >= (syncRes.total ?: 0))) {"
)

content = content.replace(
    "if (urunler.isEmpty() || urunler.size < pageSize || ((syncRes.total ?: 0) > 0 && totalFetched >= (syncRes.total ?: 0))) {",
    "if (urunler.isEmpty() || ((syncRes.total ?: 0) > 0 && totalFetched >= (syncRes.total ?: 0))) {"
)

content = content.replace(
    "if (urunler.isEmpty() || urunler.size < pageSize || ((syncRes.total ?: 0) > 0 && allMappedProducts.size >= (syncRes.total ?: 0))) {",
    "if (urunler.isEmpty() || ((syncRes.total ?: 0) > 0 && allMappedProducts.size >= (syncRes.total ?: 0))) {"
)

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w', encoding='utf-8') as f:
    f.write(content)

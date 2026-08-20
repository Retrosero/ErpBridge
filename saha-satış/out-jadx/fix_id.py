import re

with open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "r") as f:
    code = f.read()

# BarkodTanimiDto
code = code.replace(
    'val currentFingerprint = items.joinToString(",") { (it.id ?: "").toString() }',
    'val currentFingerprint = items.joinToString(",") { (it.hashCode()).toString() }',
    1 # First replacement is Barkod
)
# Wait, I shouldn't guess the order, I should do exact line numbers.

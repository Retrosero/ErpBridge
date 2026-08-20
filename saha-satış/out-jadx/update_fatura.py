import re

with open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "r") as f:
    content = f.read()

# Find suspend fun syncFaturaHareket
match = re.search(r'    suspend fun syncFaturaHareket\(.*?\n    }    suspend fun syncStatusCheck', content, re.DOTALL)
if match:
    old_func = match.group(0)
    print("Found syncFaturaHareket")
else:
    print("Could not find syncFaturaHareket")

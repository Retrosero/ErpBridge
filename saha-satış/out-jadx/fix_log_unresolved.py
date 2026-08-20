import re
with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r', encoding='utf-8') as f:
    content = f.read()

# Replace log("...") in incremental functions with android.util.Log.d("Sync", "...")
def repl(m):
    return m.group(0).replace('log(', 'android.util.Log.d("Sync", ')

content = re.sub(
    r'suspend fun syncCarilerIncremental.*?suspend fun syncUrunlerIncremental',
    repl,
    content,
    flags=re.DOTALL
)

content = re.sub(
    r'suspend fun syncUrunlerIncremental.*',
    repl,
    content,
    flags=re.DOTALL
)

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w', encoding='utf-8') as f:
    f.write(content)

import re

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Remove SyncTask abstract class from here (since it's in SyncManager)
content = re.sub(
    r'abstract class SyncTask \{.*?\}',
    'import com.example.util.SyncManager\nimport com.example.util.SyncTask\n',
    content,
    flags=re.DOTALL
)

# 2. Replace state variables with SyncManager ones
# We'll replace the block from "var isSyncAllRunning by remember" to "var consoleLogs = remember"
old_state = r'var isSyncAllRunning by remember \{ mutableStateOf\(false\) \}\s*var isSyncAllFinished by remember \{ mutableStateOf\(false\) \}\s*var currentSyncTaskIndex by remember \{ mutableStateOf\(-1\) \}\s*var currentSyncTaskName by remember \{ mutableStateOf\(""\) \}\s*var currentSyncProgress by remember \{ mutableStateOf\(0f\) \}\s*val consoleLogs = remember \{ mutableStateListOf<String>\(\) \}'

new_state = """val isSyncAllRunning by SyncManager.isSyncing.collectAsState()
    val isSyncAllFinished by SyncManager.isSyncAllFinished.collectAsState()
    val currentSyncTaskIndex by SyncManager.currentSyncTaskIndex.collectAsState()
    val currentSyncTaskName by SyncManager.currentSyncTaskName.collectAsState()
    val currentSyncProgress by SyncManager.syncProgress.collectAsState()
    val consoleLogs by SyncManager.syncLogs.collectAsState()"""

content = re.sub(old_state, new_state, content)

# 3. Replace log() and startSyncAll() with SyncManager calls.
# We have `fun log(msg: String)` and `fun startSyncAll()`
methods_block = r'fun log\(msg: String\) \{.*?\}\s*fun startSyncAll\(\) \{.*?\}\s*\}\s*startSyncAll\(\)\s*\}'

new_methods_block = """fun startSyncAll() {
        SyncManager.startSyncAll(context, apiUrl, apiKey, syncTasks)
    }"""

content = re.sub(r'fun log\(msg: String\) \{.*?log\("🎉 Toplu entegrasyon tamamlandı!"\)\s*\}\s*\}', new_methods_block, content, flags=re.DOTALL)

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'w', encoding='utf-8') as f:
    f.write(content)

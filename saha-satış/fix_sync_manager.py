import re

with open('app/src/main/java/com/example/util/SyncManager.kt', 'r', encoding='utf-8') as f:
    content = f.read()

target = """    fun startSyncAll(context: Context, apiUrl: String, apiKey: String, tasks: List<SyncTask>) {"""
replacement = """    fun resetSyncState() {
        _isSyncing.value = false
        _isSyncAllFinished.value = false
        _syncLogs.value = emptyList()
        _currentSyncTaskIndex.value = -1
    }

    fun startSyncAll(context: Context, apiUrl: String, apiKey: String, tasks: List<SyncTask>) {"""

content = content.replace(target, replacement)

with open('app/src/main/java/com/example/util/SyncManager.kt', 'w', encoding='utf-8') as f:
    f.write(content)

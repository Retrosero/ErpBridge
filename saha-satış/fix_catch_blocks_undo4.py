import re

files_to_patch = [
    'app/src/main/java/com/example/data/LicenseRepository.kt',
    'app/src/main/java/com/example/data/SyncRepository.kt',
    'app/src/main/java/com/example/data/LicenseManager.kt',
    'app/src/main/java/com/example/data/CloudSyncManager.kt',
    'app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt',
    'app/src/main/java/com/example/worker/SyncWorker.kt',
    'app/src/main/java/com/example/ui/screens/AppDataStore.kt'
]

for filepath in files_to_patch:
    with open(filepath, "r") as f:
        content = f.read()

    # Just match `if (   is kotlinx.coroutines.CancellationException) throw   ;` with regex spaces
    content = re.sub(
        r'if\s*\(\s*is\s*kotlinx\.coroutines\.CancellationException\)\s*throw\s*;',
        r'if (e is kotlinx.coroutines.CancellationException) throw e;',
        content
    )
    
    # Also I need to handle cases where exception variable was NOT `e`
    # Actually I can just remove the injected code entirely using a broader regex
    content = re.sub(
        r'com\.example\.util\.TelemetryReporter\.reportException\([a-zA-Z0-9_]+,\s*"CatchBlock_in_[^"]*"\);\s*(if\s*\([a-zA-Z0-9_]*\s*is\s*kotlinx\.coroutines\.CancellationException\)\s*throw\s*[a-zA-Z0-9_]*;)?',
        r'',
        content
    )

    with open(filepath, "w") as f:
        f.write(content)

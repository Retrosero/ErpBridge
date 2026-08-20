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

    filename = filepath.split('/')[-1]
    
    # regex to find the exact broken piece
    content = re.sub(
        r'com\.example\.util\.TelemetryReporter\.reportException\([^,]+,\s*"CatchBlock_in_"[^\)]+\);\s*if\s*\(\s*is\s*kotlinx\.coroutines\.CancellationException\)\s*throw\s*;',
        '',
        content
    )
    
    # also there's a space issue
    content = re.sub(
        r'com\.example\.util\.TelemetryReporter\.reportException\([^,]+,\s*"CatchBlock_in_"[^\)]+\);\s*if\s*\(\s*is\s*kotlinx\.coroutines\.CancellationException\)\s*throw\s*;\s*',
        '',
        content
    )

    with open(filepath, "w") as f:
        f.write(content)

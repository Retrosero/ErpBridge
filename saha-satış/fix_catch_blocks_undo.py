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

    # Revert the bad injection
    # We will search for 'catch (X: Exception) { com.example... throw  ;' and replace with 'catch (X: Exception) {'
    # or just use a regex
    content = re.sub(
        r'catch\s*\(\s*([a-zA-Z0-9_]+)\s*:\s*Exception\s*\)\s*\{\s*com\.example\.util\.TelemetryReporter\.reportException\([^\)]+\);\s*if\s*\(\s*is\s*kotlinx\.coroutines\.CancellationException\)\s*throw\s*;\s*',
        r'catch (\1: Exception) { ',
        content
    )

    with open(filepath, "w") as f:
        f.write(content)

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
    try:
        with open(filepath, "r") as f:
            content = f.read()

        # Find "catch (e: Exception) {" and insert reporting
        # Need to be careful not to double insert
        
        if "TelemetryReporter.reportException" not in content:
            new_content = re.sub(
                r'catch\s*\(\s*([a-zA-Z0-9_]+)\s*:\s*Exception\s*\)\s*\{',
                r'catch (\1: Exception) { com.example.util.TelemetryReporter.reportException(\1, "CatchBlock_in_" + "' + filepath.split('/')[-1] + '"); if (\1 is kotlinx.coroutines.CancellationException) throw \1;',
                content
            )
            with open(filepath, "w") as f:
                f.write(new_content)
    except Exception as e:
        print(f"Error processing {filepath}: {e}")


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
    
    broken_str = 'com.example.util.TelemetryReporter.reportException(e, "CatchBlock_in_" + "' + filename + '"); if (  is kotlinx.coroutines.CancellationException) throw  ;'
    broken_str2 = 'com.example.util.TelemetryReporter.reportException(e, "CatchBlock_in_' + filename + '"); if (  is kotlinx.coroutines.CancellationException) throw  ;'

    content = content.replace(broken_str, 'com.example.util.TelemetryReporter.reportException(e, "CatchBlock_in_' + filename + '"); if (e is kotlinx.coroutines.CancellationException) throw e;')
    
    # or just fix the `if (  is` -> `if (e is` and `throw  ;` -> `throw e;`
    content = content.replace('if (  is kotlinx.coroutines.CancellationException) throw  ;', 'if (e is kotlinx.coroutines.CancellationException) throw e;')

    with open(filepath, "w") as f:
        f.write(content)

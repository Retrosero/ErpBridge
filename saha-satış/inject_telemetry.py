import re
import os

def inject(filepath):
    with open(filepath, 'r') as f:
        content = f.read()
    
    # We will search for 'catch (e: Exception) {' and replace it
    new_content = re.sub(
        r'catch\s*\(\s*([a-zA-Z0-9_]+)\s*:\s*Exception\s*\)\s*\{',
        r'catch (\1: Exception) {\n            com.example.util.TelemetryReporter.reportException(\1, "CatchBlock_in_' + os.path.basename(filepath) + r'")\n            if (\1 is kotlinx.coroutines.CancellationException) throw \1\n',
        content
    )
    with open(filepath, 'w') as f:
        f.write(new_content)

inject('app/src/main/java/com/example/worker/SyncWorker.kt')
inject('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt')

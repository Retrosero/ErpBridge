import re
with open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "r") as f:
    content = f.read()

# Add import if missing
if "import com.example.util.TelemetryReporter" not in content:
    content = content.replace("import android.util.Log", "import android.util.Log\nimport com.example.util.TelemetryReporter")

# We want to replace catch (e: Exception) { \n log(...) with reportException
# But there are many, we can use regex
content = re.sub(
    r'catch \((e: Exception)\) \{\s*log\("([^"]+)(?:\$\{[^\}]+\})?"\)',
    r'catch (\1) {\n            TelemetryReporter.reportException(e, "BridgeSyncHelper_sync", "ERROR", "\2")\n            log("\2: ${e.message}")',
    content
)

with open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "w") as f:
    f.write(content)

# Also for AppDataStore
with open("app/src/main/java/com/example/ui/screens/AppDataStore.kt", "r") as f:
    content = f.read()

if "import com.example.util.TelemetryReporter" not in content:
    content = content.replace("import android.util.Log", "import android.util.Log\nimport com.example.util.TelemetryReporter")

content = re.sub(
    r'catch \((e: Throwable)\) \{\s*e\.printStackTrace\(\)\s*\}',
    r'catch (\1) {\n            TelemetryReporter.reportException(e, "AppDataStore_Operation", "ERROR")\n            e.printStackTrace()\n        }',
    content
)

with open("app/src/main/java/com/example/ui/screens/AppDataStore.kt", "w") as f:
    f.write(content)

print("Added telemetry")

import re

with open("app/src/main/java/com/example/MainActivity.kt", "r") as f:
    content = f.read()

content = content.replace("e.printStackTrace()", "e.printStackTrace()\n            com.example.util.TelemetryReporter.reportException(e, \"MainActivity_onCreate\")")
content = content.replace("catch (e: Exception) {}", "catch (e: Exception) { com.example.util.TelemetryReporter.reportException(e, \"MainActivity_Migration\") }")

with open("app/src/main/java/com/example/MainActivity.kt", "w") as f:
    f.write(content)

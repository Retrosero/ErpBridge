import re

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r', encoding='utf-8') as f:
    lines = f.readlines()

in_catch = False
for i, line in enumerate(lines):
    if "catch (e: Exception) {" in line:
        in_catch = True
    elif in_catch and "updateProgress(1.0f)" in line:
        # Check if next line is already throw e
        if i + 1 < len(lines) and "throw e" not in lines[i+1]:
            lines[i] = line.replace("updateProgress(1.0f)", "updateProgress(1.0f)\n            throw e")
    elif in_catch and line.strip() == "}":
        in_catch = False

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w', encoding='utf-8') as f:
    f.writelines(lines)

import re

with open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "r") as f:
    lines = f.readlines()

new_lines = []
for line in lines:
    if 'throw Exception("API Hatası' in line and '})")' not in line and '}")' not in line:
        if '")"' in line:
            pass # might be okay
        # Let's just fix the broken ones
        if line.strip().endswith('response.code()'):
            if '\"' in line.split("API Hatası")[1]:
                line = line.rstrip() + '}\\")")\n'
            else:
                line = line.rstrip() + '})")\n'
    new_lines.append(line)

with open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "w") as f:
    f.writelines(new_lines)

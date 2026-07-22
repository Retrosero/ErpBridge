import re

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r') as f:
    lines = f.read().split('\n')

new_lines = []
for line in lines:
    if re.match(r'^\s*\}$', line):
        new_lines.append('')
    else:
        new_lines.append(line)

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w') as f:
    f.write('\n'.join(new_lines))

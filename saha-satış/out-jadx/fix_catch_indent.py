import re

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r') as f:
    lines = f.read().split('\n')

new_lines = []
for i, line in enumerate(lines):
    if re.match(r'^\s*\}\s*catch\s*\(', line):
        # find next non-empty line
        next_indent = -1
        for j in range(i+1, len(lines)):
            if lines[j].strip() != '':
                next_indent = len(lines[j]) - len(lines[j].lstrip())
                break
        
        if next_indent != -1:
            target_indent = next_indent - 4
            if target_indent < 0: target_indent = 0
            new_lines.append(" " * target_indent + line.lstrip())
        else:
            new_lines.append(line)
    else:
        new_lines.append(line)

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w') as f:
    f.write('\n'.join(new_lines))

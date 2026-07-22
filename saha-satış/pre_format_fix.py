import re

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r') as f:
    lines = f.read().split('\n')

new_lines = []
for line in lines:
    # if line is just `else {` or `catch (...) {` preceded by spaces, we add `} ` in front of it!
    # Wait, if it already has `}`, we don't.
    if re.match(r'^\s*else\s*\{', line):
        # find leading spaces
        spaces = len(line) - len(line.lstrip())
        new_lines.append(" " * spaces + "} else {")
    elif re.match(r'^\s*catch\s*\(', line):
        spaces = len(line) - len(line.lstrip())
        new_lines.append(" " * spaces + "} " + line.lstrip())
    else:
        new_lines.append(line)

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w') as f:
    f.write('\n'.join(new_lines))

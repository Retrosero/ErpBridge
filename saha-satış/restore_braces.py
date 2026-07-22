with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r') as f:
    lines = f.read().split('\n')

new_lines = []
for line in lines:
    if line.strip() == '':
        spaces = len(line)
        # We only restore up to 20 spaces (which means original was 36 spaces).
        # We know sed removed exactly 16 spaces and }
        original = line + '                }'
        new_lines.append(original)
    else:
        new_lines.append(line)

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w') as f:
    f.write('\n'.join(new_lines))

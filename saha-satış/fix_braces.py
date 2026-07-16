with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r') as f:
    lines = f.read().split('\n')

def get_indent(line):
    return len(line) - len(line.lstrip())

new_lines = []
for i in range(len(lines)):
    line = lines[i]
    if line.strip() == '':
        # Find next non-empty line
        next_idx = i + 1
        while next_idx < len(lines) and lines[next_idx].strip() == '':
            next_idx += 1
            
        if next_idx < len(lines):
            next_line = lines[next_idx]
            next_indent = get_indent(next_line)
            # If the next line is "catch" or "else", we definitely need a brace before it
            if next_line.strip().startswith('catch') or next_line.strip().startswith('else'):
                new_lines.append(line + '                }')
                continue
                
            # If the next line has a smaller indent than 16, and we are currently missing braces?
            # It's hard to know exactly without parsing.
            
    new_lines.append(line)

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w') as f:
    f.write('\n'.join(new_lines))

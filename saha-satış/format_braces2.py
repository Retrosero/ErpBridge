with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r') as f:
    lines = f.read().split('\n')

def get_indent(line):
    if line.strip() == '':
        return 9999
    return len(line) - len(line.lstrip())

new_lines = []
stack = [] # stores opener_indent

for i, line in enumerate(lines):
    if line.strip() == '':
        new_lines.append(line)
        continue
        
    indent = get_indent(line)
    is_closing_brace = line.lstrip().startswith('}')
    
    while len(stack) > 0 and indent <= stack[-1]:
        if is_closing_brace and indent == stack[-1]:
            # It's a matching closing brace already present on the line.
            # We don't pop here, because the `for char in line` will pop it!
            break
            
        opener_indent = stack.pop()
        new_lines.append(" " * opener_indent + "}")
        
    new_lines.append(line)
    
    # Process opening and closing braces ON this line
    for char in line:
        if char == '{':
            stack.append(indent)
        elif char == '}':
            if len(stack) > 0:
                stack.pop()

while len(stack) > 0:
    opener_indent = stack.pop()
    new_lines.append(" " * opener_indent + "}")

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w') as f:
    f.write('\n'.join(new_lines))

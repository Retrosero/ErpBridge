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
        # We can keep empty lines, but we process them later or just append them
        new_lines.append(line)
        continue
        
    indent = get_indent(line)
    
    # Check if we need to close blocks
    # We close a block if the current line's indent is <= the block's opener indent.
    # EXCEPT: What if the current line starts with `.` (method chaining)?
    # Or `,` (list continuation)?
    # Or if it's `}` itself? (e.g. `} else {`). If it's `}`, we will process it, but we should STILL pop the stack!
    # Wait, if the current line is `} else {`, its indent is exactly `opener_indent`.
    # The while loop will trigger, pop the stack, and INSERT a `}`!
    # Then we will have `}` and then `} else {`. That's bad!
    # If the current line ALREADY starts with `}`, we should pop the stack BUT NOT insert a new `}`!
    
    is_closing_brace = line.lstrip().startswith('}')
    
    while len(stack) > 0 and indent <= stack[-1]:
        # If the current line is `}` and its indent == stack[-1], it IS the closing brace!
        if is_closing_brace and indent == stack[-1]:
            stack.pop()
            break # We matched it, don't insert a generated one, and stop popping for this specific brace.
            
        opener_indent = stack.pop()
        new_lines.append(" " * opener_indent + "}")
        
    new_lines.append(line)
    
    # Now process opening and closing braces ON this line
    # (e.g. if it has `{`, we push. If it has `}`, we pop)
    # A line could have multiple. e.g. `{ ... }`.
    # Let's count them.
    # We can just iterate through the string.
    for char in line:
        if char == '{':
            stack.append(indent)
        elif char == '}':
            if len(stack) > 0:
                stack.pop()

# At the end of the file, close any remaining blocks
while len(stack) > 0:
    opener_indent = stack.pop()
    new_lines.append(" " * opener_indent + "}")

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w') as f:
    f.write('\n'.join(new_lines))

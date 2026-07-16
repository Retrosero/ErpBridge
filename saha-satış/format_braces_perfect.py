import re

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r') as f:
    lines = f.read().split('\n')

# First, let's strip any blindly added braces from previous attempts.
# Since strip_all_braces.py might have already done this, we'll just do it again to be safe.
cleaned_lines = []
for line in lines:
    if re.match(r'^\s*\}$', line):
        continue
    # also strip lines that were just `                }` blindly appended
    if line.endswith('                }'):
        prefix = line[:-17]
        if prefix.strip() == '':
            continue
    cleaned_lines.append(line)

def remove_strings_and_comments(code):
    # Remove multiline strings
    code = re.sub(r'"""[\s\S]*?"""', '', code)
    # Remove single line strings (careful with escaped quotes)
    code = re.sub(r'"(?:\\.|[^"\\])*"', '', code)
    # Remove single line comments
    code = re.sub(r'//.*', '', code)
    # Remove multiline comments
    code = re.sub(r'/\*[\s\S]*?\*/', '', code)
    return code

def get_indent(line):
    if line.strip() == '':
        return 9999
    return len(line) - len(line.lstrip())

new_lines = []
stack = [] # stores opener_indent

for i, line in enumerate(cleaned_lines):
    if line.strip() == '':
        new_lines.append(line)
        continue
        
    indent = get_indent(line)
    
    # Analyze the line without strings and comments for braces
    clean_line = remove_strings_and_comments(line)
    
    is_closing_brace = line.lstrip().startswith('}')
    
    while len(stack) > 0 and indent <= stack[-1]:
        if is_closing_brace and indent == stack[-1]:
            # This line already has the closing brace we need
            break
            
        opener_indent = stack.pop()
        new_lines.append(" " * opener_indent + "}")
        
    new_lines.append(line)
    
    for char in clean_line:
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

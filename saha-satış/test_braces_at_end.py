with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'r') as f:
    lines = f.readlines()

import re

stack = []
for i, line in enumerate(lines):
    # strip single line comments
    idx = line.find('//')
    if idx != -1:
        if 'http://' not in line and 'https://' not in line:
            line = line[:idx]
            
    # replace block comments
    # (for simple per-line analysis, but let's be careful of strings)
    line = re.sub(r'".*?"', '""', line)
    
    for char in line:
        if char == '{':
            stack.append((i+1, line.strip()))
        elif char == '}':
            if len(stack) > 0:
                stack.pop()
            else:
                print(f"Extra closing brace at line {i+1}: {line.strip()}")

    if 3255 <= i+1 <= 3275:
        print(f"Line {i+1}: Stack size {len(stack)}")
        if i+1 == 3265:
            print("Stack elements at 3265:")
            for item in stack:
                print(item)

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'r') as f:
    lines = f.readlines()

import re

stack = []
for i, line in enumerate(lines):
    idx = line.find('//')
    if idx != -1:
        if 'http://' not in line and 'https://' not in line:
            line = line[:idx]
            
    line = re.sub(r'".*?"', '""', line)
    
    for char in line:
        if char == '{':
            stack.append((i+1, line.strip()))
        elif char == '}':
            if len(stack) > 0:
                stack.pop()

    if i+1 >= len(lines) - 25:
        print(f"Line {i+1}: Stack size {len(stack)}")
        if i+1 == len(lines):
            print("Remaining elements in stack at the very end:")
            for item in stack:
                print(item)

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

    if 2575 <= i+1 <= 2595:
        print(f"Line {i+1}: Stack size {len(stack)} | {line.strip() if line.strip() else 'EMPTY'}")
        if i+1 == 2585 or i+1 == 2586:
            print("Stack elements:")
            for item in stack:
                print("  ", item)

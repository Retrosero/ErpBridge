with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'r') as f:
    lines = f.readlines()

# insert '}' at line 2471 (index 2470)
lines.insert(2470, "                    }\n")

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

print(f"Total lines in simulated file: {len(lines)}")
print(f"Stack size at very end: {len(stack)}")
for item in stack:
    print(item)

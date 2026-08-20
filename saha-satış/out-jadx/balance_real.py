import re

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'r') as f:
    text = f.read()

# remove block comments
text = re.sub(r'/\*.*?\*/', '', text, flags=re.DOTALL)

lines = text.split('\n')
stack = []

for i, line in enumerate(lines):
    # remove single line comments
    idx = line.find('//')
    if idx != -1:
        # wait, what if // is inside a string?
        # let's just do a simple heuristic: if it's // we strip it, but be careful of http://
        if not 'http://' in line and not 'https://' in line:
            line = line[:idx]
            
    # remove strings
    line = re.sub(r'".*?"', '""', line)
    
    for char in line:
        if char == '{':
            stack.append(i+1)
        elif char == '}':
            if len(stack) > 0:
                stack.pop()
            else:
                print(f"EXTRA CLOSING BRACE AT {i+1}")

print(f"Remaining opening braces: {len(stack)}")
for item in stack[-5:]:
    print(item)

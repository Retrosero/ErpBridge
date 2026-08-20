with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'r') as f:
    lines = f.readlines()

stack = []
for i, line in enumerate(lines):
    import re
    cleaned = re.sub(r'".*?"', '""', line)
    
    for char in cleaned:
        if char == '{':
            stack.append((i+1, line.strip()))
        elif char == '}':
            if len(stack) > 0:
                stack.pop()
    
    if i+1 == 2961:
        for item in stack:
            print(item)

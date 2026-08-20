with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'r') as f:
    lines = f.readlines()

stack = []
for i, line in enumerate(lines):
    import re
    cleaned = re.sub(r'".*?"', '""', line)
    
    for char in cleaned:
        if char == '{':
            stack.append(i+1)
        elif char == '}':
            if len(stack) > 0:
                stack.pop()
    
    if 2000 <= i+1 <= 2100:
        if (i+1) % 10 == 0:
            print(f"Stack size at line {i+1}: {len(stack)}")

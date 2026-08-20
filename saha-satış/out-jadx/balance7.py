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
    
    if 2950 <= i+1 <= 2970:
        print(f"Line {i+1}: Stack size {len(stack)}")

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
    
    if i+1 == 1422:
        print(f"Stack size at line 1422: {len(stack)}")
        print(f"Top 5: {stack[-5:]}")

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'r') as f:
    lines = f.readlines()

stack = []
for i, line in enumerate(lines):
    # strip string literals to avoid counting braces inside strings
    import re
    cleaned = re.sub(r'".*?"', '""', line)
    
    for char in cleaned:
        if char == '{':
            stack.append((i+1, line.strip()))
        elif char == '}':
            if len(stack) > 0:
                stack.pop()
            else:
                print(f"Extra closing brace at line {i+1}: {line.strip()}")
                break
    if len(stack) == 0 and '}' in cleaned:
        pass # keep going to see if we find the first extra brace

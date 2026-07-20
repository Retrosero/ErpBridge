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
            else:
                print(f"FIRST EXTRA CLOSING BRACE AT LINE {i+1}")
                for j in range(max(0, i-10), i+2):
                    print(f"{j+1}: {lines[j].strip()}")
                import sys
                sys.exit(0)

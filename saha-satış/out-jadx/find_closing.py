with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r') as f:
    lines = f.read().split('\n')

balance = 0
for i, line in enumerate(lines):
    # simple counting
    balance += line.count('{') - line.count('}')
    if balance == 0 and 'object BridgeSyncHelper' in '\n'.join(lines[:i]):
        print(f"Object closed at line {i+1}: {line}")
        break

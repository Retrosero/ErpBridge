import re

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r') as f:
    text = f.read()

text = re.sub(r'\}\n\s*catch\s*\(', '} catch (', text)

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w') as f:
    f.write(text)

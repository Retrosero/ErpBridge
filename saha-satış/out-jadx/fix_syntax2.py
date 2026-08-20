import re

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'r') as f:
    code = f.read()

# Fix the extra brace before tableLastSyncTimes again, just in case my previous regex missed it because of spaces
code = re.sub(r'\}\n(\s+tableLastSyncTimes\[)', r'\1', code)

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'w') as f:
    f.write(code)

import re

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'r') as f:
    code = f.read()

# Add the } back before tableLastSyncTimes
code = re.sub(r'(\s+)(tableLastSyncTimes\[)', r'\1}\1\2', code)

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'w') as f:
    f.write(code)


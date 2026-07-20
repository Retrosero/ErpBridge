import re

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'r') as f:
    code = f.read()

# Aggressive clean up of the corrupted blocks
code = re.sub(r'\}\s*else\s*if\s*\(false\)\s*\{\s*else\s*\{', '} else {', code)
code = re.sub(r'\}\s*if\s*\(false\)\s*\{', '', code)
code = re.sub(r'else\s*if\s*\(false\)\s*\{\s*else\s*\{', 'else {', code)

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'w') as f:
    f.write(code)


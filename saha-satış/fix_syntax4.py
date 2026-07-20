import re

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'r') as f:
    code = f.read()

code = re.sub(r'\}\s*else\s*if\s*\(false\)\s*\{\s*else\s*if', '} else if', code)

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'w') as f:
    f.write(code)


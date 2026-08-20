import re

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r') as f:
    text = f.read()

# Replace "\n                     else if" with " else if" on the previous line if it ends with }
# Actually just search for "}\n\s*else if" and replace with "} else if"
text = re.sub(r'\}\n\s*else if', '} else if', text)

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w') as f:
    f.write(text)

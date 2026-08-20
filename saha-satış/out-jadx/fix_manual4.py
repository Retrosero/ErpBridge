import re

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r') as f:
    text = f.read()

bad_pattern = r'\s*\}\n\s*\}\n\s*\} else if \(bal > 0\.0\) \{'
good_replacement = '\n                                    } else if (bal > 0.0) {'
text = re.sub(bad_pattern, good_replacement, text)

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w') as f:
    f.write(text)

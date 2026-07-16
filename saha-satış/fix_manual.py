import re

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r') as f:
    text = f.read()

# Fix 1: bal > 0.0
# The bad part is:
#                                 }
#                             }
#                         } else if (bal > 0.0) {
bad_pattern1 = r'\s*\}\n\s*\}\n\s*\} else if \(bal > 0\.0\) \{'
good_replacement1 = '\n                                }\n                            } else if (bal > 0.0) {'
text = re.sub(bad_pattern1, good_replacement1, text)

# Let's check where the `else {` for `bal < 0.0` is.
#                                     } else {
#                                         val absBal = Math.abs(bal)
bad_pattern2 = r'\s*\} else \{\n\s*val absBal'
good_replacement2 = '\n                            } else {\n                                        val absBal'
text = re.sub(bad_pattern2, good_replacement2, text)

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w') as f:
    f.write(text)

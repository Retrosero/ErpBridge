import re

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r') as f:
    text = f.read()

bad_pattern1 = r'\n                            \} else if \(bal > 0\.0\) \{'
good_replacement1 = '\n                                    } else if (bal > 0.0) {'
text = re.sub(bad_pattern1, good_replacement1, text)

bad_pattern2 = r'\n                            \} else \{\n                                        val absBal'
good_replacement2 = '\n                                    } else {\n                                        val absBal'
text = re.sub(bad_pattern2, good_replacement2, text)

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w') as f:
    f.write(text)

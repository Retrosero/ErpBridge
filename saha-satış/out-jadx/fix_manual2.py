import re

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r') as f:
    text = f.read()

bad_pattern = r'\s*\}\n\s*\}\n\s*\}\n\s*\} else if \(u\.stockByWarehouse'
good_replacement = '\n                            } else if (u.stockByWarehouse'
text = re.sub(bad_pattern, good_replacement, text)

bad_pattern2 = r'\s*\} else if \(u\.miktarDepo'
good_replacement2 = '\n                            } else if (u.miktarDepo'
text = re.sub(bad_pattern2, good_replacement2, text)

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w') as f:
    f.write(text)

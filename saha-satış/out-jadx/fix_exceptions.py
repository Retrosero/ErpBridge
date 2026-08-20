import re

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r', encoding='utf-8') as f:
    content = f.read()

# Pattern to find catch (e: Exception) { ... updateProgress(1.0f) ... }
# and append throw e
def replace_func(match):
    catch_block = match.group(0)
    # Check if there is already a throw e
    if 'throw e' in catch_block:
        return catch_block
    return catch_block.replace('updateProgress(1.0f)', 'updateProgress(1.0f)\n            throw e')

new_content = re.sub(r'catch\s*\(e:\s*Exception\)\s*\{[^}]*?updateProgress\(1\.0f\)[^}]*\}', replace_func, content)

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w', encoding='utf-8') as f:
    f.write(new_content)

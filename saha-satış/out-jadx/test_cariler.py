import re

with open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "r") as f:
    code = f.read()

cariler_pattern = re.compile(r'suspend fun syncCariler\(.*?\n\s+updateProgress\(1\.0f\)\n\s+\} catch \(e: Exception\) \{.*?\n\s+\}\n\s+\}', re.DOTALL)
cariler_match = cariler_pattern.search(code)
if cariler_match:
    print("Found syncCariler!")
    print(cariler_match.group(0)[:500])

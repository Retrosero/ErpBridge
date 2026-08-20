import re

with open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "r") as f:
    code = f.read()

code = re.sub(
    r'if \(([a-zA-Z0-9_]+)\.size < pageSize \|\| \(\(.*?\)\)\) \{\s*hasMore = false\s*\} else \{\s*currentPage\+\+\s*\}',
    r'if (\1.size < pageSize) {\n                            hasMore = false\n                        } else {\n                            currentPage++\n                        }',
    code
)

with open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "w") as f:
    f.write(code)


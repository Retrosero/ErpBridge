import re
with open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "r") as f:
    content = f.read()

count = content.count("throw handleApiError")
print(f"Found {count} instances of throw handleApiError")

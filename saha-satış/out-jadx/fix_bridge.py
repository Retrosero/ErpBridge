import re
import os

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r') as f:
    content = f.read()

# We need to rewrite the sync functions to use the new logic.
# Wait, let's just create a new Kotlin file or apply precise regexes.
# Actually, the user's requirements apply to many sync functions in BridgeSyncHelper.
# It might be easier to write a Python script that uses regex to find `while (hasMore)` loops and modify them.

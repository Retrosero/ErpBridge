import re
import os

with open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "r") as f:
    code = f.read()

# Fix 1: Fix Pagination Condition for all while(hasMore) loops
# We'll replace the block:
# if (items.size < pageSize || ... ) { hasMore = false } else { currentPage++ }
# Or similar conditions.

# Actually, let's just do a regex replace for the `if (cariler.isEmpty() ...` and `if (cariler.size < pageSize ...` logic, because it's slightly different in each function.

# Let's inspect all 'while (hasMore)' blocks first.

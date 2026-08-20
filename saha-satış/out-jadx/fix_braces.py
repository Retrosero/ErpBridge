import re

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'r') as f:
    code = f.read()

# The pattern is:
# }
# tableLastSyncTimes[...
# We want to remove that extra `}`.
code = re.sub(r'\}\n(\s+tableLastSyncTimes\[)', r'\1', code)

# Let's check other errors.
# ErpIntegrationScreen.kt:2281:41 Unresolved reference 'tableList'.
# This means tableList declaration was broken!

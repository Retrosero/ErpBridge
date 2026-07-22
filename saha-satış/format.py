import re

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'r') as f:
    lines = f.readlines()

# The error for `users` is `2275:41 Syntax error: Expecting an element.`
# Wait, look at line 2271 in the output: `                                                            }`
# But `tableLastSyncTimes` is at 2272!
# Let's just fix `users`!
for i, line in enumerate(lines):
    if 'tableLastSyncTimes["users"]' in line:
        if lines[i-1].strip() == '}':
            lines[i-1] = '\n'
    if 'tableLastSyncTimes["products"]' in line:
        if lines[i-1].strip() == '}':
            lines[i-1] = '\n'
    if 'tableLastSyncTimes["banks"]' in line:
        if lines[i-1].strip() == '}':
            lines[i-1] = '\n'
    if 'tableLastSyncTimes["kasa_logs"]' in line:
        if lines[i-1].strip() == '}':
            lines[i-1] = '\n'
    if 'tableLastSyncTimes["sales_records"]' in line:
        if lines[i-1].strip() == '}':
            lines[i-1] = '\n'
            
with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'w') as f:
    f.writelines(lines)

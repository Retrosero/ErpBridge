import re

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'r') as f:
    lines = f.readlines()

new_lines = []
for line in lines:
    if 'tableLastSyncTimes["products"]' in line or \
       'tableLastSyncTimes["banks"]' in line or \
       'tableLastSyncTimes["kasa_logs"]' in line or \
       'tableLastSyncTimes["sales_records"]' in line or \
       'tableLastSyncTimes["users"]' in line or \
       'tableLastSyncTimes["purchase_invoices"]' in line or \
       'tableLastSyncTimes["sales_invoices"]' in line:
        
        # Don't add brace if it's the `tableList.forEach` usage!
        if 'val lastSync =' not in line:
            new_lines.append('                                                            }\n')
    new_lines.append(line)

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'w') as f:
    f.writelines(new_lines)

import re
f = "app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt"
c = open(f).read()
c = c.replace('syncRes.total > 0', '(syncRes.total ?: 0) > 0')
c = c.replace('>= syncRes.total', '>= (syncRes.total ?: 0)')
c = c.replace('totalFetched >= syncRes.total', 'totalFetched >= (syncRes.total ?: 0)')
c = c.replace('allMappedProducts.size >= syncRes.total', 'allMappedProducts.size >= (syncRes.total ?: 0)')
c = c.replace('allMappedCustomers.size >= syncRes.total', 'allMappedCustomers.size >= (syncRes.total ?: 0)')

open(f, "w").write(c)


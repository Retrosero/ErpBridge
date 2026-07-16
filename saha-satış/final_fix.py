import re

f = "app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt"
c = open(f).read()

# Lines 985, 993, 1000 etc. are because `priceLists` is nullable if `items` is nullable.
c = c.replace('val items = responseList.body()!!.items', 'val items = responseList.body()?.actualItems ?: emptyList()')

open(f, "w").write(c)

f = "app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt"
c = open(f).read()

# Lines 1597, 1600 are about `syncRes.items` being nullable.
c = c.replace('val cariler = syncRes.items', 'val cariler = syncRes.actualItems')
c = c.replace('val urunler = syncRes.items', 'val urunler = syncRes.actualItems')

# Fallback if they were already `syncRes.items ?: emptyList()`
c = c.replace('val cariler = syncRes.items ?: emptyList()', 'val cariler = syncRes.actualItems')
c = c.replace('val urunler = syncRes.items ?: emptyList()', 'val urunler = syncRes.actualItems')

open(f, "w").write(c)

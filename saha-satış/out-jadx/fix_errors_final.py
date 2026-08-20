import re

# Fix BridgeSyncHelper syntax error
f = "app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt"
c = open(f).read()
c = c.replace('/* log removed */: $safeMessage")', '/* log removed */')
# Replace Cari/Urun items with actualItems safely
# We know StokSatisFiyatListeTanimlariDto doesn't have actualItems, nor FiyatListeleriDto
# So we need to ensure they use .items
# We already did this, but let's check FiyatListeleriDto:
c = c.replace('val items2 = responseDef2.body()!!.items ?: emptyList()', 'val items2 = responseDef2.body()?.items ?: emptyList()')
c = c.replace('val items2 = responseDef2.body()!!.items', 'val items2 = responseDef2.body()?.items ?: emptyList()')
c = c.replace('val items = responseDef.body()!!.items ?: emptyList()', 'val items = responseDef.body()?.items ?: emptyList()')
c = c.replace('val items = responseDef.body()!!.items', 'val items = responseDef.body()?.items ?: emptyList()')

# BridgeSyncHelper has `val cariler = syncRes.items` -> we changed it, let's make sure it's `syncRes.actualItems`
# Ah wait, we replaced it back in ErpIntegrationScreen. But not sure if we did it right.
open(f, "w").write(c)

f = "app/src/main/java/com/example/ui/screens/SalesScreen.kt"
c = open(f).read()
c = c.replace('since=customer?.id', 'since=custId')
open(f, "w").write(c)


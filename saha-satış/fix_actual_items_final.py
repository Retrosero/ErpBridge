import re
f = "app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt"
c = open(f).read()
c = c.replace('body.actualItems', 'body.items')
c = c.replace('responseDef.body()!!.actualItems', 'responseDef.body()!!.items')
c = c.replace('responseDef2.body()!!.actualItems', 'responseDef2.body()!!.items')
c = c.replace('txRes.body()!!.actualItems', 'txRes.body()!!.items')
c = c.replace('pushRes.actualItems', 'pushRes.items')

# For syncUrunler and syncCariler, the variable is syncRes
# Let's ensure it's syncRes.actualItems
c = c.replace('syncRes.items', 'syncRes.actualItems')
open(f, "w").write(c)

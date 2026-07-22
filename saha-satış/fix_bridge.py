import re
f = "app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt"
c = open(f).read()
c = c.replace('txRes.body()!!.actualItems', 'txRes.body()!!.items')
c = c.replace('since=code', 'since=cari.erpKod')
open(f, "w").write(c)


import re
f = "app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt"
c = open(f).read()
c = c.replace('responseDef.body()?.items ?: emptyList()', 'responseDef.body()?.actualItems ?: emptyList()')
c = c.replace('responseDef2.body()?.items ?: emptyList()', 'responseDef2.body()?.actualItems ?: emptyList()')

open(f, "w").write(c)

f = "app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt"
c = open(f).read()
c = c.replace('syncRes.items ?: emptyList()', 'syncRes.actualItems')
open(f, "w").write(c)

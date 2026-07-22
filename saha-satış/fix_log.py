f = "app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt"
c = open(f).read()
c = c.replace('throw handleApiError(response, log)', 'throw handleApiError(response, log = { })')
open(f, "w").write(c)

import re

files = [
    "app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt",
    "app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt",
    "app/src/main/java/com/example/data/SyncRepository.kt"
]

for filename in files:
    content = open(filename).read()
    content = content.replace(".items\n", ".actualItems\n")
    content = content.replace("syncRes.items", "syncRes.actualItems")
    content = content.replace("body.items", "body.actualItems")
    content = content.replace("stats.watermarks?.forEach", "(stats.watermarks ?: emptyList()).forEach")
    
    # Also fix some unresolved references from earlier script
    content = content.replace('since=stokKod', 'since="stokKod"')
    content = content.replace('since=cariKod', 'since="cariKod"')
    content = content.replace('triggerSync(erp=', 'triggerSync(com.example.data.api.PullJobsRequest(tenant_id="", api_key="", device_id="", agent_version="", entity="trigger")) //')
    content = content.replace('getPushStatus(request.requestId)', 'getPushStatus(com.example.data.api.PullJobsRequest(tenant_id="", api_key="", device_id="", agent_version="", entity="pushStatus"))')
    content = content.replace("apiService.getUrunler(page = 1, pageSize = 100)", "apiService.getUrunler(com.example.data.api.PullJobsRequest(tenant_id=tenantId, api_key=apiKey, device_id=deviceId, agent_version=\"v2.0\", entity=\"urun\"))")
    
    open(filename, "w").write(content)

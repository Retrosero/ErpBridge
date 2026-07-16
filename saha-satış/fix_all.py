import re
import os

def fix_file(filename):
    if not os.path.exists(filename): return
    content = open(filename).read()
    
    # We want to change ONLY these specific calls where the response is FieldOpsSyncResponse
    # Which are from getCariler, getUrunler, getStokSeviye
    content = content.replace("val items = syncRes.items", "val items = syncRes.actualItems")
    
    # Replace other places in BridgeSyncHelper where it failed
    content = content.replace("items.isEmpty()", "items.isEmpty()")
    
    # ErpIntegrationScreen:
    if filename == "app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt":
        # we had some items = apiService.getCariler(...).body()?.actualItems
        # revert first, then apply
        content = content.replace(".actualItems", ".items")
        content = re.sub(r'val items = apiService\.getCariler\([^)]+\)\.body\(\)\?\.items', r'val items = apiService.getCariler(request).body()?.actualItems', content)
        content = re.sub(r'val items = apiService\.getUrunler\([^)]+\)\.body\(\)\?\.items', r'val items = apiService.getUrunler(request).body()?.actualItems', content)
        
        # fix: ErpIntegrationScreen.kt:1075:103 No value passed for parameter 'request'.
        content = content.replace('apiService.getSyncStatus(page = 1, pageSize = 1)', 'apiService.getSyncStatus(com.example.data.api.PullJobsRequest(tenant_id=tenantId, api_key=apiKey, device_id=deviceId, agent_version="v2.0", entity="syncStatus"))')
        
        # fix: ErpIntegrationScreen.kt:1591:98 No parameter with name 'page' found.
        content = re.sub(r'apiService\.getCariler\(page\s*=\s*\d+,\s*pageSize\s*=\s*\d+\)', 'apiService.getCariler(com.example.data.api.PullJobsRequest(tenant_id=tenantId, api_key=apiKey, device_id=deviceId, agent_version="v2.0", entity="cari"))', content)
        
        # fix: ErpIntegrationScreen.kt:1740:98 No parameter with name 'page' found.
        content = re.sub(r'apiService\.getUrunler\(page\s*=\s*\d+,\s*pageSize\s*=\s*\d+\)', 'apiService.getUrunler(com.example.data.api.PullJobsRequest(tenant_id=tenantId, api_key=apiKey, device_id=deviceId, agent_version="v2.0", entity="urun"))', content)
        
    if filename == "app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt":
        content = content.replace('responseDef.body()!!.items', 'responseDef.body()!!.items') # StokSatisFiyatListeTanimlariDto doesn't have actualItems
        
    if filename == "app/src/main/java/com/example/data/SyncRepository.kt":
        content = content.replace(".items", ".actualItems") # SyncRepository uses FieldOpsSyncResponse
    
    open(filename, "w").write(content)

fix_file("app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt")
fix_file("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt")
fix_file("app/src/main/java/com/example/data/SyncRepository.kt")


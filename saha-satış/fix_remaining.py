import re

files = ["app/src/main/java/com/example/ui/screens/StockDetailScreen.kt", "app/src/main/java/com/example/ui/screens/CustomersScreen.kt", "app/src/main/java/com/example/ui/screens/SalesScreen.kt", "app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt"]

for f in files:
    content = open(f).read()
    
    # BridgeSyncHelper
    content = content.replace('since="cariKod"', 'since=code')
    content = content.replace('val items = txRes.body()!!.actualItems', 'val items = txRes.body()!!.items') # CariHareketResponse doesn't have actualItems
    
    # StockDetailScreen
    content = content.replace('com.example.data.api.PullJobsRequest(tenant_id="", api_key="", device_id="", agent_version="", entity="stokHareket", since=stokKod)', 'com.example.data.api.PullJobsRequest(tenant_id=sharedPrefs.getString("tenant_id", "T001") ?: "T001", api_key=apiKey, device_id=sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT", agent_version="v2.0", entity="stokHareket", since=product.barcode)')
    content = content.replace('response.body()!!.items\n', 'response.body()!!.items\n')
    
    # CustomersScreen
    content = content.replace('com.example.data.api.PullJobsRequest(tenant_id="", api_key="", device_id="", agent_version="", entity="cariHareket", since=cariKod)', 'com.example.data.api.PullJobsRequest(tenant_id=sharedPrefs.getString("tenant_id", "T001") ?: "T001", api_key=apiKey, device_id=sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT", agent_version="v2.0", entity="cariHareket", since=customer.id)')
    
    # SalesScreen
    content = content.replace('com.example.data.api.PullJobsRequest(tenant_id="", api_key="", device_id="", agent_version="", entity="faturaHareket", since=cariKod)', 'com.example.data.api.PullJobsRequest(tenant_id=sharedPrefs.getString("tenant_id", "T001") ?: "T001", api_key=apiKey, device_id=sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT", agent_version="v2.0", entity="faturaHareket", since=customer.id)')
    
    # ErpIntegrationScreen triggerSync
    content = content.replace('com.example.data.api.PullJobsRequest(tenant_id="", api_key="", device_id="", agent_version="", entity="trigger")', 'com.example.data.api.PullJobsRequest(tenant_id=tenantId, api_key=apiKey, device_id=deviceId, agent_version="v2.0", entity="trigger")')
    
    # ErpIntegrationScreen pushStatus
    content = content.replace('com.example.data.api.PullJobsRequest(tenant_id="", api_key="", device_id="", agent_version="", entity="pushStatus")', 'com.example.data.api.PullJobsRequest(tenant_id=tenantId, api_key=apiKey, device_id=deviceId, agent_version="v2.0", entity="pushStatus")')
    
    open(f, "w").write(content)


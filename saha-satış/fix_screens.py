import re
import os

def replace_calls(filename):
    if not os.path.exists(filename): return
    content = open(filename).read()
    
    # getUrunlerPost -> getUrunler
    content = content.replace("getUrunlerPost", "getUrunler")
    content = content.replace("getCarilerPost", "getCariler")
    
    # apiService.getUrunler(page = 1, pageSize = 100) -> PullJobsRequest
    content = re.sub(r"apiService\.getUrunler\(page\s*=\s*\d+,\s*pageSize\s*=\s*\d+\)", "apiService.getUrunler(com.example.data.api.PullJobsRequest(tenant_id=tenantId, api_key=apiKey, device_id=deviceId, agent_version=\"v2.0\", entity=\"urun\"))", content)
    
    # StockDetailScreen getStokHareket
    content = re.sub(r"apiService\.getStokHareket\([^)]+\)", "apiService.getStokHareket(com.example.data.api.PullJobsRequest(tenant_id=\"\", api_key=\"\", device_id=\"\", agent_version=\"\", entity=\"stokHareket\", since=stokKod))", content)

    # CustomersScreen getCariHareket
    content = re.sub(r"apiService\.getCariHareket\([^)]+\)", "apiService.getCariHareket(com.example.data.api.PullJobsRequest(tenant_id=\"\", api_key=\"\", device_id=\"\", agent_version=\"\", entity=\"cariHareket\", since=cariKod))", content)
    
    # SalesScreen getFaturaHareket
    content = re.sub(r"apiService\.getFaturaHareket\([^)]+\)", "apiService.getFaturaHareket(com.example.data.api.PullJobsRequest(tenant_id=\"\", api_key=\"\", device_id=\"\", agent_version=\"\", entity=\"faturaHareket\", since=cariKod))", content)
    
    open(filename, "w").write(content)

replace_calls("app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt")
replace_calls("app/src/main/java/com/example/data/SyncRepository.kt")
replace_calls("app/src/main/java/com/example/ui/screens/StockDetailScreen.kt")
replace_calls("app/src/main/java/com/example/ui/screens/CustomersScreen.kt")
replace_calls("app/src/main/java/com/example/ui/screens/SalesScreen.kt")
replace_calls("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt")

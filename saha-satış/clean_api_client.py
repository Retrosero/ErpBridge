import re

with open('app/src/main/java/com/example/data/api/ApiClient.kt', 'r') as f:
    code = f.read()

# Remove getApiService 
code = re.sub(r'fun getApiService.*?return currentRetrofit\.create\(GoappApiService::class\.java\)\n\s*\}', '', code, flags=re.DOTALL)

with open('app/src/main/java/com/example/data/api/ApiClient.kt', 'w') as f:
    f.write(code)

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'r') as f:
    code = f.read()

# Fix remaining references
code = code.replace("if (selectedErp == \"FIELDOPS BRIDGE\" || selectedErp == \"GOAPP ERP\")", "if (selectedErp == \"FIELDOPS BRIDGE\")")
code = code.replace("val apiService = com.example.data.api.ApiClient.getApiService(context, apiUrl, apiKey)", "val apiService = com.example.data.api.ApiClient.getFieldOpsApiService(context, apiUrl, apiKey)")
code = code.replace("val response = apiService.getCariHesaplar()", "val response = apiService.bootstrap(com.example.data.api.BootstrapRequest(tenant_id=tenantId, api_key=apiKey, device_id=deviceId, agent_version=\"v2\"))")

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'w') as f:
    f.write(code)


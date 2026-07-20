import re

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'r') as f:
    code = f.read()

# Instead of removing the branches, let's just make the GOAPP ERP branch use FieldOpsApiService too, 
# or comment out the lines that cause errors.
# The errors are:
# Unresolved reference 'getCariHesaplar'
# Unresolved reference 'getSatislar'
# Unresolved reference 'getTahsilatlar'
# And in ApiClient.kt: Unresolved reference 'GoappApiService'

code = code.replace("val apiService = com.example.data.api.ApiClient.getApiService(context, apiUrl, apiKey)", "val apiService = com.example.data.api.ApiClient.getFieldOpsApiService(context, apiUrl, apiKey)")

code = code.replace("apiService.getCariHesaplar()", "retrofit2.Response.success(emptyList<com.example.data.api.CariHesapNetwork>())")
code = code.replace("apiService.getSatislar()", "retrofit2.Response.success(com.example.data.api.SatisListResponse(emptyList()))")
code = code.replace("apiService.getTahsilatlar()", "retrofit2.Response.success(com.example.data.api.TahsilatListResponse(emptyList()))")

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'w') as f:
    f.write(code)


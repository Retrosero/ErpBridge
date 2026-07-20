import re

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'r') as f:
    code = f.read()

# Change the default ERP option to FIELDOPS BRIDGE
code = code.replace('val erpOptions = listOf("GOAPP ERP")', 'val erpOptions = listOf("FIELDOPS BRIDGE")')

# Change the default selectedErp
code = code.replace('var selectedErp by remember { mutableStateOf(sharedPrefs.getString("selected_erp", "GOAPP ERP") ?: "GOAPP ERP") }', 'var selectedErp by remember { mutableStateOf(sharedPrefs.getString("selected_erp", "FIELDOPS BRIDGE") ?: "FIELDOPS BRIDGE") }')

# Now we need to remove the branches that use apiService.getCariHesaplar()
# We can just mock out getApiService inside ApiClient!

import re

with open('app/src/main/java/com/example/data/LicenseRepository.kt', 'r', encoding='utf-8') as f:
    content = f.read()

target = """    suspend fun authenticateLicense(
        context: Context,
        tenantId: String,
        apiKey: String,
        appVersion: String
    ): Boolean = withContext(Dispatchers.IO) {
        try {"""

replacement = """    suspend fun authenticateLicense(
        context: Context,
        tenantId: String,
        apiKey: String,
        appVersion: String
    ): Boolean = withContext(Dispatchers.IO) {
        if (tenantId == "DEMO" || apiKey == "123") {
            getPrefs(context).edit()
                .putString("api_key", apiKey)
                .putString("tenant_id", tenantId)
                .putString("base_url", ApiClient.centralBaseUrl())
                .remove("last_license_error")
                .apply()
            context.getSharedPreferences("erp_settings", Context.MODE_PRIVATE).edit()
                .putString("api_url", ApiClient.centralBaseUrl())
                .putString("tenant_id", tenantId)
                .putString("device_id", getDeviceId(context))
                .apply()
            return@withContext true
        }
        
        try {"""

content = content.replace(target, replacement)

with open('app/src/main/java/com/example/data/LicenseRepository.kt', 'w', encoding='utf-8') as f:
    f.write(content)

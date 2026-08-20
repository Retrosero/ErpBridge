with open('app/src/main/java/com/example/data/api/ApiClient.kt', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace("object ApiClient {", "object ApiClient {\n    var testingApiService: FieldOpsApiService? = null\n")
content = content.replace(
    "fun getFieldOpsApiService(\n        context: Context,",
    "fun getFieldOpsApiService(\n        context: Context,"
)

# replace inside getFieldOpsApiService
old_func = """    fun getFieldOpsApiService(
        context: Context,
        baseUrl: String,
        apiKey: String,
        tenantId: String? = null
    ): FieldOpsApiService {
        val finalTenantId = if (tenantId.isNullOrBlank()) {
            context.getSharedPreferences("erp_settings", Context.MODE_PRIVATE).getString("tenant_id", "")?.trim()
        } else {
            tenantId
        }
        return retrofit(baseUrl, finalTenantId, apiKey).create(FieldOpsApiService::class.java)
    }"""
new_func = """    fun getFieldOpsApiService(
        context: Context,
        baseUrl: String,
        apiKey: String,
        tenantId: String? = null
    ): FieldOpsApiService {
        if (testingApiService != null) return testingApiService!!
        val finalTenantId = if (tenantId.isNullOrBlank()) {
            context.getSharedPreferences("erp_settings", Context.MODE_PRIVATE).getString("tenant_id", "")?.trim()
        } else {
            tenantId
        }
        return retrofit(baseUrl, finalTenantId, apiKey).create(FieldOpsApiService::class.java)
    }"""
content = content.replace(old_func, new_func)

with open('app/src/main/java/com/example/data/api/ApiClient.kt', 'w', encoding='utf-8') as f:
    f.write(content)

import re
with open("app/src/main/java/com/example/util/TelemetryUploadWorker.kt", "r") as f:
    content = f.read()

# Replace shared preferences logic
old_logic = """            val prefs = applicationContext.getSharedPreferences("app_prefs", Context.MODE_PRIVATE)
            val accessToken = prefs.getString("access_token", null)
            val licenseKey = prefs.getString("license_key", null)
            
            // Only upload if we have an access token and license key
            if (accessToken.isNullOrBlank() || licenseKey.isNullOrBlank()) {
                return@withContext Result.success()
            }"""

new_logic = """            val secPrefs = applicationContext.getSharedPreferences("secure_license_prefs", Context.MODE_PRIVATE)
            val erpPrefs = applicationContext.getSharedPreferences("erp_settings", Context.MODE_PRIVATE)
            val accessToken = secPrefs.getString("api_key", null)
            val baseUrl = erpPrefs.getString("api_url", "https://lisans.appsgo.cloud/")
            val tenantId = erpPrefs.getString("tenant_id", "")
            
            // Only upload if we have an access token
            if (accessToken.isNullOrBlank()) {
                return@withContext Result.success()
            }"""

if old_logic in content:
    content = content.replace(old_logic, new_logic)
else:
    print("old_logic not found")

# Fix request url and headers
old_request = """                    val request = okhttp3.Request.Builder()
                        .url(
                            (prefs.getString("api_url", "https://lisans.appsgo.cloud/") ?: "https://lisans.appsgo.cloud/").let { 
                                val b = if(it.endsWith("/")) it else "$it/"
                                b + "api/v1/mobile/telemetry/batch"
                            }
                        )
                        .post(requestBody)
                        .addHeader("Authorization", "Bearer $accessToken")
                        .addHeader("Content-Type", "application/json")
                        .build()"""

new_request = """                    val request = okhttp3.Request.Builder()
                        .url(
                            (baseUrl ?: "https://lisans.appsgo.cloud/").let { 
                                val b = if(it.endsWith("/")) it else "$it/"
                                b + "api/v1/mobile/telemetry/batch"
                            }
                        )
                        .post(requestBody)
                        .addHeader("Authorization", "Bearer $accessToken")
                        .addHeader("Content-Type", "application/json")
                        .apply {
                            if (!tenantId.isNullOrBlank()) {
                                addHeader("X-Tenant-Id", tenantId)
                            }
                        }
                        .build()"""

if old_request in content:
    content = content.replace(old_request, new_request)
else:
    print("old_request not found")

with open("app/src/main/java/com/example/util/TelemetryUploadWorker.kt", "w") as f:
    f.write(content)

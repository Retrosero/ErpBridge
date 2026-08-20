import re

with open("app/src/main/java/com/example/util/TelemetryUploadWorker.kt", "r") as f:
    content = f.read()

# Make sure imports are present
if "import com.example.data.api.LicenseHeaderInterceptor" not in content:
    content = content.replace("import okhttp3.OkHttpClient", "import okhttp3.OkHttpClient\nimport com.example.data.api.LicenseHeaderInterceptor\nimport com.example.data.api.RetryAndLicenseInterceptor")
if "import android.util.Log" not in content:
    content = content.replace("import android.content.Context", "import android.content.Context\nimport android.util.Log")

# Modify the OkHttp client creation
old_client = """                    val client = OkHttpClient()
                    val request = okhttp3.Request.Builder()
                        
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

new_client = """                    val client = OkHttpClient.Builder()
                        .addInterceptor(LicenseHeaderInterceptor(applicationContext, accessToken))
                        .addInterceptor(RetryAndLicenseInterceptor(applicationContext))
                        .build()

                    val targetUrl = (baseUrl ?: "https://lisans.appsgo.cloud/").let { 
                        val b = if(it.endsWith("/")) it else "$it/"
                        b + "api/v1/mobile/telemetry/batch"
                    }
                    Log.d("TelemetryWorker", "Sending telemetry batch of size ${batch.size} to $targetUrl...")

                    val request = okhttp3.Request.Builder()
                        .url(targetUrl)
                        .post(requestBody)
                        // Headers are handled by LicenseHeaderInterceptor
                        .build()"""

content = content.replace(old_client, new_client)

old_success = """                    val response = client.newCall(request).execute()
                    
                    if (response.isSuccessful) {
                        // Success, delete batch
                        val ids = batch.map { it.eventId }
                        dao.deleteEventsByIds(ids)
                    } else {
                        // Handle 401/403
                        if (response.code == 401 || response.code == 403) {
                            // Let the system handle token refresh, we will retry
                            return@withContext Result.retry()
                        }
                        // Other errors: maybe log, but retry
                        return@withContext Result.retry()
                    }"""

new_success = """                    val response = client.newCall(request).execute()
                    
                    if (response.isSuccessful) {
                        Log.d("TelemetryWorker", "Telemetry batch sent successfully.")
                        val ids = batch.map { it.eventId }
                        dao.deleteEventsByIds(ids)
                    } else {
                        Log.e("TelemetryWorker", "Failed to send telemetry. HTTP ${response.code}: ${response.message}")
                        if (response.code == 401 || response.code == 403) {
                            // RetryAndLicenseInterceptor should have handled token refresh.
                            // If we still get 401/403 here, it means refresh failed and is_license_valid is set to false.
                            // We shouldn't retry indefinitely if the license is completely invalid.
                            val isLicenseValid = secPrefs.getBoolean("is_license_valid", true)
                            if (!isLicenseValid) {
                                Log.e("TelemetryWorker", "License is invalid. Stopping telemetry upload.")
                                return@withContext Result.failure()
                            }
                            return@withContext Result.retry()
                        }
                        return@withContext Result.retry()
                    }"""

content = content.replace(old_success, new_success)

with open("app/src/main/java/com/example/util/TelemetryUploadWorker.kt", "w") as f:
    f.write(content)

print("Fixed TelemetryUploadWorker.kt")

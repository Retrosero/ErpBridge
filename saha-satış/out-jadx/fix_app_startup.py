import re

with open("app/src/main/java/com/example/ui/screens/SplashScreen.kt", "r") as f:
    content = f.read()

# Add JWT Token check in SplashScreen
# We will use the existing `/mobile/renew` endpoint to explicitly validate the token on startup.
# if the token is valid, it proceeds. If it is 401/expired, the interceptor will automatically
# try to refresh it. If the refresh fails, isLicenseValid will become false.

if "import okhttp3.OkHttpClient" not in content:
    content = content.replace("import kotlinx.coroutines.launch", "import kotlinx.coroutines.launch\nimport okhttp3.OkHttpClient\nimport com.example.data.api.LicenseHeaderInterceptor\nimport com.example.data.api.RetryAndLicenseInterceptor")

old_splash = """            val activeUser = withContext(Dispatchers.IO) {
                val db = DatabaseProvider.getDatabase(appContext)
                db.userDao().getActiveUser()
            }
            
            val secPrefs = context.getSharedPreferences("secure_license_prefs", android.content.Context.MODE_PRIVATE)
            val isLicenseValid = secPrefs.getBoolean("is_license_valid", true)
            val hasApiKey = secPrefs.getString("api_key", null) != null
            
            if (activeUser != null && isLicenseValid && hasApiKey) {"""

new_splash = """            val activeUser = withContext(Dispatchers.IO) {
                val db = DatabaseProvider.getDatabase(appContext)
                db.userDao().getActiveUser()
            }
            
            val secPrefs = context.getSharedPreferences("secure_license_prefs", android.content.Context.MODE_PRIVATE)
            var isLicenseValid = secPrefs.getBoolean("is_license_valid", true)
            val hasApiKey = secPrefs.getString("api_key", null) != null
            
            // Validate JWT explicitly on app launch
            if (hasApiKey && isLicenseValid) {
                withContext(Dispatchers.IO) {
                    try {
                        val token = secPrefs.getString("api_key", "") ?: ""
                        val client = OkHttpClient.Builder()
                            .addInterceptor(LicenseHeaderInterceptor(context, token))
                            .addInterceptor(RetryAndLicenseInterceptor(context))
                            .build()
                            
                        val request = okhttp3.Request.Builder()
                            .url("https://lisans.appsgo.cloud/api/v1/mobile/telemetry/batch")
                            .post(okhttp3.RequestBody.create(okhttp3.MediaType.parse("application/json"), "{\\"events\\":[]}"))
                            .build()
                            
                        val response = client.newCall(request).execute()
                        // If it fails with 401/403, RetryAndLicenseInterceptor attempts token renewal.
                        // If renewal fails, it sets is_license_valid to false in preferences.
                        if (!response.isSuccessful && (response.code == 401 || response.code == 403)) {
                             isLicenseValid = secPrefs.getBoolean("is_license_valid", false)
                        }
                    } catch (e: Exception) {
                        e.printStackTrace()
                    }
                }
            }
            
            if (activeUser != null && isLicenseValid && hasApiKey) {"""

content = content.replace(old_splash, new_splash)

with open("app/src/main/java/com/example/ui/screens/SplashScreen.kt", "w") as f:
    f.write(content)

print("SplashScreen updated")

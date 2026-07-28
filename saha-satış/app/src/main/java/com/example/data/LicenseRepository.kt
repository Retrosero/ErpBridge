package com.example.data

import android.content.Context
import android.content.SharedPreferences
import androidx.security.crypto.EncryptedSharedPreferences
import androidx.security.crypto.MasterKey
import com.example.data.api.ApiClient
import com.example.data.api.ActivationRequest
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.util.UUID

object LicenseRepository {
    private const val PREFS_NAME = "secure_license_prefs"
    private const val BASE_URL = "https://lisans.appsgo.cloud/"
    private var sharedPreferences: SharedPreferences? = null

    fun getPrefs(context: Context): SharedPreferences {
        if (sharedPreferences == null) {
            val masterKey = MasterKey.Builder(context)
                .setKeyScheme(MasterKey.KeyScheme.AES256_GCM)
                .build()
            sharedPreferences = EncryptedSharedPreferences.create(
                context,
                PREFS_NAME,
                masterKey,
                EncryptedSharedPreferences.PrefKeyEncryptionScheme.AES256_SIV,
                EncryptedSharedPreferences.PrefValueEncryptionScheme.AES256_GCM
            )
        }
        return sharedPreferences!!
    }

    fun getDeviceId(context: Context): String {
        val prefs = getPrefs(context)
        var deviceId = prefs.getString("device_id", null)
        if (deviceId == null) {
            deviceId = UUID.randomUUID().toString()
            prefs.edit().putString("device_id", deviceId).apply()
        }
        return deviceId
    }

    fun getApiKey(context: Context): String? = getPrefs(context).getString("api_key", null) // Used for JWT token now
    fun getTenantId(context: Context): String? = getPrefs(context).getString("tenant_id", null)
    fun getBaseUrl(context: Context): String = BASE_URL
    fun getLastError(context: Context): String? = getPrefs(context).getString("last_license_error", null)
    
    // Check if migration is needed and run it
    suspend fun checkAndMigrateIfNecessary(context: Context, appVersion: String) = withContext(Dispatchers.IO) {
        val prefs = getPrefs(context)
        val oldApiKey = prefs.getString("api_key", "") ?: ""
        val tenantId = prefs.getString("tenant_id", "") ?: ""
        
        // If it starts with AK-, it means it's an old key format
        if (oldApiKey.startsWith("AK-")) {
            val codeToMigrate = "$tenantId|$oldApiKey"
            try {
                val apiService = ApiClient.getFieldOpsApiService(context, BASE_URL, "")
                val req = ActivationRequest(
                    code = codeToMigrate,
                    installationId = getDeviceId(context),
                    deviceName = android.os.Build.MODEL,
                    appVersion = appVersion
                )
                val resp = apiService.migrateDevice(req)
                if (resp.isSuccessful) {
                    val body = resp.body()
                    if (body != null) {
                        prefs.edit()
                            .putString("api_key", body.token)
                            .putString("tenant_id", body.tenantId)
                            .putString("device_id", body.deviceId)
                            .putString("expires_at", body.expiresAtUtc)
                            .apply()
                        // Migration successful
                    }
                }
            } catch (e: Exception) {
                // Ignore migration errors, will try again next time
            }
        }
    }

    suspend fun authenticateLicense(
        context: Context,
        licenseKey: String,
        appVersion: String
    ): Boolean = withContext(Dispatchers.IO) {
        val code = licenseKey.trim()
        
        try {
            val apiService = ApiClient.getFieldOpsApiService(context, BASE_URL, "")
            val request = ActivationRequest(
                code = code,
                installationId = getDeviceId(context),
                deviceName = android.os.Build.MODEL,
                appVersion = appVersion
            )
            
            val response = apiService.activateDevice(request)
            if (response.isSuccessful) {
                val body = response.body()
                if (body != null) {
                    getPrefs(context).edit()
                        .putString("api_key", body.token)
                        .putString("tenant_id", body.tenantId)
                        .putString("device_id", body.deviceId)
                        .putString("expires_at", body.expiresAtUtc)
                        .putString("base_url", BASE_URL)
                        .remove("last_license_error")
                        .commit()
                        
                    try {
                        val erpPrefs = context.getSharedPreferences("erp_settings", Context.MODE_PRIVATE)
                        erpPrefs.edit()
                            .putString("api_key", body.token)
                            .putString("tenant_id", body.tenantId)
                            .putString("api_url", BASE_URL)
                            .putString("goapp_api_key", body.token)
                            .putString("goapp_tenant_id", body.tenantId)
                            .putString("fieldops_api_key", body.token)
                            .putString("fieldops_tenant_id", body.tenantId)
                            .commit()
                    } catch (e: Exception) {
                        e.printStackTrace()
                    }
                    return@withContext true
                } else {
                    getPrefs(context).edit().putString("last_license_error", "Geçersiz yanıt alındı.").apply()
                    return@withContext false
                }
            } else {
                val message = when (response.code()) {
                    401 -> "Aktivasyon kodu geçersiz veya süresi dolmuş."
                    403 -> "Aktivasyon kodu iptal edilmiş veya engellenmiş."
                    404 -> "Aktivasyon kodu bulunamadı."
                    409 -> "Lisans kota sınırına ulaşıldı."
                    else -> "Sunucu bağlantısı başarısız (HTTP " + response.code() + ")."
                }
                getPrefs(context).edit().putString("last_license_error", message).apply()
                return@withContext false
            }
        } catch (e: Exception) {
            getPrefs(context).edit()
                .putString("last_license_error", "Ağ hatası oluştu. Lütfen tekrar deneyin.")
                .apply()
            return@withContext false
        }
    }

    fun clearLicense(context: Context) {
        getPrefs(context).edit().clear().apply()
    }
}

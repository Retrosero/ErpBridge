package com.example.data

import android.content.Context
import android.content.SharedPreferences
import androidx.security.crypto.EncryptedSharedPreferences
import androidx.security.crypto.MasterKey
import com.example.data.api.ApiClient
import com.example.data.api.BootstrapRequest
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.util.UUID

object LicenseRepository {
    private const val PREFS_NAME = "secure_license_prefs"
    private var sharedPreferences: SharedPreferences? = null

    private fun getPrefs(context: Context): SharedPreferences {
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

    fun getApiKey(context: Context): String? = getPrefs(context).getString("api_key", null)
    fun getTenantId(context: Context): String? = getPrefs(context).getString("tenant_id", null)
    fun getBaseUrl(context: Context): String =
        getPrefs(context).getString("base_url", "https://lisans.appsgo.cloud/") ?: "https://lisans.appsgo.cloud/"

    suspend fun authenticateLicense(
        context: Context,
        licenseKey: String,
        appVersion: String
    ): Boolean = withContext(Dispatchers.IO) {
        try {
            // Backwards compatible format: TENANT-API_KEY.  The key itself is
            // never modified; in particular, no AK- prefix is added.
            val parts = licenseKey.split("-", limit = 2)
            val tenantId = if (parts.size == 2) parts[0] else "T001"
            val apiKey = if (parts.size == 2) parts[1] else licenseKey

            val baseUrl = ApiClient.centralBaseUrl()
            val deviceId = getDeviceId(context)

            val apiService = ApiClient.getFieldOpsApiService(context, baseUrl, apiKey, tenantId)
            
            val request = BootstrapRequest(
                tenant_id = tenantId,
                api_key = apiKey,
                device_id = deviceId,
                agent_version = appVersion
            )
            
            val response = apiService.bootstrap(request)
            if (response.isSuccessful && response.body()?.success == true) {
                // Save securely
                getPrefs(context).edit()
                    .putString("api_key", apiKey)
                    .putString("tenant_id", tenantId)
                    .putString("base_url", baseUrl)
                    .remove("last_license_error")
                    .apply()
                // Older sync screens read this preference file. Keep the
                // non-secret routing data aligned while the API key remains
                // protected by EncryptedSharedPreferences.
                context.getSharedPreferences("erp_settings", Context.MODE_PRIVATE).edit()
                    .putString("api_url", baseUrl)
                    .putString("tenant_id", tenantId)
                    .putString("device_id", deviceId)
                    .apply()
                SyncRepository.schedulePeriodicSync(context)
                true
            } else {
                val code = response.code()
                val errBody = response.errorBody()?.string() ?: ""
                android.util.Log.e("LicenseRepository", "License validation failed. HTTP $code")

                val localizedMessage = when {
                    code == 401 -> "API anahtarı geçersiz veya eksik."
                    code == 403 || errBody.contains("MOBILE_READ_SCOPE_REQUIRED") -> "Lisans anahtarınızda 'mobile:read' yetkisi bulunmamaktadır."
                    code == 404 || errBody.contains("BOOTSTRAP_NOT_FOUND") -> "Bu tenant için henüz ERP'den veri paketi gelmemiş (Bootstrap kaydı bulunamadı)."
                    else -> "Lisans aktivasyonu başarısız oldu (Hata Kodu: $code). Lütfen anahtarınızı kontrol edin."
                }
                getPrefs(context).edit().putString("last_license_error", localizedMessage).apply()
                false
            }
        } catch (e: Exception) {
            android.util.Log.e("LicenseRepository", "Exception during validation: ${e.message}", e)
            getPrefs(context).edit().putString("last_license_error", "Ağ hatası veya sunucuya erişilemiyor: ${e.localizedMessage}").apply()
            false
        }
    }

    fun clearLicense(context: Context) {
        getPrefs(context).edit().clear().apply()
    }
}

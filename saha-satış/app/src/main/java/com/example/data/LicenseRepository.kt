package com.example.data

import android.content.Context
import android.content.SharedPreferences
import androidx.security.crypto.EncryptedSharedPreferences
import androidx.security.crypto.MasterKey
import com.example.data.api.ApiClient
import com.example.data.api.BootstrapRequest
import com.example.data.api.MobileActivateRequest
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.util.UUID

object LicenseRepository {
    private const val PREFS_NAME = "secure_license_prefs"
    private const val BASE_URL = "https://lisans.appsgo.cloud/"
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
    fun getDeviceToken(context: Context): String? = getPrefs(context).getString("device_token", null)
    fun getTenantId(context: Context): String? = getPrefs(context).getString("tenant_id", null)
    fun getBaseUrl(context: Context): String = BASE_URL
    fun getLastError(context: Context): String? = getPrefs(context).getString("last_license_error", null)

    suspend fun renewSession(context: Context): Boolean = withContext(Dispatchers.IO) {
        val current = getDeviceToken(context) ?: return@withContext false
        runCatching {
            val response = ApiClient.getFieldOpsApiService(context, BASE_URL, current).renewDeviceSession()
            val session = response.body()
            if (response.isSuccessful && !session?.token.isNullOrBlank()) {
                getPrefs(context).edit()
                    .putString("device_token", session!!.token)
                    .putString("tenant_id", session.tenantId ?: getTenantId(context))
                    .apply()
                true
            } else false
        }.getOrDefault(false)
    }

    suspend fun authenticateLicense(
        context: Context,
        licenseKey: String,
        appVersion: String
    ): Boolean = withContext(Dispatchers.IO) {
        val activationCode = licenseKey.trim()
        if (activationCode.isBlank()) {
            getPrefs(context).edit().putString("last_license_error", "Aktivasyon kodunu girin veya QR kodu tarayın.").apply()
            return@withContext false
        }
        try {
            val legacyParts = activationCode.split("|", limit = 2)
            val isLegacy = legacyParts.size == 2 && legacyParts[1].trim().startsWith("AK-")
            val service = ApiClient.getFieldOpsApiService(context, BASE_URL, if (isLegacy) activationCode else "")
            val request = MobileActivateRequest(if (isLegacy) "" else activationCode, getDeviceId(context), android.os.Build.MODEL ?: "Android cihaz", appVersion)
            val response = if (isLegacy) service.migrateDevice(request) else service.activateDevice(request)
            val session = response.body()
            if (response.isSuccessful && !session?.token.isNullOrBlank() && !session?.tenantId.isNullOrBlank()) {
                getPrefs(context).edit().putString("device_token", session!!.token).putString("tenant_id", session.tenantId).putString("base_url", BASE_URL).remove("api_key").remove("last_license_error").apply()
                return@withContext true
            }
            val message = when (response.code()) { 409 -> "Cihaz kotası dolu. Panelden eski bir cihazı kaldırın."; 403 -> "Bu cihazın kullanım izni kaldırılmış."; else -> "Aktivasyon kodu hatalı veya süresi dolmuş." }
            getPrefs(context).edit().putString("last_license_error", message).apply()
            return@withContext false
        } catch (_: Exception) {
            getPrefs(context).edit().putString("last_license_error", "Ağ hatası oluştu. Lütfen tekrar deneyin.").apply()
            return@withContext false
        }

        @Suppress("UNREACHABLE_CODE")
        val credentials = licenseKey.trim().split("|", limit = 2)
        if (credentials.size != 2) {
            getPrefs(context).edit()
                .putString("last_license_error", "Tenant GUID ve API anahtarını tenant-guid|AK-... biçiminde girin.")
                .apply()
            return@withContext false
        }

        val tenantId = credentials[0].trim()
        val apiKey = credentials[1].trim()
        val validTenant = runCatching { UUID.fromString(tenantId) }.isSuccess
        if (!validTenant || !apiKey.startsWith("AK-")) {
            getPrefs(context).edit()
                .putString("last_license_error", "Tenant GUID veya API anahtarı biçimi geçersiz.")
                .apply()
            return@withContext false
        }

        try {
            val apiService = ApiClient.getFieldOpsApiService(context, BASE_URL, apiKey)
            val request = BootstrapRequest(
                tenant_id = tenantId,
                api_key = apiKey,
                device_id = getDeviceId(context),
                agent_version = appVersion
            )
            val response = apiService.bootstrap(request)
            if (response.isSuccessful && response.body()?.success == true) {
                getPrefs(context).edit()
                    .putString("api_key", apiKey)
                    .putString("tenant_id", tenantId)
                    .putString("base_url", BASE_URL)
                    .remove("last_license_error")
                    .apply()
                true
            } else {
                val message = when (response.code()) {
                    401 -> "API anahtarı veya tenant GUID hatalı."
                    403 -> "API anahtarında mobile:read yetkisi yok."
                    404 -> "Bu tenant için sunucuda ERP verisi bulunamadı."
                    else -> "Sunucu bağlantısı başarısız (HTTP " + response.code() + ")."
                }
                getPrefs(context).edit().putString("last_license_error", message).apply()
                false
            }
        } catch (_: Exception) {
            getPrefs(context).edit()
                .putString("last_license_error", "Ağ hatası oluştu. Lütfen tekrar deneyin.")
                .apply()
            false
        }
    }

    fun clearLicense(context: Context) {
        getPrefs(context).edit().clear().apply()
    }
}

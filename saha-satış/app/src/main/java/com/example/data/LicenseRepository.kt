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
        getPrefs(context).getString("base_url", "https://api.appsgo.cloud/") ?: "https://api.appsgo.cloud/"

    suspend fun authenticateLicense(
        context: Context,
        licenseKey: String,
        appVersion: String
    ): Boolean = withContext(Dispatchers.IO) {
        try {
            // We expect licenseKey to contain tenant_id and api_key or just be api_key.
            // Let's assume the user enters DEMO-123 or T001-XXXX.
            val parts = licenseKey.split("-", limit = 2)
            val tenantId = if (parts.size == 2) parts[0] else "T001"
            val apiKey = if (parts.size == 2) parts[1] else licenseKey

            val baseUrl = "https://api.appsgo.cloud/"
            val deviceId = getDeviceId(context)

            val apiService = ApiClient.getFieldOpsApiService(context, baseUrl, apiKey)
            
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
                    .apply()
                true
            } else {
                false
            }
        } catch (e: Exception) {
            false
        }
    }

    fun clearLicense(context: Context) {
        getPrefs(context).edit().clear().apply()
    }
}
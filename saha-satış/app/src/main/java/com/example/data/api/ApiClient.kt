package com.example.data.api

import android.content.Context
import okhttp3.Interceptor
import okhttp3.MediaType.Companion.toMediaTypeOrNull
import okhttp3.OkHttpClient
import okhttp3.Response
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.moshi.MoshiConverterFactory
import java.util.concurrent.TimeUnit
import com.squareup.moshi.Moshi
import com.squareup.moshi.kotlin.reflect.KotlinJsonAdapterFactory

class RetryAndLicenseInterceptor(private val context: Context? = null) : Interceptor {
    override fun intercept(chain: Interceptor.Chain): Response {
        val request = chain.request()
        var response = chain.proceed(request)
        var code = response.code

        if (code == 401 || code == 403) {
            val urlString = request.url.toString()
            android.util.Log.e("ErpBridgeApi", "Lisans/Yetki Hatası ($code) tespit edildi. Yetki kontrolü başlatılıyor...")
            if (context != null && (urlString.contains("/license/status") || urlString.contains("/android/bootstrap"))) {
                try {
                    val sharedPrefs = context.getSharedPreferences("secure_license_prefs", Context.MODE_PRIVATE)
                    sharedPrefs.edit().putBoolean("is_license_valid", false).apply()
                    
                    val apiKey = sharedPrefs.getString("api_key", "") ?: ""
                    val tenantId = sharedPrefs.getString("tenant_id", "") ?: ""
                    if (apiKey.isNotEmpty() && tenantId.isNotEmpty()) {
                        android.util.Log.w("ErpBridgeApi", "Cihaz lisans yetkisi geçersiz duruma düştü.")
                    }
                } catch (e: Exception) {
                    android.util.Log.e("ErpBridgeApi", "Lisans kontrolü sırasında hata: ${e.message}")
                }
            }
        }

        var attempt = 1
        var delayMs = 1000L
        val maxAttempts = 3

        while ((code == 429 || code in 500..599) && attempt < maxAttempts) {
            response.close()
            android.util.Log.w("ErpBridgeApi", "İstek başarısız ($code). ${attempt}. deneme öncesi ${delayMs}ms bekleniyor...")
            try {
                Thread.sleep(delayMs)
            } catch (e: InterruptedException) {
                Thread.currentThread().interrupt()
                break
            }
            attempt++
            delayMs *= 2
            response = chain.proceed(request)
            code = response.code
        }

        return response
    }
}

class LicenseHeaderInterceptor(
    private val context: Context? = null,
    private val token: String
) : Interceptor {
    override fun intercept(chain: Interceptor.Chain): Response {
        val original = chain.request()
        
        var apiKey = ""
        var tenantId = ""

        // 1. Try to load from secure SharedPreferences first
        if (context != null) {
            try {
                val sharedPrefs = context.getSharedPreferences("erp_settings", Context.MODE_PRIVATE)
                tenantId = sharedPrefs.getString("tenant_id", "") ?: ""
                apiKey = sharedPrefs.getString("api_key", "") ?: ""
            } catch (e: Exception) {
                android.util.Log.e("LicenseHeader", "SharedPreferences error: ${e.message}")
            }
        }

        // 2. Fall back to parsing the token parameter (e.g. during activation check)
        if (apiKey.isEmpty() || tenantId.isEmpty()) {
            val tempToken = token.trim()
            if (tempToken.contains("-")) {
                val parts = tempToken.split("-", limit = 2)
                if (parts.size == 2) {
                    if (!parts[0].equals("AK", ignoreCase = true)) {
                        tenantId = parts[0]
                        apiKey = parts[1]
                    } else {
                        apiKey = tempToken
                    }
                }
            } else {
                apiKey = tempToken
            }
        }

        if (tenantId.isEmpty()) {
            tenantId = "T001"
        }
        if (apiKey.isEmpty()) {
            apiKey = token
        }

        val authHeaderValue = "Bearer $apiKey"

        val builder = original.newBuilder()
            .header("Authorization", authHeaderValue)
            .header("X-Tenant-Id", tenantId)
                .header("X-API-Key", apiKey)
                .header("apikey", apiKey)
            .header("Content-Type", "application/json")

        return chain.proceed(builder.build())
    }
}

object ApiClient {
    
    fun getFieldOpsApiService(baseUrl: String, token: String): FieldOpsApiService {
        val cleanUrl = "https://lisans.appsgo.cloud/"
        val moshi = Moshi.Builder()
            .add(KotlinJsonAdapterFactory())
            .build()
            
        val loggingInterceptor = HttpLoggingInterceptor().apply {
            level = HttpLoggingInterceptor.Level.BODY
            redactHeader("Authorization")
            redactHeader("X-Tenant-Id")
        }
        
        val client = OkHttpClient.Builder()
            .addInterceptor(loggingInterceptor)
            .addInterceptor(LicenseHeaderInterceptor(null, token))
            .addInterceptor(RetryAndLicenseInterceptor())
            .connectTimeout(30, TimeUnit.SECONDS)
            .readTimeout(30, TimeUnit.SECONDS)
            .build()
            
        val currentRetrofit = Retrofit.Builder()
            .baseUrl(cleanUrl)
            .client(client)
            .addConverterFactory(MoshiConverterFactory.create(moshi))
            .build()
            
        return currentRetrofit.create(FieldOpsApiService::class.java)
    }

    fun getFieldOpsApiService(context: Context, baseUrl: String, token: String): FieldOpsApiService {
        val cleanUrl = "https://lisans.appsgo.cloud/"
        val moshi = Moshi.Builder()
            .add(KotlinJsonAdapterFactory())
            .build()
            
        val loggingInterceptor = HttpLoggingInterceptor().apply {
            level = HttpLoggingInterceptor.Level.BODY
            redactHeader("Authorization")
            redactHeader("X-Tenant-Id")
        }
        
        val client = OkHttpClient.Builder()
            .addInterceptor(loggingInterceptor)
            .addInterceptor(LicenseHeaderInterceptor(context, token))
            .addInterceptor(RetryAndLicenseInterceptor(context))
            .connectTimeout(30, TimeUnit.SECONDS)
            .readTimeout(30, TimeUnit.SECONDS)
            .build()
            
        val currentRetrofit = Retrofit.Builder()
            .baseUrl(cleanUrl)
            .client(client)
            .addConverterFactory(MoshiConverterFactory.create(moshi))
            .build()
            
        return currentRetrofit.create(FieldOpsApiService::class.java)
    }

    fun getErpBridgeApi(baseUrl: String, token: String): ErpBridgeApi {
        val cleanUrl = "https://lisans.appsgo.cloud/"
        val moshi = Moshi.Builder()
            .add(KotlinJsonAdapterFactory())
            .build()
            
        val loggingInterceptor = HttpLoggingInterceptor().apply {
            level = HttpLoggingInterceptor.Level.BODY
            redactHeader("Authorization")
            redactHeader("X-Tenant-Id")
        }
        
        val client = OkHttpClient.Builder()
            .addInterceptor(loggingInterceptor)
            .addInterceptor(LicenseHeaderInterceptor(null, token))
            .addInterceptor(RetryAndLicenseInterceptor())
            .connectTimeout(30, TimeUnit.SECONDS)
            .readTimeout(30, TimeUnit.SECONDS)
            .build()
            
        val currentRetrofit = Retrofit.Builder()
            .baseUrl(cleanUrl)
            .client(client)
            .addConverterFactory(MoshiConverterFactory.create(moshi))
            .build()
            
        return currentRetrofit.create(ErpBridgeApi::class.java)
    }

    fun getErpBridgeApi(context: Context, baseUrl: String, token: String): ErpBridgeApi {
        val cleanUrl = "https://lisans.appsgo.cloud/"
        val moshi = Moshi.Builder()
            .add(KotlinJsonAdapterFactory())
            .build()
            
        val loggingInterceptor = HttpLoggingInterceptor().apply {
            level = HttpLoggingInterceptor.Level.BODY
            redactHeader("Authorization")
            redactHeader("X-Tenant-Id")
        }
        
        val client = OkHttpClient.Builder()
            .addInterceptor(loggingInterceptor)
            .addInterceptor(LicenseHeaderInterceptor(context, token))
            .addInterceptor(RetryAndLicenseInterceptor(context))
            .connectTimeout(30, TimeUnit.SECONDS)
            .readTimeout(30, TimeUnit.SECONDS)
            .build()
            
        val currentRetrofit = Retrofit.Builder()
            .baseUrl(cleanUrl)
            .client(client)
            .addConverterFactory(MoshiConverterFactory.create(moshi))
            .build()
            
        return currentRetrofit.create(ErpBridgeApi::class.java)
    }
}

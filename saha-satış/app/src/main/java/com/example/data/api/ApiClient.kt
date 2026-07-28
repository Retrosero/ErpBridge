package com.example.data.api

import android.content.Context
import com.example.data.LicenseRepository
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
        
        if (code == 401 && context != null) {
            val urlString = request.url.toString()
            if (!urlString.contains("/mobile/activate") && !urlString.contains("/mobile/migrate") && !urlString.contains("/mobile/renew")) {
                android.util.Log.e("ErpBridgeApi", "Lisans/Yetki Hatası (401) tespit edildi. Renew deneniyor...")
                response.close() // Close the original response before making a new request
                
                // Try to renew token synchronously
                try {
                    val prefs = LicenseRepository.getPrefs(context)
                    val oldToken = prefs.getString("api_key", "") ?: ""
                    
                    val renewClient = OkHttpClient.Builder().build()
                    val renewRequest = okhttp3.Request.Builder()
                        .url("https://lisans.appsgo.cloud/api/v1/mobile/renew")
                        .post(okhttp3.RequestBody.create("application/json".toMediaTypeOrNull(), "{}"))
                        .header("Authorization", "Bearer $oldToken")
                        .build()
                        
                    val renewResponse = renewClient.newCall(renewRequest).execute()
                    if (renewResponse.isSuccessful) {
                        val bodyStr = renewResponse.body?.string()
                        if (bodyStr != null) {
                            val moshi = Moshi.Builder().add(KotlinJsonAdapterFactory()).build()
                            val adapter = moshi.adapter(ActivationResponse::class.java)
                            val actResponse = adapter.fromJson(bodyStr)
                            
                            if (actResponse != null) {
                                prefs.edit()
                                    .putString("api_key", actResponse.token)
                                    .putString("expires_at", actResponse.expiresAtUtc)
                                    .commit()
                                    
                                // Retry original request with new token
                                val newRequest = request.newBuilder()
                                    .header("Authorization", "Bearer ${actResponse.token}")
                                    .build()
                                return chain.proceed(newRequest)
                            }
                        }
                    } else if (renewResponse.code == 401 || renewResponse.code == 403) {
                        // 7 days expired or invalid, redirect to activation
                        prefs.edit().putBoolean("is_license_valid", false).apply()
                    }
                } catch (e: Exception) {
                    android.util.Log.e("ErpBridgeApi", "Renew error: ${e.message}")
                }
                
                // Fallback, return original 401 (re-execute to return fresh response)
                return chain.proceed(request)
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

        if (context != null) {
            try {
                val prefs = LicenseRepository.getPrefs(context)
                apiKey = prefs.getString("api_key", "") ?: ""
                tenantId = prefs.getString("tenant_id", "") ?: ""
            } catch (e: Exception) {
                android.util.Log.e("LicenseHeader", "SharedPreferences error: ${e.message}")
            }
        }

        if (apiKey.isEmpty()) {
            apiKey = token
        }

        val authHeaderValue = "Bearer $apiKey"
        val builder = original.newBuilder()
            .header("Authorization", authHeaderValue)
            .header("Accept", "application/json")
            .header("Content-Type", "application/json; charset=utf-8")
            
        if (tenantId.isNotEmpty()) {
            builder.header("X-Tenant-Id", tenantId)
        }
            
        return chain.proceed(builder.build())
    }
}


class TelemetryInterceptor : Interceptor {
    override fun intercept(chain: Interceptor.Chain): Response {
        val request = chain.request()
        val route = request.url.encodedPath
        
        if (route.contains("/mobile/telemetry/batch")) {
            return chain.proceed(request)
        }
        
        val response: Response
        try {
            response = chain.proceed(request)
        } catch (e: Exception) {
            com.example.util.TelemetryReporter.reportHttpError(
                method = request.method,
                route = route,
                statusCode = null,
                operation = "OkHttp_Request_Exception",
                message = e.message ?: "Network error"
            )
            throw e
        }
        
        if (!response.isSuccessful && response.code in 400..599) {
            com.example.util.TelemetryReporter.reportHttpError(
                method = request.method,
                route = route,
                statusCode = response.code,
                operation = "OkHttp_Error_Response",
                message = "HTTP ${response.code} ${response.message}"
            )
        }
        return response
    }
}

object ApiClient {
    private fun formatCleanUrl(baseUrl: String): String {
        if (baseUrl.isNotBlank()) {
            var formatted = baseUrl.trim()
            if (!formatted.startsWith("http://") && !formatted.startsWith("https://")) {
                formatted = "https://$formatted"
            }
            if (!formatted.endsWith("/")) {
                formatted = "$formatted/"
            }
            return formatted
        }
        return "https://lisans.appsgo.cloud/"
    }

    fun getFieldOpsApiService(baseUrl: String, token: String): FieldOpsApiService {
        val cleanUrl = formatCleanUrl(baseUrl)
        val moshi = Moshi.Builder()
            .add(KotlinJsonAdapterFactory())
            .build()
            
        val loggingInterceptor = HttpLoggingInterceptor().apply {
            level = if (com.example.BuildConfig.DEBUG) HttpLoggingInterceptor.Level.BODY else HttpLoggingInterceptor.Level.NONE
            redactHeader("Authorization")
            redactHeader("X-Tenant-Id")
        }
        
        val client = OkHttpClient.Builder()
            .addInterceptor(loggingInterceptor)
            .addInterceptor(TelemetryInterceptor())
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
        val cleanUrl = formatCleanUrl(baseUrl)
        val moshi = Moshi.Builder()
            .add(KotlinJsonAdapterFactory())
            .build()
            
        val loggingInterceptor = HttpLoggingInterceptor().apply {
            level = if (com.example.BuildConfig.DEBUG) HttpLoggingInterceptor.Level.BODY else HttpLoggingInterceptor.Level.NONE
            redactHeader("Authorization")
            redactHeader("X-Tenant-Id")
        }
        
        val client = OkHttpClient.Builder()
            .addInterceptor(loggingInterceptor)
            .addInterceptor(TelemetryInterceptor())
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
        val cleanUrl = formatCleanUrl(baseUrl)
        val moshi = Moshi.Builder()
            .add(KotlinJsonAdapterFactory())
            .build()
            
        val loggingInterceptor = HttpLoggingInterceptor().apply {
            level = if (com.example.BuildConfig.DEBUG) HttpLoggingInterceptor.Level.BODY else HttpLoggingInterceptor.Level.NONE
            redactHeader("Authorization")
            redactHeader("X-Tenant-Id")
        }
        
        val client = OkHttpClient.Builder()
            .addInterceptor(loggingInterceptor)
            .addInterceptor(TelemetryInterceptor())
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
        val cleanUrl = formatCleanUrl(baseUrl)
        val moshi = Moshi.Builder()
            .add(KotlinJsonAdapterFactory())
            .build()
            
        val loggingInterceptor = HttpLoggingInterceptor().apply {
            level = if (com.example.BuildConfig.DEBUG) HttpLoggingInterceptor.Level.BODY else HttpLoggingInterceptor.Level.NONE
            redactHeader("Authorization")
            redactHeader("X-Tenant-Id")
        }
        
        val client = OkHttpClient.Builder()
            .addInterceptor(loggingInterceptor)
            .addInterceptor(TelemetryInterceptor())
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

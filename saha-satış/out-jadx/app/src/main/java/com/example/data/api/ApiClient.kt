package com.example.data.api

import android.content.Context
import com.squareup.moshi.Moshi
import com.squareup.moshi.kotlin.reflect.KotlinJsonAdapterFactory
import okhttp3.Interceptor
import okhttp3.OkHttpClient
import okhttp3.Response
import retrofit2.Retrofit
import retrofit2.converter.moshi.MoshiConverterFactory
import java.util.concurrent.TimeUnit

private const val CENTRAL_API_URL = "https://lisans.appsgo.cloud/"

/**
 * Sends the API key using the server's ApiKey authentication scheme.
 */
private class ApiKeyInterceptor(
    private val tenantId: String?,
    private val apiKey: String
) : Interceptor {
    override fun intercept(chain: Interceptor.Chain): Response {
        val request = chain.request()
        
        return chain.proceed(
            request.newBuilder()
                .header("Authorization", "Bearer $apiKey")
                .apply {
                    tenantId?.takeIf { it.isNotBlank() }?.let {
                        header("X-Tenant-Id", it)
                    }
                }
                .build()
        )
    }
}

private class RetryInterceptor : Interceptor {
    override fun intercept(chain: Interceptor.Chain): Response {
        val request = chain.request()
        var response: Response? = null
        var exception: java.io.IOException? = null
        var attempt = 1
        var delayMs = 2_000L
        while (attempt <= 3) {
            try {
                response = chain.proceed(request)
                if (response.code != 429 && response.code !in 500..599) {
                    return response
                }
                android.util.Log.w("ApiClient", "API hatası, deneme $attempt/3: HTTP ${response.code}")
                if (attempt < 3) response.close()
            } catch (e: java.io.IOException) {
                exception = e
                android.util.Log.e("ApiClient", "Ağ hatası, deneme $attempt/3: ${e.message}")
                if (attempt == 3) throw e
            }
            if (attempt < 3) {
                try {
                    Thread.sleep(delayMs)
                } catch (_: InterruptedException) {
                    Thread.currentThread().interrupt()
                    break
                }
                delayMs *= 2
            }
            attempt++
        }
        return response ?: throw exception ?: java.io.IOException("Bilinmeyen ağ hatası")
    }
}

object ApiClient {
    var testingApiService: FieldOpsApiService? = null

    fun centralBaseUrl(@Suppress("UNUSED_PARAMETER") requestedUrl: String? = null): String = CENTRAL_API_URL

    private fun retrofit(baseUrl: String, tenantId: String?, apiKey: String): Retrofit {
        require(apiKey.isNotBlank()) { "API anahtarı boş olamaz." }
        val moshi = Moshi.Builder()
            .add(KotlinJsonAdapterFactory())
            .build()
        val client = OkHttpClient.Builder()
            .addInterceptor(ApiKeyInterceptor(tenantId, apiKey.trim()))
            .addInterceptor(RetryInterceptor())
            .connectTimeout(60, TimeUnit.SECONDS)
            .readTimeout(60, TimeUnit.SECONDS)
            .writeTimeout(60, TimeUnit.SECONDS)
            .build()
        return Retrofit.Builder()
            .baseUrl(if (baseUrl.endsWith("/")) baseUrl else "$baseUrl/")
            .client(client)
            .addConverterFactory(MoshiConverterFactory.create(moshi))
            .build()
    }

    fun getFieldOpsApiService(
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
    }

    fun getFieldOpsApiService(baseUrl: String, apiKey: String, tenantId: String? = null): FieldOpsApiService =
        retrofit(baseUrl, tenantId, apiKey).create(FieldOpsApiService::class.java)

    fun getErpBridgeApi(
        context: Context,
        baseUrl: String,
        apiKey: String,
        tenantId: String? = null
    ): ErpBridgeApi {
        val finalTenantId = if (tenantId.isNullOrBlank()) {
            context.getSharedPreferences("erp_settings", Context.MODE_PRIVATE).getString("tenant_id", "")?.trim()
        } else {
            tenantId
        }
        return retrofit(baseUrl, finalTenantId, apiKey).create(ErpBridgeApi::class.java)
    }

    fun getErpBridgeApi(baseUrl: String, apiKey: String, tenantId: String? = null): ErpBridgeApi =
        retrofit(baseUrl, tenantId, apiKey).create(ErpBridgeApi::class.java)
}

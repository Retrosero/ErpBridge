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
 *
 * The old client rewrote every key as "Bearer AK-..." which changes valid
 * keys and makes the central API reject them before the request body is read.
 */
private class ApiKeyInterceptor(
    private val tenantId: String?,
    private val apiKey: String
) : Interceptor {
    override fun intercept(chain: Interceptor.Chain): Response {
        val request = chain.request()
        if (!request.url.host.endsWith("lisans.appsgo.cloud")) {
            return chain.proceed(request)
        }

        return chain.proceed(
            request.newBuilder()
                .header("Authorization", "ApiKey $apiKey")
                .header("X-Api-Key", apiKey)
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
        var response = chain.proceed(request)
        var attempt = 1
        var delayMs = 1_000L

        while ((response.code == 429 || response.code in 500..599) && attempt < 3) {
            response.close()
            try {
                Thread.sleep(delayMs)
            } catch (_: InterruptedException) {
                Thread.currentThread().interrupt()
                break
            }
            attempt++
            delayMs *= 2
            response = chain.proceed(request)
        }
        return response
    }
}

object ApiClient {
    /**
     * The app talks only to the public central API.  Admin and legacy/ngrok
     * addresses must never be used by Retrofit.
     */
    fun centralBaseUrl(@Suppress("UNUSED_PARAMETER") requestedUrl: String? = null): String = CENTRAL_API_URL

    private fun retrofit(tenantId: String?, apiKey: String): Retrofit {
        require(apiKey.isNotBlank()) { "API anahtarı boş olamaz." }

        val moshi = Moshi.Builder()
            .add(KotlinJsonAdapterFactory())
            .build()
        val client = OkHttpClient.Builder()
            .addInterceptor(ApiKeyInterceptor(tenantId, apiKey.trim()))
            .addInterceptor(RetryInterceptor())
            .connectTimeout(30, TimeUnit.SECONDS)
            .readTimeout(30, TimeUnit.SECONDS)
            .build()

        return Retrofit.Builder()
            .baseUrl(CENTRAL_API_URL)
            .client(client)
            .addConverterFactory(MoshiConverterFactory.create(moshi))
            .build()
    }

    fun getFieldOpsApiService(
        @Suppress("UNUSED_PARAMETER") context: Context,
        @Suppress("UNUSED_PARAMETER") baseUrl: String,
        apiKey: String,
        tenantId: String? = null
    ): FieldOpsApiService = retrofit(tenantId, apiKey).create(FieldOpsApiService::class.java)

    fun getFieldOpsApiService(baseUrl: String, apiKey: String, tenantId: String? = null): FieldOpsApiService =
        retrofit(tenantId, apiKey).create(FieldOpsApiService::class.java)

    fun getErpBridgeApi(
        @Suppress("UNUSED_PARAMETER") context: Context,
        @Suppress("UNUSED_PARAMETER") baseUrl: String,
        apiKey: String,
        tenantId: String? = null
    ): ErpBridgeApi = retrofit(tenantId, apiKey).create(ErpBridgeApi::class.java)

    fun getErpBridgeApi(baseUrl: String, apiKey: String, tenantId: String? = null): ErpBridgeApi =
        retrofit(tenantId, apiKey).create(ErpBridgeApi::class.java)
}

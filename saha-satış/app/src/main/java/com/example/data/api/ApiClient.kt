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

object ApiClient {
    fun getApiService(context: Context, baseUrl: String, token: String): GoappApiService {
        var cleanUrl = baseUrl.trim()
        if (cleanUrl.contains("lisanssunucu") || cleanUrl.contains("lisans.appsgo.cloud")) {
            cleanUrl = "https://api.appsgo.cloud/api"
        }
        val moshi = Moshi.Builder()
            .add(KotlinJsonAdapterFactory())
            .build()
            
        val loggingInterceptor = HttpLoggingInterceptor().apply {
            level = HttpLoggingInterceptor.Level.BASIC
        }
        
        // Load tenant ID from erp_settings securely
        val sharedPrefs = context.getSharedPreferences("erp_settings", Context.MODE_PRIVATE)
        val encTenant = sharedPrefs.getString("tenant_id_encrypted", "") ?: ""
        val tenantId = if (encTenant.isNotEmpty()) com.example.util.CryptoUtils.decrypt(encTenant) else sharedPrefs.getString("tenant_id", "T001") ?: "T001"
        
        val client = OkHttpClient.Builder()
            .addInterceptor(loggingInterceptor)
            .addInterceptor { chain ->
                val original = chain.request()
                val originalUrl = original.url
                
                // Add tenant_id and tenant query parameters if not already present
                val urlWithTenant = if (tenantId.isNotBlank() && originalUrl.queryParameter("tenant_id") == null) {
                    originalUrl.newBuilder()
                        .addQueryParameter("tenant_id", tenantId)
                        .addQueryParameter("tenant", tenantId)
                        .build()
                } else {
                    originalUrl
                }
                
                val requestBuilder = original.newBuilder().url(urlWithTenant)
                if (token.isNotBlank()) {
                    val authHeaderValue = if (token.startsWith("Bearer ", ignoreCase = true)) {
                        token
                    } else {
                        "Bearer $token"
                    }
                    requestBuilder.header("Authorization", authHeaderValue)
                    requestBuilder.header("X-Authorization", token)
                    requestBuilder.header("X-API-KEY", token)
                    requestBuilder.header("X-Token", token)
                    requestBuilder.header("apikey", token)
                }
                
                if (tenantId.isNotBlank()) {
                    requestBuilder.header("X-Tenant-Id", tenantId)
                    requestBuilder.header("tenant_id", tenantId)
                    requestBuilder.header("tenant-id", tenantId)
                    requestBuilder.header("Tenant-Id", tenantId)
                    requestBuilder.header("X-Tenant", tenantId)
                }
                
                requestBuilder.header("Accept", "application/json")
                requestBuilder.header("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36")
                chain.proceed(requestBuilder.build())
            }
            .connectTimeout(30, TimeUnit.SECONDS)
            .readTimeout(30, TimeUnit.SECONDS)
            .build()
            
        val currentRetrofit = Retrofit.Builder()
            .baseUrl(if (cleanUrl.endsWith("/")) cleanUrl else "$cleanUrl/")
            .client(client)
            .addConverterFactory(MoshiConverterFactory.create(moshi))
            .build()
            
        return currentRetrofit.create(GoappApiService::class.java)
    }

    fun getApiService(baseUrl: String, token: String): GoappApiService {
        var cleanUrl = baseUrl.trim()
        if (cleanUrl.contains("lisanssunucu") || cleanUrl.contains("lisans.appsgo.cloud")) {
            cleanUrl = "https://api.appsgo.cloud/api"
        }
        val moshi = Moshi.Builder()
            .add(KotlinJsonAdapterFactory())
            .build()
            
        val loggingInterceptor = HttpLoggingInterceptor().apply {
            level = HttpLoggingInterceptor.Level.BASIC
        }
        
        val client = OkHttpClient.Builder()
            .addInterceptor(loggingInterceptor)
            .addInterceptor { chain ->
                val original = chain.request()
                val requestBuilder = original.newBuilder()
                if (token.isNotBlank()) {
                    val authHeaderValue = if (token.startsWith("Bearer ", ignoreCase = true)) {
                        token
                    } else {
                        "Bearer $token"
                    }
                    requestBuilder.header("Authorization", authHeaderValue)
                    requestBuilder.header("X-Authorization", token)
                    requestBuilder.header("X-API-KEY", token)
                    requestBuilder.header("X-Token", token)
                    requestBuilder.header("apikey", token)
                }
                requestBuilder.header("Accept", "application/json")
                requestBuilder.header("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36")
                chain.proceed(requestBuilder.build())
            }
            .connectTimeout(30, TimeUnit.SECONDS)
            .readTimeout(30, TimeUnit.SECONDS)
            .build()
            
        val currentRetrofit = Retrofit.Builder()
            .baseUrl(if (cleanUrl.endsWith("/")) cleanUrl else "$cleanUrl/")
            .client(client)
            .addConverterFactory(MoshiConverterFactory.create(moshi))
            .build()
            
        return currentRetrofit.create(GoappApiService::class.java)
    }

    fun getFieldOpsApiService(baseUrl: String, token: String): FieldOpsApiService {
        var cleanUrl = baseUrl.trim()
        if (cleanUrl.contains("lisanssunucu") || cleanUrl.contains("lisans.appsgo.cloud")) {
            cleanUrl = "https://api.appsgo.cloud"
        }
        if (cleanUrl.endsWith("/api/") || cleanUrl.endsWith("/api")) {
            cleanUrl = cleanUrl.removeSuffix("/").removeSuffix("api").removeSuffix("/")
        }
        val moshi = Moshi.Builder()
            .add(KotlinJsonAdapterFactory())
            .build()
            
        val loggingInterceptor = HttpLoggingInterceptor().apply {
            level = HttpLoggingInterceptor.Level.BASIC
        }
        
        val client = OkHttpClient.Builder()
            .addInterceptor(loggingInterceptor)
            .connectTimeout(30, TimeUnit.SECONDS)
            .readTimeout(30, TimeUnit.SECONDS)
            .build()
            
        val currentRetrofit = Retrofit.Builder()
            .baseUrl(if (cleanUrl.endsWith("/")) cleanUrl else "$cleanUrl/")
            .client(client)
            .addConverterFactory(MoshiConverterFactory.create(moshi))
            .build()
            
        return currentRetrofit.create(FieldOpsApiService::class.java)
    }

    fun getFieldOpsApiService(context: Context, baseUrl: String, token: String): FieldOpsApiService {
        var cleanUrl = baseUrl.trim()
        if (cleanUrl.contains("lisanssunucu") || cleanUrl.contains("lisans.appsgo.cloud")) {
            cleanUrl = "https://api.appsgo.cloud"
        }
        if (cleanUrl.endsWith("/api/") || cleanUrl.endsWith("/api")) {
            cleanUrl = cleanUrl.removeSuffix("/").removeSuffix("api").removeSuffix("/")
        }
        val moshi = Moshi.Builder()
            .add(KotlinJsonAdapterFactory())
            .build()
            
        val loggingInterceptor = HttpLoggingInterceptor().apply {
            level = HttpLoggingInterceptor.Level.BASIC
        }
        
        val client = OkHttpClient.Builder()
            .addInterceptor(loggingInterceptor)
            .connectTimeout(30, TimeUnit.SECONDS)
            .readTimeout(30, TimeUnit.SECONDS)
            .build()
            
        val currentRetrofit = Retrofit.Builder()
            .baseUrl(if (cleanUrl.endsWith("/")) cleanUrl else "$cleanUrl/")
            .client(client)
            .addConverterFactory(MoshiConverterFactory.create(moshi))
            .build()
            
        return currentRetrofit.create(FieldOpsApiService::class.java)
    }
}

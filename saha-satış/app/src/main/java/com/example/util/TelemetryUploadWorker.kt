package com.example.util

import android.content.Context
import android.util.Log
import androidx.work.CoroutineWorker
import androidx.work.WorkerParameters
import androidx.work.Constraints
import androidx.work.NetworkType
import androidx.work.OneTimeWorkRequestBuilder
import androidx.work.PeriodicWorkRequestBuilder
import androidx.work.WorkManager
import androidx.work.ExistingWorkPolicy
import androidx.work.ExistingPeriodicWorkPolicy
import androidx.work.BackoffPolicy
import com.example.data.database.DatabaseProvider
import okhttp3.OkHttpClient
import com.example.data.api.LicenseHeaderInterceptor
import com.example.data.api.RetryAndLicenseInterceptor
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.util.concurrent.TimeUnit
import org.json.JSONObject
import org.json.JSONArray
import okhttp3.MediaType.Companion.toMediaTypeOrNull
import okhttp3.RequestBody.Companion.toRequestBody

class TelemetryUploadWorker(
    appContext: Context,
    workerParams: WorkerParameters
) : CoroutineWorker(appContext, workerParams) {

    override suspend fun doWork(): Result {
        return withContext(Dispatchers.IO) {
            val secPrefs = applicationContext.getSharedPreferences("secure_license_prefs", Context.MODE_PRIVATE)
            val erpPrefs = applicationContext.getSharedPreferences("erp_settings", Context.MODE_PRIVATE)
            val accessToken = secPrefs.getString("api_key", null)
            val baseUrl = erpPrefs.getString("api_url", "https://lisans.appsgo.cloud/")
            val tenantId = erpPrefs.getString("tenant_id", "")
            
            // Only upload if we have an access token
            if (accessToken.isNullOrBlank()) {
                return@withContext Result.success()
            }

            val db = DatabaseProvider.getDatabase(applicationContext)
            val dao = db.telemetryDao()

            var hasMore = true
            while (hasMore) {
                val batch = dao.getOldestEvents(20)
                if (batch.isEmpty()) {
                    hasMore = false
                    break
                }

                try {
                    val rootObj = JSONObject()
                    val eventsArray = JSONArray()
                    
                    for (event in batch) {
                        val eventObj = JSONObject()
                        eventObj.put("eventId", event.eventId)
                        eventObj.put("occurredAtUtc", event.occurredAtUtc)
                        eventObj.put("kind", event.kind)
                        eventObj.put("severity", event.severity)
                        eventObj.put("appVersion", event.appVersion)
                        eventObj.put("androidVersion", event.androidVersion)
                        eventObj.put("deviceModel", event.deviceModel)
                        eventObj.put("screen", event.screen)
                        eventObj.put("operation", event.operation)
                        eventObj.put("exceptionType", event.exceptionType)
                        eventObj.put("message", event.message)
                        eventObj.put("stackTrace", event.stackTrace)
                        if (event.httpMethod.isNotBlank()) eventObj.put("httpMethod", event.httpMethod)
                        if (event.httpRoute.isNotBlank()) eventObj.put("httpRoute", event.httpRoute)
                        event.httpStatus?.let { eventObj.put("httpStatus", it) }
                        if (event.correlationId.isNotBlank()) eventObj.put("correlationId", event.correlationId)
                        
                        try {
                            if (event.breadcrumbsJson.isNotBlank()) {
                                eventObj.put("breadcrumbs", JSONArray(event.breadcrumbsJson))
                            }
                        } catch(e: Exception) {
                            eventObj.put("breadcrumbs", JSONArray())
                        }
                        
                        eventsArray.put(eventObj)
                    }
                    
                    rootObj.put("events", eventsArray)
                    val jsonString = rootObj.toString()
                    val requestBody = jsonString.toRequestBody("application/json".toMediaTypeOrNull())
                    
                    // Direct OkHttp call or use Retrofit if available. Using RetrofitClient.
                    // Need a dedicated api endpoint. I will add it to ApiService or use okhttp directly to avoid altering ApiService too much.
                    val client = OkHttpClient.Builder()
                        .addInterceptor(LicenseHeaderInterceptor(applicationContext, accessToken))
                        .addInterceptor(RetryAndLicenseInterceptor(applicationContext))
                        .build()

                    val targetUrl = (baseUrl ?: "https://lisans.appsgo.cloud/").let {
                        val normalized = it.trimEnd('/')
                        val origin = normalized.removeSuffix("/api")
                        "$origin/api/v1/mobile/telemetry/batch"
                    }
                    Log.d("TelemetryWorker", "Sending telemetry batch of size ${batch.size} to $targetUrl...")

                    val request = okhttp3.Request.Builder()
                        .url(targetUrl)
                        .post(requestBody)
                        // Headers are handled by LicenseHeaderInterceptor
                        .build()

                    val response = client.newCall(request).execute()
                    
                    if (response.isSuccessful) {
                        Log.d("TelemetryWorker", "Telemetry batch sent successfully.")
                        val ids = batch.map { it.eventId }
                        dao.deleteEventsByIds(ids)
                    } else {
                        Log.e("TelemetryWorker", "Failed to send telemetry. HTTP ${response.code}: ${response.message}")
                        if (response.code == 401 || response.code == 403) {
                            // RetryAndLicenseInterceptor should have handled token refresh.
                            // If we still get 401/403 here, it means refresh failed and is_license_valid is set to false.
                            // We shouldn't retry indefinitely if the license is completely invalid.
                            val isLicenseValid = secPrefs.getBoolean("is_license_valid", true)
                            if (!isLicenseValid) {
                                Log.e("TelemetryWorker", "License is invalid. Stopping telemetry upload.")
                                return@withContext Result.failure()
                            }
                            return@withContext Result.retry()
                        }
                        return@withContext Result.retry()
                    }
                } catch (e: Exception) {
                    return@withContext Result.retry()
                }
            }
            
            Result.success()
        }
    }
}

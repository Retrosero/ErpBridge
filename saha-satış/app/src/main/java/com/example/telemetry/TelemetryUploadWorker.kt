package com.example.telemetry

import android.content.Context
import androidx.work.BackoffPolicy
import androidx.work.Constraints
import androidx.work.CoroutineWorker
import androidx.work.ExistingPeriodicWorkPolicy
import androidx.work.ExistingWorkPolicy
import androidx.work.NetworkType
import androidx.work.OneTimeWorkRequestBuilder
import androidx.work.PeriodicWorkRequestBuilder
import androidx.work.WorkManager
import androidx.work.WorkerParameters
import com.example.data.LicenseRepository
import com.example.data.api.ApiClient
import java.util.concurrent.TimeUnit

class TelemetryUploadWorker(context: Context, params: WorkerParameters) : CoroutineWorker(context, params) {
    override suspend fun doWork(): Result {
        val token = LicenseRepository.getDeviceToken(applicationContext)
        if (token.isNullOrBlank()) return Result.success()
        val dao = com.example.data.database.DatabaseProvider.getDatabase(applicationContext).telemetryDao()
        val candidates = dao.oldest(20)
        if (candidates.isEmpty()) return Result.success()
        var estimatedBytes = 0
        val batch = candidates.takeWhile {
            val itemBytes = (it.message.orEmpty().length + it.stackTrace.orEmpty().length +
                it.breadcrumbsJson.length + 1_500) * 2
            val fits = estimatedBytes + itemBytes <= 240_000 || estimatedBytes == 0
            if (fits) estimatedBytes += itemBytes
            fits
        }

        return runCatching {
            val response = ApiClient.getFieldOpsApiService(applicationContext, "https://lisans.appsgo.cloud/", token)
                .uploadTelemetry(TelemetryBatchRequest(batch.map(TelemetryReporter::toDto)))
            when {
                response.isSuccessful -> {
                    dao.deleteByIds(batch.map { it.eventId })
                    if (dao.oldest(1).isNotEmpty()) enqueue(applicationContext)
                    Result.success()
                }
                response.code() == 401 -> {
                    LicenseRepository.renewSession(applicationContext)
                    Result.retry()
                }
                response.code() == 403 || response.code() == 429 || response.code() >= 500 -> Result.retry()
                else -> Result.failure()
            }
        }.getOrElse {
            Result.retry()
        }
    }

    companion object {
        private const val ONCE = "mobile-telemetry-upload"
        private const val PERIODIC = "mobile-telemetry-periodic"
        private val constraints = Constraints.Builder().setRequiredNetworkType(NetworkType.CONNECTED).build()

        fun enqueue(context: Context) {
            val request = OneTimeWorkRequestBuilder<TelemetryUploadWorker>()
                .setConstraints(constraints)
                .setBackoffCriteria(BackoffPolicy.EXPONENTIAL, 30, TimeUnit.SECONDS)
                .build()
            WorkManager.getInstance(context).enqueueUniqueWork(ONCE, ExistingWorkPolicy.KEEP, request)
        }

        fun schedule(context: Context) {
            val request = PeriodicWorkRequestBuilder<TelemetryUploadWorker>(15, TimeUnit.MINUTES)
                .setConstraints(constraints)
                .setBackoffCriteria(BackoffPolicy.EXPONENTIAL, 30, TimeUnit.SECONDS)
                .build()
            WorkManager.getInstance(context).enqueueUniquePeriodicWork(PERIODIC, ExistingPeriodicWorkPolicy.KEEP, request)
            enqueue(context)
        }
    }
}

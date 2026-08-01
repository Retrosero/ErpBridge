package com.example.worker

import android.content.Context
import androidx.work.CoroutineWorker
import androidx.work.WorkerParameters
import com.example.data.SyncRepository

class SyncWorker(
    appContext: Context,
    workerParams: WorkerParameters
) : CoroutineWorker(appContext, workerParams) {

    override suspend fun doWork(): Result {
        return try {
            val pSuccess = SyncRepository.syncProducts(applicationContext)
            val cSuccess = SyncRepository.syncCustomers(applicationContext)

            // /pull may return an unbounded mixed snapshot.  The catalog methods above
            // use the bounded, page-aware endpoints and are the worker's only contract.
            if (pSuccess && cSuccess) {
                Result.success()
            } else {
                Result.retry()
            }
        } catch (e: Exception) {
            Result.retry()
        }
    }
}

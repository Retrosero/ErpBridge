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
            val pullSuccess = SyncRepository.syncAllFromPull(applicationContext)
            val pSuccess = SyncRepository.syncProducts(applicationContext)
            val cSuccess = SyncRepository.syncCustomers(applicationContext)
            val addrSuccess = SyncRepository.syncCustomerAddresses(applicationContext)
            val contactsSuccess = SyncRepository.syncCustomerContacts(applicationContext)
            val barcodesSuccess = SyncRepository.syncBarcodes(applicationContext)
            val conditionsSuccess = SyncRepository.syncSalesConditions(applicationContext)
            val cariHareketSuccess = SyncRepository.syncCariHareketleri(applicationContext)
            val stokHareketSuccess = SyncRepository.syncStokHareketleri(applicationContext)
            
            if ((pullSuccess || (pSuccess && cSuccess)) && addrSuccess && contactsSuccess && barcodesSuccess && conditionsSuccess) {
                Result.success()
            } else {
                Result.retry()
            }
        } catch (e: Exception) {
            Result.retry()
        }
    }
}

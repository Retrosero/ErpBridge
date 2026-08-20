package com.example.data

import android.content.Context
import android.util.Log
import com.example.data.api.ApiClient
import com.example.data.api.PullJobsRequest
import com.example.data.database.CustomerEntity
import com.example.data.database.DatabaseProvider
import com.example.data.database.ProductEntity
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

object SyncRepository {

    suspend fun syncProducts(context: Context): Boolean = withContext(Dispatchers.IO) {
        val apiKey = LicenseRepository.getApiKey(context)
        val tenantId = LicenseRepository.getTenantId(context)
        if (apiKey == null || tenantId == null) return@withContext false

        val baseUrl = LicenseRepository.getBaseUrl(context)
        val deviceId = LicenseRepository.getDeviceId(context)
        val apiService = ApiClient.getFieldOpsApiService(context, baseUrl, apiKey, tenantId)
        val request = PullJobsRequest(
            tenant_id = tenantId,
            api_key = apiKey,
            device_id = deviceId,
            agent_version = "1.0.0"
        )
        
        try {
            val response = apiService.getUrunler(request)
            if (response.isSuccessful) {
                val db = DatabaseProvider.getDatabase(context)
                val items = response.body()?.actualItems ?: emptyList()
                val entities = items.map {
                    ProductEntity(
                        // Room uses barcode as the primary key.  The ERP code is a stable
                        // fallback; an empty barcode would overwrite every such product.
                        barcode = it.barkod?.takeIf(String::isNotBlank)
                            ?: it.actualUrunKod.takeIf(String::isNotBlank)
                            ?: it.id ?: return@map null,
                        code = it.actualUrunKod,
                        title = it.actualUrunAd ?: "İsimsiz Ürün",
                        category = "",
                        desc = "",
                        basePrice = it.satisFiyat ?: 0.0,
                        dealerPrice = it.satisFiyat ?: 0.0,
                        wholesalePrice = it.satisFiyat ?: 0.0,
                        kdvPercent = it.kdvOrani?.toInt() ?: 18,
                        colorValue = 0xFFCCCCCC,
                        stockByWarehouseJson = "{}",
                        brand = it.actualMarka,
                        aisle = it.actualReyonKod,
                        measurement = it.actualOlcu,
                        packaging = it.actualAmbalaj,
                        cartonQuantity = it.actualKoliAdet,
                        imageUrl = ""
                    )
                }.filterNotNull()
                db.productDao().insertAll(entities)
                true
            } else {
                Log.e("SyncRepository", "Sync error HTTP ${response.code()}")
                false
            }
        } catch (e: Exception) {
            Log.e("SyncRepository", "Exception", e)
            false
        }
    }

    suspend fun syncCustomers(context: Context): Boolean = withContext(Dispatchers.IO) {
        val apiKey = LicenseRepository.getApiKey(context)
        val tenantId = LicenseRepository.getTenantId(context)
        if (apiKey == null || tenantId == null) return@withContext false

        val baseUrl = LicenseRepository.getBaseUrl(context)
        val deviceId = LicenseRepository.getDeviceId(context)
        val apiService = ApiClient.getFieldOpsApiService(context, baseUrl, apiKey, tenantId)
        val request = PullJobsRequest(
            tenant_id = tenantId,
            api_key = apiKey,
            device_id = deviceId,
            agent_version = "1.0.0"
        )
        
        try {
            val response = apiService.getCariler(request)
            if (response.isSuccessful) {
                val db = DatabaseProvider.getDatabase(context)
                val items = response.body()?.actualItems ?: emptyList()
                val entities = items.map {
                    CustomerEntity(
                        id = it.actualCariKod,
                        name = it.actualCariUnvan,
                        balance = it.balance ?: it.bakiye ?: 0.0,
                        lastVisit = it.updatedAt ?: "",
                        contact = "",
                        phone = it.telefon ?: "",
                        address = it.adres ?: "",
                        taxOffice = it.vergiDairesi ?: "",
                        taxNumber = it.vergiNo ?: "",
                        gpsLocation = "",
                        riskLimit = 0.0,
                        priceGroup = "",
                        specialDiscountPercent = 0.0,
                        transactionsJson = "[]"
                    )
                }
                db.customerDao().insertAll(entities)
                true
            } else {
                false
            }
        } catch (e: Exception) {
            false
        }
    }

    fun schedulePeriodicSync(context: Context) {
        val constraints = androidx.work.Constraints.Builder()
            .setRequiredNetworkType(androidx.work.NetworkType.CONNECTED)
            .build()
        val immediateRequest = androidx.work.OneTimeWorkRequestBuilder<com.example.worker.SyncWorker>()
            .setConstraints(constraints)
            .setBackoffCriteria(
                androidx.work.BackoffPolicy.EXPONENTIAL,
                10,
                java.util.concurrent.TimeUnit.SECONDS
            )
            .build()
        androidx.work.WorkManager.getInstance(context).enqueueUniqueWork(
            "InitialCentralSync",
            androidx.work.ExistingWorkPolicy.KEEP,
            immediateRequest
        )

        val workRequest = androidx.work.PeriodicWorkRequestBuilder<com.example.worker.SyncWorker>(1, java.util.concurrent.TimeUnit.HOURS)
            .setConstraints(constraints)
            .build()
        androidx.work.WorkManager.getInstance(context).enqueueUniquePeriodicWork(
            "PeriodicSyncWorker",
            androidx.work.ExistingPeriodicWorkPolicy.KEEP,
            workRequest
        )
    }
}

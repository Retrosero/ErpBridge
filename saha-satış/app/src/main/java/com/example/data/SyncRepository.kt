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
        val apiService = ApiClient.getFieldOpsApiService(context, baseUrl, apiKey)
        val request = PullJobsRequest(
            tenant_id = tenantId,
            api_key = "", // Avoid sending API key in request body
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
                        brand = it.actualMarka,
                        aisle = it.actualReyonKod,
                        measurement = it.actualOlcu,
                        packaging = it.actualAmbalaj,
                        cartonQuantity = it.actualKoliAdet,
                        stockByWarehouseJson = "{}",
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
        val apiService = ApiClient.getFieldOpsApiService(context, baseUrl, apiKey)
        val request = PullJobsRequest(
            tenant_id = tenantId,
            api_key = "", // Avoid sending API key in request body
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

    suspend fun syncAllFromPull(context: Context): Boolean = withContext(Dispatchers.IO) {
        val apiKey = LicenseRepository.getApiKey(context)
        val tenantId = LicenseRepository.getTenantId(context)
        if (apiKey == null || tenantId == null) return@withContext false

        val baseUrl = LicenseRepository.getBaseUrl(context)
        val deviceId = LicenseRepository.getDeviceId(context)
        val apiService = ApiClient.getFieldOpsApiService(context, baseUrl, apiKey)
        val request = PullJobsRequest(
            tenant_id = tenantId,
            api_key = "", // Avoid sending API key in request body
            device_id = deviceId,
            agent_version = "1.0.0"
        )

        try {
            val response = apiService.pullJobs(request)
            if (response.isSuccessful && response.body()?.success == true) {
                val body = response.body()!!
                val db = DatabaseProvider.getDatabase(context)
                val rawData = body.items ?: body.message // Check items or fallback
                
                // Process dynamic mapping safely
                val itemsList = mutableListOf<Map<String, Any?>>()
                if (rawData is List<*>) {
                    itemsList.addAll(rawData.filterIsInstance<Map<String, Any?>>())
                }

                importListToDatabase(context, db, itemsList)
                true
            } else {
                Log.e("SyncRepository", "Pull Sync error HTTP ${response.code()}")
                false
            }
        } catch (e: Exception) {
            Log.e("SyncRepository", "Pull Sync exception", e)
            false
        }
    }

    private suspend fun importListToDatabase(
        context: Context,
        db: com.example.data.database.AppDatabase,
        list: List<Map<String, Any?>>,
        entityName: String? = null
    ) {
        val customersToInsert = mutableListOf<CustomerEntity>()
        val productsToInsert = mutableListOf<ProductEntity>()
        val seenBarcodes = mutableSetOf<String>()

        for (item in list) {
            val resolvedEntity = entityName ?: detectEntity(item)
            when (resolvedEntity) {
                "cari", "customer", "customers" -> {
                    val id = item.getString("id", "cariKod", "erpKod", "erpRef")
                    if (id.isNotBlank()) {
                        val name = item.getString("name", "unvan", "cariUnvan")
                        customersToInsert.add(
                            CustomerEntity(
                                id = id,
                                name = name.ifBlank { "İsimsiz Cari" },
                                balance = item.getDouble("balance", "bakiye", "netBakiye"),
                                lastVisit = item.getString("lastVisit", "updatedAt", "createdAt"),
                                contact = item.getString("contact"),
                                phone = item.getString("phone", "telefon"),
                                address = item.getString("address", "adres"),
                                taxOffice = item.getString("taxOffice", "vergiDairesi"),
                                taxNumber = item.getString("taxNumber", "vergiNo", "tcKimlikNo"),
                                gpsLocation = item.getString("gpsLocation", "gpsEnlem"),
                                riskLimit = item.getDouble("riskLimit"),
                                priceGroup = item.getString("priceGroup"),
                                specialDiscountPercent = item.getDouble("specialDiscountPercent"),
                                transactionsJson = "[]"
                            )
                        )
                    }
                }
                "urun", "product", "products" -> {
                    val barcode = item.getString("barcode", "barkod")
                    val code = item.getString("code", "urun_kodu", "urunKod", "erpKod", "stokKod", "stokKodu", "stok_kodu", "sto_kod", "sto_kodu")
                    val idVal = item.getString("id")
                    var finalBarcode = barcode.ifBlank { code }.ifBlank { idVal }
                    if (finalBarcode.isBlank() || finalBarcode.lowercase() == "null" || finalBarcode.lowercase() == "none" || seenBarcodes.contains(finalBarcode)) {
                        finalBarcode = code.ifBlank { idVal }.ifBlank { java.util.UUID.randomUUID().toString() }
                    }
                    seenBarcodes.add(finalBarcode)
                    if (finalBarcode.isNotBlank()) {
                        val title = item.getString("title", "urun_adi", "urunAd", "ad", "stokAd", "stokAdi", "stok_adi", "stok_ad", "sto_isim", "sto_adi", "isim", "name")
                        val category = item.getString("category", "kategori")
                        val desc = item.getString("desc", "aciklama")
                        val basePrice = item.getDouble("basePrice", "satis_fiyati", "satisFiyat", "listeFiyati", "price", "fiyat")
                        val dealerPrice = item.getDouble("dealerPrice", "bayiFiyati", "satisFiyat")
                        val wholesalePrice = item.getDouble("wholesalePrice", "toptanFiyati", "satisFiyat")
                        val kdvPercent = item.getInt("kdvPercent", "kdv", "kdvOrani")
                        val brand = item.getString("brand", "marka", "actualMarka")
                        val aisle = item.getString("aisle", "reyon", "reyonKod", "actualReyonKod", "sto_yer_kod")
                        val measurement = item.getString("measurement", "olcu", "actualOlcu", "sto_sektor_kodu")
                        val packaging = item.getString("packaging", "ambalaj", "actualAmbalaj", "sto_ambalaj_kodu")
                        val cartonQuantity = item.getString("cartonQuantity", "koliAdet", "actualKoliAdet")
                        val stockByWarehouseJson = item.getString("stockByWarehouseJson", "stockByWarehouse", "miktarDepo")

                        productsToInsert.add(
                            ProductEntity(
                                barcode = finalBarcode,
                                code = code,
                                title = title.ifBlank { "İsimsiz Ürün" },
                                category = category,
                                desc = desc,
                                basePrice = basePrice,
                                dealerPrice = dealerPrice,
                                wholesalePrice = wholesalePrice,
                                kdvPercent = if (kdvPercent == 0) 20 else kdvPercent,
                                colorValue = 0xFFCCCCCC,
                                brand = brand,
                                aisle = aisle,
                                measurement = measurement,
                                packaging = packaging,
                                cartonQuantity = cartonQuantity,
                                stockByWarehouseJson = if (stockByWarehouseJson.isBlank()) "{}" else stockByWarehouseJson
                            )
                        )
                    }
                }
            }
        }

        if (customersToInsert.isNotEmpty()) {
            db.customerDao().insertAll(customersToInsert)
            Log.d("SyncRepository", "Imported ${customersToInsert.size} customers from /pull")
        }
        if (productsToInsert.isNotEmpty()) {
            db.productDao().insertAll(productsToInsert)
            Log.d("SyncRepository", "Imported ${productsToInsert.size} products from /pull")
        }
    }

    private fun detectEntity(item: Map<String, Any?>): String {
        if (item.containsKey("unvan") || item.containsKey("cariUnvan") || item.containsKey("cariKod") || item.containsKey("tcKimlikNo")) {
            return "cari"
        }
        if (item.containsKey("urun_adi") || item.containsKey("urunAd") || item.containsKey("barkod") || item.containsKey("satis_fiyati") || item.containsKey("stokKod") || item.containsKey("stokKodu") || item.containsKey("stok_kodu") || item.containsKey("stokAd") || item.containsKey("stokAdi") || item.containsKey("stok_adi")) {
            return "urun"
        }
        return "unknown"
    }

    private fun Map<String, Any?>.getString(vararg keys: String): String {
        for (key in keys) {
            val value = this[key]
            if (value != null) return value.toString()
        }
        return ""
    }

    private fun Map<String, Any?>.getDouble(vararg keys: String): Double {
        for (key in keys) {
            val value = this[key]
            if (value is Number) return value.toDouble()
            if (value is String) return value.toDoubleOrNull() ?: 0.0
        }
        return 0.0
    }

    private fun Map<String, Any?>.getInt(vararg keys: String): Int {
        for (key in keys) {
            val value = this[key]
            if (value is Number) return value.toInt()
            if (value is String) return value.toDoubleOrNull()?.toInt() ?: 0
        }
        return 0
    }

    suspend fun syncCustomerAddresses(context: Context): Boolean = withContext(Dispatchers.IO) {
        val apiKey = LicenseRepository.getApiKey(context)
        val tenantId = LicenseRepository.getTenantId(context)
        if (apiKey == null || tenantId == null) return@withContext false

        val baseUrl = LicenseRepository.getBaseUrl(context)
        val deviceId = LicenseRepository.getDeviceId(context)
        val apiService = ApiClient.getFieldOpsApiService(context, baseUrl, apiKey)
        val request = PullJobsRequest(
            tenant_id = tenantId,
            api_key = "",
            device_id = deviceId,
            agent_version = "1.0.0"
        )
        // Disabled sync
        Log.e("SyncRepository", "syncCustomerAddresses disabled")
        return@withContext false
    }

    suspend fun syncCustomerContacts(context: Context): Boolean = withContext(Dispatchers.IO) {
        val apiKey = LicenseRepository.getApiKey(context)
        val tenantId = LicenseRepository.getTenantId(context)
        if (apiKey == null || tenantId == null) return@withContext false

        val baseUrl = LicenseRepository.getBaseUrl(context)
        val deviceId = LicenseRepository.getDeviceId(context)
        val apiService = ApiClient.getFieldOpsApiService(context, baseUrl, apiKey)
        val request = PullJobsRequest(
            tenant_id = tenantId,
            api_key = "",
            device_id = deviceId,
            agent_version = "1.0.0"
        )
        // Disabled sync
        Log.e("SyncRepository", "syncCustomerContacts disabled")
        return@withContext false
    }

    suspend fun syncBarcodes(context: Context): Boolean = withContext(Dispatchers.IO) {
        val apiKey = LicenseRepository.getApiKey(context)
        val tenantId = LicenseRepository.getTenantId(context)
        if (apiKey == null || tenantId == null) return@withContext false

        val baseUrl = LicenseRepository.getBaseUrl(context)
        val deviceId = LicenseRepository.getDeviceId(context)
        val apiService = ApiClient.getFieldOpsApiService(context, baseUrl, apiKey)
        val request = PullJobsRequest(
            tenant_id = tenantId,
            api_key = "",
            device_id = deviceId,
            agent_version = "1.0.0"
        )
        // Disabled sync
        Log.e("SyncRepository", "syncBarcodes disabled")
        return@withContext false
    }

    suspend fun syncSalesConditions(context: Context): Boolean = withContext(Dispatchers.IO) {
        val apiKey = LicenseRepository.getApiKey(context)
        val tenantId = LicenseRepository.getTenantId(context)
        if (apiKey == null || tenantId == null) return@withContext false

        val baseUrl = LicenseRepository.getBaseUrl(context)
        val deviceId = LicenseRepository.getDeviceId(context)
        val apiService = ApiClient.getFieldOpsApiService(context, baseUrl, apiKey)
        val request = PullJobsRequest(
            tenant_id = tenantId,
            api_key = "",
            device_id = deviceId,
            agent_version = "1.0.0"
        )
        try {
            val response = apiService.getSalesConditions(request)
            if (response.isSuccessful && response.body() != null) {
                val db = com.example.data.database.DatabaseProvider.getDatabase(context)
                val items = response.body()?.actualItems ?: emptyList()
                val entities = items.map {
                    com.example.data.database.SalesConditionEntity(
                        stockCode = it.stockCode,
                        customerCode = it.customerCode,
                        warehouseNo = it.warehouseNo,
                        paymentPlanNo = it.paymentPlanNo,
                        startDate = it.startDate,
                        endDate = it.endDate,
                        grossPrice = it.grossPrice,
                        currency = it.currency,
                        discounts = it.discounts
                    )
                }
                db.salesConditionDao().replaceAll(entities)
                true
            } else {
                Log.e("SyncRepository", "syncSalesConditions error HTTP ${response.code()}")
                false
            }
        } catch (e: Exception) {
            Log.e("SyncRepository", "syncSalesConditions exception", e)
            false
        }
    }

    suspend fun syncCariHareketleri(context: Context): Boolean = withContext(Dispatchers.IO) {
        val apiKey = LicenseRepository.getApiKey(context)
        val tenantId = LicenseRepository.getTenantId(context)
        if (apiKey == null || tenantId == null) return@withContext false

        val baseUrl = LicenseRepository.getBaseUrl(context)
        val deviceId = LicenseRepository.getDeviceId(context)
        val apiService = ApiClient.getFieldOpsApiService(context, baseUrl, apiKey)
        val request = PullJobsRequest(
            tenant_id = tenantId,
            api_key = apiKey,
            device_id = deviceId,
            agent_version = "v2.0-multi-tenant",
            entity = "cariHareketleri",
            page = 1,
            pageSize = 500
        )
        try {
            val response = apiService.getCariHareketleri(request)
            if (response.isSuccessful && response.body() != null) {
                val db = com.example.data.database.DatabaseProvider.getDatabase(context)
                val items = response.body()?.actualItems ?: emptyList()
                val entities = items.map { dto ->
                    val rawDate = dto.tarih ?: ""
                    val formattedDate = try {
                        if (rawDate.contains("T")) {
                            val parts = rawDate.split("T")[0].split("-")
                            if (parts.size == 3) "${parts[2]}.${parts[1]}.${parts[0]}" else rawDate
                        } else rawDate
                    } catch (e: Exception) { rawDate }

                    val txType = when (dto.evrakTip) {
                        29 -> "SATIŞ"
                        64 -> "TAHSİLAT"
                        65 -> "TEDİYE"
                        63 -> "SATIŞ"
                        else -> when (dto.tip) {
                            0 -> "SATIŞ"
                            1 -> "TAHSİLAT"
                            2 -> "İADE"
                            3 -> "VİRMAN"
                            else -> if (dto.borcMu == true) "SATIŞ" else "TAHSİLAT"
                        }
                    }
                    val rawEvrak = dto.evrakNo ?: ""
                    val docNo = if (rawEvrak.isNotEmpty() && !rawEvrak.startsWith("FT-") && !rawEvrak.startsWith("SM-")) "FT-$rawEvrak" else rawEvrak
                    val finalDesc = if (docNo.isNotEmpty()) "$docNo - ${dto.aciklama ?: "Mikro Cari Hareketi"}" else (dto.aciklama ?: "Mikro Cari Hareketi")
                    val txId = if (docNo.isNotEmpty()) docNo else (dto.id?.toString() ?: "TX-ERP-${(Math.random() * 100000).toInt()}")
                    com.example.data.database.CariHareketEntity(
                        id = txId,
                        customerCode = (dto.cariKod ?: dto.erpRef ?: "").trim(),
                        date = formattedDate,
                        type = txType,
                        amount = dto.tutar ?: 0.0,
                        description = finalDesc,
                        erpRef = dto.erpRef,
                        recNo = dto.id?.toString(),
                        cha_recno = dto.realChaRecNo
                    )
                }
                db.cariHareketDao().replaceAll(entities)
                withContext(Dispatchers.Main) {
                    val txGrouped = entities.groupBy { it.customerCode.lowercase() }
                    for (i in com.example.ui.screens.AppDataStore.customers.indices) {
                        val cust = com.example.ui.screens.AppDataStore.customers[i]
                        val custKey = cust.id.lowercase()
                        val matches = txGrouped[custKey]
                            ?: txGrouped.entries.firstOrNull { it.key.isNotBlank() && (custKey == it.key || custKey.contains(it.key) || it.key.contains(custKey)) }?.value
                        if (matches != null && matches.isNotEmpty()) {
                            val newTxs = matches.map { dto ->
                                com.example.ui.screens.CustomerTx(
                                    id = dto.id,
                                    date = dto.date,
                                    type = dto.type,
                                    amount = dto.amount,
                                    description = dto.description,
                                    erpRef = dto.erpRef,
                                    recNo = dto.recNo,
                                    cha_recno = dto.cha_recno
                                )
                            }
                            var calculated = 0.0
                            for (tx in newTxs) {
                                val t = tx.type.uppercase()
                                if (t.contains("SATIŞ") || t.contains("SATIS") || t.contains("BORÇ") || t.contains("BORC") || t.contains("FATURA") || t.contains("TEDİYE") || t.contains("TEDIYE") || t == "0") {
                                    calculated += tx.amount
                                } else if (t.contains("TAHSİLAT") || t.contains("TAHSILAT") || t.contains("İADE") || t.contains("IADE") || t.contains("ALACAK") || t == "1") {
                                    calculated -= tx.amount
                                } else {
                                    calculated += tx.amount
                                }
                            }
                            val finalBal = if (cust.balance != 0.0) cust.balance else calculated
                            com.example.ui.screens.AppDataStore.customers[i] = cust.copy(balance = finalBal, transactions = newTxs.toMutableList())
                        }
                    }
                }
                true
            } else {
                Log.e("SyncRepository", "syncCariHareketleri error HTTP ${response.code()}")
                false
            }
        } catch (e: Exception) {
            Log.e("SyncRepository", "syncCariHareketleri exception", e)
            false
        }
    }

    suspend fun syncStokHareketleri(context: Context): Boolean = withContext(Dispatchers.IO) {
        val apiKey = LicenseRepository.getApiKey(context)
        val tenantId = LicenseRepository.getTenantId(context)
        if (apiKey == null || tenantId == null) return@withContext false

        val baseUrl = LicenseRepository.getBaseUrl(context)
        val deviceId = LicenseRepository.getDeviceId(context)
        val apiService = ApiClient.getFieldOpsApiService(context, baseUrl, apiKey)
        val request = PullJobsRequest(
            tenant_id = tenantId,
            api_key = apiKey,
            device_id = deviceId,
            agent_version = "v2.0-multi-tenant",
            entity = "stokHareket",
            page = 1,
            pageSize = 500
        )
        try {
            val response = apiService.getStokHareket(request)
            if (response.isSuccessful && response.body() != null) {
                val db = com.example.data.database.DatabaseProvider.getDatabase(context)
                val items = response.body()?.actualItems ?: emptyList()
                val entities = items.map { item ->
                    val rawDate = item.tarih ?: ""
                    val formattedDate = try {
                        if (rawDate.contains("T")) {
                            val timePart = rawDate.split("T").getOrNull(1)?.take(5) ?: "00:00"
                            val datePart = rawDate.split("T")[0].split("-")
                            if (datePart.size == 3) "${datePart[2]}.${datePart[1]}.${datePart[0]} $timePart" else rawDate
                        } else rawDate
                    } catch (e: Exception) { rawDate }

                    val moveType = when (item.tip) {
                        0 -> "Giriş"
                        1 -> "Çıkış"
                        2 -> "İade Giriş"
                        3 -> "İade Çıkış"
                        else -> "Hareket"
                    }
                    val quantityFormatted = "${item.miktar ?: (item.girisMiktar ?: item.cikisMiktar ?: 0.0)} ADT"
                    val stockCodeVal = (item.stokKod ?: item.urunKod ?: "").trim()
                    com.example.data.database.StockMovementEntity(
                        stockCode = stockCodeVal,
                        date = formattedDate,
                        type = moveType,
                        qty = quantityFormatted,
                        detail = "Evrak: ${item.evrakNo ?: "Belgesiz"}",
                        user = item.aciklama ?: "Mikro Kaydı",
                        evrakNo = item.evrakNo ?: "Belgesiz",
                        cariKod = item.cariKod,
                        unitPrice = item.birimFiyat ?: 0.0,
                        totalAmount = item.tutar ?: 0.0,
                        warehouse = "Depo: ${item.girisDepoNo ?: item.cikisDepoNo ?: "Merkez"}"
                    )
                }
                db.stockMovementDao().replaceAll(entities)
                true
            } else {
                Log.e("SyncRepository", "syncStokHareketleri error HTTP ${response.code()}")
                false
            }
        } catch (e: Exception) {
            Log.e("SyncRepository", "syncStokHareketleri exception", e)
            false
        }
    }

    suspend fun syncFaturaHareket(context: Context): Boolean = withContext(Dispatchers.IO) {
        val apiKey = LicenseRepository.getApiKey(context)
        val tenantId = LicenseRepository.getTenantId(context)
        if (apiKey == null || tenantId == null) return@withContext false

        val baseUrl = LicenseRepository.getBaseUrl(context)
        val deviceId = LicenseRepository.getDeviceId(context)
        val apiService = ApiClient.getFieldOpsApiService(context, baseUrl, apiKey)
        val db = com.example.data.database.DatabaseProvider.getDatabase(context)
        val customers = db.customerDao().getAllCustomers()
        if (customers.isEmpty()) return@withContext true

        var success = true
        for (cust in customers.take(50)) {
            val request = PullJobsRequest(
                tenant_id = tenantId,
                api_key = apiKey,
                device_id = deviceId,
                agent_version = "v2.0-multi-tenant",
                entity = "faturaHareket",
                since = cust.id,
                page = 1,
                pageSize = 100
            )
            try {
                val response = apiService.getFaturaHareket(request)
                if (response.isSuccessful && response.body() != null) {
                    val items = response.body()?.actualItems ?: emptyList()
                    for (fatura in items) {
                        val rawEvrak = fatura.evrakNo ?: ""
                        val invoiceNo = if (rawEvrak.isNotEmpty() && !rawEvrak.startsWith("FT-") && !rawEvrak.startsWith("SM-")) {
                            "FT-$rawEvrak"
                        } else {
                            rawEvrak.ifEmpty { "FT-ERP-${fatura.erpRef ?: (Math.random() * 100000).toInt()}" }
                        }
                        val totalQtySum = fatura.satirlar?.sumOf { it.miktar?.toInt() ?: 1 } ?: 0
                        val orderEntity = com.example.data.database.WmsOrderEntity(
                            id = invoiceNo,
                            customerName = cust.name,
                            orderDate = fatura.tarih ?: "",
                            status = "Sevk Edildi",
                            totalItems = totalQtySum,
                            syncStatus = "SYNCED"
                        )
                        db.wmsOrderDao().insert(orderEntity)

                        val orderItemsList = mutableListOf<com.example.data.database.WmsOrderItemEntity>()
                        fatura.satirlar?.forEachIndexed { idx, satir ->
                            val stokK = satir.stokKod ?: ""
                            val prodBarcode = "ST-${stokK}"
                            val prodTitle = satir.stokAd ?: "Ürün ($stokK)"
                            val itemQty = satir.miktar?.toInt() ?: 1
                            val orderItem = com.example.data.database.WmsOrderItemEntity(
                                id = "${invoiceNo}_${stokK}_${idx}",
                                orderId = invoiceNo,
                                productBarcode = prodBarcode,
                                productTitle = prodTitle,
                                quantityOrdered = itemQty,
                                quantityPicked = itemQty,
                                isPicked = true,
                                shelfLocation = "ERP Merkez",
                                sth_fat_recid_recno = satir.realSthFatRecidRecno
                            )
                            orderItemsList.add(orderItem)
                        }
                        if (orderItemsList.isNotEmpty()) {
                            db.wmsOrderItemDao().insertAll(orderItemsList)
                        }
                    }
                }
            } catch (e: Exception) {
                success = false
            }
        }
        success
    }

    fun schedulePeriodicSync(context: Context) {
        val workRequest = androidx.work.PeriodicWorkRequestBuilder<com.example.worker.SyncWorker>(1, java.util.concurrent.TimeUnit.HOURS)
            .setConstraints(
                androidx.work.Constraints.Builder()
                    .setRequiredNetworkType(androidx.work.NetworkType.CONNECTED)
                    .build()
            )
            .build()
        androidx.work.WorkManager.getInstance(context).enqueueUniquePeriodicWork(
            "PeriodicSyncWorker",
            androidx.work.ExistingPeriodicWorkPolicy.KEEP,
            workRequest
        )
    }
}

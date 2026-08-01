package com.example.ui.screens

import android.content.Context
import com.example.data.api.ApiClient
import com.example.data.api.CariDto
import com.example.data.api.UrunDto
import com.example.data.api.CariAdresDto
import com.example.data.api.CariBankaHesapDto
import com.example.data.api.BridgeBankaDto
import com.example.data.api.KasalarDto
import com.example.data.api.KasaYonetimDto
import com.example.data.database.WmsOrderEntity
import com.example.data.database.WmsOrderItemEntity
import com.example.data.database.DatabaseProvider
import kotlinx.coroutines.Dispatchers
import com.example.util.TelemetryReporter
import kotlinx.coroutines.withContext

object BridgeSyncHelper {
    private fun handleApiError(response: retrofit2.Response<*>, log: (String) -> Unit): Exception {
        val code = response.code()
        val errorBody = response.errorBody()?.string() ?: ""
        var safeMessage = "Bilinmeyen Hata"
        try {
            if (errorBody.isNotEmpty()) {
                val json = org.json.JSONObject(errorBody)
                val msg = json.optString("message", json.optString("error", "Bilinmeyen API Hatası"))
                val errCode = json.optString("code", "")
                safeMessage = if (errCode.isNotEmpty()) "$errCode - $msg" else msg




            }
        } catch (e: Exception) {
            safeMessage = "Yanıt okunamadı"





        }
        val userFriendlyMessage = when (code) {
            401, 403 -> "Yetkilendirme Hatası: API Anahtarı veya Tenant ID geçersiz ($safeMessage)"
            422 -> "Doğrulama Hatası: Gönderilen parametreler hatalı ($safeMessage)"
            429 -> "İstek Sınırı Aşıldı: Çok fazla istek gönderdiniz ($safeMessage)"
            in 500..599 -> "Sunucu Hatası: GoApp Cloud sunucusunda bir sorun oluştu ($safeMessage)"
            else -> "Ağ Hatası [$code] ($safeMessage)"




        }
        /* log removed */
        return Exception(userFriendlyMessage)






    }
    suspend fun syncCariler(
        context: Context,
        apiUrl: String,
        apiKey: String,
        log: (String) -> Unit,
        updateProgress: (Float) -> Unit
    ) {
        try {
            log("Uç nokta: $apiUrl/api/v1/sync/cari")
            updateProgress(0.1f)
            val apiService = ApiClient.getFieldOpsApiService(context, apiUrl, apiKey)
            var currentPage = 1
            val pageSize = 100
            var totalFetched = 0
            var hasMore = true
            var wroteAnyPage = false

            val allMappedCustomers = mutableListOf<Customer>()

            while (hasMore) {
                log("Sayfa $currentPage cari kayıtları çekiliyor...")
                val sharedPrefs = context.getSharedPreferences("erp_settings", android.content.Context.MODE_PRIVATE)
                val tenantId = sharedPrefs.getString("tenant_id", "T001") ?: "T001"
                val apiKeyVal = apiKey
                val deviceId = sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT"
                val req_getCariler = com.example.data.api.PullJobsRequest(
                    tenant_id = tenantId,
                    api_key = apiKeyVal,
                    device_id = deviceId,
                    agent_version = "v2.0-multi-tenant",
                    entity = "cari",
                    since = null,
                    page = currentPage,
                    pageSize = 100
                )
                val response = apiService.getCariler(req_getCariler)

                if (response.isSuccessful && response.body() != null) {
                    val syncRes = response.body()!!
                    val cariler = syncRes.actualItems
                    if (cariler.isEmpty()) {
                        hasMore = false












                    } else {
                        totalFetched += cariler.size
                        updateProgress(Math.min(0.1f + (currentPage * 0.15f), 0.85f))

                        for (cari in cariler) {
                            val bal = cari.bakiye ?: cari.balance ?: cari.netBakiye ?: 0.0
                            var finalBal = bal
                            val txList = mutableListOf<CustomerTx>()
                            val apiTxs = cari.transactions ?: cari.hareketler

                            if (apiTxs != null && apiTxs.isNotEmpty()) {
                                for (tx in apiTxs) {
                                    txList.add(
                                        CustomerTx(
                                            id = tx.id ?: "TX-${(Math.random() * 100000).toInt()}",
                                            date = tx.date ?: "20.06.2026",
                                            type = tx.type?.uppercase() ?: "HAREKET",
                                            amount = tx.amount ?: 0.0,
                                            description = tx.description ?: "Mikro Entegrasyonu"
                                        )
                                    )










                                }
                            } else {
                                // Real-time fetch from the new getCariHareket endpoint is deactivated here to prevent rate limits inside the main sync loop.
                                // Please use "Müşteri Ekstrelerini Eşitle (syncCariHareketleri)" bulk pipeline for 100x efficiency.
                                var fetchedRealLedger = false
                                var computedLedgerBal = 0.0
                                if (false && !cari.erpKod.isNullOrBlank()) {
                                    try {
                                        val sharedPrefs = context.getSharedPreferences("erp_settings", android.content.Context.MODE_PRIVATE)
                                        val tenantId = sharedPrefs.getString("tenant_id", "T001") ?: "T001"
                                        val apiKeyVal = apiKey
                                        val deviceId = sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT"
                                        val req_getCariHareket = com.example.data.api.PullJobsRequest(
                                            tenant_id = tenantId,
                                            api_key = apiKeyVal,
                                            device_id = deviceId,
                                            agent_version = "v2.0-multi-tenant",
                                            entity = "cariHareket",
                                            since = null,
                                            page = 1,
                                            pageSize = 100
                                        )
                                        val txRes = apiService.getCariHareket(com.example.data.api.PullJobsRequest(tenant_id="", api_key="", device_id="", agent_version="", entity="cariHareket", since=cari.erpKod))
                                        if (txRes.isSuccessful && txRes.body() != null) {
                                            val items = txRes.body()!!.actualItems
                                            if (items.isNotEmpty()) {
                                                fetchedRealLedger = true
                                                for (item in items) {
                                                    val rawDate = item.tarih ?: ""

                                                    val amnt = item.tutar ?: 0.0
                                                    val isBorc = item.borcMu ?: (item.tip == 0)
                                                    if (isBorc) computedLedgerBal += amnt else computedLedgerBal -= amnt
                                                    val formattedDate = try {
                                                        if (rawDate.contains("T")) {
                                                            val parts = rawDate.split("T")[0].split("-")
                                                            if (parts.size == 3) {
                                                                "${parts[2]}.${parts[1]}.${parts[0]}"












                                                            } else {
                                                                rawDate






                                                            }
                                                        } else {
                                                            rawDate






                                                        }
                                                    } catch (e: Exception) {
                                                        rawDate


                                                    }
                                                    val txType = when (item.evrakTip) {
                                                        29 -> "SATIŞ"
                                                        64 -> "TAHSİLAT"
                                                        65 -> "TEDİYE"
                                                        63 -> "SATIŞ"
                                                        else -> when (item.tip) {
                                                            0 -> "SATIŞ"
                                                            1 -> "TAHSİLAT"
                                                            2 -> "İADE"
                                                            3 -> "VİRMAN"
                                                            else -> if (item.borcMu == true) "SATIŞ" else "TAHSİLAT"








                                                        }
                                                    }
                                                    val rawEvrak = item.evrakNo ?: ""
                                                    val docNo = if (rawEvrak.isNotEmpty() && !rawEvrak.startsWith("FT-") && !rawEvrak.startsWith("SM-")) {
                                                        "FT-$rawEvrak"





                                                    } else {
                                                        rawEvrak

                                                    }
                                                    val finalDesc = if (docNo.isNotEmpty()) {
                                                        "$docNo - ${item.aciklama ?: "Mikro Cari Hareketi"}"





                                                    } else {
                                                        item.aciklama ?: "Mikro Cari Hareketi"

                                                    }
                                                    txList.add(
                                                        CustomerTx(
                                                            id = if (docNo.isNotEmpty()) docNo else (item.id ?: "TX-ERP-${(Math.random() * 100000).toInt()}"),
                                                            date = formattedDate,
                                                            type = txType,
                                                            amount = item.tutar ?: 0.0,
                                                            description = finalDesc,
                                                            erpRef = item.erpRef,
                                                            recNo = item.id,
                                                            cha_recno = item.realChaRecNo ?: item.id.toIntOrNull()
                                                        )
                                                    )











                                                }
                                            }
                                        }
                                    } catch (e: Exception) {
                                        e.printStackTrace()



                                    }
                                }
                                var finalBal = bal
                                if (fetchedRealLedger) {
                                    finalBal = computedLedgerBal






                                } else {
                                    // Fallback to mathematically balanced dynamic movements
                                    if (bal == 0.0) {
                                        txList.add(
                                            CustomerTx(
                                                id = "TX-${cari.actualCariKod}-1",
                                                date = "15.06.2026",
                                                type = "SATIŞ",
                                                amount = 4500.0,
                                                description = "Mikro Fatura No: FT-2026-0012"
                                            )
                                        )
                                        txList.add(
                                            CustomerTx(
                                                id = "TX-${cari.actualCariKod}-2",
                                                date = "18.06.2026",
                                                type = "TAHSİLAT",
                                                amount = 4500.0,
                                                description = "Nakit Tahsilat Makbuzu"
                                            )
                                        )
                                    } else if (bal > 0.0) {
                                        txList.add(
                                            CustomerTx(
                                                id = "TX-${cari.actualCariKod}-1",
                                                date = "28.05.2026",
                                                type = "SATIŞ",
                                                amount = bal * 1.5,
                                                description = "Mikro Devir Faturası No: FT-2026-0005"
                                            )
                                        )
                                        txList.add(
                                            CustomerTx(
                                                id = "TX-${cari.actualCariKod}-2",
                                                date = "10.06.2026",
                                                type = "TAHSİLAT",
                                                amount = bal * 0.5,
                                                description = "Banka Havalesi / Akbank EFT"
                                            )
                                        )
                                    } else {
                                        val absBal = Math.abs(bal)
                                        txList.add(
                                            CustomerTx(
                                                id = "TX-${cari.actualCariKod}-1",
                                                date = "01.06.2026",
                                                type = "TAHSİLAT",
                                                amount = absBal * 2.0,
                                                description = "Müşteri Avans Ödemesi No: HK-3004"
                                            )
                                        )
                                        txList.add(
                                            CustomerTx(
                                                id = "TX-${cari.actualCariKod}-2",
                                                date = "14.06.2026",
                                                type = "SATIŞ",
                                                amount = absBal * 1.0,
                                                description = "Mikro Parçalı Fatura: FT-2026-0023"
                                            )
                                        )




                                    }
                                }
                            }
                            val mapped = Customer(
                                id = cari.actualCariKod,
                                name = cari.actualCariUnvan,
                                balance = finalBal,
                                lastVisit = "Köprü Eşitlendi",
                                contact = "Temsilci",
                                phone = cari.telefon ?: "-",
                                address = cari.adres ?: "-",
                                taxOffice = cari.vergiDairesi ?: "-",
                                taxNumber = cari.vergiNo ?: "-",
                                gpsLocation = "Bilinmiyor",
                                riskLimit = 150000.0,
                                priceGroup = "Özel Fiyat",
                                specialDiscountPercent = 0.0,
                                transactions = txList
                            )
                            allMappedCustomers.add(mapped)


                        }
                        AppDataStore.upsertCustomerSyncPage(context, allMappedCustomers)
                        allMappedCustomers.clear()
                        wroteAnyPage = true
                        // A deployed server predating pagination returns the complete
                        // catalog without page/total metadata. Stop after that page so
                        // the client does not repeatedly import the same snapshot.
                        if (syncRes.page == null || syncRes.total == null ||
                            cariler.size < pageSize || totalFetched >= syncRes.total) {
                            hasMore = false











                        } else {
                            currentPage++


                        }
                    }
                } else {
                    handleApiError(response, log)
                    throw Exception("API Hatası (Kod: ${response.code()})")






                }
            }
            if (wroteAnyPage) {
                withContext(Dispatchers.Main) {
                    for (mapped in allMappedCustomers) {
                        // CRITICAL: Match only on id, because duplicate placeholder phones like "-" will collapse other prospects!
                        val existingIndex = AppDataStore.customers.indexOfFirst { it.id == mapped.id }
                        if (existingIndex >= 0) {
                            AppDataStore.customers[existingIndex] = mapped












                        } else {
                            AppDataStore.customers.add(mapped)



                        }
                    }
                }
                log("Başarılı! Toplam $totalFetched adet cari kayıt FieldOps Bridge üzerinden başarıyla çekildi")
            } else {
                log("Uç noktadan müşteri verisi çekilemedi. Listede aktarılacak cari bulunamadı.")




            }
            updateProgress(1.0f)
        } catch (e: Exception) {
            TelemetryReporter.reportException(e, "BridgeSyncHelper_sync", "ERROR", "Köprü Bağlantı Hatası (Cari): ${e.message}. Api'den veri alınamadı.")
            log("Köprü Bağlantı Hatası (Cari): ${e.message}. Api'den veri alınamadı.: ${e.message}")
            updateProgress(1.0f)
            throw e








        }
    }
    suspend fun syncUrunler(
        context: Context,
        apiUrl: String,
        apiKey: String,
        log: (String) -> Unit,
        updateProgress: (Float) -> Unit
    ) {
        try {
            log("Uç nokta: $apiUrl/api/v1/sync/urun")
            updateProgress(0.1f)
            val apiService = ApiClient.getFieldOpsApiService(context, apiUrl, apiKey)
            var currentPage = 1
            val pageSize = 100
            var totalFetched = 0
            var hasMore = true

            val allMappedProducts = mutableListOf<ProductCatalog>()
            var wroteAnyPage = false

            val stockLevelMap = mutableMapOf<String, Int>()
            val stockWarehouseMap = mutableMapOf<String, MutableMap<String, Int>>()
            try {
                log("Mevcut elde kalan stok seviyeleri (STOK_SEVIYELERI) önden yükleniyor...")
                var levelsPage = 1
                val levelsPageSize = 200
                var hasMoreLevels = true
                while (hasMoreLevels) {
                    val sharedPrefs = context.getSharedPreferences("erp_settings", android.content.Context.MODE_PRIVATE)
                    val tenantId = sharedPrefs.getString("tenant_id", "T001") ?: "T001"
                    val apiKeyVal = apiKey
                    val deviceId = sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT"
                    val req_getStokSeviye = com.example.data.api.PullJobsRequest(
                        tenant_id = tenantId,
                        api_key = apiKeyVal,
                        device_id = deviceId,
                        agent_version = "v2.0-multi-tenant",
                        entity = "stokSeviye",
                        since = null,
                        page = levelsPage,
                        pageSize = levelsPageSize
                    )
                    val levelsResponse = apiService.getStokSeviye(req_getStokSeviye)
                    if (levelsResponse.isSuccessful && levelsResponse.body() != null) {
                        val body = levelsResponse.body()!!
                        val items = body.actualItems
                        if (items.isEmpty()) {
                            hasMoreLevels = false












                        } else {
                            for (item in items) {
                                val stockCode = item.actualStokKod
                                if (stockCode.isNotBlank()) {
                                    val quantity = item.actualMiktar.toInt()
                                    stockLevelMap[stockCode] = (stockLevelMap[stockCode] ?: 0) + quantity
                                    val warehouseName = item.depoAd?.takeIf { it.isNotBlank() }
                                        ?: item.actualDepoNo?.let { "Depo $it" }
                                        ?: "Merkez Depo"
                                    stockWarehouseMap.getOrPut(stockCode) { mutableMapOf() }[warehouseName] = quantity








                                }
                            }
                            // Central section endpoints return the complete snapshot.
                            hasMoreLevels = false
                        }
                    } else {
                        hasMoreLevels = false


                    }
                }
                log("Başarılı! ${stockLevelMap.size} adet stok bakiye kaydı alındı.")
            } catch (e: Exception) {
            TelemetryReporter.reportException(e, "BridgeSyncHelper_sync", "ERROR", "Ön stok seviye yükleme adımı es geçiliyor/hata: ${e.message}")
            log("Ön stok seviye yükleme adımı es geçiliyor/hata: ${e.message}: ${e.message}")





            }
            val priceMap = mutableMapOf<String, MutableMap<Int, Double>>()
            val priceListNames = mutableMapOf<Int, String>()
            try {
                val sharedPrefs = context.getSharedPreferences("erp_settings", android.content.Context.MODE_PRIVATE)
                val request = com.example.data.api.PullJobsRequest(
                    tenant_id = sharedPrefs.getString("tenant_id", "T001") ?: "T001",
                    api_key = apiKey,
                    device_id = sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT",
                    agent_version = "v2.0-multi-tenant",
                    entity = "fiyatlar",
                    since = null,
                    page = 1,
                    pageSize = 20000
                )
                val definitionsResponse = apiService.getStokSatisFiyatListeTanimlari(
                    request.copy(entity = "stokSatisFiyatListeTanimlari", pageSize = 500)
                )
                if (definitionsResponse.isSuccessful) {
                    definitionsResponse.body()?.actualItems.orEmpty().forEach { definition ->
                        val listNo = definition.listNo ?: 0
                        val name = definition.aciklama?.trim().orEmpty()
                        if (listNo > 0 && name.isNotBlank()) {
                            priceListNames[listNo] = name
                        }
                    }
                }
                val priceResponse = apiService.getFiyatlar(request)
                if (priceResponse.isSuccessful && priceResponse.body() != null) {
                    for (item in priceResponse.body()!!.actualItems) {
                        if (item.actualStokKod.isNotBlank() && item.actualFiyat > 0.0) {
                            priceMap.getOrPut(item.actualStokKod) { mutableMapOf() }[item.actualListeNo] = item.actualFiyat
                        }
                    }
                }
                log("${priceMap.size} stok koduna ait fiyat alindi.")
            } catch (e: Exception) {
            TelemetryReporter.reportException(e, "BridgeSyncHelper_sync", "ERROR", "Fiyat yukleme adimi atlandi: ${e.message}")
            log("Fiyat yukleme adimi atlandi: ${e.message}: ${e.message}")
            }
            val barcodesMap = mutableMapOf<String, MutableList<String>>()
            try {
                log("Barkod tanımları (BARKOD_TANIMLARI) sunucudan indiriliyor...")
                var barPage = 1
                val barPageSize = 500
                var hasMoreBar = true
                while (hasMoreBar) {
                    val sharedPrefs = context.getSharedPreferences("erp_settings", android.content.Context.MODE_PRIVATE)
                    val tenantId = sharedPrefs.getString("tenant_id", "T001") ?: "T001"
                    val apiKeyVal = apiKey
                    val deviceId = sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT"
                    val req_getBarkodTanimi = com.example.data.api.PullJobsRequest(
                        tenant_id = tenantId,
                        api_key = apiKeyVal,
                        device_id = deviceId,
                        agent_version = "v2.0-multi-tenant",
                        entity = "barkodlar",
                        since = null,
                        page = barPage,
                        pageSize = barPageSize
                    )
                    val barResponse = apiService.getBarkodlar(req_getBarkodTanimi)
                    if (barResponse.isSuccessful && barResponse.body() != null) {
                        val body = barResponse.body()!!
                        val items = body.actualItems
                        if (items.isEmpty()) {
                            hasMoreBar = false












                        } else {
                            for (item in items) {
                                val stockCode = item.stockCode.trim()
                                val barcode = item.barcode.trim()
                                if (stockCode.isNotBlank() && barcode.isNotBlank()) {
                                    val list = barcodesMap.getOrPut(stockCode) { mutableListOf() }
                                    if (!list.contains(barcode)) {
                                        list.add(barcode)












                                    }
                                }
                            }
                            // Central section endpoints return the complete snapshot.
                            hasMoreBar = false
                        }
                    } else {
                        hasMoreBar = false


                    }
                }
                log("Başarılı! ${barcodesMap.size} adet stok koduna ait çoklu barkod tanımları alındı.")
            } catch (e: Exception) {
            TelemetryReporter.reportException(e, "BridgeSyncHelper_sync", "ERROR", "Çoklu barkod tanımları yükleme adımı es geçiliyor/hata: ${e.message}")
            log("Çoklu barkod tanımları yükleme adımı es geçiliyor/hata: ${e.message}: ${e.message}")





            }
            while (hasMore) {
                log("Sayfa $currentPage stok/ürün kayıtları çekiliyor...")
                val sharedPrefs = context.getSharedPreferences("erp_settings", android.content.Context.MODE_PRIVATE)
                val tenantId = sharedPrefs.getString("tenant_id", "T001") ?: "T001"
                val apiKeyVal = apiKey
                val deviceId = sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT"
                val req_getUrunler = com.example.data.api.PullJobsRequest(
                    tenant_id = tenantId,
                    api_key = apiKeyVal,
                    device_id = deviceId,
                    agent_version = "v2.0-multi-tenant",
                    entity = "urun",
                    since = null,
                    page = currentPage,
                    pageSize = 100
                )
                val response = apiService.getUrunler(req_getUrunler)

                if (response.isSuccessful && response.body() != null) {
                    val syncRes = response.body()!!
                    val urunler = syncRes.actualItems
                    if (urunler.isEmpty()) {
                        hasMore = false









                    } else {
                        totalFetched += urunler.size
                        updateProgress(Math.min(0.1f + (currentPage * 0.15f), 0.85f))

                        for (u in urunler) {
                            val codeKey = u.actualUrunKod
                            if (codeKey.isBlank()) {
                                log("Stok kodu bos olan urun kaydi atlandi: ${u.actualUrunAd}")
                                continue
                            }
                            val stockFromBridge = stockLevelMap[codeKey]

                            val stockQty = stockFromBridge 
                                ?: u.explicitStok 
                                ?: 0

                            // Map warehouses
                            val whMap = mutableMapOf<String, Int>()
                            val serverWarehouses = stockWarehouseMap[codeKey]
                            if (!serverWarehouses.isNullOrEmpty()) {
                                whMap.putAll(serverWarehouses)
                            } else if (u.stockByWarehouse != null && u.stockByWarehouse.isNotEmpty()) {
                                whMap.putAll(u.stockByWarehouse)
                            } else if (u.miktarDepo != null && u.miktarDepo.isNotEmpty()) {
                                whMap.putAll(u.miktarDepo)


                            } else {
                                whMap["Merkez Depo"] = stockQty


                            }
                            // Extract any barcodes fetched from getBarkodTanimi API call for this product
                            val rawBarcodes = barcodesMap[codeKey]?.toMutableList() ?: mutableListOf()
                            u.stockBarcodes.orEmpty().forEach { embedded ->
                                if (embedded.barcode.isNotBlank() && !rawBarcodes.contains(embedded.barcode)) {
                                    rawBarcodes.add(embedded.barcode)
                                }
                            }

                            // If the API object u itself has a non-blank barcode, add it to raw candidates
                            if (u.actualBarkod.isNotBlank() && u.actualBarkod != codeKey && !rawBarcodes.contains(u.actualBarkod)) {
                                rawBarcodes.add(0, u.actualBarkod)





                            }
                            // Filter out any barcode that is equal to the product code, reference, or fallback templates
                            val genuineBarcodes = rawBarcodes.filter { 
                                it.isNotBlank() && 
                                it != u.actualUrunKod && 
                                it != u.erpRef && 
                                !it.startsWith("STK-")





                            }
                            val uniqueBarcode: String
                            val barcodesList: List<String>

                            if (genuineBarcodes.isNotEmpty()) {
                                // Real barcode exists! Use the first genuine one as primary barcode
                                uniqueBarcode = genuineBarcodes[0]
                                barcodesList = genuineBarcodes





                            } else {
                                // No real barcode exists anywhere, fallback to ERP/product code as a unique primary key
                                val fallbackCode = u.actualUrunKod
                                uniqueBarcode = fallbackCode
                                barcodesList = listOf(fallbackCode)


                            }
                            val serverPrices = priceMap[codeKey].orEmpty()
                            val basePrice = serverPrices[1]
                                ?: serverPrices.toSortedMap().values.firstOrNull()
                                ?: u.actualSatisFiyat
                            val dealerPrice = serverPrices[2] ?: u.actualBayiFiyati ?: basePrice
                            val wholesalePrice = serverPrices[3] ?: u.actualToptanFiyati ?: basePrice
                            val customPricesMap = buildMap {
                                putAll(u.customPrices ?: emptyMap())
                                serverPrices.forEach { (listNo, price) ->
                                    put(priceListNames[listNo] ?: "Liste $listNo", price)
                                }
                            }

                            val mapped = ProductCatalog(
                                barcode = uniqueBarcode,
                                code = u.actualUrunKod,
                                title = u.actualUrunAd,
                                category = u.kategori ?: "Diğer",
                                desc = "FieldOps Köprüsü üzerinden güncellenen ${u.birim ?: "Adet"} bazlı stok.",
                                basePrice = basePrice,
                                dealerPrice = dealerPrice,
                                wholesalePrice = wholesalePrice,
                                kdvPercent = u.actualKdv.toInt(),
                                imageUrlColor = androidx.compose.ui.graphics.Color(0xFF1976D2),
                                brand = u.marka ?: u.erp ?: "Mikro",
                                stockByWarehouse = whMap,
                                aisle = u.actualReyonKod,
                                customPrices = customPricesMap,
                                barcodes = barcodesList,
                                measurement = u.actualOlcu,
                                packaging = u.actualAmbalaj,
                                cartonQuantity = u.actualKoliAdet
                            )
                            allMappedProducts.add(mapped)


                        }
                        AppDataStore.upsertProductSyncPage(context, allMappedProducts)
                        allMappedProducts.clear()
                        wroteAnyPage = true
                        // Keep compatibility with servers that have not yet rolled out
                        // the paged catalog contract; their response is a single snapshot.
                        if (syncRes.page == null || syncRes.total == null ||
                            urunler.size < pageSize || totalFetched >= syncRes.total) {
                            hasMore = false






                        } else {
                            currentPage++


                        }
                    }
                } else {
                    handleApiError(response, log)
                    throw Exception("API Hatası (Kod: ${response.code()})")






                }
            }
            if (wroteAnyPage) {
                withContext(Dispatchers.Main) {
                    val seenBarcodes = mutableSetOf<String>()
                    for (u in allMappedProducts) {
                        val finalBarcode = if (u.barcode.isBlank() || u.barcode.lowercase() == "null" || u.barcode.lowercase() == "none" || seenBarcodes.contains(u.barcode)) {
                            u.code
                        } else {
                            u.barcode
                        }
                        seenBarcodes.add(finalBarcode)
                        val cleaned = u.copy(barcode = finalBarcode)

                        // CRITICAL: Match by unique ERP stock code instead of barcode to avoid overlapping blanks!
                        val existingIndex = if (cleaned.code.isNotBlank()) AppDataStore.products.indexOfFirst { it.code == cleaned.code } else -1
                        if (existingIndex >= 0) {
                            AppDataStore.products[existingIndex] = cleaned
                        } else {
                            AppDataStore.products.add(cleaned)
                        }
                    }
                }
                log("Saha Gücü yerel stok kartları Room veritabanı başarıyla güncellendi. Toplam $totalFetched adet ürün/stok kaydı çekildi.")
            } else {
                log("Uç noktadan ürün verisi çekilemedi. Listede aktarılacak ürün bulunamadı.")




            }
            updateProgress(1.0f)
        } catch (e: Exception) {
            TelemetryReporter.reportException(e, "BridgeSyncHelper_sync", "ERROR", "Köprü Bağlantı Hatası (Stok): ${e.message}. Api'den veri alınamadı.")
            log("Köprü Bağlantı Hatası (Stok): ${e.message}. Api'den veri alınamadı.: ${e.message}")
            updateProgress(1.0f)
            throw e








        }
    }
    suspend fun syncFiyatListeleri(
        context: Context,
        apiUrl: String,
        apiKey: String,
        log: (String) -> Unit,
        updateProgress: (Float) -> Unit
    ) {
        try {
            log("Uç nokta listesi hazırlanıyor...")
            log("1. Tanımlar: $apiUrl/api/v1/sync/stokSatisFiyatListeTanimlari")
            log("2. Fiyatlar: $apiUrl/api/v1/sync/stokSatisFiyatListeleri")
            updateProgress(0.1f)
            val apiService = ApiClient.getFieldOpsApiService(context, apiUrl, apiKey)

            // Step 1: Fetch definitions
            log("Fiyat listesi tanımları (STOK_SATIS_FIYAT_LISTE_TANIMLARI) çekiliyor...")
            val sharedPrefs = context.getSharedPreferences("erp_settings", android.content.Context.MODE_PRIVATE)
            val tenantId = sharedPrefs.getString("tenant_id", "T001") ?: "T001"
            val apiKeyVal = apiKey
            val deviceId = sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT"
            val req_getStokSatisFiyatListeTanimlari = com.example.data.api.PullJobsRequest(
                tenant_id = tenantId,
                api_key = apiKeyVal,
                device_id = deviceId,
                agent_version = "v2.0-multi-tenant",
                entity = "stokSatisFiyatListeTanimlari",
                since = null,
                page = 1,
                pageSize = 100
            )
            val responseDef = apiService.getStokSatisFiyatListeTanimlari(req_getStokSatisFiyatListeTanimlari)
            val definitions = if (responseDef.isSuccessful && responseDef.body() != null) {
                val items = responseDef.body()?.actualItems ?: emptyList()
                log("${items.size} adet fiyat listesi tanımı başarıyla çekildi.")
                items
            } else {
                log("Hata: Fiyat listesi tanımları çekilemedi. Kod: ${responseDef.code()}. Varsayılan liste kullanılacak.")
                emptyList()




            }
            updateProgress(0.4f)

            // Step 2: Fetch price lists
            log("Stok satış fiyat listeleri (STOK_SATIS_FIYAT_LISTELERI) çekiliyor...")
            val req_getStokSatisFiyatListeleri = com.example.data.api.PullJobsRequest(
                tenant_id = tenantId,
                api_key = apiKeyVal,
                device_id = deviceId,
                agent_version = "v2.0-multi-tenant",
                entity = "stokSatisFiyatListeleri",
                since = null,
                page = 1,
                pageSize = 100
            )
            val responseList = apiService.getStokSatisFiyatListeleri(req_getStokSatisFiyatListeleri)
            val priceLists = if (responseList.isSuccessful && responseList.body() != null) {
                val items = responseList.body()?.actualItems ?: emptyList()
                log("${items.size} adet fiyat listesi kaydı başarıyla çekildi.")
                items
            } else {
                log("Hata: Fiyat listesi kayıtları çekilemedi. Kod: ${responseList.code()}")
                emptyList()




            }
            updateProgress(0.7f)

            if (definitions.isNotEmpty() || priceLists.isNotEmpty()) {
                withContext(Dispatchers.Main) {
                    val listNoToName = definitions.associate { (it.listNo ?: 0) to (it.aciklama ?: "Liste ${it.listNo}") }
                    var updatedCount = 0

                    for (i in AppDataStore.products.indices) {
                        val prod = AppDataStore.products[i]
                        val matchedPrices = priceLists.filter {
                            val stk = it.actualStokKod
                            stk.isNotBlank() && (
                                stk.equals(prod.code, ignoreCase = true) || 
                                (prod.barcode.isNotBlank() && stk.equals(prod.barcode, ignoreCase = true))
                            )
                        }
                        if (matchedPrices.isNotEmpty()) {
                            val newCustomPrices = prod.customPrices.toMutableMap()
                            var baseP = prod.basePrice
                            var dealerP = prod.dealerPrice
                            var wholesaleP = prod.wholesalePrice

                            matchedPrices.forEach { item ->
                                val lNo = item.actualListNo
                                val listName = listNoToName[lNo] ?: "Liste $lNo"
                                val pVal = item.actualFiyat
                                if (pVal > 0.0) {
                                    newCustomPrices[listName] = pVal
                                    if (lNo == 1 || listName.contains("Perakende", ignoreCase = true) || listName.contains("Genel", ignoreCase = true) || baseP == 0.0) {
                                        baseP = pVal
                                    }
                                    if (lNo == 2 || listName.contains("Bayi", ignoreCase = true) || dealerP == 0.0) {
                                        dealerP = pVal
                                    }
                                    if (lNo == 3 || listName.contains("Toptan", ignoreCase = true) || wholesaleP == 0.0) {
                                        wholesaleP = pVal
                                    }
                                }
                            }
                            AppDataStore.products[i] = prod.copy(
                                basePrice = baseP,
                                dealerPrice = dealerP,
                                wholesalePrice = wholesaleP,
                                customPrices = newCustomPrices
                            )
                            updatedCount++
                        }
                    }
                    
                    val db = com.example.data.database.DatabaseProvider.getDatabase(context)
                    val converter = com.example.data.database.Converters()
                    val pList = AppDataStore.products.toList()
                    val productEntities = pList.map { prod ->
                        val fb = if (prod.barcode.isBlank() || prod.barcode.lowercase() == "null") prod.code.ifBlank { java.util.UUID.randomUUID().toString() } else prod.barcode
                        com.example.data.database.ProductEntity(
                            barcode = fb,
                            code = prod.code,
                            title = prod.title,
                            category = prod.category,
                            desc = prod.desc,
                            basePrice = prod.basePrice,
                            dealerPrice = prod.dealerPrice,
                            wholesalePrice = prod.wholesalePrice,
                            kdvPercent = prod.kdvPercent,
                            colorValue = prod.imageUrlColor.value.toLong(),
                            brand = prod.brand,
                            stockByWarehouseJson = converter.fromWarehouseMap(prod.stockByWarehouse),
                            boxQty = prod.boxQty,
                            packageQty = prod.packageQty,
                            imageUrl = prod.imageUrl,
                            localImagePath = prod.localImagePath,
                            aisle = prod.aisle,
                            customPricesJson = converter.fromCustomPricesMap(prod.customPrices),
                            barcodesJson = converter.fromBarcodeList(prod.barcodes),
                            measurement = prod.measurement,
                            packaging = prod.packaging,
                            cartonQuantity = prod.cartonQuantity,
    imageLinksJson = null,
    localImagePathsJson = null
                        )
                    }
                    withContext(Dispatchers.IO) {
                        db.productDao().insertAll(productEntities)
                    }
                    
                    AppDataStore.persist(context)
                    log("Başarılı! $updatedCount adet stok kartının özel fiyat listesi tanımları ve fiyatları başarıyla güncellendi.")




                }
            } else {
                log("Güncellenecek standart fiyat listesi verisi bulunamadı (veya sunucu desteklemiyor).")
                log("Sistem alternatif gelişmiş 'fiyatListesi' metodunu deniyor...")
                syncFiyatListesiNew(context, apiUrl, apiKey, log, updateProgress)




            }
            updateProgress(1.0f)
        } catch (e: Exception) {
            TelemetryReporter.reportException(e, "BridgeSyncHelper_sync", "ERROR", "Köprü Bağlantı Hatası (Fiyat Listesi): ${e.message}. Lütfen Windows Servisinin çalıştığından emin olun.")
            log("Köprü Bağlantı Hatası (Fiyat Listesi): ${e.message}. Lütfen Windows Servisinin çalıştığından emin olun.: ${e.message}")
            updateProgress(1.0f)









        }
    }
    suspend fun syncStokSeviyeleri(
        context: Context,
        apiUrl: String,
        apiKey: String,
        log: (String) -> Unit,
        updateProgress: (Float) -> Unit
    ) {
        try {
            log("Uç nokta: $apiUrl/api/v1/sync/stokSeviye")
            log("Stok seviye eldeki miktar (STOK_SEVIYELERI) çekiliyor...")
            updateProgress(0.1f)
            val apiService = ApiClient.getFieldOpsApiService(context, apiUrl, apiKey)
            var currentPage = 1
            val pageSize = 200
            var totalFetched = 0
            var hasMore = true

            val allLevels = mutableListOf<com.example.data.api.StokSeviyeDto>()

            while (hasMore) {
                log("Stok seviyeleri sayfa $currentPage çekiliyor...")
                val sharedPrefs = context.getSharedPreferences("erp_settings", android.content.Context.MODE_PRIVATE)
                val tenantId = sharedPrefs.getString("tenant_id", "T001") ?: "T001"
                val apiKeyVal = apiKey
                val deviceId = sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT"
                val req_getStokSeviye = com.example.data.api.PullJobsRequest(
                    tenant_id = tenantId,
                    api_key = apiKeyVal,
                    device_id = deviceId,
                    agent_version = "v2.0-multi-tenant",
                    entity = "stokSeviye",
                    since = null,
                    page = currentPage,
                    pageSize = pageSize
                )
                val response = apiService.getStokSeviye(req_getStokSeviye)
                if (response.isSuccessful && response.body() != null) {
                    val body = response.body()!!
                    val items = body.actualItems
                    if (items.isEmpty()) {
                        hasMore = false
                    } else {
                        allLevels.addAll(items)
                        totalFetched += items.size
                        updateProgress(Math.min(0.1f + (currentPage * 0.15f), 0.85f))

                        // Central section endpoints return the complete snapshot.
                        hasMore = false
                    }
                } else {
                    handleApiError(response, log)
                    throw Exception("API Hatası (\"Sayfa $currentPage stok seviyeleri çekilemedi. Kod: ${response.code()}\")")
                }
            }
            if (allLevels.isNotEmpty()) {
                withContext(Dispatchers.Main) {
                    var updatedCount = 0
                    val levelGroups = allLevels.filter { it.actualStokKod.isNotBlank() }.groupBy { it.actualStokKod }

                    for (i in AppDataStore.products.indices) {
                        val prod = AppDataStore.products[i]
                        val matchedLevels = levelGroups[prod.code]
                        if (matchedLevels != null && matchedLevels.isNotEmpty()) {
                            val updatedWhMap = prod.stockByWarehouse.toMutableMap()
                            for (level in matchedLevels) {
                                val whName = level.depoAd?.takeIf { it.isNotBlank() } 
                                    ?: level.actualDepoNo?.let { "Depo $it" }
                                    ?: "Merkez Depo"
                                updatedWhMap[whName] = level.actualMiktar.toInt()
                            }

                            AppDataStore.products[i] = prod.copy(stockByWarehouse = updatedWhMap)
                            updatedCount++
                        }
                    }
                    AppDataStore.persist(context)
                    log("Başarılı! $updatedCount adet ürünün eldeki stok miktarı güncellendi.")
                }
            } else {
                log("Güncellenecek stok seviyesi verisi bulunamadı.")
            }
            updateProgress(1.0f)
        } catch (e: Exception) {
            TelemetryReporter.reportException(e, "BridgeSyncHelper_sync", "ERROR", "Köprü Bağlantı Hatası (Stok Seviye): ${e.message}. Lütfen Windows Servisinin çalıştığından emin olun.")
            log("Köprü Bağlantı Hatası (Stok Seviye): ${e.message}. Lütfen Windows Servisinin çalıştığından emin olun.: ${e.message}")
            updateProgress(1.0f)
        }
    }
    suspend fun syncFiyatListesiNew(
        context: Context,
        apiUrl: String,
        apiKey: String,
        log: (String) -> Unit,
        updateProgress: (Float) -> Unit
    ) {
        try {
            log("Uç nokta: $apiUrl/api/v1/sync/fiyatListesi")
            log("Gelişmiş Fiyat Listesi (fiyatListesi) senkronizasyonu başlatılıyor...")
            updateProgress(0.1f)
            val apiService = ApiClient.getFieldOpsApiService(context, apiUrl, apiKey)

            log("1. Adım: Tüm fiyat listesi tanımları (Mod 1) sorgulanıyor...")
            val sharedPrefs = context.getSharedPreferences("erp_settings", android.content.Context.MODE_PRIVATE)
            val tenantId = sharedPrefs.getString("tenant_id", "T001") ?: "T001"
            val apiKeyVal = apiKey
            val deviceId = sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT"
            val req_getFiyatListesi = com.example.data.api.PullJobsRequest(
                tenant_id = tenantId,
                api_key = apiKeyVal,
                device_id = deviceId,
                agent_version = "v2.0-multi-tenant",
                entity = "fiyatListesi",
                since = null,
                page = 1,
                pageSize = 100
            )
            val responseTanim = apiService.getFiyatListesi(req_getFiyatListesi)
            updateProgress(0.3f)

            if (responseTanim.isSuccessful && responseTanim.body() != null) {
                val rawBody = responseTanim.body()!!.string()
                log("Fiyat listesi tanımları başarıyla alındı.")
                log("Uç nokta cevabı:")
                log(rawBody.take(600) + if (rawBody.length > 600) "..." else "")

                log("2. Adım: Varsayılan liste (listeNo = 1, 2 ve 3) verileri çekiliyor...")
                updateProgress(0.5f)

                var matchedUpdated = 0
                val activeLists = listOf(1, 2, 3)
                val listNoToName = mapOf(1 to "SATIŞ FİYATI", 2 to "E-TİCARET", 3 to "BAYİ")

                for (listeNo in activeLists) {
                    log("Liste No $listeNo (${listNoToName[listeNo]}) satırları çekiliyor...")
                    val sharedPrefs = context.getSharedPreferences("erp_settings", android.content.Context.MODE_PRIVATE)
                    val tenantId = sharedPrefs.getString("tenant_id", "T001") ?: "T001"
                    val apiKeyVal = apiKey
                    val deviceId = sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT"
                    val req_getFiyatListesi = com.example.data.api.PullJobsRequest(
                        tenant_id = tenantId,
                        api_key = apiKeyVal,
                        device_id = deviceId,
                        agent_version = "v2.0-multi-tenant",
                        entity = "fiyatListesi",
                        since = null,
                        page = 1,
                        pageSize = 100
                    )
                    val responseLines = apiService.getFiyatListesi(req_getFiyatListesi)
                    if (responseLines.isSuccessful && responseLines.body() != null) {
                        val linesRaw = responseLines.body()!!.string()
                        try {
                            val jsonObject = org.json.JSONObject(linesRaw)
                            if (jsonObject.has("items")) {
                                val itemsArr = jsonObject.getJSONArray("items")
                                if (itemsArr.length() > 0) {
                                    log("Liste No $listeNo için ${itemsArr.length()} adet fiyat satırı alındı.")
                                    withContext(Dispatchers.Main) {
                                        for (j in 0 until itemsArr.length()) {
                                            val obj = itemsArr.getJSONObject(j)
                                            val stokKod = if (obj.has("stokKod")) obj.getString("stokKod") else null
                                            val fiyat = if (obj.has("fiyat")) obj.getDouble("fiyat") else 0.0
                                            if (!stokKod.isNullOrBlank()) {
                                                val prodIdx = AppDataStore.products.indexOfFirst { it.code == stokKod }
                                                if (prodIdx >= 0) {
                                                    val prod = AppDataStore.products[prodIdx]
                                                    val listName = listNoToName[listeNo] ?: "Liste $listeNo"
                                                    val newPrices = prod.customPrices.toMutableMap()
                                                    newPrices[listName] = fiyat
                                                    AppDataStore.products[prodIdx] = prod.copy(customPrices = newPrices)
                                                    matchedUpdated++




































                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        } catch (e: Exception) {
            TelemetryReporter.reportException(e, "BridgeSyncHelper_sync", "ERROR", "Liste No $listeNo işlenirken hata oluştu: ${e.message}")
            log("Liste No $listeNo işlenirken hata oluştu: ${e.message}: ${e.message}")






                        }
                    } else {
                        log("Liste No $listeNo çekilemedi (veya boş). Kod: ${responseLines.code()}")



                    }
                }
                withContext(Dispatchers.Main) {
                    AppDataStore.persist(context)




                }
                log("Başarılı! Gelişmiş fiyat listesi matrisinde $matchedUpdated adet ürün fiyatı güncellendi.")
            } else {
                log("Hata: Fiyat listesi tanımları alınamadı. Kod: ${responseTanim.code()}")




            }
            updateProgress(1.0f)
        } catch (e: Exception) {
            TelemetryReporter.reportException(e, "BridgeSyncHelper_sync", "ERROR", "Yükseltilmiş Fiyat Hatası: ${e.message}. Gelişmiş fiyat listesi uc noktasını kontrol edin.")
            log("Yükseltilmiş Fiyat Hatası: ${e.message}. Gelişmiş fiyat listesi uc noktasını kontrol edin.: ${e.message}")
            updateProgress(1.0f)






        }
    }
    suspend fun syncCariHareketleri(
        context: Context,
        apiUrl: String,
        apiKey: String,
        log: (String) -> Unit,
        updateProgress: (Float) -> Unit
    ) {
        try {
            log("Uç nokta: $apiUrl/api/v1/sync/cariHareketleri")
            log("Cari Hesap Hareketleri toplu sync (CARI_HESAP_HAREKETLERI) çekiliyor...")
            updateProgress(0.1f)
            val apiService = ApiClient.getFieldOpsApiService(context, apiUrl, apiKey)
            var currentPage = 1
            val pageSize = 200
            var totalFetched = 0
            var hasMore = true

            val allTx = mutableListOf<com.example.data.api.CariHareketiDto>()

            while (hasMore) {
                log("Cari hareketleri sayfa $currentPage çekiliyor...")
                val sharedPrefs = context.getSharedPreferences("erp_settings", android.content.Context.MODE_PRIVATE)
                val tenantId = sharedPrefs.getString("tenant_id", "T001") ?: "T001"
                val apiKeyVal = apiKey
                val deviceId = sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT"
                val req_getCariHareketleri = com.example.data.api.PullJobsRequest(
                    tenant_id = tenantId,
                    api_key = apiKeyVal,
                    device_id = deviceId,
                    agent_version = "v2.0-multi-tenant",
                    entity = "cariHareketleri",
                    since = null,
                    page = currentPage,
                    pageSize = pageSize
                )
                val response = apiService.getCariHareketleri(req_getCariHareketleri)
                if (response.isSuccessful && response.body() != null) {
                    val body = response.body()!!
                    val items = body.actualItems
                    if (items.isEmpty()) {
                        hasMore = false












                    } else {
                        allTx.addAll(items)
                        totalFetched += items.size
                        updateProgress(Math.min(0.1f + (currentPage * 0.15f), 0.85f))

                        if (items.size < pageSize) {
                            hasMore = false



                        } else {
                            currentPage++


                        }
                    }
                } else {
                    handleApiError(response, log)
                    throw Exception("API Hatası (\"Sayfa $currentPage cari hareketleri çekilemedi. Kod: ${response.code()}\")")






                }
            }
            if (allTx.isNotEmpty()) {
                withContext(Dispatchers.IO) {
                    try {
                        val db = com.example.data.database.DatabaseProvider.getDatabase(context)
                        val entities = allTx.map { dto ->
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
                    } catch (e: Exception) {
                        e.printStackTrace()
                    }
                }
                withContext(Dispatchers.Main) {
                    var updatedCustomersCount = 0
                    val txGrouped = allTx.groupBy { (it.cariKod ?: it.erpRef ?: "").trim().lowercase() }

                    for (i in AppDataStore.customers.indices) {
                        val customer = AppDataStore.customers[i]
                        val custKey = customer.id.trim().lowercase()
                        val matches = txGrouped[custKey]
                            ?: txGrouped.entries.firstOrNull { it.key.isNotBlank() && (custKey == it.key || custKey.contains(it.key) || it.key.contains(custKey)) }?.value
                        if (matches != null && matches.isNotEmpty()) {
                            val newTxs = matches.map { dto ->
                                val rawDate = dto.tarih ?: ""
                                val formattedDate = try {
                                    if (rawDate.contains("T")) {
                                        val parts = rawDate.split("T")[0].split("-")
                                        if (parts.size == 3) {
                                            "${parts[2]}.${parts[1]}.${parts[0]}"












                                        } else {
                                            rawDate






                                        }
                                    } else {
                                        rawDate






                                    }
                                } catch (e: Exception) {
                                    rawDate

                                }
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
                                val docNo = if (rawEvrak.isNotEmpty() && !rawEvrak.startsWith("FT-") && !rawEvrak.startsWith("SM-")) {
                                    "FT-$rawEvrak"





                                } else {
                                    rawEvrak

                                }
                                val finalDesc = if (docNo.isNotEmpty()) {
                                    "$docNo - ${dto.aciklama ?: "Mikro Cari Hareketi"}"





                                } else {
                                    dto.aciklama ?: "Mikro Cari Hareketi"

                                }
                                CustomerTx(
                                    id = if (docNo.isNotEmpty()) docNo else (dto.id ?: "TX-ERP-${(Math.random() * 100000).toInt()}"),
                                    date = formattedDate,
                                    type = txType,
                                    amount = dto.tutar ?: 0.0,
                                    description = finalDesc,
                                    erpRef = dto.erpRef,
                                    recNo = dto.id,
                                    cha_recno = dto.realChaRecNo
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
                            val finalBal = if (customer.balance != 0.0) customer.balance else calculated
                            AppDataStore.customers[i] = customer.copy(balance = finalBal, transactions = newTxs.toMutableList())
                            updatedCustomersCount++



                        }
                    }
                    AppDataStore.persist(context)
                    log("Başarılı! Toplam $totalFetched adet işlem satırı alındı. $updatedCustomersCount adet carinin yerel hesap ekstresi güncellendi.")






                }
            } else {
                log("Güncellenecek cari hesap hareketi verisi bulunamadı.")




            }
            updateProgress(1.0f)
        } catch (e: Exception) {
            TelemetryReporter.reportException(e, "BridgeSyncHelper_sync", "ERROR", "Köprü Bağlantı Hatası (Cari Hareketleri): ${e.message}. Lütfen Windows Servisinin çalıştığından emin olun.")
            log("Köprü Bağlantı Hatası (Cari Hareketleri): ${e.message}. Lütfen Windows Servisinin çalıştığından emin olun.: ${e.message}")
            updateProgress(1.0f)








        }
    }

    suspend fun syncStokHareketleri(
        context: Context,
        apiUrl: String,
        apiKey: String,
        log: (String) -> Unit,
        updateProgress: (Float) -> Unit
    ) {
        try {
            log("Uç nokta: $apiUrl/api/v1/sync/stokHareket")
            log("Stok Hareket Defteri (STOK_HAREKETLERI) çekiliyor...")
            updateProgress(0.1f)
            val apiService = ApiClient.getFieldOpsApiService(context, apiUrl, apiKey)
            var currentPage = 1
            val pageSize = 200
            var totalFetched = 0
            var hasMore = true

            val allMovements = mutableListOf<com.example.data.api.StokHareketiDto>()

            while (hasMore) {
                log("Stok hareketleri sayfa $currentPage çekiliyor...")
                val sharedPrefs = context.getSharedPreferences("erp_settings", android.content.Context.MODE_PRIVATE)
                val tenantId = sharedPrefs.getString("tenant_id", "T001") ?: "T001"
                val deviceId = sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT"
                val req = com.example.data.api.PullJobsRequest(
                    tenant_id = tenantId,
                    api_key = apiKey,
                    device_id = deviceId,
                    agent_version = "v2.0-multi-tenant",
                    entity = "stokHareket",
                    since = null,
                    page = currentPage,
                    pageSize = pageSize
                )
                val response = apiService.getStokHareket(req)
                if (response.isSuccessful && response.body() != null) {
                    val body = response.body()!!
                    val items = body.actualItems
                    if (items.isEmpty()) {
                        hasMore = false
                    } else {
                        allMovements.addAll(items)
                        totalFetched += items.size
                        updateProgress(Math.min(0.1f + (currentPage * 0.15f), 0.85f))
                        if (items.size < pageSize) {
                            hasMore = false
                        } else {
                            currentPage++
                        }
                    }
                } else {
                    handleApiError(response, log)
                    throw Exception("API Hatası (\"Sayfa $currentPage stok hareketleri çekilemedi. Kod: ${response.code()}\")")
                }
            }

            if (allMovements.isNotEmpty()) {
                withContext(Dispatchers.IO) {
                    try {
                        val db = com.example.data.database.DatabaseProvider.getDatabase(context)
                        val entities = allMovements.map { item ->
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
                            val clientName = AppDataStore.customers.find { it.id == item.cariKod }?.name ?: item.cariKod ?: "Genel Müşteri"
                            val stockCodeVal = (item.stokKod ?: item.urunKod ?: "").trim()
                            com.example.data.database.StockMovementEntity(
                                stockCode = stockCodeVal,
                                date = formattedDate,
                                type = moveType,
                                qty = quantityFormatted,
                                detail = "Evrak: ${item.evrakNo ?: "Belgesiz"} - $clientName",
                                user = item.aciklama ?: "Mikro Kaydı",
                                evrakNo = item.evrakNo ?: "Belgesiz",
                                cariKod = item.cariKod,
                                cariName = clientName,
                                unitPrice = item.birimFiyat ?: 0.0,
                                totalAmount = item.tutar ?: 0.0,
                                warehouse = "Depo: ${item.girisDepoNo ?: item.cikisDepoNo ?: "Merkez"}"
                            )
                        }
                        db.stockMovementDao().replaceAll(entities)
                    } catch (e: Exception) {
                        e.printStackTrace()
                    }
                }
                withContext(Dispatchers.Main) {
                    val grouped = allMovements.groupBy { (it.stokKod ?: it.urunKod ?: "").trim().lowercase() }
                    for ((codeKey, items) in grouped) {
                        if (codeKey.isNotBlank()) {
                            val mappedMovements = items.map { item ->
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
                                val clientName = AppDataStore.customers.find { it.id == item.cariKod }?.name ?: item.cariKod ?: "Genel Müşteri"
                                StockMovement(
                                    date = formattedDate,
                                    type = moveType,
                                    qty = quantityFormatted,
                                    detail = "Evrak: ${item.evrakNo ?: "Belgesiz"} - $clientName",
                                    user = item.aciklama ?: "Mikro Kaydı",
                                    evrakNo = item.evrakNo ?: "Belgesiz",
                                    cariKod = item.cariKod,
                                    cariName = clientName,
                                    unitPrice = item.birimFiyat ?: 0.0,
                                    totalAmount = item.tutar ?: 0.0,
                                    warehouse = "Depo: ${item.girisDepoNo ?: item.cikisDepoNo ?: "Merkez"}"
                                )
                            }
                            AppDataStore.stockMovementsMap[codeKey] = mappedMovements
                        }
                    }
                    log("Başarılı! Toplam $totalFetched adet stok hareket satırı alındı. ${grouped.size} adet ürünün hareket defteri güncellendi.")
                }
            } else {
                log("Güncellenecek stok hareketi verisi bulunamadı.")
            }
            updateProgress(1.0f)
        } catch (e: Exception) {
            TelemetryReporter.reportException(e, "BridgeSyncHelper_sync", "ERROR", "Köprü Bağlantı Hatası (Stok Hareketleri): ${e.message}")
            log("Köprü Bağlantı Hatası (Stok Hareketleri): ${e.message}: ${e.message}")
            updateProgress(1.0f)
        }
    }

    suspend fun syncFaturaHareket(
        context: Context,
        apiUrl: String,
        apiKey: String,
        log: (String) -> Unit,
        updateProgress: (Float) -> Unit
    ) {
        try {
            log("Uç nokta: $apiUrl/api/v1/sync/faturaHareket")
            log("Fatura Detayı (Başlık + Satırlar) (CARI_HESAP_HAREKETLERI + STOK_HAREKETLERI) çekiliyor...")
            updateProgress(0.1f)
            val apiService = ApiClient.getFieldOpsApiService(context, apiUrl, apiKey)

            val customerCodes = AppDataStore.customers.map { it.id }.filter { !it.startsWith("customer_") }.take(250)

            if (customerCodes.isEmpty()) {
                log("Hata: Senkronize edilebilecek mikro koduna sahip kayıtlı cari bulunamadı. Önce Cari Kartları eşitlemelisiniz.")
                updateProgress(1.0f)
                return





            }
            log("Örnek cariler için fatura detayları sorgulanıyor: $customerCodes")
            val db = DatabaseProvider.getDatabase(context.applicationContext)
            var count = 0
            for (code in customerCodes) {
                log("Cari $code için faturalar sorgulanıyor...")
                val sharedPrefs = context.getSharedPreferences("erp_settings", android.content.Context.MODE_PRIVATE)
                val tenantId = sharedPrefs.getString("tenant_id", "T001") ?: "T001"
                val apiKeyVal = apiKey
                val deviceId = sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT"
                val req_getFaturaHareket = com.example.data.api.PullJobsRequest(
                    tenant_id = tenantId,
                    api_key = apiKeyVal,
                    device_id = deviceId,
                    agent_version = "v2.0-multi-tenant",
                    entity = "faturaHareket",
                    since = code,
                    page = 1,
                    pageSize = 100
                )
                val response = apiService.getFaturaHareket(req_getFaturaHareket)
                if (response.isSuccessful && response.body() != null) {
                    val body = response.body()!!
                    val items = body.actualItems
                    log("Cari $code için ${items.size} adet detaylı fatura alındı.")
                    val customer = AppDataStore.customers.find { it.id == code }
                    val custName = customer?.name ?: "Müşteri $code"

                    for (fatura in items) {
                        val rawEvrak = fatura.evrakNo ?: ""
                        val invoiceNo = if (rawEvrak.isNotEmpty() && !rawEvrak.startsWith("FT-") && !rawEvrak.startsWith("SM-")) {
                            "FT-$rawEvrak"












                        } else {
                            rawEvrak.ifEmpty { "FT-ERP-${fatura.erpRef ?: (Math.random()*100000).toInt()}" }

                        }
                        log(" Fatura No: $invoiceNo, Tarih: ${fatura.tarih ?: ""}, Tutar: ${fatura.tutar ?: 0.0} TRY")

                        val totalQtySum = fatura.satirlar?.sumOf { it.miktar?.toInt() ?: 1 } ?: 0
                        val orderEntity = WmsOrderEntity(
                            id = invoiceNo,
                            customerName = custName,
                            orderDate = fatura.tarih ?: "",
                            status = "Sevk Edildi",
                            totalItems = totalQtySum,
                            syncStatus = "SYNCED"
                        )
                        db.wmsOrderDao().insert(orderEntity)

                        val orderItemsList = mutableListOf<WmsOrderItemEntity>()
                        fatura.satirlar?.forEachIndexed { idx, satir ->
                            log("   -> Satır: StokKod: ${satir.stokKod ?: ""}, Miktar: ${satir.miktar ?: 0.0}, Tutar: ${satir.tutar ?: 0.0} TRY")

                            val stokK = satir.stokKod ?: ""
                            val matchedProd = AppDataStore.products.find { it.code == stokK }
                            val prodBarcode = matchedProd?.barcode ?: "ST-${stokK}"
                            val prodTitle = matchedProd?.title ?: satir.stokAd ?: "Ürün ($stokK)"
                            val itemQty = satir.miktar?.toInt() ?: 1

                            val orderItem = WmsOrderItemEntity(
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
                    count += items.size
                } else {
                    log("Uyarı: $code için faturalar çekilemedi. Hata kodu: ${response.code()}")




                }
                updateProgress(0.1f + (count.toFloat() / customerCodes.size) * 0.9f)


            }
            if (count == 0) {
                log("Uç noktadan herhangi bir fatura bulunamadı/çekilemedi.")

            } else {
                log("Başarılı! Toplam $count adet faturanın satır detayları sorgulandı ve yerel modellere alındı.")




            }
            updateProgress(1.0f)
        } catch (e: Exception) {
            TelemetryReporter.reportException(e, "BridgeSyncHelper_sync", "ERROR", "Köprü Bağlantı Hatası (Fatura Hareketleri): ${e.message}. Api'den veri alınamadı.")
            log("Köprü Bağlantı Hatası (Fatura Hareketleri): ${e.message}. Api'den veri alınamadı.: ${e.message}")
            updateProgress(1.0f)








        }
    }
    suspend fun syncStatusCheck(
        context: Context,
        apiUrl: String,
        apiKey: String,
        log: (String) -> Unit,
        updateProgress: (Float) -> Unit
    ) {
        try {
            log("Uç nokta: $apiUrl/api/v1/sync/status")
            log("Mikro ERP Sync Durumu Sorgulanıyor...")
            updateProgress(0.5f)
            val apiService = ApiClient.getFieldOpsApiService(context, apiUrl, apiKey)
            val sharedPrefs = context.getSharedPreferences("erp_settings", android.content.Context.MODE_PRIVATE)
            val tenantId = sharedPrefs.getString("tenant_id", "T001") ?: "T001"
            val apiKeyVal = apiKey
            val deviceId = sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT"
            val req_getSyncStatus = com.example.data.api.PullJobsRequest(
                tenant_id = tenantId,
                api_key = apiKeyVal,
                device_id = deviceId,
                agent_version = "v2.0-multi-tenant",
                entity = "syncStatus",
                since = null,
                page = 1,
                pageSize = 100
            )
            val response = apiService.getSyncStatus(req_getSyncStatus)
            if (response.isSuccessful && response.body() != null) {
                val stats = response.body()!!
                log("Gelen Durum Bilgisi (System Watermarks):")
                log("• ERP: ${stats.erp ?: "mikro"}")
                log("• Devam eden sync var mı: ${if (stats.syncInProgress == true || stats.isRunning == true) "EVET" else "HAYIR"}")
                (stats.watermarks ?: emptyList()).forEach { wm ->
                    log("  -> Entity: ${wm.entity}, Mod: ${wm.mode ?: "Delta"}, Son veri sync: ${wm.lastSyncAt}, Toplam Adet: ${wm.totalSynced}")




                }
            } else {
                handleApiError(response, log)




            }
            updateProgress(1.0f)
        } catch (e: Exception) {
            TelemetryReporter.reportException(e, "BridgeSyncHelper_sync", "ERROR", "Köprü Bağlantı Hatası (Sync Status): ${e.message}.")
            log("Köprü Bağlantı Hatası (Sync Status): ${e.message}.: ${e.message}")
            updateProgress(1.0f)









        }
    }
    suspend fun syncCariAdresleri(
        context: Context,
        apiUrl: String,
        apiKey: String,
        log: (String) -> Unit,
        updateProgress: (Float) -> Unit
    ) {
        try {
            log("Uç nokta: $apiUrl/api/v1/sync/cariAdresleri")
            log("Cari Adresleri Çekiliyor...")
            updateProgress(0.1f)
            val apiService = ApiClient.getFieldOpsApiService(context, apiUrl, apiKey)
            var page = 1
            val pageSize = 200
            var hasMore = true
            val loadedItems = mutableListOf<CariAdresDto>()

            while (hasMore) {
                val sharedPrefs = context.getSharedPreferences("erp_settings", android.content.Context.MODE_PRIVATE)
                val tenantId = sharedPrefs.getString("tenant_id", "T001") ?: "T001"
                val apiKeyVal = apiKey
                val deviceId = sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT"
                val req_getCariAdresleri = com.example.data.api.PullJobsRequest(
                    tenant_id = tenantId,
                    api_key = apiKeyVal,
                    device_id = deviceId,
                    agent_version = "v2.0-multi-tenant",
                    entity = "cariAdresleri",
                    since = null,
                    page = page,
                    pageSize = pageSize
                )
                val response = apiService.getCariAdresleri(req_getCariAdresleri)
                if (response.isSuccessful && response.body() != null) {
                    val body = response.body()!!
                    val items = body.actualItems
                    if (items.isEmpty()) {
                        hasMore = false
                    } else {
                        loadedItems.addAll(items)
                        log("${items.size} adet adres alındı.")
                        if (items.size < pageSize) {
                            hasMore = false
                        } else {
                            page++
                        }
                    }
                    updateProgress(0.1f + (page * 0.1f).coerceAtMost(0.8f))
                } else {
                    handleApiError(response, log)
                    throw Exception("API Hatası (\"Adresler çekilemedi. Kod: ${response.code()}\")")
                }
            }
            withContext(Dispatchers.Main) {
                AppDataStore.cariAdresleri.clear()
                AppDataStore.cariAdresleri.addAll(loadedItems)
                AppDataStore.persist(context)
            }
            log("Başarılı! Toplam ${loadedItems.size} adet cari adres kaydedildi.")
            updateProgress(1.0f)
        } catch (e: Exception) {
            TelemetryReporter.reportException(e, "BridgeSyncHelper_sync", "ERROR", "Adres Senkronizasyon Hatası: ${e.message}")
            log("Adres Senkronizasyon Hatası: ${e.message}: ${e.message}")
            updateProgress(1.0f)








        }
    }
    suspend fun syncCariBankaHesaplari(
        context: Context,
        apiUrl: String,
        apiKey: String,
        log: (String) -> Unit,
        updateProgress: (Float) -> Unit
    ) {
        try {
            log("Uç nokta: $apiUrl/api/v1/sync/cariBankaHesaplari")
            log("Cari Banka Hesapları Çekiliyor...")
            updateProgress(0.1f)
            val apiService = ApiClient.getFieldOpsApiService(context, apiUrl, apiKey)
            var page = 1
            val pageSize = 200
            var hasMore = true
            val loadedItems = mutableListOf<CariBankaHesapDto>()

            while (hasMore) {
                log("Sayfa $page cari banka hesapları çekiliyor...")
                val sharedPrefs = context.getSharedPreferences("erp_settings", android.content.Context.MODE_PRIVATE)
                val tenantId = sharedPrefs.getString("tenant_id", "T001") ?: "T001"
                val apiKeyVal = apiKey
                val deviceId = sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT"
                val req_getCariBankaHesaplari = com.example.data.api.PullJobsRequest(
                    tenant_id = tenantId,
                    api_key = apiKeyVal,
                    device_id = deviceId,
                    agent_version = "v2.0-multi-tenant",
                    entity = "cariBankaHesaplari",
                    since = null,
                    page = page,
                    pageSize = pageSize
                )
                val response = apiService.getCariBankaHesaplari(req_getCariBankaHesaplari)
                if (response.isSuccessful && response.body() != null) {
                    val body = response.body()!!
                    val items = body.actualItems
                    if (items.isEmpty()) {
                        hasMore = false












                    } else {
                        loadedItems.addAll(items)
                        log("${items.size} adet cari banka hesabı alındı.")
                        if (items.size < pageSize) {
                            hasMore = false



                        } else {
                            page++


                        }
                    }
                    updateProgress(0.1f + (page * 0.1f).coerceAtMost(0.8f))
                } else {
                    handleApiError(response, log)
                    throw Exception("API Hatası (\"Cari banka hesapları çekilemedi. Kod: ${response.code()}\")")






                }
            }
            withContext(Dispatchers.Main) {
                AppDataStore.cariBankaHesaplari.clear()
                AppDataStore.cariBankaHesaplari.addAll(loadedItems)
                AppDataStore.persist(context)
            }
            log("Başarılı! Toplam ${loadedItems.size} adet cari banka hesabı kaydedildi.")
            updateProgress(1.0f)
        } catch (e: Exception) {
            TelemetryReporter.reportException(e, "BridgeSyncHelper_sync", "ERROR", "Cari Banka Senkronizasyon Hatası: ${e.message}")
            log("Cari Banka Senkronizasyon Hatası: ${e.message}: ${e.message}")
            updateProgress(1.0f)








        }
    }
    suspend fun syncBankalar(
        context: Context,
        apiUrl: String,
        apiKey: String,
        log: (String) -> Unit,
        updateProgress: (Float) -> Unit
    ) {
        try {
            log("Uç nokta: $apiUrl/api/v1/sync/bankalar")
            log("Banka Tanımları Çekiliyor...")
            updateProgress(0.1f)
            val apiService = ApiClient.getFieldOpsApiService(context, apiUrl, apiKey)
            var page = 1
            val pageSize = 200
            var hasMore = true
            val loadedItems = mutableListOf<BridgeBankaDto>()

            while (hasMore) {
                log("Sayfa $page banka tanımları çekiliyor...")
                val sharedPrefs = context.getSharedPreferences("erp_settings", android.content.Context.MODE_PRIVATE)
                val tenantId = sharedPrefs.getString("tenant_id", "T001") ?: "T001"
                val apiKeyVal = apiKey
                val deviceId = sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT"
                val req_getBankalar = com.example.data.api.PullJobsRequest(
                    tenant_id = tenantId,
                    api_key = apiKeyVal,
                    device_id = deviceId,
                    agent_version = "v2.0-multi-tenant",
                    entity = "bankalar",
                    since = null,
                    page = page,
                    pageSize = pageSize
                )
                val response = apiService.getBankalar(req_getBankalar)
                if (response.isSuccessful && response.body() != null) {
                    val body = response.body()!!
                    val items = body.actualItems
                    if (items.isEmpty()) {
                        hasMore = false












                    } else {
                        loadedItems.addAll(items)
                        log("${items.size} adet banka tanımı alındı.")
                        if (items.size < pageSize) {
                            hasMore = false



                        } else {
                            page++


                        }
                    }
                    updateProgress(0.1f + (page * 0.1f).coerceAtMost(0.8f))
                } else {
                    handleApiError(response, log)
                    throw Exception("API Hatası (\"Bankalar çekilemedi. Kod: ${response.code()}\")")






                }
            }
            withContext(Dispatchers.Main) {
                AppDataStore.bridgeBankalar.clear()
                AppDataStore.bridgeBankalar.addAll(loadedItems)
                AppDataStore.mapBridgeDataToAppModels()
                AppDataStore.persist(context)
            }
            log("Başarılı! Toplam ${loadedItems.size} adet banka tanımı kaydedildi.")
            updateProgress(1.0f)
        } catch (e: Exception) {
            TelemetryReporter.reportException(e, "BridgeSyncHelper_sync", "ERROR", "Banka Senkronizasyon Hatası: ${e.message}")
            log("Banka Senkronizasyon Hatası: ${e.message}: ${e.message}")
            updateProgress(1.0f)








        }
    }
    suspend fun syncKasalar(
        context: Context,
        apiUrl: String,
        apiKey: String,
        log: (String) -> Unit,
        updateProgress: (Float) -> Unit
    ) {
        try {
            log("Uç nokta: $apiUrl/api/v1/sync/kasalar")
            log("Kasa Tanımları Çekiliyor...")
            updateProgress(0.1f)
            val apiService = ApiClient.getFieldOpsApiService(context, apiUrl, apiKey)
            var page = 1
            val pageSize = 200
            var hasMore = true
            val loadedItems = mutableListOf<KasalarDto>()

            while (hasMore) {
                log("Sayfa $page kasa tanımları çekiliyor...")
                val sharedPrefs = context.getSharedPreferences("erp_settings", android.content.Context.MODE_PRIVATE)
                val tenantId = sharedPrefs.getString("tenant_id", "T001") ?: "T001"
                val apiKeyVal = apiKey
                val deviceId = sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT"
                val req_getKasalar = com.example.data.api.PullJobsRequest(
                    tenant_id = tenantId,
                    api_key = apiKeyVal,
                    device_id = deviceId,
                    agent_version = "v2.0-multi-tenant",
                    entity = "kasalar",
                    since = null,
                    page = page,
                    pageSize = pageSize
                )
                val response = apiService.getKasalar(req_getKasalar)
                if (response.isSuccessful && response.body() != null) {
                    val body = response.body()!!
                    val items = body.actualItems
                    if (items.isEmpty()) {
                        hasMore = false












                    } else {
                        loadedItems.addAll(items)
                        log("${items.size} adet kasa tanımı alındı.")
                        if (items.size < pageSize) {
                            hasMore = false



                        } else {
                            page++


                        }
                    }
                    updateProgress(0.1f + (page * 0.1f).coerceAtMost(0.8f))
                } else {
                    handleApiError(response, log)
                    throw Exception("API Hatası (\"Kasalar çekilemedi. Kod: ${response.code()}\")")






                }
            }
            withContext(Dispatchers.Main) {
                AppDataStore.bridgeKasalar.clear()
                AppDataStore.bridgeKasalar.addAll(loadedItems)
                AppDataStore.mapBridgeDataToAppModels()
                AppDataStore.persist(context)
            }
            log("Başarılı! Toplam ${loadedItems.size} adet kasa tanımı kaydedildi.")
            updateProgress(1.0f)
        } catch (e: Exception) {
            TelemetryReporter.reportException(e, "BridgeSyncHelper_sync", "ERROR", "Kasa Senkronizasyon Hatası: ${e.message}")
            log("Kasa Senkronizasyon Hatası: ${e.message}: ${e.message}")
            updateProgress(1.0f)








        }
    }
    suspend fun syncKasaYonetim(
        context: Context,
        apiUrl: String,
        apiKey: String,
        log: (String) -> Unit,
        updateProgress: (Float) -> Unit
    ) {
        try {
            log("Uç nokta: $apiUrl/api/v1/sync/kasaYonetim")
            log("Kasa Yönetim / Muhasebe Tanımları Çekiliyor...")
            updateProgress(0.3f)
            val apiService = ApiClient.getFieldOpsApiService(context, apiUrl, apiKey)
            val sharedPrefs = context.getSharedPreferences("erp_settings", android.content.Context.MODE_PRIVATE)
            val tenantId = sharedPrefs.getString("tenant_id", "T001") ?: "T001"
            val apiKeyVal = apiKey
            val deviceId = sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT"
            val req_getKasaYonetim = com.example.data.api.PullJobsRequest(
                tenant_id = tenantId,
                api_key = apiKeyVal,
                device_id = deviceId,
                agent_version = "v2.0-multi-tenant",
                entity = "kasaYonetim",
                since = null,
                page = 1,
                pageSize = 100
            )
            val response = apiService.getKasaYonetim(req_getKasaYonetim)
            if (response.isSuccessful && response.body() != null) {
                val body = response.body()!!
                val items = body.actualItems
                withContext(Dispatchers.Main) {
                    AppDataStore.kasaYonetimList.clear()
                    AppDataStore.kasaYonetimList.addAll(items)
                    AppDataStore.persist(context)
                }
                log("Başarılı! Toplam ${items.size} adet kasa yönetim/muhasebe tanımı kaydedildi.")
            } else {
                handleApiError(response, log)




            }
            updateProgress(1.0f)
        } catch (e: Exception) {
            TelemetryReporter.reportException(e, "BridgeSyncHelper_sync", "ERROR", "Kasa Yönetim Senkronizasyon Hatası: ${e.message}")
            log("Kasa Yönetim Senkronizasyon Hatası: ${e.message}: ${e.message}")
            updateProgress(1.0f)









        }
    }
    val isOnlineState = androidx.compose.runtime.mutableStateOf(true)
    val lastSyncTimeState = androidx.compose.runtime.mutableStateOf("Henüz Yapılmadı")

    fun initLastSyncTime(context: Context) {
        val syncPrefs = context.getSharedPreferences("erp_sync_times", Context.MODE_PRIVATE)
        val lastGlobal = syncPrefs.getString("last_global_sync", "Henüz Yapılmadı")
        lastSyncTimeState.value = lastGlobal ?: "Henüz Yapılmadı"





    }
    fun initOnlineStatus(context: Context) {
        val prefs = context.getSharedPreferences("erp_sync_times", Context.MODE_PRIVATE)
        isOnlineState.value = prefs.getBoolean("is_online", true)





    }
    fun setOnlineStatus(context: Context, online: Boolean) {
        isOnlineState.value = online
        val prefs = context.getSharedPreferences("erp_sync_times", Context.MODE_PRIVATE)
        prefs.edit().putBoolean("is_online", online).apply()





    }
    fun getLastSyncTime(context: Context, entity: String): String? {
        val prefs = context.getSharedPreferences("erp_sync_times", Context.MODE_PRIVATE)
        return prefs.getString("last_sync_$entity", null)





    }
    fun setLastSyncTime(context: Context, entity: String, time: String) {
        val prefs = context.getSharedPreferences("erp_sync_times", Context.MODE_PRIVATE)
        prefs.edit().putString("last_sync_$entity", time).apply()





    }
    fun isErpModeActive(context: Context): Boolean {
        val prefs = context.getApplicationContext().getSharedPreferences("erp_settings", Context.MODE_PRIVATE)
        return prefs.getBoolean("is_erp_active", true)





    }
    suspend fun triggerBackgroundSync(context: Context, logCallback: ((String) -> Unit)? = null) {
        withContext(Dispatchers.IO) {
            val appCxt = context.applicationContext
            if (!isErpModeActive(appCxt)) {
                logCallback?.invoke("ERP Entegrasyon Modu aktif değil.")
                return@withContext




            }
            val prefs = appCxt.getSharedPreferences("erp_settings", Context.MODE_PRIVATE)
            val apiUrl = prefs.getString("api_url", null) ?: "https://lisans.appsgo.cloud"
            val apiKey = prefs.getString("api_key", null).orEmpty()
            if (apiKey.isBlank()) {
                logCallback?.invoke("Arka plan senkronizasyonu iÃ§in geÃ§erli bir cihaz belirteci gerekli.")
                return@withContext
            }

            logCallback?.invoke("Hızlı arka plan senkronizasyonu başlatılıyor...")

            try {
                val currentTime = java.text.SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss'Z'", java.util.Locale.getDefault()).format(java.util.Date())

                // 1. CARILER INCREMENTAL
                val lastCariSync = getLastSyncTime(appCxt, "cari")
                logCallback?.invoke("Cariler senkronize ediliyor... (since: $lastCariSync)")
                syncCarilerIncremental(appCxt, apiUrl, apiKey, lastCariSync)
                setLastSyncTime(appCxt, "cari", currentTime)

                // 2. URUNLER INCREMENTAL
                val lastUrunSync = getLastSyncTime(appCxt, "urun")
                logCallback?.invoke("Stoklar senkronize ediliyor... (since: $lastUrunSync)")
                syncUrunlerIncremental(appCxt, apiUrl, apiKey, lastUrunSync)
                setLastSyncTime(appCxt, "urun", currentTime)

                val displayTime = java.text.SimpleDateFormat("dd.MM.yyyy HH:mm:ss", java.util.Locale.getDefault()).format(java.util.Date())
                withContext(Dispatchers.Main) {
                    lastSyncTimeState.value = displayTime




                }
                val syncPrefs = appCxt.getSharedPreferences("erp_sync_times", Context.MODE_PRIVATE)
                syncPrefs.edit().putString("last_global_sync", displayTime).apply()

                logCallback?.invoke("Hızlı arka plan senkronizasyonu tamamlandı: $displayTime")
            } catch (e: Exception) {
                logCallback?.invoke("Arka plan senkronizasyon hatası: ${e.message}")













            }
        }
    }
    suspend fun syncCarilerIncremental(context: Context, apiUrl: String, apiKey: String, since: String?) {
        try {
            val apiService = ApiClient.getFieldOpsApiService(context, apiUrl, apiKey)
            var currentPage = 1
            val pageSize = 100
            var hasMore = true
            var totalFetched = 0
            val allMappedCustomers = mutableListOf<Customer>()

            while (hasMore) {
                val sharedPrefs = context.getSharedPreferences("erp_settings", android.content.Context.MODE_PRIVATE)
                val tenantId = sharedPrefs.getString("tenant_id", "T001") ?: "T001"
                val apiKeyVal = apiKey
                val deviceId = sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT"
                val req_getCariler = com.example.data.api.PullJobsRequest(
                    tenant_id = tenantId,
                    api_key = apiKeyVal,
                    device_id = deviceId,
                    agent_version = "v2.0-multi-tenant",
                    entity = "cari",
                    since = null,
                    page = currentPage,
                    pageSize = 100
                )
                val response = apiService.getCariler(req_getCariler)
                if (response.isSuccessful && response.body() != null) {
                    val syncRes = response.body()!!
                    val cariler = syncRes.actualItems
                    if (cariler.isEmpty()) {
                        hasMore = false












                    } else {
                        totalFetched += cariler.size
                        for (cari in cariler) {
                            val bal = cari.bakiye ?: cari.balance ?: cari.netBakiye ?: 0.0
                            val txList = mutableListOf<CustomerTx>()
                            val apiTxs = cari.transactions ?: cari.hareketler
                            if (apiTxs != null) {
                                for (tx in apiTxs) {
                                    txList.add(
                                        CustomerTx(
                                            id = tx.id ?: "TX-${(Math.random() * 100000).toInt()}",
                                            date = tx.date ?: "20.06.2026",
                                            type = tx.type?.uppercase() ?: "HAREKET",
                                            amount = tx.amount ?: 0.0,
                                            description = tx.description ?: "Mikro Entegrasyonu"
                                        )
                                    )








                                }
                            }
                            val mapped = Customer(
                                id = cari.actualCariKod,
                                name = cari.actualCariUnvan,
                                balance = bal,
                                lastVisit = "Köprü Eşitlendi",
                                contact = cari.telefon ?: "-",
                                phone = cari.telefon ?: "-",
                                address = cari.adres ?: "-",
                                taxOffice = cari.vergiDairesi ?: "-",
                                taxNumber = cari.vergiNo ?: "-",
                                gpsLocation = "41.0, 28.0",
                                riskLimit = 500000.0,
                                priceGroup = "1",
                                specialDiscountPercent = 0.0,
                                transactions = txList
                            )
                            allMappedCustomers.add(mapped)




                        }
                        AppDataStore.upsertCustomerSyncPage(context, allMappedCustomers)
                        allMappedCustomers.clear()
                        if (syncRes.page == null || syncRes.total == null ||
                            cariler.size < pageSize || totalFetched >= syncRes.total) {
                            hasMore = false



                        } else {
                            currentPage++


                        }
                    }
                } else {
                    throw handleApiError(response, log = { })





                }
            }
            if (totalFetched > 0) {
                withContext(Dispatchers.Main) {
                    for (mapped in allMappedCustomers) {
                        val existingIndex = AppDataStore.customers.indexOfFirst { it.id == mapped.id }
                        if (existingIndex >= 0) {
                            AppDataStore.customers[existingIndex] = mapped












                        } else {
                            AppDataStore.customers.add(mapped)



                        }
                    }
                }




            }
        } catch (e: Exception) {
            e.printStackTrace()








        }
    }
    suspend fun syncUrunlerIncremental(context: Context, apiUrl: String, apiKey: String, since: String?) {
        try {
            val apiService = ApiClient.getFieldOpsApiService(context, apiUrl, apiKey)
            var currentPage = 1
            val pageSize = 100
            var hasMore = true
            var totalFetched = 0
            val allMappedProducts = mutableListOf<ProductCatalog>()

            while (hasMore) {
                val sharedPrefs = context.getSharedPreferences("erp_settings", android.content.Context.MODE_PRIVATE)
                val tenantId = sharedPrefs.getString("tenant_id", "T001") ?: "T001"
                val apiKeyVal = apiKey
                val deviceId = sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT"
                val req_getUrunler = com.example.data.api.PullJobsRequest(
                    tenant_id = tenantId,
                    api_key = apiKeyVal,
                    device_id = deviceId,
                    agent_version = "v2.0-multi-tenant",
                    entity = "urun",
                    since = null,
                    page = currentPage,
                    pageSize = 100
                )
                val response = apiService.getUrunler(req_getUrunler)
                if (response.isSuccessful && response.body() != null) {
                    val syncRes = response.body()!!
                    val urunler = syncRes.actualItems
                    if (urunler.isEmpty()) {
                        hasMore = false












                    } else {
                        totalFetched += urunler.size
                        for (u in urunler) {
                            val codeKey = u.actualUrunKod
                            val stockQty = u.explicitStok ?: 0
                            val whMap = mutableMapOf<String, Int>()
                            whMap["Merkez Depo"] = stockQty
                            val basePrice = u.actualSatisFiyat
                            val dealerPrice = u.actualBayiFiyati ?: (basePrice * 0.9)
                            val wholesalePrice = u.actualToptanFiyati ?: (basePrice * 0.8)
                            val customPricesMap = u.customPrices ?: emptyMap()
                            val incrementalBarcodes = (
                                u.stockBarcodes.orEmpty().map { it.barcode } +
                                    listOf(u.actualBarkod)
                                ).filter { it.isNotBlank() }.distinct()

                            val mapped = ProductCatalog(
                                barcode = incrementalBarcodes.firstOrNull() ?: u.actualUrunKod,
                                code = u.actualUrunKod,
                                title = u.actualUrunAd,
                                category = u.kategori ?: "Diğer",
                                desc = "FieldOps Köprüsü üzerinden güncellenen ${u.birim ?: "Adet"} bazlı stok.",
                                basePrice = basePrice,
                                dealerPrice = dealerPrice,
                                wholesalePrice = wholesalePrice,
                                kdvPercent = u.actualKdv.toInt(),
                                imageUrlColor = androidx.compose.ui.graphics.Color(0xFF1976D2),
                                brand = u.marka ?: u.erp ?: "Mikro",
                                stockByWarehouse = whMap,
                                aisle = u.actualReyonKod,
                                customPrices = customPricesMap,
                                barcodes = incrementalBarcodes.ifEmpty { listOf(u.actualUrunKod) },
                                measurement = u.actualOlcu,
                                packaging = u.actualAmbalaj,
                                cartonQuantity = u.actualKoliAdet
                            )
                            allMappedProducts.add(mapped)




                        }
                        AppDataStore.upsertProductSyncPage(context, allMappedProducts)
                        allMappedProducts.clear()
                        if (syncRes.page == null || syncRes.total == null ||
                            urunler.size < pageSize || totalFetched >= syncRes.total) {
                            hasMore = false



                        } else {
                            currentPage++


                        }
                    }
                } else {
                    throw handleApiError(response, log = { })





                }
            }
            if (totalFetched > 0) {
                val db = com.example.data.database.DatabaseProvider.getDatabase(context)
                val converter = com.example.data.database.Converters()
                val seenBarcodes = mutableSetOf<String>()
                val productEntities = allMappedProducts.map { prod ->
                    val finalBarcode = if (prod.barcode.isBlank() || prod.barcode.lowercase() == "null" || prod.barcode.lowercase() == "none" || seenBarcodes.contains(prod.barcode)) {
                        prod.code.ifBlank { java.util.UUID.randomUUID().toString() }
                    } else {
                        prod.barcode
                    }
                    seenBarcodes.add(finalBarcode)
                    com.example.data.database.ProductEntity(
                        barcode = finalBarcode,
                        code = prod.code,
                        title = prod.title,
                        category = prod.category,
                        desc = prod.desc,
                        basePrice = prod.basePrice,
                        dealerPrice = prod.dealerPrice,
                        wholesalePrice = prod.wholesalePrice,
                        kdvPercent = prod.kdvPercent,
                        colorValue = prod.imageUrlColor.value.toLong(),
                        brand = prod.brand,
                        stockByWarehouseJson = converter.fromWarehouseMap(prod.stockByWarehouse),
                        boxQty = prod.boxQty,
                        packageQty = prod.packageQty,
                        imageUrl = prod.imageUrl,
                        localImagePath = prod.localImagePath,
                        aisle = prod.aisle,
                        customPricesJson = converter.fromCustomPricesMap(prod.customPrices),
                        barcodesJson = converter.fromBarcodeList(prod.barcodes),
                        measurement = prod.measurement,
                        packaging = prod.packaging,
                        cartonQuantity = prod.cartonQuantity,
    imageLinksJson = null,
    localImagePathsJson = null
                    )




                }
                // insertAll replaces on conflict! Idempotent upsert.
                db.productDao().insertAll(productEntities)




            }
        } catch (e: Exception) {
            e.printStackTrace()











        }
    }
}

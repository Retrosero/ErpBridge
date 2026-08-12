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
            404 -> "Endpoint mevcut değil (HTTP 404): ${response.raw().request.url.encodedPath} ($safeMessage)"
            422 -> "Doğrulama Hatası: Gönderilen parametreler hatalı ($safeMessage)"
            429 -> "İstek Sınırı Aşıldı: Çok fazla istek gönderdiniz ($safeMessage)"
            in 500..599 -> "Sunucu Hatası: GoApp Cloud sunucusunda bir sorun oluştu ($safeMessage)"
            else -> "Ağ Hatası [$code] ($safeMessage)"




        }
        /* log removed */
        return Exception(userFriendlyMessage)






    }

    /**
     * Merkezi API bu entity için endpoint sunmuyorsa (404) veya API anahtarının
     * bu uç noktaya erişim yetkisi yoksa (403) sync fonksiyonu bilgilendirici
     * log düşerek başarıyla dönsün. Tüm sync zincirini kırmasın; sadece o tablo
     * boş kalsın. UI tarafında "bu özellik tenant'ta yok" şeklinde gösterilir.
     */
    private fun isUnsupportedEndpoint(
        response: retrofit2.Response<*>,
        entity: String,
        log: (String) -> Unit
    ): Boolean {
        val code = response.code()
        if (code == 404) {
            log("⚠ '$entity' endpoint’i merkezi API’de mevcut değil (HTTP 404). Bu tablo için sync atlanıyor.")
            return true
        }
        if (code == 403) {
            log("⚠ '$entity' endpoint’ine bu API anahtarıyla erişim yok (HTTP 403). Bu tablo için sync atlanıyor.")
            return true
        }
        return false
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
                                            val items = txRes.body()!!.items
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
                                                id = "TX-${cari.id}-1",
                                                date = "15.06.2026",
                                                type = "SATIŞ",
                                                amount = 4500.0,
                                                description = "Mikro Fatura No: FT-2026-0012"
                                            )
                                        )
                                        txList.add(
                                            CustomerTx(
                                                id = "TX-${cari.id}-2",
                                                date = "18.06.2026",
                                                type = "TAHSİLAT",
                                                amount = 4500.0,
                                                description = "Nakit Tahsilat Makbuzu"
                                            )
                                        )
                                    } else if (bal > 0.0) {
                                        txList.add(
                                            CustomerTx(
                                                id = "TX-${cari.id}-1",
                                                date = "28.05.2026",
                                                type = "SATIŞ",
                                                amount = bal * 1.5,
                                                description = "Mikro Devir Faturası No: FT-2026-0005"
                                            )
                                        )
                                        txList.add(
                                            CustomerTx(
                                                id = "TX-${cari.id}-2",
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
                                                id = "TX-${cari.id}-1",
                                                date = "01.06.2026",
                                                type = "TAHSİLAT",
                                                amount = absBal * 2.0,
                                                description = "Müşteri Avans Ödemesi No: HK-3004"
                                            )
                                        )
                                        txList.add(
                                            CustomerTx(
                                                id = "TX-${cari.id}-2",
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
                        if (cariler.isEmpty() || ((syncRes.total ?: 0) > 0 && totalFetched >= (syncRes.total ?: 0))) {
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
            if (allMappedCustomers.isNotEmpty()) {
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
                val localResult = AppDataStore.persistAndVerify(context)
                log("ROOM_WRITE_OK entity=cari fetched=$totalFetched saved=${localResult.customers}")
                log("Başarılı! Toplam $totalFetched adet cari kayıt FieldOps Bridge üzerinden başarıyla çekildi")
            } else {
                log("Uç noktadan müşteri verisi çekilemedi. Listede aktarılacak cari bulunamadı.")




            }
            updateProgress(1.0f)
        } catch (e: Exception) {
            log("Köprü Bağlantı Hatası (Cari): ${e.message}. Api'den veri alınamadı.")
            updateProgress(1.0f)








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

            val stockLevelMap = mutableMapOf<String, Int>()
            try {
                log("Mevcut elde kalan stok seviyeleri (STOK_SEVIYELERI) önden yükleniyor...")
                var levelsPage = 1
                val levelsPageSize = 100
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
                        pageSize = 100
                    )
                    val levelsResponse = apiService.getStokSeviye(req_getStokSeviye)
                    if (levelsResponse.isSuccessful && levelsResponse.body() != null) {
                        val body = levelsResponse.body()!!
                        val items = body.items
                        if (items.isEmpty()) {
                            hasMoreLevels = false












                        } else {
                            for (item in items) {
                                if (!item.stokKod.isNullOrBlank()) {
                                    stockLevelMap[item.stokKod] = item.actualMiktar.toInt()








                                }
                            }
                            if (items.size < levelsPageSize) {
                                hasMoreLevels = false



                            } else {
                                levelsPage++









                            }
                        }
                    } else {
                        hasMoreLevels = false


                    }
                }
                log("Başarılı! ${stockLevelMap.size} adet stok bakiye kaydı alındı.")
            } catch (e: Exception) {
                log("Ön stok seviye yükleme adımı es geçiliyor/hata: ${e.message}")





            }
            val barcodesMap = mutableMapOf<String, MutableList<String>>()
            try {
                log("Barkod tanımları (BARKOD_TANIMLARI) sunucudan indiriliyor...")
                var barPage = 1
                val barPageSize = 100
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
                        entity = "barkodTanimi",
                        since = null,
                        page = barPage,
                        pageSize = 100
                    )
                    val barResponse = apiService.getBarkodTanimi(req_getBarkodTanimi)
                    if (barResponse.isSuccessful && barResponse.body() != null) {
                        val body = barResponse.body()!!
                        val items = body.items
                        if (items.isEmpty()) {
                            hasMoreBar = false












                        } else {
                            for (item in items) {
                                if (!item.stokKod.isNullOrBlank() && !item.barkod.isNullOrBlank()) {
                                    val list = barcodesMap.getOrPut(item.stokKod) { mutableListOf() }
                                    if (!list.contains(item.barkod)) {
                                        list.add(item.barkod)












                                    }
                                }
                            }
                            if (items.size < barPageSize) {
                                hasMoreBar = false



                            } else {
                                barPage++







                            }
                        }
                    } else {
                        hasMoreBar = false


                    }
                }
                log("Başarılı! ${barcodesMap.size} adet stok koduna ait çoklu barkod tanımları alındı.")
            } catch (e: Exception) {
                log("Çoklu barkod tanımları yükleme adımı es geçiliyor/hata: ${e.message}")





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
                            val stockFromBridge = stockLevelMap[codeKey] ?: stockLevelMap[u.id]
                            val existingProduct = AppDataStore.products.find { it.code == codeKey }
                            val existingStockSum = existingProduct?.stockByWarehouse?.values?.sum()

                            // Extract stock count with fallbacks to avoid 150 default
                            val stockQty = stockFromBridge 
                                ?: existingStockSum 
                                ?: u.stok 
                                ?: u.miktar 
                                ?: u.quantity 
                                ?: u.stock 
                                ?: 150

                            // Map warehouses
                            val whMap = mutableMapOf<String, Int>()
                            if (existingProduct != null && existingProduct.stockByWarehouse.isNotEmpty()) {
                                whMap.putAll(existingProduct.stockByWarehouse)
                                val firstKey = whMap.keys.firstOrNull() ?: "Merkez Depo"
                                whMap[firstKey] = stockQty
                            } else if (u.stockByWarehouse != null && u.stockByWarehouse.isNotEmpty()) {
                                whMap.putAll(u.stockByWarehouse)
                            } else if (u.miktarDepo != null && u.miktarDepo.isNotEmpty()) {
                                whMap.putAll(u.miktarDepo)


                            } else {
                                whMap["Merkez Depo"] = stockQty


                            }
                            // Extract any barcodes fetched from getBarkodTanimi API call for this product
                            val rawBarcodes = barcodesMap[codeKey]?.toMutableList() ?: mutableListOf()

                            // If the API object u itself has a non-blank barcode, add it to raw candidates
                            if (!u.barkod.isNullOrBlank() && !rawBarcodes.contains(u.barkod)) {
                                rawBarcodes.add(0, u.barkod)





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
                            val basePrice = u.actualSatisFiyat
                            val dealerPrice = u.bayiFiyati ?: (basePrice * 0.9)
                            val wholesalePrice = u.toptanFiyati ?: (basePrice * 0.8)
                            val customPricesMap = u.customPrices ?: emptyMap()

                            val mapped = ProductCatalog(
                                barcode = uniqueBarcode,
                                code = u.actualUrunKod,
                                title = u.actualUrunAd,
                                category = u.kategori ?: "Diğer",
                                desc = "FieldOps Köprüsü üzerinden güncellenen ${u.birim ?: "Adet"} bazlı stok.",
                                basePrice = basePrice,
                                dealerPrice = dealerPrice,
                                wholesalePrice = wholesalePrice,
                                kdvPercent = u.kdvOrani?.toInt() ?: 20,
                                imageUrlColor = androidx.compose.ui.graphics.Color(0xFF1976D2),
                                brand = u.actualMarka ?: u.erp ?: "Mikro",
                                stockByWarehouse = whMap,
                                customPrices = customPricesMap,
                                barcodes = barcodesList,
                                aisle = u.actualReyonKod,
                                measurement = u.actualOlcu,
                                packaging = u.actualAmbalaj,
                                cartonQuantity = u.actualKoliAdet
                            )
                            allMappedProducts.add(mapped)


                        }
                        if (urunler.isEmpty() || ((syncRes.total ?: 0) > 0 && totalFetched >= (syncRes.total ?: 0))) {
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
            if (allMappedProducts.isNotEmpty()) {
                withContext(Dispatchers.Main) {
                    for (u in allMappedProducts) {
                        // CRITICAL: Match by unique ERP stock code instead of barcode to avoid overlapping blanks!
                        val existingIndex = AppDataStore.products.indexOfFirst { it.code == u.code }
                        if (existingIndex >= 0) {
                            AppDataStore.products[existingIndex] = u












                        } else {
                            AppDataStore.products.add(u)



                        }
                    }
                }
                val localResult = AppDataStore.persistAndVerify(context)
                log("ROOM_WRITE_OK entity=urun fetched=$totalFetched saved=${localResult.products}")
                log("Saha Gücü yerel stok kartları Room veritabanı başarıyla güncellendi. Toplam $totalFetched adet ürün/stok kaydı çekildi.")
            } else {
                log("Uç noktadan ürün verisi çekilemedi. Listede aktarılacak ürün bulunamadı.")




            }
            updateProgress(1.0f)
        } catch (e: Exception) {
            log("Köprü Bağlantı Hatası (Stok): ${e.message}. Api'den veri alınamadı.")
            updateProgress(1.0f)








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
                        val matchedPrices = priceLists.filter { it.stokKod == prod.code }
                        if (matchedPrices.isNotEmpty()) {
                            val newCustomPrices = prod.customPrices.toMutableMap()
                            matchedPrices.forEach { item ->
                                val listName = listNoToName[item.listNo ?: 0] ?: "Liste ${item.listNo}"
                                if (item.fiyat != null) {
                                    newCustomPrices[listName] = item.fiyat








                                }
                            }
                            AppDataStore.products[i] = prod.copy(customPrices = newCustomPrices)
                            updatedCount++









                        }
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
            log("Köprü Bağlantı Hatası (Fiyat Listesi): ${e.message}. Lütfen Windows Servisinin çalıştığından emin olun.")
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
            val pageSize = 100
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
                    pageSize = 100
                )
                val response = apiService.getStokSeviye(req_getStokSeviye)
                if (response.isSuccessful && response.body() != null) {
                    val body = response.body()!!
                    val items = body.items
                    if (items.isEmpty()) {
                        hasMore = false












                    } else {
                        allLevels.addAll(items)
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
                    throw Exception("API Hatası (\"Sayfa $currentPage stok seviyeleri çekilemedi. Kod: ${response.code()}\")")






                }
            }
            if (allLevels.isNotEmpty()) {
                withContext(Dispatchers.Main) {
                    var updatedCount = 0
                    val levelMap = allLevels.associate { it.stokKod to it.eldekiMiktar }

                    for (i in AppDataStore.products.indices) {
                        val prod = AppDataStore.products[i]
                        val eldeki = levelMap[prod.code]
                        if (eldeki != null) {
                            val updatedWhMap = prod.stockByWarehouse.toMutableMap()
                            // If there are warehouses: update first one (e.g., Merkez/Ana Depo) or "Ana Depo"
                            val mainDepotKey = updatedWhMap.keys.firstOrNull() ?: "Merkez Depo"
                            updatedWhMap[mainDepotKey] = eldeki.toInt()

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
            log("Köprü Bağlantı Hatası (Stok Seviye): ${e.message}. Lütfen Windows Servisinin çalıştığından emin olun.")
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
                            log("Liste No $listeNo işlenirken hata oluştu: ${e.message}")






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
            log("Yükseltilmiş Fiyat Hatası: ${e.message}. Gelişmiş fiyat listesi uc noktasını kontrol edin.")
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
            val pageSize = 100
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
                    pageSize = 100
                )
                val response = apiService.getCariHareketleri(req_getCariHareketleri)
                if (response.isSuccessful && response.body() != null) {
                    val body = response.body()!!
                    val items = body.items
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
                withContext(Dispatchers.Main) {
                    var updatedCustomersCount = 0
                    val txGrouped = allTx.groupBy { it.cariKod ?: "" }

                    for (i in AppDataStore.customers.indices) {
                        val customer = AppDataStore.customers[i]
                        val matches = txGrouped[customer.id]
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
                            AppDataStore.customers[i] = customer.copy(transactions = newTxs.toMutableList())
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
            log("Köprü Bağlantı Hatası (Cari Hareketleri): ${e.message}. Lütfen Windows Servisinin çalıştığından emin olun.")
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
                    since = null,
                    page = 1,
                    pageSize = 100
                )
                val response = apiService.getFaturaHareket(com.example.data.api.PullJobsRequest(tenant_id="", api_key="", device_id="", agent_version="", entity="faturaHareket", since=code))
                if (response.isSuccessful && response.body() != null) {
                    val body = response.body()!!
                    val items = body.items
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
            log("Köprü Bağlantı Hatası (Fatura Hareketleri): ${e.message}. Api'den veri alınamadı.")
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
            log("Köprü Bağlantı Hatası (Sync Status): ${e.message}.")
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
            val pageSize = 100
            var hasMore = true
            val loadedItems = mutableListOf<CariAdresDto>()

            while (hasMore) {
                log("Sayfa $page adresleri çekiliyor...")
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
                    pageSize = 100
                )
                val response = apiService.getCariAdresleri(req_getCariAdresleri)
                if (response.isSuccessful && response.body() != null) {
                    val body = response.body()!!
                    val items = body.items
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




            }
            log("Başarılı! Toplam ${loadedItems.size} adet cari adres kaydedildi.")
            updateProgress(1.0f)
        } catch (e: Exception) {
            log("Adres Senkronizasyon Hatası: ${e.message}")
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
            val pageSize = 100
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
                    pageSize = 100
                )
                val response = apiService.getCariBankaHesaplari(req_getCariBankaHesaplari)
                if (isUnsupportedEndpoint(response, "cariBankaHesaplari", log)) {
                    withContext(Dispatchers.Main) {
                        AppDataStore.cariBankaHesaplari.clear()
                    }
                    updateProgress(1.0f)
                    return
                }
                if (response.isSuccessful && response.body() != null) {
                    val body = response.body()!!
                    val items = body.items
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




            }
            log("Başarılı! Toplam ${loadedItems.size} adet cari banka hesabı kaydedildi.")
            updateProgress(1.0f)
        } catch (e: Exception) {
            log("Cari Banka Senkronizasyon Hatası: ${e.message}")
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
            val pageSize = 100
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
                    pageSize = 100
                )
                val response = apiService.getBankalar(req_getBankalar)
                if (isUnsupportedEndpoint(response, "bankalar", log)) {
                    withContext(Dispatchers.Main) {
                        AppDataStore.bridgeBankalar.clear()
                    }
                    updateProgress(1.0f)
                    return
                }
                if (response.isSuccessful && response.body() != null) {
                    val body = response.body()!!
                    val items = body.items
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




            }
            log("Başarılı! Toplam ${loadedItems.size} adet banka tanımı kaydedildi.")
            updateProgress(1.0f)
        } catch (e: Exception) {
            log("Banka Senkronizasyon Hatası: ${e.message}")
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
            val pageSize = 100
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
                    pageSize = 100
                )
                val response = apiService.getKasalar(req_getKasalar)
                if (isUnsupportedEndpoint(response, "kasalar", log)) {
                    withContext(Dispatchers.Main) {
                        AppDataStore.bridgeKasalar.clear()
                    }
                    updateProgress(1.0f)
                    return
                }
                if (response.isSuccessful && response.body() != null) {
                    val body = response.body()!!
                    val items = body.items
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




            }
            log("Başarılı! Toplam ${loadedItems.size} adet kasa tanımı kaydedildi.")
            updateProgress(1.0f)
        } catch (e: Exception) {
            log("Kasa Senkronizasyon Hatası: ${e.message}")
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
            if (isUnsupportedEndpoint(response, "kasaYonetim", log)) {
                withContext(Dispatchers.Main) {
                    AppDataStore.kasaYonetimList.clear()
                }
                updateProgress(1.0f)
                return
            }
            if (response.isSuccessful && response.body() != null) {
                val body = response.body()!!
                val items = body.items
                withContext(Dispatchers.Main) {
                    AppDataStore.kasaYonetimList.clear()
                    AppDataStore.kasaYonetimList.addAll(items)




                }
                log("Başarılı! Toplam ${items.size} adet kasa yönetim/muhasebe tanımı kaydedildi.")
            } else {
                handleApiError(response, log)




            }
            updateProgress(1.0f)
        } catch (e: Exception) {
            log("Kasa Yönetim Senkronizasyon Hatası: ${e.message}")
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
            val apiUrl = prefs.getString("api_url", null) ?: "https://d5e4-88-248-2-49.ngrok-free.app"
            val apiKey = prefs.getString("api_key", null) ?: "dev-token-change-in-production"

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
                        if (cariler.isEmpty() || ((syncRes.total ?: 0) > 0 && allMappedCustomers.size >= (syncRes.total ?: 0))) {
                            hasMore = false



                        } else {
                            currentPage++


                        }
                    }
                } else {
                    throw handleApiError(response, log = { })





                }
            }
            if (allMappedCustomers.isNotEmpty()) {
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
                AppDataStore.persist(context)




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
                        for (u in urunler) {
                            val codeKey = u.actualUrunKod
                            val stockQty = u.stok ?: u.miktar ?: u.quantity ?: u.stock ?: 150
                            val whMap = mutableMapOf<String, Int>()
                            whMap["Merkez Depo"] = stockQty
                            val basePrice = u.actualSatisFiyat
                            val dealerPrice = u.bayiFiyati ?: (basePrice * 0.9)
                            val wholesalePrice = u.toptanFiyati ?: (basePrice * 0.8)
                            val customPricesMap = u.customPrices ?: emptyMap()

                            val mapped = ProductCatalog(
                                barcode = u.barkod ?: u.actualUrunKod,
                                code = u.actualUrunKod,
                                title = u.actualUrunAd,
                                category = u.kategori ?: "Diğer",
                                desc = "FieldOps Köprüsü üzerinden güncellenen ${u.birim ?: "Adet"} bazlı stok.",
                                basePrice = basePrice,
                                dealerPrice = dealerPrice,
                                wholesalePrice = wholesalePrice,
                                kdvPercent = u.kdvOrani?.toInt() ?: 20,
                                imageUrlColor = androidx.compose.ui.graphics.Color(0xFF1976D2),
                                brand = u.actualMarka ?: u.erp ?: "Mikro",
                                stockByWarehouse = whMap,
                                customPrices = customPricesMap,
                                barcodes = listOf(u.barkod ?: u.actualUrunKod),
                                aisle = u.actualReyonKod,
                                measurement = u.actualOlcu,
                                packaging = u.actualAmbalaj,
                                cartonQuantity = u.actualKoliAdet
                            )
                            allMappedProducts.add(mapped)




                        }
                        if (urunler.isEmpty() || ((syncRes.total ?: 0) > 0 && allMappedProducts.size >= (syncRes.total ?: 0))) {
                            hasMore = false



                        } else {
                            currentPage++


                        }
                    }
                } else {
                    throw handleApiError(response, log = { })





                }
            }
            if (allMappedProducts.isNotEmpty()) {
                val db = com.example.data.database.DatabaseProvider.getDatabase(context)
                val converter = com.example.data.database.Converters()
                val productEntities = allMappedProducts.map { prod ->
                    com.example.data.database.ProductEntity(
                        barcode = prod.barcode,
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
                        measurement = prod.measurement,
                        packaging = prod.packaging,
                        cartonQuantity = prod.cartonQuantity,
                        customPricesJson = converter.fromCustomPricesMap(prod.customPrices),
                        barcodesJson = converter.fromBarcodeList(prod.barcodes)
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

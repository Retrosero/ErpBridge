import re

with open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "r") as f:
    content = f.read()

pattern = r'            val customerCodes = AppDataStore\.customers\.map \{ it\.id \}\.filter \{ !it\.startsWith\("customer_"\) \}\.take\(250\).*?            if \(count == 0\) \{.*?            \}'

replacement = """
            val sharedPrefs = context.getSharedPreferences("erp_settings", android.content.Context.MODE_PRIVATE)
            val tenantId = sharedPrefs.getString("tenant_id", "T001") ?: "T001"
            val deviceId = sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT"
            val db = DatabaseProvider.getDatabase(context.applicationContext)
            
            var currentPage = sharedPrefs.getInt("faturaHareket_last_page", 1)
            val pageSize = 500
            var totalFetched = 0
            var hasMore = true
            var lastFingerprint = ""
            
            val startTime = System.currentTimeMillis()
            
            while (hasMore) {
                log("Sayfa $currentPage fatura hareketleri çekiliyor...")
                val req = com.example.data.api.PullJobsRequest(
                    tenant_id = tenantId,
                    api_key = apiKey,
                    device_id = deviceId,
                    agent_version = "v2.0-multi-tenant",
                    entity = "faturaHareket",
                    since = null,
                    page = currentPage,
                    pageSize = pageSize
                )
                
                val response = apiService.getFaturaHareket(req)
                if (response.isSuccessful && response.body() != null) {
                    val body = response.body()!!
                    val items = body.items
                    
                    val currentFingerprint = items.joinToString(",") { it.hashCode().toString() }
                    if (items.isEmpty() || (currentFingerprint == lastFingerprint && lastFingerprint.isNotEmpty())) {
                        if (currentFingerprint == lastFingerprint && lastFingerprint.isNotEmpty()) log("Tekrarlayan sayfa algılandı, sayfalama durduruluyor.")
                        hasMore = false
                        sharedPrefs.edit().putInt("faturaHareket_last_page", 1).apply()
                    } else {
                        lastFingerprint = currentFingerprint
                        
                        db.withTransaction {
                            val orderEntities = mutableListOf<WmsOrderEntity>()
                            val orderItemEntities = mutableListOf<WmsOrderItemEntity>()
                            
                            for (fatura in items) {
                                val rawEvrak = fatura.evrakNo ?: ""
                                val invoiceNo = if (rawEvrak.isNotEmpty() && !rawEvrak.startsWith("FT-") && !rawEvrak.startsWith("SM-")) {
                                    "FT-$rawEvrak"
                                } else {
                                    rawEvrak.ifEmpty { "FT-ERP-${fatura.erpRef ?: (Math.random()*100000).toInt()}" }
                                }
                                
                                val custName = AppDataStore.customers.find { it.id == fatura.cariKod }?.name ?: "Müşteri ${fatura.cariKod}"
                                val totalQtySum = fatura.satirlar?.sumOf { it.miktar?.toInt() ?: 1 } ?: 0
                                
                                orderEntities.add(WmsOrderEntity(
                                    id = invoiceNo,
                                    customerName = custName,
                                    orderDate = fatura.tarih ?: "",
                                    status = "Sevk Edildi",
                                    totalItems = totalQtySum,
                                    syncStatus = "SYNCED"
                                ))
                                
                                fatura.satirlar?.forEachIndexed { idx, satir ->
                                    val stokK = satir.stokKod ?: ""
                                    val matchedProd = AppDataStore.products.find { it.code == stokK }
                                    val prodBarcode = matchedProd?.barcode ?: "ST-${stokK}"
                                    val prodTitle = matchedProd?.title ?: satir.stokAd ?: "Ürün ($stokK)"
                                    
                                    orderItemEntities.add(WmsOrderItemEntity(
                                        id = "${invoiceNo}_${stokK}_${idx}",
                                        orderId = invoiceNo,
                                        productBarcode = prodBarcode,
                                        productTitle = prodTitle,
                                        quantityOrdered = satir.miktar?.toInt() ?: 1,
                                        quantityPicked = satir.miktar?.toInt() ?: 1,
                                        isPicked = true,
                                        shelfLocation = "ERP Merkez",
                                        sth_fat_recid_recno = satir.realSthFatRecidRecno
                                    ))
                                }
                            }
                            
                            orderEntities.chunked(500).forEach { db.wmsOrderDao().insertAll(it) }
                            orderItemEntities.chunked(500).forEach { db.wmsOrderItemDao().insertAll(it) }
                        }
                        
                        totalFetched += items.size
                        val totalCount = body.total ?: 0
                        val progressPercent = if (totalCount > 0) (totalFetched.toFloat() / totalCount.toFloat()) else 0.5f
                        updateProgress(progressPercent)
                        
                        val elapsedSecs = (System.currentTimeMillis() - startTime) / 1000f
                        val speed = if (elapsedSecs > 0) (totalFetched / elapsedSecs).toInt() else 0
                        
                        if (totalCount > 0) {
                            com.example.util.SyncManager.updateSyncStats("FaturaHareket: $totalFetched / $totalCount kayıt (%${(progressPercent*100).toInt()}) - Hız: $speed sn/kayıt")
                        } else {
                            com.example.util.SyncManager.updateSyncStats("FaturaHareket: $totalFetched kayıt indirildi (Sayfa $currentPage) - Hız: $speed sn/kayıt")
                        }
                        
                        sharedPrefs.edit().putInt("faturaHareket_last_page", currentPage + 1).apply()
                        
                        if (items.size < pageSize) {
                            hasMore = false
                            sharedPrefs.edit().putInt("faturaHareket_last_page", 1).apply()
                        } else {
                            currentPage++
                        }
                    }
                } else {
                    handleApiError(response, log)
                    throw Exception("API Hatası (Fatura Hareketleri Sayfa $currentPage çekilemedi. Kod: ${response.code()})")
                }
            }
            if (totalFetched == 0) {
                log("Uç noktadan herhangi bir fatura bulunamadı/çekilemedi.")
            } else {
                log("Başarılı! Toplam $totalFetched adet fatura satır detayları sorgulandı ve yerel veritabanına alındı.")
            }
"""

new_content = re.sub(pattern, replacement, content, flags=re.DOTALL)
if new_content == content:
    print("Replace failed")
else:
    with open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "w") as f:
        f.write(new_content)
    print("Replace success")

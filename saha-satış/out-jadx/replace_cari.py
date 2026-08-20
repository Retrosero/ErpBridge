import re

with open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "r") as f:
    content = f.read()

pattern = r'    suspend fun syncCariHareketleri\(.*?\n    \}    suspend fun syncFaturaHareket\('

replacement = """    suspend fun syncCariHareketleri(
        context: Context,
        apiUrl: String,
        apiKey: String,
        log: (String) -> Unit,
        updateProgress: (Float) -> Unit
    ) {
        try {
            log("Uç nokta: $apiUrl/api/v1/android/sync/cariHareketleri")
            log("Cari Hesap Hareketleri toplu sync (CARI_HESAP_HAREKETLERI) çekiliyor...")
            updateProgress(0.1f)
            val apiService = ApiClient.getFieldOpsApiService(context, apiUrl, apiKey)
            
            val sharedPrefs = context.getSharedPreferences("erp_settings", android.content.Context.MODE_PRIVATE)
            val tenantId = sharedPrefs.getString("tenant_id", "T001") ?: "T001"
            val deviceId = sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT"
            val db = DatabaseProvider.getDatabase(context.applicationContext)
            
            var currentPage = sharedPrefs.getInt("cariHareketleri_last_page", 1)
            val pageSize = 500
            var totalFetched = 0
            var hasMore = true
            var lastFingerprint = ""
            
            val startTime = System.currentTimeMillis()

            while (hasMore) {
                log("Cari hareketleri sayfa $currentPage çekiliyor...")
                
                val req_getCariHareketleri = com.example.data.api.PullJobsRequest(
                    tenant_id = tenantId,
                    api_key = apiKey,
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
                    val items = body.items
                    val currentFingerprint = items.joinToString(",") { it.hashCode().toString() }
                    if (items.isEmpty() || (currentFingerprint == lastFingerprint && lastFingerprint.isNotEmpty())) {
                        if (currentFingerprint == lastFingerprint && lastFingerprint.isNotEmpty()) log("Tekrarlayan sayfa algılandı, sayfalama durduruluyor.")
                        hasMore = false
                        sharedPrefs.edit().putInt("cariHareketleri_last_page", 1).apply()
                    } else {
                        lastFingerprint = currentFingerprint
                    
                        val txGrouped = items.groupBy { it.cariKod ?: "" }
                        val affectedCustomers = mutableListOf<com.example.data.database.CustomerEntity>()
                        
                        withContext(Dispatchers.Main) {
                            for (i in AppDataStore.customers.indices) {
                                val customer = AppDataStore.customers[i]
                                val matches = txGrouped[customer.id]
                                if (matches != null && matches.isNotEmpty()) {
                                    val newTxs = matches.map { dto: com.example.data.api.CariHareketiDto ->
                                        val rawDate = dto.tarih ?: ""
                                        val formattedDate = try {
                                            if (rawDate.contains("T")) {
                                                val parts = rawDate.split("T")[0].split("-")
                                                if (parts.size == 3) {
                                                    "${parts[2]}.${parts[1]}.${parts[0]}"
                                                } else rawDate
                                            } else rawDate
                                        } catch (e: Exception) { rawDate }
                                        
                                        val amt = dto.tutar ?: 0.0
                                        val tipVal = dto.tip ?: 0
                                        val isBorc = dto.borcMu ?: (tipVal == 0)
                                        val tType = if (isBorc) "B" else "A"
                                        
                                        com.example.ui.screens.CustomerTx(
                                            id = dto.id ?: dto.evrakNo ?: "TX-ERP-${(Math.random()*100000).toInt()}",
                                            date = formattedDate,
                                            type = tType,
                                            amount = amt,
                                            description = dto.aciklama ?: dto.evrakNo ?: "Hareket",
                                            erpRef = dto.erpRef,
                                            recNo = dto.evrakNo,
                                            cha_recno = dto.cha_recno ?: dto.recno ?: dto.chaRecNo ?: dto.cha_RECno
                                        )
                                    }
                                    val existingTxs = customer.transactions.toMutableList()
                                    for (nt in newTxs) {
                                        val idx = existingTxs.indexOfFirst { it.id == nt.id }
                                        if (idx >= 0) existingTxs[idx] = nt
                                        else existingTxs.add(nt)
                                    }
                                    val updatedCust = customer.copy(transactions = existingTxs)
                                    AppDataStore.customers[i] = updatedCust
                                    
                                    affectedCustomers.add(com.example.data.database.CustomerEntity(
                                        id = updatedCust.id,
                                        name = updatedCust.name,
                                        balance = updatedCust.balance,
                                        lastVisit = updatedCust.lastVisit,
                                        contact = updatedCust.contact,
                                        phone = updatedCust.phone,
                                        address = updatedCust.address,
                                        taxOffice = updatedCust.taxOffice,
                                        taxNumber = updatedCust.taxNumber,
                                        gpsLocation = updatedCust.gpsLocation,
                                        riskLimit = updatedCust.riskLimit,
                                        priceGroup = updatedCust.priceGroup,
                                        specialDiscountPercent = updatedCust.specialDiscountPercent,
                                        transactionsJson = com.example.data.database.Converters().fromCustomerTxList(updatedCust.transactions)
                                    ))
                                }
                            }
                        }
                        
                        db.withTransaction {
                            affectedCustomers.chunked(500).forEach { db.customerDao().insertAll(it) }
                        }
                        
                        totalFetched += items.size
                        val totalCount = body.total ?: 0
                        val progressPercent = if (totalCount > 0) (totalFetched.toFloat() / totalCount.toFloat()) else 0.5f
                        updateProgress(progressPercent)
                        
                        val elapsedSecs = (System.currentTimeMillis() - startTime) / 1000f
                        val speed = if (elapsedSecs > 0) (totalFetched / elapsedSecs).toInt() else 0
                        
                        if (totalCount > 0) {
                            com.example.util.SyncManager.updateSyncStats("CariHareket: $totalFetched / $totalCount kayıt (%${(progressPercent*100).toInt()}) - Hız: $speed sn/kayıt")
                        } else {
                            com.example.util.SyncManager.updateSyncStats("CariHareket: $totalFetched kayıt indirildi (Sayfa $currentPage) - Hız: $speed sn/kayıt")
                        }
                        
                        sharedPrefs.edit().putInt("cariHareketleri_last_page", currentPage + 1).apply()

                        if (items.size < pageSize) {
                            hasMore = false
                            sharedPrefs.edit().putInt("cariHareketleri_last_page", 1).apply()
                        } else {
                            currentPage++
                        }
                    }
                } else {
                    handleApiError(response, log)
                    throw Exception("API Hatası (\"Sayfa $currentPage cari hareketleri çekilemedi. Kod: ${response.code()}\")")
                }
            }
            if (totalFetched == 0) {
                log("Güncellenecek cari hesap hareketi verisi bulunamadı.")
            } else {
                log("Başarılı! Toplam $totalFetched adet işlem satırı alındı ve veritabanına kaydedildi.")
            }
            updateProgress(1.0f)
        } catch (e: Exception) {
            log("Köprü Bağlantı Hatası (Cari Hareketleri): ${e.message}. Lütfen Windows Servisinin çalıştığından emin olun.")
            updateProgress(1.0f)
            throw e
        }
    }    suspend fun syncFaturaHareket("""

new_content = re.sub(pattern, replacement, content, flags=re.DOTALL)
if new_content == content:
    print("Replace failed")
else:
    with open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "w") as f:
        f.write(new_content)
    print("Replace success")

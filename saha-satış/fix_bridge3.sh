#!/bin/bash
sed -i '2516,$d' app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt
sed -i '2514d' app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt

cat << 'INNER_EOF' >> app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt
    suspend fun syncStokHareketleri(context: Context, apiUrl: String, apiKey: String, log: (String) -> Unit, progress: (Float) -> Unit) {
        kotlinx.coroutines.withContext(kotlinx.coroutines.Dispatchers.IO) {
            try {
                log("Stok Hareketleri alınıyor...")
                progress(0.1f)
                val sharedPrefs = context.getSharedPreferences("erp_settings", Context.MODE_PRIVATE)
                val tenantId = sharedPrefs.getString("tenant_id", "T001") ?: "T001"
                val deviceId = sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT"
                
                val apiService = com.example.data.api.ApiClient.getFieldOpsApiService(context, apiUrl, apiKey)
                
                val req = com.example.data.api.PullJobsRequest(
                    tenant_id = tenantId,
                    api_key = apiKey,
                    device_id = deviceId,
                    agent_version = "v2.0",
                    entity = "stokHareketleri",
                    since = null
                )
                
                val resp = apiService.getStokHareketleri(req)
                if (resp.isSuccessful) {
                    val body = resp.body()
                    val items = body?.items ?: emptyList()
                    log("Toplam \${items.size} hareket alındı. İşleniyor...")
                    progress(0.4f)
                    
                    val grouped = items.groupBy { it.stokKod ?: it.urunKod ?: "" }.filterKeys { it.isNotEmpty() }
                    
                    val db = com.example.data.database.DatabaseProvider.getDatabase(context)
                    val allProducts = db.productDao().getAllProducts()
                    
                    var processed = 0
                    
                    for (prod in allProducts) {
                        val productCode = prod.code
                        val movementsDto = grouped[productCode]
                        if (movementsDto != null && movementsDto.isNotEmpty()) {
                            val movements = movementsDto.map { item ->
                                val dateStr = item.tarih?.take(10)?.let {
                                    val parts = it.split("-")
                                    if (parts.size == 3) "\${parts[2]}.\${parts[1]}.\${parts[0]}" else it
                                } ?: "Bilinmeyen Tarih"
                                
                                val miktar = item.miktar ?: (if ((item.girisMiktar ?: 0.0) > 0) item.girisMiktar else item.cikisMiktar) ?: 0.0
                                val typeStr = if ((item.tip ?: -1) == 0 || miktar > 0) "Giriş" else "Çıkış"
                                val prefix = if (typeStr == "Giriş") "+" else "-"
                                val evrakNo = item.evrakNo ?: ""
                                
                                com.example.ui.screens.StockMovement(
                                    date = dateStr,
                                    type = typeStr,
                                    qty = "$prefix\${kotlin.math.abs(miktar).toInt()} ADT",
                                    detail = "Evrak No: $evrakNo | Cari: \${item.cariKod ?: ""}",
                                    user = "Sistem",
                                    evrakNo = evrakNo,
                                    cariKod = item.cariKod,
                                    cariName = item.cariKod,
                                    unitPrice = item.birimFiyat ?: 0.0,
                                    totalAmount = item.tutar ?: (kotlin.math.abs(miktar) * (item.birimFiyat ?: 0.0)),
                                    warehouse = if (typeStr == "Giriş") item.girisDepoNo?.toString() else item.cikisDepoNo?.toString()
                                )
                            }
                            
                            val converter = com.example.data.database.Converters()
                            val json = converter.fromStockMovementList(movements)
                            
                            val updatedProd = prod.copy(transactionsJson = json)
                            db.productDao().insertAll(listOf(updatedProd))
                        }
                        processed++
                        if (processed % 50 == 0) {
                            progress(0.4f + (0.6f * processed / allProducts.size))
                        }
                    }
                    
                    log("Stok hareketleri veritabanına kaydedildi.")
                    progress(1.0f)
                } else {
                    log("Sunucu hatası: \${resp.code()} - \${resp.message()}")
                }
            } catch (e: Exception) {
                log("Hata: \${e.message}")
            }
        }
    }
}
INNER_EOF

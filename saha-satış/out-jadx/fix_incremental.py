import re

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r', encoding='utf-8') as f:
    content = f.read()

# Fix syncCariler
sync_cariler_chunk = """                    allMappedCustomers.addAll(mappedList)
                    
                    withContext(Dispatchers.Main) {
                        for (mapped in mappedList) {
                            val existingIndex = AppDataStore.customers.indexOfFirst { it.id == mapped.id }
                            if (existingIndex >= 0) {
                                AppDataStore.customers[existingIndex] = mapped
                            } else {
                                AppDataStore.customers.add(mapped)
                            }
                        }
                    }
                    AppDataStore.persist(context)"""

content = content.replace('allMappedCustomers.addAll(mappedList)', sync_cariler_chunk)

# Fix syncUrunler
sync_urunler_chunk = """                    allMappedProducts.addAll(mappedList)
                    
                    withContext(Dispatchers.Main) {
                        for (u in mappedList) {
                            val existingIndex = AppDataStore.products.indexOfFirst { it.code == u.code }
                            if (existingIndex >= 0) {
                                AppDataStore.products[existingIndex] = u
                            } else {
                                AppDataStore.products.add(u)
                            }
                        }
                    }
                    AppDataStore.persist(context)"""

content = content.replace('allMappedProducts.addAll(mappedList)', sync_urunler_chunk)

# Fix syncCariHareketleri
# For syncCariHareketleri, we can't do it easily page by page because it groups by cariKod across the WHOLE list.
# But wait, we can append to transactions for that specific cari.
sync_carihar_chunk = """                    allTx.addAll(items)
                    
                    withContext(Dispatchers.Main) {
                        val txGrouped = items.groupBy { it.cariKod ?: "" }
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
                                            } else rawDate
                                        } else rawDate
                                    } catch (e: Exception) { rawDate }
                                    val rawVade = dto.vadeTarihi ?: ""
                                    val formattedVade = try {
                                        if (rawVade.contains("T")) {
                                            val parts = rawVade.split("T")[0].split("-")
                                            if (parts.size == 3) {
                                                "${parts[2]}.${parts[1]}.${parts[0]}"
                                            } else rawVade
                                        } else rawVade
                                    } catch(e: Exception) { rawVade }
                                    com.example.data.database.Transaction(
                                        id = dto.id ?: java.util.UUID.randomUUID().toString(),
                                        date = formattedDate,
                                        desc = "${dto.evrakTipi ?: "Hareket"} - ${dto.evrakSeri ?: ""}${dto.evrakSira ?: ""}",
                                        amount = dto.meblag ?: 0.0,
                                        type = if (dto.borcAlacak == 0) "B" else "A",
                                        vade = formattedVade
                                    )
                                }
                                val existingTxs = customer.transactions.toMutableList()
                                // Update existing tx by id or add new
                                for (nt in newTxs) {
                                    val idx = existingTxs.indexOfFirst { it.id == nt.id }
                                    if (idx >= 0) existingTxs[idx] = nt
                                    else existingTxs.add(nt)
                                }
                                AppDataStore.customers[i] = customer.copy(transactions = existingTxs)
                            }
                        }
                    }
                    AppDataStore.persist(context)"""

content = content.replace('allTx.addAll(items)', sync_carihar_chunk)

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w', encoding='utf-8') as f:
    f.write(content)

import re

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r') as f:
    content = f.read()

# Fix syncUrunler
old_urunler_insert = """            if (allMappedProducts.isNotEmpty()) {
                withContext(Dispatchers.Main) {
                    AppDataStore.products.clear()
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
                AppDataStore.persist(context)
                log("Saha Gücü yerel stok kartları Room veritabanı başarıyla güncellendi. Toplam $totalFetched adet ürün/stok kaydı çekildi.")"""

new_urunler_insert = """            if (allMappedProducts.isNotEmpty()) {
                val dedupedMap = mutableMapOf<String, ProductCatalog>()
                val seenBarcodes = mutableSetOf<String>()
                for (u in allMappedProducts) {
                    val finalBarcode = if (u.barcode.isBlank() || u.barcode.lowercase() == "null" || u.barcode.lowercase() == "none" || seenBarcodes.contains(u.barcode)) {
                        u.code
                    } else {
                        u.barcode
                    }
                    seenBarcodes.add(finalBarcode)
                    val cleaned = u.copy(barcode = finalBarcode)
                    if (cleaned.code.isNotBlank()) {
                        dedupedMap[cleaned.code] = cleaned
                    } else {
                        dedupedMap[java.util.UUID.randomUUID().toString()] = cleaned
                    }
                }
                val finalProductList = dedupedMap.values.toList()
                
                withContext(Dispatchers.Main) {
                    AppDataStore.products.clear()
                    AppDataStore.products.addAll(finalProductList)
                }
                
                // Directly insert into DB to avoid AppDataStore.persist memory spike
                val db = com.example.data.database.DatabaseProvider.getDatabase(context)
                val converter = com.example.data.database.Converters()
                val productEntities = finalProductList.map { prod ->
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
                        cartonQuantity = prod.cartonQuantity
                    )
                }
                db.productDao().deleteAll()
                db.productDao().insertAll(productEntities)
                
                log("Saha Gücü yerel stok kartları Room veritabanı başarıyla güncellendi. Toplam $totalFetched adet ürün/stok kaydı çekildi.")"""

content = content.replace(old_urunler_insert, new_urunler_insert)

# Fix syncCariler
old_cariler_insert = """            if (allMappedCustomers.isNotEmpty()) {
                withContext(Dispatchers.Main) {
                    AppDataStore.customers.clear()
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
                AppDataStore.persist(context)
                log("Başarılı! Toplam $totalFetched adet cari kayıt FieldOps Bridge üzerinden başarıyla çekildi")"""

new_cariler_insert = """            if (allMappedCustomers.isNotEmpty()) {
                val dedupedMap = mutableMapOf<String, Customer>()
                for (mapped in allMappedCustomers) {
                    dedupedMap[mapped.id] = mapped
                }
                val finalCustomerList = dedupedMap.values.toList()
                
                withContext(Dispatchers.Main) {
                    AppDataStore.customers.clear()
                    AppDataStore.customers.addAll(finalCustomerList)
                }
                
                // Directly insert into DB to avoid AppDataStore.persist memory spike
                val db = com.example.data.database.DatabaseProvider.getDatabase(context)
                val converter = com.example.data.database.Converters()
                val customerEntities = finalCustomerList.map { cust ->
                    com.example.data.database.CustomerEntity(
                        id = cust.id,
                        name = cust.name,
                        balance = cust.balance,
                        lastVisit = cust.lastVisit,
                        contact = cust.contact,
                        phone = cust.phone,
                        address = cust.address,
                        taxOffice = cust.taxOffice,
                        taxNumber = cust.taxNumber,
                        gpsLocation = cust.gpsLocation,
                        riskLimit = cust.riskLimit,
                        priceGroup = cust.priceGroup,
                        specialDiscountPercent = cust.specialDiscountPercent,
                        transactionsJson = converter.fromCustomerTxList(cust.transactions)
                    )
                }
                db.customerDao().deleteAll()
                db.customerDao().insertAll(customerEntities)
                
                log("Başarılı! Toplam $totalFetched adet cari kayıt FieldOps Bridge üzerinden başarıyla çekildi")"""

content = content.replace(old_cariler_insert, new_cariler_insert)


with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w') as f:
    f.write(content)

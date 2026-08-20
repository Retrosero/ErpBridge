import re

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r') as f:
    content = f.read()

# Fix syncFiyatListeleri
old_fiyat = """                            AppDataStore.products[i] = prod.copy(
                                basePrice = baseP,
                                dealerPrice = dealerP,
                                wholesalePrice = wholesaleP,
                                customPrices = newCustomPrices
                            )
                            updatedCount++
                        }
                    }
                    AppDataStore.persist(context)
                    log("Başarılı! $updatedCount adet stok kartının özel fiyat listesi tanımları ve fiyatları başarıyla güncellendi.")"""

new_fiyat = """                            AppDataStore.products[i] = prod.copy(
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
                            cartonQuantity = prod.cartonQuantity
                        )
                    }
                    withContext(Dispatchers.IO) {
                        db.productDao().insertAll(productEntities)
                    }
                    
                    AppDataStore.persist(context)
                    log("Başarılı! $updatedCount adet stok kartının özel fiyat listesi tanımları ve fiyatları başarıyla güncellendi.")"""

content = content.replace(old_fiyat, new_fiyat)

# Fix syncStokSeviyeleri
old_stok = """                        if (newStockMap != prod.stockByWarehouse) {
                            AppDataStore.products[i] = prod.copy(stockByWarehouse = newStockMap)
                            updatedCount++
                        }
                    }
                    AppDataStore.persist(context)
                    log("Başarılı! $updatedCount adet stok kartının depo bazlı elde kalan stok seviyeleri başarıyla güncellendi.")"""

new_stok = """                        if (newStockMap != prod.stockByWarehouse) {
                            AppDataStore.products[i] = prod.copy(stockByWarehouse = newStockMap)
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
                            cartonQuantity = prod.cartonQuantity
                        )
                    }
                    withContext(Dispatchers.IO) {
                        db.productDao().insertAll(productEntities)
                    }
                    
                    AppDataStore.persist(context)
                    log("Başarılı! $updatedCount adet stok kartının depo bazlı elde kalan stok seviyeleri başarıyla güncellendi.")"""

content = content.replace(old_stok, new_stok)

# Fix syncCariHareketleri
old_hareket = """                            AppDataStore.customers[i] = customer.copy(transactions = allCombinedTxs)
                            updatedCustomersCount++
                        }
                    }
                }
                AppDataStore.persist(context)
                log("Başarılı! $totalFetched adet cari işlem eşlendi ve $updatedCustomersCount cari güncellendi.")"""

new_hareket = """                            AppDataStore.customers[i] = customer.copy(transactions = allCombinedTxs)
                            updatedCustomersCount++
                        }
                    }
                }
                
                val db = com.example.data.database.DatabaseProvider.getDatabase(context)
                val converter = com.example.data.database.Converters()
                val cList = AppDataStore.customers.toList()
                val customerEntities = cList.map { cust ->
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
                withContext(Dispatchers.IO) {
                    db.customerDao().insertAll(customerEntities)
                }
                
                AppDataStore.persist(context)
                log("Başarılı! $totalFetched adet cari işlem eşlendi ve $updatedCustomersCount cari güncellendi.")"""

content = content.replace(old_hareket, new_hareket)

# Fix syncFiyatListesiNew
old_fiyat_new = """                                AppDataStore.products[existingIndex] = prod.copy(
                                    basePrice = baseP,
                                    dealerPrice = dealerP,
                                    wholesalePrice = wholesaleP,
                                    customPrices = newCustomPrices
                                )
                                updatedCount++
                            }
                        }
                    }
                    AppDataStore.persist(context)
                    log("Alternatif Fiyat Listesi Metodu ile $updatedCount adet ürünün fiyatı güncellendi.")"""

new_fiyat_new = """                                AppDataStore.products[existingIndex] = prod.copy(
                                    basePrice = baseP,
                                    dealerPrice = dealerP,
                                    wholesalePrice = wholesaleP,
                                    customPrices = newCustomPrices
                                )
                                updatedCount++
                            }
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
                            cartonQuantity = prod.cartonQuantity
                        )
                    }
                    withContext(Dispatchers.IO) {
                        db.productDao().insertAll(productEntities)
                    }
                    
                    AppDataStore.persist(context)
                    log("Alternatif Fiyat Listesi Metodu ile $updatedCount adet ürünün fiyatı güncellendi.")"""

content = content.replace(old_fiyat_new, new_fiyat_new)

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w') as f:
    f.write(content)

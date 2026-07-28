with open("app/src/main/java/com/example/ui/screens/AppDataStore.kt", "r") as f:
    content = f.read()

# Replace the db calls
old_db = """            db.bankDao().deleteAll()
            db.bankDao().insertAll(bankEntities)

            // 2. Save Kasa Logs
            val kasaEntities = kasaLogsCopy.map { KasaLogEntity(it.id, it.date, it.type, it.customerOrSupplier, it.amount, it.paymentType, it.bankName, it.desc) }
            db.kasaLogDao().deleteAll()
            db.kasaLogDao().insertAll(kasaEntities)

            // 3. Save Sales History (using index offset for id to prevent batch insert key collisions)
            val salesEntities = salesHistoryCopy.mapIndexed { idx, it ->
                SalesRecordEntity(id = idx + 1, customerId = it.customerId, productBarcode = it.productBarcode, quantity = it.quantity, price = it.price, date = it.date)
            }
            db.salesRecordDao().deleteAll()
            db.salesRecordDao().insertAll(salesEntities)

            // 4. Save products.  ERP synchronization first updates the in-memory list and
            // then calls persist(); without this write, the product list disappeared after
            // a process restart even though synchronization reported success.
            val seenBarcodes = mutableSetOf<String>()
            val productEntities = productsCopy.map { product ->
                val finalBarcode = if (product.barcode.isBlank() || product.barcode.lowercase() == "null" || product.barcode.lowercase() == "none" || seenBarcodes.contains(product.barcode)) {
                    product.code.ifBlank { java.util.UUID.randomUUID().toString() }
                } else {
                    product.barcode
                }
                seenBarcodes.add(finalBarcode)
                ProductEntity(
                    barcode = finalBarcode,
                    code = product.code,
                    title = product.title,
                    category = product.category,
                    desc = product.desc,
                    basePrice = product.basePrice,
                    dealerPrice = product.dealerPrice,
                    wholesalePrice = product.wholesalePrice,
                    kdvPercent = product.kdvPercent,
                    colorValue = product.imageUrlColor.value.toLong(),
                    brand = product.brand,
                    stockByWarehouseJson = converter.fromWarehouseMap(product.stockByWarehouse),
                    boxQty = product.boxQty,
                    packageQty = product.packageQty,
                    imageUrl = product.imageUrl,
                    localImagePath = product.localImagePath,
                    aisle = product.aisle,
                    customPricesJson = converter.fromCustomPricesMap(product.customPrices),
                    barcodesJson = converter.fromBarcodeList(product.barcodes),
                    measurement = product.measurement,
                    packaging = product.packaging,
                    cartonQuantity = product.cartonQuantity,
    imageLinksJson = null,
    localImagePathsJson = null
                )
            }
            db.productDao().deleteAll()
            db.productDao().insertAll(productEntities)

            // 5. Save Customers
            val customerEntities = customersCopy.map { cust ->
                CustomerEntity(
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
            db.customerDao().insertAll(customerEntities)"""

new_db = """            // 2. Save Kasa Logs
            val kasaEntities = kasaLogsCopy.map { KasaLogEntity(it.id, it.date, it.type, it.customerOrSupplier, it.amount, it.paymentType, it.bankName, it.desc) }

            // 3. Save Sales History (using index offset for id to prevent batch insert key collisions)
            val salesEntities = salesHistoryCopy.mapIndexed { idx, it ->
                SalesRecordEntity(id = idx + 1, customerId = it.customerId, productBarcode = it.productBarcode, quantity = it.quantity, price = it.price, date = it.date)
            }

            // 4. Save products.  ERP synchronization first updates the in-memory list and
            // then calls persist(); without this write, the product list disappeared after
            // a process restart even though synchronization reported success.
            val seenBarcodes = mutableSetOf<String>()
            val productEntities = productsCopy.map { product ->
                val finalBarcode = if (product.barcode.isBlank() || product.barcode.lowercase() == "null" || product.barcode.lowercase() == "none" || seenBarcodes.contains(product.barcode)) {
                    product.code.ifBlank { java.util.UUID.randomUUID().toString() }
                } else {
                    product.barcode
                }
                seenBarcodes.add(finalBarcode)
                ProductEntity(
                    barcode = finalBarcode,
                    code = product.code,
                    title = product.title,
                    category = product.category,
                    desc = product.desc,
                    basePrice = product.basePrice,
                    dealerPrice = product.dealerPrice,
                    wholesalePrice = product.wholesalePrice,
                    kdvPercent = product.kdvPercent,
                    colorValue = product.imageUrlColor.value.toLong(),
                    brand = product.brand,
                    stockByWarehouseJson = converter.fromWarehouseMap(product.stockByWarehouse),
                    boxQty = product.boxQty,
                    packageQty = product.packageQty,
                    imageUrl = product.imageUrl,
                    localImagePath = product.localImagePath,
                    aisle = product.aisle,
                    customPricesJson = converter.fromCustomPricesMap(product.customPrices),
                    barcodesJson = converter.fromBarcodeList(product.barcodes),
                    measurement = product.measurement,
                    packaging = product.packaging,
                    cartonQuantity = product.cartonQuantity,
                    imageLinksJson = null,
                    localImagePathsJson = null
                )
            }

            // 5. Save Customers
            val customerEntities = customersCopy.map { cust ->
                CustomerEntity(
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

            db.withTransaction {
                db.bankDao().deleteAll()
                db.bankDao().insertAll(bankEntities)
                
                db.kasaLogDao().deleteAll()
                db.kasaLogDao().insertAll(kasaEntities)
                
                db.salesRecordDao().deleteAll()
                db.salesRecordDao().insertAll(salesEntities)
                
                db.productDao().deleteAll()
                db.productDao().insertAll(productEntities)
                
                db.customerDao().deleteAll()
                db.customerDao().insertAll(customerEntities)
            }"""

if old_db in content:
    content = content.replace(old_db, new_db)
    
    # We also need to add the closing bracket for withLock
    # Find the end of persistSync
    end_part = """        } catch (e: Throwable) {
            e.printStackTrace()
        }
    }"""
    
    new_end_part = """        } catch (e: Throwable) {
            e.printStackTrace()
        }
        } // end withLock
    }"""
    content = content.replace(end_part, new_end_part)
    with open("app/src/main/java/com/example/ui/screens/AppDataStore.kt", "w") as f:
        f.write(content)
    print("Replaced successfully")
else:
    print("old_db not found")

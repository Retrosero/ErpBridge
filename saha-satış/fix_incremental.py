import re

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r') as f:
    content = f.read()

old_inc_cariler = """            if (allMappedCustomers.isNotEmpty()) {
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
            }"""

new_inc_cariler = """            if (allMappedCustomers.isNotEmpty()) {
                val db = com.example.data.database.DatabaseProvider.getDatabase(context)
                val converter = com.example.data.database.Converters()
                val customerEntities = allMappedCustomers.map { cust ->
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
                db.customerDao().insertAll(customerEntities) // Upsert in DB directly
                
                // Instead of O(N^2) index search, we just load them all again or update a map
                withContext(Dispatchers.Main) {
                    val currentMap = AppDataStore.customers.associateBy { it.id }.toMutableMap()
                    for (mapped in allMappedCustomers) {
                        currentMap[mapped.id] = mapped
                    }
                    AppDataStore.customers.clear()
                    AppDataStore.customers.addAll(currentMap.values)
                }
            }"""

content = content.replace(old_inc_cariler, new_inc_cariler)

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w') as f:
    f.write(content)

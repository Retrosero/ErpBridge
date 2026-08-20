import re

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r', encoding='utf-8') as f:
    content = f.read()

# Replace customers incremental loop
old_customers_inc = """            if (allMappedCustomers.isNotEmpty()) {
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

content = content.replace(old_customers_inc, """            if (allMappedCustomers.isNotEmpty()) {
                val currentList = AppDataStore.customers.toList()
                val customerMap = currentList.associateBy { it.id }.toMutableMap()
                for (mapped in allMappedCustomers) {
                    customerMap[mapped.id] = mapped
                }
                val mergedList = customerMap.values.toList()
                
                withContext(Dispatchers.Main) {
                    AppDataStore.customers.clear()
                    AppDataStore.customers.addAll(mergedList)
                }
                AppDataStore.persist(context)
            }""")

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w', encoding='utf-8') as f:
    f.write(content)

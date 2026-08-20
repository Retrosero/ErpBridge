import re

with open('app/src/main/java/com/example/ui/screens/AppDataStore.kt', 'r', encoding='utf-8') as f:
    content = f.read()

# Find persist function
persist_old = """    fun persist(context: Context) {
        dbScope.launch {
            persistSync(context)
        }
    }"""

persist_new = """    fun persist(context: Context) {
        val banksCopy = banks.toList()
        val kasaLogsCopy = kasaLogs.toList()
        val salesHistoryCopy = salesHistory.toList()
        val productsCopy = products.toList()
        val customersCopy = customers.toList()
        
        dbScope.launch {
            persistSync(context, banksCopy, kasaLogsCopy, salesHistoryCopy, productsCopy, customersCopy)
        }
    }"""

content = content.replace(persist_old, persist_new)

# Update persistSync signature
persist_sync_old = """    private suspend fun persistSync(context: Context) {"""
persist_sync_new = """    private suspend fun persistSync(
        context: Context,
        banks: List<Bank> = this.banks.toList(),
        kasaLogs: List<KasaLogItem> = this.kasaLogs.toList(),
        salesHistory: List<SalesRecord> = this.salesHistory.toList(),
        products: List<ProductCatalog> = this.products.toList(),
        customers: List<Customer> = this.customers.toList()
    ) {"""

content = content.replace(persist_sync_old, persist_sync_new)

with open('app/src/main/java/com/example/ui/screens/AppDataStore.kt', 'w', encoding='utf-8') as f:
    f.write(content)

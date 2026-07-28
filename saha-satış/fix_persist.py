import re
with open("app/src/main/java/com/example/ui/screens/AppDataStore.kt", "r") as f:
    content = f.read()

# Remove the lock from inside persistSync
old_persistSync_start = """    private suspend fun persistSync(
        context: Context,
        productsCopy: List<ProductCatalog>,
        customersCopy: List<Customer>,
        banksCopy: List<Bank>,
        kasaLogsCopy: List<KasaLogItem>,
        salesHistoryCopy: List<SalesRecord>
    ) {
        persistMutex.withLock {
        try {"""

new_persistSync_start = """    private suspend fun persistSync(
        context: Context,
        productsCopy: List<ProductCatalog>,
        customersCopy: List<Customer>,
        banksCopy: List<Bank>,
        kasaLogsCopy: List<KasaLogItem>,
        salesHistoryCopy: List<SalesRecord>
    ) {
        try {"""

if old_persistSync_start in content:
    content = content.replace(old_persistSync_start, new_persistSync_start)
    
    # Remove the extra } at the end
    end_part = """        } catch (e: Throwable) {
            e.printStackTrace()
        }
        } // end withLock
    }"""
    
    new_end_part = """        } catch (e: Throwable) {
            e.printStackTrace()
        }
    }"""
    content = content.replace(end_part, new_end_part)
    
    # Now add the lock to persist() and persistAndWait()
    
    # persistAndWait()
    search_paw = """    suspend fun persistAndWait(context: Context) {
        val productsCopy: List<ProductCatalog>"""
    replace_paw = """    suspend fun persistAndWait(context: Context) {
        persistMutex.withLock {
        val productsCopy: List<ProductCatalog>"""
    content = content.replace(search_paw, replace_paw)
    
    search_paw_end = """        persistSync(context, productsCopy, customersCopy, banksCopy, kasaLogsCopy, salesHistoryCopy)
    }"""
    replace_paw_end = """        persistSync(context, productsCopy, customersCopy, banksCopy, kasaLogsCopy, salesHistoryCopy)
        }
    }"""
    content = content.replace(search_paw_end, replace_paw_end)
    
    # persist()
    search_p = """    fun persist(context: Context) {
        dbScope.launch {
            val productsCopy: List<ProductCatalog>"""
    replace_p = """    fun persist(context: Context) {
        dbScope.launch {
            persistMutex.withLock {
            val productsCopy: List<ProductCatalog>"""
    content = content.replace(search_p, replace_p)
    
    search_p_end = """            persistSync(context, productsCopy, customersCopy, banksCopy, kasaLogsCopy, salesHistoryCopy)
        }
    }"""
    replace_p_end = """            persistSync(context, productsCopy, customersCopy, banksCopy, kasaLogsCopy, salesHistoryCopy)
            }
        }
    }"""
    content = content.replace(search_p_end, replace_p_end)

    with open("app/src/main/java/com/example/ui/screens/AppDataStore.kt", "w") as f:
        f.write(content)
    print("Fixed persist")
else:
    print("Could not find old_persistSync_start")


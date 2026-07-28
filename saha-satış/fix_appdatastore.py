import re

with open("app/src/main/java/com/example/ui/screens/AppDataStore.kt", "r") as f:
    content = f.read()

# 1. Add imports
if "import androidx.room.withTransaction" not in content:
    content = content.replace("import androidx.compose.ui.graphics.Color", 
                              "import androidx.compose.ui.graphics.Color\nimport androidx.room.withTransaction\nimport kotlinx.coroutines.sync.Mutex\nimport kotlinx.coroutines.sync.withLock")

# 2. Add Mutex
if "val persistMutex = Mutex()" not in content:
    content = content.replace("object AppDataStore {", "object AppDataStore {\n    private val persistMutex = Mutex()")

# 3. Modify persistSync
# We need to wrap from `try {` to the end of DB operations
search_str = """    private suspend fun persistSync(
        context: Context,
        productsCopy: List<ProductCatalog>,
        customersCopy: List<Customer>,
        banksCopy: List<Bank>,
        kasaLogsCopy: List<KasaLogItem>,
        salesHistoryCopy: List<SalesRecord>
    ) {
        try {"""

replace_str = """    private suspend fun persistSync(
        context: Context,
        productsCopy: List<ProductCatalog>,
        customersCopy: List<Customer>,
        banksCopy: List<Bank>,
        kasaLogsCopy: List<KasaLogItem>,
        salesHistoryCopy: List<SalesRecord>
    ) {
        persistMutex.withLock {
        try {"""

content = content.replace(search_str, replace_str)

# Now find the DB operations and wrap them in db.withTransaction
db_ops_start = """            // 1. Save Banks
            val bankEntities = banksCopy.map { BankEntity(it.id, it.name, it.accountNo, it.iban, it.balance) }"""

db_ops_replace = """            // 1. Save Banks
            val bankEntities = banksCopy.map { BankEntity(it.id, it.name, it.accountNo, it.iban, it.balance) }"""

content = content.replace(db_ops_start, db_ops_replace)

with open("app/src/main/java/com/example/ui/screens/AppDataStore.kt", "w") as f:
    f.write(content)

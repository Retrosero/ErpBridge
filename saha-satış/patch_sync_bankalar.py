import re

with open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "r") as f:
    content = f.read()

# For syncBankalar, insert Room save logic right before updating AppDataStore
bank_save_logic = """
            val db = com.example.data.database.DatabaseProvider.getDatabase(context.applicationContext)
            val bankEntities = loadedItems.map {
                com.example.data.database.BridgeBankaEntity(
                    id = it.erpRef ?: java.util.UUID.randomUUID().toString(),
                    kod = it.kod ?: "",
                    isim = it.isim ?: "",
                    sube = it.sube ?: "",
                    iban = it.iBANKodu ?: "",
                    hesapNumarasi = it.hesapNumarasi ?: ""
                )
            }
            db.bridgeBankaDao().deleteAll()
            bankEntities.chunked(500).forEach { db.bridgeBankaDao().insertAll(it) }

            withContext(Dispatchers.Main) {
"""

content = content.replace("withContext(Dispatchers.Main) {\n                AppDataStore.bridgeBankalar.clear()", bank_save_logic + "                AppDataStore.bridgeBankalar.clear()")

with open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "w") as f:
    f.write(content)


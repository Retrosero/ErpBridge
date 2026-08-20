import re

with open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "r") as f:
    content = f.read()

# For syncCariAdresleri
adres_save_logic = """
            val db = com.example.data.database.DatabaseProvider.getDatabase(context.applicationContext)
            val adresEntities = loadedItems.map {
                com.example.data.database.CariAdresEntity(
                    id = it.erpRef ?: java.util.UUID.randomUUID().toString(),
                    cariKod = it.cariKod ?: "",
                    adresNo = it.adresNo ?: 0,
                    il = it.il ?: "",
                    ilce = it.ilce ?: "",
                    mahalle = it.mahalle ?: "",
                    cadde = it.cadde ?: "",
                    sokak = it.sokak ?: ""
                )
            }
            db.cariAdresDao().deleteAll()
            adresEntities.chunked(500).forEach { db.cariAdresDao().insertAll(it) }

            withContext(Dispatchers.Main) {
"""

content = content.replace("withContext(Dispatchers.Main) {\n                AppDataStore.cariAdresleri.clear()", adres_save_logic + "                AppDataStore.cariAdresleri.clear()")

with open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "w") as f:
    f.write(content)


import re

with open("app/src/main/java/com/example/ui/screens/AppDataStore.kt", "r") as f:
    content = f.read()

injection = """
                    val loadedStockMovements = db.stockMovementDao().getAll()
                    val loadedCariMovements = db.cariHareketDao().getAll()
                    
                    withContext(Dispatchers.Main) {
"""

content = content.replace("withContext(Dispatchers.Main) {", injection, 1)

with open("app/src/main/java/com/example/ui/screens/AppDataStore.kt", "w") as f:
    f.write(content)

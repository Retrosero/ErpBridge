import re
import os

for root, dirs, files in os.walk("app/src/main/java/com/example"):
    for file in files:
        if file.endswith(".kt"):
            filepath = os.path.join(root, file)
            with open(filepath, "r") as f:
                content = f.read()
            
            # Find catch blocks that are entirely empty or just have spaces
            # But wait, try { db.stockMovementDao().getAll() } catch (e: Exception) {  }
            new_content = re.sub(
                r'=\s*try\s*\{\s*(db\.[a-zA-Z0-9_]+\(\)\.getAll\(\))\s*\}\s*catch\s*\(\s*[a-zA-Z0-9_]+\s*:\s*Exception\s*\)\s*\{\s*\}',
                r'= \1',
                content
            )

            # Let's just fix it generally for loadedStockMovements
            new_content = new_content.replace(
                "val loadedStockMovements = try { db.stockMovementDao().getAll() } catch (e: Exception) {  }",
                "val loadedStockMovements = db.stockMovementDao().getAll()"
            )
            
            new_content = new_content.replace(
                "val loadedCariMovements = try { db.cariHareketDao().getAll() } catch (e: Exception) {  }",
                "val loadedCariMovements = db.cariHareketDao().getAll()"
            )

            if new_content != content:
                with open(filepath, "w") as f:
                    f.write(new_content)

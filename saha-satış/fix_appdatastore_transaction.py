import re
with open('app/src/main/java/com/example/ui/screens/AppDataStore.kt', 'r', encoding='utf-8') as f:
    content = f.read()

# Make sure androidx.room.withTransaction is imported
if "import androidx.room.withTransaction" not in content:
    content = content.replace("import com.example.data.database.DatabaseProvider", "import com.example.data.database.DatabaseProvider\nimport androidx.room.withTransaction")

# Wrap the db operations in withTransaction
old_block = """            val db = DatabaseProvider.getDatabase(context)
            val converter = Converters()"""

new_block = """            val db = DatabaseProvider.getDatabase(context)
            val converter = Converters()
            db.withTransaction {"""

if "db.withTransaction {" not in content:
    content = content.replace(old_block, new_block)
    
    # We need to find the end of the db operations and close the withTransaction block
    # It ends at:
    #             val customerEntities = customers.map { c ->
    #                 ...
    #             }
    #             db.customerDao().deleteAll()
    #             customerEntities.chunked(100).forEach { db.customerDao().insertAll(it) }
    
    end_pattern = r'(db\.customerDao\(\)\.insertAll\(it\)\s*\})'
    content = re.sub(end_pattern, r'\1\n            }', content)

with open('app/src/main/java/com/example/ui/screens/AppDataStore.kt', 'w', encoding='utf-8') as f:
    f.write(content)

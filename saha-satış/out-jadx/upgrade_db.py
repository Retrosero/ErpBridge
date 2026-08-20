import re

# 1. AppDatabase.kt version 8 -> 9
with open('app/src/main/java/com/example/data/database/AppDatabase.kt', 'r') as f:
    content = f.read()
content = re.sub(r'version = 8,', 'version = 9,', content)
with open('app/src/main/java/com/example/data/database/AppDatabase.kt', 'w') as f:
    f.write(content)

# 2. DatabaseProvider.kt
with open('app/src/main/java/com/example/data/database/DatabaseProvider.kt', 'r') as f:
    content = f.read()
migration_str = """    private val MIGRATION_8_9 = object : Migration(8, 9) {
        override fun migrate(db: SupportSQLiteDatabase) {
            db.execSQL("ALTER TABLE products ADD COLUMN imageLinksJson TEXT")
            db.execSQL("ALTER TABLE products ADD COLUMN localImagePathsJson TEXT")
        }
    }

    @Volatile"""
content = content.replace("@Volatile", migration_str)
content = content.replace(".addMigrations(MIGRATION_7_8)", ".addMigrations(MIGRATION_7_8, MIGRATION_8_9)")
with open('app/src/main/java/com/example/data/database/DatabaseProvider.kt', 'w') as f:
    f.write(content)

# 3. DatabaseModels.kt
with open('app/src/main/java/com/example/data/database/DatabaseModels.kt', 'r') as f:
    content = f.read()
content = content.replace("val cartonQuantity: String? = null\n)", "val cartonQuantity: String? = null,\n    val imageLinksJson: String? = null,\n    val localImagePathsJson: String? = null\n)")
with open('app/src/main/java/com/example/data/database/DatabaseModels.kt', 'w') as f:
    f.write(content)


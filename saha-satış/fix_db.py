import re

with open("app/src/main/java/com/example/data/database/AppDatabase.kt", "r") as f:
    content = f.read()

content = content.replace("StockMovementEntity::class", "StockMovementEntity::class,\n        TelemetryEventEntity::class")
content = content.replace("version = 9,", "version = 10,")
content = content.replace("abstract fun stockMovementDao(): StockMovementDao\n}", "abstract fun stockMovementDao(): StockMovementDao\n    abstract fun telemetryDao(): TelemetryDao\n}")

with open("app/src/main/java/com/example/data/database/AppDatabase.kt", "w") as f:
    f.write(content)

with open("app/src/main/java/com/example/data/database/DatabaseProvider.kt", "r") as f:
    content = f.read()

migration_9_10 = """
    private val MIGRATION_9_10 = object : Migration(9, 10) {
        override fun migrate(db: SupportSQLiteDatabase) {
            db.execSQL("CREATE TABLE IF NOT EXISTS `telemetry_events` (`eventId` TEXT NOT NULL, `occurredAtUtc` TEXT NOT NULL, `kind` TEXT NOT NULL, `severity` TEXT NOT NULL, `appVersion` TEXT NOT NULL, `androidVersion` TEXT NOT NULL, `deviceModel` TEXT NOT NULL, `screen` TEXT NOT NULL, `operation` TEXT NOT NULL, `exceptionType` TEXT NOT NULL, `message` TEXT NOT NULL, `stackTrace` TEXT NOT NULL, `httpMethod` TEXT NOT NULL, `httpRoute` TEXT NOT NULL, `httpStatus` INTEGER, `correlationId` TEXT NOT NULL, `breadcrumbsJson` TEXT NOT NULL, PRIMARY KEY(`eventId`))")
        }
    }
"""

content = content.replace("private val MIGRATION_8_9", migration_9_10 + "\n    private val MIGRATION_8_9")
content = content.replace(".addMigrations(MIGRATION_7_8, MIGRATION_8_9)", ".addMigrations(MIGRATION_7_8, MIGRATION_8_9, MIGRATION_9_10)")

with open("app/src/main/java/com/example/data/database/DatabaseProvider.kt", "w") as f:
    f.write(content)


package com.example.data.database

import android.content.Context
import androidx.room.Room
import androidx.room.migration.Migration
import androidx.sqlite.db.SupportSQLiteDatabase

object DatabaseProvider {
    private val MIGRATION_9_10 = object : Migration(9, 10) {
        override fun migrate(db: SupportSQLiteDatabase) {
            db.execSQL("ALTER TABLE products ADD COLUMN measurement TEXT")
            db.execSQL("ALTER TABLE products ADD COLUMN packaging TEXT")
            db.execSQL("ALTER TABLE products ADD COLUMN cartonQuantity TEXT")
        }
    }

    private val MIGRATION_10_11 = object : Migration(10, 11) {
        override fun migrate(db: SupportSQLiteDatabase) {
            db.execSQL(
                """
                CREATE TABLE IF NOT EXISTS telemetry_events (
                    eventId TEXT NOT NULL PRIMARY KEY,
                    occurredAtUtc TEXT NOT NULL,
                    kind TEXT NOT NULL,
                    severity TEXT NOT NULL,
                    appVersion TEXT NOT NULL,
                    androidVersion TEXT NOT NULL,
                    deviceModel TEXT NOT NULL,
                    screen TEXT,
                    operation TEXT,
                    exceptionType TEXT,
                    message TEXT,
                    stackTrace TEXT,
                    httpMethod TEXT,
                    httpRoute TEXT,
                    httpStatus INTEGER,
                    correlationId TEXT,
                    breadcrumbsJson TEXT NOT NULL,
                    createdAtEpochMs INTEGER NOT NULL
                )
                """.trimIndent()
            )
            db.execSQL("CREATE INDEX IF NOT EXISTS index_telemetry_events_createdAtEpochMs ON telemetry_events(createdAtEpochMs)")
        }
    }

    @Volatile
    private var INSTANCE: AppDatabase? = null

    fun getDatabase(context: Context): AppDatabase {
        return INSTANCE ?: synchronized(this) {
            val instance = Room.databaseBuilder(
                context.applicationContext,
                AppDatabase::class.java,
                "field_force_db"
            )
            .addMigrations(MIGRATION_9_10, MIGRATION_10_11)
            .build()
            INSTANCE = instance
            instance
        }
    }
}

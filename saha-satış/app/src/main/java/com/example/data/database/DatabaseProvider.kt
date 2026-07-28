package com.example.data.database

import android.content.Context
import androidx.room.Room
import androidx.room.migration.Migration
import androidx.sqlite.db.SupportSQLiteDatabase

object DatabaseProvider {
    private val MIGRATION_7_8 = object : Migration(7, 8) {
        override fun migrate(db: SupportSQLiteDatabase) {
            db.execSQL("ALTER TABLE products ADD COLUMN measurement TEXT")
            db.execSQL("ALTER TABLE products ADD COLUMN packaging TEXT")
            db.execSQL("ALTER TABLE products ADD COLUMN cartonQuantity TEXT")
        }
    }

        
    private val MIGRATION_9_10 = object : Migration(9, 10) {
        override fun migrate(db: SupportSQLiteDatabase) {
            db.execSQL("CREATE TABLE IF NOT EXISTS `telemetry_events` (`eventId` TEXT NOT NULL, `occurredAtUtc` TEXT NOT NULL, `kind` TEXT NOT NULL, `severity` TEXT NOT NULL, `appVersion` TEXT NOT NULL, `androidVersion` TEXT NOT NULL, `deviceModel` TEXT NOT NULL, `screen` TEXT NOT NULL, `operation` TEXT NOT NULL, `exceptionType` TEXT NOT NULL, `message` TEXT NOT NULL, `stackTrace` TEXT NOT NULL, `httpMethod` TEXT NOT NULL, `httpRoute` TEXT NOT NULL, `httpStatus` INTEGER, `correlationId` TEXT NOT NULL, `breadcrumbsJson` TEXT NOT NULL, PRIMARY KEY(`eventId`))")
        }
    }

    private val MIGRATION_8_9 = object : Migration(8, 9) {
        override fun migrate(db: SupportSQLiteDatabase) {
            db.execSQL("ALTER TABLE products ADD COLUMN imageLinksJson TEXT")
            db.execSQL("ALTER TABLE products ADD COLUMN localImagePathsJson TEXT")
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
            .addMigrations(MIGRATION_7_8, MIGRATION_8_9, MIGRATION_9_10)
            // .fallbackToDestructiveMigration() removed to prevent data loss on schema updates
            .build()
            INSTANCE = instance
            instance
        }
    }
}

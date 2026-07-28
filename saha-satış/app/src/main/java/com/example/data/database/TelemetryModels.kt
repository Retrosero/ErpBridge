package com.example.data.database

import androidx.room.Entity
import androidx.room.PrimaryKey
import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query

@Entity(tableName = "telemetry_events")
data class TelemetryEventEntity(
    @PrimaryKey
    val eventId: String,
    val occurredAtUtc: String,
    val kind: String,
    val severity: String,
    val appVersion: String,
    val androidVersion: String,
    val deviceModel: String,
    val screen: String,
    val operation: String,
    val exceptionType: String,
    val message: String,
    val stackTrace: String,
    val httpMethod: String,
    val httpRoute: String,
    val httpStatus: Int?,
    val correlationId: String,
    val breadcrumbsJson: String
)

@Dao
interface TelemetryDao {
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insert(event: TelemetryEventEntity)

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertAll(events: List<TelemetryEventEntity>)

    @Query("SELECT * FROM telemetry_events ORDER BY occurredAtUtc ASC LIMIT :limit")
    suspend fun getOldestEvents(limit: Int): List<TelemetryEventEntity>

    @Query("DELETE FROM telemetry_events WHERE eventId IN (:eventIds)")
    suspend fun deleteEventsByIds(eventIds: List<String>)

    @Query("DELETE FROM telemetry_events WHERE occurredAtUtc < :thresholdUtc OR eventId NOT IN (SELECT eventId FROM telemetry_events ORDER BY occurredAtUtc DESC LIMIT :maxCount)")
    suspend fun cleanupOldEvents(thresholdUtc: String, maxCount: Int)
}

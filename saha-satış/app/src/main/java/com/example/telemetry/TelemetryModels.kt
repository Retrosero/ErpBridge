package com.example.telemetry

import androidx.room.Dao
import androidx.room.Entity
import androidx.room.Insert
import androidx.room.Index
import androidx.room.OnConflictStrategy
import androidx.room.Query
import com.squareup.moshi.Json

@Entity(
    tableName = "telemetry_events",
    primaryKeys = ["eventId"],
    indices = [Index("createdAtEpochMs")]
)
data class TelemetryEventEntity(
    val eventId: String,
    val occurredAtUtc: String,
    val kind: String,
    val severity: String,
    val appVersion: String,
    val androidVersion: String,
    val deviceModel: String,
    val screen: String?,
    val operation: String?,
    val exceptionType: String?,
    val message: String?,
    val stackTrace: String?,
    val httpMethod: String?,
    val httpRoute: String?,
    val httpStatus: Int?,
    val correlationId: String?,
    val breadcrumbsJson: String,
    val createdAtEpochMs: Long
)

@Dao
interface TelemetryDao {
    @Insert(onConflict = OnConflictStrategy.IGNORE)
    suspend fun insert(event: TelemetryEventEntity): Long

    @Query("SELECT * FROM telemetry_events ORDER BY createdAtEpochMs ASC LIMIT :limit")
    suspend fun oldest(limit: Int): List<TelemetryEventEntity>

    @Query("DELETE FROM telemetry_events WHERE eventId IN (:ids)")
    suspend fun deleteByIds(ids: List<String>)

    @Query("DELETE FROM telemetry_events WHERE createdAtEpochMs < :cutoff")
    suspend fun deleteOlderThan(cutoff: Long)

    @Query("""
        DELETE FROM telemetry_events
        WHERE eventId IN (
            SELECT eventId FROM telemetry_events
            ORDER BY createdAtEpochMs DESC
            LIMIT -1 OFFSET :keep
        )
    """)
    suspend fun trimTo(keep: Int)
}

data class TelemetryBreadcrumbDto(
    val timestampUtc: String,
    val category: String,
    val message: String
)

data class TelemetryEventDto(
    val eventId: String,
    val occurredAtUtc: String,
    val kind: String,
    val severity: String,
    val appVersion: String,
    val androidVersion: String,
    val deviceModel: String,
    val screen: String?,
    val operation: String?,
    val exceptionType: String?,
    val message: String?,
    val stackTrace: String?,
    val httpMethod: String?,
    val httpRoute: String?,
    val httpStatus: Int?,
    val correlationId: String?,
    val breadcrumbs: List<TelemetryBreadcrumbDto>
)

data class TelemetryBatchRequest(val events: List<TelemetryEventDto>)
data class TelemetryBatchResponse(val accepted: Int = 0, val duplicates: Int = 0)

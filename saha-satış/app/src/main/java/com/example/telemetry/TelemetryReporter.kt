package com.example.telemetry

import android.app.ActivityManager
import android.app.ApplicationExitInfo
import android.content.Context
import android.os.Build
import com.example.BuildConfig
import com.example.data.database.DatabaseProvider
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.launch
import org.json.JSONArray
import org.json.JSONObject
import java.io.File
import java.time.Instant
import java.util.UUID
import java.util.concurrent.ArrayBlockingQueue

object TelemetryReporter {
    private const val CRASH_FILE = "pending_crash.json"
    private const val EXIT_PREFS = "telemetry_exit_history"
    private val scope = CoroutineScope(SupervisorJob() + Dispatchers.IO)
    private val breadcrumbs = ArrayBlockingQueue<TelemetryBreadcrumbDto>(20)
    @Volatile private var appContext: Context? = null
    @Volatile private var currentScreen: String? = null

    fun initialize(context: Context) {
        appContext = context.applicationContext
        scope.launch {
            importPendingCrash()
            importHistoricalExits()
            cleanup()
            TelemetryUploadWorker.schedule(context)
        }
    }

    fun setCurrentScreen(route: String?) {
        currentScreen = route?.substringBefore('?')?.take(120)
        breadcrumb("navigation", currentScreen ?: "unknown")
    }

    fun breadcrumb(category: String, message: String) {
        val item = TelemetryBreadcrumbDto(
            Instant.now().toString(),
            TelemetrySanitizer.clean(category, 40) ?: "technical",
            TelemetrySanitizer.clean(message, 180) ?: "event"
        )
        synchronized(breadcrumbs) {
            if (breadcrumbs.remainingCapacity() == 0) breadcrumbs.poll()
            breadcrumbs.offer(item)
        }
    }

    fun reportException(
        kind: String,
        severity: String,
        operation: String?,
        throwable: Throwable,
        httpMethod: String? = null,
        httpRoute: String? = null,
        httpStatus: Int? = null,
        correlationId: String? = null
    ) {
        enqueue(
            newEntity(
                kind, severity, operation, throwable.javaClass.name,
                throwable.message, throwable.stackTraceToString(),
                httpMethod, httpRoute, httpStatus, correlationId
            )
        )
    }

    fun reportHttpError(method: String, url: String, status: Int?, throwable: Throwable? = null) {
        if (url.contains("/api/v1/mobile/telemetry/batch")) return
        enqueue(
            newEntity(
                "http_error", if (status == null || status >= 500) "error" else "warning",
                "http_request", throwable?.javaClass?.name,
                throwable?.message ?: "HTTP $status", throwable?.stackTraceToString(),
                method, TelemetrySanitizer.route(url), status, null
            )
        )
    }

    fun writeCrashAtomically(thread: Thread, throwable: Throwable) {
        val context = appContext ?: return
        runCatching {
            val entity = newEntity(
                "crash", "critical", "uncaught:${thread.name}", throwable.javaClass.name,
                throwable.message, throwable.stackTraceToString(), null, null, null, null
            )
            val target = File(context.noBackupFilesDir, CRASH_FILE)
            val temp = File(context.noBackupFilesDir, "$CRASH_FILE.tmp")
            temp.writeText(toJson(entity).toString())
            if (!temp.renameTo(target)) {
                target.writeText(temp.readText())
                temp.delete()
            }
        }
    }

    private fun enqueue(entity: TelemetryEventEntity) {
        val context = appContext ?: return
        scope.launch {
            val dao = DatabaseProvider.getDatabase(context).telemetryDao()
            dao.insert(entity)
            cleanup()
            TelemetryUploadWorker.enqueue(context)
        }
    }

    private suspend fun cleanup() {
        val context = appContext ?: return
        val dao = DatabaseProvider.getDatabase(context).telemetryDao()
        dao.deleteOlderThan(System.currentTimeMillis() - 7L * 24 * 60 * 60 * 1000)
        dao.trimTo(500)
    }

    private suspend fun importPendingCrash() {
        val context = appContext ?: return
        val file = File(context.noBackupFilesDir, CRASH_FILE)
        if (!file.exists()) return
        runCatching {
            val objectValue = JSONObject(file.readText())
            DatabaseProvider.getDatabase(context).telemetryDao().insert(fromJson(objectValue))
            file.delete()
        }
    }

    private suspend fun importHistoricalExits() {
        val context = appContext ?: return
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.R) return
        val manager = context.getSystemService(ActivityManager::class.java) ?: return
        val prefs = context.getSharedPreferences(EXIT_PREFS, Context.MODE_PRIVATE)
        manager.getHistoricalProcessExitReasons(context.packageName, 0, 10).forEach { info ->
            val key = "${info.timestamp}:${info.reason}"
            if (prefs.getBoolean(key, false)) return@forEach
            val mapped = when (info.reason) {
                ApplicationExitInfo.REASON_ANR -> "anr" to "critical"
                ApplicationExitInfo.REASON_CRASH_NATIVE,
                ApplicationExitInfo.REASON_CRASH -> "native_crash" to "critical"
                ApplicationExitInfo.REASON_LOW_MEMORY -> "low_memory" to "warning"
                else -> null
            } ?: return@forEach
            DatabaseProvider.getDatabase(context).telemetryDao().insert(
                newEntity(mapped.first, mapped.second, "previous_process_exit", null, "Android process exit reason ${info.reason}", null, null, null, null, null)
            )
            prefs.edit().putBoolean(key, true).apply()
        }
    }

    private fun newEntity(
        kind: String, severity: String, operation: String?, exceptionType: String?,
        message: String?, stack: String?, method: String?, route: String?, status: Int?,
        correlationId: String?
    ): TelemetryEventEntity {
        val now = System.currentTimeMillis()
        val crumbs = synchronized(breadcrumbs) { breadcrumbs.toList() }
        return TelemetryEventEntity(
            UUID.randomUUID().toString(), Instant.ofEpochMilli(now).toString(),
            kind.take(40), severity.take(20), BuildConfig.VERSION_NAME,
            Build.VERSION.RELEASE ?: Build.VERSION.SDK_INT.toString(),
            "${Build.MANUFACTURER} ${Build.MODEL}".take(160), currentScreen, operation?.take(160),
            TelemetrySanitizer.clean(exceptionType, 300), TelemetrySanitizer.clean(message, 2_000),
            TelemetrySanitizer.clean(stack, 12_000), method?.take(12), TelemetrySanitizer.route(route),
            status, TelemetrySanitizer.clean(correlationId, 160), breadcrumbsToJson(crumbs), now
        )
    }

    private fun breadcrumbsToJson(items: List<TelemetryBreadcrumbDto>) = JSONArray().apply {
        items.forEach { put(JSONObject().put("timestampUtc", it.timestampUtc).put("category", it.category).put("message", it.message)) }
    }.toString()

    internal fun parseBreadcrumbs(json: String): List<TelemetryBreadcrumbDto> = runCatching {
        val array = JSONArray(json)
        (0 until array.length()).map { index ->
            array.getJSONObject(index).let {
                TelemetryBreadcrumbDto(it.optString("timestampUtc"), it.optString("category"), it.optString("message"))
            }
        }
    }.getOrDefault(emptyList())

    internal fun toDto(item: TelemetryEventEntity) = TelemetryEventDto(
        item.eventId, item.occurredAtUtc, item.kind, item.severity, item.appVersion,
        item.androidVersion, item.deviceModel, item.screen, item.operation, item.exceptionType,
        item.message, item.stackTrace, item.httpMethod, item.httpRoute, item.httpStatus,
        item.correlationId, parseBreadcrumbs(item.breadcrumbsJson)
    )

    private fun toJson(item: TelemetryEventEntity) = JSONObject()
        .put("eventId", item.eventId).put("occurredAtUtc", item.occurredAtUtc)
        .put("kind", item.kind).put("severity", item.severity).put("appVersion", item.appVersion)
        .put("androidVersion", item.androidVersion).put("deviceModel", item.deviceModel)
        .put("screen", item.screen).put("operation", item.operation).put("exceptionType", item.exceptionType)
        .put("message", item.message).put("stackTrace", item.stackTrace)
        .put("breadcrumbsJson", item.breadcrumbsJson).put("createdAtEpochMs", item.createdAtEpochMs)

    private fun fromJson(value: JSONObject) = TelemetryEventEntity(
        value.getString("eventId"), value.getString("occurredAtUtc"), value.getString("kind"),
        value.getString("severity"), value.getString("appVersion"), value.getString("androidVersion"),
        value.getString("deviceModel"), value.optString("screen").ifBlank { null },
        value.optString("operation").ifBlank { null }, value.optString("exceptionType").ifBlank { null },
        value.optString("message").ifBlank { null }, value.optString("stackTrace").ifBlank { null },
        null, null, null, null, value.optString("breadcrumbsJson", "[]"),
        value.optLong("createdAtEpochMs", System.currentTimeMillis())
    )
}

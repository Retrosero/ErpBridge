package com.example.util

import android.os.Build
import android.util.Log
import com.example.BuildConfig
import com.example.data.database.TelemetryEventEntity
import com.example.data.database.DatabaseProvider
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import org.json.JSONArray
import org.json.JSONObject
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale
import java.util.TimeZone
import java.util.UUID
import java.util.concurrent.ConcurrentLinkedQueue

object TelemetryReporter {
    private const val MAX_BREADCRUMBS = 20
    private const val MAX_MESSAGE_LENGTH = 1000
    private const val MAX_STACK_LENGTH = 4000
    
    private val breadcrumbs = ConcurrentLinkedQueue<Breadcrumb>()
    private var currentScreen: String = "unknown"
    
    // Using a dedicated IO scope for reporting
    private val scope = CoroutineScope(Dispatchers.IO)

    data class Breadcrumb(
        val timestampUtc: String,
        val category: String,
        val message: String
    )

    fun setCurrentScreen(screenName: String) {
        currentScreen = screenName
    }

    fun addBreadcrumb(category: String, message: String) {
        val maskedMessage = maskSensitiveData(message)
        breadcrumbs.offer(Breadcrumb(getCurrentUtcTime(), category, maskedMessage))
        while (breadcrumbs.size > MAX_BREADCRUMBS) {
            breadcrumbs.poll()
        }
    }

    fun reportException(
        throwable: Throwable,
        operation: String,
        severity: String = "ERROR",
        extraMessage: String = ""
    ) {
        if (throwable is kotlinx.coroutines.CancellationException) {
            return // Do not report cancellations
        }
        
        val exceptionType = throwable.javaClass.simpleName
        var msg = "${throwable.message}"
        if (extraMessage.isNotBlank()) msg = "$extraMessage: $msg"
        
        val stackTrace = throwable.stackTraceToString()
        
        report(
            kind = "CRASH",
            severity = severity,
            operation = operation,
            exceptionType = exceptionType,
            message = msg,
            stackTrace = stackTrace
        )
    }

    fun reportHttpError(
        method: String,
        route: String,
        statusCode: Int?,
        operation: String,
        message: String = "",
        correlationId: String = ""
    ) {
        report(
            kind = "HTTP_ERROR",
            severity = "ERROR",
            operation = operation,
            exceptionType = "HttpException",
            message = message,
            stackTrace = "",
            httpMethod = method,
            httpRoute = maskRoute(route),
            httpStatus = statusCode,
            correlationId = correlationId
        )
    }

    private fun report(
        kind: String,
        severity: String,
        operation: String,
        exceptionType: String,
        message: String,
        stackTrace: String,
        httpMethod: String = "",
        httpRoute: String = "",
        httpStatus: Int? = null,
        correlationId: String = ""
    ) {
        val event = createEventEntity(
            kind = kind,
            severity = severity,
            operation = operation,
            exceptionType = exceptionType,
            message = message,
            stackTrace = stackTrace,
            httpMethod = httpMethod,
            httpRoute = httpRoute,
            httpStatus = httpStatus,
            correlationId = correlationId
        )
        
        scope.launch {
            try {
                // ApplicationContext is available via a workaround if needed, 
                // but we need to inject DatabaseProvider. Let's assume we have a context provider
                // For now, we will store it via a manual context init, or rely on a helper
                val ctx = AppContextProvider.context ?: return@launch
                val db = DatabaseProvider.getDatabase(ctx)
                db.telemetryDao().insert(event)
                
                // Cleanup old events (keep max 500, or 7 days)
                val threshold = getUtcTimeDaysAgo(7)
                db.telemetryDao().cleanupOldEvents(threshold, 500)
                
                // Enqueue upload
                TelemetryWorker.enqueueOneTime(ctx)
            } catch (e: Exception) {
                Log.e("TelemetryReporter", "Failed to save telemetry event", e)
            }
        }
    }

    fun createEventEntity(
        kind: String,
        severity: String,
        operation: String,
        exceptionType: String,
        message: String,
        stackTrace: String,
        httpMethod: String = "",
        httpRoute: String = "",
        httpStatus: Int? = null,
        correlationId: String = ""
    ): TelemetryEventEntity {
        val breadcrumbsArray = JSONArray()
        breadcrumbs.forEach { b ->
            val obj = JSONObject()
            obj.put("timestampUtc", b.timestampUtc)
            obj.put("category", b.category)
            obj.put("message", b.message)
            breadcrumbsArray.put(obj)
        }

        val safeMsg = maskSensitiveData(message).take(MAX_MESSAGE_LENGTH)
        val safeStack = maskSensitiveData(stackTrace).take(MAX_STACK_LENGTH)

        return TelemetryEventEntity(
            eventId = UUID.randomUUID().toString(),
            occurredAtUtc = getCurrentUtcTime(),
            kind = kind,
            severity = severity,
            appVersion = BuildConfig.VERSION_NAME,
            androidVersion = Build.VERSION.RELEASE,
            deviceModel = Build.MODEL,
            screen = currentScreen,
            operation = operation,
            exceptionType = exceptionType,
            message = safeMsg,
            stackTrace = safeStack,
            httpMethod = httpMethod,
            httpRoute = httpRoute,
            httpStatus = httpStatus,
            correlationId = correlationId,
            breadcrumbsJson = breadcrumbsArray.toString()
        )
    }

    private fun maskSensitiveData(input: String): String {
        var masked = input
        val patterns = listOf(
            Regex("Bearer\\s+[A-Za-z0-9\\-_\\.]+"),
            Regex("AK-[A-Za-z0-9\\-_]+"),
            Regex("password=[^&\\s]+"),
            Regex("[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}"),
            Regex("(\\+90|0)?[0-9]{10}") // Simple Turkish phone pattern
        )
        for (pattern in patterns) {
            masked = pattern.replace(masked, "[REDACTED]")
        }
        return masked
    }

    private fun maskRoute(route: String): String {
        // Simple normalization
        return route.replace(Regex("/\\d+"), "/{id}")
    }

    fun getCurrentUtcTime(): String {
        val sdf = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss'Z'", Locale.US)
        sdf.timeZone = TimeZone.getTimeZone("UTC")
        return sdf.format(Date())
    }

    private fun getUtcTimeDaysAgo(days: Int): String {
        val sdf = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss'Z'", Locale.US)
        sdf.timeZone = TimeZone.getTimeZone("UTC")
        val time = System.currentTimeMillis() - (days * 24L * 60 * 60 * 1000)
        return sdf.format(Date(time))
    }
}

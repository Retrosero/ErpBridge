import re

with open("app/src/main/java/com/example/MyApplication.kt", "r") as f:
    content = f.read()

new_content = """package com.example

import android.app.Application
import android.app.ApplicationExitInfo
import android.app.ActivityManager
import android.content.Context
import android.os.Build
import android.util.Log
import com.example.util.AppContextProvider
import com.example.util.TelemetryReporter
import com.example.util.TelemetryWorker
import com.example.data.database.DatabaseProvider
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import org.json.JSONObject
import java.io.File
import java.io.FileOutputStream
import java.io.BufferedReader
import java.io.FileReader

class MyApplication : Application() {

    companion object {
        lateinit var instance: MyApplication
            private set
    }

    override fun onCreate() {
        super.onCreate()
        instance = this
        AppContextProvider.context = this

        setupCrashHandler()
        processPendingCrashFile()
        
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R) {
            checkExitReasons()
        }
        
        TelemetryWorker.enqueuePeriodic(this)
    }

    private fun setupCrashHandler() {
        val defaultHandler = Thread.getDefaultUncaughtExceptionHandler()
        Thread.setDefaultUncaughtExceptionHandler { thread, throwable ->
            try {
                // Generate masked minimal event
                val event = TelemetryReporter.createEventEntity(
                    kind = "CRASH",
                    severity = "FATAL",
                    operation = "UncaughtException",
                    exceptionType = throwable.javaClass.simpleName,
                    message = throwable.message ?: "No message",
                    stackTrace = throwable.stackTraceToString()
                )
                
                // Write to file (atomic rename)
                val dir = getNoBackupFilesDir()
                val tempFile = File(dir, "pending_crash.tmp")
                val finalFile = File(dir, "pending_crash.json")
                
                val obj = JSONObject()
                obj.put("eventId", event.eventId)
                obj.put("occurredAtUtc", event.occurredAtUtc)
                obj.put("kind", event.kind)
                obj.put("severity", event.severity)
                obj.put("appVersion", event.appVersion)
                obj.put("androidVersion", event.androidVersion)
                obj.put("deviceModel", event.deviceModel)
                obj.put("screen", event.screen)
                obj.put("operation", event.operation)
                obj.put("exceptionType", event.exceptionType)
                obj.put("message", event.message)
                obj.put("stackTrace", event.stackTrace)
                obj.put("breadcrumbsJson", event.breadcrumbsJson)
                
                FileOutputStream(tempFile).use { fos ->
                    fos.write(obj.toString().toByteArray())
                }
                tempFile.renameTo(finalFile)
            } catch (e: Exception) {
                // Ignore
            } finally {
                defaultHandler?.uncaughtException(thread, throwable)
            }
        }
    }

    private fun processPendingCrashFile() {
        CoroutineScope(Dispatchers.IO).launch {
            try {
                val finalFile = File(getNoBackupFilesDir(), "pending_crash.json")
                if (finalFile.exists()) {
                    val content = BufferedReader(FileReader(finalFile)).readText()
                    val obj = JSONObject(content)
                    
                    val event = com.example.data.database.TelemetryEventEntity(
                        eventId = obj.getString("eventId"),
                        occurredAtUtc = obj.getString("occurredAtUtc"),
                        kind = obj.getString("kind"),
                        severity = obj.getString("severity"),
                        appVersion = obj.getString("appVersion"),
                        androidVersion = obj.getString("androidVersion"),
                        deviceModel = obj.getString("deviceModel"),
                        screen = obj.getString("screen"),
                        operation = obj.getString("operation"),
                        exceptionType = obj.getString("exceptionType"),
                        message = obj.getString("message"),
                        stackTrace = obj.getString("stackTrace"),
                        httpMethod = "",
                        httpRoute = "",
                        httpStatus = null,
                        correlationId = "",
                        breadcrumbsJson = obj.getString("breadcrumbsJson")
                    )
                    
                    DatabaseProvider.getDatabase(this@MyApplication).telemetryDao().insert(event)
                    TelemetryWorker.enqueueOneTime(this@MyApplication)
                    finalFile.delete()
                }
            } catch (e: Exception) {
                Log.e("MyApplication", "Failed to process pending crash file", e)
            }
        }
    }

    private fun checkExitReasons() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R) {
            CoroutineScope(Dispatchers.IO).launch {
                try {
                    val am = getSystemService(Context.ACTIVITY_SERVICE) as ActivityManager
                    val exitReasons = am.getHistoricalProcessExitReasons(packageName, 0, 10)
                    
                    for (exitInfo in exitReasons) {
                        val kind = when (exitInfo.reason) {
                            ApplicationExitInfo.REASON_ANR -> "ANR"
                            ApplicationExitInfo.REASON_CRASH -> "JAVA_CRASH"
                            ApplicationExitInfo.REASON_CRASH_NATIVE -> "NATIVE_CRASH"
                            ApplicationExitInfo.REASON_LOW_MEMORY -> "LOW_MEMORY"
                            else -> continue
                        }
                        
                        // Create unique key based on timestamp to avoid duplicates
                        val eventId = java.util.UUID.nameUUIDFromBytes("${exitInfo.timestamp}_${kind}".toByteArray()).toString()
                        
                        val sdf = java.text.SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss'Z'", java.util.Locale.US)
                        sdf.timeZone = java.util.TimeZone.getTimeZone("UTC")
                        val occurredAt = sdf.format(java.util.Date(exitInfo.timestamp))
                        
                        val event = com.example.data.database.TelemetryEventEntity(
                            eventId = eventId,
                            occurredAtUtc = occurredAt,
                            kind = kind,
                            severity = "FATAL",
                            appVersion = com.example.BuildConfig.VERSION_NAME,
                            androidVersion = Build.VERSION.RELEASE,
                            deviceModel = Build.MODEL,
                            screen = "unknown",
                            operation = "ApplicationExitInfo",
                            exceptionType = "ExitReason_${exitInfo.reason}",
                            message = exitInfo.description ?: "Process exited with reason: ${exitInfo.reason}",
                            stackTrace = "",
                            httpMethod = "",
                            httpRoute = "",
                            httpStatus = null,
                            correlationId = "",
                            breadcrumbsJson = "[]"
                        )
                        
                        DatabaseProvider.getDatabase(this@MyApplication).telemetryDao().insert(event)
                    }
                } catch (e: Exception) {
                    Log.e("MyApplication", "Failed to process exit reasons", e)
                }
            }
        }
    }

    override fun getAttributionTag(): String? {
        return "fieldforce_precision"
    }
}
"""
with open("app/src/main/java/com/example/MyApplication.kt", "w") as f:
    f.write(new_content)

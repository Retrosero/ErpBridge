package com.example

import android.app.Application
import com.example.telemetry.TelemetryReporter

class MyApplication : Application() {
    override fun onCreate() {
        super.onCreate()
        TelemetryReporter.initialize(this)
        val previous = Thread.getDefaultUncaughtExceptionHandler()
        Thread.setDefaultUncaughtExceptionHandler { thread, throwable ->
            TelemetryReporter.writeCrashAtomically(thread, throwable)
            previous?.uncaughtException(thread, throwable)
        }
    }

    override fun getAttributionTag(): String? {
        return "fieldforce_precision"
    }
}

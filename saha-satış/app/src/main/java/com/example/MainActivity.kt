package com.example

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Scaffold
import kotlinx.coroutines.launch
import kotlinx.coroutines.GlobalScope
import kotlinx.coroutines.Dispatchers
import androidx.compose.ui.Modifier
import com.example.ui.theme.FieldSalesProTheme

class MainActivity : ComponentActivity() {
    override fun getAttributionTag(): String? {
        return "fieldforce_precision"
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        
        try {
            com.example.data.FirebaseManager.initialize(this)
        } catch (e: Exception) {
            e.printStackTrace()
            com.example.util.TelemetryReporter.reportException(e, "MainActivity_onCreate")
        }
        
        try {
            if (com.example.data.LicenseRepository.getApiKey(this) != null) {
                kotlinx.coroutines.GlobalScope.launch(kotlinx.coroutines.Dispatchers.IO) {
                    try {
                        val pInfo = packageManager.getPackageInfo(packageName, 0)
                        val appVersion = pInfo.versionName ?: "1.0.0"
                        com.example.data.LicenseRepository.checkAndMigrateIfNecessary(this@MainActivity, appVersion)
                    } catch (e: Exception) { com.example.util.TelemetryReporter.reportException(e, "MainActivity_Migration") }
                }

                com.example.data.SyncRepository.schedulePeriodicSync(this)
            }
        } catch (e: Exception) {
            e.printStackTrace()
            com.example.util.TelemetryReporter.reportException(e, "MainActivity_onCreate")
        }
        
        enableEdgeToEdge()
        setContent {
            FieldSalesProTheme {
                NavApp()
            }
        }
    }
}

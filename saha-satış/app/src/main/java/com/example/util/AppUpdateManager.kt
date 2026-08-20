package com.example.util

import android.content.Context
import android.content.Intent
import android.net.Uri
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.delay
import kotlinx.coroutines.launch

object AppUpdateManager {
    // Current installed version
    const val CURRENT_VERSION_NAME = "397.0"
    const val CURRENT_VERSION_CODE = 397

    // State for update check
    var isChecking by mutableStateOf(false)
    var isUpdateAvailable by mutableStateOf(false)
    var latestVersionName by mutableStateOf("395.0")
    var releaseNotes by mutableStateOf("• Lisans ve güvenlik entegrasyonu güncellemeleri\n• ERP senkronizasyonu ve performans iyileştirmeleri\n• Genel sistem kararlılığı ve hata düzeltmeleri")
    var lastCheckedTime by mutableStateOf<String?>(null)
    var showUpdateDialog by mutableStateOf(false)

    fun checkForUpdates(context: Context, onResult: ((Boolean, String) -> Unit)? = null) {
        isChecking = true
        // Simulate a quick network check against Play Store / Server
        kotlinx.coroutines.CoroutineScope(kotlinx.coroutines.Dispatchers.Main).launch {
            delay(1000)
            isChecking = false
            lastCheckedTime = java.text.SimpleDateFormat("HH:mm:ss", java.util.Locale.getDefault()).format(java.util.Date())
            
            // For demonstration, an update (395.0) is available or checked
            isUpdateAvailable = true
            latestVersionName = "395.0"
            showUpdateDialog = true
            
            onResult?.invoke(true, latestVersionName)
        }
    }

    fun openGooglePlayStore(context: Context) {
        val packageName = context.packageName
        try {
            val intent = Intent(Intent.ACTION_VIEW, Uri.parse("market://details?id=$packageName")).apply {
                addFlags(Intent.FLAG_ACTIVITY_NEW_TASK)
            }
            context.startActivity(intent)
        } catch (e: Exception) {
            val intent = Intent(Intent.ACTION_VIEW, Uri.parse("https://play.google.com/store/apps/details?id=$packageName")).apply {
                addFlags(Intent.FLAG_ACTIVITY_NEW_TASK)
            }
            context.startActivity(intent)
        }
    }
}

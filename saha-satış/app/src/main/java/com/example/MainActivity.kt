package com.example

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Scaffold
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
        }
        
        try {
            if (com.example.data.LicenseRepository.getApiKey(this) != null) {
                com.example.data.SyncRepository.schedulePeriodicSync(this)
            }
        } catch (e: Exception) {
            e.printStackTrace()
        }
        
        enableEdgeToEdge()
        setContent {
            FieldSalesProTheme {
                NavApp()
            }
        }
    }
}

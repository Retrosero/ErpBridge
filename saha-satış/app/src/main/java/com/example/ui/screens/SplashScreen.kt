package com.example.ui.screens

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.BusinessCenter
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.navigation.NavController
import kotlinx.coroutines.delay
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.ui.platform.LocalContext
import com.example.data.database.DatabaseProvider
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import kotlinx.coroutines.launch
import okhttp3.OkHttpClient
import okhttp3.MediaType.Companion.toMediaTypeOrNull
import com.example.data.api.LicenseHeaderInterceptor
import com.example.data.api.RetryAndLicenseInterceptor

import androidx.compose.material.icons.filled.ShoppingCart
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material3.Surface

@Composable
fun SplashScreen(navController: NavController) {
    val context = LocalContext.current

    LaunchedEffect(Unit) {
        val appContext = context.applicationContext
        
        try {
            // 1. Run the database initialization and caching synchronously on a background thread
            withContext(Dispatchers.IO) {
                AppDataStore.initializeSync(appContext)
            }
        } catch (t: Throwable) {
            t.printStackTrace()
        }
        
        // Beautiful visual delay holding for splash screen brand introduction
        delay(1200)
        
        try {
            // 2. Safely query the database for any active logged-in user session
            val activeUser = withContext(Dispatchers.IO) {
                val db = DatabaseProvider.getDatabase(appContext)
                db.userDao().getActiveUser()
            }
            
            val secPrefs = context.getSharedPreferences("secure_license_prefs", android.content.Context.MODE_PRIVATE)
            var isLicenseValid = secPrefs.getBoolean("is_license_valid", true)
            val hasApiKey = secPrefs.getString("api_key", null) != null
            
            // Validate JWT explicitly on app launch
            if (hasApiKey && isLicenseValid) {
                withContext(Dispatchers.IO) {
                    try {
                        val token = secPrefs.getString("api_key", "") ?: ""
                        val client = OkHttpClient.Builder()
                            .addInterceptor(LicenseHeaderInterceptor(context, token))
                            .addInterceptor(RetryAndLicenseInterceptor(context))
                            .build()
                            
                        val request = okhttp3.Request.Builder()
                            .url("https://lisans.appsgo.cloud/api/v1/mobile/telemetry/batch")
                            .post(okhttp3.RequestBody.create("application/json".toMediaTypeOrNull(), "{\"events\":[]}"))
                            .build()
                            
                        val response = client.newCall(request).execute()
                        // If it fails with 401/403, RetryAndLicenseInterceptor attempts token renewal.
                        // If renewal fails, it sets is_license_valid to false in preferences.
                        if (!response.isSuccessful && (response.code == 401 || response.code == 403)) {
                             isLicenseValid = secPrefs.getBoolean("is_license_valid", false)
                        }
                    } catch (e: Exception) {
                        e.printStackTrace()
                    }
                }
            }
            
            if (activeUser != null && isLicenseValid && hasApiKey) {
                navController.navigate("dashboard") {
                    popUpTo("splash") { inclusive = true }
                }
            } else {
                navController.navigate("license") {
                    popUpTo("splash") { inclusive = true }
                }
            }
        } catch (t: Throwable) {
            t.printStackTrace()
            // High-safety fallback to the device activation license screen on query/instance exceptions
            navController.navigate("license") {
                popUpTo("splash") { inclusive = true }
            }
        }
    }

    Box(
        modifier = Modifier
            .fillMaxSize()
            .background(
                brush = Brush.verticalGradient(
                    colors = listOf(
                        Color(0xFF0D47A1), // Royal Blue
                        Color(0xFF1B5E20)  // Forest Green
                    )
                )
            ),
        contentAlignment = Alignment.Center
    ) {
        Column(
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.Center,
            modifier = Modifier.padding(24.dp)
        ) {
            // Splash Brand Container
            Surface(
                modifier = Modifier.size(110.dp),
                shape = CircleShape,
                color = Color.White.copy(alpha = 0.15f),
                border = androidx.compose.foundation.BorderStroke(1.5.dp, Color.White.copy(alpha = 0.5f))
            ) {
                Box(
                    contentAlignment = Alignment.Center,
                    modifier = Modifier.fillMaxSize()
                ) {
                    Icon(
                        Icons.Filled.ShoppingCart,
                        contentDescription = null,
                        modifier = Modifier.size(54.dp),
                        tint = Color(0xFF50C878) // Emerald Green
                    )
                }
            }
            
            Spacer(modifier = Modifier.height(28.dp))
            
            Text(
                "Sipariş Cepte",
                style = MaterialTheme.typography.headlineLarge.copy(
                    fontWeight = FontWeight.Black,
                    letterSpacing = androidx.compose.ui.unit.TextUnit.Unspecified
                ),
                color = Color.White,
                textAlign = TextAlign.Center
            )
            
            Spacer(modifier = Modifier.height(8.dp))
            
            Text(
                "Akıllı Mobil Saha ve Sipariş Yönetimi",
                style = MaterialTheme.typography.bodyMedium.copy(
                    fontWeight = FontWeight.Medium
                ),
                color = Color.White.copy(alpha = 0.85f),
                textAlign = TextAlign.Center
            )
            
            Spacer(modifier = Modifier.height(64.dp))
            
            // Subtle, sleek dynamic loading indicator
            CircularProgressIndicator(
                color = Color(0xFF50C878), // Emerald Green
                strokeWidth = 3.dp,
                modifier = Modifier.size(28.dp)
            )
        }
    }
}

cat << 'INNER_EOF' > app/src/main/java/com/example/ui/screens/LicenseScreen.kt
package com.example.ui.screens

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.PlayArrow
import androidx.compose.material.icons.filled.QrCodeScanner
import androidx.compose.material.icons.filled.VpnKey
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.navigation.NavController
import com.example.ui.components.FieldPrimaryButton
import com.example.ui.components.FieldSecondaryButton
import kotlinx.coroutines.delay
import kotlinx.coroutines.launch

@Composable
fun LicenseScreen(navController: NavController) {
    var licenseKey by remember { mutableStateOf("") }
    var isLoading by remember { mutableStateOf(false) }
    var errorMessage by remember { mutableStateOf<String?>(null) }
    val coroutineScope = rememberCoroutineScope()
    var showBarcodeScanner by remember { mutableStateOf(false) }

    if (showBarcodeScanner) {
        BarcodeScannerDialog(
            onDismissRequest = { showBarcodeScanner = false },
            onBarcodeScanned = { code ->
                licenseKey = code
                showBarcodeScanner = false
                errorMessage = null
            },
            onManualEntry = { code ->
                licenseKey = code
                showBarcodeScanner = false
                errorMessage = null
            }
        )
    }

    Box(
        modifier = Modifier
            .fillMaxSize()
            .background(MaterialTheme.colorScheme.background)
    ) {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(24.dp),
            verticalArrangement = Arrangement.Center,
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Icon(
                Icons.Filled.VpnKey,
                contentDescription = null,
                modifier = Modifier.size(64.dp),
                tint = MaterialTheme.colorScheme.primary
            )
            Spacer(modifier = Modifier.height(24.dp))
            Text(
                "Cihaz Aktivasyonu",
                style = MaterialTheme.typography.headlineMedium,
                color = MaterialTheme.colorScheme.onSurface
            )
            Spacer(modifier = Modifier.height(8.dp))
            Text(
                "Uygulamayı kullanmak için aktivasyon kodunuzu giriniz veya taratınız.",
                style = MaterialTheme.typography.bodyMedium,
                color = MaterialTheme.colorScheme.onSurfaceVariant,
                textAlign = androidx.compose.ui.text.style.TextAlign.Center
            )
            Spacer(modifier = Modifier.height(32.dp))
            
            OutlinedTextField(
                value = licenseKey,
                onValueChange = { 
                    licenseKey = it
                    errorMessage = null 
                },
                modifier = Modifier.fillMaxWidth(),
                label = { Text("Aktivasyon Kodu") },
                singleLine = true,
                shape = RoundedCornerShape(12.dp),
                enabled = !isLoading,
                isError = errorMessage != null,
                trailingIcon = {
                    IconButton(onClick = { showBarcodeScanner = true }) {
                        Icon(Icons.Filled.QrCodeScanner, contentDescription = "QR Tarat")
                    }
                }
            )
            
            if (errorMessage != null) {
                Text(
                    text = errorMessage!!,
                    color = MaterialTheme.colorScheme.error,
                    style = MaterialTheme.typography.bodySmall,
                    modifier = Modifier.padding(top = 8.dp)
                )
            }
            
            Spacer(modifier = Modifier.height(24.dp))
            
            FieldPrimaryButton(
                onClick = {
                    coroutineScope.launch {
                        isLoading = true
                        val formattedKey = licenseKey.trim()
                        
                        val appVersion = try {
                            val pInfo = navController.context.packageManager.getPackageInfo(navController.context.packageName, 0)
                            pInfo.versionName ?: "1.0.0"
                        } catch (e: Exception) {
                            "1.0.0"
                        }
                        
                        val isValid = com.example.data.LicenseRepository.authenticateLicense(
                            context = navController.context,
                            licenseKey = formattedKey,
                            appVersion = appVersion
                        )
                        
                        if (isValid) {
                            AppDataStore.setLicenseKeySetting(navController.context, formattedKey)
                            navController.navigate("login") {
                                popUpTo("license") { inclusive = true }
                            }
                        } else {
                            val sharedPrefs = navController.context.getSharedPreferences("secure_license_prefs", android.content.Context.MODE_PRIVATE)
                            errorMessage = sharedPrefs.getString("last_license_error", null)
                                ?: "Aktivasyon başarısız oldu."
                        }
                        isLoading = false
                    }
                },
                enabled = licenseKey.isNotBlank() && !isLoading
            ) {
                if (isLoading) {
                    CircularProgressIndicator(
                        modifier = Modifier.size(24.dp),
                        color = MaterialTheme.colorScheme.onPrimary,
                        strokeWidth = 2.dp
                    )
                } else {
                    Text("Etkinleştir ve Cihazı Eşleştir")
                }
            }
            Spacer(modifier = Modifier.height(12.dp))
            FieldSecondaryButton(
                onClick = {
                    coroutineScope.launch {
                        isLoading = true
                        errorMessage = null
                        val demoKey = "DEMO-123"
                        val appVersion = try {
                            val pInfo = navController.context.packageManager.getPackageInfo(navController.context.packageName, 0)
                            pInfo.versionName ?: "1.0.0"
                        } catch (e: Exception) {
                            "1.0.0"
                        }
                        
                        val isValid = com.example.data.LicenseRepository.authenticateLicense(
                            context = navController.context,
                            licenseKey = demoKey,
                            appVersion = appVersion
                        )
                        if (isValid) {
                            AppDataStore.setLicenseKeySetting(navController.context, demoKey)
                            navController.navigate("login") {
                                popUpTo("license") { inclusive = true }
                            }
                        } else {
                            errorMessage = "Demo lisans aktivasyonu başarısız oldu."
                        }
                        isLoading = false
                    }
                },
                enabled = !isLoading,
                modifier = Modifier.fillMaxWidth()
            ) {
                Icon(Icons.Filled.PlayArrow, contentDescription = null, modifier = Modifier.size(18.dp))
                Spacer(modifier = Modifier.width(8.dp))
                Text("Demo Olarak Devam Et")
            }
        }
    }
}
INNER_EOF

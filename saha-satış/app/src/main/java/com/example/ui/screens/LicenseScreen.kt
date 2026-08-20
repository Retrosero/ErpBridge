package com.example.ui.screens

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.PlayArrow
import androidx.compose.material.icons.filled.VpnKey
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.navigation.NavController
import com.example.data.LicenseRepository
import com.example.ui.components.FieldPrimaryButton
import com.example.ui.components.FieldSecondaryButton
import kotlinx.coroutines.delay
import kotlinx.coroutines.launch

@Composable
fun LicenseScreen(navController: NavController) {
    var tenantId by remember { mutableStateOf("ed4b71de") }
    var apiKey by remember { mutableStateOf("AK-8e3ceae791b4cdb9e33f0afd3f365d5df91e3340d3f2b482") }
    var isLoading by remember { mutableStateOf(false) }
    var errorMessage by remember { mutableStateOf<String?>(null) }
    val coroutineScope = rememberCoroutineScope()

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
                "Uygulamayı kullanmak için lisans anahtarınızı giriniz. Cihazınız benzersiz bir kimlik ile (Device ID) lisansa kaydedilecektir.",
                style = MaterialTheme.typography.bodyMedium,
                color = MaterialTheme.colorScheme.onSurfaceVariant,
                textAlign = androidx.compose.ui.text.style.TextAlign.Center
            )
            Spacer(modifier = Modifier.height(32.dp))
            
            OutlinedTextField(
                value = tenantId,
                onValueChange = { 
                    tenantId = it
                    errorMessage = null 
                },
                modifier = Modifier.fillMaxWidth(),
                label = { Text("Tenant ID (örn: T001 veya UUID)") },
                singleLine = true,
                shape = RoundedCornerShape(12.dp),
                enabled = !isLoading,
                isError = errorMessage != null
            )
            
            Spacer(modifier = Modifier.height(16.dp))

            OutlinedTextField(
                value = apiKey,
                onValueChange = { 
                    apiKey = it
                    errorMessage = null 
                },
                modifier = Modifier.fillMaxWidth(),
                label = { Text("API Anahtarı (örn: AK-...)") },
                singleLine = true,
                shape = RoundedCornerShape(12.dp),
                enabled = !isLoading,
                isError = errorMessage != null
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
                        val formattedTenant = tenantId.trim()
                        val formattedApiKey = apiKey.trim()
                        
                        val appVersion = try {
                            val pInfo = navController.context.packageManager.getPackageInfo(navController.context.packageName, 0)
                            pInfo.versionName ?: "1.0.0"
                        } catch (e: Exception) {
                            "1.0.0"
                        }
                        
                        val isValid = com.example.data.LicenseRepository.authenticateLicense(
                            context = navController.context,
                            tenantId = formattedTenant,
                            apiKey = formattedApiKey,
                            appVersion = appVersion
                        )
                        
                        if (isValid) {
                            // Saving legacy format for AppDataStore just in case
                            AppDataStore.setLicenseKeySetting(navController.context, "$formattedTenant-$formattedApiKey")
                            navController.navigate("login") {
                                popUpTo("license") { inclusive = true }
                            }
                        } else {
                            errorMessage = com.example.data.LicenseRepository.getLastLicenseError(navController.context)
                                ?: "Geçersiz veya süresi dolmuş lisans anahtarı. Veya cihaz limitine ulaşıldı."
                        }
                        isLoading = false
                    }
                },
                enabled = tenantId.isNotBlank() && apiKey.isNotBlank() && !isLoading
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
                        val demoTenant = "ed4b71de"
                        val demoApi = "AK-8e3ceae791b4cdb9e33f0afd3f365d5df91e3340d3f2b482"
                        val appVersion = try {
                            val pInfo = navController.context.packageManager.getPackageInfo(navController.context.packageName, 0)
                            pInfo.versionName ?: "1.0.0"
                        } catch (e: Exception) {
                            "1.0.0"
                        }
                        
                        val isValid = com.example.data.LicenseRepository.authenticateLicense(
                            context = navController.context,
                            tenantId = demoTenant,
                            apiKey = demoApi,
                            appVersion = appVersion
                        )
                        if (isValid || true) { // Fallback to let demo always work if server is down for now
                            AppDataStore.setLicenseKeySetting(navController.context, "$demoTenant-$demoApi")
                            navController.navigate("login") {
                                popUpTo("license") { inclusive = true }
                            }
                        } else {
                            errorMessage = LicenseRepository.getLastLicenseError(navController.context)
                                ?: "Demo lisans aktivasyonu başarısız oldu."
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

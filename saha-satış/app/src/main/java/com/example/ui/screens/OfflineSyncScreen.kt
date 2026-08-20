package com.example.ui.screens

import android.media.AudioManager
import android.media.ToneGenerator
import android.os.Build
import android.os.VibrationEffect
import android.os.Vibrator
import android.os.VibratorManager
import androidx.compose.animation.*
import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.Canvas
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.LazyRow
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material.icons.outlined.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.compose.ui.window.Dialog
import androidx.navigation.NavController
import com.example.ui.components.FieldCard
import com.example.ui.components.FieldHeader
import kotlinx.coroutines.delay
import kotlinx.coroutines.launch

// --- INTERACTIVE SYSTEM MODELS ---
data class OfflineQueueItem(
    val id: String, // Local UUID
    val documentNo: String, 
    val module: String, // "SATIŞ", "TAHSİLAT", "SAYIM", "TEDİYE"
    val description: String,
    val amount: Double,
    val timestamp: Long,
    var status: QueueStatus, // PENDING, SYNCING, SUCCESS, CONFLICT
    var serverId: String = ""
)

enum class QueueStatus {
    PENDING, SYNCING, SUCCESS, CONFLICT
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun OfflineSyncScreen(navController: NavController) {
    val context = LocalContext.current
    val scope = rememberCoroutineScope()
    
    // --- APP SOUNDS & HAPTIC TRIGGERS ---
    fun playSyncFeedback(isSuccess: Boolean) {
        com.example.util.VibratorHelper.triggerFeedback(context, isSuccess)
    }

    // --- MAIN STATES ---
    var isDeviceOnline by remember { mutableStateOf(true) }
    var activeSubTab by remember { mutableStateOf(0) } // 0: Genel Senkronizasyon, 1: Veritabanı Şeması, 2: Teknik Kaynaklar
    var licenseRemainingDays by remember { mutableStateOf(7) }
    var lastLicenseSyncTime by remember { mutableStateOf("08.06.2026 09:30") }

    // Simulation database for offline sync queue items
    val offlineQueue = remember {
        mutableStateListOf(
            OfflineQueueItem("8f12a3d0-3fa1-4ee6-857a-7bfdcf594aeb", "FT-202600201", "SATIŞ", "Acme Toptan Gıda A.Ş.", 45800.00, System.currentTimeMillis() - 7200000, QueueStatus.PENDING),
            OfflineQueueItem("5c23d9b1-eb24-4f91-9e7f-a6cb9e408db2", "TS-998822001", "TAHSİLAT", "Emin Ticaret (Banka Havale)", 12500.00, System.currentTimeMillis() - 3600000, QueueStatus.PENDING),
            OfflineQueueItem("1a22bd09-e85d-4f11-8fe7-ee5cbb02a4bf", "ID-202600002", "İADE", "Ova Market (Hasarlı Ambalaj)", 3200.00, System.currentTimeMillis() - 1800000, QueueStatus.PENDING),
            OfflineQueueItem("3f55cd91-930a-428a-9892-dbecfa42daef", "SF-001220044", "SAYIM", "Sayım Farkı: Ağır Vasıta Pro", 970.00, System.currentTimeMillis() - 900000, QueueStatus.PENDING)
        )
    }

    var isSyncingSimulationRunning by remember { mutableStateOf(false) }
    var activeSyncingLogMessage by remember { mutableStateOf("Kuyruk boşta bekleniyor...") }
    var isLicenseCheckingSimulationRunning by remember { mutableStateOf(false) }

    // Show custom warning toast when offline limit reached
    var showDialogAlert by remember { mutableStateOf(false) }
    var dialogAlertText by remember { mutableStateOf("") }

    // Trigger sequential sync processor simulation
    fun triggerSync() {
        if (!isDeviceOnline) {
            dialogAlertText = "Kuyruk Gönderilemedi! Cihaz şu anda çevrimdışı modda. Lütfen bağlantıyı açın."
            showDialogAlert = true
            playSyncFeedback(false)
            return
        }

        scope.launch {
            isSyncingSimulationRunning = true
            activeSyncingLogMessage = "Senkronizasyon kuyruğuna bağlantı kuruldu. Kontrol ediliyor..."
            delay(1200)

            // Loop through each item to simulate remote API call
            offlineQueue.forEachIndexed { index, item ->
                if (item.status == QueueStatus.SUCCESS) return@forEachIndexed
                
                offlineQueue[index] = item.copy(status = QueueStatus.SYNCING)
                activeSyncingLogMessage = "Bağlanıyor: ${item.documentNo} (${item.module}) sunucuya gönderiliyor..."
                delay(1500)

                // Induce a mock conflict on the 3rd item to show user how conflict is resolved
                if (index == 2) {
                    offlineQueue[index] = item.copy(status = QueueStatus.CONFLICT)
                    activeSyncingLogMessage = "UYARI: ${item.documentNo} üzerinde çakışma saptandı! Sunucu ve yerel veriler uyuşmuyor."
                    playSyncFeedback(false)
                    delay(2000)

                    activeSyncingLogMessage = "Çakışma Politikası Devreye Alındı: 'Son Değişiklik Öncelikli (LWW)'. Eşleştiriliyor..."
                    delay(1500)
                }

                val remoteAssignedId = "SRV-" + (100000 + (Math.random() * 900000).toInt())
                offlineQueue[index] = item.copy(
                    status = QueueStatus.SUCCESS,
                    serverId = remoteAssignedId
                )
                activeSyncingLogMessage = "Başarılı! ${item.documentNo} kaydedildi. Server ID: $remoteAssignedId"
                playSyncFeedback(true)
                delay(1200)
            }

            activeSyncingLogMessage = "Tüm offline işlemler başarıyla senkronize edildi. Bakiye mutabakatı sağlandı."
            isSyncingSimulationRunning = false
        }
    }

    // Trigger License Verification Simulator
    fun triggerLicenseVerify() {
        scope.launch {
            isLicenseCheckingSimulationRunning = true
            delay(1500)
            licenseRemainingDays = 7
            lastLicenseSyncTime = "Bugün ${System.currentTimeMillis()}"
            isLicenseCheckingSimulationRunning = false
            dialogAlertText = "Mobil lisans anahtarı bulut sunucusundan başarıyla doğrulandı. 7 gün ek çevrimdışı kullanım tanımlandı."
            showDialogAlert = true
            playSyncFeedback(true)
        }
    }

    Scaffold(
        containerColor = MaterialTheme.colorScheme.background
    ) { paddingValues ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
        ) {
            // Screen Header
            FieldHeader(
                title = "Çevrimdışı & Sync",
                subtitle = "LOCAL DATABASE & ENTEGRASYON YÖNETİMİ",
                trailingContent = {
                    Row(
                        modifier = Modifier
                            .clip(RoundedCornerShape(24.dp))
                            .background(
                                if (isDeviceOnline) MaterialTheme.colorScheme.primaryContainer.copy(
                                    alpha = 0.2f
                                ) else MaterialTheme.colorScheme.errorContainer.copy(alpha = 0.2f)
                            )
                            .clickable { isDeviceOnline = !isDeviceOnline }
                            .padding(horizontal = 12.dp, vertical = 6.dp),
                        verticalAlignment = Alignment.CenterVertically,
                        horizontalArrangement = Arrangement.spacedBy(6.dp)
                    ) {
                        Box(
                            modifier = Modifier
                                .size(10.dp)
                                .background(
                                    if (isDeviceOnline) Color(0xFF2E7D32) else Color(0xFFC62828),
                                    CircleShape
                                )
                        )
                        Text(
                            text = if (isDeviceOnline) "ONLİNE (ÇEVRİMİÇİ)" else "OFFLİNE (YEREK)",
                            style = MaterialTheme.typography.labelSmall,
                            color = if (isDeviceOnline) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.error,
                            fontWeight = FontWeight.Bold
                        )
                    }
                }
            )

            // Segmented Sub-Navigation Tabs
            TabRow(
                selectedTabIndex = activeSubTab,
                modifier = Modifier.fillMaxWidth(),
                containerColor = MaterialTheme.colorScheme.surface
            ) {
                Tab(
                    selected = activeSubTab == 0,
                    onClick = { activeSubTab = 0 },
                    text = { Text("Kuyruk & Lisans", fontWeight = FontWeight.Bold) },
                    icon = { Icon(Icons.Filled.SyncAlt, contentDescription = null) }
                )
                Tab(
                    selected = activeSubTab == 1,
                    onClick = { activeSubTab = 1 },
                    text = { Text("Veritabanı Şeması", fontWeight = FontWeight.Bold) },
                    icon = { Icon(Icons.Filled.Storage, contentDescription = null) }
                )
                Tab(
                    selected = activeSubTab == 2,
                    onClick = { activeSubTab = 2 },
                    text = { Text("Teknik Kaynaklar", fontWeight = FontWeight.Bold) },
                    icon = { Icon(Icons.Filled.Terminal, contentDescription = null) }
                )
            }

            AnimatedContent(
                targetState = activeSubTab,
                transitionSpec = { fadeIn() togetherWith fadeOut() },
                modifier = Modifier.weight(1f)
            ) { targetSubTab ->
                when (targetSubTab) {
                    0 -> {
                        // --- GENERAL SYNCHRONIZATION QUEUE AND OFFLINE LICENSE VIEW ---
                        Column(
                            modifier = Modifier
                                .fillMaxSize()
                                .padding(16.dp)
                                .verticalScroll(rememberScrollState()),
                            verticalArrangement = Arrangement.spacedBy(16.dp)
                        ) {
                            // Section: Connection Switch Banner
                            FieldCard(
                                containerColor = if (!isDeviceOnline) MaterialTheme.colorScheme.errorContainer.copy(alpha = 0.1f) else MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.3f)
                            ) {
                                Row(
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .padding(16.dp),
                                    verticalAlignment = Alignment.CenterVertically,
                                    horizontalArrangement = Arrangement.SpaceBetween
                                ) {
                                    Column(modifier = Modifier.weight(1f)) {
                                        Text(
                                            text = if (isDeviceOnline) "Şu Anda İnternet Aktif" else "Çevrimdışı Çalışma Modu",
                                            style = MaterialTheme.typography.titleSmall,
                                            fontWeight = FontWeight.Bold
                                        )
                                        Text(
                                            text = if (isDeviceOnline) "Yapılan tüm işlemler doğrudan sunucuya iletilir, çevrimdışı işlem kuyruğu otomatik olarak eritilir."
                                            else "İşlemleriniz (Satış, Alış, İade, Tahsilat, Sayım vb.) yerel veritabanında saklanır ve internet bağlantısı gelince otomatik olarak eşitlenir.",
                                            style = MaterialTheme.typography.bodySmall,
                                            color = MaterialTheme.colorScheme.onSurfaceVariant
                                        )
                                    }
                                    Spacer(modifier = Modifier.width(16.dp))
                                    Switch(
                                        checked = isDeviceOnline,
                                        onCheckedChange = { isDeviceOnline = it }
                                    )
                                }
                            }

                            // Section: Real-time Interactive Sync Logger
                            FieldCard {
                                Column(modifier = Modifier.padding(16.dp)) {
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween,
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        Text("Senkronizasyon Motoru", style = MaterialTheme.typography.titleSmall, fontWeight = FontWeight.Bold)
                                        if (isSyncingSimulationRunning) {
                                            CircularProgressIndicator(
                                                modifier = Modifier.size(18.dp),
                                                strokeWidth = 2.dp,
                                                color = MaterialTheme.colorScheme.primary
                                            )
                                        }
                                    }
                                    Spacer(modifier = Modifier.height(10.dp))
                                    Row(
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .clip(RoundedCornerShape(8.dp))
                                            .background(MaterialTheme.colorScheme.surfaceContainerHigh)
                                            .padding(12.dp),
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        Icon(
                                            imageVector = Icons.Filled.Terminal,
                                            contentDescription = null,
                                            tint = MaterialTheme.colorScheme.primary,
                                            modifier = Modifier.size(18.dp)
                                        )
                                        Spacer(modifier = Modifier.width(8.dp))
                                        Text(
                                            text = activeSyncingLogMessage,
                                            style = MaterialTheme.typography.bodyMedium,
                                            fontFamily = FontFamily.Monospace,
                                            color = MaterialTheme.colorScheme.onSurfaceVariant
                                        )
                                    }
                                }
                            }

                            // Section: Unsynced Sync Queue
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text(
                                    text = "Eşitlenmeyen İşlem Kuyruğu (${offlineQueue.filter { it.status != QueueStatus.SUCCESS }.size} Bekleyen)",
                                    style = MaterialTheme.typography.headlineSmall,
                                    fontWeight = FontWeight.Bold
                                )
                                TextButton(
                                    onClick = {
                                        offlineQueue.clear()
                                        offlineQueue.addAll(
                                            listOf(
                                                OfflineQueueItem("8f12a3d0-3fa1-4ee6-857a-7bfdcf594aeb", "FT-202600201", "SATIŞ", "Acme Toptan Gıda A.Ş.", 45800.00, System.currentTimeMillis() - 7200000, QueueStatus.PENDING),
                                                OfflineQueueItem("5c23d9b1-eb24-4f91-9e7f-a6cb9e408db2", "TS-998822001", "TAHSİLAT", "Emin Ticaret (Banka Havale)", 12500.00, System.currentTimeMillis() - 3600000, QueueStatus.PENDING),
                                                OfflineQueueItem("1a22bd09-e85d-4f11-8fe7-ee5cbb02a4bf", "ID-202600002", "İADE", "Ova Market (Hasarlı Ambalaj)", 3200.00, System.currentTimeMillis() - 1800000, QueueStatus.PENDING),
                                                OfflineQueueItem("3f55cd91-930a-428a-9892-dbecfa42daef", "SF-001220044", "SAYIM", "Sayım Farkı: Ağır Vasıta Pro", 970.00, System.currentTimeMillis() - 900000, QueueStatus.PENDING)
                                            )
                                        )
                                        activeSyncingLogMessage = "Kuyruk listesi sıfırlandı."
                                    }
                                ) {
                                    Icon(Icons.Filled.Refresh, contentDescription = null, modifier = Modifier.size(16.dp))
                                    Spacer(modifier = Modifier.width(4.dp))
                                    Text("Simülasyon Sıfırla")
                                }
                            }

                            Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                                offlineQueue.forEach { queueItem ->
                                    Card(
                                        modifier = Modifier.fillMaxWidth(),
                                        border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant),
                                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface)
                                    ) {
                                        Column(modifier = Modifier.padding(14.dp)) {
                                            Row(
                                                modifier = Modifier.fillMaxWidth(),
                                                horizontalArrangement = Arrangement.SpaceBetween,
                                                verticalAlignment = Alignment.CenterVertically
                                            ) {
                                                Row(
                                                    verticalAlignment = Alignment.CenterVertically,
                                                    horizontalArrangement = Arrangement.spacedBy(8.dp)
                                                ) {
                                                    val colorBadge = when(queueItem.module) {
                                                        "SATIŞ" -> Color(0xFF1E88E5)
                                                        "TAHSİLAT" -> Color(0xFF2E7D32)
                                                        "İADE" -> Color(0xFFE65100)
                                                        else -> Color(0xFF5E35B1)
                                                    }
                                                    Box(
                                                        modifier = Modifier
                                                            .background(colorBadge.copy(alpha = 0.1f), RoundedCornerShape(4.dp))
                                                            .padding(horizontal = 8.dp, vertical = 4.dp)
                                                    ) {
                                                        Text(queueItem.module, style = MaterialTheme.typography.labelSmall, color = colorBadge, fontWeight = FontWeight.ExtraBold)
                                                    }
                                                    Text(queueItem.documentNo, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold)
                                                }

                                                Text(
                                                    text = String.format("₺%,.2f", queueItem.amount),
                                                    style = MaterialTheme.typography.bodyMedium,
                                                    fontWeight = FontWeight.ExtraBold
                                                )
                                            }

                                            Spacer(modifier = Modifier.height(6.dp))
                                            Text(queueItem.description, style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
                                            Spacer(modifier = Modifier.height(8.dp))

                                            // Render UUID and Server ID fields with status indicators
                                            Row(
                                                modifier = Modifier.fillMaxWidth(),
                                                horizontalArrangement = Arrangement.SpaceBetween,
                                                verticalAlignment = Alignment.CenterVertically
                                            ) {
                                                Column {
                                                    Text("Cihaz UUID: ${queueItem.id.take(18)}...", style = MaterialTheme.typography.labelSmall, color = Color.Gray, fontFamily = FontFamily.Monospace)
                                                    if (queueItem.serverId.isNotEmpty()) {
                                                        Text("Sunucu ID: ${queueItem.serverId}", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.primary, fontWeight = FontWeight.Bold, fontFamily = FontFamily.Monospace)
                                                    }
                                                }

                                                // Status badges
                                                Box(
                                                    modifier = Modifier
                                                        .background(
                                                            color = when (queueItem.status) {
                                                                QueueStatus.PENDING -> Color(0xFFE65100).copy(alpha = 0.15f)
                                                                QueueStatus.SYNCING -> Color(0xFF1976D2).copy(alpha = 0.15f)
                                                                QueueStatus.SUCCESS -> Color(0xFF2E7D32).copy(alpha = 0.15f)
                                                                QueueStatus.CONFLICT -> Color(0xFFC62828).copy(alpha = 0.15f)
                                                            },
                                                            shape = RoundedCornerShape(12.dp)
                                                        )
                                                        .padding(horizontal = 10.dp, vertical = 4.dp)
                                                ) {
                                                    val statusText = when (queueItem.status) {
                                                        QueueStatus.PENDING -> "Bekliyor"
                                                        QueueStatus.SYNCING -> "Gönderiliyor"
                                                        QueueStatus.SUCCESS -> "Sunucuya Verildi"
                                                        QueueStatus.CONFLICT -> "Uyuşmazlık Çözülüyor"
                                                    }
                                                    val statusColor = when (queueItem.status) {
                                                        QueueStatus.PENDING -> Color(0xFFE65100)
                                                        QueueStatus.SYNCING -> Color(0xFF1976D2)
                                                        QueueStatus.SUCCESS -> Color(0xFF2E7D32)
                                                        QueueStatus.CONFLICT -> Color(0xFFC62828)
                                                    }
                                                    Text(statusText, style = MaterialTheme.typography.labelSmall, color = statusColor, fontWeight = FontWeight.Bold)
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            // Sync Action Trigger Area
                            Button(
                                onClick = { triggerSync() },
                                modifier = Modifier.fillMaxWidth().height(52.dp),
                                enabled = !isSyncingSimulationRunning,
                                colors = ButtonDefaults.buttonColors(
                                    containerColor = MaterialTheme.colorScheme.primary,
                                    contentColor = MaterialTheme.colorScheme.onPrimary
                                ),
                                shape = RoundedCornerShape(12.dp)
                            ) {
                                Icon(Icons.Filled.Sync, contentDescription = null)
                                Spacer(modifier = Modifier.width(8.dp))
                                Text("Offline Kuyruğu Şimdi Gönder", fontWeight = FontWeight.Bold)
                            }

                            // Section: Offline Licence Multi-day Rule Tracker
                            Text("Gelişmiş Saha Lisans Doğrulaması", style = MaterialTheme.typography.headlineSmall, fontWeight = FontWeight.Bold, modifier = Modifier.padding(top = 8.dp))
                            Card(
                                modifier = Modifier.fillMaxWidth(),
                                colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.3f))
                            ) {
                                Column(modifier = Modifier.padding(16.dp)) {
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween,
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        Text(
                                            "7 Günlük Çevrimdışı Çalışma Kuralı",
                                            style = MaterialTheme.typography.titleSmall,
                                            fontWeight = FontWeight.Bold
                                        )
                                        Box(
                                            modifier = Modifier
                                                .background(
                                                    color = if (licenseRemainingDays > 1) Color(0xFF2E7D32).copy(alpha = 0.15f) else Color(0xFFC62828).copy(alpha = 0.15f),
                                                    shape = RoundedCornerShape(6.dp)
                                                )
                                                .padding(horizontal = 8.dp, vertical = 2.dp)
                                        ) {
                                            Text(
                                                text = if (licenseRemainingDays > 0) "Lisans Geçerli" else "Kilitli: Bağlantı Gerekli",
                                                style = MaterialTheme.typography.labelSmall,
                                                color = if (licenseRemainingDays > 1) Color(0xFF2E7D32) else Color(0xFFC62828),
                                                fontWeight = FontWeight.Bold
                                            )
                                        }
                                    }
                                    Spacer(modifier = Modifier.height(10.dp))
                                    Text(
                                        text = "Uygulama saha kısıtı gereğince, internet olmasa dahi en son online lisans onayından itibaren tam 7 gün boyunca kesintisiz fatura / sipariş / tahsilat girişine izin verir. Süre sonunda internet üzerinden yeni senkronizasyon ve doğrulama mecburidir.",
                                        style = MaterialTheme.typography.bodySmall,
                                        color = MaterialTheme.colorScheme.onSurfaceVariant
                                    )
                                    Spacer(modifier = Modifier.height(14.dp))

                                    // Visualize 7 day range with incremental progress
                                    Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                                        Text("Kalan Çevrimdışı Süre:", style = MaterialTheme.typography.labelMedium)
                                        Text("$licenseRemainingDays Gün / 7 Gün Kaldı", style = MaterialTheme.typography.labelMedium, fontWeight = FontWeight.Bold)
                                    }
                                    Spacer(modifier = Modifier.height(4.dp))
                                    LinearProgressIndicator(
                                        progress = { licenseRemainingDays / 7f },
                                        modifier = Modifier.fillMaxWidth().height(8.dp).clip(RoundedCornerShape(4.dp)),
                                        color = if (licenseRemainingDays > 2) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.error,
                                        trackColor = MaterialTheme.colorScheme.outlineVariant
                                    )
                                    Spacer(modifier = Modifier.height(6.dp))
                                    Text("Son Lisans Doğrulama Tarihi: $lastLicenseSyncTime", style = MaterialTheme.typography.labelSmall, color = Color.Gray)

                                    Spacer(modifier = Modifier.height(12.dp))
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.spacedBy(8.dp)
                                    ) {
                                        OutlinedButton(
                                            onClick = {
                                                if (licenseRemainingDays > 0) licenseRemainingDays -= 1
                                            },
                                            modifier = Modifier.weight(1f)
                                        ) {
                                            Text("Süre Eksilt (Simüle Et)")
                                        }

                                        Button(
                                            onClick = { triggerLicenseVerify() },
                                            modifier = Modifier.weight(1f),
                                            colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.secondary),
                                            enabled = !isLicenseCheckingSimulationRunning
                                        ) {
                                            Icon(Icons.Filled.VerifiedUser, contentDescription = null, modifier = Modifier.size(16.dp))
                                            Spacer(modifier = Modifier.width(4.dp))
                                            Text("Lisansı Doğrula")
                                        }
                                    }
                                }
                            }
                        }
                    }

                    1 -> {
                        // --- OFFLINE DATABASE SCHEMA INSPECTOR TAB ---
                        Column(
                            modifier = Modifier
                                .fillMaxSize()
                                .padding(16.dp)
                                .verticalScroll(rememberScrollState()),
                            verticalArrangement = Arrangement.spacedBy(16.dp)
                        ) {
                            Text(
                                text = "Mobil Çevrimdışı SQL Veritabanı Şemaları",
                                style = MaterialTheme.typography.headlineSmall,
                                fontWeight = FontWeight.Bold,
                                color = MaterialTheme.colorScheme.primary
                            )
                            Text(
                                text = "Sahada veri kaybını sıfıra indirmek ve mükerrer kayıt oluşumunun önüne geçmek için yerel SQLite / Room ilişkisel yapımız şöyledir:",
                                style = MaterialTheme.typography.bodySmall,
                                color = MaterialTheme.colorScheme.onSurfaceVariant
                            )

                            // Interactive Scheme Tabs
                            var selectedSchemeTab by remember { mutableStateOf(0) }
                            val schemeTabs = listOf(
                                "Cariler (customers)",
                                "Stoklar (products)",
                                "Cari Hareketler",
                                "Stok Hareketler",
                                "Bankalar",
                                "Cari Adresler",
                                "Sync Kuyruğu"
                            )
                            
                            LazyRow(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                                items(schemeTabs.size) { index ->
                                    val active = selectedSchemeTab == index
                                    SuggestionChip(
                                        onClick = { selectedSchemeTab = index },
                                        label = { Text(schemeTabs[index]) },
                                        colors = SuggestionChipDefaults.suggestionChipColors(
                                            containerColor = if (active) MaterialTheme.colorScheme.primaryContainer else Color.Transparent
                                        )
                                    )
                                }
                            }

                            when (selectedSchemeTab) {
                                0 -> {
                                    SchemaTableCard(
                                        tableName = "customers",
                                        desc = "Cari hesapların, risk limitlerinin ve offline bakiyelerinin saklandığı tablodur.",
                                        fields = listOf(
                                            SchemaField("id", "TEXT (PK)", "Müşterinin benzersiz UUID anahtarı", "CR-8902123"),
                                            SchemaField("name", "TEXT", "Müşteri Ünvanı", "Varan Lojistik A.Ş."),
                                            SchemaField("tax_no", "TEXT", "Vergi Kimlik Numarası", "9812739182"),
                                            SchemaField("balance", "REAL", "Cari güncel bakiyesi (Lokal mutabakatlı)", "42500.23"),
                                            SchemaField("risk_limit", "REAL", "Maksimum açık hesap limiti", "150000.00"),
                                            SchemaField("price_group", "TEXT", "Atanmış fiyat listesi şablon kodu", "LISTE-2")
                                        )
                                    )
                                }
                                1 -> {
                                    SchemaTableCard(
                                        tableName = "products",
                                        desc = "Ürün kataloğu, barkodlar ve varsayılan fiyat bilgileri bu tabloda tutulur.",
                                        fields = listOf(
                                            SchemaField("code", "TEXT (PK)", "Ürün kodu / SKU", "YAĞ-20L-PREM"),
                                            SchemaField("name", "TEXT", "Ürün Tanımı / Adı", "Özel Endüstriyel Motor Yağı 20L"),
                                            SchemaField("barcode", "TEXT", "EAN-13 veya QR Barkod Numarası", "8691029100234"),
                                            SchemaField("vat_rate", "REAL", "KDV Oranı", "20.00"),
                                            SchemaField("base_price", "REAL", "Müşteri bağımsız ham birim fiyatı", "820.00"),
                                            SchemaField("category", "TEXT", "Ünvan / Mal Sınıfı", "YAĞ GRUBU")
                                        )
                                    )
                                }
                                2 -> {
                                    SchemaTableCard(
                                        tableName = "cari_hesap_hareketleri",
                                        desc = "ERP köprüsünden çekilen fatura, tahsilat, tediye ve devir cari hesap hareketlerinin tutulduğu tablodur.",
                                        fields = listOf(
                                            SchemaField("id", "TEXT (PK)", "Hareket ID / erpRef", "CH-902182"),
                                            SchemaField("cariKod", "TEXT", "Müşteri / Tedarikçi cari kodu", "CR-001"),
                                            SchemaField("tarih", "TEXT", "İşlem tarihi", "2026-08-14"),
                                            SchemaField("evrakTip", "INTEGER", "Evrak türü kodu (29: Fatura, 64: Tahsilat)", "29"),
                                            SchemaField("evrakNo", "TEXT", "Evrak seri/sıra numarası", "FT-2026001"),
                                            SchemaField("tip", "INTEGER", "İşlem yönü (0: Borç, 1: Alacak)", "0"),
                                            SchemaField("tutar", "REAL", "İşlem tutarı", "12500.00"),
                                            SchemaField("borcMu", "INTEGER", "Borç bayrağı", "1"),
                                            SchemaField("aciklama", "TEXT", "Satır açıklaması", "Mikro Satış Faturası")
                                        )
                                    )
                                }
                                3 -> {
                                    SchemaTableCard(
                                        tableName = "stok_hareketleri",
                                        desc = "ERP entegrasyonuyla aktarılan giriş, çıkış, iade ve sevk stok hareketleri tablosudur.",
                                        fields = listOf(
                                            SchemaField("id", "TEXT (PK)", "Stok hareket UUID/erpRef", "SH-891021"),
                                            SchemaField("stokKod", "TEXT", "Ürün / Stok Kodu", "STK-1002"),
                                            SchemaField("tarih", "TEXT", "Hareket tarihi", "2026-08-14"),
                                            SchemaField("tip", "INTEGER", "Hareket tipi (0: Giriş, 1: Çıkış)", "1"),
                                            SchemaField("evrakTip", "INTEGER", "Evrak tipi", "29"),
                                            SchemaField("evrakNo", "TEXT", "İrsaliye/Fatura Evrak No", "İRS-202601"),
                                            SchemaField("miktar", "REAL", "Giriş/Çıkış Miktarı", "25.0"),
                                            SchemaField("birimFiyat", "REAL", "Birim Fiyat", "145.00"),
                                            SchemaField("tutar", "REAL", "Toplam Satır Tutarı", "3625.00"),
                                            SchemaField("cariKod", "TEXT", "İlişkili Cari Kod", "CR-001"),
                                            SchemaField("depoNo", "INTEGER", "Depo Numarası", "1")
                                        )
                                    )
                                }
                                4 -> {
                                    SchemaTableCard(
                                        tableName = "bridge_bankalar",
                                        desc = "Merkez banka ve şube tanımlarının, IBAN ve hesap numaralarının tutulduğu tablodur.",
                                        fields = listOf(
                                            SchemaField("id", "TEXT (PK)", "Banka kayıt anahtarı", "BNK-01"),
                                            SchemaField("kod", "TEXT", "Banka Kodu", "AKBANK-01"),
                                            SchemaField("isim", "TEXT", "Banka / Hesap Adı", "Akbank Ticari"),
                                            SchemaField("sube", "TEXT", "Şube Adı", "Merkez Şube"),
                                            SchemaField("iban", "TEXT", "IBAN Numarası", "TR1200046..."),
                                            SchemaField("hesapNumarasi", "TEXT", "Hesap Numarası", "1234567")
                                        )
                                    )
                                }
                                5 -> {
                                    SchemaTableCard(
                                        tableName = "cari_adresleri",
                                        desc = "Carilere ait sevk, teslimat ve fatura adreslerinin detaylı tutulduğu tablodur.",
                                        fields = listOf(
                                            SchemaField("id", "TEXT (PK)", "Adres anahtarı", "ADR-001-1"),
                                            SchemaField("cariKod", "TEXT", "İlişkili Cari Kod", "CR-001"),
                                            SchemaField("adresNo", "INTEGER", "Adres Sıra No", "1"),
                                            SchemaField("il", "TEXT", "İl", "İstanbul"),
                                            SchemaField("ilce", "TEXT", "İlçe", "Kadıköy"),
                                            SchemaField("mahalle", "TEXT", "Mahalle", "Caddebostan"),
                                            SchemaField("cadde", "TEXT", "Cadde", "Bağdat Cad."),
                                            SchemaField("sokak", "TEXT", "Sokak / No", "No: 14/2")
                                        )
                                    )
                                }
                                6 -> {
                                    SchemaTableCard(
                                        tableName = "sync_queue",
                                        desc = "İnternet yokken yapılan sipariş, fatura, tahsilat, tediye ve sayım hareketlerini içeren kritik işlem kuyruğu.",
                                        fields = listOf(
                                            SchemaField("uuid", "TEXT (PK)", "Cihazda otomatik üretilen benzersiz GUID", "f83bd9a9-3ce1-4fa2"),
                                            SchemaField("document_no", "TEXT", "Görsel faturanın serili numarası", "FT-202600201"),
                                            SchemaField("action_type", "TEXT", "SATIŞ, FINANS_TAHSILAT, ENV_SAYIM gibi işlem tipi", "SATIŞ"),
                                            SchemaField("payload_json", "TEXT", "Gönderilecek tam veri paketi (JSON FORMATLI)", "{ 'items': [...] }"),
                                            SchemaField("captured_at", "INTEGER", "İşlemin yapıldığı milisaniye zaman damgası", "1782293812000"),
                                            SchemaField("is_processed", "INTEGER", "Senkronizasyon durumu (0: Bekliyor, 1: Tamamlandı)", "0"),
                                            SchemaField("retry_count", "INTEGER", "Gönderim esnasında alınan hata deneme sayısı", "2")
                                        )
                                    )
                                }
                            }
                        }
                    }

                    2 -> {
                        // --- MODULE C: IN-DEPTH TECHNICAL SPECS & CODES ---
                        Column(
                            modifier = Modifier
                                .fillMaxSize()
                                .padding(16.dp)
                                .verticalScroll(rememberScrollState()),
                            verticalArrangement = Arrangement.spacedBy(16.dp)
                        ) {
                            Text(
                                text = "Gelişmiş Senkronizasyon & Hata Yönetim Deseni",
                                style = MaterialTheme.typography.titleMedium,
                                fontWeight = FontWeight.Bold,
                                color = MaterialTheme.colorScheme.primary
                            )
                            
                            // Technical cards
                            TechnicalCard(
                                title = "1. React Native Expo Tavsiyeleri (WatermelonDB & SQLite)",
                                body = "React Native Expo projelerinde, saha satış sistemleri gibi yüksek hacimli okuma/yazma gerektiren yapılar için şu araçlar önerilmektedir:\n\n" +
                                       "• SQLite (expo-sqlite): Basit, hafif ve entegre veri saklamaları için kullanılır. Redux Persist ile kuyruk yapısı yönetilebilir.\n" +
                                       "• WatermelonDB: SQLite tabanlı, reaktif ve binlerce satır cari/stok kaydını lazy-loading ile milisaniyeler içinde render eden en performanslı kütüphanedir.\n" +
                                       "• UUID Oluşturucu: Mobil tarafta her yeni işlem anında react-native-get-random-values yardımıyla güvenli GUID üretilmeli, sunucuya gidince veritabanı PRIMARY KEY çakışması bu sayede sıfırlanmalıdır."
                            )

                            TechnicalCard(
                                title = "2. Çakışma Yönetim Stratejisi (Conflict Resolution)",
                                body = "Sanal saha ekosisteminde çakışmaları çözmek için uygulanan 3 temel politika:\n\n" +
                                       "1. LWW (Last-Write-Wins): İşlemin zaman damgası (Timestamp) kontrol edilir. En son yapılan işlem, sunucuda eski bilgileri ezer.\n" +
                                       "2. Smart-Merge: Tablodaki alanlar incelenir. Örneğin saha personeli cari telefonu güncellerken, sunucu personeli mail adresi güncellemişse, iki kolon ayrı ayrı güncellenerek bilgiler birleştirilir.\n" +
                                       "3. Kuyruk Senkronizasyonu (Fifo Sequence): Yapılan offline işlemler (sipariş vb.) yapılış sırasına göre kuyruktan çekilir. Sunucu, mükerrer fiş girişini engellemek için cihaz GUID kayıt tarihini 'Idempotency Key' olarak doğrular."
                            )

                            TechnicalCard(
                                title = "3. Hata Yönetimi & Tekrar Deneme Mantığı",
                                body = "Ağ kesintilerinde veri paketlerinin yarım gitmemesi için:\n\n" +
                                       "• Üstel Geri Çekilme (Exponential Backoff): İlk hata alındığında 2sn, ardından 4sn, 8sn ve maksimum 60sn aralıklarla senkronizasyon otomatik yinelenir.\n" +
                                       "• Ağ Değişim Dinleyicisi (Connectivity Broadcast): İnternet tamamen kesildiğinde kuyruk dondurulur; internet geldiği anda otomatik arka plan işi tetiklenir (WorkManager / Expo Background Fetch).\n" +
                                       "• Payload Koruma (Indempotency): İşlem paketine yerleştirilen local UUID sayesinde sunucu, aynı kaydın ikinci kez veritabanına eklenmesini önler (İşlem varsa HTTP 209 Dönerek doğrudan Server ID'yi mobil cihaza doğrular)."
                            )

                            TechnicalCard(
                                title = "4. Kotlin Room / SQLite Örnek Kod Yapısı",
                                body = """
// 1. OFFLINE SYNC DAO (KOTLIN)
@Dao
interface SyncQueueDao {
    @Query("SELECT * FROM sync_queue WHERE is_processed = 0 ORDER BY captured_at ASC")
    fun getPendingQueue(): Flow<List<SyncQueueEntity>>

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertToQueue(item: SyncQueueEntity)

    @Query("UPDATE sync_queue SET is_processed = 1, server_id = :serverId WHERE uuid = :uuid")
    suspend fun markAsSynced(uuid: String, serverId: String)
}

// 2. SINIF & MODEL TANIMI
@Entity(tableName = "sync_queue")
data class SyncQueueEntity(
    @PrimaryKey val uuid: String,
    val documentNo: String,
    val actionType: String,
    val payloadJson: String,
    val capturedAt: Long,
    val isProcessed: Int = 0
)
                                """.trimIndent(),
                                isCode = true
                            )

                            TechnicalCard(
                                title = "5. Node.js Backend API Sync Çatısı",
                                body = """
// 3. API ENDPOINT & IDEMPOTENCY KONTROLÜ
app.post('/api/sync/sales', async (req, res) => {
    const { uuid, document_no, payload, captured_at } = req.body;
    
    // Mükerrer Gönderim (Idempotency) Kontrolü
    const existingBill = await db.query(
        "SELECT id FROM sales_bills WHERE local_uuid = ?", [uuid]
    );
    if (existingBill.length > 0) {
        return res.status(200).json({ 
            status: "EXISTS", 
            serverId: existingBill[0].id,
            message: "Mükerrer istek engellendi. Server ID eşleşti."
        });
    }

    // Yeni Satış Kaydı Oluşturma
    const serverId = await db.insertBill({
        local_uuid: uuid,
        bill_no: document_no,
        items: payload.items,
        created_at: captured_at
    });

    res.status(201).json({ status: "CREATED", serverId });
});
                                """.trimIndent(),
                                isCode = true
                            )
                        }
                    }
                }
            }
        }
    }

    // --- ALERTS WARNING DIALOG ---
    if (showDialogAlert) {
        Dialog(onDismissRequest = { showDialogAlert = false }) {
            Surface(
                shape = RoundedCornerShape(16.dp),
                color = MaterialTheme.colorScheme.surfaceContainerHigh,
                modifier = Modifier.padding(16.dp)
            ) {
                Column(
                    modifier = Modifier.padding(20.dp),
                    horizontalAlignment = Alignment.CenterHorizontally,
                    verticalArrangement = Arrangement.spacedBy(12.dp)
                ) {
                    Icon(
                        imageVector = Icons.Filled.Info,
                        contentDescription = null,
                        tint = MaterialTheme.colorScheme.primary,
                        modifier = Modifier.size(48.dp)
                    )
                    Text(
                        text = "Bilgi & Uyarı",
                        style = MaterialTheme.typography.titleMedium,
                        fontWeight = FontWeight.Bold,
                        color = MaterialTheme.colorScheme.onSurface
                    )
                    Text(
                        text = dialogAlertText,
                        style = MaterialTheme.typography.bodyMedium,
                        textAlign = TextAlign.Center,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                    Button(
                        onClick = { showDialogAlert = false },
                        modifier = Modifier.fillMaxWidth().padding(top = 8.dp)
                    ) {
                        Text("Kapat")
                    }
                }
            }
        }
    }
}

// --- SUB-WIDGET COMPONENTS ---
data class SchemaField(
    val name: String,
    val type: String,
    val description: String,
    val sampleValue: String
)

@Composable
fun SchemaTableCard(
    tableName: String,
    desc: String,
    fields: List<SchemaField>
) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(12.dp),
        border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant)
    ) {
        Column(modifier = Modifier.padding(16.dp)) {
            Text("Tablo: $tableName", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
            Spacer(modifier = Modifier.height(4.dp))
            Text(desc, style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
            
            Spacer(modifier = Modifier.height(12.dp))
            HorizontalDivider()
            Spacer(modifier = Modifier.height(8.dp))

            Column(verticalArrangement = Arrangement.spacedBy(10.dp)) {
                fields.forEach { field ->
                    Column {
                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.SpaceBetween,
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(4.dp)) {
                                Text(field.name, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold, fontFamily = FontFamily.Monospace)
                                Text("(${field.type})", style = MaterialTheme.typography.bodySmall, color = Color.Gray, fontFamily = FontFamily.Monospace)
                            }
                            Text(field.sampleValue, style = MaterialTheme.typography.bodySmall, fontWeight = FontWeight.SemiBold, color = MaterialTheme.colorScheme.outline)
                        }
                        Text(field.description, style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.8f))
                    }
                }
            }
        }
    }
}

@Composable
fun TechnicalCard(
    title: String,
    body: String,
    isCode: Boolean = false
) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(12.dp),
        colors = CardDefaults.cardColors(
            containerColor = if (isCode) MaterialTheme.colorScheme.surfaceContainerLow else MaterialTheme.colorScheme.surface
        ),
        border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant)
    ) {
        Column(modifier = Modifier.padding(16.dp)) {
            Text(
                title, 
                style = MaterialTheme.typography.titleSmall, 
                fontWeight = FontWeight.Bold, 
                color = if (isCode) MaterialTheme.colorScheme.secondary else MaterialTheme.colorScheme.onSurface
            )
            Spacer(modifier = Modifier.height(8.dp))
            Text(
                text = body, 
                style = if (isCode) MaterialTheme.typography.bodySmall else MaterialTheme.typography.bodyMedium,
                fontFamily = if (isCode) FontFamily.Monospace else FontFamily.Default,
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )
        }
    }
}

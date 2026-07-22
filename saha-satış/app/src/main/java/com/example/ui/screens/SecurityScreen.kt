package com.example.ui.screens

import android.media.AudioManager
import android.media.ToneGenerator
import android.os.Build
import android.os.VibrationEffect
import android.os.Vibrator
import android.os.VibratorManager
import androidx.compose.animation.*
import androidx.compose.foundation.BorderStroke
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

// --- INTERACTIVE SECURITY DATA MODELS ---
data class UserRole(
    val id: String,
    val name: String,
    val description: String,
    val defaultPermissions: List<String>,
    val color: Color
)

data class PermissionItem(
    val key: String,
    val label: String,
    val category: String // "TİCARİ", "FİNANS", "STOK", "YÖNETİM"
)

data class SecurityLog(
    val id: String,
    val timestamp: String,
    val action: String,
    val username: String,
    val severity: String, // "INFO", "WARNING", "DANGER"
    val extraDetails: String
)

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun SecurityScreen(navController: NavController) {
    val context = LocalContext.current
    val scope = rememberCoroutineScope()

    // --- APP SOUND & HAPTIC SYSTEM ---
    fun playSecurityFeedback(isCritical: Boolean) {
        com.example.util.VibratorHelper.triggerFeedback(context, !isCritical)
    }

    // --- REPORT TYPES DEFINITIONS ---
    val roles = remember {
        listOf(
            UserRole(
                "OWNER", "Firma Sahibi", "Tüm şirket verilerine, karlılığa tam yetkili.",
                listOf("SATIŞ", "ALIŞ", "İADE", "TAHSİLAT", "TEDİYE", "STOK_GOR", "STOK_DEG", "SAYIM", "RAPOR", "FIYAT", "ISKONTO", "CARI_BAK", "KATALOG", "KULLANICI_EKLE", "AYAR_DEG"),
                Color(0xFFE53935)
            ),
            UserRole(
                "MANAGER", "Yönetici", "Saha ekiplerini ve cari limitleri yönetir.",
                listOf("SATIŞ", "ALIŞ", "İADE", "TAHSİLAT", "TEDİYE", "STOK_GOR", "SAYIM", "RAPOR", "FIYAT", "ISKONTO", "CARI_BAK", "KATALOG", "AYAR_DEG"),
                Color(0xFF3949AB)
            ),
            UserRole(
                "SALES", "Satış Personeli", "Sahada fatura, sipariş, katalog ve iade yönetir.",
                listOf("SATIŞ", "İADE", "TAHSİLAT", "STOK_GOR", "KATALOG", "CARI_BAK", "ISKONTO"),
                Color(0xFF43A047)
            ),
            UserRole(
                "WAREHOUSE", "Depo Personeli", "Transiti, sayımı ve stok girişlerini yönetir.",
                listOf("STOK_GOR", "STOK_DEG", "SAYIM", "KATALOG"),
                Color(0xFFFB8C00)
            ),
            UserRole(
                "CASHIER", "Tahsilat Personeli", "Saha tahsilat ve banka havale girişleri yapar.",
                listOf("TAHSİLAT", "TEDİYE", "CARI_BAK", "RAPOR"),
                Color(0xFF00ACC1)
            ),
            UserRole(
                "REPORTER", "Rapor Kullanıcısı", "Sadece KPI, grafik ve analiz raporlarını izler.",
                listOf("RAPOR", "STOK_GOR", "CARI_BAK"),
                Color(0xFF8E24AA)
            ),
            UserRole(
                "SUPER", "Süper Admin", "Altyapı lisanslama ve tüm sistem seviyesi yetkiler.",
                listOf("SATIŞ", "ALIŞ", "İADE", "TAHSİLAT", "TEDİYE", "STOK_GOR", "STOK_DEG", "SAYIM", "RAPOR", "FIYAT", "ISKONTO", "CARI_BAK", "KATALOG", "KULLANICI_EKLE", "AYAR_DEG"),
                Color(0xFF1E88E5)
            )
        )
    }

    val permissionsList = remember {
        listOf(
            PermissionItem("SATIŞ", "Satış Yapabilir", "TİCARİ"),
            PermissionItem("ALIŞ", "Alış Yapabilir", "TİCARİ"),
            PermissionItem("İADE", "İade Alabilir", "TİCARİ"),
            PermissionItem("TAHSİLAT", "Tahsilat Yapabilir", "FİNANS"),
            PermissionItem("TEDİYE", "Tediye Ödemesi Yapabilir", "FİNANS"),
            PermissionItem("CARI_BAK", "Cari Bakiye Görebilir", "FİNANS"),
            PermissionItem("STOK_GOR", "Stok Envanteri Görebilir", "STOK"),
            PermissionItem("STOK_DEG", "Stok Miktarı Değiştirebilir", "STOK"),
            PermissionItem("SAYIM", "Depo Sayımı Yapabilir", "STOK"),
            PermissionItem("KATALOG", "Katalog & Resim Görebilir", "STOK"),
            PermissionItem("RAPOR", "Grafik Raporları Görebilir", "YÖNETİM"),
            PermissionItem("FIYAT", "Fiyat Değiştirebilir", "YÖNETİM"),
            PermissionItem("ISKONTO", "Özel İskonto Tanımlayabilir", "YÖNETİM"),
            PermissionItem("KULLANICI_EKLE", "Yeni Kullanıcı Ekleyebilir", "YÖNETİM"),
            PermissionItem("AYAR_DEG", "Sistem Ayarı Değiştirebilir", "YÖNETİM")
        )
    }

    val securityLogs = remember {
        mutableStateListOf(
            SecurityLog("1", "16:42:10", "JWT_REFRESH_SUCCESS", "Ahmet Yılmaz", "INFO", "Yeni Access Token başarıyla sağlandı."),
            SecurityLog("2", "16:41:02", "OFFLINK_PROCESS_FAIL", "Bilinmeyen Cihaz", "WARNING", "Eşitlenmemiş bakiye transferi tespiti."),
            SecurityLog("3", "16:32:45", "UNAUTHORIZED_PREVENTION", "Yusuf Demir", "DANGER", "Fiyat Değiştirme yetkisi engellendi!"),
            SecurityLog("4", "16:30:12", "DEVICE_BIND_VERIFIED", "Ahmet Yılmaz", "INFO", "Xiaomi RedMi-Note ile cihaz kilidi doğrulandı.")
        )
    }

    // --- NAVIGATION TABS CONTROLLER ---
    var activeTab by remember { mutableStateOf(0) } // 0: Rol Matrisi & Cihaz Kilidi, 1: DB & Middleware Şemaları, 2: Teknik Kaynaklar

    // --- SELECTED SIMULATOR COMPONENT STATES ---
    var selectedRole by remember { mutableStateOf(roles[2]) } // Default: Satış Personeli
    var jwtTokenTimeLeft by remember { mutableStateOf(118) } // Expires in 118 seconds
    var deviceLockedToUser by remember { mutableStateOf(true) }
    var activeDeviceUuid by remember { mutableStateOf("uuid-8f92-ka21-72bd-1002") }

    // Simulation Trigger
    var isSimulatingTokenRefresh by remember { mutableStateOf(false) }
    var showInfoAlertDial by remember { mutableStateOf(false) }
    var infoAlertText by remember { mutableStateOf("") }

    // Countdown Timer simulation
    LaunchedEffect(key1 = jwtTokenTimeLeft) {
        if (jwtTokenTimeLeft > 0) {
            delay(1000)
            jwtTokenTimeLeft--
        } else {
            jwtTokenTimeLeft = 120 // Reset automatically
        }
    }

    fun triggerTokenRefresh() {
        scope.launch {
            isSimulatingTokenRefresh = true
            delay(1200)
            jwtTokenTimeLeft = 120
            securityLogs.add(0, SecurityLog(
                (securityLogs.size + 1).toString(),
                "Şimdi",
                "JWT_REFRESH_SUCCESS",
                "Ahmet Yılmaz",
                "INFO",
                "Cihaz JWT Access Token süresi yenilendi."
            ))
            isSimulatingTokenRefresh = false
            playSecurityFeedback(false)
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
            // Header
            FieldHeader(
                title = "Güvenlik & Yetki",
                subtitle = "SİSTEM ROL MATRİSİ VE KİLİTLEME YÖNETİMİ",
                trailingContent = {
                    Row(
                        modifier = Modifier
                            .clip(RoundedCornerShape(12.dp))
                            .background(MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.2f))
                            .padding(horizontal = 12.dp, vertical = 6.dp),
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Icon(Icons.Filled.Security, contentDescription = null, modifier = Modifier.size(16.dp), tint = MaterialTheme.colorScheme.primary)
                        Spacer(modifier = Modifier.width(4.dp))
                        Text("SSL AES-256", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.primary, fontWeight = FontWeight.Bold)
                    }
                }
            )

            // Primary Tabs Selection
            TabRow(
                selectedTabIndex = activeTab,
                modifier = Modifier.fillMaxWidth(),
                containerColor = MaterialTheme.colorScheme.surface
            ) {
                Tab(
                    selected = activeTab == 0,
                    onClick = { activeTab = 0 },
                    text = { Text("Rol & Cihaz Koruma", fontWeight = FontWeight.Bold) },
                    icon = { Icon(Icons.Filled.VerifiedUser, contentDescription = null) }
                )
                Tab(
                    selected = activeTab == 1,
                    onClick = { activeTab = 1 },
                    text = { Text("Erişim Tabloları", fontWeight = FontWeight.Bold) },
                    icon = { Icon(Icons.Filled.Shield, contentDescription = null) }
                )
                Tab(
                    selected = activeTab == 2,
                    onClick = { activeTab = 2 },
                    text = { Text("Teknik Alt Yapı", fontWeight = FontWeight.Bold) },
                    icon = { Icon(Icons.Filled.Lock, contentDescription = null) }
                )
            }

            AnimatedContent(
                targetState = activeTab,
                transitionSpec = { fadeIn() togetherWith fadeOut() },
                modifier = Modifier.weight(1f)
            ) { target ->
                when (target) {
                    0 -> {
                        // --- INTERACTIVE SIMULATOR FOR ROLE MATRIX AND JWT CONTROL ---
                        Column(
                            modifier = Modifier
                                .fillMaxSize()
                                .padding(16.dp)
                                .verticalScroll(rememberScrollState()),
                            verticalArrangement = Arrangement.spacedBy(16.dp)
                        ) {
                            // Section: Active JWT Token Status
                            FieldCard(
                                containerColor = if (jwtTokenTimeLeft < 30) MaterialTheme.colorScheme.errorContainer.copy(alpha = 0.1f) else MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.3f)
                            ) {
                                Column(modifier = Modifier.padding(16.dp)) {
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween,
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        Column {
                                            Text("JWT Oturum Güvenliği", style = MaterialTheme.typography.titleSmall, fontWeight = FontWeight.Bold)
                                            Text("Süre bitince otomatik Refresh Token kullanılacaktır.", style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
                                        }

                                        if (isSimulatingTokenRefresh) {
                                            CircularProgressIndicator(modifier = Modifier.size(24.dp))
                                        } else {
                                            IconButton(onClick = { triggerTokenRefresh() }) {
                                                Icon(Icons.Filled.Refresh, contentDescription = null, tint = MaterialTheme.colorScheme.primary)
                                            }
                                        }
                                    }
                                    Spacer(modifier = Modifier.height(12.dp))

                                    Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                                        Text("Kalan Oturum Süresi (Access Token):", style = MaterialTheme.typography.labelMedium)
                                        Text("$jwtTokenTimeLeft Sn / 120 Sn", style = MaterialTheme.typography.labelMedium, fontWeight = FontWeight.Bold)
                                    }
                                    Spacer(modifier = Modifier.height(6.dp))
                                    LinearProgressIndicator(
                                        progress = { jwtTokenTimeLeft / 120f },
                                        modifier = Modifier.fillMaxWidth().height(8.dp).clip(RoundedCornerShape(4.dp)),
                                        color = if (jwtTokenTimeLeft < 30) MaterialTheme.colorScheme.error else MaterialTheme.colorScheme.primary,
                                        trackColor = MaterialTheme.colorScheme.outlineVariant
                                    )
                                }
                            }

                            // Section: Device Hardware Binding status screen
                            FieldCard {
                                Column(modifier = Modifier.padding(16.dp), verticalArrangement = Arrangement.spacedBy(12.dp)) {
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween,
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                                            Icon(Icons.Filled.PhonelinkLock, contentDescription = null, tint = if (deviceLockedToUser) Color(0xFF2E7D32) else Color(0xFFC62828))
                                            Text("Cihaz Bağlama (Device Binding)", style = MaterialTheme.typography.titleSmall, fontWeight = FontWeight.Bold)
                                        }

                                        Switch(
                                            checked = deviceLockedToUser,
                                            onCheckedChange = {
                                                deviceLockedToUser = it
                                                playSecurityFeedback(!it)
                                                securityLogs.add(0, SecurityLog(
                                                    (securityLogs.size + 1).toString(),
                                                    "Şimdi",
                                                    if (it) "DEVICE_BIND_ENABLED" else "DEVICE_BIND_DISABLED",
                                                    "Ahmet Yılmaz",
                                                    if (it) "INFO" else "WARNING",
                                                    if (it) "Donanım bağlama aktifleştirildi." else "Kritik Uyarı: Cihaz kilidi deaktife çekildi!"
                                                ))
                                            }
                                        )
                                    }

                                    Text(
                                        text = "Firma yetkililerinin tanımladığı bu parametre ile, saha personelinin kullanıcı hesabı sadece tanımlı tek bir mobil donanımla eşleştirilir. Hesap çalınsa dahi başka bir telefondan giriş yapılması engellenir.",
                                        style = MaterialTheme.typography.bodySmall,
                                        color = MaterialTheme.colorScheme.onSurfaceVariant
                                    )

                                    Row(
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .clip(RoundedCornerShape(8.dp))
                                            .background(MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f))
                                            .padding(10.dp),
                                        horizontalArrangement = Arrangement.SpaceBetween
                                    ) {
                                        Column {
                                            Text("Sistem UUID Anahtarı", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                            Text(activeDeviceUuid, style = MaterialTheme.typography.bodyMedium, fontFamily = FontFamily.Monospace, fontWeight = FontWeight.Bold)
                                        }
                                        TextButton(onClick = {
                                            activeDeviceUuid = "uuid-" + (1000 + (Math.random() * 9000).toInt()) + "-ka21-72bd-" + (1000 + (Math.random() * 9000).toInt())
                                            playSecurityFeedback(false)
                                        }) {
                                            Text("Yeni UUID Ata")
                                        }
                                    }
                                }
                            }

                            // Interactive Role vs Permission Simulator
                            Text(
                                text = "Kullanıcı Rolü & Yetki Matrisi",
                                style = MaterialTheme.typography.headlineSmall,
                                fontWeight = FontWeight.Bold,
                                modifier = Modifier.padding(top = 8.dp)
                            )
                            Text(
                                text = "Lütfen simüle etmek istediğiniz saha çalışan rolünü seçin. Matristen anlık yetkilendirme değişikliklerini izleyin.",
                                style = MaterialTheme.typography.bodySmall,
                                color = MaterialTheme.colorScheme.onSurfaceVariant
                            )

                            // Horizontally scrolls user roles
                            LazyRow(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                                items(roles) { role ->
                                    val isSelected = selectedRole.id == role.id
                                    FilterChip(
                                        selected = isSelected,
                                        onClick = {
                                            selectedRole = role
                                            playSecurityFeedback(false)
                                        },
                                        label = { Text(role.name) },
                                        leadingIcon = {
                                            Icon(
                                                imageVector = if (isSelected) Icons.Filled.CheckCircle else Icons.Filled.RadioButtonUnchecked,
                                                contentDescription = null,
                                                modifier = Modifier.size(16.dp),
                                                tint = if (isSelected) role.color else Color.Gray
                                            )
                                        },
                                        colors = FilterChipDefaults.filterChipColors(
                                            selectedContainerColor = role.color.copy(alpha = 0.15f),
                                            selectedLabelColor = role.color
                                        )
                                    )
                                }
                            }

                            // Role Description display
                            Card(
                                colors = CardDefaults.cardColors(containerColor = selectedRole.color.copy(alpha = 0.05f)),
                                border = BorderStroke(1.dp, selectedRole.color.copy(alpha = 0.2f))
                            ) {
                                Row(
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .padding(14.dp),
                                    verticalAlignment = Alignment.CenterVertically
                                ) {
                                    Icon(Icons.Filled.Info, contentDescription = null, tint = selectedRole.color, modifier = Modifier.size(24.dp))
                                    Spacer(modifier = Modifier.width(12.dp))
                                    Column {
                                        Text(selectedRole.name, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold, color = selectedRole.color)
                                        Text(selectedRole.description, style = MaterialTheme.typography.labelMedium, color = MaterialTheme.colorScheme.onSurfaceVariant)
                                    }
                                }
                            }

                            // Dynamic Permission Grid Simulator
                            FieldCard {
                                Column(modifier = Modifier.padding(16.dp), verticalArrangement = Arrangement.spacedBy(10.dp)) {
                                    Text("Yetki ve Sınır Listesi", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold)

                                    // Render all permissions from system list
                                    permissionsList.forEach { permission ->
                                        val isAuthorized = selectedRole.defaultPermissions.contains(permission.key)
                                        Row(
                                            modifier = Modifier
                                                .fillMaxWidth()
                                                .clip(RoundedCornerShape(8.dp))
                                                .background(if (isAuthorized) Color(0xFFE8F5E9).copy(alpha = 0.4f) else Color(0xFFFFEBEE).copy(alpha = 0.3f))
                                            .padding(horizontal = 12.dp, vertical = 8.dp),
                                            verticalAlignment = Alignment.CenterVertically,
                                            horizontalArrangement = Arrangement.SpaceBetween
                                        ) {
                                            Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                                                Icon(
                                                    imageVector = if (isAuthorized) Icons.Filled.CheckCircle else Icons.Filled.Cancel,
                                                    contentDescription = null,
                                                    tint = if (isAuthorized) Color(0xFF2E7D32) else Color(0xFFC62828),
                                                    modifier = Modifier.size(18.dp)
                                                )
                                                Column {
                                                    Text(permission.label, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold)
                                                    Text("Kanal: ${permission.category}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                                }
                                            }

                                            Surface(
                                                color = if (isAuthorized) Color(0xFF2E7D32).copy(alpha = 0.15f) else Color(0xFFC62828).copy(alpha = 0.15f),
                                                shape = RoundedCornerShape(12.dp)
                                            ) {
                                                Text(
                                                    text = if (isAuthorized) "İZİN VERİLDİ" else "KILITLI",
                                                    modifier = Modifier.padding(horizontal = 10.dp, vertical = 4.dp),
                                                    style = MaterialTheme.typography.labelSmall,
                                                    color = if (isAuthorized) Color(0xFF2E7D32) else Color(0xFFC62828),
                                                    fontWeight = FontWeight.Bold
                                                )
                                            }
                                        }
                                    }
                                }
                            }

                            // Section: Real-time Interactive System Audit Logger Output
                            Text("Sistem Güvenlik Günlük Kaydı (Logs)", style = MaterialTheme.typography.headlineSmall, fontWeight = FontWeight.Bold, modifier = Modifier.padding(top = 8.dp))
                            Card(
                                modifier = Modifier.fillMaxWidth(),
                                colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.3f)),
                                border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant)
                            ) {
                                Column(modifier = Modifier.padding(14.dp), verticalArrangement = Arrangement.spacedBy(8.dp)) {
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween,
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        Text("Audit Trail Tracker", style = MaterialTheme.typography.titleSmall, fontWeight = FontWeight.Bold)
                                        TextButton(onClick = { securityLogs.clear() }) {
                                            Text("Temizle", color = MaterialTheme.colorScheme.error)
                                        }
                                    }

                                    securityLogs.forEach { logItem ->
                                        Row(
                                            modifier = Modifier
                                                .fillMaxWidth()
                                                .clip(RoundedCornerShape(6.dp))
                                                .background(MaterialTheme.colorScheme.surface)
                                                .padding(10.dp),
                                            verticalAlignment = Alignment.CenterVertically,
                                            horizontalArrangement = Arrangement.spacedBy(8.dp)
                                        ) {
                                            val badgeColor = when (logItem.severity) {
                                                "INFO" -> Color(0xFF1E88E5)
                                                "WARNING" -> Color(0xFFEF6C00)
                                                "DANGER" -> Color(0xFFC62828)
                                                else -> Color.Gray
                                            }

                                            Box(
                                                modifier = Modifier
                                                    .background(badgeColor, CircleShape)
                                                    .size(10.dp)
                                            )

                                            Column(modifier = Modifier.weight(1f)) {
                                                Row(
                                                    modifier = Modifier.fillMaxWidth(),
                                                    horizontalArrangement = Arrangement.SpaceBetween
                                                ) {
                                                    Text(logItem.action, style = MaterialTheme.typography.labelMedium, fontWeight = FontWeight.ExtraBold, color = badgeColor)
                                                    Text(logItem.timestamp, style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                                }
                                                Text(logItem.extraDetails, style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
                                                Text("Kullanıcı: ${logItem.username}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    1 -> {
                        // --- ACCESSIBILITY AND INTERACTIVE DATABASE TAB SCHEMAS DISPLAY ---
                        Column(
                            modifier = Modifier
                                .fillMaxSize()
                                .padding(16.dp)
                                .verticalScroll(rememberScrollState()),
                            verticalArrangement = Arrangement.spacedBy(16.dp)
                        ) {
                            Text(
                                text = "Firma Yetkilendirme & Rol SQL Şemaları",
                                style = MaterialTheme.typography.headlineSmall,
                                fontWeight = FontWeight.Bold,
                                color = MaterialTheme.colorScheme.primary
                            )
                            Text(
                                text = "Kullanıcı yetki, engelleme, eşleştirme ve audit günlükleri için tasarlanan üretim sınıfı SQL ilişkisel veritabanı şeması aşağıdaki gibidir:",
                                style = MaterialTheme.typography.bodySmall,
                                color = MaterialTheme.colorScheme.onSurfaceVariant
                            )

                            // Multi-tab Schema selector for SQLite / Postgres parity
                            var selectedSqlSubTab by remember { mutableStateOf(0) }
                            val subSqlTabs = listOf("Roller & Yetkiler", "Cihaz Eşleme", "Audit Loglama")

                            LazyRow(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                                items(subSqlTabs.size) { idx ->
                                    val isAct = selectedSqlSubTab == idx
                                    SuggestionChip(
                                        onClick = { selectedSqlSubTab = idx },
                                        label = { Text(subSqlTabs[idx]) },
                                        colors = SuggestionChipDefaults.suggestionChipColors(
                                            containerColor = if (isAct) MaterialTheme.colorScheme.primaryContainer else Color.Transparent
                                        )
                                    )
                                }
                            }

                             when (selectedSqlSubTab) {
                                 0 -> {
                                     SecuritySchemaTableCard(
                                         tableName = "user_roles & permissions",
                                         desc = "Sistem üzerinde tanımlı olan statik ve dinamik yetki gruplarını, rollerini tutan tablodur.",
                                         fields = listOf(
                                             SecuritySchemaField("id", "INTEGER (PK)", "Otomatik artan birincil anahtar", "1"),
                                             SecuritySchemaField("role_key", "TEXT (UNIQUE)", "Rolün benzersiz kodu", "SALES"),
                                             SecuritySchemaField("role_name", "TEXT", "Rolün kullanıcı arayüzündeki adı", "Satış Temsilcisi"),
                                             SecuritySchemaField("permissions_csv", "TEXT", "İzin verilen yetki anahtarlarının virgülle birleştirilmiş hali", "SATIŞ,İADE,CARI_BAK"),
                                             SecuritySchemaField("is_active", "INTEGER", "Oturum kullanımına açık olup olmadığı", "1")
                                         )
                                     )
                                 }
                                 1 -> {
                                     SecuritySchemaTableCard(
                                         tableName = "device_bindings",
                                         desc = "Saha ekiplerinin tablet/telefon UUID'lerini ve bunların kullanıcı eşleşmelerini kitleyen tablolar.",
                                         fields = listOf(
                                             SecuritySchemaField("id", "INTEGER (PK)", "Otomatik artan birincil anahtar", "14"),
                                             SecuritySchemaField("user_id", "TEXT (FK)", "Tesis personeli kayıt referansı", "USR-100223"),
                                             SecuritySchemaField("device_uuid", "TEXT (UNIQUE)", "Mobil cihazın elde edilen unikal donanımsal UUID kodu", "uuid-8f92-ka21"),
                                             SecuritySchemaField("model_info", "TEXT", "Cihazın marka ve model bilgisi", "Xiaomi Redmi Note 10"),
                                             SecuritySchemaField("is_activated", "INTEGER", "Firma yöneticisi tarafından yetkilendirildi mi?", "1"),
                                             SecuritySchemaField("bound_at", "TIMESTAMP", "İlk eşleme yapılma zamanı", "2026-06-08 14:32:00")
                                         )
                                     )
                                 }
                                 2 -> {
                                     SecuritySchemaTableCard(
                                         tableName = "security_audit_logs",
                                         desc = "Güvenlik ihlallerini, yetkisiz sayfa isteklerini ve kritik para transferlerini kaydeden değiştirilemez günlük tablosu.",
                                         fields = listOf(
                                             SecuritySchemaField("id", "INTEGER (PK)", "Otomatik artan log ID'si", "10492"),
                                             SecuritySchemaField("action_type", "TEXT", "Yapılan işlem tipi (UNAUTHORIZED_ACTION, LOGIN_FAIL vb.)", "UNAUTHORIZED_PREVENTION"),
                                             SecuritySchemaField("user_id", "TEXT", "İşlemi tetikleyen kullanıcının ID'si", "USR-40212"),
                                             SecuritySchemaField("ip_address", "TEXT", "İstek atılan ağın anlık IP adresi", "192.168.1.42"),
                                             SecuritySchemaField("is_offline_captured", "INTEGER", "Çevrimdışı yapılıp sonradan mı eşlendi? (0: Online, 1: Offline)", "1"),
                                             SecuritySchemaField("payload", "TEXT", "İşlem detayları / Hata içeriği", "{ 'targetRoute': 'pricing_edit_attempt' }"),
                                             SecuritySchemaField("created_at", "TIMESTAMP", "Uzak / Yerel işlem kayıt tarihi", "2026-06-08 16:32:45")
                                         )
                                     )
                                 }
                             }
                        }
                    }

                    2 -> {
                        // --- SECTION 3: IN-DEPTH BACKEND CODES & MIDDLEWARE CODES ---
                        Column(
                            modifier = Modifier
                                .fillMaxSize()
                                .padding(16.dp)
                                .verticalScroll(rememberScrollState()),
                            verticalArrangement = Arrangement.spacedBy(16.dp)
                        ) {
                            Text(
                                text = "Gelişmiş Backend Entegrasyon Kodları",
                                style = MaterialTheme.typography.titleMedium,
                                fontWeight = FontWeight.Bold,
                                color = MaterialTheme.colorScheme.primary
                            )

                             SecurityTechnicalCard(
                                 title = "1. JWT Authentication & Refresh Token Stratejisi (Node.js)",
                                 body = "Mobil saha uygulamasında JWT güvenliğinin ve Token tazeleme sisteminin backend Node.js middleware yapısı şu şekildedir:\n\n" +
                                        "• Access Token süresi 15 dakika gibi kısa tutulur. Böylelikle yetkisi alınan personelin ekranı hızla bloke olur.\n" +
                                        "• Refresh Token ise veritabanında Hash'li olarak saklanır ve 30 gün boyunca yeni Access Token üretimine olanak tanır.\n" +
                                        "• İstek başlığından (Header) Authorization şeması süzülür."
                             )
 
                             SecurityTechnicalCard(
                                 title = "2. Express.js Guard Middleware Örneği",
                                 body = """
 // GÜVENLİK VE ROL DOĞRULAMA MIDDLEWARE (NODE.JS + EXPRESS)
 const jwt = require('jsonwebtoken');
 
 const authenticateAndCheckPermission = (requiredPermission) => {
     return async (req, res, next) => {
         try {
             const authHeader = req.headers['authorization'];
             const token = authHeader && authHeader.split(' ')[1];
             
             if (!token) return res.status(401).json({ error: "Erişim Reddedildi. Token Eksik!" });
 
             // JWT Doğrula
             const decoded = jwt.verify(token, process.env.JWT_ACCESS_SECRET);
             req.user = decoded; // { userId, role, permissions, deviceUuid }
 
             // Device Binding Kontrolü
             const clientDeviceUuid = req.headers['x-device-uuid'];
             if (req.user.deviceUuid !== clientDeviceUuid) {
                 return res.status(403).json({ error: "Lütfen kayıtlı mobil cihazınızdan bağlanın!" });
             }
 
             // Yetki Kontrolü
             if (!req.user.permissions.includes(requiredPermission)) {
                 // Veritabanına sızma/yetkisiz eylem girişimi loglanır
                 await auditLogModel.create({
                     action_type: 'UNAUTHORIZED_PREVENTION',
                     user_id: req.user.userId,
                     payload: JSON.stringify({ requiredPermission })
                 });
 
                 return res.status(403).json({ error: "Bu işlem için yetkiniz bulunmuyor!" });
             }
 
             next();
         } catch (error) {
             return res.status(401).json({ error: "Oturum süresi dolmuş veya geçersiz token!" });
         }
     };
 };
                                 """.trimIndent(),
                                 isCode = true
                             )
 
                             SecurityTechnicalCard(
                                 title = "3. Plan & Rol ve Modül Sınımlamaları",
                                 body = "Ticari saha yazılımında ölçeklenebilirlik açısından paket ve plan sınırlamaları şöyle yapılır:\n\n" +
                                        "• Standart Paket: Maksimum 3 kullanıcı (Sadece Satış ve Tahsilat). Depo/Katalog sayfa kısıtı vardır.\n" +
                                        "• Profesyonel Paket: Depo ve Sayım yetkileri aktif, çoklu depo stok sayımı yapılabilir.\n" +
                                        "• Enterprise Paket: Detaylı Raporlar, Cari Risk Limiti Yönetimi, Özel İskonto Tanımlama ve Sınırsız Kullanıcı.\n" +
                                        "• Kontrol Mekanizması: Hem mobil taraf `NavHost` yapısında hem de sunucu API'larında lisans planı katmanı dinamik olarak doğrulanır."
                             )
 
                             SecurityTechnicalCard(
                                 title = "4. Kotlin Android Yetki Kontrol Katmanı",
                                 body = """
 // ANDROID COMPOSABLE SECURE AREA (KOTLIN)
 @Composable
 fun SecureActionButton(
     userPermissions: List<String>,
     requiredPerm: String,
     onClick: () -> Unit,
     content: @Composable RowScope.() -> Unit
 ) {
     val isAuthorized = userPermissions.contains(requiredPerm)
     
     Button(
         onClick = {
             if (isAuthorized) {
                 onClick()
             } else {
                 showToast("Bu eylemi yapmaya yetkiniz bulunmuyor!")
             }
         },
         colors = ButtonDefaults.buttonColors(
             containerColor = if (isAuthorized) MaterialTheme.colorScheme.primary 
                              else MaterialTheme.colorScheme.error.copy(alpha = 0.5f)
         )
     ) {
         if (!isAuthorized) {
             Icon(Icons.Filled.Lock, contentDescription = null, modifier = Modifier.size(16.dp))
             Spacer(modifier = Modifier.width(4.dp))
         }
         content()
     }
 }
                                 """.trimIndent(),
                                 isCode = true
                             )
                        }
                    }
                }
            }
        }
    }

    // Alert Dialog
    if (showInfoAlertDial) {
        Dialog(onDismissRequest = { showInfoAlertDial = false }) {
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
                    Icon(Icons.Filled.Info, contentDescription = null, tint = MaterialTheme.colorScheme.primary, modifier = Modifier.size(36.dp))
                    Text(infoAlertText, textAlign = TextAlign.Center, style = MaterialTheme.typography.bodyMedium)
                    Button(onClick = { showInfoAlertDial = false }) {
                        Text("Kapat")
                    }
                }
            }
        }
    }
}

@Composable
fun SecuritySchemaTableCard(
    tableName: String,
    desc: String,
    fields: List<SecuritySchemaField>
) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant),
        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface)
    ) {
        Column(modifier = Modifier.padding(16.dp)) {
            Text("Tablo Adı: $tableName", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
            Text(desc, style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
            
            Spacer(modifier = Modifier.height(12.dp))
            Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
            Spacer(modifier = Modifier.height(8.dp))

            fields.forEach { field ->
                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(vertical = 6.dp),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.Top
                ) {
                    Column(modifier = Modifier.weight(1f)) {
                        Row(verticalAlignment = Alignment.CenterVertically) {
                            Text(field.name, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold, fontFamily = FontFamily.Monospace)
                            Spacer(modifier = Modifier.width(6.dp))
                            Surface(
                                color = MaterialTheme.colorScheme.secondaryContainer,
                                shape = RoundedCornerShape(4.dp)
                            ) {
                                Text(field.type, modifier = Modifier.padding(horizontal = 4.dp, vertical = 2.dp), style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Bold)
                            }
                        }
                        Text(field.desc, style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                    }
                    Text("Değer: ${field.example}", style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Normal, color = MaterialTheme.colorScheme.outline)
                }
                Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.2f))
            }
        }
    }
}

data class SecuritySchemaField(
    val name: String,
    val type: String,
    val desc: String,
    val example: String
)

@Composable
fun SecurityTechnicalCard(
    title: String,
    body: String,
    isCode: Boolean = false
) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant),
        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface)
    ) {
        Column(modifier = Modifier.padding(14.dp)) {
            Text(title, style = MaterialTheme.typography.titleSmall, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
            Spacer(modifier = Modifier.height(8.dp))
            if (isCode) {
                Box(
                    modifier = Modifier
                        .fillMaxWidth()
                        .clip(RoundedCornerShape(8.dp))
                        .background(Color(0xFF1E1E1E))
                        .padding(12.dp)
                ) {
                    Text(
                        text = body,
                        color = Color(0xFFD4D4D4),
                        fontFamily = FontFamily.Monospace,
                        style = MaterialTheme.typography.bodySmall
                    )
                }
            } else {
                Text(body, style = MaterialTheme.typography.bodyMedium, color = MaterialTheme.colorScheme.onSurfaceVariant)
            }
        }
    }
}

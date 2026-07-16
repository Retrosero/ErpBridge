package com.example.ui.screens

import androidx.compose.foundation.background
import androidx.compose.ui.graphics.Color
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.zIndex
import androidx.compose.ui.input.pointer.pointerInput
import kotlin.math.roundToInt
import androidx.compose.foundation.gestures.detectDragGesturesAfterLongPress
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.navigation.NavController
import com.example.ui.components.FieldCard
import kotlinx.coroutines.launch

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun MoreScreen(navController: NavController) {
    val snackbarHostState = remember { SnackbarHostState() }
    val scope = rememberCoroutineScope()
    val context = androidx.compose.ui.platform.LocalContext.current

    // Prefix & Sequence States
    var salesPrefix by remember { mutableStateOf("SAT") }
    var salesNo by remember { mutableStateOf("00000451") }
    
    var purchasePrefix by remember { mutableStateOf("AL") }
    var purchaseNo by remember { mutableStateOf("00000124") }
    
    var returnsPrefix by remember { mutableStateOf("IAD") }
    var returnsNo by remember { mutableStateOf("00000084") }
    
    var collectionPrefix by remember { mutableStateOf("TSH") }
    var collectionNo by remember { mutableStateOf("00001590") }
    
    var disbursementPrefix by remember { mutableStateOf("TED") }
    var disbursementNo by remember { mutableStateOf("00000213") }

    Scaffold(
        snackbarHost = { SnackbarHost(snackbarHostState) },
        containerColor = MaterialTheme.colorScheme.background,
        contentWindowInsets = WindowInsets(0.dp)
    ) { innerPadding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(
                    top = 0.dp,
                    bottom = innerPadding.calculateBottomPadding(),
                    start = 8.dp,
                    end = 8.dp
                )
        ) {
            // LazyColumn to render selected tab's items snuggly
            LazyColumn(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(top = 4.dp, bottom = 4.dp),
                verticalArrangement = Arrangement.spacedBy(10.dp)
            ) {
                when (AppDataStore.moreSelectedTabIndex) {
                    0 -> { // --- TAB 1: PROFİL ---
                        item {
                            Card(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(vertical = 4.dp),
                                colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
                                shape = RoundedCornerShape(12.dp),
                                border = androidx.compose.foundation.BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant)
                            ) {
                                Row(
                                    modifier = Modifier.padding(12.dp),
                                    verticalAlignment = Alignment.CenterVertically,
                                    horizontalArrangement = Arrangement.spacedBy(12.dp)
                                ) {
                                    Box(
                                        modifier = Modifier
                                            .size(50.dp)
                                            .background(MaterialTheme.colorScheme.primaryContainer, CircleShape),
                                        contentAlignment = Alignment.Center
                                    ) {
                                        Icon(
                                            Icons.Filled.Person,
                                            contentDescription = null,
                                            modifier = Modifier.size(26.dp),
                                            tint = MaterialTheme.colorScheme.onPrimaryContainer
                                        )
                                    }
                                    Column(modifier = Modifier.weight(1f)) {
                                        Text(
                                            text = "Serhan Kalay",
                                            style = MaterialTheme.typography.titleMedium,
                                            fontWeight = FontWeight.Bold,
                                            color = MaterialTheme.colorScheme.onSurface
                                        )
                                        Text(
                                            text = "Saha Satış Müdürü",
                                            style = MaterialTheme.typography.bodySmall,
                                            color = MaterialTheme.colorScheme.onSurfaceVariant
                                        )
                                        Spacer(modifier = Modifier.height(2.dp))
                                        Surface(
                                            color = MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.15f),
                                            shape = RoundedCornerShape(4.dp)
                                        ) {
                                            Row(
                                                modifier = Modifier.padding(horizontal = 6.dp, vertical = 2.dp),
                                                verticalAlignment = Alignment.CenterVertically
                                            ) {
                                                Icon(Icons.Filled.Verified, contentDescription = null, modifier = Modifier.size(10.dp), tint = MaterialTheme.colorScheme.primary)
                                                Spacer(modifier = Modifier.width(4.dp))
                                                Text(
                                                    text = "Aktif Hesap",
                                                    style = MaterialTheme.typography.labelSmall,
                                                    color = MaterialTheme.colorScheme.primary,
                                                    fontSize = 10.sp
                                                )
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // Users details cards in compact formatting
                        item {
                            FieldCard {
                                Column {
                                    Box(
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .background(MaterialTheme.colorScheme.surfaceContainerLowest)
                                            .padding(horizontal = 12.dp, vertical = 8.dp)
                                    ) {
                                        Text(
                                            text = "KULLANICI BİLGİLERİ",
                                            style = MaterialTheme.typography.labelMedium,
                                            color = MaterialTheme.colorScheme.primary,
                                            fontWeight = FontWeight.Bold
                                        )
                                    }
                                    Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
                                    
                                    ProfileInfoRow(label = "E-posta", value = "serhan.kalayy@gmail.com")
                                    Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.3f))
                                    ProfileInfoRow(label = "Rol / Yetki Yeteneği", value = "Süper Yönetici / Bölge Müdürü")
                                    Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.3f))
                                    ProfileInfoRow(label = "Çalışma Bölgesi", value = "Kuzey Marmara Sektörü")
                                    Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.3f))
                                    ProfileInfoRow(label = "Giriş Güvenliği", value = "Biometrik / PIN Aktif")
                                }
                            }
                        }

                        item {
                            FieldCard {
                                Column {
                                    Box(
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .background(MaterialTheme.colorScheme.surfaceContainerLowest)
                                            .padding(horizontal = 12.dp, vertical = 8.dp)
                                    ) {
                                        Text(
                                            text = "CİHAZ VE LİSANS BİLGİSİ",
                                            style = MaterialTheme.typography.labelMedium,
                                            color = MaterialTheme.colorScheme.primary,
                                            fontWeight = FontWeight.Bold
                                        )
                                    }
                                    Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
                                    
                                    ProfileInfoRow(label = "Cihaz Kodu (UUID)", value = "AIS-FIELD-883F-992A")
                                    Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.3f))
                                    ProfileInfoRow(label = "Lisans Durumu", value = "Ömür Boyu Kurumsal Lisans")
                                    Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.3f))
                                    ProfileInfoRow(label = "Son Entegrasyon", value = "Bugün, 12:45")
                                }
                            }
                        }

                        item {
                            var activeSubPack by remember { mutableStateOf(AppDataStore.subscriptionPack) }
                            var activeCompId by remember { mutableStateOf(AppDataStore.companyId) }
                            var isSyncingUp by remember { mutableStateOf(false) }
                            var isSyncingDown by remember { mutableStateOf(false) }

                            FieldCard {
                                Column(modifier = Modifier.padding(12.dp)) {
                                    Text(
                                        text = "BULUT ENTEGRASYONU VE SUBSCRIPTION PLANLARI",
                                        style = MaterialTheme.typography.labelMedium,
                                        color = MaterialTheme.colorScheme.primary,
                                        fontWeight = FontWeight.Bold,
                                        modifier = Modifier.padding(bottom = 8.dp)
                                    )
                                    Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f), modifier = Modifier.padding(bottom = 12.dp))

                                    // 3-Way Segmented buttons for Subscription Plan
                                    Text(
                                        text = "Aktif Plan Seçimi:",
                                        style = MaterialTheme.typography.titleSmall,
                                        fontWeight = FontWeight.Bold
                                    )
                                    Spacer(modifier = Modifier.height(8.dp))

                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.spacedBy(8.dp)
                                    ) {
                                        // Standart
                                        Card(
                                            modifier = Modifier
                                                .weight(1f)
                                                .clickable {
                                                    activeSubPack = "local"
                                                    AppDataStore.setSubscriptionPackSetting(context, "local")
                                                    scope.launch {
                                                        snackbarHostState.showSnackbar("Plandiye Standart (Local-Only) seçildi.")
                                                    }
                                                },
                                            shape = RoundedCornerShape(8.dp),
                                            colors = CardDefaults.cardColors(
                                                containerColor = if (activeSubPack == "local") MaterialTheme.colorScheme.secondaryContainer else MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f)
                                            )
                                        ) {
                                            Column(
                                                modifier = Modifier.padding(8.dp),
                                                horizontalAlignment = Alignment.CenterHorizontally
                                            ) {
                                                Icon(Icons.Filled.CloudOff, contentDescription = null, modifier = Modifier.size(20.dp))
                                                Spacer(modifier = Modifier.height(4.dp))
                                                Text("Standart", style = MaterialTheme.typography.labelMedium, fontWeight = FontWeight.Bold)
                                                Text("Local-Only", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.outline)
                                            }
                                        }

                                        // Back-Up
                                        Card(
                                            modifier = Modifier
                                                .weight(1f)
                                                .clickable {
                                                    activeSubPack = "backup"
                                                    AppDataStore.setSubscriptionPackSetting(context, "backup")
                                                    scope.launch {
                                                        snackbarHostState.showSnackbar("Back-Up (Yedeklemeli) Planı seçildi.")
                                                    }
                                                },
                                            shape = RoundedCornerShape(8.dp),
                                            colors = CardDefaults.cardColors(
                                                containerColor = if (activeSubPack == "backup") MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f)
                                            )
                                        ) {
                                            Column(
                                                modifier = Modifier.padding(8.dp),
                                                horizontalAlignment = Alignment.CenterHorizontally
                                            ) {
                                                Icon(Icons.Filled.CloudQueue, contentDescription = null, modifier = Modifier.size(20.dp))
                                                Spacer(modifier = Modifier.height(4.dp))
                                                Text("Yedeklemeli", style = MaterialTheme.typography.labelMedium, fontWeight = FontWeight.Bold)
                                                Text("Cloud Backup", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.outline)
                                            }
                                        }

                                        // Shirket
                                        Card(
                                            modifier = Modifier
                                                .weight(1f)
                                                .clickable {
                                                    activeSubPack = "company"
                                                    AppDataStore.setSubscriptionPackSetting(context, "company")
                                                    scope.launch {
                                                        snackbarHostState.showSnackbar("Kurumsal Plan (Çoklu-Kullanıcı) seçildi.")
                                                    }
                                                },
                                            shape = RoundedCornerShape(8.dp),
                                            colors = CardDefaults.cardColors(
                                                containerColor = if (activeSubPack == "company") MaterialTheme.colorScheme.tertiaryContainer else MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f)
                                            )
                                        ) {
                                            Column(
                                                modifier = Modifier.padding(8.dp),
                                                horizontalAlignment = Alignment.CenterHorizontally
                                            ) {
                                                Icon(Icons.Filled.CloudSync, contentDescription = null, modifier = Modifier.size(20.dp))
                                                Spacer(modifier = Modifier.height(4.dp))
                                                Text("Kurumsal", style = MaterialTheme.typography.labelMedium, fontWeight = FontWeight.Bold)
                                                Text("Multi-User", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.outline)
                                            }
                                        }
                                    }

                                    Spacer(modifier = Modifier.height(12.dp))

                                    // Segment Details & Explanations
                                    Text(
                                        text = when(activeSubPack) {
                                            "local" -> "• Standart Paket: Veriler güvenli şekilde bu cihazda (local SQLite/Room) saklanır. Şirket dışı bağımsız saha personelleri için internet gerektirmeyen veri gizliliği sunar."
                                            "backup" -> "• Back-Up Paketi: Verileriniz hem yerel saklanır hem de otomatik olarak buluta yedeklenir. Cihaz silinmesi durumunda buluttan geri yükleyebilirsiniz."
                                            else -> "• Kurumsal Şirket Paketi (Bulut Ortak): Aynı şirket koduna sahip tüm cihazlar ortak bir bulut deposunda gerçek zamanlı çalışırlar. ERP bağlantısı gerektirmeyen işletmeler için idealdir."
                                        },
                                        style = MaterialTheme.typography.bodySmall,
                                        color = MaterialTheme.colorScheme.onSurfaceVariant
                                    )

                                    Spacer(modifier = Modifier.height(12.dp))

                                    // Multi-tenant configuration
                                    if (activeSubPack == "company") {
                                        OutlinedTextField(
                                            value = activeCompId,
                                            onValueChange = {
                                                activeCompId = it
                                                AppDataStore.setCompanyIdSetting(context, it)
                                            },
                                            label = { Text("Şirket Kodu / Tenant ID") },
                                            placeholder = { Text("örn: comp_turkuaz") },
                                            singleLine = true,
                                            modifier = Modifier.fillMaxWidth()
                                        )

                                        Spacer(modifier = Modifier.height(8.dp))

                                        Row(
                                            modifier = Modifier.fillMaxWidth(),
                                            horizontalArrangement = Arrangement.spacedBy(8.dp)
                                        ) {
                                            Button(
                                                onClick = {
                                                    val generated = "comp-" + (1000..9999).random()
                                                    activeCompId = generated
                                                    AppDataStore.setCompanyIdSetting(context, generated)
                                                    scope.launch {
                                                        snackbarHostState.showSnackbar("Yeni Şirket Kodu Üretildi: $generated")
                                                    }
                                                },
                                                colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.secondary),
                                                shape = RoundedCornerShape(8.dp),
                                                modifier = Modifier.weight(1f)
                                            ) {
                                                Text("Yeni Kod Üret", style = MaterialTheme.typography.labelMedium)
                                            }

                                            Button(
                                                onClick = {
                                                    if (activeCompId.isBlank()) {
                                                        scope.launch { snackbarHostState.showSnackbar("Lütfen geçerli bir şirket kodu girin!") }
                                                        return@Button
                                                    }
                                                    isSyncingDown = true
                                                    scope.launch {
                                                        val path = "companies/${activeCompId.trim().lowercase()}"
                                                        val success = com.example.data.CloudSyncManager.downloadAndOverwriteData(context, path)
                                                        isSyncingDown = false
                                                        if (success) {
                                                            snackbarHostState.showSnackbar("Şirket havuzundan ($activeCompId) tüm veriler başarıyla çekildi!")
                                                        } else {
                                                            snackbarHostState.showSnackbar("Veriler çekilemedi! İnternet veya veri kaydı mevcut değil.")
                                                        }
                                                    }
                                                },
                                                colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.tertiary),
                                                shape = RoundedCornerShape(8.dp),
                                                modifier = Modifier.weight(1.2f)
                                            ) {
                                                if (isSyncingDown) {
                                                    CircularProgressIndicator(modifier = Modifier.size(16.dp), strokeWidth = 2.dp, color = MaterialTheme.colorScheme.onTertiary)
                                                } else {
                                                    Text("Havuzdan Çek", style = MaterialTheme.typography.labelMedium)
                                                }
                                            }
                                        }

                                        Spacer(modifier = Modifier.height(8.dp))
                                        Text(
                                            text = "Hedef Bulut Yolu: /companies/${activeCompId.trim().lowercase().ifBlank { "[BELİRTİLMEDİ]" }}",
                                            style = MaterialTheme.typography.labelSmall,
                                            fontWeight = FontWeight.Bold,
                                            color = MaterialTheme.colorScheme.primary
                                        )
                                    }

                                    if (activeSubPack == "backup") {
                                        val backupPath = com.example.data.CloudSyncManager.getPersonalBackupPath()
                                        
                                        Row(
                                            modifier = Modifier.fillMaxWidth(),
                                            horizontalArrangement = Arrangement.spacedBy(8.dp)
                                        ) {
                                            Button(
                                                onClick = {
                                                    isSyncingUp = true
                                                    scope.launch {
                                                        val success = com.example.data.CloudSyncManager.uploadAllData(backupPath)
                                                        isSyncingUp = false
                                                        if (success) {
                                                            snackbarHostState.showSnackbar("Yerel veritabanınız tamamen buluta yedeklendi!")
                                                        } else {
                                                            snackbarHostState.showSnackbar("Bulut yedeklemesi başarısız oldu!")
                                                        }
                                                    }
                                                },
                                                colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.primary),
                                                shape = RoundedCornerShape(8.dp),
                                                modifier = Modifier.weight(1f)
                                            ) {
                                                if (isSyncingUp) {
                                                    CircularProgressIndicator(modifier = Modifier.size(16.dp), strokeWidth = 2.dp, color = MaterialTheme.colorScheme.onPrimary)
                                                } else {
                                                    Icon(Icons.Filled.CloudUpload, contentDescription = null, modifier = Modifier.size(16.dp))
                                                    Spacer(modifier = Modifier.width(4.dp))
                                                    Text("Yedek Gönder", style = MaterialTheme.typography.labelSmall)
                                                }
                                            }

                                            Button(
                                                onClick = {
                                                    isSyncingDown = true
                                                    scope.launch {
                                                        val success = com.example.data.CloudSyncManager.downloadAndOverwriteData(context, backupPath)
                                                        isSyncingDown = false
                                                        if (success) {
                                                            snackbarHostState.showSnackbar("Buluttaki yedeğiniz başarıyla geri yüklendi!")
                                                        } else {
                                                            snackbarHostState.showSnackbar("Bulut yedeği yüklenirken hata oluştu veya yedek yok.")
                                                        }
                                                    }
                                                },
                                                colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.secondary),
                                                shape = RoundedCornerShape(8.dp),
                                                modifier = Modifier.weight(1f)
                                            ) {
                                                if (isSyncingDown) {
                                                    CircularProgressIndicator(modifier = Modifier.size(16.dp), strokeWidth = 2.dp, color = MaterialTheme.colorScheme.onSecondary)
                                                } else {
                                                    Icon(Icons.Filled.CloudDownload, contentDescription = null, modifier = Modifier.size(16.dp))
                                                    Spacer(modifier = Modifier.width(4.dp))
                                                    Text("Yedeği Yükle", style = MaterialTheme.typography.labelSmall)
                                                }
                                            }
                                        }

                                        Spacer(modifier = Modifier.height(8.dp))
                                        Text(
                                            text = "Hedef Bulut Yedekleme Klasörü: /$backupPath",
                                            style = MaterialTheme.typography.labelSmall,
                                            fontWeight = FontWeight.Bold,
                                            color = MaterialTheme.colorScheme.primary
                                        )
                                    }
                                }
                            }
                        }

                        // Logout button
                        item {
                            val localContext = context
                            OutlinedButton(
                                onClick = {
                                    scope.launch {
                                        try {
                                            val db = com.example.data.database.DatabaseProvider.getDatabase(localContext)
                                            kotlinx.coroutines.withContext(kotlinx.coroutines.Dispatchers.IO) {
                                                db.userDao().clearSessions()
                                            }
                                            kotlinx.coroutines.withContext(kotlinx.coroutines.Dispatchers.Main) {
                                                navController.navigate("login") {
                                                    popUpTo(0) { inclusive = true }
                                                }
                                            }
                                        } catch (e: Exception) {
                                            e.printStackTrace()
                                        }
                                    }
                                },
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .height(50.dp)
                                    .padding(vertical = 4.dp),
                                shape = RoundedCornerShape(8.dp),
                                colors = ButtonDefaults.outlinedButtonColors(
                                    contentColor = MaterialTheme.colorScheme.error
                                ),
                                border = androidx.compose.foundation.BorderStroke(1.dp, MaterialTheme.colorScheme.error)
                            ) {
                                Icon(Icons.Filled.Logout, contentDescription = null, modifier = Modifier.size(16.dp))
                                Spacer(modifier = Modifier.width(6.dp))
                                Text("Hesaptan Çıkış Yap", style = MaterialTheme.typography.labelMedium)
                            }
                        }
                    }

                    1 -> { // --- TAB 2: GÖRÜNÜM (APPEARANCE & SCREEN LAYOUTS) ---
                        // KPI Selection Block with Reordering
                        item {
                            FieldCard {
                                Column {
                                    Box(
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .background(MaterialTheme.colorScheme.surfaceContainerLowest)
                                            .padding(horizontal = 12.dp, vertical = 8.dp)
                                    ) {
                                        Text(
                                            text = "KONTROL PANELİ GÖSTERGELERİ (KPI)",
                                            style = MaterialTheme.typography.labelMedium,
                                            color = MaterialTheme.colorScheme.primary,
                                            fontWeight = FontWeight.Bold
                                        )
                                    }
                                    Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
                                    
                                    Text(
                                        text = "Ana sayfada görüntülenecek göstergeleri seçin ve yön tuşlarıyla sıralayın. Seçilen tüm göstergeler çiftler halinde ekrene sığdırılır.",
                                        style = MaterialTheme.typography.labelSmall,
                                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                                        modifier = Modifier.padding(12.dp)
                                    )

                                    val kpiMapping = mapOf(
                                        "ciro" to Pair("Günlük Ciro (₺ 54.230,50)", Icons.Filled.Payments),
                                        "ziyaret" to Pair("Ziyaret / Hedef (14 / 20)", Icons.Filled.PersonSearch),
                                        "bekleyen_satis" to Pair("Bekleyenler (6 Fiş / 12.450 ₺)", Icons.Filled.HourglassEmpty),
                                        "tahsilat" to Pair("Günlük Tahsilat (₺ 28.750,00)", Icons.Filled.Payments),
                                        "onay_bekleyen" to Pair("Onay Bekleyen İşlemler (3 Sipariş)", Icons.Filled.Schedule)
                                    )

                                    val sortedKpis = remember(AppDataStore.activeKpiList) {
                                        val active = AppDataStore.activeKpiList.filter { kpiMapping.containsKey(it) }
                                        val inactive = kpiMapping.keys.filter { !active.contains(it) }
                                        active + inactive
                                    }

                                    sortedKpis.forEach { key ->
                                        val info = kpiMapping[key] ?: return@forEach
                                        val label = info.first
                                        val icon = info.second
                                        val isSelected = AppDataStore.activeKpiList.contains(key)
                                        Row(
                                            modifier = Modifier
                                                .fillMaxWidth()
                                                .clickable {
                                                    val newList = if (isSelected) {
                                                        AppDataStore.activeKpiList.filter { it != key }
                                                    } else {
                                                        AppDataStore.activeKpiList + key
                                                    }
                                                    AppDataStore.setActiveKpiListSetting(context, newList)
                                                }
                                                .padding(horizontal = 12.dp, vertical = 6.dp),
                                            horizontalArrangement = Arrangement.SpaceBetween,
                                            verticalAlignment = Alignment.CenterVertically
                                        ) {
                                            Row(
                                                verticalAlignment = Alignment.CenterVertically,
                                                horizontalArrangement = Arrangement.spacedBy(10.dp),
                                                modifier = Modifier.weight(1f)
                                            ) {
                                                Icon(icon, contentDescription = null, tint = if (isSelected) MaterialTheme.colorScheme.primary else Color.Gray, modifier = Modifier.size(18.dp))
                                                Text(label, style = MaterialTheme.typography.bodySmall, fontWeight = if (isSelected) FontWeight.SemiBold else FontWeight.Normal)
                                            }
                                            Row(
                                                verticalAlignment = Alignment.CenterVertically,
                                                horizontalArrangement = Arrangement.spacedBy(4.dp)
                                            ) {
                                                if (isSelected) {
                                                    val activeList = AppDataStore.activeKpiList
                                                    val idx = activeList.indexOf(key)
                                                    IconButton(
                                                        onClick = {
                                                            if (idx > 0) {
                                                                val newList = activeList.toMutableList()
                                                                val temp = newList[idx]
                                                                newList[idx] = newList[idx - 1]
                                                                newList[idx - 1] = temp
                                                                AppDataStore.setActiveKpiListSetting(context, newList)
                                                            }
                                                        },
                                                        enabled = idx > 0,
                                                        modifier = Modifier.size(24.dp)
                                                    ) {
                                                        Icon(Icons.Filled.ArrowUpward, contentDescription = "Yukarı Taşı", modifier = Modifier.size(16.dp))
                                                    }
                                                    IconButton(
                                                        onClick = {
                                                            if (idx < activeList.size - 1) {
                                                                val newList = activeList.toMutableList()
                                                                val temp = newList[idx]
                                                                newList[idx] = newList[idx + 1]
                                                                newList[idx + 1] = temp
                                                                AppDataStore.setActiveKpiListSetting(context, newList)
                                                            }
                                                        },
                                                        enabled = idx < activeList.size - 1,
                                                        modifier = Modifier.size(24.dp)
                                                    ) {
                                                        Icon(Icons.Filled.ArrowDownward, contentDescription = "Aşağı Taşı", modifier = Modifier.size(16.dp))
                                                    }
                                                }
                                                Checkbox(
                                                    checked = isSelected,
                                                    onCheckedChange = { checked ->
                                                        val newList = if (!checked) {
                                                            AppDataStore.activeKpiList.filter { it != key }
                                                        } else {
                                                            AppDataStore.activeKpiList + key
                                                        }
                                                        AppDataStore.setActiveKpiListSetting(context, newList)
                                                    },
                                                    modifier = Modifier.size(24.dp)
                                                )
                                            }
                                        }
                                        Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.2f))
                                    }
                                }
                            }
                        }

                        // App Grid Modules Visibility Block
                        item {
                            FieldCard {
                                Column {
                                    Box(
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .background(MaterialTheme.colorScheme.surfaceContainerLowest)
                                            .padding(horizontal = 12.dp, vertical = 8.dp)
                                    ) {
                                        Text(
                                            text = "HIZLI MODÜL GÖSTERİM AYARLARI",
                                            style = MaterialTheme.typography.labelMedium,
                                            color = MaterialTheme.colorScheme.primary,
                                            fontWeight = FontWeight.Bold
                                        )
                                    }
                                    Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))

                                    Text(
                                        text = "Ana sayfada 4'lü ızgara halinde gösterilecek olan mobil işlem modüllerini aktifleştirin veya gizleyin.",
                                        style = MaterialTheme.typography.labelSmall,
                                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                                        modifier = Modifier.padding(12.dp)
                                    )

                                    val baseCompModules = mapOf(
                                        "sales" to "Satış Faturaları",
                                        "suspended_sales" to "Bekleyenler",
                                        "operations/purchase" to "Alış",
                                        "operations/returns" to "İade",
                                        "operations/collection" to "Tahsilat İşlemleri",
                                        "operations/disbursement" to "Tediye İşlemleri",
                                        "operations/cashbox" to "Kasa Defteri",
                                        "operations/eod" to "Gün Sonu Raporu",
                                        "customers" to "Cari Kartlar",
                                        "reports" to "Rapor Analiz",
                                        "operations/stocks" to "Stok Listesi",
                                        "operations/counting" to "Saha Sayımı",
                                        "operations/warehouses" to "Depolar",
                                        "wms_warehouse" to "Depo Modülü (WMS)",
                                        "catalog" to "Ürün Kataloğu",
                                        "operations/approvals" to "Onay Merkezi",
                                        "operations/expenses" to "Şirket Giderleri",
                                        "operations/vehicles" to "Şirket Araçları"
                                    )

                                    val sortedCompModules = remember(AppDataStore.quickActionsOrder) {
                                        val currentOrder = AppDataStore.quickActionsOrder.toMutableList()
                                        baseCompModules.keys.forEach { if (!currentOrder.contains(it)) currentOrder.add(it) }
                                        currentOrder.filter { baseCompModules.containsKey(it) }
                                    }

                                    val listState = remember(sortedCompModules) {
                                        mutableStateListOf<String>().apply { addAll(sortedCompModules) }
                                    }

                                    var draggedItem by remember { mutableStateOf<String?>(null) }
                                    var dragOffset by remember { mutableStateOf(0f) }

                                    val density = androidx.compose.ui.platform.LocalDensity.current
                                    val itemHeightPx = with(density) { 60.dp.toPx() } // Approx row height

                                    Column(modifier = Modifier.fillMaxWidth()) {
                                        listState.forEachIndexed { index, route ->
                                            val label = baseCompModules[route] ?: route
                                            val isVisible = AppDataStore.visibleModules.contains(route)
                                            val isDragged = draggedItem == route
                                            
                                            val yOffset = if (isDragged) dragOffset else 0f
                                            
                                            Row(
                                                modifier = Modifier
                                                    .fillMaxWidth()
                                                    .offset { androidx.compose.ui.unit.IntOffset(0, yOffset.roundToInt()) }
                                                    .zIndex(if (isDragged) 10f else 1f)
                                                    .pointerInput(route) {
                                                        detectDragGesturesAfterLongPress(
                                                            onDragStart = { draggedItem = route; dragOffset = 0f },
                                                            onDrag = { change, dragAmount ->
                                                                change.consume()
                                                                dragOffset += dragAmount.y
                                                                val activeIndex = listState.indexOf(draggedItem)
                                                                if (activeIndex != -1) {
                                                                    val rowDiff = (dragOffset / itemHeightPx).roundToInt()
                                                                    if (rowDiff != 0) {
                                                                        val targetIndex = (activeIndex + rowDiff).coerceIn(0, listState.size - 1)
                                                                        if (targetIndex != activeIndex) {
                                                                            val item = listState.removeAt(activeIndex)
                                                                            listState.add(targetIndex, item)
                                                                            dragOffset -= rowDiff * itemHeightPx
                                                                        }
                                                                    }
                                                                }
                                                            },
                                                            onDragEnd = {
                                                                draggedItem = null
                                                                dragOffset = 0f
                                                                AppDataStore.setQuickActionsOrderSetting(context, listState.toList())
                                                            },
                                                            onDragCancel = { draggedItem = null; dragOffset = 0f }
                                                        )
                                                    }
                                                    .background(if (isDragged) MaterialTheme.colorScheme.surfaceVariant else MaterialTheme.colorScheme.surface)
                                                    .padding(horizontal = 16.dp, vertical = 6.dp),
                                                horizontalArrangement = Arrangement.SpaceBetween,
                                                verticalAlignment = Alignment.CenterVertically
                                            ) {
                                                Icon(
                                                    imageVector = Icons.Filled.DragIndicator,
                                                    contentDescription = "Sırala",
                                                    tint = MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.5f),
                                                    modifier = Modifier.padding(end = 12.dp)
                                                )
                                                Text(
                                                    text = label,
                                                    style = MaterialTheme.typography.bodySmall,
                                                    fontWeight = if (isVisible) FontWeight.SemiBold else FontWeight.Normal,
                                                    modifier = Modifier.weight(1f)
                                                )
                                                Switch(
                                                    checked = isVisible,
                                                    onCheckedChange = { checked ->
                                                        val newSet = if (!checked) {
                                                            AppDataStore.visibleModules.filter { it != route }.toSet()
                                                        } else {
                                                            AppDataStore.visibleModules + route
                                                        }
                                                        AppDataStore.setVisibleModulesSetting(context, newSet)
                                                    }
                                                )
                                            }
                                            if (!isDragged) {
                                                Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.2f))
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // Bottom Navbar configuration Block with Reordering
                        item {
                            FieldCard {
                                Column {
                                    Box(
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .background(MaterialTheme.colorScheme.surfaceContainerLowest)
                                            .padding(horizontal = 12.dp, vertical = 8.dp)
                                    ) {
                                        Text(
                                            text = "ALT MENÜ (NAVBAR) AYARLARI",
                                            style = MaterialTheme.typography.labelMedium,
                                            color = MaterialTheme.colorScheme.primary,
                                            fontWeight = FontWeight.Bold
                                        )
                                    }
                                    Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))

                                    Text(
                                        text = "Alt menüde (navbar) gösterilecek olan 4 ana ekranı seçin ve yön tuşlarıyla sıralayın (Kalanlar diğer kısımdan seçilir).",
                                        style = MaterialTheme.typography.labelSmall,
                                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                                        modifier = Modifier.padding(12.dp)
                                    )

                                    val allTabsOptions = listOf(
                                        "sales" to "Satış Faturaları",
                                        "suspended_sales" to "Bekleyenler",
                                        "operations/purchase" to "Alış",
                                        "operations/returns" to "İade",
                                        "operations/collection" to "Tahsilat İşlemleri",
                                        "operations/disbursement" to "Tediye İşlemleri",
                                        "operations/cashbox" to "Kasa Defteri",
                                        "operations/eod" to "Gün Sonu Raporu",
                                        "customers" to "Cari Kartlar",
                                        "reports" to "Rapor Analiz",
                                        "operations/stocks" to "Stok Listesi",
                                        "operations/counting" to "Saha Sayımı",
                                        "operations/warehouses" to "Depolar",
                                        "wms_warehouse" to "Depo Modülü (WMS)",
                                        "catalog" to "Ürün Kataloğu",
                                        "operations/approvals" to "Onay Merkezi",
                                        "operations/expenses" to "Şirket Giderleri",
                                        "operations/vehicles" to "Şirket Araçları"
                                    )

                                    val activeTabs = AppDataStore.bottomBarTabs
                                    if (activeTabs.isNotEmpty()) {
                                        Text(
                                            text = "AKTİF SIRALAMA (Değiştirmek için yön tuşlarını kullanın):",
                                            style = MaterialTheme.typography.labelSmall,
                                            fontWeight = FontWeight.Bold,
                                            color = MaterialTheme.colorScheme.primary,
                                            modifier = Modifier.padding(horizontal = 12.dp, vertical = 6.dp)
                                        )
                                        
                                        activeTabs.forEachIndexed { idx, route ->
                                            val label = allTabsOptions.firstOrNull { it.first == route }?.second ?: route
                                            Row(
                                                modifier = Modifier
                                                    .fillMaxWidth()
                                                    .padding(horizontal = 12.dp, vertical = 4.dp),
                                                horizontalArrangement = Arrangement.SpaceBetween,
                                                verticalAlignment = Alignment.CenterVertically
                                            ) {
                                                Text(
                                                    text = "${idx + 1}. $label",
                                                    style = MaterialTheme.typography.bodySmall,
                                                    fontWeight = FontWeight.SemiBold
                                                )
                                                Row(horizontalArrangement = Arrangement.spacedBy(4.dp)) {
                                                    IconButton(
                                                        onClick = {
                                                            if (idx > 0) {
                                                                val newList = activeTabs.toMutableList()
                                                                val temp = newList[idx]
                                                                newList[idx] = newList[idx - 1]
                                                                newList[idx - 1] = temp
                                                                AppDataStore.setBottomBarTabsSetting(context, newList)
                                                            }
                                                        },
                                                        enabled = idx > 0,
                                                        modifier = Modifier.size(24.dp)
                                                    ) {
                                                        Icon(Icons.Filled.ArrowUpward, contentDescription = "Yukarı Taşı", modifier = Modifier.size(16.dp))
                                                    }
                                                    IconButton(
                                                        onClick = {
                                                            if (idx < activeTabs.size - 1) {
                                                                val newList = activeTabs.toMutableList()
                                                                val temp = newList[idx]
                                                                newList[idx] = newList[idx + 1]
                                                                newList[idx + 1] = temp
                                                                AppDataStore.setBottomBarTabsSetting(context, newList)
                                                            }
                                                        },
                                                        enabled = idx < activeTabs.size - 1,
                                                        modifier = Modifier.size(24.dp)
                                                    ) {
                                                        Icon(Icons.Filled.ArrowDownward, contentDescription = "Aşağı Taşı", modifier = Modifier.size(16.dp))
                                                    }
                                                }
                                            }
                                            Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.15f), modifier = Modifier.padding(horizontal = 12.dp))
                                        }
                                        Spacer(modifier = Modifier.height(10.dp))
                                    }

                                    Text(
                                        text = "MENÜ EKLE / KALDIR (Tam olarak 4 adet aktif edilmelidir):",
                                        style = MaterialTheme.typography.labelSmall,
                                        fontWeight = FontWeight.Bold,
                                        modifier = Modifier.padding(horizontal = 12.dp, vertical = 4.dp)
                                    )

                                    allTabsOptions.forEach { (route, label) ->
                                        val isSelected = AppDataStore.bottomBarTabs.contains(route)
                                        Row(
                                            modifier = Modifier
                                                .fillMaxWidth()
                                                .clickable {
                                                    val currentList = AppDataStore.bottomBarTabs
                                                    val newList = if (isSelected) {
                                                        if (currentList.size > 4) {
                                                            currentList.filter { it != route }
                                                        } else {
                                                            android.widget.Toast.makeText(context, "Alt menü düzeni için tam olarak 4 adet öğe seçilmelidir.", android.widget.Toast.LENGTH_SHORT).show()
                                                            currentList
                                                        }
                                                    } else {
                                                        if (currentList.size < 4) {
                                                            currentList + route
                                                        } else {
                                                            currentList.drop(1) + route
                                                        }
                                                    }
                                                    AppDataStore.setBottomBarTabsSetting(context, newList)
                                                }
                                                .padding(horizontal = 12.dp, vertical = 8.dp),
                                            horizontalArrangement = Arrangement.SpaceBetween,
                                            verticalAlignment = Alignment.CenterVertically
                                        ) {
                                            Text(
                                                text = label,
                                                style = MaterialTheme.typography.bodySmall,
                                                fontWeight = if (isSelected) FontWeight.SemiBold else FontWeight.Normal,
                                                modifier = Modifier.weight(1f)
                                            )
                                            Checkbox(
                                                checked = isSelected,
                                                onCheckedChange = { checked ->
                                                    val currentList = AppDataStore.bottomBarTabs
                                                    val newList = if (!checked) {
                                                        if (currentList.size > 4) {
                                                            currentList.filter { it != route }
                                                        } else {
                                                            android.widget.Toast.makeText(context, "Alt menü düzeni için tam olarak 4 adet öğe seçilmelidir.", android.widget.Toast.LENGTH_SHORT).show()
                                                            currentList
                                                        }
                                                    } else {
                                                        if (currentList.size < 4) {
                                                            currentList + route
                                                        } else {
                                                            currentList.drop(1) + route
                                                        }
                                                    }
                                                    AppDataStore.setBottomBarTabsSetting(context, newList)
                                                }
                                            )
                                        }
                                        Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.2f))
                                    }
                                }
                            }
                        }
                    }

                    2 -> { // --- TAB 3: FİNANS & SERİ (OPERATIONS, ACCOUNTING AND NUMERATORS) ---
                        // Accounting & Stock Stock Block
                        item {
                            FieldCard {
                                Column {
                                    Box(
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .background(MaterialTheme.colorScheme.surfaceContainerLowest)
                                            .padding(horizontal = 12.dp, vertical = 8.dp)
                                    ) {
                                        Text(
                                            text = "MUHASEBE & STOK AYARLARI",
                                            style = MaterialTheme.typography.labelMedium,
                                            color = MaterialTheme.colorScheme.primary,
                                            fontWeight = FontWeight.Bold
                                        )
                                    }
                                    Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
                                    
                                    Row(
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .padding(12.dp),
                                        horizontalArrangement = Arrangement.SpaceBetween,
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        Column(modifier = Modifier.weight(1f)) {
                                            Text(
                                                text = "Eksi Stok Satışına İzin Ver",
                                                style = MaterialTheme.typography.bodySmall,
                                                fontWeight = FontWeight.Bold,
                                                color = MaterialTheme.colorScheme.onSurface
                                            )
                                            Text(
                                                text = "Kapalıysa, depoda yetersiz olan ürünlerin satış faturası kesilerek eksi bakiyeye düşmesine izin verilmez.",
                                                style = MaterialTheme.typography.labelSmall,
                                                color = MaterialTheme.colorScheme.onSurfaceVariant
                                            )
                                        }
                                        Spacer(modifier = Modifier.width(16.dp))
                                        Switch(
                                            checked = AppDataStore.allowNegativeStock,
                                            onCheckedChange = { AppDataStore.setAllowNegativeStockSetting(context, it) }
                                        )
                                    }
                                }
                            }
                        }

                        // Numerators details configurations Block
                        item {
                            FieldCard {
                                Column {
                                    Box(
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .background(MaterialTheme.colorScheme.surfaceContainerLowest)
                                            .padding(horizontal = 12.dp, vertical = 8.dp)
                                    ) {
                                        Text(
                                            text = "EVRAK SERİ & NUMARATÖR AYARLARI",
                                            style = MaterialTheme.typography.labelMedium,
                                            color = MaterialTheme.colorScheme.primary,
                                            fontWeight = FontWeight.Bold
                                        )
                                    }
                                    Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
                                    
                                    Column(
                                        modifier = Modifier.padding(12.dp),
                                        verticalArrangement = Arrangement.spacedBy(12.dp)
                                    ) {
                                        Text(
                                            text = "Sahada kesilecek evraklara (satış, alış, iade, tahsilat, tediye vb.) ait unikal harf serilerini ve başlangıç sıra numaralarını aşağıdan yapılandırabilirsiniz.",
                                            style = MaterialTheme.typography.labelSmall,
                                            color = MaterialTheme.colorScheme.onSurfaceVariant
                                        )

                                        NumeratorRow(
                                            title = "Satış Serisi & Sıra No",
                                            icon = Icons.Filled.ShoppingCart,
                                            prefix = salesPrefix,
                                            onPrefixChange = { salesPrefix = it },
                                            no = salesNo,
                                            onNoChange = { salesNo = it }
                                        )

                                        Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.3f))

                                        NumeratorRow(
                                            title = "Alış Serisi & Sıra No",
                                            icon = Icons.Filled.Inventory2,
                                            prefix = purchasePrefix,
                                            onPrefixChange = { purchasePrefix = it },
                                            no = purchaseNo,
                                            onNoChange = { purchaseNo = it }
                                        )

                                        Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.3f))

                                        NumeratorRow(
                                            title = "İade Serisi & Sıra No",
                                            icon = Icons.Filled.KeyboardReturn,
                                            prefix = returnsPrefix,
                                            onPrefixChange = { returnsPrefix = it },
                                            no = returnsNo,
                                            onNoChange = { returnsNo = it }
                                        )

                                        Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.3f))

                                        NumeratorRow(
                                            title = "Tahsilat Serisi & Sıra No",
                                            icon = Icons.Filled.Payments,
                                            prefix = collectionPrefix,
                                            onPrefixChange = { collectionPrefix = it },
                                            no = collectionNo,
                                            onNoChange = { collectionNo = it }
                                        )

                                        Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.3f))

                                        NumeratorRow(
                                            title = "Tediye Serisi & Sıra No",
                                            icon = Icons.Filled.AccountBalanceWallet,
                                            prefix = disbursementPrefix,
                                            onPrefixChange = { disbursementPrefix = it },
                                            no = disbursementNo,
                                            onNoChange = { disbursementNo = it }
                                        )

                                        Spacer(modifier = Modifier.height(4.dp))

                                        Button(
                                            onClick = {
                                                scope.launch {
                                                    snackbarHostState.showSnackbar(
                                                        "Tüm numaratör şablonları başarıyla güncellendi ve kaydedildi."
                                                    )
                                                }
                                            },
                                            modifier = Modifier
                                                .fillMaxWidth()
                                                .height(52.dp),
                                            shape = RoundedCornerShape(12.dp),
                                            colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.primary)
                                        ) {
                                            Icon(Icons.Filled.Save, contentDescription = null, modifier = Modifier.size(20.dp))
                                            Spacer(modifier = Modifier.width(8.dp))
                                            Text("Numaratörleri Kaydet & Kilitle", fontWeight = FontWeight.Bold)
                                        }
                                    }
                                }
                            }
                        }

                        // Bank Details Block
                        item {
                            var showAddBankDialog by remember { mutableStateOf(false) }
                            var newBankName by remember { mutableStateOf("") }
                            var newBankAccountNo by remember { mutableStateOf("") }
                            var newBankIban by remember { mutableStateOf("") }
                            var newBankInitialBalance by remember { mutableStateOf("") }

                            FieldCard {
                                Column(modifier = Modifier.padding(bottom = 12.dp)) {
                                    Box(
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .background(MaterialTheme.colorScheme.surfaceContainerLowest)
                                            .padding(horizontal = 12.dp, vertical = 8.dp)
                                    ) {
                                        Row(
                                            modifier = Modifier.fillMaxWidth(),
                                            horizontalArrangement = Arrangement.SpaceBetween,
                                            verticalAlignment = Alignment.CenterVertically
                                        ) {
                                            Text(
                                                text = "SİSTEM BANKA VE KASA HESAPLARI",
                                                style = MaterialTheme.typography.labelMedium,
                                                color = MaterialTheme.colorScheme.primary,
                                                fontWeight = FontWeight.Bold
                                            )
                                            val isErp = AppDataStore.isErpModeActive(context)
                                            if (isErp) {
                                                Text(
                                                    text = "SALT OKUNUR (ERP)",
                                                    style = MaterialTheme.typography.labelSmall,
                                                    color = MaterialTheme.colorScheme.secondary,
                                                    fontWeight = FontWeight.Bold,
                                                    modifier = Modifier.padding(end = 4.dp)
                                                )
                                            } else {
                                                TextButton(
                                                    onClick = {
                                                        newBankName = ""
                                                        newBankAccountNo = ""
                                                        newBankIban = ""
                                                        newBankInitialBalance = ""
                                                        showAddBankDialog = true
                                                    }
                                                ) {
                                                    Icon(Icons.Filled.Add, contentDescription = "Ekle", modifier = Modifier.size(16.dp))
                                                    Spacer(modifier = Modifier.width(4.dp))
                                                    Text("Banka Ekle", style = MaterialTheme.typography.labelMedium)
                                                }
                                            }
                                        }
                                    }
                                    Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
                                    
                                    // Bank Accounts List
                                    if (AppDataStore.banks.isEmpty()) {
                                        Box(
                                            modifier = Modifier
                                                .fillMaxWidth()
                                                .padding(24.dp),
                                            contentAlignment = Alignment.Center
                                        ) {
                                            Text("Kayıtlı banka hesabı bulunmamaktadır.", color = Color.Gray, style = MaterialTheme.typography.bodyMedium)
                                        }
                                    } else {
                                        AppDataStore.banks.forEach { bank ->
                                            Column(
                                                modifier = Modifier
                                                    .fillMaxWidth()
                                                    .padding(horizontal = 16.dp, vertical = 8.dp)
                                            ) {
                                                Row(
                                                    modifier = Modifier.fillMaxWidth(),
                                                    horizontalArrangement = Arrangement.SpaceBetween,
                                                    verticalAlignment = Alignment.CenterVertically
                                                ) {
                                                    Column {
                                                        Text(bank.name, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodyMedium)
                                                        Text("Hesap No: ${bank.accountNo} | IBAN: ${bank.iban}", style = MaterialTheme.typography.bodySmall, color = Color.Gray)
                                                    }
                                                    Text(
                                                        String.format("%,.2f ₺", bank.balance),
                                                        color = MaterialTheme.colorScheme.secondary,
                                                        fontWeight = FontWeight.Bold,
                                                        style = MaterialTheme.typography.bodyMedium
                                                    )
                                                }
                                            }
                                            Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.3f), modifier = Modifier.padding(horizontal = 16.dp))
                                        }
                                    }
                                }
                            }

                            // Add Bank Dialog
                            if (showAddBankDialog) {
                                AlertDialog(
                                    onDismissRequest = { showAddBankDialog = false },
                                    title = { Text("Yeni Banka Tanımla") },
                                    text = {
                                        Column(verticalArrangement = Arrangement.spacedBy(12.dp)) {
                                            OutlinedTextField(
                                                value = newBankName,
                                                onValueChange = { newBankName = it },
                                                label = { Text("Banka Şube / Hesap Adı") },
                                                modifier = Modifier.fillMaxWidth()
                                            )
                                            OutlinedTextField(
                                                value = newBankAccountNo,
                                                onValueChange = { newBankAccountNo = it },
                                                label = { Text("Hesap Numarası") },
                                                modifier = Modifier.fillMaxWidth()
                                            )
                                            OutlinedTextField(
                                                value = newBankIban,
                                                onValueChange = { newBankIban = it },
                                                label = { Text("IBAN Numarası") },
                                                modifier = Modifier.fillMaxWidth()
                                            )
                                            OutlinedTextField(
                                                value = newBankInitialBalance,
                                                onValueChange = { newBankInitialBalance = it },
                                                label = { Text("İlk Kasa Bakiyesi (₺)") },
                                                modifier = Modifier.fillMaxWidth()
                                            )
                                        }
                                    },
                                    confirmButton = {
                                        Button(
                                            onClick = {
                                                if (newBankName.isNotBlank()) {
                                                    val initialBal = newBankInitialBalance.toDoubleOrNull() ?: 0.0
                                                    val generatedId = "B-" + (100 + AppDataStore.banks.size)
                                                    AppDataStore.banks.add(
                                                        Bank(
                                                            id = generatedId,
                                                            name = newBankName,
                                                            accountNo = newBankAccountNo,
                                                            iban = newBankIban,
                                                            balance = initialBal
                                                        )
                                                    )
                                                    scope.launch {
                                                        snackbarHostState.showSnackbar("${newBankName} hesabı başarıyla eklendi.")
                                                    }
                                                    showAddBankDialog = false
                                                }
                                            }
                                        ) {
                                            Text("Banka Kartını Aç")
                                        }
                                    },
                                    dismissButton = {
                                        TextButton(onClick = { showAddBankDialog = false }) {
                                            Text("Vazgeç")
                                        }
                                    }
                                )
                            }
                        }
                    }

                    3 -> { // --- TAB 4: SİSTEM & VERİ (APP PREFERENCES, DATA SYNC AND CORES) ---
                        // App Preferences Block
                        item {
                            FieldCard {
                                Column {
                                    Box(
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .background(MaterialTheme.colorScheme.surfaceContainerLowest)
                                            .padding(horizontal = 12.dp, vertical = 8.dp)
                                    ) {
                                        Text(
                                            text = "UYGULAMA TERCİHLERİ",
                                            style = MaterialTheme.typography.labelMedium,
                                            color = MaterialTheme.colorScheme.primary,
                                            fontWeight = FontWeight.Bold
                                        )
                                    }
                                    Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
                                    SettingsItem(
                                        icon = Icons.Filled.Map,
                                        title = "Bölge Seçimi",
                                        subtitle = "Kuzey Marmara Sektörü"
                                    )
                                    Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
                                    SettingsItem(
                                        icon = Icons.Filled.Language,
                                        title = "Dil Seçeneği",
                                        subtitle = "Türkçe (Varsayılan)"
                                    )
                                }
                            }
                        }

                        // Sync Block
                        item {
                            FieldCard {
                                Column {
                                    Box(
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .background(MaterialTheme.colorScheme.surfaceContainerLowest)
                                            .padding(horizontal = 12.dp, vertical = 8.dp)
                                    ) {
                                        Text(
                                            text = "VERI & SENKRONİZASYON",
                                            style = MaterialTheme.typography.labelMedium,
                                            color = MaterialTheme.colorScheme.primary,
                                            fontWeight = FontWeight.Bold
                                        )
                                    }
                                    Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
                                    SettingsItem(
                                        icon = Icons.Filled.Sync,
                                        title = "Senkronizasyon Ayarları",
                                        subtitle = "Sadece Wi-Fi (Otomatik)",
                                        onClick = { navController.navigate("offline_sync") }
                                    )
                                    Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
                                    SettingsItem(
                                        icon = Icons.Filled.Dns,
                                        title = "ERP Entegrasyon Merkezi",
                                        subtitle = "Mikro, Logo, Paraşüt, BizimHesap, Bilnex",
                                        onClick = { navController.navigate("erp_integration") }
                                    )
                                    Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
                                    SettingsItem(
                                        icon = Icons.Filled.ImportExport,
                                        title = "Excel İle İçe/Dışa Aktar",
                                        subtitle = "Cari ve Stok verilerini Excel formatı ile yükle/indir",
                                        onClick = { navController.navigate("import_export") }
                                    )
                                    Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
                                    TextButton(
                                        onClick = { navController.navigate("offline_sync") },
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .padding(12.dp)
                                    ) {
                                        Icon(Icons.Filled.CloudSync, contentDescription = null, modifier = Modifier.size(16.dp))
                                        Spacer(modifier = Modifier.width(6.dp))
                                        Text("Şimdi Senkronize Et", style = MaterialTheme.typography.labelMedium)
                                    }
                                }
                            }
                        }

                        // Security System & Permissions
                        item {
                            FieldCard {
                                Column {
                                    Box(
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .background(MaterialTheme.colorScheme.surfaceContainerLowest)
                                            .padding(horizontal = 12.dp, vertical = 8.dp)
                                    ) {
                                        Text(
                                            text = "SİSTEM GÜVENLİĞİ & YETKİLENDİRME",
                                            style = MaterialTheme.typography.labelMedium,
                                            color = MaterialTheme.colorScheme.primary,
                                            fontWeight = FontWeight.Bold
                                        )
                                    }
                                    Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
                                    SettingsItem(
                                        icon = Icons.Filled.Security,
                                        title = "Rol & Yetki Matrisi",
                                        subtitle = "Saha ekibi yetkilerini ve engellerini simüle et",
                                        onClick = { navController.navigate("security") }
                                    )
                                    Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
                                    SettingsItem(
                                        icon = Icons.Filled.PhonelinkLock,
                                        title = "Cihaz Kilitleme & Donanım",
                                        subtitle = "Tek cihaz donanım kilidi (Device Binding) ayarları",
                                        onClick = { navController.navigate("security") }
                                    )
                                }
                            }
                        }

                        // App Version Badge under Tab Items
                        item {
                            Box(
                                modifier = Modifier.fillMaxWidth().padding(vertical = 16.dp),
                                contentAlignment = Alignment.Center
                            ) {
                                Text(
                                    text = "Saha Gücü - Klasik Sürüm ${com.example.BuildConfig.VERSION_NAME} (Build ${com.example.BuildConfig.VERSION_CODE})",
                                    style = MaterialTheme.typography.labelSmall,
                                    color = MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.5f),
                                    fontWeight = FontWeight.Bold
                                )
                            }
                        }
                    }

                    4 -> { // --- TAB 5: ONAY MERKEZİ (APPROVALS CONFIGURATION) ---
                        item {
                            FieldCard {
                                Column {
                                    Box(
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .background(MaterialTheme.colorScheme.surfaceContainerLowest)
                                            .padding(horizontal = 12.dp, vertical = 8.dp)
                                    ) {
                                        Text(
                                            text = "OTOMATİK ONAY SİSTEMİ",
                                            style = MaterialTheme.typography.labelMedium,
                                            color = MaterialTheme.colorScheme.primary,
                                            fontWeight = FontWeight.Bold
                                        )
                                    }
                                    Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))

                                    Text(
                                        text = "Sahadan gönderilen evrakların onay süreçlerini bu alandan yapılandırabilirsiniz.",
                                        style = MaterialTheme.typography.labelSmall,
                                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                                        modifier = Modifier.padding(12.dp)
                                    )

                                    // Option 1: Send to Approval Center (Manual Approval)
                                    Row(
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .clickable {
                                                AppDataStore.autoApproveAllTransactions = false
                                                AppDataStore.sendToApprovalCenterDirectly = true
                                            }
                                            .padding(horizontal = 12.dp, vertical = 10.dp),
                                        verticalAlignment = Alignment.CenterVertically,
                                        horizontalArrangement = Arrangement.spacedBy(10.dp)
                                    ) {
                                        RadioButton(
                                            selected = !AppDataStore.autoApproveAllTransactions,
                                            onClick = {
                                                AppDataStore.autoApproveAllTransactions = false
                                                AppDataStore.sendToApprovalCenterDirectly = true
                                            }
                                        )
                                        Column(modifier = Modifier.weight(1f)) {
                                            Text(
                                                text = "Onay Merkezine Gönder (Manuel Kontrol)",
                                                style = MaterialTheme.typography.bodySmall,
                                                fontWeight = FontWeight.Bold,
                                                color = MaterialTheme.colorScheme.onSurface
                                            )
                                            Text(
                                                text = "Müşteri risk limiti aşımları, ekstra iskontolar vb. durumlarda evraklar Onay Merkezi havuzuna düşer ve yönetici onayı beklenir.",
                                                style = MaterialTheme.typography.labelSmall,
                                                color = MaterialTheme.colorScheme.onSurfaceVariant
                                            )
                                        }
                                    }

                                    Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.2f))

                                    // Option 2: Auto Approve All Transactions
                                    Row(
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .clickable {
                                                AppDataStore.autoApproveAllTransactions = true
                                                AppDataStore.sendToApprovalCenterDirectly = false
                                            }
                                            .padding(horizontal = 12.dp, vertical = 10.dp),
                                        verticalAlignment = Alignment.CenterVertically,
                                        horizontalArrangement = Arrangement.spacedBy(10.dp)
                                    ) {
                                        RadioButton(
                                            selected = AppDataStore.autoApproveAllTransactions,
                                            onClick = {
                                                AppDataStore.autoApproveAllTransactions = true
                                                AppDataStore.sendToApprovalCenterDirectly = false
                                            }
                                        )
                                        Column(modifier = Modifier.weight(1f)) {
                                            Text(
                                                text = "Tüm Evrakları & Faturaları Otomatik Onayla",
                                                style = MaterialTheme.typography.bodySmall,
                                                fontWeight = FontWeight.Bold,
                                                color = MaterialTheme.colorScheme.onSurface
                                            )
                                            Text(
                                                text = "Sahaya düşen Satış, Tahsilat, İade, Alış ve Tediye belgeleri dahil tüm fatura biçimleri onay merkezine gitmeden otomatik olarak doğrudan onaylanır.",
                                                style = MaterialTheme.typography.labelSmall,
                                                color = MaterialTheme.colorScheme.onSurfaceVariant
                                            )
                                        }
                                    }
                                }
                            }
                        }

                        // Details explanation card about system approvals
                        item {
                            FieldCard {
                                Column(modifier = Modifier.padding(12.dp)) {
                                    Row(
                                        verticalAlignment = Alignment.CenterVertically,
                                        horizontalArrangement = Arrangement.spacedBy(8.dp)
                                    ) {
                                        Icon(
                                            Icons.Filled.VerifiedUser,
                                            contentDescription = null,
                                            tint = MaterialTheme.colorScheme.primary,
                                            modifier = Modifier.size(18.dp)
                                        )
                                        Text(
                                            text = "Onaylanan Belge Türleri",
                                            style = MaterialTheme.typography.titleSmall,
                                            fontWeight = FontWeight.Bold,
                                            color = MaterialTheme.colorScheme.primary
                                        )
                                    }
                                    Spacer(modifier = Modifier.height(6.dp))
                                    Text(
                                        text = "Otomatik onay seçeneği aktif olduğunda aşağıdaki tüm işlem türleri anında onaylanır ve doğrudan ERP entegrasyon merkezine sevk edilir:",
                                        style = MaterialTheme.typography.labelSmall,
                                        color = MaterialTheme.colorScheme.onSurfaceVariant
                                    )
                                    Spacer(modifier = Modifier.height(6.dp))
                                    
                                    val documentTypes = listOf(
                                        "Satış Faturaları (Sipariş limit aşımları)",
                                        "Tahsilat Girişleri (Elden tahsilat, Çek/Senet ciro)",
                                        "İade Faturaları (Hasarlı/Kırık ürün iadeleri)",
                                        "Alış Siparişleri (Tedarikçi yüksek alımları)",
                                        "Tediye Girişleri (Ödeme ve gider makbuzları)"
                                    )
                                    
                                    documentTypes.forEach { type ->
                                        Row(
                                            verticalAlignment = Alignment.CenterVertically,
                                            horizontalArrangement = Arrangement.spacedBy(6.dp),
                                            modifier = Modifier.padding(vertical = 1.dp)
                                        ) {
                                            Icon(
                                                Icons.Filled.Check,
                                                contentDescription = null,
                                                tint = Color(0xFF4CAF50),
                                                modifier = Modifier.size(10.dp)
                                            )
                                            Text(
                                                text = type,
                                                style = MaterialTheme.typography.labelSmall,
                                                color = MaterialTheme.colorScheme.onSurface
                                            )
                                        }
                                    }
                                }
                            }
                        }
                    }
                    5 -> { // --- TAB 5: TANIMLAMALAR ---
                        item {
                            DefinitionsTab(snackbarHostState)
                        }
                    }
                }
            }
        }
    }
}

@Composable
fun DefinitionsTab(snackbarHostState: SnackbarHostState) {
    val context = androidx.compose.ui.platform.LocalContext.current
    var selectedDefinitionKey by remember { mutableStateOf("Banka") }
    var searchQuery by remember { mutableStateOf("") }
    var definitionValueInput by remember { mutableStateOf("") }
    val scope = rememberCoroutineScope()

    val currentList = AppDataStore.definitions[selectedDefinitionKey] ?: emptyList()
    val filteredList = if (searchQuery.isBlank()) currentList else {
        currentList.filter { it.contains(searchQuery, ignoreCase = true) }
    }

    Column(modifier = Modifier.fillMaxWidth(), verticalArrangement = Arrangement.spacedBy(16.dp)) {
        // Dropdown to select which definition to edit
        FieldCard {
            Column(modifier = Modifier.padding(12.dp), verticalArrangement = Arrangement.spacedBy(8.dp)) {
                Text(
                    text = "Tanımlama Türü Seçin",
                    style = MaterialTheme.typography.titleSmall,
                    fontWeight = FontWeight.Bold,
                    color = MaterialTheme.colorScheme.primary
                )
                
                var expanded by remember { mutableStateOf(false) }
                Box(modifier = Modifier.fillMaxWidth()) {
                    OutlinedButton(
                        onClick = { expanded = true },
                        modifier = Modifier.fillMaxWidth(),
                        contentPadding = PaddingValues(horizontal = 16.dp, vertical = 12.dp)
                    ) {
                        Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.SpaceBetween, modifier = Modifier.fillMaxWidth()) {
                            Text(selectedDefinitionKey)
                            Icon(Icons.Filled.ArrowDropDown, contentDescription = null)
                        }
                    }
                    DropdownMenu(
                        expanded = expanded,
                        onDismissRequest = { expanded = false },
                        modifier = Modifier.fillMaxWidth(0.9f)
                    ) {
                        AppDataStore.definitions.keys.forEach { key ->
                            DropdownMenuItem(
                                text = { Text(key) },
                                onClick = {
                                    selectedDefinitionKey = key
                                    expanded = false
                                }
                            )
                        }
                    }
                }
            }
        }

        // Add New Definition
        val isErp = AppDataStore.isErpModeActive(context)
        val isReadOnlyDef = isErp && (selectedDefinitionKey == "Fiyat" || selectedDefinitionKey == "Banka")

        if (isReadOnlyDef) {
            FieldCard {
                Row(
                    modifier = Modifier.fillMaxWidth().padding(12.dp),
                    verticalAlignment = Alignment.CenterVertically,
                    horizontalArrangement = Arrangement.spacedBy(10.dp)
                ) {
                    Icon(
                        imageVector = Icons.Filled.Lock,
                        contentDescription = "Salt Okunur",
                        tint = MaterialTheme.colorScheme.secondary,
                        modifier = Modifier.size(20.dp)
                    )
                    Text(
                        text = "ERP Entegrasyon Modu Aktif. $selectedDefinitionKey Tanımları sadece ERP Entegrasyon Merkezinden otomatik olarak eşitlenebilir ve güncellenebilir. Yerel değişiklik kilitlidir.",
                        style = MaterialTheme.typography.bodySmall,
                        color = MaterialTheme.colorScheme.secondary,
                        fontWeight = FontWeight.Medium
                    )
                }
            }
        } else {
            FieldCard {
                Column(modifier = Modifier.padding(12.dp), verticalArrangement = Arrangement.spacedBy(8.dp)) {
                    Text(
                        text = "Yeni Ekle",
                        style = MaterialTheme.typography.titleSmall,
                        fontWeight = FontWeight.Bold,
                        color = MaterialTheme.colorScheme.primary
                    )
                    Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                        OutlinedTextField(
                            value = definitionValueInput,
                            onValueChange = { definitionValueInput = it },
                            modifier = Modifier.weight(1f),
                            placeholder = { Text("$selectedDefinitionKey Adı") },
                            singleLine = true
                        )
                        Button(
                            onClick = {
                                if (definitionValueInput.isNotBlank()) {
                                    val current = AppDataStore.definitions[selectedDefinitionKey]?.toMutableList() ?: mutableListOf()
                                    if (!current.contains(definitionValueInput)) {
                                        current.add(definitionValueInput)
                                        AppDataStore.definitions[selectedDefinitionKey] = current
                                        AppDataStore.persist(context)
                                        definitionValueInput = ""
                                        scope.launch { snackbarHostState.showSnackbar("Eklendi.") }
                                    }
                                }
                            }
                        ) {
                            Icon(Icons.Filled.Add, contentDescription = null)
                            Spacer(Modifier.width(4.dp))
                            Text("Ekle")
                        }
                    }
                }
            }
        }

        // List
        FieldCard {
            Column(modifier = Modifier.padding(12.dp), verticalArrangement = Arrangement.spacedBy(8.dp)) {
                Text(
                    text = "Kayıtlı $selectedDefinitionKey Listesi",
                    style = MaterialTheme.typography.titleSmall,
                    fontWeight = FontWeight.Bold,
                    color = MaterialTheme.colorScheme.primary
                )
                OutlinedTextField(
                    value = searchQuery,
                    onValueChange = { searchQuery = it },
                    modifier = Modifier.fillMaxWidth(),
                    placeholder = { Text("İsimle Ara...") },
                    leadingIcon = { Icon(Icons.Filled.Search, contentDescription = null) },
                    singleLine = true
                )
                
                Divider(modifier = Modifier.padding(vertical = 4.dp), color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
                
                if (filteredList.isEmpty()) {
                    Text("Kayıt bulunamadı.", modifier = Modifier.padding(8.dp), color = Color.Gray, style = MaterialTheme.typography.bodySmall)
                } else {
                            var itemToDelete by remember { mutableStateOf<String?>(null) }
                            var showTransferDialog by remember { mutableStateOf(false) }
                            var transferTarget by remember { mutableStateOf("") }
                            
                            if (showTransferDialog && itemToDelete != null) {
                                val currentKey = selectedDefinitionKey
                                val oldItem = itemToDelete!!
                                AlertDialog(
                                    onDismissRequest = { showTransferDialog = false },
                                    title = { Text("Tanım Kullanımda", fontWeight = FontWeight.Bold) },
                                    text = {
                                        Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                                            Text(
                                                "Simek istediğiniz '$oldItem' tanımı bazı kayıtlarda kullanımda!\n\n" +
                                                "Mevcut kayıtların bozulmaması için, kullanımda olan kayıtları başka bir tanıma aktarmanız gerekmektedir. Lütfen aktarmak için hedef seçin:"
                                            )
                                            var transferExpanded by remember { mutableStateOf(false) }
                                            Box(modifier = Modifier.fillMaxWidth()) {
                                                OutlinedTextField(
                                                    value = transferTarget,
                                                    onValueChange = {},
                                                    readOnly = true,
                                                    label = { Text("Yeni Tanım (Hedef)") },
                                                    trailingIcon = {
                                                        IconButton(onClick = { transferExpanded = true }) {
                                                            Icon(Icons.Filled.ArrowDropDown, null)
                                                        }
                                                    },
                                                    modifier = Modifier.fillMaxWidth().clickable { transferExpanded = true }
                                                )
                                                DropdownMenu(expanded = transferExpanded, onDismissRequest = { transferExpanded = false }) {
                                                    val others = currentList.filter { it != oldItem }
                                                    others.forEach { target ->
                                                        DropdownMenuItem(
                                                            text = { Text(target) },
                                                            onClick = { transferTarget = target; transferExpanded = false }
                                                        )
                                                    }
                                                }
                                            }
                                        }
                                    },
                                    confirmButton = {
                                        Button(
                                            onClick = {
                                                if (transferTarget.isNotEmpty()) {
                                                    // Move logic
                                                    when (currentKey) {
                                                        "Kategori" -> {
                                                            for (i in AppDataStore.products.indices) {
                                                                if (AppDataStore.products[i].category == oldItem) {
                                                                    AppDataStore.products[i] = AppDataStore.products[i].copy(category = transferTarget)
                                                                }
                                                            }
                                                        }
                                                        "Depo" -> {
                                                            for (i in AppDataStore.products.indices) {
                                                                val p = AppDataStore.products[i]
                                                                if (p.stockByWarehouse.containsKey(oldItem)) {
                                                                    val qty = p.stockByWarehouse[oldItem] ?: 0
                                                                    val newMap = p.stockByWarehouse.toMutableMap()
                                                                    newMap.remove(oldItem)
                                                                    newMap[transferTarget] = (newMap[transferTarget] ?: 0) + qty
                                                                    AppDataStore.products[i] = p.copy(stockByWarehouse = newMap)
                                                                }
                                                            }
                                                        }
                                                        "KDV" -> {
                                                            val targetKdvInt = transferTarget.replace("%", "").toIntOrNull() ?: 20
                                                            for (i in AppDataStore.products.indices) {
                                                                if (AppDataStore.products[i].kdvPercent.toString() == oldItem.replace("%", "")) {
                                                                    AppDataStore.products[i] = AppDataStore.products[i].copy(kdvPercent = targetKdvInt)
                                                                }
                                                            }
                                                        }
                                                        "Müşteri", "Fiyat" -> {
                                                            for (i in AppDataStore.customers.indices) {
                                                                if (AppDataStore.customers[i].priceGroup == oldItem) {
                                                                    AppDataStore.customers[i] = AppDataStore.customers[i].copy(priceGroup = transferTarget)
                                                                }
                                                            }
                                                        }
                                                        "Banka" -> {
                                                            for (i in AppDataStore.banks.indices) {
                                                                if (AppDataStore.banks[i].name == oldItem) {
                                                                    AppDataStore.banks[i] = AppDataStore.banks[i].copy(name = transferTarget)
                                                                }
                                                            }
                                                        }
                                                    }
                                                    
                                                    // Now safe to delete
                                                    val list = AppDataStore.definitions[currentKey]?.toMutableList() ?: mutableListOf()
                                                    list.remove(oldItem)
                                                    AppDataStore.definitions[currentKey] = list
                                                    
                                                    AppDataStore.persist(context)
                                                    showTransferDialog = false
                                                    itemToDelete = null
                                                    scope.launch { snackbarHostState.showSnackbar("Kayıtlar aktarıldı ve silindi.") }
                                                }
                                            },
                                            enabled = transferTarget.isNotEmpty()
                                        ) {
                                            Text("Aktar ve Sil")
                                        }
                                    },
                                    dismissButton = {
                                        TextButton(onClick = { showTransferDialog = false; itemToDelete = null }) { Text("İptal") }
                                    }
                                )
                            }
                            
                            filteredList.forEach { item ->
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(vertical = 4.dp),
                            horizontalArrangement = Arrangement.SpaceBetween,
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Text(text = item, style = MaterialTheme.typography.bodyMedium)
                            if (isReadOnlyDef) {
                                Icon(
                                    imageVector = Icons.Filled.Lock,
                                    contentDescription = "Salt Okunur",
                                    tint = MaterialTheme.colorScheme.outline.copy(alpha = 0.5f),
                                    modifier = Modifier.size(18.dp)
                                )
                            } else {
                                IconButton(
                                    onClick = {
                                        val inUseCount = when (selectedDefinitionKey) {
                                            "Kategori" -> AppDataStore.products.count { it.category == item }
                                            "Depo" -> AppDataStore.products.count { it.stockByWarehouse.containsKey(item) }
                                            "KDV" -> AppDataStore.products.count { it.kdvPercent.toString() == item.replace("%", "") }
                                            "Fiyat", "Müşteri" -> AppDataStore.customers.count { it.priceGroup == item }
                                            "Banka" -> AppDataStore.banks.count { it.name == item }
                                            else -> 0
                                        }
                                        
                                        if (inUseCount > 0) {
                                            itemToDelete = item
                                            transferTarget = ""
                                            showTransferDialog = true
                                        } else {
                                            val current = AppDataStore.definitions[selectedDefinitionKey]?.toMutableList() ?: mutableListOf()
                                            current.remove(item)
                                            AppDataStore.definitions[selectedDefinitionKey] = current
                                            AppDataStore.persist(context)
                                            scope.launch { snackbarHostState.showSnackbar("Silindi.") }
                                        }
                                    },
                                    modifier = Modifier.size(24.dp)
                                ) {
                                    Icon(Icons.Filled.Delete, contentDescription = null, tint = MaterialTheme.colorScheme.error)
                                }
                            }
                        }
                        Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.3f))
                    }
                }
            }
        }
    }
}

@Composable
fun SettingsItem(
    icon: androidx.compose.ui.graphics.vector.ImageVector,
    title: String,
    subtitle: String,
    onClick: () -> Unit = {}
) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .clickable { onClick() }
            .padding(horizontal = 12.dp, vertical = 8.dp),
        verticalAlignment = Alignment.CenterVertically
    ) {
        Box(
            modifier = Modifier
                .size(34.dp)
                .background(MaterialTheme.colorScheme.surfaceVariant, CircleShape),
            contentAlignment = Alignment.Center
        ) {
            Icon(icon, contentDescription = null, tint = MaterialTheme.colorScheme.onSurfaceVariant, modifier = Modifier.size(18.dp))
        }
        Spacer(modifier = Modifier.width(12.dp))
        Column(modifier = Modifier.weight(1f)) {
            Text(
                text = title,
                style = MaterialTheme.typography.bodySmall,
                fontWeight = FontWeight.SemiBold,
                color = MaterialTheme.colorScheme.onSurface
            )
            Text(
                text = subtitle,
                style = MaterialTheme.typography.labelSmall,
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )
        }
        Icon(
            Icons.Filled.ChevronRight,
            contentDescription = null,
            tint = MaterialTheme.colorScheme.onSurfaceVariant,
            modifier = Modifier.size(16.dp)
        )
    }
}

@Composable
fun ProfileInfoRow(label: String, value: String) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .padding(horizontal = 12.dp, vertical = 6.dp),
        horizontalArrangement = Arrangement.SpaceBetween,
        verticalAlignment = Alignment.CenterVertically
    ) {
        Text(
            text = label,
            style = MaterialTheme.typography.labelSmall,
            color = Color.Gray
        )
        Text(
            text = value,
            style = MaterialTheme.typography.bodySmall,
            fontWeight = FontWeight.SemiBold,
            color = MaterialTheme.colorScheme.onSurface
        )
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun NumeratorRow(
    title: String,
    icon: androidx.compose.ui.graphics.vector.ImageVector,
    prefix: String,
    onPrefixChange: (String) -> Unit,
    no: String,
    onNoChange: (String) -> Unit
) {
    Column(
        modifier = Modifier.fillMaxWidth(),
        verticalArrangement = Arrangement.spacedBy(8.dp)
    ) {
        Row(
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(6.dp)
        ) {
            Icon(
                icon,
                contentDescription = null,
                tint = MaterialTheme.colorScheme.primary,
                modifier = Modifier.size(16.dp)
            )
            Text(
                text = title,
                style = MaterialTheme.typography.titleSmall,
                fontWeight = FontWeight.Bold,
                color = MaterialTheme.colorScheme.onSurface
            )
        }
        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.spacedBy(8.dp)
        ) {
            OutlinedTextField(
                value = prefix,
                onValueChange = onPrefixChange,
                label = { Text("Seri", fontSize = 10.sp) },
                placeholder = { Text("AAA", fontSize = 12.sp) },
                singleLine = true,
                textStyle = MaterialTheme.typography.bodySmall,
                modifier = Modifier.weight(1f).height(52.dp),
                colors = OutlinedTextFieldDefaults.colors(
                    focusedBorderColor = MaterialTheme.colorScheme.primary,
                    unfocusedBorderColor = MaterialTheme.colorScheme.outlineVariant
                )
            )
            OutlinedTextField(
                value = no,
                onValueChange = onNoChange,
                label = { Text("Mevcut Sıra No", fontSize = 10.sp) },
                placeholder = { Text("00000001", fontSize = 12.sp) },
                singleLine = true,
                textStyle = MaterialTheme.typography.bodySmall,
                modifier = Modifier.weight(2.2f).height(52.dp),
                colors = OutlinedTextFieldDefaults.colors(
                    focusedBorderColor = MaterialTheme.colorScheme.primary,
                    unfocusedBorderColor = MaterialTheme.colorScheme.outlineVariant
                )
            )
        }
    }
}

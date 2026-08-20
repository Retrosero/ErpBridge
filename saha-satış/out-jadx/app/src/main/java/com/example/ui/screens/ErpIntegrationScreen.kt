package com.example.ui.screens

import android.content.Context
import android.widget.Toast
import androidx.compose.animation.AnimatedContent
import androidx.compose.animation.core.tween
import androidx.compose.animation.fadeIn
import androidx.compose.animation.fadeOut
import androidx.compose.animation.togetherWith
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.lazy.itemsIndexed
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.navigation.NavController
import com.example.data.LicenseRepository
import kotlinx.coroutines.delay
import kotlinx.coroutines.launch

import com.example.util.SyncManager
import com.example.util.SyncTask


@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ErpIntegrationScreen(navController: NavController) {
    val context = LocalContext.current
    val scope = rememberCoroutineScope()
    val sharedPrefs = remember { context.getSharedPreferences("erp_settings", Context.MODE_PRIVATE) }

    var activeTab by remember { mutableStateOf(0) }

    var apiUrl by remember { mutableStateOf(LicenseRepository.getBaseUrl(context)) }
    var apiKey by remember { mutableStateOf(LicenseRepository.getApiKey(context) ?: sharedPrefs.getString("api_key", "") ?: "") }
    var tenantId by remember { mutableStateOf(LicenseRepository.getTenantId(context) ?: sharedPrefs.getString("tenant_id", "") ?: "") }
    
    val consoleLogs by SyncManager.syncLogs.collectAsState()
    
    val isSyncAllRunning by SyncManager.isSyncing.collectAsState()
    val isSyncAllFinished by SyncManager.isSyncAllFinished.collectAsState()
    val currentSyncTaskIndex by SyncManager.currentSyncTaskIndex.collectAsState()
    val currentSyncTaskName by SyncManager.currentSyncTaskName.collectAsState()
    val currentSyncTaskProgress by SyncManager.syncProgress.collectAsState()
    val currentSyncStats by SyncManager.currentSyncStats.collectAsState()
    
    val syncTasks = remember {
        listOf(
            object : SyncTask() {
                override val name = "Cari Kartlar"
                override val description = "CARI_HESAPLAR kopyalanıyor..."
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {
                    BridgeSyncHelper.syncCariler(ctx, url, key, log, progress)
                }
            },
            object : SyncTask() {
                override val name = "Stok Kartları"
                override val description = "STOKLAR kopyalanıyor..."
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {
                    BridgeSyncHelper.syncUrunler(ctx, url, key, log, progress)
                }
            },
            object : SyncTask() {
                override val name = "Cari Hesap Hareketleri"
                override val description = "CARI_HESAP_HAREKETLERI kopyalanıyor..."
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {
                    BridgeSyncHelper.syncCariHareketleri(ctx, url, key, log, progress)
                }
            },
            object : SyncTask() {
                override val name = "Fatura Detayları & Kalemler"
                override val description = "faturaHareket kopyalanıyor..."
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {
                    BridgeSyncHelper.syncFaturaHareket(ctx, url, key, log, progress)
                }
            },
            object : SyncTask() {
                override val name = "Stok Seviyeleri (Eldeki Miktar)"
                override val description = "STOK_HAREKETTEN_ELDEKI_MIKTAR_VIEW kopyalanıyor..."
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {
                    BridgeSyncHelper.syncStokSeviyeleri(ctx, url, key, log, progress)
                }
            },
            object : SyncTask() {
                override val name = "Fiyat Liste Tanımları"
                override val description = "STOK_SATIS_FIYAT_LISTE_TANIMLARI kopyalanıyor..."
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {
                    BridgeSyncHelper.syncFiyatListeleri(ctx, url, key, log, progress)
                }
            },
            object : SyncTask() {
                override val name = "Gelişmiş Fiyat Listeleri"
                override val description = "fiyatListesi kopyalanıyor..."
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {
                    BridgeSyncHelper.syncFiyatListesiNew(ctx, url, key, log, progress)
                }
            },
            object : SyncTask() {
                override val name = "Cari Hesap Adresleri"
                override val description = "CARI_HESAP_ADRESLERI kopyalanıyor..."
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {
                    BridgeSyncHelper.syncCariAdresleri(ctx, url, key, log, progress)
                }
            },
            object : SyncTask() {
                override val name = "Cari Banka Hesapları"
                override val description = "cariBankaHesaplari kopyalanıyor..."
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {
                    BridgeSyncHelper.syncCariBankaHesaplari(ctx, url, key, log, progress)
                }
            },
            object : SyncTask() {
                override val name = "Banka Tanımları"
                override val description = "BANKALAR kopyalanıyor..."
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {
                    BridgeSyncHelper.syncBankalar(ctx, url, key, log, progress)
                }
            },
            object : SyncTask() {
                override val name = "Kasa Tanımları"
                override val description = "KASALAR kopyalanıyor..."
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {
                    BridgeSyncHelper.syncKasalar(ctx, url, key, log, progress)
                }
            },
            object : SyncTask() {
                override val name = "Kasa Yönetimi & Muhasebe"
                override val description = "KASALAR_YONETIM kopyalanıyor..."
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {
                    BridgeSyncHelper.syncKasaYonetim(ctx, url, key, log, progress)
                }
            },
            object : SyncTask() {
                override val name = "Köprü Durumu & Watermarklar"
                override val description = "Watermarks kopyalanıyor..."
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {
                    BridgeSyncHelper.syncStatusCheck(ctx, url, key, log, progress)
                }
            }
        )
    }

    fun startSyncAll() {
        SyncManager.startSyncAll(context.applicationContext, apiUrl, apiKey, syncTasks)
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("ERP Entegrasyon Merkezi") },
                navigationIcon = {
                    IconButton(onClick = { navController.popBackStack() }) {
                        Icon(Icons.Filled.ArrowBack, contentDescription = "Back")
                    }
                },
                colors = TopAppBarDefaults.topAppBarColors(
                    containerColor = MaterialTheme.colorScheme.surface,
                    titleContentColor = MaterialTheme.colorScheme.onSurface
                )
            )
        }
    ) { paddingValues ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
        ) {
            TabRow(
                selectedTabIndex = activeTab,
                containerColor = MaterialTheme.colorScheme.surface,
                contentColor = MaterialTheme.colorScheme.primary
            ) {
                Tab(
                    selected = activeTab == 0,
                    onClick = { activeTab = 0 },
                    text = { Text("Bağlantı Ayarları", fontWeight = FontWeight.SemiBold) }
                )
                Tab(
                    selected = activeTab == 1,
                    onClick = { activeTab = 1 },
                    text = { Text("Saha Senkronizasyonu", fontWeight = FontWeight.SemiBold) }
                )
            }
            
            AnimatedContent(
                targetState = activeTab,
                transitionSpec = { fadeIn(tween(300)) togetherWith fadeOut(tween(300)) },
                modifier = Modifier.weight(1f)
            ) { targetTab ->
                when (targetTab) {
                    0 -> {
                        Column(
                            modifier = Modifier
                                .fillMaxSize()
                                .verticalScroll(rememberScrollState())
                                .padding(16.dp),
                            verticalArrangement = Arrangement.spacedBy(16.dp)
                        ) {
                            Text("Sunucu Bilgileri", style = MaterialTheme.typography.titleMedium, color = MaterialTheme.colorScheme.primary)
                            
                            OutlinedTextField(
                                value = apiUrl,
                                onValueChange = { apiUrl = it },
                                label = { Text("API Sunucu Adresi") },
                                modifier = Modifier.fillMaxWidth(),
                                shape = RoundedCornerShape(12.dp)
                            )
                            
                            OutlinedTextField(
                                value = tenantId,
                                onValueChange = { tenantId = it },
                                label = { Text("Tenant ID / Kurum Kodu") },
                                modifier = Modifier.fillMaxWidth(),
                                shape = RoundedCornerShape(12.dp)
                            )
                            
                            OutlinedTextField(
                                value = apiKey,
                                onValueChange = { apiKey = it },
                                label = { Text("API Anahtarı / Token") },
                                modifier = Modifier.fillMaxWidth(),
                                shape = RoundedCornerShape(12.dp)
                            )
                            
                            Spacer(modifier = Modifier.height(8.dp))
                            
                            Button(
                                onClick = {
                                    sharedPrefs.edit().apply {
                                        putString("api_url", apiUrl)
                                        putString("tenant_id", tenantId)
                                        putString("api_key", apiKey)
                                        apply()
                                    }
                                    Toast.makeText(context, "Ayarlar kaydedildi.", Toast.LENGTH_SHORT).show()
                                },
                                modifier = Modifier.fillMaxWidth().height(50.dp),
                                shape = RoundedCornerShape(12.dp)
                            ) {
                                Text("Ayarları Kaydet")
                            }
                        }
                    }
                    1 -> {
                        Column(
                            modifier = Modifier.fillMaxSize().padding(16.dp),
                            verticalArrangement = Arrangement.spacedBy(16.dp)
                        ) {
                            Card(
                                modifier = Modifier.fillMaxWidth(),
                                colors = CardDefaults.cardColors(
                                    containerColor = MaterialTheme.colorScheme.primaryContainer
                                ),
                                shape = RoundedCornerShape(16.dp)
                            ) {
                                Column(
                                    modifier = Modifier.padding(16.dp),
                                    verticalArrangement = Arrangement.spacedBy(12.dp)
                                ) {
                                    Text(
                                        "Merkezi Senkronizasyon",
                                        style = MaterialTheme.typography.titleMedium,
                                        fontWeight = FontWeight.Bold,
                                        color = MaterialTheme.colorScheme.onPrimaryContainer
                                    )
                                    Text(
                                        "Tüm veri tablolarını ERP sunucusundan çekerek yerel veritabanını günceller. Bu işlem veri boyutuna göre birkaç dakika sürebilir.",
                                        style = MaterialTheme.typography.bodyMedium,
                                        color = MaterialTheme.colorScheme.onPrimaryContainer.copy(alpha = 0.8f)
                                    )
                                    
                                    Button(
                                        onClick = { startSyncAll() },
                                        enabled = !isSyncAllRunning,
                                        modifier = Modifier.fillMaxWidth().height(50.dp),
                                        colors = ButtonDefaults.buttonColors(
                                            containerColor = MaterialTheme.colorScheme.primary,
                                            contentColor = MaterialTheme.colorScheme.onPrimary
                                        )
                                    ) {
                                        Icon(Icons.Filled.Sync, contentDescription = null, modifier = Modifier.size(18.dp))
                                        Spacer(modifier = Modifier.width(8.dp))
                                        Text(if (isSyncAllRunning) "Senkronize Ediliyor..." else "Tüm Verileri Senkronize Et")
                                    }
                                }
                            }
                            
                            if (!(isSyncAllRunning || isSyncAllFinished)) {
                                Text(
                                    "Bireysel Tablo Senkronizasyonu",
                                    style = MaterialTheme.typography.titleSmall,
                                    fontWeight = FontWeight.Bold,
                                    color = MaterialTheme.colorScheme.onBackground
                                )
                                LazyColumn(
                                    modifier = Modifier.fillMaxWidth().weight(1f),
                                    verticalArrangement = Arrangement.spacedBy(8.dp)
                                ) {
                                    items(items = syncTasks) { task ->
                                        Card(
                                            modifier = Modifier.fillMaxWidth(),
                                            colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f)),
                                            shape = RoundedCornerShape(8.dp)
                                        ) {
                                            Row(
                                                modifier = Modifier.fillMaxWidth().padding(12.dp),
                                                horizontalArrangement = Arrangement.SpaceBetween,
                                                verticalAlignment = Alignment.CenterVertically
                                            ) {
                                                Column(modifier = Modifier.weight(1f)) {
                                                    Text(task.name, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold)
                                                    Text(task.description, style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
                                                }
                                                Spacer(modifier = Modifier.width(8.dp))
                                                Button(
                                                    onClick = {
                                                        SyncManager.startSyncAll(context.applicationContext, apiUrl, apiKey, listOf(task))
                                                    },
                                                    enabled = !isSyncAllRunning,
                                                    colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.secondary),
                                                    contentPadding = androidx.compose.foundation.layout.PaddingValues(horizontal = 12.dp, vertical = 6.dp)
                                                ) {
                                                    Text("İndir", style = MaterialTheme.typography.labelMedium)
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            
                            if (isSyncAllRunning || isSyncAllFinished) {
                                Card(
                                    modifier = Modifier.fillMaxWidth().weight(1f),
                                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.3f)),
                                    border = androidx.compose.foundation.BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant)
                                ) {
                                    Column(
                                        modifier = Modifier.fillMaxSize().padding(12.dp)
                                    ) {
                                        Row(
                                            modifier = Modifier.fillMaxWidth(),
                                            horizontalArrangement = Arrangement.SpaceBetween,
                                            verticalAlignment = Alignment.CenterVertically
                                        ) {
                                            Text("İşlem Günlüğü", style = MaterialTheme.typography.labelMedium, fontWeight = FontWeight.Bold)
                                            Row {
                                                if (!isSyncAllRunning && isSyncAllFinished) {
                                                    IconButton(onClick = { SyncManager.resetSyncState() }, modifier = Modifier.size(24.dp)) {
                                                        Icon(Icons.Filled.Close, contentDescription = "Kapat", modifier = Modifier.size(16.dp))
                                                    }
                                                    Spacer(modifier = Modifier.width(16.dp))
                                                }
                                                IconButton(
                                                onClick = {
                                                    val clipboard = context.getSystemService(Context.CLIPBOARD_SERVICE) as android.content.ClipboardManager
                                                    val clip = android.content.ClipData.newPlainText("İşlem Günlüğü", consoleLogs.reversed().joinToString("\n"))
                                                    clipboard.setPrimaryClip(clip)
                                                    Toast.makeText(context, "Günlük kopyalandı", Toast.LENGTH_SHORT).show()
                                                },
                                                modifier = Modifier.size(24.dp)
                                            ) {
                                                Icon(Icons.Filled.ContentCopy, contentDescription = "Kopyala", modifier = Modifier.size(16.dp))
                                            }
                                        }
                                        Spacer(modifier = Modifier.height(8.dp))
                                        if (currentSyncTaskIndex >= 0) {
                                            Text(
                                                "Şu anki tablo: $currentSyncTaskName", 
                                                style = MaterialTheme.typography.bodySmall, 
                                                color = MaterialTheme.colorScheme.primary
                                            )
                                            LinearProgressIndicator(
                                                progress = { currentSyncTaskProgress },
                                                modifier = Modifier.fillMaxWidth().padding(vertical = 8.dp)
                                            )
                                            if (currentSyncStats.isNotEmpty()) {
                                                Text(
                                                    text = currentSyncStats,
                                                    style = MaterialTheme.typography.bodySmall,
                                                    color = MaterialTheme.colorScheme.onSurfaceVariant
                                                )
                                            }
                                        }
                                        Spacer(modifier = Modifier.height(8.dp))
                                        LazyColumn(
                                            modifier = Modifier.fillMaxSize(),
                                            reverseLayout = true
                                        ) {
                                            itemsIndexed(consoleLogs) { _, line ->
                                                Text(
                                                    text = line,
                                                    style = MaterialTheme.typography.bodySmall,
                                                    color = if (line.contains("hata", ignoreCase = true) || line.contains("⚠️")) MaterialTheme.colorScheme.error 
                                                           else if (line.contains("✅")) Color(0xFF2E7D32) 
                                                           else MaterialTheme.colorScheme.onSurfaceVariant,
                                                    modifier = Modifier.padding(vertical = 2.dp)
                                                )
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
}
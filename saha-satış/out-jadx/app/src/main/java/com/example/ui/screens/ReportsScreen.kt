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
import androidx.compose.foundation.text.KeyboardOptions
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
import androidx.compose.ui.geometry.Size
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.StrokeCap
import androidx.compose.ui.graphics.drawscope.Stroke
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
import com.example.ui.components.FieldPrimaryButton
import com.example.ui.components.FieldSecondaryButton
import com.example.ui.components.FieldHeader
import kotlinx.coroutines.delay
import kotlinx.coroutines.launch

// --- REPORT REPRESENTATIVE DATA MODELS ---
data class ReportType(
    val id: String,
    val title: String,
    val category: String, // "SATIS", "FINANS", "ENVANTER"
    val icon: ImageVector,
    val primaryColor: Color
)

data class ReportRowRecord(
    val index: String,
    val label: String,
    val secondaryLabel: String,
    val value: String,
    val progress: Float // 0f to 1f for custom bar lengths
)

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ReportsScreen(navController: NavController) {
    val context = LocalContext.current
    val scope = rememberCoroutineScope()
    val snackbarHostState = remember { SnackbarHostState() }

    // --- SOUND & HAPTIC FEEDBACKS ---
    fun playReportFeedback(isSuccess: Boolean) {
        com.example.util.VibratorHelper.triggerFeedback(context, isSuccess)
    }

    // --- REPORT TYPES DEFINITIONS ---
    val reportTypes = remember {
        listOf(
            ReportType("DS", "Günlük Satış Raporu", "SATIS", Icons.Filled.TrendingUp, Color(0xFF1E88E5)),
            ReportType("WS", "Haftalık Satış Raporu", "SATIS", Icons.Filled.DateRange, Color(0xFF3949AB)),
            ReportType("MS", "Aylık Satış Raporu", "SATIS", Icons.Filled.Assessment, Color(0xFF5E35B1)),
            ReportType("SS", "Personel Satış Raporu", "SATIS", Icons.Filled.People, Color(0xFFF4511E)),
            ReportType("CS", "Cari Satış Raporu", "SATIS", Icons.Filled.AssignmentInd, Color(0xFF00ACC1)),
            ReportType("PS", "Ürün Satış Raporu", "SATIS", Icons.Filled.Shop, Color(0xFF43A047)),
            ReportType("CR", "Tahsilat Raporu", "FINANS", Icons.Filled.AddCard, Color(0xFF2E7D32)),
            ReportType("PR", "Tediye Raporu", "FINANS", Icons.Filled.Payment, Color(0xFFC62828)),
            ReportType("STR", "Stok Envanter Raporu", "ENVANTER", Icons.Filled.Warehouse, Color(0xFFE65100)),
            ReportType("CD", "Sayım Fark Raporu", "ENVANTER", Icons.Filled.Compare, Color(0xFFD84315)),
            ReportType("WSR", "Depo Stok Dağılımı", "ENVANTER", Icons.Filled.Store, Color(0xFF00838F))
        )
    }

    // --- CONTROLLER TABS: 0 -> Saha Raporları Panel, 1 -> Teknik Mimari & API Şemaları ---
    var activeMainTab by remember { mutableStateOf(0) }

    // --- INTERACTIVE REPORT STATES ---
    var selectedReport by remember { mutableStateOf(reportTypes[0]) } // Default: Günlük Satış Raporu
    var chartTypeToggle by remember { mutableStateOf("BAR") } // "BAR", "LINE"

    // --- FILTERS STATE ---
    var filterDateRange by remember { mutableStateOf("Bugün") } // "Bugün", "Bu Hafta", "Bu Ay", "Özel"
    var filterCustomer by remember { mutableStateOf("Tümü") } // "Tümü", "Acme Corp Ltd.", "Global Petrol A.Ş.", "Akkurt Market"
    var filterProduct by remember { mutableStateOf("Tümü") } // "Tümü", "Endüstriyel Motor Yağı", "Hava Filtresi Pro", "Çelik Rulman"
    var filterWarehouse by remember { mutableStateOf("Tümü") } // "Tümü", "Ana Depo", "Ankara Merkez", "Ege Bölge"
    var filterStaff by remember { mutableStateOf("Tümü") } // "Tümü", "Serhan Kalay", "Ahmet Yılmaz", "Ayşe Demir"

    // Filter Dialog Display
    var showFilterSelectionDialog by remember { mutableStateOf(false) }

    // Mock Excel/PDF Export State
    var showExportProgressDialog by remember { mutableStateOf(false) }
    var exportProgressValue by remember { mutableStateOf(0f) }
    var exportFinishedType by remember { mutableStateOf("") } // "PDF" veya "EXCEL"

    // Dynamic metrics calculated based on chosen filters & reports
    val kpiTotalRevenue = remember(selectedReport, filterDateRange, filterCustomer, filterStaff) {
        var base = when(selectedReport.id) {
            "DS" -> 42500.00
            "WS" -> 184200.00
            "MS" -> 725000.00
            "SS" -> 142500.00
            "CS" -> 98500.00
            "PS" -> 133400.00
            "CR" -> 85300.00 // Finans Tahsilat
            "PR" -> 32400.00 // Finans Tediye
            "STR" -> 412000.00 // Stok Değeri
            "CD" -> -3200.00 // Sayım Fark Değeri
            else -> 122000.00
        }
        // Apply multipliers from filters to simulate real reactivity
        if (filterCustomer != "Tümü") base *= 0.45
        if (filterStaff != "Tümü") base *= 0.35
        if (filterWarehouse != "Tümü") base *= 0.60
        if (filterDateRange == "Bu Hafta") base *= 1.2
        if (filterDateRange == "Bu Ay") base *= 3.5
        base
    }

    val kpiSecondaryMetricLabel = when(selectedReport.category) {
        "SATIS" -> "Toplam Evrak Adedi"
        "FINANS" -> "Mutabık Kalan Cari"
        else -> "Aktif Mal Grubu"
    }
    val kpiSecondaryMetricVal = when(selectedReport.category) {
        "SATIS" -> if (filterDateRange == "Bugün") "24 Fatura" else "118 Evrak"
        "FINANS" -> "14 Cari Hesap"
        else -> "6 Temel Sınıf"
    }

    // Dynamic Chart Values (y values mapped from 0f to 1f for Compose canvas rendering)
    val chartDataPoints = remember(selectedReport, filterDateRange, filterCustomer) {
        when(selectedReport.id) {
            "DS" -> listOf(0.2f, 0.35f, 0.15f, 0.45f, 0.70f, 0.85f, 0.95f, 0.60f) // Hours (09:00 - 17:00)
            "WS" -> listOf(0.85f, 0.70f, 0.90f, 0.65f, 0.95f, 0.40f, 0.15f) // Days of week
            "MS" -> listOf(0.40f, 0.55f, 0.65f, 0.80f, 0.94f, 0.72f) // Weeks of general period
            "SS" -> listOf(0.95f, 0.70f, 0.45f, 0.30f) // Yusuf, Mehmet, Ayşe, Kemal
            "CS" -> listOf(0.85f, 0.50f, 0.40f, 0.25f, 0.15f) // Customers top 5
            "PS" -> listOf(0.90f, 0.65f, 0.40f, 0.35f, 0.12f) // Products volume
            "CR" -> listOf(0.50f, 0.95f, 0.60f, 0.40f) // Cash, Card, EFT, Cheque
            "PR" -> listOf(0.30f, 0.85f, 0.15f) // Cash out, EFT, Return
            "STR" -> listOf(0.95f, 0.60f, 0.20f, 0.80f) // Main, Ankara, Ege, Transit stocks
            "CD" -> listOf(0.12f, 0.05f, 0.85f, 0.45f) // Discrepancies
            else -> listOf(0.5f, 0.5f, 0.5f, 0.5f)
        }
    }

    val chartLabels = remember(selectedReport) {
        when(selectedReport.id) {
            "DS" -> listOf("09", "10", "11", "12", "13", "14", "15", "16")
            "WS" -> listOf("Pzt", "Sal", "Çar", "Per", "Cum", "Cmt", "Paz")
            "MS" -> listOf("H1", "H2", "H3", "H4", "H5", "H6")
            "SS" -> listOf("Yusuf", "Mehmet", "Ayşe", "Kemal")
            "CS" -> listOf("Acme", "Global", "Akkurt", "Ege", "Atasoy")
            "PS" -> listOf("Yağ 20L", "Filter", "Rulman", "Civata", "Diğer")
            "CR" -> listOf("Nakit", "Kart", "EFT", "Senet")
            "PR" -> listOf("Nakit", "Havale", "Kasa")
            "STR" -> listOf("Ana D.", "Ankara", "Ege B.", "Mobil")
            "CD" -> listOf("Yağ", "Filter", "Rulman", "Kablo")
            else -> listOf("1", "2", "3", "4")
        }
    }

    // Dynamic Report rows for ledger lists
    val ledgerRows = remember(selectedReport, filterDateRange, filterCustomer, filterProduct) {
        when(selectedReport.id) {
            "DS", "WS", "MS" -> listOf(
                ReportRowRecord("1", "Saha Satış Faturası - Acme Corp", "08.06.2026 - No: FT-12002", "₺14.250,00", 0.95f),
                ReportRowRecord("2", "Saha Satış Faturası - Akkurt Gıda", "08.06.2026 - No: FT-12056", "₺8.500,00", 0.65f),
                ReportRowRecord("3", "Perakende Madeni Yağ Faturası", "08.06.2026 - No: FT-12057", "₺4.150,00", 0.35f),
                ReportRowRecord("4", "Saha Siparişi - Global Petrol", "07.06.2026 - No: SP-991", "₺2.400,00", 0.18f)
            )
            "SS" -> listOf(
                ReportRowRecord("1", "Yusuf Demir (Güney Bölge)", "48 Fatura Girişi", "₺84.500,00", 0.95f),
                ReportRowRecord("2", "Mehmet Kaya (Anadolu)", "32 Fatura Girişi", "₺54.200,00", 0.65f),
                ReportRowRecord("3", "Ayşe Can (Ege Bölge)", "22 Fatura Girişi", "₺41.600,00", 0.45f),
                ReportRowRecord("4", "Kemal Aksu (Marmara)", "12 Fatura Girişi", "₺22.100,00", 0.25f)
            )
            "CS" -> listOf(
                ReportRowRecord("1", "Acme Corp Logistics Ltd.", "6 Fatura Hareket", "₺54.250,00", 0.95f),
                ReportRowRecord("2", "Global Petrol Kimya A.Ş.", "4 Fatura Hareket", "₺30.310,00", 0.55f),
                ReportRowRecord("3", "Akkurt Market Gıda", "3 Fatura Hareket", "₺18.500,00", 0.35f),
                ReportRowRecord("4", "Atasoy Rulman Sanayi", "1 Fatura Hareket", "₺8.900,00", 0.15f)
            )
            "PS" -> listOf(
                ReportRowRecord("1", "Ultra Performans Motor Yağı 20L", "85 Kutu Satıldı", "₺58.450,00", 0.95f),
                ReportRowRecord("2", "Hava Filtresi - Ağır Vasıta Pro", "60 Adet Satıldı", "₺31.500,00", 0.55f),
                ReportRowRecord("3", "Çelik Rulman 120mm Devir", "22 Adet Satıldı", "₺19.580,00", 0.35f),
                ReportRowRecord("4", "Çelik Civata Takımı M8", "12 Pak. Satıldı", "₺2.180,00", 0.05f)
            )
            "CR" -> listOf(
                ReportRowRecord("1", "Finans Kredi Kartı Tahsilat", "POS Terminal 01 - Acme Corp", "₺35.000,00", 0.95f),
                ReportRowRecord("2", "Banka Havale / EFT", "YapıKredi - Global Petrol", "₺28.500,00", 0.75f),
                ReportRowRecord("3", "Nakit Tahsilat Makbuzu", "Saha Satıcı 01 - Akkurt Market", "₺15.000,00", 0.45f)
            )
            "PR" -> listOf(
                ReportRowRecord("1", "Toptan Alıcı Cari Ödeme (EFT)", "Vakıfbank - Merkez Finans", "₺25.000,00", 0.95f),
                ReportRowRecord("2", "Nakit İade Ödemesi", "Saha Kasa - Nakit Geri Ödeme", "₺7.400,00", 0.30f)
            )
            "STR" -> listOf(
                ReportRowRecord("1", "Ultra Performans Motor Yağı 20L", "199 Kutu Envanter", "₺185.000,00", 0.95f),
                ReportRowRecord("2", "Çelik Rulman 120mm Devir", "327 Adet Envanter", "₺154.000,00", 0.75f),
                ReportRowRecord("3", "Hava Filtresi - Ağır Vasıta Pro", "101 Adet Envanter", "₺65.000,00", 0.45f)
            )
            "CD" -> listOf(
                ReportRowRecord("1", "Hava Filtresi - Ölçüm Sapması", "Beklenen: 89, Sayılan: 87 (Eksik)", "-₺970,00", 0.85f),
                ReportRowRecord("2", "Çelik Rulman - Hücre Sapması", "Beklenen: 24, Sayılan: 25 (Fazla)", "+₺890,00", 0.75f),
                ReportRowRecord("3", "Sarf Civata M8 - Paket Sapması", "Beklenen: 320, Sayılan: 318 (Eksik)", "-₺360,00", 0.35f)
            )
            "WSR" -> listOf(
                ReportRowRecord("1", "Ana Depo (Merkez Lojistik)", "4 SKU Sınıfı - Envanter Dolu", "₺245.000,00", 0.95f),
                ReportRowRecord("2", "Ankara Bölge Depo", "3 SKU Sınıfı - Transit Sevk", "₺110.000,00", 0.55f),
                ReportRowRecord("3", "Ege Şube Deposu", "3 SKU Sınıfı - Bölge Sevk", "₺57.000,00", 0.25f)
            )
            else -> emptyList()
        }
    }

    // --- MAIN SCREEN ARCHITECTURE ---
    Scaffold(
        snackbarHost = { SnackbarHost(snackbarHostState) },
        containerColor = MaterialTheme.colorScheme.background
    ) { paddingValues ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
        ) {
            // Main App Header
            FieldHeader(
                title = "Analiz & Raporlama",
                subtitle = "SAHA SATIŞ YÖNETİCİ PANELİ",
                trailingContent = {
                    Row(
                        horizontalArrangement = Arrangement.spacedBy(8.dp),
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Surface(
                            color = MaterialTheme.colorScheme.primaryContainer,
                            shape = CircleShape,
                            modifier = Modifier.size(36.dp)
                        ) {
                            Box(contentAlignment = Alignment.Center) {
                                Icon(
                                    imageVector = Icons.Filled.VerifiedUser,
                                    contentDescription = "Safe",
                                    tint = MaterialTheme.colorScheme.onPrimaryContainer,
                                    modifier = Modifier.size(18.dp)
                                )
                            }
                        }
                    }
                }
            )

            // Segmented Main Navigation Tabs (Reports Screen vs Technical Spec Specs)
            TabRow(
                selectedTabIndex = activeMainTab,
                modifier = Modifier.fillMaxWidth(),
                containerColor = MaterialTheme.colorScheme.surface
            ) {
                Tab(
                    selected = activeMainTab == 0,
                    onClick = { activeMainTab = 0 },
                    text = { Text("Grafik & Raporlar", fontWeight = FontWeight.Bold) },
                    icon = { Icon(Icons.Filled.TableChart, contentDescription = null) }
                )
                Tab(
                    selected = activeMainTab == 1,
                    onClick = { activeMainTab = 1 },
                    text = { Text("Teknik Mimari (SQL/API)", fontWeight = FontWeight.Bold) },
                    icon = { Icon(Icons.Filled.Code, contentDescription = null) }
                )
            }

            AnimatedContent(
                targetState = activeMainTab,
                transitionSpec = { fadeIn() togetherWith fadeOut() },
                modifier = Modifier.weight(1f)
            ) { targetTab ->
                when (targetTab) {
                    0 -> {
                        // --- INTERACTIVE GRAPH & REPORTS SCREEN ---
                        Column(
                            modifier = Modifier
                                .fillMaxSize()
                                .padding(16.dp)
                                .verticalScroll(rememberScrollState()),
                            verticalArrangement = Arrangement.spacedBy(16.dp)
                        ) {
                            
                            // Top Row: Active Report Selector
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Column(modifier = Modifier.weight(1f)) {
                                    Text("Aktif Rapor Grubu", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.outline)
                                    Row(
                                        verticalAlignment = Alignment.CenterVertically,
                                        horizontalArrangement = Arrangement.spacedBy(4.dp)
                                    ) {
                                        Icon(selectedReport.icon, contentDescription = null, modifier = Modifier.size(18.dp), tint = selectedReport.primaryColor)
                                        Text(
                                            selectedReport.title,
                                            style = MaterialTheme.typography.titleMedium,
                                            fontWeight = FontWeight.ExtraBold,
                                            color = MaterialTheme.colorScheme.onSurface,
                                            maxLines = 1,
                                            overflow = TextOverflow.Ellipsis
                                        )
                                    }
                                }

                                Row(
                                    horizontalArrangement = Arrangement.spacedBy(8.dp)
                                ) {
                                    Button(
                                        onClick = { showFilterSelectionDialog = true },
                                        colors = ButtonDefaults.buttonColors(
                                            containerColor = MaterialTheme.colorScheme.secondaryContainer,
                                            contentColor = MaterialTheme.colorScheme.onSecondaryContainer
                                        ),
                                        contentPadding = PaddingValues(horizontal = 12.dp, vertical = 6.dp)
                                    ) {
                                        Icon(Icons.Filled.FilterList, contentDescription = null, modifier = Modifier.size(16.dp))
                                        Spacer(modifier = Modifier.width(4.dp))
                                        Text("Filtreler", style = MaterialTheme.typography.labelMedium)
                                        
                                        // Count active filters
                                        var activeCount = 0
                                        if (filterDateRange != "Bugün") activeCount++
                                        if (filterCustomer != "Tümü") activeCount++
                                        if (filterProduct != "Tümü") activeCount++
                                        if (filterWarehouse != "Tümü") activeCount++
                                        if (filterStaff != "Tümü") activeCount++
                                        if (activeCount > 0) {
                                            Spacer(modifier = Modifier.width(4.dp))
                                            Box(
                                                modifier = Modifier
                                                    .background(MaterialTheme.colorScheme.error, CircleShape)
                                                    .size(18.dp),
                                                contentAlignment = Alignment.Center
                                            ) {
                                                Text(activeCount.toString(), color = Color.White, style = MaterialTheme.typography.labelSmall)
                                            }
                                        }
                                    }
                                }
                            }

                            // Sub-Selector: Horizontal Swipe of Report Submodules
                            LazyRow(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.spacedBy(8.dp)
                            ) {
                                items(reportTypes) { type ->
                                    val isSelected = selectedReport.id == type.id
                                    FilterChip(
                                        selected = isSelected,
                                        onClick = {
                                            selectedReport = type
                                            playReportFeedback(true)
                                        },
                                        label = { Text(type.title) },
                                        leadingIcon = { Icon(type.icon, contentDescription = null, modifier = Modifier.size(14.dp)) },
                                        colors = FilterChipDefaults.filterChipColors(
                                            selectedContainerColor = type.primaryColor.copy(alpha = 0.15f),
                                            selectedLabelColor = type.primaryColor
                                        )
                                    )
                                }
                            }

                            // KPI Overview Section
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.spacedBy(8.dp)
                            ) {
                                // Primary Revenue Card
                                FieldCard(
                                    modifier = Modifier.weight(1f),
                                    containerColor = selectedReport.primaryColor.copy(alpha = 0.05f)
                                ) {
                                    Column(modifier = Modifier.padding(12.dp)) {
                                        Text(
                                            if (selectedReport.category == "ENVANTER") "Aktif Envanter Değeri" else "Rapor Tutar Toplamı",
                                            style = MaterialTheme.typography.labelSmall,
                                            color = MaterialTheme.colorScheme.onSurfaceVariant
                                        )
                                        Spacer(modifier = Modifier.height(4.dp))
                                        Text(
                                            text = String.format("₺%,.2f", kpiTotalRevenue),
                                            style = MaterialTheme.typography.titleMedium,
                                            fontWeight = FontWeight.Bold,
                                            color = if (kpiTotalRevenue >= 0) MaterialTheme.colorScheme.onSurface else MaterialTheme.colorScheme.error
                                        )
                                        Row(
                                            verticalAlignment = Alignment.CenterVertically,
                                            horizontalArrangement = Arrangement.spacedBy(4.dp),
                                            modifier = Modifier.padding(top = 4.dp)
                                        ) {
                                            Icon(
                                                imageVector = if (kpiTotalRevenue >= 0) Icons.Filled.TrendingUp else Icons.Filled.TrendingDown,
                                                contentDescription = null,
                                                tint = if (kpiTotalRevenue >= 0) Color(0xFF2E7D32) else Color(0xFFC62828),
                                                modifier = Modifier.size(14.dp)
                                            )
                                            Text(
                                                text = if (kpiTotalRevenue >= 0) "+%8.2 (ERP uyumlu)" else "-%2.1 (Sapma)",
                                                style = MaterialTheme.typography.labelSmall,
                                                color = if (kpiTotalRevenue >= 0) Color(0xFF2E7D32) else Color(0xFFC62828)
                                            )
                                        }
                                    }
                                }

                                // Secondary Volume Card
                                FieldCard(modifier = Modifier.weight(1f)) {
                                    Column(modifier = Modifier.padding(12.dp)) {
                                        Text(
                                            kpiSecondaryMetricLabel,
                                            style = MaterialTheme.typography.labelSmall,
                                            color = MaterialTheme.colorScheme.onSurfaceVariant
                                        )
                                        Spacer(modifier = Modifier.height(4.dp))
                                        Text(
                                            text = kpiSecondaryMetricVal,
                                            style = MaterialTheme.typography.titleMedium,
                                            fontWeight = FontWeight.Bold
                                        )
                                        Row(
                                            verticalAlignment = Alignment.CenterVertically,
                                            horizontalArrangement = Arrangement.spacedBy(4.dp),
                                            modifier = Modifier.padding(top = 4.dp)
                                        ) {
                                            Icon(Icons.Filled.SyncLock, contentDescription = null, tint = MaterialTheme.colorScheme.primary, modifier = Modifier.size(14.dp))
                                            Text("Offline Gecikme: 0sn", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.outline)
                                        }
                                    }
                                }
                            }

                            // Dynamic Visual Chart Card (Drawn with custom Jetpack Compose Canvas)
                            FieldCard {
                                Column(modifier = Modifier.padding(16.dp)) {
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween,
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        Column {
                                            Text("Grafik Trend Analizi", style = MaterialTheme.typography.titleSmall, fontWeight = FontWeight.Bold)
                                            Text("Filtre: $filterDateRange", style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.outline)
                                        }

                                        // Chart Type Toggle Buttons
                                        Row(
                                            modifier = Modifier
                                                .background(MaterialTheme.colorScheme.surfaceVariant, RoundedCornerShape(8.dp))
                                                .padding(2.dp)
                                        ) {
                                            listOf("BAR", "LINE").forEach { mode ->
                                                val active = chartTypeToggle == mode
                                                Box(
                                                    modifier = Modifier
                                                        .clip(RoundedCornerShape(6.dp))
                                                        .background(if (active) MaterialTheme.colorScheme.surface else Color.Transparent)
                                                        .clickable { chartTypeToggle = mode }
                                                        .padding(horizontal = 12.dp, vertical = 4.dp),
                                                    contentAlignment = Alignment.Center
                                                ) {
                                                    Icon(
                                                        imageVector = if (mode == "BAR") Icons.Filled.BarChart else Icons.Filled.ShowChart,
                                                        contentDescription = null,
                                                        tint = if (active) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.outline,
                                                        modifier = Modifier.size(18.dp)
                                                    )
                                                }
                                            }
                                        }
                                    }

                                    Spacer(modifier = Modifier.height(16.dp))

                                    // THE CANVAS GRAPH
                                    Box(
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .height(180.dp),
                                        contentAlignment = Alignment.BottomCenter
                                    ) {
                                        val gridColor = MaterialTheme.colorScheme.outlineVariant
                                        val primaryColor = selectedReport.primaryColor

                                        Canvas(modifier = Modifier.fillMaxSize()) {
                                            val width = size.width
                                            val height = size.height

                                            // 1. Draw horizontal grid lines
                                            val gridParts = 4
                                            for (i in 0..gridParts) {
                                                val y = (height / gridParts) * i
                                                drawLine(
                                                    color = gridColor,
                                                    start = Offset(0f, y),
                                                    end = Offset(width, y),
                                                    strokeWidth = 1.dp.toPx()
                                                )
                                            }

                                            // 2. Draw mock data points
                                            if (chartTypeToggle == "BAR") {
                                                val barWidthFraction = 0.5f
                                                val numPoints = chartDataPoints.size
                                                val stepX = width / numPoints
                                                
                                                chartDataPoints.forEachIndexed { idx, pointVal ->
                                                    val xCenter = stepX * idx + (stepX / 2)
                                                    val barWidth = stepX * barWidthFraction
                                                    val barHeight = height * pointVal
                                                    
                                                    // Draw visual bar
                                                    drawRect(
                                                        brush = Brush.verticalGradient(
                                                            colors = listOf(primaryColor, primaryColor.copy(alpha = 0.3f))
                                                        ),
                                                        topLeft = Offset(xCenter - (barWidth / 2), height - barHeight),
                                                        size = Size(barWidth, barHeight),
                                                    )
                                                }
                                            } else {
                                                // LINE Chart
                                                val numPoints = chartDataPoints.size
                                                val stepX = width / (numPoints - 1)
                                                var previousOffset: Offset? = null

                                                chartDataPoints.forEachIndexed { idx, pointVal ->
                                                    val x = stepX * idx
                                                    val y = height - (height * pointVal)
                                                    val currentOffset = Offset(x, y)

                                                    // Draw connectors
                                                    previousOffset?.let { prev ->
                                                        drawLine(
                                                            color = primaryColor,
                                                            start = prev,
                                                            end = currentOffset,
                                                            strokeWidth = 3.dp.toPx(),
                                                            cap = StrokeCap.Round
                                                        )
                                                    }

                                                    // Draw dots
                                                    drawCircle(
                                                        color = primaryColor,
                                                        radius = 5.dp.toPx(),
                                                        center = currentOffset
                                                    )
                                                    drawCircle(
                                                        color = Color.White,
                                                        radius = 2.dp.toPx(),
                                                        center = currentOffset
                                                    )

                                                    previousOffset = currentOffset
                                                }
                                            }
                                        }
                                    }

                                    // Chart Labels Row
                                    Spacer(modifier = Modifier.height(8.dp))
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween
                                    ) {
                                        chartLabels.forEach { label ->
                                            Text(
                                                text = label,
                                                style = MaterialTheme.typography.labelSmall,
                                                color = MaterialTheme.colorScheme.outline,
                                                modifier = Modifier.width(36.dp),
                                                textAlign = TextAlign.Center
                                            )
                                        }
                                    }
                                }
                            }

                            // Dynamic Ledger Data List Card
                            FieldCard {
                                Column(modifier = Modifier.padding(16.dp)) {
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween,
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        Text(
                                            "Ayrıntılı Rapor Satırları (${ledgerRows.size} Kayıt)",
                                            style = MaterialTheme.typography.titleSmall,
                                            fontWeight = FontWeight.Bold
                                        )
                                        Surface(
                                            color = MaterialTheme.colorScheme.surfaceVariant,
                                            shape = RoundedCornerShape(12.dp)
                                        ) {
                                            Text(
                                                "Offline Hazır",
                                                modifier = Modifier.padding(6.dp, 2.dp),
                                                style = MaterialTheme.typography.labelSmall,
                                                color = MaterialTheme.colorScheme.onSurfaceVariant
                                            )
                                        }
                                    }

                                    Spacer(modifier = Modifier.height(12.dp))

                                    if (ledgerRows.isEmpty()) {
                                        Box(
                                            modifier = Modifier
                                                .fillMaxWidth()
                                                .padding(24.dp),
                                            contentAlignment = Alignment.Center
                                        ) {
                                            Text("Süzgeç kriterlerine uygun veri bulunamadı.")
                                        }
                                    } else {
                                        Column(verticalArrangement = Arrangement.spacedBy(10.dp)) {
                                            ledgerRows.forEach { rowVal ->
                                                Column(modifier = Modifier.fillMaxWidth()) {
                                                    Row(
                                                        modifier = Modifier.fillMaxWidth(),
                                                        horizontalArrangement = Arrangement.SpaceBetween,
                                                        verticalAlignment = Alignment.CenterVertically
                                                    ) {
                                                        Column(modifier = Modifier.weight(1f)) {
                                                            Text(rowVal.label, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold)
                                                            Text(rowVal.secondaryLabel, style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.outline)
                                                        }
                                                        Text(rowVal.value, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.ExtraBold, color = selectedReport.primaryColor)
                                                    }
                                                    
                                                    // Beautiful subtle visual proportional bar
                                                    Spacer(modifier = Modifier.height(4.dp))
                                                    Box(
                                                        modifier = Modifier
                                                            .fillMaxWidth()
                                                            .height(4.dp)
                                                            .clip(RoundedCornerShape(2.dp))
                                                            .background(MaterialTheme.colorScheme.outlineVariant)
                                                    ) {
                                                        Box(
                                                            modifier = Modifier
                                                                .fillMaxWidth(rowVal.progress)
                                                                .height(4.dp)
                                                                .background(selectedReport.primaryColor)
                                                        )
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            // Dynamic PDF & Excel Trigger Area
                            Card(
                                colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.secondaryContainer.copy(alpha = 0.2f)),
                                border = BorderStroke(1.dp, MaterialTheme.colorScheme.secondary.copy(alpha = 0.2f)),
                                shape = RoundedCornerShape(16.dp)
                            ) {
                                Column(modifier = Modifier.padding(16.dp), verticalArrangement = Arrangement.spacedBy(8.dp)) {
                                    Text(
                                        "Raporu Dışa Aktar (E-Posta / WhatsApp / Yazıcı)",
                                        style = MaterialTheme.typography.labelSmall,
                                        color = MaterialTheme.colorScheme.onSecondaryContainer,
                                        fontWeight = FontWeight.Bold
                                    )
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.spacedBy(8.dp)
                                    ) {
                                        Button(
                                            onClick = {
                                                exportFinishedType = "EXCEL"
                                                showExportProgressDialog = true
                                                exportProgressValue = 0f
                                            },
                                            modifier = Modifier.weight(1f),
                                            colors = ButtonDefaults.buttonColors(
                                                containerColor = MaterialTheme.colorScheme.primary,
                                                contentColor = MaterialTheme.colorScheme.onPrimary
                                            )
                                        ) {
                                            Icon(Icons.Filled.TableChart, contentDescription = null, modifier = Modifier.size(16.dp))
                                            Spacer(modifier = Modifier.width(4.dp))
                                            Text("Excel Aktar")
                                        }

                                        Button(
                                            onClick = {
                                                exportFinishedType = "PDF"
                                                showExportProgressDialog = true
                                                exportProgressValue = 0f
                                            },
                                            modifier = Modifier.weight(1f),
                                            colors = ButtonDefaults.buttonColors(
                                                containerColor = selectedReport.primaryColor,
                                                contentColor = Color.White
                                            )
                                        ) {
                                            Icon(Icons.Filled.PictureAsPdf, contentDescription = null, modifier = Modifier.size(16.dp))
                                            Spacer(modifier = Modifier.width(4.dp))
                                            Text("PDF İndir")
                                        }
                                    }
                                }
                            }
                        }
                    }

                    1 -> {
                        // --- MODULE B: HIGH-FIDELITY TECHNICAL ARCHITECTURE SPEC CARD DISPLAY ---
                        Column(
                            modifier = Modifier
                                .fillMaxSize()
                                .padding(16.dp)
                                .verticalScroll(rememberScrollState()),
                            verticalArrangement = Arrangement.spacedBy(16.dp)
                        ) {
                            Text(
                                text = "Saha Satış Sistemi Raporlama Mimarisi",
                                style = MaterialTheme.typography.titleMedium,
                                fontWeight = FontWeight.Bold,
                                color = MaterialTheme.colorScheme.primary
                            )
                            Text(
                                text = "Bu banyoda, saha uygulamalarında veritabanı performansı, API yapısı, offline hesaplamalar ve plan yetkilendirilmesi kısıtlamalarını içeren teknik kod tabanı mimarisi ayrıntandırılmıştır.",
                                style = MaterialTheme.typography.bodySmall,
                                color = MaterialTheme.colorScheme.onSurfaceVariant
                            )

                            // Specs Area: DB & API Collapsible
                            ExpandableSpecItem(
                                title = "1. Veritabanı Tablo Yapısı (Room & SQL)",
                                desc = "Saha çevrimdışı çalışabilirlik, sayım ve bakiye hesaplamalarını yapan temel SQL şemaları:",
                                code = """
-- 1. CARI HESAP TABLOSU (Müşteri)
CREATE TABLE customers (
    id TEXT PRIMARY KEY NOT NULL,
    name TEXT NOT NULL,
    balance REAL NOT NULL DEFAULT 0.0, -- Bakiye
    risk_limit REAL NOT NULL DEFAULT 0.0,
    price_group TEXT,
    special_discount REAL DEFAULT 0.0
);

-- 2. ENTEGRE FINANSAL HAREKETLER TABLOSU
CREATE TABLE customer_ledger (
    id TEXT PRIMARY KEY NOT NULL,
    customer_id TEXT NOT NULL,
    date TEXT NOT NULL,
    type TEXT NOT NULL, -- 'SATIŞ', 'TAHSİLAT', 'TEDİYE'
    amount REAL NOT NULL,
    payment_method TEXT, -- 'Nakit', 'Kart', 'EFT'
    description TEXT,
    is_synchronized INTEGER NOT NULL DEFAULT 0, -- 0: Offline, 1: Entegre
    FOREIGN KEY(customer_id) REFERENCES customers(id)
);

-- 3. DEPO STOK TABLOSU
CREATE TABLE warehouse_stocks (
    warehouse_id TEXT NOT NULL,
    product_code TEXT NOT NULL,
    qty INTEGER NOT NULL DEFAULT 0,
    PRIMARY KEY(warehouse_id, product_code)
);

-- 4. FIZEKSEL SAYIM MODÜLÜ TABLOSU 
CREATE TABLE physical_counts (
    id TEXT PRIMARY KEY NOT NULL,
    warehouse_id TEXT NOT NULL,
    date TEXT NOT NULL,
    product_code TEXT NOT NULL,
    expected_qty INTEGER NOT NULL,
    counted_qty INTEGER NOT NULL,
    is_synced INTEGER DEFAULT 0
);
                                """.trimIndent(),
                                isInitiallyExpanded = true
                            )

                            ExpandableSpecItem(
                                title = "2. RESTful Sunucu API Endpointleri",
                                desc = "Arka plan (ERP / Sync Server) ile veri alışverişi sağlayan uç noktalar (Endpoints):",
                                code = """
// 1. GET /api/v1/reports/sales/summary?start_date=..&customer_id=..
// Satış trendleri ve KPI değerlerini döner.
{
  "total_revenue": 42500.0,
  "document_count": 24,
  "trend_points": [0.2, 0.35, 0.15, 0.45, 0.7, 0.85, 0.95, 0.6]
}

// 2. GET /api/v1/reports/sales/by-staff?start_date=..&staff_id=..
// Saha personel bazlı ciro karşılaştırma raporu.

// 3. POST /api/v1/sync/collection-payment
// Çevrimdışı yapılan Tahsilat ve Tediye belgelerini ERP'ye fırlatır.
Request Body:
{
  "transactions": [
    {
      "tx_id": "COL-4201",
      "customer_id": "CUS-10045",
      "type": "TAHSİLAT",
      "amount": 2500.0,
      "payment_method": "Nakit",
      "timestamp": "2026-06-08T16:00:00Z"
    }
  ]
}
                                """.trimIndent()
                            )

                            ExpandableSpecItem(
                                title = "3. Çevrimdışı Çalışma & Bakiye Hesaplama",
                                desc = "Güvenilir yerel hesaplamalar ve senkronizasyon kuyruğu algoritması:",
                                code = """
/**
 * 1. Bakiye Hesaplama Mantığı:
 * -------------------------------------------------------------
 * Müşterinin bakiye formülü:
 * Cari Bakiye = (Toplam Faturalandırılmış Borç + Yapılan Tediyeler) 
 *               - (Yapılan Tahsilatlar + İadeler)
 * 
 * 2. Çevrimdışı Kayıt kuyruk mimarisi (Indexed Queue):
 * Bir tahsilat yapıldığında cihaz internet yoksa 'is_synchronized = 0' olarak yerel Room veritabanına yaılır.
 * Cihaz internete bağlandığı anda bir WorkManager tetiklenir:
 * FIFO prensibine göre kayıtar tek tek API'ye postalanır ve 'is_synchronized = 1' olarak güncellenir.
 */
                                """.trimIndent()
                            )

                            ExpandableSpecItem(
                                title = "4. Performans & Özet Tablo (Cache) Stratejisi",
                                desc = "Uygulama açılışında anlık binlerce hareketin yükünü hafifletmek için pre-aggregated özet tabloları:",
                                code = """
-- Her gece çalışan cron job ile doldurulan 'daily_sales_rollup' tablosu:
CREATE TABLE daily_sales_rollup (
    rollup_date TEXT NOT NULL,
    customer_id TEXT,
    product_code TEXT,
    staff_id TEXT,
    total_sales_volume REAL,
    total_invoice_count INTEGER,
    PRIMARY KEY (rollup_date, customer_id, product_code, staff_id)
);
-- Bu sayede uygulama ciro grafiğini çizerken milyarlarca satırı gezmek yerine hızlı rollup tablosunu sorgular.
                                """.trimIndent()
                            )

                            ExpandableSpecItem(
                                title = "5. Rol & Plan Bazlı Rapor Kısıtlamaları",
                                desc = "SAAS paket yetkilendirme şeması (Premium vs Standard):",
                                code = """
// Saha Plasiyer Rolü (Sadece kendi müşterilerini ve kendi günlük satışlarını görebilir)
// Bölge Koordinatörü (Bölgesel tüm alt depoları, personelleri ve sayım farklarını görebilir)
// Genel Yönetici / Owner (Limit ve kısıtlama olmaksızın tüm konsolu yönetir)

fun isReportAccessibleForPlan(reportId: String, currentPlan: String): Boolean {
    val proAndEnterpriseReports = listOf("CD", "WSR", "SS") // Sayım Fark, Depo Dağılımı ve Personel Raporları
    if (proAndEnterpriseReports.contains(reportId)) {
        return currentPlan == "PRO" || currentPlan == "ENTERPRISE"
    }
    return true // Başlangıç raporları herkese açık
}
                                """.trimIndent()
                            )
                        }
                    }
                }
            }
        }
    }

    // --- DIALOG A: INDEPENDENT INTERACTIVE FILTER DRAWER DIALOG ---
    if (showFilterSelectionDialog) {
        Dialog(onDismissRequest = { showFilterSelectionDialog = false }) {
            Box(
                modifier = Modifier
                    .fillMaxWidth()
                    .clip(RoundedCornerShape(20.dp))
                    .background(MaterialTheme.colorScheme.surface)
                    .padding(20.dp)
            ) {
                Column(
                    verticalArrangement = Arrangement.spacedBy(16.dp)
                ) {
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.SpaceBetween,
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Text(
                            text = "Rapor Detay Süzgeçleri",
                            style = MaterialTheme.typography.titleMedium,
                            fontWeight = FontWeight.Bold,
                            color = MaterialTheme.colorScheme.primary
                        )
                        IconButton(onClick = { showFilterSelectionDialog = false }) {
                            Icon(Icons.Filled.Close, contentDescription = null)
                        }
                    }

                    HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)

                    // 1. Date Range
                    Column(verticalArrangement = Arrangement.spacedBy(4.dp)) {
                        Text("Tarih Aralığı", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.outline)
                        Row(horizontalArrangement = Arrangement.spacedBy(6.dp)) {
                            listOf("Bugün", "Bu Hafta", "Bu Ay").forEach { range ->
                                val active = filterDateRange == range
                                Box(
                                    modifier = Modifier
                                        .clip(RoundedCornerShape(8.dp))
                                        .background(if (active) MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surfaceVariant)
                                        .clickable { filterDateRange = range }
                                        .padding(8.dp, 4.dp)
                                ) {
                                    Text(range, style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Bold, color = if (active) MaterialTheme.colorScheme.onPrimaryContainer else MaterialTheme.colorScheme.onSurfaceVariant)
                                }
                            }
                        }
                    }

                    // 2. Customer Select
                    Column(verticalArrangement = Arrangement.spacedBy(4.dp)) {
                        Text("Cari Müşteri Filtresi", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.outline)
                        Row(horizontalArrangement = Arrangement.spacedBy(6.dp)) {
                            listOf("Tümü", "Acme", "Global", "Akkurt").forEach { abbreviation ->
                                val fullName = when(abbreviation) {
                                    "Acme" -> "Acme Corp Ltd."
                                    "Global" -> "Global Petrol A.Ş."
                                    "Akkurt" -> "Akkurt Market"
                                    else -> "Tümü"
                                }
                                val active = filterCustomer == fullName
                                Box(
                                    modifier = Modifier
                                        .clip(RoundedCornerShape(8.dp))
                                        .background(if (active) MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surfaceVariant)
                                        .clickable { filterCustomer = fullName }
                                        .padding(8.dp, 4.dp)
                                ) {
                                    Text(abbreviation, style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Bold, color = if (active) MaterialTheme.colorScheme.onPrimaryContainer else MaterialTheme.colorScheme.onSurfaceVariant)
                                }
                            }
                        }
                    }

                    // 3. Staff Select
                    Column(verticalArrangement = Arrangement.spacedBy(4.dp)) {
                        Text("Personel Filtresi", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.outline)
                        Row(horizontalArrangement = Arrangement.spacedBy(6.dp)) {
                            listOf("Tümü", "Serhan", "Ahmet", "Ayşe").forEach { abbreviation ->
                                val fullName = when(abbreviation) {
                                    "Serhan" -> "Serhan Kalay"
                                    "Ahmet" -> "Ahmet Yılmaz"
                                    "Ayşe" -> "Ayşe Demir"
                                    else -> "Tümü"
                                }
                                val active = filterStaff == fullName
                                Box(
                                    modifier = Modifier
                                        .clip(RoundedCornerShape(8.dp))
                                        .background(if (active) MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surfaceVariant)
                                        .clickable { filterStaff = fullName }
                                        .padding(8.dp, 4.dp)
                                ) {
                                    Text(abbreviation, style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Bold, color = if (active) MaterialTheme.colorScheme.onPrimaryContainer else MaterialTheme.colorScheme.onSurfaceVariant)
                                }
                            }
                        }
                    }

                    // 4. Warehouse Select
                    Column(verticalArrangement = Arrangement.spacedBy(4.dp)) {
                        Text("Depo Filtresi", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.outline)
                        Row(horizontalArrangement = Arrangement.spacedBy(6.dp)) {
                            listOf("Tümü", "Ana", "Ankara", "Ege").forEach { abbreviation ->
                                val fullName = when(abbreviation) {
                                    "Ana" -> "Ana Depo"
                                    "Ankara" -> "Ankara Merkez"
                                    "Ege" -> "Ege Bölge"
                                    else -> "Tümü"
                                }
                                val active = filterWarehouse == fullName
                                Box(
                                    modifier = Modifier
                                        .clip(RoundedCornerShape(8.dp))
                                        .background(if (active) MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surfaceVariant)
                                        .clickable { filterWarehouse = fullName }
                                        .padding(8.dp, 4.dp)
                                ) {
                                    Text(abbreviation, style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Bold, color = if (active) MaterialTheme.colorScheme.onPrimaryContainer else MaterialTheme.colorScheme.onSurfaceVariant)
                                }
                            }
                        }
                    }

                    HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)

                    // Actions
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        OutlinedButton(
                            onClick = {
                                // Reset All Filters
                                filterDateRange = "Bugün"
                                filterCustomer = "Tümü"
                                filterProduct = "Tümü"
                                filterWarehouse = "Tümü"
                                filterStaff = "Tümü"
                                playReportFeedback(false)
                                showFilterSelectionDialog = false
                            },
                            modifier = Modifier.weight(1f)
                        ) {
                            Text("Sıfırla")
                        }

                        Button(
                            onClick = {
                                playReportFeedback(true)
                                showFilterSelectionDialog = false
                                scope.launch {
                                    snackbarHostState.showSnackbar("Rapor süzgeçleri başarıyla uygulandı.")
                                }
                            },
                            modifier = Modifier.weight(1.2f)
                        ) {
                            Text("Süzgeçleri Uygula")
                        }
                    }
                }
            }
        }
    }

    // --- DIALOG B: PDF/EXCEL PRODUCTION & SHARE SIMULATION ---
    if (showExportProgressDialog) {
        // Increment progress simulating local JVM SQLite aggregations to Excel
        LaunchedEffect(Unit) {
            for (i in 1..10) {
                delay(120)
                exportProgressValue = i / 10f
            }
            playReportFeedback(true)
            showExportProgressDialog = false
            
            // Show successful message of Mock Document Share Options
            scope.launch {
                snackbarHostState.showSnackbar("${selectedReport.title} ${exportFinishedType} dosyası dışa aktarıldı!")
            }
        }

        Dialog(onDismissRequest = { }) {
            Box(
                modifier = Modifier
                    .fillMaxWidth(0.85f)
                    .clip(RoundedCornerShape(20.dp))
                    .background(MaterialTheme.colorScheme.surface)
                    .padding(24.dp),
                contentAlignment = Alignment.Center
            ) {
                Column(
                    horizontalAlignment = Alignment.CenterHorizontally,
                    verticalArrangement = Arrangement.spacedBy(16.dp)
                ) {
                    CircularProgressIndicator(
                        progress = { exportProgressValue },
                        color = selectedReport.primaryColor,
                    )
                    
                    Text(
                        text = "Yerel veritabanı taranıyor...",
                        style = MaterialTheme.typography.bodyMedium,
                        fontWeight = FontWeight.Bold
                    )
                    Text(
                        text = "Saha Raporu ${exportFinishedType} dosyasına dönüştürülüyor...",
                        style = MaterialTheme.typography.labelSmall,
                        color = MaterialTheme.colorScheme.outline,
                        textAlign = TextAlign.Center
                    )
                }
            }
        }
    }
}

// --- SUB-WIDGET: COLLAPSIBLE EXPANDABLE TECHNICAL CARD ---
@Composable
fun ExpandableSpecItem(
    title: String,
    desc: String,
    code: String,
    isInitiallyExpanded: Boolean = false
) {
    var expanded by remember { mutableStateOf(isInitiallyExpanded) }

    Surface(
        modifier = Modifier
            .fillMaxWidth()
            .clip(RoundedCornerShape(12.dp))
            .border(1.dp, MaterialTheme.colorScheme.outlineVariant, RoundedCornerShape(12.dp))
            .clickable { expanded = !expanded },
        color = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.3f)
    ) {
        Column(modifier = Modifier.padding(14.dp)) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                Text(
                    text = title,
                    style = MaterialTheme.typography.bodyMedium,
                    fontWeight = FontWeight.Bold,
                    color = MaterialTheme.colorScheme.primary,
                    modifier = Modifier.weight(1f)
                )
                Icon(
                    imageVector = if (expanded) Icons.Filled.ExpandLess else Icons.Filled.ExpandMore,
                    contentDescription = null,
                    tint = MaterialTheme.colorScheme.primary
                )
            }

            AnimatedVisibility(
                visible = expanded,
                enter = expandVertically() + fadeIn(),
                exit = shrinkVertically() + fadeOut()
            ) {
                Column(
                    modifier = Modifier.padding(top = 10.dp),
                    verticalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    Text(desc, style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
                    
                    // Code container block styled like a professional Terminal
                    Surface(
                        modifier = Modifier.fillMaxWidth(),
                        color = Color(0xFF1E1E1E),
                        shape = RoundedCornerShape(8.dp)
                    ) {
                        Text(
                            text = code,
                            fontFamily = FontFamily.Monospace,
                            style = MaterialTheme.typography.labelSmall,
                            color = Color(0xFFE0E0E0),
                            modifier = Modifier
                                .padding(12.dp)
                                .fillMaxWidth()
                        )
                    }
                }
            }
        }
    }
}

package com.example

import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.layout.widthIn
import androidx.compose.ui.unit.sp
import androidx.compose.foundation.layout.offset
import androidx.compose.foundation.layout.navigationBarsPadding
import androidx.compose.foundation.layout.statusBarsPadding
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.ui.draw.clip
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Button
import androidx.compose.material3.NavigationBarItem
import androidx.compose.material3.BadgedBox
import androidx.compose.material3.Badge
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.calculateStartPadding
import androidx.compose.foundation.layout.calculateEndPadding
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.setValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.ui.Modifier
import androidx.compose.ui.input.pointer.pointerInput
import androidx.compose.foundation.gestures.detectTapGestures
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.draw.shadow
import androidx.compose.ui.unit.dp
import androidx.compose.ui.Alignment
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.animation.animateContentSize
import androidx.compose.animation.AnimatedVisibility
import androidx.compose.animation.expandVertically
import androidx.compose.animation.shrinkVertically
import androidx.compose.animation.fadeIn
import androidx.compose.animation.fadeOut
import androidx.navigation.NavGraph.Companion.findStartDestination
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.currentBackStackEntryAsState
import androidx.navigation.compose.rememberNavController
import com.example.ui.screens.*
import kotlinx.coroutines.launch

@Composable
fun NavApp() {
    val context = androidx.compose.ui.platform.LocalContext.current
    val scope = androidx.compose.runtime.rememberCoroutineScope()
    
    androidx.compose.runtime.remember {
        BridgeSyncHelper.initLastSyncTime(context)
        BridgeSyncHelper.initOnlineStatus(context)
        true
    }
    
    val navController = rememberNavController()
    val isOnline by BridgeSyncHelper.isOnlineState
    var showConnectionDialog by remember { mutableStateOf(false) }
    var isManualSyncInProgress by remember { mutableStateOf(false) }
    val lastSyncTime by BridgeSyncHelper.lastSyncTimeState

    androidx.compose.runtime.LaunchedEffect(Unit) {
        if (BridgeSyncHelper.isErpModeActive(context) && BridgeSyncHelper.isOnlineState.value) {
            BridgeSyncHelper.triggerBackgroundSync(context)
        }
    }

    if (showConnectionDialog) {
        ConnectionStatusDialog(
            onDismiss = { showConnectionDialog = false },
            isOnline = isOnline,
            onToggleOnline = { newVal ->
                BridgeSyncHelper.setOnlineStatus(context, newVal)
            },
            lastSyncTime = lastSyncTime,
            onManualSync = {
                scope.launch {
                    isManualSyncInProgress = true
                    BridgeSyncHelper.triggerBackgroundSync(context)
                    isManualSyncInProgress = false
                }
            },
            isSyncing = isManualSyncInProgress
        )
    }

    Scaffold(
        topBar = {
            val navBackStackEntry by navController.currentBackStackEntryAsState()
            val currentRoute = navBackStackEntry?.destination?.route
            if (currentRoute != "splash" && currentRoute != "catalog" && currentRoute != "wms_warehouse" && currentRoute != "sales") {
                Card(
                    modifier = Modifier
                        .fillMaxWidth()
                        .shadow(elevation = 3.dp, shape = RoundedCornerShape(bottomStart = 20.dp, bottomEnd = 20.dp)),
                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
                    shape = RoundedCornerShape(bottomStart = 20.dp, bottomEnd = 20.dp, topStart = 0.dp, topEnd = 0.dp)
                ) {
                    Row(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(horizontal = 16.dp, vertical = 12.dp)
                            .statusBarsPadding(),
                        horizontalArrangement = Arrangement.SpaceBetween,
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        if (currentRoute == "dashboard") {
                            Row(
                                verticalAlignment = Alignment.CenterVertically,
                                horizontalArrangement = Arrangement.spacedBy(10.dp),
                                modifier = Modifier.clickable { navController.navigate("dashboard") { launchSingleTop = true } }
                            ) {
                                Box(
                                    modifier = Modifier
                                        .size(42.dp)
                                        .background(MaterialTheme.colorScheme.primaryContainer, CircleShape),
                                    contentAlignment = Alignment.Center
                                ) {
                                    Text(
                                        text = "GO",
                                        color = MaterialTheme.colorScheme.onPrimaryContainer,
                                        fontWeight = FontWeight.Bold,
                                        style = MaterialTheme.typography.titleMedium
                                    )
                                }
                                Column {
                                    Text(
                                        text = "Gürbüz Oyuncak",
                                        style = MaterialTheme.typography.titleMedium,
                                        fontWeight = FontWeight.Bold,
                                        color = MaterialTheme.colorScheme.onSurface
                                    )
                                    Text(
                                        text = "Serhan Kalay",
                                        style = MaterialTheme.typography.bodySmall,
                                        color = MaterialTheme.colorScheme.onSurfaceVariant
                                    )
                                }
                            }
                        } else {
                            val activeCust = com.example.ui.screens.AppDataStore.activeSelectedCustomer.value
                            if (currentRoute == "customers" && activeCust != null) {
                                Row(
                                    verticalAlignment = Alignment.CenterVertically,
                                    horizontalArrangement = Arrangement.spacedBy(10.dp)
                                ) {
                                    androidx.compose.material3.IconButton(
                                        onClick = { com.example.ui.screens.AppDataStore.activeSelectedCustomer.value = null },
                                        modifier = Modifier
                                            .background(MaterialTheme.colorScheme.surfaceVariant, shape = CircleShape)
                                            .size(36.dp)
                                    ) {
                                        Icon(Icons.Filled.ArrowBack, contentDescription = "Geri", modifier = Modifier.size(18.dp))
                                    }
                                    Column {
                                        Text(
                                            text = activeCust.name,
                                            style = MaterialTheme.typography.titleMedium,
                                            fontWeight = FontWeight.Bold,
                                            color = MaterialTheme.colorScheme.onSurface,
                                            maxLines = 1,
                                            modifier = Modifier.widthIn(max = 200.dp)
                                        )
                                        Row(verticalAlignment = Alignment.CenterVertically) {
                                            Icon(
                                                imageVector = Icons.Filled.Tag,
                                                contentDescription = null,
                                                modifier = Modifier.size(12.dp),
                                                tint = MaterialTheme.colorScheme.outline
                                            )
                                            Spacer(modifier = Modifier.width(4.dp))
                                            Text(
                                                text = "Cari Kodu: ${activeCust.id}",
                                                style = MaterialTheme.typography.labelSmall,
                                                color = MaterialTheme.colorScheme.outline
                                            )
                                        }
                                    }
                                }
                            } else {
                                Row(
                                    verticalAlignment = Alignment.CenterVertically,
                                    horizontalArrangement = Arrangement.spacedBy(10.dp)
                                ) {
                                    androidx.compose.material3.IconButton(
                                        onClick = {
                                            if (currentRoute == "sales" && com.example.ui.screens.AppDataStore.salesSelectedTab.value != 0) {
                                                com.example.ui.screens.AppDataStore.salesSelectedTab.value = 0
                                            } else {
                                                navController.popBackStack()
                                            }
                                        },
                                        modifier = Modifier
                                            .background(MaterialTheme.colorScheme.surfaceVariant, shape = CircleShape)
                                            .size(36.dp)
                                    ) {
                                        Icon(Icons.Filled.ArrowBack, contentDescription = "Geri", modifier = Modifier.size(18.dp))
                                    }
                                    Text(
                                        text = currentRoute?.let { route ->
                                            when {
                                                route == "dashboard" -> "Ana Sayfa"
                                                route == "customers" -> "Cari Hesaplar"
                                                route == "catalog" -> "Ürün Kataloğu"
                                                route == "reports" -> "Grafik & Analiz Raporları"
                                                route == "more" -> "Ayarlar & Hub"
                                                route == "login" -> "Kullanıcı Girişi"
                                                route == "license" -> "Aktivasyon & Lisans"
                                                route == "sales" -> {
                                                    when (com.example.ui.screens.AppDataStore.salesSelectedTab.value) {
                                                        2 -> "Sipariş Sepetiniz"
                                                        1 -> "Cari Hesap Bilgileri"
                                                        else -> "Satış & Fatura Paneli"
                                                    }
                                                }
                                                route == "offline_sync" -> "Veri Senkronizasyonu"
                                                route == "security" -> "Güvenlik & PIN"
                                                route == "erp_integration" -> "ERP Entegrasyon"
                                                route == "import_export" -> "Excel İçe/Dışa Aktarım"
                                                route == "suspended_sales" -> "Bekleyenler"
                                                route.startsWith("stock_detail") -> "Stok Kart Detayı"
                                                route.startsWith("operations") -> {
                                                    val module = navBackStackEntry?.arguments?.getString("module") ?: ""
                                                    when (module) {
                                                        "purchase" -> "Alış Siparişi"
                                                        "returns" -> "Müşteri İade Kabul"
                                                        "collection" -> "Saha Tahsilat"
                                                        "disbursement" -> "Supplier Tediye"
                                                        "stocks" -> "Mevcut Stoklar"
                                                        "counting" -> "Hızlı Sayım"
                                                        "warehouses" -> "Depo Listesi"
                                                        "cashbox" -> "Kasa & Banka Defteri"
                                                        "eod" -> "Saha Gün Sonu Kapanışı"
                                                        "approvals" -> "Onay Merkezi"
                                                        else -> "İşlemler"
                                                    }
                                                }
                                                else -> route.replace("_", " ").split(" ").joinToString(" ") { it.replaceFirstChar { c -> c.uppercase() } }
                                            }
                                        } ?: "Saha Gücü",
                                        style = MaterialTheme.typography.titleMedium,
                                        fontWeight = FontWeight.Bold,
                                        color = MaterialTheme.colorScheme.onSurface
                                    )
                                }
                            }
                        }

                        // Action controls integrated beautifully in top bar
                        Row(
                            verticalAlignment = Alignment.CenterVertically,
                            horizontalArrangement = Arrangement.spacedBy(6.dp)
                        ) {
                            if (currentRoute == "customers" && com.example.ui.screens.AppDataStore.activeSelectedCustomer.value != null) {
                                androidx.compose.material3.IconButton(
                                    onClick = { com.example.ui.screens.AppDataStore.customerShowEditDialog.value = true },
                                    modifier = Modifier
                                        .background(MaterialTheme.colorScheme.primaryContainer, shape = CircleShape)
                                        .size(36.dp)
                                        .testTag("edit_customer_btn")
                                ) {
                                    Icon(
                                        Icons.Filled.Edit,
                                        contentDescription = "Düzenle",
                                        tint = MaterialTheme.colorScheme.primary,
                                        modifier = Modifier.size(18.dp)
                                    )
                                }
                            }

                            if (currentRoute?.startsWith("stock_detail") == true) {
                                // Product Edit Button (Icon-only) on TopAppBar
                                val appCxt = androidx.compose.ui.platform.LocalContext.current
                                val isErp = com.example.ui.screens.AppDataStore.isErpModeActive(appCxt)
                                if (!isErp) {
                                    androidx.compose.material3.IconButton(
                                        onClick = { com.example.ui.screens.AppDataStore.showStockDetailEditDialog.value = true },
                                        modifier = Modifier
                                            .background(MaterialTheme.colorScheme.primaryContainer, shape = CircleShape)
                                            .size(36.dp)
                                            .testTag("edit_product_detail_btn")
                                    ) {
                                        Icon(
                                            Icons.Filled.Edit,
                                            contentDescription = "Düzenle",
                                            tint = MaterialTheme.colorScheme.primary,
                                            modifier = Modifier.size(18.dp)
                                        )
                                    }
                                }
                            }

                            if (currentRoute == "catalog") {
                                val isGridView = com.example.ui.screens.AppDataStore.catalogSelectedViewMode.value == "Grid"
                                androidx.compose.material3.IconButton(
                                    onClick = {
                                        com.example.ui.screens.AppDataStore.catalogSelectedViewMode.value = if (isGridView) "List" else "Grid"
                                    },
                                    modifier = Modifier
                                        .background(MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.5f), shape = CircleShape)
                                        .size(36.dp)
                                ) {
                                    Icon(
                                        imageVector = if (isGridView) Icons.Filled.ViewList else Icons.Filled.GridView,
                                        contentDescription = "Görünüm Değiştir",
                                        tint = MaterialTheme.colorScheme.primary,
                                        modifier = Modifier.size(18.dp)
                                    )
                                }
                            }

                            // Settings Button
                            androidx.compose.material3.IconButton(
                                onClick = { navController.navigate("more") },
                                modifier = Modifier
                                    .background(MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.5f), shape = CircleShape)
                                    .size(36.dp)
                            ) {
                                Icon(
                                    Icons.Filled.Settings,
                                    contentDescription = "Ayarlar",
                                    tint = MaterialTheme.colorScheme.primary,
                                    modifier = Modifier.size(18.dp)
                                )
                            }

                            // Sync status icon
                            androidx.compose.material3.IconButton(
                                onClick = { showConnectionDialog = true },
                                modifier = Modifier
                                    .background(if (isOnline) Color(0xFFE8F5E9) else Color(0xFFFFEBEE), shape = CircleShape)
                                    .size(36.dp)
                            ) {
                                Icon(
                                    if (isOnline) Icons.Filled.Wifi else Icons.Filled.WifiOff,
                                    contentDescription = "Online Status",
                                    tint = if (isOnline) Color(0xFF43A047) else Color(0xFFD32F2F),
                                    modifier = Modifier.size(18.dp)
                                )
                            }
                        }
                    }
                }
            }
        },
        bottomBar = {
            val navBackStackEntry by navController.currentBackStackEntryAsState()
            val currentRoute = navBackStackEntry?.destination?.route
            val moduleArg = navBackStackEntry?.arguments?.getString("module")
            val isCustomModuleBar = currentRoute?.startsWith("operations") == true && (moduleArg == "collection" || moduleArg == "disbursement" || moduleArg == "counting")
            var isSalesBarExpanded by remember { mutableStateOf(true) }
            var isSalesPlusToggled by remember { mutableStateOf(false) }
            var isCustomersBarExpanded by remember { mutableStateOf(true) }
            var isCatalogBarExpanded by remember { mutableStateOf(true) }
            var showCatalogFilters by remember { mutableStateOf(false) }
            var showStocksFilters by remember { mutableStateOf(false) }

            if (currentRoute != null && currentRoute != "splash" && currentRoute != "login" && currentRoute != "license" && !isCustomModuleBar) {
                // Show product scanner global alert if scanned from global bottom bar scanner
                if (com.example.ui.screens.AppDataStore.globalShowBarcodeScanner) {
                    com.example.ui.screens.BarcodeScannerDialog(
                        onDismissRequest = { com.example.ui.screens.AppDataStore.globalShowBarcodeScanner = false },
                        onBarcodeScanned = { code ->
                            com.example.ui.screens.AppDataStore.globalShowBarcodeScanner = false
                            com.example.ui.screens.AppDataStore.globalScannedBarcode = code
                        },
                        onSimulateScan = { simulatedBarcode ->
                            com.example.ui.screens.AppDataStore.globalShowBarcodeScanner = false
                            com.example.ui.screens.AppDataStore.globalScannedBarcode = simulatedBarcode
                        }
                    )
                }

                if (com.example.ui.screens.AppDataStore.globalScannedBarcode != null) {
                    val scannedCode = com.example.ui.screens.AppDataStore.globalScannedBarcode
                    val product = com.example.ui.screens.AppDataStore.products.find { it.barcode == scannedCode || it.barcodes.contains(scannedCode) || it.code == scannedCode }
                    androidx.compose.material3.AlertDialog(
                        onDismissRequest = { com.example.ui.screens.AppDataStore.globalScannedBarcode = null },
                        title = {
                            androidx.compose.material3.Text("Ürün Bilgisi", fontWeight = androidx.compose.ui.text.font.FontWeight.Bold)
                        },
                        text = {
                            if (product != null) {
                                androidx.compose.foundation.layout.Column(verticalArrangement = androidx.compose.foundation.layout.Arrangement.spacedBy(8.dp)) {
                                    androidx.compose.material3.Text(product.title, fontWeight = androidx.compose.ui.text.font.FontWeight.Bold, style = androidx.compose.material3.MaterialTheme.typography.titleMedium)
                                    androidx.compose.material3.Text("Kategori: ${product.category}", style = androidx.compose.material3.MaterialTheme.typography.bodySmall)
                                    androidx.compose.material3.Text("Fiyat: ₺${product.basePrice}", style = androidx.compose.material3.MaterialTheme.typography.bodyMedium, color = androidx.compose.material3.MaterialTheme.colorScheme.primary)
                                    val totalStock = product.stockByWarehouse.values.sum()
                                    androidx.compose.material3.Text("Toplam Stok: $totalStock AD", style = androidx.compose.material3.MaterialTheme.typography.bodyMedium, fontWeight = androidx.compose.ui.text.font.FontWeight.SemiBold)
                                }
                            } else {
                                androidx.compose.material3.Text("Bu barkoda ait ürün bulunamadı: $scannedCode")
                            }
                        },
                        confirmButton = {
                            androidx.compose.material3.TextButton(onClick = { com.example.ui.screens.AppDataStore.globalScannedBarcode = null }) {
                                androidx.compose.material3.Text("Tamam")
                            }
                        }
                    )
                }

                if (currentRoute == "sales") {
                    // Custom single-row bottom bar for Sales Screen has been hidden/removed from here.
                    // Tab switching and search have been integrated into the top of the SalesScreen itself.
                } else if (currentRoute?.startsWith("operations") == true && moduleArg == "purchase") {
                    // Custom bottom bar for Purchase (Alış)
                    val cartSize = com.example.ui.screens.AppDataStore.purchaseCart.sumOf { it.qty }
                    val activeTab = com.example.ui.screens.AppDataStore.purchaseSelectedTab.value
                    Card(
                        modifier = Modifier
                            .fillMaxWidth()
                            .animateContentSize(),
                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.95f)),
                        shape = RoundedCornerShape(topStart = 24.dp, topEnd = 24.dp, bottomStart = 0.dp, bottomEnd = 0.dp),
                        elevation = CardDefaults.cardElevation(defaultElevation = 8.dp)
                    ) {
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .navigationBarsPadding()
                                .padding(vertical = 12.dp, horizontal = 12.dp),
                            horizontalArrangement = Arrangement.SpaceAround,
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            // 1. Cari (Supplier) selection view button
                            val isTab0Selected = activeTab == 0
                            IconButton(
                                onClick = { com.example.ui.screens.AppDataStore.purchaseSelectedTab.value = 0 },
                                modifier = Modifier
                                    .background(
                                        if (isTab0Selected) MaterialTheme.colorScheme.primaryContainer else Color.Transparent,
                                        shape = RoundedCornerShape(12.dp)
                                    )
                                    .size(48.dp)
                            ) {
                                Icon(
                                    imageVector = Icons.Filled.Group,
                                    contentDescription = "Cari Seçimi",
                                    tint = if (isTab0Selected) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant
                                )
                            }

                            // 2. Ürünler add product tab button
                            val isTab1Selected = activeTab == 1
                            IconButton(
                                onClick = { com.example.ui.screens.AppDataStore.purchaseSelectedTab.value = 1 },
                                modifier = Modifier
                                    .background(
                                        if (isTab1Selected) MaterialTheme.colorScheme.primaryContainer else Color.Transparent,
                                        shape = RoundedCornerShape(12.dp)
                                    )
                                    .size(48.dp)
                            ) {
                                Icon(
                                    imageVector = Icons.Filled.AddShoppingCart,
                                    contentDescription = "Ürün Bilgisi Girişi",
                                    tint = if (isTab1Selected) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant
                                )
                            }

                            // 3. Sepet list / cart view button (compares and toggles)
                            val isTab2Selected = activeTab == 2
                            IconButton(
                                onClick = {
                                    if (isTab2Selected) {
                                        com.example.ui.screens.AppDataStore.purchaseSelectedTab.value = 0
                                    } else {
                                        com.example.ui.screens.AppDataStore.purchaseSelectedTab.value = 2
                                    }
                                },
                                modifier = Modifier
                                    .background(
                                        if (isTab2Selected) MaterialTheme.colorScheme.primaryContainer else Color.Transparent,
                                        shape = RoundedCornerShape(12.dp)
                                    )
                                    .size(48.dp)
                            ) {
                                Box(contentAlignment = Alignment.TopEnd) {
                                    Icon(
                                        imageVector = Icons.Filled.ShoppingCart,
                                        contentDescription = "Sepet",
                                        tint = if (isTab2Selected) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant
                                    )
                                    if (cartSize > 0) {
                                        Box(
                                            modifier = Modifier
                                                .offset(x = 6.dp, y = (-4).dp)
                                                .background(MaterialTheme.colorScheme.error, CircleShape)
                                                .padding(horizontal = 4.dp, vertical = 2.dp)
                                        ) {
                                            Text(
                                                text = cartSize.toString(),
                                                color = Color.White,
                                                style = MaterialTheme.typography.bodySmall.copy(fontSize = 9.sp, fontWeight = FontWeight.Bold)
                                            )
                                        }
                                    }
                                }
                            }

                            // 4. Quick Barcode scanner activation button
                            IconButton(
                                onClick = { com.example.ui.screens.AppDataStore.purchaseShowBarcodeScanner.value = true },
                                modifier = Modifier
                                    .background(
                                        MaterialTheme.colorScheme.secondaryContainer,
                                        shape = RoundedCornerShape(12.dp)
                                    )
                                    .size(48.dp)
                            ) {
                                Icon(
                                    imageVector = Icons.Filled.QrCodeScanner,
                                    contentDescription = "Barkod Tarayıcıyı Aç",
                                    tint = MaterialTheme.colorScheme.onSecondaryContainer
                                )
                            }
                        }
                    }
                } else if (currentRoute == "customers") {
                    val activeCust = com.example.ui.screens.AppDataStore.activeSelectedCustomer.value
                    if (activeCust != null) {
                        // Custom bottom bar with Özet, Hareketler, Ürünler, Notlar buttons for active customer details
                        Card(
                            modifier = Modifier
                                .fillMaxWidth()
                                .animateContentSize(),
                            colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.95f)),
                            shape = RoundedCornerShape(topStart = 24.dp, topEnd = 24.dp, bottomStart = 0.dp, bottomEnd = 0.dp),
                            elevation = CardDefaults.cardElevation(defaultElevation = 8.dp)
                        ) {
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .navigationBarsPadding()
                                    .padding(vertical = 12.dp, horizontal = 16.dp),
                                horizontalArrangement = Arrangement.SpaceAround,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                val selectedTab = com.example.ui.screens.AppDataStore.customerDetailActiveTab.value
                                
                                // Tab 0: Özet
                                val isTab0 = selectedTab == 0
                                Column(
                                    modifier = Modifier
                                        .weight(1f)
                                        .clip(RoundedCornerShape(16.dp))
                                        .clickable { com.example.ui.screens.AppDataStore.customerDetailActiveTab.value = 0 }
                                        .padding(vertical = 8.dp),
                                    horizontalAlignment = Alignment.CenterHorizontally,
                                    verticalArrangement = Arrangement.Center
                                ) {
                                    Icon(
                                        imageVector = Icons.Filled.Info,
                                        contentDescription = "Özet",
                                        tint = if (isTab0) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.6f),
                                        modifier = Modifier.size(24.dp)
                                    )
                                    Spacer(modifier = Modifier.height(4.dp))
                                    Text(
                                        text = "Özet",
                                        style = MaterialTheme.typography.labelSmall,
                                        color = if (isTab0) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.6f),
                                        fontWeight = if (isTab0) FontWeight.Bold else FontWeight.Normal
                                    )
                                }

                                // Tab 1: Hareketler
                                val isTab1 = selectedTab == 1
                                Column(
                                    modifier = Modifier
                                        .weight(1f)
                                        .clip(RoundedCornerShape(16.dp))
                                        .clickable { com.example.ui.screens.AppDataStore.customerDetailActiveTab.value = 1 }
                                        .padding(vertical = 8.dp),
                                    horizontalAlignment = Alignment.CenterHorizontally,
                                    verticalArrangement = Arrangement.Center
                                ) {
                                    Icon(
                                        imageVector = Icons.Filled.ReceiptLong,
                                        contentDescription = "Hareketler",
                                        tint = if (isTab1) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.6f),
                                        modifier = Modifier.size(24.dp)
                                    )
                                    Spacer(modifier = Modifier.height(4.dp))
                                    Text(
                                        text = "Hareketler",
                                        style = MaterialTheme.typography.labelSmall,
                                        color = if (isTab1) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.6f),
                                        fontWeight = if (isTab1) FontWeight.Bold else FontWeight.Normal
                                    )
                                }

                                // Tab 2: Ürünler
                                val isTab2 = selectedTab == 2
                                Column(
                                    modifier = Modifier
                                        .weight(1f)
                                        .clip(RoundedCornerShape(16.dp))
                                        .clickable { com.example.ui.screens.AppDataStore.customerDetailActiveTab.value = 2 }
                                        .padding(vertical = 8.dp),
                                    horizontalAlignment = Alignment.CenterHorizontally,
                                    verticalArrangement = Arrangement.Center
                                ) {
                                    Icon(
                                        imageVector = Icons.Filled.ShoppingBag,
                                        contentDescription = "Ürünler",
                                        tint = if (isTab2) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.6f),
                                        modifier = Modifier.size(24.dp)
                                    )
                                    Spacer(modifier = Modifier.height(4.dp))
                                    Text(
                                        text = "Ürünler",
                                        style = MaterialTheme.typography.labelSmall,
                                        color = if (isTab2) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.6f),
                                        fontWeight = if (isTab2) FontWeight.Bold else FontWeight.Normal
                                    )
                                }

                                // Tab 3: Notlar
                                val isTab3 = selectedTab == 3
                                Column(
                                    modifier = Modifier
                                        .weight(1f)
                                        .clip(RoundedCornerShape(16.dp))
                                        .clickable { com.example.ui.screens.AppDataStore.customerDetailActiveTab.value = 3 }
                                        .padding(vertical = 8.dp),
                                    horizontalAlignment = Alignment.CenterHorizontally,
                                    verticalArrangement = Arrangement.Center
                                ) {
                                    Icon(
                                        imageVector = Icons.Filled.EditNote,
                                        contentDescription = "Notlar",
                                        tint = if (isTab3) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.6f),
                                        modifier = Modifier.size(24.dp)
                                    )
                                    Spacer(modifier = Modifier.height(4.dp))
                                    Text(
                                        text = "Notlar",
                                        style = MaterialTheme.typography.labelSmall,
                                        color = if (isTab3) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.6f),
                                        fontWeight = if (isTab3) FontWeight.Bold else FontWeight.Normal
                                    )
                                }
                            }
                        }
                    } else {
                        // Custom non-expandable unified bottom bar for Customers Screen
                        Card(
                            modifier = Modifier
                                .fillMaxWidth()
                                .animateContentSize(),
                            colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.95f)),
                            shape = RoundedCornerShape(topStart = 24.dp, topEnd = 24.dp, bottomStart = 0.dp, bottomEnd = 0.dp),
                            elevation = CardDefaults.cardElevation(defaultElevation = 8.dp)
                        ) {
                        Column(
                            modifier = Modifier
                                .fillMaxWidth()
                                .navigationBarsPadding()
                                .padding(vertical = 8.dp, horizontal = 12.dp)
                        ) {
                            // Row 1: Search box & "Yeni" button side by side (unified and clean)
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(vertical = 4.dp),
                                horizontalArrangement = Arrangement.spacedBy(8.dp),
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                androidx.compose.material3.OutlinedTextField(
                                    value = com.example.ui.screens.AppDataStore.customerSearchQuery.value,
                                    onValueChange = { com.example.ui.screens.AppDataStore.customerSearchQuery.value = it },
                                    placeholder = { Text("Müşteri ara...", style = MaterialTheme.typography.bodySmall) },
                                    leadingIcon = { Icon(Icons.Filled.Search, null, modifier = Modifier.size(18.dp)) },
                                    trailingIcon = {
                                        if (com.example.ui.screens.AppDataStore.customerSearchQuery.value.isNotEmpty()) {
                                            IconButton(onClick = { com.example.ui.screens.AppDataStore.customerSearchQuery.value = "" }) {
                                                Icon(Icons.Filled.Close, null, modifier = Modifier.size(18.dp))
                                            }
                                        }
                                    },
                                    modifier = Modifier
                                        .weight(1f)
                                        .height(48.dp),
                                    textStyle = MaterialTheme.typography.bodySmall,
                                    singleLine = true,
                                    shape = RoundedCornerShape(12.dp),
                                    colors = androidx.compose.material3.OutlinedTextFieldDefaults.colors(
                                        unfocusedContainerColor = MaterialTheme.colorScheme.surfaceContainerLowest,
                                        focusedContainerColor = MaterialTheme.colorScheme.surfaceContainerLowest,
                                    )
                                )

                                // Yeni Cari Ekleme Düğmesi
                                Button(
                                    onClick = { com.example.ui.screens.AppDataStore.customerShowAddDialog.value = true },
                                    shape = RoundedCornerShape(12.dp),
                                    contentPadding = PaddingValues(horizontal = 12.dp, vertical = 6.dp),
                                    modifier = Modifier.height(48.dp)
                                ) {
                                    Icon(Icons.Filled.PersonAdd, contentDescription = "Yeni Cari", modifier = Modifier.size(18.dp))
                                    Spacer(modifier = Modifier.width(4.dp))
                                    Text("Yeni", style = MaterialTheme.typography.bodySmall)
                                }
                            }

                            Spacer(modifier = Modifier.height(2.dp))
                            androidx.compose.material3.HorizontalDivider(color = MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.12f))
                            Spacer(modifier = Modifier.height(2.dp))

                            // Row 3: Standard bottom navigation tabs so they can navigate perfectly!
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(top = 8.dp),
                                horizontalArrangement = Arrangement.SpaceAround,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                val activeTabs = com.example.ui.screens.AppDataStore.bottomBarTabs.take(4)
                                val screenMap = mapOf(
                                    "dashboard" to Screen("dashboard", "Giriş", Icons.Filled.Dashboard),
                                    "customers" to Screen("customers", "Cari", Icons.Filled.PersonSearch),
                                    "catalog" to Screen("catalog", "Katalog", Icons.Filled.Inventory),
                                    "reports" to Screen("reports", "Rapor", Icons.Filled.Analytics),
                                    "more" to Screen("more", "Ayarlar", Icons.Filled.MoreHoriz),
                                    "sales" to Screen("sales", "Satış", Icons.Filled.ShoppingCart),
                                    "suspended_sales" to Screen("suspended_sales", "Bekleyen", Icons.Filled.HourglassEmpty)
                                )

                                for (i in 0..1) {
                                    val routeStr = activeTabs.getOrNull(i) ?: "dashboard"
                                    val screen = screenMap[routeStr] ?: screenMap["dashboard"]!!
                                    val isSelected = currentRoute == screen.route

                                    Column(
                                        modifier = Modifier
                                            .weight(1f)
                                            .clip(RoundedCornerShape(16.dp))
                                            .clickable {
                                                navController.navigate(screen.route) {
                                                    popUpTo(navController.graph.findStartDestination().id) {
                                                        saveState = true
                                                    }
                                                    launchSingleTop = true
                                                    restoreState = true
                                                }
                                            }
                                            .padding(vertical = 6.dp),
                                        horizontalAlignment = Alignment.CenterHorizontally,
                                        verticalArrangement = Arrangement.Center
                                    ) {
                                        Icon(
                                            imageVector = screen.icon,
                                            contentDescription = screen.title,
                                            tint = if (isSelected) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.6f),
                                            modifier = Modifier.size(24.dp)
                                        )
                                        Spacer(modifier = Modifier.height(4.dp))
                                        Text(
                                            text = screen.title,
                                            style = MaterialTheme.typography.labelSmall,
                                            color = if (isSelected) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.6f),
                                            fontWeight = if (isSelected) FontWeight.Bold else FontWeight.Normal
                                        )
                                    }
                                }

                                // Center Scanner Hub (with same style)
                                Box(
                                    modifier = Modifier
                                        .padding(horizontal = 4.dp)
                                        .size(48.dp)
                                        .shadow(elevation = 6.dp, shape = CircleShape)
                                        .background(MaterialTheme.colorScheme.primary, CircleShape)
                                        .clickable {
                                            com.example.ui.screens.AppDataStore.globalShowBarcodeScanner = true
                                        },
                                    contentAlignment = Alignment.Center
                                ) {
                                    Icon(
                                        imageVector = Icons.Filled.QrCodeScanner,
                                        contentDescription = "Barkod Oku",
                                        tint = MaterialTheme.colorScheme.onPrimary,
                                        modifier = Modifier.size(24.dp)
                                    )
                                }

                                for (i in 2..3) {
                                    val routeStr = activeTabs.getOrNull(i) ?: "catalog"
                                    val screen = screenMap[routeStr] ?: screenMap["catalog"]!!
                                    val isSelected = currentRoute == screen.route

                                    Column(
                                        modifier = Modifier
                                            .weight(1f)
                                            .clip(RoundedCornerShape(16.dp))
                                            .clickable {
                                                navController.navigate(screen.route) {
                                                    popUpTo(navController.graph.findStartDestination().id) {
                                                        saveState = true
                                                    }
                                                    launchSingleTop = true
                                                    restoreState = true
                                                }
                                            }
                                            .padding(vertical = 6.dp),
                                        horizontalAlignment = Alignment.CenterHorizontally,
                                        verticalArrangement = Arrangement.Center
                                    ) {
                                        Icon(
                                            imageVector = screen.icon,
                                            contentDescription = screen.title,
                                            tint = if (isSelected) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.6f),
                                            modifier = Modifier.size(24.dp)
                                        )
                                        Spacer(modifier = Modifier.height(4.dp))
                                        Text(
                                            text = screen.title,
                                            style = MaterialTheme.typography.labelSmall,
                                            color = if (isSelected) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.6f),
                                            fontWeight = if (isSelected) FontWeight.Bold else FontWeight.Normal
                                        )
                                    }
                                }
                            }
                        }
                    }
                    }
                } else if (currentRoute == "catalog") {
                    // Custom single-row bottom bar for Catalog Screen
                    Card(
                        modifier = Modifier
                            .fillMaxWidth()
                            .animateContentSize(),
                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.95f)),
                        shape = RoundedCornerShape(topStart = 24.dp, topEnd = 24.dp, bottomStart = 0.dp, bottomEnd = 0.dp),
                        elevation = CardDefaults.cardElevation(defaultElevation = 8.dp)
                    ) {
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .navigationBarsPadding()
                                .padding(vertical = 12.dp, horizontal = 12.dp),
                            horizontalArrangement = Arrangement.spacedBy(6.dp),
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            // Back Button (Geri butonu, arama kutusunun soluna alındı)
                            IconButton(
                                onClick = { navController.popBackStack() },
                                modifier = Modifier
                                    .background(
                                        MaterialTheme.colorScheme.surfaceContainerLowest,
                                        shape = RoundedCornerShape(10.dp)
                                    )
                                    .size(36.dp)
                            ) {
                                Icon(
                                    imageVector = Icons.Filled.ArrowBack,
                                    contentDescription = "Geri",
                                    tint = MaterialTheme.colorScheme.onSurfaceVariant,
                                    modifier = Modifier.size(18.dp)
                                )
                            }

                            // 1. Search Bar Field (Takes up remaining weight)
                            androidx.compose.material3.OutlinedTextField(
                                value = com.example.ui.screens.AppDataStore.catalogSearchQuery.value,
                                onValueChange = { com.example.ui.screens.AppDataStore.catalogSearchQuery.value = it },
                                placeholder = { Text("Katalogda ara...", style = MaterialTheme.typography.bodySmall) },
                                leadingIcon = { Icon(Icons.Filled.Search, null, modifier = Modifier.size(18.dp)) },
                                trailingIcon = {
                                    if (com.example.ui.screens.AppDataStore.catalogSearchQuery.value.isNotEmpty()) {
                                        IconButton(onClick = { com.example.ui.screens.AppDataStore.catalogSearchQuery.value = "" }) {
                                            Icon(Icons.Filled.Close, null, modifier = Modifier.size(18.dp))
                                        }
                                    }
                                },
                                modifier = Modifier
                                    .weight(1f)
                                    .height(48.dp),
                                textStyle = MaterialTheme.typography.bodySmall,
                                singleLine = true,
                                shape = RoundedCornerShape(12.dp),
                                colors = androidx.compose.material3.OutlinedTextFieldDefaults.colors(
                                    unfocusedContainerColor = MaterialTheme.colorScheme.surfaceContainerLowest,
                                    focusedContainerColor = MaterialTheme.colorScheme.surfaceContainerLowest,
                                )
                            )

                            // 2. Plus/Toggle Filter Button
                            IconButton(
                                onClick = { showCatalogFilters = !showCatalogFilters },
                                modifier = Modifier
                                    .background(
                                        if (showCatalogFilters) MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surfaceVariant,
                                        shape = RoundedCornerShape(12.dp)
                                    )
                                    .size(48.dp)
                            ) {
                                Icon(
                                    imageVector = if (showCatalogFilters) Icons.Filled.Close else Icons.Filled.Add,
                                    contentDescription = "Filtreleri Göster",
                                    tint = if (showCatalogFilters) MaterialTheme.colorScheme.onPrimaryContainer else MaterialTheme.colorScheme.onSurfaceVariant
                                )
                            }

                            // 3. Conditional filter & sort buttons
                            if (showCatalogFilters) {
                                // Filter Selector (triggers advanced filter dialog)
                                val activeFilterCount = (if (com.example.ui.screens.AppDataStore.catalogFilterBrands.value.isNotEmpty()) 1 else 0) +
                                         (if (com.example.ui.screens.AppDataStore.catalogFilterCategories.value.isNotEmpty()) 1 else 0) +
                                         (if (com.example.ui.screens.AppDataStore.catalogFilterAmbalajs.value.isNotEmpty()) 1 else 0) +
                                         (if (com.example.ui.screens.AppDataStore.catalogFilterMinPrice.value.isNotEmpty()) 1 else 0) +
                                         (if (com.example.ui.screens.AppDataStore.catalogFilterMaxPrice.value.isNotEmpty()) 1 else 0) +
                                         (if (com.example.ui.screens.AppDataStore.catalogFilterMinStock.value.isNotEmpty()) 1 else 0) +
                                         (if (com.example.ui.screens.AppDataStore.catalogFilterMaxStock.value.isNotEmpty()) 1 else 0) +
                                         (if (com.example.ui.screens.AppDataStore.catalogFilterHideNoPhoto.value) 1 else 0) +
                                         (if (com.example.ui.screens.AppDataStore.catalogFilterHideOutOfStock.value) 1 else 0)

                                 IconButton(
                                     onClick = { com.example.ui.screens.AppDataStore.catalogShowFiltersDialog.value = true },
                                     modifier = Modifier
                                         .background(
                                             if (activeFilterCount > 0) MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surfaceVariant,
                                             shape = RoundedCornerShape(12.dp)
                                         )
                                         .size(48.dp)
                                 ) {
                                     Icon(
                                         Icons.Filled.FilterList,
                                         contentDescription = "Filtreler",
                                         tint = if (activeFilterCount > 0) MaterialTheme.colorScheme.onPrimaryContainer else MaterialTheme.colorScheme.onSurfaceVariant
                                     )
                                 }

                                // Sort Selector
                                var sortMenuExt by remember { mutableStateOf(false) }
                                Box {
                                    val currentField = com.example.ui.screens.AppDataStore.catalogSelectedSortField.value
                                    val isAsc = com.example.ui.screens.AppDataStore.catalogSelectedSortAsc.value
                                    val isNotDefaultSort = currentField != "İsim" || !isAsc
                                    IconButton(
                                        onClick = { sortMenuExt = true },
                                        modifier = Modifier
                                            .background(
                                                if (isNotDefaultSort) MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surfaceVariant,
                                                shape = RoundedCornerShape(12.dp)
                                            )
                                            .size(48.dp)
                                    ) {
                                        Icon(
                                            Icons.Filled.Sort,
                                            contentDescription = "Sıralama",
                                            tint = if (isNotDefaultSort) MaterialTheme.colorScheme.onPrimaryContainer else MaterialTheme.colorScheme.onSurfaceVariant
                                        )
                                    }
                                    androidx.compose.material3.DropdownMenu(
                                        expanded = sortMenuExt,
                                        onDismissRequest = { sortMenuExt = false }
                                    ) {
                                        val sortOptions = listOf(
                                            Triple("İsim", true, "İsim [A-Z]"),
                                            Triple("İsim", false, "İsim [Z-A]"),
                                            Triple("Kod", true, "Kod [Artan]"),
                                            Triple("Kod", false, "Kod [Azalan]"),
                                            Triple("Fiyat", true, "Fiyat [Artan]"),
                                            Triple("Fiyat", false, "Fiyat [Azalan]"),
                                            Triple("Marka", true, "Marka [A-Z]"),
                                            Triple("Marka", false, "Marka [Z-A]"),
                                            Triple("Stok", true, "Stok [Artan]"),
                                            Triple("Stok", false, "Stok [Azalan]")
                                        )
                                        sortOptions.forEach { opt ->
                                            androidx.compose.material3.DropdownMenuItem(
                                                text = { Text(opt.third, fontWeight = if (currentField == opt.first && isAsc == opt.second) FontWeight.Bold else FontWeight.Normal) },
                                                onClick = {
                                                    com.example.ui.screens.AppDataStore.catalogSelectedSortField.value = opt.first
                                                    com.example.ui.screens.AppDataStore.catalogSelectedSortAsc.value = opt.second
                                                    sortMenuExt = false
                                                },
                                                leadingIcon = {
                                                    if (currentField == opt.first && isAsc == opt.second) {
                                                        Icon(Icons.Filled.Check, contentDescription = null, modifier = Modifier.size(18.dp))
                                                    }
                                                }
                                            )
                                        }
                                    }
                                }

                                // View Mode Selector (Görünüm İkonu)
                                val isGridView = com.example.ui.screens.AppDataStore.catalogSelectedViewMode.value == "Grid"
                                IconButton(
                                    onClick = {
                                        com.example.ui.screens.AppDataStore.catalogSelectedViewMode.value = if (isGridView) "List" else "Grid"
                                    },
                                    modifier = Modifier
                                        .background(
                                            MaterialTheme.colorScheme.primaryContainer,
                                            shape = RoundedCornerShape(12.dp)
                                        )
                                        .size(48.dp)
                                ) {
                                    Icon(
                                        imageVector = if (isGridView) Icons.Filled.ViewList else Icons.Filled.GridView,
                                        contentDescription = "Görünüm Değiştir",
                                        tint = MaterialTheme.colorScheme.onPrimaryContainer
                                    )
                                }
                            }

                            // 4. Shopping Cart (Sepet) Button
                            val cartSize = com.example.ui.screens.AppDataStore.catalogCartItems.size
                            Box {
                                IconButton(
                                    onClick = {
                                        com.example.ui.screens.AppDataStore.catalogShowCartDialog.value = true
                                    },
                                    modifier = Modifier
                                        .background(MaterialTheme.colorScheme.surfaceVariant, shape = RoundedCornerShape(12.dp))
                                        .size(48.dp)
                                ) {
                                    Icon(
                                        imageVector = Icons.Filled.ShoppingCart,
                                        contentDescription = "Sepet",
                                        tint = MaterialTheme.colorScheme.onSurfaceVariant
                                    )
                                }
                                if (cartSize > 0) {
                                    Box(
                                        modifier = Modifier
                                            .align(Alignment.TopEnd)
                                            .offset(x = 4.dp, y = (-4).dp)
                                            .background(MaterialTheme.colorScheme.error, shape = CircleShape)
                                            .size(18.dp),
                                        contentAlignment = Alignment.Center
                                    ) {
                                        Text(
                                            text = cartSize.toString(),
                                            style = MaterialTheme.typography.labelSmall.copy(fontSize = 10.sp),
                                            color = MaterialTheme.colorScheme.onError,
                                            fontWeight = FontWeight.Bold
                                        )
                                    }
                                }
                            }

                            // 5. Barcode Scanner Button
                            IconButton(
                                onClick = {
                                    com.example.ui.screens.AppDataStore.catalogShowBarcodeScanner.value = true
                                },
                                modifier = Modifier
                                    .background(MaterialTheme.colorScheme.primary, shape = RoundedCornerShape(12.dp))
                                    .size(48.dp)
                            ) {
                                Icon(
                                    imageVector = Icons.Filled.QrCodeScanner,
                                    contentDescription = "Barkod Okut",
                                    tint = MaterialTheme.colorScheme.onPrimary
                                )
                            }
                        }
                    }
                } else if (currentRoute?.startsWith("operations") == true && moduleArg == "stocks") {
                    // Custom single-row bottom bar for Stocks Screen
                    Card(
                        modifier = Modifier
                            .fillMaxWidth()
                            .animateContentSize(),
                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.95f)),
                        shape = RoundedCornerShape(topStart = 24.dp, topEnd = 24.dp, bottomStart = 0.dp, bottomEnd = 0.dp),
                        elevation = CardDefaults.cardElevation(defaultElevation = 8.dp)
                    ) {
                        Column(
                            modifier = Modifier
                                .fillMaxWidth()
                                .navigationBarsPadding()
                                .padding(vertical = 12.dp, horizontal = 12.dp),
                            verticalArrangement = Arrangement.spacedBy(8.dp)
                        ) {
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.spacedBy(6.dp),
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                // 1. Search Bar Field (Narrowed when filters are shown)
                                androidx.compose.material3.OutlinedTextField(
                                    value = com.example.ui.screens.AppDataStore.stocksSearchQuery.value,
                                    onValueChange = { com.example.ui.screens.AppDataStore.stocksSearchQuery.value = it },
                                    placeholder = { Text("Stoklarda ara...", style = MaterialTheme.typography.bodySmall) },
                                    leadingIcon = { Icon(Icons.Filled.Search, null, modifier = Modifier.size(18.dp)) },
                                    trailingIcon = {
                                        if (com.example.ui.screens.AppDataStore.stocksSearchQuery.value.isNotEmpty()) {
                                            IconButton(onClick = { com.example.ui.screens.AppDataStore.stocksSearchQuery.value = "" }) {
                                                Icon(Icons.Filled.Close, null, modifier = Modifier.size(18.dp))
                                            }
                                        }
                                    },
                                    modifier = Modifier
                                        .weight(if (showStocksFilters) 0.8f else 1.5f)
                                        .height(48.dp),
                                    textStyle = MaterialTheme.typography.bodySmall,
                                    singleLine = true,
                                    shape = RoundedCornerShape(12.dp),
                                    colors = androidx.compose.material3.OutlinedTextFieldDefaults.colors(
                                        unfocusedContainerColor = MaterialTheme.colorScheme.surfaceContainerLowest,
                                        focusedContainerColor = MaterialTheme.colorScheme.surfaceContainerLowest,
                                    )
                                )

                                // Category and sorting in the same row when opened
                                if (showStocksFilters) {
                                    // Category Filter Dropdown
                                    var catMenuExt by remember { mutableStateOf(false) }
                                    Box(modifier = Modifier.weight(0.7f)) {
                                        androidx.compose.material3.Button(
                                            onClick = { catMenuExt = true },
                                            colors = androidx.compose.material3.ButtonDefaults.buttonColors(
                                                containerColor = if (com.example.ui.screens.AppDataStore.stocksSelectedCategory.value != "Hepsi") MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surfaceVariant,
                                                contentColor = if (com.example.ui.screens.AppDataStore.stocksSelectedCategory.value != "Hepsi") MaterialTheme.colorScheme.onPrimaryContainer else MaterialTheme.colorScheme.onSurfaceVariant
                                            ),
                                            shape = RoundedCornerShape(12.dp),
                                            modifier = Modifier.fillMaxWidth().height(48.dp),
                                            contentPadding = PaddingValues(horizontal = 4.dp)
                                        ) {
                                            Icon(Icons.Filled.FilterList, null, modifier = Modifier.size(14.dp))
                                            Spacer(modifier = Modifier.width(2.dp))
                                            Text(
                                                text = com.example.ui.screens.AppDataStore.stocksSelectedCategory.value,
                                                style = MaterialTheme.typography.labelSmall,
                                                maxLines = 1,
                                                overflow = androidx.compose.ui.text.style.TextOverflow.Ellipsis
                                            )
                                        }
                                        androidx.compose.material3.DropdownMenu(
                                            expanded = catMenuExt,
                                            onDismissRequest = { catMenuExt = false }
                                        ) {
                                            val cats = listOf("Hepsi") + com.example.ui.screens.AppDataStore.products.map { it.category }.distinct().filter { it.isNotBlank() }.sorted()
                                            cats.forEach { cat ->
                                                androidx.compose.material3.DropdownMenuItem(
                                                    text = { Text(cat, fontWeight = if (com.example.ui.screens.AppDataStore.stocksSelectedCategory.value == cat) FontWeight.Bold else FontWeight.Normal) },
                                                    onClick = {
                                                        com.example.ui.screens.AppDataStore.stocksSelectedCategory.value = cat
                                                        catMenuExt = false
                                                    },
                                                    leadingIcon = {
                                                        if (com.example.ui.screens.AppDataStore.stocksSelectedCategory.value == cat) {
                                                            Icon(Icons.Filled.Check, contentDescription = null, modifier = Modifier.size(18.dp))
                                                        }
                                                    }
                                                )
                                            }
                                        }
                                    }

                                    // Sort order Dropdown
                                    var sortMenuExt by remember { mutableStateOf(false) }
                                    Box(modifier = Modifier.weight(0.7f)) {
                                        androidx.compose.material3.Button(
                                            onClick = { sortMenuExt = true },
                                            colors = androidx.compose.material3.ButtonDefaults.buttonColors(
                                                containerColor = if (com.example.ui.screens.AppDataStore.stocksSelectedSortOrder.value != "Ürün Adı [A-Z]") MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surfaceVariant,
                                                contentColor = if (com.example.ui.screens.AppDataStore.stocksSelectedSortOrder.value != "Ürün Adı [A-Z]") MaterialTheme.colorScheme.onPrimaryContainer else MaterialTheme.colorScheme.onSurfaceVariant
                                            ),
                                            shape = RoundedCornerShape(12.dp),
                                            modifier = Modifier.fillMaxWidth().height(48.dp),
                                            contentPadding = PaddingValues(horizontal = 4.dp)
                                        ) {
                                            Icon(Icons.Filled.Sort, null, modifier = Modifier.size(14.dp))
                                            Spacer(modifier = Modifier.width(2.dp))
                                            Text(
                                                text = com.example.ui.screens.AppDataStore.stocksSelectedSortOrder.value.replace("Ürün Adı ", "").replace("Miktar ", ""),
                                                style = MaterialTheme.typography.labelSmall,
                                                maxLines = 1,
                                                overflow = androidx.compose.ui.text.style.TextOverflow.Ellipsis
                                            )
                                        }
                                        androidx.compose.material3.DropdownMenu(
                                            expanded = sortMenuExt,
                                            onDismissRequest = { sortMenuExt = false }
                                        ) {
                                            val orderModes = listOf("Ürün Adı [A-Z]", "Ürün Adı [Z-A]", "Miktar [Azalan]", "Miktar [Artan]")
                                            orderModes.forEach { mode ->
                                                androidx.compose.material3.DropdownMenuItem(
                                                    text = { Text(mode, fontWeight = if (com.example.ui.screens.AppDataStore.stocksSelectedSortOrder.value == mode) FontWeight.Bold else FontWeight.Normal) },
                                                    onClick = {
                                                        com.example.ui.screens.AppDataStore.stocksSelectedSortOrder.value = mode
                                                        sortMenuExt = false
                                                    },
                                                    leadingIcon = {
                                                        if (com.example.ui.screens.AppDataStore.stocksSelectedSortOrder.value == mode) {
                                                            Icon(Icons.Filled.Check, contentDescription = null, modifier = Modifier.size(18.dp))
                                                        }
                                                    }
                                                )
                                            }
                                        }
                                    }

                                    // Add Product Button Inside Filter Menu
                                    val appCxt = androidx.compose.ui.platform.LocalContext.current
                                    val isErp = com.example.ui.screens.AppDataStore.isErpModeActive(appCxt)
                                    if (!isErp) {
                                        IconButton(
                                            onClick = { com.example.ui.screens.AppDataStore.stocksShowAddProductDialog.value = true },
                                            modifier = Modifier
                                                .background(MaterialTheme.colorScheme.secondaryContainer, shape = RoundedCornerShape(12.dp))
                                                .size(48.dp)
                                        ) {
                                            Icon(
                                                imageVector = Icons.Filled.Add,
                                                contentDescription = "Yeni Ürün Ekle",
                                                tint = MaterialTheme.colorScheme.onSecondaryContainer
                                            )
                                        }
                                    }
                                }

                                // Filter Button (replaced + toggle with actual filter toggle icon)
                                IconButton(
                                    onClick = { showStocksFilters = !showStocksFilters },
                                    modifier = Modifier
                                        .background(
                                            if (showStocksFilters) MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surfaceVariant,
                                            shape = RoundedCornerShape(12.dp)
                                        )
                                        .size(48.dp)
                                ) {
                                    Icon(
                                        imageVector = if (showStocksFilters) Icons.Filled.Close else Icons.Filled.FilterList,
                                        contentDescription = "Filtreleri Göster",
                                        tint = if (showStocksFilters) MaterialTheme.colorScheme.onPrimaryContainer else MaterialTheme.colorScheme.onSurfaceVariant
                                    )
                                }

                                // 4. Barcode Scanner Button
                                IconButton(
                                    onClick = {
                                        com.example.ui.screens.AppDataStore.stocksShowBarcodeScanner.value = true
                                    },
                                    modifier = Modifier
                                        .background(MaterialTheme.colorScheme.primary, shape = RoundedCornerShape(12.dp))
                                        .size(48.dp)
                                ) {
                                    Icon(
                                        imageVector = Icons.Filled.QrCodeScanner,
                                        contentDescription = "Barkod Okut",
                                        tint = MaterialTheme.colorScheme.onPrimary
                                    )
                                }
                            }
                        }
                    }
                } else if (currentRoute?.startsWith("operations") == true && moduleArg == "approvals") {
                    // Custom combined bottom bar for Approvals Screen (Onay Merkezi)
                    Card(
                        modifier = Modifier
                            .fillMaxWidth()
                            .animateContentSize(),
                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.95f)),
                        shape = RoundedCornerShape(topStart = 24.dp, topEnd = 24.dp, bottomStart = 0.dp, bottomEnd = 0.dp),
                        elevation = CardDefaults.cardElevation(defaultElevation = 8.dp)
                    ) {
                        Column(
                            modifier = Modifier
                                .fillMaxWidth()
                                .navigationBarsPadding()
                                .padding(vertical = 8.dp),
                            verticalArrangement = Arrangement.spacedBy(4.dp)
                        ) {
                            // Row 1: Search / Filters Row
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(horizontal = 12.dp, vertical = 4.dp),
                                horizontalArrangement = Arrangement.spacedBy(8.dp),
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                val context = androidx.compose.ui.platform.LocalContext.current
                                val calendar = java.util.Calendar.getInstance()
                                val datePickerDialog = remember {
                                    android.app.DatePickerDialog(
                                        context,
                                        { _, year, month, dayOfMonth ->
                                            val selectedDateStr = String.format("%02d.%02d.%04d", dayOfMonth, month + 1, year)
                                            com.example.ui.screens.AppDataStore.approvalSelectedDateFilter.value = selectedDateStr
                                        },
                                        calendar.get(java.util.Calendar.YEAR),
                                        calendar.get(java.util.Calendar.MONTH),
                                        calendar.get(java.util.Calendar.DAY_OF_MONTH)
                                    )
                                }

                                val isSearchExpanded = com.example.ui.screens.AppDataStore.approvalIsSearchExpanded.value
                                val searchQuery = com.example.ui.screens.AppDataStore.approvalSearchQuery.value
                                val selectedFilter = com.example.ui.screens.AppDataStore.approvalSelectedFilter.value

                                IconButton(
                                    onClick = { com.example.ui.screens.AppDataStore.approvalIsSearchExpanded.value = !isSearchExpanded },
                                    modifier = Modifier
                                        .background(
                                            if (isSearchExpanded) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.secondaryContainer,
                                            shape = CircleShape
                                        )
                                        .size(40.dp)
                                ) {
                                    Icon(
                                        imageVector = if (isSearchExpanded) Icons.Filled.Close else Icons.Filled.Add,
                                        contentDescription = "Filtre ve Arama Göster",
                                        tint = if (isSearchExpanded) MaterialTheme.colorScheme.onPrimary else MaterialTheme.colorScheme.onSecondaryContainer,
                                        modifier = Modifier.size(20.dp)
                                    )
                                }

                                if (isSearchExpanded) {
                                    androidx.compose.material3.OutlinedTextField(
                                        value = searchQuery,
                                        onValueChange = { com.example.ui.screens.AppDataStore.approvalSearchQuery.value = it },
                                        placeholder = { Text("Arama...", style = MaterialTheme.typography.bodySmall) },
                                        leadingIcon = { Icon(Icons.Filled.Search, null, modifier = Modifier.size(16.dp)) },
                                        trailingIcon = {
                                            if (searchQuery.isNotEmpty()) {
                                                IconButton(onClick = { com.example.ui.screens.AppDataStore.approvalSearchQuery.value = "" }) {
                                                    Icon(Icons.Filled.Close, null, modifier = Modifier.size(16.dp))
                                                }
                                            }
                                        },
                                        modifier = Modifier
                                            .weight(1f)
                                            .height(40.dp),
                                        textStyle = MaterialTheme.typography.bodySmall,
                                        singleLine = true,
                                        shape = RoundedCornerShape(10.dp),
                                        colors = androidx.compose.material3.OutlinedTextFieldDefaults.colors(
                                            unfocusedContainerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.4f),
                                            focusedContainerColor = MaterialTheme.colorScheme.surface,
                                        )
                                    )

                                    IconButton(
                                        onClick = { datePickerDialog.show() },
                                        modifier = Modifier
                                            .background(MaterialTheme.colorScheme.primaryContainer, shape = RoundedCornerShape(10.dp))
                                            .size(40.dp)
                                    ) {
                                        Icon(
                                            Icons.Filled.DateRange,
                                            contentDescription = "Tarih Seç",
                                            tint = MaterialTheme.colorScheme.onPrimaryContainer,
                                            modifier = Modifier.size(20.dp)
                                        )
                                    }

                                    // Status filter rotating button (Bekleyen -> Onaylanan -> Reddedilen)
                                    val statusFilter = com.example.ui.screens.AppDataStore.approvalStatusFilter.value
                                    val statusIcon = when (statusFilter) {
                                        "Onaylanan" -> Icons.Filled.CheckCircle
                                        "Reddedilen" -> Icons.Filled.Close
                                        else -> Icons.Filled.HourglassEmpty
                                    }
                                    val statusBg = when (statusFilter) {
                                        "Onaylanan" -> Color(0xFF2E7D32)
                                        "Reddedilen" -> Color(0xFFC62828)
                                        else -> Color(0xFFF57C00)
                                    }
                                    val statusTint = Color.White
                                    IconButton(
                                        onClick = {
                                            val nextVal = when (statusFilter) {
                                                "Bekleyen" -> "Onaylanan"
                                                "Onaylanan" -> "Reddedilen"
                                                else -> "Bekleyen"
                                            }
                                            com.example.ui.screens.AppDataStore.approvalStatusFilter.value = nextVal
                                        },
                                        modifier = Modifier
                                            .background(statusBg, shape = RoundedCornerShape(10.dp))
                                            .size(40.dp)
                                    ) {
                                        Icon(
                                            statusIcon,
                                            contentDescription = "Durum: $statusFilter",
                                            tint = statusTint,
                                            modifier = Modifier.size(20.dp)
                                        )
                                    }
                                } else {
                                    androidx.compose.foundation.lazy.LazyRow(
                                        modifier = Modifier.weight(1f),
                                        horizontalArrangement = Arrangement.spacedBy(8.dp),
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        val categories = listOf("Tümü", "Satış", "Tahsilat", "İade", "Alış", "Tediye")
                                        items(categories.size) { index ->
                                            val filter = categories[index]
                                            val isSel = selectedFilter == filter
                                            val count = if (filter == "Tümü") {
                                                com.example.ui.screens.AppDataStore.approvalItems.size
                                            } else {
                                                com.example.ui.screens.AppDataStore.approvalItems.count { it.type == filter }
                                            }
                                            Card(
                                                onClick = { com.example.ui.screens.AppDataStore.approvalSelectedFilter.value = filter },
                                                colors = CardDefaults.cardColors(
                                                    containerColor = if (isSel) MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surfaceVariant
                                                ),
                                                shape = RoundedCornerShape(12.dp)
                                            ) {
                                                Row(
                                                    modifier = Modifier.padding(horizontal = 12.dp, vertical = 8.dp),
                                                    verticalAlignment = Alignment.CenterVertically,
                                                    horizontalArrangement = Arrangement.spacedBy(4.dp)
                                                ) {
                                                    Text(
                                                        text = filter,
                                                        style = MaterialTheme.typography.labelSmall,
                                                        fontWeight = if (isSel) FontWeight.Bold else FontWeight.Normal,
                                                        color = if (isSel) MaterialTheme.colorScheme.onPrimaryContainer else MaterialTheme.colorScheme.onSurfaceVariant
                                                    )
                                                    if (count > 0) {
                                                        Box(
                                                            modifier = Modifier
                                                                .background(Color(0xFFE53935), shape = CircleShape)
                                                                .padding(horizontal = 6.dp, vertical = 2.dp),
                                                            contentAlignment = Alignment.Center
                                                        ) {
                                                            Text(
                                                                text = count.toString(),
                                                                style = MaterialTheme.typography.labelSmall.copy(fontSize = 10.sp, fontWeight = FontWeight.Bold),
                                                                color = Color.White
                                                            )
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (com.example.ui.screens.AppDataStore.approvalIsSearchExpanded.value) {
                                Spacer(modifier = Modifier.height(2.dp))
                                androidx.compose.material3.HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
                                Spacer(modifier = Modifier.height(2.dp))

                                androidx.compose.foundation.lazy.LazyRow(
                                    modifier = Modifier.fillMaxWidth().padding(horizontal = 12.dp),
                                    horizontalArrangement = Arrangement.spacedBy(8.dp),
                                    verticalAlignment = Alignment.CenterVertically
                                ) {
                                    val categories = listOf("Tümü", "Satış", "Tahsilat", "İade", "Alış", "Tediye")
                                    items(categories.size) { index ->
                                        val filter = categories[index]
                                        val isSel = com.example.ui.screens.AppDataStore.approvalSelectedFilter.value == filter
                                        val count = if (filter == "Tümü") {
                                            com.example.ui.screens.AppDataStore.approvalItems.size
                                        } else {
                                            com.example.ui.screens.AppDataStore.approvalItems.count { it.type == filter }
                                        }
                                        Card(
                                            onClick = { com.example.ui.screens.AppDataStore.approvalSelectedFilter.value = filter },
                                            colors = CardDefaults.cardColors(
                                                containerColor = if (isSel) MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surfaceVariant
                                            ),
                                            shape = RoundedCornerShape(12.dp)
                                        ) {
                                            Row(
                                                modifier = Modifier.padding(horizontal = 12.dp, vertical = 8.dp),
                                                verticalAlignment = Alignment.CenterVertically,
                                                horizontalArrangement = Arrangement.spacedBy(4.dp)
                                            ) {
                                                Text(
                                                    text = filter,
                                                    style = MaterialTheme.typography.labelSmall,
                                                    fontWeight = if (isSel) FontWeight.Bold else FontWeight.Normal,
                                                    color = if (isSel) MaterialTheme.colorScheme.onPrimaryContainer else MaterialTheme.colorScheme.onSurfaceVariant
                                                )
                                                if (count > 0) {
                                                    Box(
                                                        modifier = Modifier
                                                            .background(Color(0xFFE53935), shape = CircleShape)
                                                            .padding(horizontal = 6.dp, vertical = 2.dp),
                                                        contentAlignment = Alignment.Center
                                                    ) {
                                                        Text(
                                                            text = count.toString(),
                                                            style = MaterialTheme.typography.labelSmall.copy(fontSize = 10.sp, fontWeight = FontWeight.Bold),
                                                            color = Color.White
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
                } else if (currentRoute == "more") {
                    Card(
                        modifier = Modifier
                            .fillMaxWidth()
                            .animateContentSize(),
                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.95f)),
                        shape = RoundedCornerShape(topStart = 24.dp, topEnd = 24.dp, bottomStart = 0.dp, bottomEnd = 0.dp),
                        elevation = CardDefaults.cardElevation(defaultElevation = 8.dp)
                    ) {
                        Column(
                            modifier = Modifier
                                .fillMaxWidth()
                                .navigationBarsPadding()
                                .padding(vertical = 12.dp)
                        ) {
                            androidx.compose.material3.ScrollableTabRow(
                                selectedTabIndex = com.example.ui.screens.AppDataStore.moreSelectedTabIndex,
                                edgePadding = 12.dp,
                                containerColor = Color.Transparent,
                                contentColor = MaterialTheme.colorScheme.primary,
                                modifier = Modifier.fillMaxWidth()
                            ) {
                                val moreTabs = listOf(
                                    Pair("Profil", Icons.Filled.Person),
                                    Pair("Görünüm", Icons.Filled.GridView),
                                    Pair("Finans", Icons.Filled.AccountBalance),
                                    Pair("Sistem", Icons.Filled.Settings),
                                    Pair("Onay Merkezi", Icons.Filled.VerifiedUser),
                                    Pair("Tanımlamalar", Icons.Filled.List)
                                )
                                moreTabs.forEachIndexed { index, (title, icon) ->
                                    androidx.compose.material3.Tab(
                                        selected = com.example.ui.screens.AppDataStore.moreSelectedTabIndex == index,
                                        onClick = { com.example.ui.screens.AppDataStore.moreSelectedTabIndex = index },
                                        text = {
                                            Text(
                                                text = title,
                                                style = MaterialTheme.typography.bodySmall,
                                                fontWeight = if (com.example.ui.screens.AppDataStore.moreSelectedTabIndex == index) FontWeight.Bold else FontWeight.Normal
                                            )
                                        },
                                        icon = { Icon(icon, contentDescription = title, modifier = Modifier.size(18.dp)) }
                                    )
                                }
                            }
                        }
                    }
                } else if (currentRoute == "wms_warehouse") {
                    Card(
                        modifier = Modifier
                            .fillMaxWidth()
                            .animateContentSize(),
                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.95f)),
                        shape = RoundedCornerShape(topStart = 24.dp, topEnd = 24.dp, bottomStart = 0.dp, bottomEnd = 0.dp),
                        elevation = CardDefaults.cardElevation(defaultElevation = 8.dp)
                    ) {
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .navigationBarsPadding()
                                .height(72.dp)
                                .padding(horizontal = 8.dp),
                            horizontalArrangement = Arrangement.SpaceAround,
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            val selectedTab = com.example.ui.screens.AppDataStore.wmsSelectedTab.value

                            // Tab 0: Toplama
                            Column(
                                modifier = Modifier
                                    .weight(1f)
                                    .clip(RoundedCornerShape(16.dp))
                                    .clickable {
                                        com.example.ui.screens.AppDataStore.wmsSelectedTab.value = 0
                                    }
                                    .padding(vertical = 6.dp),
                                horizontalAlignment = Alignment.CenterHorizontally,
                                verticalArrangement = Arrangement.Center
                            ) {
                                Icon(
                                    imageVector = Icons.Filled.MoveToInbox,
                                    contentDescription = "Sipariş Toplama",
                                    tint = if (selectedTab == 0) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.7f),
                                    modifier = Modifier.size(24.dp)
                                )
                                Spacer(modifier = Modifier.height(2.dp))
                                Text(
                                    text = "Toplama",
                                    style = MaterialTheme.typography.labelSmall.copy(
                                        fontWeight = if (selectedTab == 0) FontWeight.Bold else FontWeight.Normal,
                                        fontSize = 11.sp
                                    ),
                                    color = if (selectedTab == 0) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.7f)
                                )
                            }

                            // Tab 1: Kontrol
                            Column(
                                modifier = Modifier
                                    .weight(1f)
                                    .clip(RoundedCornerShape(16.dp))
                                    .clickable {
                                        com.example.ui.screens.AppDataStore.wmsSelectedTab.value = 1
                                    }
                                    .padding(vertical = 6.dp),
                                horizontalAlignment = Alignment.CenterHorizontally,
                                verticalArrangement = Arrangement.Center
                            ) {
                                Icon(
                                    imageVector = Icons.Filled.FactCheck,
                                    contentDescription = "Kontrol",
                                    tint = if (selectedTab == 1) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.7f),
                                    modifier = Modifier.size(24.dp)
                                )
                                Spacer(modifier = Modifier.height(2.dp))
                                Text(
                                    text = "Kontrol",
                                    style = MaterialTheme.typography.labelSmall.copy(
                                        fontWeight = if (selectedTab == 1) FontWeight.Bold else FontWeight.Normal,
                                        fontSize = 11.sp
                                    ),
                                    color = if (selectedTab == 1) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.7f)
                                )
                            }

                            // Barkod Okuyucu Action
                            Column(
                                modifier = Modifier
                                    .weight(1f)
                                    .clip(RoundedCornerShape(16.dp))
                                    .clickable {
                                        com.example.ui.screens.AppDataStore.wmsShowScannerDialog.value = true
                                    }
                                    .padding(vertical = 6.dp),
                                horizontalAlignment = Alignment.CenterHorizontally,
                                verticalArrangement = Arrangement.Center
                            ) {
                                Icon(
                                    imageVector = Icons.Filled.QrCodeScanner,
                                    contentDescription = "Barkod Tarat",
                                    tint = MaterialTheme.colorScheme.primary,
                                    modifier = Modifier.size(28.dp)
                                )
                                Spacer(modifier = Modifier.height(2.dp))
                                Text(
                                    text = "Barkod",
                                    style = MaterialTheme.typography.labelSmall.copy(
                                        fontWeight = FontWeight.Bold,
                                        fontSize = 11.sp
                                    ),
                                    color = MaterialTheme.colorScheme.primary
                                )
                            }

                            // Tab 2: Yükleme
                            Column(
                                modifier = Modifier
                                    .weight(1f)
                                    .clip(RoundedCornerShape(16.dp))
                                    .clickable {
                                        com.example.ui.screens.AppDataStore.wmsSelectedTab.value = 2
                                    }
                                    .padding(vertical = 6.dp),
                                horizontalAlignment = Alignment.CenterHorizontally,
                                verticalArrangement = Arrangement.Center
                            ) {
                                Icon(
                                    imageVector = Icons.Filled.LocalShipping,
                                    contentDescription = "Yükleme",
                                    tint = if (selectedTab == 2) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.7f),
                                    modifier = Modifier.size(24.dp)
                                )
                                Spacer(modifier = Modifier.height(2.dp))
                                Text(
                                    text = "Yükleme",
                                    style = MaterialTheme.typography.labelSmall.copy(
                                        fontWeight = if (selectedTab == 2) FontWeight.Bold else FontWeight.Normal,
                                        fontSize = 11.sp
                                    ),
                                    color = if (selectedTab == 2) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.7f)
                                )
                            }
                        }
                    }
                } else {
                    // Full width modern standard bottom bar with rounded top corners only
                    Card(
                        modifier = Modifier
                            .fillMaxWidth(),
                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.95f)),
                        shape = RoundedCornerShape(topStart = 24.dp, topEnd = 24.dp, bottomStart = 0.dp, bottomEnd = 0.dp),
                        elevation = CardDefaults.cardElevation(defaultElevation = 8.dp)
                    ) {
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .navigationBarsPadding()
                                .height(72.dp)
                                .padding(horizontal = 8.dp),
                            horizontalArrangement = Arrangement.SpaceAround,
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            val activeTabs = com.example.ui.screens.AppDataStore.bottomBarTabs.take(4)
                            val screenMap = mapOf(
                                "dashboard" to Screen("dashboard", "Giriş", Icons.Filled.Dashboard),
                                "customers" to Screen("customers", "Cari", Icons.Filled.PersonSearch),
                                "catalog" to Screen("catalog", "Katalog", Icons.Filled.Inventory),
                                "reports" to Screen("reports", "Rapor", Icons.Filled.Analytics),
                                "more" to Screen("more", "Ayarlar", Icons.Filled.MoreHoriz),
                                "sales" to Screen("sales", "Satış", Icons.Filled.ShoppingCart),
                                "suspended_sales" to Screen("suspended_sales", "Bekleyen", Icons.Filled.HourglassEmpty),
                                "operations/purchase" to Screen("operations/purchase", "Alış", Icons.Filled.Inventory2),
                                "operations/returns" to Screen("operations/returns", "İade", Icons.Filled.KeyboardReturn),
                                "operations/collection" to Screen("operations/collection", "Tahsilat", Icons.Filled.Payments),
                                "operations/disbursement" to Screen("operations/disbursement", "Ödeme", Icons.Filled.AccountBalanceWallet),
                                "operations/cashbox" to Screen("operations/cashbox", "Kasa", Icons.Filled.AccountBalance),
                                "operations/eod" to Screen("operations/eod", "Gün Sonu", Icons.Filled.CheckCircle),
                                "operations/stocks" to Screen("operations/stocks", "Stoklar", Icons.Filled.Layers),
                                "operations/counting" to Screen("operations/counting", "Sayım", Icons.Filled.QrCodeScanner),
                                "operations/warehouses" to Screen("operations/warehouses", "Depolar", Icons.Filled.Warehouse),
                                "operations/approvals" to Screen("operations/approvals", "Onay", Icons.Filled.AssignmentTurnedIn)
                            )

                            // First 2 configurable tabs
                            for (i in 0..1) {
                                val routeStr = activeTabs.getOrNull(i) ?: "dashboard"
                                val screen = screenMap[routeStr] ?: screenMap["dashboard"]!!
                                val isSelected = currentRoute == screen.route

                                Column(
                                    modifier = Modifier
                                        .weight(1f)
                                        .clip(RoundedCornerShape(16.dp))
                                        .clickable {
                                            navController.navigate(screen.route) {
                                                popUpTo(navController.graph.findStartDestination().id) {
                                                    saveState = true
                                                }
                                                launchSingleTop = true
                                                restoreState = true
                                            }
                                        }
                                        .padding(vertical = 6.dp),
                                    horizontalAlignment = Alignment.CenterHorizontally,
                                    verticalArrangement = Arrangement.Center
                                ) {
                                    Icon(
                                        imageVector = screen.icon,
                                        contentDescription = screen.title,
                                        tint = if (isSelected) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.6f),
                                        modifier = Modifier.size(24.dp)
                                    )
                                    Spacer(modifier = Modifier.height(4.dp))
                                    Text(
                                        text = screen.title,
                                        style = MaterialTheme.typography.labelSmall,
                                        color = if (isSelected) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.6f),
                                        fontWeight = if (isSelected) FontWeight.Bold else FontWeight.Normal
                                    )
                                }
                            }

                            // Middle Settings Hub
                            Box(
                                modifier = Modifier
                                    .padding(horizontal = 4.dp)
                                    .size(54.dp)
                                    .shadow(elevation = 6.dp, shape = CircleShape)
                                    .background(MaterialTheme.colorScheme.primary, CircleShape)
                                    .clickable {
                                        com.example.ui.screens.AppDataStore.globalShowBarcodeScanner = true
                                    },
                                contentAlignment = Alignment.Center
                            ) {
                                Icon(
                                    imageVector = Icons.Filled.QrCodeScanner,
                                    contentDescription = "Barkod Oku",
                                    tint = MaterialTheme.colorScheme.onPrimary,
                                    modifier = Modifier.size(28.dp)
                                )
                            }

                            // Last 2 configurable tabs
                            for (i in 2..3) {
                                val routeStr = activeTabs.getOrNull(i) ?: "catalog"
                                val screen = screenMap[routeStr] ?: screenMap["catalog"]!!
                                val isSelected = currentRoute == screen.route

                                Column(
                                    modifier = Modifier
                                        .weight(1f)
                                        .clip(RoundedCornerShape(16.dp))
                                        .clickable {
                                            navController.navigate(screen.route) {
                                                popUpTo(navController.graph.findStartDestination().id) {
                                                    saveState = true
                                                }
                                                launchSingleTop = true
                                                restoreState = true
                                            }
                                        }
                                        .padding(vertical = 6.dp),
                                    horizontalAlignment = Alignment.CenterHorizontally,
                                    verticalArrangement = Arrangement.Center
                                ) {
                                    Icon(
                                        imageVector = screen.icon,
                                        contentDescription = screen.title,
                                        tint = if (isSelected) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.6f),
                                        modifier = Modifier.size(24.dp)
                                    )
                                    Spacer(modifier = Modifier.height(4.dp))
                                    Text(
                                        text = screen.title,
                                        style = MaterialTheme.typography.labelSmall,
                                        color = if (isSelected) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.6f),
                                        fontWeight = if (isSelected) FontWeight.Bold else FontWeight.Normal
                                    )
                                }
                            }
                        }
                    }
                }
            }
        }
    ) { innerPadding ->
        val navBackStackEntry by navController.currentBackStackEntryAsState()
        val currentRoute = navBackStackEntry?.destination?.route
        val moduleArg = navBackStackEntry?.arguments?.getString("module")
        val isCustomModuleBar = currentRoute?.startsWith("operations") == true && (moduleArg == "collection" || moduleArg == "disbursement" || moduleArg == "counting")

        val adjustedPadding = if (isCustomModuleBar) {
            PaddingValues(
                top = innerPadding.calculateTopPadding(),
                bottom = 0.dp,
                start = innerPadding.calculateStartPadding(androidx.compose.ui.unit.LayoutDirection.Ltr),
                end = innerPadding.calculateEndPadding(androidx.compose.ui.unit.LayoutDirection.Ltr)
            )
        } else if (currentRoute == "sales") {
            PaddingValues(
                top = 0.dp,
                bottom = 0.dp,
                start = innerPadding.calculateStartPadding(androidx.compose.ui.unit.LayoutDirection.Ltr),
                end = innerPadding.calculateEndPadding(androidx.compose.ui.unit.LayoutDirection.Ltr)
            )
        } else if (currentRoute == "catalog") {
            PaddingValues(
                top = 0.dp,
                bottom = innerPadding.calculateBottomPadding(),
                start = innerPadding.calculateStartPadding(androidx.compose.ui.unit.LayoutDirection.Ltr),
                end = innerPadding.calculateEndPadding(androidx.compose.ui.unit.LayoutDirection.Ltr)
            )
        } else {
            innerPadding
        }

        Box(modifier = Modifier.padding(adjustedPadding).fillMaxSize()) {
            NavHost(navController = navController, startDestination = "splash") {
                composable(Screen.Dashboard.route) { DashboardScreen(navController) }
                composable(Screen.Customers.route) { CustomersScreen(navController) }
                composable(Screen.Catalog.route) { CatalogScreen(navController) }
                composable(Screen.Reports.route) { ReportsScreen(navController) }
                composable(Screen.More.route) { MoreScreen(navController) }
                composable("wms_warehouse") { WarehouseScreen(navController) }
                
                // New routes based on instructions
                composable("splash") { SplashScreen(navController) }
                composable("license") { LicenseScreen(navController) }
                composable("login") { LoginScreen(navController) }
                composable("sales") { SalesScreen(navController) }
                composable("suspended_sales") { SuspendedSalesScreen(navController) }
                composable("offline_sync") { OfflineSyncScreen(navController) }
                composable("security") { SecurityScreen(navController) }
                composable("erp_integration") { ErpIntegrationScreen(navController) }
                composable("import_export") { ImportExportScreen(navController) }
                composable("operations/{module}") { backStackEntry ->
                    val module = backStackEntry.arguments?.getString("module") ?: "purchase"
                    OperationsScreen(module, navController)
                }
                composable("stock_detail/{barcode}") { backStackEntry ->
                    val barcode = backStackEntry.arguments?.getString("barcode")
                    StockDetailScreen(barcode, navController)
                }
            }
        }
    }
}

open class Screen(val route: String, val title: String, val icon: androidx.compose.ui.graphics.vector.ImageVector) {
    object Dashboard : Screen("dashboard", "Giriş", Icons.Filled.Dashboard)
    object Customers : Screen("customers", "Cari", Icons.Filled.PersonSearch)
    object Catalog : Screen("catalog", "Katalog", Icons.Filled.Inventory)
    object Reports : Screen("reports", "Rapor", Icons.Filled.Analytics)
    object More : Screen("more", "Ayarlar", Icons.Filled.MoreHoriz)
}

@Composable
fun ConnectionStatusDialog(
    onDismiss: () -> Unit,
    isOnline: Boolean,
    onToggleOnline: (Boolean) -> Unit,
    lastSyncTime: String,
    onManualSync: () -> Unit,
    isSyncing: Boolean
) {
    androidx.compose.ui.window.Dialog(onDismissRequest = onDismiss) {
        Card(
            modifier = Modifier
                .fillMaxWidth()
                .padding(16.dp),
            shape = RoundedCornerShape(16.dp),
            colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
            elevation = CardDefaults.cardElevation(defaultElevation = 6.dp)
        ) {
            Column(
                modifier = Modifier
                    .padding(24.dp)
                    .fillMaxWidth(),
                horizontalAlignment = Alignment.CenterHorizontally,
                verticalArrangement = Arrangement.spacedBy(16.dp)
            ) {
                // Header Icon
                Box(
                    modifier = Modifier
                        .size(56.dp)
                        .background(
                            if (isOnline) Color(0xFFE8F5E9) else Color(0xFFFFEBEE),
                            shape = CircleShape
                        ),
                    contentAlignment = Alignment.Center
                ) {
                    Icon(
                        imageVector = if (isOnline) Icons.Filled.Wifi else Icons.Filled.WifiOff,
                        contentDescription = null,
                        tint = if (isOnline) Color(0xFF43A047) else Color(0xFFD32F2F),
                        modifier = Modifier.size(28.dp)
                    )
                }

                // Title
                Text(
                    text = "Bağlantı Durumu",
                    style = MaterialTheme.typography.titleMedium,
                    fontWeight = FontWeight.Bold,
                    color = MaterialTheme.colorScheme.onSurface
                )

                androidx.compose.material3.HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)

                // Online/Offline switch
                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(vertical = 4.dp),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Column(modifier = Modifier.weight(1f)) {
                        Text(
                            text = if (isOnline) "Çevrimiçi Mod" else "Çevrimdışı (Offline)",
                            style = MaterialTheme.typography.bodyLarge,
                            fontWeight = FontWeight.SemiBold
                        )
                        Text(
                            text = if (isOnline) "Uygulama sunucuya bağlı." else "İnternet bağlantısı kesildi.",
                            style = MaterialTheme.typography.bodySmall,
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                    }
                    androidx.compose.material3.Switch(
                        checked = isOnline,
                        onCheckedChange = onToggleOnline,
                        modifier = Modifier.testTag("offline_toggle_switch")
                    )
                }

                androidx.compose.material3.HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)

                // Last sync time
                Column(
                    modifier = Modifier.fillMaxWidth(),
                    verticalArrangement = Arrangement.spacedBy(4.dp)
                ) {
                    Text(
                        text = "Son Senkronizasyon",
                        style = MaterialTheme.typography.bodySmall,
                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                        fontWeight = FontWeight.SemiBold
                    )
                    Row(
                        verticalAlignment = Alignment.CenterVertically,
                        horizontalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        Icon(
                            imageVector = Icons.Filled.History,
                            contentDescription = null,
                            tint = MaterialTheme.colorScheme.primary,
                            modifier = Modifier.size(16.dp)
                        )
                        Text(
                            text = lastSyncTime,
                            style = MaterialTheme.typography.bodyMedium,
                            fontWeight = FontWeight.Bold,
                            color = MaterialTheme.colorScheme.onSurface
                        )
                    }
                }

                Spacer(modifier = Modifier.height(4.dp))

                // Actions: Manual Sync & Close
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    androidx.compose.material3.OutlinedButton(
                        onClick = onDismiss,
                        modifier = Modifier.weight(1f),
                        contentPadding = androidx.compose.foundation.layout.PaddingValues(horizontal = 4.dp)
                    ) {
                        Text(
                            text = "Kapat",
                            maxLines = 1,
                            style = MaterialTheme.typography.labelLarge
                        )
                    }

                    Button(
                        onClick = onManualSync,
                        enabled = isOnline && !isSyncing,
                        modifier = Modifier
                            .weight(1f)
                            .testTag("manual_sync_button"),
                        contentPadding = androidx.compose.foundation.layout.PaddingValues(horizontal = 4.dp)
                    ) {
                        if (isSyncing) {
                            androidx.compose.material3.CircularProgressIndicator(
                                modifier = Modifier.size(18.dp),
                                color = MaterialTheme.colorScheme.onPrimary,
                                strokeWidth = 2.dp
                            )
                        } else {
                            Icon(
                                imageVector = Icons.Filled.Sync,
                                contentDescription = null,
                                modifier = Modifier.size(16.dp)
                            )
                            Spacer(modifier = Modifier.width(4.dp))
                            Text(
                                text = "Güncelle",
                                maxLines = 1,
                                style = MaterialTheme.typography.labelLarge,
                                fontSize = 12.sp
                            )
                        }
                    }
                }
            }
        }
    }
}

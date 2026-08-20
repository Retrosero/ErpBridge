package com.example.ui.screens

import androidx.compose.animation.*
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.grid.GridCells
import androidx.compose.foundation.lazy.grid.LazyVerticalGrid
import androidx.compose.foundation.lazy.grid.items
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material.icons.outlined.CheckCircle
import androidx.compose.material.icons.outlined.CloudOff
import androidx.compose.material.icons.outlined.CloudSync
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.draw.shadow
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.navigation.NavController
import com.example.ui.components.FieldCard
import androidx.compose.ui.zIndex
import androidx.compose.ui.input.pointer.pointerInput
import androidx.compose.foundation.gestures.detectDragGesturesAfterLongPress
import kotlin.math.roundToInt
import com.example.util.AppUpdateManager

@Composable
fun DashboardScreen(navController: NavController) {
    val scrollState = rememberScrollState()

    var showBarcodeScanner by remember { mutableStateOf(false) }
    var scannedProductBarcode by remember { mutableStateOf<String?>(null) }

    if (showBarcodeScanner) {
        BarcodeScannerDialog(
            onDismissRequest = { showBarcodeScanner = false },
            onBarcodeScanned = { code ->
                showBarcodeScanner = false
                scannedProductBarcode = code
            },
            onSimulateScan = { simulatedBarcode ->
                showBarcodeScanner = false
                scannedProductBarcode = simulatedBarcode
            }
        )
    }

    if (scannedProductBarcode != null) {
        val product = AppDataStore.products.find { it.barcode == scannedProductBarcode || it.barcodes.contains(scannedProductBarcode) || it.code == scannedProductBarcode }
        AlertDialog(
            onDismissRequest = { scannedProductBarcode = null },
            title = {
                Text("Ürün Bilgisi", fontWeight = FontWeight.Bold)
            },
            text = {
                if (product != null) {
                    Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                        Text(product.title, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.titleMedium)
                        Text("Kategori: ${product.category}", style = MaterialTheme.typography.bodySmall)
                        Text("Fiyat: ₺${product.basePrice}", style = MaterialTheme.typography.bodyMedium, color = MaterialTheme.colorScheme.primary)
                        val totalStock = product.stockByWarehouse.values.sum()
                        Text("Toplam Stok: $totalStock AD", style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.SemiBold)
                    }
                } else {
                    Text("Bu barkoda ait ürün bulunamadı: $scannedProductBarcode")
                }
            },
            confirmButton = {
                TextButton(onClick = { scannedProductBarcode = null }) {
                    Text("Tamam")
                }
            }
        )
    }

    Column(
        modifier = Modifier
            .fillMaxSize()
            .background(MaterialTheme.colorScheme.background)
    ) {
        BoxWithConstraints(modifier = Modifier.fillMaxSize()) {
            val maxWidth = this.maxWidth
            // Scrollable content underneath the edge-to-edge navbar
            Column(
                modifier = Modifier
                    .fillMaxSize()
                    .verticalScroll(scrollState)
                    .padding(16.dp),
                verticalArrangement = Arrangement.spacedBy(16.dp)
            ) {
                val context = androidx.compose.ui.platform.LocalContext.current

                // --- IN-APP UPDATE NOTIFICATION BANNER ---
                if (AppUpdateManager.isUpdateAvailable) {
                    Card(
                        modifier = Modifier.fillMaxWidth(),
                        colors = CardDefaults.cardColors(
                            containerColor = MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.9f)
                        ),
                        shape = RoundedCornerShape(16.dp),
                        elevation = CardDefaults.cardElevation(defaultElevation = 2.dp)
                    ) {
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(12.dp),
                            verticalAlignment = Alignment.CenterVertically,
                            horizontalArrangement = Arrangement.spacedBy(12.dp)
                        ) {
                            Box(
                                modifier = Modifier
                                    .size(40.dp)
                                    .background(MaterialTheme.colorScheme.primary, CircleShape),
                                contentAlignment = Alignment.Center
                            ) {
                                Icon(
                                    imageVector = Icons.Filled.SystemUpdate,
                                    contentDescription = "Güncelleme",
                                    tint = MaterialTheme.colorScheme.onPrimary,
                                    modifier = Modifier.size(22.dp)
                                )
                            }
                            Column(modifier = Modifier.weight(1f)) {
                                Text(
                                    text = "Yeni Güncelleme Mevcut! (v${AppUpdateManager.latestVersionName})",
                                    style = MaterialTheme.typography.titleSmall,
                                    fontWeight = FontWeight.Bold,
                                    color = MaterialTheme.colorScheme.onPrimaryContainer
                                )
                                Text(
                                    text = "Performans ve güvenlik iyileştirmeleri için Google Play'den güncelleyin.",
                                    style = MaterialTheme.typography.bodySmall,
                                    fontSize = 11.sp,
                                    color = MaterialTheme.colorScheme.onPrimaryContainer.copy(alpha = 0.8f)
                                )
                            }
                            Button(
                                onClick = { AppUpdateManager.openGooglePlayStore(context) },
                                shape = RoundedCornerShape(10.dp),
                                contentPadding = PaddingValues(horizontal = 12.dp, vertical = 6.dp),
                                colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.primary)
                            ) {
                                Text(
                                    text = "Güncelle",
                                    style = MaterialTheme.typography.labelSmall,
                                    fontWeight = FontWeight.Bold
                                )
                            }
                        }
                    }
                }

                // --- DYNAMIC SUMMARY STATS ---
                val activeKpis = AppDataStore.activeKpiList
                if (activeKpis.isNotEmpty()) {
                    val kpiList = remember(activeKpis) {
                        mutableStateListOf<String>().apply {
                            addAll(activeKpis)
                        }
                    }

                    var draggedKpi by remember { mutableStateOf<String?>(null) }
                    var kpiDragOffset by remember { mutableStateOf(androidx.compose.ui.geometry.Offset.Zero) }

                    val kpiDensity = androidx.compose.ui.platform.LocalDensity.current
                    val kpiNumColumns = if (maxWidth >= 900.dp) 4 else if (maxWidth >= 600.dp) 3 else 2
                    val kpiWidthPx = with(kpiDensity) { (maxWidth / kpiNumColumns).toPx() }
                    val kpiHeightPx = with(kpiDensity) { 100.dp.toPx() }

                    val kpiChunks = kpiList.chunked(kpiNumColumns)
                    Column(
                        modifier = Modifier.fillMaxWidth(),
                        verticalArrangement = Arrangement.spacedBy(10.dp)
                    ) {
                        kpiChunks.forEach { chunk ->
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.spacedBy(10.dp)
                            ) {
                                chunk.forEach { key ->
                                    val isDragged = draggedKpi == key
                                    val translateOffset = if (isDragged) {
                                        Modifier.offset {
                                            androidx.compose.ui.unit.IntOffset(
                                                kpiDragOffset.x.roundToInt(),
                                                kpiDragOffset.y.roundToInt()
                                            )
                                        }
                                    } else {
                                        Modifier
                                    }

                                    Box(
                                        modifier = Modifier
                                            .weight(1f)
                                            .then(translateOffset)
                                            .zIndex(if (isDragged) 10f else 1f)
                                            .pointerInput(key) {
                                                detectDragGesturesAfterLongPress(
                                                    onDragStart = {
                                                        draggedKpi = key
                                                        kpiDragOffset = androidx.compose.ui.geometry.Offset.Zero
                                                    },
                                                    onDrag = { change, dragAmount ->
                                                        change.consume()
                                                        kpiDragOffset += dragAmount

                                                        val activeIndex = kpiList.indexOf(draggedKpi)
                                                        if (activeIndex != -1) {
                                                            val colDiff = (kpiDragOffset.x / kpiWidthPx).roundToInt()
                                                            val rowDiff = (kpiDragOffset.y / kpiHeightPx).roundToInt()
                                                            if (colDiff != 0 || rowDiff != 0) {
                                                                val currentCol = activeIndex % kpiNumColumns
                                                                val currentRow = activeIndex / kpiNumColumns
                                                                val targetCol = (currentCol + colDiff).coerceIn(0, kpiNumColumns - 1)
                                                                val targetRow = (currentRow + rowDiff).coerceAtLeast(0)
                                                                val targetIndex = targetRow * kpiNumColumns + targetCol
                                                                if (targetIndex in kpiList.indices && targetIndex != activeIndex) {
                                                                    val item = kpiList.removeAt(activeIndex)
                                                                    kpiList.add(targetIndex, item)

                                                                    kpiDragOffset = androidx.compose.ui.geometry.Offset(
                                                                        kpiDragOffset.x - (targetCol - currentCol) * kpiWidthPx,
                                                                        kpiDragOffset.y - (targetRow - currentRow) * kpiHeightPx
                                                                    )
                                                                }
                                                            }
                                                        }
                                                    },
                                                    onDragEnd = {
                                                        draggedKpi = null
                                                        kpiDragOffset = androidx.compose.ui.geometry.Offset.Zero
                                                        AppDataStore.setActiveKpiListSetting(context, kpiList.toList())
                                                    },
                                                    onDragCancel = {
                                                        draggedKpi = null
                                                        kpiDragOffset = androidx.compose.ui.geometry.Offset.Zero
                                                    }
                                                )
                                            }
                                    ) {
                                        KpiCard(key = key)
                                    }
                                }
                                if (chunk.size < kpiNumColumns) {
                                    for (i in 0 until (kpiNumColumns - chunk.size)) {
                                        Spacer(modifier = Modifier.weight(1f))
                                    }
                                }
                            }
                        }
                    }
                }

                // --- SECTION HEADER ---
                Text(
                    text = "Modüller",
                    style = MaterialTheme.typography.titleMedium,
                    fontWeight = FontWeight.Bold,
                    color = MaterialTheme.colorScheme.onSurface,
                    modifier = Modifier.padding(top = 8.dp)
                )

                // --- MODERN BRIGHT GRID (DYNAMIC FILTERED & ROUTED 4-COLUMN) ---
                val primaryContainer = MaterialTheme.colorScheme.primaryContainer
                val primary = MaterialTheme.colorScheme.primary
                val tertiaryContainer = MaterialTheme.colorScheme.tertiaryContainer
                val tertiary = MaterialTheme.colorScheme.tertiary
                val secondaryContainer = MaterialTheme.colorScheme.secondaryContainer
                val onSecondaryContainer = MaterialTheme.colorScheme.onSecondaryContainer
                val errorContainer = MaterialTheme.colorScheme.errorContainer
                val error = MaterialTheme.colorScheme.error
                val surfaceVariant = MaterialTheme.colorScheme.surfaceVariant
                val onSurfaceVariant = MaterialTheme.colorScheme.onSurfaceVariant
                val secondary = MaterialTheme.colorScheme.secondary

                val allActionItems = remember(primaryContainer, primary, tertiaryContainer, tertiary, secondaryContainer, onSecondaryContainer, errorContainer, error, surfaceVariant, onSurfaceVariant, secondary) {
                    listOf(
                        QuickAction("Satış", Icons.Filled.ShoppingCart, primaryContainer, primary, "sales"),
                        QuickAction("Bekleyen", Icons.Filled.HourglassEmpty, primaryContainer, primary, "suspended_sales"),
                        QuickAction("Alış", Icons.Filled.Inventory2, primaryContainer, primary, "operations/purchase"),
                        QuickAction("İade", Icons.Filled.KeyboardReturn, primaryContainer, primary, "operations/returns"),
                        QuickAction("Tahsilat", Icons.Filled.Payments, primaryContainer, primary, "operations/collection"),
                        QuickAction("Ödeme", Icons.Filled.AccountBalanceWallet, primaryContainer, primary, "operations/disbursement"),
                        QuickAction("Kasa", Icons.Filled.AccountBalance, primaryContainer, primary, "operations/cashbox"),
                        QuickAction("Gün Sonu", Icons.Filled.CheckCircle, primaryContainer, primary, "operations/eod"),
                        QuickAction("Müşteriler", Icons.Filled.Group, primaryContainer, primary, "customers"),
                        QuickAction("Raporlar", Icons.Filled.Assessment, primaryContainer, primary, "reports"),
                        QuickAction("Stoklar", Icons.Filled.Layers, primaryContainer, primary, "operations/stocks"),
                        QuickAction("Sayım", Icons.Filled.QrCodeScanner, primaryContainer, primary, "operations/counting"),
                        QuickAction("Depolar", Icons.Filled.Warehouse, primaryContainer, primary, "operations/warehouses"),
                        QuickAction("Depo (WMS)", Icons.Filled.MoveToInbox, primaryContainer, primary, "wms_warehouse"),
                        QuickAction("Katalog", Icons.Filled.MenuBook, primaryContainer, primary, "catalog"),
                        QuickAction("Onay Merkezi", Icons.Filled.AssignmentTurnedIn, primaryContainer, primary, "operations/approvals"),
                        QuickAction("Giderler", Icons.Filled.ReceiptLong, primaryContainer, primary, "operations/expenses"),
                        QuickAction("Araçlar", Icons.Filled.DirectionsCar, primaryContainer, primary, "operations/vehicles")
                    )
                }

                // Sort items dynamically based on quickActionsOrder
                val sortedAllActions = remember(AppDataStore.quickActionsOrder) {
                    val orderMap = AppDataStore.quickActionsOrder.withIndex().associate { it.value to it.index }
                    allActionItems.sortedBy { action ->
                        orderMap[action.route] ?: 999
                    }
                }

                // Filter based on visibleModules setting (excluding "more" since it's now permanently in the center bottom bar)
                val actions = sortedAllActions.filter { it.route != "more" && AppDataStore.visibleModules.contains(it.route) }

                // Live state tracking during drag gestures to swap positions in UI dynamically
                val actionsList = remember(actions) {
                    mutableStateListOf<QuickAction>().apply {
                        addAll(actions)
                    }
                }

                var draggedRoute by remember { mutableStateOf<String?>(null) }
                var dragOffset by remember { mutableStateOf(androidx.compose.ui.geometry.Offset.Zero) }

                val density = androidx.compose.ui.platform.LocalDensity.current
                val numColumns = if (maxWidth >= 900.dp) 7 else if (maxWidth >= 600.dp) 6 else 4
                val itemSizePx = with(density) { (maxWidth / numColumns).toPx() } // estimate cell width for correct swap grid bounds

                val rows = actionsList.chunked(numColumns)
                Column(
                    modifier = Modifier.fillMaxWidth(),
                    verticalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    rows.forEach { rowItems ->
                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.spacedBy(8.dp)
                        ) {
                            rowItems.forEach { action ->
                                val isDragged = draggedRoute == action.route
                                val translateOffset = if (isDragged) {
                                    Modifier.offset {
                                        androidx.compose.ui.unit.IntOffset(
                                            dragOffset.x.roundToInt(),
                                            dragOffset.y.roundToInt()
                                        )
                                    }
                                } else {
                                    Modifier
                                }

                                Box(
                                    modifier = Modifier
                                        .weight(1f)
                                        .then(translateOffset)
                                        .zIndex(if (isDragged) 10f else 1f)
                                        .pointerInput(action.route) {
                                            detectDragGesturesAfterLongPress(
                                                onDragStart = {
                                                    draggedRoute = action.route
                                                    dragOffset = androidx.compose.ui.geometry.Offset.Zero
                                                },
                                                onDrag = { change, dragAmount ->
                                                    change.consume()
                                                    dragOffset += dragAmount

                                                    val activeIndex = actionsList.indexOfFirst { it.route == draggedRoute }
                                                    if (activeIndex != -1) {
                                                        val colDiff = (dragOffset.x / itemSizePx).roundToInt()
                                                        val rowDiff = (dragOffset.y / itemSizePx).roundToInt()
                                                        if (colDiff != 0 || rowDiff != 0) {
                                                            val currentCol = activeIndex % numColumns
                                                            val currentRow = activeIndex / numColumns
                                                            val targetCol = (currentCol + colDiff).coerceIn(0, numColumns - 1)
                                                            val targetRow = (currentRow + rowDiff).coerceAtLeast(0)
                                                            val targetIndex = targetRow * numColumns + targetCol
                                                            if (targetIndex in actionsList.indices && targetIndex != activeIndex) {
                                                                val item = actionsList.removeAt(activeIndex)
                                                                actionsList.add(targetIndex, item)

                                                                dragOffset = androidx.compose.ui.geometry.Offset(
                                                                    dragOffset.x - (targetCol - currentCol) * itemSizePx,
                                                                    dragOffset.y - (targetRow - currentRow) * itemSizePx
                                                                )
                                                            }
                                                        }
                                                    }
                                                },
                                                onDragEnd = {
                                                    draggedRoute = null
                                                    dragOffset = androidx.compose.ui.geometry.Offset.Zero
                                                    // Persist the custom drag order
                                                    val newOrder = actionsList.mapNotNull { it.route }
                                                    AppDataStore.setQuickActionsOrderSetting(context, newOrder)
                                                },
                                                onDragCancel = {
                                                    draggedRoute = null
                                                    dragOffset = androidx.compose.ui.geometry.Offset.Zero
                                                }
                                            )
                                        }
                                ) {
                                    QuickActionItem(action = action, onClick = {
                                        if (draggedRoute == null) {
                                            action.route?.let { navController.navigate(it) }
                                        }
                                    })
                                }
                            }
                            if (rowItems.size < numColumns) {
                                for (i in 0 until (numColumns - rowItems.size)) {
                                    Spacer(modifier = Modifier.weight(1f))
                                }
                            }
                        }
                    }
                }
            }
        }

        Spacer(modifier = Modifier.height(16.dp))
    }
}

data class QuickAction(
    val title: String,
    val icon: ImageVector,
    val containerColor: Color,
    val iconColor: Color,
    val route: String? = null
)

@Composable
fun QuickActionItem(action: QuickAction, onClick: () -> Unit) {
    // Determine dynamic badge count
    val badgeCount = when (action.route) {
        "operations/approvals" -> com.example.ui.screens.AppDataStore.approvalItems.size
        "suspended_sales" -> com.example.ui.screens.AppDataStore.suspendedSales.size
        else -> 0
    }

    Card(
        modifier = Modifier
            .fillMaxWidth()
            .border(
                width = 1.dp,
                color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.4f),
                shape = RoundedCornerShape(16.dp)
            )
            .clickable(onClick = onClick)
            .testTag("quick_action_${action.title.lowercase()}"),
        shape = RoundedCornerShape(16.dp),
        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
        elevation = CardDefaults.cardElevation(defaultElevation = 1.dp)
    ) {
        Column(
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.Center,
            modifier = Modifier
                .fillMaxWidth()
                .padding(vertical = 12.dp, horizontal = 4.dp)
        ) {
            Box(
                contentAlignment = Alignment.Center
            ) {
                Box(
                    modifier = Modifier
                        .size(38.dp)
                        .background(MaterialTheme.colorScheme.primary.copy(alpha = 0.08f), shape = CircleShape),
                    contentAlignment = Alignment.Center
                ) {
                    Icon(
                        action.icon,
                        contentDescription = action.title,
                        tint = MaterialTheme.colorScheme.primary,
                        modifier = Modifier.size(20.dp)
                    )
                }

                if (badgeCount > 0) {
                    Box(
                        modifier = Modifier
                            .align(Alignment.TopEnd)
                            .offset(x = 8.dp, y = (-4).dp)
                            .background(Color(0xFFE53935), shape = CircleShape)
                            .padding(horizontal = 6.dp, vertical = 2.dp),
                        contentAlignment = Alignment.Center
                    ) {
                        Text(
                            text = badgeCount.toString(),
                            style = MaterialTheme.typography.labelSmall.copy(
                                fontSize = 9.sp,
                                fontWeight = FontWeight.Bold
                            ),
                            color = Color.White
                        )
                    }
                }
            }
            Spacer(modifier = Modifier.height(8.dp))
            Text(
                text = action.title,
                style = MaterialTheme.typography.labelSmall,
                fontWeight = FontWeight.SemiBold,
                color = MaterialTheme.colorScheme.onSurface,
                textAlign = TextAlign.Center,
                maxLines = 1,
                overflow = androidx.compose.ui.text.style.TextOverflow.Ellipsis
            )
        }
    }
}

@Composable
fun KpiCard(key: String) {
    Card(
        modifier = Modifier
            .fillMaxWidth()
            .shadow(elevation = 2.dp, shape = RoundedCornerShape(16.dp)),
        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
        shape = RoundedCornerShape(16.dp)
    ) {
        Column(modifier = Modifier.padding(12.dp)) {
            when (key) {
                "ciro" -> {
                    Row(
                        verticalAlignment = Alignment.CenterVertically,
                        horizontalArrangement = Arrangement.spacedBy(6.dp)
                    ) {
                        Box(
                            modifier = Modifier
                                .size(28.dp)
                                .background(MaterialTheme.colorScheme.primaryContainer, shape = CircleShape),
                            contentAlignment = Alignment.Center
                        ) {
                            Icon(
                                Icons.Filled.Payments,
                                contentDescription = null,
                                tint = MaterialTheme.colorScheme.primary,
                                modifier = Modifier.size(14.dp)
                            )
                        }
                        Text(
                            text = "Günlük Ciro",
                            style = MaterialTheme.typography.labelMedium,
                            color = MaterialTheme.colorScheme.onSurfaceVariant,
                            fontWeight = FontWeight.Bold,
                            maxLines = 1
                        )
                    }
                    Spacer(modifier = Modifier.height(8.dp))
                    Text(
                        text = "₺54.230,50",
                        style = MaterialTheme.typography.titleMedium,
                        fontWeight = FontWeight.Black,
                        color = MaterialTheme.colorScheme.onSurface
                    )
                    Row(
                        verticalAlignment = Alignment.CenterVertically,
                        horizontalArrangement = Arrangement.spacedBy(2.dp),
                        modifier = Modifier.padding(top = 2.dp)
                    ) {
                        Icon(
                            Icons.Filled.TrendingUp,
                            contentDescription = null,
                            tint = MaterialTheme.colorScheme.tertiary,
                            modifier = Modifier.size(12.dp)
                        )
                        Text(
                            text = "Bugün +12.5%",
                            style = MaterialTheme.typography.labelSmall,
                            color = MaterialTheme.colorScheme.tertiary,
                            fontWeight = FontWeight.Bold
                        )
                    }
                }
                "ziyaret" -> {
                    Row(
                        verticalAlignment = Alignment.CenterVertically,
                        horizontalArrangement = Arrangement.spacedBy(6.dp)
                    ) {
                        Box(
                            modifier = Modifier
                                .size(28.dp)
                                .background(MaterialTheme.colorScheme.secondaryContainer, shape = CircleShape),
                            contentAlignment = Alignment.Center
                        ) {
                            Icon(
                                Icons.Filled.PersonSearch,
                                contentDescription = null,
                                tint = MaterialTheme.colorScheme.secondary,
                                modifier = Modifier.size(14.dp)
                            )
                        }
                        Text(
                            text = "Ziyaret/Hedef",
                            style = MaterialTheme.typography.labelMedium,
                            color = MaterialTheme.colorScheme.onSurfaceVariant,
                            fontWeight = FontWeight.Bold,
                            maxLines = 1
                        )
                    }
                    Spacer(modifier = Modifier.height(8.dp))
                    Row(verticalAlignment = Alignment.Bottom) {
                        Text(
                            text = "14",
                            style = MaterialTheme.typography.titleMedium,
                            fontWeight = FontWeight.Black,
                            color = MaterialTheme.colorScheme.onSurface
                        )
                        Text(
                            text = " / 20",
                            style = MaterialTheme.typography.labelSmall,
                            color = MaterialTheme.colorScheme.onSurfaceVariant,
                            modifier = Modifier.padding(start = 2.dp, bottom = 1.dp)
                        )
                    }
                    Spacer(modifier = Modifier.height(6.dp))
                    Box(
                        modifier = Modifier
                            .fillMaxWidth()
                            .height(4.dp)
                            .background(
                                MaterialTheme.colorScheme.surfaceVariant,
                                shape = CircleShape
                            )
                    ) {
                        Box(
                            modifier = Modifier
                                .fillMaxHeight()
                                .fillMaxWidth(0.7f)
                                .background(MaterialTheme.colorScheme.secondary, shape = CircleShape)
                        )
                    }
                }
                "bekleyen_satis" -> {
                    Row(
                        verticalAlignment = Alignment.CenterVertically,
                        horizontalArrangement = Arrangement.spacedBy(6.dp)
                    ) {
                        Box(
                            modifier = Modifier
                                .size(28.dp)
                                .background(MaterialTheme.colorScheme.tertiaryContainer, shape = CircleShape),
                            contentAlignment = Alignment.Center
                        ) {
                            Icon(
                                Icons.Filled.HourglassEmpty,
                                contentDescription = null,
                                tint = MaterialTheme.colorScheme.tertiary,
                                modifier = Modifier.size(14.dp)
                            )
                        }
                        Text(
                            text = "Bekleyen Satış",
                            style = MaterialTheme.typography.labelMedium,
                            color = MaterialTheme.colorScheme.onSurfaceVariant,
                            fontWeight = FontWeight.Bold,
                            maxLines = 1
                        )
                    }
                    Spacer(modifier = Modifier.height(8.dp))
                    val suspendedCount = AppDataStore.suspendedSales.size
                    val suspendedTotal = AppDataStore.suspendedSales.sumOf { it.totalAmount }
                    val displayCount = if (suspendedCount > 0) suspendedCount else 6
                    val displayTotal = if (suspendedTotal > 0.0) suspendedTotal else 12450.00
                    Text(
                        text = "₺%,.2f".format(displayTotal).replace(",", "."),
                        style = MaterialTheme.typography.titleMedium,
                        fontWeight = FontWeight.Black,
                        color = MaterialTheme.colorScheme.onSurface
                    )
                    Text(
                        text = "$displayCount Bekleyen Fiş",
                        style = MaterialTheme.typography.labelSmall,
                        color = Color.Gray,
                        modifier = Modifier.padding(top = 2.dp)
                    )
                }
                "tahsilat" -> {
                    Row(
                        verticalAlignment = Alignment.CenterVertically,
                        horizontalArrangement = Arrangement.spacedBy(6.dp)
                    ) {
                        Box(
                            modifier = Modifier
                                .size(28.dp)
                                .background(Color(0xFFE8F5E9), shape = CircleShape),
                            contentAlignment = Alignment.Center
                        ) {
                            Icon(
                                Icons.Filled.Payments,
                                contentDescription = null,
                                tint = Color(0xFF43A047),
                                modifier = Modifier.size(14.dp)
                            )
                        }
                        Text(
                            text = "Günlük Tahsilat",
                            style = MaterialTheme.typography.labelMedium,
                            color = MaterialTheme.colorScheme.onSurfaceVariant,
                            fontWeight = FontWeight.Bold,
                            maxLines = 1
                        )
                    }
                    Spacer(modifier = Modifier.height(8.dp))
                    val logsTotal = AppDataStore.kasaLogs.filter { it.type == "Tahsilat" || it.type == "Tahsilat (Nakit)" || it.type == "Tahsilat (Banka)" }.sumOf { it.amount }
                    val displayTotal = if (logsTotal > 0.0) logsTotal else 28750.00
                    Text(
                        text = "₺%,.2f".format(displayTotal).replace(",", "."),
                        style = MaterialTheme.typography.titleMedium,
                        fontWeight = FontWeight.Black,
                        color = MaterialTheme.colorScheme.onSurface
                    )
                    Text(
                        text = "İşlenen Tahsilatlar",
                        style = MaterialTheme.typography.labelSmall,
                        color = Color.Gray,
                        modifier = Modifier.padding(top = 2.dp)
                    )
                }
                "onay_bekleyen" -> {
                    val pendingCount = com.example.ui.screens.AppDataStore.approvalItems.size
                    Row(
                        verticalAlignment = Alignment.CenterVertically,
                        horizontalArrangement = Arrangement.spacedBy(6.dp)
                    ) {
                        Box(
                            modifier = Modifier
                                .size(28.dp)
                                .background(Color(0xFFFFF3E0), shape = CircleShape),
                            contentAlignment = Alignment.Center
                        ) {
                            Icon(
                                Icons.Filled.Schedule,
                                contentDescription = null,
                                tint = Color(0xFFF57C00),
                                modifier = Modifier.size(14.dp)
                            )
                        }
                        Text(
                            text = "Onay Bekleyen",
                            style = MaterialTheme.typography.labelMedium,
                            color = MaterialTheme.colorScheme.onSurfaceVariant,
                            fontWeight = FontWeight.Bold,
                            maxLines = 1
                        )
                    }
                    Spacer(modifier = Modifier.height(8.dp))
                    Text(
                        text = if (pendingCount > 0) "$pendingCount İşlem" else "Bekleyen Yok",
                        style = MaterialTheme.typography.titleMedium,
                        fontWeight = FontWeight.Black,
                        color = MaterialTheme.colorScheme.onSurface
                    )
                    Text(
                        text = if (pendingCount > 0) {
                            com.example.ui.screens.AppDataStore.approvalItems.groupBy { it.type }
                                .map { "${it.value.size} ${it.key}" }
                                .joinToString(", ")
                        } else {
                            "Tüm işlemler temiz"
                        },
                        style = MaterialTheme.typography.labelSmall,
                        color = Color.Gray,
                        modifier = Modifier.padding(top = 2.dp),
                        maxLines = 1,
                        overflow = androidx.compose.ui.text.style.TextOverflow.Ellipsis
                    )
                }
            }
        }
    }
}

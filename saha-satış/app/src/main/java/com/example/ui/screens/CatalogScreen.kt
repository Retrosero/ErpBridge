package com.example.ui.screens

import android.media.AudioManager
import android.media.ToneGenerator
import android.os.Build
import android.os.VibrationEffect
import android.os.Vibrator
import android.os.VibratorManager
import androidx.compose.animation.*
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.LazyRow
import androidx.compose.foundation.lazy.grid.GridCells
import androidx.compose.foundation.lazy.grid.LazyVerticalGrid
import androidx.compose.foundation.lazy.grid.items
import androidx.compose.foundation.lazy.grid.itemsIndexed
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.lazy.itemsIndexed
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.foundation.pager.HorizontalPager
import androidx.compose.foundation.pager.rememberPagerState
import androidx.compose.ui.platform.testTag
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.compose.ui.window.Dialog
import androidx.navigation.NavController
import com.example.ui.components.FieldCard
import com.example.ui.components.FieldHeader
import com.example.ui.components.FieldPrimaryButton
import com.example.ui.components.FieldSecondaryButton
import com.example.ui.components.AdvancedFilterDialog
import androidx.compose.foundation.horizontalScroll
import kotlinx.coroutines.delay
import kotlinx.coroutines.launch

// --- INTERACTIVE SYSTEM MODELS ---

data class Warehouse(
    val id: String,
    val name: String,
    val activeStaff: Int,
    val totalSKU: Int
)

data class CountItem(
    val barcode: String,
    val name: String,
    val expectedQty: Int,
    var countedQty: Int
)

data class StockTransfer(
    val id: String,
    val date: String,
    val origin: String,
    val dest: String,
    val itemTitle: String,
    val qty: Int,
    val state: String // "Tamamlandı" veya "Bekliyor"
)

@Composable
fun getCategoryIcon(categoryName: String): androidx.compose.ui.graphics.vector.ImageVector {
    return when (categoryName) {
        "Tümü" -> Icons.Filled.Category
        "Endüstriyel Yağlar" -> Icons.Filled.WaterDrop
        "Filtre Grupları" -> Icons.Filled.FilterAlt
        "Yedek Parça" -> Icons.Filled.SettingsSuggest
        "Sarf Malzemeler" -> Icons.Filled.Construction
        else -> Icons.Filled.Folder
    }
}

@Composable
fun CategoryItem(
    name: String,
    icon: androidx.compose.ui.graphics.vector.ImageVector,
    isSelected: Boolean,
    isLandscape: Boolean,
    onClick: () -> Unit
) {
    Surface(
        onClick = onClick,
        modifier = Modifier
            .then(
                if (isLandscape) {
                    Modifier
                        .fillMaxWidth()
                        .padding(vertical = 4.dp, horizontal = 8.dp)
                } else {
                    Modifier.padding(horizontal = 4.dp, vertical = 6.dp)
                }
            )
            .testTag("category_item_${name.lowercase()}"),
        shape = RoundedCornerShape(16.dp),
        color = if (isSelected) MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f),
        tonalElevation = if (isSelected) 4.dp else 0.dp,
        border = if (isSelected) androidx.compose.foundation.BorderStroke(1.5.dp, MaterialTheme.colorScheme.primary) else null
    ) {
        if (isLandscape) {
            Row(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(horizontal = 12.dp, vertical = 12.dp),
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.spacedBy(8.dp)
            ) {
                Icon(
                    imageVector = icon,
                    contentDescription = name,
                    tint = if (isSelected) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant,
                    modifier = Modifier.size(20.dp)
                )
                Text(
                    text = name,
                    style = MaterialTheme.typography.bodySmall,
                    fontWeight = if (isSelected) FontWeight.Bold else FontWeight.Medium,
                    color = if (isSelected) MaterialTheme.colorScheme.onPrimaryContainer else MaterialTheme.colorScheme.onSurfaceVariant,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis
                )
            }
        } else {
            Row(
                modifier = Modifier
                    .height(44.dp)
                    .padding(horizontal = 12.dp),
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.spacedBy(6.dp)
            ) {
                Icon(
                    imageVector = icon,
                    contentDescription = name,
                    tint = if (isSelected) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant,
                    modifier = Modifier.size(18.dp)
                )
                Text(
                    text = name,
                    style = MaterialTheme.typography.bodySmall,
                    fontWeight = if (isSelected) FontWeight.Bold else FontWeight.Medium,
                    color = if (isSelected) MaterialTheme.colorScheme.onPrimaryContainer else MaterialTheme.colorScheme.onSurfaceVariant,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis
                )
            }
        }
    }
}

data class PendingCartAdd(
    val title: String,
    val existingQty: Int,
    val additionQty: Int,
    val totalQty: Int,
    val onConfirm: () -> Unit
)

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun CatalogScreen(navController: NavController) {
    val context = LocalContext.current
    val scope = rememberCoroutineScope()
    val snackbarHostState = remember { SnackbarHostState() }

    var pendingCartAddInfo by remember { mutableStateOf<PendingCartAdd?>(null) }

    // --- 1. MOCK SEED DATABASE ---
    val initialProducts = AppDataStore.products.toList()

    val warehouses = remember {
        listOf(
            Warehouse("WH-01", "Ana Depo", 8, 4),
            Warehouse("WH-02", "Ankara Merkez", 3, 3),
            Warehouse("WH-03", "Ege Bölge", 2, 3)
        )
    }

    // Transfers list state
    val transfersList = remember {
        mutableStateListOf(
            StockTransfer("TR-501", "08.06.2026", "Ana Depo", "Ankara Merkez", "Hava Filtresi - Ağır Vasıta Pro", 25, "Tamamlandı"),
            StockTransfer("TR-502", "08.06.2026", "Ana Depo", "Ege Bölge", "Ultra Performans Motor Yağı 20L", 10, "Bekliyor")
        )
    }

    // --- 2. CONTROLLING TABS & SCREEN MODES ---
    // Toggle: 0 -> Katalog & Stoklar, 1 -> Sayım Modülü (Barkod), 2 -> Depo & Sevk
    var activeSubModuleTab by AppDataStore.catalogActiveTab

    // --- NAVIGATION FILTER STATES (TAB 0) ---
    var catalogSearchQuery by AppDataStore.catalogSearchQuery
    var selectedCategoryFilter by AppDataStore.catalogSelectedCategory
    var catalogSortOrder by AppDataStore.catalogSelectedSortOrder
    var catalogShowBarcodeScanner by AppDataStore.catalogShowBarcodeScanner
    val selectedCustomer by AppDataStore.activeSelectedCustomer
    val selectedPriceTier = selectedCustomer?.priceGroup ?: "Perakende"
    var showCustomerPickerDialog by remember { mutableStateOf(false) }
    var selectedStockDetailProduct by remember { mutableStateOf<ProductCatalog?>(null) }

    var showCatalogQtyDialog by remember { mutableStateOf(false) }
    var catalogQtyDialogProduct by remember { mutableStateOf<ProductCatalog?>(null) }
    var catalogQtyDialogAmountStr by remember { mutableStateOf("1") }
    var largeImageViewerProduct by remember { mutableStateOf<ProductCatalog?>(null) }

    // --- SOUND & VIBE FEEDBACKS ---
    fun playFeedbackSuccess(isOk: Boolean) {
        com.example.util.VibratorHelper.triggerFeedback(context, isOk)
    }

    // --- TAB 1: SAYIM MODÜLÜ STATE (Billion-Dollar Stock Count Module) ---
    var countingWarehouse by remember { mutableStateOf("Ana Depo") }
    val sessionCountingItems = remember {
        mutableStateListOf(
            CountItem("8690123456789", "Ultra Performans Motor Yağı 20L", 145, 0),
            CountItem("8699876543210", "Hava Filtresi - Ağır Vasıta Pro", 89, 0),
            CountItem("1234567890123", "Çelik Rulman 120mm - Yüksek Devir", 0, 0),
            CountItem("8681122334455", "Çelik Civata Takımı M8 (100 ADET)", 320, 0)
        )
    }
    var manualBarcodeEntryField by remember { mutableStateOf("") }
    var flashLightEnabled by remember { mutableStateOf(false) }

    // Manual Barcode processor
    fun processStockCountScan(barcodeStr: String) {
        val trimmed = barcodeStr.trim()
        if (trimmed.isEmpty()) return

        val item = sessionCountingItems.find { it.barcode == trimmed }
        if (item != null) {
            val idx = sessionCountingItems.indexOf(item)
            sessionCountingItems[idx] = item.copy(countedQty = item.countedQty + 1)
            playFeedbackSuccess(true)
            scope.launch {
                snackbarHostState.showSnackbar("Okutuldu: ${item.name} (${sessionCountingItems[idx].countedQty} adet)")
            }
        } else {
            // Check in catalog if a product exists with this barcode but not in counting list
            val globalProd = initialProducts.find { it.barcode == trimmed }
            if (globalProd != null) {
                sessionCountingItems.add(CountItem(trimmed, globalProd.title, 0, 1))
                playFeedbackSuccess(true)
            } else {
                playFeedbackSuccess(false)
                scope.launch {
                    snackbarHostState.showSnackbar("Bilinmeyen ürün barkodu: $trimmed! Sayıma eklenemedi.", "Yeni Ekle")
                }
            }
        }
        manualBarcodeEntryField = ""
    }

    // --- TAB 2: DEPO TRANSFER FORM STATE ---
    var showTransferDialog by remember { mutableStateOf(false) }
    var showAddProductDialog by AppDataStore.catalogShowAddProductDialog
    var showCatalogCartDialog by AppDataStore.catalogShowCartDialog

    if (catalogShowBarcodeScanner) {
        BarcodeScannerDialog(
            onDismissRequest = { catalogShowBarcodeScanner = false },
            onBarcodeScanned = { code ->
                catalogShowBarcodeScanner = false
                catalogSearchQuery = code
                playFeedbackSuccess(true)
            },
            onSimulateScan = { simulatedBarcode ->
                catalogShowBarcodeScanner = false
                catalogSearchQuery = simulatedBarcode
                playFeedbackSuccess(true)
            }
        )
    }

    Scaffold(
        snackbarHost = { SnackbarHost(snackbarHostState) },
        containerColor = MaterialTheme.colorScheme.background,
        contentWindowInsets = WindowInsets(0, 0, 0, 0)
    ) { paddingValues ->
        Box(modifier = Modifier.fillMaxSize()) {
            Column(
                modifier = Modifier
                    .fillMaxSize()
                    .statusBarsPadding()
                    .padding(paddingValues)
            ) {
            AnimatedContent(
                targetState = activeSubModuleTab,
                transitionSpec = {
                    fadeIn() togetherWith fadeOut()
                },
                modifier = Modifier.weight(1f)
            ) { targetTab ->
                when (targetTab) {
                    0 -> {
                        // --- MODULE A: INDUSTRIAL VISUAL INDUSTRIAL CATALOG & STOCKS ---
                        val configuration = androidx.compose.ui.platform.LocalConfiguration.current
                        val isLandscape = configuration.orientation == android.content.res.Configuration.ORIENTATION_LANDSCAPE
                        val categoriesList = remember(initialProducts) {
                            listOf("Tümü") + initialProducts.map { it.category }.distinct().sorted()
                        }

                        val filteredCollection = initialProducts.filter { prod ->
                            val matchesSearch = catalogSearchQuery.isEmpty() ||
                                    prod.title.contains(catalogSearchQuery, ignoreCase = true) ||
                                    prod.code.contains(catalogSearchQuery, ignoreCase = true) ||
                                    prod.barcode == catalogSearchQuery
                            
                            val matchesCategory = if (AppDataStore.catalogFilterCategories.value.isNotEmpty()) {
                                AppDataStore.catalogFilterCategories.value.contains(prod.category)
                            } else {
                                selectedCategoryFilter == "Tümü" || prod.category == selectedCategoryFilter
                            }
                            
                            val matchesBrand = AppDataStore.catalogFilterBrands.value.isEmpty() || 
                                               AppDataStore.catalogFilterBrands.value.contains(prod.brand ?: "Belirtilmemiş")
                            
                            val matchesAmbalaj = AppDataStore.catalogFilterAmbalajs.value.isEmpty() || 
                                                 AppDataStore.catalogFilterAmbalajs.value.contains(prod.inferredAmbalaj)
                            
                            val price = prod.getPriceForGroup(selectedPriceTier)
                            val minPriceVal = AppDataStore.catalogFilterMinPrice.value.toDoubleOrNull()
                            val maxPriceVal = AppDataStore.catalogFilterMaxPrice.value.toDoubleOrNull()
                            val matchesPrice = (minPriceVal == null || price >= minPriceVal) && 
                                               (maxPriceVal == null || price <= maxPriceVal)
                            
                            val totalStock = prod.stockByWarehouse.values.sum()
                            val minStockVal = AppDataStore.catalogFilterMinStock.value.toIntOrNull()
                            val maxStockVal = AppDataStore.catalogFilterMaxStock.value.toIntOrNull()
                            val matchesStock = (minStockVal == null || totalStock >= minStockVal) && 
                                               (maxStockVal == null || totalStock <= maxStockVal)
                            
                            val hasPhoto = !prod.imageUrl.isNullOrEmpty() || !prod.localImagePath.isNullOrEmpty()
                            val matchesNoPhoto = !AppDataStore.catalogFilterHideNoPhoto.value || hasPhoto
                            val matchesOutOfStock = !AppDataStore.catalogFilterHideOutOfStock.value || totalStock > 0
                            
                            matchesSearch && matchesCategory && matchesBrand && matchesAmbalaj && matchesPrice && matchesStock && matchesNoPhoto && matchesOutOfStock
                        }.sortedWith(
                            run {
                                val comparator = when (AppDataStore.catalogSelectedSortField.value) {
                                    "İsim" -> compareBy<ProductCatalog> { it.title }
                                    "Kod" -> compareBy { it.code }
                                    "Fiyat" -> compareBy { it.getPriceForGroup(selectedPriceTier) }
                                    "Marka" -> compareBy { it.brand ?: "" }
                                    "Stok" -> compareBy { it.stockByWarehouse.values.sum() }
                                    else -> compareBy { it.title }
                                }
                                if (AppDataStore.catalogSelectedSortAsc.value) comparator else comparator.reversed()
                            }
                        )

                        var visibleItemCount by remember(
                            catalogSearchQuery, selectedCategoryFilter,
                            AppDataStore.catalogSelectedSortField.value, AppDataStore.catalogSelectedSortAsc.value,
                            AppDataStore.catalogFilterBrands.value, AppDataStore.catalogFilterCategories.value, AppDataStore.catalogFilterAmbalajs.value,
                            AppDataStore.catalogFilterMinPrice.value, AppDataStore.catalogFilterMaxPrice.value,
                            AppDataStore.catalogFilterMinStock.value, AppDataStore.catalogFilterMaxStock.value,
                            AppDataStore.catalogFilterHideNoPhoto.value, AppDataStore.catalogFilterHideOutOfStock.value
                        ) { mutableStateOf(100) }
                        val displayedProducts = filteredCollection.take(visibleItemCount)

                        if (isLandscape) {
                            Row(modifier = Modifier.fillMaxSize()) {
                                // Left scrollable categories column (always visible on far left in landscape)
                                Column(
                                    modifier = Modifier
                                        .width(180.dp)
                                        .fillMaxHeight()
                                        .background(MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.25f))
                                        .verticalScroll(rememberScrollState())
                                        .padding(vertical = 8.dp),
                                    verticalArrangement = Arrangement.spacedBy(4.dp),
                                    horizontalAlignment = Alignment.CenterHorizontally
                                ) {
                                    categoriesList.forEach { categoryName ->
                                        CategoryItem(
                                            name = categoryName,
                                            icon = getCategoryIcon(categoryName),
                                            isSelected = selectedCategoryFilter == categoryName,
                                            isLandscape = true,
                                            onClick = { selectedCategoryFilter = categoryName }
                                        )
                                    }
                                }

                                // Right main content area
                                Column(
                                    modifier = Modifier
                                        .weight(1f)
                                        .fillMaxHeight()
                                        .padding(start = 8.dp, end = 8.dp, top = 4.dp, bottom = 4.dp),
                                    verticalArrangement = Arrangement.spacedBy(8.dp)
                                ) {
                                    CatalogFilterAndSortBar(selectedPriceTier)
                                    Card(
                                        modifier = Modifier.fillMaxWidth(),
                                        colors = CardDefaults.cardColors(
                                            containerColor = MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.3f)
                                        ),
                                        shape = RoundedCornerShape(12.dp)
                                    ) {
                                        Row(
                                            modifier = Modifier.fillMaxWidth().padding(horizontal = 12.dp, vertical = 4.dp),
                                            verticalAlignment = Alignment.CenterVertically,
                                            horizontalArrangement = Arrangement.SpaceBetween
                                        ) {
                                            Row(verticalAlignment = Alignment.CenterVertically, modifier = Modifier.weight(1f)) {
                                                Icon(Icons.Filled.People, null, tint = MaterialTheme.colorScheme.primary, modifier = Modifier.size(16.dp))
                                                Spacer(modifier = Modifier.width(6.dp))
                                                if (selectedCustomer != null) {
                                                    Row(
                                                        verticalAlignment = Alignment.CenterVertically,
                                                        horizontalArrangement = Arrangement.spacedBy(4.dp),
                                                        modifier = Modifier.fillMaxWidth()
                                                    ) {
                                                        Text(
                                                            text = selectedCustomer!!.name,
                                                            style = MaterialTheme.typography.bodySmall,
                                                            fontWeight = FontWeight.Bold,
                                                            maxLines = 1,
                                                            overflow = TextOverflow.Ellipsis,
                                                            modifier = Modifier.weight(1f, fill = false)
                                                        )
                                                        Text(
                                                            text = "(${String.format("%.2f ₺", selectedCustomer!!.balance)})",
                                                            style = MaterialTheme.typography.bodySmall.copy(fontSize = 11.sp),
                                                            color = MaterialTheme.colorScheme.secondary,
                                                            maxLines = 1,
                                                            modifier = Modifier.padding(start = 2.dp)
                                                        )
                                                    }
                                                } else {
                                                    Text(
                                                        text = "Müşteri Seçilmedi ⚠️",
                                                        style = MaterialTheme.typography.bodySmall,
                                                        fontWeight = FontWeight.Bold,
                                                        maxLines = 1,
                                                        overflow = TextOverflow.Ellipsis
                                                    )
                                                }
                                            }
                                            TextButton(
                                                onClick = { showCustomerPickerDialog = true },
                                                contentPadding = PaddingValues(horizontal = 8.dp, vertical = 2.dp),
                                                modifier = Modifier.height(28.dp)
                                            ) {
                                                Text(if (selectedCustomer == null) "Seç" else "Değiştir", style = MaterialTheme.typography.labelMedium, fontWeight = FontWeight.Bold)
                                            }
                                        }
                                    }
                                    if (filteredCollection.isEmpty()) {
                                        Box(
                                            modifier = Modifier.fillMaxSize(),
                                            contentAlignment = Alignment.Center
                                        ) {
                                            Text("Eşleşen katalog ürünü bulunamadı.", style = MaterialTheme.typography.bodyLarge, color = MaterialTheme.colorScheme.outline)
                                        }
                                    } else {
                                        val isGridView = AppDataStore.catalogSelectedViewMode.value == "Grid"
                                        if (isGridView) {
                                            LazyVerticalGrid(
                                                columns = GridCells.Adaptive(minSize = 150.dp),
                                                modifier = Modifier.fillMaxSize(),
                                                horizontalArrangement = Arrangement.spacedBy(8.dp),
                                                verticalArrangement = Arrangement.spacedBy(8.dp),
                                                contentPadding = PaddingValues(bottom = 8.dp)
                                            ) {
                                                itemsIndexed(displayedProducts) { index, prod ->
                                                     if (index >= displayedProducts.size - 5 && displayedProducts.size < filteredCollection.size) {
                                                         LaunchedEffect(Unit) {
                                                             visibleItemCount += 100
                                                         }
                                                     }
                                                     InteractiveProductGridCard(
                                                         product = prod,
                                                         priceTier = selectedPriceTier,
                                                         onSelectStock = { navController.navigate("stock_detail/${prod.barcode}") },
                                                         onAddToCart = {
                                                              catalogQtyDialogProduct = prod
                                                              catalogQtyDialogAmountStr = "1"
                                                              showCatalogQtyDialog = true
                                                          },
                                                         onImageClick = { largeImageViewerProduct = prod }
                                                     )
                                                }
                                            }
                                        } else {
                                            LazyColumn(
                                                modifier = Modifier.fillMaxSize(),
                                                verticalArrangement = Arrangement.spacedBy(8.dp),
                                                contentPadding = PaddingValues(bottom = 8.dp)
                                            ) {
                                                itemsIndexed(displayedProducts) { index, prod ->
                                                     if (index >= displayedProducts.size - 5 && displayedProducts.size < filteredCollection.size) {
                                                         LaunchedEffect(Unit) {
                                                             visibleItemCount += 100
                                                         }
                                                     }
                                                    InteractiveProductListCard(
                                                        product = prod,
                                                        priceTier = selectedPriceTier,
                                                        onSelectStock = { navController.navigate("stock_detail/${prod.barcode}") },
                                                        onAddToCart = {
                                                              catalogQtyDialogProduct = prod
                                                              catalogQtyDialogAmountStr = "1"
                                                              showCatalogQtyDialog = true
                                                          },
                                                         onImageClick = { largeImageViewerProduct = prod }
                                                    )
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        } else {
                            Column(modifier = Modifier.fillMaxSize()) {
                                Card(
                                    modifier = Modifier.fillMaxWidth().padding(horizontal = 8.dp, vertical = 4.dp),
                                    colors = CardDefaults.cardColors(
                                        containerColor = MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.3f)
                                    ),
                                    shape = RoundedCornerShape(12.dp)
                                ) {
                                    Row(
                                        modifier = Modifier.fillMaxWidth().padding(horizontal = 12.dp, vertical = 4.dp),
                                        verticalAlignment = Alignment.CenterVertically,
                                        horizontalArrangement = Arrangement.SpaceBetween
                                    ) {
                                        Row(verticalAlignment = Alignment.CenterVertically, modifier = Modifier.weight(1f)) {
                                            Icon(Icons.Filled.People, null, tint = MaterialTheme.colorScheme.primary, modifier = Modifier.size(16.dp))
                                            Spacer(modifier = Modifier.width(6.dp))
                                            if (selectedCustomer != null) {
                                                Row(
                                                    verticalAlignment = Alignment.CenterVertically,
                                                    horizontalArrangement = Arrangement.spacedBy(4.dp),
                                                    modifier = Modifier.fillMaxWidth()
                                                ) {
                                                    Text(
                                                        text = selectedCustomer!!.name,
                                                        style = MaterialTheme.typography.bodySmall,
                                                        fontWeight = FontWeight.Bold,
                                                        maxLines = 1,
                                                        overflow = TextOverflow.Ellipsis,
                                                        modifier = Modifier.weight(1f, fill = false)
                                                    )
                                                    Text(
                                                        text = "(${String.format("%.2f ₺", selectedCustomer!!.balance)})",
                                                        style = MaterialTheme.typography.bodySmall.copy(fontSize = 11.sp),
                                                        color = MaterialTheme.colorScheme.secondary,
                                                        maxLines = 1,
                                                        modifier = Modifier.padding(start = 2.dp)
                                                    )
                                                }
                                            } else {
                                                Text(
                                                    text = "Müşteri Seçilmedi ⚠️",
                                                    style = MaterialTheme.typography.bodySmall,
                                                    fontWeight = FontWeight.Bold,
                                                    maxLines = 1,
                                                    overflow = TextOverflow.Ellipsis
                                                )
                                            }
                                        }
                                        TextButton(
                                            onClick = { showCustomerPickerDialog = true },
                                            contentPadding = PaddingValues(horizontal = 8.dp, vertical = 2.dp),
                                            modifier = Modifier.height(28.dp)
                                        ) {
                                            Text(if (selectedCustomer == null) "Seç" else "Değiştir", style = MaterialTheme.typography.labelMedium, fontWeight = FontWeight.Bold)
                                        }
                                    }
                                }

                                // Top horizontal scrollable categories Row (Portrait mode)
                                LazyRow(
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .background(MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.25f))
                                        .padding(vertical = 4.dp, horizontal = 4.dp),
                                    horizontalArrangement = Arrangement.spacedBy(4.dp),
                                    verticalAlignment = Alignment.CenterVertically
                                ) {
                                    items(categoriesList) { categoryName ->
                                        CategoryItem(
                                            name = categoryName,
                                            icon = getCategoryIcon(categoryName),
                                            isSelected = selectedCategoryFilter == categoryName,
                                            isLandscape = false,
                                            onClick = { selectedCategoryFilter = categoryName }
                                        )
                                    }
                                }

                                Spacer(modifier = Modifier.height(4.dp))

                                // Products Area
                                Column(
                                    modifier = Modifier
                                        .weight(1f)
                                        .fillMaxWidth()
                                        .padding(start = 8.dp, end = 8.dp, bottom = 4.dp),
                                    verticalArrangement = Arrangement.spacedBy(8.dp)
                                ) {
                                    CatalogFilterAndSortBar(selectedPriceTier)
                                    if (filteredCollection.isEmpty()) {
                                        Box(
                                            modifier = Modifier.fillMaxSize(),
                                            contentAlignment = Alignment.Center
                                        ) {
                                            Text("Eşleşen katalog ürünü bulunamadı.", style = MaterialTheme.typography.bodyLarge, color = MaterialTheme.colorScheme.outline)
                                        }
                                    } else {
                                        val isGridView = AppDataStore.catalogSelectedViewMode.value == "Grid"
                                        if (isGridView) {
                                            LazyVerticalGrid(
                                                columns = GridCells.Adaptive(minSize = 150.dp),
                                                modifier = Modifier.fillMaxSize(),
                                                horizontalArrangement = Arrangement.spacedBy(8.dp),
                                                verticalArrangement = Arrangement.spacedBy(8.dp),
                                                contentPadding = PaddingValues(bottom = 8.dp)
                                            ) {
                                                itemsIndexed(displayedProducts) { index, prod ->
                                                     if (index >= displayedProducts.size - 5 && displayedProducts.size < filteredCollection.size) {
                                                         LaunchedEffect(Unit) {
                                                             visibleItemCount += 100
                                                         }
                                                     }
                                                     InteractiveProductGridCard(
                                                         product = prod,
                                                         priceTier = selectedPriceTier,
                                                         onSelectStock = { navController.navigate("stock_detail/${prod.barcode}") },
                                                         onAddToCart = {
                                                              catalogQtyDialogProduct = prod
                                                              catalogQtyDialogAmountStr = "1"
                                                              showCatalogQtyDialog = true
                                                          },
                                                         onImageClick = { largeImageViewerProduct = prod }
                                                     )
                                                }
                                            }
                                        } else {
                                            LazyColumn(
                                                modifier = Modifier.fillMaxSize(),
                                                verticalArrangement = Arrangement.spacedBy(8.dp),
                                                contentPadding = PaddingValues(bottom = 8.dp)
                                            ) {
                                                itemsIndexed(displayedProducts) { index, prod ->
                                                     if (index >= displayedProducts.size - 5 && displayedProducts.size < filteredCollection.size) {
                                                         LaunchedEffect(Unit) {
                                                             visibleItemCount += 100
                                                         }
                                                     }
                                                    InteractiveProductListCard(
                                                        product = prod,
                                                        priceTier = selectedPriceTier,
                                                        onSelectStock = { navController.navigate("stock_detail/${prod.barcode}") },
                                                        onAddToCart = {
                                                              catalogQtyDialogProduct = prod
                                                              catalogQtyDialogAmountStr = "1"
                                                              showCatalogQtyDialog = true
                                                          },
                                                         onImageClick = { largeImageViewerProduct = prod }
                                                    )
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    1 -> {
                        // --- MODULE B: DIGITAL STOCK COUNT & PHYSICAL DIFFERENCE MODULE (Sayım Modülü) ---
                        Column(
                            modifier = Modifier
                                .fillMaxSize()
                                .padding(16.dp),
                            verticalArrangement = Arrangement.spacedBy(16.dp)
                        ) {
                            // Warehouse Select
                            FieldCard {
                                Column(modifier = Modifier.padding(16.dp)) {
                                    Text("Sayım Yapılacak Depo Seçimi", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.primary)
                                    Spacer(modifier = Modifier.height(4.dp))
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween,
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                                            Icon(Icons.Filled.Warehouse, contentDescription = null, tint = MaterialTheme.colorScheme.primary)
                                            Text(countingWarehouse, style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold)
                                        }
                                        Row(horizontalArrangement = Arrangement.spacedBy(6.dp)) {
                                            val depos = AppDataStore.definitions["Depo"] ?: emptyList()
                                            depos.take(3).forEach { shortName ->
                                                val fName = shortName
                                                ElevatedFilterChip(
                                                    selected = countingWarehouse == fName,
                                                    onClick = { countingWarehouse = fName },
                                                    label = { Text(shortName) }
                                                )
                                            }
                                        }
                                    }
                                }
                            }

                            // Camera Scanner Simulator Bar
                            Surface(
                                color = MaterialTheme.colorScheme.surfaceContainerHigh,
                                shape = RoundedCornerShape(16.dp),
                                modifier = Modifier.fillMaxWidth()
                            ) {
                                Column(
                                    modifier = Modifier.padding(12.dp),
                                    verticalArrangement = Arrangement.spacedBy(8.dp),
                                    horizontalAlignment = Alignment.CenterHorizontally
                                ) {
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween,
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        Row(
                                            verticalAlignment = Alignment.CenterVertically,
                                            horizontalArrangement = Arrangement.spacedBy(8.dp)
                                        ) {
                                            Icon(Icons.Filled.Videocam, contentDescription = null, tint = MaterialTheme.colorScheme.primary)
                                            Text("Cihaz Kamerası Aktif (Sayıcı)", style = MaterialTheme.typography.bodySmall, fontWeight = FontWeight.Bold)
                                        }
                                        IconButton(onClick = { flashLightEnabled = !flashLightEnabled }) {
                                            Icon(
                                                imageVector = if (flashLightEnabled) Icons.Filled.FlashOn else Icons.Filled.FlashOff,
                                                contentDescription = "Flashlight",
                                                tint = if (flashLightEnabled) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.outline
                                            )
                                        }
                                    }

                                    // Camera scan screen mockup
                                    Box(
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .height(90.dp)
                                            .clip(RoundedCornerShape(8.dp))
                                            .background(Color.Black),
                                        contentAlignment = Alignment.Center
                                    ) {
                                        Box(
                                            modifier = Modifier
                                                .fillMaxWidth(0.8f)
                                                .height(2.dp)
                                                .background(Color.Red)
                                        )
                                        Text(
                                            "Kamerayı Barkoda Yaklaştırın",
                                            style = MaterialTheme.typography.labelSmall,
                                            color = Color.White.copy(alpha = 0.6f),
                                            modifier = Modifier.align(Alignment.BottomCenter).padding(bottom = 8.dp)
                                        )
                                    }

                                    // Raw inputs simulation
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.spacedBy(8.dp),
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        OutlinedTextField(
                                            value = manualBarcodeEntryField,
                                            onValueChange = { manualBarcodeEntryField = it },
                                            placeholder = { Text("Barkod veya manuel giriş...") },
                                            modifier = Modifier.weight(1f),
                                            singleLine = true,
                                            keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                                            colors = OutlinedTextFieldDefaults.colors(
                                                unfocusedContainerColor = MaterialTheme.colorScheme.surface
                                            )
                                        )
                                        IconButton(
                                            onClick = { processStockCountScan(manualBarcodeEntryField) },
                                            modifier = Modifier
                                                .background(MaterialTheme.colorScheme.primary, shape = RoundedCornerShape(12.dp))
                                                .size(48.dp)
                                        ) {
                                            Icon(Icons.Filled.Check, contentDescription = "Add", tint = MaterialTheme.colorScheme.onPrimary)
                                        }
                                    }
                                    
                                    // Quick simulation shortcuts
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween,
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        Text("Barkod Simulasyon:", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
                                        Row(horizontalArrangement = Arrangement.spacedBy(6.dp)) {
                                            Button(
                                                onClick = { processStockCountScan("8690123456789") },
                                                colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.surfaceVariant, contentColor = MaterialTheme.colorScheme.onSurfaceVariant),
                                                contentPadding = PaddingValues(horizontal = 6.dp, vertical = 2.dp)
                                            ) {
                                                Text("Yağ", style = MaterialTheme.typography.labelSmall)
                                            }
                                            Button(
                                                onClick = { processStockCountScan("8699876543210") },
                                                colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.surfaceVariant, contentColor = MaterialTheme.colorScheme.onSurfaceVariant),
                                                contentPadding = PaddingValues(horizontal = 6.dp, vertical = 2.dp)
                                            ) {
                                                Text("Filtre", style = MaterialTheme.typography.labelSmall)
                                            }
                                            Button(
                                                onClick = { processStockCountScan("9999999999999") }, // Non-existent
                                                colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.errorContainer, contentColor = MaterialTheme.colorScheme.onErrorContainer),
                                                contentPadding = PaddingValues(horizontal = 6.dp, vertical = 2.dp)
                                            ) {
                                                Text("Hatalı", style = MaterialTheme.typography.labelSmall)
                                            }
                                        }
                                    }
                                }
                            }

                            // Dynamic expected vs counted list header
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text("Sayılan Ürün Listesi", style = MaterialTheme.typography.titleSmall, fontWeight = FontWeight.Bold)
                                Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                                    TextButton(onClick = {
                                        sessionCountingItems.indices.forEach { idx ->
                                            sessionCountingItems[idx] = sessionCountingItems[idx].copy(countedQty = 0)
                                        }
                                    }) {
                                        Text("Sıfırla")
                                    }
                                }
                            }

                            // List counted
                            LazyColumn(
                                modifier = Modifier.weight(1f),
                                verticalArrangement = Arrangement.spacedBy(10.dp)
                            ) {
                                items(sessionCountingItems) { count ->
                                    val discrepancy = count.countedQty - count.expectedQty
                                    FieldCard {
                                        Row(
                                            modifier = Modifier
                                                .fillMaxWidth()
                                                .padding(12.dp),
                                            horizontalArrangement = Arrangement.SpaceBetween,
                                            verticalAlignment = Alignment.CenterVertically
                                        ) {
                                            Column(modifier = Modifier.weight(1f)) {
                                                Text(count.name, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold, maxLines = 1, overflow = TextOverflow.Ellipsis)
                                                Text("Barkod: ${count.barcode}", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.outline)
                                                Row(
                                                    horizontalArrangement = Arrangement.spacedBy(12.dp),
                                                    modifier = Modifier.padding(top = 4.dp)
                                                ) {
                                                    Text("Beklenen: ${count.expectedQty}", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.outline)
                                                    Text("Fark: $discrepancy", style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Bold, color = if (discrepancy < 0) MaterialTheme.colorScheme.error else if (discrepancy > 0) MaterialTheme.colorScheme.secondary else MaterialTheme.colorScheme.primary)
                                                }
                                            }

                                            Row(
                                                verticalAlignment = Alignment.CenterVertically,
                                                horizontalArrangement = Arrangement.spacedBy(8.dp)
                                            ) {
                                                IconButton(
                                                    onClick = {
                                                        if (count.countedQty > 0) {
                                                            val idx = sessionCountingItems.indexOf(count)
                                                            sessionCountingItems[idx] = count.copy(countedQty = count.countedQty - 1)
                                                        }
                                                    },
                                                    modifier = Modifier.size(24.dp)
                                                ) {
                                                    Icon(Icons.Filled.RemoveCircleOutline, contentDescription = null, tint = MaterialTheme.colorScheme.primary)
                                                }
                                                
                                                Text(count.countedQty.toString(), style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold, modifier = Modifier.width(32.dp), textAlign = TextAlign.Center)

                                                IconButton(
                                                    onClick = {
                                                        val idx = sessionCountingItems.indexOf(count)
                                                        sessionCountingItems[idx] = count.copy(countedQty = count.countedQty + 1)
                                                    },
                                                    modifier = Modifier.size(24.dp)
                                                ) {
                                                    Icon(Icons.Filled.AddCircleOutline, contentDescription = null, tint = MaterialTheme.colorScheme.primary)
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            // Commit Counts
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.spacedBy(12.dp)
                            ) {
                                // PDF / Difference Report
                                OutlinedButton(
                                    onClick = {
                                        playFeedbackSuccess(true)
                                        scope.launch {
                                            snackbarHostState.showSnackbar("Sayım Fark Raporu (PDF) oluşturuldu ve paylaşıldı.")
                                        }
                                    },
                                    modifier = Modifier.weight(1f)
                                ) {
                                    Icon(Icons.Filled.Assessment, contentDescription = null, modifier = Modifier.size(16.dp))
                                    Spacer(modifier = Modifier.width(4.dp))
                                    Text("Fark Raporu")
                                }

                                Button(
                                    onClick = {
                                        playFeedbackSuccess(true)
                                        scope.launch {
                                            snackbarHostState.showSnackbar("Fiziksel Sayım Belgesi kaydedildi. Kuyruğa eklendi.")
                                        }
                                    },
                                    modifier = Modifier.weight(1.3f)
                                ) {
                                    Icon(Icons.Filled.CloudDone, contentDescription = null, modifier = Modifier.size(16.dp))
                                    Spacer(modifier = Modifier.width(4.dp))
                                    Text("Sayımları Tamamla")
                                }
                            }
                        }
                    }

                    2 -> {
                        // --- MODULE C: DEPOLAR LİSTESİ & DEPOLAR ARASI STOK AKTARIM (Sevk) ---
                        Column(
                            modifier = Modifier
                                .fillMaxSize()
                                .padding(16.dp),
                            verticalArrangement = Arrangement.spacedBy(16.dp)
                        ) {
                            // Warehouse Listing Grid
                            Text("Saha Depoları & Cihaz Aktiflikleri", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold)

                            LazyVerticalGrid(
                                columns = GridCells.Fixed(2),
                                modifier = Modifier.height(140.dp),
                                horizontalArrangement = Arrangement.spacedBy(12.dp),
                                verticalArrangement = Arrangement.spacedBy(12.dp)
                            ) {
                                items(warehouses) { wh ->
                                    FieldCard {
                                        Column(modifier = Modifier.padding(12.dp)) {
                                            Icon(Icons.Filled.Warehouse, contentDescription = null, tint = MaterialTheme.colorScheme.primary)
                                            Spacer(modifier = Modifier.height(4.dp))
                                            Text(wh.name, style = MaterialTheme.typography.titleSmall, fontWeight = FontWeight.Bold)
                                            Text("Aktif Personel: ${wh.activeStaff}", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.outline)
                                            Text("${wh.totalSKU} Temel Stok Grubu", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
                                        }
                                    }
                                }
                            }

                            // Dynamic transfers view
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text("Saha Arası Depo Sevk Hareketleri", style = MaterialTheme.typography.titleSmall, fontWeight = FontWeight.Bold)
                                Button(
                                    onClick = { showTransferDialog = true },
                                    colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.secondaryContainer, contentColor = MaterialTheme.colorScheme.onSecondaryContainer),
                                    contentPadding = PaddingValues(horizontal = 8.dp, vertical = 4.dp)
                                ) {
                                    Icon(Icons.Filled.Add, contentDescription = null, modifier = Modifier.size(16.dp))
                                    Spacer(modifier = Modifier.width(4.dp))
                                    Text("Sevk Oluştur", style = MaterialTheme.typography.labelSmall)
                                }
                            }

                            LazyColumn(
                                modifier = Modifier.weight(1f),
                                verticalArrangement = Arrangement.spacedBy(10.dp)
                            ) {
                                items(transfersList) { trans ->
                                    FieldCard {
                                        Row(
                                            modifier = Modifier
                                                .fillMaxWidth()
                                                .padding(12.dp),
                                            horizontalArrangement = Arrangement.SpaceBetween,
                                            verticalAlignment = Alignment.CenterVertically
                                        ) {
                                            Column(modifier = Modifier.weight(1f)) {
                                                Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(6.dp)) {
                                                    Icon(Icons.Filled.Label, contentDescription = null, modifier = Modifier.size(16.dp), tint = MaterialTheme.colorScheme.secondary)
                                                    Text(trans.id, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold)
                                                    Surface(
                                                        color = if (trans.state == "Tamamlandı") MaterialTheme.colorScheme.secondaryContainer else MaterialTheme.colorScheme.errorContainer,
                                                        shape = RoundedCornerShape(4.dp)
                                                    ) {
                                                        Text(trans.state, style = MaterialTheme.typography.labelSmall, color = if (trans.state == "Tamamlandı") MaterialTheme.colorScheme.onSecondaryContainer else MaterialTheme.colorScheme.onErrorContainer, modifier = Modifier.padding(horizontal = 4.dp))
                                                    }
                                                }
                                                Text(trans.itemTitle, style = MaterialTheme.typography.bodySmall, fontWeight = FontWeight.SemiBold)
                                                Row(horizontalArrangement = Arrangement.spacedBy(12.dp), modifier = Modifier.padding(top = 4.dp)) {
                                                    Text("Çıkış: ${trans.origin}", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.outline)
                                                    Text("Giriş: ${trans.dest}", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.outline)
                                                    Text("Tarih: ${trans.date}", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.outline)
                                                }
                                            }
                                            Column(horizontalAlignment = Alignment.End) {
                                                Text("${trans.qty} ADT", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.ExtraBold, color = MaterialTheme.colorScheme.primary)
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

    // --- DIALOG A: DETAILED WAREHOUSE STOCK BIZ AREA ---
    selectedStockDetailProduct?.let { prod ->
        Dialog(onDismissRequest = { selectedStockDetailProduct = null }) {
            Box(
                modifier = Modifier
                    .fillMaxWidth()
                    .clip(RoundedCornerShape(20.dp))
                    .background(MaterialTheme.colorScheme.surface)
                    .padding(20.dp)
            ) {
                Column(verticalArrangement = Arrangement.spacedBy(16.dp)) {
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.SpaceBetween,
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Text("Depo Bazlı Stok Detayları", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
                        IconButton(onClick = { selectedStockDetailProduct = null }) {
                            Icon(Icons.Filled.Close, contentDescription = null)
                        }
                    }

                    HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)

                    // Product brief
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.spacedBy(12.dp)
                    ) {
                        Box(
                            modifier = Modifier
                                .size(48.dp)
                                .clip(RoundedCornerShape(8.dp))
                                .background(prod.imageUrlColor),
                            contentAlignment = Alignment.Center
                        ) {
                            Text(prod.code.take(3), color = Color.White, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.labelSmall)
                        }
                        Column {
                            Text(prod.title, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold)
                            Text("Barkod: ${prod.barcode}", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.outline)
                        }
                    }

                    HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)

                    // Breakdown lists
                    prod.stockByWarehouse.forEach { (wh, quantity) ->
                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.SpaceBetween,
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                                Icon(Icons.Filled.Warehouse, contentDescription = null, modifier = Modifier.size(18.dp), tint = MaterialTheme.colorScheme.outline)
                                Text(wh, style = MaterialTheme.typography.bodyMedium)
                            }
                            Text(
                                text = "$quantity Adet",
                                style = MaterialTheme.typography.bodyLarge,
                                fontWeight = FontWeight.Bold,
                                color = if (quantity > 10) MaterialTheme.colorScheme.secondary else if (quantity > 0) MaterialTheme.colorScheme.error else MaterialTheme.colorScheme.outline
                            )
                        }
                    }

                    HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)

                    Button(
                        onClick = { selectedStockDetailProduct = null },
                        modifier = Modifier.fillMaxWidth()
                    ) {
                        Text("Kapat")
                    }
                }
            }
        }
    }

    if (showAddProductDialog) {
        val currentCount = AppDataStore.products.size
        val context = LocalContext.current
        AddProductCatalogDialog(
            onDismiss = { showAddProductDialog = false },
            onSave = { newProd ->
                if (!com.example.data.LicenseManager.canAddMoreProducts(currentCount, AppDataStore.licenseKey)) {
                     showAddProductDialog = false
                     playFeedbackSuccess(false)
                     scope.launch {
                         val limit = com.example.data.LicenseManager.getProductLimit(AppDataStore.licenseKey)
                         snackbarHostState.showSnackbar("Lisans limitine ulaştınız. (En fazla $limit ürün eklenebilir)")
                     }
                } else {
                    scope.launch(kotlinx.coroutines.Dispatchers.IO) {
                        com.example.data.database.DatabaseProvider.getDatabase(context).productDao().insert(
                            com.example.data.database.ProductEntity(
                                barcode = newProd.barcode,
                                code = newProd.code,
                                title = newProd.title,
                                category = newProd.category,
                                desc = newProd.desc,
                                basePrice = newProd.basePrice,
                                dealerPrice = newProd.dealerPrice,
                                wholesalePrice = newProd.wholesalePrice,
                                kdvPercent = newProd.kdvPercent,
                                colorValue = newProd.imageUrlColor.value.toLong(),
                                brand = newProd.brand,
                                stockByWarehouseJson = "{}",
                                boxQty = newProd.boxQty,
                                packageQty = newProd.packageQty,
                                imageUrl = newProd.imageUrl,
                                localImagePath = newProd.localImagePath,
                                aisle = newProd.aisle,
                                customPricesJson = "{}",
                                barcodesJson = "[]"
                            )
                        )
                    }
                    showAddProductDialog = false
                    playFeedbackSuccess(true)
                    scope.launch {
                        snackbarHostState.showSnackbar("Yeni ürün (${newProd.title}) katalog envanterine başarıyla eklendi!")
                    }
                }
            }
        )
    }

    if (largeImageViewerProduct != null) {
        val prod = largeImageViewerProduct!!
        val images = remember(prod.imageUrl, prod.localImagePath) {
            val list = mutableListOf<Any>()
            val localFiles = mutableListOf<java.io.File>()
            if (!prod.localImagePath.isNullOrBlank()) {
                val paths = prod.localImagePath.split(Regex("[,;|\\s]+"))
                    .map { it.trim() }
                    .filter { it.isNotEmpty() }
                paths.forEach { path ->
                    val file = java.io.File(path)
                    if (file.exists() && file.length() > 0) {
                        localFiles.add(file)
                    }
                }
            }
            val remoteUrls = mutableListOf<String>()
            if (!prod.imageUrl.isNullOrBlank()) {
                val urls = prod.imageUrl.split(Regex("[,;|\\s]+"))
                    .map { it.trim() }
                    .filter { it.isNotEmpty() }
                remoteUrls.addAll(urls)
            }
            list.addAll(localFiles)
            remoteUrls.forEachIndexed { idx, url ->
                if (idx >= localFiles.size) {
                    list.add(url)
                }
            }
            list
        }
        
        Dialog(onDismissRequest = { largeImageViewerProduct = null }) {
            Card(
                modifier = Modifier
                    .fillMaxWidth()
                    .wrapContentHeight()
                    .padding(8.dp),
                shape = RoundedCornerShape(24.dp),
                colors = CardDefaults.cardColors(
                    containerColor = MaterialTheme.colorScheme.surface
                ),
                elevation = CardDefaults.cardElevation(defaultElevation = 8.dp)
            ) {
                Column(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(16.dp),
                    horizontalAlignment = Alignment.CenterHorizontally,
                    verticalArrangement = Arrangement.spacedBy(16.dp)
                ) {
                    // Header Bar
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.SpaceBetween,
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Column(modifier = Modifier.weight(1f)) {
                            Text(
                                text = prod.title,
                                fontWeight = FontWeight.Bold,
                                style = MaterialTheme.typography.titleMedium,
                                maxLines = 1,
                                overflow = TextOverflow.Ellipsis
                            )
                            Text(
                                text = "${prod.category} | ${prod.code}",
                                style = MaterialTheme.typography.labelSmall,
                                color = MaterialTheme.colorScheme.primary
                            )
                        }
                        IconButton(onClick = { largeImageViewerProduct = null }) {
                            Icon(Icons.Filled.Close, contentDescription = "Kapat")
                        }
                    }

                    // Large swipable picture frame
                    Box(
                        modifier = Modifier
                            .fillMaxWidth()
                            .height(300.dp)
                            .clip(RoundedCornerShape(16.dp))
                            .background(Color.White),
                        contentAlignment = Alignment.Center
                    ) {
                        if (images.isEmpty()) {
                            val icon = when (prod.category) {
                                "Endüstriyel Yağlar" -> Icons.Filled.WaterDrop
                                "Filtre Grupları" -> Icons.Filled.FilterAlt
                                "Yedek Parça" -> Icons.Filled.SettingsSuggest
                                else -> Icons.Filled.Construction
                            }
                            Icon(
                                imageVector = icon,
                                contentDescription = null,
                                tint = prod.imageUrlColor,
                                modifier = Modifier.size(80.dp)
                            )
                        } else {
                            val largePagerState = androidx.compose.foundation.pager.rememberPagerState(pageCount = { images.size })
                            Box(modifier = Modifier.fillMaxSize()) {
                                androidx.compose.foundation.pager.HorizontalPager(
                                    state = largePagerState,
                                    modifier = Modifier.fillMaxSize()
                                ) { page ->
                                    val imageModel = images[page]
                                    Box(
                                        modifier = Modifier.fillMaxSize(),
                                        contentAlignment = Alignment.Center
                                    ) {
                                        coil.compose.AsyncImage(
                                            model = imageModel,
                                            contentDescription = "${prod.title} - ${page + 1}",
                                            modifier = Modifier
                                                .fillMaxSize()
                                                .padding(8.dp),
                                            contentScale = androidx.compose.ui.layout.ContentScale.Fit // Fit proportionally to frame!
                                        )
                                    }
                                }

                                // Custom dot indicator overlay
                                if (images.size > 1) {
                                    Row(
                                        modifier = Modifier
                                            .align(Alignment.BottomCenter)
                                            .padding(bottom = 12.dp)
                                            .background(Color.Black.copy(alpha = 0.5f), RoundedCornerShape(12.dp))
                                            .padding(horizontal = 8.dp, vertical = 4.dp),
                                        horizontalArrangement = Arrangement.spacedBy(6.dp),
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        repeat(images.size) { index ->
                                            val active = largePagerState.currentPage == index
                                            Box(
                                                modifier = Modifier
                                                    .size(8.dp)
                                                    .background(
                                                        color = if (active) Color.White else Color.White.copy(alpha = 0.5f),
                                                        shape = CircleShape
                                                    )
                                            )
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Stock info Summary
                    Row(
                        modifier = Modifier
                            .fillMaxWidth()
                            .background(MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.3f), RoundedCornerShape(12.dp))
                            .padding(12.dp),
                        horizontalArrangement = Arrangement.SpaceBetween,
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Column {
                            val totalQty = prod.stockByWarehouse.values.sum()
                            Text("Stok Durumu", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                            Text(
                                text = "$totalQty ADET",
                                style = MaterialTheme.typography.bodyLarge,
                                fontWeight = FontWeight.Bold,
                                color = if (totalQty > 0) Color(0xFF43A047) else Color.Red
                            )
                        }
                        Column(horizontalAlignment = Alignment.End) {
                            Text("Barkod Numarası", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                            Text(
                                text = prod.barcode,
                                style = MaterialTheme.typography.bodyMedium,
                                fontWeight = FontWeight.SemiBold
                            )
                        }
                    }

                    // Nice and elegant dismiss button
                    Button(
                        onClick = { largeImageViewerProduct = null },
                        modifier = Modifier.fillMaxWidth(),
                        shape = RoundedCornerShape(12.dp)
                    ) {
                        Text("Kapat")
                    }
                }
            }
        }
    }

    if (showCatalogQtyDialog && catalogQtyDialogProduct != null) {
        val prod = catalogQtyDialogProduct!!
        Dialog(onDismissRequest = { showCatalogQtyDialog = false }) {
            Card(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(16.dp),
                shape = RoundedCornerShape(24.dp),
                colors = CardDefaults.cardColors(
                    containerColor = MaterialTheme.colorScheme.surface
                ),
                elevation = CardDefaults.cardElevation(defaultElevation = 8.dp)
            ) {
                Column(
                    modifier = Modifier
                        .padding(20.dp)
                        .fillMaxWidth(),
                    horizontalAlignment = Alignment.CenterHorizontally,
                    verticalArrangement = Arrangement.spacedBy(12.dp)
                ) {
                    Text(
                        text = "Ürün Ekleme Miktarı",
                        style = MaterialTheme.typography.titleMedium,
                        fontWeight = FontWeight.Bold,
                        color = MaterialTheme.colorScheme.onSurface
                    )
                    
                    Text(prod.title, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodyLarge, textAlign = TextAlign.Center)
                    Text("KOD: ${prod.code}", style = MaterialTheme.typography.bodySmall, color = Color.Gray)
                    
                     val displayPrice = prod.getPriceForGroup(selectedPriceTier)
                    
                    Text(
                        text = String.format("Birim Fiyat: %,.2f ₺", displayPrice),
                        style = MaterialTheme.typography.labelLarge,
                        fontWeight = FontWeight.SemiBold,
                        color = MaterialTheme.colorScheme.primary
                    )

                    val countNum = catalogQtyDialogAmountStr.toIntOrNull() ?: 1
                    val stockAvailable = prod.stockByWarehouse.values.sum()
                    val existingItem = AppDataStore.catalogCartItems.find { it.product.barcode == prod.barcode }
                    val requestedAmount = countNum + (existingItem?.quantity ?: 0)
                    val stockError = !AppDataStore.allowNegativeStock && requestedAmount > stockAvailable

                    if (stockError) {
                        Text(
                            text = "Yetersiz Stok! Depoda bulunan: $stockAvailable. (Sepettekiler dahil talep: $requestedAmount)",
                            color = MaterialTheme.colorScheme.error,
                            style = MaterialTheme.typography.bodySmall,
                            fontWeight = FontWeight.Bold,
                            textAlign = TextAlign.Center
                        )
                    } else {
                        Text(
                            text = "Mevcut Toplam Stok: $stockAvailable ADET",
                            color = MaterialTheme.colorScheme.secondary,
                            style = MaterialTheme.typography.bodySmall,
                            fontWeight = FontWeight.Medium
                        )
                    }

                    Spacer(modifier = Modifier.height(8.dp))

                    Row(
                        verticalAlignment = Alignment.CenterVertically,
                        horizontalArrangement = Arrangement.Center,
                        modifier = Modifier.fillMaxWidth()
                    ) {
                        IconButton(
                            onClick = {
                                val current = catalogQtyDialogAmountStr.toIntOrNull() ?: 1
                                if (current > 1) {
                                    catalogQtyDialogAmountStr = (current - 1).toString()
                                }
                            },
                            modifier = Modifier
                                .background(MaterialTheme.colorScheme.primaryContainer, shape = CircleShape)
                                .size(40.dp)
                        ) {
                            Icon(Icons.Filled.Remove, contentDescription = "Azalt", tint = MaterialTheme.colorScheme.onPrimaryContainer)
                        }

                        OutlinedTextField(
                            value = catalogQtyDialogAmountStr,
                            onValueChange = { newValue ->
                                if (newValue.isEmpty() || newValue.all { it.isDigit() }) {
                                    catalogQtyDialogAmountStr = newValue
                                }
                            },
                            modifier = Modifier
                                .width(100.dp)
                                .padding(horizontal = 12.dp)
                                .height(52.dp),
                            textStyle = MaterialTheme.typography.titleMedium.copy(
                                textAlign = TextAlign.Center,
                                fontWeight = FontWeight.Bold
                            ),
                            singleLine = true,
                            keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number)
                        )

                        IconButton(
                            onClick = {
                                val current = catalogQtyDialogAmountStr.toIntOrNull() ?: 0
                                catalogQtyDialogAmountStr = (current + 1).toString()
                            },
                            modifier = Modifier
                                .background(MaterialTheme.colorScheme.primaryContainer, shape = CircleShape)
                                .size(40.dp)
                        ) {
                            Icon(Icons.Filled.Add, contentDescription = "Arttır", tint = MaterialTheme.colorScheme.onPrimaryContainer)
                        }
                    }

                    Spacer(modifier = Modifier.height(12.dp))

                    if ((prod.boxQty != null && prod.boxQty > 1) || (prod.packageQty != null && prod.packageQty > 1)) {
                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.spacedBy(8.dp)
                        ) {
                            if (prod.boxQty != null && prod.boxQty > 1) {
                                OutlinedButton(
                                    onClick = {
                                        val qty = prod.boxQty
                                        val existingItem = AppDataStore.catalogCartItems.find { it.product.barcode == prod.barcode }
                                        val requestedAmount = qty + (existingItem?.quantity ?: 0)
                                        val stockAvailable = prod.stockByWarehouse.values.sum()
                                        val stockError = !AppDataStore.allowNegativeStock && requestedAmount > stockAvailable
                                        
                                        if (!stockError) {
                                            if (existingItem != null) {
                                                pendingCartAddInfo = PendingCartAdd(
                                                    title = prod.title,
                                                    existingQty = existingItem.quantity,
                                                    additionQty = qty,
                                                    totalQty = requestedAmount,
                                                    onConfirm = {
                                                        val idx = AppDataStore.catalogCartItems.indexOf(existingItem)
                                                        if (idx != -1) {
                                                            AppDataStore.catalogCartItems[idx] = existingItem.copy(quantity = requestedAmount)
                                                        }
                                                        playFeedbackSuccess(true)
                                                        showCatalogQtyDialog = false
                                                    }
                                                )
                                            } else {
                                                AppDataStore.catalogCartItems.add(CartItem(product = prod, quantity = qty))
                                                playFeedbackSuccess(true)
                                                showCatalogQtyDialog = false
                                            }
                                        } else {
                                            playFeedbackSuccess(false)
                                        }
                                    },
                                    contentPadding = PaddingValues(horizontal = 8.dp, vertical = 6.dp),
                                    modifier = Modifier.weight(1f)
                                ) {
                                    Icon(Icons.Filled.Inbox, contentDescription = null, modifier = Modifier.size(14.dp))
                                    Spacer(modifier = Modifier.width(4.dp))
                                    Text("${prod.boxQty} Adet (Koli)", style = MaterialTheme.typography.labelSmall)
                                }
                            }
                            if (prod.packageQty != null && prod.packageQty > 1) {
                                OutlinedButton(
                                    onClick = {
                                        val qty = prod.packageQty
                                        val existingItem = AppDataStore.catalogCartItems.find { it.product.barcode == prod.barcode }
                                        val requestedAmount = qty + (existingItem?.quantity ?: 0)
                                        val stockAvailable = prod.stockByWarehouse.values.sum()
                                        val stockError = !AppDataStore.allowNegativeStock && requestedAmount > stockAvailable
                                        
                                        if (!stockError) {
                                            if (existingItem != null) {
                                                pendingCartAddInfo = PendingCartAdd(
                                                    title = prod.title,
                                                    existingQty = existingItem.quantity,
                                                    additionQty = qty,
                                                    totalQty = requestedAmount,
                                                    onConfirm = {
                                                        val idx = AppDataStore.catalogCartItems.indexOf(existingItem)
                                                        if (idx != -1) {
                                                            AppDataStore.catalogCartItems[idx] = existingItem.copy(quantity = requestedAmount)
                                                        }
                                                        playFeedbackSuccess(true)
                                                        showCatalogQtyDialog = false
                                                    }
                                                )
                                            } else {
                                                AppDataStore.catalogCartItems.add(CartItem(product = prod, quantity = qty))
                                                playFeedbackSuccess(true)
                                                showCatalogQtyDialog = false
                                            }
                                        } else {
                                            playFeedbackSuccess(false)
                                        }
                                    },
                                    contentPadding = PaddingValues(horizontal = 8.dp, vertical = 6.dp),
                                    modifier = Modifier.weight(1f)
                                ) {
                                    Icon(Icons.Filled.Layers, contentDescription = null, modifier = Modifier.size(14.dp))
                                    Spacer(modifier = Modifier.width(4.dp))
                                    Text("${prod.packageQty} Adet (Paket)", style = MaterialTheme.typography.labelSmall)
                                }
                            }
                        }
                        Spacer(modifier = Modifier.height(12.dp))
                    }

                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        OutlinedButton(
                            onClick = { showCatalogQtyDialog = false },
                            modifier = Modifier.weight(1f)
                        ) {
                            Text("İptal")
                        }
                        Button(
                            onClick = {
                                val currentQty = catalogQtyDialogAmountStr.toIntOrNull() ?: 1
                                if (currentQty > 0) {
                                    val existing = AppDataStore.catalogCartItems.find { it.product.barcode == prod.barcode }
                                    if (existing != null) {
                                        pendingCartAddInfo = PendingCartAdd(
                                            title = prod.title,
                                            existingQty = existing.quantity,
                                            additionQty = currentQty,
                                            totalQty = existing.quantity + currentQty,
                                            onConfirm = {
                                                val idx = AppDataStore.catalogCartItems.indexOf(existing)
                                                if (idx != -1) {
                                                    AppDataStore.catalogCartItems[idx] = existing.copy(quantity = existing.quantity + currentQty)
                                                }
                                                playFeedbackSuccess(true)
                                                scope.launch {
                                                    snackbarHostState.showSnackbar("${prod.title} katalog sepetine eklendi ($currentQty adet).")
                                                }
                                                showCatalogQtyDialog = false
                                            }
                                        )
                                    } else {
                                        AppDataStore.catalogCartItems.add(CartItem(prod, currentQty))
                                        playFeedbackSuccess(true)
                                        scope.launch {
                                            snackbarHostState.showSnackbar("${prod.title} katalog sepetine eklendi ($currentQty adet).")
                                        }
                                        showCatalogQtyDialog = false
                                    }
                                }
                            },
                            enabled = !stockError && (catalogQtyDialogAmountStr.toIntOrNull() ?: 0) > 0,
                            modifier = Modifier.weight(1f)
                        ) {
                            Text("Ekle")
                        }
                    }
                }
            }
        }
    }

    if (pendingCartAddInfo != null) {
        val info = pendingCartAddInfo!!
        AlertDialog(
            onDismissRequest = { pendingCartAddInfo = null },
            title = {
                Row(
                    verticalAlignment = Alignment.CenterVertically,
                    horizontalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    Icon(Icons.Filled.Info, contentDescription = null, tint = MaterialTheme.colorScheme.primary)
                    Text("Ürün Zaten Sepette Var", fontWeight = FontWeight.Bold)
                }
            },
            text = {
                Column(verticalArrangement = Arrangement.spacedBy(10.dp)) {
                    Text(text = info.title, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodyLarge)
                    Text(
                        text = "Bu ürün zaten katalog sepetinizde bulunuyor. Mevcut miktar güncellenecektir.",
                        style = MaterialTheme.typography.bodyMedium
                    )
                    Card(
                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f)),
                        modifier = Modifier.fillMaxWidth().padding(top = 8.dp)
                    ) {
                        Column(modifier = Modifier.padding(12.dp), verticalArrangement = Arrangement.spacedBy(6.dp)) {
                            Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                                Text("Açıklama", fontWeight = FontWeight.SemiBold, style = MaterialTheme.typography.bodySmall)
                                Text("Miktar", fontWeight = FontWeight.SemiBold, style = MaterialTheme.typography.bodySmall)
                            }
                            HorizontalDivider(modifier = Modifier.padding(vertical = 4.dp))
                            Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                                Text("Daha Önce Eklenen:", style = MaterialTheme.typography.bodyMedium)
                                Text("${info.existingQty} Adet", fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodyMedium)
                            }
                            Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                                Text("Şimdi Eklenen:", style = MaterialTheme.typography.bodyMedium)
                                Text("+ ${info.additionQty} Adet", fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary, style = MaterialTheme.typography.bodyMedium)
                            }
                            HorizontalDivider(modifier = Modifier.padding(vertical = 4.dp))
                            Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                                Text("Toplam Miktar:", fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodyMedium)
                                Text("${info.totalQty} Adet", fontWeight = FontWeight.Black, color = MaterialTheme.colorScheme.secondary, style = MaterialTheme.typography.bodyMedium)
                            }
                        }
                    }
                }
            },
            confirmButton = {
                Button(
                    onClick = {
                        info.onConfirm()
                        pendingCartAddInfo = null
                    }
                ) {
                    Text("Kabul Et ve Ekle")
                }
            },
            dismissButton = {
                TextButton(
                    onClick = { pendingCartAddInfo = null }
                ) {
                    Text("İptal Et")
                }
            }
        )
    }

    if (showCustomerPickerDialog) {
        AlertDialog(
            onDismissRequest = { showCustomerPickerDialog = false },
            title = { Text("Cari Hesap Kartı Seçin") },
            text = {
                LazyColumn(
                    modifier = Modifier
                        .fillMaxWidth()
                        .height(350.dp),
                    verticalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    items(AppDataStore.customers) { cust ->
                        Box(
                            modifier = Modifier
                                .fillMaxWidth()
                                .background(
                                    if (selectedCustomer?.id == cust.id) MaterialTheme.colorScheme.primaryContainer 
                                    else MaterialTheme.colorScheme.surfaceVariant,
                                    shape = RoundedCornerShape(8.dp)
                                )
                                .clickable {
                                    AppDataStore.activeSelectedCustomer.value = cust
                                    showCustomerPickerDialog = false
                                    playFeedbackSuccess(true)
                                }
                                .padding(12.dp)
                        ) {
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Column {
                                    Text(cust.name, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodyMedium)
                                    Text("Kod: ${cust.id} | Vergi No: ${cust.taxNumber}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                    Text("Fiyat: ${cust.priceGroup}", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.secondary)
                                }
                                Text(
                                    String.format("%.2f ₺", cust.balance),
                                    fontWeight = FontWeight.Bold,
                                    color = if (cust.balance >= 0) MaterialTheme.colorScheme.error else Color(0xFF43A047),
                                    style = MaterialTheme.typography.labelMedium
                                )
                            }
                        }
                    }
                }
            },
            confirmButton = {
                TextButton(onClick = { showCustomerPickerDialog = false }) {
                    Text("Kapat")
                }
            }
        )
    }

    // Cart is now opened inside full screen in-page layout below

    // --- DIALOG B: CREATE NEW SEVK / STOCK TRANSFER DIALOG ---
    if (showTransferDialog) {
        var originDep by remember { mutableStateOf("Ana Depo") }
        var targetDep by remember { mutableStateOf("Ankara Merkez") }
        var selectedItem by remember { mutableStateOf(initialProducts.firstOrNull()) } // default to first product
        var qtyInput by remember { mutableStateOf("15") }
        
        Dialog(onDismissRequest = { showTransferDialog = false }) {
            Box(
                modifier = Modifier
                    .fillMaxWidth()
                    .clip(RoundedCornerShape(20.dp))
                    .background(MaterialTheme.colorScheme.surface)
                    .padding(20.dp)
            ) {
                Column(verticalArrangement = Arrangement.spacedBy(16.dp)) {
                    Text("Yeni Stok Sevk Belgesi", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
                    
                    HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)

                    // Origin & Destination Warehouses
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.spacedBy(12.dp)
                    ) {
                        Column(modifier = Modifier.weight(1f)) {
                            Text("Çıkış Deposu", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.outline)
                            Text(originDep, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold)
                        }
                        Icon(Icons.Filled.ArrowForward, contentDescription = null, tint = MaterialTheme.colorScheme.primary, modifier = Modifier.align(Alignment.CenterVertically))
                        Column(modifier = Modifier.weight(1f)) {
                            Text("Giriş Deposu", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.outline)
                            Text(targetDep, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold)
                        }
                    }

                    // Product pickers (list selection simulation via lazy column row item click or simplified options)
                    Column(verticalArrangement = Arrangement.spacedBy(4.dp)) {
                        Text("Sevk Edilecek Ürün", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.primary)
                        Box(
                            modifier = Modifier
                                .fillMaxWidth()
                                .background(MaterialTheme.colorScheme.surfaceVariant, RoundedCornerShape(8.dp))
                                .clickable {
                                    if (initialProducts.isNotEmpty()) {
                                        // simple rotate list items for simulation
                                        val currentIdx = initialProducts.indexOf(selectedItem)
                                        val nextIdx = (currentIdx + 1) % initialProducts.size
                                        selectedItem = initialProducts[nextIdx]
                                    }
                                }
                                .padding(12.dp)
                        ) {
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Column {
                                    Text(selectedItem?.title ?: "Ürün Bulunmamaktadır", style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.SemiBold, maxLines = 1)
                                    Text("Kod: ${selectedItem?.code ?: "-"}", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.outline)
                                }
                                Icon(Icons.Filled.SwapHoriz, contentDescription = "Sıradaki", tint = MaterialTheme.colorScheme.primary)
                            }
                        }
                    }

                    // Quantity
                    OutlinedTextField(
                        value = qtyInput,
                        onValueChange = { qtyInput = it },
                        modifier = Modifier.fillMaxWidth(),
                        label = { Text("Transfer Edilecek Miktar") },
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                        singleLine = true
                    )

                    HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)

                    // Actions
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        OutlinedButton(onClick = { showTransferDialog = false }, modifier = Modifier.weight(1f)) {
                            Text("Vazgeç")
                        }
                        Button(
                            onClick = {
                                val amount = qtyInput.toIntOrNull() ?: 1
                                val trId = "TR-" + (500 + transfersList.size + 1)
                                val date = "08.06.2026"
                                val newTransfer = StockTransfer(
                                    id = trId,
                                    date = date,
                                    origin = originDep,
                                    dest = targetDep,
                                    itemTitle = selectedItem?.title ?: "",
                                    qty = amount,
                                    state = "Bekliyor"
                                )
                                transfersList.add(0, newTransfer)
                                playFeedbackSuccess(true)
                                showTransferDialog = false
                            },
                            modifier = Modifier.weight(1.2f)
                        ) {
                            Text("Sevk Belgesi Kaydet")
                        }
                    }
                }
            }
        }

        } // end of main Column

        // FULL PAGE OVERLAY FOR SHOPPING CART (IN-PAGE FULL SCREEN, NOT POPUP)
        androidx.compose.animation.AnimatedVisibility(
            visible = showCatalogCartDialog,
            enter = androidx.compose.animation.scaleIn(initialScale = 0.95f) + androidx.compose.animation.fadeIn(),
            exit = androidx.compose.animation.scaleOut(targetScale = 0.95f) + androidx.compose.animation.fadeOut(),
            modifier = Modifier.fillMaxSize()
        ) {
            Surface(
                modifier = Modifier.fillMaxSize(),
                color = MaterialTheme.colorScheme.background
            ) {
                CartContentBody(
                    selectedPriceTier = selectedPriceTier,
                    onClose = { showCatalogCartDialog = false },
                    isRightDrawer = true
                )
            }
        }
    }
}



@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun AddProductCatalogDialog(
    onDismiss: () -> Unit,
    onSave: (ProductCatalog) -> Unit
) {
    val dialogContext = LocalContext.current
    var barcode by remember { mutableStateOf("") }
    var code by remember { mutableStateOf("") }
    var title by remember { mutableStateOf("") }
    var category by remember { mutableStateOf("Tümü") }
    var marka by remember { mutableStateOf("") }
    var aisle by remember { mutableStateOf("") }
    var desc by remember { mutableStateOf("") }
    val definedPrices = remember { AppDataStore.definitions["Fiyat"] ?: emptyList() }
    val priceValuesMap = remember { androidx.compose.runtime.mutableStateMapOf<String, String>() }
    var kdvText by remember { mutableStateOf("20") }
    var boxQtyText by remember { mutableStateOf("") }
    var packQtyText by remember { mutableStateOf("") }
    var imageUrlText by remember { mutableStateOf("") }
    
    var stockAna by remember { mutableStateOf("50") }
    var stockAnkara by remember { mutableStateOf("10") }
    var stockEge by remember { mutableStateOf("5") }

    var titleError by remember { mutableStateOf(false) }
    var codeError by remember { mutableStateOf(false) }

    Dialog(onDismissRequest = onDismiss) {
        Box(
            modifier = Modifier
                .fillMaxWidth()
                .clip(RoundedCornerShape(20.dp))
                .background(MaterialTheme.colorScheme.surface)
                .padding(20.dp)
        ) {
            Column(
                modifier = Modifier
                    .fillMaxHeight(0.85f)
                    .verticalScroll(rememberScrollState()),
                verticalArrangement = Arrangement.spacedBy(16.dp)
            ) {
                Text(
                    text = "Yeni Ürün / Stok Kartı Girişi",
                    style = MaterialTheme.typography.titleMedium,
                    fontWeight = FontWeight.Bold,
                    color = MaterialTheme.colorScheme.primary
                )

                HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)

                OutlinedTextField(
                    value = title,
                    onValueChange = { title = it; titleError = false },
                    label = { Text("Ürün Adı / Açıklaması *") },
                    isError = titleError,
                    singleLine = true,
                    modifier = Modifier.fillMaxWidth()
                )
                if (titleError) {
                    Text("Ürün adı boş geçilemez!", color = MaterialTheme.colorScheme.error, style = MaterialTheme.typography.labelSmall)
                }

                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    OutlinedTextField(
                        value = code,
                        onValueChange = { code = it; codeError = false },
                        label = { Text("Stok Kodu *") },
                        isError = codeError,
                        singleLine = true,
                        modifier = Modifier.weight(1.2f)
                    )
                    OutlinedTextField(
                        value = barcode,
                        onValueChange = { barcode = it },
                        label = { Text("Barkod No") },
                        singleLine = true,
                        modifier = Modifier.weight(1.5f)
                    )
                }
                if (codeError) {
                    Text("Stok kodu boş geçilemez!", color = MaterialTheme.colorScheme.error, style = MaterialTheme.typography.labelSmall)
                }

                // Description
                OutlinedTextField(
                    value = desc,
                    onValueChange = { desc = it },
                    label = { Text("Detaylı Ürün Açıklaması") },
                    maxLines = 3,
                    modifier = Modifier.fillMaxWidth()
                )

                // Category, Marka, Reyon selection dropdown
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    Box(modifier = Modifier.weight(1f)) {
                        SearchableDropdown(
                            label = "Ürün Kategorisi",
                            items = AppDataStore.definitions["Kategori"] ?: emptyList(),
                            selectedValue = category,
                            onValueChange = { category = it }
                        )
                    }
                    Box(modifier = Modifier.weight(1f)) {
                        SearchableDropdown(
                            label = "Marka",
                            items = AppDataStore.definitions["Marka"] ?: emptyList(),
                            selectedValue = marka,
                            onValueChange = { marka = it }
                        )
                    }
                }
                
                Box(modifier = Modifier.fillMaxWidth()) {
                    SearchableDropdown(
                        label = "Reyon",
                        items = AppDataStore.definitions["Reyon"] ?: emptyList(),
                        selectedValue = aisle,
                        onValueChange = { aisle = it }
                    )
                }

                // Dynamic Prices Block based on defined prices with formulas
                definedPrices.chunked(2).forEach { rowItems ->
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        rowItems.forEach { priceType ->
                            Box(modifier = Modifier.weight(1f)) {
                                val priceValueStr = priceValuesMap[priceType] ?: ""
                                val holdsFormula = priceValueStr.isNotBlank() && priceValueStr.toDoubleOrNull() == null
                                val evaluatedFormulaPrice = if (holdsFormula) {
                                    com.example.util.PriceFormulaEvaluator.evaluate(priceValueStr, priceValuesMap.toMap())
                                } else null

                                OutlinedTextField(
                                    value = priceValueStr,
                                    onValueChange = { priceValuesMap[priceType] = it },
                                    label = { Text("$priceType (TL)") },
                                    modifier = Modifier.fillMaxWidth(),
                                    shape = RoundedCornerShape(10.dp),
                                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Text),
                                    singleLine = true,
                                    supportingText = if (evaluatedFormulaPrice != null) {
                                        { Text("Hesaplanan: ₺${String.format("%,.2f", evaluatedFormulaPrice)}", color = MaterialTheme.colorScheme.primary, style = MaterialTheme.typography.labelSmall) }
                                    } else if (holdsFormula) {
                                        { Text("Geçersiz formül", color = MaterialTheme.colorScheme.error, style = MaterialTheme.typography.labelSmall) }
                                    } else null,
                                    trailingIcon = if (evaluatedFormulaPrice != null) {
                                        {
                                            IconButton(onClick = {
                                                priceValuesMap[priceType] = String.format(java.util.Locale.US, "%.2f", evaluatedFormulaPrice)
                                            }) {
                                                Icon(
                                                    imageVector = Icons.Filled.Calculate,
                                                    contentDescription = "Formülü Uygula",
                                                    tint = MaterialTheme.colorScheme.primary
                                                )
                                            }
                                        }
                                    } else null
                                )
                            }
                        }
                        if (rowItems.size < 2) {
                            Spacer(modifier = Modifier.weight(1f))
                        }
                    }
                }

                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    Box(modifier = Modifier.weight(1f)) {
                        SearchableDropdown(
                            label = "KDV Oranı (%)",
                            items = listOf("0", "1", "8", "10", "18", "20"),
                            selectedValue = kdvText.replace("%", ""),
                            onValueChange = { kdvText = it }
                        )
                    }
                    Spacer(modifier = Modifier.weight(1f))
                }

                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    OutlinedTextField(
                        value = boxQtyText,
                        onValueChange = { boxQtyText = it },
                        label = { Text("Koli İçi Adet") },
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                        singleLine = true,
                        modifier = Modifier.weight(1f)
                    )
                    OutlinedTextField(
                        value = packQtyText,
                        onValueChange = { packQtyText = it },
                        label = { Text("Paket İçi Adet") },
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                        singleLine = true,
                        modifier = Modifier.weight(1f)
                    )
                }

                OutlinedTextField(
                    value = imageUrlText,
                    onValueChange = { imageUrlText = it },
                    label = { Text("Resim URL'si (imageUrl)") },
                    singleLine = true,
                    modifier = Modifier.fillMaxWidth(),
                    placeholder = { Text("https://example.com/image.jpg") }
                )

                Spacer(modifier = Modifier.height(12.dp))

                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    OutlinedButton(
                        onClick = onDismiss,
                        modifier = Modifier.weight(1f)
                    ) {
                        Text("Vazgeç")
                    }
                    Button(
                        onClick = {
                            var hasErr = false
                            if (title.isBlank()) {
                                titleError = true
                                hasErr = true
                            }
                            if (code.isBlank()) {
                                codeError = true
                                hasErr = true
                            }

                            // Evaluate formulas or clean doubles
                            val resolvedPrices = mutableMapOf<String, Double>()
                            val temporaryPriceMap = priceValuesMap.toMap()
                            var hasPriceError = false

                            temporaryPriceMap.forEach { (priceType, priceValue) ->
                                if (priceValue.isNotBlank()) {
                                    val directDouble = priceValue.toDoubleOrNull()
                                    if (directDouble != null) {
                                        resolvedPrices[priceType] = directDouble
                                    } else {
                                        val evaluated = com.example.util.PriceFormulaEvaluator.evaluate(priceValue, temporaryPriceMap)
                                        if (evaluated != null) {
                                            resolvedPrices[priceType] = evaluated
                                        } else {
                                            hasPriceError = true
                                        }
                                    }
                                } else {
                                    resolvedPrices[priceType] = 0.0
                                }
                            }

                            if (hasPriceError) {
                                hasErr = true
                                android.widget.Toast.makeText(dialogContext, "Lütfen fiyat alanlarına geçerli sayısal değerler veya formümler girin (örn: Perakende * 1.2)!", android.widget.Toast.LENGTH_LONG).show()
                            }

                            if (!hasErr) {
                                val finalBarcode = if (barcode.isBlank()) (8690000000000L..(8699999999999L)).random().toString() else barcode
                                val basePrice = resolvedPrices["Perakende"] ?: 0.0
                                val dealerPrice = resolvedPrices["Bayi"] ?: 0.0
                                val wholesalePrice = resolvedPrices["Toptan"] ?: 0.0
                                val kdvPercent = kdvText.toIntOrNull() ?: 20

                                val boxQty = boxQtyText.toIntOrNull()
                                val packQty = packQtyText.toIntOrNull()

                                val randomColor = when (category) {
                                    "Endüstriyel Yağlar" -> Color(0xFFFFB300)
                                    "Filtre Grupları" -> Color(0xFF1E88E5)
                                    "Yedek Parça" -> Color(0xFF43A047)
                                    else -> Color(0xFFE53935)
                                }

                                val newProduct = ProductCatalog(
                                    barcode = finalBarcode,
                                    code = code,
                                    title = title,
                                    category = category,
                                    desc = desc,
                                    basePrice = basePrice,
                                    dealerPrice = dealerPrice,
                                    wholesalePrice = wholesalePrice,
                                    customPrices = resolvedPrices,
                                    kdvPercent = kdvPercent,
                                    boxQty = boxQty,
                                    packageQty = packQty,
                                    imageUrlColor = randomColor,
                                    brand = if (marka.isBlank()) null else marka,
                                    aisle = if (aisle.isBlank()) null else aisle,
                                    imageUrl = if (imageUrlText.isBlank()) null else imageUrlText,
                                    stockByWarehouse = mapOf(
                                        "Ana Depo" to 0,
                                        "Ankara Merkez" to 0,
                                        "Ege Bölge" to 0
                                    )
                                )
                                onSave(newProduct)
                            }
                        },
                        modifier = Modifier
                            .weight(1.2f)
                            .testTag("submit_new_product")
                    ) {
                        Text("Kaydet")
                    }
                }
            }
        }
    }
}

// Helper Widget: Product List Card for Catalogue Screen View
@Composable
fun InteractiveProductListCard(
    product: ProductCatalog,
    priceTier: String,
    onSelectStock: () -> Unit,
    onAddToCart: () -> Unit,
    onImageClick: () -> Unit = {}
) {
    val displayPrice = product.getPriceForGroup(priceTier)
    val totalStockSum = product.stockByWarehouse.values.sum()

    Card(
        modifier = Modifier
            .fillMaxWidth()
            .clickable { onSelectStock() },
        shape = RoundedCornerShape(12.dp),
        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
        border = androidx.compose.foundation.BorderStroke(0.5.dp, MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.4f)),
        elevation = CardDefaults.cardElevation(defaultElevation = 1.dp)
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(10.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            // Far Left: Elegant Image Frame
            Box(
                modifier = Modifier
                    .size(84.dp)
                    .clip(RoundedCornerShape(12.dp))
                    .background(MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.25f))
                    .border(0.5.dp, MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.6f), RoundedCornerShape(12.dp))
                    .clickable { onImageClick() },
                contentAlignment = Alignment.Center
            ) {
                ProductImageSwiper(
                    product = product,
                    modifier = Modifier.fillMaxSize(),
                    onImageClick = onImageClick
                )
            }

            Spacer(modifier = Modifier.width(12.dp))

            // Middle: Core Product Information
            Column(
                modifier = Modifier.weight(1f),
                verticalArrangement = Arrangement.spacedBy(3.dp)
            ) {
                // Category Pill Badge
                Box(
                    modifier = Modifier
                        .background(
                            color = MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.3f),
                            shape = RoundedCornerShape(4.dp)
                        )
                        .padding(horizontal = 6.dp, vertical = 2.dp)
                ) {
                    Text(
                        text = product.category.uppercase(),
                        style = MaterialTheme.typography.labelSmall.copy(
                            fontWeight = FontWeight.Bold,
                            fontSize = 9.sp,
                            letterSpacing = 0.3.sp
                        ),
                        color = MaterialTheme.colorScheme.primary
                    )
                }

                Text(
                    text = product.title,
                    fontWeight = FontWeight.Bold,
                    style = MaterialTheme.typography.titleSmall.copy(lineHeight = 16.sp),
                    color = MaterialTheme.colorScheme.onSurface,
                    maxLines = 2,
                    overflow = TextOverflow.Ellipsis
                )

                Text(
                    text = "KOD: ${product.code}  •  Barkod: ${product.barcode}",
                    style = MaterialTheme.typography.labelSmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.7f),
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis
                )

                // Package / Box quantities
                if ((product.boxQty != null && product.boxQty > 1) || (product.packageQty != null && product.packageQty > 1)) {
                    val boxText = if (product.boxQty != null && product.boxQty > 1) "Koli x${product.boxQty}" else ""
                    val packText = if (product.packageQty != null && product.packageQty > 1) "Paket x${product.packageQty}" else ""
                    val joined = listOf(boxText, packText).filter { it.isNotEmpty() }.joinToString(" | ")
                    Box(
                        modifier = Modifier
                            .background(MaterialTheme.colorScheme.secondaryContainer.copy(alpha = 0.25f), RoundedCornerShape(4.dp))
                            .padding(horizontal = 4.dp, vertical = 1.dp)
                    ) {
                        Text(
                            text = joined,
                            style = MaterialTheme.typography.labelSmall.copy(fontSize = 9.sp),
                            color = MaterialTheme.colorScheme.secondary,
                            fontWeight = FontWeight.SemiBold
                        )
                    }
                }

                // Sub-information: Stock info with helper indicator
                Row(
                    verticalAlignment = Alignment.CenterVertically,
                    horizontalArrangement = Arrangement.spacedBy(4.dp),
                    modifier = Modifier.padding(top = 1.dp)
                ) {
                    val hasStock = totalStockSum > 0
                    Box(
                        modifier = Modifier
                            .size(6.dp)
                            .background(
                                color = if (hasStock) Color(0xFF4CAF50) else Color(0xFFE53935),
                                shape = CircleShape
                            )
                    )
                    Text(
                        text = if (hasStock) "Mevcut: $totalStockSum adet" else "Stok Yok",
                        style = MaterialTheme.typography.labelSmall,
                        color = if (hasStock) MaterialTheme.colorScheme.onSurfaceVariant else Color(0xFFE53935),
                        fontWeight = FontWeight.Medium
                    )
                }
            }

            Spacer(modifier = Modifier.width(8.dp))

            // Right: Transaction Panel (Price & Add Button)
            Column(
                horizontalAlignment = Alignment.End,
                verticalArrangement = Arrangement.spacedBy(6.dp)
            ) {
                Column(horizontalAlignment = Alignment.End) {
                    Text(
                        text = String.format("%,.2f ₺", displayPrice),
                        fontWeight = FontWeight.Bold,
                        style = MaterialTheme.typography.titleMedium,
                        color = MaterialTheme.colorScheme.primary
                    )
                    Text(
                        text = "+%${product.kdvPercent} KDV",
                        style = MaterialTheme.typography.labelSmall.copy(fontSize = 9.sp),
                        color = MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.5f)
                    )
                }

                IconButton(
                    onClick = onAddToCart,
                    modifier = Modifier
                        .size(36.dp)
                        .background(
                            color = if (AppDataStore.allowNegativeStock || totalStockSum > 0) 
                                MaterialTheme.colorScheme.primary
                            else 
                                MaterialTheme.colorScheme.surfaceVariant,
                            shape = RoundedCornerShape(8.dp)
                        ),
                    enabled = AppDataStore.allowNegativeStock || totalStockSum > 0
                ) {
                    Icon(
                        imageVector = Icons.Filled.AddShoppingCart,
                        contentDescription = "Sipariş Ekle",
                        tint = if (AppDataStore.allowNegativeStock || totalStockSum > 0) 
                            MaterialTheme.colorScheme.onPrimary
                        else 
                            MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.4f),
                        modifier = Modifier.size(16.dp)
                    )
                }
            }
        }
    }
}

// Helper Widget: Product Grid Card for Catalogue Screen View
@Composable
fun InteractiveProductGridCard(
    product: ProductCatalog,
    priceTier: String,
    onSelectStock: () -> Unit,
    onAddToCart: () -> Unit,
    onImageClick: () -> Unit = {}
) {
    val displayPrice = product.getPriceForGroup(priceTier)
    val totalStockSum = product.stockByWarehouse.values.sum()

    Card(
        modifier = Modifier
            .fillMaxWidth()
            .height(310.dp)
            .clickable { onSelectStock() },
        shape = RoundedCornerShape(12.dp),
        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
        border = androidx.compose.foundation.BorderStroke(0.5.dp, MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.4f)),
        elevation = CardDefaults.cardElevation(defaultElevation = 1.dp)
    ) {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(8.dp)
        ) {
            // Image / Icon Box at the top
            Box(
                modifier = Modifier
                    .fillMaxWidth()
                    .height(115.dp)
                    .clip(RoundedCornerShape(8.dp))
                    .background(MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.2f))
                    .border(0.5.dp, MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.4f), RoundedCornerShape(8.dp)),
                contentAlignment = Alignment.Center
            ) {
                ProductImageSwiper(
                    product = product,
                    modifier = Modifier.fillMaxSize(),
                    onImageClick = onImageClick
                )
            }

            Spacer(modifier = Modifier.height(6.dp))

            // Category Badge Pill
            Box(
                modifier = Modifier
                    .background(
                        color = MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.3f),
                        shape = RoundedCornerShape(4.dp)
                    )
                    .padding(horizontal = 6.dp, vertical = 2.dp)
            ) {
                Text(
                    text = product.category.uppercase(),
                    style = MaterialTheme.typography.labelSmall.copy(
                        fontWeight = FontWeight.Bold,
                        fontSize = 8.sp,
                        letterSpacing = 0.3.sp
                    ),
                    color = MaterialTheme.colorScheme.primary,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis
                )
            }

            Spacer(modifier = Modifier.height(2.dp))

            Text(
                text = product.title,
                fontWeight = FontWeight.Bold,
                style = MaterialTheme.typography.bodyMedium.copy(fontSize = 13.sp, lineHeight = 16.sp),
                color = MaterialTheme.colorScheme.onSurface,
                maxLines = 2,
                overflow = TextOverflow.Ellipsis
            )

            Text(
                text = "Kod: ${product.code}",
                style = MaterialTheme.typography.labelSmall,
                color = Color.Gray,
                maxLines = 1,
                overflow = TextOverflow.Ellipsis
            )

            // Stock & KDV
            Row(
                modifier = Modifier.fillMaxWidth().padding(top = 2.dp),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                val hasStock = totalStockSum > 0
                Text(
                    text = if (hasStock) "$totalStockSum Adet" else "Mevcut Değil",
                    style = MaterialTheme.typography.labelSmall.copy(fontSize = 10.sp),
                    fontWeight = FontWeight.Bold,
                    color = if (hasStock) Color(0xFF4CAF50) else Color(0xFFE53935)
                )
                Text(
                    text = "KDV: %${product.kdvPercent}",
                    style = MaterialTheme.typography.labelSmall.copy(fontSize = 10.sp),
                    color = Color.DarkGray
                )
            }

            Spacer(modifier = Modifier.weight(1f))

            if ((product.boxQty != null && product.boxQty > 1) || (product.packageQty != null && product.packageQty > 1)) {
                val boxText = if (product.boxQty != null && product.boxQty > 1) "Koli: ${product.boxQty}" else ""
                val packText = if (product.packageQty != null && product.packageQty > 1) "Paket: ${product.packageQty}" else ""
                val joined = listOf(boxText, packText).filter { it.isNotEmpty() }.joinToString(" | ")
                Text(joined, style = MaterialTheme.typography.labelSmall.copy(fontSize = 10.sp), color = MaterialTheme.colorScheme.secondary, fontWeight = FontWeight.SemiBold, maxLines = 1, overflow = TextOverflow.Ellipsis)
            }

            // Price Big
            Text(
                text = String.format("%,.2f ₺", displayPrice),
                fontWeight = FontWeight.ExtraBold,
                style = MaterialTheme.typography.titleMedium.copy(fontSize = 16.sp),
                color = MaterialTheme.colorScheme.primary,
                modifier = Modifier.align(Alignment.End)
            )

            Spacer(modifier = Modifier.height(4.dp))

            // Action Buttons Row
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.spacedBy(4.dp)
            ) {
                Button(
                    onClick = onAddToCart,
                    modifier = Modifier.weight(1f).height(32.dp),
                    contentPadding = PaddingValues(0.dp),
                    shape = RoundedCornerShape(8.dp),
                    enabled = AppDataStore.allowNegativeStock || totalStockSum > 0,
                    colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.primary)
                ) {
                    Icon(Icons.Filled.AddShoppingCart, contentDescription = null, modifier = Modifier.size(12.dp))
                    Spacer(modifier = Modifier.width(4.dp))
                    Text("Sipariş", style = MaterialTheme.typography.labelSmall.copy(fontSize = 11.sp, fontWeight = FontWeight.Bold))
                }
            }
        }
    }
}

@Composable
fun ProductImageSwiper(
    product: ProductCatalog,
    modifier: Modifier = Modifier,
    onImageClick: () -> Unit = {}
) {
    val images = remember(product.imageUrl, product.localImagePath) {
        val list = mutableListOf<Any>()
        val localFiles = mutableListOf<java.io.File>()
        if (!product.localImagePath.isNullOrBlank()) {
            val paths = product.localImagePath.split(Regex("[,;|\\s]+"))
                .map { it.trim() }
                .filter { it.isNotEmpty() }
            paths.forEach { path ->
                val file = java.io.File(path)
                if (file.exists() && file.length() > 0) {
                    localFiles.add(file)
                }
            }
        }
        val remoteUrls = mutableListOf<String>()
        if (!product.imageUrl.isNullOrBlank()) {
            val urls = product.imageUrl.split(Regex("[,;|\\s]+"))
                .map { it.trim() }
                .filter { it.isNotEmpty() }
            remoteUrls.addAll(urls)
        }
        list.addAll(localFiles)
        remoteUrls.forEachIndexed { idx, url ->
            if (idx >= localFiles.size) {
                list.add(url)
            }
        }
        list
    }

    Box(
        modifier = modifier
            .clip(RoundedCornerShape(12.dp)),
        contentAlignment = Alignment.Center
    ) {
        if (images.isEmpty()) {
            val icon = when (product.category) {
                "Endüstriyel Yağlar" -> Icons.Filled.WaterDrop
                "Filtre Grupları" -> Icons.Filled.FilterAlt
                "Yedek Parça" -> Icons.Filled.SettingsSuggest
                else -> Icons.Filled.Construction
            }
            Icon(
                imageVector = icon,
                contentDescription = null,
                tint = product.imageUrlColor,
                modifier = Modifier.size(36.dp).clickable { onImageClick() }
            )
        } else {
            val pagerState = androidx.compose.foundation.pager.rememberPagerState(pageCount = { images.size })
            val coroutineScope = rememberCoroutineScope()
            Box(modifier = Modifier.fillMaxSize()) {
                androidx.compose.foundation.pager.HorizontalPager(
                    state = pagerState,
                    modifier = Modifier.fillMaxSize()
                ) { page ->
                    val imageModel = images[page]
                    Box(
                        modifier = Modifier.fillMaxSize(),
                        contentAlignment = Alignment.Center
                    ) {
                        coil.compose.AsyncImage(
                            model = imageModel,
                            contentDescription = "${product.title} - Resim ${page + 1}",
                            modifier = Modifier
                                .fillMaxSize()
                                .clickable { onImageClick() }
                                .padding(4.dp),
                            contentScale = androidx.compose.ui.layout.ContentScale.Fit // Fit proportionally to frame!
                        )
                    }
                }

                // Manual left-right navigation arrows overlay for easily swiping pictures
                if (images.size > 1) {
                    if (pagerState.currentPage > 0) {
                        IconButton(
                            onClick = {
                                coroutineScope.launch {
                                    pagerState.animateScrollToPage(pagerState.currentPage - 1)
                                }
                            },
                            modifier = Modifier
                                .align(Alignment.CenterStart)
                                .padding(start = 2.dp)
                                .background(Color.Black.copy(alpha = 0.4f), CircleShape)
                                .size(24.dp)
                        ) {
                            Icon(
                                imageVector = Icons.Filled.ChevronLeft,
                                contentDescription = "Geri",
                                tint = Color.White,
                                modifier = Modifier.size(16.dp)
                            )
                        }
                    }

                    if (pagerState.currentPage < images.size - 1) {
                        IconButton(
                            onClick = {
                                coroutineScope.launch {
                                    pagerState.animateScrollToPage(pagerState.currentPage + 1)
                                }
                            },
                            modifier = Modifier
                                .align(Alignment.CenterEnd)
                                .padding(end = 2.dp)
                                .background(Color.Black.copy(alpha = 0.4f), CircleShape)
                                .size(24.dp)
                        ) {
                            Icon(
                                imageVector = Icons.Filled.ChevronRight,
                                contentDescription = "İleri",
                                tint = Color.White,
                                modifier = Modifier.size(16.dp)
                            )
                        }
                    }
                }

                // Tiny dot indicators if multiple images
                if (images.size > 1) {
                    Row(
                        modifier = Modifier
                            .align(Alignment.BottomCenter)
                            .padding(bottom = 6.dp)
                            .background(Color.Black.copy(alpha = 0.4f), RoundedCornerShape(8.dp))
                            .padding(horizontal = 4.dp, vertical = 2.dp),
                        horizontalArrangement = Arrangement.spacedBy(4.dp),
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        repeat(images.size) { index ->
                            val active = pagerState.currentPage == index
                            Box(
                                modifier = Modifier
                                    .size(5.dp)
                                    .background(
                                        color = if (active) Color.White else Color.White.copy(alpha = 0.4f),
                                        shape = CircleShape
                                    )
                            )
                        }
                    }
                }
            }
        }
    }
}

@Composable
private fun CartContentBody(
    selectedPriceTier: String,
    onClose: () -> Unit,
    isRightDrawer: Boolean
) {
    val context = androidx.compose.ui.platform.LocalContext.current
    val playFeedbackSuccess = { isOk: Boolean ->
        try {
            val toneG = android.media.ToneGenerator(android.media.AudioManager.STREAM_NOTIFICATION, 90)
            if (isOk) {
                toneG.startTone(android.media.ToneGenerator.TONE_PROP_BEEP, 120)
            } else {
                toneG.startTone(android.media.ToneGenerator.TONE_CDMA_SOFT_ERROR_LITE, 300)
            }
        } catch (_: Exception) {}
    }

    var deleteConfirmationItem by remember { mutableStateOf<CartItem?>(null) }
    var noteEditingItem by remember { mutableStateOf<CartItem?>(null) }
    var editingNoteText by remember { mutableStateOf("") }

    Column(
        modifier = Modifier
            .padding(20.dp)
            .fillMaxWidth()
            .then(if (isRightDrawer) Modifier.fillMaxHeight() else Modifier.wrapContentHeight()),
        verticalArrangement = Arrangement.spacedBy(12.dp)
    ) {
        // Header
        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.SpaceBetween,
            verticalAlignment = Alignment.CenterVertically
        ) {
            Row(
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.spacedBy(8.dp)
            ) {
                Icon(
                    imageVector = Icons.Filled.ShoppingCart,
                    contentDescription = null,
                    tint = MaterialTheme.colorScheme.primary,
                    modifier = Modifier.size(24.dp)
                )
                Text(
                    text = "Katalog Sepeti",
                    style = MaterialTheme.typography.titleLarge,
                    fontWeight = FontWeight.Bold,
                    color = MaterialTheme.colorScheme.onSurface
                )
            }
            IconButton(onClick = onClose) {
                Icon(
                    imageVector = Icons.Filled.Close,
                    contentDescription = "Kapat",
                    tint = MaterialTheme.colorScheme.outline
                )
            }
        }

        HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))

        if (AppDataStore.catalogCartItems.isEmpty()) {
            Box(
                modifier = Modifier
                    .fillMaxWidth()
                    .then(if (isRightDrawer) Modifier.weight(1f) else Modifier.height(160.dp)),
                contentAlignment = Alignment.Center
            ) {
                Column(
                    horizontalAlignment = Alignment.CenterHorizontally,
                    verticalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    Icon(
                        imageVector = Icons.Filled.ShoppingCart,
                        contentDescription = null,
                        tint = MaterialTheme.colorScheme.outline.copy(alpha = 0.5f),
                        modifier = Modifier.size(48.dp)
                    )
                    Text(
                        text = "Sepetiniz henüz boş.",
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.outline
                    )
                }
            }
        } else {
            // Cart Item List
            LazyColumn(
                modifier = Modifier
                    .fillMaxWidth()
                    .then(if (isRightDrawer) Modifier.weight(1f) else Modifier.heightIn(max = 240.dp)),
                verticalArrangement = Arrangement.spacedBy(8.dp)
            ) {
                items(AppDataStore.catalogCartItems) { item ->
                    val price = item.product.getPriceForGroup(selectedPriceTier)
                    Row(
                        modifier = Modifier
                            .fillMaxWidth()
                            .background(
                                color = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.4f),
                                shape = RoundedCornerShape(12.dp)
                            )
                            .padding(8.dp),
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Column(modifier = Modifier.weight(1f)) {
                            Text(
                                text = item.product.title,
                                style = MaterialTheme.typography.bodyMedium,
                                fontWeight = FontWeight.Bold,
                                maxLines = 1,
                                color = MaterialTheme.colorScheme.onSurface
                            )
                            Text(
                                text = String.format("Birim: %,.2f ₺", price),
                                style = MaterialTheme.typography.bodySmall,
                                color = MaterialTheme.colorScheme.onSurfaceVariant
                            )

                            // Show product note if not empty
                            if (item.note.isNotEmpty()) {
                                Spacer(modifier = Modifier.height(4.dp))
                                Row(
                                    verticalAlignment = Alignment.CenterVertically,
                                    horizontalArrangement = Arrangement.spacedBy(4.dp),
                                    modifier = Modifier
                                        .background(MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.2f), shape = RoundedCornerShape(6.dp))
                                        .padding(horizontal = 8.dp, vertical = 4.dp)
                                ) {
                                    Icon(Icons.Filled.Info, contentDescription = null, tint = MaterialTheme.colorScheme.primary, modifier = Modifier.size(12.dp))
                                    Text(
                                        text = "Not: ${item.note}",
                                        style = MaterialTheme.typography.labelSmall,
                                        color = MaterialTheme.colorScheme.onPrimaryContainer,
                                        maxLines = 2,
                                        overflow = androidx.compose.ui.text.style.TextOverflow.Ellipsis
                                    )
                                }
                            }
                        }

                        Row(
                            verticalAlignment = Alignment.CenterVertically,
                            horizontalArrangement = Arrangement.spacedBy(4.dp)
                        ) {
                            // Note Icon Button
                            IconButton(
                                onClick = {
                                    noteEditingItem = item
                                    editingNoteText = item.note
                                },
                                modifier = Modifier.size(32.dp)
                            ) {
                                Icon(
                                    imageVector = if (item.note.isNotEmpty()) Icons.Filled.Notes else Icons.Filled.Edit,
                                    contentDescription = "Not Ekle",
                                    tint = if (item.note.isNotEmpty()) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.outline,
                                    modifier = Modifier.size(18.dp)
                                )
                            }

                            // Minus Button
                            IconButton(
                                onClick = {
                                    val existing = AppDataStore.catalogCartItems.find { it.product.barcode == item.product.barcode }
                                    if (existing != null) {
                                        val idx = AppDataStore.catalogCartItems.indexOf(existing)
                                        if (existing.quantity > 1) {
                                            AppDataStore.catalogCartItems[idx] = existing.copy(quantity = existing.quantity - 1)
                                        } else {
                                            deleteConfirmationItem = item
                                        }
                                    }
                                },
                                modifier = Modifier.size(32.dp)
                            ) {
                                Icon(
                                    imageVector = Icons.Filled.RemoveCircleOutline,
                                    contentDescription = "Azalt",
                                    tint = MaterialTheme.colorScheme.primary,
                                    modifier = Modifier.size(18.dp)
                                )
                            }

                            // Keyboard editable quantity text field
                            var quantityText by remember(item.quantity) { mutableStateOf(item.quantity.toString()) }
                            androidx.compose.foundation.text.BasicTextField(
                                value = quantityText,
                                onValueChange = { newVal ->
                                    if (newVal.isEmpty() || newVal.all { it.isDigit() }) {
                                        quantityText = newVal
                                        val parsed = newVal.toIntOrNull() ?: 0
                                        if (parsed > 0) {
                                            val existing = AppDataStore.catalogCartItems.find { it.product.barcode == item.product.barcode }
                                            if (existing != null) {
                                                val idx = AppDataStore.catalogCartItems.indexOf(existing)
                                                if (idx != -1) {
                                                    AppDataStore.catalogCartItems[idx] = existing.copy(quantity = parsed)
                                                }
                                            }
                                        }
                                    }
                                },
                                textStyle = MaterialTheme.typography.bodyMedium.copy(
                                    textAlign = androidx.compose.ui.text.style.TextAlign.Center,
                                    fontWeight = FontWeight.Bold,
                                    color = MaterialTheme.colorScheme.onSurface
                                ),
                                keyboardOptions = androidx.compose.foundation.text.KeyboardOptions(keyboardType = androidx.compose.ui.text.input.KeyboardType.Number),
                                singleLine = true,
                                modifier = Modifier
                                    .width(44.dp)
                                    .background(MaterialTheme.colorScheme.surface, shape = RoundedCornerShape(4.dp))
                                    .border(1.dp, MaterialTheme.colorScheme.outlineVariant, shape = RoundedCornerShape(4.dp))
                                    .padding(vertical = 4.dp, horizontal = 2.dp)
                            )

                            // Plus Button
                            IconButton(
                                onClick = {
                                    val existing = AppDataStore.catalogCartItems.find { it.product.barcode == item.product.barcode }
                                    if (existing != null) {
                                        val idx = AppDataStore.catalogCartItems.indexOf(existing)
                                        AppDataStore.catalogCartItems[idx] = existing.copy(quantity = existing.quantity + 1)
                                    }
                                },
                                modifier = Modifier.size(32.dp)
                            ) {
                                Icon(
                                    imageVector = Icons.Filled.AddCircleOutline,
                                    contentDescription = "Arttır",
                                    tint = MaterialTheme.colorScheme.primary,
                                    modifier = Modifier.size(18.dp)
                                )
                            }

                            // Delete Button
                            IconButton(
                                onClick = {
                                    deleteConfirmationItem = item
                                },
                                modifier = Modifier.size(32.dp)
                            ) {
                                Icon(
                                    imageVector = Icons.Filled.Delete,
                                    contentDescription = "Sil",
                                    tint = MaterialTheme.colorScheme.error,
                                    modifier = Modifier.size(18.dp)
                                )
                            }
                        }
                    }
                }
            }
        }

        HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))

        // Total Row & Actions
        val totalPrice = AppDataStore.catalogCartItems.sumOf { item ->
            val p = item.product.getPriceForGroup(selectedPriceTier)
            p * item.quantity
        }

        Row(
            modifier = Modifier.fillMaxWidth().padding(vertical = 4.dp),
            horizontalArrangement = Arrangement.SpaceBetween,
            verticalAlignment = Alignment.CenterVertically
        ) {
            Text(
                text = "Toplam Tutar:",
                style = MaterialTheme.typography.bodyLarge,
                fontWeight = FontWeight.Bold,
                color = MaterialTheme.colorScheme.onSurface
            )
            Text(
                text = String.format("%,.2f ₺", totalPrice),
                style = MaterialTheme.typography.titleMedium,
                fontWeight = FontWeight.Bold,
                color = MaterialTheme.colorScheme.primary
            )
        }

        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.spacedBy(8.dp)
        ) {
            OutlinedButton(
                onClick = {
                    AppDataStore.catalogCartItems.clear()
                },
                modifier = Modifier.weight(1f),
                border = androidx.compose.foundation.BorderStroke(1.dp, MaterialTheme.colorScheme.error),
                colors = ButtonDefaults.outlinedButtonColors(contentColor = MaterialTheme.colorScheme.error)
            ) {
                Icon(Icons.Filled.Delete, contentDescription = null, modifier = Modifier.size(18.dp))
                Spacer(modifier = Modifier.width(4.dp))
                Text("Temizle", style = MaterialTheme.typography.bodySmall)
            }

            Button(
                onClick = {
                    val customer = AppDataStore.activeSelectedCustomer.value
                    if (customer == null) {
                        playFeedbackSuccess(false)
                        android.widget.Toast.makeText(context, "Hata: Müşteri seçimi yapmadan işlemi kapatamazsınız! Lütfen önce bir müşteri (cari) seçin.", android.widget.Toast.LENGTH_LONG).show()
                    } else {
                        // Gather notes
                        val notesText = AppDataStore.catalogCartItems
                            .filter { it.note.trim().isNotEmpty() }
                            .joinToString("; ") { "${it.product.title}: ${it.note}" }
                        val notesSuffix = if (notesText.isNotEmpty()) "\n[Notlar: $notesText]" else ""

                        if (AppDataStore.sendToApprovalCenterDirectly) {
                            val approvalTxId = "FT-" + (12400 + AppDataStore.kasaLogs.size)
                            val appItem = ApprovalItem(
                                id = approvalTxId,
                                type = "Satış",
                                customerName = customer.name,
                                description = "Katalog Sipariş Faturası - Tutar: ${String.format("%.2f ₺", totalPrice)} (${AppDataStore.catalogCartItems.size} kalem ürün)$notesSuffix",
                                amount = totalPrice,
                                time = "08.06.2026 18:00",
                                reason = "Katalog Siparişi Onay Talebi",
                                paymentType = "Cari Borç",
                                orderNote = notesText
                            )
                            AppDataStore.approvalOrderItemsMap[approvalTxId] = AppDataStore.catalogCartItems.toList()
                            AppDataStore.approvalItems.add(0, appItem)
                            playFeedbackSuccess(true)
                            AppDataStore.catalogCartItems.clear()
                            AppDataStore.persist(context)
                            android.widget.Toast.makeText(context, "Katalog sipariş kaydı onay merkezine başarıyla gönderildi.", android.widget.Toast.LENGTH_LONG).show()
                            onClose()
                        } else {
                            // 1. Deduct Inventory stock
                            AppDataStore.catalogCartItems.forEach { item ->
                                val currentStock = item.product.stockByWarehouse.toMutableMap()
                                val currentWarehouse = AppDataStore.activeSelectedWarehouse.value
                                val stockVal = currentStock[currentWarehouse] ?: 0
                                currentStock[currentWarehouse] = if (AppDataStore.allowNegativeStock) {
                                    stockVal - item.quantity
                                } else {
                                    maxOf(0, stockVal - item.quantity)
                                }
                                
                                val originalIndex = AppDataStore.products.indexOfFirst { it.barcode == item.product.barcode }
                                if (originalIndex != -1) {
                                    AppDataStore.products[originalIndex] = item.product.copy(
                                        stockByWarehouse = currentStock
                                    )
                                }
                                
                                // Add to salesHistory
                                AppDataStore.salesHistory.add(
                                    SalesRecord(
                                        customerId = customer.id,
                                        productBarcode = item.product.barcode,
                                        quantity = item.quantity,
                                        price = item.product.getPriceForGroup(selectedPriceTier),
                                        date = "08.06.2026"
                                    )
                                )
                            }
                            
                            // 2. Add to Customer's balance
                            val index = AppDataStore.customers.indexOfFirst { it.id == customer.id }
                            if (index != -1) {
                                val c = AppDataStore.customers[index]
                                c.balance += totalPrice
                                c.transactions.add(
                                    CustomerTx(
                                        id = "TX-" + (20000 + c.transactions.size),
                                        date = "08.06.2026",
                                        type = "SATIŞ",
                                        amount = totalPrice,
                                        description = "Katalog Sipariş Faturası No: FT-" + (12400 + AppDataStore.kasaLogs.size) + notesSuffix
                                    )
                                )
                            }
                            
                            // 3. KasaLog
                            AppDataStore.kasaLogs.add(
                                KasaLogItem(
                                    id = "K-" + (2000 + AppDataStore.kasaLogs.size),
                                    date = "08.06.2026 18:00",
                                    type = "Satış",
                                    customerOrSupplier = customer.name,
                                    amount = totalPrice,
                                    paymentType = "Nakit",
                                    bankName = null,
                                    desc = "Katalog Siparişi Fatura No: FT-" + (12400 + AppDataStore.kasaLogs.size) + notesSuffix
                                )
                            )
                            
                            playFeedbackSuccess(true)
                            AppDataStore.catalogCartItems.clear()
                            AppDataStore.persist(context)
                            android.widget.Toast.makeText(context, "Katalog siparişi başarıyla onaylandı ve satış kaydedildi.", android.widget.Toast.LENGTH_LONG).show()
                            onClose()
                        }
                    }
                },
                modifier = Modifier.weight(1f)
            ) {
                Icon(Icons.Filled.Check, contentDescription = null, modifier = Modifier.size(18.dp))
                Spacer(modifier = Modifier.width(4.dp))
                Text("Onayla", style = MaterialTheme.typography.bodySmall)
            }
        }
    }

    // Dialog components for the Cart itself logic:
    // 1. Delete confirmation dialog
    if (deleteConfirmationItem != null) {
        val itemToDelete = deleteConfirmationItem!!
        AlertDialog(
            onDismissRequest = { deleteConfirmationItem = null },
            title = { Text("Ürünü Sepetten Çıkar", fontWeight = FontWeight.Bold) },
            text = {
                Text(
                    text = "${itemToDelete.product.title} ürününü sepetinizden silmek istediğinize emin misiniz?",
                    style = MaterialTheme.typography.bodyMedium
                )
            },
            confirmButton = {
                Button(
                    onClick = {
                        AppDataStore.catalogCartItems.remove(itemToDelete)
                        deleteConfirmationItem = null
                    },
                    colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.error)
                ) {
                    Text("Evet, Sil")
                }
            },
            dismissButton = {
                TextButton(onClick = { deleteConfirmationItem = null }) {
                    Text("Vazgeç")
                }
            }
        )
    }

    // 2. Note editing dialog
    if (noteEditingItem != null) {
        val currentItem = noteEditingItem!!
        AlertDialog(
            onDismissRequest = { noteEditingItem = null },
            title = {
                Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    Icon(Icons.Filled.Notes, contentDescription = null, tint = MaterialTheme.colorScheme.primary)
                    Text("Ürün Notu Ekle", fontWeight = FontWeight.Bold)
                }
            },
            text = {
                Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                    Text(currentItem.product.title, fontWeight = FontWeight.SemiBold, style = MaterialTheme.typography.bodyMedium)
                    OutlinedTextField(
                        value = editingNoteText,
                        onValueChange = { editingNoteText = it },
                        label = { Text("Ürün Notu / Özel Talimat") },
                        placeholder = { Text("Hediye paketi, özel kesim, renk vb...") },
                        modifier = Modifier.fillMaxWidth(),
                        minLines = 3,
                        maxLines = 5
                    )
                }
            },
            confirmButton = {
                Button(
                    onClick = {
                        val idx = AppDataStore.catalogCartItems.indexOf(currentItem)
                        if (idx != -1) {
                            AppDataStore.catalogCartItems[idx] = currentItem.copy(note = editingNoteText)
                        }
                        noteEditingItem = null
                    }
                ) {
                    Text("Kaydet")
                }
            },
            dismissButton = {
                TextButton(onClick = { noteEditingItem = null }) {
                    Text("Vazgeç")
                }
            }
        )
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun CatalogFilterAndSortBar(selectedPriceTier: String) {
    val allBrands = remember(AppDataStore.products.size) { (AppDataStore.products.mapNotNull { it.brand }.distinct() + listOf("Belirtilmemiş")).sorted() }
    val allCats = remember(AppDataStore.products.size) { AppDataStore.products.map { it.category }.distinct().sorted() }
    val allAmbalajs = remember { listOf("Adet", "Koli", "Paket", "Çuval", "Kutu") }

    AdvancedFilterDialog(
        showDialog = AppDataStore.catalogShowFiltersDialog.value,
        onDismiss = { AppDataStore.catalogShowFiltersDialog.value = false },
        brands = allBrands,
        categories = allCats,
        ambalajs = allAmbalajs,
        selectedBrands = AppDataStore.catalogFilterBrands.value,
        onBrandsChange = { AppDataStore.catalogFilterBrands.value = it },
        selectedCategories = AppDataStore.catalogFilterCategories.value,
        onCategoriesChange = { AppDataStore.catalogFilterCategories.value = it },
        selectedAmbalajs = AppDataStore.catalogFilterAmbalajs.value,
        onAmbalajsChange = { AppDataStore.catalogFilterAmbalajs.value = it },
        minPrice = AppDataStore.catalogFilterMinPrice.value,
        onMinPriceChange = { AppDataStore.catalogFilterMinPrice.value = it },
        maxPrice = AppDataStore.catalogFilterMaxPrice.value,
        onMaxPriceChange = { AppDataStore.catalogFilterMaxPrice.value = it },
        minStock = AppDataStore.catalogFilterMinStock.value,
        onMinStockChange = { AppDataStore.catalogFilterMinStock.value = it },
        maxStock = AppDataStore.catalogFilterMaxStock.value,
        onMaxStockChange = { AppDataStore.catalogFilterMaxStock.value = it },
        hideNoPhoto = AppDataStore.catalogFilterHideNoPhoto.value,
        onHideNoPhotoChange = { AppDataStore.catalogFilterHideNoPhoto.value = it },
        hideOutOfStock = AppDataStore.catalogFilterHideOutOfStock.value,
        onHideOutOfStockChange = { AppDataStore.catalogFilterHideOutOfStock.value = it },
        onReset = {
            AppDataStore.catalogFilterBrands.value = emptySet()
            AppDataStore.catalogFilterCategories.value = emptySet()
            AppDataStore.catalogFilterAmbalajs.value = emptySet()
            AppDataStore.catalogFilterMinPrice.value = ""
            AppDataStore.catalogFilterMaxPrice.value = ""
            AppDataStore.catalogFilterMinStock.value = ""
            AppDataStore.catalogFilterMaxStock.value = ""
            AppDataStore.catalogFilterHideNoPhoto.value = false
            AppDataStore.catalogFilterHideOutOfStock.value = false
        }
    )
}

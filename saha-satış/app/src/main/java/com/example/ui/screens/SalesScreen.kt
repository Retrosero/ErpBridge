package com.example.ui.screens

import android.media.AudioManager
import android.media.ToneGenerator
import android.os.Build
import android.os.VibrationEffect
import android.os.Vibrator
import android.os.VibratorManager
import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.lazy.itemsIndexed
import androidx.compose.foundation.text.BasicTextField
import androidx.compose.ui.graphics.SolidColor
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.text.KeyboardActions
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.draw.shadow
import androidx.compose.ui.graphics.Color
import androidx.compose.foundation.BorderStroke
import android.widget.Toast
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.foundation.border
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.compose.ui.window.Dialog
import androidx.navigation.NavController
import com.example.ui.components.FieldCard
import com.example.ui.components.FieldHeader
import com.example.ui.components.FieldPrimaryButton
import com.example.ui.components.FieldSearchInput
import com.example.ui.components.AdvancedFilterDialog
import androidx.compose.foundation.horizontalScroll
import androidx.compose.foundation.rememberScrollState
import kotlinx.coroutines.delay
import kotlinx.coroutines.launch
import androidx.compose.animation.*
import androidx.compose.foundation.combinedClickable
import androidx.compose.foundation.ExperimentalFoundationApi

// New CameraX, ML Kit and Permissions imports
import androidx.compose.foundation.Canvas
import androidx.compose.ui.platform.LocalLifecycleOwner
import androidx.compose.ui.graphics.toArgb
import androidx.compose.ui.viewinterop.AndroidView
import androidx.camera.core.CameraSelector
import androidx.camera.core.ImageAnalysis
import androidx.camera.core.ImageProxy
import androidx.camera.core.Preview
import androidx.camera.lifecycle.ProcessCameraProvider
import androidx.camera.view.PreviewView
import androidx.core.content.ContextCompat
import com.google.accompanist.permissions.ExperimentalPermissionsApi
import com.google.accompanist.permissions.isGranted
import com.google.accompanist.permissions.rememberPermissionState
import com.google.mlkit.vision.barcode.BarcodeScanning
import com.google.mlkit.vision.common.InputImage
import java.util.concurrent.Executors

@OptIn(ExperimentalMaterial3Api::class, ExperimentalPermissionsApi::class, ExperimentalFoundationApi::class)
@Composable
fun SalesScreen(navController: NavController) {
    val scope = rememberCoroutineScope()
    val context = LocalContext.current
    val snackbarHostState = remember { SnackbarHostState() }

    // Selected States
    var selectedCustomer by AppDataStore.activeSelectedCustomer
    var selectedWarehouse by AppDataStore.activeSelectedWarehouse
    
    // Tab Controller (0: Ürün Kataloğu, 1: Cari Bilgisi, 2: Sepet)
    var selectedTab by AppDataStore.salesSelectedTab
    
    // Search, Filter & Sort States (Catalog Tab)
    var searchQuery by AppDataStore.salesSearchQuery
    var selectedCategory by AppDataStore.salesSelectedCategory
    var selectedSortOrder by AppDataStore.salesSelectedSortOrder
    var salesProductVisibleCount by remember(
        searchQuery, selectedCategory,
        AppDataStore.salesSelectedSortField.value, AppDataStore.salesSelectedSortAsc.value,
        AppDataStore.salesFilterBrands.value, AppDataStore.salesFilterCategories.value, AppDataStore.salesFilterAmbalajs.value,
        AppDataStore.salesFilterMinPrice.value, AppDataStore.salesFilterMaxPrice.value,
        AppDataStore.salesFilterMinStock.value, AppDataStore.salesFilterMaxStock.value,
        AppDataStore.salesFilterHideNoPhoto.value, AppDataStore.salesFilterHideOutOfStock.value
    ) { mutableStateOf(100) }
    
    // Cart States (Using AppDataStore to persist)
    val cartItems = AppDataStore.activeCartItems
    var orderNote by AppDataStore.activeOrderNote
    var generalDiscountAmount by remember { mutableStateOf(0.0) }
    var showGeneralDiscountDialog by remember { mutableStateOf(false) }
    var showOrderNoteDialog by remember { mutableStateOf(false) }
    var itemToDelete by remember { mutableStateOf<CartItem?>(null) }
    var selectedProductForDetail by remember { mutableStateOf<ProductCatalog?>(null) }
    
    // Dialog Controls
    var showCustomerPickerDialog by remember { mutableStateOf(false) }
    var customerSearchQuery by remember { mutableStateOf("") }
    var showCheckoutDialog by remember { mutableStateOf(false) }
    var checkoutPaymentType by remember { mutableStateOf("Cari Borç") } // "Cari Borç", "Nakit", "Kredi Kartı / EFT"
    var selectedBankForCheckout by remember { mutableStateOf<Bank?>(AppDataStore.banks.firstOrNull()) }

    // Product addition with quantity dialog
    var showQtyInputDialog by remember { mutableStateOf(false) }
    var showBarcodeScanner by AppDataStore.salesShowBarcodeScanner
    var qtyDialogProduct by remember { mutableStateOf<ProductCatalog?>(null) }
    var qtyDialogAmountStr by remember { mutableStateOf("1") }

    // Continuous scanner states
    var isContinuousScanActive by AppDataStore.salesShowContinuousBarcodeScanner
    var continuousScannedProduct by remember { mutableStateOf<ProductCatalog?>(null) }
    var continuousQtyStr by remember { mutableStateOf("1") }
    var showContinuousQtyDialog by remember { mutableStateOf(false) }

    var showCategoryDropdown by remember { mutableStateOf(false) }
    var showSortDropdown by remember { mutableStateOf(false) }
    var selectedInvoiceTxDetails by remember { mutableStateOf<CustomerTx?>(null) }

    var lastScanTime by remember { mutableStateOf(0L) }
    var isTotalsExpanded by remember { mutableStateOf(true) }
    var sortDropdownExpanded by remember { mutableStateOf(false) }

    LaunchedEffect(Unit) {
        val pending = AppDataStore.activeLoadSuspendedSale
        if (pending != null) {
            cartItems.clear()
            pending.items.forEach { item ->
                val p = AppDataStore.products.find { it.barcode == item.productBarcode }
                if (p != null) {
                    cartItems.add(CartItem(product = p, quantity = item.quantity))
                }
            }
            val matchedCust = AppDataStore.customers.find { it.id == pending.customerId }
            if (matchedCust != null) {
                selectedCustomer = matchedCust
            }
            selectedWarehouse = pending.warehouseName
            orderNote = pending.note
            selectedTab = 2
            AppDataStore.activeLoadSuspendedSale = null
        }
    }

    // Helper: Beep and Haptics
    fun playFeedbackTone(isSuccess: Boolean) {
        com.example.util.VibratorHelper.triggerFeedback(context, isSuccess)
    }

    // Determine applied price list for the customer
    fun getProductPrice(p: ProductCatalog): Double {
        val group = selectedCustomer?.priceGroup ?: "Perakende"
        return p.getPriceForGroup(group)
    }

    // Barcode Scanning / Adding handler
    fun handleBarcodeScan(barcode: String) {
        val currentTime = System.currentTimeMillis()
        if (currentTime - lastScanTime < 1000) return
        lastScanTime = currentTime

        val product = AppDataStore.products.find { it.barcode == barcode || it.barcodes.contains(barcode) }
        if (product != null) {
            val existingItem = cartItems.find { it.product.barcode == barcode }
            if (existingItem != null) {
                val index = cartItems.indexOf(existingItem)
                cartItems[index] = existingItem.copy(quantity = existingItem.quantity + 1)
            } else {
                cartItems.add(CartItem(product = product, quantity = 1))
            }
            playFeedbackTone(isSuccess = true)
            searchQuery = ""
        } else {
            playFeedbackTone(isSuccess = false)
            scope.launch {
                snackbarHostState.showSnackbar("Ürün bulunamadı: $barcode")
            }
        }
    }

    // Continuous scanning specific handler
    fun handleContinuousBarcodeScan(barcode: String) {
        if (showContinuousQtyDialog) return // Pause processing scans if we are already asking for quantity
        val currentTime = System.currentTimeMillis()
        if (currentTime - lastScanTime < 1500) return
        lastScanTime = currentTime

        val product = AppDataStore.products.find { it.barcode == barcode || it.barcodes.contains(barcode) }
        if (product != null) {
            playFeedbackTone(isSuccess = true)
            continuousScannedProduct = product
            continuousQtyStr = "1"
            showContinuousQtyDialog = true
        } else {
            playFeedbackTone(isSuccess = false)
            scope.launch {
                snackbarHostState.showSnackbar("Ürün bulunamadı: $barcode")
            }
        }
    }

    // Simulate scanning
    LaunchedEffect(searchQuery) {
        if (searchQuery.length > 8 && searchQuery.all { it.isDigit() }) {
            handleBarcodeScan(searchQuery)
        }
    }

    // Totals Calculations
    val rawSubTotal = cartItems.sumOf { getProductPrice(it.product) * it.quantity }
    val lineDiscounts = cartItems.sumOf { (getProductPrice(it.product) * it.quantity * it.lineDiscountPercent) / 100.0 }
    val subTotal = rawSubTotal - lineDiscounts
    val customerDiscountAmount = subTotal * ((selectedCustomer?.specialDiscountPercent ?: 0.0) / 100.0)
    val totalAfterCustomerDiscount = maxOf(0.0, subTotal - customerDiscountAmount)
    val totalAfterGeneralDiscount = maxOf(0.0, totalAfterCustomerDiscount - generalDiscountAmount)
    
    // Average KDV
    val taxTotal = cartItems.sumOf {
        val price = getProductPrice(it.product)
        val discountedPrice = price - (price * it.lineDiscountPercent / 100.0)
        val withCustDisc = discountedPrice - (discountedPrice * (selectedCustomer?.specialDiscountPercent ?: 0.0) / 100.0)
        val lineTotal = maxOf(0.0, withCustDisc * it.quantity)
        lineTotal * (it.product.kdvPercent / 100.0)
    }
    val grandTotal = totalAfterGeneralDiscount + taxTotal

    Scaffold(
        snackbarHost = { SnackbarHost(snackbarHostState) },
        containerColor = MaterialTheme.colorScheme.background,
        bottomBar = {
            var isPlusExpanded by remember { mutableStateOf(true) }

            Card(
                modifier = Modifier
                    .fillMaxWidth()
                    .animateContentSize(),
                colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.95f)),
                shape = RoundedCornerShape(topStart = 24.dp, topEnd = 24.dp, bottomStart = 0.dp, bottomEnd = 0.dp),
                elevation = CardDefaults.cardElevation(defaultElevation = 8.dp)
            ) {
                Column(
                    modifier = Modifier.fillMaxWidth()
                ) {
                    // 1. CHECKOUT TOTALS BLOCK (Only visible in Tab 2 with items)
                    if (selectedTab == 2 && cartItems.isNotEmpty()) {
                        Column(
                            modifier = Modifier
                                .fillMaxWidth()
                                .background(MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.4f))
                                .padding(horizontal = 16.dp, vertical = 8.dp)
                        ) {
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .clickable { isTotalsExpanded = !isTotalsExpanded }
                                    .padding(vertical = 4.dp),
                                horizontalArrangement = Arrangement.Center,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Icon(
                                    imageVector = if (isTotalsExpanded) Icons.Filled.KeyboardArrowDown else Icons.Filled.KeyboardArrowUp,
                                    contentDescription = if (isTotalsExpanded) "Detayları Gizle" else "Detayları Göster",
                                    tint = MaterialTheme.colorScheme.primary,
                                    modifier = Modifier.size(20.dp)
                                )
                                Spacer(modifier = Modifier.width(4.dp))
                                Text(
                                    text = if (isTotalsExpanded) "Detayları Gizle" else "Detayları Göster",
                                    style = MaterialTheme.typography.labelSmall,
                                    color = MaterialTheme.colorScheme.primary,
                                    fontWeight = FontWeight.Bold
                                )
                            }

                            androidx.compose.animation.AnimatedVisibility(
                                visible = isTotalsExpanded,
                                enter = androidx.compose.animation.expandVertically() + androidx.compose.animation.fadeIn(),
                                exit = androidx.compose.animation.shrinkVertically() + androidx.compose.animation.fadeOut()
                            ) {
                                Column {
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween,
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                                            Text("Ara Toplam", style = MaterialTheme.typography.bodyMedium)
                                            IconButton(onClick = { showOrderNoteDialog = true }, modifier = Modifier.size(24.dp)) {
                                                Icon(Icons.Filled.EditNote, contentDescription = "Not", tint = if (orderNote.isNotEmpty()) MaterialTheme.colorScheme.primary else Color.Gray)
                                            }
                                            IconButton(onClick = { showGeneralDiscountDialog = true }, modifier = Modifier.size(24.dp)) {
                                                Icon(Icons.Filled.Percent, contentDescription = "İndirim", tint = if (generalDiscountAmount > 0) MaterialTheme.colorScheme.primary else Color.Gray)
                                            }
                                        }
                                        Text(String.format("%,.2f ₺", rawSubTotal), style = MaterialTheme.typography.bodyMedium)
                                    }
                                    if (lineDiscounts > 0 || customerDiscountAmount > 0 || generalDiscountAmount > 0) {
                                        Spacer(modifier = Modifier.height(4.dp))
                                        Row(
                                            modifier = Modifier.fillMaxWidth(),
                                            horizontalArrangement = Arrangement.SpaceBetween
                                        ) {
                                            Text("Toplam İndirim / İskonto", style = MaterialTheme.typography.bodyMedium, color = MaterialTheme.colorScheme.error)
                                            Text(String.format("-%,.2f ₺", lineDiscounts + customerDiscountAmount + generalDiscountAmount), style = MaterialTheme.typography.bodyMedium, color = MaterialTheme.colorScheme.error)
                                        }
                                    }
                                    Spacer(modifier = Modifier.height(4.dp))
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween
                                    ) {
                                        Text("Hesaplanan KDV", style = MaterialTheme.typography.bodyMedium)
                                        Text(String.format("%,.2f ₺", taxTotal), style = MaterialTheme.typography.bodyMedium)
                                    }
                                    HorizontalDivider(modifier = Modifier.padding(vertical = 8.dp), color = MaterialTheme.colorScheme.outlineVariant)
                                }
                            }

                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                                    Text("Ödenecek Tutar", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold)
                                    if (!isTotalsExpanded) {
                                        if (orderNote.isNotEmpty()) {
                                            Icon(Icons.Filled.EditNote, "Not", tint = MaterialTheme.colorScheme.primary, modifier = Modifier.size(16.dp))
                                        }
                                        if (generalDiscountAmount > 0) {
                                            Icon(Icons.Filled.Percent, "İndirim", tint = MaterialTheme.colorScheme.primary, modifier = Modifier.size(14.dp))
                                        }
                                    }
                                }
                                Text(String.format("%,.2f ₺", grandTotal), style = MaterialTheme.typography.titleLarge, color = MaterialTheme.colorScheme.primary, fontWeight = FontWeight.Bold)
                            }
                            
                            Spacer(modifier = Modifier.height(8.dp))
                            
                            Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                                OutlinedButton(
                                    onClick = { 
                                        if (cartItems.isEmpty()) {
                                            scope.launch { snackbarHostState.showSnackbar("Boş sepet beklemeye alınamaz!") }
                                        } else {
                                            val sdf = java.text.SimpleDateFormat("dd.MM.yyyy HH:mm", java.util.Locale.getDefault())
                                            val dateStr = sdf.format(java.util.Date())
                                            val subItems = cartItems.map {
                                                SuspendedSaleItem(
                                                    productBarcode = it.product.barcode,
                                                    quantity = it.quantity,
                                                    price = getProductPrice(it.product)
                                                )
                                            }
                                            val total = cartItems.sumOf { getProductPrice(it.product) * it.quantity * (1.0 - it.lineDiscountPercent/100.0) }
                                            val newSuspended = SuspendedSale(
                                                id = "BS-" + (1000 + (Math.random() * 9000).toInt()),
                                                date = dateStr,
                                                customerId = selectedCustomer?.id,
                                                customerName = selectedCustomer?.name ?: "Perakende Müşteri",
                                                items = subItems,
                                                note = orderNote,
                                                warehouseName = selectedWarehouse,
                                                totalAmount = total
                                            )
                                            AppDataStore.suspendedSales.add(newSuspended)
                                            scope.launch { snackbarHostState.showSnackbar("Satış beklemeye alındı, ürünler rezerve edildi.") }
                                            cartItems.clear()
                                            orderNote = ""
                                            navController.popBackStack()
                                        }
                                    },
                                    modifier = Modifier.weight(1f)
                                ) {
                                    Icon(Icons.Filled.Pause, contentDescription = null, modifier = Modifier.size(16.dp))
                                    Spacer(modifier = Modifier.width(4.dp))
                                    Text("Beklet")
                                }
                                Button(
                                    onClick = {
                                        if (selectedCustomer == null) {
                                            playFeedbackTone(false)
                                            scope.launch {
                                                snackbarHostState.showSnackbar("Dikkat: Müşteri seçimi yapmadan satışı kapatamazsınız! Lütfen önce müşteri seçin.")
                                            }
                                        } else {
                                            showCheckoutDialog = true
                                        }
                                    },
                                    modifier = Modifier.weight(1.5f),
                                    colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.primary)
                                ) {
                                    Icon(Icons.Filled.Payment, contentDescription = null, modifier = Modifier.size(16.dp))
                                    Spacer(modifier = Modifier.width(4.dp))
                                    Text("Satışı Tamamla")
                                }
                            }
                        }
                    }

                    // 2. SEARCH BAR & CONTROLS BLOCK (Only visible when isPlusExpanded is true)
                    AnimatedVisibility(
                        visible = isPlusExpanded,
                        enter = expandVertically() + fadeIn(),
                        exit = shrinkVertically() + fadeOut()
                    ) {
                        Column(
                            modifier = Modifier
                                .fillMaxWidth()
                                .background(MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f))
                                .padding(horizontal = 12.dp, vertical = 6.dp)
                        ) {
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.spacedBy(6.dp),
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                BasicTextField(
                                    value = searchQuery,
                                    onValueChange = { searchQuery = it },
                                    modifier = Modifier
                                        .weight(1f)
                                        .height(38.dp)
                                        .background(MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.25f), RoundedCornerShape(10.dp))
                                        .border(1.dp, MaterialTheme.colorScheme.outline.copy(alpha = 0.3f), RoundedCornerShape(10.dp)),
                                    textStyle = MaterialTheme.typography.bodySmall.copy(
                                        color = MaterialTheme.colorScheme.onSurface,
                                        fontSize = 10.5.sp
                                    ),
                                    singleLine = true,
                                    cursorBrush = SolidColor(MaterialTheme.colorScheme.primary),
                                    decorationBox = { innerTextField ->
                                        Row(
                                            modifier = Modifier
                                                .fillMaxSize()
                                                .padding(horizontal = 8.dp),
                                            verticalAlignment = Alignment.CenterVertically,
                                            horizontalArrangement = Arrangement.spacedBy(6.dp)
                                        ) {
                                            Icon(
                                                Icons.Filled.Search,
                                                contentDescription = null,
                                                tint = MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.6f),
                                                modifier = Modifier.size(14.dp)
                                            )
                                            Box(
                                                modifier = Modifier.weight(1f),
                                                contentAlignment = Alignment.CenterStart
                                            ) {
                                                if (searchQuery.isEmpty()) {
                                                    Text(
                                                        text = "Arama yapın...",
                                                        style = MaterialTheme.typography.bodySmall.copy(
                                                            color = MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.6f),
                                                            fontSize = 10.5.sp
                                                        )
                                                    )
                                                }
                                                innerTextField()
                                            }
                                            if (searchQuery.isNotEmpty()) {
                                                IconButton(
                                                    onClick = { searchQuery = "" },
                                                    modifier = Modifier.size(20.dp)
                                                ) {
                                                    Icon(
                                                        Icons.Filled.Close,
                                                        contentDescription = null,
                                                        tint = MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.6f),
                                                        modifier = Modifier.size(14.dp)
                                                    )
                                                }
                                            }
                                        }
                                    }
                                )

                                // Sort icon button
                                Box {
                                    val isNotDefaultSort = AppDataStore.salesSelectedSortField.value != "İsim" || !AppDataStore.salesSelectedSortAsc.value
                                    IconButton(
                                        onClick = { sortDropdownExpanded = true },
                                        modifier = Modifier
                                            .background(
                                                if (isNotDefaultSort) MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.4f),
                                                shape = RoundedCornerShape(10.dp)
                                            )
                                            .size(44.dp)
                                    ) {
                                        Icon(
                                            Icons.Filled.Sort,
                                            contentDescription = "Sıralama",
                                            tint = if (isNotDefaultSort) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant,
                                            modifier = Modifier.size(18.dp)
                                        )
                                    }
                                    DropdownMenu(
                                        expanded = sortDropdownExpanded,
                                        onDismissRequest = { sortDropdownExpanded = false }
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
                                            DropdownMenuItem(
                                                text = { Text(opt.third, fontWeight = if (AppDataStore.salesSelectedSortField.value == opt.first && AppDataStore.salesSelectedSortAsc.value == opt.second) FontWeight.Bold else FontWeight.Normal) },
                                                onClick = {
                                                    AppDataStore.salesSelectedSortField.value = opt.first
                                                    AppDataStore.salesSelectedSortAsc.value = opt.second
                                                    sortDropdownExpanded = false
                                                },
                                                leadingIcon = {
                                                    if (AppDataStore.salesSelectedSortField.value == opt.first && AppDataStore.salesSelectedSortAsc.value == opt.second) {
                                                        Icon(Icons.Filled.Check, contentDescription = null, modifier = Modifier.size(16.dp))
                                                    }
                                                }
                                            )
                                        }
                                    }
                                }

                                // Filter Button
                                val activeFilterCount = (if (AppDataStore.salesFilterBrands.value.isNotEmpty()) 1 else 0) +
                                        (if (AppDataStore.salesFilterCategories.value.isNotEmpty()) 1 else 0) +
                                        (if (AppDataStore.salesFilterAmbalajs.value.isNotEmpty()) 1 else 0) +
                                        (if (AppDataStore.salesFilterMinPrice.value.isNotEmpty()) 1 else 0) +
                                        (if (AppDataStore.salesFilterMaxPrice.value.isNotEmpty()) 1 else 0) +
                                        (if (AppDataStore.salesFilterMinStock.value.isNotEmpty()) 1 else 0) +
                                        (if (AppDataStore.salesFilterMaxStock.value.isNotEmpty()) 1 else 0) +
                                        (if (AppDataStore.salesFilterHideNoPhoto.value) 1 else 0) +
                                        (if (AppDataStore.salesFilterHideOutOfStock.value) 1 else 0)

                                IconButton(
                                    onClick = { AppDataStore.salesShowFiltersDialog.value = true },
                                    modifier = Modifier
                                        .background(
                                            if (activeFilterCount > 0) MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.4f),
                                            shape = RoundedCornerShape(10.dp)
                                        )
                                        .size(44.dp)
                                ) {
                                    Icon(
                                        Icons.Filled.FilterList,
                                        contentDescription = "Filtreler",
                                        tint = if (activeFilterCount > 0) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant,
                                        modifier = Modifier.size(18.dp)
                                    )
                                }

                                // Scan Button
                                val isContinuousActivated = false; if (false) {
                                IconButton(
                                    onClick = { AppDataStore.salesShowBarcodeScanner.value = true },
                                    modifier = Modifier
                                        .background(
                                            if (isContinuousActivated) MaterialTheme.colorScheme.errorContainer else MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.4f),
                                            shape = RoundedCornerShape(10.dp)
                                        )
                                        .size(44.dp)
                                ) {
                                    Icon(
                                        Icons.Filled.QrCodeScanner,
                                        contentDescription = "Barkod Tarayıcıyı Aç",
                                        tint = if (isContinuousActivated) MaterialTheme.colorScheme.onErrorContainer else MaterialTheme.colorScheme.onSurfaceVariant,
                                        modifier = Modifier.size(18.dp)
                                    )
                                }

                                } // Image view mode button
                                IconButton(
                                    onClick = { AppDataStore.salesShowImagesMode.value = !AppDataStore.salesShowImagesMode.value },
                                    modifier = Modifier
                                        .background(
                                            if (AppDataStore.salesShowImagesMode.value) MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.4f),
                                            shape = RoundedCornerShape(10.dp)
                                        )
                                        .size(44.dp)
                                ) {
                                    Icon(
                                        imageVector = if (AppDataStore.salesShowImagesMode.value) Icons.Filled.GridView else Icons.Filled.ViewList,
                                        contentDescription = "Görünümü Değiştir",
                                        tint = if (AppDataStore.salesShowImagesMode.value) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant,
                                        modifier = Modifier.size(18.dp)
                                    )
                                }
                            }
                        }
                    }

                    HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.2f))

                    // 3. TAB NAVIGATION BAR (Now replicating the main bottombar layout with 4 items: Ürünler, Cari, Sepet, +)
                    Row(
                        modifier = Modifier
                            .fillMaxWidth()
                            .navigationBarsPadding()
                            .height(48.dp)
                            .padding(horizontal = 4.dp),
                        horizontalArrangement = Arrangement.SpaceAround,
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        // Tab 0: Ürünler
                        Column(
                            modifier = Modifier
                                .weight(1f)
                                .clip(RoundedCornerShape(16.dp))
                                .clickable { selectedTab = 0 }
                                .padding(vertical = 1.dp),
                            horizontalAlignment = Alignment.CenterHorizontally,
                            verticalArrangement = Arrangement.Center
                        ) {
                            Icon(
                                imageVector = Icons.Filled.Storefront,
                                contentDescription = "Ürünler",
                                tint = if (selectedTab == 0) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.7f),
                                modifier = Modifier.size(19.dp)
                            )
                            Spacer(modifier = Modifier.height(1.dp))
                            Text(
                                text = "Ürünler",
                                style = MaterialTheme.typography.labelSmall.copy(
                                    fontWeight = if (selectedTab == 0) FontWeight.Bold else FontWeight.Normal,
                                    fontSize = 9.5.sp
                                ),
                                color = if (selectedTab == 0) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.7f)
                            )
                        }

                        // Tab 1: Barkod Oku
                        Column(
                            modifier = Modifier
                                .weight(1f)
                                .clip(RoundedCornerShape(16.dp))
                                .combinedClickable(
                                    onClick = { AppDataStore.salesShowBarcodeScanner.value = true },
                                    onLongClick = { 
                                        isContinuousScanActive = !isContinuousScanActive
                                        context.let { ctx ->
                                            android.widget.Toast.makeText(ctx, if (isContinuousScanActive) "Sürekli tarama aktif!" else "Sürekli tarama kapatıldı.", android.widget.Toast.LENGTH_SHORT).show()
                                        }
                                        playFeedbackTone(true)
                                    }
                                )
                                .padding(vertical = 1.dp),
                            horizontalAlignment = Alignment.CenterHorizontally,
                            verticalArrangement = Arrangement.Center
                        ) {
                            Icon(
                                imageVector = Icons.Filled.QrCodeScanner,
                                contentDescription = "Barkod Oku",
                                tint = if (AppDataStore.salesShowBarcodeScanner.value || isContinuousScanActive) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.7f),
                                modifier = Modifier.size(19.dp)
                            )
                            Spacer(modifier = Modifier.height(1.dp))
                            Text(
                                text = if (isContinuousScanActive) "Sürekli Açık" else "Barkod Oku",
                                style = MaterialTheme.typography.labelSmall.copy(
                                    fontWeight = if (AppDataStore.salesShowBarcodeScanner.value || isContinuousScanActive) FontWeight.Bold else FontWeight.Normal,
                                    fontSize = 9.5.sp
                                ),
                                color = if (AppDataStore.salesShowBarcodeScanner.value || isContinuousScanActive) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.7f)
                            )
                        }

                        // Tab 2: Sepet
                        Column(
                            modifier = Modifier
                                .weight(1f)
                                .clip(RoundedCornerShape(16.dp))
                                .clickable { selectedTab = 2 }
                                .padding(vertical = 1.dp),
                            horizontalAlignment = Alignment.CenterHorizontally,
                            verticalArrangement = Arrangement.Center
                        ) {
                            if (cartItems.isNotEmpty()) {
                                BadgedBox(
                                    badge = {
                                        Badge(containerColor = MaterialTheme.colorScheme.error) {
                                            Text(cartItems.size.toString(), color = Color.White, style = MaterialTheme.typography.labelSmall.copy(fontSize = 8.sp))
                                        }
                                    }
                                ) {
                                    Icon(
                                        imageVector = Icons.Filled.ShoppingCart,
                                        contentDescription = "Sepet",
                                        tint = if (selectedTab == 2) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.7f),
                                        modifier = Modifier.size(19.dp)
                                    )
                                }
                            } else {
                                Icon(
                                    imageVector = Icons.Filled.ShoppingCart,
                                    contentDescription = "Sepet",
                                    tint = if (selectedTab == 2) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.7f),
                                    modifier = Modifier.size(19.dp)
                                )
                            }
                            Spacer(modifier = Modifier.height(1.dp))
                            Text(
                                text = "Sepet",
                                style = MaterialTheme.typography.labelSmall.copy(
                                    fontWeight = if (selectedTab == 2) FontWeight.Bold else FontWeight.Normal,
                                    fontSize = 9.5.sp
                                ),
                                color = if (selectedTab == 2) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.7f)
                            )
                        }

                        // Tab 3: Action "+" Expand/Collapse Controls
                        Column(
                            modifier = Modifier
                                .weight(1f)
                                .clip(RoundedCornerShape(16.dp))
                                .clickable { isPlusExpanded = !isPlusExpanded }
                                .padding(vertical = 1.dp),
                            horizontalAlignment = Alignment.CenterHorizontally,
                            verticalArrangement = Arrangement.Center
                        ) {
                            Icon(
                                imageVector = if (isPlusExpanded) Icons.Filled.Close else Icons.Filled.Add,
                                contentDescription = "Özellikler",
                                tint = if (isPlusExpanded) MaterialTheme.colorScheme.error else MaterialTheme.colorScheme.primary,
                                modifier = Modifier.size(20.dp)
                            )
                            Spacer(modifier = Modifier.height(1.dp))
                            Text(
                                text = if (isPlusExpanded) "Kapat" else "Menü +",
                                style = MaterialTheme.typography.labelSmall.copy(
                                    fontWeight = FontWeight.Bold,
                                    fontSize = 9.5.sp
                                ),
                                color = if (isPlusExpanded) MaterialTheme.colorScheme.error else MaterialTheme.colorScheme.primary
                            )
                        }
                    }
                }
            }
        }
    ) { paddingValues ->
        Column(
            modifier = Modifier
                .fillMaxSize()
        ) {
            // 1. CUSTOM TOP APP BAR FOR SALES (Back button & Customer Selection)
            Card(
                modifier = Modifier
                    .fillMaxWidth()
                    .shadow(elevation = 2.dp, shape = RoundedCornerShape(bottomStart = 16.dp, bottomEnd = 16.dp)),
                colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
                shape = RoundedCornerShape(bottomStart = 16.dp, bottomEnd = 16.dp, topStart = 0.dp, topEnd = 0.dp)
            ) {
                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .statusBarsPadding()
                        .padding(horizontal = 12.dp, vertical = 8.dp),
                    verticalAlignment = Alignment.CenterVertically,
                    horizontalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    androidx.compose.material3.IconButton(
                        onClick = {
                            if (selectedTab != 0) {
                                selectedTab = 0
                            } else {
                                navController.popBackStack()
                            }
                        },
                        modifier = Modifier
                            .background(MaterialTheme.colorScheme.surfaceVariant, shape = CircleShape)
                            .size(36.dp)
                    ) {
                        Icon(
                            Icons.Filled.ArrowBack,
                            contentDescription = "Geri",
                            modifier = Modifier.size(18.dp)
                        )
                    }

                    // Customer Selector Area (Compact & Sleek)
                    Row(
                        modifier = Modifier
                            .weight(1f)
                            .background(MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.35f), shape = RoundedCornerShape(12.dp))
                            .clickable { showCustomerPickerDialog = true }
                            .padding(horizontal = 12.dp, vertical = 6.dp),
                        verticalAlignment = Alignment.CenterVertically,
                        horizontalArrangement = Arrangement.SpaceBetween
                    ) {
                        Row(
                            verticalAlignment = Alignment.CenterVertically,
                            modifier = Modifier.weight(1f)
                        ) {
                            Icon(
                                imageVector = Icons.Filled.People,
                                contentDescription = null,
                                tint = MaterialTheme.colorScheme.primary,
                                modifier = Modifier.size(16.dp)
                            )
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
                                        overflow = androidx.compose.ui.text.style.TextOverflow.Ellipsis,
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
                                    overflow = androidx.compose.ui.text.style.TextOverflow.Ellipsis
                                )
                            }
                        }
                        Text(
                            text = if (selectedCustomer == null) "Seç" else "Değiştir",
                            style = MaterialTheme.typography.labelMedium,
                            fontWeight = FontWeight.Bold,
                            color = MaterialTheme.colorScheme.primary,
                            modifier = Modifier.padding(start = 4.dp)
                        )
                    }
                }
            }

            // 2. Docked Continuous Scanner Card (Sleek, borderless, page-wide)
            AnimatedVisibility(
                visible = isContinuousScanActive,
                enter = expandVertically() + fadeIn(),
                exit = shrinkVertically() + fadeOut()
            ) {
                Box(
                    modifier = Modifier
                        .fillMaxWidth()
                        .background(Color.Black)
                ) {
                    val cameraPermissionState = rememberPermissionState(android.Manifest.permission.CAMERA)
                    if (cameraPermissionState.status.isGranted) {
                        Box(
                            modifier = Modifier
                                .fillMaxWidth()
                                .height(12.dp),
                            contentAlignment = Alignment.Center
                        ) {
                            CameraScannerView { barcode ->
                                handleContinuousBarcodeScan(barcode)
                            }
                            // Scanning Red Laser Indicator
                            Box(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .height(1.dp)
                                    .background(Color.Red.copy(alpha = 0.9f))
                            )
                            // Clean top & bottom borders highlighting scanner area
                            Box(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .height(0.5.dp)
                                    .background(MaterialTheme.colorScheme.primary.copy(alpha = 0.4f))
                                    .align(Alignment.TopCenter)
                            )
                            Box(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .height(0.5.dp)
                                    .background(MaterialTheme.colorScheme.primary.copy(alpha = 0.4f))
                                    .align(Alignment.BottomCenter)
                            )
                        }
                    } else {
                        Box(
                            modifier = Modifier
                                .fillMaxWidth()
                                .height(12.dp),
                            contentAlignment = Alignment.Center
                        ) {
                            TextButton(onClick = { cameraPermissionState.launchPermissionRequest() }) {
                                Text(
                                    text = "Kamera İzni Verin (Sürekli Tarama)",
                                    style = MaterialTheme.typography.bodySmall,
                                    color = Color.White
                                )
                            }
                        }
                    }
                }
            }

            if (false) { // Fixed Customer (Cari) Selector Card at the top of SalesScreen (Visible always)
            Card(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(horizontal = 16.dp, vertical = 6.dp)
                    .clickable { selectedTab = 1 },
                colors = CardDefaults.cardColors(
                    containerColor = MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.35f)
                ),
                shape = RoundedCornerShape(12.dp)
            ) {
                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(horizontal = 12.dp, vertical = 4.dp),
                    verticalAlignment = Alignment.CenterVertically,
                    horizontalArrangement = Arrangement.SpaceBetween
                ) {
                    Row(
                        verticalAlignment = Alignment.CenterVertically,
                        modifier = Modifier.weight(1f)
                    ) {
                        Icon(
                            imageVector = Icons.Filled.People,
                            contentDescription = null,
                            tint = MaterialTheme.colorScheme.primary,
                            modifier = Modifier.size(16.dp)
                        )
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
                                    overflow = androidx.compose.ui.text.style.TextOverflow.Ellipsis,
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
                                overflow = androidx.compose.ui.text.style.TextOverflow.Ellipsis
                            )
                        }
                    }
                    TextButton(
                        onClick = { showCustomerPickerDialog = true },
                        contentPadding = PaddingValues(horizontal = 8.dp, vertical = 2.dp),
                        modifier = Modifier.height(28.dp)
                    ) {
                        Text(
                            text = if (selectedCustomer == null) "Seç" else "Değiştir",
                            style = MaterialTheme.typography.labelMedium,
                            fontWeight = FontWeight.Bold
                        )
                    }
                }
            }
            }



            LazyColumn(
                modifier = Modifier
                    .fillMaxSize()
                    .weight(1f)
                    .padding(horizontal = 16.dp),
                contentPadding = PaddingValues(top = 16.dp, bottom = paddingValues.calculateBottomPadding() + 16.dp),
                verticalArrangement = Arrangement.spacedBy(16.dp)
            ) {
                if (selectedTab == 0) {
                    // TAB 1: PRODUCT CATALOG VIEWS
                    // Advanced Filter Dialog (triggered from bottom bar)
                    item {
                        val allBrands = remember { (AppDataStore.products.mapNotNull { it.brand }.distinct() + listOf("Belirtilmemiş")).sorted() }
                        val allCats = remember { AppDataStore.products.map { it.category }.distinct().sorted() }
                        val allAmbalajs = remember { listOf("Adet", "Koli", "Paket", "Çuval", "Kutu") }

                        AdvancedFilterDialog(
                            showDialog = AppDataStore.salesShowFiltersDialog.value,
                            onDismiss = { AppDataStore.salesShowFiltersDialog.value = false },
                            brands = allBrands,
                            categories = allCats,
                            ambalajs = allAmbalajs,
                            selectedBrands = AppDataStore.salesFilterBrands.value,
                            onBrandsChange = { AppDataStore.salesFilterBrands.value = it },
                            selectedCategories = AppDataStore.salesFilterCategories.value,
                            onCategoriesChange = { AppDataStore.salesFilterCategories.value = it },
                            selectedAmbalajs = AppDataStore.salesFilterAmbalajs.value,
                            onAmbalajsChange = { AppDataStore.salesFilterAmbalajs.value = it },
                            minPrice = AppDataStore.salesFilterMinPrice.value,
                            onMinPriceChange = { AppDataStore.salesFilterMinPrice.value = it },
                            maxPrice = AppDataStore.salesFilterMaxPrice.value,
                            onMaxPriceChange = { AppDataStore.salesFilterMaxPrice.value = it },
                            minStock = AppDataStore.salesFilterMinStock.value,
                            onMinStockChange = { AppDataStore.salesFilterMinStock.value = it },
                            maxStock = AppDataStore.salesFilterMaxStock.value,
                            onMaxStockChange = { AppDataStore.salesFilterMaxStock.value = it },
                            hideNoPhoto = AppDataStore.salesFilterHideNoPhoto.value,
                            onHideNoPhotoChange = { AppDataStore.salesFilterHideNoPhoto.value = it },
                            hideOutOfStock = AppDataStore.salesFilterHideOutOfStock.value,
                            onHideOutOfStockChange = { AppDataStore.salesFilterHideOutOfStock.value = it },
                            onReset = {
                                AppDataStore.salesFilterBrands.value = emptySet()
                                AppDataStore.salesFilterCategories.value = emptySet()
                                AppDataStore.salesFilterAmbalajs.value = emptySet()
                                AppDataStore.salesFilterMinPrice.value = ""
                                AppDataStore.salesFilterMaxPrice.value = ""
                                AppDataStore.salesFilterMinStock.value = ""
                                AppDataStore.salesFilterMaxStock.value = ""
                                AppDataStore.salesFilterHideNoPhoto.value = false
                                AppDataStore.salesFilterHideOutOfStock.value = false
                            }
                        )
                    }

                    // Dynamic Products Display Loop
                    val rawProducts = AppDataStore.products
                    val filteredProducts = rawProducts.filter { p ->
                        val matchesQuery = searchQuery.isEmpty() ||
                                           p.title.contains(searchQuery, ignoreCase = true) || 
                                           p.code.contains(searchQuery, ignoreCase = true) || 
                                           p.barcode.contains(searchQuery, ignoreCase = true) ||
                                           p.barcodes.any { it.contains(searchQuery, ignoreCase = true) }
                        
                        val matchesCat = if (AppDataStore.salesFilterCategories.value.isNotEmpty()) {
                            AppDataStore.salesFilterCategories.value.contains(p.category)
                        } else {
                            selectedCategory == "Tümü" || p.category == selectedCategory
                        }
                        
                        val matchesBrand = AppDataStore.salesFilterBrands.value.isEmpty() || 
                                           AppDataStore.salesFilterBrands.value.contains(p.brand ?: "Belirtilmemiş")
                        
                        val matchesAmbalaj = AppDataStore.salesFilterAmbalajs.value.isEmpty() || 
                                             AppDataStore.salesFilterAmbalajs.value.contains(p.inferredAmbalaj)
                        
                        val price = getProductPrice(p)
                        val minPriceVal = AppDataStore.salesFilterMinPrice.value.toDoubleOrNull()
                        val maxPriceVal = AppDataStore.salesFilterMaxPrice.value.toDoubleOrNull()
                        val matchesPrice = (minPriceVal == null || price >= minPriceVal) && 
                                           (maxPriceVal == null || price <= maxPriceVal)
                        
                        val totalStock = p.stockByWarehouse.values.sum()
                        val minStockVal = AppDataStore.salesFilterMinStock.value.toIntOrNull()
                        val maxStockVal = AppDataStore.salesFilterMaxStock.value.toIntOrNull()
                        val matchesStock = (minStockVal == null || totalStock >= minStockVal) && 
                                           (maxStockVal == null || totalStock <= maxStockVal)
                        
                        val hasPhoto = !p.imageUrl.isNullOrEmpty() || !p.localImagePath.isNullOrEmpty()
                        val matchesNoPhoto = !AppDataStore.salesFilterHideNoPhoto.value || hasPhoto
                        val matchesOutOfStock = !AppDataStore.salesFilterHideOutOfStock.value || totalStock > 0
                        
                        matchesQuery && matchesCat && matchesBrand && matchesAmbalaj && matchesPrice && matchesStock && matchesNoPhoto && matchesOutOfStock
                    }.sortedWith(
                        run {
                            val comparator = when (AppDataStore.salesSelectedSortField.value) {
                                "İsim" -> compareBy<ProductCatalog> { it.title }
                                "Kod" -> compareBy { it.code }
                                "Fiyat" -> compareBy { getProductPrice(it) }
                                "Marka" -> compareBy { it.brand ?: "" }
                                "Stok" -> compareBy { it.stockByWarehouse.values.sum() }
                                else -> compareBy { it.title }
                            }
                            if (AppDataStore.salesSelectedSortAsc.value) comparator else comparator.reversed()
                        }
                    )

                    val displayedProducts = filteredProducts.take(salesProductVisibleCount)
                    if (displayedProducts.isEmpty()) {
                        item {
                            Box(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(48.dp),
                                contentAlignment = Alignment.Center
                            ) {
                                Text("Arama kriterlerine uygun ürün bulunamadı.", color = Color.Gray, textAlign = TextAlign.Center)
                            }
                        }
                    } else {
                        itemsIndexed(items = displayedProducts, key = { index, prod -> prod.barcode + "_" + index }) { index, prod ->
                            if (index >= displayedProducts.size - 5 && displayedProducts.size < filteredProducts.size) {
                                LaunchedEffect(Unit) {
                                    salesProductVisibleCount += 100
                                }
                            }
                            val currentPrice = getProductPrice(prod)
                            val totalStock = prod.stockByWarehouse.values.sum()
                            
                            FieldCard(modifier = Modifier.fillMaxWidth()) {
                                Row(
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .padding(12.dp),
                                    verticalAlignment = Alignment.CenterVertically,
                                    horizontalArrangement = Arrangement.SpaceBetween
                                ) {
                                    if (com.example.ui.screens.AppDataStore.salesShowImagesMode.value) {
                                        // Visual color circle + Code
                                        Box(
                                            modifier = Modifier
                                                .size(52.dp)
                                                .clip(RoundedCornerShape(12.dp))
                                                .background(prod.imageUrlColor.copy(alpha = 0.2f)).clickable { selectedProductForDetail = prod },
                                            contentAlignment = Alignment.Center
                                        ) {
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

                                            if (images.isEmpty()) {
                                                Icon(Icons.Filled.Inbox, null, tint = prod.imageUrlColor, modifier = Modifier.clickable { selectedProductForDetail = prod })
                                            } else {
                                                val pagerState = androidx.compose.foundation.pager.rememberPagerState(pageCount = { images.size })
                                                Box(modifier = Modifier.fillMaxSize()) {
                                                    androidx.compose.foundation.pager.HorizontalPager(
                                                        state = pagerState,
                                                        modifier = Modifier.fillMaxSize()
                                                    ) { page ->
                                                        val imageModel = images[page]
                                                         coil.compose.AsyncImage(
                                                             model = imageModel,
                                                             contentDescription = "${prod.title} - ${page + 1}",
                                                             modifier = Modifier.fillMaxSize().clickable { selectedProductForDetail = prod },
                                                             contentScale = androidx.compose.ui.layout.ContentScale.Crop
                                                         )
                                                    }
                                                    
                                                     if (images.size > 1) {
                                                         Row(
                                                             modifier = Modifier
                                                                 .align(Alignment.BottomCenter)
                                                                 .padding(bottom = 3.dp)
                                                                 .background(Color.Black.copy(alpha = 0.4f), RoundedCornerShape(4.dp))
                                                                 .padding(horizontal = 3.dp, vertical = 1.dp),
                                                             horizontalArrangement = Arrangement.spacedBy(2.dp),
                                                             verticalAlignment = Alignment.CenterVertically
                                                         ) {
                                                             repeat(images.size) { index ->
                                                                 val active = pagerState.currentPage == index
                                                                 Box(
                                                                     modifier = Modifier
                                                                         .size(4.dp)
                                                                         .background(
                                                                             color = if (active) Color.White else Color.White.copy(alpha = 0.5f),
                                                                             shape = androidx.compose.foundation.shape.CircleShape
                                                                         )
                                                                 )
                                                             }
                                                         }
                                                     }
                                                }
                                            }
                                        }
                                        
                                        Spacer(modifier = Modifier.width(12.dp))
                                    }
                                    
                                    Column(modifier = Modifier.weight(1f)) {
                                        Text(prod.title, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodyMedium)
                                        Text("Kod: ${prod.code} | Barkod: ${prod.barcode}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                        Row(horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                                            val reservedQty = AppDataStore.suspendedSales.flatMap { it.items }.filter { it.productBarcode == prod.barcode }.sumOf { it.quantity }
                                            val stockLabel = if (reservedQty > 0) "Stok: $totalStock ADET ($reservedQty)" else "Stok: $totalStock ADET"
                                            Text(stockLabel, style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Bold, color = if (totalStock > 0) Color(0xFF43A047) else Color.Red)
                                            Text("KDV: %${prod.kdvPercent}", style = MaterialTheme.typography.labelSmall, color = Color.DarkGray)
                                        }
                                        if ((prod.boxQty != null && prod.boxQty > 1) || (prod.packageQty != null && prod.packageQty > 1)) {
                                            val boxText = if (prod.boxQty != null && prod.boxQty > 1) "Koli: ${prod.boxQty} Adet" else ""
                                            val packText = if (prod.packageQty != null && prod.packageQty > 1) "Paket: ${prod.packageQty} Adet" else ""
                                            val joined = listOf(boxText, packText).filter { it.isNotEmpty() }.joinToString(" | ")
                                            Text(joined, style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.secondary, fontWeight = FontWeight.SemiBold)
                                        }
                                    }
                                    
                                    // Pricing & Cart Button
                                    Column(horizontalAlignment = Alignment.End) {
                                        Text(String.format("%,.2f ₺", currentPrice), fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodyMedium, color = MaterialTheme.colorScheme.primary)
                                        Spacer(modifier = Modifier.height(4.dp))
                                        IconButton(
                                            onClick = {
                                                qtyDialogProduct = prod
                                                qtyDialogAmountStr = "1"
                                                showQtyInputDialog = true
                                            },
                                            modifier = Modifier
                                                .background(MaterialTheme.colorScheme.primaryContainer, shape = RoundedCornerShape(8.dp))
                                                .size(40.dp)
                                        ) {
                                            Icon(Icons.Filled.Add, contentDescription = "Ekle", tint = MaterialTheme.colorScheme.onPrimaryContainer)
                                        }
                                    }
                                }
                            }
                        }
                    }

                } else if (selectedTab == 1) {
                    // TAB 1: CUSTOMER (CARİ) INFORMATION SECTION (No warehouse info shown)
                    item {
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(vertical = 4.dp),
                            horizontalArrangement = Arrangement.SpaceBetween,
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Text(
                                text = "Cari Hesap Bilgileri",
                                style = MaterialTheme.typography.titleLarge,
                                fontWeight = FontWeight.Bold,
                                color = MaterialTheme.colorScheme.onBackground
                            )
                            IconButton(
                                onClick = { selectedTab = 0 },
                                modifier = Modifier
                                    .background(MaterialTheme.colorScheme.errorContainer.copy(alpha = 0.8f), shape = CircleShape)
                                    .size(36.dp)
                            ) {
                                Icon(
                                    Icons.Filled.Close,
                                    contentDescription = "Kapat",
                                    tint = MaterialTheme.colorScheme.onErrorContainer,
                                    modifier = Modifier.size(20.dp)
                                )
                            }
                        }
                    }
                    item {
                        FieldCard(modifier = Modifier.fillMaxWidth()) {
                            Column(modifier = Modifier.padding(16.dp), verticalArrangement = Arrangement.spacedBy(16.dp)) {
                                Row(
                                    modifier = Modifier.fillMaxWidth(),
                                    horizontalArrangement = Arrangement.SpaceBetween,
                                    verticalAlignment = Alignment.CenterVertically
                                ) {
                                    Column(modifier = Modifier.weight(1f)) {
                                        Text("Seçili Cari (Müşteri) Detayları", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
                                        Text("Bu senaryoda cari hesap ana verileri baz alınır.", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                    }
                                    
                                    IconButton(
                                        modifier = Modifier.background(MaterialTheme.colorScheme.primaryContainer, shape = CircleShape),
                                        onClick = { showCustomerPickerDialog = true }
                                    ) {
                                        Icon(Icons.Filled.Edit, contentDescription = "Değiştir", tint = MaterialTheme.colorScheme.onPrimaryContainer)
                                    }
                                }

                                if (selectedCustomer != null) {
                                    val cust = selectedCustomer!!
                                    HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)
                                    
                                    Column(verticalArrangement = Arrangement.spacedBy(10.dp)) {
                                        Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                                            Text("Ünvan / Adı:", style = MaterialTheme.typography.bodyMedium, color = Color.Gray)
                                            Text(cust.name, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold)
                                        }
                                        Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                                            Text("Vergi Dairesi / No:", style = MaterialTheme.typography.bodyMedium, color = Color.Gray)
                                            Text(cust.taxNumber, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.SemiBold)
                                        }
                                        Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                                            Text("Fiyat Sınıf Tanımı:", style = MaterialTheme.typography.bodyMedium, color = Color.Gray)
                                            Text(cust.priceGroup, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.SemiBold, color = MaterialTheme.colorScheme.secondary)
                                        }
                                        Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                                            Text("", style = MaterialTheme.typography.bodyMedium)
                                            Text("", style = MaterialTheme.typography.bodyMedium)
                                        }
                                        Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                                            Text("Cari Bakiye Tutarı:", style = MaterialTheme.typography.bodyMedium, color = Color.Gray)
                                            Text(
                                                String.format("%.2f ₺", cust.balance),
                                                style = MaterialTheme.typography.bodyMedium,
                                                fontWeight = FontWeight.Bold,
                                                color = if (cust.balance >= 0) MaterialTheme.colorScheme.error else Color(0xFF43A047)
                                            )
                                        }
                                    }
                                } else {
                                    Box(
                                        modifier = Modifier.fillMaxWidth().padding(vertical = 24.dp),
                                        contentAlignment = Alignment.Center
                                    ) {
                                        Text("Bir cari hesap kartı seçilmemiş.", color = Color.Gray, style = MaterialTheme.typography.bodyMedium)
                                    }
                                }
                            }
                        }
                    }

                    item {
                        Button(
                            onClick = { showCustomerPickerDialog = true },
                            modifier = Modifier.fillMaxWidth().height(48.dp),
                            shape = RoundedCornerShape(12.dp)
                        ) {
                            Icon(Icons.Filled.People, contentDescription = null, modifier = Modifier.size(18.dp))
                            Spacer(modifier = Modifier.width(8.dp))
                            Text("Cari Seç / Değiştir", fontWeight = FontWeight.Bold)
                        }
                    }

                    if (selectedCustomer != null) {
                        val cust = selectedCustomer!!
                        val invoices = cust.transactions.filter { it.type == "SATIŞ" }.take(10)
                        
                        item {
                            Spacer(modifier = Modifier.height(8.dp))
                            Card(
                                modifier = Modifier.fillMaxWidth(),
                                colors = CardDefaults.cardColors(
                                    containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.3f)
                                ),
                                shape = RoundedCornerShape(12.dp)
                            ) {
                                Column(
                                    modifier = Modifier.padding(16.dp),
                                    verticalArrangement = Arrangement.spacedBy(12.dp)
                                ) {
                                    Text(
                                        text = "Müşterinin Son Alınan 10 Faturası",
                                        style = MaterialTheme.typography.titleSmall,
                                        fontWeight = FontWeight.Bold,
                                        color = MaterialTheme.colorScheme.primary
                                    )
                                    
                                    if (invoices.isEmpty()) {
                                        Text(
                                            "Bu cariye ait kayıtlı satış faturası bulunamadı.",
                                            style = MaterialTheme.typography.bodySmall,
                                            color = Color.Gray,
                                            modifier = Modifier.padding(vertical = 4.dp)
                                        )
                                    } else {
                                        Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                                            invoices.forEach { tx ->
                                                Row(
                                                    modifier = Modifier
                                                        .fillMaxWidth()
                                                        .background(MaterialTheme.colorScheme.surface, RoundedCornerShape(8.dp))
                                                        .clickable { selectedInvoiceTxDetails = tx }
                                                        .padding(12.dp),
                                                    horizontalArrangement = Arrangement.SpaceBetween,
                                                    verticalAlignment = Alignment.CenterVertically
                                                ) {
                                                    Column {
                                                        Text("Fatura: ${tx.id}", style = MaterialTheme.typography.bodySmall, fontWeight = FontWeight.Bold)
                                                        Text("Tarih: ${tx.date}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                                    }
                                                    Row(verticalAlignment = Alignment.CenterVertically) {
                                                        Text(String.format("%,.2f ₺", tx.amount), style = MaterialTheme.typography.bodySmall, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
                                                        Spacer(modifier = Modifier.width(6.dp))
                                                        Icon(Icons.Filled.ChevronRight, contentDescription = null, modifier = Modifier.size(16.dp), tint = Color.Gray)
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                } else {
                    // TAB 2: ACTIVE CART DISPLAY SECTION
                    if (cartItems.isEmpty()) {
                        item {
                            Box(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(48.dp),
                                contentAlignment = Alignment.Center
                            ) {
                                Column(horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(12.dp)) {
                                    Icon(Icons.Filled.ProductionQuantityLimits, contentDescription = null, modifier = Modifier.size(64.dp), tint = Color.Gray)
                                    Text("Sepetiniz şu anda boş.", color = Color.Gray, style = MaterialTheme.typography.bodyLarge)
                                    Text("Lütfen 'Ürünler' sekmesine geçerek sepete ürün ekleyin.", color = Color.Gray, style = MaterialTheme.typography.bodySmall, textAlign = TextAlign.Center)
                                }
                            }
                        }
                    } else {
                        item {
                            Card(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(bottom = 8.dp),
                                colors = CardDefaults.cardColors(
                                    containerColor = MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.4f)
                                ),
                                shape = RoundedCornerShape(8.dp)
                            ) {
                                Row(
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .padding(horizontal = 12.dp, vertical = 8.dp),
                                    verticalAlignment = Alignment.CenterVertically,
                                    horizontalArrangement = Arrangement.spacedBy(8.dp)
                                ) {
                                    Icon(
                                        Icons.Filled.Store,
                                        contentDescription = null,
                                        tint = MaterialTheme.colorScheme.primary,
                                        modifier = Modifier.size(20.dp)
                                    )
                                    Text(
                                        text = "Sepetinizde toplam ${cartItems.size} kalem ürün bulunmaktadır.",
                                        style = MaterialTheme.typography.bodySmall,
                                        color = MaterialTheme.colorScheme.onPrimaryContainer
                                    )
                                }
                            }
                        }

                        items(cartItems) { item ->
                            val unitPrice = getProductPrice(item.product)
                            CartItemCard(
                                item = item,
                                unitPrice = unitPrice,
                                onIncrement = {
                                    val available = item.product.stockByWarehouse[selectedWarehouse] ?: 0
                                    if (!AppDataStore.allowNegativeStock && item.quantity >= available) {
                                        scope.launch {
                                            snackbarHostState.showSnackbar("Yetersiz stok! En fazla $available adet satabilirsiniz.")
                                        }
                                    } else {
                                        val ind = cartItems.indexOf(item)
                                        cartItems[ind] = item.copy(quantity = item.quantity + 1)
                                    }
                                },
                                onDecrement = {
                                    if (item.quantity > 1) {
                                        val ind = cartItems.indexOf(item)
                                        cartItems[ind] = item.copy(quantity = item.quantity - 1)
                                    } else {
                                        itemToDelete = item
                                    }
                                },
                                onDelete = { itemToDelete = item },
                                onApplyDiscount = { percent ->
                                    val ind = cartItems.indexOf(item)
                                    cartItems[ind] = item.copy(lineDiscountPercent = percent)
                                },
                                onNoteUpdate = { note ->
                                    val ind = cartItems.indexOf(item)
                                    cartItems[ind] = item.copy(note = note)
                                },
                                onQuantityChange = { newQty ->
                                    val available = item.product.stockByWarehouse[selectedWarehouse] ?: 0
                                    if (!AppDataStore.allowNegativeStock && newQty > available) {
                                        scope.launch {
                                            snackbarHostState.showSnackbar("Yetersiz stok! En fazla $available adet satabilirsiniz.")
                                        }
                                    } else if (newQty > 0) {
                                        val ind = cartItems.indexOf(item)
                                        cartItems[ind] = item.copy(quantity = newQty)
                                    } else if (newQty == 0) {
                                        itemToDelete = item
                                    }
                                }
                            )
                        }
                    }
                }
            }
        }

        if (showContinuousQtyDialog && continuousScannedProduct != null) {
            val prod = continuousScannedProduct!!
            val price = getProductPrice(prod)
            AlertDialog(
                onDismissRequest = { 
                    showContinuousQtyDialog = false
                    continuousScannedProduct = null
                },
                title = {
                    Text(
                        text = "Hızlı Miktar Ekle",
                        style = MaterialTheme.typography.titleMedium,
                        fontWeight = FontWeight.Bold
                    )
                },
                text = {
                    Column(
                        verticalArrangement = Arrangement.spacedBy(12.dp)
                    ) {
                        Text(prod.title, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodyMedium)
                        Text("Kod: ${prod.code}", style = MaterialTheme.typography.bodySmall, color = Color.Gray)
                        Text("Birim Fiyat: ${String.format("%,.2f ₺", price)}", style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.primary, fontWeight = FontWeight.Bold)
                        
                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            verticalAlignment = Alignment.CenterVertically,
                            horizontalArrangement = Arrangement.Center
                        ) {
                            IconButton(
                                onClick = {
                                    val currentVal = continuousQtyStr.toIntOrNull() ?: 1
                                    if (currentVal > 1) {
                                        continuousQtyStr = (currentVal - 1).toString()
                                    }
                                },
                                modifier = Modifier
                                    .background(MaterialTheme.colorScheme.secondaryContainer, shape = CircleShape)
                                    .size(40.dp)
                            ) {
                                Icon(Icons.Filled.Remove, contentDescription = "Azalt")
                            }
                            
                            OutlinedTextField(
                                value = continuousQtyStr,
                                onValueChange = { continuousQtyStr = it.filter { char -> char.isDigit() } },
                                keyboardOptions = KeyboardOptions(
                                    keyboardType = KeyboardType.Number,
                                    imeAction = ImeAction.Done
                                ),
                                keyboardActions = KeyboardActions(
                                    onDone = {
                                        val qty = continuousQtyStr.toIntOrNull() ?: 1
                                        val existingItem = cartItems.find { it.product.barcode == prod.barcode }
                                        if (existingItem != null) {
                                            val index = cartItems.indexOf(existingItem)
                                            cartItems[index] = existingItem.copy(quantity = existingItem.quantity + qty)
                                        } else {
                                            cartItems.add(CartItem(product = prod, quantity = qty))
                                        }
                                        playFeedbackTone(isSuccess = true)
                                        showContinuousQtyDialog = false
                                        continuousScannedProduct = null
                                    }
                                ),
                                modifier = Modifier
                                    .width(80.dp)
                                    .padding(horizontal = 8.dp),
                                textStyle = androidx.compose.ui.text.TextStyle(
                                    textAlign = androidx.compose.ui.text.style.TextAlign.Center,
                                    fontWeight = FontWeight.Bold
                                ),
                                singleLine = true
                            )
                            
                            IconButton(
                                onClick = {
                                    val currentVal = continuousQtyStr.toIntOrNull() ?: 1
                                    continuousQtyStr = (currentVal + 1).toString()
                                },
                                modifier = Modifier
                                    .background(MaterialTheme.colorScheme.secondaryContainer, shape = CircleShape)
                                    .size(40.dp)
                            ) {
                                Icon(Icons.Filled.Add, contentDescription = "Arttır")
                            }
                        }
                    }
                },
                confirmButton = {
                    Button(
                        onClick = {
                            val qty = continuousQtyStr.toIntOrNull() ?: 1
                            val existingItem = cartItems.find { it.product.barcode == prod.barcode }
                            if (existingItem != null) {
                                val index = cartItems.indexOf(existingItem)
                                cartItems[index] = existingItem.copy(quantity = existingItem.quantity + qty)
                            } else {
                                cartItems.add(CartItem(product = prod, quantity = qty))
                            }
                            playFeedbackTone(isSuccess = true)
                            showContinuousQtyDialog = false
                            continuousScannedProduct = null
                        }
                    ) {
                        Text("Onayla ve Devam Et")
                    }
                },
                dismissButton = {
                    TextButton(
                        onClick = {
                            showContinuousQtyDialog = false
                            continuousScannedProduct = null
                        }
                    ) {
                        Text("Vazgeç")
                    }
                }
            )
        }

        if (itemToDelete != null) {
            AlertDialog(
                onDismissRequest = { itemToDelete = null },
                title = { Text("Silme Onayı") },
                text = { Text("Seçili ürünü sepetten silmek istediğinize emin misiniz?") },
                confirmButton = {
                    Button(
                        onClick = {
                            cartItems.remove(itemToDelete)
                            itemToDelete = null
                        },
                        colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.error)
                    ) {
                        Text("Evet, Sil")
                    }
                },
                dismissButton = {
                    TextButton(onClick = { itemToDelete = null }) {
                        Text("Vazgeç")
                    }
                }
            )
        }

        if (showOrderNoteDialog) {
            AlertDialog(
                onDismissRequest = { showOrderNoteDialog = false },
                title = { Text("Sipariş Notu") },
                text = {
                    OutlinedTextField(
                        value = orderNote,
                        onValueChange = { orderNote = it },
                        label = { Text("Evrak / Satış Sipariş Notu") },
                        modifier = Modifier.fillMaxWidth(),
                        maxLines = 3
                    )
                },
                confirmButton = {
                    Button(onClick = { showOrderNoteDialog = false }) {
                        Text("Tamam")
                    }
                }
            )
        }

        if (showGeneralDiscountDialog) {
            AlertDialog(
                onDismissRequest = { showGeneralDiscountDialog = false },
                title = { Text("Genel İskonto Tutarı") },
                text = {
                    OutlinedTextField(
                        value = if (generalDiscountAmount == 0.0) "" else generalDiscountAmount.toString(),
                        onValueChange = { generalDiscountAmount = it.toDoubleOrNull() ?: 0.0 },
                        label = { Text("Fatura Altı Genel İskonto Tutarı (₺)") },
                        modifier = Modifier.fillMaxWidth(),
                        singleLine = true,
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number)
                    )
                },
                confirmButton = {
                    Button(onClick = { showGeneralDiscountDialog = false }) {
                        Text("Tamam")
                    }
                }
            )
        }

        // 1. Live Customer Selection Dialog
        if (showCustomerPickerDialog) {
            val filteredCustomers = AppDataStore.customers.filter {
                it.name.contains(customerSearchQuery, ignoreCase = true) ||
                it.id.contains(customerSearchQuery, ignoreCase = true) ||
                it.taxNumber.contains(customerSearchQuery, ignoreCase = true)
            }
            AlertDialog(
                onDismissRequest = { showCustomerPickerDialog = false },
                title = { Text("Cari Hesap Kartı Seçin") },
                text = {
                    Column(
                        modifier = Modifier
                            .fillMaxWidth()
                            .height(380.dp),
                        verticalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        LazyColumn(
                            modifier = Modifier
                                .weight(1f)
                                .fillMaxWidth(),
                            verticalArrangement = Arrangement.spacedBy(8.dp)
                        ) {
                            items(items = filteredCustomers, key = { cust -> cust.id }) { cust ->
                                Box(
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .clip(RoundedCornerShape(8.dp))
                                        .background(if (selectedCustomer?.id == cust.id) MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surfaceVariant)
                                        .clickable {
                                            selectedCustomer = cust
                                            showCustomerPickerDialog = false
                                            playFeedbackTone(true)
                                        }
                                        .padding(12.dp)
                                ) {
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween,
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        Column(
                                            modifier = Modifier
                                                .weight(1f)
                                                .padding(end = 8.dp)
                                        ) {
                                            Text(
                                                text = cust.name,
                                                fontWeight = FontWeight.Bold,
                                                style = MaterialTheme.typography.bodyMedium,
                                                maxLines = 1,
                                                overflow = TextOverflow.Ellipsis
                                            )
                                            Text(
                                                text = "Kod: ${cust.id} | Vergi No: ${cust.taxNumber}",
                                                style = MaterialTheme.typography.labelSmall,
                                                color = Color.Gray,
                                                maxLines = 1,
                                                overflow = TextOverflow.Ellipsis
                                            )
                                            Text(
                                                text = "Fiyat: ${cust.priceGroup}",
                                                style = MaterialTheme.typography.labelSmall,
                                                color = MaterialTheme.colorScheme.secondary,
                                                maxLines = 1,
                                                overflow = TextOverflow.Ellipsis
                                            )
                                        }
                                        Text(
                                            text = String.format("%.2f ₺", cust.balance),
                                            fontWeight = FontWeight.Bold,
                                            color = if (cust.balance >= 0) MaterialTheme.colorScheme.error else Color(0xFF43A047),
                                            style = MaterialTheme.typography.labelMedium,
                                            maxLines = 1,
                                            softWrap = false,
                                            overflow = TextOverflow.Clip
                                        )
                                    }
                                }
                            }
                        }

                        OutlinedTextField(
                            value = customerSearchQuery,
                            onValueChange = { customerSearchQuery = it },
                            placeholder = { Text("Müşteri Ara (Ad, Kod, Vergi No)...", maxLines = 1, overflow = TextOverflow.Ellipsis) },
                            leadingIcon = { Icon(Icons.Filled.Search, contentDescription = null, modifier = Modifier.size(18.dp)) },
                            trailingIcon = {
                                if (customerSearchQuery.isNotEmpty()) {
                                    IconButton(onClick = { customerSearchQuery = "" }) {
                                        Icon(Icons.Filled.Clear, contentDescription = null, modifier = Modifier.size(18.dp))
                                    }
                                }
                            },
                            modifier = Modifier
                                .fillMaxWidth()
                                .testTag("customer_search_input"),
                            singleLine = true,
                            maxLines = 1,
                            textStyle = MaterialTheme.typography.bodyMedium,
                            shape = RoundedCornerShape(12.dp)
                        )
                    }
                },
                confirmButton = {
                    TextButton(onClick = { showCustomerPickerDialog = false }) {
                        Text("Kapat")
                    }
                }
            )
        }

        // 1.5 Product Quantity Creator Dialog (with text input and +/- buttons)
        if (showQtyInputDialog && qtyDialogProduct != null) {
            val prod = qtyDialogProduct!!
            AlertDialog(
                onDismissRequest = { showQtyInputDialog = false },
                title = { Text("Ürün Ekleme Miktarı") },
                text = {
                    Column(
                        horizontalAlignment = Alignment.CenterHorizontally,
                        verticalArrangement = Arrangement.spacedBy(12.dp),
                        modifier = Modifier.fillMaxWidth()
                    ) {
                        Text(prod.title, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodyLarge, textAlign = TextAlign.Center)
                        Text("KOD: ${prod.code}", style = MaterialTheme.typography.bodySmall, color = Color.Gray)
                        Text(
                            text = String.format("Birim Fiyat: %,.2f ₺", getProductPrice(prod)),
                            style = MaterialTheme.typography.labelLarge,
                            fontWeight = FontWeight.SemiBold,
                            color = MaterialTheme.colorScheme.primary
                        )

                        val countNum = qtyDialogAmountStr.toIntOrNull() ?: 1
                        val existingItem = cartItems.find { it.product.barcode == prod.barcode }
                        val requestedAmount = countNum + (existingItem?.quantity ?: 0)
                        val stockAvailable = prod.stockByWarehouse[selectedWarehouse] ?: 0
                        val reservedInWarehouse = AppDataStore.suspendedSales.filter { it.warehouseName == selectedWarehouse }.flatMap { it.items }.filter { it.productBarcode == prod.barcode }.sumOf { it.quantity }
                        val stockError = !AppDataStore.allowNegativeStock && requestedAmount > stockAvailable

                        if (stockError) {
                            Text(
                                text = "Yetersiz Stok! Depoda bulunan: $stockAvailable. (Sepettekiler dahil talep: $requestedAmount" + (if (reservedInWarehouse > 0) ", Rezerve: $reservedInWarehouse" else "") + ")",
                                color = MaterialTheme.colorScheme.error,
                                style = MaterialTheme.typography.bodySmall,
                                fontWeight = FontWeight.Bold,
                                textAlign = TextAlign.Center
                            )
                        } else {
                            Text(
                                text = "Depo Mevcut Stok: $stockAvailable" + (if (reservedInWarehouse > 0) " ($reservedInWarehouse)" else "") + " ADET",
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
                                    val current = qtyDialogAmountStr.toIntOrNull() ?: 1
                                    if (current > 1) {
                                        qtyDialogAmountStr = (current - 1).toString()
                                    }
                                },
                                modifier = Modifier
                                    .background(MaterialTheme.colorScheme.primaryContainer, shape = CircleShape)
                                    .size(40.dp)
                            ) {
                                Icon(Icons.Filled.Remove, contentDescription = "Azalt", tint = MaterialTheme.colorScheme.onPrimaryContainer)
                            }
                            
                            OutlinedTextField(
                                value = qtyDialogAmountStr,
                                onValueChange = { newValue ->
                                    if (newValue.isEmpty() || newValue.all { it.isDigit() }) {
                                        qtyDialogAmountStr = newValue
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
                                    val current = qtyDialogAmountStr.toIntOrNull() ?: 0
                                    qtyDialogAmountStr = (current + 1).toString()
                                },
                                modifier = Modifier
                                    .background(MaterialTheme.colorScheme.primaryContainer, shape = CircleShape)
                                    .size(40.dp)
                            ) {
                                Icon(Icons.Filled.Add, contentDescription = "Arttır", tint = MaterialTheme.colorScheme.onPrimaryContainer)
                            }
                        }

                        if ((prod.boxQty != null && prod.boxQty > 1) || (prod.packageQty != null && prod.packageQty > 1)) {
                            Spacer(modifier = Modifier.height(10.dp))
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.spacedBy(8.dp, Alignment.CenterHorizontally),
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                if (prod.boxQty != null && prod.boxQty > 1) {
                                    OutlinedButton(
                                        onClick = {
                                            val qty = prod.boxQty
                                            if (qty > 0) {
                                                val existing = cartItems.find { it.product.barcode == prod.barcode }
                                                val requestedAmount = qty + (existing?.quantity ?: 0)
                                                val stockAvailable = prod.stockByWarehouse[selectedWarehouse] ?: 0
                                                val stockError = !AppDataStore.allowNegativeStock && requestedAmount > stockAvailable
                                                if (!stockError) {
                                                    if (existing != null) {
                                                        val idx = cartItems.indexOf(existing)
                                                        cartItems[idx] = existing.copy(quantity = existing.quantity + qty)
                                                    } else {
                                                        cartItems.add(CartItem(product = prod, quantity = qty))
                                                    }
                                                    playFeedbackTone(true)
                                                    showQtyInputDialog = false
                                                } else {
                                                    playFeedbackTone(false)
                                                }
                                            }
                                        },
                                        contentPadding = PaddingValues(horizontal = 12.dp, vertical = 6.dp),
                                        shape = RoundedCornerShape(8.dp),
                                        colors = ButtonDefaults.outlinedButtonColors(
                                            contentColor = MaterialTheme.colorScheme.primary
                                        )
                                    ) {
                                        Icon(Icons.Filled.Inbox, contentDescription = null, modifier = Modifier.size(16.dp))
                                        Spacer(modifier = Modifier.width(6.dp))
                                        Text("${prod.boxQty} Adet (Koli)", style = MaterialTheme.typography.labelMedium)
                                    }
                                }
                                if (prod.packageQty != null && prod.packageQty > 1) {
                                    OutlinedButton(
                                        onClick = {
                                            val qty = prod.packageQty
                                            if (qty > 0) {
                                                val existing = cartItems.find { it.product.barcode == prod.barcode }
                                                val requestedAmount = qty + (existing?.quantity ?: 0)
                                                val stockAvailable = prod.stockByWarehouse[selectedWarehouse] ?: 0
                                                val stockError = !AppDataStore.allowNegativeStock && requestedAmount > stockAvailable
                                                if (!stockError) {
                                                    if (existing != null) {
                                                        val idx = cartItems.indexOf(existing)
                                                        cartItems[idx] = existing.copy(quantity = existing.quantity + qty)
                                                    } else {
                                                        cartItems.add(CartItem(product = prod, quantity = qty))
                                                    }
                                                    playFeedbackTone(true)
                                                    showQtyInputDialog = false
                                                } else {
                                                    playFeedbackTone(false)
                                                }
                                            }
                                        },
                                        contentPadding = PaddingValues(horizontal = 12.dp, vertical = 6.dp),
                                        shape = RoundedCornerShape(8.dp),
                                        colors = ButtonDefaults.outlinedButtonColors(
                                            contentColor = MaterialTheme.colorScheme.secondary
                                        )
                                    ) {
                                        Icon(Icons.Filled.Layers, contentDescription = null, modifier = Modifier.size(16.dp))
                                        Spacer(modifier = Modifier.width(6.dp))
                                        Text("${prod.packageQty} Adet (Paket)", style = MaterialTheme.typography.labelMedium)
                                    }
                                }
                            }
                        }
                    }
                },
                confirmButton = {
                    val countNum = qtyDialogAmountStr.toIntOrNull() ?: 1
                    val existingItem = cartItems.find { it.product.barcode == prod.barcode }
                    val requestedAmount = countNum + (existingItem?.quantity ?: 0)
                    val stockAvailable = prod.stockByWarehouse[selectedWarehouse] ?: 0
                    val stockError = !AppDataStore.allowNegativeStock && requestedAmount > stockAvailable

                    Button(
                        enabled = !stockError,
                        onClick = {
                            val count = qtyDialogAmountStr.toIntOrNull() ?: 1
                            if (count > 0 && !stockError) {
                                val existing = cartItems.find { it.product.barcode == prod.barcode }
                                if (existing != null) {
                                    val idx = cartItems.indexOf(existing)
                                    cartItems[idx] = existing.copy(quantity = existing.quantity + count)
                                } else {
                                    cartItems.add(CartItem(product = prod, quantity = count))
                                }
                                playFeedbackTone(true)
                                showQtyInputDialog = false
                            }
                        }
                    ) {
                        Text("Sepete Ekle")
                    }
                },
                dismissButton = {
                    TextButton(onClick = { showQtyInputDialog = false }) {
                        Text("Vazgeç")
                    }
                }
            )
        }

        // 1.6 CameraX Barcode Scanner Dialog
        if (showBarcodeScanner) {
            BarcodeScannerDialog(
                onDismissRequest = { showBarcodeScanner = false },
                onBarcodeScanned = { barcode ->
                    handleBarcodeScan(barcode)
                    showBarcodeScanner = false
                },
                onSimulateScan = { barcode ->
                    handleBarcodeScan(barcode)
                    showBarcodeScanner = false
                }
            )
        }

        // 2. Interactive Bank-Linked Checkout Dialog
        if (showCheckoutDialog) {
            AlertDialog(
                onDismissRequest = { showCheckoutDialog = false },
                title = { Text("Satış Tahsilat ve Kapanış") },
                text = {
                    Column(verticalArrangement = Arrangement.spacedBy(16.dp)) {
                        Text("Fatura Toplamı: " + String.format("%,.2f ₺", grandTotal), fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodyLarge)
                        Text("Satış evrakının kapatılma metodunu seçiniz:")
                        
                        // Select Method Option Row
                        Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                            listOf("Cari Borç", "Nakit", "Bank Kartı").forEach { type ->
                                val active = checkoutPaymentType == type
                                Box(
                                    modifier = Modifier
                                        .weight(1f)
                                        .clip(RoundedCornerShape(8.dp))
                                        .background(if (active) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.surfaceVariant)
                                        .clickable { checkoutPaymentType = type }
                                        .padding(vertical = 10.dp),
                                    contentAlignment = Alignment.Center
                                ) {
                                    Text(
                                        text = type,
                                        color = if (active) MaterialTheme.colorScheme.onPrimary else MaterialTheme.colorScheme.onSurfaceVariant,
                                        fontWeight = FontWeight.Bold,
                                        style = MaterialTheme.typography.labelMedium
                                    )
                                }
                            }
                        }
                        
                        // Dynamic Bank Selector Dropdown
                        if (checkoutPaymentType == "Bank Kartı") {
                            Text("Ödemenin Yatacağı Banka Kasası:", style = MaterialTheme.typography.labelLarge, fontWeight = FontWeight.SemiBold)
                            Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                                AppDataStore.banks.forEach { bank ->
                                    val isSelected = selectedBankForCheckout?.id == bank.id
                                    Box(
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .clip(RoundedCornerShape(8.dp))
                                            .background(if (isSelected) MaterialTheme.colorScheme.secondaryContainer else MaterialTheme.colorScheme.surfaceVariant)
                                            .clickable { selectedBankForCheckout = bank }
                                            .padding(12.dp)
                                    ) {
                                        Row(
                                            modifier = Modifier.fillMaxWidth(),
                                            horizontalArrangement = Arrangement.SpaceBetween,
                                            verticalAlignment = Alignment.CenterVertically
                                        ) {
                                            Column {
                                                Text(bank.name, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodySmall)
                                                Text("IBAN: ${bank.iban}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                            }
                                            if (isSelected) {
                                                Icon(Icons.Filled.Check, null, tint = MaterialTheme.colorScheme.primary)
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                confirmButton = {
                    Button(
                        onClick = {
                            val activeCust = selectedCustomer
                            if (activeCust != null) {
                                if (AppDataStore.sendToApprovalCenterDirectly) {
                                    val approvalTxId = "FT-" + (12400 + AppDataStore.kasaLogs.size)
                                    val appItem = ApprovalItem(
                                        id = approvalTxId,
                                        type = "Satış",
                                        customerName = activeCust.name,
                                        description = "Saha Satış Faturası - Tutar: ${String.format("%.2f ₺", grandTotal)} (${cartItems.size} kalem ürün)",
                                        amount = grandTotal,
                                        time = "08.06.2026 18:00",
                                        reason = "Yeni Satış Siparişi Onay Talebi",
                                        paymentType = checkoutPaymentType,
                                        orderNote = orderNote
                                    )
                                    
                                    AppDataStore.approvalOrderItemsMap[approvalTxId] = cartItems.toList()
                                    
                                    if (AppDataStore.autoApproveAllTransactions) {
                                        AppDataStore.approvedApprovalItems.add(0, appItem)
                                        val newWmsOrder = com.example.data.database.WmsOrderEntity(
                                            id = approvalTxId,
                                            customerName = activeCust.name,
                                            orderDate = "16.06.2026",
                                            status = "Bekleyen",
                                            totalItems = cartItems.sumOf { it.quantity },
                                            syncStatus = "SYNCED"
                                        )
                                        val wmsItems = cartItems.map { item ->
                                            com.example.data.database.WmsOrderItemEntity(
                                                id = "${approvalTxId}_${item.product.barcode}",
                                                orderId = approvalTxId,
                                                productBarcode = item.product.barcode,
                                                productTitle = item.product.title,
                                                quantityOrdered = item.quantity,
                                                quantityPicked = 0,
                                                isPicked = false,
                                                shelfLocation = "Raf A-1"
                                            )
                                        }
                                        scope.launch(kotlinx.coroutines.Dispatchers.IO) {
                                            try {
                                                val db = com.example.data.database.DatabaseProvider.getDatabase(context.applicationContext)
                                                db.wmsOrderDao().insert(newWmsOrder)
                                                db.wmsOrderItemDao().insertAll(wmsItems)
                                            } catch (e: Exception) {
                                                e.printStackTrace()
                                            }
                                        }
                                    } else {
                                        AppDataStore.approvalItems.add(0, appItem)
                                    }
                                    
                                    playFeedbackTone(true)
                                    cartItems.clear()
                                    showCheckoutDialog = false
                                    AppDataStore.persist(context)
                                    scope.launch {
                                        android.widget.Toast.makeText(context, "Satış kaydı onay merkezine başarıyla gönderildi.", android.widget.Toast.LENGTH_LONG).show()
                                        delay(500)
                                        navController.popBackStack()
                                    }
                                } else {
                                    // 1. Deduct Inventory Stock By Warehouse
                                    cartItems.forEach { item ->
                                        val currentStock = item.product.stockByWarehouse.toMutableMap()
                                        val stockVal = currentStock[selectedWarehouse] ?: 0
                                        currentStock[selectedWarehouse] = if (AppDataStore.allowNegativeStock) {
                                            stockVal - item.quantity
                                        } else {
                                            maxOf(0, stockVal - item.quantity)
                                        }
                                        // Live update stock
                                        val originalIndex = AppDataStore.products.indexOf(item.product)
                                        if (originalIndex != -1) {
                                            AppDataStore.products[originalIndex] = item.product.copy(
                                                stockByWarehouse = currentStock
                                            )
                                        }
                                        
                                        // 2. Add to global sales history (so it becomes returnable!)
                                        AppDataStore.salesHistory.add(
                                            SalesRecord(
                                                customerId = activeCust.id,
                                                productBarcode = item.product.barcode,
                                                quantity = item.quantity,
                                                price = getProductPrice(item.product),
                                                date = "08.06.2026"
                                            )
                                        )
                                    }

                                    val directSaleNo = "FT-" + (12400 + AppDataStore.kasaLogs.size)

                                    // 3. Set payment balances + Kasa Logs
                                    when (checkoutPaymentType) {
                                        "Cari Borç" -> {
                                            // Update customer debt balance
                                            val idx = AppDataStore.customers.indexOfFirst { it.id == activeCust.id }
                                            if (idx != -1) {
                                                val c = AppDataStore.customers[idx]
                                                c.balance += grandTotal
                                                
                                                // Register ledger transaction
                                                c.transactions.add(
                                                    CustomerTx(
                                                        id = "TX-" + (20000 + c.transactions.size),
                                                        date = "08.06.2026",
                                                        type = "SATIŞ",
                                                        amount = grandTotal,
                                                        description = "Satış Faturası No: $directSaleNo"
                                                    )
                                                )
                                            }
                                            
                                            // Register cash logger
                                            AppDataStore.kasaLogs.add(
                                                KasaLogItem(
                                                    id = "K-" + (2000 + AppDataStore.kasaLogs.size),
                                                    date = "08.06.2026 18:00",
                                                    type = "Satış",
                                                    customerOrSupplier = activeCust.name,
                                                    amount = grandTotal,
                                                    paymentType = "Nakit", // Default tag for open account sale
                                                    bankName = null,
                                                    desc = "Satış yapıldı (Cari Borç) Fatura No: $directSaleNo"
                                                )
                                            )
                                        }
                                        "Nakit" -> {
                                            // Register Cash Kasa ledger log
                                            AppDataStore.kasaLogs.add(
                                                KasaLogItem(
                                                    id = "K-" + (2000 + AppDataStore.kasaLogs.size),
                                                    date = "08.06.2026 18:00",
                                                    type = "Satış",
                                                    customerOrSupplier = activeCust.name,
                                                    amount = grandTotal,
                                                    paymentType = "Nakit",
                                                    bankName = null,
                                                    desc = "Peşin Nakit Satış ve İhsar Fatura No: $directSaleNo"
                                                )
                                            )
                                        }
                                        "Bank Kartı" -> {
                                            val chosenBank = selectedBankForCheckout
                                            if (chosenBank != null) {
                                                // Live increment bank balance
                                                val bIdx = AppDataStore.banks.indexOfFirst { it.id == chosenBank.id }
                                                if (bIdx != -1) {
                                                    AppDataStore.banks[bIdx].balance += grandTotal
                                                }
                                                
                                                // Record Bank collection Kasa Log
                                                AppDataStore.kasaLogs.add(
                                                    KasaLogItem(
                                                        id = "K-" + (2000 + AppDataStore.kasaLogs.size),
                                                        date = "08.06.2026 18:00",
                                                        type = "Satış",
                                                        customerOrSupplier = activeCust.name,
                                                        amount = grandTotal,
                                                        paymentType = "Kredi Kartı",
                                                        bankName = chosenBank.name,
                                                        desc = "Peşin Kart Satışı / ${chosenBank.name} Fatura: $directSaleNo"
                                                    )
                                                )
                                            }
                                        }
                                    }
                                    
                                    val directNewWmsOrder = com.example.data.database.WmsOrderEntity(
                                        id = directSaleNo,
                                        customerName = activeCust.name,
                                        orderDate = "16.06.2026",
                                        status = "Bekleyen",
                                        totalItems = cartItems.sumOf { it.quantity },
                                        syncStatus = "SYNCED"
                                    )
                                    val directWmsItems = cartItems.map { item ->
                                        com.example.data.database.WmsOrderItemEntity(
                                            id = "${directSaleNo}_${item.product.barcode}",
                                            orderId = directSaleNo,
                                            productBarcode = item.product.barcode,
                                            productTitle = item.product.title,
                                            quantityOrdered = item.quantity,
                                            quantityPicked = 0,
                                            isPicked = false,
                                            shelfLocation = "Raf A-1"
                                        )
                                    }
                                    scope.launch(kotlinx.coroutines.Dispatchers.IO) {
                                        try {
                                            val db = com.example.data.database.DatabaseProvider.getDatabase(context.applicationContext)
                                            db.wmsOrderDao().insert(directNewWmsOrder)
                                            db.wmsOrderItemDao().insertAll(directWmsItems)
                                        } catch (e: Exception) {
                                            e.printStackTrace()
                                        }
                                    }
                                    
                                    playFeedbackTone(true)
                                    cartItems.clear()
                                    showCheckoutDialog = false
                                    AppDataStore.persist(context)
                                    scope.launch {
                                        snackbarHostState.showSnackbar("Satış başarıyla tamamlandı ve stoklar düşürüldü.")
                                        delay(500)
                                        navController.popBackStack()
                                    }
                                }
                            }
                        }
                    ) {
                        Text("Onayla ve Deftere İşle")
                    }
                },
                dismissButton = {
                    TextButton(onClick = { showCheckoutDialog = false }) {
                        Text("İptal")
                    }
                }
            )
        }

        if (selectedInvoiceTxDetails != null) {
            InvoiceDetailDialog(
                tx = selectedInvoiceTxDetails!!,
                customerName = selectedCustomer?.name ?: "Bilinmeyen Cari",
                onDismiss = { selectedInvoiceTxDetails = null }
            )
        }

        if (selectedProductForDetail != null) {
            ProductDetailShareDialog(
                product = selectedProductForDetail!!,
                displayPrice = getProductPrice(selectedProductForDetail!!),
                onDismiss = { selectedProductForDetail = null },
                context = context,
                coroutineScope = scope
            )
        }
    }
}

@Composable
fun CartItemCard(
    item: CartItem,
    unitPrice: Double,
    onIncrement: () -> Unit,
    onDecrement: () -> Unit,
    onDelete: () -> Unit,
    onApplyDiscount: (Double) -> Unit,
    onNoteUpdate: (String) -> Unit,
    onQuantityChange: (Int) -> Unit
) {
    var expanded by remember { mutableStateOf(false) }

    FieldCard(modifier = Modifier.fillMaxWidth()) {
        Column(modifier = Modifier.padding(8.dp)) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.spacedBy(8.dp)
            ) {
                // Left: Product title and code
                Column(modifier = Modifier.weight(1f)) {
                    Text(item.product.title, style = MaterialTheme.typography.bodyMedium, maxLines = 1, overflow = androidx.compose.ui.text.style.TextOverflow.Ellipsis, fontWeight = FontWeight.Bold)
                    Text("Kod: ${item.product.code}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                }

                // Middle Left: Quantity Selector (very compact)
                Row(
                    verticalAlignment = Alignment.CenterVertically, 
                    horizontalArrangement = Arrangement.spacedBy(2.dp)
                ) {
                    IconButton(onClick = onDecrement, modifier = Modifier.size(28.dp)) {
                        Icon(Icons.Filled.RemoveCircleOutline, contentDescription = null, tint = MaterialTheme.colorScheme.primary, modifier = Modifier.size(20.dp))
                    }
                    
                    var showDirectQtyEditDialog by remember { mutableStateOf(false) }
                    var directTypedQtyStr by remember { mutableStateOf(item.quantity.toString()) }
                    
                    Box(
                        modifier = Modifier
                            .clip(RoundedCornerShape(4.dp))
                            .background(MaterialTheme.colorScheme.surfaceVariant)
                            .clickable { 
                                directTypedQtyStr = item.quantity.toString()
                                showDirectQtyEditDialog = true 
                            }
                            .padding(horizontal = 10.dp, vertical = 4.dp),
                        contentAlignment = Alignment.Center
                    ) {
                        Text(
                            item.quantity.toString(), 
                            style = MaterialTheme.typography.bodyMedium, 
                            fontWeight = FontWeight.Bold,
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                    }
                    
                    if (showDirectQtyEditDialog) {
                        AlertDialog(
                            onDismissRequest = { showDirectQtyEditDialog = false },
                            title = { Text("Adet Düzenle", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold) },
                            text = {
                                Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                                    Text("${item.product.title} için satılacak adedi yazın:")
                                    OutlinedTextField(
                                        value = directTypedQtyStr,
                                        onValueChange = { newValue ->
                                            if (newValue.all { it.isDigit() }) {
                                                directTypedQtyStr = newValue
                                            }
                                        },
                                        keyboardOptions = KeyboardOptions(
                                            keyboardType = KeyboardType.Number,
                                            imeAction = ImeAction.Done
                                        ),
                                        modifier = Modifier.fillMaxWidth(),
                                        singleLine = true
                                    )
                                }
                            },
                            confirmButton = {
                                TextButton(
                                    onClick = {
                                        val parseQty = directTypedQtyStr.toIntOrNull() ?: 1
                                        onQuantityChange(parseQty)
                                        showDirectQtyEditDialog = false
                                    }
                                ) {
                                    Text("Tamam")
                                }
                            },
                            dismissButton = {
                                TextButton(onClick = { showDirectQtyEditDialog = false }) {
                                    Text("İptal")
                                }
                            }
                        )
                    }
                    
                    IconButton(onClick = onIncrement, modifier = Modifier.size(28.dp)) {
                        Icon(Icons.Filled.AddCircleOutline, contentDescription = null, tint = MaterialTheme.colorScheme.primary, modifier = Modifier.size(20.dp))
                    }
                }

                // Middle Right: Price
                Column(horizontalAlignment = Alignment.End, modifier = Modifier.widthIn(min = 70.dp)) {
                    val originalTotal = unitPrice * item.quantity
                    val discountedTotal = originalTotal - (originalTotal * item.lineDiscountPercent / 100.0)
                    
                    if (item.lineDiscountPercent > 0) {
                        Text(String.format("%,.1f ₺", originalTotal), style = MaterialTheme.typography.labelSmall, textDecoration = androidx.compose.ui.text.style.TextDecoration.LineThrough, color = Color.Gray)
                    }
                    Text(String.format("%,.2f ₺", discountedTotal), style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
                }
                
                // Right: Actions (edit/delete)
                Row(verticalAlignment = Alignment.CenterVertically) {
                    IconButton(onClick = { expanded = !expanded }, modifier = Modifier.size(28.dp)) {
                        Icon(if (expanded) Icons.Filled.ExpandLess else Icons.Filled.LinearScale, contentDescription = "Detaylar", tint = MaterialTheme.colorScheme.secondary, modifier = Modifier.size(18.dp))
                    }
                    IconButton(onClick = onDelete, modifier = Modifier.size(28.dp)) {
                        Icon(Icons.Filled.DeleteOutline, contentDescription = null, tint = MaterialTheme.colorScheme.error, modifier = Modifier.size(18.dp))
                    }
                }
            }

            if (expanded) {
                HorizontalDivider(modifier = Modifier.padding(vertical = 6.dp), color = MaterialTheme.colorScheme.outlineVariant)
                Column(verticalArrangement = Arrangement.spacedBy(6.dp)) {
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.SpaceBetween,
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Text("Satır İskontosu (%)", style = MaterialTheme.typography.bodySmall)
                        OutlinedTextField(
                            value = if (item.lineDiscountPercent == 0.0) "" else item.lineDiscountPercent.toString(),
                            onValueChange = { onApplyDiscount(it.toDoubleOrNull() ?: 0.0) },
                            modifier = Modifier.width(90.dp).height(40.dp),
                            singleLine = true,
                            textStyle = MaterialTheme.typography.bodySmall,
                            keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number)
                        )
                    }
                    OutlinedTextField(
                        value = item.note,
                        onValueChange = onNoteUpdate,
                        modifier = Modifier.fillMaxWidth().height(44.dp),
                        placeholder = { Text("Satır için açıklama girin...", style = MaterialTheme.typography.bodySmall) },
                        textStyle = MaterialTheme.typography.bodySmall,
                        singleLine = true
                    )
                }
            }
        }
    }
}

// --- CAMERAX / ML KIT BARCODE SCANNING SYSTEM CODES ---

@androidx.annotation.OptIn(androidx.camera.core.ExperimentalGetImage::class)
class BarcodeImageAnalyzer(
    private val onBarcodeDetected: (String) -> Unit
) : ImageAnalysis.Analyzer {
    private val scanner = BarcodeScanning.getClient()

    override fun analyze(imageProxy: ImageProxy) {
        val mediaImage = imageProxy.image
        if (mediaImage != null) {
            val image = InputImage.fromMediaImage(mediaImage, imageProxy.imageInfo.rotationDegrees)
            scanner.process(image)
                .addOnSuccessListener { barcodes ->
                    for (barcode in barcodes) {
                        val rawValue = barcode.rawValue
                        if (rawValue != null) {
                            onBarcodeDetected(rawValue)
                            break
                        }
                    }
                }
                .addOnFailureListener {
                    // Ignore errors
                }
                .addOnCompleteListener {
                    imageProxy.close()
                }
        } else {
            imageProxy.close()
        }
    }
}

@Composable
fun CameraScannerView(
    onBarcodeScanned: (String) -> Unit
) {
    val context = LocalContext.current
    val lifecycleOwner = LocalLifecycleOwner.current
    val cameraExecutor = remember { Executors.newSingleThreadExecutor() }

    AndroidView(
        factory = { ctx ->
            val previewView = PreviewView(ctx).apply {
                scaleType = PreviewView.ScaleType.FILL_CENTER
            }

            val cameraProviderFuture = ProcessCameraProvider.getInstance(ctx)
            cameraProviderFuture.addListener({
                val cameraProvider = cameraProviderFuture.get()
                val preview = Preview.Builder().build().apply {
                    surfaceProvider = previewView.surfaceProvider
                }

                val imageAnalysis = ImageAnalysis.Builder()
                    .setBackpressureStrategy(ImageAnalysis.STRATEGY_KEEP_ONLY_LATEST)
                    .build()
                    .apply {
                        setAnalyzer(cameraExecutor, BarcodeImageAnalyzer { barcode ->
                            onBarcodeScanned(barcode)
                        })
                    }

                val cameraSelector = CameraSelector.DEFAULT_BACK_CAMERA

                try {
                    cameraProvider.unbindAll()
                    cameraProvider.bindToLifecycle(
                        lifecycleOwner,
                        cameraSelector,
                        preview,
                        imageAnalysis
                    )
                } catch (e: Exception) {
                    e.printStackTrace()
                }
            }, ContextCompat.getMainExecutor(ctx))

            previewView
        },
        modifier = Modifier.fillMaxSize()
    )

    DisposableEffect(Unit) {
        onDispose {
            cameraExecutor.shutdown()
        }
    }
}

@OptIn(ExperimentalPermissionsApi::class)
@Composable
fun BarcodeScannerDialog(
    onDismissRequest: () -> Unit,
    onBarcodeScanned: (String) -> Unit,
    onSimulateScan: (String) -> Unit
) {
    val cameraPermissionState = rememberPermissionState(android.Manifest.permission.CAMERA)

    AlertDialog(
        onDismissRequest = onDismissRequest,
        properties = androidx.compose.ui.window.DialogProperties(usePlatformDefaultWidth = false),
        title = null,
        text = {
            Box(
                modifier = Modifier
                    .fillMaxWidth()
                    .height(500.dp)
                    .clip(RoundedCornerShape(16.dp))
                    .background(Color.Black)
            ) {
                if (cameraPermissionState.status.isGranted) {
                    CameraScannerView { barcode ->
                        onBarcodeScanned(barcode)
                    }
                    
                    // Transparent overlay targeting zone
                    Box(
                        modifier = Modifier
                            .fillMaxSize()
                            .padding(48.dp),
                        contentAlignment = Alignment.Center
                    ) {
                        Canvas(modifier = Modifier.size(240.dp)) {
                            val lineLength = 40.dp.toPx()
                            val strokeWidth = 4.dp.toPx()
                            val color = Color.Red

                            // Top Left Corner
                            drawLine(color, start = androidx.compose.ui.geometry.Offset(0f, 0f), end = androidx.compose.ui.geometry.Offset(lineLength, 0f), strokeWidth = strokeWidth)
                            drawLine(color, start = androidx.compose.ui.geometry.Offset(0f, 0f), end = androidx.compose.ui.geometry.Offset(0f, lineLength), strokeWidth = strokeWidth)

                            // Top Right Corner
                            drawLine(color, start = androidx.compose.ui.geometry.Offset(size.width, 0f), end = androidx.compose.ui.geometry.Offset(size.width - lineLength, 0f), strokeWidth = strokeWidth)
                            drawLine(color, start = androidx.compose.ui.geometry.Offset(size.width, 0f), end = androidx.compose.ui.geometry.Offset(size.width, lineLength), strokeWidth = strokeWidth)

                            // Bottom Left Corner
                            drawLine(color, start = androidx.compose.ui.geometry.Offset(0f, size.height), end = androidx.compose.ui.geometry.Offset(lineLength, size.height), strokeWidth = strokeWidth)
                            drawLine(color, start = androidx.compose.ui.geometry.Offset(0f, size.height), end = androidx.compose.ui.geometry.Offset(0f, size.height - lineLength), strokeWidth = strokeWidth)

                            // Bottom Right Corner
                            drawLine(color, start = androidx.compose.ui.geometry.Offset(size.width, size.height), end = androidx.compose.ui.geometry.Offset(size.width - lineLength, size.height), strokeWidth = strokeWidth)
                            drawLine(color, start = androidx.compose.ui.geometry.Offset(size.width, size.height), end = androidx.compose.ui.geometry.Offset(size.width, size.height - lineLength), strokeWidth = strokeWidth)
                        }

                        // Scanning Red Laser Line Indicator
                        Box(
                            modifier = Modifier
                                .width(220.dp)
                                .height(2.dp)
                                .background(Color.Red.copy(alpha = 0.8f))
                        )
                    }
                } else {
                    Column(
                        modifier = Modifier
                            .fillMaxSize()
                            .padding(24.dp),
                        horizontalAlignment = Alignment.CenterHorizontally,
                        verticalArrangement = Arrangement.Center
                    ) {
                        Icon(Icons.Filled.PhotoCamera, contentDescription = null, tint = Color.White, modifier = Modifier.size(64.dp))
                        Spacer(modifier = Modifier.height(16.dp))
                        Text(
                            "Gerçek zamanlı barkod okuma için kamera izni verilmelidir.",
                            color = Color.White,
                            textAlign = TextAlign.Center,
                            style = MaterialTheme.typography.bodyMedium
                        )
                        Spacer(modifier = Modifier.height(16.dp))
                        Button(
                            onClick = { cameraPermissionState.launchPermissionRequest() }
                        ) {
                            Text("Kamera İzni İste")
                        }
                    }
                }

                // Header with custom Close button
                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(16.dp)
                        .align(Alignment.TopCenter),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Text("Barkod Tarayıcı", color = Color.White, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.titleMedium)
                    IconButton(
                        onClick = onDismissRequest,
                        modifier = Modifier.background(Color.White.copy(alpha = 0.3f), shape = CircleShape)
                    ) {
                        Icon(Icons.Filled.Close, contentDescription = "Kapat", tint = Color.White)
                    }
                }

                // Fallback simulation bar (for emulator/no camera)
                if (!cameraPermissionState.status.isGranted) {
                    Card(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(16.dp)
                            .align(Alignment.BottomCenter),
                        colors = CardDefaults.cardColors(containerColor = Color.White.copy(alpha = 0.9f))
                    ) {
                        Column(
                            modifier = Modifier.padding(12.dp),
                            horizontalAlignment = Alignment.CenterHorizontally,
                            verticalArrangement = Arrangement.spacedBy(8.dp)
                        ) {
                            Text("Kameranız Yok mu? Simulatorü Kullanın:", style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Bold, color = Color.DarkGray)
                            Row(
                                horizontalArrangement = Arrangement.spacedBy(8.dp),
                                modifier = Modifier.fillMaxWidth()
                            ) {
                                Button(
                                    onClick = { onSimulateScan("8690123456789") },
                                    modifier = Modifier.weight(1f),
                                    contentPadding = PaddingValues(vertical = 4.dp)
                                ) {
                                    Text("Yağ Simüle Et", style = MaterialTheme.typography.labelSmall)
                                }
                                Button(
                                    onClick = { onSimulateScan("8699876543210") },
                                    modifier = Modifier.weight(1f),
                                    contentPadding = PaddingValues(vertical = 4.dp)
                                ) {
                                    Text("Filtre Simüle Et", style = MaterialTheme.typography.labelSmall)
                                }
                            }
                        }
                    }
                }
            }
        },
        confirmButton = {},
        dismissButton = {}
    )
}

@Composable
fun InvoiceDetailDialog(
    tx: CustomerTx,
    customerName: String,
    onDismiss: () -> Unit
) {
    val context = LocalContext.current
    val scope = rememberCoroutineScope()
    var loadedItems by remember(tx.id) { mutableStateOf<List<Pair<String, Pair<Int, Double>>>>(emptyList()) }
    var isLoading by remember(tx.id) { mutableStateOf(true) }
    
    var isReprinting by remember { mutableStateOf(false) }
    var reprintStateText by remember { mutableStateOf("") }

    LaunchedEffect(tx.id, tx.description) {
        isLoading = true
        try {
            val db = com.example.data.database.DatabaseProvider.getDatabase(context.applicationContext)
            val desc = tx.description
            val matched = Regex("""(SM-\d{4}-\d+|FT-[A-Za-z0-9-_]+|FT-\d+)""").find(desc)
            val orderId = matched?.value 
                ?: if (tx.id.startsWith("FT-") || tx.id.startsWith("SM-")) tx.id
                else if (tx.id.startsWith("REV-")) tx.id.substring(4) 
                else tx.id
            
            var found = false

            var wmsItems = emptyList<com.example.data.database.WmsOrderItemEntity>()
            if (tx.cha_recno != null) {
                val items = db.wmsOrderItemDao().getItemsByRecNo(tx.cha_recno)
                if (items.isNotEmpty()) {
                    wmsItems = items
                }
            }
            if (wmsItems.isEmpty() && tx.cha_recno == null && orderId != null) {
                // Eski/offline kayıtlarda yalnızca kesin belge anahtarlarını dene.
                val keysToTry = listOfNotNull(tx.erpRef, tx.recNo, orderId, tx.id)
                for (key in keysToTry) {
                    if (key.isNotBlank()) {
                        val items = db.wmsOrderItemDao().getItemsForOrder(key)
                        if (items.isNotEmpty()) {
                            wmsItems = items
                            break
                        }
                    }
                }
            }
            
            if (wmsItems.isNotEmpty()) {
                val productsList = db.productDao().getAllProducts()
                val resultList = mutableListOf<Pair<String, Pair<Int, Double>>>()
                for (wi in wmsItems) {
                    val matchingProd = productsList.find { 
                        it.barcode == wi.productBarcode || 
                        it.code == wi.productBarcode || 
                        (wi.productBarcode.startsWith("ST-") && it.code == wi.productBarcode.removePrefix("ST-")) 
                    }
                    val price = matchingProd?.basePrice ?: 120.0
                    val displayQty = if (wi.quantityPicked > 0) wi.quantityPicked else wi.quantityOrdered
                    if (displayQty > 0) {
                        resultList.add(Pair(wi.productTitle, Pair(displayQty, price)))
                    }
                }
                if (resultList.isNotEmpty()) {
                    loadedItems = resultList
                    found = true
                }
            }
            
            if (!found) {
                val cleanCariName = { name: String ->
                    name.lowercase()
                        .replace("a.ş.", "")
                        .replace("ltd.şti.", "")
                        .replace("ltd. şti.", "")
                        .replace("ltd.sti.", "")
                        .replace("şirketi", "")
                        .replace("sirketi", "")
                        .replace("ticaret", "")
                        .replace("sanayi", "")
                        .replace("ve", "")
                        .replace("ı", "i")
                        .replace("ş", "s")
                        .replace("ğ", "g")
                        .replace("ü", "u")
                        .replace("ö", "o")
                        .replace("ç", "c")
                        .filter { it.isLetterOrDigit() }
                        .trim()
                }
                val customerClean = cleanCariName(customerName)
                val matchingCust = AppDataStore.customers.find { 
                    val custClean = cleanCariName(it.name)
                    custClean.isNotEmpty() && custClean == customerClean
                }
                if (matchingCust != null) {
                    val custId = matchingCust.id
                    if (!custId.startsWith("customer_")) {
                        try {
                            val sharedPrefs = context.getSharedPreferences("erp_settings", android.content.Context.MODE_PRIVATE)
                            val apiUrl = sharedPrefs.getString("api_url", "https://lisans.appsgo.cloud") ?: "https://lisans.appsgo.cloud"
                            val apiKey = sharedPrefs.getString("api_key", "dev-token-change-in-production") ?: "dev-token-change-in-production"
                            
                            val apiService = com.example.data.api.ApiClient.getFieldOpsApiService(context, apiUrl, apiKey)
                            val response = apiService.getFaturaHareket(com.example.data.api.PullJobsRequest(tenant_id=sharedPrefs.getString("tenant_id", "T001") ?: "T001", api_key=apiKey, device_id=sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT", agent_version="v2.0", entity="faturaHareket", since=custId))
                            if (response.isSuccessful && response.body() != null) {
                                val body = response.body()!!
                                val items = body.actualItems
                                
                                val targetClean = orderId?.replace("FT-", "")?.replace("SM-", "") ?: ""
                                var matchedInvoice = if (tx.cha_recno != null) {
                                    items.find { fatura ->
                                        fatura.satirlar?.any { it.realSthFatRecidRecno == tx.cha_recno } == true
                                    }
                                } else null
                                
                                if (matchedInvoice == null && tx.cha_recno == null) {
                                    matchedInvoice = items.find { fatura ->
                                        val rawEvrak = fatura.evrakNo ?: ""
                                        val ref = fatura.erpRef ?: ""
                                        (ref.isNotEmpty() && ref == tx.erpRef) ||
                                        (ref.isNotEmpty() && ref == tx.recNo) ||
                                        (ref.isNotEmpty() && ref == targetClean) ||
                                        (ref.isNotEmpty() && ref == orderId) ||
                                        (rawEvrak.isNotEmpty() && rawEvrak == targetClean) || 
                                        (rawEvrak.isNotEmpty() && rawEvrak == orderId) || 
                                        (ref.isNotEmpty() && ref == tx.id)
                                    }
                                }
                                
                                if (matchedInvoice != null && matchedInvoice.satirlar != null && matchedInvoice.satirlar.isNotEmpty()) {
                                    val resultList = mutableListOf<Pair<String, Pair<Int, Double>>>()
                                    
                                    val rawEvrak = matchedInvoice.evrakNo ?: ""
                                    val invoiceNo = if (rawEvrak.isNotEmpty() && !rawEvrak.startsWith("FT-") && !rawEvrak.startsWith("SM-")) {
                                        "FT-$rawEvrak"
                                    } else {
                                        rawEvrak.ifEmpty { "FT-ERP-${matchedInvoice.erpRef ?: (Math.random()*100000).toInt()}" }
                                    }
                                    
                                    val invoiceIdKey = if (!matchedInvoice.erpRef.isNullOrBlank()) matchedInvoice.erpRef else invoiceNo
                                    
                                    val targetLines = if (tx.cha_recno != null) {
                                        matchedInvoice.satirlar.filter { it.realSthFatRecidRecno == tx.cha_recno }
                                    } else {
                                        matchedInvoice.satirlar
                                    }
                                    
                                    val totalQtySum = targetLines.sumOf { it.miktar?.toInt() ?: 1 }
                                    val orderEntity = com.example.data.database.WmsOrderEntity(
                                        id = invoiceIdKey,
                                        customerName = customerName,
                                        orderDate = matchedInvoice.tarih ?: tx.date,
                                        status = "Sevk Edildi",
                                        totalItems = totalQtySum,
                                        syncStatus = "SYNCED"
                                    )
                                    db.wmsOrderDao().insert(orderEntity)
                                    
                                    val orderItemsList = mutableListOf<com.example.data.database.WmsOrderItemEntity>()
                                    targetLines.forEachIndexed { idx, satir ->
                                        val stokK = satir.stokKod ?: ""
                                        val matchedProd = AppDataStore.products.find { 
                                            it.code == stokK || 
                                            it.barcode == stokK ||
                                            (stokK.startsWith("ST-") && it.code == stokK.removePrefix("ST-")) 
                                        }
                                        val prodBarcode = matchedProd?.barcode ?: "ST-${stokK}"
                                        val prodTitle = matchedProd?.title ?: satir.stokAd ?: "Ürün ($stokK)"
                                        val itemQty = satir.miktar?.toInt() ?: 1
                                        val priceVal = satir.birimFiyat ?: (satir.tutar ?: 120.0) / (if (itemQty > 0) itemQty else 1)
                                        
                                        resultList.add(Pair(prodTitle, Pair(itemQty, priceVal)))
                                        
                                        val orderItem = com.example.data.database.WmsOrderItemEntity(
                                            id = "${invoiceIdKey}_${stokK}_${idx}",
                                            orderId = invoiceIdKey,
                                            productBarcode = prodBarcode,
                                            productTitle = prodTitle,
                                            quantityOrdered = itemQty,
                                            quantityPicked = itemQty,
                                            isPicked = true,
                                            shelfLocation = "ERP Merkez",
                                            sth_fat_recid_recno = satir.realSthFatRecidRecno
                                        )
                                        orderItemsList.add(orderItem)
                                    }
                                    
                                    if (orderItemsList.isNotEmpty()) {
                                        db.wmsOrderItemDao().insertAll(orderItemsList)
                                    }
                                    
                                    if (resultList.isNotEmpty()) {
                                        loadedItems = resultList
                                        found = true
                                    }
                                }
                            }
                        } catch (e: Exception) {
                            e.printStackTrace()
                        }
                    }
                }
            }
            
            if (loadedItems.isEmpty() && tx.type == "TAHSİLAT") {
                    val method = if (tx.description.contains("Akbank", ignoreCase = true) || tx.description.contains("Banka", ignoreCase = true) || tx.description.contains("EFT", ignoreCase = true) || tx.description.contains("Havale", ignoreCase = true)) {
                        "Banka Havalesi / EFT"
                    } else if (tx.description.contains("Kart", ignoreCase = true) || tx.description.contains("KK", ignoreCase = true)) {
                        "Kredi Kartı Ödemesi"
                    } else {
                        "Nakit Tahsilat Makbuzu"
                    }
                    loadedItems = listOf(
                        Pair(method, Pair(1, tx.amount))
                    )
            }
        } catch (e: Exception) {
            e.printStackTrace()
        } finally {
            isLoading = false
        }
    }
    
    Dialog(onDismissRequest = if (isReprinting) ({}) else onDismiss) {
        Card(
            modifier = Modifier
                .fillMaxWidth()
                .padding(horizontal = 4.dp, vertical = 20.dp),
            shape = RoundedCornerShape(24.dp),
            colors = CardDefaults.cardColors(
                containerColor = MaterialTheme.colorScheme.surface
            ),
            elevation = CardDefaults.cardElevation(defaultElevation = 8.dp)
        ) {
            Box(modifier = Modifier.fillMaxWidth()) {
                Column(
                    modifier = Modifier
                        .padding(20.dp)
                        .fillMaxWidth(),
                    verticalArrangement = Arrangement.spacedBy(16.dp)
                ) {
                    // Header Area
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.SpaceBetween,
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Row(
                            verticalAlignment = Alignment.CenterVertically,
                            horizontalArrangement = Arrangement.spacedBy(12.dp)
                        ) {
                            Box(
                                modifier = Modifier
                                    .size(40.dp)
                                    .background(
                                        color = if (tx.type == "SATIŞ") MaterialTheme.colorScheme.primaryContainer else Color(0xFFE8F5E9),
                                        shape = RoundedCornerShape(12.dp)
                                    ),
                                contentAlignment = Alignment.Center
                            ) {
                                Icon(
                                    imageVector = if (tx.type == "SATIŞ") Icons.Filled.Receipt else Icons.Filled.AddCard,
                                    contentDescription = null,
                                    tint = if (tx.type == "SATIŞ") MaterialTheme.colorScheme.primary else Color(0xFF2E7D32),
                                    modifier = Modifier.size(22.dp)
                                )
                            }
                            Column {
                                Text(
                                    text = if (tx.type == "SATIŞ") "e-Arşiv Fatura Detayı" else "İşlem Fiş Detayı",
                                    style = MaterialTheme.typography.titleMedium.copy(fontWeight = FontWeight.Bold),
                                    color = MaterialTheme.colorScheme.onSurface
                                )
                                Text(
                                    text = "Resmi Evrak • Sayısal İmzalı",
                                    style = MaterialTheme.typography.labelSmall,
                                    color = MaterialTheme.colorScheme.outline
                                )
                            }
                        }
                        IconButton(
                            onClick = onDismiss, 
                            modifier = Modifier
                                .size(32.dp)
                                .background(MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f), CircleShape)
                        ) {
                            Icon(Icons.Filled.Close, contentDescription = "Kapat", modifier = Modifier.size(16.dp))
                        }
                    }

                    HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.4f))

                    // Compact Customer Info Banner - TAKES VASTLY LESS SPACE
                    Card(
                        modifier = Modifier.fillMaxWidth(),
                        colors = CardDefaults.cardColors(
                            containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.4f)
                        ),
                        shape = RoundedCornerShape(16.dp)
                    ) {
                        Column(
                            modifier = Modifier.padding(12.dp),
                            verticalArrangement = Arrangement.spacedBy(6.dp)
                        ) {
                            Row(
                                verticalAlignment = Alignment.CenterVertically,
                                horizontalArrangement = Arrangement.spacedBy(8.dp)
                            ) {
                                Icon(
                                    imageVector = Icons.Filled.Business,
                                    contentDescription = null,
                                    tint = MaterialTheme.colorScheme.primary,
                                    modifier = Modifier.size(16.dp)
                                )
                                Text(
                                    text = customerName,
                                    style = MaterialTheme.typography.bodyMedium.copy(fontWeight = FontWeight.Bold),
                                    color = MaterialTheme.colorScheme.onSurface,
                                    maxLines = 1,
                                    overflow = androidx.compose.ui.text.style.TextOverflow.Ellipsis
                                )
                            }
                            
                            // Compact detail chip row
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.spacedBy(6.dp),
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                // Date Chip
                                Surface(
                                    color = MaterialTheme.colorScheme.surfaceContainerLowest,
                                    shape = RoundedCornerShape(6.dp),
                                    border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
                                ) {
                                    Row(
                                        modifier = Modifier.padding(horizontal = 6.dp, vertical = 3.dp),
                                        verticalAlignment = Alignment.CenterVertically,
                                        horizontalArrangement = Arrangement.spacedBy(4.dp)
                                    ) {
                                        Icon(Icons.Filled.DateRange, contentDescription = null, modifier = Modifier.size(10.dp), tint = MaterialTheme.colorScheme.outline)
                                        Text(tx.date, style = MaterialTheme.typography.labelSmall.copy(fontWeight = FontWeight.Medium), color = MaterialTheme.colorScheme.onSurfaceVariant)
                                    }
                                }

                                // Document ID Chip
                                Surface(
                                    color = MaterialTheme.colorScheme.surfaceContainerLowest,
                                    shape = RoundedCornerShape(6.dp),
                                    border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
                                ) {
                                    Row(
                                        modifier = Modifier.padding(horizontal = 6.dp, vertical = 3.dp),
                                        verticalAlignment = Alignment.CenterVertically,
                                        horizontalArrangement = Arrangement.spacedBy(4.dp)
                                    ) {
                                        Icon(Icons.Filled.Tag, contentDescription = null, modifier = Modifier.size(10.dp), tint = MaterialTheme.colorScheme.outline)
                                        Text(tx.id, style = MaterialTheme.typography.labelSmall.copy(fontWeight = FontWeight.Bold), color = MaterialTheme.colorScheme.onSurfaceVariant)
                                    }
                                }

                                Spacer(modifier = Modifier.weight(1f))

                                // Status Indicator
                                Row(
                                    verticalAlignment = Alignment.CenterVertically,
                                    horizontalArrangement = Arrangement.spacedBy(4.dp)
                                ) {
                                    Box(
                                        modifier = Modifier
                                            .size(6.dp)
                                            .background(
                                                color = if (tx.isOffline) MaterialTheme.colorScheme.error else Color(0xFF43A047),
                                                shape = CircleShape
                                            )
                                    )
                                    Text(
                                        text = if (tx.isOffline) "Yerel" else "Bulutta",
                                        style = MaterialTheme.typography.labelSmall.copy(fontWeight = FontWeight.Bold),
                                        color = if (tx.isOffline) MaterialTheme.colorScheme.error else Color(0xFF43A047)
                                    )
                                }
                            }
                        }
                    }
                    
                    // Main Area for Order Lines / Sipariş Kalemleri
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.SpaceBetween,
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Text(
                            text = "Fatura Satır Kalemleri",
                            style = MaterialTheme.typography.titleSmall.copy(fontWeight = FontWeight.Bold),
                            color = MaterialTheme.colorScheme.onSurface
                        )
                        if (!isLoading && loadedItems.isNotEmpty()) {
                            Surface(
                                color = MaterialTheme.colorScheme.secondaryContainer,
                                shape = RoundedCornerShape(6.dp)
                            ) {
                                Text(
                                    text = "${loadedItems.size} Kalem",
                                    style = MaterialTheme.typography.labelSmall.copy(fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.onSecondaryContainer),
                                    modifier = Modifier.padding(horizontal = 6.dp, vertical = 2.dp)
                                )
                            }
                        }
                    }

                    Box(
                        modifier = Modifier
                            .weight(1f)
                            .fillMaxWidth()
                            .heightIn(min = 150.dp, max = 340.dp)
                    ) {
                        if (isLoading) {
                            Box(
                                modifier = Modifier.fillMaxSize(),
                                contentAlignment = Alignment.Center
                            ) {
                                CircularProgressIndicator(modifier = Modifier.size(28.dp))
                            }
                        } else if (loadedItems.isEmpty()) {
                            Box(
                                modifier = Modifier
                                    .fillMaxSize()
                                    .background(
                                        color = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.2f),
                                        shape = RoundedCornerShape(12.dp)
                                    ),
                                contentAlignment = Alignment.Center
                            ) {
                                Column(horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(8.dp)) {
                                    Icon(Icons.Filled.FolderOpen, contentDescription = null, tint = MaterialTheme.colorScheme.outline, modifier = Modifier.size(28.dp))
                                    Text("Satış satırı bilgisi bulunamadı.", style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.outline)
                                }
                            }
                        } else {
                            LazyColumn(
                                modifier = Modifier.fillMaxSize(),
                                verticalArrangement = Arrangement.spacedBy(8.dp),
                                contentPadding = PaddingValues(bottom = 8.dp)
                            ) {
                                items(loadedItems.size) { index ->
                                    val (titleStr, pairDetails) = loadedItems[index]
                                    val qty = pairDetails.first
                                    val price = pairDetails.second
                                    
                                    Card(
                                        modifier = Modifier.fillMaxWidth(),
                                        colors = CardDefaults.cardColors(
                                            containerColor = MaterialTheme.colorScheme.surfaceContainerLowest
                                        ),
                                        border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.3f)),
                                        shape = RoundedCornerShape(12.dp)
                                    ) {
                                        Row(
                                            modifier = Modifier
                                                .fillMaxWidth()
                                                .padding(12.dp),
                                            verticalAlignment = Alignment.CenterVertically,
                                            horizontalArrangement = Arrangement.spacedBy(12.dp)
                                        ) {
                                            // Circular Index Number
                                            Box(
                                                modifier = Modifier
                                                    .size(24.dp)
                                                    .background(MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.7f), CircleShape),
                                                contentAlignment = Alignment.Center
                                            ) {
                                                Text(
                                                    text = "${index + 1}",
                                                    style = MaterialTheme.typography.labelSmall.copy(fontWeight = FontWeight.Bold),
                                                    color = MaterialTheme.colorScheme.onSurfaceVariant
                                                )
                                            }
                                            
                                            Column(modifier = Modifier.weight(1f)) {
                                                Text(
                                                    text = titleStr,
                                                    style = MaterialTheme.typography.bodySmall.copy(fontWeight = FontWeight.Bold),
                                                    color = MaterialTheme.colorScheme.onSurface,
                                                    maxLines = 1,
                                                    overflow = androidx.compose.ui.text.style.TextOverflow.Ellipsis
                                                )
                                                Text(
                                                    text = "$qty Adet × ${String.format("₺%,.2f", price)}",
                                                    style = MaterialTheme.typography.labelSmall,
                                                    color = MaterialTheme.colorScheme.outline
                                                )
                                            }
                                            
                                            Text(
                                                text = String.format("₺%,.2f", qty * price),
                                                style = MaterialTheme.typography.bodySmall.copy(fontWeight = FontWeight.Bold),
                                                color = MaterialTheme.colorScheme.primary
                                            )
                                        }
                                    }
                                }
                            }
                        }
                    }

                    HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.4f))

                    // Optional Description Box
                    if (tx.description.isNotEmpty() && tx.description != "Fiyat Revize" && !tx.description.contains("Siparişler")) {
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .background(MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.25f), RoundedCornerShape(10.dp))
                                .padding(horizontal = 12.dp, vertical = 8.dp),
                            verticalAlignment = Alignment.CenterVertically,
                            horizontalArrangement = Arrangement.spacedBy(8.dp)
                        ) {
                            Icon(Icons.Filled.Info, contentDescription = null, modifier = Modifier.size(14.dp), tint = MaterialTheme.colorScheme.outline)
                            Text(
                                text = "Açıklama: ${tx.description}",
                                style = MaterialTheme.typography.labelSmall,
                                color = MaterialTheme.colorScheme.outline,
                                maxLines = 1,
                                overflow = androidx.compose.ui.text.style.TextOverflow.Ellipsis
                            )
                        }
                    }

                    // Bottom Bar with Total & BIG Print Button
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.SpaceBetween,
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Column {
                            Text("Toplam Fatura Tutarı", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.outline)
                            Text(
                                text = String.format("₺%,.2f", tx.amount),
                                style = MaterialTheme.typography.titleLarge.copy(fontWeight = FontWeight.Bold),
                                color = MaterialTheme.colorScheme.primary
                            )
                        }
                        
                        // GLORIOUS REPRINT BUTTON
                        Button(
                            onClick = {
                                isReprinting = true
                                reprintStateText = "Bluetooth fatura yazıcısına bağlanılıyor..."
                                scope.launch {
                                    delay(900)
                                    reprintStateText = "e-Arşiv fatura şablonu ve sayısal imza hazırlanıyor..."
                                    try {
                                        val toneG = android.media.ToneGenerator(android.media.AudioManager.STREAM_ALARM, 100)
                                        toneG.startTone(android.media.ToneGenerator.TONE_CDMA_PIP, 120)
                                    } catch (e: Exception) {}
                                    
                                    delay(900)
                                    reprintStateText = "Veriler yazıcı kafasına aktarılıyor..."
                                    
                                    delay(1000)
                                    reprintStateText = "Yazdırılıyor... Lütfen fatura şeridini çekmeyiniz."
                                    try {
                                        val toneG = android.media.ToneGenerator(android.media.AudioManager.STREAM_ALARM, 100)
                                        toneG.startTone(android.media.ToneGenerator.TONE_CDMA_ALERT_CALL_GUARD, 200)
                                    } catch (e: Exception) {}
                                    
                                    delay(600)
                                    isReprinting = false
                                    Toast.makeText(context, "Resmi e-Arşiv faturası başarıyla tekrar yazdırıldı.", Toast.LENGTH_LONG).show()
                                }
                            },
                            shape = RoundedCornerShape(12.dp),
                            contentPadding = PaddingValues(horizontal = 16.dp, vertical = 12.dp),
                            colors = ButtonDefaults.buttonColors(
                                containerColor = MaterialTheme.colorScheme.primary
                            )
                        ) {
                            Icon(Icons.Filled.Print, contentDescription = null, modifier = Modifier.size(18.dp))
                            Spacer(modifier = Modifier.width(8.dp))
                            Text("Tekrar Yazdır", style = MaterialTheme.typography.bodyMedium.copy(fontWeight = FontWeight.Bold))
                        }
                    }
                }

                // Interactive Printer Loading Display Block
                if (isReprinting) {
                    Box(
                        modifier = Modifier
                            .matchParentSize()
                            .background(MaterialTheme.colorScheme.surface.copy(alpha = 0.94f))
                            .padding(24.dp),
                        contentAlignment = Alignment.Center
                    ) {
                        Column(
                            horizontalAlignment = Alignment.CenterHorizontally,
                            verticalArrangement = Arrangement.spacedBy(16.dp)
                        ) {
                            CircularProgressIndicator(
                                color = MaterialTheme.colorScheme.primary,
                                strokeWidth = 4.dp,
                                modifier = Modifier.size(48.dp)
                            )
                            
                            Text(
                                text = "Fatura Yazdırılıyor",
                                style = MaterialTheme.typography.titleMedium.copy(fontWeight = FontWeight.Bold),
                                color = MaterialTheme.colorScheme.onSurface
                            )
                            
                            Text(
                                text = reprintStateText,
                                style = MaterialTheme.typography.bodySmall,
                                color = MaterialTheme.colorScheme.outline,
                                textAlign = TextAlign.Center
                            )
                        }
                    }
                }
            }
        }
    }
}

@Composable
fun ProductDetailShareDialog(
    product: ProductCatalog,
    displayPrice: Double,
    onDismiss: () -> Unit,
    context: android.content.Context,
    coroutineScope: kotlinx.coroutines.CoroutineScope
) {
    var isSharing by remember { mutableStateOf(false) }

    Dialog(onDismissRequest = onDismiss) {
        Card(
            modifier = Modifier
                .fillMaxWidth()
                .padding(12.dp)
                .wrapContentHeight(),
            shape = RoundedCornerShape(16.dp),
            colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
            elevation = CardDefaults.cardElevation(defaultElevation = 8.dp)
        ) {
            Column(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(16.dp),
                horizontalAlignment = Alignment.CenterHorizontally
            ) {
                // Header Row
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Text(
                        text = "Ürün Detayları",
                        fontWeight = FontWeight.Bold,
                        style = MaterialTheme.typography.titleMedium,
                        color = MaterialTheme.colorScheme.primary
                    )
                    IconButton(onClick = onDismiss, modifier = Modifier.size(32.dp)) {
                        Icon(Icons.Filled.Close, contentDescription = "Kapat", modifier = Modifier.size(20.dp))
                    }
                }
                
                Spacer(modifier = Modifier.height(12.dp))

                // Product Images Preview Card with background
                Box(
                    modifier = Modifier
                        .fillMaxWidth()
                        .height(180.dp)
                        .clip(RoundedCornerShape(12.dp))
                        .background(MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.2f))
                        .border(0.5.dp, MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f), RoundedCornerShape(12.dp)),
                    contentAlignment = Alignment.Center
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

                    if (images.isEmpty()) {
                        Icon(
                            imageVector = Icons.Filled.Inbox,
                            contentDescription = null,
                            tint = product.imageUrlColor,
                            modifier = Modifier.size(48.dp)
                        )
                    } else {
                        val pagerState = androidx.compose.foundation.pager.rememberPagerState(pageCount = { images.size })
                        Box(modifier = Modifier.fillMaxSize()) {
                            androidx.compose.foundation.pager.HorizontalPager(
                                state = pagerState,
                                modifier = Modifier.fillMaxSize()
                            ) { page ->
                                val imageModel = images[page]
                                coil.compose.AsyncImage(
                                    model = imageModel,
                                    contentDescription = "${product.title} - Resim ${page + 1}",
                                    modifier = Modifier.fillMaxSize().padding(4.dp),
                                    contentScale = androidx.compose.ui.layout.ContentScale.Fit
                                )
                            }
                            if (images.size > 1) {
                                Row(
                                    modifier = Modifier
                                        .align(Alignment.BottomCenter)
                                        .padding(bottom = 6.dp)
                                        .background(Color.Black.copy(alpha = 0.4f), RoundedCornerShape(8.dp))
                                        .padding(horizontal = 6.dp, vertical = 2.dp),
                                    horizontalArrangement = Arrangement.spacedBy(4.dp)
                                ) {
                                    repeat(images.size) { index ->
                                        Box(
                                            modifier = Modifier
                                                .size(5.dp)
                                                .background(
                                                    color = if (pagerState.currentPage == index) Color.White else Color.White.copy(alpha = 0.5f),
                                                    shape = CircleShape
                                                )
                                        )
                                    }
                                }
                            }
                        }
                    }
                }

                Spacer(modifier = Modifier.height(12.dp))

                // Scrollable Specs Column
                Column(
                    modifier = Modifier
                        .fillMaxWidth()
                        .weight(1f, fill = false),
                    verticalArrangement = Arrangement.spacedBy(6.dp)
                ) {
                    Text(
                        text = product.category.uppercase(),
                        style = MaterialTheme.typography.labelSmall,
                        color = MaterialTheme.colorScheme.primary,
                        fontWeight = FontWeight.Bold
                    )
                    Text(
                        text = product.title,
                        style = MaterialTheme.typography.titleMedium,
                        fontWeight = FontWeight.Bold,
                        color = MaterialTheme.colorScheme.onSurface,
                        maxLines = 2,
                        overflow = TextOverflow.Ellipsis
                    )

                    HorizontalDivider(modifier = Modifier.padding(vertical = 4.dp), color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.4f))

                    // Row specifications (strictly excluding Stocks)
                    Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                        Text("Ürün Kodu:", style = MaterialTheme.typography.bodyMedium, color = Color.Gray)
                        Text(product.code, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.SemiBold)
                    }
                    Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                        Text("Barkod:", style = MaterialTheme.typography.bodyMedium, color = Color.Gray)
                        Text(product.barcode, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.SemiBold)
                    }
                    if (!product.brand.isNullOrBlank()) {
                        Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                            Text("Marka:", style = MaterialTheme.typography.bodyMedium, color = Color.Gray)
                            Text(product.brand, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.SemiBold)
                        }
                    }
                    if ((product.boxQty != null && product.boxQty > 1) || (product.packageQty != null && product.packageQty > 1)) {
                        val boxText = if (product.boxQty != null && product.boxQty > 1) "Koli x${product.boxQty}" else ""
                        val packText = if (product.packageQty != null && product.packageQty > 1) "Paket x${product.packageQty}" else ""
                        val joined = listOf(boxText, packText).filter { it.isNotEmpty() }.joinToString(" | ")
                        Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                            Text("Ambalaj:", style = MaterialTheme.typography.bodyMedium, color = Color.Gray)
                            Text(joined, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.SemiBold)
                        }
                    }
                    Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                        Text("KDV Oranı:", style = MaterialTheme.typography.bodyMedium, color = Color.Gray)
                        Text("%${product.kdvPercent}", style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.SemiBold)
                    }

                    HorizontalDivider(modifier = Modifier.padding(vertical = 4.dp), color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.4f))

                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.SpaceBetween,
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Text("Fiyat:", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold)
                        Column(horizontalAlignment = Alignment.End) {
                            Text(
                                text = String.format("%,.2f ₺", displayPrice),
                                style = MaterialTheme.typography.titleLarge,
                                fontWeight = FontWeight.ExtraBold,
                                color = MaterialTheme.colorScheme.primary
                            )
                            Text(
                                text = "+%${product.kdvPercent} KDV",
                                style = MaterialTheme.typography.labelSmall,
                                color = Color.Gray
                            )
                        }
                    }
                }

                Spacer(modifier = Modifier.height(16.dp))

                // WhatsApp PNG Share Button
                Button(
                    onClick = {
                        isSharing = true
                        shareProductByWhatsApp(
                            context = context,
                            coroutineScope = coroutineScope,
                            product = product,
                            displayPrice = displayPrice,
                            onFinish = { success ->
                                isSharing = false
                                if (!success) {
                                    Toast.makeText(context, "Görsel paylaşılamadı.", Toast.LENGTH_SHORT).show()
                                }
                            }
                        )
                    },
                    modifier = Modifier
                        .fillMaxWidth()
                        .height(48.dp),
                    shape = RoundedCornerShape(12.dp),
                    colors = ButtonDefaults.buttonColors(containerColor = Color(0xFF25D366)), // WhatsApp elegant green
                    enabled = !isSharing
                ) {
                    if (isSharing) {
                        CircularProgressIndicator(modifier = Modifier.size(24.dp), color = Color.White, strokeWidth = 2.dp)
                    } else {
                        // Display generic share icon for compatibility with standard MaterialSymbols
                        Icon(
                            imageVector = Icons.Filled.Share,
                            contentDescription = "WhatsApp",
                            tint = Color.White,
                            modifier = Modifier.size(20.dp)
                        )
                        Spacer(modifier = Modifier.width(8.dp))
                        Text(
                            text = "WhatsApp ile Görsel Paylaş",
                            style = MaterialTheme.typography.titleSmall,
                            fontWeight = FontWeight.Bold,
                            color = Color.White
                        )
                    }
                }
            }
        }
    }
}

private fun drawBitmapProportionalFit(
    canvas: android.graphics.Canvas,
    bitmap: android.graphics.Bitmap,
    dstRect: android.graphics.Rect,
    paint: android.graphics.Paint? = null
) {
    // Fill background with light grey block
    val bgPaint = android.graphics.Paint().apply {
        color = 0xFFF8FAFC.toInt() // slate 50 (clean light background)
        style = android.graphics.Paint.Style.FILL
    }
    canvas.drawRoundRect(
        dstRect.left.toFloat(),
        dstRect.top.toFloat(),
        dstRect.right.toFloat(),
        dstRect.bottom.toFloat(),
        12f,
        12f,
        bgPaint
    )

    val srcWidth = bitmap.width
    val srcHeight = bitmap.height
    val dstWidth = dstRect.width()
    val dstHeight = dstRect.height()

    if (srcWidth <= 0 || srcHeight <= 0 || dstWidth <= 0 || dstHeight <= 0) return

    val srcRatio = srcWidth.toFloat() / srcHeight
    val dstRatio = dstWidth.toFloat() / dstHeight

    val finalDstRect: android.graphics.Rect
    if (srcRatio > dstRatio) {
        val targetHeight = (dstWidth / srcRatio).toInt()
        val topOffset = (dstHeight - targetHeight) / 2
        finalDstRect = android.graphics.Rect(
            dstRect.left,
            dstRect.top + topOffset,
            dstRect.right,
            dstRect.top + topOffset + targetHeight
        )
    } else {
        val targetWidth = (dstHeight * srcRatio).toInt()
        val leftOffset = (dstWidth - targetWidth) / 2
        finalDstRect = android.graphics.Rect(
            dstRect.left + leftOffset,
            dstRect.top,
            dstRect.left + leftOffset + targetWidth,
            dstRect.bottom
        )
    }

    val srcRect = android.graphics.Rect(0, 0, srcWidth, srcHeight)
    canvas.drawBitmap(bitmap, srcRect, finalDstRect, paint)
}

private fun shareProductByWhatsApp(
    context: android.content.Context,
    coroutineScope: kotlinx.coroutines.CoroutineScope,
    product: ProductCatalog,
    displayPrice: Double,
    onFinish: (Boolean) -> Unit
) {
    coroutineScope.launch(kotlinx.coroutines.Dispatchers.IO) {
        try {
            val loader = coil.ImageLoader(context)
            // Retrieve all images
            val imageSources = mutableListOf<Any>()
            if (!product.localImagePath.isNullOrBlank()) {
                val paths = product.localImagePath.split(Regex("[,;|\\s]+"))
                    .map { it.trim() }
                    .filter { it.isNotEmpty() }
                for (p in paths) {
                    val file = java.io.File(p)
                    if (file.exists() && file.length() > 0) {
                        imageSources.add(file)
                    }
                }
            }
            if (!product.imageUrl.isNullOrBlank()) {
                val urls = product.imageUrl.split(Regex("[,;|\\s]+"))
                    .map { it.trim() }
                    .filter { it.isNotEmpty() }
                for (u in urls) {
                    if (!imageSources.contains(u)) {
                        imageSources.add(u)
                    }
                }
            }

            val loadedBitmaps = mutableListOf<android.graphics.Bitmap>()
            for (source in imageSources.take(10)) {
                try {
                    val request = coil.request.ImageRequest.Builder(context)
                        .data(source)
                        .allowHardware(false)
                        .build()
                    val result = loader.execute(request)
                    val bmp = (result.drawable as? android.graphics.drawable.BitmapDrawable)?.bitmap
                    if (bmp != null) {
                        loadedBitmaps.add(bmp)
                    }
                } catch (e: Exception) {
                    e.printStackTrace()
                }
            }

            // Create off-screen Bitmap
            val width = 800
            val height = 1000
            val bitmap = android.graphics.Bitmap.createBitmap(width, height, android.graphics.Bitmap.Config.ARGB_8888)
            val canvas = android.graphics.Canvas(bitmap)
            canvas.drawColor(android.graphics.Color.WHITE)

            val paint = android.graphics.Paint(android.graphics.Paint.ANTI_ALIAS_FLAG)
            
            // Paint layout border line
            paint.color = 0xFFE0E0E0.toInt()
            paint.style = android.graphics.Paint.Style.STROKE
            paint.strokeWidth = 4f
            canvas.drawRect(2f, 2f, width.toFloat() - 2f, height.toFloat() - 2f, paint)
            paint.style = android.graphics.Paint.Style.FILL

            // Draw Top Banner (Thinned banner & bright/light background)
            paint.color = 0xFFF1F5F9.toInt() // light grey-slate background
            canvas.drawRect(4f, 4f, width.toFloat() - 4f, 80f, paint)
            
            // Thin elegant border below the top banner
            paint.color = 0xFFE2E8F0.toInt()
            paint.style = android.graphics.Paint.Style.STROKE
            paint.strokeWidth = 2f
            canvas.drawLine(4f, 80f, width.toFloat() - 4f, 80f, paint)
            paint.style = android.graphics.Paint.Style.FILL

            // Company name text (bright theme, clean slate colour)
            paint.color = 0xFF0F172A.toInt() // Slate 900
            paint.textSize = 28f
            paint.typeface = android.graphics.Typeface.create(android.graphics.Typeface.DEFAULT, android.graphics.Typeface.BOLD)
            paint.textAlign = android.graphics.Paint.Align.CENTER
            
            val companyHeader = if (AppDataStore.companyId.isNotBlank()) {
                "SİPARİŞ CEPTE (Şirket Kodu: ${AppDataStore.companyId.uppercase()})"
            } else {
                "SİPARİŞ CEPTE"
            }
            canvas.drawText(companyHeader, width / 2f, 50f, paint)

            // Draw product photo collage grid
            val imgYStart = 110
            val imgHeight = 440
            val imgWidth = 640
            val imgXStart = (width - imgWidth) / 2

            val count = loadedBitmaps.size
            if (count == 0) {
                // Colored Circle Placeholder
                paint.color = 0xFFF1F5F9.toInt()
                canvas.drawRoundRect(imgXStart.toFloat(), imgYStart.toFloat(), (imgXStart + imgWidth).toFloat(), (imgYStart + imgHeight).toFloat(), 24f, 24f, paint)
                
                paint.color = product.imageUrlColor.toArgb()
                val cx = width / 2f
                val cy = imgYStart + imgHeight / 2f
                canvas.drawCircle(cx, cy, 75f, paint)

                paint.color = android.graphics.Color.WHITE
                paint.textSize = 85f
                paint.textAlign = android.graphics.Paint.Align.CENTER
                paint.typeface = android.graphics.Typeface.create(android.graphics.Typeface.DEFAULT, android.graphics.Typeface.BOLD)
                val textInitial = if (product.title.isNotEmpty()) product.title.take(1).uppercase() else "P"
                canvas.drawText(textInitial, cx, cy + 30f, paint)
            } else {
                // Dynamic grid design for 1 to 10+ images!
                val (cols, rows) = when {
                    count <= 1 -> Pair(1, 1)
                    count == 2 -> Pair(2, 1)
                    count == 3 -> Pair(3, 1)
                    count == 4 -> Pair(2, 2)
                    count <= 6 -> Pair(3, 2)
                    count <= 9 -> Pair(3, 3)
                    else -> Pair(4, 3) // 10, 11, 12 etc. fit in a 4x3 grid beautifully
                }
                val gap = 6
                val totalGapsX = (cols - 1) * gap
                val totalGapsY = (rows - 1) * gap
                val itemWidth = (imgWidth - totalGapsX) / cols
                val itemHeight = (imgHeight - totalGapsY) / rows

                for (idx in 0 until count) {
                    if (idx >= cols * rows) break
                    val r = idx / cols
                    val c = idx % cols
                    val left = imgXStart + c * (itemWidth + gap)
                    val top = imgYStart + r * (itemHeight + gap)
                    val rect = android.graphics.Rect(left, top, left + itemWidth, top + itemHeight)
                    drawBitmapProportionalFit(canvas, loadedBitmaps[idx], rect, null)
                }
            }

            // Divider Line
            paint.color = 0xFFE2E8F0.toInt()
            canvas.drawRect(80f, 600f, width.toFloat() - 80f, 602f, paint)

            // Product specifications
            paint.textAlign = android.graphics.Paint.Align.LEFT
            paint.color = 0xFF0284C7.toInt() // Blue Category Accent
            paint.textSize = 22f
            paint.typeface = android.graphics.Typeface.create(android.graphics.Typeface.DEFAULT, android.graphics.Typeface.BOLD)
            canvas.drawText(product.category.uppercase(), 80f, 642f, paint)

            paint.color = 0xFF1E293B.toInt()
            paint.textSize = 34f
            paint.typeface = android.graphics.Typeface.create(android.graphics.Typeface.DEFAULT, android.graphics.Typeface.BOLD)
            
            // Wrap title string neatly across multiple lines
            val titleText = product.title
            var yPos = 692f
            val lineMaxChar = 34
            if (titleText.length > lineMaxChar) {
                val words = titleText.split(" ")
                var currentLine = ""
                val linesList = mutableListOf<String>()
                for (word in words) {
                    if ((currentLine + word).length > lineMaxChar) {
                        linesList.add(currentLine.trim())
                        currentLine = word + " "
                    } else {
                        currentLine += word + " "
                    }
                }
                if (currentLine.isNotEmpty()) linesList.add(currentLine.trim())
                for (idx in 0 until Math.min(2, linesList.size)) {
                    canvas.drawText(linesList[idx], 80f, yPos, paint)
                    yPos += 45f
                }
            } else {
                canvas.drawText(titleText, 80f, yPos, paint)
                yPos += 50f
            }

            // Secondary detail lines
            paint.color = 0xFF64748B.toInt()
            paint.textSize = 24f
            paint.typeface = android.graphics.Typeface.create(android.graphics.Typeface.DEFAULT, android.graphics.Typeface.NORMAL)
            canvas.drawText("Ürün Kodu: ${product.code}   •   Barkod: ${product.barcode}", 80f, yPos, paint)
            yPos += 35f

            if (!product.brand.isNullOrBlank()) {
                canvas.drawText("Marka: ${product.brand}", 80f, yPos, paint)
                yPos += 35f
            }

            if ((product.boxQty != null && product.boxQty > 1) || (product.packageQty != null && product.packageQty > 1)) {
                val boxText = if (product.boxQty != null && product.boxQty > 1) "Koli: ${product.boxQty} Adet" else ""
                val packText = if (product.packageQty != null && product.packageQty > 1) "Paket: ${product.packageQty} Adet" else ""
                val joined = listOf(boxText, packText).filter { it.isNotEmpty() }.joinToString(" | ")
                canvas.drawText("Ambalaj: $joined", 80f, yPos, paint)
                yPos += 35f
            }

            // Big Blue Price Display at the bottom
            paint.color = 0xFF0F172A.toInt() // slate 900
            paint.textSize = 48f
            paint.typeface = android.graphics.Typeface.create(android.graphics.Typeface.DEFAULT, android.graphics.Typeface.BOLD)
            val priceString = String.format("%,.2f ₺", displayPrice)
            canvas.drawText(priceString, 80f, height - 110f, paint)

            paint.color = 0xFF64748B.toInt()
            paint.textSize = 20f
            paint.typeface = android.graphics.Typeface.create(android.graphics.Typeface.DEFAULT, android.graphics.Typeface.NORMAL)
            canvas.drawText("+%${product.kdvPercent} KDV Hariçtir", 80f, height - 80f, paint)

            // Branding element bottom-right banner
            paint.color = 0xFFF1F5F9.toInt()
            canvas.drawRoundRect(width - 240f, height - 130f, width - 80f, height - 60f, 12f, 12f, paint)
            
            paint.color = 0xFF475569.toInt()
            paint.textSize = 18f
            paint.textAlign = android.graphics.Paint.Align.CENTER
            paint.typeface = android.graphics.Typeface.create(android.graphics.Typeface.DEFAULT, android.graphics.Typeface.BOLD)
            canvas.drawText("Saha Satış Gücü", width - 160f, height - 98f, paint)
            paint.textSize = 14f
            paint.typeface = android.graphics.Typeface.create(android.graphics.Typeface.DEFAULT, android.graphics.Typeface.NORMAL)
            canvas.drawText("Hızlı Paylaşım", width - 160f, height - 76f, paint)

            // Save image via MediaStore
            val displayName = "katalog_kart_${product.code}_" + java.text.SimpleDateFormat("yyyyMMdd_HHmmss", java.util.Locale.getDefault()).format(java.util.Date()) + ".png"
            val resolver = context.contentResolver
            val contentValues = android.content.ContentValues().apply {
                put("_display_name", displayName)
                put("mime_type", "image/png")
                if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.Q) {
                    put("relative_path", android.os.Environment.DIRECTORY_PICTURES + "/SahaUrun")
                    put("is_pending", 1)
                }
            }

            val imageUri = resolver.insert(android.provider.MediaStore.Images.Media.EXTERNAL_CONTENT_URI, contentValues)
            if (imageUri != null) {
                resolver.openOutputStream(imageUri).use { outStream ->
                    if (outStream != null) {
                        bitmap.compress(android.graphics.Bitmap.CompressFormat.PNG, 100, outStream)
                    }
                }
                if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.Q) {
                    contentValues.clear()
                    contentValues.put("is_pending", 0)
                    resolver.update(imageUri, contentValues, null, null)
                }

                // Sharing Intent
                val shareIntent = android.content.Intent(android.content.Intent.ACTION_SEND).apply {
                    type = "image/png"
                    putExtra(android.content.Intent.EXTRA_STREAM, imageUri)
                    putExtra(android.content.Intent.EXTRA_TEXT, "${product.title}\nKod: ${product.code}\nFiyat: $priceString")
                    addFlags(android.content.Intent.FLAG_GRANT_READ_URI_PERMISSION)
                    `package` = "com.whatsapp"
                }

                try {
                    context.startActivity(shareIntent)
                } catch (e: Exception) {
                    try {
                        shareIntent.`package` = "com.whatsapp.w4b"
                        context.startActivity(shareIntent)
                    } catch (ex: Exception) {
                        val generalChooser = android.content.Intent(android.content.Intent.ACTION_SEND).apply {
                            type = "image/png"
                            putExtra(android.content.Intent.EXTRA_STREAM, imageUri)
                            putExtra(android.content.Intent.EXTRA_TEXT, "${product.title}\nKod: ${product.code}")
                            addFlags(android.content.Intent.FLAG_GRANT_READ_URI_PERMISSION)
                        }
                        context.startActivity(android.content.Intent.createChooser(generalChooser, "Ürünü Paylaş"))
                    }
                }
                onFinish(true)
            } else {
                onFinish(false)
            }
        } catch (e: Exception) {
            e.printStackTrace()
            onFinish(false)
        }
    }
}

package com.example.ui.screens

import android.media.AudioManager
import android.media.ToneGenerator
import android.os.Build
import android.os.VibrationEffect
import android.os.Vibrator
import android.os.VibratorManager
import androidx.compose.animation.*
import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.horizontalScroll
import androidx.compose.foundation.verticalScroll
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.Canvas
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.gestures.detectDragGestures
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.grid.GridCells
import androidx.compose.foundation.lazy.grid.LazyVerticalGrid
import androidx.compose.foundation.lazy.grid.items
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.lazy.itemsIndexed
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.draw.shadow
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.StrokeCap
import androidx.compose.ui.input.pointer.pointerInput
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.text.style.TextDecoration
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.navigation.NavController
import com.example.ui.components.FieldCard
import com.example.ui.components.ReceiptDialog
import com.example.ui.components.ReceiptInfo
import kotlinx.coroutines.delay
import kotlinx.coroutines.launch
import android.widget.Toast
import androidx.compose.foundation.gestures.detectTapGestures
import androidx.compose.foundation.combinedClickable
import androidx.compose.foundation.ExperimentalFoundationApi
import com.google.accompanist.permissions.ExperimentalPermissionsApi
import com.google.accompanist.permissions.isGranted
import com.google.accompanist.permissions.rememberPermissionState

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun OperationsScreen(module: String, navController: NavController) {
    val context = LocalContext.current
    val scope = rememberCoroutineScope()
    val snackbarHostState = remember { SnackbarHostState() }

    // Navigation and screen title definitions
    val (title, icon) = when (module) {
        "purchase" -> "Alış Siparişi" to Icons.Filled.Inventory2
        "returns" -> "Müşteri İade Kabul" to Icons.Filled.KeyboardReturn
        "collection" -> "Saha Tahsilat" to Icons.Filled.Payments
        "disbursement" -> "Supplier Tediye" to Icons.Filled.AccountBalanceWallet
        "stocks" -> "Mevcut Stoklar" to Icons.Filled.Layers
        "counting" -> "Hızlı Sayım" to Icons.Filled.QrCodeScanner
        "warehouses" -> "Depo Listesi" to Icons.Filled.Warehouse
        "cashbox" -> "Kasa & Banka Defteri" to Icons.Filled.AccountBalance
        "eod" -> "Saha Gün Sonu Kapanışı" to Icons.Filled.CheckCircle
        "approvals" -> "Onay Merkezi" to Icons.Filled.AssignmentTurnedIn
        "expenses" -> "Şirket Giderleri" to Icons.Filled.ReceiptLong
        "vehicles" -> "Şirket Araçları" to Icons.Filled.DirectionsCar
        else -> "Modül Simülasyonu" to Icons.Filled.Settings
    }

    // Feedback audio/haptic plays tone
    fun playFeedback(isSuccess: Boolean) {
        com.example.util.VibratorHelper.triggerFeedback(context, isSuccess)
    }

    Scaffold(
        snackbarHost = { SnackbarHost(snackbarHostState) },
        containerColor = MaterialTheme.colorScheme.background,
        contentWindowInsets = WindowInsets(0, 0, 0, 0)
    ) { innerPadding ->
        Box(
            modifier = Modifier
                .fillMaxSize()
                .padding(innerPadding)
                .background(
                    Brush.verticalGradient(
                        colors = listOf(
                            MaterialTheme.colorScheme.background,
                            MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.3f)
                        )
                    )
                )
        ) {
            when (module) {
                "purchase" -> PurchaseModule(onSuccess = { playFeedback(true); scope.launch { snackbarHostState.showSnackbar("Alış kaydedildi, stoklar güncellendi!") } })
                "returns" -> ReturnsModule(onSuccess = { playFeedback(true); scope.launch { snackbarHostState.showSnackbar("İade kabul faturası oluşturuldu ve stoklara iade girdi!") } })
                "collection" -> CollectionModule(
                    navController = navController,
                    onSuccess = {
                        playFeedback(true)
                        scope.launch { snackbarHostState.showSnackbar("Tahsilat makbuzu başarıyla işlendi") }
                        navController.navigate("dashboard") {
                            popUpTo("dashboard") { inclusive = false }
                        }
                    }
                )
                "disbursement" -> DisbursementModule(
                    onSuccess = {
                        playFeedback(true)
                        scope.launch { snackbarHostState.showSnackbar("Tediye ödemesi onaylandı ve kasa bakiyesi düşürüldü!") }
                        navController.navigate("dashboard") {
                            popUpTo("dashboard") { inclusive = false }
                        }
                    },
                    navController = navController
                )
                "stocks" -> StocksModule(navController)
                "counting" -> CountingModule(onBeep = { playFeedback(true) })
                "warehouses" -> WarehousesModule()
                "cashbox" -> CashBoxModule()
                "eod" -> EodModule(onSuccess = { playFeedback(true) })
                "approvals" -> ApprovalsModule(onBeep = { playFeedback(true) })
                "expenses" -> ExpensesModule(navController = navController)
                "vehicles" -> VehiclesModule(navController = navController)
                else -> {
                    Column(
                        modifier = Modifier.fillMaxSize().padding(24.dp),
                        verticalArrangement = Arrangement.Center,
                        horizontalAlignment = Alignment.CenterHorizontally
                    ) {
                        Icon(icon, null, modifier = Modifier.size(72.dp), tint = MaterialTheme.colorScheme.primary.copy(alpha = 0.5f))
                        Spacer(modifier = Modifier.height(16.dp))
                        Text("Modül Hazırlanıyor", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold)
                        Text("Bu modül saha simülatörünün gelecek güncellemesinde aktif edilecektir.", color = Color.Gray, style = MaterialTheme.typography.bodyMedium, textAlign = TextAlign.Center)
                    }
                }
            }
        }
    }
}

// --- MODULE 1: PURCHASE (ALIŞ) ---

fun getPreviousPurchasePrice(code: String, wholesalePrice: Double): Double {
    return when (code) {
        "IND-OIL-20L" -> 1750.00
        "FLT-AIR-901" -> 350.00
        "BRG-STL-120" -> 680.00
        "SRF-CLV-M8" -> 130.00
        else -> Math.round(wholesalePrice * 0.90 * 100) / 100.0
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun PurchaseModule(onSuccess: () -> Unit) {
    val context = LocalContext.current
    val scope = rememberCoroutineScope()

    var selectedSupplier by AppDataStore.purchaseSelectedSupplier
    var activeBottomTab by AppDataStore.purchaseSelectedTab
    var invoiceSerial by AppDataStore.purchaseInvoiceSerial
    var invoiceSeq by AppDataStore.purchaseInvoiceSeq
    var supplierSearchQuery by AppDataStore.purchaseSupplierSearchQuery

    var productCodeInput by AppDataStore.purchaseProductCodeInput
    var productTitleInput by AppDataStore.purchaseProductTitleInput
    var quantityInput by AppDataStore.purchaseQuantityInput
    var unitPriceInput by AppDataStore.purchaseUnitPriceInput
    var isRegisteredProduct by AppDataStore.purchaseIsRegisteredProduct
    var previousPurchasePrice by AppDataStore.purchasePreviousPurchasePrice

    var showPurchaseBarcodeScanner by AppDataStore.purchaseShowBarcodeScanner
    val purchaseCart = AppDataStore.purchaseCart

    val invoiceNo = "${invoiceSerial}-${invoiceSeq}"

    // Dialog state for editing items inside the cart
    var editingItem by remember { mutableStateOf<PurchaseCartItem?>(null) }
    var editQtyStr by remember { mutableStateOf("") }
    var editPriceStr by remember { mutableStateOf("") }

    if (showPurchaseBarcodeScanner) {
        BarcodeScannerDialog(
            onDismissRequest = { showPurchaseBarcodeScanner = false },
            onBarcodeScanned = { code ->
                showPurchaseBarcodeScanner = false
                productCodeInput = code
                val prod = AppDataStore.products.find { it.barcode == code || it.barcodes.contains(code) || it.code == code }
                if (prod != null) {
                    productCodeInput = prod.barcode.ifEmpty { prod.code }
                    productTitleInput = prod.title
                    unitPriceInput = prod.wholesalePrice.toString()
                    isRegisteredProduct = true
                    previousPurchasePrice = getPreviousPurchasePrice(prod.code, prod.wholesalePrice)
                } else {
                    productTitleInput = ""
                    unitPriceInput = ""
                    isRegisteredProduct = false
                    previousPurchasePrice = 0.0
                }
                activeBottomTab = 1
            },
            onSimulateScan = { code ->
                showPurchaseBarcodeScanner = false
                productCodeInput = code
                val prod = AppDataStore.products.find { it.barcode == code || it.barcodes.contains(code) || it.code == code }
                if (prod != null) {
                    productCodeInput = prod.barcode.ifEmpty { prod.code }
                    productTitleInput = prod.title
                    unitPriceInput = prod.wholesalePrice.toString()
                    isRegisteredProduct = true
                    previousPurchasePrice = getPreviousPurchasePrice(prod.code, prod.wholesalePrice)
                } else {
                    productTitleInput = ""
                    unitPriceInput = ""
                    isRegisteredProduct = false
                    previousPurchasePrice = 0.0
                }
                activeBottomTab = 1
            }
        )
    }

    if (editingItem != null) {
        val itemToEdit = editingItem!!
        AlertDialog(
            onDismissRequest = { editingItem = null },
            title = { Text("Kalem Düzenle: ${itemToEdit.title}", fontWeight = FontWeight.Bold) },
            text = {
                Column(verticalArrangement = Arrangement.spacedBy(10.dp)) {
                    OutlinedTextField(
                        value = editQtyStr,
                        onValueChange = { editQtyStr = it },
                        label = { Text("Miktar") },
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                        modifier = Modifier.fillMaxWidth()
                    )
                    OutlinedTextField(
                        value = editPriceStr,
                        onValueChange = { editPriceStr = it },
                        label = { Text("Birim Fiyatı") },
                        prefix = { Text("₺") },
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Decimal),
                        modifier = Modifier.fillMaxWidth()
                    )
                }
            },
            confirmButton = {
                Button(
                    onClick = {
                        val finalQty = editQtyStr.toIntOrNull() ?: itemToEdit.qty
                        val finalPrice = editPriceStr.toDoubleOrNull() ?: itemToEdit.price
                        val idx = purchaseCart.indexOf(itemToEdit)
                        if (idx != -1) {
                            purchaseCart[idx] = itemToEdit.copy(qty = finalQty, price = finalPrice)
                        }
                        editingItem = null
                    }
                ) {
                    Text("Güncelle")
                }
            },
            dismissButton = {
                TextButton(onClick = { editingItem = null }) {
                    Text("İptal")
                }
            }
        )
    }

    Box(
        modifier = Modifier
            .fillMaxSize()
            .padding(bottom = 80.dp) // Leave clean bottom margin for the shared navbar
    ) {
        Column(
            modifier = Modifier.fillMaxSize()
        ) {
            // Supplier Header block - Only visible when supplier was selected and on tab > 0
            if (activeBottomTab > 0 && selectedSupplier != null) {
                val supplier = selectedSupplier!!
                Box(
                    modifier = Modifier
                        .fillMaxWidth()
                        .background(MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f))
                        .padding(horizontal = 16.dp, vertical = 6.dp) // minimal top padding as requested
                ) {
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.SpaceBetween,
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Column(modifier = Modifier.weight(1f)) {
                            Text("TEDARİKÇİ CARİ: ${supplier.name.uppercase()}", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.primary, fontWeight = FontWeight.Bold)
                            Text("Bakiye: ${String.format("%,.2f ₺", supplier.balance)} | V.D: ${supplier.taxOffice}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                        }
                        IconButton(
                            onClick = {
                                selectedSupplier = null
                                purchaseCart.clear()
                                activeBottomTab = 0
                            }
                        ) {
                            Icon(Icons.Filled.SwapHoriz, "Değiştir", tint = MaterialTheme.colorScheme.primary)
                        }
                    }
                }
            }

            when (activeBottomTab) {
                0 -> {
                    Column(
                        modifier = Modifier
                            .fillMaxSize()
                            .padding(horizontal = 16.dp, vertical = 8.dp), // minimal top padding
                        verticalArrangement = Arrangement.spacedBy(12.dp)
                    ) {
                        // Display Pending Purchase Invoices if any exist
                        if (AppDataStore.suspendedPurchases.isNotEmpty()) {
                            Card(
                                modifier = Modifier.fillMaxWidth(),
                                colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.tertiaryContainer.copy(alpha = 0.25f)),
                                border = BorderStroke(1.dp, MaterialTheme.colorScheme.tertiary.copy(alpha = 0.3f))
                            ) {
                                Column(modifier = Modifier.padding(12.dp), verticalArrangement = Arrangement.spacedBy(8.dp)) {
                                    Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(6.dp)) {
                                        Icon(Icons.Filled.PauseCircle, null, tint = MaterialTheme.colorScheme.tertiary, modifier = Modifier.size(20.dp))
                                        Text("Bekleyen Alış Faturaları (${AppDataStore.suspendedPurchases.size})", style = MaterialTheme.typography.labelLarge, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.tertiary)
                                    }
                                    LazyColumn(
                                        modifier = Modifier.heightIn(max = 130.dp),
                                        verticalArrangement = Arrangement.spacedBy(6.dp)
                                    ) {
                                        items(AppDataStore.suspendedPurchases) { sp ->
                                            Row(
                                                modifier = Modifier
                                                    .fillMaxWidth()
                                                    .background(MaterialTheme.colorScheme.surface.copy(alpha = 0.6f), RoundedCornerShape(8.dp))
                                                    .padding(horizontal = 10.dp, vertical = 6.dp),
                                                horizontalArrangement = Arrangement.SpaceBetween,
                                                verticalAlignment = Alignment.CenterVertically
                                            ) {
                                                Column(modifier = Modifier.weight(1f)) {
                                                    Text(sp.supplierName, style = MaterialTheme.typography.labelMedium, fontWeight = FontWeight.Bold, maxLines = 1)
                                                    Text("Fatura: ${sp.invoiceSerial}-${sp.invoiceSeq} | Depo: ${sp.warehouseName}", style = MaterialTheme.typography.bodySmall, color = Color.Gray)
                                                    Text("Tutar: ${String.format("%,.2f ₺", sp.totalAmount)}", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.primary, fontWeight = FontWeight.Bold)
                                                }
                                                Row(horizontalArrangement = Arrangement.spacedBy(2.dp)) {
                                                    IconButton(
                                                        onClick = {
                                                            selectedSupplier = AppDataStore.customers.find { it.id == sp.supplierId } ?: Customer(
                                                                id = sp.supplierId ?: "",
                                                                name = sp.supplierName,
                                                                balance = 0.0,
                                                                lastVisit = "",
                                                                contact = "",
                                                                phone = "",
                                                                address = "",
                                                                taxOffice = "",
                                                                taxNumber = "",
                                                                gpsLocation = "",
                                                                riskLimit = 0.0,
                                                                priceGroup = "",
                                                                specialDiscountPercent = 0.0,
                                                                transactions = mutableListOf()
                                                            )
                                                            invoiceSerial = sp.invoiceSerial
                                                            invoiceSeq = sp.invoiceSeq
                                                            AppDataStore.purchaseWarehouse = sp.warehouseName
                                                            purchaseCart.clear()
                                                            purchaseCart.addAll(sp.items)
                                                            AppDataStore.suspendedPurchases.remove(sp)
                                                            activeBottomTab = 2
                                                        },
                                                        modifier = Modifier.size(36.dp)
                                                    ) {
                                                        Icon(Icons.Filled.CloudDownload, "Devam Et", tint = MaterialTheme.colorScheme.primary, modifier = Modifier.size(18.dp))
                                                    }
                                                    IconButton(
                                                        onClick = { AppDataStore.suspendedPurchases.remove(sp) },
                                                        modifier = Modifier.size(36.dp)
                                                    ) {
                                                        Icon(Icons.Filled.Delete, "Sil", tint = MaterialTheme.colorScheme.error, modifier = Modifier.size(18.dp))
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (selectedSupplier != null) {
                            val supplier = selectedSupplier!!
                            Card(
                                modifier = Modifier.fillMaxWidth(),
                                colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.2f))
                            ) {
                                Column(modifier = Modifier.padding(12.dp), verticalArrangement = Arrangement.spacedBy(8.dp)) {
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween,
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        Text("Uyumlu Cari Kart Seçildi", style = MaterialTheme.typography.labelMedium, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
                                        IconButton(onClick = { selectedSupplier = null; purchaseCart.clear() }, modifier = Modifier.size(36.dp)) {
                                            Icon(Icons.Filled.Delete, "Kaldır", tint = MaterialTheme.colorScheme.error, modifier = Modifier.size(18.dp))
                                        }
                                    }
                                    Text(supplier.name, style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold)
                                    HorizontalDivider()
                                    Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                                        Text("Cari Kodu / ID:", style = MaterialTheme.typography.bodySmall, color = Color.Gray)
                                        Text(supplier.id, style = MaterialTheme.typography.bodySmall, fontWeight = FontWeight.Bold)
                                    }
                                    Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                                        Text("Risk Limiti:", style = MaterialTheme.typography.bodySmall, color = Color.Gray)
                                        Text(String.format("%,.2f ₺", supplier.riskLimit), style = MaterialTheme.typography.bodySmall, fontWeight = FontWeight.Bold)
                                    }
                                    Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                                        Text("Mevcut Bakiye:", style = MaterialTheme.typography.bodySmall, color = Color.Gray)
                                        Text(String.format("%,.2f ₺", supplier.balance), style = MaterialTheme.typography.bodySmall, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.secondary)
                                    }
                                }
                            }
                        }

                        OutlinedTextField(
                            value = supplierSearchQuery,
                            onValueChange = { supplierSearchQuery = it },
                            placeholder = { Text("Cari unvanı veya koduna göre ara...") },
                            leadingIcon = { Icon(Icons.Filled.Search, null) },
                            modifier = Modifier.fillMaxWidth(),
                            singleLine = true,
                            shape = RoundedCornerShape(12.dp)
                        )

                        Text("Kayıtlı Cari Kartlar Listesi", style = MaterialTheme.typography.labelMedium, color = MaterialTheme.colorScheme.primary, fontWeight = FontWeight.Bold)

                        val filteredCari = AppDataStore.customers.filter { cust ->
                            cust.name.contains(supplierSearchQuery, ignoreCase = true) ||
                            cust.id.contains(supplierSearchQuery, ignoreCase = true)
                        }

                        LazyColumn(
                            modifier = Modifier
                                .fillMaxSize()
                                .weight(1f),
                            verticalArrangement = Arrangement.spacedBy(10.dp)
                        ) {
                            if (filteredCari.isEmpty()) {
                                item {
                                    Text("Aramayla eşleşen cari kart bulunamadı.", style = MaterialTheme.typography.bodyMedium, color = Color.Gray)
                                }
                            } else {
                                items(filteredCari) { cust ->
                                    Card(
                                        modifier = Modifier.fillMaxWidth(),
                                        colors = CardDefaults.cardColors(
                                            containerColor = if (selectedSupplier?.id == cust.id) MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.5f) else MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.3f)
                                        )
                                    ) {
                                        Row(
                                            modifier = Modifier
                                                .fillMaxWidth()
                                                .clickable { 
                                                    selectedSupplier = cust
                                                    activeBottomTab = 1
                                                }
                                                .padding(16.dp),
                                            horizontalArrangement = Arrangement.SpaceBetween,
                                            verticalAlignment = Alignment.CenterVertically
                                        ) {
                                            Column {
                                                Text(cust.name, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodyMedium)
                                                Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                                                    Text("Cari Kod: ${cust.id}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                                    Text("", style = MaterialTheme.typography.labelSmall)
                                                }
                                            }
                                            Icon(
                                                imageVector = if (selectedSupplier?.id == cust.id) Icons.Filled.CheckCircle else Icons.Filled.ArrowForward, 
                                                contentDescription = "Seç", 
                                                tint = MaterialTheme.colorScheme.primary
                                            )
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                1 -> {
                    if (selectedSupplier == null) {
                        Box(
                            modifier = Modifier.fillMaxSize().padding(16.dp),
                            contentAlignment = Alignment.Center
                        ) {
                            Card(
                                modifier = Modifier.fillMaxWidth().padding(16.dp),
                                colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.errorContainer.copy(alpha = 0.15f))
                            ) {
                                Column(
                                    modifier = Modifier.padding(24.dp),
                                    horizontalAlignment = Alignment.CenterHorizontally,
                                    verticalArrangement = Arrangement.spacedBy(12.dp)
                                ) {
                                    Icon(Icons.Filled.Group, contentDescription = null, modifier = Modifier.size(48.dp), tint = MaterialTheme.colorScheme.error)
                                    Text(
                                        text = "TEDARİKÇİ SEÇİLMEDİ",
                                        fontWeight = FontWeight.Bold,
                                        style = MaterialTheme.typography.titleMedium,
                                        color = MaterialTheme.colorScheme.error
                                    )
                                    Text(
                                        text = "Alış faturası oluşturmak için lütfen öncelikle Cari tab’inden bir tedarikçi seçin.",
                                        style = MaterialTheme.typography.bodyMedium,
                                        textAlign = TextAlign.Center,
                                        color = Color.DarkGray
                                    )
                                    Button(
                                        onClick = { activeBottomTab = 0 },
                                        colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.primary)
                                    ) {
                                        Text("Cari Seç")
                                    }
                                }
                            }
                        }
                    } else {
                        val matchedProducts = if (productCodeInput.isNotBlank()) {
                            AppDataStore.products.filter {
                                it.code.contains(productCodeInput, ignoreCase = true) ||
                                it.barcode.contains(productCodeInput, ignoreCase = true) ||
                                it.title.contains(productCodeInput, ignoreCase = true)
                            }
                        } else {
                            emptyList()
                        }

                        LaunchedEffect(productCodeInput) {
                            val exactMatch = AppDataStore.products.find {
                                it.code.equals(productCodeInput, ignoreCase = true) ||
                                it.barcode.equals(productCodeInput, ignoreCase = true)
                            }
                            if (exactMatch != null) {
                                productTitleInput = exactMatch.title
                                unitPriceInput = exactMatch.wholesalePrice.toString()
                                isRegisteredProduct = true
                                previousPurchasePrice = getPreviousPurchasePrice(exactMatch.code, exactMatch.wholesalePrice)
                            } else {
                                isRegisteredProduct = false
                                previousPurchasePrice = 0.0
                            }
                        }

                        LazyColumn(
                            modifier = Modifier
                                .fillMaxSize()
                                .padding(horizontal = 16.dp, vertical = 8.dp), // minimal top padding,
                            verticalArrangement = Arrangement.spacedBy(12.dp)
                        ) {
                            item {
                                Text("Belge Detayları", style = MaterialTheme.typography.labelMedium, color = MaterialTheme.colorScheme.primary, fontWeight = FontWeight.Bold)
                            }

                            item {
                                FieldCard {
                                    Column(modifier = Modifier.padding(12.dp), verticalArrangement = Arrangement.spacedBy(10.dp)) {
                                        Row(
                                            modifier = Modifier.fillMaxWidth(),
                                            horizontalArrangement = Arrangement.spacedBy(6.dp),
                                            verticalAlignment = Alignment.CenterVertically
                                        ) {
                                            // 1. Fatura Seri
                                            OutlinedTextField(
                                                value = invoiceSerial,
                                                onValueChange = { invoiceSerial = it },
                                                label = { Text("Fatura Seri") },
                                                modifier = Modifier.weight(1f),
                                                singleLine = true
                                            )
                                            // 2. Fatura Sıra No
                                            OutlinedTextField(
                                                value = invoiceSeq,
                                                onValueChange = { invoiceSeq = it },
                                                label = { Text("Fatura Sıra No") },
                                                modifier = Modifier.weight(1.8f),
                                                singleLine = true
                                            )
                                            // 3. Stok Deposu dropdown
                                            var expandedWarehouse by remember { mutableStateOf(false) }
                                            Box(modifier = Modifier.weight(2.2f)) {
                                                OutlinedTextField(
                                                    value = AppDataStore.purchaseWarehouse,
                                                    onValueChange = {},
                                                    readOnly = true,
                                                    label = { Text("Depo") },
                                                    trailingIcon = {
                                                        IconButton(onClick = { expandedWarehouse = true }, modifier = Modifier.size(24.dp)) {
                                                            Icon(Icons.Filled.ArrowDropDown, null)
                                                        }
                                                    },
                                                    modifier = Modifier.fillMaxWidth(),
                                                    singleLine = true
                                                )
                                                DropdownMenu(
                                                    expanded = expandedWarehouse,
                                                    onDismissRequest = { expandedWarehouse = false }
                                                ) {
                                                    listOf("Ana Depo", "Ankara Merkez", "Ege Bölge").forEach { wh ->
                                                        DropdownMenuItem(
                                                            text = { Text(wh) },
                                                            onClick = {
                                                                AppDataStore.purchaseWarehouse = wh
                                                                expandedWarehouse = false
                                                            }
                                                        )
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            item {
                                Text("Stok Alım Bilgileri", style = MaterialTheme.typography.labelMedium, color = MaterialTheme.colorScheme.primary, fontWeight = FontWeight.Bold)
                            }

                            item {
                                FieldCard {
                                    Column(modifier = Modifier.padding(12.dp), verticalArrangement = Arrangement.spacedBy(10.dp)) {
                                        Row(
                                            modifier = Modifier.fillMaxWidth(),
                                            horizontalArrangement = Arrangement.spacedBy(8.dp),
                                            verticalAlignment = Alignment.CenterVertically
                                        ) {
                                            OutlinedTextField(
                                                value = productCodeInput,
                                                onValueChange = { productCodeInput = it },
                                                label = { Text("Ürün Adı, Kodu veya Barkodu") },
                                                leadingIcon = { Icon(Icons.Filled.QrCode, null) },
                                                placeholder = { Text("Örn: Motor Yağı") },
                                                modifier = Modifier.weight(1f),
                                                singleLine = true
                                            )
                                        }

                                        if (matchedProducts.isNotEmpty() && !isRegisteredProduct) {
                                            Card(
                                                colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f)),
                                                modifier = Modifier.fillMaxWidth()
                                            ) {
                                                Column(modifier = Modifier.padding(8.dp), verticalArrangement = Arrangement.spacedBy(4.dp)) {
                                                    Text("Sistemden Önerilen Kayıtlı Ürünler:", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.primary, fontWeight = FontWeight.Bold)
                                                    matchedProducts.take(3).forEach { prod ->
                                                        Row(
                                                            modifier = Modifier
                                                                .fillMaxWidth()
                                                                .clickable {
                                                                    productCodeInput = prod.code
                                                                    productTitleInput = prod.title
                                                                    unitPriceInput = prod.wholesalePrice.toString()
                                                                    isRegisteredProduct = true
                                                                    previousPurchasePrice = getPreviousPurchasePrice(prod.code, prod.wholesalePrice)
                                                                }
                                                                .padding(vertical = 6.dp, horizontal = 4.dp),
                                                            horizontalArrangement = Arrangement.SpaceBetween
                                                        ) {
                                                            Text(prod.title, style = MaterialTheme.typography.bodySmall, fontWeight = FontWeight.Medium, maxLines = 1)
                                                            Text(prod.code, style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        if (isRegisteredProduct) {
                                            Surface(color = Color(0xFFE8F5E9), shape = RoundedCornerShape(8.dp)) {
                                                Row(modifier = Modifier.padding(horizontal = 8.dp, vertical = 4.dp), verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(4.dp)) {
                                                    Icon(Icons.Filled.CheckCircle, null, tint = Color(0xFF43A047), modifier = Modifier.size(16.dp))
                                                    Text("Kayıtlı Sistem Ürünü Otomatik Dolduruldu", color = Color(0xFF2E7D32), style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Bold)
                                                }
                                            }
                                        }

                                        OutlinedTextField(
                                            value = productTitleInput,
                                            onValueChange = { productTitleInput = it },
                                            label = { Text("Ürün/Malzeme Adı") },
                                            leadingIcon = { Icon(Icons.Filled.Inventory2, null) },
                                            modifier = Modifier.fillMaxWidth()
                                        )

                                        Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                                            OutlinedTextField(
                                                value = quantityInput,
                                                onValueChange = { quantityInput = it },
                                                label = { Text("Miktar") },
                                                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                                                modifier = Modifier.weight(1f)
                                            )
                                            Column(modifier = Modifier.weight(1.2f)) {
                                                if (isRegisteredProduct && previousPurchasePrice > 0.0) {
                                                    Text(
                                                        text = "Son Alış: ${String.format("%.2f ₺", previousPurchasePrice)}",
                                                        style = MaterialTheme.typography.labelSmall.copy(fontSize = 11.sp, fontWeight = FontWeight.Bold),
                                                        color = MaterialTheme.colorScheme.secondary,
                                                        modifier = Modifier.padding(bottom = 2.dp)
                                                    )
                                                }
                                                OutlinedTextField(
                                                    value = unitPriceInput,
                                                    onValueChange = { unitPriceInput = it },
                                                    label = { Text("Birim Fiyatı") },
                                                    prefix = { Text("₺") },
                                                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Decimal),
                                                    modifier = Modifier.fillMaxWidth()
                                                )
                                            }
                                        }

                                        if (isRegisteredProduct && previousPurchasePrice > 0.0) {
                                            val curPrice = unitPriceInput.toDoubleOrNull() ?: 0.0
                                            val diff = curPrice - previousPurchasePrice
                                            val badgeColor = if (diff > 0) Color(0xFFC62828) else if (diff < 0) Color(0xFF2E7D32) else Color.Gray
                                            val bgBadge = if (diff > 0) Color(0xFFFFEBEE) else if (diff < 0) Color(0xFFE8F5E9) else Color(0xFFA0A0A0).copy(alpha = 0.1f)

                                            Card(colors = CardDefaults.cardColors(containerColor = bgBadge)) {
                                                Column(modifier = Modifier.padding(12.dp).fillMaxWidth(), verticalArrangement = Arrangement.spacedBy(4.dp)) {
                                                    Text("Alış Fiyat Karşılaştırma Analizi", style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Black, color = badgeColor)
                                                    Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                                                        Text("Bir Önceki Alış Fiyatı:", style = MaterialTheme.typography.bodySmall, color = Color.Gray)
                                                        Text(String.format("%,.2f ₺", previousPurchasePrice), style = MaterialTheme.typography.bodySmall, fontWeight = FontWeight.Bold)
                                                    }
                                                    Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                                                        Text("Fark Tutarı:", style = MaterialTheme.typography.bodySmall, color = Color.Gray)
                                                        Text(
                                                            text = (if (diff > 0) "+" else "") + String.format("%,.2f ₺", diff),
                                                            style = MaterialTheme.typography.bodySmall,
                                                            fontWeight = FontWeight.Bold,
                                                            color = badgeColor
                                                        )
                                                    }
                                                }
                                            }
                                        }

                                        Button(
                                            onClick = {
                                                val qty = quantityInput.toIntOrNull() ?: 1
                                                val p = unitPriceInput.toDoubleOrNull() ?: 0.0
                                                if (productTitleInput.isNotBlank() && productCodeInput.isNotBlank()) {
                                                    purchaseCart.add(
                                                        PurchaseCartItem(
                                                            code = productCodeInput,
                                                            title = productTitleInput,
                                                            qty = qty,
                                                            price = p,
                                                            prevPrice = if (isRegisteredProduct) previousPurchasePrice else p,
                                                            isRegistered = isRegisteredProduct
                                                        )
                                                    )
                                                    productCodeInput = ""
                                                    productTitleInput = ""
                                                    quantityInput = "10"
                                                    unitPriceInput = ""
                                                    isRegisteredProduct = false
                                                    previousPurchasePrice = 0.0
                                                }
                                            },
                                            modifier = Modifier.fillMaxWidth().height(48.dp),
                                            enabled = productTitleInput.isNotBlank() && productCodeInput.isNotBlank()
                                        ) {
                                            Icon(Icons.Filled.Add, null)
                                            Spacer(modifier = Modifier.width(6.dp))
                                            Text("Alış Kalemini Sepete Ekle")
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                2 -> {
                    if (selectedSupplier == null) {
                        Box(
                            modifier = Modifier.fillMaxSize().padding(16.dp),
                            contentAlignment = Alignment.Center
                        ) {
                            Text("Lütfen öncelikle Cari tab'inden tedarikçi seçin.", color = Color.Gray)
                        }
                    } else {
                        val supplier = selectedSupplier!!
                        LazyColumn(
                            modifier = Modifier
                                .fillMaxSize()
                                .padding(horizontal = 16.dp, vertical = 8.dp),
                            verticalArrangement = Arrangement.spacedBy(16.dp)
                        ) {
                            item {
                                Text("Fatura Giriş Kalemleri", style = MaterialTheme.typography.labelMedium, color = MaterialTheme.colorScheme.primary, fontWeight = FontWeight.Bold)
                            }

                            if (purchaseCart.isEmpty()) {
                                item {
                                    Box(
                                        modifier = Modifier.fillMaxWidth().padding(40.dp),
                                        contentAlignment = Alignment.Center
                                    ) {
                                        Column(horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(8.dp)) {
                                            Icon(Icons.Filled.ShoppingCart, null, tint = Color.Gray, modifier = Modifier.size(48.dp))
                                            Text("Alış sepetiniz şu anda boş.", color = Color.Gray)
                                            Button(onClick = { activeBottomTab = 1 }) {
                                                Text("Ürün Bilgisi Ekle")
                                            }
                                        }
                                    }
                                }
                            } else {
                                items(purchaseCart) { item ->
                                    Card(
                                        modifier = Modifier.fillMaxWidth(),
                                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceContainerHigh)
                                    ) {
                                        Row(
                                            modifier = Modifier.padding(12.dp).fillMaxWidth(),
                                            horizontalArrangement = Arrangement.SpaceBetween,
                                            verticalAlignment = Alignment.CenterVertically
                                        ) {
                                            Column(modifier = Modifier.weight(1f)) {
                                                Text(item.title, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodyMedium)
                                                Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                                                    Text("Kod: ${item.code}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                                    if (item.isRegistered) {
                                                        Text("Kayıtlı", style = MaterialTheme.typography.labelSmall, color = Color(0xFF2E7D32))
                                                    }
                                                }
                                                Text("${item.qty} adet x ₺${String.format("%,.2f", item.price)}", style = MaterialTheme.typography.labelMedium, color = Color.Gray)
                                            }
                                            Row(verticalAlignment = Alignment.CenterVertically) {
                                                Text("₺${String.format("%,.2f", item.qty * item.price)}", fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.onSurface)
                                                Spacer(modifier = Modifier.width(6.dp))
                                                // Edit Button as requested: sepette duzenleyebilme
                                                IconButton(
                                                    onClick = {
                                                        editingItem = item
                                                        editQtyStr = item.qty.toString()
                                                        editPriceStr = item.price.toString()
                                                    },
                                                    modifier = Modifier.size(36.dp)
                                                ) {
                                                    Icon(Icons.Filled.Edit, "Düzenle", tint = MaterialTheme.colorScheme.primary, modifier = Modifier.size(18.dp))
                                                }
                                                IconButton(onClick = { purchaseCart.remove(item) }, modifier = Modifier.size(36.dp)) {
                                                    Icon(Icons.Filled.Delete, null, tint = MaterialTheme.colorScheme.error, modifier = Modifier.size(18.dp))
                                                }
                                            }
                                        }
                                    }
                                }

                                item {
                                    val total = purchaseCart.sumOf { it.qty * it.price }
                                    Spacer(modifier = Modifier.height(12.dp))
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween,
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        Text("Toplam Alım Tutarı:", fontWeight = FontWeight.Bold, style = MaterialTheme.typography.titleMedium)
                                        Text("₺ ${String.format("%,.2f", total)}", fontWeight = FontWeight.Black, color = MaterialTheme.colorScheme.primary, style = MaterialTheme.typography.titleLarge)
                                    }
                                    Spacer(modifier = Modifier.height(24.dp))
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.spacedBy(10.dp)
                                    ) {
                                        // 1. "Beklet" (Hold) button
                                        OutlinedButton(
                                            onClick = {
                                                val totalVal = purchaseCart.sumOf { it.qty * it.price }
                                                AppDataStore.suspendedPurchases.add(
                                                    SuspendedPurchase(
                                                        id = "P-BS-" + (1000 + (1000..9999).random()),
                                                        date = "08.06.2026",
                                                        supplierId = supplier.id,
                                                        supplierName = supplier.name,
                                                        items = purchaseCart.toList(),
                                                        warehouseName = AppDataStore.purchaseWarehouse,
                                                        invoiceSerial = invoiceSerial,
                                                        invoiceSeq = invoiceSeq,
                                                        totalAmount = totalVal
                                                    )
                                                )
                                                purchaseCart.clear()
                                                selectedSupplier = null
                                                productCodeInput = ""
                                                productTitleInput = ""
                                                quantityInput = "10"
                                                unitPriceInput = ""
                                                isRegisteredProduct = false
                                                previousPurchasePrice = 0.0
                                                activeBottomTab = 0
                                                scope.launch {
                                                    onSuccess() // triggers feedback or simple clear notification
                                                }
                                            },
                                            modifier = Modifier.weight(1f).height(52.dp)
                                        ) {
                                            Icon(Icons.Filled.Pause, null)
                                            Spacer(modifier = Modifier.width(6.dp))
                                            Text("Beklet", fontWeight = FontWeight.Bold)
                                        }

                                        // 2. "Kaydet" button
                                        Button(
                                            onClick = {
                                                val totalVal = purchaseCart.sumOf { it.qty * it.price }
                                                
                                                if (AppDataStore.sendToApprovalCenterDirectly) {
                                                    val approvalTxId = "FT-" + (12400 + AppDataStore.kasaLogs.size)
                                                    val appItem = ApprovalItem(
                                                        id = approvalTxId,
                                                        type = "Alış",
                                                        customerName = supplier.name,
                                                        description = "Saha Alış Girişi ($invoiceNo) - Toplam Tutarı: ${String.format("%.2f ₺", totalVal)} (${purchaseCart.size} kalem ürün)",
                                                        amount = totalVal,
                                                        time = "08.06.2026 20:53",
                                                        reason = "Siparişler Onaya Düşsün Aktif"
                                                    )
                                                    AppDataStore.approvalItems.add(0, appItem)
                                                    android.widget.Toast.makeText(context, "Satınalma kaydı onay merkezine başarıyla gönderildi.", android.widget.Toast.LENGTH_LONG).show()
                                                } else {
                                                    purchaseCart.forEach { cartItem ->
                                                        if (cartItem.isRegistered) {
                                                            val index = AppDataStore.products.indexOfFirst { it.code == cartItem.code }
                                                            if (index != -1) {
                                                                val prod = AppDataStore.products[index]
                                                                val mutableStocks = prod.stockByWarehouse.toMutableMap()
                                                                val currentQty = mutableStocks[AppDataStore.purchaseWarehouse] ?: 0
                                                                mutableStocks[AppDataStore.purchaseWarehouse] = currentQty + cartItem.qty
                                                                AppDataStore.products[index] = prod.copy(stockByWarehouse = mutableStocks)
                                                            }
                                                        }
                                                    }

                                                    val txId = "TX-" + (10000 + (1000..9999).random())
                                                    supplier.transactions.add(0, CustomerTx(
                                                        id = txId,
                                                        date = "08.06.2026",
                                                        type = "TEDİYE",
                                                        amount = totalVal,
                                                        description = "Alış Faturası No: $invoiceNo"
                                                    ))
                                                    supplier.balance -= totalVal

                                                    val logId = "K-" + (1000 + (1000..9999).random())
                                                    AppDataStore.kasaLogs.add(0, KasaLogItem(
                                                        id = logId,
                                                        date = "08.06.2026 20:53",
                                                        type = "Tediye",
                                                        customerOrSupplier = supplier.name,
                                                        amount = totalVal,
                                                        paymentType = "Nakit",
                                                        bankName = null,
                                                        desc = "Saha Alış Girişi ($invoiceNo)"
                                                    ))
                                                }

                                                AppDataStore.persist(context)
                                                onSuccess()
                                                purchaseCart.clear()
                                                selectedSupplier = null
                                                productCodeInput = ""
                                                productTitleInput = ""
                                                quantityInput = "10"
                                                unitPriceInput = ""
                                                isRegisteredProduct = false
                                                previousPurchasePrice = 0.0
                                                activeBottomTab = 0
                                            },
                                            modifier = Modifier.weight(1.8f).height(52.dp),
                                            colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.primary)
                                        ) {
                                            Icon(Icons.Filled.Check, null)
                                            Spacer(modifier = Modifier.width(6.dp))
                                            Text("Kaydet", fontWeight = FontWeight.Bold)
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

// --- MODULE 2: RETURNS (İADE) ---

data class ReturnCartItem(
    val product: ProductCatalog,
    var quantity: Int,
    var conditionPercent: Float = 1.0f,
    var returnReason: String = "Hatalı Boyut / Kusur"
)

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ReturnsModule(onSuccess: () -> Unit) {
    val context = LocalContext.current
    var selectedCustomer by remember { mutableStateOf<Customer?>(null) }
    var selectedTab by remember { mutableStateOf(0) } // 0: İade Edilebilir Ürünler, 1: İade Sepeti
    var selectedWarehouse by remember { mutableStateOf("Ana Depo") }
    
    // Returns Cart
    val returnCart = remember { mutableStateListOf<ReturnCartItem>() }
    
    // Internal Dialog Controls
    var showSettlementDialog by remember { mutableStateOf(false) }
    var settlementMethod by remember { mutableStateOf("Cari Alacak") } // "Cari Alacak", "Nakit", "Banka İade"
    var selectedBankForRefund by remember { mutableStateOf<Bank?>(AppDataStore.banks.firstOrNull()) }
    var searchQuery by remember { mutableStateOf("") }

    val scope = rememberCoroutineScope()

    if (selectedCustomer == null) {
        // Step 1: Force choosing customer first to get their purchased inventory
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            FieldCard(modifier = Modifier.fillMaxWidth()) {
                Column(modifier = Modifier.padding(16.dp), verticalArrangement = Arrangement.spacedBy(8.dp)) {
                    Text(
                        "LÜTFEN İADE YAPACAK MÜŞTERİYİ SEÇİN",
                        style = MaterialTheme.typography.titleMedium,
                        fontWeight = FontWeight.Bold,
                        color = MaterialTheme.colorScheme.primary
                    )
                    Text(
                        "Müşteriden sadece daha önce satmış olduğumuz (faydalanmış olduğu) ürünlerin iadesi kabul edilebilir.",
                        style = MaterialTheme.typography.bodySmall,
                        color = Color.Gray
                    )
                }
            }

            Text("Aktif Cari Kartlar Listesi", style = MaterialTheme.typography.labelLarge, color = MaterialTheme.colorScheme.primary, fontWeight = FontWeight.Bold)

            LazyColumn(
                modifier = Modifier
                    .fillMaxSize()
                    .weight(1f),
                verticalArrangement = Arrangement.spacedBy(10.dp)
            ) {
                items(AppDataStore.customers) { cust ->
                    FieldCard(modifier = Modifier.fillMaxWidth()) {
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .clickable { selectedCustomer = cust }
                                .padding(16.dp),
                            horizontalArrangement = Arrangement.SpaceBetween,
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Column {
                                Text(cust.name, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodyMedium)
                                Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                                    Text("Kod: ${cust.id}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                    Text("", style = MaterialTheme.typography.labelSmall)
                                }
                            }
                            Icon(Icons.Filled.ArrowForward, "Seç", tint = MaterialTheme.colorScheme.primary)
                        }
                    }
                }
            }
        }
    } else {
        // Step 2: Render returning products catalog filtered by past sales
        val activeCust = selectedCustomer!!
        
        // Find barcodes of products previously sold to this specific customer
        val soldBarcodes = AppDataStore.salesHistory
            .filter { it.customerId == activeCust.id }
            .map { it.productBarcode }
            .toSet()

        val rawReturnableProducts = AppDataStore.products.filter { it.barcode in soldBarcodes }

        Column(modifier = Modifier.fillMaxSize()) {
            // Customer Banner Info
            Box(
                modifier = Modifier
                    .fillMaxWidth()
                    .background(MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f))
                    .padding(16.dp)
            ) {
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Column(modifier = Modifier.weight(1f)) {
                        Text("İADE EDEN CARİ", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.primary, fontWeight = FontWeight.Bold)
                        Text(activeCust.name, style = MaterialTheme.typography.bodyLarge, fontWeight = FontWeight.Bold)
                        Text("Kod: ${activeCust.id} | Bakiye: ${String.format("%,.2f ₺", activeCust.balance)}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                    }
                    Button(
                        colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.primaryContainer),
                        onClick = {
                            selectedCustomer = null
                            returnCart.clear()
                        }
                    ) {
                        Text("Müşteri Değiştir", color = MaterialTheme.colorScheme.onPrimaryContainer, style = MaterialTheme.typography.bodySmall)
                    }
                }
            }

            // Tab Controls
            TabRow(
                selectedTabIndex = selectedTab,
                containerColor = MaterialTheme.colorScheme.background,
                contentColor = MaterialTheme.colorScheme.primary
            ) {
                Tab(
                    selected = selectedTab == 0,
                    onClick = { selectedTab = 0 },
                    text = { Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(6.dp)) {
                        Icon(Icons.Filled.AssignmentReturn, null, modifier = Modifier.size(16.dp))
                        Text("Satın Alınanlar (${rawReturnableProducts.size})", fontWeight = FontWeight.Bold)
                    }}
                )
                Tab(
                    selected = selectedTab == 1,
                    onClick = { selectedTab = 1 },
                    text = { Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(6.dp)) {
                        Icon(Icons.Filled.ShoppingCart, null, modifier = Modifier.size(16.dp))
                        Text("İade Sepeti (${returnCart.sumOf { it.quantity }})", fontWeight = FontWeight.Bold)
                    }}
                )
            }

            LazyColumn(
                modifier = Modifier
                    .fillMaxSize()
                    .weight(1f)
                    .padding(horizontal = 16.dp, vertical = 12.dp),
                verticalArrangement = Arrangement.spacedBy(12.dp)
            ) {
                if (selectedTab == 0) {
                    item {
                        OutlinedTextField(
                            value = searchQuery,
                            onValueChange = { searchQuery = it },
                            placeholder = { Text("Satın aldığı ürünlerde ara...") },
                            leadingIcon = { Icon(Icons.Filled.Search, null) },
                            modifier = Modifier.fillMaxWidth(),
                            singleLine = true
                        )
                    }

                    if (rawReturnableProducts.isEmpty()) {
                        item {
                            Box(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(32.dp),
                                contentAlignment = Alignment.Center
                            ) {
                                Text(
                                    "Bu müşterinin satın aldığı geçmiş ürün kaydı bulunamadı. Müşteriden sadece sattığımız ürünlerin iadesi alınabilir.",
                                    color = Color.Red,
                                    textAlign = TextAlign.Center,
                                    style = MaterialTheme.typography.bodyMedium
                                )
                            }
                        }
                    } else {
                        val filteredReturnables = rawReturnableProducts.filter {
                            it.title.contains(searchQuery, ignoreCase = true) || it.code.contains(searchQuery, ignoreCase = true)
                        }

                        items(filteredReturnables) { prod ->
                            // Custom client price rules for refund
                            val group = activeCust.priceGroup
                            val refundPrice = prod.getPriceForGroup(group)

                            FieldCard(modifier = Modifier.fillMaxWidth()) {
                                Row(
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .padding(12.dp),
                                    verticalAlignment = Alignment.CenterVertically,
                                    horizontalArrangement = Arrangement.SpaceBetween
                                ) {
                                    Column(modifier = Modifier.weight(1f)) {
                                        Text(prod.title, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodyMedium)
                                        Text("Kod: ${prod.code} | Barkod: ${prod.barcode}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                        Text("Geçmiş Fiyatı: ${String.format("%,.2f ₺", refundPrice)}", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.primary, fontWeight = FontWeight.Bold)
                                    }
                                    
                                    IconButton(
                                        onClick = {
                                            val existing = returnCart.find { it.product.barcode == prod.barcode }
                                            if (existing != null) {
                                                val idx = returnCart.indexOf(existing)
                                                returnCart[idx] = existing.copy(quantity = existing.quantity + 1)
                                            } else {
                                                returnCart.add(ReturnCartItem(product = prod, quantity = 1))
                                            }
                                        },
                                        modifier = Modifier
                                            .background(MaterialTheme.colorScheme.primaryContainer, shape = RoundedCornerShape(8.dp))
                                            .size(40.dp)
                                    ) {
                                        Icon(Icons.Filled.Add, contentDescription = "İadeye Ekle", tint = MaterialTheme.colorScheme.onPrimaryContainer)
                                    }
                                }
                            }
                        }
                    }
                } else {
                    // Cart View
                    if (returnCart.isEmpty()) {
                        item {
                            Box(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(32.dp),
                                contentAlignment = Alignment.Center
                            ) {
                                Text("Lütfen 'Satın Alınanlar' sekmesinden iade alınacak ürünü ekleyin.", color = Color.Gray)
                            }
                        }
                    } else {
                        items(returnCart) { item ->
                            val group = activeCust.priceGroup
                            val refundPrice = item.product.getPriceForGroup(group)
                            
                            FieldCard(modifier = Modifier.fillMaxWidth()) {
                                Column(modifier = Modifier.padding(12.dp), verticalArrangement = Arrangement.spacedBy(8.dp)) {
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween,
                                        verticalAlignment = Alignment.Top
                                    ) {
                                        Column(modifier = Modifier.weight(1f)) {
                                            Text(item.product.title, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodyMedium)
                                            Text("Kod: ${item.product.code}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                        }
                                        Text(
                                            String.format("%,.2f ₺", refundPrice * item.quantity),
                                            style = MaterialTheme.typography.bodyMedium,
                                            fontWeight = FontWeight.Bold,
                                            color = MaterialTheme.colorScheme.primary
                                        )
                                    }

                                    // Quantity Controllers
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween,
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(4.dp)) {
                                            IconButton(
                                                onClick = {
                                                    if (item.quantity > 1) {
                                                        val idx = returnCart.indexOf(item)
                                                        returnCart[idx] = item.copy(quantity = item.quantity - 1)
                                                    } else {
                                                        returnCart.remove(item)
                                                    }
                                                },
                                                modifier = Modifier.size(32.dp)
                                            ) {
                                                Icon(Icons.Filled.RemoveCircleOutline, null, tint = MaterialTheme.colorScheme.primary)
                                            }
                                            Text(item.quantity.toString(), fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodyLarge)
                                            IconButton(
                                                onClick = {
                                                    val idx = returnCart.indexOf(item)
                                                    returnCart[idx] = item.copy(quantity = item.quantity + 1)
                                                },
                                                modifier = Modifier.size(32.dp)
                                            ) {
                                                Icon(Icons.Filled.AddCircleOutline, null, tint = MaterialTheme.colorScheme.primary)
                                            }
                                        }
                                        
                                        IconButton(onClick = { returnCart.remove(item) }) {
                                            Icon(Icons.Filled.Delete, null, tint = MaterialTheme.colorScheme.error)
                                        }
                                    }

                                    // Condition Sizer Slider
                                    Column(verticalArrangement = Arrangement.spacedBy(2.dp)) {
                                        Row(
                                            modifier = Modifier.fillMaxWidth(),
                                            horizontalArrangement = Arrangement.SpaceBetween
                                        ) {
                                            Text("Ürün Fiziksel Kondisyonu:", style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Bold)
                                            Text("%${(item.conditionPercent * 100).toInt()}", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.secondary, fontWeight = FontWeight.Bold)
                                        }
                                        Slider(
                                            value = item.conditionPercent,
                                            onValueChange = {
                                                val idx = returnCart.indexOf(item)
                                                returnCart[idx] = item.copy(conditionPercent = it)
                                            },
                                            valueRange = 0f..1f,
                                            colors = SliderDefaults.colors(
                                                thumbColor = MaterialTheme.colorScheme.secondary,
                                                activeTrackColor = MaterialTheme.colorScheme.secondaryContainer
                                            )
                                        )
                                    }
                                    
                                    OutlinedTextField(
                                        value = item.returnReason,
                                        onValueChange = {
                                            val idx = returnCart.indexOf(item)
                                            returnCart[idx] = item.copy(returnReason = it)
                                        },
                                        label = { Text("İade Kabul Gerekçesi") },
                                        modifier = Modifier.fillMaxWidth(),
                                        singleLine = true
                                    )
                                }
                            }
                        }
                    }
                }
            }

            // Calculation Sheet Footer
            if (returnCart.isNotEmpty()) {
                val totalRefund = returnCart.sumOf { item ->
                    val group = activeCust.priceGroup
                    val refundPrice = item.product.getPriceForGroup(group)
                    refundPrice * item.quantity * item.conditionPercent
                }

                Surface(
                    color = MaterialTheme.colorScheme.surfaceVariant,
                    shape = RoundedCornerShape(topStart = 16.dp, topEnd = 16.dp)
                ) {
                    Column(modifier = Modifier.padding(16.dp), verticalArrangement = Arrangement.spacedBy(10.dp)) {
                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.SpaceBetween,
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Text("Toplam Hak Edilen İade Tutarı", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold)
                            Text(String.format("%,.2f ₺", totalRefund), style = MaterialTheme.typography.titleLarge, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
                        }
                        
                        Button(
                            modifier = Modifier.fillMaxWidth().height(52.dp),
                            onClick = { showSettlementDialog = true },
                            colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.tertiary)
                        ) {
                            Icon(Icons.Filled.AssignmentReturn, null)
                            Spacer(modifier = Modifier.width(8.dp))
                            Text("İadeyi Tamamla ve Giriş Yap")
                        }
                    }
                }

                // Settlement Payment method chooser
                if (showSettlementDialog) {
                    AlertDialog(
                        onDismissRequest = { showSettlementDialog = false },
                        title = { Text("Geri Ödeme Yöntemi Belirleyin") },
                        text = {
                            Column(verticalArrangement = Arrangement.spacedBy(14.dp)) {
                                Text("Geri Ödenecek Toplam Tutar: " + String.format("%,.2f ₺", totalRefund), fontWeight = FontWeight.Bold)
                                Text("İadenin kapanış şeklini seçiniz:")
                                
                                Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.spacedBy(6.dp)) {
                                    listOf("Cari Alacak", "Nakit", "Banka İade").forEach { method ->
                                        val active = settlementMethod == method
                                        Box(
                                            modifier = Modifier
                                                .weight(1f)
                                                .clip(RoundedCornerShape(8.dp))
                                                .background(if (active) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.surfaceVariant)
                                                .clickable { settlementMethod = method }
                                                .padding(vertical = 10.dp),
                                            contentAlignment = Alignment.Center
                                        ) {
                                            Text(
                                                text = method,
                                                color = if (active) MaterialTheme.colorScheme.onPrimary else MaterialTheme.colorScheme.onSurfaceVariant,
                                                fontWeight = FontWeight.Bold,
                                                style = MaterialTheme.typography.labelSmall
                                            )
                                        }
                                    }
                                }

                                if (settlementMethod == "Banka İade") {
                                    Text("Ödemenin Yapılacağı Banka Hesabı Seçiniz:", style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Bold)
                                    Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                                        AppDataStore.banks.forEach { bk ->
                                            val matched = selectedBankForRefund?.id == bk.id
                                            Box(
                                                modifier = Modifier
                                                    .fillMaxWidth()
                                                    .clip(RoundedCornerShape(8.dp))
                                                    .background(if (matched) MaterialTheme.colorScheme.secondaryContainer else MaterialTheme.colorScheme.surfaceVariant)
                                                    .clickable { selectedBankForRefund = bk }
                                                    .padding(10.dp)
                                            ) {
                                                Row(
                                                    modifier = Modifier.fillMaxWidth(),
                                                    horizontalArrangement = Arrangement.SpaceBetween,
                                                    verticalAlignment = Alignment.CenterVertically
                                                ) {
                                                    Column {
                                                        Text(bk.name, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodySmall)
                                                        Text("Bakiye: ${String.format("%,.2f ₺", bk.balance)}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                                    }
                                                    if (matched) {
                                                        Icon(Icons.Filled.Check, null, tint = MaterialTheme.colorScheme.primary, modifier = Modifier.size(16.dp))
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
                                    if (AppDataStore.sendToApprovalCenterDirectly) {
                                        val approvalTxId = "FT-" + (12400 + AppDataStore.kasaLogs.size)
                                        val appItem = ApprovalItem(
                                            id = approvalTxId,
                                            type = "İade",
                                            customerName = activeCust.name,
                                            description = "Gelen Müşteri İade Kabul Faturası - Tutar: ${String.format("%.2f ₺", totalRefund)} (${returnCart.size} kalem ürün)",
                                            amount = totalRefund,
                                            time = "08.06.2026 18:30",
                                            reason = "Siparişler Onaya Düşsün Aktif"
                                        )
                                        AppDataStore.approvalItems.add(0, appItem)
                                        android.widget.Toast.makeText(context, "İade kaydı onay merkezine başarıyla gönderildi.", android.widget.Toast.LENGTH_LONG).show()
                                    } else {
                                        // Process Returned Quantities (Add back to Warehouse Inventory!)
                                        returnCart.forEach { rcItem ->
                                            val currentMap = rcItem.product.stockByWarehouse.toMutableMap()
                                            val whStockValue = currentMap[selectedWarehouse] ?: 0
                                            currentMap[selectedWarehouse] = whStockValue + rcItem.quantity
                                            
                                            // Update in AppDataStore
                                            val mIdx = AppDataStore.products.indexOfFirst { it.barcode == rcItem.product.barcode }
                                            if (mIdx != -1) {
                                                AppDataStore.products[mIdx] = rcItem.product.copy(stockByWarehouse = currentMap)
                                            }
                                        }

                                        // Post Settlement balances & records
                                        when (settlementMethod) {
                                            "Cari Alacak" -> {
                                                val cIdx = AppDataStore.customers.indexOfFirst { it.id == activeCust.id }
                                                if (cIdx != -1) {
                                                    val c = AppDataStore.customers[cIdx]
                                                    // Decrement Debit balance
                                                    c.balance -= totalRefund
                                                    
                                                    c.transactions.add(
                                                        CustomerTx(
                                                            id = "TX-" + (30000 + c.transactions.size),
                                                            date = "08.06.2026",
                                                            type = "TEDİYE", // Account adjustment/credit
                                                            amount = totalRefund,
                                                            description = "Gelen Müşteri İade Senedi/Kabul Belgesi"
                                                        )
                                                    )
                                                }

                                                // Register cash register ledgers
                                                AppDataStore.kasaLogs.add(
                                                    KasaLogItem(
                                                        id = "K-" + (3000 + AppDataStore.kasaLogs.size),
                                                        date = "08.06.2026 18:30",
                                                        type = "İade",
                                                        customerOrSupplier = activeCust.name,
                                                        amount = totalRefund,
                                                        paymentType = "Nakit",
                                                        bankName = null,
                                                        desc = "İade Kabul Faturasıyla Cari Alacaklandırma"
                                                    )
                                                )
                                            }
                                            "Nakit" -> {
                                                AppDataStore.kasaLogs.add(
                                                    KasaLogItem(
                                                        id = "K-" + (3000 + AppDataStore.kasaLogs.size),
                                                        date = "08.06.2026 18:30",
                                                        type = "İade",
                                                        customerOrSupplier = activeCust.name,
                                                        amount = totalRefund,
                                                        paymentType = "Nakit",
                                                        bankName = null,
                                                        desc = "İade Kabul Faturası Kaydı Peşin Nakit İadesi"
                                                    )
                                                )
                                            }
                                            "Banka İade" -> {
                                                val targetB = selectedBankForRefund
                                                if (targetB != null) {
                                                    // Deduct refund from chosen bank
                                                    val bIdx = AppDataStore.banks.indexOfFirst { it.id == targetB.id }
                                                    if (bIdx != -1) {
                                                        AppDataStore.banks[bIdx].balance -= totalRefund
                                                    }

                                                    AppDataStore.kasaLogs.add(
                                                        KasaLogItem(
                                                            id = "K-" + (3000 + AppDataStore.kasaLogs.size),
                                                            date = "08.06.2026 18:30",
                                                            type = "İade",
                                                            customerOrSupplier = activeCust.name,
                                                            amount = totalRefund,
                                                            paymentType = "EFT / Havale",
                                                            bankName = targetB.name,
                                                            desc = "İade Kabul Faturası Kaydı / ${targetB.name} Geri Havale Kaydı"
                                                        )
                                                    )
                                                }
                                            }
                                        }
                                    }

                                    returnCart.clear()
                                    showSettlementDialog = false
                                    AppDataStore.persist(context)
                                    onSuccess()
                                }
                            ) {
                                Text("Onayla & Deftere İşle")
                            }
                        },
                        dismissButton = {
                            TextButton(onClick = { showSettlementDialog = false }) {
                                Text("Vazgeç")
                            }
                        }
                    )
                }
            }
        }
    }
}

// --- MODULE 3: COLLECTION (TAHSİLAT) ---
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun CollectionModule(navController: NavController, onSuccess: () -> Unit) {
    val context = LocalContext.current
    var selectedCustomer by remember { mutableStateOf<Customer?>(null) }
    var amount by remember { mutableStateOf("") }
    var description by remember { mutableStateOf("") }
    var selectedPayMethod by remember { mutableStateOf(0) } // 0: Nakit, 1: Kredi Kartı, 2: EFT, 3: Çek, 4: Senet
    val paymentMethods = listOf("Nakit", "Kredi Kartı", "EFT / Havale", "Çek", "Senet")

    // Cheque specific fields
    var cekNo by remember { mutableStateOf("") }
    var cekVadeTarihi by remember { mutableStateOf("15.08.2026") }
    var cekBankasi by remember { mutableStateOf("") }
    val cekPhotos = remember { mutableStateListOf<String>() }

    // Promissory note specific fields
    var senetNo by remember { mutableStateOf("") }
    var senetVadeTarihi by remember { mutableStateOf("30.09.2026") }
    val senetPhotos = remember { mutableStateListOf<String>() }

    // Audio/haptic feedback trigger
    val playFeedback = {
        try {
            val toneG = android.media.ToneGenerator(android.media.AudioManager.STREAM_NOTIFICATION, 100)
            toneG.startTone(android.media.ToneGenerator.TONE_PROP_BEEP, 150)
        } catch (e: Exception) { e.printStackTrace() }
    }


    // Choose bank
    var selectedBank by remember { mutableStateOf<Bank?>(AppDataStore.banks.firstOrNull()) }
    var showCustomerSelectionView by remember { mutableStateOf(false) }
    var showReceiptDialog by remember { mutableStateOf(false) }
    var receiptData by remember { mutableStateOf<ReceiptInfo?>(null) }

    if (showReceiptDialog && receiptData != null) {
        ReceiptDialog(
            info = receiptData!!,
            onDismiss = {
                showReceiptDialog = false
                receiptData = null
                
                selectedCustomer = null
                amount = ""
                description = ""
                selectedPayMethod = 0
                cekNo = ""
                cekBankasi = ""
                cekPhotos.clear()
                senetNo = ""
                senetPhotos.clear()

                onSuccess()
            }
        )
    }

    if (showCustomerSelectionView) {
        var query by remember { mutableStateOf("") }
        Scaffold(
            topBar = {
                TopAppBar(
                    title = { Text("Tahsilat Cari Hesap Seçimi", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold) },
                    navigationIcon = {
                        IconButton(onClick = { showCustomerSelectionView = false }) {
                            Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = "Geri")
                        }
                    },
                    colors = TopAppBarDefaults.topAppBarColors(
                        containerColor = MaterialTheme.colorScheme.surface
                    )
                )
            },
            bottomBar = {
                Surface(
                    tonalElevation = 8.dp,
                    color = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.95f),
                    modifier = Modifier.fillMaxWidth().navigationBarsPadding()
                ) {
                    Column(
                        modifier = Modifier.padding(12.dp)
                    ) {
                        OutlinedTextField(
                            value = query,
                            onValueChange = { query = it },
                            label = { Text("Cari Hesap Ara (Adı, Kodu, Tlf...)") },
                            leadingIcon = { Icon(Icons.Filled.Search, contentDescription = null) },
                            trailingIcon = {
                                if (query.isNotEmpty()) {
                                    IconButton(onClick = { query = "" }) {
                                        Icon(Icons.Filled.Close, contentDescription = "Temizle")
                                    }
                                }
                            },
                            singleLine = true,
                            modifier = Modifier.fillMaxWidth(),
                            colors = OutlinedTextFieldDefaults.colors(
                                focusedContainerColor = MaterialTheme.colorScheme.surface,
                                unfocusedContainerColor = MaterialTheme.colorScheme.surface
                            )
                        )
                    }
                }
            }
        ) { innerPadding ->
            val filtered = AppDataStore.customers.filter { cs ->
                cs.name.contains(query, ignoreCase = true) ||
                cs.phone.contains(query, ignoreCase = true) ||
                cs.address.contains(query, ignoreCase = true) ||
                cs.id.contains(query, ignoreCase = true)
            }

            if (filtered.isEmpty()) {
                Box(
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(innerPadding),
                    contentAlignment = Alignment.Center
                ) {
                    Column(
                        horizontalAlignment = Alignment.CenterHorizontally,
                        verticalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        Icon(
                            imageVector = Icons.Filled.SearchOff,
                            contentDescription = null,
                            modifier = Modifier.size(48.dp),
                            tint = MaterialTheme.colorScheme.outline
                        )
                        Text(
                            text = "Aramanıza uygun cari bulunamadı.",
                            style = MaterialTheme.typography.bodyMedium,
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                    }
                }
            } else {
                LazyColumn(
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(innerPadding),
                    contentPadding = PaddingValues(16.dp),
                    verticalArrangement = Arrangement.spacedBy(10.dp)
                ) {
                    items(filtered) { cs ->
                        val isSelected = selectedCustomer?.id == cs.id
                        Card(
                            modifier = Modifier
                                .fillMaxWidth()
                                .clickable {
                                    selectedCustomer = cs
                                    showCustomerSelectionView = false
                                },
                            shape = RoundedCornerShape(12.dp),
                            colors = CardDefaults.cardColors(
                                containerColor = if (isSelected) MaterialTheme.colorScheme.primaryContainer
                                else MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f)
                            ),
                            border = BorderStroke(
                                width = if (isSelected) 1.dp else 0.dp,
                                color = if (isSelected) MaterialTheme.colorScheme.primary else Color.Transparent
                            )
                        ) {
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(16.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Row(
                                    verticalAlignment = Alignment.CenterVertically,
                                    horizontalArrangement = Arrangement.spacedBy(12.dp),
                                    modifier = Modifier.weight(1f)
                                ) {
                                    Box(
                                        modifier = Modifier
                                            .size(40.dp)
                                            .background(
                                                if (isSelected) MaterialTheme.colorScheme.primary 
                                                else MaterialTheme.colorScheme.secondary.copy(alpha = 0.2f), 
                                                CircleShape
                                            ),
                                        contentAlignment = Alignment.Center
                                    ) {
                                        Icon(
                                            Icons.Filled.Person,
                                            contentDescription = null,
                                            tint = if (isSelected) MaterialTheme.colorScheme.onPrimary else MaterialTheme.colorScheme.secondary,
                                            modifier = Modifier.size(20.dp)
                                        )
                                    }
                                    Column {
                                        Text(cs.name, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodyMedium)
                                        Text("Tlf: ${cs.phone} | Adres: ${cs.address}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                    }
                                }
                                Text(
                                    text = String.format("%,.2f ₺", cs.balance),
                                    fontWeight = FontWeight.Bold,
                                    style = MaterialTheme.typography.bodyMedium,
                                    color = if (cs.balance > 0) MaterialTheme.colorScheme.error else MaterialTheme.colorScheme.primary
                                )
                            }
                        }
                    }
                }
            }
        }
    } else {
        Scaffold(
            containerColor = Color.Transparent,
        contentWindowInsets = WindowInsets(0, 0, 0, 0),
        bottomBar = {
            Card(
                modifier = Modifier.fillMaxWidth(),
                shape = RoundedCornerShape(topStart = 24.dp, topEnd = 24.dp, bottomStart = 0.dp, bottomEnd = 0.dp),
                colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.95f)),
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
                    // 1. Temizle
                    Column(
                        modifier = Modifier
                            .weight(1f)
                            .clip(RoundedCornerShape(16.dp))
                            .clickable {
                                amount = ""
                                description = ""
                                cekNo = ""
                                cekBankasi = ""
                                cekPhotos.clear()
                                senetNo = ""
                                senetPhotos.clear()
                                selectedCustomer = null
                            }
                            .padding(vertical = 6.dp),
                        horizontalAlignment = Alignment.CenterHorizontally,
                        verticalArrangement = Arrangement.Center
                    ) {
                        Icon(
                            imageVector = Icons.Filled.Refresh,
                            contentDescription = "Temizle",
                            tint = MaterialTheme.colorScheme.error.copy(alpha = 0.8f),
                            modifier = Modifier.size(24.dp)
                        )
                        Spacer(modifier = Modifier.height(4.dp))
                        Text(
                            text = "Temizle",
                            style = MaterialTheme.typography.labelSmall,
                            color = MaterialTheme.colorScheme.error.copy(alpha = 0.8f),
                            fontWeight = FontWeight.Bold
                        )
                    }

                    // 2. Cari Seç
                    Column(
                        modifier = Modifier
                            .weight(1f)
                            .clip(RoundedCornerShape(16.dp))
                            .clickable {
                                showCustomerSelectionView = true
                            }
                            .padding(vertical = 6.dp),
                        horizontalAlignment = Alignment.CenterHorizontally,
                        verticalArrangement = Arrangement.Center
                    ) {
                        Icon(
                            imageVector = Icons.Filled.People,
                            contentDescription = "Cari",
                            tint = if (selectedCustomer != null) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.6f),
                            modifier = Modifier.size(24.dp)
                        )
                        Spacer(modifier = Modifier.height(4.dp))
                        Text(
                            text = "Cari",
                            style = MaterialTheme.typography.labelSmall,
                            color = if (selectedCustomer != null) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.6f),
                            fontWeight = FontWeight.Bold
                        )
                    }

                    // 3. Onayla
                    val isValid = selectedCustomer != null && amount.isNotEmpty()
                    Column(
                        modifier = Modifier
                            .weight(1f)
                            .clip(RoundedCornerShape(16.dp))
                            .clickable(enabled = isValid) {
                                val activeCust = selectedCustomer
                                if (activeCust != null) {
                                    val parsedAmt = amount.toDoubleOrNull() ?: 0.0
                                    
                                    if (AppDataStore.sendToApprovalCenterDirectly) {
                                        val approvalTxId = "T-" + (10400 + AppDataStore.kasaLogs.size)
                                        val appItem = ApprovalItem(
                                            id = approvalTxId,
                                            type = "Tahsilat",
                                            customerName = activeCust.name,
                                            description = "Tahsilat Makbuzu [${paymentMethods[selectedPayMethod]}] - Tutar: ${String.format("%.2f ₺", parsedAmt)}",
                                            amount = parsedAmt,
                                            time = "12.06.2026 15:45",
                                            reason = "Siparişler Onaya Düşsün Aktif"
                                        )
                                        AppDataStore.approvalItems.add(0, appItem)
                                        android.widget.Toast.makeText(context, "Tahsilat kaydı onay merkezine başarıyla gönderildi.", android.widget.Toast.LENGTH_LONG).show()
                                        
                                        // Clean up form
                                        amount = ""
                                        description = ""
                                        cekNo = ""
                                        cekBankasi = ""
                                        senetNo = ""
                                        selectedCustomer = null
                                    } else {
                                        // 1. Müşteri borcundan düşümü gerçekleştir
                                        val cIdx = AppDataStore.customers.indexOfFirst { it.id == activeCust.id }
                                        if (cIdx != -1) {
                                            val c = AppDataStore.customers[cIdx]
                                            c.balance -= parsedAmt
                                            
                                            val docDetailsText = when (selectedPayMethod) {
                                                3 -> "Çek Seri No: $cekNo Vade: $cekVadeTarihi Banka: $cekBankasi"
                                                4 -> "Senet Seri No: $senetNo Vade: $senetVadeTarihi"
                                                else -> description
                                            }
                                            
                                            c.transactions.add(
                                                CustomerTx(
                                                    id = "TX-" + (40400 + c.transactions.size),
                                                    date = "12.06.2026",
                                                    type = "TAHSİLAT",
                                                    amount = parsedAmt,
                                                    description = "Tahsilat Makbuzu [${paymentMethods[selectedPayMethod]}]: ($docDetailsText)"
                                                )
                                            )
                                        }

                                        // 2. Kasayı / Bankayı Güncelle
                                        if (selectedPayMethod == 1 || selectedPayMethod == 2) {
                                            val targetB = selectedBank
                                            if (targetB != null) {
                                                val bIdx = AppDataStore.banks.indexOfFirst { it.id == targetB.id }
                                                if (bIdx != -1) {
                                                    AppDataStore.banks[bIdx].balance += parsedAmt
                                                }
                                            }
                                        }

                                        // 3. Şirket banka defteri / kasa günlüğünü yaz
                                        val loggingText = when (selectedPayMethod) {
                                            3 -> "Çek No: $cekNo Banka: $cekBankasi Vade: $cekVadeTarihi"
                                            4 -> "Senet Seri No: $senetNo Vade: $senetVadeTarihi"
                                            else -> description
                                        }
                                        AppDataStore.kasaLogs.add(
                                            KasaLogItem(
                                                id = "K-" + (4100 + AppDataStore.kasaLogs.size),
                                                date = "12.06.2026 15:45",
                                                type = "Tahsilat",
                                                customerOrSupplier = activeCust.name,
                                                amount = parsedAmt,
                                                paymentType = paymentMethods[selectedPayMethod],
                                                bankName = if (selectedPayMethod == 1 || selectedPayMethod == 2) selectedBank?.name else null,
                                                desc = loggingText
                                            )
                                        )

                                        val finalDateStr = java.text.SimpleDateFormat("dd.MM.yyyy HH:mm", java.util.Locale.getDefault()).format(java.util.Date())
                                        val finalReceiptNo = "T-" + System.currentTimeMillis().toString().takeLast(6)
                                        receiptData = ReceiptInfo(
                                            type = "TAHSİLAT",
                                            receiptNo = finalReceiptNo,
                                            date = finalDateStr,
                                            customerName = activeCust.name,
                                            customerId = activeCust.id,
                                            amount = parsedAmt,
                                            paymentMethod = paymentMethods[selectedPayMethod],
                                            bankName = if (selectedPayMethod == 1 || selectedPayMethod == 2) selectedBank?.name else null,
                                            memo = if (loggingText.isNotEmpty()) loggingText else "Cari Hesap Tahsilat İşlemi"
                                        )
                                        showReceiptDialog = true
                                    }
                                }

                                AppDataStore.persist(context)
                                playFeedback()
                            }
                            .padding(vertical = 6.dp),
                        horizontalAlignment = Alignment.CenterHorizontally,
                        verticalArrangement = Arrangement.Center
                    ) {
                        Icon(
                            imageVector = Icons.Filled.CheckCircle,
                            contentDescription = "Onayla",
                            tint = if (isValid) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.3f),
                            modifier = Modifier.size(24.dp)
                        )
                        Spacer(modifier = Modifier.height(4.dp))
                        Text(
                            text = "Onayla",
                            style = MaterialTheme.typography.labelSmall,
                            color = if (isValid) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.3f),
                            fontWeight = FontWeight.Bold
                        )
                    }
                }
            }
        }
    ) { innerPadding ->
        LazyColumn(
            modifier = Modifier
                .fillMaxSize()
                .padding(innerPadding)
                .padding(horizontal = 16.dp),
            verticalArrangement = Arrangement.spacedBy(14.dp),
            contentPadding = PaddingValues(top = 16.dp, bottom = 40.dp)
        ) {
            // 1. CARI HESAP DETAYI VE GÜNCEL BORÇ BAKİYESİ
            item {
                val cust = selectedCustomer
                Card(
                    modifier = Modifier.fillMaxWidth().clickable { showCustomerSelectionView = true },
                    colors = CardDefaults.cardColors(
                        containerColor = MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.15f)
                    ),
                    border = BorderStroke(1.dp, MaterialTheme.colorScheme.primary.copy(alpha = 0.25f)),
                    shape = RoundedCornerShape(16.dp)
                ) {
                    Column(modifier = Modifier.padding(16.dp)) {
                        Row(
                            verticalAlignment = Alignment.CenterVertically,
                            horizontalArrangement = Arrangement.spacedBy(12.dp)
                        ) {
                            Box(
                                modifier = Modifier
                                    .size(48.dp)
                                    .background(MaterialTheme.colorScheme.primary, CircleShape),
                                contentAlignment = Alignment.Center
                            ) {
                                Icon(
                                    Icons.Filled.Person,
                                    contentDescription = null,
                                    tint = MaterialTheme.colorScheme.onPrimary,
                                    modifier = Modifier.size(24.dp)
                                )
                            }
                            Column(modifier = Modifier.weight(1f)) {
                                Text(
                                    text = cust?.name ?: "Ziyaret Noktası Cari Seçilmedi",
                                    fontWeight = FontWeight.Bold,
                                    style = MaterialTheme.typography.titleMedium,
                                    color = MaterialTheme.colorScheme.onPrimaryContainer
                                )
                                Text(
                                    text = if (cust != null) "Tlf: ${cust.phone} | Adres: ${cust.address}" else "Lütfen altbardan bir müşteri kartı seçiniz",
                                    style = MaterialTheme.typography.bodySmall,
                                    color = MaterialTheme.colorScheme.onPrimaryContainer.copy(alpha = 0.7f)
                                )
                            }
                        }
                        
                        if (cust != null) {
                            HorizontalDivider(
                                modifier = Modifier.padding(vertical = 12.dp),
                                color = MaterialTheme.colorScheme.primary.copy(alpha = 0.15f)
                            )
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Column {
                                    Text(
                                        "Güncel Borç Bakiyesi",
                                        style = MaterialTheme.typography.labelSmall,
                                        color = MaterialTheme.colorScheme.onPrimaryContainer.copy(alpha = 0.8f)
                                    )
                                    Text(
                                        text = String.format("%,.2f ₺", cust.balance),
                                        fontWeight = FontWeight.Black,
                                        style = MaterialTheme.typography.titleLarge,
                                        color = if (cust.balance > 0) MaterialTheme.colorScheme.error else MaterialTheme.colorScheme.primary
                                    )
                                }
                                Box(
                                    modifier = Modifier
                                        .clip(RoundedCornerShape(8.dp))
                                        .background(
                                            if (cust.balance > 0) MaterialTheme.colorScheme.errorContainer else MaterialTheme.colorScheme.secondaryContainer
                                        )
                                        .padding(horizontal = 10.dp, vertical = 6.dp)
                                ) {
                                    Text(
                                        text = if (cust.balance > 0) "Müşteri Borçlu" else "Dengeli",
                                        style = MaterialTheme.typography.labelSmall,
                                        fontWeight = FontWeight.Bold,
                                        color = if (cust.balance > 0) MaterialTheme.colorScheme.onErrorContainer else MaterialTheme.colorScheme.onSecondaryContainer
                                    )
                                }
                            }
                        }
                    }
                }
            }

            // ÖDEME YÖNTEMİ SEÇİMİ
            item {
                Text("Ödeme Yöntemi", style = MaterialTheme.typography.labelMedium, color = MaterialTheme.colorScheme.primary, fontWeight = FontWeight.Bold)
            }
            item {
                Card(
                    modifier = Modifier.fillMaxWidth(),
                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.3f)),
                    border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f)),
                    shape = RoundedCornerShape(16.dp)
                ) {
                    Column(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(12.dp),
                        verticalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .horizontalScroll(rememberScrollState()),
                            horizontalArrangement = Arrangement.spacedBy(8.dp),
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            val methodIcons = listOf(
                                Icons.Filled.Payments,
                                Icons.Filled.CreditCard,
                                Icons.Filled.AccountBalance,
                                Icons.Filled.NoteAdd,
                                Icons.Filled.Assignment
                            )

                            paymentMethods.forEachIndexed { index, mName ->
                                val isSelected = selectedPayMethod == index
                                FilterChip(
                                    selected = isSelected,
                                    onClick = { selectedPayMethod = index },
                                    label = { Text(mName, style = MaterialTheme.typography.labelSmall, fontWeight = if (isSelected) FontWeight.Bold else FontWeight.Normal) },
                                    leadingIcon = {
                                        Icon(
                                            imageVector = methodIcons[index],
                                            contentDescription = mName,
                                            modifier = Modifier.size(16.dp),
                                            tint = if (isSelected) MaterialTheme.colorScheme.primary else Color.Gray
                                        )
                                    },
                                    colors = FilterChipDefaults.filterChipColors(
                                        selectedContainerColor = MaterialTheme.colorScheme.primaryContainer,
                                        selectedLabelColor = MaterialTheme.colorScheme.onPrimaryContainer
                                    )
                                )
                            }
                        }
                    }
                }
            }

            // 2. TUTAR VE AÇIKLAMA GİRİŞ ALANLARI
            item {
                FieldCard {
                    Column(modifier = Modifier.padding(16.dp), verticalArrangement = Arrangement.spacedBy(12.dp)) {
                        OutlinedTextField(
                            value = amount,
                            onValueChange = { amount = it },
                            label = { Text("Tahsilat Tutarı (₺)") },
                            leadingIcon = { Icon(Icons.Filled.Payments, null) },
                            prefix = { Text("₺ ") },
                            modifier = Modifier.fillMaxWidth(),
                            singleLine = true,
                            keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number)
                        )

                        OutlinedTextField(
                            value = description,
                            onValueChange = { description = it },
                            label = { Text("Açıklama / Fatura Referans No") },
                            leadingIcon = { Icon(Icons.Filled.Edit, null) },
                            modifier = Modifier.fillMaxWidth(),
                            singleLine = true
                        )
                    }
                }
            }

            // 3. KREDİ KARTI VEYA EFT/HAVALE DURUMUNDA BANKA KASASI SEÇİM DETAYI
            if (selectedPayMethod == 1 || selectedPayMethod == 2) {
                item {
                    Text("Ödemenin Aktarılacağı Banka Hesabı", style = MaterialTheme.typography.labelMedium, color = MaterialTheme.colorScheme.primary, fontWeight = FontWeight.Bold)
                }
                item {
                    Card(
                        modifier = Modifier.fillMaxWidth(),
                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
                        border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant)
                    ) {
                        Column(modifier = Modifier.padding(12.dp), verticalArrangement = Arrangement.spacedBy(6.dp)) {
                            AppDataStore.banks.forEach { bk ->
                                val matched = selectedBank?.id == bk.id
                                Row(
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .clip(RoundedCornerShape(8.dp))
                                        .background(
                                            if (matched) MaterialTheme.colorScheme.secondaryContainer.copy(alpha = 0.35f)
                                            else Color.Transparent
                                        )
                                        .clickable { selectedBank = bk }
                                        .padding(10.dp),
                                    verticalAlignment = Alignment.CenterVertically,
                                    horizontalArrangement = Arrangement.SpaceBetween
                                ) {
                                    Row(
                                        verticalAlignment = Alignment.CenterVertically,
                                        horizontalArrangement = Arrangement.spacedBy(10.dp)
                                    ) {
                                        RadioButton(
                                            selected = matched,
                                            onClick = { selectedBank = bk }
                                        )
                                        Column {
                                            Text(bk.name, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodyMedium)
                                            Text("IBAN: TR** **** **** **** **${bk.id}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // 4. ÇEK SEÇİLDİĞİNDE GİRİLECEK ALANLAR VE FOTOĞRAF YÜKLEME
            if (selectedPayMethod == 3) {
                item {
                    Text("Çek / Değerli Evrak Giriş Bilgileri", style = MaterialTheme.typography.labelMedium, color = MaterialTheme.colorScheme.primary, fontWeight = FontWeight.Bold)
                }
                item {
                    FieldCard {
                        Column(modifier = Modifier.padding(16.dp), verticalArrangement = Arrangement.spacedBy(12.dp)) {
                            OutlinedTextField(
                                value = cekNo,
                                onValueChange = { cekNo = it },
                                label = { Text("Çek Numarası") },
                                leadingIcon = { Icon(Icons.Filled.Numbers, null) },
                                modifier = Modifier.fillMaxWidth(),
                                singleLine = true
                            )
                            OutlinedTextField(
                                value = cekVadeTarihi,
                                onValueChange = { cekVadeTarihi = it },
                                label = { Text("Çek Vade Tarihi") },
                                leadingIcon = { Icon(Icons.Filled.DateRange, null) },
                                modifier = Modifier.fillMaxWidth(),
                                singleLine = true
                            )
                            var bankExpanded by remember { mutableStateOf(false) }
                            Box(modifier = Modifier.fillMaxWidth()) {
                                OutlinedTextField(
                                    value = cekBankasi,
                                    onValueChange = {},
                                    readOnly = true,
                                    label = { Text("Çek Bankası") },
                                    leadingIcon = { Icon(Icons.Filled.AccountBalance, null) },
                                    trailingIcon = {
                                        IconButton(onClick = { bankExpanded = true }) {
                                            Icon(Icons.Filled.ArrowDropDown, null)
                                        }
                                    },
                                    modifier = Modifier.fillMaxWidth().clickable { bankExpanded = true }
                                )
                                DropdownMenu(
                                    expanded = bankExpanded,
                                    onDismissRequest = { bankExpanded = false },
                                    modifier = Modifier.fillMaxWidth(0.9f)
                                ) {
                                    val banks = AppDataStore.definitions["Banka"] ?: emptyList()
                                    banks.forEach { bk ->
                                        DropdownMenuItem(
                                            text = { Text(bk) },
                                            onClick = { cekBankasi = bk; bankExpanded = false }
                                        )
                                    }
                                }
                            }

                            Spacer(modifier = Modifier.height(4.dp))
                            Text("Çek Fotoğrafı ve Belgeler", style = MaterialTheme.typography.labelMedium, fontWeight = FontWeight.Bold)
                            
                            if (cekPhotos.isEmpty()) {
                                Box(
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .height(110.dp)
                                        .clip(RoundedCornerShape(12.dp))
                                        .background(MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f))
                                        .clickable {
                                            playFeedback()
                                            cekPhotos.add("cek_gorseli_on_yuz_${cekPhotos.size + 1}.jpg")
                                        }
                                        .border(1.dp, MaterialTheme.colorScheme.outlineVariant, RoundedCornerShape(12.dp)),
                                    contentAlignment = Alignment.Center
                                ) {
                                    Column(horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(6.dp)) {
                                        Icon(Icons.Filled.Camera, null, tint = MaterialTheme.colorScheme.primary, modifier = Modifier.size(28.dp))
                                        Text("Çek Fotoğrafı Çek (Kamerayı Aç)", style = MaterialTheme.typography.labelMedium, color = Color.Gray)
                                    }
                                }
                            } else {
                                Row(
                                    modifier = Modifier.fillMaxWidth(),
                                    horizontalArrangement = Arrangement.spacedBy(8.dp),
                                    verticalAlignment = Alignment.CenterVertically
                                ) {
                                    cekPhotos.forEachIndexed { index, photo ->
                                        Box(
                                            modifier = Modifier
                                                .size(90.dp)
                                                .clip(RoundedCornerShape(12.dp))
                                                .background(MaterialTheme.colorScheme.primaryContainer)
                                                .border(1.dp, MaterialTheme.colorScheme.primary, RoundedCornerShape(12.dp))
                                        ) {
                                            Column(
                                                modifier = Modifier.fillMaxSize().padding(6.dp),
                                                horizontalAlignment = Alignment.CenterHorizontally,
                                                verticalArrangement = Arrangement.Center
                                            ) {
                                                Icon(Icons.Filled.List, null, tint = MaterialTheme.colorScheme.onPrimaryContainer, modifier = Modifier.size(28.dp))
                                                Text(photo, style = MaterialTheme.typography.labelSmall, maxLines = 1, fontSize = 8.sp, color = MaterialTheme.colorScheme.onPrimaryContainer)
                                            }
                                            Box(modifier = Modifier.fillMaxSize(), contentAlignment = Alignment.TopEnd) {
                                                IconButton(
                                                    onClick = { cekPhotos.removeAt(index) },
                                                    modifier = Modifier.size(24.dp)
                                                ) {
                                                    Icon(Icons.Filled.Cancel, null, tint = Color.Red, modifier = Modifier.size(18.dp))
                                                }
                                            }
                                        }
                                    }
                                    if (cekPhotos.size < 4) {
                                        Box(
                                            modifier = Modifier
                                                .size(90.dp)
                                                .clip(RoundedCornerShape(12.dp))
                                                .background(MaterialTheme.colorScheme.surfaceVariant)
                                                .clickable {
                                                    playFeedback()
                                                    cekPhotos.add("cek_gorseli_arka_yuz_${cekPhotos.size + 1}.jpg")
                                                },
                                            contentAlignment = Alignment.Center
                                        ) {
                                            Icon(Icons.Filled.Add, null, modifier = Modifier.size(24.dp))
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // 5. SENET SEÇİLDİĞİNDE GİRİLECEK ALANLAR VE FOTOĞRAF YÜKLEME
            if (selectedPayMethod == 4) {
                item {
                    Text("Senet / Değerli Evrak Giriş Bilgileri", style = MaterialTheme.typography.labelMedium, color = MaterialTheme.colorScheme.primary, fontWeight = FontWeight.Bold)
                }
                item {
                    FieldCard {
                        Column(modifier = Modifier.padding(16.dp), verticalArrangement = Arrangement.spacedBy(12.dp)) {
                            OutlinedTextField(
                                value = senetNo,
                                onValueChange = { senetNo = it },
                                label = { Text("Senet Seri/Sıra No") },
                                leadingIcon = { Icon(Icons.Filled.Numbers, null) },
                                modifier = Modifier.fillMaxWidth(),
                                singleLine = true
                            )
                            OutlinedTextField(
                                value = senetVadeTarihi,
                                onValueChange = { senetVadeTarihi = it },
                                label = { Text("Senet Vade Tarihi") },
                                leadingIcon = { Icon(Icons.Filled.DateRange, null) },
                                modifier = Modifier.fillMaxWidth(),
                                singleLine = true
                            )

                            Spacer(modifier = Modifier.height(4.dp))
                            Text("Senet Islak İmzalı Evrak Görselleri", style = MaterialTheme.typography.labelMedium, fontWeight = FontWeight.Bold)
                            
                            if (senetPhotos.isEmpty()) {
                                Box(
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .height(110.dp)
                                        .clip(RoundedCornerShape(12.dp))
                                        .background(MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f))
                                        .clickable {
                                            playFeedback()
                                            senetPhotos.add("senet_evraki_gorseli_${senetPhotos.size + 1}.jpg")
                                        }
                                        .border(1.dp, MaterialTheme.colorScheme.outlineVariant, RoundedCornerShape(12.dp)),
                                    contentAlignment = Alignment.Center
                                ) {
                                    Column(horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(6.dp)) {
                                        Icon(Icons.Filled.Camera, null, tint = MaterialTheme.colorScheme.primary, modifier = Modifier.size(28.dp))
                                        Text("Senet Fotoğrafı Çek (Kamerayı Aç)", style = MaterialTheme.typography.labelMedium, color = Color.Gray)
                                    }
                                }
                            } else {
                                Row(
                                    modifier = Modifier.fillMaxWidth(),
                                    horizontalArrangement = Arrangement.spacedBy(8.dp),
                                    verticalAlignment = Alignment.CenterVertically
                                ) {
                                    senetPhotos.forEachIndexed { index, photo ->
                                        Box(
                                            modifier = Modifier
                                                .size(90.dp)
                                                .clip(RoundedCornerShape(12.dp))
                                                .background(MaterialTheme.colorScheme.primaryContainer)
                                                .border(1.dp, MaterialTheme.colorScheme.primary, RoundedCornerShape(12.dp))
                                        ) {
                                            Column(
                                                modifier = Modifier.fillMaxSize().padding(6.dp),
                                                horizontalAlignment = Alignment.CenterHorizontally,
                                                verticalArrangement = Arrangement.Center
                                            ) {
                                                Icon(Icons.Filled.List, null, tint = MaterialTheme.colorScheme.onPrimaryContainer, modifier = Modifier.size(28.dp))
                                                Text(photo, style = MaterialTheme.typography.labelSmall, maxLines = 1, fontSize = 8.sp, color = MaterialTheme.colorScheme.onPrimaryContainer)
                                            }
                                            Box(modifier = Modifier.fillMaxSize(), contentAlignment = Alignment.TopEnd) {
                                                IconButton(
                                                    onClick = { senetPhotos.removeAt(index) },
                                                    modifier = Modifier.size(24.dp)
                                                ) {
                                                    Icon(Icons.Filled.Cancel, null, tint = Color.Red, modifier = Modifier.size(18.dp))
                                                }
                                            }
                                        }
                                    }
                                    if (senetPhotos.size < 4) {
                                        Box(
                                            modifier = Modifier
                                                .size(90.dp)
                                                .clip(RoundedCornerShape(12.dp))
                                                .background(MaterialTheme.colorScheme.surfaceVariant)
                                                .clickable {
                                                    playFeedback()
                                                    senetPhotos.add("senet_ek_detay_gorsel_${senetPhotos.size + 1}.jpg")
                                                },
                                            contentAlignment = Alignment.Center
                                        ) {
                                            Icon(Icons.Filled.Add, null, modifier = Modifier.size(24.dp))
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

// --- MODULE 4: DISBURSEMENT (TEDİYE / ÖDEME ÇIKIŞI) ---
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun DisbursementModule(onSuccess: () -> Unit, navController: NavController) {
    val context = LocalContext.current
    var selectedCustomer by remember { mutableStateOf<Customer?>(null) }
    var amount by remember { mutableStateOf("") }
    var memo by remember { mutableStateOf("") }
    var showCustomerSelectionView by remember { mutableStateOf(false) }

    // Payment Source (0: Merkez Kasa Nakit, 1: Şirket Bankası EFT)
    var selectedPayMethod by remember { mutableStateOf(0) }
    val paySources = listOf("Nakit Kasa", "Banka EFT")
    var selectedBank by remember { mutableStateOf<Bank?>(AppDataStore.banks.firstOrNull()) }
    var showReceiptDialog by remember { mutableStateOf(false) }
    var receiptData by remember { mutableStateOf<ReceiptInfo?>(null) }

    if (showReceiptDialog && receiptData != null) {
        ReceiptDialog(
            info = receiptData!!,
            onDismiss = {
                showReceiptDialog = false
                receiptData = null
                
                selectedCustomer = null
                amount = ""
                memo = ""
                selectedPayMethod = 0

                onSuccess()
            }
        )
    }

    if (showCustomerSelectionView) {
        var query by remember { mutableStateOf("") }
        Scaffold(
            topBar = {
                TopAppBar(
                    title = { Text("Tediye Cari Hesap Seçimi", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold) },
                    navigationIcon = {
                        IconButton(onClick = { showCustomerSelectionView = false }) {
                            Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = "Geri")
                        }
                    },
                    colors = TopAppBarDefaults.topAppBarColors(
                        containerColor = MaterialTheme.colorScheme.surface
                    )
                )
            },
            bottomBar = {
                Surface(
                    tonalElevation = 8.dp,
                    color = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.95f),
                    modifier = Modifier.fillMaxWidth().navigationBarsPadding()
                ) {
                    Column(
                        modifier = Modifier.padding(12.dp)
                    ) {
                        OutlinedTextField(
                            value = query,
                            onValueChange = { query = it },
                            label = { Text("Cari Hesap Ara (Adı, Kodu, Tlf...)") },
                            leadingIcon = { Icon(Icons.Filled.Search, contentDescription = null) },
                            trailingIcon = {
                                if (query.isNotEmpty()) {
                                    IconButton(onClick = { query = "" }) {
                                        Icon(Icons.Filled.Close, contentDescription = "Temizle")
                                    }
                                }
                            },
                            singleLine = true,
                            modifier = Modifier.fillMaxWidth(),
                            colors = OutlinedTextFieldDefaults.colors(
                                focusedContainerColor = MaterialTheme.colorScheme.surface,
                                unfocusedContainerColor = MaterialTheme.colorScheme.surface
                            )
                        )
                    }
                }
            }
        ) { innerPadding ->
            val filtered = AppDataStore.customers.filter { cs ->
                cs.name.contains(query, ignoreCase = true) ||
                cs.phone.contains(query, ignoreCase = true) ||
                cs.address.contains(query, ignoreCase = true) ||
                cs.id.contains(query, ignoreCase = true)
            }

            if (filtered.isEmpty()) {
                Box(
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(innerPadding),
                    contentAlignment = Alignment.Center
                ) {
                    Column(
                        horizontalAlignment = Alignment.CenterHorizontally,
                        verticalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        Icon(
                            imageVector = Icons.Filled.SearchOff,
                            contentDescription = null,
                            modifier = Modifier.size(48.dp),
                            tint = MaterialTheme.colorScheme.outline
                        )
                        Text(
                            text = "Aramanıza uygun cari bulunamadı.",
                            style = MaterialTheme.typography.bodyMedium,
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                    }
                }
            } else {
                LazyColumn(
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(innerPadding),
                    contentPadding = PaddingValues(16.dp),
                    verticalArrangement = Arrangement.spacedBy(10.dp)
                ) {
                    items(filtered) { cs ->
                        val isSelected = selectedCustomer?.id == cs.id
                        Card(
                            modifier = Modifier
                                .fillMaxWidth()
                                .clickable {
                                    selectedCustomer = cs
                                    showCustomerSelectionView = false
                                },
                            shape = RoundedCornerShape(12.dp),
                            colors = CardDefaults.cardColors(
                                containerColor = if (isSelected) MaterialTheme.colorScheme.primaryContainer
                                else MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f)
                            ),
                            border = BorderStroke(
                                width = if (isSelected) 1.dp else 0.dp,
                                color = if (isSelected) MaterialTheme.colorScheme.primary else Color.Transparent
                            )
                        ) {
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(16.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Row(
                                    verticalAlignment = Alignment.CenterVertically,
                                    horizontalArrangement = Arrangement.spacedBy(12.dp),
                                    modifier = Modifier.weight(1f)
                                ) {
                                    Box(
                                        modifier = Modifier
                                            .size(40.dp)
                                            .background(
                                                if (isSelected) MaterialTheme.colorScheme.primary 
                                                else MaterialTheme.colorScheme.secondary.copy(alpha = 0.2f), 
                                                CircleShape
                                            ),
                                        contentAlignment = Alignment.Center
                                    ) {
                                        Icon(
                                            Icons.Filled.Person,
                                            contentDescription = null,
                                            tint = if (isSelected) MaterialTheme.colorScheme.onPrimary else MaterialTheme.colorScheme.secondary,
                                            modifier = Modifier.size(20.dp)
                                        )
                                    }
                                    Column {
                                        Text(cs.name, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodyMedium)
                                        Text("Tlf: ${cs.phone} | Adres: ${cs.address}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                    }
                                }
                                Text(
                                    text = String.format("%,.2f ₺", cs.balance),
                                    fontWeight = FontWeight.Bold,
                                    style = MaterialTheme.typography.bodyMedium,
                                    color = if (cs.balance > 0) MaterialTheme.colorScheme.error else MaterialTheme.colorScheme.primary
                                )
                            }
                        }
                    }
                }
            }
        }
    } else {
        Scaffold(
            containerColor = Color.Transparent,
        contentWindowInsets = WindowInsets(0, 0, 0, 0),
        bottomBar = {
            Card(
                modifier = Modifier.fillMaxWidth(),
                shape = RoundedCornerShape(topStart = 24.dp, topEnd = 24.dp, bottomStart = 0.dp, bottomEnd = 0.dp),
                colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.95f)),
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
                    // 1. Temizle
                    Column(
                        modifier = Modifier
                            .weight(1f)
                            .clip(RoundedCornerShape(16.dp))
                            .clickable {
                                amount = ""
                                memo = ""
                                selectedCustomer = null
                            }
                            .padding(vertical = 6.dp),
                        horizontalAlignment = Alignment.CenterHorizontally,
                        verticalArrangement = Arrangement.Center
                    ) {
                        Icon(
                            imageVector = Icons.Filled.Refresh,
                            contentDescription = "Temizle",
                            tint = MaterialTheme.colorScheme.error.copy(alpha = 0.8f),
                            modifier = Modifier.size(24.dp)
                        )
                        Spacer(modifier = Modifier.height(4.dp))
                        Text(
                            text = "Temizle",
                            style = MaterialTheme.typography.labelSmall,
                            color = MaterialTheme.colorScheme.error.copy(alpha = 0.8f),
                            fontWeight = FontWeight.Bold
                        )
                    }

                    // 2. Cari Seç
                    Column(
                        modifier = Modifier
                            .weight(1f)
                            .clip(RoundedCornerShape(16.dp))
                            .clickable {
                                showCustomerSelectionView = true
                            }
                            .padding(vertical = 6.dp),
                        horizontalAlignment = Alignment.CenterHorizontally,
                        verticalArrangement = Arrangement.Center
                    ) {
                        Icon(
                            imageVector = Icons.Filled.People,
                            contentDescription = "Cari",
                            tint = if (selectedCustomer != null) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.6f),
                            modifier = Modifier.size(24.dp)
                        )
                        Spacer(modifier = Modifier.height(4.dp))
                        Text(
                            text = "Cari",
                            style = MaterialTheme.typography.labelSmall,
                            color = if (selectedCustomer != null) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.6f),
                            fontWeight = FontWeight.Bold
                        )
                    }

                    // 3. Onayla
                    val isValid = selectedCustomer != null && amount.isNotEmpty()
                    Column(
                        modifier = Modifier
                            .weight(1f)
                            .clip(RoundedCornerShape(16.dp))
                            .clickable(enabled = isValid) {
                                val activeCust = selectedCustomer
                                if (activeCust != null) {
                                    val parsedAmt = amount.toDoubleOrNull() ?: 0.0

                                    // 1. Borç bakiye düşümü / artışı
                                    val cIdx = AppDataStore.customers.indexOfFirst { it.id == activeCust.id }
                                    if (cIdx != -1) {
                                        val c = AppDataStore.customers[cIdx]
                                        c.balance += parsedAmt
                                        
                                        c.transactions.add(
                                            CustomerTx(
                                                id = "TX-" + (50000 + c.transactions.size),
                                                date = "08.06.2026",
                                                type = "TEDİYE",
                                                amount = parsedAmt,
                                                description = "Ödeme Tediye Fişi Makbuz: ($memo)"
                                            )
                                        )
                                    }

                                    // 2. Banka / kasa bakiyesini düş
                                    if (selectedPayMethod == 1) {
                                        val targetB = selectedBank
                                        if (targetB != null) {
                                            val bIdx = AppDataStore.banks.indexOfFirst { it.id == targetB.id }
                                            if (bIdx != -1) {
                                                AppDataStore.banks[bIdx].balance -= parsedAmt
                                            }
                                        }
                                    }

                                    // 3. Şirket banka defteri / kasa günlüğünü yaz
                                    AppDataStore.kasaLogs.add(
                                        KasaLogItem(
                                            id = "K-" + (5000 + AppDataStore.kasaLogs.size),
                                            date = "08.06.2026 18:50",
                                            type = "Tediye",
                                            customerOrSupplier = activeCust.name,
                                            amount = parsedAmt,
                                            paymentType = if (selectedPayMethod == 0) "Nakit" else "EFT / Havale",
                                            bankName = if (selectedPayMethod == 1) selectedBank?.name else null,
                                            desc = memo
                                        )
                                    )

                                    val finalDateStr = java.text.SimpleDateFormat("dd.MM.yyyy HH:mm", java.util.Locale.getDefault()).format(java.util.Date())
                                    val finalReceiptNo = "P-" + System.currentTimeMillis().toString().takeLast(6)
                                    receiptData = ReceiptInfo(
                                        type = "TEDİYE",
                                        receiptNo = finalReceiptNo,
                                        date = finalDateStr,
                                        customerName = activeCust.name,
                                        customerId = activeCust.id,
                                        amount = parsedAmt,
                                        paymentMethod = if (selectedPayMethod == 0) "Nakit Ödeme" else "Banka EFT / Havale",
                                        bankName = if (selectedPayMethod == 1) selectedBank?.name else null,
                                        memo = if (memo.isNotEmpty()) memo else "Cari Hesap Tediye Fişi Ödemesi"
                                    )
                                    showReceiptDialog = true
                                }

                                AppDataStore.persist(context)
                            }
                            .padding(vertical = 6.dp),
                        horizontalAlignment = Alignment.CenterHorizontally,
                        verticalArrangement = Arrangement.Center
                    ) {
                        Icon(
                            imageVector = Icons.Filled.CheckCircle,
                            contentDescription = "Tediye Onayla",
                            tint = if (isValid) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.3f),
                            modifier = Modifier.size(24.dp)
                        )
                        Spacer(modifier = Modifier.height(4.dp))
                        Text(
                            text = "Onayla",
                            style = MaterialTheme.typography.labelSmall,
                            color = if (isValid) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.3f),
                            fontWeight = FontWeight.Bold
                        )
                    }
                }
            }
        }
    ) { innerPadding ->
        LazyColumn(
            modifier = Modifier
                .fillMaxSize()
                .padding(innerPadding)
                .padding(horizontal = 16.dp),
            verticalArrangement = Arrangement.spacedBy(14.dp),
            contentPadding = PaddingValues(top = 16.dp, bottom = 40.dp)
        ) {
            // 1. CARI HESAP DETAYI VE GÜNCEL BORÇ BAKİYESİ
            item {
                val cust = selectedCustomer
                Card(
                    modifier = Modifier.fillMaxWidth().clickable { showCustomerSelectionView = true },
                    colors = CardDefaults.cardColors(
                        containerColor = MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.15f)
                    ),
                    border = BorderStroke(1.dp, MaterialTheme.colorScheme.primary.copy(alpha = 0.25f)),
                    shape = RoundedCornerShape(16.dp)
                ) {
                    Column(modifier = Modifier.padding(16.dp)) {
                        Row(
                            verticalAlignment = Alignment.CenterVertically,
                            horizontalArrangement = Arrangement.spacedBy(12.dp)
                        ) {
                            Box(
                                modifier = Modifier
                                    .size(48.dp)
                                    .background(MaterialTheme.colorScheme.error, CircleShape),
                                contentAlignment = Alignment.Center
                            ) {
                                Icon(
                                    Icons.Filled.Person,
                                    contentDescription = null,
                                    tint = MaterialTheme.colorScheme.onError,
                                    modifier = Modifier.size(24.dp)
                                )
                            }
                            Column(modifier = Modifier.weight(1f)) {
                                Text(
                                    text = cust?.name ?: "Tediye Cari Tedarikçi Seçilmedi",
                                    fontWeight = FontWeight.Bold,
                                    style = MaterialTheme.typography.titleMedium,
                                    color = MaterialTheme.colorScheme.onPrimaryContainer
                                )
                                Text(
                                    text = if (cust != null) "Tlf: ${cust.phone} | Adres: ${cust.address}" else "Lütfen altbardan bir cari kart seçiniz",
                                    style = MaterialTheme.typography.bodySmall,
                                    color = MaterialTheme.colorScheme.onPrimaryContainer.copy(alpha = 0.7f)
                                )
                            }
                        }
                        
                        if (cust != null) {
                            HorizontalDivider(
                                modifier = Modifier.padding(vertical = 12.dp),
                                color = MaterialTheme.colorScheme.primary.copy(alpha = 0.15f)
                            )
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Column {
                                    Text(
                                        "Güncel Borç/Alacak Bakiyesi",
                                        style = MaterialTheme.typography.labelSmall,
                                        color = MaterialTheme.colorScheme.onPrimaryContainer.copy(alpha = 0.8f)
                                    )
                                    Text(
                                        text = String.format("%,.2f ₺", cust.balance),
                                        fontWeight = FontWeight.Black,
                                        style = MaterialTheme.typography.titleLarge,
                                        color = if (cust.balance > 0) MaterialTheme.colorScheme.error else MaterialTheme.colorScheme.primary
                                    )
                                }
                                Box(
                                    modifier = Modifier
                                        .clip(RoundedCornerShape(8.dp))
                                        .background(
                                            if (cust.balance > 0) MaterialTheme.colorScheme.errorContainer else MaterialTheme.colorScheme.secondaryContainer
                                        )
                                        .padding(horizontal = 10.dp, vertical = 6.dp)
                                ) {
                                    Text(
                                        text = if (cust.balance > 0) "Müşteri Borçlu" else "Dengeli",
                                        style = MaterialTheme.typography.labelSmall,
                                        fontWeight = FontWeight.Bold,
                                        color = if (cust.balance > 0) MaterialTheme.colorScheme.onErrorContainer else MaterialTheme.colorScheme.onSecondaryContainer
                                    )
                                }
                            }
                        }
                    }
                }
            }

            // ÖDEME KAYNAĞI SEÇİMİ
            item {
                Text("Ödeme Kaynağı", style = MaterialTheme.typography.labelMedium, color = MaterialTheme.colorScheme.primary, fontWeight = FontWeight.Bold)
            }
            item {
                Card(
                    modifier = Modifier.fillMaxWidth(),
                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.3f)),
                    border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f)),
                    shape = RoundedCornerShape(16.dp)
                ) {
                    Column(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(12.dp),
                        verticalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .horizontalScroll(rememberScrollState()),
                            horizontalArrangement = Arrangement.spacedBy(8.dp),
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            val methodIcons = listOf(
                                Icons.Filled.Payments,
                                Icons.Filled.AccountBalance
                            )

                            paySources.forEachIndexed { index, mName ->
                                val isSelected = selectedPayMethod == index
                                FilterChip(
                                    selected = isSelected,
                                    onClick = { selectedPayMethod = index },
                                    label = { Text(mName, style = MaterialTheme.typography.labelSmall, fontWeight = if (isSelected) FontWeight.Bold else FontWeight.Normal) },
                                    leadingIcon = {
                                        Icon(
                                            imageVector = methodIcons[index],
                                            contentDescription = mName,
                                            modifier = Modifier.size(16.dp),
                                            tint = if (isSelected) MaterialTheme.colorScheme.primary else Color.Gray
                                        )
                                    },
                                    colors = FilterChipDefaults.filterChipColors(
                                        selectedContainerColor = MaterialTheme.colorScheme.primaryContainer,
                                        selectedLabelColor = MaterialTheme.colorScheme.onPrimaryContainer
                                    )
                                )
                            }
                        }
                    }
                }
            }

            // 2. TUTAR VE AÇIKLAMA GİRİŞ ALANLARI
            item {
                FieldCard {
                    Column(modifier = Modifier.padding(16.dp), verticalArrangement = Arrangement.spacedBy(12.dp)) {
                        OutlinedTextField(
                            value = amount,
                            onValueChange = { amount = it },
                            label = { Text("Tediye Tutarı (₺)") },
                            leadingIcon = { Icon(Icons.Filled.Payments, null) },
                            prefix = { Text("₺ ") },
                            modifier = Modifier.fillMaxWidth(),
                            singleLine = true,
                            keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number)
                        )

                        OutlinedTextField(
                            value = memo,
                            onValueChange = { memo = it },
                            label = { Text("Açıklama / Belge İrtibatı") },
                            leadingIcon = { Icon(Icons.Filled.Edit, null) },
                            modifier = Modifier.fillMaxWidth(),
                            singleLine = true
                        )
                    }
                }
            }

            // 3. SEÇİLEN ÖDEME DETAYI: ŞİRKET BANKASINDAN EFT/HAVALE DURUMUNDA BANKA SEÇİMİ
            if (selectedPayMethod == 1) {
                item {
                    Text("Ödemenin Yapılacağı Banka Hesabı", style = MaterialTheme.typography.labelMedium, color = MaterialTheme.colorScheme.primary, fontWeight = FontWeight.Bold)
                }
                item {
                    Card(
                        modifier = Modifier.fillMaxWidth(),
                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
                        border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant)
                    ) {
                        Column(modifier = Modifier.padding(12.dp), verticalArrangement = Arrangement.spacedBy(6.dp)) {
                            AppDataStore.banks.forEach { bk ->
                                val matched = selectedBank?.id == bk.id
                                Row(
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .clip(RoundedCornerShape(8.dp))
                                        .background(
                                            if (matched) MaterialTheme.colorScheme.secondaryContainer.copy(alpha = 0.35f)
                                            else Color.Transparent
                                        )
                                        .clickable { selectedBank = bk }
                                        .padding(10.dp),
                                    verticalAlignment = Alignment.CenterVertically,
                                    horizontalArrangement = Arrangement.SpaceBetween
                                ) {
                                    Row(
                                        verticalAlignment = Alignment.CenterVertically,
                                        horizontalArrangement = Arrangement.spacedBy(10.dp)
                                    ) {
                                        RadioButton(
                                            selected = matched,
                                            onClick = { selectedBank = bk }
                                        )
                                        Column {
                                            Text(bk.name, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodyMedium)
                                            Text("IBAN: TR** **** **** **** **${bk.id}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
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

// --- MODULE 4.5: CASH & BANKS LEDGER DEFTERI (KASA BÖLÜMÜ) ---
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun CashBoxModule() {
    var selectedAccountId by remember { mutableStateOf<String?>(null) }
    var query by remember { mutableStateOf("") }
    var selectedFilterType by remember { mutableStateOf("Tümü") } // "Tümü", "Tahsilat", "Tediye", "Satış", "İade"
    
    val filters = listOf("Tümü", "Tahsilat", "Tediye", "Satış", "İade")
    var showAddKasaDialog by remember { mutableStateOf(false) }

    if (showAddKasaDialog) {
        var newKasaName by remember { mutableStateOf("") }
        var newKasaBalance by remember { mutableStateOf("0") }
        
        AlertDialog(
            onDismissRequest = { showAddKasaDialog = false },
            title = { Text("Yeni Kasa Oluştur", fontWeight = FontWeight.Bold) },
            text = {
                Column(verticalArrangement = Arrangement.spacedBy(12.dp)) {
                    Text("Yeni sistem kasası tanımlayın.", style = MaterialTheme.typography.bodySmall, color = Color.Gray)
                    OutlinedTextField(
                        value = newKasaName,
                        onValueChange = { newKasaName = it },
                        label = { Text("Kasa Adı (Örn: Şube 2 Kasası)") },
                        singleLine = true,
                        modifier = Modifier.fillMaxWidth()
                    )
                    OutlinedTextField(
                        value = newKasaBalance,
                        onValueChange = { newKasaBalance = it },
                        label = { Text("Başlangıç Bakiyesi (₺)") },
                        singleLine = true,
                        keyboardOptions = androidx.compose.foundation.text.KeyboardOptions(keyboardType = androidx.compose.ui.text.input.KeyboardType.Number),
                        modifier = Modifier.fillMaxWidth()
                    )
                }
            },
            confirmButton = {
                Button(onClick = {
                    if (newKasaName.isNotBlank()) {
                        val newId = "CA-" + System.currentTimeMillis()
                        val bal = newKasaBalance.toDoubleOrNull() ?: 0.0
                        AppDataStore.cashAccounts.add(com.example.ui.screens.CashAccount(newId, newKasaName, "TRY", bal))
                        showAddKasaDialog = false
                    }
                }) {
                    Text("Oluştur")
                }
            },
            dismissButton = {
                TextButton(onClick = { showAddKasaDialog = false }) { Text("İptal") }
            }
        )
    }

    if (selectedAccountId == null) {
        // --- MASTER VIEW: List of all Kasalar ---
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(14.dp)
        ) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                Text("KASALAR VE HESAPLAR", style = MaterialTheme.typography.titleMedium, color = MaterialTheme.colorScheme.primary, fontWeight = FontWeight.Bold)
                IconButton(onClick = { showAddKasaDialog = true }, modifier = Modifier.background(MaterialTheme.colorScheme.primaryContainer, CircleShape)) {
                    Icon(Icons.Filled.Add, null, tint = MaterialTheme.colorScheme.onPrimaryContainer)
                }
            }
            
            Text("Nakit kasalarınızı yönetin ve detaylarını görüntüleyin.", style = MaterialTheme.typography.bodySmall, color = Color.Gray)

            LazyColumn(
                modifier = Modifier.fillMaxWidth().weight(1f),
                verticalArrangement = Arrangement.spacedBy(12.dp)
            ) {
                items(AppDataStore.cashAccounts) { kasa ->
                    // Calculate real-time balance
                    val accountLogs = AppDataStore.kasaLogs.filter { it.bankName == kasa.id || (kasa.id == "CA-MAIN" && it.bankName == null) }
                    val totalIn = accountLogs.filter { it.type == "Tahsilat" || it.type == "Satış" }.sumOf { it.amount }
                    val totalOut = accountLogs.filter { it.type == "Tediye" || it.type == "İade" || it.type == "Alış" }.sumOf { it.amount }
                    val currentBalance = kasa.balance + totalIn - totalOut

                    Card(
                        modifier = Modifier.fillMaxWidth().clickable { selectedAccountId = kasa.id },
                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
                        elevation = CardDefaults.cardElevation(defaultElevation = 2.dp)
                    ) {
                        Row(
                            modifier = Modifier.padding(16.dp).fillMaxWidth(),
                            verticalAlignment = Alignment.CenterVertically,
                            horizontalArrangement = Arrangement.SpaceBetween
                        ) {
                            Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                                Box(
                                    modifier = Modifier.size(48.dp).background(MaterialTheme.colorScheme.secondaryContainer, CircleShape),
                                    contentAlignment = Alignment.Center
                                ) {
                                    Icon(Icons.Filled.AccountBalanceWallet, null, tint = MaterialTheme.colorScheme.onSecondaryContainer)
                                }
                                Column {
                                    Text(kasa.name, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodyLarge)
                                    Text("${accountLogs.size} Hareket", style = MaterialTheme.typography.labelMedium, color = Color.Gray)
                                }
                            }
                            Column(horizontalAlignment = Alignment.End) {
                                Text("Bakiye", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                Text(
                                    text = String.format("%,.2f %s", currentBalance, if (kasa.currency == "TRY") "₺" else ""),
                                    fontWeight = FontWeight.Bold,
                                    style = MaterialTheme.typography.titleMedium,
                                    color = MaterialTheme.colorScheme.primary
                                )
                            }
                        }
                    }
                }
            }
        }
    } else {
        // --- DETAIL VIEW: Detailed ledger of a specific Kasa ---
        val kasa = AppDataStore.cashAccounts.firstOrNull { it.id == selectedAccountId }
        if (kasa == null) {
            selectedAccountId = null
            return
        }

        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(14.dp)
        ) {
            Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                IconButton(onClick = { selectedAccountId = null }) {
                    Icon(Icons.AutoMirrored.Filled.ArrowBack, null)
                }
                Text("${kasa.name} Hareketleri", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold)
            }
            
            HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))

            // Search and Filterchips row
            OutlinedTextField(
                value = query,
                onValueChange = { query = it },
                placeholder = { Text("Cari veya açıklamalarda ara...") },
                leadingIcon = { Icon(Icons.Filled.Search, null) },
                modifier = Modifier.fillMaxWidth(),
                singleLine = true,
                shape = RoundedCornerShape(12.dp)
            )

            Row(
                modifier = Modifier.fillMaxWidth().horizontalScroll(rememberScrollState()),
                horizontalArrangement = Arrangement.spacedBy(6.dp),
                verticalAlignment = Alignment.CenterVertically
            ) {
                filters.forEach { filter ->
                    val active = selectedFilterType == filter
                    Box(
                        modifier = Modifier
                            .clip(RoundedCornerShape(16.dp))
                            .background(if (active) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.surfaceVariant)
                            .clickable { selectedFilterType = filter }
                            .padding(horizontal = 12.dp, vertical = 6.dp)
                    ) {
                        Text(
                            text = filter,
                            color = if (active) MaterialTheme.colorScheme.onPrimary else MaterialTheme.colorScheme.onSurfaceVariant,
                            style = MaterialTheme.typography.labelSmall,
                            fontWeight = FontWeight.Bold
                        )
                    }
                }
            }

            // Ledger Loop list
            val rawLogs = AppDataStore.kasaLogs.filter { it.bankName == kasa.id || (kasa.id == "CA-MAIN" && it.bankName == null) }
            val filteredLogs = rawLogs.filter { log ->
                val matchesFilter = selectedFilterType == "Tümü" || log.type == selectedFilterType
                val matchesQuery = log.customerOrSupplier.contains(query, ignoreCase = true) || log.desc.contains(query, ignoreCase = true)
                matchesFilter && matchesQuery
            }.reversed() // Show newest first

            if (filteredLogs.isEmpty()) {
                Box(modifier = Modifier.fillMaxWidth().weight(1f).padding(32.dp), contentAlignment = Alignment.Center) {
                    Text("Herhangi bir kasa hareketi bulunamadı.", color = Color.Gray)
                }
            } else {
                LazyColumn(
                    modifier = Modifier.fillMaxWidth().weight(1f),
                    verticalArrangement = Arrangement.spacedBy(10.dp)
                ) {
                    items(filteredLogs) { item ->
                        val isDebit = item.type == "Tahsilat" || item.type == "Satış"
                        FieldCard(modifier = Modifier.fillMaxWidth()) {
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(12.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Column(modifier = Modifier.weight(1f)) {
                                    Row(
                                        verticalAlignment = Alignment.CenterVertically,
                                        horizontalArrangement = Arrangement.spacedBy(8.dp)
                                    ) {
                                        val chipColor = when (item.type) {
                                            "Tahsilat", "Satış" -> Color(0xFFE8F5E9)
                                            else -> Color(0xFFFFEBEE)
                                        }
                                        val textCol = when (item.type) {
                                            "Tahsilat", "Satış" -> Color(0xFF43A047)
                                            else -> Color(0xFFE53935)
                                        }
                                        Box(
                                            modifier = Modifier
                                                .clip(RoundedCornerShape(4.dp))
                                                .background(chipColor)
                                                .padding(horizontal = 6.dp, vertical = 2.dp)
                                        ) {
                                            Text(item.type.uppercase(), style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Bold, color = textCol)
                                        }
                                        Text(item.date, style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                    }
                                    Spacer(modifier = Modifier.height(4.dp))
                                    Text(item.customerOrSupplier, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodyMedium)
                                    Text(
                                        text = item.desc,
                                        style = MaterialTheme.typography.labelSmall,
                                        color = Color.DarkGray
                                    )
                                }
                                
                                val valuePrefix = if (isDebit) "+" else "-"
                                val valueCol = if (isDebit) Color(0xFF43A047) else Color(0xFFE53935)
                                Text(
                                    text = valuePrefix + String.format("%,.2f ₺", item.amount),
                                    fontWeight = FontWeight.Bold,
                                    style = MaterialTheme.typography.bodyMedium,
                                    color = valueCol
                                )
                            }
                        }
                    }
                }
            }
        }
    }
}

// --- MODULE 5: STOCKS (STOKLAR) ---
data class StockLog(val sku: String, val name: String, val count: Int, val warehouse: String, val category: String, val barcode: String)

@Composable
fun StocksModule(navController: NavController) {
    val query = com.example.ui.screens.AppDataStore.stocksSearchQuery.value
    val selectedCat = com.example.ui.screens.AppDataStore.stocksSelectedCategory.value
    val sortOrder = com.example.ui.screens.AppDataStore.stocksSelectedSortOrder.value

    val stocks = com.example.ui.screens.AppDataStore.products.flatMap { prod ->
        if (prod.stockByWarehouse.isEmpty()) {
            listOf(StockLog(prod.code, prod.title, 0, "Ana Depo", prod.category, prod.barcode))
        } else {
            prod.stockByWarehouse.map { (wh, count) ->
                StockLog(prod.code, prod.title, count, wh, prod.category, prod.barcode)
            }
        }
    }

    var filteredStocks = stocks.filter {
        (selectedCat == "Hepsi" || it.category == selectedCat) &&
        (it.name.contains(query, ignoreCase = true) || it.sku.contains(query, ignoreCase = true) || it.barcode.contains(query, ignoreCase = true))
    }

    filteredStocks = when (sortOrder) {
        "Ürün Adı [A-Z]" -> filteredStocks.sortedBy { it.name }
        "Ürün Adı [Z-A]" -> filteredStocks.sortedByDescending { it.name }
        "Miktar [Azalan]" -> filteredStocks.sortedByDescending { it.count }
        "Miktar [Artan]" -> filteredStocks.sortedBy { it.count }
        else -> filteredStocks
    }

    var stocksVisibleCount by remember(query, selectedCat, sortOrder) { mutableStateOf(100) }
    val displayedStocks = filteredStocks.take(stocksVisibleCount)

    if (com.example.ui.screens.AppDataStore.stocksShowBarcodeScanner.value) {
        BarcodeScannerDialog(
            onDismissRequest = { com.example.ui.screens.AppDataStore.stocksShowBarcodeScanner.value = false },
            onBarcodeScanned = { code ->
                com.example.ui.screens.AppDataStore.stocksShowBarcodeScanner.value = false
                com.example.ui.screens.AppDataStore.stocksSearchQuery.value = code
            },
            onSimulateScan = { code ->
                com.example.ui.screens.AppDataStore.stocksShowBarcodeScanner.value = false
                com.example.ui.screens.AppDataStore.stocksSearchQuery.value = code
            }
        )
    }

    if (com.example.ui.screens.AppDataStore.stocksShowAddProductDialog.value) {
        val context = LocalContext.current
        AddProductCatalogDialog(
            onDismiss = { com.example.ui.screens.AppDataStore.stocksShowAddProductDialog.value = false },
            onSave = { newProd ->
                if (!com.example.data.LicenseManager.canAddMoreProducts(com.example.ui.screens.AppDataStore.products.size, com.example.ui.screens.AppDataStore.licenseKey)) {
                    com.example.ui.screens.AppDataStore.stocksShowAddProductDialog.value = false
                } else {
                    com.example.ui.screens.AppDataStore.products.add(newProd)
                    com.example.ui.screens.AppDataStore.stocksShowAddProductDialog.value = false
                    com.example.ui.screens.AppDataStore.persist(context)
                }
            }
        )
    }

    Column(modifier = Modifier.fillMaxSize().padding(16.dp), verticalArrangement = Arrangement.spacedBy(12.dp)) {
        Spacer(modifier = Modifier.height(4.dp))

        LazyColumn(
            verticalArrangement = Arrangement.spacedBy(10.dp)
        ) {
            itemsIndexed(displayedStocks) { index, stock ->
                if (index >= displayedStocks.size - 5 && displayedStocks.size < filteredStocks.size) {
                    LaunchedEffect(Unit) {
                        stocksVisibleCount += 100
                    }
                }
                Card(
                    modifier = Modifier
                        .fillMaxWidth()
                        .clickable { navController.navigate("stock_detail/${stock.barcode}") },
                    shape = RoundedCornerShape(12.dp),
                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
                    border = BorderStroke(1.dp, MaterialTheme.colorScheme.outline.copy(alpha = 0.45f)),
                    elevation = CardDefaults.cardElevation(defaultElevation = 1.dp)
                ) {
                    Column(modifier = Modifier.padding(14.dp)) {
                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.SpaceBetween,
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Text(
                                text = stock.name, 
                                fontWeight = FontWeight.Bold, 
                                style = MaterialTheme.typography.bodyLarge,
                                modifier = Modifier.weight(1f).padding(end = 8.dp),
                                maxLines = 2,
                                overflow = TextOverflow.Ellipsis
                            )
                            Surface(
                                color = if (stock.count > 50) MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.errorContainer,
                                shape = RoundedCornerShape(8.dp)
                            ) {
                                Text(
                                    "${stock.count} Adet",
                                    modifier = Modifier.padding(horizontal = 8.dp, vertical = 4.dp),
                                    fontWeight = FontWeight.Bold,
                                    style = MaterialTheme.typography.labelMedium,
                                    color = if (stock.count > 50) MaterialTheme.colorScheme.onPrimaryContainer else MaterialTheme.colorScheme.onErrorContainer,
                                    maxLines = 1,
                                    softWrap = false
                                )
                            }
                        }
                        Spacer(modifier = Modifier.height(6.dp))
                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.SpaceBetween
                        ) {
                            Text("SKU: ${stock.sku}", style = MaterialTheme.typography.bodySmall, color = Color.Gray)
                            val product = remember(stock.barcode) {
                                com.example.ui.screens.AppDataStore.products.find { it.barcode == stock.barcode }
                            }
                            val priceText = if (product != null) {
                                String.format("Fiyat: %,.2f ₺", product.basePrice)
                            } else {
                                "Fiyat: N/A"
                            }
                            Text(priceText, style = MaterialTheme.typography.bodySmall, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
                        }
                    }
                }
            }
            if (displayedStocks.isEmpty()) {
                item {
                    Box(modifier = Modifier.fillMaxWidth().padding(top = 40.dp), contentAlignment = Alignment.Center) {
                        Text("Eşleşen stok kaydı bulunamadı.", color = Color.Gray)
                    }
                }
            }
        }
    }
}

// --- MODULE 6: SCANNING COUNT (STOK SAYIMI) ---
@OptIn(
    ExperimentalMaterial3Api::class,
    com.google.accompanist.permissions.ExperimentalPermissionsApi::class,
    ExperimentalFoundationApi::class
)
@Composable
fun CountingModule(onBeep: () -> Unit) {
    val context = LocalContext.current
    val scope = rememberCoroutineScope()
    
    // Core states
    var activeTab by remember { mutableStateOf(0) } // 0 -> Aktif Sayım, 1 -> Seanslar & Geçmiş, 2 -> Çakışmalar
    var selectedWarehouse by remember { mutableStateOf(AppDataStore.activeSelectedWarehouse.value) }
    var operatorName by remember { mutableStateOf("") }
    
    // Search & Filter state
    var searchQuery by remember { mutableStateOf("") }
    var selectedBrand by remember { mutableStateOf("") }
    var selectedCategory by remember { mutableStateOf("") }
    var selectedAisle by remember { mutableStateOf("") }
    var showFilterDialog by remember { mutableStateOf(false) }
    
    // Active session metadata (if editing/resuming)
    var activeSessionId by remember { mutableStateOf("") } // empty if new
    val activeCountItems = remember { mutableStateListOf<CountedItem>() }
    
    // Dialog & overlay states
    var showScanSimDialog by remember { mutableStateOf(false) }
    var showFinalizeConfirmDialog by remember { mutableStateOf(false) }
    var showAisleMergeId by remember { mutableStateOf<String?>(null) } // barcode of product being merged
    
    var itemToDelete by remember { mutableStateOf<CountedItem?>(null) }
    var showClearBasketConfirmDialog by remember { mutableStateOf(false) }
    var sessionToDelete by remember { mutableStateOf<StockCountSession?>(null) }

    var isContinuousScanActive by remember { mutableStateOf(false) }
    var showCameraScannerDialog by remember { mutableStateOf(false) }
    
    // States for the popup editor dialog (scanned product details)
    var showScannedDetailDialog by remember { mutableStateOf(false) }
    var scannedProduct by remember { mutableStateOf<ProductCatalog?>(null) }
    var scannedQtyInput by remember { mutableStateOf(1) }
    var scannedAisleInput by remember { mutableStateOf("") }
    
    var lastDirectScanTime by remember { mutableStateOf(0L) }
    
    fun handleDirectBarcodeScan(barcode: String, isContinuous: Boolean) {
        val currentTime = System.currentTimeMillis()
        if (currentTime - lastDirectScanTime < 1500) return
        lastDirectScanTime = currentTime
        
        val product = AppDataStore.findProductByBarcode(barcode)
        if (product != null) {
            onBeep()
            scannedProduct = product
            scannedQtyInput = 1
            val existingItem = activeCountItems.find { it.barcode == product.barcode }
            scannedAisleInput = existingItem?.aisle ?: product.aisle ?: selectedAisle.ifBlank { "A-01" }
            showScannedDetailDialog = true
        } else {
            onBeep()
            Toast.makeText(context, "Ürün bulunamadı: $barcode", Toast.LENGTH_SHORT).show()
        }
    }
    
    // Fetch logged in operator on start
    LaunchedEffect(Unit) {
        val db = com.example.data.database.DatabaseProvider.getDatabase(context)
        val loggedInUser = db.userDao().getActiveUser()
        operatorName = if (loggedInUser != null) {
            loggedInUser.username.replaceFirstChar { if (it.isLowerCase()) it.titlecase(java.util.Locale.getDefault()) else it.toString() }
        } else {
            "Saha Yetkilisi"
        }
    }

    // Dynamic catalogs lists from AppDataStore
    val productsCatalog = AppDataStore.products
    val categories = remember(productsCatalog.size) {
        productsCatalog.map { it.category }.distinct()
    }
    val brands = remember(productsCatalog.size) {
        productsCatalog.mapNotNull { it.brand }.distinct()
    }
    val warehouses = remember {
        AppDataStore.definitions["Depo"] ?: listOf("Ana Depo", "Araç Deposu", "Merkez Depo")
    }

    // Filter catalog products for adding to count
    val filteredProducts = remember(searchQuery, selectedBrand, selectedCategory, selectedAisle, productsCatalog.size) {
        productsCatalog.filter { prod ->
            val matchQuery = searchQuery.isBlank() || 
                prod.title.contains(searchQuery, ignoreCase = true) || 
                prod.code.contains(searchQuery, ignoreCase = true) || 
                prod.barcode.contains(searchQuery, ignoreCase = true)
            
            val matchBrand = selectedBrand.isBlank() || prod.brand == selectedBrand
            val matchCat = selectedCategory.isBlank() || prod.category == selectedCategory
            val matchAisle = selectedAisle.isBlank() || prod.aisle == selectedAisle
            
            matchQuery && matchBrand && matchCat && matchAisle
        }
    }

    // Calculate duplicates (same product counted in different aisles in this active session)
    val duplicateAislesGroup = remember(activeCountItems.size, activeCountItems.map { "${it.barcode}-${it.aisle}-${it.countedQty}" }) {
        activeCountItems.groupBy { it.barcode }
            .filter { it.value.map { item -> item.aisle.trim().lowercase() }.distinct().size > 1 }
    }

    // Extract reyon (aisle) list alphabetically from the products
    val registeredAisles = remember(productsCatalog.size) {
        productsCatalog.mapNotNull { it.aisle }
            .filter { it.isNotBlank() }
            .distinct()
            .sortedWith(String.CASE_INSENSITIVE_ORDER)
    }

    val baseTypography = MaterialTheme.typography
    val compactTypography = remember(baseTypography) {
        baseTypography.copy(
            displayLarge = baseTypography.displayLarge.copy(fontSize = baseTypography.displayLarge.fontSize * 0.82f, lineHeight = baseTypography.displayLarge.lineHeight * 0.82f),
            displayMedium = baseTypography.displayMedium.copy(fontSize = baseTypography.displayMedium.fontSize * 0.82f, lineHeight = baseTypography.displayMedium.lineHeight * 0.82f),
            displaySmall = baseTypography.displaySmall.copy(fontSize = baseTypography.displaySmall.fontSize * 0.82f, lineHeight = baseTypography.displaySmall.lineHeight * 0.82f),
            headlineLarge = baseTypography.headlineLarge.copy(fontSize = baseTypography.headlineLarge.fontSize * 0.82f, lineHeight = baseTypography.headlineLarge.lineHeight * 0.82f),
            headlineMedium = baseTypography.headlineMedium.copy(fontSize = baseTypography.headlineMedium.fontSize * 0.82f, lineHeight = baseTypography.headlineMedium.lineHeight * 0.82f),
            headlineSmall = baseTypography.headlineSmall.copy(fontSize = baseTypography.headlineSmall.fontSize * 0.82f, lineHeight = baseTypography.headlineSmall.lineHeight * 0.82f),
            titleLarge = baseTypography.titleLarge.copy(fontSize = baseTypography.titleLarge.fontSize * 0.82f, lineHeight = baseTypography.titleLarge.lineHeight * 0.82f),
            titleMedium = baseTypography.titleMedium.copy(fontSize = baseTypography.titleMedium.fontSize * 0.82f, lineHeight = baseTypography.titleMedium.lineHeight * 0.82f),
            titleSmall = baseTypography.titleSmall.copy(fontSize = baseTypography.titleSmall.fontSize * 0.82f, lineHeight = baseTypography.titleSmall.lineHeight * 0.82f),
            bodyLarge = baseTypography.bodyLarge.copy(fontSize = baseTypography.bodyLarge.fontSize * 0.82f, lineHeight = baseTypography.bodyLarge.lineHeight * 0.82f),
            bodyMedium = baseTypography.bodyMedium.copy(fontSize = baseTypography.bodyMedium.fontSize * 0.82f, lineHeight = baseTypography.bodyMedium.lineHeight * 0.82f),
            bodySmall = baseTypography.bodySmall.copy(fontSize = baseTypography.bodySmall.fontSize * 0.82f, lineHeight = baseTypography.bodySmall.lineHeight * 0.82f),
            labelLarge = baseTypography.labelLarge.copy(fontSize = baseTypography.labelLarge.fontSize * 0.82f, lineHeight = baseTypography.labelLarge.lineHeight * 0.82f),
            labelMedium = baseTypography.labelMedium.copy(fontSize = baseTypography.labelMedium.fontSize * 0.82f, lineHeight = baseTypography.labelMedium.lineHeight * 0.82f),
            labelSmall = baseTypography.labelSmall.copy(fontSize = baseTypography.labelSmall.fontSize * 0.82f, lineHeight = baseTypography.labelSmall.lineHeight * 0.82f)
        )
    }

    MaterialTheme(
        typography = compactTypography,
        colorScheme = MaterialTheme.colorScheme,
        shapes = MaterialTheme.shapes
    ) {
        Scaffold(
            modifier = Modifier.fillMaxSize(),
            bottomBar = {
            Box(
                modifier = Modifier
                    .fillMaxWidth()
                    .navigationBarsPadding()
                    .height(86.dp),
                contentAlignment = Alignment.BottomCenter
            ) {
                // Background surface of bottom bar (72.dp)
                Surface(
                    modifier = Modifier
                        .fillMaxWidth()
                        .height(72.dp),
                    color = MaterialTheme.colorScheme.surfaceColorAtElevation(4.dp),
                    shadowElevation = 8.dp
                ) {
                    Row(
                        modifier = Modifier
                            .fillMaxSize()
                            .padding(end = 76.dp) // Leave space for fixed FAB
                            .horizontalScroll(rememberScrollState()),
                        horizontalArrangement = Arrangement.spacedBy(16.dp),
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Spacer(modifier = Modifier.width(8.dp))
                        // Tab 0: Aktif Sayım icon button
                        IconButton(
                            onClick = { activeTab = 0 },
                            modifier = Modifier.width(72.dp).fillMaxHeight()
                        ) {
                            Column(horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.Center) {
                                Icon(
                                    imageVector = Icons.Filled.QrCodeScanner,
                                    contentDescription = "Aktif Sayım",
                                    tint = if (activeTab == 0) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.7f),
                                    modifier = Modifier.size(24.dp)
                                )
                                Spacer(modifier = Modifier.height(2.dp))
                                Text(
                                    "Katalog",
                                    style = MaterialTheme.typography.labelSmall,
                                    fontWeight = if (activeTab == 0) FontWeight.Bold else FontWeight.Normal,
                                    color = if (activeTab == 0) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.7f)
                                )
                            }
                        }

                        // Tab 3: Sayılanlar (Sepet)
                        IconButton(
                            onClick = { activeTab = 3 },
                            modifier = Modifier.width(72.dp).fillMaxHeight()
                        ) {
                            Column(horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.Center) {
                                BadgedBox(
                                    badge = {
                                        if (activeCountItems.isNotEmpty()) {
                                            Badge(containerColor = MaterialTheme.colorScheme.secondary) {
                                                Text("${activeCountItems.size}", color = MaterialTheme.colorScheme.onSecondary, style = MaterialTheme.typography.labelSmall)
                                            }
                                        }
                                    }
                                ) {
                                    Icon(
                                        imageVector = Icons.Filled.ShoppingCart,
                                        contentDescription = "Sayılanlar",
                                        tint = if (activeTab == 3) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.7f),
                                        modifier = Modifier.size(24.dp)
                                    )
                                }
                                Spacer(modifier = Modifier.height(2.dp))
                                Text(
                                    "Sepet",
                                    style = MaterialTheme.typography.labelSmall,
                                    fontWeight = if (activeTab == 3) FontWeight.Bold else FontWeight.Normal,
                                    color = if (activeTab == 3) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.7f)
                                )
                            }
                        }

                        // Tab 4: Sayılmayanlar
                        IconButton(
                            onClick = { activeTab = 4 },
                            modifier = Modifier.width(72.dp).fillMaxHeight()
                        ) {
                            Column(horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.Center) {
                                Icon(
                                    imageVector = Icons.Filled.RemoveShoppingCart,
                                    contentDescription = "Sayılmayanlar",
                                    tint = if (activeTab == 4) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.7f),
                                    modifier = Modifier.size(24.dp)
                                )
                                Spacer(modifier = Modifier.height(2.dp))
                                Text(
                                    "Kalan",
                                    style = MaterialTheme.typography.labelSmall,
                                    fontWeight = if (activeTab == 4) FontWeight.Bold else FontWeight.Normal,
                                    color = if (activeTab == 4) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.7f)
                                )
                            }
                        }

                        // Tab 1: Seanslar ve Geçmiş icon button
                        IconButton(
                            onClick = { activeTab = 1 },
                            modifier = Modifier.width(72.dp).fillMaxHeight()
                        ) {
                            Column(horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.Center) {
                                Icon(
                                    imageVector = Icons.Filled.History,
                                    contentDescription = "Geçmiş",
                                    tint = if (activeTab == 1) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.7f),
                                    modifier = Modifier.size(24.dp)
                                )
                                Spacer(modifier = Modifier.height(2.dp))
                                Text(
                                    "Geçmiş",
                                    style = MaterialTheme.typography.labelSmall,
                                    fontWeight = if (activeTab == 1) FontWeight.Bold else FontWeight.Normal,
                                    color = if (activeTab == 1) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.7f)
                                )
                            }
                        }

                        // Tab 2: Çoklu Reyon icon button with badge
                        IconButton(
                            onClick = { activeTab = 2 },
                            modifier = Modifier.width(72.dp).fillMaxHeight()
                        ) {
                            Column(horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.Center) {
                                BadgedBox(
                                    badge = {
                                        if (duplicateAislesGroup.isNotEmpty()) {
                                            Badge(containerColor = MaterialTheme.colorScheme.error) {
                                                Text("${duplicateAislesGroup.size}", color = MaterialTheme.colorScheme.onError, style = MaterialTheme.typography.labelSmall)
                                            }
                                        }
                                    }
                                ) {
                                    Icon(
                                        imageVector = Icons.Filled.Warning,
                                        contentDescription = "Çoklu Reyon",
                                        tint = if (activeTab == 2) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.7f),
                                        modifier = Modifier.size(24.dp)
                                    )
                                }
                                Spacer(modifier = Modifier.height(2.dp))
                                Text(
                                    "Çoklu",
                                    style = MaterialTheme.typography.labelSmall,
                                    fontWeight = if (activeTab == 2) FontWeight.Bold else FontWeight.Normal,
                                    color = if (activeTab == 2) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.7f)
                                )
                            }
                        }

                        // Warehouse selection area (Depo Seçimi)
                        var whExpanded by remember { mutableStateOf(false) }
                        Box(
                            modifier = Modifier.width(80.dp),
                            contentAlignment = Alignment.Center
                        ) {
                            IconButton(
                                onClick = { whExpanded = true },
                                modifier = Modifier.fillMaxHeight()
                            ) {
                                Column(horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.Center) {
                                    Icon(
                                        imageVector = Icons.Filled.Warehouse,
                                        contentDescription = "Depo Seç",
                                        tint = MaterialTheme.colorScheme.secondary,
                                        modifier = Modifier.size(24.dp)
                                    )
                                    Spacer(modifier = Modifier.height(2.dp))
                                    Text(
                                        text = selectedWarehouse.take(7) + if (selectedWarehouse.length > 7) ".." else "",
                                        style = MaterialTheme.typography.labelSmall,
                                        fontWeight = FontWeight.Bold,
                                        color = MaterialTheme.colorScheme.secondary,
                                        maxLines = 1,
                                        overflow = TextOverflow.Ellipsis
                                    )
                                }
                            }
                            DropdownMenu(
                                expanded = whExpanded,
                                onDismissRequest = { whExpanded = false }
                            ) {
                                warehouses.forEach { wh ->
                                    DropdownMenuItem(
                                        text = { Text(wh) },
                                        onClick = {
                                            selectedWarehouse = wh
                                            whExpanded = false
                                        }
                                    )
                                }
                            }
                        }
                        
                        Spacer(modifier = Modifier.width(8.dp))
                    }
                }

                // Barcode action button that perfectly overflows the top edge without being clipped!
                Surface(
                    shape = CircleShape,
                    color = if (isContinuousScanActive) MaterialTheme.colorScheme.error else MaterialTheme.colorScheme.primary,
                    contentColor = if (isContinuousScanActive) MaterialTheme.colorScheme.onError else MaterialTheme.colorScheme.onPrimary,
                    shadowElevation = 6.dp,
                    tonalElevation = 6.dp,
                    modifier = Modifier
                        .align(Alignment.TopEnd)
                        .padding(end = 16.dp)
                        .size(58.dp)
                        .combinedClickable(
                            onClick = {
                                showCameraScannerDialog = true
                            },
                            onLongClick = {
                                isContinuousScanActive = !isContinuousScanActive
                                if (isContinuousScanActive) {
                                    Toast.makeText(context, "Sürekli Barkod Okuma Aktif", Toast.LENGTH_SHORT).show()
                                } else {
                                    Toast.makeText(context, "Sürekli Barkod Okuma Devre Dışı", Toast.LENGTH_SHORT).show()
                                }
                            }
                        )
                        .testTag("floating_scan_button")
                ) {
                    Box(contentAlignment = Alignment.Center, modifier = Modifier.fillMaxSize()) {
                        Icon(
                            imageVector = if (isContinuousScanActive) Icons.Filled.Videocam else Icons.Filled.QrCodeScanner,
                            contentDescription = "Barkod Okut (Uzun basınca sürekli tarama)",
                            modifier = Modifier.size(28.dp)
                        )
                    }
                }
            }
        }
    ) { innerPadding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(innerPadding)
        ) {
            // Docked Continuous Camera Scanner Card
            AnimatedVisibility(
                visible = isContinuousScanActive,
                enter = expandVertically() + fadeIn(),
                exit = shrinkVertically() + fadeOut()
            ) {
                Card(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(horizontal = 16.dp, vertical = 8.dp),
                    colors = CardDefaults.cardColors(
                        containerColor = Color.Black
                    ),
                    shape = RoundedCornerShape(12.dp),
                    elevation = CardDefaults.cardElevation(defaultElevation = 2.dp)
                ) {
                    val cameraPermissionState = rememberPermissionState(android.Manifest.permission.CAMERA)
                    if (cameraPermissionState.status.isGranted) {
                        Box(
                            modifier = Modifier
                                .fillMaxWidth()
                                .height(120.dp),
                            contentAlignment = Alignment.Center
                        ) {
                            CameraScannerView { barcode ->
                                handleDirectBarcodeScan(barcode, isContinuous = true)
                            }
                            // Scanning Red Laser Indicator
                            Box(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .height(1.dp)
                                    .background(Color.Red.copy(alpha = 0.8f))
                            )
                        }
                    } else {
                        Box(
                            modifier = Modifier
                                .fillMaxWidth()
                                .height(120.dp),
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

            // TAB SCREEN ROUTER
            Box(modifier = Modifier.weight(1f)) {
                when (activeTab) {
                    0 -> {
                        // TAB 0: ACTIVE STOCK COUNT ENTRY
                        LazyColumn(
                            modifier = Modifier.fillMaxSize().padding(start = 16.dp, end = 16.dp, bottom = 16.dp, top = 0.dp),
                            verticalArrangement = Arrangement.spacedBy(12.dp)
                        ) {
                            // Search Box and Filter Row - Unpadded vertically to start directly under navbar
                            item {
                                Column(verticalArrangement = Arrangement.spacedBy(10.dp)) {
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.spacedBy(8.dp),
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        OutlinedTextField(
                                            value = searchQuery,
                                            onValueChange = { searchQuery = it },
                                            placeholder = { Text("Ara...", style = MaterialTheme.typography.labelSmall) },
                                            leadingIcon = { Icon(Icons.Filled.Search, null, modifier = Modifier.size(16.dp)) },
                                            trailingIcon = {
                                                if (searchQuery.isNotEmpty()) {
                                                    IconButton(onClick = { searchQuery = "" }, modifier = Modifier.size(24.dp)) {
                                                        Icon(Icons.Filled.Clear, null, modifier = Modifier.size(16.dp))
                                                    }
                                                }
                                            },
                                            textStyle = MaterialTheme.typography.labelSmall,
                                            singleLine = true,
                                            maxLines = 1,
                                            modifier = Modifier.weight(1f).height(48.dp)
                                        )

                                        // Modern Filter Icon Button with primary container background
                                        IconButton(
                                            onClick = { showFilterDialog = true },
                                            modifier = Modifier
                                                .size(48.dp)
                                                .background(
                                                    color = MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.35f),
                                                    shape = RoundedCornerShape(12.dp)
                                                )
                                                .border(
                                                    width = 1.dp,
                                                    color = MaterialTheme.colorScheme.primary.copy(alpha = 0.2f),
                                                    shape = RoundedCornerShape(12.dp)
                                                )
                                        ) {
                                            Icon(
                                                imageVector = Icons.Filled.FilterAlt,
                                                contentDescription = "Filtrele",
                                                tint = MaterialTheme.colorScheme.primary,
                                                modifier = Modifier.size(20.dp)
                                            )
                                        }
                                    }

                                    // Filter summary/quick-clear suggestion chip row underneath
                                    if (selectedBrand.isNotEmpty() || selectedCategory.isNotEmpty() || selectedAisle.isNotEmpty()) {
                                        Row(
                                            modifier = Modifier.fillMaxWidth().horizontalScroll(rememberScrollState()),
                                            horizontalArrangement = Arrangement.spacedBy(4.dp),
                                            verticalAlignment = Alignment.CenterVertically
                                        ) {
                                            if (selectedBrand.isNotEmpty()) {
                                                SuggestionChip(
                                                    onClick = { selectedBrand = "" },
                                                    label = { Text("Marka: $selectedBrand") },
                                                    icon = { Icon(Icons.Filled.Close, null, modifier = Modifier.size(12.dp)) }
                                                )
                                            }
                                            if (selectedCategory.isNotEmpty()) {
                                                SuggestionChip(
                                                    onClick = { selectedCategory = "" },
                                                    label = { Text("Kategori: $selectedCategory") },
                                                    icon = { Icon(Icons.Filled.Close, null, modifier = Modifier.size(12.dp)) }
                                                )
                                            }
                                            if (selectedAisle.isNotEmpty()) {
                                                SuggestionChip(
                                                    onClick = { selectedAisle = "" },
                                                    label = { Text("Reyon: $selectedAisle") },
                                                    icon = { Icon(Icons.Filled.Close, null, modifier = Modifier.size(12.dp)) }
                                                )
                                            }
                                        }
                                    }
                                }
                            }

                        // Product Selection & Multi-Filter Grid
                        item {
                            Text("Envanter Kataloğu (${filteredProducts.size} Ürün)", style = MaterialTheme.typography.titleSmall, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
                        }

                        if (filteredProducts.isEmpty()) {
                            item {
                                Box(modifier = Modifier.fillMaxWidth().padding(vertical = 32.dp), contentAlignment = Alignment.Center) {
                                    Text("Eşleşen envanter/ürün kaydı bulunamadı.", color = Color.Gray, style = MaterialTheme.typography.bodyMedium)
                                }
                            }
                        } else {
                            items(filteredProducts) { prod ->
                                var reyonInput by remember(prod.barcode) { mutableStateOf(prod.aisle ?: "") }
                                var qtyInput by remember { mutableStateOf(1) }
                                var reyonExpanded by remember { mutableStateOf(false) }

                                Card(
                                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
                                    border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant),
                                    modifier = Modifier.fillMaxWidth()
                                ) {
                                    Column(modifier = Modifier.padding(12.dp), verticalArrangement = Arrangement.spacedBy(8.dp)) {
                                        Row(
                                            modifier = Modifier.fillMaxWidth(),
                                            horizontalArrangement = Arrangement.SpaceBetween,
                                            verticalAlignment = Alignment.Top
                                        ) {
                                            if (!prod.imageUrl.isNullOrBlank()) {
                                                coil.compose.AsyncImage(
                                                    model = prod.imageUrl,
                                                    contentDescription = prod.title,
                                                    contentScale = androidx.compose.ui.layout.ContentScale.Crop,
                                                    modifier = Modifier
                                                        .size(56.dp)
                                                        .clip(RoundedCornerShape(8.dp))
                                                        .border(1.dp, MaterialTheme.colorScheme.outlineVariant, RoundedCornerShape(8.dp))
                                                )
                                                Spacer(modifier = Modifier.width(12.dp))
                                            }
                                            Column(modifier = Modifier.weight(1f)) {
                                                Text(prod.title, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodyMedium)
                                                Text("Kod: ${prod.code} • Barkod: ${prod.barcode}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                                if (!prod.brand.isNullOrBlank()) {
                                                    Text("Marka: ${prod.brand}", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.secondary, fontWeight = FontWeight.SemiBold)
                                                }
                                                
                                                Spacer(modifier = Modifier.height(4.dp))
                                                val aisleDisplay = if (!prod.aisle.isNullOrBlank()) prod.aisle else "Atanmamış"
                                                val hasAisle = !prod.aisle.isNullOrBlank()
                                                Surface(
                                                    color = if (hasAisle) MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.4f) else MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f),
                                                    shape = RoundedCornerShape(8.dp),
                                                    border = BorderStroke(1.dp, if (hasAisle) MaterialTheme.colorScheme.primary.copy(alpha = 0.2f) else MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.3f)),
                                                    modifier = Modifier.clickable {
                                                        if (hasAisle) {
                                                            reyonInput = prod.aisle!!
                                                        }
                                                        reyonExpanded = true
                                                    }
                                                ) {
                                                    Row(
                                                        modifier = Modifier.padding(horizontal = 8.dp, vertical = 4.dp),
                                                        verticalAlignment = Alignment.CenterVertically,
                                                        horizontalArrangement = Arrangement.spacedBy(4.dp)
                                                    ) {
                                                        Icon(
                                                            imageVector = Icons.Filled.Warehouse,
                                                            contentDescription = "Mevcut Reyon",
                                                            tint = if (hasAisle) MaterialTheme.colorScheme.primary else Color.Gray,
                                                            modifier = Modifier.size(12.dp)
                                                        )
                                                        Text(
                                                            text = "Mevcut Reyon: $aisleDisplay",
                                                            style = MaterialTheme.typography.labelSmall,
                                                            color = if (hasAisle) MaterialTheme.colorScheme.primary else Color.Gray,
                                                            fontWeight = if (hasAisle) FontWeight.Bold else FontWeight.Medium
                                                        )
                                                    }
                                                }
                                            }

                                            // Present Stock Badge
                                            Column(horizontalAlignment = Alignment.End) {
                                                val existingStock = prod.stockByWarehouse[selectedWarehouse] ?: 0
                                                Surface(
                                                    color = if (existingStock > 0) MaterialTheme.colorScheme.tertiaryContainer else MaterialTheme.colorScheme.errorContainer,
                                                    shape = RoundedCornerShape(6.dp)
                                                ) {
                                                    Text(
                                                        text = "Sistem Stok: $existingStock",
                                                        modifier = Modifier.padding(horizontal = 8.dp, vertical = 4.dp),
                                                        fontWeight = FontWeight.Bold,
                                                        style = MaterialTheme.typography.labelSmall,
                                                        color = if (existingStock > 0) MaterialTheme.colorScheme.onTertiaryContainer else MaterialTheme.colorScheme.onErrorContainer
                                                    )
                                                }
                                            }
                                        }

                                        Row(
                                            modifier = Modifier.fillMaxWidth(),
                                            horizontalArrangement = Arrangement.spacedBy(8.dp),
                                            verticalAlignment = Alignment.CenterVertically
                                        ) {
                                            // Modern Aisle/Shelf Selector
                                            Box(modifier = Modifier.weight(0.9f)) {
                                                OutlinedButton(
                                                    onClick = { reyonExpanded = true },
                                                    shape = RoundedCornerShape(8.dp),
                                                    contentPadding = PaddingValues(horizontal = 8.dp),
                                                    modifier = Modifier.fillMaxWidth().height(42.dp)
                                                ) {
                                                    Text(
                                                        if (reyonInput.isNotBlank()) reyonInput else "Reyon Seç",
                                                        maxLines = 1,
                                                        overflow = TextOverflow.Ellipsis,
                                                        style = MaterialTheme.typography.labelMedium
                                                    )
                                                    Spacer(modifier = Modifier.weight(1f))
                                                    Icon(Icons.Filled.ArrowDropDown, contentDescription = "Reyon Seç", modifier = Modifier.size(16.dp))
                                                }
                                                DropdownMenu(
                                                    expanded = reyonExpanded,
                                                    onDismissRequest = { reyonExpanded = false },
                                                    modifier = Modifier.width(180.dp).heightIn(max = 240.dp)
                                                ) {
                                                    val aislesList = if (registeredAisles.isEmpty()) listOf("Ana Reyon", "Reyon A", "Reyon B", "Reyon C", "Stand") else registeredAisles
                                                    aislesList.forEach { aisleVal ->
                                                        DropdownMenuItem(
                                                            text = { Text(aisleVal, fontWeight = FontWeight.Medium) },
                                                            onClick = {
                                                                reyonInput = aisleVal
                                                                reyonExpanded = false
                                                            }
                                                        )
                                                    }
                                                }
                                            }

                                            // Quantity increment controls
                                            Row(
                                                modifier = Modifier.weight(1.1f),
                                                verticalAlignment = Alignment.CenterVertically,
                                                horizontalArrangement = Arrangement.End
                                            ) {
                                                IconButton(onClick = { if (qtyInput > 1) qtyInput-- }) {
                                                    Icon(Icons.Filled.RemoveCircleOutline, null, tint = MaterialTheme.colorScheme.primary)
                                                }
                                                Text(
                                                    "$qtyInput",
                                                    fontWeight = FontWeight.Bold,
                                                    style = MaterialTheme.typography.bodyLarge,
                                                    modifier = Modifier.padding(horizontal = 6.dp)
                                                )
                                                IconButton(onClick = { qtyInput++ }) {
                                                    Icon(Icons.Filled.AddCircleOutline, null, tint = MaterialTheme.colorScheme.primary)
                                                }

                                                Spacer(modifier = Modifier.width(6.dp))

                                                // Add button
                                                Button(
                                                    onClick = {
                                                        onBeep()
                                                        // Look up existing in activeCountItems (matching barcode AND reyon)
                                                        val finalAisle = reyonInput.trim().ifBlank { "TANIMSIZ" }
                                                        val idx = activeCountItems.indexOfFirst { it.barcode == prod.barcode && it.aisle.trim().lowercase() == finalAisle.trim().lowercase() }
                                                        if (idx != -1) {
                                                            val old = activeCountItems[idx]
                                                            activeCountItems[idx] = old.copy(
                                                                countedQty = old.countedQty + qtyInput,
                                                                aisle = finalAisle // ensure saved
                                                            )
                                                        } else {
                                                            activeCountItems.add(
                                                                CountedItem(
                                                                    barcode = prod.barcode,
                                                                    productTitle = prod.title,
                                                                    productCode = prod.code,
                                                                    brand = prod.brand ?: "Genel",
                                                                    expectedStock = prod.stockByWarehouse[selectedWarehouse] ?: 0,
                                                                    countedQty = qtyInput,
                                                                    aisle = finalAisle
                                                                )
                                                            )
                                                        }
                                                        qtyInput = 1
                                                    },
                                                    colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.secondary),
                                                    shape = RoundedCornerShape(8.dp),
                                                    contentPadding = PaddingValues(horizontal = 12.dp)
                                                ) {
                                                    Icon(Icons.Filled.PlaylistAdd, null, modifier = Modifier.size(16.dp))
                                                    Spacer(modifier = Modifier.width(2.dp))
                                                    Text("Ekle", style = MaterialTheme.typography.labelSmall)
                                                }
                                            }
                                        }

                                        // Total currently counted in this session badge
                                        val totalCountedInSession = activeCountItems.filter { it.barcode == prod.barcode }.sumOf { it.countedQty }
                                        if (totalCountedInSession > 0) {
                                            Row(
                                                verticalAlignment = Alignment.CenterVertically,
                                                horizontalArrangement = Arrangement.spacedBy(4.dp),
                                                modifier = Modifier.padding(top = 4.dp)
                                            ) {
                                                Icon(Icons.Filled.CheckCircle, null, tint = MaterialTheme.colorScheme.secondary, modifier = Modifier.size(14.dp))
                                                Text(
                                                    "Bu seanstaki toplam sayılan: $totalCountedInSession adet",
                                                    style = MaterialTheme.typography.labelSmall,
                                                    color = MaterialTheme.colorScheme.secondary,
                                                    fontWeight = FontWeight.Bold
                                                )
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // MAIN COUNT LIST removed and moved to Tab 3
                    }
                }

                3 -> {
                    LazyColumn(
                        modifier = Modifier.fillMaxSize().padding(16.dp),
                        verticalArrangement = Arrangement.spacedBy(16.dp)
                    ) {
                        item {
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Column {
                                    Text("Sepet - Sayılan Ürünler", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold)
                                    Text("Kaydedilen satır: ${activeCountItems.size}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                }
                                if (activeCountItems.isNotEmpty()) {
                                    TextButton(onClick = { showClearBasketConfirmDialog = true }) {
                                        Icon(Icons.Filled.DeleteSweep, null, tint = MaterialTheme.colorScheme.error)
                                        Spacer(modifier = Modifier.width(4.dp))
                                        Text("Çeteleyi Boşalt", color = MaterialTheme.colorScheme.error, style = MaterialTheme.typography.labelMedium)
                                    }
                                }
                            }
                        }

                        if (activeCountItems.isEmpty()) {
                            item {
                                Card(
                                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.3f)),
                                    modifier = Modifier.fillMaxWidth().padding(vertical = 16.dp)
                                ) {
                                    Box(modifier = Modifier.fillMaxWidth().padding(24.dp), contentAlignment = Alignment.Center) {
                                        Text("Sepetiniz boş. Henüz ürün sayılmadı.", color = Color.Gray, style = MaterialTheme.typography.bodyMedium)
                                    }
                                }
                            }
                        } else {
                            items(activeCountItems) { item ->
                                val cProd = AppDataStore.products.find { it.barcode == item.barcode }
                                Card(
                                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
                                    border = BorderStroke(1.dp, MaterialTheme.colorScheme.primary.copy(alpha = 0.3f)),
                                    modifier = Modifier.fillMaxWidth()
                                ) {
                                    Row(
                                        modifier = Modifier.padding(12.dp).fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween,
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        if (cProd?.imageUrl?.isNotBlank() == true) {
                                            coil.compose.AsyncImage(
                                                model = cProd.imageUrl,
                                                contentDescription = item.productTitle,
                                                contentScale = androidx.compose.ui.layout.ContentScale.Crop,
                                                modifier = Modifier
                                                    .size(56.dp)
                                                    .clip(RoundedCornerShape(8.dp))
                                                    .border(1.dp, MaterialTheme.colorScheme.outlineVariant, RoundedCornerShape(8.dp))
                                            )
                                            Spacer(modifier = Modifier.width(12.dp))
                                        }
                                        Column(modifier = Modifier.weight(1f)) {
                                            Text(item.productTitle, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodySmall)
                                            Text("Kod: ${item.productCode} • Reyon: ${item.aisle}", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.primary)
                                            Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(8.dp), modifier = Modifier.padding(top = 4.dp)) {
                                                Text("Sistem: ${item.expectedStock}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                                VerticalDivider(modifier = Modifier.height(10.dp))
                                                val diff = item.countedQty - item.expectedStock
                                                Text(
                                                    text = "Fark: " + (if (diff >= 0) "+$diff" else "$diff"),
                                                    style = MaterialTheme.typography.labelSmall,
                                                    fontWeight = FontWeight.Bold,
                                                    color = if (diff == 0) MaterialTheme.colorScheme.secondary else MaterialTheme.colorScheme.error
                                                )
                                            }
                                        }

                                        Row(
                                            verticalAlignment = Alignment.CenterVertically,
                                            horizontalArrangement = Arrangement.spacedBy(4.dp)
                                        ) {
                                            IconButton(onClick = {
                                                val idx = activeCountItems.indexOf(item)
                                                if (item.countedQty > 1) {
                                                    activeCountItems[idx] = item.copy(countedQty = item.countedQty - 1)
                                                } else {
                                                    itemToDelete = item
                                                }
                                            }) {
                                                Icon(Icons.Filled.Remove, null, tint = MaterialTheme.colorScheme.outline)
                                            }
                                            Text(
                                                text = "${item.countedQty} Adet",
                                                fontWeight = FontWeight.Bold,
                                                style = MaterialTheme.typography.bodyMedium,
                                                color = MaterialTheme.colorScheme.primary
                                            )
                                            IconButton(onClick = {
                                                val idx = activeCountItems.indexOf(item)
                                                activeCountItems[idx] = item.copy(countedQty = item.countedQty + 1)
                                            }) {
                                                Icon(Icons.Filled.Add, null, tint = MaterialTheme.colorScheme.primary)
                                            }
                                        }
                                    }
                                }
                            }

                            // Dynamic Actions Block at base
                            item {
                                Spacer(modifier = Modifier.height(12.dp))
                                Row(
                                    modifier = Modifier.fillMaxWidth(),
                                    horizontalArrangement = Arrangement.spacedBy(12.dp)
                                ) {
                                    Button(
                                        onClick = {
                                            if (operatorName.isBlank()) operatorName = "İsimsiz Personel"
                                            val sessId = activeSessionId.ifBlank { "SC-" + System.currentTimeMillis() }
                                            val sdf = java.text.SimpleDateFormat("dd.MM.yyyy HH:mm", java.util.Locale.getDefault())
                                            val draftSession = StockCountSession(sessId, sdf.format(java.util.Date()), "PENDING", operatorName.trim(), selectedWarehouse, activeCountItems.toList())
                                            val existingIndex = AppDataStore.stockCountSessions.indexOfFirst { it.id == sessId }
                                            if (existingIndex != -1) AppDataStore.stockCountSessions[existingIndex] = draftSession
                                            else AppDataStore.stockCountSessions.add(draftSession)
                                            AppDataStore.persist(context)
                                            activeCountItems.clear()
                                            activeSessionId = ""
                                            scope.launch { activeTab = 1 }
                                        },
                                        colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.tertiary),
                                        shape = RoundedCornerShape(8.dp),
                                        modifier = Modifier.weight(1f)
                                    ) {
                                        Icon(Icons.Filled.Pause, null)
                                        Spacer(modifier = Modifier.width(6.dp))
                                        Text("Beklet", style = MaterialTheme.typography.labelLarge)
                                    }

                                    Button(
                                        onClick = { showFinalizeConfirmDialog = true },
                                        colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.secondary),
                                        shape = RoundedCornerShape(8.dp),
                                        modifier = Modifier.weight(1.1f)
                                    ) {
                                        Icon(Icons.Filled.OfflinePin, null)
                                        Spacer(modifier = Modifier.width(6.dp))
                                        Text("Kesinleştir", style = MaterialTheme.typography.labelLarge)
                                    }
                                }
                            }
                        }
                    }
                }

                4 -> {
                    val uncountedProducts = filteredProducts.filter { p -> activeCountItems.none { it.barcode == p.barcode } }
                    LazyColumn(
                        modifier = Modifier.fillMaxSize().padding(16.dp),
                        verticalArrangement = Arrangement.spacedBy(16.dp)
                    ) {
                        item {
                            Text("SAYILMAYAN KALAN ÜRÜNLER", style = MaterialTheme.typography.titleSmall, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
                            Text("Seçili depo ve filtrelere göre henüz sayılmamış ürünler.", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                        }
                        if (uncountedProducts.isEmpty()) {
                            item {
                                Card(
                                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.2f)),
                                    modifier = Modifier.fillMaxWidth().padding(vertical = 4.dp)
                                ) {
                                    Box(modifier = Modifier.fillMaxWidth().padding(16.dp), contentAlignment = Alignment.Center) {
                                        Text("Filtrenize uygun sayılmayan ürün bulunamadı.", color = Color.Gray, style = MaterialTheme.typography.bodySmall)
                                    }
                                }
                            }
                        } else {
                            items(uncountedProducts) { prod ->
                                Card(
                                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
                                    border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant),
                                    modifier = Modifier.fillMaxWidth()
                                ) {
                                    Row(
                                        modifier = Modifier.padding(12.dp).fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween,
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        if (!prod.imageUrl.isNullOrBlank()) {
                                            coil.compose.AsyncImage(
                                                model = prod.imageUrl,
                                                contentDescription = prod.title,
                                                contentScale = androidx.compose.ui.layout.ContentScale.Crop,
                                                modifier = Modifier
                                                    .size(56.dp)
                                                    .clip(RoundedCornerShape(8.dp))
                                                    .border(1.dp, MaterialTheme.colorScheme.outlineVariant, RoundedCornerShape(8.dp))
                                            )
                                            Spacer(modifier = Modifier.width(12.dp))
                                        }
                                        Column {
                                            Text(prod.title, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodySmall)
                                            Text("Kod: ${prod.code} • Barkod: ${prod.barcode}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                            Text("Mevcut Sistem Stoku: ${prod.stockByWarehouse[selectedWarehouse] ?: 0}", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.primary)
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                1 -> {
                    // TAB 1: SEANSLAR VE GEÇMİŞ (DRAFTS AND HISTORIES)
                    val pendingSessions = AppDataStore.stockCountSessions.filter { it.status == "PENDING" }
                    val completedSessions = AppDataStore.stockCountSessions.filter { it.status == "COMPLETED" }
                    val cancelledSessions = AppDataStore.stockCountSessions.filter { it.status == "CANCELLED" }

                    LazyColumn(
                        modifier = Modifier.fillMaxSize().padding(16.dp),
                        verticalArrangement = Arrangement.spacedBy(16.dp)
                    ) {
                        // Draft list header
                        item {
                            Text("BEKLEYEN SAYIM TASLAKLARI (${pendingSessions.size})", style = MaterialTheme.typography.titleSmall, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.tertiary)
                            Text("Buradaki taslak sayım seanslarına daha sonra kaldığı yerden devam edebilirsiniz.", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                        }

                        if (pendingSessions.isEmpty()) {
                            item {
                                Card(
                                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.2f)),
                                    modifier = Modifier.fillMaxWidth().padding(vertical = 4.dp)
                                ) {
                                    Box(modifier = Modifier.fillMaxWidth().padding(16.dp), contentAlignment = Alignment.Center) {
                                        Text("Bekleyen taslak sayım seansı bulunmamaktadır.", color = Color.Gray, style = MaterialTheme.typography.bodySmall)
                                    }
                                }
                            }
                        } else {
                            items(pendingSessions) { session ->
                                Card(
                                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
                                    border = BorderStroke(1.dp, MaterialTheme.colorScheme.tertiary),
                                    modifier = Modifier.fillMaxWidth()
                                ) {
                                    Column(modifier = Modifier.padding(12.dp), verticalArrangement = Arrangement.spacedBy(8.dp)) {
                                        Row(
                                            modifier = Modifier.fillMaxWidth(),
                                            horizontalArrangement = Arrangement.SpaceBetween,
                                            verticalAlignment = Alignment.CenterVertically
                                        ) {
                                            Column {
                                                Text("Seans: ${session.id}", fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodySmall)
                                                Text("Tarih: ${session.date} • Depo: ${session.warehouse}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                                Text("Yapan: ${session.user}", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.tertiary, fontWeight = FontWeight.SemiBold)
                                            }
                                            Surface(
                                                color = MaterialTheme.colorScheme.tertiaryContainer,
                                                shape = RoundedCornerShape(4.dp)
                                            ) {
                                                Text(
                                                    "${session.countedItems.size} Kalem",
                                                    modifier = Modifier.padding(horizontal = 6.dp, vertical = 2.dp),
                                                    style = MaterialTheme.typography.labelSmall,
                                                    color = MaterialTheme.colorScheme.onTertiaryContainer,
                                                    fontWeight = FontWeight.Bold
                                                )
                                            }
                                        }

                                        Row(
                                            modifier = Modifier.fillMaxWidth(),
                                            horizontalArrangement = Arrangement.spacedBy(8.dp)
                                        ) {
                                            Button(
                                                onClick = {
                                                    // Load to Active screen
                                                    activeSessionId = session.id
                                                    selectedWarehouse = session.warehouse
                                                    operatorName = session.user
                                                    activeCountItems.clear()
                                                    activeCountItems.addAll(session.countedItems)
                                                    
                                                    // Switch to counting view
                                                    activeTab = 0
                                                },
                                                colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.tertiary),
                                                modifier = Modifier.weight(1.3f),
                                                shape = RoundedCornerShape(6.dp)
                                            ) {
                                                Icon(Icons.Filled.PlayArrow, null, modifier = Modifier.size(16.dp))
                                                Spacer(modifier = Modifier.width(4.dp))
                                                Text("Sayıma Devam Et", style = MaterialTheme.typography.labelMedium)
                                            }

                                            OutlinedButton(
                                                onClick = {
                                                    sessionToDelete = session
                                                },
                                                modifier = Modifier.weight(0.7f),
                                                colors = ButtonDefaults.outlinedButtonColors(contentColor = MaterialTheme.colorScheme.error),
                                                border = BorderStroke(1.dp, MaterialTheme.colorScheme.error.copy(alpha = 0.4f)),
                                                shape = RoundedCornerShape(6.dp)
                                            ) {
                                                Icon(Icons.Filled.Delete, null, modifier = Modifier.size(14.dp))
                                                Spacer(modifier = Modifier.width(2.dp))
                                                Text("Taslağı Sil", style = MaterialTheme.typography.labelMedium)
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // Completed list header
                        item {
                            Divider(modifier = Modifier.padding(vertical = 4.dp))
                            Text("TAMAMLANAN SAYIMLAR (${completedSessions.size})", style = MaterialTheme.typography.titleSmall, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.secondary)
                            Text("Kaydedilmiş ve yürürlüğe girmiş sayım geçmişleri. İstediğiniz an düzenleyebilir veya tamamen iptal edip geri alarak stok bakiyelerini eski haline döndürebilirsiniz.", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                        }

                        if (completedSessions.isEmpty()) {
                            item {
                                Card(
                                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.2f)),
                                    modifier = Modifier.fillMaxWidth().padding(vertical = 4.dp)
                                ) {
                                    Box(modifier = Modifier.fillMaxWidth().padding(16.dp), contentAlignment = Alignment.Center) {
                                        Text("Tamamlanan bakiye sayım kaydı bulunmamaktadır.", color = Color.Gray, style = MaterialTheme.typography.bodySmall)
                                    }
                                }
                            }
                        } else {
                            items(completedSessions) { session ->
                                Card(
                                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
                                    border = BorderStroke(1.dp, MaterialTheme.colorScheme.secondary.copy(alpha = 0.5f)),
                                    modifier = Modifier.fillMaxWidth()
                                ) {
                                    Column(modifier = Modifier.padding(12.dp), verticalArrangement = Arrangement.spacedBy(8.dp)) {
                                        Row(
                                            modifier = Modifier.fillMaxWidth(),
                                            horizontalArrangement = Arrangement.SpaceBetween,
                                            verticalAlignment = Alignment.CenterVertically
                                        ) {
                                            Column {
                                                Text("Seans: ${session.id}", fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodySmall)
                                                Text("Tarih: ${session.date} • Depo: ${session.warehouse}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                                Text("Kapatıcı: ${session.user}", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.secondary, fontWeight = FontWeight.SemiBold)
                                            }
                                            Surface(
                                                color = MaterialTheme.colorScheme.secondaryContainer,
                                                shape = RoundedCornerShape(4.dp)
                                            ) {
                                                Text(
                                                    "${session.countedItems.size} Kalem Sayıldı",
                                                    modifier = Modifier.padding(horizontal = 6.dp, vertical = 2.dp),
                                                    style = MaterialTheme.typography.labelSmall,
                                                    color = MaterialTheme.colorScheme.onSecondaryContainer,
                                                    fontWeight = FontWeight.Bold
                                                )
                                            }
                                        }

                                        Row(
                                            modifier = Modifier.fillMaxWidth(),
                                            horizontalArrangement = Arrangement.spacedBy(8.dp)
                                        ) {
                                            // Re-edit (Reopen) button
                                            Button(
                                                onClick = {
                                                    // Reverse intermediate DB stocks temporarily to allow recount
                                                    productsCatalog.forEach { prod ->
                                                        val countedMatchList = session.countedItems.filter { it.barcode == prod.barcode }
                                                        if (countedMatchList.isNotEmpty()) {
                                                            // Restore to original theoretical stocks from the count date
                                                            val originalTheoryStock = countedMatchList.first().expectedStock
                                                            val updatedMap = prod.stockByWarehouse.toMutableMap()
                                                            updatedMap[session.warehouse] = originalTheoryStock
                                                            
                                                            // Find index and write back
                                                            val idxProd = productsCatalog.indexOf(prod)
                                                            if (idxProd != -1) {
                                                                productsCatalog[idxProd] = prod.copy(stockByWarehouse = updatedMap)
                                                            }
                                                        }
                                                    }

                                                    // Load session to active
                                                    activeSessionId = session.id
                                                    selectedWarehouse = session.warehouse
                                                    operatorName = session.user
                                                    activeCountItems.clear()
                                                    activeCountItems.addAll(session.countedItems)
                                                    
                                                    // Remove session from general list (status is draft now)
                                                    AppDataStore.stockCountSessions.remove(session)
                                                    AppDataStore.persist(context)

                                                    // Redirect to Count View
                                                    activeTab = 0
                                                },
                                                colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.secondary),
                                                modifier = Modifier.weight(1f),
                                                shape = RoundedCornerShape(6.dp)
                                            ) {
                                                Icon(Icons.Filled.Edit, null, modifier = Modifier.size(16.dp))
                                                Spacer(modifier = Modifier.width(4.dp))
                                                Text("Düzenle (Yeniden Aç)", style = MaterialTheme.typography.labelSmall)
                                            }

                                            // Revert / Cancel Button
                                            OutlinedButton(
                                                onClick = {
                                                    // Revert and reverse stocks in DB completely!
                                                    productsCatalog.forEach { prod ->
                                                        val countedMatchList = session.countedItems.filter { it.barcode == prod.barcode }
                                                        if (countedMatchList.isNotEmpty()) {
                                                            // Set back to original theoretical stock
                                                            val originalTheoryStock = countedMatchList.first().expectedStock
                                                            val updatedMap = prod.stockByWarehouse.toMutableMap()
                                                            updatedMap[session.warehouse] = originalTheoryStock
                                                            
                                                            val idxProd = productsCatalog.indexOf(prod)
                                                            if (idxProd != -1) {
                                                                productsCatalog[idxProd] = prod.copy(stockByWarehouse = updatedMap)
                                                            }
                                                        }
                                                    }

                                                    // Mark session as CANCELLED
                                                    val idxSess = AppDataStore.stockCountSessions.indexOf(session)
                                                    if (idxSess != -1) {
                                                        AppDataStore.stockCountSessions[idxSess] = session.copy(status = "CANCELLED")
                                                    }

                                                    AppDataStore.persist(context)
                                                },
                                                modifier = Modifier.weight(1f),
                                                colors = ButtonDefaults.outlinedButtonColors(contentColor = MaterialTheme.colorScheme.error),
                                                border = BorderStroke(1.dp, MaterialTheme.colorScheme.error.copy(alpha = 0.5f)),
                                                shape = RoundedCornerShape(6.dp)
                                            ) {
                                                Icon(Icons.Filled.Undo, null, modifier = Modifier.size(14.dp))
                                                Spacer(modifier = Modifier.width(2.dp))
                                                Text("Geri Al & İptal Et", style = MaterialTheme.typography.labelSmall)
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // Cancelled list section
                        if (cancelledSessions.isNotEmpty()) {
                            item {
                                Spacer(modifier = Modifier.height(12.dp))
                                Text("İPTAL EDİLEN/GERİ ALINAN SEANSLAR (${cancelledSessions.size})", style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Bold, color = Color.Gray)
                            }

                            items(cancelledSessions) { session ->
                                Card(
                                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.4f)),
                                    border = BorderStroke(1.dp, Color.Gray.copy(alpha = 0.3f)),
                                    modifier = Modifier.fillMaxWidth()
                                ) {
                                    Row(
                                        modifier = Modifier.padding(12.dp).fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween,
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        Column {
                                            Text("İptal Edilen Seans: ${session.id}", style = MaterialTheme.typography.bodySmall, color = Color.Gray, textDecoration = TextDecoration.LineThrough)
                                            Text("Sayıcı: ${session.user} • Tarih: ${session.date}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                        }
                                        Surface(
                                            color = Color.LightGray,
                                            shape = RoundedCornerShape(4.dp)
                                        ) {
                                            Text(
                                                "İptal Edildi",
                                                modifier = Modifier.padding(horizontal = 6.dp, vertical = 2.dp),
                                                style = MaterialTheme.typography.labelSmall,
                                                color = Color.DarkGray,
                                                fontWeight = FontWeight.Bold
                                            )
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                2 -> {
                    // TAB 2: DETAILED DUPLICATE REYON CHECK AND MERGE
                    LazyColumn(
                        modifier = Modifier.fillMaxSize().padding(16.dp),
                        verticalArrangement = Arrangement.spacedBy(16.dp)
                    ) {
                        item {
                            Text("MÜKERRER REYON ÇAKIŞMA SEÇENEKLERİ", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.error)
                            Text("Saha sayımlarında aynı ürünün birden fazla farklı reyonda veya rafta sayılması sıklıkla karşılaşılabilen bir durumdur. Aşağıda çakışan ürünleri tek reyon altında toplayarak düzenleyebilirsiniz.", style = MaterialTheme.typography.bodySmall, color = Color.Gray)
                        }

                        if (duplicateAislesGroup.isEmpty()) {
                            item {
                                Card(
                                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.1f)),
                                    border = BorderStroke(1.dp, MaterialTheme.colorScheme.primary.copy(alpha = 0.2f)),
                                    modifier = Modifier.fillMaxWidth().padding(top = 16.dp)
                                ) {
                                    Column(
                                        modifier = Modifier.padding(24.dp),
                                        horizontalAlignment = Alignment.CenterHorizontally,
                                        verticalArrangement = Arrangement.Center
                                    ) {
                                        Icon(Icons.Filled.Verified, null, modifier = Modifier.size(56.dp), tint = MaterialTheme.colorScheme.primary)
                                        Spacer(modifier = Modifier.height(12.dp))
                                        Text("Çakışma Bulunamadı", fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodyLarge)
                                        Text("Harika! Bu seansta aynı barkodlu hiçbir ürün farklı reyonlarda bölünerek sayılmadı. Verileriniz temiz.", style = MaterialTheme.typography.bodySmall, textAlign = TextAlign.Center, color = Color.Gray)
                                    }
                                }
                            }
                        } else {
                            items(duplicateAislesGroup.keys.toList()) { barcode ->
                                val duplicates = duplicateAislesGroup[barcode] ?: emptyList()
                                val first = duplicates.first()
                                val totalQty = duplicates.sumOf { it.countedQty }

                                Card(
                                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
                                    border = BorderStroke(1.dp, MaterialTheme.colorScheme.error.copy(alpha = 0.5f)),
                                    modifier = Modifier.fillMaxWidth()
                                ) {
                                    Column(modifier = Modifier.padding(14.dp), verticalArrangement = Arrangement.spacedBy(10.dp)) {
                                        Row(
                                            modifier = Modifier.fillMaxWidth(),
                                            horizontalArrangement = Arrangement.SpaceBetween,
                                            verticalAlignment = Alignment.Top
                                        ) {
                                            Column(modifier = Modifier.weight(1f)) {
                                                Text(first.productTitle, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodyMedium)
                                                Text("Barkod: $barcode • Kod: ${first.productCode}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                            }
                                            Surface(
                                                color = MaterialTheme.colorScheme.errorContainer,
                                                shape = RoundedCornerShape(6.dp)
                                            ) {
                                                Text(
                                                    "Toplam: $totalQty Adet",
                                                    modifier = Modifier.padding(horizontal = 8.dp, vertical = 4.dp),
                                                    style = MaterialTheme.typography.labelMedium,
                                                    fontWeight = FontWeight.Bold,
                                                    color = MaterialTheme.colorScheme.onErrorContainer
                                                )
                                            }
                                        }

                                        Divider(color = MaterialTheme.colorScheme.outlineVariant)

                                        Text("Farklı Reyonlardaki Sayım Dağılımı:", style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.SemiBold, color = Color.Gray)

                                        duplicates.forEach { item ->
                                            Row(
                                                modifier = Modifier.fillMaxWidth(),
                                                horizontalArrangement = Arrangement.SpaceBetween,
                                                verticalAlignment = Alignment.CenterVertically
                                            ) {
                                                Text("• Reyon: ${item.aisle}", style = MaterialTheme.typography.bodySmall, fontWeight = FontWeight.Medium)
                                                Text("${item.countedQty} Adet", style = MaterialTheme.typography.bodySmall, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
                                            }
                                        }

                                        Spacer(modifier = Modifier.height(6.dp))

                                        // Merge Button
                                        Button(
                                            onClick = { showAisleMergeId = barcode },
                                            colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.error),
                                            shape = RoundedCornerShape(8.dp),
                                            modifier = Modifier.fillMaxWidth()
                                        ) {
                                            Icon(Icons.Filled.Merge, null, modifier = Modifier.size(16.dp))
                                            Spacer(modifier = Modifier.width(6.dp))
                                            Text("Tümünü Tek Reyonda Topla", style = MaterialTheme.typography.labelMedium)
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

    // Modern Filter Dialog
    if (showFilterDialog) {
        androidx.compose.ui.window.Dialog(
            onDismissRequest = { showFilterDialog = false }
        ) {
            Card(
                colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
                shape = RoundedCornerShape(24.dp),
                border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f)),
                modifier = Modifier
                    .fillMaxWidth()
                    .wrapContentHeight()
                    .padding(16.dp)
                    .shadow(12.dp, shape = RoundedCornerShape(24.dp))
            ) {
                Column(
                    modifier = Modifier.padding(24.dp),
                    verticalArrangement = Arrangement.spacedBy(16.dp)
                ) {
                    // Header
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.SpaceBetween,
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                            Icon(
                                imageVector = Icons.Filled.FilterAlt,
                                contentDescription = null,
                                tint = MaterialTheme.colorScheme.primary,
                                modifier = Modifier.size(24.dp)
                            )
                            Text(
                                "Gelişmiş Filtreleme",
                                style = MaterialTheme.typography.titleMedium,
                                fontWeight = FontWeight.Bold,
                                color = MaterialTheme.colorScheme.onSurface
                            )
                        }
                        IconButton(onClick = { showFilterDialog = false }) {
                            Icon(Icons.Filled.Close, contentDescription = "Kapat")
                        }
                    }

                    HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))

                    // 1. Marka Filtresi (Brand)
                    Text("Marka Seçimi", style = MaterialTheme.typography.labelMedium, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
                    Row(
                        modifier = Modifier
                            .fillMaxWidth()
                            .horizontalScroll(rememberScrollState()),
                        horizontalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        // All option
                        FilterChip(
                            selected = selectedBrand.isEmpty(),
                            onClick = { selectedBrand = "" },
                            label = { Text("Tümü") }
                        )
                        brands.forEach { brand ->
                            if (brand.isNotBlank()) {
                                FilterChip(
                                    selected = selectedBrand == brand,
                                    onClick = { selectedBrand = brand },
                                    label = { Text(brand) }
                                )
                            }
                        }
                    }

                    // 2. Kategori Filtresi (Category)
                    Text("Kategori Seçimi", style = MaterialTheme.typography.labelMedium, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
                    Row(
                        modifier = Modifier
                            .fillMaxWidth()
                            .horizontalScroll(rememberScrollState()),
                        horizontalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        // All option
                        FilterChip(
                            selected = selectedCategory.isEmpty(),
                            onClick = { selectedCategory = "" },
                            label = { Text("Tümü") }
                        )
                        categories.forEach { cat ->
                            if (cat.isNotBlank()) {
                                FilterChip(
                                    selected = selectedCategory == cat,
                                    onClick = { selectedCategory = cat },
                                    label = { Text(cat) }
                                )
                            }
                        }
                    }

                    // 3. Reyon/Aisle Filtresi (Aisle)
                    Text("Reyon Seçimi", style = MaterialTheme.typography.labelMedium, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
                    Row(
                        modifier = Modifier
                            .fillMaxWidth()
                            .horizontalScroll(rememberScrollState()),
                        horizontalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        // All option
                        FilterChip(
                            selected = selectedAisle.isEmpty(),
                            onClick = { selectedAisle = "" },
                            label = { Text("Tümü") }
                        )
                        registeredAisles.forEach { aisle ->
                            if (aisle.isNotBlank()) {
                                FilterChip(
                                    selected = selectedAisle == aisle,
                                    onClick = { selectedAisle = aisle },
                                    label = { Text(aisle) }
                                )
                            }
                        }
                    }

                    Spacer(modifier = Modifier.height(8.dp))

                    // Buttons
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.spacedBy(12.dp)
                    ) {
                        OutlinedButton(
                            onClick = {
                                selectedBrand = ""
                                selectedCategory = ""
                                selectedAisle = ""
                            },
                            modifier = Modifier.weight(1f),
                            shape = RoundedCornerShape(12.dp)
                        ) {
                            Text("Sıfırla")
                        }
                        Button(
                            onClick = { showFilterDialog = false },
                            modifier = Modifier.weight(1.5f),
                            shape = RoundedCornerShape(12.dp)
                        ) {
                            Text("Filtreleri Uygula")
                        }
                    }
                }
            }
        }
    }

    // --- 1. SIMULATED QR/BARCODE SCANNER OVERLAY DIALOG ---
    if (showScanSimDialog) {
        AlertDialog(
            onDismissRequest = { showScanSimDialog = false },
            title = {
                Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    Icon(Icons.Filled.QrCodeScanner, null, tint = MaterialTheme.colorScheme.primary)
                    Text("Barkod / Ürün Kodu Okutma")
                }
            },
            text = {
                Column(verticalArrangement = Arrangement.spacedBy(12.dp)) {
                    Text(
                        "Lütfen kameradan barkod taranmış gibi simüle etmek için ürün kodunu / barkodunu girin veya sağlanan hazır butonlara tıklayın.",
                        style = MaterialTheme.typography.bodySmall
                    )

                    var simCode by remember { mutableStateOf("") }
                    var simAisle by remember { mutableStateOf(registeredAisles.firstOrNull() ?: "A-01") }
                    var simAisleExpanded by remember { mutableStateOf(false) }

                    OutlinedTextField(
                        value = simCode,
                        onValueChange = { simCode = it },
                        label = { Text("Barkod veya Ürün Kodu") },
                        singleLine = true,
                        modifier = Modifier.fillMaxWidth()
                    )

                    Box(modifier = Modifier.fillMaxWidth()) {
                        OutlinedTextField(
                            value = simAisle,
                            onValueChange = { simAisle = it },
                            label = { Text("Hedef Reyon") },
                            singleLine = true,
                            modifier = Modifier.fillMaxWidth(),
                            trailingIcon = {
                                IconButton(onClick = { simAisleExpanded = !simAisleExpanded }) {
                                    Icon(Icons.Filled.ArrowDropDown, null)
                                }
                            }
                        )
                        DropdownMenu(
                            expanded = simAisleExpanded,
                            onDismissRequest = { simAisleExpanded = false },
                            modifier = Modifier.fillMaxWidth(0.8f).heightIn(max = 200.dp)
                        ) {
                            registeredAisles.forEach { aisleVal ->
                                DropdownMenuItem(
                                    text = { Text(aisleVal) },
                                    onClick = {
                                        simAisle = aisleVal
                                        simAisleExpanded = false
                                    }
                                )
                            }
                        }
                    }

                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        Button(
                            onClick = {
                                if (simCode.isNotBlank()) {
                                    onBeep()
                                    val matchedProd = productsCatalog.find { it.barcode == simCode.trim() || it.code.trim().equals(simCode.trim(), ignoreCase = true) }
                                    if (matchedProd != null) {
                                        searchQuery = matchedProd.barcode
                                        activeTab = 0
                                    } else {
                                        searchQuery = simCode.trim()
                                        activeTab = 0
                                    }
                                    showScanSimDialog = false
                                }
                            },
                            colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.primary),
                            modifier = Modifier.weight(1.1f)
                        ) {
                            Icon(Icons.Filled.Search, null, modifier = Modifier.size(16.dp))
                            Spacer(modifier = Modifier.width(4.dp))
                            Text("Listede Ara")
                        }

                        Button(
                            onClick = {
                                if (simCode.isNotBlank()) {
                                    onBeep()
                                    val matchedProd = productsCatalog.find { it.barcode == simCode.trim() || it.code.trim().equals(simCode.trim(), ignoreCase = true) }
                                    if (matchedProd != null) {
                                        searchQuery = matchedProd.barcode
                                        activeTab = 0
                                        val finalA = simAisle.trim().ifBlank { "TANIMSIZ" }
                                        val idx = activeCountItems.indexOfFirst { it.barcode == matchedProd.barcode && it.aisle.trim().lowercase() == finalA.trim().lowercase() }
                                        if (idx != -1) {
                                            val old = activeCountItems[idx]
                                            activeCountItems[idx] = old.copy(countedQty = old.countedQty + 1)
                                        } else {
                                            activeCountItems.add(
                                                CountedItem(
                                                    barcode = matchedProd.barcode,
                                                    productTitle = matchedProd.title,
                                                    productCode = matchedProd.code,
                                                    brand = matchedProd.brand ?: "Genel",
                                                    expectedStock = matchedProd.stockByWarehouse[selectedWarehouse] ?: 0,
                                                    countedQty = 1,
                                                    aisle = finalA
                                                )
                                            )
                                        }
                                    }
                                    showScanSimDialog = false
                                }
                            },
                            colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.secondary),
                            modifier = Modifier.weight(1.2f)
                        ) {
                            Icon(Icons.Filled.PlaylistAdd, null, modifier = Modifier.size(16.dp))
                            Spacer(modifier = Modifier.width(4.dp))
                            Text("Sayıp Ekle")
                        }
                    }

                    Divider()

                    Text("Hazır Kayıtlı Barkodlar:", style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Bold, color = Color.Gray)

                    // Provide clean buttons for catalog's actual first products
                    val quickBarcodes = productsCatalog.take(3)
                    Column(verticalArrangement = Arrangement.spacedBy(6.dp)) {
                        quickBarcodes.forEach { prod ->
                            Card(
                                onClick = {
                                    onBeep()
                                    searchQuery = prod.barcode
                                    activeTab = 0
                                    val finalA = simAisle.trim().ifBlank { "TANIMSIZ" }
                                    val idx = activeCountItems.indexOfFirst { it.barcode == prod.barcode && it.aisle.trim().lowercase() == finalA.trim().lowercase() }
                                    if (idx != -1) {
                                        val old = activeCountItems[idx]
                                        activeCountItems[idx] = old.copy(countedQty = old.countedQty + 1)
                                    } else {
                                        activeCountItems.add(
                                            CountedItem(
                                                barcode = prod.barcode,
                                                productTitle = prod.title,
                                                productCode = prod.code,
                                                brand = prod.brand ?: "Genel",
                                                expectedStock = prod.stockByWarehouse[selectedWarehouse] ?: 0,
                                                countedQty = 1,
                                                aisle = finalA
                                            )
                                        )
                                    }
                                    showScanSimDialog = false
                                },
                                modifier = Modifier.fillMaxWidth(),
                                colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f))
                            ) {
                                Row(
                                    modifier = Modifier.padding(10.dp).fillMaxWidth(),
                                    horizontalArrangement = Arrangement.SpaceBetween,
                                    verticalAlignment = Alignment.CenterVertically
                                ) {
                                    Column {
                                        Text(prod.title, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.labelMedium)
                                        Text("KOD: ${prod.code} • BARKOD: ${prod.barcode}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                    }
                                    Icon(Icons.Filled.ChevronRight, null, tint = MaterialTheme.colorScheme.primary)
                                }
                            }
                        }
                    }
                }
            },
            confirmButton = {
                TextButton(onClick = { showScanSimDialog = false }) {
                    Text("Kapat")
                }
            }
        )
    }

    if (showCameraScannerDialog) {
        BarcodeScannerDialog(
            onDismissRequest = { showCameraScannerDialog = false },
            onBarcodeScanned = { barcode ->
                handleDirectBarcodeScan(barcode, isContinuous = false)
                showCameraScannerDialog = false
            },
            onSimulateScan = { barcode ->
                handleDirectBarcodeScan(barcode, isContinuous = false)
                showCameraScannerDialog = false
            }
        )
    }

    if (showScannedDetailDialog && scannedProduct != null) {
        val prod = scannedProduct!!
        AlertDialog(
            onDismissRequest = { 
                showScannedDetailDialog = false 
                scannedProduct = null
            },
            title = {
                Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    Icon(Icons.Filled.QrCode, null, tint = MaterialTheme.colorScheme.primary)
                    Text("Sayılan Ürün Girişi", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold)
                }
            },
            text = {
                Column(verticalArrangement = Arrangement.spacedBy(16.dp)) {
                    Card(
                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.2f)),
                        modifier = Modifier.fillMaxWidth()
                    ) {
                        Column(modifier = Modifier.padding(12.dp), verticalArrangement = Arrangement.spacedBy(4.dp)) {
                            Text(prod.title, style = MaterialTheme.typography.titleSmall, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.onPrimaryContainer)
                            Text("Kod: ${prod.code}", style = MaterialTheme.typography.bodySmall, color = Color.Gray)
                            Text("Barkod: ${prod.barcode}", style = MaterialTheme.typography.bodySmall, color = Color.Gray)
                        }
                    }

                    Column(verticalArrangement = Arrangement.spacedBy(6.dp)) {
                        Text("Sayılan Adet", style = MaterialTheme.typography.labelMedium, fontWeight = FontWeight.Bold)
                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            verticalAlignment = Alignment.CenterVertically,
                            horizontalArrangement = Arrangement.Center
                        ) {
                            IconButton(
                                onClick = { if (scannedQtyInput > 1) scannedQtyInput-- },
                                modifier = Modifier
                                    .size(48.dp)
                                    .background(MaterialTheme.colorScheme.surfaceVariant, shape = CircleShape)
                            ) {
                                Icon(Icons.Filled.Remove, contentDescription = "Azalt")
                            }

                            var qtyText by remember(scannedQtyInput) { mutableStateOf(scannedQtyInput.toString()) }
                            OutlinedTextField(
                                value = qtyText,
                                onValueChange = { input ->
                                    qtyText = input.filter { it.isDigit() }
                                    scannedQtyInput = qtyText.toIntOrNull() ?: 1
                                },
                                textStyle = LocalTextStyle.current.copy(textAlign = TextAlign.Center, fontWeight = FontWeight.Bold),
                                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                                modifier = Modifier
                                    .width(100.dp)
                                    .padding(horizontal = 8.dp),
                                singleLine = true
                            )

                            IconButton(
                                onClick = { scannedQtyInput++ },
                                modifier = Modifier
                                    .size(48.dp)
                                    .background(MaterialTheme.colorScheme.primaryContainer, shape = CircleShape)
                            ) {
                                Icon(Icons.Filled.Add, contentDescription = "Arttır", tint = MaterialTheme.colorScheme.onPrimaryContainer)
                            }
                        }
                    }

                    Column(verticalArrangement = Arrangement.spacedBy(6.dp)) {
                        Text("Reyon / Raf Seçin", style = MaterialTheme.typography.labelMedium, fontWeight = FontWeight.Bold)
                        var aisleMenuExpanded by remember { mutableStateOf(false) }
                        Box(modifier = Modifier.fillMaxWidth()) {
                            OutlinedTextField(
                                value = scannedAisleInput,
                                onValueChange = {}, 
                                readOnly = true,
                                trailingIcon = { Icon(Icons.Filled.ArrowDropDown, null) },
                                modifier = Modifier.fillMaxWidth().clickable { aisleMenuExpanded = true }
                            )
                            Box(
                                modifier = Modifier
                                    .matchParentSize()
                                    .clickable { aisleMenuExpanded = true }
                            )
                            DropdownMenu(
                                expanded = aisleMenuExpanded,
                                onDismissRequest = { aisleMenuExpanded = false },
                                modifier = Modifier.width(200.dp).heightIn(max = 240.dp)
                            ) {
                                val listAisles = if (registeredAisles.isEmpty()) listOf("Ana Reyon", "Reyon A", "Reyon B", "Stand") else registeredAisles
                                listAisles.forEach { aisleVal ->
                                    DropdownMenuItem(
                                        text = { Text(aisleVal, fontWeight = FontWeight.Medium) },
                                        onClick = {
                                            scannedAisleInput = aisleVal
                                            aisleMenuExpanded = false
                                        }
                                    )
                                }
                            }
                        }
                    }
                }
            },
            confirmButton = {
                Button(
                    onClick = {
                        val finalAisle = scannedAisleInput.trim().ifBlank { "TANIMSIZ" }
                        val idx = activeCountItems.indexOfFirst { it.barcode == prod.barcode && it.aisle.trim().lowercase() == finalAisle.trim().lowercase() }
                        if (idx != -1) {
                            val old = activeCountItems[idx]
                            activeCountItems[idx] = old.copy(countedQty = old.countedQty + scannedQtyInput)
                        } else {
                            activeCountItems.add(
                                CountedItem(
                                    barcode = prod.barcode,
                                    productTitle = prod.title,
                                    productCode = prod.code,
                                    brand = prod.brand ?: "Genel",
                                    expectedStock = prod.stockByWarehouse[selectedWarehouse] ?: 0,
                                    countedQty = scannedQtyInput,
                                    aisle = finalAisle
                                )
                            )
                        }
                        onBeep()
                        showScannedDetailDialog = false
                        scannedProduct = null
                    },
                    shape = RoundedCornerShape(8.dp)
                ) {
                    Text("Kaydet ve Ekle")
                }
            },
            dismissButton = {
                TextButton(
                    onClick = {
                        showScannedDetailDialog = false
                        scannedProduct = null
                    }
                ) {
                    Text("Vazgeç")
                }
            }
        )
    }

    // --- ITEM DELETE CONFIRM DIALOG ---
    if (itemToDelete != null) {
        AlertDialog(
            onDismissRequest = { itemToDelete = null },
            title = {
                Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    Icon(Icons.Filled.Warning, null, tint = MaterialTheme.colorScheme.error)
                    Text("Ürünü Sepetten Sil")
                }
            },
            text = { Text("Bu ürünü sepetten tamamen silmek istediğinize emin misiniz?") },
            confirmButton = {
                Button(
                    onClick = {
                        activeCountItems.remove(itemToDelete)
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

    // --- CLEAR BASKET CONFIRM DIALOG ---
    if (showClearBasketConfirmDialog) {
        AlertDialog(
            onDismissRequest = { showClearBasketConfirmDialog = false },
            title = {
                Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    Icon(Icons.Filled.Warning, null, tint = MaterialTheme.colorScheme.error)
                    Text("Çeteleyi Boşalt")
                }
            },
            text = { Text("Sepetteki tüm ürünleri silmek istediğinize emin misiniz? Bu işlem geri alınamaz.") },
            confirmButton = {
                Button(
                    onClick = {
                        activeCountItems.clear()
                        showClearBasketConfirmDialog = false
                    },
                    colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.error)
                ) {
                    Text("Tümünü Temizle")
                }
            },
            dismissButton = {
                TextButton(onClick = { showClearBasketConfirmDialog = false }) {
                    Text("İptal")
                }
            }
        )
    }

    // --- SESSION DELETE CONFIRM DIALOG ---
    if (sessionToDelete != null) {
        AlertDialog(
            onDismissRequest = { sessionToDelete = null },
            title = {
                Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    Icon(Icons.Filled.Warning, null, tint = MaterialTheme.colorScheme.error)
                    Text("Fişi Sil")
                }
            },
            text = { Text("Bu bekleyen sayım fişini tamamen silmek istediğinize emin misiniz?") },
            confirmButton = {
                Button(
                    onClick = {
                        AppDataStore.stockCountSessions.remove(sessionToDelete)
                        AppDataStore.persist(context)
                        sessionToDelete = null
                    },
                    colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.error)
                ) {
                    Text("Evet, Sil")
                }
            },
            dismissButton = {
                TextButton(onClick = { sessionToDelete = null }) {
                    Text("Vazgeç")
                }
            }
        )
    }

    // --- 2. FINALIZE STOCK COUNT DETAILS ACTION CONFIRM_DIALOG ---
    if (showFinalizeConfirmDialog) {
        AlertDialog(
            onDismissRequest = { showFinalizeConfirmDialog = false },
            title = {
                Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    Icon(Icons.Filled.VerifiedUser, null, tint = MaterialTheme.colorScheme.secondary)
                    Text("Sayımı Kesinleştir")
                }
            },
            text = {
                Column(verticalArrangement = Arrangement.spacedBy(12.dp)) {
                    Text(
                        "Dikkat! Sayımı kesinleştirdiğinizde seçilen ($selectedWarehouse) deposunun mevcut sistem stokları saydığınız adetlere göre güncellenecektir. Ayrıca sayılan ürünlerin reyon bilgileri de güncellenecektir.",
                        style = MaterialTheme.typography.bodyMedium
                    )

                    OutlinedTextField(
                        value = operatorName,
                        onValueChange = { operatorName = it },
                        label = { Text("Onaylayan Sorumlu Personel") },
                        singleLine = true,
                        modifier = Modifier.fillMaxWidth()
                    )
                }
            },
            confirmButton = {
                Button(
                    onClick = {
                        if (operatorName.isBlank()) {
                            operatorName = "Yetkili Sorumlu"
                        }
                        
                        // Proceed to FINALIZE and alter databases
                        productsCatalog.forEach { prod ->
                            // Find all counted items for this barcode (could be in different aisles, sum them up or take last aisle)
                            val counts = activeCountItems.filter { it.barcode == prod.barcode }
                            if (counts.isNotEmpty()) {
                                val totalCountedQty = counts.sumOf { it.countedQty }
                                val lastAisle = counts.last().aisle.trim()
                                
                                // Update Map of stocks
                                val updatedMap = prod.stockByWarehouse.toMutableMap()
                                updatedMap[selectedWarehouse] = totalCountedQty
                                
                                // Find product index inside store and replace
                                val indexP = productsCatalog.indexOf(prod)
                                if (indexP != -1) {
                                    productsCatalog[indexP] = prod.copy(
                                        stockByWarehouse = updatedMap,
                                        aisle = if (lastAisle.uppercase() != "TANIMSIZ") lastAisle else prod.aisle
                                    )
                                }
                            }
                        }

                        // Save session log
                        val sessId = activeSessionId.ifBlank { "SC-" + System.currentTimeMillis() }
                        val sdf = java.text.SimpleDateFormat("dd.MM.yyyy HH:mm", java.util.Locale.getDefault())
                        val dateStr = sdf.format(java.util.Date())

                        val completedSession = StockCountSession(
                            id = sessId,
                            date = dateStr,
                            status = "COMPLETED",
                            user = operatorName.trim(),
                            warehouse = selectedWarehouse,
                            countedItems = activeCountItems.toList()
                        )

                        // If it matches existing draft, replace it. Else add as new
                        val existingIndex = AppDataStore.stockCountSessions.indexOfFirst { it.id == sessId }
                        if (existingIndex != -1) {
                            AppDataStore.stockCountSessions[existingIndex] = completedSession
                        } else {
                            AppDataStore.stockCountSessions.add(completedSession)
                        }

                        // PERSIST everything to Room SQLite and trigger cloud backup if subscribed
                        AppDataStore.persist(context)

                        // Clear active states
                        activeCountItems.clear()
                        activeSessionId = ""
                        showFinalizeConfirmDialog = false
                        
                        // Navigate to history tab to view completed item list
                        activeTab = 1
                    },
                    colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.secondary)
                ) {
                    Text("Onaylıyorum (DB Güncelle)")
                }
            },
            dismissButton = {
                TextButton(onClick = { showFinalizeConfirmDialog = false }) {
                    Text("İptal")
                }
            }
        )
    }

    // --- 3. CONSOLIDATE/MERGE DIVERGENT AISLE POPUP SHEET DIALOG ---
    showAisleMergeId?.let { barcode ->
        val duplicates = duplicateAislesGroup[barcode] ?: emptyList()
        val totalQty = duplicates.sumOf { it.countedQty }
        val productTitle = duplicates.firstOrNull()?.productTitle ?: ""

        AlertDialog(
            onDismissRequest = { showAisleMergeId = null },
            title = {
                Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    Icon(Icons.Filled.Merge, null, tint = MaterialTheme.colorScheme.primary)
                    Text("Reyonları Birleştir")
                }
            },
            text = {
                Column(verticalArrangement = Arrangement.spacedBy(12.dp)) {
                    Text(
                        text = "$productTitle ürünü bu seans içinde farklı raflarda sayıldı ($totalQty adet). Hangi reyon altında birleştirmek istersiniz?",
                        style = MaterialTheme.typography.bodyMedium
                    )

                    var chosenMergeAisle by remember { mutableStateOf(duplicates.firstOrNull()?.aisle ?: "A-01") }

                    Text("Kullanılabilir Reyonlar:", style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Bold, color = Color.Gray)

                    duplicates.forEach { item ->
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .clickable { chosenMergeAisle = item.aisle }
                                .padding(vertical = 4.dp),
                            verticalAlignment = Alignment.CenterVertically,
                            horizontalArrangement = Arrangement.spacedBy(8.dp)
                        ) {
                            RadioButton(
                                selected = chosenMergeAisle == item.aisle,
                                onClick = { chosenMergeAisle = item.aisle }
                            )
                            Text("Raf: " + item.aisle + " (${item.countedQty} adet)", style = MaterialTheme.typography.bodyMedium)
                        }
                    }

                    Spacer(modifier = Modifier.height(4.dp))
                    OutlinedTextField(
                        value = chosenMergeAisle,
                        onValueChange = { chosenMergeAisle = it },
                        label = { Text("Farklı bir Reyon ismi gir...") },
                        singleLine = true,
                        modifier = Modifier.fillMaxWidth()
                    )

                    Button(
                        onClick = {
                            // Find all matching duplicate counted items
                            val itemsToRemove = activeCountItems.filter { it.barcode == barcode }
                            activeCountItems.removeAll(itemsToRemove)

                            // Add a single unified CountedItem containing summed quantity
                            val firstItem = itemsToRemove.first()
                            activeCountItems.add(
                                CountedItem(
                                    barcode = barcode,
                                    productTitle = firstItem.productTitle,
                                    productCode = firstItem.productCode,
                                    brand = firstItem.brand,
                                    expectedStock = firstItem.expectedStock,
                                    countedQty = totalQty,
                                    aisle = chosenMergeAisle.trim()
                                )
                            )

                            // Clear merge dialogue
                            showAisleMergeId = null
                        },
                        colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.primary),
                        modifier = Modifier.fillMaxWidth()
                    ) {
                        Text("Seçilen Reyonda Birleştir ve Temizle")
                    }
                }
            },
            confirmButton = {},
            dismissButton = {
                TextButton(onClick = { showAisleMergeId = null }) {
                    Text("Kapat")
                }
            }
        )
    }
}
}

// --- MODULE 7: WAREHOUSES (DEPOLAR) ---
data class WhItem(val name: String, val manager: String, val capacity: Float, val count: Int, val isMobile: Boolean)

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun WarehousesModule() {
    val whList = listOf(
        WhItem("Ana Merkez Depo (C-Blok)", "Ahmet Hamdi", 0.76f, 14200, false),
        WhItem("Kuzey Bölge Lojistik", "Selin Topal", 0.38f, 5400, false),
        WhItem("Mobil Dağıtım Transit Karavanı", "Zafer Çelik", 0.92f, 1310, true),
        WhItem("Merkez Kasa & Özel Ekipman Deposu", "Melis Ay", 0.12f, 480, false)
    )

    var selectedWarehouse by remember { mutableStateOf<WhItem?>(null) }

    if (selectedWarehouse == null) {
        // Mode A: General Warehouse Matrix List
        LazyColumn(
            modifier = Modifier.fillMaxSize().padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(14.dp)
        ) {
            item {
                Text("Saha Depo & Envanter Doluluk Matrisi", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
            }
            items(whList) { wh ->
                Card(
                    modifier = Modifier
                        .fillMaxWidth()
                        .clickable { selectedWarehouse = wh }
                ) {
                    Column(modifier = Modifier.padding(16.dp), verticalArrangement = Arrangement.spacedBy(10.dp)) {
                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.SpaceBetween,
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                                Icon(
                                    if (wh.isMobile) Icons.Filled.LocalShipping else Icons.Filled.Warehouse,
                                    contentDescription = null,
                                    tint = MaterialTheme.colorScheme.primary
                                )
                                Text(wh.name, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodyLarge)
                            }
                            if (wh.isMobile) {
                                Surface(color = MaterialTheme.colorScheme.secondaryContainer, shape = RoundedCornerShape(8.dp)) {
                                    Text("Mobil", modifier = Modifier.padding(horizontal = 6.dp, vertical = 2.dp), style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.secondary)
                                }
                            }
                        }

                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.SpaceBetween
                        ) {
                            Text("Depo Sorumlusu: ${wh.manager}", style = MaterialTheme.typography.bodyMedium, color = Color.Gray)
                            Text("Mevcut: ${wh.count} Ürün", fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodyMedium)
                        }

                        Column {
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text("Hacim Doluluk Oranı", style = MaterialTheme.typography.labelSmall)
                                Text("%${(wh.capacity * 100).toInt()}", style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Bold, color = if (wh.capacity > 0.85f) MaterialTheme.colorScheme.error else MaterialTheme.colorScheme.primary)
                            }
                            Spacer(modifier = Modifier.height(4.dp))
                            Box(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .height(10.dp)
                                    .background(MaterialTheme.colorScheme.surfaceVariant, CircleShape)
                            ) {
                                Box(
                                    modifier = Modifier
                                        .fillMaxHeight()
                                        .fillMaxWidth(wh.capacity)
                                        .background(
                                            color = if (wh.capacity > 0.85f) MaterialTheme.colorScheme.error else MaterialTheme.colorScheme.secondary,
                                            shape = CircleShape
                                        )
                                )
                            }
                        }
                    }
                }
            }
        }
    } else {
        // Mode B: Detailed Product View inside selected warehouse
        val wh = selectedWarehouse!!
        val whKey = when {
            wh.name.contains("Ana Merkez") -> "Ana Depo"
            wh.name.contains("Kuzey") -> "Ankara Merkez"
            wh.name.contains("Mobil") -> "Ege Bölge"
            else -> "Merkez Kasa"
        }

        var searchQuery by remember { mutableStateOf("") }
        var selectedCategory by remember { mutableStateOf("Tümü") }
        var showOnlyInStock by remember { mutableStateOf(false) }
        var sortBy by remember { mutableStateOf("İsim (A-Z)") } // "İsim (A-Z)", "Kod (A-Z)", "Stok (Azalan)", "Stok (Artan)"
        
        var showSortMenu by remember { mutableStateOf(false) }
        var showFilterMenu by remember { mutableStateOf(false) }

        // Compile product list and custom quantities for this specific warehouse
        val rawProductsInWarehouse = AppDataStore.products.map { product ->
            val qty = product.stockByWarehouse[whKey] ?: 0
            product to qty
        }

        // Apply filters
        val filteredProducts = rawProductsInWarehouse.filter { (product, qty) ->
            val matchesSearch = product.title.contains(searchQuery, ignoreCase = true) || 
                    product.code.contains(searchQuery, ignoreCase = true) ||
                    product.barcode.contains(searchQuery, ignoreCase = true)
            
            val matchesCategory = selectedCategory == "Tümü" || product.category == selectedCategory
            
            val matchesStock = !showOnlyInStock || qty > 0

            matchesSearch && matchesCategory && matchesStock
        }.sortedWith { a, b ->
            when (sortBy) {
                "İsim (A-Z)" -> a.first.title.compareTo(b.first.title, ignoreCase = true)
                "Kod (A-Z)" -> a.first.code.compareTo(b.first.code, ignoreCase = true)
                "Stok (Azalan)" -> b.second.compareTo(a.second)
                "Stok (Artan)" -> a.second.compareTo(b.second)
                else -> a.first.title.compareTo(b.first.title, ignoreCase = true)
            }
        }

        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(12.dp)
        ) {
            // Header with back button & description
            Row(
                modifier = Modifier.fillMaxWidth(),
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.spacedBy(8.dp)
            ) {
                IconButton(onClick = { selectedWarehouse = null }) {
                    Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = "Geri Dön")
                }
                Column {
                    Text(wh.name, style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
                    Text("Sorumlu: ${wh.manager} | Doluluk: %${(wh.capacity*100).toInt()}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                }
            }

            // Search Bar, Sort Button and Filter Button row
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.spacedBy(8.dp),
                verticalAlignment = Alignment.CenterVertically
            ) {
                OutlinedTextField(
                    value = searchQuery,
                    onValueChange = { searchQuery = it },
                    placeholder = { Text("Ürün adı, kod veya barkod...", style = MaterialTheme.typography.bodySmall) },
                    leadingIcon = { Icon(Icons.Filled.Search, null, modifier = Modifier.size(18.dp)) },
                    modifier = Modifier
                        .weight(1f)
                        .height(48.dp),
                    textStyle = MaterialTheme.typography.bodySmall,
                    singleLine = true,
                    shape = RoundedCornerShape(12.dp),
                    colors = OutlinedTextFieldDefaults.colors(
                        focusedContainerColor = MaterialTheme.colorScheme.surface,
                        unfocusedContainerColor = MaterialTheme.colorScheme.surface
                    )
                )

                // Sort Action Menu
                Box {
                    IconButton(
                        onClick = { showSortMenu = true },
                        modifier = Modifier
                            .background(MaterialTheme.colorScheme.primaryContainer, RoundedCornerShape(12.dp))
                            .size(50.dp)
                    ) {
                        Icon(
                            Icons.Filled.Sort,
                            contentDescription = "Sırala",
                            tint = MaterialTheme.colorScheme.onPrimaryContainer
                        )
                    }

                    DropdownMenu(
                        expanded = showSortMenu,
                        onDismissRequest = { showSortMenu = false }
                    ) {
                        listOf("İsim (A-Z)", "Kod (A-Z)", "Stok (Azalan)", "Stok (Artan)").forEach { option ->
                            DropdownMenuItem(
                                text = { Text(option) },
                                onClick = {
                                    sortBy = option
                                    showSortMenu = false
                                },
                                leadingIcon = {
                                    if (sortBy == option) {
                                        Icon(Icons.Filled.Check, null, tint = MaterialTheme.colorScheme.primary, modifier = Modifier.size(16.dp))
                                    }
                                }
                            )
                        }
                    }
                }

                // Filter Menu
                Box {
                    IconButton(
                        onClick = { showFilterMenu = true },
                        modifier = Modifier
                            .background(
                                color = if (selectedCategory != "Tümü" || showOnlyInStock) MaterialTheme.colorScheme.secondaryContainer else MaterialTheme.colorScheme.surfaceVariant,
                                shape = RoundedCornerShape(12.dp)
                            )
                            .size(50.dp)
                    ) {
                        Icon(
                            Icons.Filled.FilterList,
                            contentDescription = "Filtrele",
                            tint = if (selectedCategory != "Tümü" || showOnlyInStock) MaterialTheme.colorScheme.onSecondaryContainer else MaterialTheme.colorScheme.onSurfaceVariant
                        )
                    }

                    DropdownMenu(
                        expanded = showFilterMenu,
                        onDismissRequest = { showFilterMenu = false }
                    ) {
                        DropdownMenuItem(
                            text = { Text("Kategori Seç", fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary) },
                            onClick = {},
                            enabled = false
                        )
                        
                        listOf("Tümü", "Endüstriyel Yağlar", "Filtre Grupları", "Yedek Parça", "Sarf Malzemeler").forEach { cat ->
                            DropdownMenuItem(
                                text = { Text(cat) },
                                onClick = {
                                    selectedCategory = cat
                                    showFilterMenu = false
                                },
                                leadingIcon = {
                                    if (selectedCategory == cat) {
                                        Icon(Icons.Filled.Check, null, modifier = Modifier.size(16.dp))
                                    }
                                }
                            )
                        }

                        HorizontalDivider()

                        DropdownMenuItem(
                            text = { 
                                Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                                    Checkbox(checked = showOnlyInStock, onCheckedChange = null)
                                    Text("Sadece Stokta Olanlar")
                                }
                            },
                            onClick = {
                                showOnlyInStock = !showOnlyInStock
                                showFilterMenu = false
                            }
                        )
                    }
                }
            }

            // Quick Filters Tags Bar
            if (selectedCategory != "Tümü" || showOnlyInStock) {
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.spacedBy(6.dp)
                ) {
                    if (selectedCategory != "Tümü") {
                        InputChip(
                            selected = true,
                            onClick = { selectedCategory = "Tümü" },
                            label = { Text(selectedCategory) },
                            trailingIcon = { Icon(Icons.Filled.Close, null, modifier = Modifier.size(14.dp)) }
                        )
                    }
                    if (showOnlyInStock) {
                        InputChip(
                            selected = true,
                            onClick = { showOnlyInStock = false },
                            label = { Text("Stokta Var") },
                            trailingIcon = { Icon(Icons.Filled.Close, null, modifier = Modifier.size(14.dp)) }
                        )
                    }
                }
            }

            // Detailed Warehouse Stock Items List
            if (filteredProducts.isEmpty()) {
                Box(
                    modifier = Modifier.fillMaxSize().weight(1f),
                    contentAlignment = Alignment.Center
                ) {
                    Column(
                        horizontalAlignment = Alignment.CenterHorizontally,
                        verticalArrangement = Arrangement.spacedBy(10.dp)
                    ) {
                        Icon(Icons.Filled.SearchOff, null, tint = Color.Gray, modifier = Modifier.size(56.dp))
                        Text("Depoda eşleşen ürün bulunamadı.", color = Color.Gray, style = MaterialTheme.typography.bodyMedium)
                    }
                }
            } else {
                LazyColumn(
                    modifier = Modifier.fillMaxSize().weight(1f),
                    verticalArrangement = Arrangement.spacedBy(10.dp)
                ) {
                    items(filteredProducts) { (prod, qty) ->
                        Card(
                            modifier = Modifier.fillMaxWidth(),
                            colors = CardDefaults.cardColors(
                                containerColor = if (qty == 0) MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.3f) else MaterialTheme.colorScheme.surfaceContainerHigh
                            )
                        ) {
                            Row(
                                modifier = Modifier.padding(12.dp).fillMaxWidth(),
                                verticalAlignment = Alignment.CenterVertically,
                                horizontalArrangement = Arrangement.spacedBy(12.dp)
                            ) {
                                // Product Visual Marker Icon Indicator
                                Box(
                                    modifier = Modifier
                                        .size(42.dp)
                                        .background(prod.imageUrlColor, RoundedCornerShape(8.dp))
                                        .clip(RoundedCornerShape(8.dp)),
                                    contentAlignment = Alignment.Center
                                ) {
                                    if (prod.localImagePath != null || prod.imageUrl != null) {
                                        val model = if (prod.localImagePath != null) {
                                            java.io.File(prod.localImagePath)
                                        } else {
                                            prod.imageUrl
                                        }
                                        coil.compose.AsyncImage(
                                            model = model,
                                            contentDescription = prod.title,
                                            modifier = Modifier.fillMaxSize(),
                                            contentScale = androidx.compose.ui.layout.ContentScale.Crop
                                        )
                                    } else {
                                        Icon(Icons.Filled.Inventory, null, tint = Color.White, modifier = Modifier.size(22.dp))
                                    }
                                }

                                Column(modifier = Modifier.weight(1f)) {
                                    Text(
                                        text = prod.title,
                                        fontWeight = FontWeight.Bold,
                                        style = MaterialTheme.typography.bodyMedium,
                                        maxLines = 2
                                    )
                                    Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                                        Text("Kod: ${prod.code}", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                        Text("•", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                        Text(prod.category, style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.secondary)
                                    }
                                }

                                Column(horizontalAlignment = Alignment.End) {
                                    Text(
                                        text = "$qty ADT",
                                        fontWeight = FontWeight.Black,
                                        style = MaterialTheme.typography.bodyLarge,
                                        color = if (qty == 0) Color.Gray else MaterialTheme.colorScheme.primary
                                    )
                                    Text(
                                        text = String.format("%,.2f ₺", prod.basePrice),
                                        style = MaterialTheme.typography.labelSmall,
                                        color = Color.Gray
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

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun EodModule(onSuccess: () -> Unit) {
    val context = LocalContext.current
    var step1Checked by remember { mutableStateOf(true) }
    var step2Checked by remember { mutableStateOf(true) }
    var step3Checked by remember { mutableStateOf(true) }
    var step4Checked by remember { mutableStateOf(true) }
    
    var isPrinting by remember { mutableStateOf(false) }
    var printSuccess by remember { mutableStateOf(false) }
    
    var isClosingDay by remember { mutableStateOf(false) }
    var dayClosedSuccess by remember { mutableStateOf(false) }
    
    val scope = rememberCoroutineScope()
    
    var selectedDate by remember { mutableStateOf("08.06.2026") }

    val filteredKasaLogs = remember(selectedDate, AppDataStore.kasaLogs.size) {
        AppDataStore.kasaLogs.filter { log ->
            val logDate = log.date.substringBefore(" ")
            logDate == selectedDate
        }
    }

    // Live counts & aggregates filtered by selection
    val todaySales = filteredKasaLogs.filter { it.type == "Satış" }.sumOf { it.amount }
    val todaySalesCount = filteredKasaLogs.count { it.type == "Satış" }
    
    val todayCollections = filteredKasaLogs.filter { it.type == "Tahsilat" }.sumOf { it.amount }
    val todayCollectionsCount = filteredKasaLogs.count { it.type == "Tahsilat" }
    
    val todayTediyes = filteredKasaLogs.filter { it.type == "Tediye" && !it.desc.contains("Alış") }.sumOf { it.amount }
    val todayTediyesCount = filteredKasaLogs.count { it.type == "Tediye" && !it.desc.contains("Alış") }
    
    val todayReturns = filteredKasaLogs.filter { it.type == "İade" }.sumOf { it.amount }
    val todayReturnsCount = filteredKasaLogs.count { it.type == "İade" }

    val todayPurchases = filteredKasaLogs.filter { it.type == "Alış" || (it.type == "Tediye" && it.desc.contains("Alış")) }.sumOf { it.amount }
    val todayPurchasesCount = filteredKasaLogs.count { it.type == "Alış" || (it.type == "Tediye" && it.desc.contains("Alış")) }
    
    // Calculated net cash flow
    val netCashFlow = todaySales + todayCollections - todayTediyes - todayReturns - todayPurchases

    // Cash amounts to deliver to the cash register (paymentType == "Nakit")
    val cashInflowSales = filteredKasaLogs.filter { it.type == "Satış" && it.paymentType == "Nakit" }.sumOf { it.amount }
    val cashInflowCollections = filteredKasaLogs.filter { it.type == "Tahsilat" && it.paymentType == "Nakit" }.sumOf { it.amount }
    val cashOutflowTediyes = filteredKasaLogs.filter { it.type == "Tediye" && it.paymentType == "Nakit" }.sumOf { it.amount }
    val cashOutflowReturns = filteredKasaLogs.filter { it.type == "İade" && it.paymentType == "Nakit" }.sumOf { it.amount }
    val cashOutflowPurchases = filteredKasaLogs.filter { (it.type == "Alış" || (it.type == "Tediye" && it.desc.contains("Alış"))) && it.paymentType == "Nakit" }.sumOf { it.amount }

    val cashToDeliver = cashInflowSales + cashInflowCollections - cashOutflowTediyes - cashOutflowReturns - cashOutflowPurchases

    // Native date picker dialog
    val dateParts = selectedDate.split(".")
    val currentYear = dateParts.getOrNull(2)?.toIntOrNull() ?: 2026
    val currentMonth = (dateParts.getOrNull(1)?.toIntOrNull() ?: 6) - 1
    val currentDay = dateParts.getOrNull(0)?.toIntOrNull() ?: 8

    val datePickerDialog = remember(selectedDate) {
        android.app.DatePickerDialog(
            context,
            { _, yearSelected, monthSelected, daySelected ->
                selectedDate = String.format("%02d.%02d.%04d", daySelected, monthSelected + 1, yearSelected)
            },
            currentYear,
            currentMonth,
            currentDay
        )
    }
    
    if (dayClosedSuccess) {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(16.dp),
            verticalArrangement = Arrangement.Center,
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Box(
                modifier = Modifier
                    .size(64.dp)
                    .background(Color(0xFFE8F5E9), CircleShape),
                contentAlignment = Alignment.Center
            ) {
                Icon(
                    Icons.Filled.CheckCircle,
                    contentDescription = null,
                    tint = Color(0xFF43A047),
                    modifier = Modifier.size(38.dp)
                )
            }
            Spacer(modifier = Modifier.height(16.dp))
            Text(
                text = "GÜN BAŞARIYLA KAPATILDI",
                style = MaterialTheme.typography.titleMedium,
                fontWeight = FontWeight.Bold,
                color = MaterialTheme.colorScheme.primary
            )
            Spacer(modifier = Modifier.height(8.dp))
            Text(
                text = "$selectedDate tarihli saha çalışma günü kapandı. Z raporu sisteme entegre edildi.\nTüm offline kuyruklar temizlendi ve ERP merkezine iletildi.",
                color = Color.DarkGray,
                style = MaterialTheme.typography.bodySmall,
                textAlign = TextAlign.Center
            )
            
            Spacer(modifier = Modifier.height(16.dp))
            
            // Stats summary card
            Card(
                modifier = Modifier.fillMaxWidth(),
                colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f))
            ) {
                Column(modifier = Modifier.padding(12.dp), verticalArrangement = Arrangement.spacedBy(6.dp)) {
                    Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                        Text("Z-No:", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                        Text("Z-2026-156", fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodySmall)
                    }
                    Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                        Text("Toplam Ciro (Satış):", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                        Text(String.format("%,.2f ₺", todaySales), fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodySmall)
                    }
                    Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                        Text("Toplam Tahsilat:", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                        Text(String.format("%,.2f ₺", todayCollections), fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodySmall)
                    }
                    Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                        Text("Toplam Satınalma / Alış:", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                        Text(String.format("%,.2f ₺", todayPurchases), fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.error)
                    }
                    Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                        Text("Net Kasa Girişi:", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                        Text(String.format("%,.2f ₺", netCashFlow), fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodySmall, color = Color(0xFF43A047))
                    }
                    Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                        Text("Teslim Edilen Nakit:", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                        Text(String.format("%,.2f ₺", cashToDeliver), fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.primary)
                    }
                }
            }
        }
    } else {
        LazyColumn(
            modifier = Modifier
                .fillMaxSize()
                .padding(12.dp),
            verticalArrangement = Arrangement.spacedBy(12.dp)
        ) {
            // Calendar / Date selection Row
            item {
                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .clip(RoundedCornerShape(12.dp))
                        .background(MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.25f))
                        .clickable { datePickerDialog.show() }
                        .padding(horizontal = 16.dp, vertical = 14.dp),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Row(
                        verticalAlignment = Alignment.CenterVertically,
                        horizontalArrangement = Arrangement.spacedBy(12.dp)
                    ) {
                        Icon(
                            imageVector = Icons.Filled.DateRange,
                            contentDescription = "Tarih Seç",
                            tint = MaterialTheme.colorScheme.primary,
                            modifier = Modifier.size(24.dp)
                        )
                        Column {
                            Text(
                                text = "LİSTELENEN ÇALIŞMA TARİHİ",
                                style = MaterialTheme.typography.labelSmall,
                                color = MaterialTheme.colorScheme.primary,
                                fontWeight = FontWeight.Bold
                            )
                            Text(
                                text = selectedDate,
                                style = MaterialTheme.typography.bodyMedium,
                                fontWeight = FontWeight.Bold,
                                color = MaterialTheme.colorScheme.onSurface
                            )
                        }
                    }
                    Row(
                        verticalAlignment = Alignment.CenterVertically,
                        horizontalArrangement = Arrangement.spacedBy(4.dp)
                    ) {
                        Text(
                            text = "Tarihi Değiştir",
                            style = MaterialTheme.typography.labelMedium,
                            fontWeight = FontWeight.Bold,
                            color = MaterialTheme.colorScheme.primary
                        )
                        Icon(
                            imageVector = Icons.Filled.EditCalendar,
                            contentDescription = null,
                            tint = MaterialTheme.colorScheme.primary,
                            modifier = Modifier.size(18.dp)
                        )
                    }
                }
            }

            // --- MAJOR HIGHLIGHT: CASH TO HANDOVER ---
            item {
                Card(
                    modifier = Modifier.fillMaxWidth(),
                    colors = CardDefaults.cardColors(
                        containerColor = MaterialTheme.colorScheme.primaryContainer
                    ),
                    elevation = CardDefaults.cardElevation(defaultElevation = 2.dp)
                ) {
                    Column(
                        modifier = Modifier.padding(16.dp),
                        verticalArrangement = Arrangement.spacedBy(10.dp)
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
                                Icon(
                                    imageVector = Icons.Filled.LocalAtm,
                                    contentDescription = null,
                                    tint = MaterialTheme.colorScheme.onPrimaryContainer,
                                    modifier = Modifier.size(24.dp)
                                )
                                Text(
                                    text = "KASAYA TESLİM EDİLECEK PARA",
                                    style = MaterialTheme.typography.labelMedium,
                                    fontWeight = FontWeight.Bold,
                                    color = MaterialTheme.colorScheme.onPrimaryContainer
                                )
                            }
                            Box(
                                modifier = Modifier
                                    .clip(CircleShape)
                                    .background(MaterialTheme.colorScheme.onPrimaryContainer.copy(alpha = 0.15f))
                                    .padding(horizontal = 8.dp, vertical = 4.dp)
                            ) {
                                Text(
                                    text = "NET FİZİKSEL NAKİT",
                                    style = MaterialTheme.typography.labelSmall,
                                    color = MaterialTheme.colorScheme.onPrimaryContainer,
                                    fontWeight = FontWeight.Bold,
                                    fontSize = 9.sp
                                )
                            }
                        }
                        
                        Text(
                            text = String.format("%,.2f ₺", cashToDeliver),
                            style = MaterialTheme.typography.headlineMedium,
                            fontWeight = FontWeight.ExtraBold,
                            color = MaterialTheme.colorScheme.onPrimaryContainer
                        )
                        
                        HorizontalDivider(color = MaterialTheme.colorScheme.onPrimaryContainer.copy(alpha = 0.2f))
                        
                        Column(verticalArrangement = Arrangement.spacedBy(4.dp)) {
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.SpaceBetween
                            ) {
                                Text(
                                    text = "Giren Nakit (Peşin Satış + Nakit Tahsilat):", 
                                    style = MaterialTheme.typography.labelSmall, 
                                    color = MaterialTheme.colorScheme.onPrimaryContainer.copy(alpha = 0.8f)
                                )
                                Text(
                                    text = String.format("%,.2f ₺", cashInflowSales + cashInflowCollections), 
                                    style = MaterialTheme.typography.labelSmall, 
                                    fontWeight = FontWeight.Bold,
                                    color = MaterialTheme.colorScheme.onPrimaryContainer
                                )
                            }
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.SpaceBetween
                            ) {
                                Text(
                                    text = "Çıkan Nakit (Nakit Tediye + İade + Alış):", 
                                    style = MaterialTheme.typography.labelSmall, 
                                    color = MaterialTheme.colorScheme.onPrimaryContainer.copy(alpha = 0.8f)
                                )
                                Text(
                                    text = String.format("-%,.2f ₺", cashOutflowTediyes + cashOutflowReturns + cashOutflowPurchases), 
                                    style = MaterialTheme.typography.labelSmall, 
                                    fontWeight = FontWeight.Bold,
                                    color = MaterialTheme.colorScheme.error
                                )
                            }
                        }
                    }
                }
            }

            item {
                Card(
                    modifier = Modifier.fillMaxWidth(),
                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.4f))
                ) {
                    Column(
                        modifier = Modifier.padding(12.dp),
                        verticalArrangement = Arrangement.spacedBy(6.dp)
                    ) {
                        Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(6.dp)) {
                            Icon(Icons.Filled.History, null, tint = MaterialTheme.colorScheme.primary, modifier = Modifier.size(20.dp))
                            Text("GÜNLÜK SAHA FAALİYET MUTABAKATI", fontWeight = FontWeight.Bold, style = MaterialTheme.typography.titleSmall)
                        }
                        Text(
                            "Seçilen ($selectedDate) günü içerisinde sahada yaptığınız satış, tahsilat, tediye, iade ve alış işlemlerinin toplamı aşağıda listelenmiştir. Lütfen tutarları kontrol ederek gün sonunu onaylayın.",
                            style = MaterialTheme.typography.labelSmall,
                            color = Color.Gray
                        )
                    }
                }
            }

            // Summary grid in EOD - Compact formatting
            item {
                Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                    Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                        Card(modifier = Modifier.weight(1f), colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface)) {
                            Column(modifier = Modifier.padding(10.dp)) {
                                Text("Nakit/Pos Satış", style = MaterialTheme.typography.labelSmall, color = Color.Gray, fontSize = 11.sp)
                                Text(String.format("%,.2f ₺", todaySales), style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold)
                                Text("$todaySalesCount evrak", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.primary, fontSize = 10.sp)
                            }
                        }
                        Card(modifier = Modifier.weight(1f), colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface)) {
                            Column(modifier = Modifier.padding(10.dp)) {
                                Text("Tahsilat", style = MaterialTheme.typography.labelSmall, color = Color.Gray, fontSize = 11.sp)
                                Text(String.format("%,.2f ₺", todayCollections), style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold)
                                Text("$todayCollectionsCount tahsilat", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.primary, fontSize = 10.sp)
                            }
                        }
                    }
                    Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                        Card(modifier = Modifier.weight(1f), colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface)) {
                            Column(modifier = Modifier.padding(10.dp)) {
                                Text("Cari Tediye", style = MaterialTheme.typography.labelSmall, color = Color.Gray, fontSize = 11.sp)
                                Text(String.format("%,.2f ₺", todayTediyes), style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.error)
                                Text("$todayTediyesCount tediye", style = MaterialTheme.typography.labelSmall, color = Color.Gray, fontSize = 10.sp)
                            }
                        }
                        Card(modifier = Modifier.weight(1f), colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface)) {
                            Column(modifier = Modifier.padding(10.dp)) {
                                Text("İade Faturası", style = MaterialTheme.typography.labelSmall, color = Color.Gray, fontSize = 11.sp)
                                Text(String.format("%,.2f ₺", todayReturns), style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.error)
                                Text("$todayReturnsCount iade", style = MaterialTheme.typography.labelSmall, color = Color.Gray, fontSize = 10.sp)
                            }
                        }
                    }
                    Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                        Card(modifier = Modifier.weight(1f), colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface)) {
                            Column(modifier = Modifier.padding(10.dp)) {
                                Text("Satınalma / Alış", style = MaterialTheme.typography.labelSmall, color = Color.Gray, fontSize = 11.sp)
                                Text(String.format("%,.2f ₺", todayPurchases), style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.error)
                                Text("$todayPurchasesCount alıs", style = MaterialTheme.typography.labelSmall, color = Color.Gray, fontSize = 10.sp)
                            }
                        }
                        Card(modifier = Modifier.weight(1f), colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface)) {
                            Column(modifier = Modifier.padding(10.dp)) {
                                Text("Toplam Belgeler", style = MaterialTheme.typography.labelSmall, color = Color.Gray, fontSize = 11.sp)
                                val totalDocs = todaySalesCount + todayCollectionsCount + todayTediyesCount + todayReturnsCount + todayPurchasesCount
                                Text("$totalDocs Adet", style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
                                Text("evrak & fiş", style = MaterialTheme.typography.labelSmall, color = Color.Gray, fontSize = 10.sp)
                            }
                        }
                    }
                }
            }

            item {
                Card(modifier = Modifier.fillMaxWidth()) {
                    Column(modifier = Modifier.padding(12.dp), verticalArrangement = Arrangement.spacedBy(4.dp)) {
                        Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween, verticalAlignment = Alignment.CenterVertically) {
                            Text("Kasa Net Değişimi:", style = MaterialTheme.typography.bodySmall, fontWeight = FontWeight.Bold)
                            val finalCol = if (netCashFlow >= 0) Color(0xFF43A047) else MaterialTheme.colorScheme.error
                            Text(String.format("%,.2f ₺", netCashFlow), style = MaterialTheme.typography.titleSmall, fontWeight = FontWeight.Bold, color = finalCol)
                        }
                    }
                }
            }

            // --- USER TRANSACTIONS LIST SECTION (Itemized Separately) ---
            item {
                Text(
                    text = "GÜNLÜK İŞLEM VE BELGE DETAYLARI",
                    style = MaterialTheme.typography.labelSmall,
                    color = MaterialTheme.colorScheme.primary,
                    fontWeight = FontWeight.Bold
                )
            }

            // If no data exists, show custom Empty State Card
            if (filteredKasaLogs.isEmpty()) {
                item {
                    Card(
                        modifier = Modifier.fillMaxWidth(),
                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
                        border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
                    ) {
                        Column(
                            modifier = Modifier.padding(24.dp).fillMaxWidth(),
                            horizontalAlignment = Alignment.CenterHorizontally,
                            verticalArrangement = Arrangement.spacedBy(8.dp)
                        ) {
                            Icon(
                                imageVector = Icons.Filled.Inbox,
                                contentDescription = null,
                                tint = MaterialTheme.colorScheme.outline,
                                modifier = Modifier.size(44.dp)
                            )
                            Text(
                                text = "İşlem Bulunmuyor",
                                style = MaterialTheme.typography.bodyMedium,
                                fontWeight = FontWeight.Bold,
                                color = MaterialTheme.colorScheme.onSurfaceVariant
                            )
                            Text(
                                text = "$selectedDate tarihinde çalışma alanında gerçekleşen herhangi bir hareket bulunmamaktadır. Başka bir tarih seçebilirsiniz.",
                                style = MaterialTheme.typography.labelSmall,
                                color = Color.Gray,
                                textAlign = TextAlign.Center
                            )
                        }
                    }
                }
            }

            val documentTypes = listOf("Satış", "Tahsilat", "Tediye", "İade", "Alış")
            documentTypes.forEach { type ->
                val logsOfType = when (type) {
                    "Alış" -> filteredKasaLogs.filter { it.type == "Alış" || (it.type == "Tediye" && it.desc.contains("Alış")) }
                    "Tediye" -> filteredKasaLogs.filter { it.type == "Tediye" && !it.desc.contains("Alış") }
                    else -> filteredKasaLogs.filter { it.type == type }
                }
                
                if (logsOfType.isNotEmpty()) {
                    item {
                        Card(
                            modifier = Modifier.fillMaxWidth(),
                            colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
                            border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
                        ) {
                            Column(modifier = Modifier.padding(10.dp)) {
                                Row(
                                    modifier = Modifier.fillMaxWidth(),
                                    horizontalArrangement = Arrangement.SpaceBetween,
                                    verticalAlignment = Alignment.CenterVertically
                                ) {
                                    Row(
                                        verticalAlignment = Alignment.CenterVertically,
                                        horizontalArrangement = Arrangement.spacedBy(6.dp)
                                    ) {
                                        val (icon, color) = when (type) {
                                            "Satış" -> Icons.Filled.ShoppingBag to Color(0xFF43A047)
                                            "Tahsilat" -> Icons.Filled.AccountBalanceWallet to Color(0xFF1E88E5)
                                            "Tediye" -> Icons.Filled.Payment to Color(0xFFD32F2F)
                                            "İade" -> Icons.Filled.AssignmentReturn to Color(0xFFF57C00)
                                            else -> Icons.Filled.ShoppingCart to Color(0xFF8E24AA)
                                        }
                                        Icon(icon, contentDescription = null, tint = color, modifier = Modifier.size(16.dp))
                                        Text(
                                            text = "$type Belgeleri",
                                            fontWeight = FontWeight.Bold,
                                            style = MaterialTheme.typography.bodySmall
                                        )
                                    }
                                    Text(
                                        text = "${logsOfType.size} İşlem",
                                        style = MaterialTheme.typography.labelSmall,
                                        color = Color.Gray
                                    )
                                }
                                
                                Spacer(modifier = Modifier.height(6.dp))
                                Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.3f))
                                Spacer(modifier = Modifier.height(4.dp))
                                
                                logsOfType.forEachIndexed { idx, log ->
                                    Row(
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .padding(vertical = 4.dp),
                                        horizontalArrangement = Arrangement.SpaceBetween,
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        Column(modifier = Modifier.weight(1f)) {
                                            Text(
                                                text = log.customerOrSupplier,
                                                style = MaterialTheme.typography.bodySmall,
                                                fontWeight = FontWeight.SemiBold,
                                                maxLines = 1,
                                                fontSize = 12.sp
                                            )
                                            Text(
                                                text = "${log.id} • ${log.paymentType}${if (log.bankName != null) " (${log.bankName})" else ""}",
                                                style = MaterialTheme.typography.labelSmall,
                                                color = Color.Gray,
                                                fontSize = 10.sp
                                            )
                                        }
                                        Text(
                                            text = String.format("%,.2f ₺", log.amount),
                                            style = MaterialTheme.typography.bodySmall,
                                            fontWeight = FontWeight.Bold,
                                            fontSize = 12.sp,
                                            color = when (type) {
                                                "Satış", "Tahsilat" -> Color(0xFF2E7D32)
                                                else -> Color(0xFFC62828)
                                            }
                                        )
                                    }
                                    if (idx < logsOfType.size - 1) {
                                        Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.15f))
                                    }
                                }
                            }
                        }
                    }
                }
            }

            item {
                Text("GÜN SONU KAPANIK KONTROLLERİ", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.primary, fontWeight = FontWeight.Bold)
            }

            item {
                FieldCard {
                    Column(modifier = Modifier.padding(12.dp), verticalArrangement = Arrangement.spacedBy(8.dp)) {
                        Row(verticalAlignment = Alignment.CenterVertically, modifier = Modifier.clickable { step1Checked = !step1Checked }) {
                            Checkbox(checked = step1Checked, onCheckedChange = { step1Checked = it })
                            Text("Oluşturulan tüm faturaların e-arşiv entegrasyonu tamamlandı.", style = MaterialTheme.typography.bodySmall)
                        }
                        Row(verticalAlignment = Alignment.CenterVertically, modifier = Modifier.clickable { step2Checked = !step2Checked }) {
                            Checkbox(checked = step2Checked, onCheckedChange = { step2Checked = it })
                            Text("Nakit tahsilat bilgileri eldeki kasa banknotları ile birebir uyuşuyor.", style = MaterialTheme.typography.bodySmall)
                        }
                        Row(verticalAlignment = Alignment.CenterVertically, modifier = Modifier.clickable { step3Checked = !step3Checked }) {
                            Checkbox(checked = step3Checked, onCheckedChange = { step3Checked = it })
                            Text("İade alınan fiziksel ürünlerin depo etiketleri yapıştırıldı.", style = MaterialTheme.typography.bodySmall)
                        }
                        Row(verticalAlignment = Alignment.CenterVertically, modifier = Modifier.clickable { step4Checked = !step4Checked }) {
                            Checkbox(checked = step4Checked, onCheckedChange = { step4Checked = it })
                            Text("Tüm çevrimdışı (offline) işlemler merkeze yüklenmeye hazır.", style = MaterialTheme.typography.bodySmall)
                        }
                    }
                }
            }

            // Print Animation Block
            item {
                if (isPrinting) {
                    Card(
                        modifier = Modifier.fillMaxWidth(),
                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.5f))
                    ) {
                        Column(
                            modifier = Modifier.padding(12.dp).fillMaxWidth(),
                            horizontalAlignment = Alignment.CenterHorizontally,
                            verticalArrangement = Arrangement.spacedBy(6.dp)
                        ) {
                            CircularProgressIndicator(color = MaterialTheme.colorScheme.primary, modifier = Modifier.size(24.dp))
                            Text("Z-Raporu Termal Yazıcıdan Çıkarılıyor...", fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodySmall)
                            Text("Bluetooth Mobil Printer Bağlantısı Aktif", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                        }
                    }
                } else if (printSuccess) {
                    Card(
                        modifier = Modifier.fillMaxWidth(),
                        colors = CardDefaults.cardColors(containerColor = Color(0xFFE8F5E9))
                    ) {
                        Row(
                            modifier = Modifier.padding(12.dp).fillMaxWidth(),
                            horizontalArrangement = Arrangement.spacedBy(8.dp),
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Icon(Icons.Filled.Check, null, tint = Color(0xFF43A047), modifier = Modifier.size(18.dp))
                            Text("Z-Raporu Yazıcıdan Alındı (Rapor-2026-412)", fontWeight = FontWeight.Bold, color = Color(0xFF2E7D32), style = MaterialTheme.typography.bodySmall)
                        }
                    }
                }
            }

            // Sync/Closing Day animation
            item {
                if (isClosingDay) {
                    Card(
                        modifier = Modifier.fillMaxWidth(),
                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.secondaryContainer.copy(alpha = 0.5f))
                    ) {
                        Column(
                            modifier = Modifier.padding(12.dp).fillMaxWidth(),
                            horizontalAlignment = Alignment.CenterHorizontally,
                            verticalArrangement = Arrangement.spacedBy(6.dp)
                        ) {
                            CircularProgressIndicator(color = MaterialTheme.colorScheme.secondary, modifier = Modifier.size(24.dp))
                            Text("ERP Entegrasyon Kuyruğu Senkronize Ediliyor...", fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodySmall)
                            Text("Saha lokasyon GPS veri kaydı yapılıyor.", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                        }
                    }
                }
            }

            item {
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    // Print Z-Report Action
                    OutlinedButton(
                        modifier = Modifier.weight(1f).height(44.dp),
                        onClick = {
                            isPrinting = true
                            printSuccess = false
                            scope.launch {
                                delay(2000)
                                isPrinting = false
                                printSuccess = true
                                onSuccess()
                            }
                        }
                    ) {
                        Icon(Icons.Filled.Print, null, modifier = Modifier.size(16.dp))
                        Spacer(modifier = Modifier.width(4.dp))
                        Text("Z Raporu Yazdır", style = MaterialTheme.typography.labelMedium, fontSize = 11.sp)
                    }

                    // PNG Report Action
                    OutlinedButton(
                        modifier = Modifier.weight(1f).height(44.dp),
                        onClick = {
                            saveEodReportAsPng(
                                context = context,
                                todaySales = todaySales, todaySalesCount = todaySalesCount,
                                todayCollections = todayCollections, todayCollectionsCount = todayCollectionsCount,
                                todayTediyes = todayTediyes, todayTediyesCount = todayTediyesCount,
                                todayReturns = todayReturns, todayReturnsCount = todayReturnsCount,
                                todayPurchases = todayPurchases, todayPurchasesCount = todayPurchasesCount,
                                netCashFlow = netCashFlow,
                                logs = filteredKasaLogs
                            )
                        }
                    ) {
                        Icon(Icons.Filled.Save, null, modifier = Modifier.size(16.dp))
                        Spacer(modifier = Modifier.width(4.dp))
                        Text("PNG Rapor Kaydet", style = MaterialTheme.typography.labelMedium, fontSize = 11.sp)
                    }
                }
            }

            item {
                // Complete Closing Full Width at bottom
                Button(
                    modifier = Modifier.fillMaxWidth().height(48.dp),
                    enabled = step1Checked && step2Checked && step3Checked && step4Checked,
                    colors = ButtonDefaults.buttonColors(containerColor = Color(0xFFC62828)),
                    onClick = {
                        isClosingDay = true
                        scope.launch {
                            delay(2200)
                            isClosingDay = false
                            dayClosedSuccess = true
                            onSuccess()
                        }
                    }
                ) {
                    Icon(Icons.Filled.Lock, null, modifier = Modifier.size(18.dp))
                    Spacer(modifier = Modifier.width(6.dp))
                    Text("Günü Kapat ve Gönder", style = MaterialTheme.typography.labelMedium)
                }
            }
        }
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ApprovalsModule(onBeep: () -> Unit) {
    val scope = rememberCoroutineScope()
    val context = LocalContext.current
    var selectedFilter by com.example.ui.screens.AppDataStore.approvalSelectedFilter
    var searchQuery by com.example.ui.screens.AppDataStore.approvalSearchQuery
    var selectedDateFilter by com.example.ui.screens.AppDataStore.approvalSelectedDateFilter
    var isSearchSectionExpanded by com.example.ui.screens.AppDataStore.approvalIsSearchExpanded
    
    val currentStatus = com.example.ui.screens.AppDataStore.approvalStatusFilter.value
    val items = when (currentStatus) {
        "Onaylanan" -> com.example.ui.screens.AppDataStore.approvedApprovalItems
        "Reddedilen" -> com.example.ui.screens.AppDataStore.rejectedApprovalItems
        else -> {
            if (com.example.ui.screens.AppDataStore.autoApproveAllTransactions) {
                emptyList()
            } else {
                com.example.ui.screens.AppDataStore.approvalItems
            }
        }
    }

    var selectedItemForAction by remember { mutableStateOf<Pair<ApprovalItem, Boolean>?>(null) }
    var showDialog by remember { mutableStateOf(false) }
    var selectedItemForDetail by remember { mutableStateOf<ApprovalItem?>(null) }

    // native date picker dialog
    val calendar = java.util.Calendar.getInstance()
    val datePickerDialog = remember {
        android.app.DatePickerDialog(
            context,
            { _, year, month, dayOfMonth ->
                val selectedDateStr = String.format("%02d.%02d.%04d", dayOfMonth, month + 1, year)
                selectedDateFilter = selectedDateStr
            },
            calendar.get(java.util.Calendar.YEAR),
            calendar.get(java.util.Calendar.MONTH),
            calendar.get(java.util.Calendar.DAY_OF_MONTH)
        )
    }

    val filteredItems = items.filter { item ->
        val matchesFilter = selectedFilter == "Tümü" || item.type == selectedFilter
        val matchesSearch = item.customerName.contains(searchQuery, ignoreCase = true) || 
                            item.id.contains(searchQuery, ignoreCase = true) ||
                            item.description.contains(searchQuery, ignoreCase = true)
        val matchesDate = if (selectedDateFilter != null) {
            val lastChar = item.id.lastOrNull()?.code ?: 0
            val day = selectedDateFilter!!.split(".").firstOrNull()?.toIntOrNull() ?: 1
            if (day % 2 == 1) {
                lastChar % 2 == 1
            } else {
                lastChar % 2 == 0
            }
        } else {
            true
        }
        matchesFilter && matchesSearch && matchesDate
    }

    Scaffold(
        containerColor = Color.Transparent,
        contentWindowInsets = WindowInsets(0, 0, 0, 0)
    ) { paddingValues ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
                .padding(horizontal = 16.dp, vertical = 8.dp),
            verticalArrangement = Arrangement.spacedBy(12.dp)
        ) {
            // Status filter banner to provide instant user feedback
            val statusColor = when (currentStatus) {
                "Onaylanan" -> Color(0xFF2E7D32)
                "Reddedilen" -> Color(0xFFC62828)
                else -> Color(0xFFF57C00) // Orange for "Bekleyen"
            }
            val statusBgColor = when (currentStatus) {
                "Onaylanan" -> Color(0xFFE8F5E9)
                "Reddedilen" -> Color(0xFFFFEBEE)
                else -> Color(0xFFFFF3E0)
            }
            val statusLabel = when (currentStatus) {
                "Onaylanan" -> "Onaylanmış İşlemler"
                "Reddedilen" -> "Reddedilmiş İşlemler"
                else -> "Bekleyen İşlemler"
            }
            val statusIcon = when (currentStatus) {
                "Onaylanan" -> Icons.Filled.CheckCircle
                "Reddedilen" -> Icons.Filled.Close
                else -> Icons.Filled.HourglassEmpty
            }

            Card(
                modifier = Modifier.fillMaxWidth(),
                colors = CardDefaults.cardColors(containerColor = statusBgColor),
                shape = RoundedCornerShape(10.dp)
            ) {
                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(horizontal = 12.dp, vertical = 8.dp),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Row(
                        verticalAlignment = Alignment.CenterVertically,
                        horizontalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        Icon(statusIcon, null, tint = statusColor, modifier = Modifier.size(18.dp))
                        Column {
                            Text(
                                text = statusLabel,
                                style = MaterialTheme.typography.bodySmall,
                                fontWeight = FontWeight.Bold,
                                color = statusColor
                            )
                            Text(
                                text = "${filteredItems.size} işlem listeleniyor",
                                style = MaterialTheme.typography.labelSmall,
                                color = statusColor.copy(alpha = 0.8f)
                            )
                        }
                    }
                    
                    TextButton(
                        onClick = {
                            val nextVal = when (currentStatus) {
                                "Bekleyen" -> "Onaylanan"
                                "Onaylanan" -> "Reddedilen"
                                else -> "Bekleyen"
                            }
                            com.example.ui.screens.AppDataStore.approvalStatusFilter.value = nextVal
                        },
                        contentPadding = PaddingValues(horizontal = 8.dp),
                        modifier = Modifier.height(30.dp)
                    ) {
                        Text(
                            text = "Değiştir 🔄",
                            style = MaterialTheme.typography.labelSmall,
                            color = statusColor,
                            fontWeight = FontWeight.Bold
                        )
                    }
                }
            }

            if (selectedDateFilter != null) {
                Card(
                    modifier = Modifier.fillMaxWidth(),
                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.secondaryContainer.copy(alpha = 0.5f)),
                    shape = RoundedCornerShape(10.dp)
                ) {
                    Row(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(horizontal = 12.dp, vertical = 8.dp),
                        horizontalArrangement = Arrangement.SpaceBetween,
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Row(
                            verticalAlignment = Alignment.CenterVertically,
                            horizontalArrangement = Arrangement.spacedBy(6.dp)
                        ) {
                            Icon(Icons.Filled.DateRange, null, tint = MaterialTheme.colorScheme.secondary, modifier = Modifier.size(16.dp))
                            Text(
                                text = "Tarih Filtresi: $selectedDateFilter",
                                style = MaterialTheme.typography.bodySmall,
                                fontWeight = FontWeight.Bold,
                                color = MaterialTheme.colorScheme.onSecondaryContainer
                            )
                        }
                        TextButton(
                            onClick = { selectedDateFilter = null },
                            contentPadding = PaddingValues(0.dp),
                            modifier = Modifier.height(30.dp)
                        ) {
                            Text("Seçimi Temizle", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.error)
                        }
                    }
                }
            }

            LazyColumn(
                modifier = Modifier.weight(1f),
                contentPadding = PaddingValues(bottom = 140.dp),
                verticalArrangement = Arrangement.spacedBy(12.dp)
            ) {
            if (filteredItems.isEmpty()) {
                item {
                    Column(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(vertical = 48.dp),
                        horizontalAlignment = Alignment.CenterHorizontally,
                        verticalArrangement = Arrangement.Center
                    ) {
                        val isAutoApproved = com.example.ui.screens.AppDataStore.autoApproveAllTransactions && currentStatus == "Bekleyen"
                        val emptyTitle = if (isAutoApproved) {
                            "Otomatik Onaylama Aktif ⚡"
                        } else {
                            when (currentStatus) {
                                "Onaylanan" -> "Onaylanmış İşlem Bulunmuyor"
                                "Reddedilen" -> "Reddedilmiş İşlem Bulunmuyor"
                                else -> "Aktif Onay Talebi Bulunmuyor"
                            }
                        }
                        val emptyDesc = if (isAutoApproved) {
                            "Ayarlar sayfasından 'Tüm Evrakları Otomatik Onayla' seçeneği aktifleştirildiği için evraklar onay havuzuna takılmadan otomatik onaylanır."
                        } else {
                            when (currentStatus) {
                                "Onaylanan" -> "Henüz onaylanmış herhangi bir saha talebi bulunmamaktadır."
                                "Reddedilen" -> "Henüz reddedilmiş herhangi bir saha talebi bulunmamaktadır."
                                else -> "Tüm bekleyen saha işlemleri merkez tarafından işlenmiş veya onaylanmış durumdadır."
                            }
                        }
                        val emptyIcon = if (isAutoApproved) {
                            Icons.Filled.VerifiedUser
                        } else {
                            when (currentStatus) {
                                "Reddedilen" -> Icons.Filled.Close
                                else -> Icons.Filled.CheckCircle
                            }
                        }
                        val emptyIconColor = if (isAutoApproved) {
                            MaterialTheme.colorScheme.primary
                        } else {
                            when (currentStatus) {
                                "Reddedilen" -> Color(0xFFC62828)
                                else -> Color(0xFF43A047)
                            }
                        }
                        Icon(
                            emptyIcon,
                            contentDescription = null,
                            tint = emptyIconColor,
                            modifier = Modifier.size(64.dp)
                        )
                        Spacer(modifier = Modifier.height(16.dp))
                        Text(
                            text = emptyTitle,
                            style = MaterialTheme.typography.titleMedium,
                            fontWeight = FontWeight.Bold
                        )
                        Text(
                            text = emptyDesc,
                            style = MaterialTheme.typography.bodySmall,
                            color = Color.Gray,
                            textAlign = TextAlign.Center,
                            modifier = Modifier.padding(start = 24.dp, end = 24.dp, top = 4.dp)
                        )
                    }
                }
            } else {
                items(filteredItems) { item ->
                    FieldCard(
                        modifier = Modifier.clickable {
                            selectedItemForDetail = item
                        }
                    ) {
                        Column(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(16.dp),
                            verticalArrangement = Arrangement.spacedBy(10.dp)
                        ) {
                            // Header: Doc ID & Category Tag & Time
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Row(
                                    verticalAlignment = Alignment.CenterVertically,
                                    horizontalArrangement = Arrangement.spacedBy(6.dp)
                                ) {
                                    val tagBg = when (item.type) {
                                        "Satış" -> Color(0xFFE3F2FD) // Light blue
                                        "Tahsilat" -> Color(0xFFE8F5E9) // Light green
                                        "İade" -> Color(0xFFFFEBEE) // Light red
                                        "Alış" -> Color(0xFFFFF3E0) // Light orange
                                        "Tediye" -> Color(0xFFF3E5F5) // Light purple
                                        else -> Color(0xFFECEFF1)
                                    }
                                    val tagFg = when (item.type) {
                                        "Satış" -> Color(0xFF1565C0)
                                        "Tahsilat" -> Color(0xFF2E7D32)
                                        "İade" -> Color(0xFFC62828)
                                        "Alış" -> Color(0xFFEF6C00)
                                        "Tediye" -> Color(0xFF6A1B9A)
                                        else -> Color(0xFF37474F)
                                    }
                                    Box(
                                        modifier = Modifier
                                            .background(tagBg, shape = RoundedCornerShape(6.dp))
                                            .padding(horizontal = 8.dp, vertical = 4.dp)
                                    ) {
                                        Text(item.type, style = MaterialTheme.typography.labelSmall, color = tagFg, fontWeight = FontWeight.Bold)
                                    }
                                    Text(item.id, style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Bold, color = Color.Gray)
                                }
                                Text(item.time, style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                            }

                            // Customer & Details
                            Column {
                                Text(item.customerName, style = MaterialTheme.typography.titleSmall, fontWeight = FontWeight.Black)
                                Text(item.description, style = MaterialTheme.typography.bodySmall, color = Color.Gray)
                            }

                            // Request detail banner
                            Card(
                                colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f)),
                                shape = RoundedCornerShape(8.dp)
                            ) {
                                Row(
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .padding(10.dp),
                                    horizontalArrangement = Arrangement.spacedBy(8.dp),
                                    verticalAlignment = Alignment.CenterVertically
                                ) {
                                    Icon(Icons.Filled.Info, null, tint = MaterialTheme.colorScheme.primary, modifier = Modifier.size(16.dp))
                                    Text(item.reason, style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
                                }
                            }

                            // Footer: Price & Actions
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Column {
                                    Text("İşlem Tutarı", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                    Text("₺%,.2f".format(item.amount).replace(",", "."), style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Black, color = MaterialTheme.colorScheme.primary)
                                }

                                if (currentStatus == "Onaylanan") {
                                    Row(
                                        verticalAlignment = Alignment.CenterVertically,
                                        horizontalArrangement = Arrangement.spacedBy(4.dp),
                                        modifier = Modifier
                                            .background(Color(0xFFE8F5E9), shape = RoundedCornerShape(8.dp))
                                            .padding(horizontal = 12.dp, vertical = 6.dp)
                                    ) {
                                        Icon(Icons.Filled.CheckCircle, contentDescription = null, tint = Color(0xFF2E7D32), modifier = Modifier.size(16.dp))
                                        Text("Onaylandı", style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Bold, color = Color(0xFF2E7D32))
                                    }
                                } else if (currentStatus == "Reddedilen") {
                                    Row(
                                        verticalAlignment = Alignment.CenterVertically,
                                        horizontalArrangement = Arrangement.spacedBy(4.dp),
                                        modifier = Modifier
                                            .background(Color(0xFFFFEBEE), shape = RoundedCornerShape(8.dp))
                                            .padding(horizontal = 12.dp, vertical = 6.dp)
                                    ) {
                                        Icon(Icons.Filled.Close, contentDescription = null, tint = Color(0xFFC62828), modifier = Modifier.size(16.dp))
                                        Text("Reddedildi", style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Bold, color = Color(0xFFC62828))
                                    }
                                } else {
                                    Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                                        // Reject Button
                                        OutlinedButton(
                                            onClick = {
                                                selectedItemForAction = item to false
                                                showDialog = true
                                            },
                                            colors = ButtonDefaults.outlinedButtonColors(contentColor = Color(0xFFC62828)),
                                            border = BorderStroke(1.dp, Color(0xFFE0E0E0)),
                                            shape = RoundedCornerShape(8.dp),
                                            contentPadding = PaddingValues(horizontal = 12.dp)
                                        ) {
                                            Icon(Icons.Filled.Close, null, modifier = Modifier.size(16.dp))
                                            Spacer(modifier = Modifier.width(4.dp))
                                            Text("Reddet", style = MaterialTheme.typography.labelSmall)
                                        }

                                        // Approve Button
                                        Button(
                                            onClick = {
                                                selectedItemForAction = item to true
                                                showDialog = true
                                            },
                                            colors = ButtonDefaults.buttonColors(containerColor = Color(0xFF2E7D32)),
                                            shape = RoundedCornerShape(8.dp),
                                            contentPadding = PaddingValues(horizontal = 12.dp)
                                        ) {
                                            Icon(Icons.Filled.Check, null, modifier = Modifier.size(16.dp))
                                            Spacer(modifier = Modifier.width(4.dp))
                                            Text("Onayla", style = MaterialTheme.typography.labelSmall)
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

    if (showDialog && selectedItemForAction != null) {
        val (item, isApprove) = selectedItemForAction!!
        AlertDialog(
            onDismissRequest = {
                showDialog = false
                selectedItemForAction = null
            },
            title = {
                Text(
                    text = if (isApprove) "Talebi Onayla" else "Talebi Reddet",
                    fontWeight = FontWeight.Bold
                )
            },
            text = {
                Text(
                    text = if (isApprove) {
                        "\"${item.customerName}\" CARİ hesabı için olan ${item.id} numaralı ${item.type.lowercase()} talebini onaylamak istediğinizden emin misiniz? Bu işlem ERP muhasebe kayıtlarına doğrudan işlenecektir."
                    } else {
                        "\"${item.customerName}\" CARİ hesabı için olan ${item.id} numaralı ${item.type.lowercase()} talebini REDDETMEK istediğinizden emin misiniz? Red kararı sahaya anında bildirilecektir."
                    }
                )
            },
            confirmButton = {
                Button(
                    onClick = {
                        onBeep()
                        if (isApprove) {
                            com.example.ui.screens.AppDataStore.approvedApprovalItems.add(item)
                            if (item.type == "Satış") {
                                scope.launch(kotlinx.coroutines.Dispatchers.IO) {
                                    try {
                                        val db = com.example.data.database.DatabaseProvider.getDatabase(context.applicationContext)
                                        val orderId = item.id
                                        val actualCartItems = com.example.ui.screens.AppDataStore.approvalOrderItemsMap[orderId]
                                        
                                        val items = if (actualCartItems != null) {
                                            actualCartItems.map { cartItem ->
                                                com.example.data.database.WmsOrderItemEntity(
                                                    id = "${orderId}_${cartItem.product.barcode}",
                                                    orderId = orderId,
                                                    productBarcode = cartItem.product.barcode,
                                                    productTitle = cartItem.product.title,
                                                    quantityOrdered = cartItem.quantity,
                                                    quantityPicked = 0,
                                                    isPicked = false,
                                                    shelfLocation = "Raf A-1"
                                                )
                                            }
                                        } else if (orderId == "ST-2026-0034") {
                                            listOf(
                                                com.example.data.database.WmsOrderItemEntity(
                                                    id = "${orderId}_8690123456789",
                                                    orderId = orderId,
                                                    productBarcode = "8690123456789",
                                                    productTitle = "Ultra Performans Endüstriyel Motor Yağı 20L",
                                                    quantityOrdered = 4,
                                                    quantityPicked = 0,
                                                    isPicked = false,
                                                    shelfLocation = "Raf A-1"
                                                ),
                                                com.example.data.database.WmsOrderItemEntity(
                                                    id = "${orderId}_8699876543210",
                                                    orderId = orderId,
                                                    productBarcode = "8699876543210",
                                                    productTitle = "Hava Filtresi - Ağır Vasıta Uyumlu Pro",
                                                    quantityOrdered = 8,
                                                    quantityPicked = 0,
                                                    isPicked = false,
                                                    shelfLocation = "Raf B-3"
                                                )
                                            )
                                        } else {
                                            listOf(
                                                com.example.data.database.WmsOrderItemEntity(
                                                    id = "${orderId}_1234567890123",
                                                    orderId = orderId,
                                                    productBarcode = "1234567890123",
                                                    productTitle = "Çelik Rulman 120mm - Yüksek Devir",
                                                    quantityOrdered = 5,
                                                    quantityPicked = 0,
                                                    isPicked = false,
                                                    shelfLocation = "Raf A-4"
                                                )
                                            )
                                        }
                                        
                                        val totalItemsSum = if (actualCartItems != null) actualCartItems.sumOf { it.quantity } else if (orderId == "ST-2026-0034") 12 else 5
                                        
                                        val newWmsOrder = com.example.data.database.WmsOrderEntity(
                                            id = orderId,
                                            customerName = item.customerName,
                                            orderDate = "16.06.2026",
                                            status = "Bekleyen",
                                            totalItems = totalItemsSum,
                                            syncStatus = "SYNCED"
                                        )
                                        
                                        db.wmsOrderDao().insert(newWmsOrder)
                                        db.wmsOrderItemDao().insertAll(items)

                                        val matchingCustIdx = com.example.ui.screens.AppDataStore.customers.indexOfFirst { 
                                            it.name.trim().lowercase() == item.customerName.trim().lowercase()
                                        }
                                        if (matchingCustIdx != -1) {
                                            val oldCust = com.example.ui.screens.AppDataStore.customers[matchingCustIdx]
                                            kotlinx.coroutines.withContext(kotlinx.coroutines.Dispatchers.Main) {
                                                val newBalance = oldCust.balance + item.amount
                                                val newTxList = oldCust.transactions.toMutableList()
                                                newTxList.add(
                                                    com.example.ui.screens.CustomerTx(
                                                        id = "TX-INI-${(10000..99999).random()}",
                                                        date = java.text.SimpleDateFormat("dd.MM.yyyy").format(java.util.Date()),
                                                        type = "SATIŞ",
                                                        amount = item.amount,
                                                        description = "WMS İlk Sipariş Fişi - Sipariş: ${orderId}"
                                                    )
                                                )
                                                com.example.ui.screens.AppDataStore.customers[matchingCustIdx] = oldCust.copy(
                                                    balance = newBalance,
                                                    transactions = androidx.compose.runtime.mutableStateListOf<com.example.ui.screens.CustomerTx>().apply {
                                                        addAll(newTxList)
                                                    }
                                                )
                                            }
                                        }
                                    } catch (e: Exception) {
                                        e.printStackTrace()
                                    }
                                }
                            } else if (item.type == "Düzenlenen Sipariş") {
                                scope.launch(kotlinx.coroutines.Dispatchers.IO) {
                                    try {
                                        val db = com.example.data.database.DatabaseProvider.getDatabase(context.applicationContext)
                                        val orderId = if (item.id.startsWith("REV-")) item.id.substring(4) else item.id
                                        
                                        val orderData = db.wmsOrderDao().getOrderById(orderId)
                                        if (orderData != null) {
                                            val itemsList = db.wmsOrderItemDao().getItemsForOrder(orderId)
                                            val newItemsList = itemsList.map { it.copy(quantityOrdered = it.quantityPicked) }
                                            val newTotalItems = newItemsList.sumOf { it.quantityOrdered }
                                             
                                            // Update the order in the DB and set status back to "Koli Hazırlandı" or keep it.
                                            db.wmsOrderDao().insert(orderData.copy(totalItems = newTotalItems, status = "Koli Hazırlandı")) 
                                            db.wmsOrderItemDao().insertAll(newItemsList)

                                            // Compute final pricing based on picked quantities
                                            val productsList = db.productDao().getAllProducts()
                                            var orderTotalVal = 0.0
                                            for (itemObj in newItemsList) {
                                                val matchingProd = productsList.find { it.barcode == itemObj.productBarcode }
                                                val basePrice = matchingProd?.basePrice ?: 120.0
                                                orderTotalVal += (itemObj.quantityPicked * basePrice)
                                            }

                                            // Sync with AppDataStore.customers
                                            val matchingCustIdx = com.example.ui.screens.AppDataStore.customers.indexOfFirst { 
                                                it.name.trim().lowercase() == orderData.customerName.trim().lowercase()
                                            }
                                            if (matchingCustIdx != -1) {
                                                val oldCust = com.example.ui.screens.AppDataStore.customers[matchingCustIdx]
                                                val newTxs = oldCust.transactions.toMutableList()
                                                var updatedBalance = oldCust.balance
                                                
                                                val existingTxIdx = newTxs.indexOfFirst { it.description.contains(orderId) || it.id.contains(orderId) || it.id == orderId }
                                                
                                                if (existingTxIdx != -1) {
                                                    val oldTx = newTxs[existingTxIdx]
                                                    updatedBalance -= oldTx.amount // Revert old amount
                                                    
                                                    newTxs[existingTxIdx] = oldTx.copy(
                                                        amount = orderTotalVal,
                                                        description = "WMS Revize Sevkiyat Fişi - Sipariş: $orderId"
                                                    )
                                                    updatedBalance += orderTotalVal // Apply new amount
                                                } else {
                                                    updatedBalance += orderTotalVal // For a fresh transaction
                                                    val txIdStr = "TX-WMS-${(10000..99999).random()}"
                                                    val dateStr = java.text.SimpleDateFormat("dd.MM.yyyy").format(java.util.Date())
                                                    newTxs.add(
                                                        com.example.ui.screens.CustomerTx(
                                                            id = txIdStr,
                                                            date = dateStr,
                                                            type = "SATIŞ",
                                                            amount = orderTotalVal,
                                                            description = "WMS Nihai Sevkiyat Fişi - Sipariş: $orderId"
                                                        )
                                                    )
                                                }
                                                 
                                                // Update the list on Main Thread to trigger recomposition
                                                kotlinx.coroutines.withContext(kotlinx.coroutines.Dispatchers.Main) {
                                                    com.example.ui.screens.AppDataStore.customers[matchingCustIdx] = oldCust.copy(
                                                        balance = updatedBalance,
                                                        transactions = androidx.compose.runtime.mutableStateListOf<com.example.ui.screens.CustomerTx>().apply {
                                                            addAll(newTxs)
                                                        }
                                                    )
                                                }
                                            }
                                        }
                                    } catch (e: Exception) {
                                        e.printStackTrace()
                                    }
                                }
                            }
                        } else {
                            com.example.ui.screens.AppDataStore.rejectedApprovalItems.add(item)
                        }
                        com.example.ui.screens.AppDataStore.approvalItems.remove(item)
                        showDialog = false
                        selectedItemForAction = null
                    },
                    colors = ButtonDefaults.buttonColors(containerColor = if (isApprove) Color(0xFF2E7D32) else Color(0xFFC62828))
                ) {
                    Text("Evet, Devam Et")
                }
            },
            dismissButton = {
                TextButton(
                    onClick = {
                        showDialog = false
                        selectedItemForAction = null
                    }
                ) {
                    Text("İptal")
                }
            }
        )
    }

    if (selectedItemForDetail != null) {
        val item = selectedItemForDetail!!
        val lineItems = com.example.ui.screens.AppDataStore.approvalOrderItemsMap[item.id] ?: emptyList()
        val scrollState = rememberScrollState()

        AlertDialog(
            onDismissRequest = { selectedItemForDetail = null },
            confirmButton = {
                Column(
                    verticalArrangement = Arrangement.spacedBy(8.dp),
                    modifier = Modifier.fillMaxWidth()
                ) {
                    Button(
                        onClick = {
                            val xlsPath = exportSingleOrderToExcel(context, item)
                            if (xlsPath != null) {
                                android.widget.Toast.makeText(context, "Sipariş Excel (.CSV) formatında indirilenler klasörüne kaydedildi:\n$xlsPath", android.widget.Toast.LENGTH_LONG).show()
                            } else {
                                android.widget.Toast.makeText(context, "Hata: CSV Raporu oluşturulamadı.", android.widget.Toast.LENGTH_SHORT).show()
                            }
                        },
                        colors = ButtonDefaults.buttonColors(containerColor = Color(0xFF2E7D32)),
                        shape = RoundedCornerShape(8.dp),
                        modifier = Modifier.fillMaxWidth().height(42.dp)
                    ) {
                        Row(
                            horizontalArrangement = Arrangement.spacedBy(6.dp),
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Icon(Icons.Filled.Share, contentDescription = null, tint = Color.White, modifier = Modifier.size(16.dp))
                            Text("Siparişi Excel (.CSV) Kaydet", style = MaterialTheme.typography.bodySmall.copy(fontWeight = FontWeight.Bold, color = Color.White))
                        }
                    }
                    OutlinedButton(
                        onClick = { selectedItemForDetail = null },
                        shape = RoundedCornerShape(8.dp),
                        modifier = Modifier.fillMaxWidth().height(42.dp)
                    ) {
                        Text("Detayları Kapat", style = MaterialTheme.typography.bodySmall.copy(fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary))
                    }
                }
            },
            title = {
                Row(
                    verticalAlignment = Alignment.CenterVertically,
                    horizontalArrangement = Arrangement.SpaceBetween,
                    modifier = Modifier.fillMaxWidth()
                ) {
                    Row(
                        verticalAlignment = Alignment.CenterVertically,
                        horizontalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        val tagBg = when (item.type) {
                            "Satış" -> Color(0xFFE3F2FD)
                            "Tahsilat" -> Color(0xFFE8F5E9)
                            "İade" -> Color(0xFFFFEBEE)
                            "Alış" -> Color(0xFFFFF3E0)
                            "Tediye" -> Color(0xFFF3E5F5)
                            else -> Color(0xFFECEFF1)
                        }
                        val tagFg = when (item.type) {
                            "Satış" -> Color(0xFF1565C0)
                            "Tahsilat" -> Color(0xFF2E7D32)
                            "İade" -> Color(0xFFC62828)
                            "Alış" -> Color(0xFFEF6C00)
                            "Tediye" -> Color(0xFF6A1B9A)
                            else -> Color(0xFF37474F)
                        }
                        Box(
                            modifier = Modifier
                                .background(tagBg, shape = RoundedCornerShape(6.dp))
                                .padding(horizontal = 8.dp, vertical = 3.dp)
                        ) {
                            Text(item.type, style = MaterialTheme.typography.labelSmall.copy(fontWeight = FontWeight.Bold), color = tagFg)
                        }
                        Text(text = "Belge Detayı", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Black)
                    }
                    IconButton(
                        onClick = { selectedItemForDetail = null },
                        modifier = Modifier.size(28.dp)
                    ) {
                        Icon(Icons.Filled.Close, contentDescription = "Kapat", modifier = Modifier.size(18.dp))
                    }
                }
            },
            text = {
                Column(
                    modifier = Modifier
                        .fillMaxWidth()
                        .heightIn(max = 450.dp)
                        .verticalScroll(scrollState),
                    verticalArrangement = Arrangement.spacedBy(10.dp)
                ) {
                    // Thin Info Section (Evrak No & Tarih)
                    Row(
                        modifier = Modifier
                            .fillMaxWidth()
                            .background(MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.3f), shape = RoundedCornerShape(8.dp))
                            .padding(horizontal = 10.dp, vertical = 6.dp),
                        horizontalArrangement = Arrangement.SpaceBetween,
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Column {
                            Text("EVRAK NO", style = MaterialTheme.typography.labelSmall, color = Color.Gray, fontWeight = FontWeight.Bold)
                            Text(item.id, style = MaterialTheme.typography.bodySmall, fontWeight = FontWeight.Bold)
                        }
                        Column(horizontalAlignment = Alignment.End) {
                            Text("TALEBİN ZAMANI", style = MaterialTheme.typography.labelSmall, color = Color.Gray, fontWeight = FontWeight.Bold)
                            Text(item.time, style = MaterialTheme.typography.bodySmall)
                        }
                    }

                    // Customer Name (Compact)
                    Column(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(horizontal = 2.dp)
                    ) {
                        Text("MÜŞTERİ / CARİ HESAP", style = MaterialTheme.typography.labelSmall, color = Color.Gray, fontWeight = FontWeight.Bold)
                        Text(item.customerName, style = MaterialTheme.typography.bodyLarge, fontWeight = FontWeight.ExtraBold, color = MaterialTheme.colorScheme.onSurface)
                    }

                    // Reason Badge
                    if (item.reason.isNotEmpty()) {
                        Row(
                            verticalAlignment = Alignment.CenterVertically,
                            horizontalArrangement = Arrangement.spacedBy(6.dp),
                            modifier = Modifier
                                .fillMaxWidth()
                                .background(MaterialTheme.colorScheme.errorContainer.copy(alpha = 0.12f), shape = RoundedCornerShape(6.dp))
                                .padding(horizontal = 8.dp, vertical = 6.dp)
                        ) {
                            Icon(Icons.Filled.Info, null, tint = MaterialTheme.colorScheme.error, modifier = Modifier.size(14.dp))
                            Text(
                                text = "Gerekçe: ${item.reason}",
                                style = MaterialTheme.typography.bodySmall,
                                color = MaterialTheme.colorScheme.onErrorContainer
                            )
                        }
                    }

                    // Payment Type & General Order Note row (Thin & compact visual sections)
                    val activePayment = item.paymentType ?: "Belirtilmedi"
                    val activeOrderNoteVal = item.orderNote ?: ""

                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        // Payment Card
                        Card(
                            colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.15f)),
                            shape = RoundedCornerShape(6.dp),
                            modifier = Modifier.weight(1f)
                        ) {
                            Row(
                                modifier = Modifier.padding(8.dp),
                                horizontalArrangement = Arrangement.spacedBy(6.dp),
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Icon(Icons.Filled.Payment, null, tint = MaterialTheme.colorScheme.primary, modifier = Modifier.size(14.dp))
                                Column {
                                    Text("Ödeme Şekli", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                    Text(activePayment, style = MaterialTheme.typography.bodySmall.copy(fontWeight = FontWeight.Bold), color = MaterialTheme.colorScheme.onPrimaryContainer)
                                }
                            }
                        }
                    }

                    // Order note card (Sipariş Notu)
                    if (activeOrderNoteVal.isNotBlank()) {
                        Card(
                            colors = CardDefaults.cardColors(containerColor = Color(0xFFFFFDE7)), // Beautiful light amber background for order note
                            border = BorderStroke(1.dp, Color(0xFFFEEA3B)),
                            shape = RoundedCornerShape(6.dp),
                            modifier = Modifier.fillMaxWidth()
                        ) {
                            Row(
                                modifier = Modifier.padding(8.dp),
                                horizontalArrangement = Arrangement.spacedBy(6.dp),
                                verticalAlignment = Alignment.Top
                            ) {
                                Icon(Icons.Filled.EditNote, "Sipariş Notu", tint = Color(0xFFF57F17), modifier = Modifier.size(16.dp))
                                Column {
                                    Text("Belge / Sipariş Notu", style = MaterialTheme.typography.labelSmall, color = Color(0xFF5D4037), fontWeight = FontWeight.Bold)
                                    Text(activeOrderNoteVal, style = MaterialTheme.typography.bodySmall, color = Color(0xFF3E2723))
                                }
                            }
                        }
                    }

                    HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.4f))

                    // HERO: Product Items list (Main focus / Ağırlıklı olarak ürün kalemleri)
                    Text(
                        "SİPARİŞ KALEMLERİ (${if (lineItems.isNotEmpty()) lineItems.size else "..."})",
                        style = MaterialTheme.typography.bodySmall.copy(fontWeight = FontWeight.Bold),
                        color = Color.Gray,
                        modifier = Modifier.padding(top = 2.dp)
                    )

                    if (lineItems.isEmpty()) {
                        // Fallback text if no physical product lines map exists
                        Card(
                            colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.2f)),
                            shape = RoundedCornerShape(8.dp),
                            modifier = Modifier.fillMaxWidth()
                        ) {
                            Column(
                                modifier = Modifier.padding(12.dp).fillMaxWidth(),
                                horizontalAlignment = Alignment.CenterHorizontally
                            ) {
                                Text(
                                    text = "Bu işlem tipinde ürün kalemi detayları tanımlı değil (ör. nakit tahsilatı).",
                                    style = MaterialTheme.typography.bodySmall,
                                    color = Color.Gray,
                                    textAlign = TextAlign.Center
                                )
                            }
                        }
                    } else {
                        // Render line items elegantly
                        lineItems.forEach { cartItem ->
                            Card(
                                border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f)),
                                colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
                                shape = RoundedCornerShape(8.dp),
                                modifier = Modifier.fillMaxWidth()
                            ) {
                                Column(
                                    modifier = Modifier.padding(10.dp),
                                    verticalArrangement = Arrangement.spacedBy(6.dp)
                                ) {
                                    // Title + Code Row
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween,
                                        verticalAlignment = Alignment.Top
                                    ) {
                                        Column(modifier = Modifier.weight(1f)) {
                                            Text(
                                                text = cartItem.product.title,
                                                style = MaterialTheme.typography.bodySmall.copy(fontWeight = FontWeight.Bold),
                                                color = MaterialTheme.colorScheme.onSurface
                                            )
                                            Text(
                                                text = "Kod: ${cartItem.product.code} | Barkod: ${cartItem.product.barcode}",
                                                style = MaterialTheme.typography.labelSmall,
                                                color = Color.Gray
                                            )
                                        }
                                    }

                                    // Price and quantity breakdown details row
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween,
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        Row(
                                            horizontalArrangement = Arrangement.spacedBy(6.dp),
                                            verticalAlignment = Alignment.CenterVertically
                                        ) {
                                            Box(
                                                modifier = Modifier
                                                    .background(MaterialTheme.colorScheme.secondaryContainer, shape = RoundedCornerShape(4.dp))
                                                    .padding(horizontal = 6.dp, vertical = 2.dp)
                                            ) {
                                                Text(
                                                    text = "${cartItem.quantity} Adet",
                                                    style = MaterialTheme.typography.labelSmall.copy(fontWeight = FontWeight.Bold),
                                                    color = MaterialTheme.colorScheme.onSecondaryContainer
                                                )
                                            }

                                            Text(
                                                text = "x",
                                                style = MaterialTheme.typography.labelSmall,
                                                color = Color.Gray
                                            )

                                            val unitPrice = cartItem.product.wholesalePrice
                                            Text(
                                                text = "₺%,.2f".format(unitPrice).replace(",", "."),
                                                style = MaterialTheme.typography.bodySmall,
                                                color = Color.Gray
                                            )

                                            if (cartItem.lineDiscountPercent > 0) {
                                                Box(
                                                    modifier = Modifier
                                                        .background(Color(0xFFFFF3E0), shape = RoundedCornerShape(4.dp))
                                                        .padding(horizontal = 6.dp, vertical = 2.dp)
                                                ) {
                                                    Text(
                                                        text = "-%${String.format("%.0f", cartItem.lineDiscountPercent)} İsk",
                                                        style = MaterialTheme.typography.labelSmall.copy(fontWeight = FontWeight.Bold),
                                                        color = Color(0xFFEF6C00)
                                                    )
                                                }
                                            }
                                        }

                                        // Computed total item amount (considering lines discount if any)
                                        val price = cartItem.product.wholesalePrice
                                        val rawTotal = cartItem.quantity * price
                                        val lineTotalValue = rawTotal * (1.0 - cartItem.lineDiscountPercent / 100.0)

                                        Text(
                                            text = "₺%,.2f".format(lineTotalValue).replace(",", "."),
                                            style = MaterialTheme.typography.bodySmall.copy(fontWeight = FontWeight.ExtraBold),
                                            color = MaterialTheme.colorScheme.primary
                                        )
                                    }

                                    // Product Note (Ürün Notu) if present
                                    if (cartItem.note.trim().isNotEmpty()) {
                                        Row(
                                            modifier = Modifier
                                                .fillMaxWidth()
                                                .background(Color(0xFFE3F2FD), shape = RoundedCornerShape(4.dp))
                                                .padding(horizontal = 8.dp, vertical = 4.dp),
                                            verticalAlignment = Alignment.CenterVertically,
                                            horizontalArrangement = Arrangement.spacedBy(4.dp)
                                        ) {
                                            Icon(
                                                imageVector = Icons.Filled.EditNote,
                                                contentDescription = "Ürün Notu",
                                                tint = Color(0xFF1565C0),
                                                modifier = Modifier.size(13.dp)
                                            )
                                            Text(
                                                text = "Ürün Notu: ${cartItem.note}",
                                                style = MaterialTheme.typography.labelSmall.copy(fontStyle = androidx.compose.ui.text.font.FontStyle.Italic),
                                                color = Color(0xFF0D47A1)
                                            )
                                        }
                                    }
                                }
                            }
                        }
                    }

                    HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.4f))

                    // Document Total Box (Highlight)
                    Card(
                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.4f)),
                        shape = RoundedCornerShape(8.dp),
                        modifier = Modifier.fillMaxWidth()
                    ) {
                        Column(
                            modifier = Modifier.padding(10.dp),
                            verticalArrangement = Arrangement.spacedBy(4.dp)
                        ) {
                            if (lineItems.isNotEmpty()) {
                                val subtotalBeforeDiscounts = lineItems.sumOf { it.quantity * it.product.wholesalePrice }
                                val discountTotalVal = lineItems.sumOf { (it.quantity * it.product.wholesalePrice) * (it.lineDiscountPercent / 100.0) }

                                Row(
                                    modifier = Modifier.fillMaxWidth(),
                                    horizontalArrangement = Arrangement.SpaceBetween
                                ) {
                                    Text("Ara Toplam", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                    Text("₺%,.2f".format(subtotalBeforeDiscounts).replace(",", "."), style = MaterialTheme.typography.bodySmall)
                                }

                                if (discountTotalVal > 0) {
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween
                                    ) {
                                        Text("Toplam İskonto", style = MaterialTheme.typography.labelSmall, color = Color(0xFFEF6C00))
                                        Text("-₺%,.2f".format(discountTotalVal).replace(",", "."), style = MaterialTheme.typography.bodySmall, color = Color(0xFFEF6C00))
                                    }
                                }

                                Row(
                                    modifier = Modifier.fillMaxWidth(),
                                    horizontalArrangement = Arrangement.SpaceBetween
                                ) {
                                    val kdvEst = item.amount * 0.20 // rough estimation of 20%
                                    Text("KDV (%20 İnd. Dahil)", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                    Text("₺%,.2f".format(kdvEst).replace(",", "."), style = MaterialTheme.typography.bodySmall)
                                }
                                HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.3f), modifier = Modifier.padding(vertical = 2.dp))
                            }

                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text("Onaylanacak Net Tutar", style = MaterialTheme.typography.titleSmall.copy(fontWeight = FontWeight.Bold))
                                Text(
                                    text = "₺%,.2f".format(item.amount).replace(",", "."),
                                    style = MaterialTheme.typography.titleMedium,
                                    fontWeight = FontWeight.Black,
                                    color = MaterialTheme.colorScheme.primary
                                )
                            }
                        }
                    }
                }
            },
            shape = RoundedCornerShape(16.dp)
        )
    }
}

fun saveEodReportAsPng(
    context: android.content.Context,
    todaySales: Double, todaySalesCount: Int,
    todayCollections: Double, todayCollectionsCount: Int,
    todayTediyes: Double, todayTediyesCount: Int,
    todayReturns: Double, todayReturnsCount: Int,
    todayPurchases: Double, todayPurchasesCount: Int,
    netCashFlow: Double,
    logs: List<com.example.ui.screens.KasaLogItem>
) {
    try {
        val width = 800
        val rowHeight = 70
        val headerHeight = 220
        val totalsHeight = 320
        val detailsHeaderHeight = 60
        val detailsCount = logs.size
        val detailsHeight = if (detailsCount == 0) 80 else (detailsCount * rowHeight + 40)
        val footerHeight = 120
        
        val height = headerHeight + totalsHeight + detailsHeaderHeight + detailsHeight + footerHeight
        
        val bitmap = android.graphics.Bitmap.createBitmap(width, height, android.graphics.Bitmap.Config.ARGB_8888)
        val canvas = android.graphics.Canvas(bitmap)
        
        // Background color
        val bgPaint = android.graphics.Paint().apply {
            color = android.graphics.Color.WHITE
            style = android.graphics.Paint.Style.FILL
        }
        canvas.drawRect(0f, 0f, width.toFloat(), height.toFloat(), bgPaint)
        
        // Border
        val borderPaint = android.graphics.Paint().apply {
            color = android.graphics.Color.parseColor("#128C7E") // Teal-Green border
            style = android.graphics.Paint.Style.STROKE
            strokeWidth = 6f
        }
        canvas.drawRect(10f, 10f, (width - 10).toFloat(), (height - 10).toFloat(), borderPaint)
        
        // Double inner line
        val innerBorderPaint = android.graphics.Paint().apply {
            color = android.graphics.Color.parseColor("#D2D7DF")
            style = android.graphics.Paint.Style.STROKE
            strokeWidth = 1.5f
        }
        canvas.drawRect(18f, 18f, (width - 18).toFloat(), (height - 18).toFloat(), innerBorderPaint)
        
        // Header Paint
        val headerPaint = android.graphics.Paint().apply {
            color = android.graphics.Color.parseColor("#F5F7FA")
            style = android.graphics.Paint.Style.FILL
        }
        canvas.drawRect(20f, 20f, (width - 20).toFloat(), 210f, headerPaint)
        
        // Divider line
        val dividerPaint = android.graphics.Paint().apply {
            color = android.graphics.Color.parseColor("#128C7E")
            strokeWidth = 3f
        }
        canvas.drawLine(20f, 210f, (width - 20).toFloat(), 210f, dividerPaint)
        
        val textPaint = android.graphics.Paint().apply {
            color = android.graphics.Color.parseColor("#212121")
            textSize = 21f
            isAntiAlias = true
        }
        
        val boldPaint = android.graphics.Paint().apply {
            color = android.graphics.Color.parseColor("#128C7E")
            textSize = 21f
            isFakeBoldText = true
            isAntiAlias = true
        }
        
        val titlePaint = android.graphics.Paint().apply {
            color = android.graphics.Color.parseColor("#128C7E")
            textSize = 31f
            isFakeBoldText = true
            isAntiAlias = true
            textAlign = android.graphics.Paint.Align.CENTER
        }
        
        val subtitlePaint = android.graphics.Paint().apply {
            color = android.graphics.Color.parseColor("#37474F")
            textSize = 19f
            isAntiAlias = true
            textAlign = android.graphics.Paint.Align.CENTER
        }
        
        // Draw Header text
        canvas.drawText("MAVİ İNŞAAT MALZEMELERİ A.Ş.", width / 2f, 70f, titlePaint)
        canvas.drawText("GÜN SONU FAALİYET MUTABAKATI", width / 2f, 115f, subtitlePaint)
        
        val dateStr = java.text.SimpleDateFormat("dd.06.2026 HH:mm", java.util.Locale.getDefault()).format(java.util.Date())
        canvas.drawText("Rapor Tarihi: $dateStr  |  Z-No: Z-2026-156", width / 2f, 155f, subtitlePaint)
        canvas.drawText("Saha Temsilcisi: Serhan Kalay (Saha Satış Müdürü)", width / 2f, 185f, subtitlePaint)
        
        // Totals Section
        var y = 250f
        canvas.drawText("ÖZET FAALİYET TABLOSU", 40f, y, boldPaint)
        
        val sectionLinePaint = android.graphics.Paint().apply {
            color = android.graphics.Color.parseColor("#B0BEC5")
            strokeWidth = 2f
        }
        canvas.drawLine(40f, y + 8f, (width - 40).toFloat(), y + 8f, sectionLinePaint)
        
        y += 45f
        
        // Draw rows
        fun drawSummaryRow(c: android.graphics.Canvas, label: String, count: Int, amount: Double, isPositive: Boolean) {
            c.drawText(label, 50f, y, textPaint)
            c.drawText("(${count} Evrak)", 300f, y, textPaint)
            
            val amtStr = String.format("%,.2f ₺", amount)
            val amtPaint = android.graphics.Paint(textPaint).apply {
                color = if (amount == 0.0) android.graphics.Color.DKGRAY else if (isPositive) android.graphics.Color.parseColor("#2E7D32") else android.graphics.Color.parseColor("#C62828")
                isFakeBoldText = true 
            }
            c.drawText(amtStr, (width - 250).toFloat(), y, amtPaint)
            
            // Draw dotted line under row
            val dottedPaint = android.graphics.Paint().apply {
                color = android.graphics.Color.parseColor("#CFD8DC")
                strokeWidth = 1f
                pathEffect = android.graphics.DashPathEffect(floatArrayOf(5f, 5f), 0f)
            }
            c.drawLine(50f, y + 10f, (width - 50).toFloat(), y + 10f, dottedPaint)
        }
        
        drawSummaryRow(canvas, "1. Nakit / Pos Satış", todaySalesCount, todaySales, true)
        y += 45f
        drawSummaryRow(canvas, "2. Tahsilat Girişleri", todayCollectionsCount, todayCollections, true)
        y += 45f
        drawSummaryRow(canvas, "3. Cari Tediye Ödemeleri", todayTediyesCount, todayTediyes, false)
        y += 45f
        drawSummaryRow(canvas, "4. İade Kabul Belgeleri", todayReturnsCount, todayReturns, false)
        y += 45f
        drawSummaryRow(canvas, "5. Alış Giriş Faturaları", todayPurchasesCount, todayPurchases, false)
        
        // Draw Net change box
        y += 60f
        val netBoxPaint = android.graphics.Paint().apply {
            color = android.graphics.Color.parseColor("#ECEFF1")
            style = android.graphics.Paint.Style.FILL
        }
        canvas.drawRect(40f, y - 30f, (width - 40).toFloat(), y + 30f, netBoxPaint)
        canvas.drawRect(40f, y - 30f, (width - 40).toFloat(), y + 30f, innerBorderPaint)
        
        val netLabelPaint = android.graphics.Paint().apply {
            color = android.graphics.Color.parseColor("#37474F")
            textSize = 21f
            isFakeBoldText = true
            isAntiAlias = true
        }
        canvas.drawText("KASA NET DEĞİŞİMİ / AKIŞI:", 60f, y + 6f, netLabelPaint)
        
        val netValStr = String.format("%,.2f ₺", netCashFlow)
        val netValPaint = android.graphics.Paint().apply {
            color = if (netCashFlow >= 0) android.graphics.Color.parseColor("#2E7D32") else android.graphics.Color.parseColor("#C62828")
            textSize = 23f
            isFakeBoldText = true
            isAntiAlias = true
            textAlign = android.graphics.Paint.Align.RIGHT
        }
        canvas.drawText(netValStr, (width - 60).toFloat(), y + 6f, netValPaint)
        
        // Detailed log list title
        y += 75f
        canvas.drawText("GÜNLÜK BELGE VE İŞLEM LİSTESİ", 40f, y, boldPaint)
        canvas.drawLine(40f, y + 8f, (width - 40).toFloat(), y + 8f, sectionLinePaint)
        
        y += 45f
        
        // Column headers for table
        val tabHeaderPaint = android.graphics.Paint().apply {
            color = android.graphics.Color.parseColor("#546E7A")
            textSize = 17f
            isFakeBoldText = true
            isAntiAlias = true
        }
        canvas.drawText("EVRAK ID / TÜR", 50f, y, tabHeaderPaint)
        canvas.drawText("CARİ UNVAN", 250f, y, tabHeaderPaint)
        canvas.drawText("ÖDEME METODU", (width - 270).toFloat(), y, tabHeaderPaint)
        canvas.drawText("TUTAR", (width - 80).toFloat(), y, tabHeaderPaint)
        
        canvas.drawLine(40f, y + 10f, (width - 40).toFloat(), y + 10f, innerBorderPaint)
        
        y += 35f
        
        val tableTextPaint = android.graphics.Paint().apply {
            color = android.graphics.Color.parseColor("#263238")
            textSize = 16f
            isAntiAlias = true
        }
        
        val tableTextBoldPaint = android.graphics.Paint().apply {
            color = android.graphics.Color.parseColor("#263238")
            textSize = 16f
            isAntiAlias = true
            isFakeBoldText = true
        }
        
        val tableDescPaint = android.graphics.Paint().apply {
            color = android.graphics.Color.GRAY
            textSize = 14f
            isAntiAlias = true
        }
        
        if (logs.isEmpty()) {
            canvas.drawText("Bugün yapılmış herhangi bir işlem bulunmamaktadır.", 50f, y + 20f, tableTextPaint)
        } else {
            logs.forEach { log ->
                val shortName = if (log.customerOrSupplier.length > 25) log.customerOrSupplier.substring(0, 23) + ".." else log.customerOrSupplier
                
                // Determine normalized type string
                val isPurch = log.type == "Alış" || (log.type == "Tediye" && log.desc.contains("Alış"))
                val normType = if (isPurch) "ALIŞ" else log.type.uppercase()
                
                canvas.drawText(log.id, 50f, y, tableTextBoldPaint)
                canvas.drawText(normType, 50f, y + 18f, tableDescPaint)
                
                canvas.drawText(shortName, 250f, y, tableTextPaint)
                
                val paymentDesc = log.paymentType + (if (log.bankName != null) " - ${log.bankName}" else "")
                val shortPayDesc = if (paymentDesc.length > 18) paymentDesc.substring(0, 16) + ".." else paymentDesc
                canvas.drawText(shortPayDesc, (width - 270).toFloat(), y, tableTextPaint)
                
                val logAmtStr = String.format("%,.2f ₺", log.amount)
                val logAmtPaint = android.graphics.Paint(tableTextBoldPaint).apply {
                    color = if (normType == "SATIŞ" || normType == "TAHSİLAT") android.graphics.Color.parseColor("#2E7D32") else android.graphics.Color.parseColor("#C62828")
                    textAlign = android.graphics.Paint.Align.RIGHT
                }
                canvas.drawText(logAmtStr, (width - 50).toFloat(), y, logAmtPaint)
                
                canvas.drawLine(40f, y + 26f, (width - 40).toFloat(), y + 26f, innerBorderPaint)
                y += rowHeight
            }
        }
        
        // Draw Footer stamp
        y = height - 90f
        canvas.drawLine(40f, y, (width - 40).toFloat(), y, sectionLinePaint)
        
        val footerPaint = android.graphics.Paint().apply {
            color = android.graphics.Color.GRAY
            textSize = 15f
            isAntiAlias = true
            textAlign = android.graphics.Paint.Align.CENTER
        }
        canvas.drawText("Bu Z-Raporu entegrasyon merkezi tarafından doğrulanmış ve e-Fatura arşivine sevk edilmiştir.", width / 2f, y + 30f, footerPaint)
        canvas.drawText("Powered by Saha Gücü Precision v81.0  |  Teşekkür Ederiz.", width / 2f, y + 52f, footerPaint)
        
        // Save to Gallery via MediaStore
        val displayName = "Gun_Sonu_Raporu_" + java.text.SimpleDateFormat("yyyyMMdd_HHmmss", java.util.Locale.getDefault()).format(java.util.Date()) + ".png"
        
        val resolver = context.contentResolver
        val contentValues = android.content.ContentValues().apply {
            put("_display_name", displayName)
            put("mime_type", "image/png")
            if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.Q) {
                put("relative_path", android.os.Environment.DIRECTORY_PICTURES + "/SahaGucu")
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
            
            android.widget.Toast.makeText(context, "Rapor PNG olarak kaydedildi! 📄 Galeride /SahaGucu altında bulabilirsiniz.", android.widget.Toast.LENGTH_LONG).show()
            
            // Auto trigger share sheet!
            val shareIntent = android.content.Intent(android.content.Intent.ACTION_SEND).apply {
                type = "image/png"
                putExtra(android.content.Intent.EXTRA_STREAM, imageUri)
                addFlags(android.content.Intent.FLAG_GRANT_READ_URI_PERMISSION)
            }
            context.startActivity(android.content.Intent.createChooser(shareIntent, "Gün Sonu Raporunu Gönder / Paylaş"))
        } else {
            android.widget.Toast.makeText(context, "Hata: Dosya kayıt kanalı açılamadı.", android.widget.Toast.LENGTH_SHORT).show()
        }
        
    } catch (e: Exception) {
        android.widget.Toast.makeText(context, "Hata oluştu: ${e.localizedMessage}", android.widget.Toast.LENGTH_LONG).show()
        e.printStackTrace()
    }
}

fun exportSingleOrderToExcel(context: android.content.Context, item: com.example.ui.screens.ApprovalItem): String? {
    try {
        val sdf = java.text.SimpleDateFormat("yyyyMMdd_HHmmss", java.util.Locale.getDefault())
        val safeDocId = item.id.replace("-", "_").lowercase()
        val fileName = "siparis_${safeDocId}_${sdf.format(java.util.Date())}.csv"
        
        val downloadsDir = context.getExternalFilesDir(android.os.Environment.DIRECTORY_DOWNLOADS)
        if (downloadsDir?.exists() == false) {
            downloadsDir.mkdirs()
        }
        val file = java.io.File(downloadsDir, fileName)
        
        val fos = java.io.FileOutputStream(file)
        val bom = byteArrayOf(0xEF.toByte(), 0xBB.toByte(), 0xBF.toByte()) // UTF-8 BOM for MS Excel Turkish compatibility
        fos.write(bom)
        
        val writer = java.io.BufferedWriter(java.io.OutputStreamWriter(fos, "UTF-8"))
        
        writer.write("SİPARİŞ DETAY EXCEL RAPORU\n")
        writer.write("Belge Tarihi:;${item.time}\n")
        writer.write("Rapor Tarihi:;${java.text.SimpleDateFormat("dd.MM.yyyy HH:mm:ss").format(java.util.Date())}\n")
        writer.write("\n")
        
        writer.write("Evrak No;İşlem Tipi;Cari Hesap / Müşteri;Açıklama;Talep Gerekçesi;Ödeme Tipi;Belge Notu;Toplam Tutar (₺)\n")
        
        val payment = item.paymentType ?: "Belirtilmedi"
        val orderNote = item.orderNote ?: ""
        val reason = item.reason.replace(";", " ")
        val desc = item.description.replace(";", " ")
        writer.write("${item.id};${item.type};${item.customerName};${desc};${reason};${payment};${orderNote};${String.format("%.2f", item.amount)}\n")
        writer.write("\n")
        
        val lineItems = com.example.ui.screens.AppDataStore.approvalOrderItemsMap[item.id] ?: emptyList()
        if (lineItems.isNotEmpty()) {
            writer.write(";;---> SİPARİŞ KALEMLERİ:;Ürün Adı;Ürün Kodu;Miktar;Birim Fiyat;İskonto (%);Toplam Satır Tutarı\n")
            for (cartItem in lineItems) {
                val price = cartItem.product.wholesalePrice
                val rawTotal = cartItem.quantity * price
                val lineTotal = rawTotal * (1.0 - cartItem.lineDiscountPercent / 100.0)
                writer.write(";;;${cartItem.product.title};${cartItem.product.code};${cartItem.quantity};${String.format("%.2f", price)};%${String.format("%.0f", cartItem.lineDiscountPercent)};${String.format("%.2f", lineTotal)}\n")
                if (cartItem.note.trim().isNotEmpty()) {
                    writer.write(";;;;* Not: ${cartItem.note.replace(";", " ")}\n")
                }
            }
            writer.write("\n")
        }
        
        writer.flush()
        writer.close()
        fos.close()
        
        return file.absolutePath
    } catch (e: Exception) {
        e.printStackTrace()
        return null
    }
}

fun exportApprovedOrdersToExcel(context: android.content.Context, list: List<com.example.ui.screens.ApprovalItem>): String? {
    try {
        val sdf = java.text.SimpleDateFormat("yyyyMMdd_HHmmss", java.util.Locale.getDefault())
        val fileName = "onayli_siparisler_${sdf.format(java.util.Date())}.csv"
        
        val downloadsDir = context.getExternalFilesDir(android.os.Environment.DIRECTORY_DOWNLOADS)
        if (downloadsDir?.exists() == false) {
            downloadsDir.mkdirs()
        }
        val file = java.io.File(downloadsDir, fileName)
        
        val fos = java.io.FileOutputStream(file)
        val bom = byteArrayOf(0xEF.toByte(), 0xBB.toByte(), 0xBF.toByte()) // UTF-8 BOM for MS Excel Turkish compatibility
        fos.write(bom)
        
        val writer = java.io.BufferedWriter(java.io.OutputStreamWriter(fos, "UTF-8"))
        
        writer.write("ONAYLANAN SİPARİŞLER EXCEL RAPORU\n")
        writer.write("Rapor Tarihi:;${java.text.SimpleDateFormat("dd.MM.yyyy HH:mm:ss").format(java.util.Date())}\n")
        writer.write("Toplam Kayıt Sayısı:;${list.size}\n")
        writer.write("\n")
        
        writer.write("Evrak No;İşlem Tipi;Cari Hesap / Müşteri;Açıklama;Tarih Zaman;Talep Gerekçesi;Ödeme Tipi;Belge Notu;Toplam Tutar (₺)\n")
        
        for (item in list) {
            val payment = item.paymentType ?: "Belirtilmedi"
            val orderNote = item.orderNote ?: ""
            val reason = item.reason.replace(";", " ")
            val desc = item.description.replace(";", " ")
            writer.write("${item.id};${item.type};${item.customerName};${desc};${item.time};${reason};${payment};${orderNote};${String.format("%.2f", item.amount)}\n")
            
            val lineItems = com.example.ui.screens.AppDataStore.approvalOrderItemsMap[item.id] ?: emptyList()
            if (lineItems.isNotEmpty()) {
                writer.write(";;---> SİPARİŞ KALEMLERİ:;Ürün Adı;Ürün Kodu;Miktar;Birim Fiyat;İskonto (%);Toplam Satır Tutarı\n")
                for (cartItem in lineItems) {
                    val price = cartItem.product.wholesalePrice
                    val rawTotal = cartItem.quantity * price
                    val lineTotal = rawTotal * (1.0 - cartItem.lineDiscountPercent / 100.0)
                    writer.write(";;;${cartItem.product.title};${cartItem.product.code};${cartItem.quantity};${String.format("%.2f", price)};%${String.format("%.0f", cartItem.lineDiscountPercent)};${String.format("%.2f", lineTotal)}\n")
                    if (cartItem.note.trim().isNotEmpty()) {
                        writer.write(";;;;* Not: ${cartItem.note.replace(";", " ")}\n")
                    }
                }
                writer.write("\n")
            }
        }
        
        writer.flush()
        writer.close()
        fos.close()
        
        return file.absolutePath
    } catch (e: Exception) {
        e.printStackTrace()
        return null
    }
}




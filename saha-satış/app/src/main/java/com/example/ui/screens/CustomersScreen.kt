package com.example.ui.screens

import android.media.AudioManager
import android.media.ToneGenerator
import android.os.Build
import android.os.VibrationEffect
import android.os.Vibrator
import android.os.VibratorManager
import androidx.compose.animation.*
import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.LazyRow
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
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
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.compose.ui.window.Dialog
import androidx.compose.ui.window.DialogProperties
import androidx.navigation.NavController
import com.example.ui.components.FieldCard
import com.example.ui.components.FieldPrimaryButton
import android.widget.Toast
import com.example.ui.components.FieldHeader
import com.example.ui.components.FieldSecondaryButton
import kotlinx.coroutines.delay
import kotlinx.coroutines.launch
import androidx.compose.ui.graphics.Color

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun CustomersScreen(navController: NavController) {
    val context = LocalContext.current
    val scope = rememberCoroutineScope()
    val snackbarHostState = remember { SnackbarHostState() }

    // --- VIEW / COMPONENT STATES ---
    val searchQuery by AppDataStore.customerSearchQuery
    val selectedFilterTab by AppDataStore.customerSelectedFilterTab
    
    // --- MUTABLE DATA STATE ---
    val dbCustomers by com.example.data.database.DatabaseProvider.getDatabase(context).customerDao().getAllCustomersFlow().collectAsState(initial = emptyList())
    val customersList = dbCustomers.map { c ->
        Customer(
            id = c.id,
            name = c.name,
            balance = c.balance,
            lastVisit = c.lastVisit,
            contact = c.contact,
            phone = c.phone,
            address = c.address,
            taxOffice = c.taxOffice,
            taxNumber = c.taxNumber,
            gpsLocation = c.gpsLocation,
            riskLimit = c.riskLimit,
            priceGroup = c.priceGroup,
            specialDiscountPercent = c.specialDiscountPercent,
            transactions = com.example.data.database.Converters().toCustomerTxList(c.transactionsJson).toMutableList()
        )
    }

    var selectedCustomer by AppDataStore.activeSelectedCustomer
    var showCollectionDialog by remember { mutableStateOf(false) }
    var showPaymentDialog by remember { mutableStateOf(false) }
    var showAddCustomerDialog by AppDataStore.customerShowAddDialog
    var showEditCustomerDialog by AppDataStore.customerShowEditDialog

    // --- RECEIPT/MAKBUZ POPUP DETAIL ---
    var generatedReceipt by remember { mutableStateOf<ReceiptData?>(null) }

    // --- FEEDBACK FUNCTIONS ---
    fun triggerFeedback(isSuccess: Boolean) {
        com.example.util.VibratorHelper.triggerFeedback(context, isSuccess)
    }

    // --- RENDER SCREEN LAYOUT ---
    Scaffold(
        snackbarHost = { SnackbarHost(snackbarHostState) },
        containerColor = MaterialTheme.colorScheme.background,
        contentWindowInsets = androidx.compose.foundation.layout.WindowInsets(0, 0, 0, 0)
    ) { paddingValues ->
        Box(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
        ) {
            Column(
                modifier = Modifier.fillMaxSize().padding(top = 12.dp),
                verticalArrangement = Arrangement.spacedBy(10.dp)
            ) {
                // Customer List Rows (padded)
                val filteredCustomers = customersList.filter { customer ->
                    customer.name.contains(searchQuery, ignoreCase = true) ||
                    customer.id.contains(searchQuery, ignoreCase = true) ||
                    customer.contact.contains(searchQuery, ignoreCase = true) ||
                    customer.taxNumber.contains(searchQuery, ignoreCase = true)
                }

                if (filteredCustomers.isEmpty()) {
                    Box(
                        modifier = Modifier
                            .fillMaxWidth()
                            .weight(1f)
                            .padding(horizontal = 16.dp),
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
                            .weight(1f)
                            .padding(horizontal = 16.dp),
                        contentPadding = PaddingValues(bottom = 140.dp),
                        verticalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        items(filteredCustomers) { customer ->
                            CariRowCard(
                                customer = customer,
                                onSelect = {
                                    selectedCustomer = customer
                                },
                                onQuickCollection = {
                                    selectedCustomer = customer
                                    showCollectionDialog = true
                                },
                                onQuickSales = {
                                    navController.navigate("sales")
                                }
                            )
                        }
                    }
                }
            }

            // --- DETAILS BOTTOM PANEL / MODAL (Cari Detay) ---
            selectedCustomer?.let { customer ->
                if (!showCollectionDialog && !showPaymentDialog) {
                    CariDetailsDrawer(
                        customer = customer,
                        onClose = { selectedCustomer = null },
                        onInitiateCollection = { showCollectionDialog = true },
                        onInitiatePayment = { showPaymentDialog = true },
                        onInitiateSales = {
                            selectedCustomer = null
                            navController.navigate("sales")
                        }
                    )
                }
            }
        }
    }

    // --- COLLECTION DIALOG (Tahsilat Modülü UI) ---
    if (showCollectionDialog && selectedCustomer != null) {
        TransActionDialog(
            title = "Nakit / Kredi / EFT Tahsilat Girişi",
            customer = selectedCustomer!!,
            isCollection = true,
            onDismiss = { showCollectionDialog = false },
            onSave = { amount, type, desc ->
                // Update customer transactions dynamically
                val txId = "COL-" + (1000..9999).random()
                val date = "08.06.2026"
                val newTx = CustomerTx(txId, date, "TAHSİLAT", amount, "$type - $desc", isOffline = true)
                
                selectedCustomer!!.transactions.add(0, newTx)
                selectedCustomer!!.balance -= amount // Tahsilat bakiyeyi düşürür

                triggerFeedback(isSuccess = true)
                showCollectionDialog = false

                // Prepare Receipt Details
                generatedReceipt = ReceiptData(
                    txId = txId,
                    date = date,
                    companyName = selectedCustomer!!.name,
                    taxInfo = "${selectedCustomer!!.taxOffice} / ${selectedCustomer!!.taxNumber}",
                    type = "TAHSİLAT MAKBUZU",
                    amount = amount,
                    paymentType = type,
                    description = desc,
                    isOffline = true,
                    remainingBalance = selectedCustomer!!.balance
                )

                AppDataStore.persist(context)
                scope.launch {
                    snackbarHostState.showSnackbar("İşlem çevrimdışı kaydedildi. Kuyrukta senkronize edilmeyi bekliyor.")
                }
            }
        )
    }

    // --- PAYMENT DIALOG (Tediye Modülü UI) ---
    if (showPaymentDialog && selectedCustomer != null) {
        TransActionDialog(
            title = "Firmaya Tediye (Ödeme) Çıkışı",
            customer = selectedCustomer!!,
            isCollection = false,
            onDismiss = { showPaymentDialog = false },
            onSave = { amount, type, desc ->
                // Update customer transactions
                val txId = "PAY-" + (1000..9999).random()
                val date = "08.06.2026"
                val newTx = CustomerTx(txId, date, "TEDİYE", amount, "$type - $desc", isOffline = true)
                
                selectedCustomer!!.transactions.add(0, newTx)
                selectedCustomer!!.balance += amount // Tediye borcumuzu kapatır, bakiyemizi artırır/azaltır şeklinde işlenir

                triggerFeedback(isSuccess = true)
                showPaymentDialog = false

                // Prepare Receipt Details
                generatedReceipt = ReceiptData(
                    txId = txId,
                    date = date,
                    companyName = selectedCustomer!!.name,
                    taxInfo = "${selectedCustomer!!.taxOffice} / ${selectedCustomer!!.taxNumber}",
                    type = "TEDİYE (ÖDEME) MAKBUZU",
                    amount = amount,
                    paymentType = type,
                    description = desc,
                    isOffline = true,
                    remainingBalance = selectedCustomer!!.balance
                )

                AppDataStore.persist(context)
                scope.launch {
                    snackbarHostState.showSnackbar("Tediye işlemi kaydedildi. ERP senkronizasyonu hazır.")
                }
            }
        )
    }

    if (showAddCustomerDialog) {
        AddCustomerDialog(
            onDismiss = { showAddCustomerDialog = false },
            onSave = { newCust ->
                scope.launch(kotlinx.coroutines.Dispatchers.IO) {
                    com.example.data.database.DatabaseProvider.getDatabase(context).customerDao().insert(
                        com.example.data.database.CustomerEntity(
                            id = newCust.id,
                            name = newCust.name,
                            balance = newCust.balance,
                            lastVisit = newCust.lastVisit,
                            contact = newCust.contact,
                            phone = newCust.phone,
                            address = newCust.address,
                            taxOffice = newCust.taxOffice,
                            taxNumber = newCust.taxNumber,
                            gpsLocation = newCust.gpsLocation,
                            riskLimit = newCust.riskLimit,
                            priceGroup = newCust.priceGroup,
                            specialDiscountPercent = newCust.specialDiscountPercent,
                            transactionsJson = "[]"
                        )
                    )
                }
                showAddCustomerDialog = false
                triggerFeedback(isSuccess = true)
                scope.launch {
                    snackbarHostState.showSnackbar("Yeni cari hesap (${newCust.name}) başarıyla kaydedildi!")
                }
            }
        )
    }

    if (showEditCustomerDialog && selectedCustomer != null) {
        EditCustomerDialog(
            customer = selectedCustomer!!,
            onDismiss = { showEditCustomerDialog = false },
            onSave = { updatedCust ->
                scope.launch(kotlinx.coroutines.Dispatchers.IO) {
                    com.example.data.database.DatabaseProvider.getDatabase(context).customerDao().insert(
                        com.example.data.database.CustomerEntity(
                            id = updatedCust.id,
                            name = updatedCust.name,
                            balance = updatedCust.balance,
                            lastVisit = updatedCust.lastVisit,
                            contact = updatedCust.contact,
                            phone = updatedCust.phone,
                            address = updatedCust.address,
                            taxOffice = updatedCust.taxOffice,
                            taxNumber = updatedCust.taxNumber,
                            gpsLocation = updatedCust.gpsLocation,
                            riskLimit = updatedCust.riskLimit,
                            priceGroup = updatedCust.priceGroup,
                            specialDiscountPercent = updatedCust.specialDiscountPercent,
                            transactionsJson = "[]"
                        )
                    )
                }
                selectedCustomer = updatedCust
                showEditCustomerDialog = false
                triggerFeedback(isSuccess = true)
                scope.launch {
                    snackbarHostState.showSnackbar("Cari hesap (${updatedCust.name}) başarıyla güncellendi!")
                }
            }
        )
    }

    // --- RECEIPT VIEW DIALOG (Interactive Makbuz/Fiş Çıktısı Altyapısı) ---
    generatedReceipt?.let { receipt ->
        Dialog(
            onDismissRequest = { generatedReceipt = null },
            properties = DialogProperties(usePlatformDefaultWidth = false)
        ) {
            Box(
                modifier = Modifier
                    .fillMaxWidth(0.9f)
                    .clip(RoundedCornerShape(24.dp))
                    .background(MaterialTheme.colorScheme.surface)
                    .padding(20.dp)
            ) {
                Column(
                    verticalArrangement = Arrangement.spacedBy(16.dp),
                    horizontalAlignment = Alignment.CenterHorizontally
                ) {
                    // Title and Icon
                    Icon(
                        imageVector = Icons.Filled.ReceiptLong,
                        contentDescription = null,
                        tint = MaterialTheme.colorScheme.primary,
                        modifier = Modifier.size(48.dp)
                    )
                    Text(
                        text = receipt.type,
                        style = MaterialTheme.typography.titleMedium,
                        fontWeight = FontWeight.Bold,
                        color = MaterialTheme.colorScheme.primary
                    )
                    
                    HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)

                    // Receipt Info
                    Column(
                        modifier = Modifier.fillMaxWidth(),
                        verticalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        ReceiptRow("Makbuz No", receipt.txId)
                        ReceiptRow("Tarih", receipt.date)
                        ReceiptRow("Cari Unvan", receipt.companyName)
                        ReceiptRow("Vergi No", receipt.taxInfo)
                        ReceiptRow("Ödeme Yöntemi", receipt.paymentType)
                        ReceiptRow("Açıklama", receipt.description)
                        
                        HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant, modifier = Modifier.padding(vertical = 4.dp))
                        
                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.SpaceBetween
                        ) {
                            Text("İşlem Tutarı", style = MaterialTheme.typography.bodyLarge, fontWeight = FontWeight.Bold)
                            Text(String.format("₺%.2f", receipt.amount), style = MaterialTheme.typography.bodyLarge, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
                        }

                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.SpaceBetween
                        ) {
                            Text("Güncel Cari Bakiye", style = MaterialTheme.typography.bodyMedium)
                            val bLabel = if (receipt.remainingBalance > 0) " (Borçlu)" else if (receipt.remainingBalance < 0) " (Alacaklı)" else ""
                            Text(String.format("₺%.2f%s", Math.abs(receipt.remainingBalance), bLabel), style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold, color = if (receipt.remainingBalance > 0) MaterialTheme.colorScheme.error else MaterialTheme.colorScheme.secondary)
                        }
                    }

                    if (receipt.isOffline) {
                        Surface(
                            color = MaterialTheme.colorScheme.errorContainer.copy(alpha = 0.4f),
                            shape = RoundedCornerShape(8.dp),
                            modifier = Modifier.fillMaxWidth()
                        ) {
                            Row(
                                modifier = Modifier.padding(12.dp),
                                horizontalArrangement = Arrangement.spacedBy(8.dp),
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Icon(Icons.Filled.WifiOff, contentDescription = null, tint = MaterialTheme.colorScheme.onErrorContainer, modifier = Modifier.size(18.dp))
                                Text("Yerel Veritabanına Yazıldı (Çevrimdışı Mod)", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.onErrorContainer)
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
                                triggerFeedback(isSuccess = true)
                                scope.launch {
                                    snackbarHostState.showSnackbar("PDF oluşturuldu ve sistem paylaşım menüsü açıldı.")
                                }
                            },
                            modifier = Modifier.weight(1f)
                        ) {
                            Icon(Icons.Filled.Share, contentDescription = null, modifier = Modifier.size(16.dp))
                            Spacer(modifier = Modifier.width(4.dp))
                            Text("Makbuzu Paylaş")
                        }

                        Button(
                            onClick = { generatedReceipt = null },
                            modifier = Modifier.weight(1f)
                        ) {
                            Text("Kapat")
                        }
                    }
                }
            }
        }
    }
}

// --- SUB-WIDGET: CARI LIST CARD ROW ---
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun CariRowCard(
    customer: Customer,
    onSelect: () -> Unit,
    onQuickCollection: () -> Unit,
    onQuickSales: () -> Unit
) {
    FieldCard(
        modifier = Modifier
            .fillMaxWidth()
            .clickable { onSelect() }
    ) {
        Column {
            Row(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(16.dp),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                Column(modifier = Modifier.weight(1f)) {
                    Text(
                        text = customer.name,
                        style = MaterialTheme.typography.titleMedium,
                        fontWeight = FontWeight.Bold,
                        color = MaterialTheme.colorScheme.onSurface
                    )
                    Spacer(modifier = Modifier.height(4.dp))
                    Row(verticalAlignment = Alignment.CenterVertically) {
                        Icon(Icons.Filled.Tag, contentDescription = null, modifier = Modifier.size(14.dp), tint = MaterialTheme.colorScheme.onSurfaceVariant)
                        Spacer(modifier = Modifier.width(4.dp))
                        Text(
                            text = customer.id,
                            style = MaterialTheme.typography.labelSmall,
                            color = MaterialTheme.colorScheme.onSurfaceVariant,
                            maxLines = 1,
                            overflow = androidx.compose.ui.text.style.TextOverflow.Ellipsis,
                            modifier = Modifier.weight(1f, fill = false)
                        )
                        Spacer(modifier = Modifier.width(16.dp))
                        Icon(Icons.Filled.Person, contentDescription = null, modifier = Modifier.size(14.dp), tint = MaterialTheme.colorScheme.onSurfaceVariant)
                        Spacer(modifier = Modifier.width(4.dp))
                        Text(
                            text = customer.contact,
                            style = MaterialTheme.typography.labelSmall,
                            color = MaterialTheme.colorScheme.onSurfaceVariant,
                            maxLines = 1,
                            overflow = androidx.compose.ui.text.style.TextOverflow.Ellipsis,
                            modifier = Modifier.weight(1f, fill = false)
                        )
                    }
                }
                
                // Balance
                Column(horizontalAlignment = Alignment.End) {
                    val bal = customer.calculatedBalance
                    Text(
                        text = String.format("₺%,.2f", Math.abs(bal)),
                        style = MaterialTheme.typography.titleLarge,
                        fontWeight = FontWeight.Bold,
                        color = if (bal > 0) MaterialTheme.colorScheme.error else if (bal < 0) MaterialTheme.colorScheme.secondary else MaterialTheme.colorScheme.outline
                    )
                    Text(
                        text = if (bal > 0) "Borçlu" else if (bal < 0) "Alacaklı" else "Dengede",
                        style = MaterialTheme.typography.labelSmall,
                        fontWeight = FontWeight.Bold,
                        color = if (bal > 0) MaterialTheme.colorScheme.error else if (bal < 0) MaterialTheme.colorScheme.secondary else MaterialTheme.colorScheme.outline
                    )
                }
            }

            Box(
                modifier = Modifier
                    .fillMaxWidth()
                    .background(MaterialTheme.colorScheme.surfaceContainerLow)
                    .padding(horizontal = 16.dp, vertical = 12.dp)
            ) {
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Text(
                        text = "Son ziyaret: ${customer.lastVisit}",
                        style = MaterialTheme.typography.bodySmall,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                    Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                        IconButton(
                            onClick = onQuickCollection,
                            modifier = Modifier
                                .background(MaterialTheme.colorScheme.primaryContainer, CircleShape)
                                .size(36.dp)
                        ) {
                            Icon(Icons.Filled.RequestQuote, contentDescription = "Quick Collection", tint = MaterialTheme.colorScheme.onPrimaryContainer, modifier = Modifier.size(18.dp))
                        }
                        IconButton(
                            onClick = onQuickSales,
                            modifier = Modifier
                                .background(MaterialTheme.colorScheme.primaryContainer, CircleShape)
                                .size(36.dp)
                        ) {
                            Icon(Icons.Filled.ShoppingCart, contentDescription = "Quick Sale", tint = MaterialTheme.colorScheme.onPrimaryContainer, modifier = Modifier.size(18.dp))
                        }
                    }
                }
            }
        }
    }
}

// --- SUB-WIDGET: CARI DETAL DRAWER OVERLAY ---
@Composable
fun CariDetailsDrawer(
    customer: Customer,
    onClose: () -> Unit,
    onInitiateCollection: () -> Unit,
    onInitiatePayment: () -> Unit,
    onInitiateSales: () -> Unit
) {
    val context = LocalContext.current
    val sharedPre = remember(customer.id) { context.getSharedPreferences("customer_notes", android.content.Context.MODE_PRIVATE) }
    
    // Helper function to dynamically parse list of notes
    fun loadCustomerNotes(cId: String): List<Pair<String, String>> {
        val raw = sharedPre.getString(cId, "") ?: ""
        val time = sharedPre.getString(cId + "_time", "") ?: ""
        if (raw.isBlank()) return emptyList()
        
        if (raw.trim().startsWith("[")) {
            try {
                val arr = org.json.JSONArray(raw)
                val list = mutableListOf<Pair<String, String>>()
                for (i in 0 until arr.length()) {
                    val obj = arr.getJSONObject(i)
                    list.add(Pair(obj.getString("text"), obj.getString("date")))
                }
                return list
            } catch (e: Exception) {
                // fallback
            }
        }
        val dateStr = if (time.isNotEmpty()) time else "Önceki"
        return listOf(Pair(raw, dateStr))
    }

    // Helper function to save list of notes to SharedPreferences
    fun saveCustomerNotes(cId: String, notes: List<Pair<String, String>>) {
        val arr = org.json.JSONArray()
        for (note in notes) {
            val obj = org.json.JSONObject()
            obj.put("text", note.first)
            obj.put("date", note.second)
            arr.put(obj)
        }
        sharedPre.edit()
            .putString(cId, arr.toString())
            .apply()
    }

    var notesList by remember(customer.id) { mutableStateOf(loadCustomerNotes(customer.id)) }
    var noteText by remember(customer.id) { mutableStateOf("") }
    val activeTab = com.example.ui.screens.AppDataStore.customerDetailActiveTab.value
    var selectedInvoiceTxDetails by remember { mutableStateOf<CustomerTx?>(null) }

    var dbOrderRecords by remember(customer.id) { mutableStateOf<List<SalesRecord>>(emptyList()) }
    LaunchedEffect(customer.id, customer.name) {
        try {
            val db = com.example.data.database.DatabaseProvider.getDatabase(context.applicationContext)
            
            // Proactively and dynamically fetch detailed invoices via FieldOps API if not a local draft
            if (!customer.id.startsWith("customer_")) {
                try {
                    val sharedPrefs = context.getSharedPreferences("erp_settings", android.content.Context.MODE_PRIVATE)
                    val apiUrl = sharedPrefs.getString("api_url", "https://lisans.appsgo.cloud") ?: "https://lisans.appsgo.cloud"
                    val apiKey = sharedPrefs.getString("api_key", null).orEmpty()
                    
                    val apiService = com.example.data.api.ApiClient.getFieldOpsApiService(context, apiUrl, apiKey)
                    val response = apiService.getFaturaHareket(com.example.data.api.PullJobsRequest(tenant_id=sharedPrefs.getString("tenant_id", "T001") ?: "T001", api_key=apiKey, device_id=sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT", agent_version="v2.0", entity="faturaHareket", since=customer.id))
                    if (response.isSuccessful && response.body() != null) {
                        val body = response.body()!!
                        val items = body.actualItems
                        
                        for (fatura in items) {
                            val rawEvrak = fatura.evrakNo ?: ""
                            val invoiceNo = if (rawEvrak.isNotEmpty() && !rawEvrak.startsWith("FT-") && !rawEvrak.startsWith("SM-")) {
                                "FT-$rawEvrak"
                            } else {
                                rawEvrak.ifEmpty { "FT-ERP-${fatura.erpRef ?: (Math.random()*100000).toInt()}" }
                            }
                            
                            val totalQtySum = fatura.satirlar?.sumOf { it.miktar?.toInt() ?: 1 } ?: 0
                            val orderEntity = com.example.data.database.WmsOrderEntity(
                                id = invoiceNo,
                                customerName = customer.name,
                                orderDate = fatura.tarih ?: "",
                                status = "Sevk Edildi",
                                totalItems = totalQtySum,
                                syncStatus = "SYNCED"
                            )
                            db.wmsOrderDao().insert(orderEntity)
                            
                            val orderItemsList = mutableListOf<com.example.data.database.WmsOrderItemEntity>()
                            fatura.satirlar?.forEachIndexed { idx, satir ->
                                val stokK = satir.stokKod ?: ""
                                val matchedProd = AppDataStore.products.find { it.code == stokK }
                                val prodBarcode = matchedProd?.barcode ?: "ST-${stokK}"
                                val prodTitle = matchedProd?.title ?: satir.stokAd ?: "Ürün ($stokK)"
                                val itemQty = satir.miktar?.toInt() ?: 1
                                
                                val orderItem = com.example.data.database.WmsOrderItemEntity(
                                    id = "${invoiceNo}_${stokK}_${idx}",
                                    orderId = invoiceNo,
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
                        }
                    }
                } catch (ne: Exception) {
                    ne.printStackTrace()
                }
            }

            // Query products using both direct transaction links (cha_recno / sth_fat_recid_recno, id / orderId) and clean name matching
            val txRecNos = customer.transactions.mapNotNull { it.cha_recno }
            val txIds = customer.transactions.map { it.id }.filter { it.isNotEmpty() }
            val txErpRefs = customer.transactions.mapNotNull { it.erpRef }.filter { it.isNotEmpty() }
            
            val matchedItems = mutableListOf<com.example.data.database.WmsOrderItemEntity>()
            
            // 1. Get by sth_fat_recid_recno (cha_recno mapping to stok_hareketleri)
            for (recNo in txRecNos) {
                val items = db.wmsOrderItemDao().getItemsByRecNo(recNo)
                matchedItems.addAll(items)
            }
            
            // 2. Get by orderId matching transactions
            val allKeys = (txIds + txErpRefs).distinct()
            for (key in allKeys) {
                val items = db.wmsOrderItemDao().getItemsForOrder(key)
                matchedItems.addAll(items)
                
                val cleanKey = key.replace("FT-", "").replace("SM-", "")
                if (cleanKey != key && cleanKey.isNotEmpty()) {
                    val cleanItems = db.wmsOrderItemDao().getItemsForOrder(cleanKey)
                    matchedItems.addAll(cleanItems)
                }
            }
            
            // 3. Fallback name-based matching for legacy/local orders
            val allOrders = db.wmsOrderDao().getAllOrders()
            val cleanNameFn = { name: String ->
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
            
            val custClean = cleanNameFn(customer.name)
            val matchedOrders = allOrders.filter { ord ->
                val ordClean = cleanNameFn(ord.customerName)
                (ordClean.isNotEmpty() && ordClean == custClean) || 
                (custClean.isNotEmpty() && ordClean.contains(custClean)) || 
                (ordClean.isNotEmpty() && custClean.contains(ordClean)) ||
                ord.customerName.trim().equals(customer.name.trim(), ignoreCase = true)
            }
            
            for (ord in matchedOrders) {
                val items = db.wmsOrderItemDao().getItemsForOrder(ord.id)
                matchedItems.addAll(items)
            }
            
            // Collect unique items and map to SalesRecord
            val uniqueMatchedItems = matchedItems.distinctBy { "${it.orderId}_${it.productBarcode}" }
            val resultRecords = mutableListOf<SalesRecord>()
            for (item in uniqueMatchedItems) {
                val qty = if (item.quantityPicked > 0) item.quantityPicked else item.quantityOrdered
                if (qty > 0) {
                    val matchingProd = AppDataStore.products.find { 
                        it.barcode == item.productBarcode || 
                        it.code == item.productBarcode ||
                        (item.productBarcode.startsWith("ST-") && it.code == item.productBarcode.removePrefix("ST-"))
                    }
                    val price = matchingProd?.basePrice ?: 120.0
                    
                    var orderDate = "20.06.2026"
                    val parentOrder = db.wmsOrderDao().getOrderById(item.orderId)
                    if (parentOrder != null && parentOrder.orderDate.isNotEmpty()) {
                        orderDate = parentOrder.orderDate
                    } else {
                        val matchedTx = customer.transactions.find { 
                            it.id == item.orderId || 
                            it.erpRef == item.orderId || 
                            it.cha_recno == item.sth_fat_recid_recno 
                        }
                        if (matchedTx != null) {
                            orderDate = matchedTx.date
                        }
                    }
                    
                    resultRecords.add(
                        SalesRecord(
                            customerId = customer.id,
                            productBarcode = item.productBarcode,
                            quantity = qty,
                            price = price,
                            date = orderDate
                        )
                    )
                }
            }
            dbOrderRecords = resultRecords
        } catch (e: Exception) {
            e.printStackTrace()
        }
    }

    LaunchedEffect(customer.id) {
        com.example.ui.screens.AppDataStore.customerDetailActiveTab.value = 0
    }

    Surface(
        modifier = Modifier
            .fillMaxSize(),
        color = MaterialTheme.colorScheme.background
    ) {
        Column(modifier = Modifier.fillMaxSize()) {
            // Scrollable dynamic detail workspace
            Box(
                modifier = Modifier
                    .weight(1f)
                    .fillMaxWidth()
                    .padding(horizontal = 16.dp, vertical = 2.dp)
            ) {
                when (activeTab) {
                    0 -> { // Özet (Summary) Tab Dashboard
                        LazyColumn(
                            verticalArrangement = Arrangement.spacedBy(12.dp),
                            modifier = Modifier.fillMaxSize()
                        ) {
                            // Balance & Risk Limit section
                            item {
                                Row(
                                    modifier = Modifier.fillMaxWidth(),
                                    horizontalArrangement = Arrangement.spacedBy(10.dp)
                                ) {
                                    val bal = customer.calculatedBalance
                                    val balanceBgColor = if (bal > 0) {
                                        MaterialTheme.colorScheme.errorContainer.copy(alpha = 0.3f)
                                    } else if (bal < 0) {
                                        Color(0xFFE8F5E9)
                                    } else {
                                        MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f)
                                    }
                                    
                                    val balanceTextColor = if (bal > 0) {
                                        MaterialTheme.colorScheme.error
                                    } else if (bal < 0) {
                                        Color(0xFF2E7D32)
                                    } else {
                                        MaterialTheme.colorScheme.onSurfaceVariant
                                    }

                                    Card(
                                        colors = CardDefaults.cardColors(containerColor = balanceBgColor),
                                        modifier = Modifier.weight(1f),
                                        shape = RoundedCornerShape(14.dp)
                                    ) {
                                        Column(modifier = Modifier.padding(12.dp)) {
                                            Text("Cari Bakiye", style = MaterialTheme.typography.labelSmall, color = balanceTextColor, fontWeight = FontWeight.Bold)
                                            Spacer(modifier = Modifier.height(4.dp))
                                            Text(
                                                text = String.format("₺%,.2f", Math.abs(bal)),
                                                style = MaterialTheme.typography.titleMedium,
                                                fontWeight = FontWeight.Bold,
                                                color = balanceTextColor
                                            )
                                            Text(
                                                text = if (bal > 0) "Alacaklıyız (Borçlu)" else if (bal < 0) "Borçluyuz (Alacaklı)" else "Bakiye SIFIR",
                                                style = MaterialTheme.typography.labelSmall,
                                                color = MaterialTheme.colorScheme.onSurfaceVariant
                                            )
                                        }
                                    }

                                    Card(
                                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.3f)),
                                        modifier = Modifier.weight(1f),
                                        shape = RoundedCornerShape(14.dp)
                                    ) {
                                        Column(modifier = Modifier.padding(12.dp)) {
                                            Text("Risk Limiti", style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Bold)
                                            Spacer(modifier = Modifier.height(4.dp))
                                            Text(
                                                text = String.format("₺%,.2f", customer.riskLimit),
                                                style = MaterialTheme.typography.titleMedium,
                                                fontWeight = FontWeight.Bold
                                            )
                                            
                                            val progress = if (customer.riskLimit > 0 && bal > 0) (bal / customer.riskLimit).toFloat().coerceIn(0f, 1f) else 0f
                                            val progressPercent = (progress * 100).toInt()
                                            val progressColor = if (progress > 0.85f) MaterialTheme.colorScheme.error else MaterialTheme.colorScheme.primary
                                            
                                            Column(modifier = Modifier.padding(top = 4.dp)) {
                                                Text("Limit Kullanım: %$progressPercent", style = MaterialTheme.typography.labelSmall, color = progressColor, fontWeight = FontWeight.Bold)
                                                Spacer(modifier = Modifier.height(4.dp))
                                                LinearProgressIndicator(
                                                    progress = progress,
                                                    modifier = Modifier
                                                        .fillMaxWidth()
                                                        .height(6.dp)
                                                        .clip(RoundedCornerShape(3.dp)),
                                                    color = progressColor,
                                                    trackColor = MaterialTheme.colorScheme.outlineVariant
                                                )
                                            }
                                        }
                                    }
                                }
                            }

                            // --- USER REQUESTED: SPECIAL OPERATIONS / ACTIONS (ÖZEL HAREKETLER) ---
                            item {
                                Card(
                                    modifier = Modifier.fillMaxWidth(),
                                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceContainerLow),
                                    shape = RoundedCornerShape(14.dp)
                                ) {
                                    Column(modifier = Modifier.padding(12.dp)) {
                                        Text(
                                            text = "Hızlı & Özel Hareketler",
                                            style = MaterialTheme.typography.labelMedium,
                                            fontWeight = FontWeight.Bold,
                                            color = MaterialTheme.colorScheme.primary
                                        )
                                        Spacer(modifier = Modifier.height(10.dp))
                                        
                                        Row(
                                            modifier = Modifier.fillMaxWidth(),
                                            horizontalArrangement = Arrangement.spacedBy(8.dp)
                                        ) {
                                            // Action 1: Satış
                                            Column(
                                                modifier = Modifier
                                                    .weight(1f)
                                                    .background(MaterialTheme.colorScheme.primaryContainer, RoundedCornerShape(12.dp))
                                                    .clickable { onInitiateSales() }
                                                    .padding(12.dp),
                                                horizontalAlignment = Alignment.CenterHorizontally,
                                                verticalArrangement = Arrangement.Center
                                            ) {
                                                Icon(
                                                    imageVector = Icons.Filled.AddShoppingCart,
                                                    contentDescription = null,
                                                    tint = MaterialTheme.colorScheme.onPrimaryContainer,
                                                    modifier = Modifier.size(24.dp)
                                                )
                                                Spacer(modifier = Modifier.height(6.dp))
                                                Text("Yeni Satış", style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.onPrimaryContainer, maxLines = 1)
                                            }

                                            // Action 2: Tahsilat
                                            Column(
                                                modifier = Modifier
                                                    .weight(1f)
                                                    .background(Color(0xFFE8F5E9), RoundedCornerShape(12.dp))
                                                    .clickable { onInitiateCollection() }
                                                    .padding(12.dp),
                                                horizontalAlignment = Alignment.CenterHorizontally,
                                                verticalArrangement = Arrangement.Center
                                            ) {
                                                Icon(
                                                    imageVector = Icons.Filled.AddCard,
                                                    contentDescription = null,
                                                    tint = Color(0xFF2E7D32),
                                                    modifier = Modifier.size(24.dp)
                                                )
                                                Spacer(modifier = Modifier.height(6.dp))
                                                Text("Tahsilat Al", style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Bold, color = Color(0xFF2E7D32), maxLines = 1)
                                            }

                                            // Action 3: Tediye
                                            Column(
                                                modifier = Modifier
                                                    .weight(1f)
                                                    .background(Color(0xFFFFEBEE), RoundedCornerShape(12.dp))
                                                    .clickable { onInitiatePayment() }
                                                    .padding(12.dp),
                                                horizontalAlignment = Alignment.CenterHorizontally,
                                                verticalArrangement = Arrangement.Center
                                            ) {
                                                Icon(
                                                    imageVector = Icons.Filled.Payment,
                                                    contentDescription = null,
                                                    tint = Color(0xFFC62828),
                                                    modifier = Modifier.size(24.dp)
                                                )
                                                Spacer(modifier = Modifier.height(6.dp))
                                                Text("Tediye Yap", style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Bold, color = Color(0xFFC62828), maxLines = 1)
                                            }
                                        }
                                    }
                                }
                            }

                            // Communication and Contact Details Row
                            item {
                                Card(
                                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceContainerLow),
                                    shape = RoundedCornerShape(14.dp),
                                    modifier = Modifier.fillMaxWidth()
                                ) {
                                    Column(
                                        modifier = Modifier.padding(12.dp),
                                        verticalArrangement = Arrangement.spacedBy(10.dp)
                                    ) {
                                        Text("İletişim Bilgileri", style = MaterialTheme.typography.labelMedium, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
                                        HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))

                                        // Contact Person Row
                                        Row(
                                            modifier = Modifier.fillMaxWidth(),
                                            horizontalArrangement = Arrangement.SpaceBetween,
                                            verticalAlignment = Alignment.CenterVertically
                                        ) {
                                            Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                                                Icon(Icons.Filled.Person, contentDescription = null, tint = MaterialTheme.colorScheme.primary, modifier = Modifier.size(16.dp))
                                                Text("Yetkili Kişi", style = MaterialTheme.typography.bodyMedium, color = MaterialTheme.colorScheme.outline)
                                            }
                                            Text(customer.contact, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold)
                                        }

                                        HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.2f))

                                        // Phone Row with Click-to-Call
                                        Row(
                                            modifier = Modifier
                                                .fillMaxWidth()
                                                .clickable {
                                                    try {
                                                        val intent = android.content.Intent(
                                                            android.content.Intent.ACTION_DIAL,
                                                            android.net.Uri.parse("tel:${customer.phone}")
                                                        )
                                                        context.startActivity(intent)
                                                    } catch (e: Exception) {
                                                        Toast.makeText(context, "Arama başlatılamadı.", Toast.LENGTH_SHORT).show()
                                                    }
                                                }
                                                .padding(vertical = 4.dp),
                                            horizontalArrangement = Arrangement.SpaceBetween,
                                            verticalAlignment = Alignment.CenterVertically
                                        ) {
                                            Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                                                Icon(Icons.Filled.Phone, contentDescription = null, tint = Color(0xFF2E7D32), modifier = Modifier.size(16.dp))
                                                Text("Telefon", style = MaterialTheme.typography.bodyMedium, color = MaterialTheme.colorScheme.outline)
                                            }
                                            Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(4.dp)) {
                                                Text(customer.phone, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
                                                Icon(Icons.Filled.Call, contentDescription = "Ara", tint = MaterialTheme.colorScheme.primary, modifier = Modifier.size(14.dp))
                                            }
                                        }
                                    }
                                }
                            }

                            // Billing Details Card
                            item {
                                Card(
                                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceContainerLow),
                                    shape = RoundedCornerShape(14.dp),
                                    modifier = Modifier.fillMaxWidth()
                                ) {
                                    Column(
                                        modifier = Modifier.padding(12.dp),
                                        verticalArrangement = Arrangement.spacedBy(10.dp)
                                    ) {
                                        Text("Fatura & Vergi Detayları", style = MaterialTheme.typography.labelMedium, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
                                        HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))

                                        Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                                            Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                                                Icon(Icons.Filled.Business, contentDescription = null, tint = MaterialTheme.colorScheme.outline, modifier = Modifier.size(16.dp))
                                                Text("Vergi Dairesi", style = MaterialTheme.typography.bodyMedium)
                                            }
                                            Text(customer.taxOffice, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold)
                                        }

                                        Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                                            Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                                                Icon(Icons.Filled.Badge, contentDescription = null, tint = MaterialTheme.colorScheme.outline, modifier = Modifier.size(16.dp))
                                                Text("Vergi No", style = MaterialTheme.typography.bodyMedium)
                                            }
                                            Text(customer.taxNumber, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold)
                                        }

                                        Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                                            Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                                                Icon(Icons.Filled.LocalOffer, contentDescription = null, tint = MaterialTheme.colorScheme.outline, modifier = Modifier.size(16.dp))
                                                Text("Fiyat Grubu", style = MaterialTheme.typography.bodyMedium)
                                            }
                                            Text(customer.priceGroup, style = MaterialTheme.typography.bodyMedium)
                                        }

                                        Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                                            Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                                                Spacer(modifier = Modifier.size(16.dp))
                                                Text("", style = MaterialTheme.typography.bodyMedium)
                                            }
                                            Text("", style = MaterialTheme.typography.bodyMedium)
                                        }
                                    }
                                }
                            }

                            // Address Card with Map routing intent
                            item {
                                Card(
                                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceContainerLow),
                                    shape = RoundedCornerShape(14.dp),
                                    modifier = Modifier.fillMaxWidth()
                                ) {
                                    Column(
                                        modifier = Modifier.padding(12.dp),
                                        verticalArrangement = Arrangement.spacedBy(10.dp)
                                    ) {
                                        Text("Adres & Konum", style = MaterialTheme.typography.labelMedium, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
                                        HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))

                                        Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween, verticalAlignment = Alignment.CenterVertically) {
                                            Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                                                Icon(Icons.Filled.Place, contentDescription = null, tint = Color(0xFFC62828), modifier = Modifier.size(16.dp))
                                                Text("Ziyaret Noktası (GPS)", style = MaterialTheme.typography.bodyMedium)
                                            }
                                            Text(customer.gpsLocation.ifEmpty { "Konum Kayıtlı Değil" }, style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.outline)
                                        }

                                        Column(
                                            modifier = Modifier
                                                .fillMaxWidth()
                                                .background(MaterialTheme.colorScheme.surface, RoundedCornerShape(8.dp))
                                                .clickable {
                                                    try {
                                                        val encodedAddress = java.net.URLEncoder.encode(customer.address, "UTF-8")
                                                        val intent = android.content.Intent(
                                                            android.content.Intent.ACTION_VIEW,
                                                            android.net.Uri.parse("geo:0,0?q=$encodedAddress")
                                                        )
                                                        context.startActivity(intent)
                                                    } catch (e: Exception) {
                                                        Toast.makeText(context, "Harita uygulaması bulunamadı.", Toast.LENGTH_SHORT).show()
                                                    }
                                                }
                                                .padding(10.dp)
                                        ) {
                                            Row(
                                                modifier = Modifier.fillMaxWidth(),
                                                horizontalArrangement = Arrangement.SpaceBetween,
                                                verticalAlignment = Alignment.CenterVertically
                                            ) {
                                                Text("Fatura & Sevk Adresi", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.primary, fontWeight = FontWeight.Bold)
                                                Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(2.dp)) {
                                                    Text("Haritada Aç", style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.primary)
                                                    Icon(Icons.Filled.Directions, contentDescription = "Yol Tarifi", tint = MaterialTheme.colorScheme.primary, modifier = Modifier.size(14.dp))
                                                }
                                            }
                                            Spacer(modifier = Modifier.height(6.dp))
                                            Text(customer.address, style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.onSurface)
                                        }
                                    }
                                }
                            }
                        }
                    }

                    1 -> { // Hareketler Tab
                        if (customer.transactions.isEmpty()) {
                            Box(modifier = Modifier.fillMaxSize(), contentAlignment = Alignment.Center) {
                                Column(horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(8.dp)) {
                                    Icon(Icons.Filled.ReceiptLong, contentDescription = null, modifier = Modifier.size(48.dp), tint = MaterialTheme.colorScheme.outline)
                                    Text("Herhangi bir işlem hareketi mevcut değil.", style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.outline)
                                }
                            }
                        } else {
                            // Extract unique years from the transactions list dynamically
                            val availableYears = remember(customer.transactions) {
                                val years = customer.transactions.map { tx ->
                                    val trimmed = tx.date.trim()
                                    var extracted = "Tümü"
                                    if (trimmed.length >= 4) {
                                        if (trimmed.contains(".")) {
                                            val parts = trimmed.split(".")
                                            val last = parts.lastOrNull()?.trim() ?: ""
                                            if (last.length == 4 && last.toIntOrNull() != null) extracted = last
                                        } else if (trimmed.contains("-")) {
                                            val first = trimmed.split("-").firstOrNull()?.trim() ?: ""
                                            if (first.length == 4 && first.toIntOrNull() != null) extracted = first
                                        } else {
                                            val last4 = trimmed.takeLast(4)
                                            if (last4.toIntOrNull() != null) extracted = last4
                                        }
                                    }
                                    extracted
                                }.filter { it != "Tümü" && it.toIntOrNull() != null }.toSet().toList().sortedDescending()
                                listOf("Tümü") + years
                            }
                            var selectedYear by remember { mutableStateOf("Tümü") }
                            val filteredTransactions = remember(customer.transactions, selectedYear) {
                                if (selectedYear == "Tümü") {
                                    customer.transactions
                                } else {
                                    customer.transactions.filter { tx ->
                                        val trimmed = tx.date.trim()
                                        var extracted = ""
                                        if (trimmed.length >= 4) {
                                            if (trimmed.contains(".")) {
                                                val parts = trimmed.split(".")
                                                val last = parts.lastOrNull()?.trim() ?: ""
                                                if (last.length == 4 && last.toIntOrNull() != null) extracted = last
                                            } else if (trimmed.contains("-")) {
                                                val first = trimmed.split("-").firstOrNull()?.trim() ?: ""
                                                if (first.length == 4 && first.toIntOrNull() != null) extracted = first
                                            } else {
                                                val last4 = trimmed.takeLast(4)
                                                if (last4.toIntOrNull() != null) extracted = last4
                                            }
                                        }
                                        extracted == selectedYear
                                    }
                                }
                            }

                            Column(
                                modifier = Modifier.fillMaxSize(),
                                verticalArrangement = Arrangement.spacedBy(8.dp)
                            ) {
                                // Elegant custom M3 chip row for Year Selection
                                Row(
                                    modifier = Modifier.fillMaxWidth().padding(horizontal = 4.dp, vertical = 4.dp),
                                    horizontalArrangement = Arrangement.spacedBy(8.dp),
                                    verticalAlignment = Alignment.CenterVertically
                                ) {
                                    Text(
                                        text = "Yıl Seçimi:",
                                        style = MaterialTheme.typography.labelMedium,
                                        fontWeight = FontWeight.Bold,
                                        color = MaterialTheme.colorScheme.onSurfaceVariant
                                    )
                                    LazyRow(
                                        horizontalArrangement = Arrangement.spacedBy(6.dp),
                                        modifier = Modifier.weight(1f)
                                    ) {
                                        items(availableYears.size) { index ->
                                            val year = availableYears[index]
                                            val isSelected = year == selectedYear
                                            Surface(
                                                modifier = Modifier.clickable { selectedYear = year },
                                                color = if (isSelected) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f),
                                                shape = RoundedCornerShape(20.dp),
                                                border = androidx.compose.foundation.BorderStroke(
                                                    width = 1.dp,
                                                    color = if (isSelected) Color.Transparent else MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.6f)
                                                )
                                            ) {
                                                Text(
                                                    text = year,
                                                    style = MaterialTheme.typography.bodySmall,
                                                    fontWeight = if (isSelected) FontWeight.Bold else FontWeight.Normal,
                                                    color = if (isSelected) MaterialTheme.colorScheme.onPrimary else MaterialTheme.colorScheme.onSurface,
                                                    modifier = Modifier.padding(horizontal = 12.dp, vertical = 6.dp)
                                                )
                                            }
                                        }
                                    }
                                }

                                if (filteredTransactions.isEmpty()) {
                                    Box(modifier = Modifier.weight(1f).fillMaxWidth(), contentAlignment = Alignment.Center) {
                                        Text("$selectedYear yılı için işlem hareketi bulunmamaktadır.", style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.outline)
                                    }
                                } else {
                                    LazyColumn(
                                        verticalArrangement = Arrangement.spacedBy(8.dp),
                                        modifier = Modifier.weight(1f).fillMaxWidth()
                                    ) {
                                        items(filteredTransactions) { tx ->
                                            Card(
                                                colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceContainerLow),
                                                modifier = Modifier
                                                    .fillMaxWidth()
                                                    .clickable { selectedInvoiceTxDetails = tx },
                                                shape = RoundedCornerShape(12.dp)
                                            ) {
                                                Row(
                                                    modifier = Modifier
                                                        .fillMaxWidth()
                                                        .padding(12.dp),
                                                    horizontalArrangement = Arrangement.SpaceBetween,
                                                    verticalAlignment = Alignment.CenterVertically
                                                ) {
                                                    Row(
                                                        modifier = Modifier.weight(1f),
                                                        verticalAlignment = Alignment.CenterVertically,
                                                        horizontalArrangement = Arrangement.spacedBy(12.dp)
                                                    ) {
                                                        val (icon, tintColor, bgColor) = when (tx.type) {
                                                            "SATIŞ" -> Triple(Icons.Filled.LocalMall, MaterialTheme.colorScheme.primary, MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.4f))
                                                            "TAHSİLAT" -> Triple(Icons.Filled.AddCard, Color(0xFF2E7D32), Color(0xFFE8F5E9))
                                                            else -> Triple(Icons.Filled.Payment, Color(0xFFC62828), Color(0xFFFFEBEE))
                                                        }

                                                        Box(
                                                            modifier = Modifier
                                                                .size(40.dp)
                                                                .background(bgColor, CircleShape),
                                                            contentAlignment = Alignment.Center
                                                        ) {
                                                            Icon(icon, contentDescription = null, tint = tintColor, modifier = Modifier.size(20.dp))
                                                        }

                                                        Column {
                                                            Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(6.dp)) {
                                                                Text(tx.type, style = MaterialTheme.typography.labelMedium, fontWeight = FontWeight.Black, color = tintColor)
                                                                if (tx.isOffline) {
                                                                    Surface(color = MaterialTheme.colorScheme.errorContainer, shape = RoundedCornerShape(4.dp)) {
                                                                        Text("Çevrimdışı", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.onErrorContainer, modifier = Modifier.padding(horizontal = 4.dp))
                                                                    }
                                                                }
                                                            }
                                                            Text(tx.description, style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
                                                            Text(tx.date, style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.outline)
                                                        }
                                                    }

                                                    Column(horizontalAlignment = Alignment.End) {
                                                        Text(
                                                            text = String.format("₺%,.2f", tx.amount),
                                                            style = MaterialTheme.typography.bodyMedium,
                                                            fontWeight = FontWeight.Bold,
                                                            color = MaterialTheme.colorScheme.onSurface
                                                        )
                                                        Icon(
                                                            imageVector = Icons.Filled.ChevronRight,
                                                            contentDescription = "Detaylar",
                                                            modifier = Modifier.size(16.dp),
                                                            tint = MaterialTheme.colorScheme.outline
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

                    2 -> { // Ürünler Tab (Products Purchased)
                        val purchasedList = remember(customer.id, dbOrderRecords) {
                            val unifiedList = mutableListOf<SalesRecord>()
                            // Add all SQLite matched orders first
                            unifiedList.addAll(dbOrderRecords)
                            // Add current session's memory sales history
                            val memoryRecords = AppDataStore.salesHistory.filter { it.customerId == customer.id }
                            for (mr in memoryRecords) {
                                val exists = unifiedList.any { 
                                    it.productBarcode == mr.productBarcode && 
                                    it.quantity == mr.quantity && 
                                    it.date == mr.date 
                                }
                                if (!exists) {
                                    unifiedList.add(mr)
                                }
                            }
                            // Map combined products list
                            unifiedList.map { record ->
                                val matchingProduct = AppDataStore.products.find { 
                                    it.barcode == record.productBarcode || 
                                    it.code == record.productBarcode ||
                                    (record.productBarcode.startsWith("ST-") && it.code == record.productBarcode.removePrefix("ST-"))
                                }
                                Pair(record, matchingProduct)
                            }
                        }

                        var productSearchQuery by remember { mutableStateOf("") }
                        val filteredPurchasedList = remember(purchasedList, productSearchQuery) {
                            purchasedList.filter { (record, product) ->
                                val title = product?.title ?: record.productBarcode
                                val code = product?.code ?: ""
                                title.contains(productSearchQuery, ignoreCase = true) || code.contains(productSearchQuery, ignoreCase = true)
                            }
                        }

                        Column(
                            modifier = Modifier.fillMaxSize(),
                            verticalArrangement = Arrangement.spacedBy(10.dp)
                        ) {
                            // Search bar for products
                            OutlinedTextField(
                                value = productSearchQuery,
                                onValueChange = { productSearchQuery = it },
                                placeholder = { Text("Satın alınan ürünlerde ara...", style = MaterialTheme.typography.bodyMedium) },
                                leadingIcon = { Icon(Icons.Filled.Search, null, tint = MaterialTheme.colorScheme.outline) },
                                trailingIcon = {
                                    if (productSearchQuery.isNotEmpty()) {
                                        IconButton(onClick = { productSearchQuery = "" }) {
                                            Icon(Icons.Filled.Clear, null)
                                        }
                                    }
                                },
                                modifier = Modifier.fillMaxWidth(),
                                shape = RoundedCornerShape(12.dp),
                                singleLine = true,
                                colors = OutlinedTextFieldDefaults.colors(
                                    unfocusedContainerColor = MaterialTheme.colorScheme.surfaceContainerLowest,
                                    focusedContainerColor = MaterialTheme.colorScheme.surfaceContainerLowest
                                )
                            )

                            if (filteredPurchasedList.isEmpty()) {
                                Box(modifier = Modifier.weight(1f).fillMaxWidth(), contentAlignment = Alignment.Center) {
                                    Column(horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(4.dp)) {
                                        Icon(Icons.Filled.ShoppingBag, contentDescription = null, modifier = Modifier.size(48.dp), tint = MaterialTheme.colorScheme.outline)
                                        Text("Satın alınan ürün bulunmamaktadır.", style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.outline)
                                    }
                                }
                            } else {
                                LazyColumn(
                                    verticalArrangement = Arrangement.spacedBy(8.dp),
                                    modifier = Modifier.weight(1f).fillMaxWidth()
                                ) {
                                    items(items = filteredPurchasedList, key = { (record, _) -> record.date + "_" + record.productBarcode }) { (record, product) ->
                                        Card(
                                            colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceContainerLow),
                                            modifier = Modifier.fillMaxWidth(),
                                            shape = RoundedCornerShape(12.dp)
                                        ) {
                                            Row(
                                                modifier = Modifier
                                                    .fillMaxWidth()
                                                    .padding(12.dp),
                                                horizontalArrangement = Arrangement.SpaceBetween,
                                                verticalAlignment = Alignment.CenterVertically
                                            ) {
                                                Row(
                                                    modifier = Modifier.weight(1f),
                                                    verticalAlignment = Alignment.CenterVertically,
                                                    horizontalArrangement = Arrangement.spacedBy(12.dp)
                                                ) {
                                                    Box(
                                                        modifier = Modifier
                                                            .size(40.dp)
                                                            .background(MaterialTheme.colorScheme.surface, CircleShape),
                                                        contentAlignment = Alignment.Center
                                                    ) {
                                                        Icon(
                                                            imageVector = Icons.Filled.ShoppingBag,
                                                            contentDescription = null,
                                                            tint = MaterialTheme.colorScheme.primary,
                                                            modifier = Modifier.size(20.dp)
                                                        )
                                                    }
                                                    Column {
                                                        Text(product?.title ?: record.productBarcode, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold)
                                                        Row(
                                                            verticalAlignment = Alignment.CenterVertically,
                                                            horizontalArrangement = Arrangement.spacedBy(8.dp)
                                                        ) {
                                                            Text("Kod: ${product?.code ?: "N/A"}", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.outline)
                                                            Text("•", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.outline)
                                                            Text(record.date, style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.outline)
                                                        }
                                                    }
                                                }
                                                Column(horizontalAlignment = Alignment.End) {
                                                    Text(
                                                        text = "${record.quantity} Adet",
                                                        style = MaterialTheme.typography.bodyMedium,
                                                        fontWeight = FontWeight.Bold,
                                                        color = MaterialTheme.colorScheme.primary
                                                    )
                                                    Text(
                                                        text = String.format("₺%,.2f", record.price),
                                                        style = MaterialTheme.typography.labelSmall,
                                                        color = MaterialTheme.colorScheme.outline
                                                    )
                                                    Text(
                                                        text = "Toplam: ${String.format("₺%,.2f", record.quantity * record.price)}",
                                                        style = MaterialTheme.typography.bodySmall,
                                                        fontWeight = FontWeight.Bold,
                                                        color = MaterialTheme.colorScheme.onSurface
                                                    )
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    3 -> { // Cari Not Tab (Notepad)
                        Column(
                            modifier = Modifier.fillMaxSize(),
                            verticalArrangement = Arrangement.spacedBy(12.dp)
                        ) {
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(6.dp)) {
                                    Icon(Icons.Filled.EditNote, contentDescription = null, tint = MaterialTheme.colorScheme.primary)
                                    Text(
                                        "Görüşme ve Ziyaret Notları",
                                        style = MaterialTheme.typography.titleMedium,
                                        fontWeight = FontWeight.Bold,
                                        color = MaterialTheme.colorScheme.onSurface
                                    )
                                }
                                Text(
                                    "${notesList.size} Not",
                                    style = MaterialTheme.typography.labelSmall,
                                    color = MaterialTheme.colorScheme.outline
                                )
                            }

                            // Input section to add a new note
                            Card(
                                modifier = Modifier.fillMaxWidth(),
                                colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceContainerLow),
                                shape = RoundedCornerShape(12.dp),
                                border = androidx.compose.foundation.BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
                            ) {
                                Column(modifier = Modifier.padding(12.dp), verticalArrangement = Arrangement.spacedBy(8.dp)) {
                                    OutlinedTextField(
                                        value = noteText,
                                        onValueChange = { noteText = it },
                                        placeholder = { Text("Yeni bir müşteri ziyareti veya randevu notu ekleyin...", style = MaterialTheme.typography.bodyMedium, color = MaterialTheme.colorScheme.outline) },
                                        shape = RoundedCornerShape(10.dp),
                                        modifier = Modifier.fillMaxWidth(),
                                        textStyle = MaterialTheme.typography.bodyMedium,
                                        minLines = 2,
                                        maxLines = 4,
                                        colors = OutlinedTextFieldDefaults.colors(
                                            unfocusedBorderColor = MaterialTheme.colorScheme.outlineVariant,
                                            focusedBorderColor = MaterialTheme.colorScheme.primary
                                        )
                                    )
                                    
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.End
                                    ) {
                                        Button(
                                            onClick = {
                                                if (noteText.isNotBlank()) {
                                                    val currentTime = java.text.SimpleDateFormat("dd.MM.yyyy HH:mm").format(java.util.Date())
                                                    val newList = notesList.toMutableList()
                                                    newList.add(0, Pair(noteText.trim(), currentTime)) // newest first
                                                    saveCustomerNotes(customer.id, newList)
                                                    notesList = newList
                                                    noteText = ""
                                                    Toast.makeText(context, "Yeni not kaydedildi.", Toast.LENGTH_SHORT).show()
                                                } else {
                                                    Toast.makeText(context, "Lütfen not metni yazın.", Toast.LENGTH_SHORT).show()
                                                }
                                            },
                                            shape = RoundedCornerShape(8.dp),
                                            contentPadding = PaddingValues(horizontal = 16.dp, vertical = 8.dp)
                                        ) {
                                            Icon(Icons.Filled.Add, contentDescription = null, modifier = Modifier.size(16.dp))
                                            Spacer(modifier = Modifier.width(6.dp))
                                            Text("Yeni Not Ekle", style = MaterialTheme.typography.labelMedium, fontWeight = FontWeight.Bold)
                                        }
                                    }
                                }
                            }

                            // Scrollable list of existing notes
                            if (notesList.isEmpty()) {
                                Box(
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .weight(1f),
                                    contentAlignment = Alignment.Center
                                ) {
                                    Column(
                                        horizontalAlignment = Alignment.CenterHorizontally,
                                        verticalArrangement = Arrangement.spacedBy(4.dp)
                                    ) {
                                        Icon(Icons.Filled.EditNote, contentDescription = null, modifier = Modifier.size(48.dp), tint = MaterialTheme.colorScheme.outline.copy(alpha = 0.5f))
                                        Text("Henüz kaydedilmiş bir not bulunmamaktadır.", style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.outline)
                                    }
                                }
                            } else {
                                LazyColumn(
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .weight(1f),
                                    verticalArrangement = Arrangement.spacedBy(8.dp)
                                ) {
                                    items(notesList) { (text, date) ->
                                        Card(
                                            modifier = Modifier.fillMaxWidth(),
                                            colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.3f)),
                                            shape = RoundedCornerShape(10.dp)
                                        ) {
                                            Column(
                                                modifier = Modifier.padding(12.dp),
                                                verticalArrangement = Arrangement.spacedBy(6.dp)
                                            ) {
                                                Row(
                                                    modifier = Modifier.fillMaxWidth(),
                                                    horizontalArrangement = Arrangement.SpaceBetween,
                                                    verticalAlignment = Alignment.CenterVertically
                                                ) {
                                                    Surface(
                                                        color = MaterialTheme.colorScheme.primaryContainer,
                                                        shape = RoundedCornerShape(6.dp)
                                                    ) {
                                                         Text(
                                                             text = date,
                                                             style = MaterialTheme.typography.labelSmall,
                                                             fontWeight = FontWeight.Bold,
                                                             color = MaterialTheme.colorScheme.onPrimaryContainer,
                                                             modifier = Modifier.padding(horizontal = 8.dp, vertical = 3.dp)
                                                         )
                                                     }
                                                     
                                                     IconButton(
                                                         onClick = {
                                                             val newList = notesList.toMutableList()
                                                             newList.remove(Pair(text, date))
                                                             saveCustomerNotes(customer.id, newList)
                                                             notesList = newList
                                                             Toast.makeText(context, "Not silindi.", Toast.LENGTH_SHORT).show()
                                                         },
                                                         modifier = Modifier.size(24.dp)
                                                     ) {
                                                         Icon(
                                                             imageVector = Icons.Filled.Delete,
                                                             contentDescription = "Sil",
                                                             tint = MaterialTheme.colorScheme.error.copy(alpha = 0.7f),
                                                             modifier = Modifier.size(16.dp)
                                                         )
                                                     }
                                                 }
                                                 
                                                 Text(
                                                     text = text,
                                                     style = MaterialTheme.typography.bodyMedium,
                                                     color = MaterialTheme.colorScheme.onSurface
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

            if (selectedInvoiceTxDetails != null) {
                InvoiceDetailDialog(
                    tx = selectedInvoiceTxDetails!!,
                    customerName = customer.name,
                    onDismiss = { selectedInvoiceTxDetails = null }
                )
            }
        }
    }
}

// --- SUB-WIDGET: ROW VALUES COMPONENTS ---
@Composable
fun DetailRow(icon: androidx.compose.ui.graphics.vector.ImageVector, label: String, valText: String) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .padding(vertical = 4.dp),
        verticalAlignment = Alignment.Top,
        horizontalArrangement = Arrangement.spacedBy(12.dp)
    ) {
        Icon(icon, contentDescription = null, tint = MaterialTheme.colorScheme.primary, modifier = Modifier.size(20.dp))
        Column(modifier = Modifier.weight(1f)) {
            Text(label, style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.outline)
            Text(valText, style = MaterialTheme.typography.bodyMedium, color = MaterialTheme.colorScheme.onSurface)
        }
    }
}

@Composable
fun ReceiptRow(label: String, valText: String) {
    Row(
        modifier = Modifier.fillMaxWidth(),
        horizontalArrangement = Arrangement.SpaceBetween,
        verticalAlignment = Alignment.Top
    ) {
        Text(label, style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
        Text(valText, style = MaterialTheme.typography.bodySmall, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.onSurface, textAlign = TextAlign.End, modifier = Modifier.width(180.dp))
    }
}

// --- TRANSACTION POPUP FORM ---
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun TransActionDialog(
    title: String,
    customer: Customer,
    isCollection: Boolean,
    onDismiss: () -> Unit,
    onSave: (amount: Double, type: String, description: String) -> Unit
) {
    var amountText by remember { mutableStateOf("") }
    var selectedType by remember { mutableStateOf("Nakit") }
    var descriptionText by remember { mutableStateOf("") }
    var isError by remember { mutableStateOf(false) }

    Dialog(onDismissRequest = onDismiss) {
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
                Text(title, style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
                
                HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)

                Text(
                    text = "Cari: ${customer.name}",
                    style = MaterialTheme.typography.bodyMedium,
                    fontWeight = FontWeight.SemiBold
                )

                // Selectable Modes
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    val types = listOf("Nakit", "Kredi Kartı", "Havale / EFT")
                    types.forEach { type ->
                        val isSelected = selectedType == type
                        Box(
                            modifier = Modifier
                                .weight(1f)
                                .clip(RoundedCornerShape(8.dp))
                                .background(if (isSelected) MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surfaceVariant)
                                .clickable { selectedType = type }
                                .padding(vertical = 10.dp),
                            contentAlignment = Alignment.Center
                        ) {
                            Text(
                                text = type,
                                style = MaterialTheme.typography.labelSmall,
                                fontWeight = FontWeight.Bold,
                                color = if (isSelected) MaterialTheme.colorScheme.onPrimaryContainer else MaterialTheme.colorScheme.onSurfaceVariant
                            )
                        }
                    }
                }

                // Amount text input
                OutlinedTextField(
                    value = amountText,
                    onValueChange = {
                        amountText = it
                        isError = false
                    },
                    modifier = Modifier.fillMaxWidth(),
                    label = { Text("Tutar (₺)") },
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                    singleLine = true,
                    isError = isError
                )

                // Description Input
                OutlinedTextField(
                    value = descriptionText,
                    onValueChange = { descriptionText = it },
                    modifier = Modifier.fillMaxWidth(),
                    label = { Text("Açıklama / Not") },
                    maxLines = 3
                )

                HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)

                // Dialog Buttons
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    OutlinedButton(onClick = onDismiss, modifier = Modifier.weight(1f)) {
                        Text("Vazgeç")
                    }
                    Button(
                        onClick = {
                            val amt = amountText.toDoubleOrNull()
                            if (amt != null && amt > 0) {
                                onSave(amt, selectedType, descriptionText)
                            } else {
                                isError = true
                            }
                        },
                        modifier = Modifier.weight(1f)
                    ) {
                        Text("Makbuzu Kaydet")
                    }
                }
            }
        }
    }
}

// Receipt Class Struct Helper
data class ReceiptData(
    val txId: String,
    val date: String,
    val companyName: String,
    val taxInfo: String,
    val type: String,
    val amount: Double,
    val paymentType: String,
    val description: String,
    val isOffline: Boolean,
    val remainingBalance: Double
)

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun AddCustomerDialog(
    onDismiss: () -> Unit,
    onSave: (Customer) -> Unit
) {
    var name by remember { mutableStateOf("") }
    var contact by remember { mutableStateOf("") }
    var phone by remember { mutableStateOf("") }
    var address by remember { mutableStateOf("") }
    var taxOffice by remember { mutableStateOf("") }
    var taxNumber by remember { mutableStateOf("") }
    var priceGroup by remember { mutableStateOf("Bayi-1 Klasmanı") }
    var riskLimitText by remember { mutableStateOf("50000") }
    var discountText by remember { mutableStateOf("0") }
    
    var nameError by remember { mutableStateOf(false) }

    Dialog(
        onDismissRequest = onDismiss,
        properties = DialogProperties(usePlatformDefaultWidth = false)
    ) {
        Surface(
            modifier = Modifier.fillMaxSize(),
            color = MaterialTheme.colorScheme.background
        ) {
            Column(
                modifier = Modifier
                    .fillMaxSize()
                    .windowInsetsPadding(WindowInsets.statusBars)
            ) {
                // Custom Navbar / Top Bar for Full Screen Dialog
                Card(
                    modifier = Modifier.fillMaxWidth(),
                    shape = RoundedCornerShape(0.dp),
                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
                    elevation = CardDefaults.cardElevation(defaultElevation = 2.dp)
                ) {
                    Row(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(horizontal = 16.dp, vertical = 14.dp),
                        verticalAlignment = Alignment.CenterVertically,
                        horizontalArrangement = Arrangement.SpaceBetween
                    ) {
                        Row(
                            verticalAlignment = Alignment.CenterVertically,
                            horizontalArrangement = Arrangement.spacedBy(12.dp)
                        ) {
                            IconButton(
                                onClick = onDismiss,
                                modifier = Modifier
                                    .background(MaterialTheme.colorScheme.surfaceVariant, shape = CircleShape)
                                    .size(36.dp)
                            ) {
                                Icon(
                                    imageVector = Icons.Filled.Close,
                                    contentDescription = "Kapat",
                                    tint = MaterialTheme.colorScheme.onSurfaceVariant,
                                    modifier = Modifier.size(18.dp)
                                )
                            }
                            Column {
                                Text(
                                    text = "Yeni Cari Hesap Kaydı",
                                    style = MaterialTheme.typography.titleMedium,
                                    fontWeight = FontWeight.Bold,
                                    color = MaterialTheme.colorScheme.onSurface
                                )
                            }
                        }

                        Button(
                            onClick = {
                                if (name.isBlank()) {
                                    nameError = true
                                } else {
                                    val randomId = "CUS-" + (10000..99999).random()
                                    val riskLimit = riskLimitText.toDoubleOrNull() ?: 50000.0
                                    val discount = discountText.toDoubleOrNull() ?: 0.0
                                    val newCust = Customer(
                                        id = randomId,
                                        name = name,
                                        balance = 0.0,
                                        lastVisit = "Yeni Kayıt",
                                        contact = contact,
                                        phone = phone,
                                        address = address,
                                        taxOffice = taxOffice,
                                        taxNumber = taxNumber,
                                        gpsLocation = "41.0745° N, 28.7951° E",
                                        riskLimit = riskLimit,
                                        priceGroup = priceGroup,
                                        specialDiscountPercent = discount,
                                        transactions = mutableStateListOf()
                                    )
                                    onSave(newCust)
                                }
                            },
                            shape = RoundedCornerShape(12.dp),
                            modifier = Modifier.testTag("submit_new_customer")
                        ) {
                            Icon(Icons.Filled.Save, contentDescription = null, modifier = Modifier.size(16.dp))
                            Spacer(modifier = Modifier.width(6.dp))
                            Text("Kaydet")
                        }
                    }
                }

                Column(
                    modifier = Modifier
                        .weight(1f)
                        .fillMaxWidth()
                        .verticalScroll(rememberScrollState())
                        .padding(20.dp),
                    verticalArrangement = Arrangement.spacedBy(16.dp)
                ) {
                    Text(
                        text = "Cari Kimlik & İletişim Bilgileri",
                        style = MaterialTheme.typography.titleSmall,
                        fontWeight = FontWeight.Bold,
                        color = MaterialTheme.colorScheme.primary
                    )

                    OutlinedTextField(
                        value = name,
                        onValueChange = { name = it; nameError = false },
                        label = { Text("Ünvan / Cari Adı *") },
                        isError = nameError,
                        singleLine = true,
                        modifier = Modifier.fillMaxWidth()
                    )
                    if (nameError) {
                        Text("Cari adı boş geçilemez!", color = MaterialTheme.colorScheme.error, style = MaterialTheme.typography.labelSmall)
                    }

                    OutlinedTextField(
                        value = contact,
                        onValueChange = { contact = it },
                        label = { Text("Yetkili Kişi") },
                        singleLine = true,
                        modifier = Modifier.fillMaxWidth()
                    )

                    OutlinedTextField(
                        value = phone,
                        onValueChange = { phone = it },
                        label = { Text("Telefon") },
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Phone),
                        singleLine = true,
                        modifier = Modifier.fillMaxWidth()
                    )

                    OutlinedTextField(
                        value = address,
                        onValueChange = { address = it },
                        label = { Text("Adres") },
                        maxLines = 3,
                        modifier = Modifier.fillMaxWidth()
                    )

                    HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant, modifier = Modifier.padding(vertical = 8.dp))

                    Text(
                        text = "Finansal & Vergi Parametreleri",
                        style = MaterialTheme.typography.titleSmall,
                        fontWeight = FontWeight.Bold,
                        color = MaterialTheme.colorScheme.primary
                    )

                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        OutlinedTextField(
                            value = taxOffice,
                            onValueChange = { taxOffice = it },
                            label = { Text("Vergi Dairesi") },
                            singleLine = true,
                            modifier = Modifier.weight(1f)
                        )
                        OutlinedTextField(
                            value = taxNumber,
                            onValueChange = { taxNumber = it },
                            label = { Text("Vergi No") },
                            keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                            singleLine = true,
                            modifier = Modifier.weight(1.2f)
                        )
                    }

                    OutlinedTextField(
                        value = riskLimitText,
                        onValueChange = { riskLimitText = it },
                        label = { Text("Risk Limiti (TL)") },
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                        singleLine = true,
                        modifier = Modifier.fillMaxWidth()
                    )

                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        OutlinedTextField(
                            value = discountText,
                            onValueChange = { discountText = it },
                            label = { Text("Özel İskonto (%)") },
                            keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                            singleLine = true,
                            modifier = Modifier.weight(1f)
                        )
                        
                        val priceGroups = AppDataStore.definitions["Müşteri"] ?: emptyList()
                        var expanded by remember { mutableStateOf(false) }
                        Box(modifier = Modifier.weight(1.2f)) {
                            OutlinedTextField(
                                value = priceGroup,
                                onValueChange = {},
                                readOnly = true,
                                label = { Text("Müşteri Tipi") },
                                trailingIcon = {
                                    IconButton(onClick = { expanded = true }) {
                                        Icon(Icons.Filled.ArrowDropDown, contentDescription = null)
                                    }
                                },
                                modifier = Modifier.fillMaxWidth().clickable { expanded = true }
                            )
                            DropdownMenu(
                                expanded = expanded,
                                onDismissRequest = { expanded = false }
                            ) {
                                priceGroups.forEach { group ->
                                    DropdownMenuItem(
                                        text = { Text(group) },
                                        onClick = {
                                            priceGroup = group
                                            expanded = false
                                        }
                                    )
                                }
                            }
                        }
                    }

                    Spacer(modifier = Modifier.height(30.dp))
                }
            }
        }
    }
}

@Composable
fun EditCustomerDialog(
    customer: Customer,
    onDismiss: () -> Unit,
    onSave: (Customer) -> Unit
) {
    var name by remember { mutableStateOf(customer.name) }
    var contact by remember { mutableStateOf(customer.contact) }
    var phone by remember { mutableStateOf(customer.phone) }
    var address by remember { mutableStateOf(customer.address) }
    var taxOffice by remember { mutableStateOf(customer.taxOffice) }
    var taxNumber by remember { mutableStateOf(customer.taxNumber) }
    var priceGroup by remember { mutableStateOf(customer.priceGroup) }
    var riskLimitText by remember { mutableStateOf(customer.riskLimit.toInt().toString()) }
    var discountText by remember { mutableStateOf(customer.specialDiscountPercent.toInt().toString()) }
    
    var nameError by remember { mutableStateOf(false) }

    Dialog(
        onDismissRequest = onDismiss,
        properties = DialogProperties(usePlatformDefaultWidth = false)
    ) {
        Surface(
            modifier = Modifier.fillMaxSize(),
            color = MaterialTheme.colorScheme.background
        ) {
            Column(
                modifier = Modifier
                    .fillMaxSize()
                    .windowInsetsPadding(WindowInsets.statusBars)
            ) {
                // Custom Navbar / Top Bar for Full Screen Dialog
                Card(
                    modifier = Modifier.fillMaxWidth(),
                    shape = RoundedCornerShape(0.dp),
                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
                    elevation = CardDefaults.cardElevation(defaultElevation = 2.dp)
                ) {
                    Row(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(horizontal = 16.dp, vertical = 14.dp),
                        verticalAlignment = Alignment.CenterVertically,
                        horizontalArrangement = Arrangement.SpaceBetween
                    ) {
                        Row(
                            verticalAlignment = Alignment.CenterVertically,
                            horizontalArrangement = Arrangement.spacedBy(12.dp)
                        ) {
                            IconButton(
                                onClick = onDismiss,
                                modifier = Modifier
                                    .background(MaterialTheme.colorScheme.surfaceVariant, shape = CircleShape)
                                    .size(36.dp)
                            ) {
                                Icon(
                                    imageVector = Icons.Filled.Close,
                                    contentDescription = "Kapat",
                                    tint = MaterialTheme.colorScheme.onSurfaceVariant,
                                    modifier = Modifier.size(18.dp)
                                )
                            }
                            Column {
                                Text(
                                    text = "Cari Hesap Düzenle",
                                    style = MaterialTheme.typography.titleMedium,
                                    fontWeight = FontWeight.Bold,
                                    color = MaterialTheme.colorScheme.onSurface
                                )
                                Text(
                                    text = "Kod: ${customer.id}",
                                    style = MaterialTheme.typography.labelSmall,
                                    color = MaterialTheme.colorScheme.outline
                                )
                            }
                        }

                        Button(
                            onClick = {
                                if (name.isBlank()) {
                                    nameError = true
                                } else {
                                    val riskLimit = riskLimitText.toDoubleOrNull() ?: customer.riskLimit
                                    val discount = discountText.toDoubleOrNull() ?: customer.specialDiscountPercent
                                    val updatedCust = customer.copy(
                                        name = name,
                                        contact = contact,
                                        phone = phone,
                                        address = address,
                                        taxOffice = taxOffice,
                                        taxNumber = taxNumber,
                                        riskLimit = riskLimit,
                                        priceGroup = priceGroup,
                                        specialDiscountPercent = discount
                                    )
                                    onSave(updatedCust)
                                }
                            },
                            shape = RoundedCornerShape(12.dp),
                            modifier = Modifier.testTag("save_customer_edit_btn")
                        ) {
                            Icon(Icons.Filled.Save, contentDescription = null, modifier = Modifier.size(16.dp))
                            Spacer(modifier = Modifier.width(6.dp))
                            Text("Kaydet")
                        }
                    }
                }

                Column(
                    modifier = Modifier
                        .weight(1f)
                        .fillMaxWidth()
                        .verticalScroll(rememberScrollState())
                        .padding(20.dp),
                    verticalArrangement = Arrangement.spacedBy(16.dp)
                ) {
                    Text(
                        text = "Cari Kimlik & İletişim Bilgileri",
                        style = MaterialTheme.typography.titleSmall,
                        fontWeight = FontWeight.Bold,
                        color = MaterialTheme.colorScheme.primary
                    )

                    OutlinedTextField(
                        value = name,
                        onValueChange = { name = it; nameError = false },
                        label = { Text("Ünvan / Cari Adı *") },
                        isError = nameError,
                        singleLine = true,
                        modifier = Modifier.fillMaxWidth()
                    )
                    if (nameError) {
                        Text("Cari adı boş geçilemez!", color = MaterialTheme.colorScheme.error, style = MaterialTheme.typography.labelSmall)
                    }

                    OutlinedTextField(
                        value = contact,
                        onValueChange = { contact = it },
                        label = { Text("Yetkili Kişi") },
                        singleLine = true,
                        modifier = Modifier.fillMaxWidth()
                    )

                    OutlinedTextField(
                        value = phone,
                        onValueChange = { phone = it },
                        label = { Text("Telefon") },
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Phone),
                        singleLine = true,
                        modifier = Modifier.fillMaxWidth()
                    )

                    OutlinedTextField(
                        value = address,
                        onValueChange = { address = it },
                        label = { Text("Adres") },
                        maxLines = 3,
                        modifier = Modifier.fillMaxWidth()
                    )

                    HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant, modifier = Modifier.padding(vertical = 8.dp))

                    Text(
                        text = "Finansal & Vergi Parametreleri",
                        style = MaterialTheme.typography.titleSmall,
                        fontWeight = FontWeight.Bold,
                        color = MaterialTheme.colorScheme.primary
                    )

                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        OutlinedTextField(
                            value = taxOffice,
                            onValueChange = { taxOffice = it },
                            label = { Text("Vergi Dairesi") },
                            singleLine = true,
                            modifier = Modifier.weight(1f)
                        )
                        OutlinedTextField(
                            value = taxNumber,
                            onValueChange = { taxNumber = it },
                            label = { Text("Vergi No") },
                            keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                            singleLine = true,
                            modifier = Modifier.weight(1.2f)
                        )
                    }

                    OutlinedTextField(
                        value = riskLimitText,
                        onValueChange = { riskLimitText = it },
                        label = { Text("Risk Limiti (TL)") },
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                        singleLine = true,
                        modifier = Modifier.fillMaxWidth()
                    )

                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        OutlinedTextField(
                            value = discountText,
                            onValueChange = { discountText = it },
                            label = { Text("Özel İskonto (%)") },
                            keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                            singleLine = true,
                            modifier = Modifier.weight(1f)
                        )
                        
                        val priceGroups = AppDataStore.definitions["Müşteri"] ?: emptyList()
                        var expanded by remember { mutableStateOf(false) }
                        Box(modifier = Modifier.weight(1.2f)) {
                            OutlinedTextField(
                                value = priceGroup,
                                onValueChange = {},
                                readOnly = true,
                                label = { Text("Müşteri Tipi") },
                                trailingIcon = {
                                    IconButton(onClick = { expanded = true }) {
                                        Icon(Icons.Filled.ArrowDropDown, contentDescription = null)
                                    }
                                },
                                modifier = Modifier.fillMaxWidth().clickable { expanded = true }
                            )
                            DropdownMenu(
                                expanded = expanded,
                                onDismissRequest = { expanded = false }
                            ) {
                                priceGroups.forEach { group ->
                                    DropdownMenuItem(
                                        text = { Text(group) },
                                        onClick = {
                                            priceGroup = group
                                            expanded = false
                                        }
                                    )
                                }
                            }
                        }
                    }

                    Spacer(modifier = Modifier.height(30.dp))
                }
            }
        }
    }
}

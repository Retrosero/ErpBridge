package com.example.ui.screens

import android.Manifest
import android.content.Context
import android.widget.Toast
import androidx.camera.core.CameraSelector
import com.example.data.database.WmsOrderEntity
import com.example.data.database.WmsOrderItemEntity
import androidx.compose.ui.text.TextStyle
import androidx.camera.core.ImageAnalysis
import androidx.camera.core.ImageProxy
import androidx.camera.core.Preview
import androidx.camera.lifecycle.ProcessCameraProvider
import androidx.camera.view.PreviewView
import androidx.compose.animation.*
import androidx.compose.foundation.*
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material.icons.outlined.QrCodeScanner
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.LocalLifecycleOwner
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.foundation.text.BasicTextField
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.compose.ui.viewinterop.AndroidView
import androidx.compose.ui.window.Dialog
import androidx.compose.ui.window.DialogProperties
import androidx.core.content.ContextCompat
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.NavController
import com.google.accompanist.permissions.ExperimentalPermissionsApi
import com.google.accompanist.permissions.isGranted
import com.google.accompanist.permissions.rememberPermissionState
import com.google.mlkit.vision.barcode.BarcodeScanning
import com.google.mlkit.vision.common.InputImage
import java.util.concurrent.Executors

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun WarehouseScreen(navController: NavController) {
    val context = LocalContext.current
    val viewModel = remember { WarehouseViewModel(context) }

    val orders by viewModel.orders.collectAsStateWithLifecycle()
    val activeOrder by viewModel.activeOrder.collectAsStateWithLifecycle()
    val activeOrderItems by viewModel.activeOrderItems.collectAsStateWithLifecycle()
    val isSyncing by viewModel.isSyncing.collectAsStateWithLifecycle()
    val syncStatusText by viewModel.syncStatusText.collectAsStateWithLifecycle()
    val scanFeedback by viewModel.scanFeedback.collectAsStateWithLifecycle()

    var showScannerDialog by AppDataStore.wmsShowScannerDialog
    var selectedTab by AppDataStore.wmsSelectedTab
    var searchQuery by remember { mutableStateOf("") }

    LaunchedEffect(selectedTab) {
        viewModel.clearActiveOrder()
        viewModel.loadOrders()
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = {
                    if (activeOrder == null) {
                        Text(
                            text = "Depo Sevkiyat Modülü",
                            style = MaterialTheme.typography.titleMedium.copy(fontWeight = FontWeight.Bold)
                        )
                    } else {
                        val order = activeOrder!!
                        Row(
                            modifier = Modifier.fillMaxWidth().padding(end = 4.dp),
                            verticalAlignment = Alignment.CenterVertically,
                            horizontalArrangement = Arrangement.SpaceBetween
                        ) {
                            Column {
                                Text(
                                    text = order.customerName,
                                    style = MaterialTheme.typography.titleSmall.copy(fontWeight = FontWeight.Bold),
                                    maxLines = 1,
                                    overflow = androidx.compose.ui.text.style.TextOverflow.Ellipsis
                                )
                                Text(
                                    text = "Sipariş: ${order.id} • ${order.totalItems} Kalem",
                                    style = MaterialTheme.typography.labelSmall.copy(
                                        color = MaterialTheme.colorScheme.onPrimaryContainer.copy(alpha = 0.8f)
                                    ),
                                    maxLines = 1
                                )
                            }
                        }
                    }
                },
                navigationIcon = {
                    if (activeOrder != null) {
                        IconButton(onClick = { viewModel.clearActiveOrder() }) {
                            Icon(Icons.Filled.ArrowBack, contentDescription = "Geri")
                        }
                    } else {
                        IconButton(onClick = { navController.popBackStack() }) {
                            Icon(Icons.Filled.ArrowBack, contentDescription = "Ana Menü")
                        }
                    }
                },
                actions = {
                    if (activeOrder == null) {
                        IconButton(onClick = { viewModel.syncAll() }) {
                            if (isSyncing) {
                                CircularProgressIndicator(modifier = Modifier.size(24.dp), strokeWidth = 2.dp, color = MaterialTheme.colorScheme.onPrimaryContainer)
                            } else {
                                Icon(Icons.Filled.Sync, contentDescription = "Senkronize Et")
                            }
                        }
                    } else {
                        IconButton(onClick = { showScannerDialog = true }) {
                            Icon(Icons.Filled.QrCodeScanner, contentDescription = "Barkod Tarat", modifier = Modifier.testTag("wms_toolbar_scan_btn"))
                        }
                    }
                },
                colors = TopAppBarDefaults.topAppBarColors(
                    containerColor = MaterialTheme.colorScheme.primaryContainer,
                    titleContentColor = MaterialTheme.colorScheme.onPrimaryContainer,
                    navigationIconContentColor = MaterialTheme.colorScheme.onPrimaryContainer,
                    actionIconContentColor = MaterialTheme.colorScheme.onPrimaryContainer
                )
            )
        },
        bottomBar = {
            if (activeOrder != null && (activeOrder!!.status == "Bekleyen" || activeOrder!!.status == "Toplanıyor")) {
                Surface(
                    tonalElevation = 6.dp,
                    shadowElevation = 8.dp,
                    color = MaterialTheme.colorScheme.surface,
                    modifier = Modifier.fillMaxWidth()
                ) {
                    Row(
                        modifier = Modifier
                            .fillMaxWidth()
                            .navigationBarsPadding()
                            .padding(horizontal = 16.dp, vertical = 8.dp),
                        verticalAlignment = Alignment.CenterVertically,
                        horizontalArrangement = Arrangement.SpaceBetween
                    ) {
                        val pickedCount = activeOrderItems.count { it.isPicked }
                        val totalCount = activeOrderItems.size
                        Column {
                            Text(
                                text = "Toplama İlerlemesi",
                                style = MaterialTheme.typography.labelSmall,
                                color = MaterialTheme.colorScheme.outline
                            )
                            Text(
                                text = "$pickedCount / $totalCount Ürün",
                                style = MaterialTheme.typography.titleMedium.copy(fontWeight = FontWeight.Bold),
                                color = MaterialTheme.colorScheme.primary
                            )
                        }

                        Button(
                            onClick = {
                                viewModel.finishPickingAndSendToControl()
                            },
                            colors = ButtonDefaults.buttonColors(
                                containerColor = Color(0xFF2E7D32), // Vibrant Success/Emerald Green
                                contentColor = Color.White
                            ),
                            shape = RoundedCornerShape(10.dp),
                            modifier = Modifier
                                .height(40.dp)
                                .testTag("wms_finish_picking_btn"),
                        ) {
                            Icon(
                                imageVector = Icons.Filled.CheckCircle,
                                contentDescription = null,
                                modifier = Modifier.size(16.dp)
                            )
                            Spacer(modifier = Modifier.width(6.dp))
                            Text(
                                text = "Bitir ve Onaya Gönder",
                                style = MaterialTheme.typography.labelLarge.copy(fontWeight = FontWeight.Bold)
                            )
                        }
                    }
                }
            }
        }
    ) { paddingValues ->
        Box(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
                .background(MaterialTheme.colorScheme.background)
        ) {
            Column(modifier = Modifier.fillMaxSize()) {
                // Sync notification bar
                if (syncStatusText.isNotEmpty()) {
                    Card(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(8.dp),
                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.secondaryContainer)
                    ) {
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(12.dp),
                            horizontalArrangement = Arrangement.spacedBy(10.dp),
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            CircularProgressIndicator(modifier = Modifier.size(20.dp), strokeWidth = 2.dp)
                            Text(text = syncStatusText, style = MaterialTheme.typography.bodyMedium)
                        }
                    }
                }

                // Scan feedback notification bar
                if (scanFeedback != null) {
                    Card(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(8.dp),
                        colors = CardDefaults.cardColors(
                            containerColor = if (scanFeedback!!.contains("Hata")) MaterialTheme.colorScheme.errorContainer else MaterialTheme.colorScheme.tertiaryContainer
                        )
                    ) {
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(12.dp),
                            horizontalArrangement = Arrangement.spacedBy(8.dp),
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Icon(
                                imageVector = if (scanFeedback!!.contains("Hata")) Icons.Filled.Error else Icons.Filled.CheckCircle,
                                contentDescription = "Feedback Status",
                                tint = if (scanFeedback!!.contains("Hata")) MaterialTheme.colorScheme.error else MaterialTheme.colorScheme.tertiary
                            )
                            Text(
                                text = scanFeedback!!,
                                style = MaterialTheme.typography.bodyLarge.copy(fontWeight = FontWeight.SemiBold),
                                modifier = Modifier.weight(1f)
                            )
                        }
                    }
                }

                if (activeOrder == null) {
                    Row(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(horizontal = 16.dp, vertical = 8.dp)
                            .background(
                                MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f),
                                RoundedCornerShape(16.dp)
                            )
                            .padding(4.dp),
                        horizontalArrangement = Arrangement.spacedBy(4.dp)
                    ) {
                        val tabs = listOf(
                            Triple(0, "Toplama", Icons.Filled.List),
                            Triple(1, "Kontrol / Onay", Icons.Filled.FactCheck),
                            Triple(2, "Sevk & Teslim", Icons.Filled.LocalShipping)
                        )
                        tabs.forEach { (index, title, icon) ->
                            val isSelected = selectedTab == index
                            Box(
                                modifier = Modifier
                                    .weight(1f)
                                    .clip(RoundedCornerShape(12.dp))
                                    .background(
                                        if (isSelected) MaterialTheme.colorScheme.primary 
                                        else Color.Transparent
                                    )
                                    .clickable { selectedTab = index }
                                    .padding(vertical = 10.dp),
                                contentAlignment = Alignment.Center
                            ) {
                                Row(
                                    verticalAlignment = Alignment.CenterVertically,
                                    horizontalArrangement = Arrangement.Center
                                ) {
                                    Icon(
                                        icon,
                                        contentDescription = null,
                                        tint = if (isSelected) MaterialTheme.colorScheme.onPrimary 
                                               else MaterialTheme.colorScheme.onSurfaceVariant,
                                        modifier = Modifier.size(16.dp)
                                    )
                                    Spacer(modifier = Modifier.width(6.dp))
                                    Text(
                                        text = title,
                                        style = MaterialTheme.typography.bodyMedium.copy(fontWeight = FontWeight.Bold),
                                        color = if (isSelected) MaterialTheme.colorScheme.onPrimary 
                                                else MaterialTheme.colorScheme.onSurfaceVariant
                                    )
                                }
                            }
                        }
                    }

                    val filteredOrders = remember(orders, selectedTab) {
                        orders.filter { order ->
                            when (selectedTab) {
                                0 -> order.status == "Bekleyen" || order.status == "Toplanıyor"
                                1 -> order.status == "Toplandı" || order.status == "Onay Bekliyor"
                                else -> order.status == "Koli Hazırlandı" || order.status == "Sevk Edildi"
                            }
                        }
                    }

                    if (filteredOrders.isEmpty()) {
                        Box(
                            modifier = Modifier
                                .weight(1f)
                                .fillMaxWidth(),
                            contentAlignment = Alignment.Center
                        ) {
                            Column(
                                horizontalAlignment = Alignment.CenterHorizontally,
                                verticalArrangement = Arrangement.spacedBy(12.dp)
                            ) {
                                Icon(Icons.Filled.Inbox, contentDescription = null, modifier = Modifier.size(64.dp), tint = MaterialTheme.colorScheme.outline)
                                Text(
                                    text = "Bu aşamada sipariş bulunmamaktadır.",
                                    style = MaterialTheme.typography.bodyLarge,
                                    color = MaterialTheme.colorScheme.outline
                                )
                                Button(onClick = { viewModel.syncAll() }) {
                                    Icon(Icons.Filled.Sync, contentDescription = null)
                                    Spacer(modifier = Modifier.width(8.dp))
                                    Text("ERP Güncellemesi Çek")
                                }
                            }
                        }
                    } else {
                        LazyColumn(
                            modifier = Modifier
                                .weight(1f)
                                .fillMaxWidth()
                                .testTag("wms_orders_list"),
                            contentPadding = PaddingValues(start = 16.dp, end = 16.dp, bottom = 16.dp, top = 0.dp),
                            verticalArrangement = Arrangement.spacedBy(12.dp)
                        ) {
                            items(filteredOrders) { order ->
                                OrderCard(
                                    order = order,
                                    onClick = { viewModel.selectOrder(order) }
                                )
                            }
                        }
                    }
                } else {
                    // Order details view (Picking panel and Pack & Ship panels)
                    val order = activeOrder!!

                    Column(
                        modifier = Modifier
                            .weight(1f)
                            .fillMaxWidth()
                            .padding(start = 8.dp, end = 8.dp, bottom = 4.dp, top = 0.dp),
                        verticalArrangement = Arrangement.spacedBy(6.dp)
                    ) {
                        // State based navigation inside order detail
                        when (order.status) {
                            "Bekleyen", "Toplanıyor" -> {
                                val filteredItems = remember(activeOrderItems, searchQuery) {
                                    if (searchQuery.isBlank()) {
                                        activeOrderItems
                                    } else {
                                        val filter = searchQuery.lowercase().trim()
                                        activeOrderItems.filter { item ->
                                            item.productTitle.lowercase().contains(filter) ||
                                            item.productBarcode.lowercase().contains(filter) ||
                                            (item.shelfLocation ?: "").lowercase().contains(filter)
                                        }
                                    }
                                }

                                // Search bar and scanner combined in a beautiful, thin, single horizontal row
                                Row(
                                    modifier = Modifier.fillMaxWidth().padding(vertical = 2.dp),
                                    horizontalArrangement = Arrangement.spacedBy(8.dp),
                                    verticalAlignment = Alignment.CenterVertically
                                ) {
                                    OutlinedTextField(
                                        value = searchQuery,
                                        onValueChange = { searchQuery = it },
                                        modifier = Modifier
                                            .weight(1f)
                                            .heightIn(max = 44.dp)
                                            .testTag("wms_search_input"),
                                        placeholder = { Text("Ürün adı veya barkod ara...", style = MaterialTheme.typography.bodyMedium) },
                                        leadingIcon = { Icon(Icons.Filled.Search, contentDescription = "Ara", modifier = Modifier.size(18.dp)) },
                                        trailingIcon = {
                                            if (searchQuery.isNotEmpty()) {
                                                IconButton(onClick = { searchQuery = "" }, modifier = Modifier.size(24.dp)) {
                                                    Icon(Icons.Filled.Clear, contentDescription = "Temizle", modifier = Modifier.size(16.dp))
                                                }
                                            }
                                        },
                                        singleLine = true,
                                        shape = RoundedCornerShape(8.dp),
                                        colors = OutlinedTextFieldDefaults.colors(
                                            focusedBorderColor = MaterialTheme.colorScheme.primary,
                                            unfocusedBorderColor = MaterialTheme.colorScheme.outline.copy(alpha = 0.3f)
                                        )
                                    )

                                    IconButton(
                                        onClick = { showScannerDialog = true },
                                        colors = IconButtonDefaults.iconButtonColors(containerColor = MaterialTheme.colorScheme.primaryContainer),
                                        modifier = Modifier.size(44.dp)
                                    ) {
                                        Icon(Icons.Filled.QrCodeScanner, contentDescription = "Tarat", tint = MaterialTheme.colorScheme.onPrimaryContainer, modifier = Modifier.size(18.dp))
                                    }
                                }

                                Row(
                                    modifier = Modifier.fillMaxWidth().padding(horizontal = 4.dp),
                                    horizontalArrangement = Arrangement.SpaceBetween,
                                    verticalAlignment = Alignment.CenterVertically
                                ) {
                                    Text(
                                        text = "Toplama Listesi (${filteredItems.size}/${activeOrderItems.size})",
                                        style = MaterialTheme.typography.bodyMedium.copy(fontWeight = FontWeight.Bold),
                                        color = MaterialTheme.colorScheme.onSurface
                                    )
                                }

                                LazyColumn(
                                    modifier = Modifier
                                        .weight(1f)
                                        .fillMaxWidth()
                                        .testTag("wms_picking_list"),
                                    verticalArrangement = Arrangement.spacedBy(6.dp)
                                ) {
                                    items(filteredItems) { item ->
                                        PickingItemCard(
                                            item = item,
                                            onConfirmManual = { viewModel.pickItemManually(item) },
                                            onReset = { viewModel.resetItemPicked(item) },
                                            onUpdateQuantity = { increment -> viewModel.updateItemQuantity(item, increment) },
                                            onSetQuantity = { qty -> viewModel.setItemQuantity(item, qty) }
                                        )
                                    }
                                }
                            }
                            "Toplandı" -> {
                                // Control & Verification Checklist Screen
                                Row(
                                    modifier = Modifier.fillMaxWidth(),
                                    horizontalArrangement = Arrangement.SpaceBetween,
                                    verticalAlignment = Alignment.CenterVertically
                                ) {
                                    Text(
                                        text = "Fiziki Kontrol ve Kabul Listesi",
                                        style = MaterialTheme.typography.titleMedium,
                                        fontWeight = FontWeight.Bold
                                    )
                                    val discrepancyCount = remember(activeOrderItems) {
                                        activeOrderItems.count { it.quantityPicked != it.quantityOrdered }
                                    }
                                    if (discrepancyCount > 0) {
                                        Surface(
                                            color = MaterialTheme.colorScheme.errorContainer,
                                            shape = RoundedCornerShape(6.dp)
                                        ) {
                                            Text(
                                                text = "$discrepancyCount Uyuşmazlık!",
                                                style = MaterialTheme.typography.labelSmall.copy(fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.error),
                                                modifier = Modifier.padding(horizontal = 6.dp, vertical = 3.dp)
                                            )
                                        }
                                    }
                                }

                                Card(
                                    modifier = Modifier.fillMaxWidth(),
                                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.25f))
                                ) {
                                    Row(
                                        modifier = Modifier.padding(12.dp),
                                        verticalAlignment = Alignment.CenterVertically,
                                        horizontalArrangement = Arrangement.spacedBy(10.dp)
                                    ) {
                                        Icon(Icons.Filled.FactCheck, contentDescription = null, tint = MaterialTheme.colorScheme.primary)
                                        Text(
                                            text = "Aşağıdaki toplanan ürünlerin fiziki miktarlarını, eksik veya fazlalık durumlarını teyit edip sevkiyat paketlemesini onaylayın.",
                                            style = MaterialTheme.typography.bodySmall,
                                            color = MaterialTheme.colorScheme.onPrimaryContainer
                                        )
                                    }
                                }

                                LazyColumn(
                                    modifier = Modifier
                                        .weight(1f)
                                        .fillMaxWidth()
                                        .testTag("wms_control_list"),
                                    verticalArrangement = Arrangement.spacedBy(10.dp)
                                ) {
                                    items(activeOrderItems) { item ->
                                        ControlItemCard(item = item)
                                    }
                                }

                                Button(
                                    onClick = {
                                        viewModel.generatePackageBarcode(order.id) {
                                            Toast.makeText(context, "Miktarlar kontrol edildi. Nihai Onay için merkeze gönderildi!", Toast.LENGTH_LONG).show()
                                        }
                                    },
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .height(56.dp)
                                        .testTag("wms_generate_box_btn"),
                                    shape = RoundedCornerShape(12.dp),
                                    colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.primary)
                                ) {
                                    Icon(Icons.Filled.Send, contentDescription = null)
                                    Spacer(modifier = Modifier.width(8.dp))
                                    Text("Fişi Güncelle ve Onaya Gönder", style = MaterialTheme.typography.bodyLarge.copy(fontWeight = FontWeight.Bold))
                                }
                            }
                            "Onay Bekliyor" -> {
                                Box(
                                    modifier = Modifier
                                        .weight(1f)
                                        .fillMaxWidth(),
                                    contentAlignment = Alignment.Center
                                ) {
                                    Card(
                                        modifier = Modifier.fillMaxWidth(),
                                        colors = CardDefaults.cardColors(
                                            containerColor = MaterialTheme.colorScheme.secondaryContainer.copy(alpha = 0.5f)
                                        ),
                                        border = androidx.compose.foundation.BorderStroke(1.dp, MaterialTheme.colorScheme.secondary.copy(alpha = 0.3f))
                                    ) {
                                        Column(
                                            modifier = Modifier.padding(24.dp),
                                            horizontalAlignment = Alignment.CenterHorizontally,
                                            verticalArrangement = Arrangement.spacedBy(16.dp)
                                        ) {
                                            Icon(
                                                imageVector = Icons.Filled.HourglassEmpty,
                                                contentDescription = null,
                                                modifier = Modifier.size(72.dp),
                                                tint = MaterialTheme.colorScheme.secondary
                                            )
                                            Text(
                                                text = "Merkez Onayı Bekleniyor",
                                                style = MaterialTheme.typography.titleLarge.copy(fontWeight = FontWeight.Bold),
                                                textAlign = TextAlign.Center,
                                                color = MaterialTheme.colorScheme.onSecondaryContainer
                                            )
                                            Text(
                                                text = "Miktar kontrolü sonrasında düzenlenen fiş onay masasına iletilmiştir. Cari bakiye ve fatura güncellenmesi onaylandığı anda araç sevkiyatına devam edebilirsiniz.",
                                                style = MaterialTheme.typography.bodyMedium,
                                                textAlign = TextAlign.Center,
                                                color = MaterialTheme.colorScheme.onSecondaryContainer.copy(alpha = 0.8f)
                                            )
                                            
                                            Divider(color = MaterialTheme.colorScheme.onSecondaryContainer.copy(alpha = 0.2f))
                                             
                                            Text(
                                                text = "Onaylanacak Sevkiyat Listesi",
                                                style = MaterialTheme.typography.bodyMedium.copy(fontWeight = FontWeight.Bold),
                                                color = MaterialTheme.colorScheme.primary
                                            )
                                             
                                            Card(
                                                modifier = Modifier.fillMaxWidth(),
                                                colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface)
                                            ) {
                                                Column(
                                                    modifier = Modifier.padding(12.dp),
                                                    verticalArrangement = Arrangement.spacedBy(8.dp)
                                                 ) {
                                                     for (item in activeOrderItems) {
                                                         Row(
                                                             modifier = Modifier.fillMaxWidth(),
                                                             horizontalArrangement = Arrangement.SpaceBetween
                                                         ) {
                                                             Text(
                                                                 text = item.productTitle,
                                                                 style = MaterialTheme.typography.bodySmall.copy(fontWeight = FontWeight.SemiBold),
                                                                 modifier = Modifier.weight(1f)
                                                             )
                                                             Text(
                                                                 text = "${item.quantityPicked} Adet",
                                                                 style = MaterialTheme.typography.bodySmall.copy(fontWeight = FontWeight.Bold),
                                                                 color = if (item.quantityPicked != item.quantityOrdered) MaterialTheme.colorScheme.error else MaterialTheme.colorScheme.onSurface
                                                             )
                                                         }
                                                     }
                                                 }
                                             }
                                             
                                             Button(
                                                 onClick = { viewModel.clearActiveOrder() },
                                                 modifier = Modifier.fillMaxWidth()
                                             ) {
                                                 Text("Sipariş Listesine Geri Dön")
                                             }
                                         }
                                     }
                                 }
                             }
                            "Koli Hazırlandı" -> {
                                // Vehicle Assignment & Shipping Layout Screen
                                var plateInput by remember { mutableStateOf("") }
                                Box(
                                    modifier = Modifier
                                        .weight(1f)
                                        .fillMaxWidth(),
                                    contentAlignment = Alignment.Center
                                ) {
                                    Card(
                                        modifier = Modifier.fillMaxWidth(),
                                        elevation = CardDefaults.cardElevation(defaultElevation = 2.dp)
                                    ) {
                                        Column(
                                            modifier = Modifier.padding(24.dp),
                                            horizontalAlignment = Alignment.CenterHorizontally,
                                            verticalArrangement = Arrangement.spacedBy(16.dp)
                                        ) {
                                            Icon(Icons.Filled.LocalShipping, contentDescription = null, modifier = Modifier.size(72.dp), tint = MaterialTheme.colorScheme.secondary)
                                            Text(
                                                text = "Kolileme Hazır!",
                                                style = MaterialTheme.typography.titleLarge.copy(fontWeight = FontWeight.Bold),
                                                textAlign = TextAlign.Center
                                            )
                                            Text(
                                                text = "Koli Barkodu: ${order.packageBarcode ?: "Bilinmiyor"}",
                                                style = MaterialTheme.typography.bodyMedium,
                                                fontWeight = FontWeight.Bold,
                                                color = MaterialTheme.colorScheme.primary
                                            )
                                            Text(
                                                text = "Sevkiyat yapacak araç plakasını eşitleyerek sevkiyat çıkış zimmetini yapın.",
                                                style = MaterialTheme.typography.bodySmall,
                                                textAlign = TextAlign.Center,
                                                color = MaterialTheme.colorScheme.outline
                                            )

                                            OutlinedTextField(
                                                value = plateInput,
                                                onValueChange = { plateInput = it.uppercase() },
                                                label = { Text("Sevkiyat Araç Plakası") },
                                                placeholder = { Text("Örn: 34 ABC 123") },
                                                leadingIcon = { Icon(Icons.Filled.Badge, contentDescription = null) },
                                                modifier = Modifier
                                                    .fillMaxWidth()
                                                    .testTag("wms_plate_input"),
                                                singleLine = true
                                            )

                                            Button(
                                                onClick = {
                                                    if (plateInput.isBlank()) {
                                                        Toast.makeText(context, "Lütfen plaka girin!", Toast.LENGTH_SHORT).show()
                                                    } else {
                                                        viewModel.checkOutVehicle(order.id, plateInput) {
                                                            Toast.makeText(context, "Zimmetleme ve Sevkiyat Başarılı!", Toast.LENGTH_SHORT).show()
                                                        }
                                                    }
                                                },
                                                modifier = Modifier
                                                    .fillMaxWidth()
                                                    .height(56.dp)
                                                    .testTag("wms_checkout_btn"),
                                                shape = RoundedCornerShape(12.dp)
                                            ) {
                                                Icon(Icons.Filled.LocalShipping, contentDescription = null)
                                                Spacer(modifier = Modifier.width(8.dp))
                                                Text("Araca Zimmetle / Sevk Et", style = MaterialTheme.typography.bodyLarge.copy(fontWeight = FontWeight.Bold))
                                            }
                                        }
                                    }
                                }
                            }
                            "Sevk Edildi" -> {
                                // Complete screen layout
                                Box(
                                    modifier = Modifier
                                        .weight(1f)
                                        .fillMaxWidth(),
                                    contentAlignment = Alignment.Center
                                ) {
                                    Card(
                                        modifier = Modifier.fillMaxWidth(),
                                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.tertiaryContainer)
                                    ) {
                                        Column(
                                            modifier = Modifier.padding(24.dp),
                                            horizontalAlignment = Alignment.CenterHorizontally,
                                            verticalArrangement = Arrangement.spacedBy(16.dp)
                                        ) {
                                            Icon(Icons.Filled.CloudDone, contentDescription = null, modifier = Modifier.size(80.dp), tint = MaterialTheme.colorScheme.tertiary)
                                            Text(
                                                text = "Sevkiyat Tamamlandı!",
                                                style = MaterialTheme.typography.titleLarge.copy(fontWeight = FontWeight.Bold),
                                                textAlign = TextAlign.Center,
                                                color = MaterialTheme.colorScheme.onTertiaryContainer
                                            )
                                            Text(
                                                text = "Siparişiniz başarılı bir şekilde araç plaka ${order.vehiclePlate} zimmetinde kargoya verilmiştir.",
                                                style = MaterialTheme.typography.bodyMedium,
                                                textAlign = TextAlign.Center,
                                                color = MaterialTheme.colorScheme.onTertiaryContainer
                                            )
                                            Text(
                                                text = "Koli takip barkodu: ${order.packageBarcode}",
                                                style = MaterialTheme.typography.bodyMedium,
                                                fontWeight = FontWeight.Bold,
                                                color = MaterialTheme.colorScheme.onTertiaryContainer
                                            )

                                            Button(
                                                onClick = { viewModel.clearActiveOrder() },
                                                modifier = Modifier.fillMaxWidth()
                                            ) {
                                                Text("Sipariş Listesine Geri Dön")
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

    if (showScannerDialog) {
        WmsBarcodeScannerDialog(
            onDismissRequest = { showScannerDialog = false },
            onBarcodeScanned = { barcode ->
                val detected = viewModel.onBarcodeScanned(barcode)
                if (detected) {
                    showScannerDialog = false
                }
            }
        )
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun OrderCard(
    order: WmsOrderEntity,
    onClick: () -> Unit
) {
    Card(
        modifier = Modifier
            .fillMaxWidth()
            .clickable(onClick = onClick)
            .testTag("wms_order_item_${order.id}"),
        elevation = CardDefaults.cardElevation(defaultElevation = 2.dp)
    ) {
        Column(modifier = Modifier.padding(16.dp)) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                Text(
                    text = order.id,
                    style = MaterialTheme.typography.titleMedium.copy(fontWeight = FontWeight.Bold),
                    color = MaterialTheme.colorScheme.primary
                )
                Text(text = order.orderDate, style = MaterialTheme.typography.bodyMedium)
            }
            Text(
                text = order.customerName,
                style = MaterialTheme.typography.bodyLarge,
                modifier = Modifier.padding(vertical = 4.dp),
                maxLines = 1
            )
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                Row(
                    verticalAlignment = Alignment.CenterVertically,
                    horizontalArrangement = Arrangement.spacedBy(4.dp)
                ) {
                    val statusColor = when (order.status) {
                        "Bekleyen" -> MaterialTheme.colorScheme.error
                        "Toplanıyor" -> MaterialTheme.colorScheme.primary
                        else -> MaterialTheme.colorScheme.tertiary
                    }
                    Box(
                        modifier = Modifier
                            .size(10.dp)
                            .clip(RoundedCornerShape(5.dp))
                            .background(statusColor)
                    )
                    Text(text = order.status, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold, color = statusColor)
                }

                Text(
                    text = "${order.totalItems} Adet Kalem",
                    style = MaterialTheme.typography.bodyLarge,
                    fontWeight = FontWeight.Bold,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
            }
        }
    }
}

@Composable
fun PickingItemCard(
    item: WmsOrderItemEntity,
    onConfirmManual: () -> Unit,
    onReset: () -> Unit,
    onUpdateQuantity: (Boolean) -> Unit,
    onSetQuantity: (Int) -> Unit
) {
    val isPartial = item.quantityPicked > 0 && item.quantityPicked < item.quantityOrdered
    val isExcess = item.quantityPicked > item.quantityOrdered

    val borderStrokeColor = when {
        item.isPicked && !isExcess -> MaterialTheme.colorScheme.tertiary
        isExcess -> MaterialTheme.colorScheme.error // Custom red border for excess pickers
        isPartial -> Color(0xFFFF9800) // Vibrant orange border for partial picks
        else -> MaterialTheme.colorScheme.outline.copy(alpha = 0.2f)
    }

    val cardBgColor = when {
        item.isPicked && !isExcess -> MaterialTheme.colorScheme.tertiaryContainer.copy(alpha = 0.15f)
        isExcess -> MaterialTheme.colorScheme.errorContainer.copy(alpha = 0.12f)
        isPartial -> Color(0xFFFFF3E0) // Distinct warm warning color for partial picks
        else -> MaterialTheme.colorScheme.surface
    }

    val productInfo = remember(item.productBarcode) {
        AppDataStore.products.find { it.barcode == item.productBarcode }
    }

    var textVal by remember(item.quantityPicked) { mutableStateOf(item.quantityPicked.toString()) }

    Card(
        modifier = Modifier
            .fillMaxWidth()
            .testTag("wms_pick_item_${item.productBarcode}"),
        border = BorderStroke(if (isPartial || item.isPicked || isExcess) 1.5.dp else 1.dp, borderStrokeColor),
        colors = CardDefaults.cardColors(containerColor = cardBgColor)
    ) {
        Column(modifier = Modifier.fillMaxWidth()) {
            Row(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(horizontal = 8.dp, vertical = 6.dp),
                horizontalArrangement = Arrangement.spacedBy(8.dp),
                verticalAlignment = Alignment.CenterVertically
            ) {
                // Product Image Container
                val imageModel = remember(productInfo?.imageUrl) {
                    productInfo?.imageUrl?.split(Regex("[,;|\\s]+"))?.map { it.trim() }?.firstOrNull { it.isNotEmpty() }
                }
                Box(
                    modifier = Modifier
                        .size(36.dp)
                        .clip(RoundedCornerShape(4.dp))
                        .background(productInfo?.imageUrlColor?.copy(alpha = 0.12f) ?: MaterialTheme.colorScheme.surfaceVariant),
                    contentAlignment = Alignment.Center
                ) {
                    if (imageModel != null) {
                        coil.compose.AsyncImage(
                            model = imageModel,
                            contentDescription = item.productTitle,
                            modifier = Modifier.fillMaxSize(),
                            contentScale = androidx.compose.ui.layout.ContentScale.Crop
                        )
                    } else {
                        val initials = if (item.productTitle.isNotBlank()) item.productTitle.take(2).uppercase() else "PR"
                        Text(
                            text = initials,
                            style = MaterialTheme.typography.bodyMedium.copy(
                                fontWeight = FontWeight.Bold,
                                color = productInfo?.imageUrlColor ?: MaterialTheme.colorScheme.primary
                            )
                        )
                    }
                }

                // Title + Subtitle Column
                Column(modifier = Modifier.weight(1f)) {
                    Text(
                        text = item.productTitle,
                        style = MaterialTheme.typography.bodySmall.copy(fontWeight = FontWeight.Bold),
                        maxLines = 1,
                        overflow = androidx.compose.ui.text.style.TextOverflow.Ellipsis
                    )
                    Row(
                        horizontalArrangement = Arrangement.spacedBy(6.dp),
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Text(
                            text = item.productBarcode,
                            style = MaterialTheme.typography.labelSmall,
                            color = MaterialTheme.colorScheme.outline
                        )
                        if (item.shelfLocation != null) {
                            Text(
                                text = "• Raf: ${item.shelfLocation}",
                                style = MaterialTheme.typography.labelSmall.copy(fontWeight = FontWeight.Bold),
                                color = MaterialTheme.colorScheme.secondary
                            )
                        }
                    }
                }

                // Middle: Quantity Picked Info Status (e.g. "2 / 5")
                Column(
                    horizontalAlignment = Alignment.End,
                    modifier = Modifier.padding(horizontal = 4.dp)
                ) {
                    Text(
                        text = "${item.quantityPicked} / ${item.quantityOrdered}",
                        style = MaterialTheme.typography.bodyMedium.copy(fontWeight = FontWeight.ExtraBold),
                        color = when {
                            isExcess -> MaterialTheme.colorScheme.error
                            isPartial -> Color(0xFFE65100)
                            item.isPicked -> MaterialTheme.colorScheme.tertiary
                            else -> MaterialTheme.colorScheme.onSurface
                        }
                    )
                    Text(
                        text = "Adet",
                        style = MaterialTheme.typography.labelSmall,
                        color = MaterialTheme.colorScheme.outline
                    )
                }

                // Interactive Counter with "- [input field] +" Buttons layout
                Row(
                    verticalAlignment = Alignment.CenterVertically,
                    horizontalArrangement = Arrangement.spacedBy(4.dp)
                ) {
                    // Minus Button
                    IconButton(
                        onClick = { onUpdateQuantity(false) },
                        modifier = Modifier
                            .size(28.dp)
                            .background(
                                color = if (isPartial) Color(0xFFFFE0B2) else MaterialTheme.colorScheme.surfaceVariant,
                                shape = RoundedCornerShape(4.dp)
                            ),
                        enabled = item.quantityPicked > 0
                    ) {
                        Icon(
                            imageVector = Icons.Filled.Remove,
                            contentDescription = "Azalt",
                            tint = if (item.quantityPicked > 0) MaterialTheme.colorScheme.error else Color.Gray,
                            modifier = Modifier.size(12.dp)
                        )
                    }

                    // Field
                    Box(
                        modifier = Modifier
                            .width(42.dp)
                            .height(28.dp)
                            .background(
                                color = if (item.isPicked) MaterialTheme.colorScheme.tertiaryContainer else MaterialTheme.colorScheme.surfaceVariant,
                                shape = RoundedCornerShape(4.dp)
                            ),
                        contentAlignment = Alignment.Center
                    ) {
                        BasicTextField(
                            value = textVal,
                            onValueChange = { newVal ->
                                val filtered = newVal.filter { it.isDigit() }
                                textVal = filtered
                                if (filtered.isNotEmpty()) {
                                    filtered.toIntOrNull()?.let { targetInt ->
                                        onSetQuantity(targetInt.coerceIn(0, 999))
                                    }
                                } else {
                                    onSetQuantity(0)
                                }
                            },
                            keyboardOptions = KeyboardOptions(
                                keyboardType = KeyboardType.Number,
                                imeAction = ImeAction.Done
                            ),
                            singleLine = true,
                            textStyle = MaterialTheme.typography.labelLarge.copy(
                                textAlign = TextAlign.Center,
                                fontWeight = FontWeight.Bold,
                                color = if (item.isPicked) MaterialTheme.colorScheme.onTertiaryContainer else MaterialTheme.colorScheme.onSurface
                            ),
                            modifier = Modifier.fillMaxWidth().padding(horizontal = 2.dp)
                        )
                    }

                    // Plus Button
                    IconButton(
                        onClick = { onUpdateQuantity(true) },
                        modifier = Modifier
                            .size(28.dp)
                            .background(
                                color = if (isPartial) Color(0xFFFFE0B2) else MaterialTheme.colorScheme.surfaceVariant,
                                shape = RoundedCornerShape(4.dp)
                            )
                    ) {
                        Icon(
                            imageVector = Icons.Filled.Add,
                            contentDescription = "Arttır",
                            tint = MaterialTheme.colorScheme.primary,
                            modifier = Modifier.size(12.dp)
                        )
                    }
                }
            }

            // Compact bottom actions (Sıfırla / Tamamla) row only when relevant
            if (item.quantityPicked > 0 || (!item.isPicked || isPartial)) {
                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(start = 8.dp, end = 8.dp, bottom = 4.dp),
                    horizontalArrangement = Arrangement.End,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    if (item.quantityPicked > 0) {
                        TextButton(
                            onClick = onReset,
                            colors = ButtonDefaults.textButtonColors(contentColor = MaterialTheme.colorScheme.error),
                            modifier = Modifier.height(24.dp),
                            contentPadding = PaddingValues(horizontal = 4.dp, vertical = 0.dp)
                        ) {
                            Icon(Icons.Filled.Refresh, contentDescription = null, modifier = Modifier.size(12.dp))
                            Spacer(modifier = Modifier.width(3.dp))
                            Text("Sıfırla", style = MaterialTheme.typography.labelSmall)
                        }
                    }

                    if (!item.isPicked || isPartial) {
                        Spacer(modifier = Modifier.width(8.dp))
                        TextButton(
                            onClick = onConfirmManual,
                            colors = ButtonDefaults.textButtonColors(contentColor = MaterialTheme.colorScheme.primary),
                            modifier = Modifier.height(24.dp),
                            contentPadding = PaddingValues(horizontal = 4.dp, vertical = 0.dp)
                        ) {
                            Icon(Icons.Filled.Check, contentDescription = null, modifier = Modifier.size(12.dp))
                            Spacer(modifier = Modifier.width(3.dp))
                            Text("Tamamla", style = MaterialTheme.typography.labelSmall.copy(fontWeight = FontWeight.Bold))
                        }
                    }
                }
            }

            // Beautiful thin horizontal progress line right at the very bottom edge of the card
            val progressValue = remember(item.quantityPicked, item.quantityOrdered) {
                if (item.quantityOrdered > 0) {
                    (item.quantityPicked.toFloat() / item.quantityOrdered.toFloat()).coerceIn(0f, 1f)
                } else {
                    0f
                }
            }
            LinearProgressIndicator(
                progress = progressValue,
                modifier = Modifier
                    .fillMaxWidth()
                    .height(2.dp),
                color = when {
                    isExcess -> MaterialTheme.colorScheme.error
                    isPartial -> Color(0xFFFF9800)
                    item.isPicked -> MaterialTheme.colorScheme.tertiary
                    else -> MaterialTheme.colorScheme.primary
                },
                trackColor = Color.Transparent
            )
        }
    }
}

// Custom specialized camera scanning view for WMS using CameraX and ML Kit 
@OptIn(ExperimentalPermissionsApi::class)
@Composable
fun WmsBarcodeScannerDialog(
    onDismissRequest: () -> Unit,
    onBarcodeScanned: (String) -> Unit
) {
    val cameraPermissionState = rememberPermissionState(Manifest.permission.CAMERA)
    var manualBarcodeValue by remember { mutableStateOf("") }
    val context = LocalContext.current

    Dialog(
        onDismissRequest = onDismissRequest,
        properties = DialogProperties(usePlatformDefaultWidth = false)
    ) {
        Surface(
            modifier = Modifier.fillMaxSize(),
            color = MaterialTheme.colorScheme.background
        ) {
            Column(modifier = Modifier.fillMaxSize()) {
                // Header of scanner screen
                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .background(MaterialTheme.colorScheme.surfaceVariant)
                        .padding(16.dp),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Text("Barkod Tarayıcı", style = MaterialTheme.typography.titleLarge.copy(fontWeight = FontWeight.Bold))
                    IconButton(onClick = onDismissRequest) {
                        Icon(Icons.Filled.Close, contentDescription = "Kapat")
                    }
                }

                // Camera target view zone
                Box(
                    modifier = Modifier
                        .weight(1f)
                        .fillMaxWidth()
                        .background(Color.Black),
                    contentAlignment = Alignment.Center
                ) {
                    if (cameraPermissionState.status.isGranted) {
                        WmsCameraScannerView { barcode ->
                            onBarcodeScanned(barcode)
                        }

                        // Guidelines crosshair overlay
                        Box(
                            modifier = Modifier
                                .size(240.dp)
                                .border(BorderStroke(4.dp, Color.Red), RoundedCornerShape(12.dp))
                        ) {
                            Text(
                                "Barkodu buraya ortalayın",
                                style = MaterialTheme.typography.bodySmall.copy(color = Color.White),
                                modifier = Modifier
                                    .align(Alignment.BottomCenter)
                                    .padding(8.dp)
                                    .background(Color.Black.copy(alpha = 0.6f), RoundedCornerShape(4.dp))
                                    .padding(horizontal = 6.dp, vertical = 2.dp)
                            )
                        }
                    } else {
                        Column(
                            modifier = Modifier.padding(24.dp),
                            horizontalAlignment = Alignment.CenterHorizontally,
                            verticalArrangement = Arrangement.spacedBy(16.dp)
                        ) {
                            Icon(Icons.Filled.CameraEnhance, contentDescription = null, modifier = Modifier.size(64.dp), tint = Color.LightGray)
                            Text(
                                "Kamera izni verilmedi. Barkodları taramak için kamera izni gerekiyor.",
                                style = MaterialTheme.typography.bodyMedium,
                                color = Color.White,
                                textAlign = TextAlign.Center
                            )
                            Button(onClick = { cameraPermissionState.launchPermissionRequest() }) {
                                Text("İzin İste")
                            }
                        }
                    }
                }

                // Emulator/Manual entry debugging section
                Card(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(16.dp),
                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface)
                ) {
                    Column(
                        modifier = Modifier.padding(16.dp),
                        verticalArrangement = Arrangement.spacedBy(12.dp)
                    ) {
                        Text("Manuel / Test Barkod Simülasyonu", style = MaterialTheme.typography.bodyMedium.copy(fontWeight = FontWeight.Bold))
                        
                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            verticalAlignment = Alignment.CenterVertically,
                            horizontalArrangement = Arrangement.spacedBy(8.dp)
                        ) {
                            OutlinedTextField(
                                value = manualBarcodeValue,
                                onValueChange = { manualBarcodeValue = it },
                                placeholder = { Text("Örn: 8690123456789") },
                                label = { Text("Simüle Edilecek Ürün Barkodu") },
                                singleLine = true,
                                modifier = Modifier
                                    .weight(1f)
                                    .testTag("wms_simulated_barcode_input"),
                                colors = TextFieldDefaults.colors(
                                    focusedContainerColor = Color.Transparent,
                                    unfocusedContainerColor = Color.Transparent
                                )
                            )

                            Button(
                                onClick = {
                                    if (manualBarcodeValue.isNotBlank()) {
                                        onBarcodeScanned(manualBarcodeValue.trim())
                                        manualBarcodeValue = ""
                                    } else {
                                        Toast.makeText(context, "Bir barkod numarası girin!", Toast.LENGTH_SHORT).show()
                                    }
                                },
                                modifier = Modifier
                                    .height(56.dp)
                                    .testTag("wms_simulate_scan_btn")
                            ) {
                                Text("Simüle Et")
                            }
                        }

                        // Hotkey quick test buttons
                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.spacedBy(8.dp)
                        ) {
                            OutlinedButton(
                                onClick = { onBarcodeScanned("8690123456789") },
                                modifier = Modifier.weight(1f)
                            ) {
                                Text("8690123456789", fontSize = 10.sp)
                            }
                            OutlinedButton(
                                onClick = { onBarcodeScanned("8699876543210") },
                                modifier = Modifier.weight(1f)
                            ) {
                                Text("8699876543210", fontSize = 10.sp)
                            }
                            OutlinedButton(
                                onClick = { onBarcodeScanned("1234567890123") },
                                modifier = Modifier.weight(1f)
                            ) {
                                Text("1234567890123", fontSize = 10.sp)
                            }
                        }
                    }
                }
            }
        }
    }
}

@Composable
fun WmsCameraScannerView(
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
                        setAnalyzer(cameraExecutor, WmsBarcodeImageAnalyzer { barcode ->
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

@androidx.annotation.OptIn(androidx.camera.core.ExperimentalGetImage::class)
class WmsBarcodeImageAnalyzer(
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
fun WarehouseStageProgress(status: String) {
    val activeStep = when (status) {
        "Bekleyen", "Toplanıyor" -> 1
        "Toplandı" -> 2
        "Koli Hazırlandı" -> 3
        else -> 4 // Sevk Edildi
    }
    
    Card(
        modifier = Modifier.fillMaxWidth(),
        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f))
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(10.dp),
            horizontalArrangement = Arrangement.SpaceBetween,
            verticalAlignment = Alignment.CenterVertically
        ) {
            // Step 1: Toplama
            Column(horizontalAlignment = Alignment.CenterHorizontally, modifier = Modifier.weight(1f)) {
                Box(
                    modifier = Modifier
                        .size(28.dp)
                        .clip(androidx.compose.foundation.shape.CircleShape)
                        .background(if (activeStep >= 1) MaterialTheme.colorScheme.primary else Color.LightGray),
                    contentAlignment = Alignment.Center
                ) {
                    if (activeStep > 1) {
                        Icon(Icons.Filled.Check, null, tint = Color.White, modifier = Modifier.size(14.dp))
                    } else {
                        Text("1", color = Color.White, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodySmall)
                    }
                }
                Spacer(modifier = Modifier.height(2.dp))
                Text("Toplama", style = MaterialTheme.typography.labelSmall, fontWeight = if (activeStep == 1) FontWeight.Bold else FontWeight.Normal, color = if (activeStep == 1) MaterialTheme.colorScheme.primary else Color.Gray)
            }
            
            // Link 1 -> 2
            Divider(
                modifier = Modifier.weight(0.3f).padding(bottom = 12.dp),
                color = if (activeStep >= 2) MaterialTheme.colorScheme.primary else Color.LightGray,
                thickness = 2.dp
            )
            
            // Step 2: Kontrol
            Column(horizontalAlignment = Alignment.CenterHorizontally, modifier = Modifier.weight(1f)) {
                Box(
                    modifier = Modifier
                        .size(28.dp)
                        .clip(androidx.compose.foundation.shape.CircleShape)
                        .background(if (activeStep >= 2) MaterialTheme.colorScheme.primary else Color.LightGray),
                    contentAlignment = Alignment.Center
                ) {
                    if (activeStep > 2) {
                        Icon(Icons.Filled.Check, null, tint = Color.White, modifier = Modifier.size(14.dp))
                    } else {
                        Text("2", color = Color.White, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodySmall)
                    }
                }
                Spacer(modifier = Modifier.height(2.dp))
                Text("Kontrol", style = MaterialTheme.typography.labelSmall, fontWeight = if (activeStep == 2) FontWeight.Bold else FontWeight.Normal, color = if (activeStep == 2) MaterialTheme.colorScheme.primary else Color.Gray)
            }
            
            // Link 2 -> 3
            Divider(
                modifier = Modifier.weight(0.3f).padding(bottom = 12.dp),
                color = if (activeStep >= 3) MaterialTheme.colorScheme.primary else Color.LightGray,
                thickness = 2.dp
            )
            
            // Step 3: Yükleme
            Column(horizontalAlignment = Alignment.CenterHorizontally, modifier = Modifier.weight(1f)) {
                Box(
                    modifier = Modifier
                        .size(28.dp)
                        .clip(androidx.compose.foundation.shape.CircleShape)
                        .background(if (activeStep >= 3) MaterialTheme.colorScheme.primary else Color.LightGray),
                    contentAlignment = Alignment.Center
                ) {
                    if (activeStep > 3) {
                        Icon(Icons.Filled.Check, null, tint = Color.White, modifier = Modifier.size(14.dp))
                    } else {
                        Text("3", color = Color.White, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodySmall)
                    }
                }
                Spacer(modifier = Modifier.height(2.dp))
                Text("Araç Yükleme", style = MaterialTheme.typography.labelSmall, fontWeight = if (activeStep == 3) FontWeight.Bold else FontWeight.Normal, color = if (activeStep == 3) MaterialTheme.colorScheme.primary else Color.Gray)
            }
        }
    }
}

@Composable
fun ControlItemCard(
    item: WmsOrderItemEntity
) {
    val discrepancy = item.quantityPicked - item.quantityOrdered
    val productInfo = remember(item.productBarcode) {
        AppDataStore.products.find { it.barcode == item.productBarcode }
    }
    
    Card(
        modifier = Modifier.fillMaxWidth(),
        border = BorderStroke(
            width = if (discrepancy != 0) 2.dp else 1.dp,
            color = when {
                discrepancy < 0 -> Color(0xFFFF9800) // Amber/Orange
                discrepancy > 0 -> MaterialTheme.colorScheme.error // Red
                else -> MaterialTheme.colorScheme.tertiary.copy(alpha = 0.4f) // Teal/Green
            }
        ),
        colors = CardDefaults.cardColors(
            containerColor = when {
                discrepancy < 0 -> Color(0xFFFFF3E0) // Warm light warning alert
                discrepancy > 0 -> MaterialTheme.colorScheme.errorContainer.copy(alpha = 0.15f) // Error alert
                else -> MaterialTheme.colorScheme.surface
            }
        )
    ) {
        Column(modifier = Modifier.padding(12.dp)) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.spacedBy(11.dp),
                verticalAlignment = Alignment.CenterVertically
            ) {
                // Product Image Container
                val imageModel = remember(productInfo?.imageUrl) {
                    productInfo?.imageUrl?.split(Regex("[,;|\\s]+"))?.map { it.trim() }?.firstOrNull { it.isNotEmpty() }
                }
                Box(
                    modifier = Modifier
                        .size(56.dp)
                        .clip(RoundedCornerShape(8.dp))
                        .background(productInfo?.imageUrlColor?.copy(alpha = 0.15f) ?: MaterialTheme.colorScheme.surfaceVariant),
                    contentAlignment = Alignment.Center
                ) {
                    if (imageModel != null) {
                        coil.compose.AsyncImage(
                            model = imageModel,
                            contentDescription = item.productTitle,
                            modifier = Modifier.fillMaxSize(),
                            contentScale = androidx.compose.ui.layout.ContentScale.Crop
                        )
                    } else {
                        val initials = if (item.productTitle.isNotBlank()) item.productTitle.take(2).uppercase() else "PR"
                        Text(
                            text = initials,
                            style = MaterialTheme.typography.titleMedium.copy(
                                fontWeight = FontWeight.Bold,
                                color = productInfo?.imageUrlColor ?: MaterialTheme.colorScheme.primary
                            )
                        )
                    }
                }

                Column(modifier = Modifier.weight(1f)) {
                    Text(
                        text = item.productTitle,
                        style = MaterialTheme.typography.bodyMedium.copy(fontWeight = FontWeight.Bold)
                    )
                    Spacer(modifier = Modifier.height(2.dp))
                    Text(
                        text = "Barkod: ${item.productBarcode} | Konum: ${item.shelfLocation ?: "Belirtilmemiş"}",
                        style = MaterialTheme.typography.labelSmall,
                        color = MaterialTheme.colorScheme.outline
                    )
                }
            }

            Spacer(modifier = Modifier.height(8.dp))
            HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.4f))
            Spacer(modifier = Modifier.height(8.dp))

            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                Row(
                    horizontalArrangement = Arrangement.spacedBy(16.dp),
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Column(horizontalAlignment = Alignment.CenterHorizontally) {
                        Text("Sipariş", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                        Text("${item.quantityOrdered}", style = MaterialTheme.typography.titleMedium.copy(fontWeight = FontWeight.Bold))
                    }
                    Text("➔", style = TextStyle(fontWeight = FontWeight.Bold, color = Color.Gray))
                    Column(horizontalAlignment = Alignment.CenterHorizontally) {
                        Text("Toplanan", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                        Text(
                            text = "${item.quantityPicked}",
                            style = MaterialTheme.typography.titleMedium.copy(
                                fontWeight = FontWeight.Bold,
                                color = if (discrepancy != 0) MaterialTheme.colorScheme.error else MaterialTheme.colorScheme.tertiary
                            )
                        )
                    }
                }

                // Alert Warning Badge
                when {
                    discrepancy < 0 -> {
                        Surface(
                            shape = RoundedCornerShape(8.dp),
                            color = Color(0xFFFF9800).copy(alpha = 0.2f),
                            border = BorderStroke(1.dp, Color(0xFFFF9800))
                        ) {
                            Row(
                                modifier = Modifier.padding(horizontal = 8.dp, vertical = 4.dp),
                                horizontalArrangement = Arrangement.spacedBy(4.dp),
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Icon(Icons.Filled.Warning, null, tint = Color(0xFFFF9800), modifier = Modifier.size(14.dp))
                                Text(
                                    "Eksik Ürün (-${-discrepancy})",
                                    style = MaterialTheme.typography.labelSmall.copy(fontWeight = FontWeight.Bold, color = Color(0xFFE65100))
                                )
                            }
                        }
                    }
                    discrepancy > 0 -> {
                        Surface(
                            shape = RoundedCornerShape(8.dp),
                            color = MaterialTheme.colorScheme.errorContainer,
                            border = BorderStroke(1.dp, MaterialTheme.colorScheme.error)
                        ) {
                            Row(
                                modifier = Modifier.padding(horizontal = 8.dp, vertical = 4.dp),
                                horizontalArrangement = Arrangement.spacedBy(4.dp),
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Icon(Icons.Filled.Error, null, tint = MaterialTheme.colorScheme.error, modifier = Modifier.size(14.dp))
                                Text(
                                    "Fazla Ürün (+${discrepancy})",
                                    style = MaterialTheme.typography.labelSmall.copy(fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.error)
                                )
                            }
                        }
                    }
                    else -> {
                        Surface(
                            shape = RoundedCornerShape(8.dp),
                            color = MaterialTheme.colorScheme.tertiaryContainer.copy(alpha = 0.4f),
                            border = BorderStroke(1.dp, MaterialTheme.colorScheme.tertiary)
                        ) {
                            Row(
                                modifier = Modifier.padding(horizontal = 8.dp, vertical = 4.dp),
                                horizontalArrangement = Arrangement.spacedBy(4.dp),
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Icon(Icons.Filled.CheckCircle, null, tint = MaterialTheme.colorScheme.tertiary, modifier = Modifier.size(14.dp))
                                Text(
                                    "Adet Tamam",
                                    style = MaterialTheme.typography.labelSmall.copy(fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.onTertiaryContainer)
                                )
                            }
                        }
                    }
                }
            }

            if (discrepancy != 0) {
                Spacer(modifier = Modifier.height(8.dp))
                Surface(
                    color = MaterialTheme.colorScheme.surfaceVariant,
                    shape = RoundedCornerShape(6.dp),
                    modifier = Modifier.fillMaxWidth()
                ) {
                    Text(
                        text = if (discrepancy < 0) {
                            "⚠️ DİKKAT! Bu üründen siparişe göre ${-discrepancy} adet EKSİK toplatılmıştır!"
                        } else {
                            "⚠️ DİKKAT! Bu üründen siparişe göre ${discrepancy} adet FAZLA toplatılmıştır!"
                        },
                        style = MaterialTheme.typography.labelSmall.copy(
                            fontWeight = FontWeight.Bold,
                            color = if (discrepancy < 0) Color(0xFFE65100) else MaterialTheme.colorScheme.error
                        ),
                        modifier = Modifier.padding(8.dp),
                        textAlign = TextAlign.Center
                    )
                }
            }
        }
    }
}

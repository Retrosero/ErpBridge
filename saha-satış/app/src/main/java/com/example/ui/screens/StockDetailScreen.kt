package com.example.ui.screens

import android.content.Context
import android.media.AudioManager
import android.media.ToneGenerator
import android.os.Build
import android.os.VibrationEffect
import android.os.Vibrator
import android.os.VibratorManager
import androidx.compose.animation.*
import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.LazyRow
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.foundation.horizontalScroll
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.draw.shadow
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.compose.ui.window.Dialog
import androidx.navigation.NavController
import coil.compose.AsyncImage
import com.example.ui.components.FieldCard
import kotlinx.coroutines.launch

data class StockMovement(
    val date: String,
    val type: String, // "Giriş", "Çıkış", "Sevk"
    val qty: String,
    val detail: String,
    val user: String,
    val runningBalance: String? = null,
    val evrakNo: String? = null,
    val cariKod: String? = null,
    val cariName: String? = null,
    val unitPrice: Double? = null,
    val totalAmount: Double? = null,
    val warehouse: String? = null
)

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun StockDetailScreen(barcode: String?, navController: NavController) {
    val context = LocalContext.current
    val scope = rememberCoroutineScope()
    val snackbarHostState = remember { SnackbarHostState() }

    val initialProduct = remember(barcode) {
        AppDataStore.products.find {
            it.barcode == barcode || it.barcodes.contains(barcode) || it.code == barcode
        }
    }
    if (initialProduct == null) {
        Scaffold(
            topBar = {
                TopAppBar(
                    title = { Text("Stok Kartı") },
                    navigationIcon = {
                        IconButton(onClick = { navController.popBackStack() }) {
                            Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = "Geri")
                        }
                    }
                )
            }
        ) { padding ->
            Box(
                modifier = Modifier.fillMaxSize().padding(padding),
                contentAlignment = Alignment.Center
            ) {
                Text("Stok kartı bulunamadı.", color = MaterialTheme.colorScheme.onSurfaceVariant)
            }
        }
        return
    }

    var product by remember(barcode) {
        mutableStateOf(initialProduct)
    }

    val totalStock = product.stockByWarehouse.values.sum()

    var movements by remember(product.code) {
        mutableStateOf<List<StockMovement>>(emptyList())
    }
    var movementsLoading by remember(product.code) { mutableStateOf(true) }

    var selectedMovementForDetail by remember { mutableStateOf<StockMovement?>(null) }

    LaunchedEffect(product.code, product.barcode) {
        val codeKey = product.code.trim().lowercase()
        val barcodeKey = product.barcode.trim().lowercase()
        val cachedMovements: List<StockMovement>? = AppDataStore.stockMovementsMap[codeKey]
            ?: AppDataStore.stockMovementsMap[barcodeKey]

        if (cachedMovements != null && cachedMovements.isNotEmpty()) {
            movements = calculateBalancesForMovements(cachedMovements, totalStock.toDouble())
            movementsLoading = false
            return@LaunchedEffect
        }

        movements = emptyList()
        movementsLoading = true
        try {
            val sharedPrefs = context.getSharedPreferences("erp_settings", Context.MODE_PRIVATE)
            val apiUrl = sharedPrefs.getString("api_url", "https://d5e4-88-248-2-49.ngrok-free.app") ?: "https://d5e4-88-248-2-49.ngrok-free.app"
            val apiKey = sharedPrefs.getString("api_key", "dev-token-change-in-production") ?: "dev-token-change-in-production"
            val tenantId = sharedPrefs.getString("tenant_id", "T001") ?: "T001"
            val deviceId = sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT"

            val apiService = com.example.data.api.ApiClient.getFieldOpsApiService(context, apiUrl, apiKey)
            
            val searchKey = if (product.code.isNotBlank()) product.code else product.barcode
            var response = apiService.getStokHareket(com.example.data.api.PullJobsRequest(
                tenant_id = tenantId,
                api_key = apiKey,
                device_id = deviceId,
                agent_version = "v2.0",
                entity = "stokHareket",
                since = searchKey,
                page = 1,
                pageSize = 50
            ))

            var items = if (response.isSuccessful && response.body() != null) response.body()!!.actualItems else emptyList()

            if (items.isEmpty() && product.barcode.isNotBlank() && searchKey != product.barcode) {
                val responseBarcode = apiService.getStokHareket(com.example.data.api.PullJobsRequest(
                    tenant_id = tenantId,
                    api_key = apiKey,
                    device_id = deviceId,
                    agent_version = "v2.0",
                    entity = "stokHareket",
                    since = product.barcode,
                    page = 1,
                    pageSize = 50
                ))
                if (responseBarcode.isSuccessful && responseBarcode.body() != null) {
                    items = responseBarcode.body()!!.actualItems
                }
            }
            
            if (items.isNotEmpty()) {
                val realMovements = items.map { item ->
                    val rawDate = item.tarih ?: ""
                    val formattedDate = try {
                        if (rawDate.contains("T")) {
                            val timePart = rawDate.split("T").getOrNull(1)?.take(5) ?: "00:00"
                            val datePart = rawDate.split("T")[0].split("-")
                            if (datePart.size == 3) {
                                "${datePart[2]}.${datePart[1]}.${datePart[0]} $timePart"
                            } else {
                                rawDate
                            }
                        } else {
                            rawDate
                        }
                    } catch (e: Exception) {
                        rawDate
                    }

                    val moveType = when (item.tip) {
                        0 -> "Giriş"
                        1 -> "Çıkış"
                        2 -> "İade Giriş"
                        3 -> "İade Çıkış"
                        else -> "Hareket"
                    }
                    
                    val signedQuantity = item.miktar
                        ?: ((item.girisMiktar ?: 0.0) - (item.cikisMiktar ?: 0.0))
                    val quantityFormatted = "${kotlin.math.abs(signedQuantity)} ADT"
                    val clientName = AppDataStore.customers.find { it.id == item.cariKod }?.name 
                        ?: item.cariKod
                        ?: "Cari bilgisi yok"
                    
                    val detailText = "Evrak: ${item.evrakNo ?: "Belgesiz"} - $clientName"
                    val userText = item.aciklama ?: "Mikro stok hareketi"
                    
                    StockMovement(
                        date = formattedDate,
                        type = moveType,
                        qty = quantityFormatted,
                        detail = detailText,
                        user = userText,
                        evrakNo = item.evrakNo ?: "Belgesiz",
                        cariKod = item.cariKod,
                        cariName = clientName,
                        unitPrice = item.birimFiyat,
                        totalAmount = item.tutar ?: item.birimFiyat?.let { kotlin.math.abs(signedQuantity) * it },
                        warehouse = "Depo: ${if (item.girisDepoNo != null) item.girisDepoNo else if (item.cikisDepoNo != null) item.cikisDepoNo else "Merkez"}"
                    )
                }
                AppDataStore.stockMovementsMap[codeKey] = realMovements
                if (barcodeKey.isNotBlank()) AppDataStore.stockMovementsMap[barcodeKey] = realMovements
                movements = calculateBalancesForMovements(realMovements, totalStock.toDouble())
            } else {
                movements = emptyList()
            }
        } catch (e: Exception) {
            e.printStackTrace()
            movements = emptyList()
        } finally {
            movementsLoading = false
        }
    }

    // Real product images from DB/Catalog, falling back to specific, beautiful product-focused photos
    val productImages = remember(product.barcode, product.category, product.imageUrl, product.localImagePath) {
        val list = mutableListOf<Any>()
        
        val currentPath = product.localImagePath
        val currentUrl = product.imageUrl

        val localFiles = mutableListOf<java.io.File>()
        if (!currentPath.isNullOrBlank()) {
            val paths = currentPath.split(Regex("[,;|\\s]+"))
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
        if (!currentUrl.isNullOrBlank()) {
            val urls = currentUrl.split(Regex("[,;|\\s]+"))
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
        
        // 3. Fallback only if no custom paths exist
        if (list.isEmpty()) {
            when (product.barcode) {
                "8690123456789" -> listOf(
                    "https://images.unsplash.com/photo-1619642751034-765dfdf7c58e?auto=format&fit=crop&w=600&q=80",
                    "https://images.unsplash.com/photo-1486006920555-c77dce18193b?auto=format&fit=crop&w=600&q=80",
                    "https://images.unsplash.com/photo-1518364538800-6bcb3f25da49?auto=format&fit=crop&w=600&q=80"
                )
                "8699876543210" -> listOf(
                    "https://images.unsplash.com/photo-1555215695-3004980ad54e?auto=format&fit=crop&w=600&q=80",
                    "https://images.unsplash.com/photo-1486006920555-c77dce18193b?auto=format&fit=crop&w=600&q=80",
                    "https://images.unsplash.com/photo-1504384308090-c894fdcc538d?auto=format&fit=crop&w=600&q=80"
                )
                "1234567890123" -> listOf(
                    "https://images.unsplash.com/photo-1581092160607-ee22621dd758?auto=format&fit=crop&w=600&q=80",
                    "https://images.unsplash.com/photo-1537462715879-360eeb61a0bc?auto=format&fit=crop&w=600&q=80",
                    "https://images.unsplash.com/photo-1504198453319-5ce911bafcde?auto=format&fit=crop&w=600&q=80"
                )
                "8681122334455" -> listOf(
                    "https://images.unsplash.com/photo-1590372847146-2674026ec1dc?auto=format&fit=crop&w=600&q=80",
                    "https://images.unsplash.com/photo-1534224039826-c7a0dea0e66a?auto=format&fit=crop&w=600&q=80",
                    "https://images.unsplash.com/photo-1513258496099-48168024aec0?auto=format&fit=crop&w=600&q=80"
                )
                else -> {
                    val urlSpec = when (product.category) {
                        "Endüstriyel Yağlar" -> "https://images.unsplash.com/photo-1619642751034-765dfdf7c58e?auto=format&fit=crop&w=600&q=80"
                        "Filtre Grupları" -> "https://images.unsplash.com/photo-1555215695-3004980ad54e?auto=format&fit=crop&w=600&q=80"
                        "Yedek Parça" -> "https://images.unsplash.com/photo-1581092160607-ee22621dd758?auto=format&fit=crop&w=600&q=80"
                        else -> "https://images.unsplash.com/photo-1590372847146-2674026ec1dc?auto=format&fit=crop&w=600&q=80"
                    }
                    listOf(
                        urlSpec,
                        "https://images.unsplash.com/photo-1581091226825-a6a2a5aee158?auto=format&fit=crop&w=600&q=80",
                        "https://images.unsplash.com/photo-1537462715879-360eeb61a0bc?auto=format&fit=crop&w=600&q=80"
                    )
                }
            }
        } else {
            list
        }
    }

    var showEditDialog by AppDataStore.showStockDetailEditDialog

    // Sound logic
    fun playSound() {
        com.example.util.VibratorHelper.triggerFeedback(context, true)
    }

    Scaffold(
        snackbarHost = { SnackbarHost(snackbarHostState) },
        containerColor = MaterialTheme.colorScheme.background,
        contentWindowInsets = WindowInsets(0, 0, 0, 0)
    ) { innerPadding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(innerPadding)
                .background(
                    Brush.verticalGradient(
                        colors = listOf(
                            MaterialTheme.colorScheme.background,
                            MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.2f)
                        )
                    )
                )
        ) {
            LazyColumn(
                modifier = Modifier
                    .weight(1f)
                    .fillMaxWidth()
                    .padding(horizontal = 16.dp),
                verticalArrangement = Arrangement.spacedBy(8.dp),
                contentPadding = PaddingValues(top = 0.dp, bottom = 0.dp)
            ) {
                // --- 1. PRODUCT IMAGES CAROUSEL (Ürünün Resimleri) ---
                item {
                    Column(
                        modifier = Modifier
                            .fillMaxWidth()
                            .clip(RoundedCornerShape(16.dp))
                            .background(MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.4f))
                            .padding(12.dp),
                        verticalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        var selectedImageIndex by remember { mutableStateOf(0) }
                        
                        // Main Zoomable Image Card
                        Box(
                            modifier = Modifier
                                .fillMaxWidth()
                                .height(220.dp)
                                .clip(RoundedCornerShape(12.dp))
                                .background(Color.White)
                        ) {
                            AsyncImage(
                                model = productImages.getOrNull(selectedImageIndex) ?: "",
                                contentDescription = "${product.title} Ana Görseli",
                                modifier = Modifier
                                    .fillMaxSize()
                                    .padding(8.dp),
                                contentScale = ContentScale.Fit
                            )

                            // Tag overlay indicating current view angle
                            Surface(
                                color = MaterialTheme.colorScheme.primary.copy(alpha = 0.85f),
                                shape = RoundedCornerShape(bottomStart = 8.dp),
                                modifier = Modifier.align(Alignment.TopEnd)
                            ) {
                                Text(
                                    text = "Açı: ${selectedImageIndex + 1} / ${productImages.size}",
                                    modifier = Modifier.padding(horizontal = 8.dp, vertical = 4.dp),
                                    style = MaterialTheme.typography.labelSmall,
                                    color = Color.White,
                                    fontWeight = FontWeight.Bold
                                )
                            }
                        }

                        // Horizontal thumbnail carousel
                        LazyRow(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.spacedBy(8.dp),
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            items(productImages.size) { index ->
                                val isSelected = selectedImageIndex == index
                                Card(
                                    modifier = Modifier
                                        .size(60.dp)
                                        .clickable { selectedImageIndex = index }
                                        .border(
                                            width = if (isSelected) 3.dp else 1.dp,
                                            color = if (isSelected) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.outlineVariant,
                                            shape = RoundedCornerShape(8.dp)
                                        ),
                                    shape = RoundedCornerShape(8.dp),
                                    colors = CardDefaults.cardColors(containerColor = Color.White)
                                ) {
                                    AsyncImage(
                                        model = productImages[index],
                                        contentDescription = "Görsel Seçici $index",
                                        modifier = Modifier
                                            .fillMaxSize()
                                            .padding(4.dp),
                                        contentScale = ContentScale.Fit
                                    )
                                }
                            }
                        }
                    }
                }

                // --- 2. HERO PRODUCT INFO CARD ---
                item {
                    FieldCard {
                        Column(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(16.dp),
                            verticalArrangement = Arrangement.spacedBy(12.dp)
                        ) {
                            Row(
                         …11177 tokens truncated…Text(label) },
            trailingIcon = {
                IconButton(onClick = { expanded = !expanded }) {
                    Icon(Icons.Filled.ArrowDropDown, "Aç")
                }
            },
            modifier = Modifier
                .fillMaxWidth()
                .clickable { expanded = true },
            shape = RoundedCornerShape(10.dp)
        )
        // Hidden invisible box to capture clicks on OutlinedTextField easily
        Box(
            modifier = Modifier
                .matchParentSize()
                .background(Color.Transparent)
                .clickable { expanded = true }
        )
    }

    if (expanded) {
        androidx.compose.ui.window.Dialog(onDismissRequest = { expanded = false }) {
            Surface(
                shape = RoundedCornerShape(16.dp),
                color = MaterialTheme.colorScheme.surface,
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(vertical = 24.dp)
            ) {
                Column(modifier = Modifier.padding(16.dp)) {
                    Text(label, style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold)
                    Spacer(modifier = Modifier.height(12.dp))
                    OutlinedTextField(
                        value = searchQuery,
                        onValueChange = { searchQuery = it },
                        modifier = Modifier.fillMaxWidth(),
                        placeholder = { Text("Ara...") },
                        leadingIcon = { Icon(Icons.Filled.Search, null) },
                        singleLine = true,
                        shape = RoundedCornerShape(8.dp)
                    )
                    Spacer(modifier = Modifier.height(12.dp))
                    LazyColumn(modifier = Modifier.heightIn(max = 300.dp)) {
                        items(filteredItems) { item ->
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .clickable {
                                        onValueChange(item)
                                        expanded = false
                                        searchQuery = ""
                                    }
                                    .padding(vertical = 12.dp, horizontal = 8.dp),
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text(item, style = MaterialTheme.typography.bodyMedium)
                                if (item == selectedValue) {
                                    Spacer(modifier = Modifier.weight(1f))
                                    Icon(Icons.Filled.Check, null, tint = MaterialTheme.colorScheme.primary)
                                }
                            }
                            androidx.compose.material3.Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
                        }
                    }
                    Spacer(modifier = Modifier.height(8.dp))
                    TextButton(
                        onClick = { expanded = false },
                        modifier = Modifier.align(Alignment.End)
                    ) {
                        Text("Kapat")
                    }
                }
            }
        }
    }
}

@Composable
fun SpecRow(label: String, valStr: String) {
    Row(
        modifier = Modifier.fillMaxWidth(),
        horizontalArrangement = Arrangement.SpaceBetween,
        verticalAlignment = Alignment.CenterVertically
    ) {
        Text(label, style = MaterialTheme.typography.bodyMedium, color = Color.Gray)
        Text(valStr, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.onSurface)
    }
}

@Composable
fun PriceMiniCard(
    label: String,
    price: Double,
    badge: String,
    badgeColor: Color,
    badgeTextColor: Color,
    modifier: Modifier = Modifier
) {
    Card(
        modifier = modifier
            .border(
                BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant),
                shape = RoundedCornerShape(12.dp)
            ),
        shape = RoundedCornerShape(12.dp),
        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface)
    ) {
        Column(
            modifier = Modifier.padding(10.dp),
            verticalArrangement = Arrangement.spacedBy(6.dp),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Surface(
                color = badgeColor,
                shape = RoundedCornerShape(4.dp)
            ) {
                Text(
                    text = badge,
                    modifier = Modifier.padding(horizontal = 4.dp, vertical = 2.dp),
                    style = MaterialTheme.typography.labelSmall,
                    color = badgeTextColor,
                    fontWeight = FontWeight.Bold,
                    fontSize = 9.sp
                )
            }
            Text(
                label,
                style = MaterialTheme.typography.labelMedium,
                color = Color.Gray,
                maxLines = 1
            )
            Text(
                text = String.format("₺%,.0f", price),
                style = MaterialTheme.typography.bodyMedium,
                fontWeight = FontWeight.ExtraBold,
                color = MaterialTheme.colorScheme.primary
            )
        }
    }
}

@Composable
fun MovementRow(movement: StockMovement, onClick: () -> Unit = {}) {
    Card(
        modifier = Modifier
            .fillMaxWidth()
            .clickable { onClick() },
        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceContainerLowest),
        border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(12.dp),
            horizontalArrangement = Arrangement.spacedBy(10.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            val (icon, color) = when (movement.type) {
                "Giriş" -> Icons.Default.ArrowDownward to MaterialTheme.colorScheme.tertiary
                "Çıkış" -> Icons.Default.ArrowUpward to MaterialTheme.colorScheme.error
                else -> Icons.Default.CompareArrows to MaterialTheme.colorScheme.secondary
            }
            Box(
                modifier = Modifier
                    .size(36.dp)
                    .background(color.copy(alpha = 0.12f), CircleShape),
                contentAlignment = Alignment.Center
            ) {
                Icon(icon, contentDescription = null, tint = color, modifier = Modifier.size(18.dp))
            }

            Column(modifier = Modifier.weight(1f)) {
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Text(
                        movement.detail,
                        fontWeight = FontWeight.Bold,
                        style = MaterialTheme.typography.bodySmall,
                        maxLines = 1,
                        overflow = TextOverflow.Ellipsis,
                        modifier = Modifier.weight(1f)
                    )
                    Text(
                        movement.qty,
                        fontWeight = FontWeight.ExtraBold,
                        style = MaterialTheme.typography.bodySmall,
                        color = color
                    )
                }
                Spacer(modifier = Modifier.height(2.dp))
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Text("Açıklama: ${movement.user}", style = MaterialTheme.typography.labelSmall, color = Color.Gray, maxLines = 1, modifier = Modifier.weight(1f))
                    Column(horizontalAlignment = Alignment.End) {
                        Text(movement.date, style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                        if (!movement.runningBalance.isNullOrBlank()) {
                            Text(
                                "Bakiye: ${movement.runningBalance}",
                                style = MaterialTheme.typography.labelSmall,
                                fontWeight = FontWeight.SemiBold,
                                color = MaterialTheme.colorScheme.primary
                            )
                        }
                    }
                }
            }
        }
    }
}

@Composable
fun RichTextBubble(text: String) {
    Box(
        modifier = Modifier
            .fillMaxWidth()
            .clip(RoundedCornerShape(10.dp))
            .background(MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.6f))
            .padding(10.dp)
    ) {
        Row(
            horizontalArrangement = Arrangement.spacedBy(8.dp),
            verticalAlignment = Alignment.Top
        ) {
            Icon(
                Icons.Filled.Info,
                contentDescription = null,
                tint = MaterialTheme.colorScheme.primary,
                modifier = Modifier.size(16.dp).padding(top = 2.dp)
            )
            Text(
                text = text,
                style = MaterialTheme.typography.bodySmall,
                color = MaterialTheme.colorScheme.onSurfaceVariant,
                lineHeight = 14.sp
            )
        }
    }
}

@Composable
fun SectionHeader(title: String, icon: ImageVector) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .padding(top = 8.dp, bottom = 4.dp),
        horizontalArrangement = Arrangement.spacedBy(6.dp),
        verticalAlignment = Alignment.CenterVertically
    ) {
        Icon(
            icon,
            contentDescription = null,
            tint = MaterialTheme.colorScheme.outline,
            modifier = Modifier.size(16.dp)
        )
        Text(
            text = title,
            fontWeight = FontWeight.Bold,
            style = MaterialTheme.typography.titleSmall,
            color = MaterialTheme.colorScheme.primary
        )
    }
}

fun calculateBalancesForMovements(
    rawList: List<StockMovement>,
    currentTotalStock: Double
): List<StockMovement> {
    if (rawList.isEmpty()) return emptyList()
    
    val sortedNewestFirst = rawList.map { mov ->
        val timestamp = try {
            val parts = mov.date.split(" ")
            val dateParts = parts.getOrNull(0)?.split(".")
            val timeParts = parts.getOrNull(1)?.split(":")
            if (dateParts?.size == 3) {
                val day = dateParts[0].toIntOrNull() ?: 1
                val month = dateParts[1].toIntOrNull() ?: 1
                val year = dateParts[2].toIntOrNull() ?: 2026
                val hour = timeParts?.getOrNull(0)?.toIntOrNull() ?: 0
                val minute = timeParts?.getOrNull(1)?.toIntOrNull() ?: 0
                (year * 100000000L) + (month * 1000000L) + (day * 10000L) + (hour * 100L) + minute
            } else {
                0L
            }
        } catch (e: Exception) {
            0L
        }
        mov to timestamp
    }.sortedByDescending { it.second }.map { it.first }

    val processed = mutableListOf<StockMovement>()
    var currentBal = currentTotalStock

    for (mov in sortedNewestFirst) {
        val displayBal = if (currentBal % 1.0 == 0.0) "${currentBal.toInt()} ADT" else String.format(java.util.Locale.US, "%.1f ADT", currentBal)
        processed.add(mov.copy(runningBalance = displayBal))

        val qtyVal = mov.qty.replace(Regex("[^0-9.,-]"), "").replace(",", ".").toDoubleOrNull() ?: 0.0
        val qtyAbs = Math.abs(qtyVal)
        val isInput = mov.type.contains("Giriş", ignoreCase = true) || mov.type.contains("Giris", ignoreCase = true) || mov.type.contains("Devir", ignoreCase = true)
        val isOutput = mov.type.contains("Çıkış", ignoreCase = true) || mov.type.contains("Cikis", ignoreCase = true) || mov.type.contains("Satış", ignoreCase = true) || mov.type.contains("Satis", ignoreCase = true) || mov.qty.startsWith("-")

        if (isInput) {
            currentBal -= qtyAbs
        } else if (isOutput) {
            currentBal += qtyAbs
        }
    }
    
    return processed
}

@Composable
fun StockMovementDetailDialog(movement: StockMovement, onDismiss: () -> Unit) {
    AlertDialog(
        onDismissRequest = onDismiss,
        title = {
            Row(
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.spacedBy(8.dp),
                modifier = Modifier.fillMaxWidth()
            ) {
                Icon(
                    imageVector = Icons.Filled.ReceiptLong,
                    contentDescription = null,
                    tint = MaterialTheme.colorScheme.primary,
                    modifier = Modifier.size(24.dp)
                )
                Text(
                    text = "Fiş / Belge Detayı",
                    style = MaterialTheme.typography.titleMedium,
                    fontWeight = FontWeight.Bold
                )
            }
        },
        text = {
            Column(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(vertical = 4.dp),
                verticalArrangement = Arrangement.spacedBy(12.dp)
            ) {
                // Evrak No Header
                Surface(
                    color = MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.3f),
                    shape = RoundedCornerShape(8.dp),
                    modifier = Modifier.fillMaxWidth()
                ) {
                    Column(modifier = Modifier.padding(12.dp)) {
                        Text(
                            text = "EVRAK BİLGİSİ",
                            style = MaterialTheme.typography.labelSmall,
                            color = MaterialTheme.colorScheme.primary,
                            fontWeight = FontWeight.Bold
                        )
                        Text(
                            text = movement.evrakNo ?: movement.detail.substringBefore(" - ").replace("Evrak: ", "").trim(),
                            style = MaterialTheme.typography.bodyLarge,
                            fontWeight = FontWeight.ExtraBold,
                            color = MaterialTheme.colorScheme.onPrimaryContainer
                        )
                    }
                }

                // Details List
                Column(
                    verticalArrangement = Arrangement.spacedBy(10.dp),
                    modifier = Modifier.fillMaxWidth()
                ) {
                    DetailRow(
                        label = "Müşteri / Cari",
                        value = movement.cariName ?: movement.detail.substringAfter(" - ", "Genel Müşteri").trim(),
                        icon = Icons.Filled.Person
                    )
                    
                    if (!movement.cariKod.isNullOrBlank()) {
                        DetailRow(
                            label = "Cari Kodu",
                            value = movement.cariKod,
                            icon = Icons.Filled.Tag
                        )
                    }
                    
                    DetailRow(
                        label = "İşlem Türü",
                        value = movement.type,
                        icon = Icons.Filled.Category
                    )
                    
                    DetailRow(
                        label = "Tarih",
                        value = movement.date,
                        icon = Icons.Filled.CalendarToday
                    )
                    
                    DetailRow(
                        label = "Miktar",
                        value = movement.qty,
                        icon = Icons.Filled.Layers,
                        valueColor = if (movement.type.contains("Giriş", true) || movement.type.contains("Giris", true)) {
                            MaterialTheme.colorScheme.tertiary
                        } else {
                            MaterialTheme.colorScheme.error
                        }
                    )
                    
                    val calculatedAmount = movement.totalAmount ?: 0.0
                    val qtyParsed = movement.qty.replace(Regex("[^0-9.,-]"), "").replace(",", ".").toDoubleOrNull() ?: 1.0
                    val finalQty = if (qtyParsed == 0.0) 1.0 else Math.abs(qtyParsed)
                    val unitPrice = movement.unitPrice ?: if (calculatedAmount > 0.0) (calculatedAmount / finalQty) else 0.0
                    
                    if (calculatedAmount > 0.0 || unitPrice > 0.0) {
                        DetailRow(
                            label = "Birim Fiyat",
                            value = String.format("₺%,.2f", unitPrice),
                            icon = Icons.Filled.LocalActivity
                        )
                        DetailRow(
                            label = "Toplam Tutar",
                            value = String.format("₺%,.2f", if (calculatedAmount > 0.0) calculatedAmount else (unitPrice * finalQty)),
                            icon = Icons.Filled.Payments,
                            valueColor = MaterialTheme.colorScheme.primary
                        )
                    }

                    DetailRow(
                        label = "Depo",
                        value = movement.warehouse ?: "Merkez Depo",
                        icon = Icons.Filled.Storefront
                    )

                    DetailRow(
                        label = "Açıklama",
                        value = movement.user,
                        icon = Icons.Filled.Description
                    )

                    if (!movement.runningBalance.isNullOrBlank()) {
                        DetailRow(
                            label = "İşlem Sonrası Bakiye",
                            value = movement.runningBalance,
                            icon = Icons.Filled.Analytics,
                            valueColor = MaterialTheme.colorScheme.secondary
                        )
                    }
                }
            }
        },
        confirmButton = {
            Button(
                onClick = onDismiss,
                shape = RoundedCornerShape(8.dp),
                modifier = Modifier.testTag("close_movement_detail_btn")
            ) {
                Text("Kapat")
            }
        }
    )
}

@Composable
fun DetailRow(
    label: String,
    value: String,
    icon: ImageVector,
    valueColor: Color = MaterialTheme.colorScheme.onSurface
) {
    Row(
        modifier = Modifier.fillMaxWidth(),
        horizontalArrangement = Arrangement.spacedBy(8.dp),
        verticalAlignment = Alignment.Top
    ) {
        Icon(
            imageVector = icon,
            contentDescription = null,
            tint = MaterialTheme.colorScheme.secondary.copy(alpha = 0.7f),
            modifier = Modifier.size(18.dp).padding(top = 2.dp)
        )
        Column(modifier = Modifier.weight(1f)) {
            Text(
                text = label,
                style = MaterialTheme.typography.labelSmall,
                color = Color.Gray,
                fontWeight = FontWeight.Bold
            )
            Text(
                text = value,
                style = MaterialTheme.typography.bodySmall,
                fontWeight = FontWeight.Medium,
                color = valueColor
            )
        }
    }
}

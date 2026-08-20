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

// Mock database provider matching Catalog and Stocks matching fallbacks
object StockDetailDb {
    val products = listOf(
        ProductCatalog(
            barcode = "8690123456789",
            code = "IND-OIL-20L",
            title = "Ultra Performans Endüstriyel Motor Yağı 20L",
            category = "Endüstriyel Yağlar",
            desc = "Ağır sanayi makineleri için özel formüle edilmiş, yüksek ısıya dayanıklı tam sentetik motor yağı.",
            basePrice = 2450.00,
            dealerPrice = 2150.00,
            wholesalePrice = 1950.00,
            kdvPercent = 20,
            imageUrlColor = Color(0xFFFFB300),
            stockByWarehouse = mapOf("Ana Depo" to 145, "Ankara Merkez" to 42, "Ege Bölge" to 12)
        )
    )

    fun getProductBySearch(query: String?): ProductCatalog? {
        if (query.isNullOrBlank()) return null
        return AppDataStore.products.find { it.barcode == query || it.barcodes.contains(query) || it.code == query }
    }
}

// Simulated data class for ledger timeline
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

    // Statefully bind product from global AppDataStore for real editing reactiveness
    var product by remember(barcode) {
        mutableStateOf(
            AppDataStore.products.find { it.barcode == barcode || it.barcodes.contains(barcode) }
                ?: AppDataStore.products.firstOrNull()
                ?: StockDetailDb.products[0]
        )
    }

    val totalStock = product.stockByWarehouse.values.sum()

    // Simulated/Stateful movements
    var movements by remember(product.code) {
        mutableStateOf<List<StockMovement>>(emptyList())
    }

    var selectedMovementForDetail by remember { mutableStateOf<StockMovement?>(null) }

    LaunchedEffect(product.code) {
        // First set simulated fallback data
        val whList = product.stockByWarehouse.keys.toList()
        val primaryWh = whList.getOrNull(0) ?: "Merkez Depo"
        val secondaryWh = whList.getOrNull(1) ?: "Saha Depo"
        val brandName = product.brand ?: "Mikro"
        val fallback = listOf(
            StockMovement(
                date = "19.06.2026 16:30", 
                type = "Devir Giriş", 
                qty = "${product.stockByWarehouse[primaryWh] ?: 120} ADT", 
                detail = "Evrak: DEV-5057 - Gürbüz Oyuncak Merkez", 
                user = "Başlangıç devir verisi otomatik girişi", 
                evrakNo = "DEV-5057",
                cariName = "Gürbüz Oyuncak Merkez",
                unitPrice = 45.0,
                totalAmount = (product.stockByWarehouse[primaryWh] ?: 120) * 45.0,
                warehouse = primaryWh
            ),
            StockMovement(
                date = "19.06.2026 17:15", 
                type = "Sevk", 
                qty = "12 ADT", 
                detail = "Evrak: SVK-9122 - Ankara Şube Deposu", 
                user = "Saha şubeleri arası stok nakli", 
                evrakNo = "SVK-9122",
                cariName = "Ankara Şube Deposu",
                unitPrice = 0.0,
                totalAmount = 0.0,
                warehouse = "$primaryWh ➜ $secondaryWh"
            ),
            StockMovement(
                date = "20.06.2026 09:12", 
                type = "Saha Satışı", 
                qty = "2 ADT", 
                detail = "Evrak: ST-188 - Yıldırım Metal Döküm A.Ş.", 
                user = "Saha plasiyer mobil sipariş entegrasyonu", 
                evrakNo = "ST-188",
                cariName = "Yıldırım Metal Döküm A.Ş.",
                unitPrice = 125.0,
                totalAmount = 250.0,
                warehouse = secondaryWh
            )
        )
        movements = calculateBalancesForMovements(fallback, totalStock.toDouble())

        // Then attempt to pull real live stock movements from the bridge
        try {
            val sharedPrefs = context.getSharedPreferences("erp_settings", Context.MODE_PRIVATE)
            val apiUrl = sharedPrefs.getString("api_url", "https://d5e4-88-248-2-49.ngrok-free.app") ?: "https://d5e4-88-248-2-49.ngrok-free.app"
            val apiKey = com.example.data.LicenseRepository.getApiKey(context) ?: ""
            
            val apiService = com.example.data.api.ApiClient.getFieldOpsApiService(context, apiUrl, apiKey)
            val response = apiService.getStokHareket(com.example.data.api.PullJobsRequest(tenant_id=sharedPrefs.getString("tenant_id", "T001") ?: "T001", api_key=apiKey, device_id=sharedPrefs.getString("device_id", "DEVICE_DEFAULT") ?: "DEVICE_DEFAULT", agent_version="v2.0", entity="stokHareket", since=product.barcode))
            
            if (response.isSuccessful && response.body() != null) {
                val items = response.body()!!.items
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
                        
                        val quantityFormatted = "${item.miktar ?: (item.girisMiktar ?: item.cikisMiktar ?: 0.0)} ADT"
                        
                        // Look up customer name from AppDataStore
                        val clientName = AppDataStore.customers.find { it.id == item.cariKod }?.name 
                            ?: item.cariKod
                            ?: "Genel Müşteri"
                        
                        val detailText = "Evrak: ${item.evrakNo ?: "Belgesiz"} - $clientName"
                        val userText = item.aciklama ?: "Mikro Kaydı"
                        
                        StockMovement(
                            date = formattedDate,
                            type = moveType,
                            qty = quantityFormatted,
                            detail = detailText,
                            user = userText,
                            evrakNo = item.evrakNo ?: "Belgesiz",
                            cariKod = item.cariKod,
                            cariName = clientName,
                            unitPrice = item.birimFiyat ?: product.dealerPrice,
                            totalAmount = item.tutar ?: ((item.miktar ?: (item.girisMiktar ?: item.cikisMiktar ?: 1.0)) * (item.birimFiyat ?: product.dealerPrice)),
                            warehouse = "Depo: ${if (item.girisDepoNo != null) item.girisDepoNo else if (item.cikisDepoNo != null) item.cikisDepoNo else "Merkez"}"
                        )
                    }
                    movements = calculateBalancesForMovements(realMovements, totalStock.toDouble())
                }
            }
        } catch (e: Exception) {
            e.printStackTrace()
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
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.spacedBy(12.dp),
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                // Category Icon circle
                                Box(
                                    modifier = Modifier
                                        .size(56.dp)
                                        .background(product.imageUrlColor.copy(alpha = 0.15f), CircleShape),
                                    contentAlignment = Alignment.Center
                                ) {
                                    val icon = when (product.category) {
                                        "Endüstriyel Yağlar" -> Icons.Filled.WaterDrop
                                        "Filtre Grupları" -> Icons.Filled.FilterAlt
                                        "Yedek Parça" -> Icons.Filled.SettingsSuggest
                                        else -> Icons.Filled.Construction
                                    }
                                    Icon(
                                        icon,
                                        contentDescription = null,
                                        tint = product.imageUrlColor,
                                        modifier = Modifier.size(24.dp)
                                    )
                                }
                                
                                Column(modifier = Modifier.weight(1f)) {
                                    Text(
                                        text = product.category.uppercase(),
                                        style = MaterialTheme.typography.labelSmall,
                                        color = MaterialTheme.colorScheme.primary,
                                        fontWeight = FontWeight.Bold,
                                        letterSpacing = 1.sp
                                    )
                                    Text(
                                        text = product.title,
                                        style = MaterialTheme.typography.titleMedium,
                                        fontWeight = FontWeight.ExtraBold,
                                        color = MaterialTheme.colorScheme.onBackground,
                                        maxLines = 2,
                                        overflow = TextOverflow.Ellipsis
                                    )
                                }
                            }

                            HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.6f))

                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Column {
                                    Text("ÜRÜN KODU", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                    Text(product.code, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold)
                                }
                                Column {
                                    Text("BARKOD NO", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                    Text(product.barcode, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold)
                                }
                                Surface(
                                    color = if (totalStock > 50) MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.errorContainer,
                                    shape = RoundedCornerShape(8.dp)
                                ) {
                                    Text(
                                        text = if (totalStock > 50) "Bol Stok" else if (totalStock > 0) "Kritik Seviye" else "Tükendi",
                                        modifier = Modifier.padding(horizontal = 8.dp, vertical = 4.dp),
                                        style = MaterialTheme.typography.labelSmall,
                                        fontWeight = FontWeight.Bold,
                                        color = if (totalStock > 50) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.error
                                    )
                                }
                            }

                            if (product.barcodes.size > 1) {
                                Spacer(modifier = Modifier.height(2.dp))
                                Row(
                                    modifier = Modifier.fillMaxWidth(),
                                    horizontalArrangement = Arrangement.spacedBy(6.dp),
                                    verticalAlignment = Alignment.CenterVertically
                                ) {
                                    Text("Ek Barkodlar:", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                    Row(
                                        modifier = Modifier.horizontalScroll(rememberScrollState()),
                                        horizontalArrangement = Arrangement.spacedBy(4.dp)
                                    ) {
                                        product.barcodes.filter { it != product.barcode }.forEach { bar ->
                                            Surface(
                                                color = MaterialTheme.colorScheme.secondaryContainer.copy(alpha = 0.61f),
                                                shape = RoundedCornerShape(6.dp)
                                            ) {
                                                Text(
                                                    text = bar,
                                                    modifier = Modifier.padding(horizontal = 6.dp, vertical = 2.dp),
                                                    style = MaterialTheme.typography.labelSmall,
                                                    fontWeight = FontWeight.Bold,
                                                    color = MaterialTheme.colorScheme.onSecondaryContainer
                                                )
                                            }
                                        }
                                    }
                                }
                            }

                            if (product.desc.isNotBlank()) {
                                Spacer(modifier = Modifier.height(4.dp))
                                RichTextBubble(text = product.desc)
                            }
                        }
                    }
                }

                // --- 3. STORAGE WAREHOUSE BREAKDOWN ---
                item {
                    SectionHeader(title = "Depo Dağılım Matrisi", icon = Icons.Filled.Warehouse)
                }

                item {
                    FieldCard {
                        Column(
                            modifier = Modifier.padding(16.dp),
                            verticalArrangement = Arrangement.spacedBy(14.dp)
                        ) {
                            product.stockByWarehouse.forEach { (whName, qty) ->
                                Column(verticalArrangement = Arrangement.spacedBy(4.dp)) {
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween,
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        Row(
                                            verticalAlignment = Alignment.CenterVertically,
                                            horizontalArrangement = Arrangement.spacedBy(6.dp)
                                        ) {
                                            Icon(
                                                Icons.Filled.Warehouse,
                                                contentDescription = null,
                                                modifier = Modifier.size(16.dp),
                                                tint = MaterialTheme.colorScheme.outline
                                            )
                                            Text(whName, fontWeight = FontWeight.SemiBold, style = MaterialTheme.typography.bodyMedium)
                                        }
                                        Text(
                                            "$qty AD",
                                            fontWeight = FontWeight.Bold,
                                            style = MaterialTheme.typography.bodyLarge,
                                            color = if (qty > 50) MaterialTheme.colorScheme.primary else if (qty > 0) MaterialTheme.colorScheme.secondary else MaterialTheme.colorScheme.error
                                        )
                                    }

                                    // Capacity progress bar
                                    val progressFraction = remember(qty) {
                                        (qty.toFloat() / 350f).coerceIn(0.01f, 1f)
                                    }
                                    Box(
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .height(6.dp)
                                            .background(
                                                MaterialTheme.colorScheme.surfaceVariant,
                                                shape = CircleShape
                                            )
                                    ) {
                                        Box(
                                            modifier = Modifier
                                                .fillMaxHeight()
                                                .fillMaxWidth(progressFraction)
                                                .background(
                                                    if (qty > 50) MaterialTheme.colorScheme.secondary else MaterialTheme.colorScheme.outline,
                                                    shape = CircleShape
                                                )
                                        )
                                    }
                                }
                            }

                            HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.6f))

                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text("Toplam Dağıtılmış Stok:", fontWeight = FontWeight.Bold, style = MaterialTheme.typography.titleSmall)
                                Text(
                                    "$totalStock ADET",
                                    fontWeight = FontWeight.Black,
                                    style = MaterialTheme.typography.titleMedium,
                                    color = MaterialTheme.colorScheme.primary
                                )
                            }
                        }
                    }
                }

                // --- 4. PACKAGE SECTIONS (Koli & Paket Adetleri) ---
                if (product.boxQty != null || product.packageQty != null) {
                    item {
                        SectionHeader(title = "Paketleme & Ambalajlama", icon = Icons.Filled.Inbox)
                    }
                    item {
                        FieldCard {
                            Row(
                                modifier = Modifier.fillMaxWidth().padding(16.dp),
                                horizontalArrangement = Arrangement.SpaceEvenly,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                if (product.boxQty != null) {
                                    Column(horizontalAlignment = Alignment.CenterHorizontally) {
                                        Icon(Icons.Filled.Inbox, contentDescription = null, tint = MaterialTheme.colorScheme.secondary, modifier = Modifier.size(28.dp))
                                        Spacer(modifier = Modifier.height(4.dp))
                                        Text("Koli Kapasitesi", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                        Spacer(modifier = Modifier.height(2.dp))
                                        Text("${product.boxQty} ADET", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold)
                                    }
                                }
                                if (product.boxQty != null && product.packageQty != null) {
                                    VerticalDivider(modifier = Modifier.height(50.dp), color = MaterialTheme.colorScheme.outlineVariant)
                                }
                                if (product.packageQty != null) {
                                    Column(horizontalAlignment = Alignment.CenterHorizontally) {
                                        Icon(Icons.Filled.Layers, contentDescription = null, tint = MaterialTheme.colorScheme.tertiary, modifier = Modifier.size(28.dp))
                                        Spacer(modifier = Modifier.height(4.dp))
                                        Text("Paket İçi Adet", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                        Spacer(modifier = Modifier.height(2.dp))
                                        Text("${product.packageQty} ADET", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold)
                                    }
                                }
                            }
                        }
                    }
                }

                // --- 5. PRICING GRID ---
                item {
                    SectionHeader(title = "Satış Fiyat Tipleri", icon = Icons.Filled.LocalAtm)
                }

                item {
                    val definedPrices = AppDataStore.definitions["Fiyat"] ?: listOf("Perakende", "Bayi", "Toptan")
                    Column(
                        modifier = Modifier.fillMaxWidth(),
                        verticalArrangement = Arrangement.spacedBy(10.dp)
                    ) {
                        definedPrices.chunked(3).forEach { rowItems ->
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.spacedBy(10.dp)
                            ) {
                                rowItems.forEach { priceType ->
                                    val priceVal = product.customPrices[priceType] ?: when (priceType) {
                                        "Perakende" -> product.basePrice
                                        "Bayi" -> product.dealerPrice
                                        "Toptan" -> product.wholesalePrice
                                        else -> 0.0
                                    }
                                    val badge = when (priceType) {
                                        "Perakende" -> "Katalog"
                                        "Bayi" -> "-%12 İsk."
                                        "Toptan" -> "-%20 İsk."
                                        else -> "Özel"
                                    }
                                    val badgeColor = when (priceType) {
                                        "Perakende" -> MaterialTheme.colorScheme.primaryContainer
                                        "Bayi" -> MaterialTheme.colorScheme.secondaryContainer
                                        "Toptan" -> MaterialTheme.colorScheme.errorContainer
                                        else -> MaterialTheme.colorScheme.tertiaryContainer
                                    }
                                    val badgeTextColor = when (priceType) {
                                        "Perakende" -> MaterialTheme.colorScheme.onPrimaryContainer
                                        "Bayi" -> MaterialTheme.colorScheme.onSecondaryContainer
                                        "Toptan" -> MaterialTheme.colorScheme.onErrorContainer
                                        else -> MaterialTheme.colorScheme.onTertiaryContainer
                                    }
                                    PriceMiniCard(
                                        label = priceType,
                                        price = priceVal,
                                        badge = badge,
                                        modifier = Modifier.weight(1f),
                                        badgeColor = badgeColor,
                                        badgeTextColor = badgeTextColor
                                    )
                                }
                                if (rowItems.size < 3) {
                                    for (i in 0 until (3 - rowItems.size)) {
                                        Spacer(modifier = Modifier.weight(1f))
                                    }
                                }
                            }
                        }
                    }
                }

                // --- 6. DETAILS / TECHNICAL SPECIFICATIONS ---
                item {
                    SectionHeader(title = "Teknik Kart Bilgileri", icon = Icons.Filled.List)
                }

                item {
                    FieldCard {
                        Column(
                            modifier = Modifier.padding(16.dp),
                            verticalArrangement = Arrangement.spacedBy(10.dp)
                        ) {
                            SpecRow("Vergi Grubu", "+ %${product.kdvPercent} KDV Dahil")
                            SpecRow("Saha Raf Hücresi", "Koridor C / Bölme-${product.code.takeLast(3)}")
                            SpecRow("Paketleme / Ambalaj", if (product.category.contains("Yağ")) "20L Plastik Bidon" else "Karton Kutu (Korumalı)")
                            SpecRow("Birim Ağırlık", if (product.category.contains("Yağ")) "18.40 Kg" else "1.15 Kg")
                            SpecRow("Saha Sayım Sıklığı", "Haftalık Zorunlu")
                        }
                    }
                }

                // --- 7. RECENT TRANSACTIONS LEDGER ---
                item {
                    SectionHeader(title = "Son Stok Hareketleri", icon = Icons.Filled.History)
                }

                items(movements) { mov ->
                    MovementRow(mov, onClick = { selectedMovementForDetail = mov })
                }
            }
        }
    }

    if (selectedMovementForDetail != null) {
        val movement = selectedMovementForDetail!!
        val docNo = movement.evrakNo ?: movement.detail.substringBefore(" - ").replace("Evrak: ", "").trim()
        val mappedTx = CustomerTx(
            id = docNo,
            date = movement.date,
            type = if (movement.type.contains("Giriş", ignoreCase = true) || movement.type.contains("Giris", ignoreCase = true)) "ALIM" else "SATIŞ",
            amount = movement.totalAmount ?: 0.0,
            description = movement.detail
        )
        val cari = movement.cariName ?: movement.detail.substringAfter(" - ", "Genel Müşteri").trim()
        InvoiceDetailDialog(
            tx = mappedTx,
            customerName = cari,
            onDismiss = { selectedMovementForDetail = null }
        )
    }

    // --- FULL FEATURED INTERACTIVE PRODUCT EDIT DIALOG (Tüm Bilgilerini Güncelleme Formu) ---
    if (showEditDialog) {
        var editTitle by remember { mutableStateOf(product.title) }
        var editCode by remember { mutableStateOf(product.code) }
        var editBarcode by remember { mutableStateOf(product.barcode) }
        var editCategory by remember { mutableStateOf(product.category) }
        var editMarka by remember { mutableStateOf(product.brand ?: "") }
        var editDesc by remember { mutableStateOf(product.desc) }
        val definedPrices = remember { AppDataStore.definitions["Fiyat"] ?: emptyList() }
        val priceValuesMap = remember {
            val map = androidx.compose.runtime.mutableStateMapOf<String, String>()
            definedPrices.forEach { priceType ->
                val priceVal = product.customPrices[priceType] ?: when (priceType) {
                    "Perakende" -> product.basePrice
                    "Bayi" -> product.dealerPrice
                    "Toptan" -> product.wholesalePrice
                    else -> 0.0
                }
                map[priceType] = if (priceVal > 0.0) priceVal.toString() else ""
            }
            map
        }
        var editKdvPercent by remember { mutableStateOf(product.kdvPercent.toString()) }
        
        var editBoxQty by remember { mutableStateOf(product.boxQty?.toString() ?: "") }
        var editPackageQty by remember { mutableStateOf(product.packageQty?.toString() ?: "") }

        var editImageUrl by remember { mutableStateOf(product.imageUrl ?: "") }
        var editLocalImagePath by remember { mutableStateOf(product.localImagePath ?: "") }

        var editStockAnaDepo by remember { mutableStateOf((product.stockByWarehouse["Ana Depo"] ?: 0).toString()) }
        var editStockAnkara by remember { mutableStateOf((product.stockByWarehouse["Ankara Merkez"] ?: 0).toString()) }
        var editStockEge by remember { mutableStateOf((product.stockByWarehouse["Ege Bölge"] ?: 0).toString()) }

        var validationError by remember { mutableStateOf("") }

        androidx.activity.compose.BackHandler { showEditDialog = false }
        Scaffold(
            modifier = Modifier.fillMaxSize(), // Ensure it covers the base Scaffold
            containerColor = MaterialTheme.colorScheme.background,
            topBar = {
                @OptIn(ExperimentalMaterial3Api::class)
                TopAppBar(
                    title = { Text("Stok Kartını Düzenle", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold) },
                    navigationIcon = {
                        IconButton(onClick = { showEditDialog = false }) {
                            Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = "Geri Çık")
                        }
                    }
                )
            }
        ) { editPadding ->
            Box(modifier = Modifier.fillMaxSize().padding(editPadding)) {
                Column(
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(horizontal = 16.dp, vertical = 8.dp)
                ) {
                    // Header
                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.SpaceBetween,
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Text(
                                "Stok Kartını Düzenle",
                                style = MaterialTheme.typography.titleLarge,
                                fontWeight = FontWeight.Black,
                                color = MaterialTheme.colorScheme.primary
                            )
                            IconButton(onClick = { showEditDialog = false }) {
                                Icon(Icons.Filled.Close, contentDescription = "Kapat")
                            }
                        }

                        Spacer(modifier = Modifier.height(12.dp))

                        // Scrollable Form Area
                        Column(
                            modifier = Modifier
                                .weight(1f)
                                .fillMaxWidth()
                                .verticalScroll(rememberScrollState()),
                            verticalArrangement = Arrangement.spacedBy(14.dp)
                        ) {
                            if (validationError.isNotEmpty()) {
                                Surface(
                                    color = MaterialTheme.colorScheme.errorContainer,
                                    shape = RoundedCornerShape(8.dp),
                                    modifier = Modifier.fillMaxWidth()
                                ) {
                                    Text(
                                        text = validationError,
                                        style = MaterialTheme.typography.labelSmall,
                                        color = MaterialTheme.colorScheme.onErrorContainer,
                                        modifier = Modifier.padding(10.dp),
                                        fontWeight = FontWeight.Bold
                                    )
                                }
                            }

                            // 1. Core info
                            OutlinedTextField(
                                value = editTitle,
                                onValueChange = { editTitle = it },
                                label = { Text("Ürün Adı / Başlığı") },
                                modifier = Modifier.fillMaxWidth(),
                                shape = RoundedCornerShape(10.dp)
                            )

                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.spacedBy(10.dp)
                            ) {
                                OutlinedTextField(
                                    value = editCode,
                                    onValueChange = { editCode = it },
                                    label = { Text("Stok SKU Kodu") },
                                    modifier = Modifier.weight(1f),
                                    shape = RoundedCornerShape(10.dp)
                                )
                                OutlinedTextField(
                                    value = editBarcode,
                                    onValueChange = { editBarcode = it },
                                    label = { Text("Barkod No") },
                                    modifier = Modifier.weight(1.2f),
                                    shape = RoundedCornerShape(10.dp),
                                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number)
                                )
                            }

                            // Category & Marka
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.spacedBy(10.dp)
                            ) {
                                Box(modifier = Modifier.weight(1f)) {
                                    SearchableDropdown(
                                        label = "Ürün Kategorisi",
                                        items = AppDataStore.definitions["Kategori"] ?: emptyList(),
                                        selectedValue = editCategory,
                                        onValueChange = { editCategory = it }
                                    )
                                }
                                Box(modifier = Modifier.weight(1f)) {
                                    SearchableDropdown(
                                        label = "Marka",
                                        items = AppDataStore.definitions["Marka"] ?: emptyList(),
                                        selectedValue = editMarka,
                                        onValueChange = { editMarka = it }
                                    )
                                }
                            }

                            OutlinedTextField(
                                value = editDesc,
                                onValueChange = { editDesc = it },
                                label = { Text("Ürün Açıklaması") },
                                modifier = Modifier.fillMaxWidth(),
                                minLines = 2,
                                maxLines = 4,
                                shape = RoundedCornerShape(10.dp)
                            )

                            // 2. Pricing and Taxes (Base, Dealer, Wholesale, KDV)
                            Text("Fiyatlandırma & KDV", style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
                            
                            val priceChunked = definedPrices.chunked(2)
                            priceChunked.forEach { rowItems ->
                                Row(
                                    modifier = Modifier.fillMaxWidth(),
                                    horizontalArrangement = Arrangement.spacedBy(8.dp)
                                ) {
                                    rowItems.forEach { priceType ->
                                        val priceValueStr = priceValuesMap[priceType] ?: ""
                                        val holdsFormula = priceValueStr.isNotBlank() && priceValueStr.toDoubleOrNull() == null
                                        val evaluatedFormulaPrice = if (holdsFormula) {
                                            com.example.util.PriceFormulaEvaluator.evaluate(priceValueStr, priceValuesMap.toMap())
                                        } else null

                                        OutlinedTextField(
                                            value = priceValueStr,
                                            onValueChange = { priceValuesMap[priceType] = it },
                                            label = { Text("$priceType (TL)") },
                                            modifier = Modifier.weight(1f),
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
                                    if (rowItems.size < 2) {
                                        Spacer(modifier = Modifier.weight(1f))
                                    }
                                }
                            }

                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.spacedBy(10.dp),
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Box(modifier = Modifier.weight(1.2f)) {
                                    SearchableDropdown(
                                        label = "KDV Oranı (%)",
                                        items = listOf("0", "1", "8", "10", "18", "20"),
                                        selectedValue = editKdvPercent,
                                        onValueChange = { editKdvPercent = it }
                                    )
                                }
                                Spacer(modifier = Modifier.weight(1f))
                            }

                            // 3. Packaging details
                            Text("Paket & Koli İçi Adetleri", style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.spacedBy(10.dp)
                            ) {
                                OutlinedTextField(
                                    value = editBoxQty,
                                    onValueChange = { editBoxQty = it },
                                    label = { Text("Koli İçi (Adet)") },
                                    modifier = Modifier.weight(1f),
                                    shape = RoundedCornerShape(10.dp),
                                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number)
                                )
                                OutlinedTextField(
                                    value = editPackageQty,
                                    onValueChange = { editPackageQty = it },
                                    label = { Text("Paket İçi (Adet)") },
                                    modifier = Modifier.weight(1f),
                                    shape = RoundedCornerShape(10.dp),
                                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number)
                                )
                            }

                            // 4. Visual Media inputs
                            Text("Ürün Görselleri", style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
                            OutlinedTextField(
                                value = editImageUrl,
                                onValueChange = { editImageUrl = it },
                                label = { Text("Görsel Linkleri (Virgül ile ayrılmış listeler desteklenir)") },
                                placeholder = { Text("https://example.com/resim1.jpg, https://example.com/resim2.jpg") },
                                modifier = Modifier.fillMaxWidth(),
                                shape = RoundedCornerShape(10.dp)
                            )
                            OutlinedTextField(
                                value = editLocalImagePath,
                                onValueChange = { editLocalImagePath = it },
                                label = { Text("Yerel Yakalanan Fotoğraf Dosya Yolu (Opsiyonel)") },
                                modifier = Modifier.fillMaxWidth(),
                                shape = RoundedCornerShape(10.dp)
                            )
                        }

                        Spacer(modifier = Modifier.height(16.dp))

                        // Controls
                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.spacedBy(12.dp)
                        ) {
                            OutlinedButton(
                                onClick = { showEditDialog = false },
                                modifier = Modifier.weight(1f),
                                shape = RoundedCornerShape(12.dp)
                            ) {
                                Text("Vazgeç")
                            }

                            Button(
                                onClick = {
                                    // Resolve formulas or clean doubles
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

                                    val basePriceD = resolvedPrices["Perakende"] ?: 0.0
                                    val dealerPriceD = resolvedPrices["Bayi"] ?: 0.0
                                    val wholesalePriceD = resolvedPrices["Toptan"] ?: 0.0
                                    val kdvI = editKdvPercent.toIntOrNull()
                                    val boxI = editBoxQty.toIntOrNull()
                                    val packI = editPackageQty.toIntOrNull()

                                    if (editTitle.isBlank() || editCode.isBlank() || editBarcode.isBlank()) {
                                        validationError = "Lütfen başlık, kod ve barkod alanlarını doldurun!"
                                    } else if (hasPriceError) {
                                        validationError = "Lütfen tüm fiyat alanlarına geçerli sayısal değerler veya geçerli formüller girin (örn: Perakende * 1.2)!"
                                    } else if (kdvI == null) {
                                        validationError = "Lütfen geçerli KDV oranı girin!"
                                    } else {
                                        validationError = ""
                                        // Find index and update globally in AppDataStore
                                        val idx = AppDataStore.products.indexOfFirst { it.barcode == product.barcode }
                                        if (idx != -1) {
                                            val updated = product.copy(
                                                title = editTitle,
                                                code = editCode,
                                                barcode = editBarcode,
                                                category = editCategory,
                                                brand = if (editMarka.isBlank()) null else editMarka,
                                                desc = editDesc,
                                                basePrice = basePriceD,
                                                dealerPrice = dealerPriceD,
                                                wholesalePrice = wholesalePriceD,
                                                customPrices = resolvedPrices,
                                                kdvPercent = kdvI,
                                                boxQty = boxI,
                                                packageQty = packI,
                                                imageUrl = if (editImageUrl.isBlank()) null else editImageUrl,
                                                localImagePath = if (editLocalImagePath.isBlank()) null else editLocalImagePath,
                                                stockByWarehouse = product.stockByWarehouse
                                            )
                                            AppDataStore.products[idx] = updated
                                            product = updated // Refresh view
                                            AppDataStore.persist(context) // Write to Room Database!
                                            playSound()
                                            showEditDialog = false
                                            scope.launch {
                                                snackbarHostState.showSnackbar("Ürün bilgileri başarıyla veri tabanına kaydedildi.")
                                            }
                                        } else {
                                            validationError = "Hata: Ürün ana listede bulunamadı."
                                        }
                                    }
                                },
                                modifier = Modifier.weight(1.5f).testTag("save_product_detail_btn"),
                                shape = RoundedCornerShape(12.dp)
                            ) {
                                Icon(Icons.Filled.Save, contentDescription = null, modifier = Modifier.size(18.dp))
                                Spacer(modifier = Modifier.width(6.dp))
                                Text("Bilgileri Kaydet")
                            }
                        }
                    }
                }
            }
        }
    }

@Composable
fun SearchableDropdown(
    label: String,
    items: List<String>,
    selectedValue: String,
    onValueChange: (String) -> Unit
) {
    var expanded by remember { mutableStateOf(false) }
    var searchQuery by remember { mutableStateOf("") }
    
    val filteredItems = items.filter { it.contains(searchQuery, ignoreCase = true) }

    Box {
        OutlinedTextField(
            value = selectedValue,
            onValueChange = {},
            readOnly = true,
            label = { Text(label) },
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

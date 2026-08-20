package com.example.ui.screens

import android.net.Uri
import android.widget.Toast
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.navigation.NavController
import com.example.util.DataSyncHelper
import kotlinx.coroutines.launch
import kotlinx.coroutines.async
import kotlinx.coroutines.awaitAll
import kotlinx.coroutines.coroutineScope
import kotlinx.coroutines.withContext
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.sync.withPermit
import java.io.File
import androidx.compose.ui.graphics.Color

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ImportExportScreen(navController: NavController) {
    val context = LocalContext.current
    val scope = rememberCoroutineScope()
    val snackbarHostState = remember { SnackbarHostState() }
    
    var showLoading by remember { mutableStateOf(false) }
    var importProgress by remember { mutableStateOf(0.0f) }
    var importStatusText by remember { mutableStateOf("") }
    
    val defaultCustomersIds = remember { AppDataStore.defaultCustomers.map { it.id }.toSet() }
    val defaultProductsBarcodes = remember { AppDataStore.defaultProducts.map { it.barcode }.toSet() }

    var showDeleteCustomersConfirm by remember { mutableStateOf(false) }
    var showDeleteProductsConfirm by remember { mutableStateOf(false) }
    var showResetDemoConfirm by remember { mutableStateOf(false) }

    val importedCustomersCount = AppDataStore.customers.count { it.id !in defaultCustomersIds }
    val importedProductsCount = AppDataStore.products.count { it.barcode !in defaultProductsBarcodes }
    
    val customerImportLauncher = rememberLauncherForActivityResult(ActivityResultContracts.GetContent()) { uri: Uri? ->
        if (uri != null) {
            scope.launch {
                showLoading = true
                importProgress = 0.0f
                importStatusText = "CSV dosyası okunuyor..."
                kotlinx.coroutines.delay(500)
                
                importProgress = 0.25f
                importStatusText = "Cari hesap bilgileri doğrulanıyor..."
                val newCustomers = DataSyncHelper.parseCustomerCsv(context, uri)
                kotlinx.coroutines.delay(600)
                
                importProgress = 0.60f
                importStatusText = "Sütunlar ve kayıt yapıları ayrıştırılıyor..."
                kotlinx.coroutines.delay(500)
                
                if (newCustomers.isNotEmpty()) {
                    importProgress = 0.85f
                    importStatusText = "Sistem hafızasına aktarılıyor ve yerel veritabanına yazılıyor..."
                    kotlinx.coroutines.delay(600)
                    
                    AppDataStore.customers.addAll(newCustomers)
                    AppDataStore.persist(context)
                    
                    importProgress = 1.0f
                    importStatusText = "Eşitleme başarıyla tamamlandı!"
                    kotlinx.coroutines.delay(500)
                    
                    Toast.makeText(context, "${newCustomers.size} adet cari başarıyla eklendi.", Toast.LENGTH_LONG).show()
                } else {
                    Toast.makeText(context, "Hata: Geçerli cari bulunamadı.", Toast.LENGTH_LONG).show()
                }
                showLoading = false
            }
        }
    }
    
    val productImportLauncher = rememberLauncherForActivityResult(ActivityResultContracts.GetContent()) { uri: Uri? ->
        if (uri != null) {
            scope.launch {
                showLoading = true
                importProgress = 0.0f
                importStatusText = "CSV dosyası okunuyor..."
                kotlinx.coroutines.delay(500)
                
                importProgress = 0.20f
                importStatusText = "Katalog verileri ve barkod yapılandırmaları kontrol ediliyor..."
                val newProducts = DataSyncHelper.parseProductCsv(context, uri)
                kotlinx.coroutines.delay(600)
                
                importProgress = 0.45f
                importStatusText = "KDV oranları, fiyat grupları ve sevk miktarları eşleştiriliyor..."
                kotlinx.coroutines.delay(600)
                
                if (newProducts.isNotEmpty()) {
                    importStatusText = "Resim URL'leri analiz ediliyor..."
                    kotlinx.coroutines.delay(200)
                    
                    val finalProducts = withContext(Dispatchers.IO) {
                        val semaphore = kotlinx.coroutines.sync.Semaphore(15) // Limit concurrent downloads to 15
                        val total = newProducts.size
                        
                        coroutineScope {
                            val deferredList = newProducts.map { prod ->
                                async {
                                    if (!prod.imageUrl.isNullOrBlank()) {
                                        semaphore.withPermit {
                                            try {
                                                val urls = prod.imageUrl.split(Regex("[,;|\\s]+")).map { it.trim() }.filter { it.isNotBlank() }
                                                val downloadedPaths = urls.mapIndexed { urlIndex, url ->
                                                    val fileName = "prod_${prod.barcode}_${urlIndex}_${System.currentTimeMillis()}.jpg"
                                                    DataSyncHelper.downloadImageToLocal(context, url, fileName)
                                                }.filterNotNull()
                                                
                                                val localPathStr = if (downloadedPaths.isNotEmpty()) downloadedPaths.joinToString(",") else null
                                                prod.copy(localImagePath = localPathStr)
                                            } catch (e: Exception) {
                                                prod
                                            }
                                        }
                                    } else {
                                        prod
                                    }
                                }
                            }
                            
                            deferredList.mapIndexed { index, deferred ->
                                val prod = deferred.await()
                                // Throttle UI thread updates to avoid heavy Compose recompositions
                                if (index % 100 == 0 || index == total - 1) {
                                    val ratio = 0.50f + (index.toFloat() / total) * 0.35f
                                    withContext(Dispatchers.Main) {
                                        importProgress = ratio
                                        importStatusText = "Görseller çözümleniyor (${index + 1}/$total)..."
                                    }
                                }
                                prod
                            }
                        }
                    }
                    
                    importProgress = 0.90f
                    importStatusText = "Katalog ürünleri yerel veritabanına kaydediliyor..."
                    kotlinx.coroutines.delay(500)
                    
                    var addedCount = 0
                    var updatedCount = 0
                    
                    for (newProd in finalProducts) {
                        val existingIndex = AppDataStore.products.indexOfFirst { 
                            it.code.trim().equals(newProd.code.trim(), ignoreCase = true) 
                        }
                        if (existingIndex != -1) {
                            val existingProd = AppDataStore.products[existingIndex]
                            
                            // Robust merging: do not overwrite existing values with empty/blank from excel
                            val mergedProd = existingProd.copy(
                                barcode = if (newProd.barcode.isBlank()) existingProd.barcode else newProd.barcode,
                                title = if (newProd.title.isBlank()) existingProd.title else newProd.title,
                                category = if (newProd.category.isBlank()) existingProd.category else newProd.category,
                                desc = if (newProd.desc.isBlank()) existingProd.desc else newProd.desc,
                                basePrice = if (newProd.basePrice == -1.0) existingProd.basePrice else newProd.basePrice,
                                dealerPrice = if (newProd.dealerPrice == -1.0) existingProd.dealerPrice else newProd.dealerPrice,
                                wholesalePrice = if (newProd.wholesalePrice == -1.0) existingProd.wholesalePrice else newProd.wholesalePrice,
                                kdvPercent = if (newProd.kdvPercent == -1) existingProd.kdvPercent else newProd.kdvPercent,
                                brand = if (newProd.brand.isNullOrBlank()) existingProd.brand else newProd.brand,
                                stockByWarehouse = if (newProd.stockByWarehouse.isEmpty()) existingProd.stockByWarehouse else newProd.stockByWarehouse,
                                boxQty = newProd.boxQty ?: existingProd.boxQty,
                                packageQty = newProd.packageQty ?: existingProd.packageQty,
                                imageUrl = if (newProd.imageUrl.isNullOrBlank()) existingProd.imageUrl else newProd.imageUrl,
                                localImagePath = newProd.localImagePath ?: existingProd.localImagePath,
                                aisle = if (newProd.aisle.isNullOrBlank()) existingProd.aisle else newProd.aisle,
                                customPrices = if (newProd.customPrices.isEmpty()) existingProd.customPrices else newProd.customPrices,
                                barcodes = if (newProd.barcodes.isEmpty()) existingProd.barcodes else newProd.barcodes
                            )
                            AppDataStore.products[existingIndex] = mergedProd
                            updatedCount++
                        } else {
                            // Cleaning sentinel values for a brand new product
                            val cleanedProd = newProd.copy(
                                basePrice = if (newProd.basePrice == -1.0) 0.0 else newProd.basePrice,
                                dealerPrice = if (newProd.dealerPrice == -1.0) 0.0 else newProd.dealerPrice,
                                wholesalePrice = if (newProd.wholesalePrice == -1.0) 0.0 else newProd.wholesalePrice,
                                kdvPercent = if (newProd.kdvPercent == -1) 20 else newProd.kdvPercent
                            )
                            AppDataStore.products.add(cleanedProd)
                            addedCount++
                        }
                    }
                    AppDataStore.persist(context)
                    
                    importProgress = 1.0f
                    importStatusText = "Katalog eşitlemesi başarıyla tamamlandı!"
                    kotlinx.coroutines.delay(500)
                    
                    Toast.makeText(context, "$addedCount yeni ürün eklendi, $updatedCount ürün güncellendi.", Toast.LENGTH_LONG).show()
                } else {
                    Toast.makeText(context, "Hata: Geçerli ürün bulunamadı.", Toast.LENGTH_LONG).show()
                }
                showLoading = false
            }
        }
    }
 
    Scaffold(
        snackbarHost = { SnackbarHost(snackbarHostState) }
    ) { paddingValues ->
        Box(modifier = Modifier.fillMaxSize()) {
            LazyColumn(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(paddingValues)
                    .padding(16.dp),
                verticalArrangement = Arrangement.spacedBy(16.dp)
            ) {
                item {
                    Row(
                        verticalAlignment = Alignment.CenterVertically,
                        modifier = Modifier.fillMaxWidth()
                    ) {
                        IconButton(onClick = { navController.popBackStack() }) {
                            Icon(
                                imageVector = Icons.AutoMirrored.Filled.ArrowBack,
                                contentDescription = "Geri"
                            )
                        }
                        Spacer(modifier = Modifier.width(8.dp))
                        Text(
                            "Toplu Veri Aktarımı",
                            style = MaterialTheme.typography.titleLarge,
                            fontWeight = FontWeight.Bold
                        )
                    }
                    Spacer(modifier = Modifier.height(8.dp))
                    Text(
                        "Uygulamaya yeni hesaplar veya ürünler eklemek için Excel kopyanızı (.csv olarak kaydederek) sisteme yükleyebilirsiniz. Test amaçlı 50 gerçek isimli cari ve 300 gerçek isimli ürün dosyalarını doğrudan indirip deneyebilirsiniz.", 
                        style = MaterialTheme.typography.bodyMedium,
                        color = Color.Gray
                    )
                    Spacer(modifier = Modifier.height(8.dp))
                }
                
                // --- CUSTOMERS CARD ---
                item {
                    Card(
                        modifier = Modifier.fillMaxWidth(),
                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f)),
                        shape = androidx.compose.foundation.shape.RoundedCornerShape(16.dp)
                    ) {
                        Column(modifier = Modifier.padding(16.dp)) {
                            Row(verticalAlignment = Alignment.CenterVertically) {
                                Box(
                                    modifier = Modifier
                                        .size(40.dp)
                                        .background(MaterialTheme.colorScheme.primary.copy(alpha = 0.1f), shape = androidx.compose.foundation.shape.CircleShape),
                                    contentAlignment = Alignment.Center
                                ) {
                                    Icon(
                                        Icons.Filled.Group,
                                        contentDescription = "Cari",
                                        tint = MaterialTheme.colorScheme.primary
                                    )
                                }
                                Spacer(modifier = Modifier.width(12.dp))
                                Text(
                                    "Cari (Müşteri) Verileri",
                                    style = MaterialTheme.typography.titleMedium,
                                    fontWeight = FontWeight.Bold
                                )
                            }
                            Spacer(modifier = Modifier.height(16.dp))
                            
                            // Download Buttons
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.spacedBy(8.dp)
                            ) {
                                OutlinedButton(
                                    onClick = {
                                        scope.launch {
                                            val content = DataSyncHelper.generateCustomerCsvTemplate()
                                            val success = DataSyncHelper.saveCsvTemplateToDownloads(context, "Musteri_Sablonu.csv", content)
                                            if(success) {
                                                snackbarHostState.showSnackbar("Boş Müşteri Şablonu indirildi.")
                                            } else {
                                                snackbarHostState.showSnackbar("Şablon kaydedilirken hata oluştu.")
                                            }
                                        }
                                    },
                                    modifier = Modifier.weight(1f),
                                    shape = androidx.compose.foundation.shape.RoundedCornerShape(12.dp),
                                    contentPadding = PaddingValues(vertical = 12.dp)
                                ) {
                                    Icon(Icons.Filled.Download, contentDescription = "Şablon", modifier = Modifier.size(16.dp))
                                    Spacer(modifier = Modifier.width(6.dp))
                                    Text("Şablon İndir", style = MaterialTheme.typography.bodyMedium)
                                }
                                
                                Button(
                                    onClick = {
                                        scope.launch {
                                            val content = DataSyncHelper.generateRealCustomersCsv()
                                            val success = DataSyncHelper.saveCsvTemplateToDownloads(context, "Gercek_50_Musteri_Test.csv", content)
                                            if(success) {
                                                snackbarHostState.showSnackbar("50 Gerçek İsimli Müşteri CSV dosyası İndirilenler klasörüne kaydedildi.")
                                            } else {
                                                snackbarHostState.showSnackbar("Dosya oluşturulurken hata oluştu.")
                                            }
                                        }
                                    },
                                    colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.secondary),
                                    modifier = Modifier.weight(1f),
                                    shape = androidx.compose.foundation.shape.RoundedCornerShape(12.dp),
                                    contentPadding = PaddingValues(vertical = 12.dp)
                                ) {
                                    Icon(Icons.Filled.ContentCopy, contentDescription = "50 Cari", modifier = Modifier.size(16.dp))
                                    Spacer(modifier = Modifier.width(6.dp))
                                    Text("50 Cari İndir", style = MaterialTheme.typography.bodyMedium)
                                }
                            }
                            
                            Spacer(modifier = Modifier.height(8.dp))
                            
                            Button(
                                onClick = { customerImportLauncher.launch("text/*") },
                                modifier = Modifier.fillMaxWidth(),
                                shape = androidx.compose.foundation.shape.RoundedCornerShape(12.dp),
                                contentPadding = PaddingValues(vertical = 14.dp)
                            ) {
                                Icon(Icons.Filled.UploadFile, contentDescription = "Yükle")
                                Spacer(modifier = Modifier.width(8.dp))
                                Text("CSV / Excel ile Cari Yükle", style = MaterialTheme.typography.titleSmall)
                            }
                        }
                    }
                }
                
                // --- PRODUCTS CARD ---
                item {
                    Card(
                        modifier = Modifier.fillMaxWidth(),
                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f)),
                        shape = androidx.compose.foundation.shape.RoundedCornerShape(16.dp)
                    ) {
                        Column(modifier = Modifier.padding(16.dp)) {
                            Row(verticalAlignment = Alignment.CenterVertically) {
                                Box(
                                    modifier = Modifier
                                        .size(40.dp)
                                        .background(MaterialTheme.colorScheme.primary.copy(alpha = 0.1f), shape = androidx.compose.foundation.shape.CircleShape),
                                    contentAlignment = Alignment.Center
                                ) {
                                    Icon(
                                        Icons.Filled.Inventory,
                                        contentDescription = "Stok",
                                        tint = MaterialTheme.colorScheme.primary
                                    )
                                }
                                Spacer(modifier = Modifier.width(12.dp))
                            Text(
                                "Stok (Ürün) Verileri",
                                style = MaterialTheme.typography.titleMedium,
                                fontWeight = FontWeight.Bold
                            )
                        }
                        Spacer(modifier = Modifier.height(8.dp))
                        Text(
                            "Ürünler için dış web linkleri ekleyebilirsiniz (imageUrl sütunu). Aynı ürünün 1'den 10'a kadar farklı resimleri varsa, bu linkleri aralarına noktalı virgül (;) koyarak tek bir hücrede birleştirebilirsiniz. Uygulama resimleri otomatik indirip cihaza çevrimdışı kaydeder.",
                            style = MaterialTheme.typography.bodySmall,
                            color = Color.Gray
                        )
                            Spacer(modifier = Modifier.height(16.dp))
 
                            // Download Buttons
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.spacedBy(8.dp)
                            ) {
                                OutlinedButton(
                                    onClick = {
                                        scope.launch {
                                            val content = DataSyncHelper.generateProductCsvTemplate()
                                            val success = DataSyncHelper.saveCsvTemplateToDownloads(context, "Urun_Sablonu.csv", content)
                                            if(success) {
                                                snackbarHostState.showSnackbar("Boş Ürün Şablonu indirildi.")
                                            } else {
                                                snackbarHostState.showSnackbar("Şablon kaydedilirken hata oluştu.")
                                            }
                                        }
                                    },
                                    modifier = Modifier.weight(1f),
                                    shape = androidx.compose.foundation.shape.RoundedCornerShape(12.dp),
                                    contentPadding = PaddingValues(vertical = 12.dp)
                                ) {
                                    Icon(Icons.Filled.Download, contentDescription = "Şablon", modifier = Modifier.size(16.dp))
                                    Spacer(modifier = Modifier.width(6.dp))
                                    Text("Şablon İndir", style = MaterialTheme.typography.bodyMedium)
                                }
                                
                                Button(
                                    onClick = {
                                        scope.launch {
                                            val content = DataSyncHelper.generateRealProductsCsv()
                                            val success = DataSyncHelper.saveCsvTemplateToDownloads(context, "Gercek_300_Urun_Test.csv", content)
                                            if(success) {
                                                snackbarHostState.showSnackbar("300 Gerçek İsimli Ürün CSV dosyası İndirilenler klasörüne kaydedildi.")
                                            } else {
                                                snackbarHostState.showSnackbar("Dosya oluşturulurken hata oluştu.")
                                            }
                                        }
                                    },
                                    colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.secondary),
                                    modifier = Modifier.weight(1f),
                                    shape = androidx.compose.foundation.shape.RoundedCornerShape(12.dp),
                                    contentPadding = PaddingValues(vertical = 12.dp)
                                ) {
                                    Icon(Icons.Filled.ContentCopy, contentDescription = "300 Ürün", modifier = Modifier.size(16.dp))
                                    Spacer(modifier = Modifier.width(6.dp))
                                    Text("300 Ürün İndir", style = MaterialTheme.typography.bodyMedium)
                                }
                            }
                            
                            Spacer(modifier = Modifier.height(8.dp))
                            
                            Button(
                                onClick = { productImportLauncher.launch("text/*") },
                                modifier = Modifier.fillMaxWidth(),
                                shape = androidx.compose.foundation.shape.RoundedCornerShape(12.dp),
                                contentPadding = PaddingValues(vertical = 14.dp)
                            ) {
                                Icon(Icons.Filled.UploadFile, contentDescription = "Yükle")
                                Spacer(modifier = Modifier.width(8.dp))
                                Text("CSV / Excel ile Ürün Yükle", style = MaterialTheme.typography.titleSmall)
                            }
                        }
                    }
                }

                // --- EXCEL DATA MANAGEMENT SECTION ---
                item {
                    Card(
                        modifier = Modifier.fillMaxWidth().padding(top = 8.dp),
                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.errorContainer.copy(alpha = 0.08f)),
                        border = androidx.compose.foundation.BorderStroke(1.dp, MaterialTheme.colorScheme.error.copy(alpha = 0.35f)),
                        shape = androidx.compose.foundation.shape.RoundedCornerShape(16.dp)
                    ) {
                        Column(modifier = Modifier.padding(16.dp)) {
                            Row(verticalAlignment = Alignment.CenterVertically) {
                                Box(
                                    modifier = Modifier
                                        .size(40.dp)
                                        .background(MaterialTheme.colorScheme.error.copy(alpha = 0.1f), shape = androidx.compose.foundation.shape.CircleShape),
                                    contentAlignment = Alignment.Center
                                ) {
                                    Icon(
                                        Icons.Filled.DeleteSweep,
                                        contentDescription = "Yüklenen Verileri Temizle",
                                        tint = MaterialTheme.colorScheme.error
                                    )
                                }
                                Spacer(modifier = Modifier.width(12.dp))
                                Text(
                                    "Yüklenen Verileri Temizle",
                                    style = MaterialTheme.typography.titleMedium,
                                    fontWeight = FontWeight.Bold,
                                    color = MaterialTheme.colorScheme.error
                                )
                            }
                            Spacer(modifier = Modifier.height(12.dp))
                            Text(
                                "Excel dosyasından sonradan eklediğiniz verileri sistemden temizleyebilir veya tüm sistemi ilk varsayılan demo haline geri döndürebilirsiniz.",
                                style = MaterialTheme.typography.bodySmall,
                                color = MaterialTheme.colorScheme.onSurfaceVariant
                            )
                            Spacer(modifier = Modifier.height(16.dp))

                            // Customer Clean Block
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .background(MaterialTheme.colorScheme.surface.copy(alpha = 0.82f), androidx.compose.foundation.shape.RoundedCornerShape(12.dp))
                                    .padding(12.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Column(modifier = Modifier.weight(1f)) {
                                    Text(
                                        "Müşteri (Cari) Verileri",
                                        style = MaterialTheme.typography.bodyMedium,
                                        fontWeight = FontWeight.Bold
                                    )
                                    Text(
                                        "Toplam: ${AppDataStore.customers.size} cari (${importedCustomersCount} yüklenen veri)",
                                        style = MaterialTheme.typography.bodySmall,
                                        color = Color.Gray
                                    )
                                }
                                Button(
                                    onClick = { showDeleteCustomersConfirm = true },
                                    colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.error),
                                    enabled = importedCustomersCount > 0,
                                    shape = androidx.compose.foundation.shape.RoundedCornerShape(8.dp),
                                    contentPadding = PaddingValues(horizontal = 12.dp, vertical = 6.dp)
                                ) {
                                    Icon(Icons.Filled.Delete, contentDescription = null, modifier = Modifier.size(16.dp))
                                    Spacer(modifier = Modifier.width(4.dp))
                                    Text("Sil", style = MaterialTheme.typography.bodyMedium)
                                }
                            }

                            Spacer(modifier = Modifier.height(12.dp))

                            // Product Clean Block
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .background(MaterialTheme.colorScheme.surface.copy(alpha = 0.82f), androidx.compose.foundation.shape.RoundedCornerShape(12.dp))
                                    .padding(12.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Column(modifier = Modifier.weight(1f)) {
                                    Text(
                                        "Stok (Ürün) Verileri",
                                        style = MaterialTheme.typography.bodyMedium,
                                        fontWeight = FontWeight.Bold
                                    )
                                    Text(
                                        "Toplam: ${AppDataStore.products.size} ürün (${importedProductsCount} yüklenen veri)",
                                        style = MaterialTheme.typography.bodySmall,
                                        color = Color.Gray
                                    )
                                }
                                Button(
                                    onClick = { showDeleteProductsConfirm = true },
                                    colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.error),
                                    enabled = importedProductsCount > 0,
                                    shape = androidx.compose.foundation.shape.RoundedCornerShape(8.dp),
                                    contentPadding = PaddingValues(horizontal = 12.dp, vertical = 6.dp)
                                ) {
                                    Icon(Icons.Filled.Delete, contentDescription = null, modifier = Modifier.size(16.dp))
                                    Spacer(modifier = Modifier.width(4.dp))
                                    Text("Sil", style = MaterialTheme.typography.bodyMedium)
                                }
                            }

                            Spacer(modifier = Modifier.height(16.dp))

                            OutlinedButton(
                                onClick = { showResetDemoConfirm = true },
                                modifier = Modifier.fillMaxWidth(),
                                border = androidx.compose.foundation.BorderStroke(1.dp, MaterialTheme.colorScheme.error.copy(alpha = 0.6f)),
                                colors = ButtonDefaults.outlinedButtonColors(contentColor = MaterialTheme.colorScheme.error),
                                shape = androidx.compose.foundation.shape.RoundedCornerShape(12.dp)
                            ) {
                                Icon(Icons.Filled.Refresh, contentDescription = null, modifier = Modifier.size(16.dp))
                                Spacer(modifier = Modifier.width(6.dp))
                                Text("Tüm Sistemi Fabrika Ayarlarına Sıfırla", style = MaterialTheme.typography.bodyMedium)
                            }
                        }
                    }
                }
            }
            
            // --- PREMIUM DESIGN ANIMATED IMPORT OVERLAY ---
            if (showLoading) {
                androidx.compose.ui.window.Dialog(
                    onDismissRequest = {}
                ) {
                    Card(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(16.dp),
                        shape = androidx.compose.foundation.shape.RoundedCornerShape(24.dp),
                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface)
                    ) {
                        Column(
                            modifier = Modifier
                                .padding(24.dp)
                                .fillMaxWidth(),
                            horizontalAlignment = Alignment.CenterHorizontally,
                            verticalArrangement = Arrangement.spacedBy(16.dp)
                        ) {
                            Box(
                                contentAlignment = Alignment.Center,
                                modifier = Modifier.size(80.dp)
                            ) {
                                CircularProgressIndicator(
                                    progress = { importProgress },
                                    modifier = Modifier.size(72.dp),
                                    color = MaterialTheme.colorScheme.primary,
                                    strokeWidth = 6.dp,
                                    trackColor = MaterialTheme.colorScheme.primary.copy(alpha = 0.15f)
                                )
                                Box(
                                    modifier = Modifier
                                        .size(48.dp)
                                        .background(MaterialTheme.colorScheme.primaryContainer, shape = androidx.compose.foundation.shape.CircleShape),
                                    contentAlignment = Alignment.Center
                                ) {
                                    Icon(
                                        imageVector = Icons.Filled.CloudUpload,
                                        contentDescription = "Excel Yükleniyor",
                                        tint = MaterialTheme.colorScheme.primary,
                                        modifier = Modifier.size(24.dp)
                                    )
                                }
                            }
                            
                            Text(
                                "Excel Veri Aktarımı",
                                style = MaterialTheme.typography.titleMedium,
                                fontWeight = FontWeight.Bold,
                                color = MaterialTheme.colorScheme.onSurface
                            )
                            
                            LinearProgressIndicator(
                                progress = { importProgress },
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .height(8.dp)
                                    .background(MaterialTheme.colorScheme.surfaceVariant, shape = androidx.compose.foundation.shape.CircleShape),
                                color = MaterialTheme.colorScheme.primary
                            )
                            
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text(
                                    text = "%${(importProgress * 100).toInt()}",
                                    style = MaterialTheme.typography.titleLarge,
                                    fontWeight = FontWeight.Black,
                                    color = MaterialTheme.colorScheme.primary
                                )
                                Text(
                                    text = "Eşitleniyor...",
                                    style = MaterialTheme.typography.labelSmall,
                                    color = MaterialTheme.colorScheme.primary,
                                    fontWeight = FontWeight.SemiBold
                                )
                            }
                            
                            Text(
                                text = importStatusText,
                                style = MaterialTheme.typography.bodySmall,
                                color = Color.Gray,
                                textAlign = androidx.compose.ui.text.style.TextAlign.Center,
                                modifier = Modifier.height(40.dp)
                            )
                        }
                    }
                }
            }
        }
    }

    // --- CONFIRMATION DIALOGS ---

    if (showDeleteCustomersConfirm) {
        AlertDialog(
            onDismissRequest = { showDeleteCustomersConfirm = false },
            title = { Text("Carileri Silme Onayı") },
            text = { Text("Sonradan Excel/CSV ile yüklediğiniz $importedCustomersCount adet cari hesabını silmek istediğinize emin misiniz? (Orijinal demo carileri korunacaktır)") },
            confirmButton = {
                Button(
                    colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.error),
                    onClick = {
                        scope.launch {
                            AppDataStore.customers.removeAll { it.id !in defaultCustomersIds }
                            AppDataStore.persist(context)
                            showDeleteCustomersConfirm = false
                            Toast.makeText(context, "Excel ile yüklenen cariler başarıyla silindi.", Toast.LENGTH_SHORT).show()
                        }
                    }
                ) {
                    Text("Evet, Sil")
                }
            },
            dismissButton = {
                TextButton(onClick = { showDeleteCustomersConfirm = false }) {
                    Text("Vazgeç")
                }
            }
        )
    }

    if (showDeleteProductsConfirm) {
        AlertDialog(
            onDismissRequest = { showDeleteProductsConfirm = false },
            title = { Text("Ürünleri Silme Onayı") },
            text = { Text("Sonradan Excel/CSV ile yüklediğiniz $importedProductsCount adet katalog ürününü silmek istediğinize emin misiniz? (Orijinal demo ürünleri korunacaktır)") },
            confirmButton = {
                Button(
                    colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.error),
                    onClick = {
                        scope.launch {
                            AppDataStore.products.removeAll { it.barcode !in defaultProductsBarcodes }
                            AppDataStore.persist(context)
                            showDeleteProductsConfirm = false
                            Toast.makeText(context, "Excel ile yüklenen ürünler başarıyla silindi.", Toast.LENGTH_SHORT).show()
                        }
                    }
                ) {
                    Text("Evet, Sil")
                }
            },
            dismissButton = {
                TextButton(onClick = { showDeleteProductsConfirm = false }) {
                    Text("Vazgeç")
                }
            }
        )
    }

    if (showResetDemoConfirm) {
        AlertDialog(
            onDismissRequest = { showResetDemoConfirm = false },
            title = { Text("Sistemi Sıfırlama Onayı") },
            text = { Text("Tüm cari, ürün, kasa ve banka kayıtlarını temizleyip ilk kurulumdaki varsayılan demo verilerine sıfırlamak istiyor musunuz? Bu işlem geri alınamaz!") },
            confirmButton = {
                Button(
                    colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.error),
                    onClick = {
                        scope.launch {
                            AppDataStore.loadDemoDataSync(context)
                            showResetDemoConfirm = false
                            Toast.makeText(context, "Tüm sistem verileri ilk demo haline sıfırlandı.", Toast.LENGTH_LONG).show()
                        }
                    }
                ) {
                    Text("Evet, Sıfırla")
                }
            },
            dismissButton = {
                TextButton(onClick = { showResetDemoConfirm = false }) {
                    Text("Vazgeç")
                }
            }
        )
    }
}

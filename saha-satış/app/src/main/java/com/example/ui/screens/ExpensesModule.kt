package com.example.ui.screens

import android.widget.Toast
import androidx.compose.animation.*
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
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
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.navigation.NavController
import java.text.SimpleDateFormat
import java.util.*

import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import android.graphics.Bitmap
import android.net.Uri
import java.io.File
import java.io.FileOutputStream
import android.Manifest
import android.content.pm.PackageManager
import androidx.core.content.ContextCompat

// Helpers for caching images
fun saveBitmapToCache(context: android.content.Context, bitmap: Bitmap): String? {
    return try {
        val file = File(context.cacheDir, "cap_${System.currentTimeMillis()}.jpg")
        val out = FileOutputStream(file)
        bitmap.compress(Bitmap.CompressFormat.JPEG, 90, out)
        out.flush()
        out.close()
        file.absolutePath
    } catch (e: Exception) {
        e.printStackTrace()
        null
    }
}

fun saveUriToCache(context: android.content.Context, uri: Uri): String? {
    return try {
        val inputStream = context.contentResolver.openInputStream(uri) ?: return null
        val file = File(context.cacheDir, "pick_${System.currentTimeMillis()}.jpg")
        val out = FileOutputStream(file)
        inputStream.use { input ->
            out.use { output ->
                input.copyTo(output)
            }
        }
        file.absolutePath
    } catch (e: Exception) {
        e.printStackTrace()
        null
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ExpensesModule(navController: NavController) {
    val context = LocalContext.current
    val scope = rememberCoroutineScope()

    // Screen states
    var searchQuery by remember { mutableStateOf("") }
    var selectedCategoryTab by remember { mutableStateOf("Tümü") }
    var showAddDialog by remember { mutableStateOf(false) }
    var expenseToDelete by remember { mutableStateOf<Expense?>(null) }
    var selectedCcBankName by remember { mutableStateOf("") }
    var fullScreenPhotoPath by remember { mutableStateOf<String?>(null) }

    // Selected Calendar for Monthly filters (Default: Now)
    var selectedCalendar by remember { mutableStateOf(Calendar.getInstance().apply { time = Date() }) }

    val turkishMonths = listOf(
        "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran",
        "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık"
    )
    val monthStr = turkishMonths[selectedCalendar.get(Calendar.MONTH)]
    val yearStr = selectedCalendar.get(Calendar.YEAR).toString()
    val selectedMonthYearLabel = "$monthStr $yearStr"

    val monthFilter = String.format(".%02d.%04d", selectedCalendar.get(Calendar.MONTH) + 1, selectedCalendar.get(Calendar.YEAR))

    // Filtered by current selected month first
    val monthlyExpenses = remember(selectedCalendar.time, AppDataStore.expenses.size) {
        AppDataStore.expenses.filter { it.date.contains(monthFilter) }
    }

    LaunchedEffect(AppDataStore.banks.size) {
        if (selectedCcBankName.isEmpty() && AppDataStore.banks.isNotEmpty()) {
            selectedCcBankName = AppDataStore.banks.first().name
        }
    }

    // Dialog state
    var amountInput by remember { mutableStateOf("") }
    var selectedExpenseCategory by remember { mutableStateOf("Yemek") }
    var expenseDescription by remember { mutableStateOf("") }
    var selectedPaymentSourceId by remember { mutableStateOf("CA-MAIN") } // default Merkez Kasa
    var selectedPhotoUriPath by remember { mutableStateOf<String?>(null) }

    // Launcher for Gallery Picker (GetContent contract returns a Uri)
    val galleryLauncher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.GetContent()
    ) { uri: Uri? ->
        if (uri != null) {
            val cachedPath = saveUriToCache(context, uri)
            if (cachedPath != null) {
                selectedPhotoUriPath = cachedPath
            }
        }
    }

    // Launcher for Camera capture (TakePicturePreview contract returns a Bitmap)
    val cameraLauncher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.TakePicturePreview()
    ) { bitmap: Bitmap? ->
        if (bitmap != null) {
            val cachedPath = saveBitmapToCache(context, bitmap)
            if (cachedPath != null) {
                selectedPhotoUriPath = cachedPath
            }
        }
    }

    val cameraPermissionLauncher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.RequestPermission()
    ) { isGranted ->
        if (isGranted) {
            try {
                cameraLauncher.launch(null)
            } catch (e: Exception) {
                Toast.makeText(context, "Kamera başlatılamadı: ${e.localizedMessage}", Toast.LENGTH_SHORT).show()
            }
        } else {
            Toast.makeText(context, "Kamera izni verilmedi. Fotoğraf çekmek için izin gereklidir.", Toast.LENGTH_SHORT).show()
        }
    }

    val categories = listOf("Yemek", "Kırtasiye", "Kargo", "Yol/Yakıt", "Bakım", "Diğer")
    val categoryTabs = listOf("Tümü") + categories

    // Calculate total spend for that month
    val totalExpenseSum = remember(monthlyExpenses) {
        monthlyExpenses.sumOf { it.amount }
    }

    // Filtered list of that month
    val filteredExpenses = remember(searchQuery, selectedCategoryTab, monthlyExpenses) {
        monthlyExpenses.filter { exp ->
            val matchesCategory = selectedCategoryTab == "Tümü" || exp.category == selectedCategoryTab
            val matchesSearch = exp.description.contains(searchQuery, ignoreCase = true) || 
                                exp.category.contains(searchQuery, ignoreCase = true) ||
                                exp.id.contains(searchQuery, ignoreCase = true)
            matchesCategory && matchesSearch
        }
    }

    // Spend distribution by category for visual graph of that month
    val categoryDistribution = remember(monthlyExpenses) {
        categories.associateWith { cat ->
            monthlyExpenses.filter { it.category == cat }.sumOf { it.amount }
        }
    }

    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(16.dp),
        verticalArrangement = Arrangement.spacedBy(16.dp)
    ) {
        // --- MONTH SELECTOR CARD ---
        Card(
            modifier = Modifier.fillMaxWidth(),
            shape = RoundedCornerShape(16.dp),
            colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f)),
            border = androidx.compose.foundation.BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.3f))
        ) {
            Row(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(horizontal = 8.dp, vertical = 6.dp),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                IconButton(
                    onClick = {
                        val newCal = Calendar.getInstance().apply {
                            time = selectedCalendar.time
                            add(Calendar.MONTH, -1)
                        }
                        selectedCalendar = newCal
                    }
                ) {
                    Icon(
                        imageVector = Icons.Filled.ChevronLeft,
                        contentDescription = "Önceki Ay",
                        tint = MaterialTheme.colorScheme.primary
                    )
                }

                Row(
                    verticalAlignment = Alignment.CenterVertically,
                    horizontalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    Icon(
                        imageVector = Icons.Filled.CalendarMonth,
                        contentDescription = null,
                        tint = MaterialTheme.colorScheme.primary,
                        modifier = Modifier.size(20.dp)
                    )
                    Text(
                        text = selectedMonthYearLabel,
                        style = MaterialTheme.typography.titleMedium,
                        fontWeight = FontWeight.Bold,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }

                IconButton(
                    onClick = {
                        val newCal = Calendar.getInstance().apply {
                            time = selectedCalendar.time
                            add(Calendar.MONTH, 1)
                        }
                        selectedCalendar = newCal
                    }
                ) {
                    Icon(
                        imageVector = Icons.Filled.ChevronRight,
                        contentDescription = "Sonraki Ay",
                        tint = MaterialTheme.colorScheme.primary
                    )
                }
            }
        }

        // --- 1. TOTAL STATS CARD ---
        Card(
            modifier = Modifier.fillMaxWidth(),
            shape = RoundedCornerShape(20.dp),
            colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.primaryContainer),
            elevation = CardDefaults.cardElevation(defaultElevation = 2.dp)
        ) {
            Column(
                modifier = Modifier
                    .fillMaxWidth()
                    .background(
                        Brush.horizontalGradient(
                            listOf(
                                MaterialTheme.colorScheme.primaryContainer,
                                MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f)
                            )
                        )
                    )
                    .padding(18.dp)
            ) {
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Column {
                        Text(
                            "Toplam Şirket Gideri",
                            style = MaterialTheme.typography.titleMedium,
                            color = MaterialTheme.colorScheme.onPrimaryContainer.copy(alpha = 0.8f)
                        )
                        Spacer(modifier = Modifier.height(4.dp))
                        Text(
                            text = String.format("%,.2f ₺", totalExpenseSum),
                            style = MaterialTheme.typography.headlineLarge,
                            fontWeight = FontWeight.ExtraBold,
                            color = MaterialTheme.colorScheme.onPrimaryContainer
                        )
                    }
                    IconButton(
                        onClick = { showAddDialog = true },
                        modifier = Modifier
                            .size(52.dp)
                            .background(MaterialTheme.colorScheme.primary, CircleShape)
                            .testTag("add_expense_btn")
                    ) {
                        Icon(
                            imageVector = Icons.Filled.Add,
                            contentDescription = "Gider Ekle",
                            tint = MaterialTheme.colorScheme.onPrimary
                        )
                    }
                }
            }
        }

        // --- 2. CATEGORY CHART DISTRIBUTION ---
        Card(
            modifier = Modifier.fillMaxWidth(),
            shape = RoundedCornerShape(16.dp),
            colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
            border = androidx.compose.foundation.BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
        ) {
            Column(
                modifier = Modifier.padding(14.dp),
                verticalArrangement = Arrangement.spacedBy(10.dp)
            ) {
                Text(
                    text = "Kategori Bazlı Gider Dağılımı",
                    style = MaterialTheme.typography.titleSmall,
                    fontWeight = FontWeight.Bold,
                    color = MaterialTheme.colorScheme.primary
                )

                categories.forEach { cat ->
                    val catAmount = categoryDistribution[cat] ?: 0.0
                    val percentage = if (totalExpenseSum > 0) (catAmount / totalExpenseSum).toFloat() else 0f
                    
                    val catColor = when (cat) {
                        "Yemek" -> Color(0xFFE28743)
                        "Kırtasiye" -> Color(0xFF76B5C5)
                        "Kargo" -> Color(0xFF1E3D59)
                        "Yol/Yakıt" -> Color(0xFFD3A625)
                        "Bakım" -> Color(0xFF1E824C)
                        else -> Color(0xFF8E44AD)
                    }

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
                                Box(
                                    modifier = Modifier
                                        .size(10.dp)
                                        .background(catColor, CircleShape)
                                )
                                Text(
                                    text = cat,
                                    style = MaterialTheme.typography.bodySmall,
                                    fontWeight = FontWeight.Medium
                                )
                            }
                            Text(
                                text = String.format("%,.2f ₺ (%.1f%%)", catAmount, percentage * 100),
                                style = MaterialTheme.typography.bodySmall,
                                fontWeight = FontWeight.Bold,
                                color = MaterialTheme.colorScheme.onSurface
                            )
                        }
                        
                        // Progress Bar
                        Box(
                            modifier = Modifier
                                .fillMaxWidth()
                                .height(6.dp)
                                .clip(CircleShape)
                                .background(MaterialTheme.colorScheme.surfaceVariant)
                        ) {
                            Box(
                                modifier = Modifier
                                    .fillMaxWidth(if (percentage > 0) percentage else 0.001f)
                                    .fillMaxHeight()
                                    .background(catColor)
                            )
                        }
                    }
                }
            }
        }

        // --- 3. FILTER TAB SCROLL AND SEARCH ---
        OutlinedTextField(
            value = searchQuery,
            onValueChange = { searchQuery = it },
            placeholder = { Text("Açıklama, kategori veya ID ara...") },
            leadingIcon = { Icon(Icons.Filled.Search, null) },
            singleLine = true,
            modifier = Modifier.fillMaxWidth(),
            colors = OutlinedTextFieldDefaults.colors(
                unfocusedBorderColor = MaterialTheme.colorScheme.outlineVariant,
                focusedBorderColor = MaterialTheme.colorScheme.primary
            ),
            shape = RoundedCornerShape(12.dp)
        )

        ScrollableTabRow(
            selectedTabIndex = categoryTabs.indexOf(selectedCategoryTab).coerceAtLeast(0),
            edgePadding = 0.dp,
            divider = {},
            containerColor = Color.Transparent,
            modifier = Modifier.fillMaxWidth(),
            indicator = {}
        ) {
            categoryTabs.forEach { tab ->
                val isSelected = selectedCategoryTab == tab
                Tab(
                    selected = isSelected,
                    onClick = { selectedCategoryTab = tab },
                    text = {
                        Text(
                            text = tab,
                            fontWeight = if (isSelected) FontWeight.Bold else FontWeight.Normal,
                            fontSize = 13.sp,
                            color = if (isSelected) MaterialTheme.colorScheme.onPrimary else MaterialTheme.colorScheme.primary
                        )
                    },
                    modifier = Modifier
                        .padding(horizontal = 4.dp, vertical = 2.dp)
                        .clip(RoundedCornerShape(30.dp))
                        .background(
                            if (isSelected) MaterialTheme.colorScheme.primary 
                            else MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.4f)
                        )
                        .height(36.dp)
                )
            }
        }

        // --- 4. LIST OF EXPENSES ---
        if (filteredExpenses.isEmpty()) {
            Box(
                modifier = Modifier
                    .fillMaxWidth()
                    .weight(1f),
                contentAlignment = Alignment.Center
            ) {
                Column(
                    horizontalAlignment = Alignment.CenterHorizontally,
                    verticalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    Icon(
                        imageVector = Icons.Filled.ReceiptLong,
                        contentDescription = null,
                        modifier = Modifier.size(64.dp),
                        tint = MaterialTheme.colorScheme.outline.copy(alpha = 0.4f)
                    )
                    Text(
                        text = "Aranan kriterlere uygun gider kaydı bulunamadı.",
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.outline,
                        textAlign = TextAlign.Center
                    )
                }
            }
        } else {
            LazyColumn(
                modifier = Modifier
                    .fillMaxWidth()
                    .weight(1f),
                verticalArrangement = Arrangement.spacedBy(8.dp)
            ) {
                items(filteredExpenses) { exp ->
                    val catColor = when (exp.category) {
                        "Yemek" -> Color(0xFFE28743)
                        "Kırtasiye" -> Color(0xFF76B5C5)
                        "Kargo" -> Color(0xFF1E3D59)
                        "Yol/Yakıt" -> Color(0xFFD3A625)
                        "Bakım" -> Color(0xFF1E824C)
                        else -> Color(0xFF8E44AD)
                    }

                    val catIcon = when (exp.category) {
                        "Yemek" -> Icons.Filled.Fastfood
                        "Kırtasiye" -> Icons.Filled.Create
                        "Kargo" -> Icons.Filled.LocalShipping
                        "Yol/Yakıt" -> Icons.Filled.TimeToLeave
                        "Bakım" -> Icons.Filled.Build
                        else -> Icons.Filled.Payments
                    }

                    Card(
                        modifier = Modifier.fillMaxWidth(),
                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.2f)),
                        shape = RoundedCornerShape(12.dp),
                        border = androidx.compose.foundation.BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.4f))
                    ) {
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(12.dp),
                            verticalAlignment = Alignment.CenterVertically,
                            horizontalArrangement = Arrangement.SpaceBetween
                        ) {
                            Row(
                                modifier = Modifier.weight(1f),
                                horizontalArrangement = Arrangement.spacedBy(10.dp),
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Box(
                                    modifier = Modifier
                                        .size(40.dp)
                                        .background(catColor.copy(alpha = 0.15f), CircleShape),
                                    contentAlignment = Alignment.Center
                                ) {
                                    Icon(catIcon, null, tint = catColor, modifier = Modifier.size(20.dp))
                                }
                                Column(modifier = Modifier.weight(1f)) {
                                    Row(
                                        verticalAlignment = Alignment.CenterVertically,
                                        horizontalArrangement = Arrangement.spacedBy(6.dp)
                                    ) {
                                        Text(
                                            text = exp.category,
                                            fontWeight = FontWeight.Bold,
                                            style = MaterialTheme.typography.bodyMedium,
                                            color = MaterialTheme.colorScheme.onSurface
                                        )
                                        Surface(
                                            color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f),
                                            shape = RoundedCornerShape(4.dp)
                                        ) {
                                            Text(
                                                text = exp.id,
                                                style = MaterialTheme.typography.labelSmall,
                                                modifier = Modifier.padding(horizontal = 4.dp, vertical = 1.dp),
                                                color = MaterialTheme.colorScheme.onSurfaceVariant
                                            )
                                        }
                                    }
                                    Spacer(modifier = Modifier.height(2.dp))
                                    Text(
                                        text = exp.description,
                                        style = MaterialTheme.typography.bodySmall,
                                        color = MaterialTheme.colorScheme.onSurfaceVariant
                                    )
                                    Spacer(modifier = Modifier.height(2.dp))
                                    Row(
                                        horizontalArrangement = Arrangement.spacedBy(8.dp),
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        Text(
                                            text = exp.date,
                                            style = MaterialTheme.typography.labelSmall,
                                            color = Color.Gray
                                        )
                                        val sourceName = when {
                                            exp.paymentSourceId == "CA-MAIN" -> "Merkez Kasa"
                                            exp.paymentSourceId.startsWith("CA-CC:") -> "Kredi Kartı (${exp.paymentSourceId.substringAfter("CA-CC:")})"
                                            else -> "Banka Hesabı"
                                        }
                                        Text(
                                            text = "• $sourceName",
                                            style = MaterialTheme.typography.labelSmall,
                                            color = MaterialTheme.colorScheme.outline
                                        )
                                    }
                                }
                            }

                            Row(
                                verticalAlignment = Alignment.CenterVertically,
                                horizontalArrangement = Arrangement.spacedBy(8.dp)
                            ) {
                                if (exp.photoUri != null) {
                                    Box(
                                        modifier = Modifier
                                            .size(40.dp)
                                            .clip(RoundedCornerShape(8.dp))
                                            .background(MaterialTheme.colorScheme.surfaceVariant)
                                            .border(1.dp, MaterialTheme.colorScheme.outlineVariant, RoundedCornerShape(8.dp))
                                            .clickable { fullScreenPhotoPath = exp.photoUri },
                                        contentAlignment = Alignment.Center
                                    ) {
                                        coil.compose.AsyncImage(
                                            model = exp.photoUri,
                                            contentDescription = "Gider Fişi",
                                            modifier = Modifier.fillMaxSize(),
                                            contentScale = androidx.compose.ui.layout.ContentScale.Crop
                                        )
                                    }
                                }

                                Text(
                                    text = String.format("-%,.2f ₺", exp.amount),
                                    fontWeight = FontWeight.ExtraBold,
                                    color = MaterialTheme.colorScheme.error,
                                    style = MaterialTheme.typography.bodyMedium
                                )
                                IconButton(
                                    onClick = {
                                        expenseToDelete = exp
                                    },
                                    modifier = Modifier.size(36.dp)
                                ) {
                                    Icon(
                                        imageVector = Icons.Filled.Delete,
                                        contentDescription = "Sil",
                                        tint = MaterialTheme.colorScheme.error.copy(alpha = 0.7f),
                                        modifier = Modifier.size(18.dp)
                                    )
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    // --- DIALOG FOR ADDING EXPENSE ---
    if (showAddDialog) {
        AlertDialog(
            onDismissRequest = { showAddDialog = false },
            title = {
                Text(
                    text = "Yeni Şirket Gideri İşle",
                    fontWeight = FontWeight.Bold,
                    color = MaterialTheme.colorScheme.primary
                )
            },
            text = {
                Column(
                    modifier = Modifier.fillMaxWidth(),
                    verticalArrangement = Arrangement.spacedBy(12.dp)
                ) {
                    OutlinedTextField(
                        value = amountInput,
                        onValueChange = { amountInput = it },
                        label = { Text("Tutar (₺)") },
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Decimal),
                        singleLine = true,
                        modifier = Modifier.fillMaxWidth()
                    )

                    // Category Selector Dropdown (Simulated via FlowRow or elegant radio pills for instant choice)
                    Text(
                        "Gider Kategorisi",
                        style = MaterialTheme.typography.labelMedium,
                        fontWeight = FontWeight.Bold,
                        color = MaterialTheme.colorScheme.primary
                    )
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.spacedBy(4.dp)
                    ) {
                        Column(
                            modifier = Modifier.weight(1f),
                            verticalArrangement = Arrangement.spacedBy(4.dp)
                        ) {
                            categories.take(3).forEach { cat ->
                                Button(
                                    onClick = { selectedExpenseCategory = cat },
                                    colors = ButtonDefaults.buttonColors(
                                        containerColor = if (selectedExpenseCategory == cat) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.surfaceVariant,
                                        contentColor = if (selectedExpenseCategory == cat) MaterialTheme.colorScheme.onPrimary else MaterialTheme.colorScheme.onSurfaceVariant
                                    ),
                                    shape = RoundedCornerShape(8.dp),
                                    modifier = Modifier.fillMaxWidth(),
                                    contentPadding = PaddingValues(horizontal = 4.dp, vertical = 2.dp)
                                ) {
                                    Text(cat, fontSize = 11.sp, fontWeight = FontWeight.Bold)
                                }
                            }
                        }
                        Column(
                            modifier = Modifier.weight(1f),
                            verticalArrangement = Arrangement.spacedBy(4.dp)
                        ) {
                            categories.drop(3).forEach { cat ->
                                Button(
                                    onClick = { selectedExpenseCategory = cat },
                                    colors = ButtonDefaults.buttonColors(
                                        containerColor = if (selectedExpenseCategory == cat) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.surfaceVariant,
                                        contentColor = if (selectedExpenseCategory == cat) MaterialTheme.colorScheme.onPrimary else MaterialTheme.colorScheme.onSurfaceVariant
                                    ),
                                    shape = RoundedCornerShape(8.dp),
                                    modifier = Modifier.fillMaxWidth(),
                                    contentPadding = PaddingValues(horizontal = 4.dp, vertical = 2.dp)
                                ) {
                                    Text(cat, fontSize = 11.sp, fontWeight = FontWeight.Bold)
                                }
                            }
                        }
                    }

                    // Payment source account selection
                    Text(
                        "Kasa / Ödeme Kaynağı",
                        style = MaterialTheme.typography.labelMedium,
                        fontWeight = FontWeight.Bold,
                        color = MaterialTheme.colorScheme.primary
                    )
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.spacedBy(6.dp)
                    ) {
                        Card(
                            modifier = Modifier
                                .weight(1f)
                                .clickable { selectedPaymentSourceId = "CA-MAIN" },
                            colors = CardDefaults.cardColors(
                                containerColor = if (selectedPaymentSourceId == "CA-MAIN") MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surface
                            ),
                            border = androidx.compose.foundation.BorderStroke(
                                width = 1.dp,
                                color = if (selectedPaymentSourceId == "CA-MAIN") MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.outlineVariant
                            )
                        ) {
                            Column(modifier = Modifier.padding(6.dp), horizontalAlignment = Alignment.CenterHorizontally) {
                                Icon(Icons.Filled.Money, null, tint = if (selectedPaymentSourceId == "CA-MAIN") MaterialTheme.colorScheme.primary else Color.Gray, modifier = Modifier.size(20.dp))
                                Spacer(modifier = Modifier.height(2.dp))
                                Text("Merkez Kasa", fontSize = 10.sp, fontWeight = FontWeight.Bold, maxLines = 1)
                            }
                        }

                        Card(
                            modifier = Modifier
                                .weight(1f)
                                .clickable { selectedPaymentSourceId = "CA-BANK" },
                            colors = CardDefaults.cardColors(
                                containerColor = if (selectedPaymentSourceId == "CA-BANK") MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surface
                            ),
                            border = androidx.compose.foundation.BorderStroke(
                                width = 1.dp,
                                color = if (selectedPaymentSourceId == "CA-BANK") MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.outlineVariant
                            )
                        ) {
                            Column(modifier = Modifier.padding(6.dp), horizontalAlignment = Alignment.CenterHorizontally) {
                                Icon(Icons.Filled.AccountBalance, null, tint = if (selectedPaymentSourceId == "CA-BANK") MaterialTheme.colorScheme.primary else Color.Gray, modifier = Modifier.size(20.dp))
                                Spacer(modifier = Modifier.height(2.dp))
                                Text("Banka Hesabı", fontSize = 10.sp, fontWeight = FontWeight.Bold, maxLines = 1)
                            }
                        }

                        Card(
                            modifier = Modifier
                                .weight(1f)
                                .clickable { selectedPaymentSourceId = "CA-CARD" },
                            colors = CardDefaults.cardColors(
                                containerColor = if (selectedPaymentSourceId == "CA-CARD") MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surface
                            ),
                            border = androidx.compose.foundation.BorderStroke(
                                width = 1.dp,
                                color = if (selectedPaymentSourceId == "CA-CARD") MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.outlineVariant
                            )
                        ) {
                            Column(modifier = Modifier.padding(6.dp), horizontalAlignment = Alignment.CenterHorizontally) {
                                Icon(Icons.Filled.CreditCard, null, tint = if (selectedPaymentSourceId == "CA-CARD") MaterialTheme.colorScheme.primary else Color.Gray, modifier = Modifier.size(20.dp))
                                Spacer(modifier = Modifier.height(2.dp))
                                Text("Kredi Kartı", fontSize = 10.sp, fontWeight = FontWeight.Bold, maxLines = 1)
                            }
                        }
                    }

                    if (selectedPaymentSourceId == "CA-CARD") {
                        var bankDropdownExpanded by remember { mutableStateOf(false) }
                        Column(
                            modifier = Modifier.fillMaxWidth(),
                            verticalArrangement = Arrangement.spacedBy(4.dp)
                        ) {
                            Text(
                                "Ödeme Yapılan Banka",
                                style = MaterialTheme.typography.labelMedium,
                                fontWeight = FontWeight.Bold,
                                color = MaterialTheme.colorScheme.primary
                            )
                            
                            Box(modifier = Modifier.fillMaxWidth()) {
                                OutlinedButton(
                                    onClick = { bankDropdownExpanded = true },
                                    modifier = Modifier.fillMaxWidth(),
                                    shape = RoundedCornerShape(8.dp),
                                    contentPadding = PaddingValues(horizontal = 12.dp, vertical = 12.dp)
                                ) {
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween,
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        Text(
                                            text = selectedCcBankName.ifEmpty { "Bankayı Seçiniz..." },
                                            color = MaterialTheme.colorScheme.onSurface,
                                            style = MaterialTheme.typography.bodyMedium
                                        )
                                        Icon(
                                            imageVector = Icons.Filled.ArrowDropDown,
                                            contentDescription = null,
                                            tint = MaterialTheme.colorScheme.primary
                                        )
                                    }
                                }
                                
                                DropdownMenu(
                                    expanded = bankDropdownExpanded,
                                    onDismissRequest = { bankDropdownExpanded = false },
                                    modifier = Modifier.fillMaxWidth()
                                ) {
                                    val banksList = AppDataStore.banks.map { it.name }.ifEmpty { listOf("Garanti Ticari", "Akbank Şirket", "YapıKredi E-Ticaret") }
                                    banksList.forEach { valName ->
                                        DropdownMenuItem(
                                            text = { Text(valName) },
                                            onClick = {
                                                selectedCcBankName = valName
                                                bankDropdownExpanded = false
                                            }
                                        )
                                    }
                                }
                            }
                        }
                    }

                    OutlinedTextField(
                        value = expenseDescription,
                        onValueChange = { expenseDescription = it },
                        label = { Text("Gider Detayı / Açıklama") },
                        placeholder = { Text("örn. Yurtiçi kargo cari sevk ücreti, temizlik deterjan alımı...") },
                        singleLine = false,
                        maxLines = 3,
                        modifier = Modifier.fillMaxWidth()
                    )

                    // --- FOTOĞRAF EKLEME SEÇENEKLERİ ---
                    Text(
                        "Gider Belgesi / Fiş Fotoğrafı",
                        style = MaterialTheme.typography.labelMedium,
                        fontWeight = FontWeight.Bold,
                        color = MaterialTheme.colorScheme.primary
                    )
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.spacedBy(8.dp),
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        OutlinedButton(
                            onClick = {
                                val permissionCheck = ContextCompat.checkSelfPermission(context, Manifest.permission.CAMERA)
                                if (permissionCheck == PackageManager.PERMISSION_GRANTED) {
                                    try {
                                        cameraLauncher.launch(null)
                                    } catch (e: Exception) {
                                        Toast.makeText(context, "Kamera başlatılamadı: ${e.localizedMessage}", Toast.LENGTH_SHORT).show()
                                    }
                                } else {
                                    cameraPermissionLauncher.launch(Manifest.permission.CAMERA)
                                }
                            },
                            modifier = Modifier.weight(1f),
                            shape = RoundedCornerShape(8.dp),
                            contentPadding = PaddingValues(vertical = 10.dp)
                        ) {
                            Icon(Icons.Filled.PhotoCamera, null, modifier = Modifier.size(16.dp))
                            Spacer(modifier = Modifier.width(4.dp))
                            Text("Foto Çek", fontSize = 11.sp, fontWeight = FontWeight.Bold)
                        }

                        OutlinedButton(
                            onClick = { galleryLauncher.launch("image/*") },
                            modifier = Modifier.weight(1f),
                            shape = RoundedCornerShape(8.dp),
                            contentPadding = PaddingValues(vertical = 10.dp)
                        ) {
                            Icon(Icons.Filled.PhotoLibrary, null, modifier = Modifier.size(16.dp))
                            Spacer(modifier = Modifier.width(4.dp))
                            Text("Galeriden Seç", fontSize = 11.sp, fontWeight = FontWeight.Bold)
                        }
                    }

                    // Selected Image Preview
                    if (selectedPhotoUriPath != null) {
                        Box(
                            modifier = Modifier
                                .fillMaxWidth()
                                .height(120.dp)
                                .clip(RoundedCornerShape(8.dp))
                                .background(MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f))
                                .border(1.dp, MaterialTheme.colorScheme.outlineVariant, RoundedCornerShape(8.dp)),
                            contentAlignment = Alignment.Center
                        ) {
                            coil.compose.AsyncImage(
                                model = selectedPhotoUriPath,
                                contentDescription = "Seçilen Fotoğraf",
                                modifier = Modifier.fillMaxSize(),
                                contentScale = androidx.compose.ui.layout.ContentScale.Fit
                            )
                            IconButton(
                                onClick = { selectedPhotoUriPath = null },
                                modifier = Modifier
                                    .align(Alignment.TopEnd)
                                    .padding(4.dp)
                                    .background(Color.Black.copy(alpha = 0.6f), CircleShape)
                                    .size(24.dp)
                            ) {
                                Icon(Icons.Filled.Close, null, tint = Color.White, modifier = Modifier.size(14.dp))
                            }
                        }
                    }
                }
            },
            confirmButton = {
                Button(
                    onClick = {
                        val amountVal = amountInput.toDoubleOrNull()
                        if (amountVal != null && amountVal > 0) {
                            if (expenseDescription.isNotBlank()) {
                                val currentDateStr = SimpleDateFormat("dd.MM.yyyy HH:mm").format(Date())
                                
                                // Determine the stored payment course ID and type details
                                val finalPaymentSourceId = if (selectedPaymentSourceId == "CA-CARD") {
                                    "CA-CC:" + selectedCcBankName.ifEmpty { "Kredi Kartı" }
                                } else {
                                    selectedPaymentSourceId
                                }
                                
                                val paymentTypeStr = when (selectedPaymentSourceId) {
                                    "CA-MAIN" -> "Nakit"
                                    "CA-CARD" -> "Kredi Kartı"
                                    else -> "Banka"
                                }
                                
                                val bankNameStr = when (selectedPaymentSourceId) {
                                    "CA-CARD" -> selectedCcBankName.ifEmpty { "Kredi Kartı" }
                                    else -> selectedPaymentSourceId
                                }

                                // 1. Save Gider entry
                                val expId = "EXP-" + (1000 + AppDataStore.expenses.size)
                                val newExpVal = Expense(
                                    id = expId,
                                    date = currentDateStr,
                                    category = selectedExpenseCategory,
                                    amount = amountVal,
                                    description = expenseDescription.trim(),
                                    paymentSourceId = finalPaymentSourceId,
                                    photoUri = selectedPhotoUriPath
                                )
                                AppDataStore.expenses.add(0, newExpVal) // Newest first
 
                                 // 2. Direct kasa outlet log
                                 val logId = "K-" + (10000 + AppDataStore.kasaLogs.size)
                                 val safeLog = KasaLogItem(
                                     id = logId,
                                     date = currentDateStr,
                                     type = "Tediye",
                                     customerOrSupplier = "Gider: " + selectedExpenseCategory,
                                     amount = amountVal,
                                     paymentType = paymentTypeStr,
                                     bankName = bankNameStr,
                                     desc = expenseDescription.trim()
                                 )
                                 AppDataStore.kasaLogs.add(safeLog)
 
                                 AppDataStore.persist(context)
 
                                 Toast.makeText(context, "Gider başarıyla işlendi ve kasadan düşüldü.", Toast.LENGTH_SHORT).show()
                                 showAddDialog = false
                                 amountInput = ""
                                 expenseDescription = ""
                                 selectedPhotoUriPath = null
                             } else {
                                 Toast.makeText(context, "Lütfen bir açıklama giriniz.", Toast.LENGTH_SHORT).show()
                             }
                         } else {
                             Toast.makeText(context, "Geçerli bir tutar yazınız.", Toast.LENGTH_SHORT).show()
                         }
                     },
                     modifier = Modifier.testTag("save_expense_btn")
                 ) {
                     Text("Deftere Kaydet")
                 }
             },
             dismissButton = {
                 TextButton(onClick = { 
                     showAddDialog = false
                     selectedPhotoUriPath = null
                 }) {
                     Text("İptal")
                 }
             }
         )
     }

    if (expenseToDelete != null) {
        val targetExp = expenseToDelete!!
        AlertDialog(
            onDismissRequest = { expenseToDelete = null },
            icon = { Icon(Icons.Filled.Warning, contentDescription = null, tint = MaterialTheme.colorScheme.error) },
            title = {
                Text("Gider Kaydını Sil", fontWeight = FontWeight.Bold)
            },
            text = {
                val detailSource = when {
                    targetExp.paymentSourceId == "CA-MAIN" -> "Merkez Kasa"
                    targetExp.paymentSourceId.startsWith("CA-CC:") -> "Kredi Kartı (${targetExp.paymentSourceId.substringAfter("CA-CC:")})"
                    else -> "Banka Hesabı"
                }
                Text("${targetExp.description} açıklamalı, $detailSource kaynaklı ve ${String.format("%,.2f ₺", targetExp.amount)} tutarlı gider kaydını silmek istediğinize emin misiniz? İlgili kasa/banka kaydı da otomatik olarak geri alınarak bakiye düzeltilecektir.")
            },
            confirmButton = {
                Button(
                    onClick = {
                        val match = AppDataStore.kasaLogs.find { log ->
                            log.amount == targetExp.amount && 
                            log.desc == targetExp.description &&
                            (
                                (targetExp.paymentSourceId == "CA-MAIN" && log.paymentType == "Nakit") ||
                                (targetExp.paymentSourceId == "CA-BANK" && log.paymentType == "Banka") ||
                                (targetExp.paymentSourceId.startsWith("CA-CC:") && log.paymentType == "Kredi Kartı" && log.bankName == targetExp.paymentSourceId.substringAfter("CA-CC:"))
                            )
                        }
                        if (match != null) {
                            AppDataStore.kasaLogs.remove(match)
                        }
                        AppDataStore.expenses.remove(targetExp)
                        AppDataStore.persist(context)
                        Toast.makeText(context, "Gider kaydı ve ilgili kasa çıkışı iptal edildi.", Toast.LENGTH_SHORT).show()
                        expenseToDelete = null
                    },
                    colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.error)
                ) {
                    Text("Evet, Sil")
                }
            },
            dismissButton = {
                TextButton(onClick = { expenseToDelete = null }) {
                    Text("Vazgeç")
                }
            }
        )
    }

    if (fullScreenPhotoPath != null) {
        androidx.compose.ui.window.Dialog(onDismissRequest = { fullScreenPhotoPath = null }) {
            Card(
                shape = RoundedCornerShape(16.dp),
                colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface)
            ) {
                Column(
                    modifier = Modifier.padding(16.dp),
                    horizontalAlignment = Alignment.CenterHorizontally,
                    verticalArrangement = Arrangement.spacedBy(12.dp)
                ) {
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.SpaceBetween,
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Text("Gider Belgesi / Fiş", fontWeight = FontWeight.Bold, style = MaterialTheme.typography.titleMedium)
                        IconButton(onClick = { fullScreenPhotoPath = null }) {
                            Icon(Icons.Filled.Close, null)
                        }
                    }
                    coil.compose.AsyncImage(
                        model = fullScreenPhotoPath,
                        contentDescription = "Büyük Fiş Görseli",
                        modifier = Modifier
                            .fillMaxWidth()
                            .heightIn(max = 400.dp)
                            .clip(RoundedCornerShape(8.dp)),
                        contentScale = androidx.compose.ui.layout.ContentScale.Fit
                    )
                }
            }
        }
    }
}

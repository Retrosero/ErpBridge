package com.example.ui.screens

import android.widget.Toast
import androidx.compose.animation.*
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.navigation.NavController
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import android.graphics.Bitmap
import android.net.Uri
import android.Manifest
import android.content.pm.PackageManager
import androidx.core.content.ContextCompat
import org.json.JSONArray
import org.json.JSONObject
import java.text.SimpleDateFormat
import java.util.*

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun VehiclesModule(navController: NavController) {
    val context = LocalContext.current

    // Screen state
    var searchQuery by remember { mutableStateOf("") }
    var selectedVehicleForDetails by remember { mutableStateOf<Vehicle?>(null) }
    
    // Main dialogs
    var showAddVehicleDialog by remember { mutableStateOf(false) }
    var showUpdateKmDialog by remember { mutableStateOf<Vehicle?>(null) }
    var showAddMaintenanceDialog by remember { mutableStateOf<Vehicle?>(null) }
    var vehicleToDelete by remember { mutableStateOf<Vehicle?>(null) }

    // New vehicle input fields
    var plateInput by remember { mutableStateOf("") }
    var brandModelInput by remember { mutableStateOf("") }
    var currentKmInput by remember { mutableStateOf("") }
    var lastOilChangeInput by remember { mutableStateOf("") }
    var nextOilChangeInput by remember { mutableStateOf("") }
    var lastMaintenanceDateInput by remember { mutableStateOf("") }
    var nextMaintenanceDateInput by remember { mutableStateOf("") }
    var notesInput by remember { mutableStateOf("") }

    // Quick Update KM fields
    var quickKmInput by remember { mutableStateOf("") }

    // New Maintenance fields
    var maintCostInput by remember { mutableStateOf("") }
    var maintDescInput by remember { mutableStateOf("") }
    var nextOilTargetInput by remember { mutableStateOf("") }
    var nextMaintDateTargetInput by remember { mutableStateOf("") }
    var payFromKasaCheckbox by remember { mutableStateOf(true) }
    var maintKasaSelectionId by remember { mutableStateOf("CA-MAIN") } // default Merkez Kasa

    var selectedMaintPhotoPath by remember { mutableStateOf<String?>(null) }
    var fullScreenPhotoPath by remember { mutableStateOf<String?>(null) }

    // Launcher for Gallery Picker (GetContent contract returns a Uri)
    val galleryLauncher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.GetContent()
    ) { uri: Uri? ->
        if (uri != null) {
            val cachedPath = saveUriToCache(context, uri)
            if (cachedPath != null) {
                selectedMaintPhotoPath = cachedPath
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
                selectedMaintPhotoPath = cachedPath
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

    // Status Helper to yield alerts
    fun getMaintenanceAlerts(v: Vehicle): List<Pair<String, Color>> {
        val alerts = mutableListOf<Pair<String, Color>>()
        
        // Oil check
        val remainsOil = v.nextOilChangeKm - v.currentKm
        if (remainsOil < 0) {
            alerts.add("Yağ Değişimi Gecikti (${-remainsOil} Fark/KM)" to Color(0xFFC0392B)) // red
        } else if (remainsOil <= 1500) {
            alerts.add("Yağ Değişimi Yaklaşıyor ($remainsOil KM kaldı)" to Color(0xFFD35400)) // orange
        }

        // Date check
        try {
            val sdf = SimpleDateFormat("dd.MM.yyyy", Locale.getDefault())
            val nextDate = sdf.parse(v.nextMaintenanceDate)
            val today = Date()
            if (nextDate != null) {
                val diffMs = nextDate.time - today.time
                val diffDays = diffMs / (1000 * 60 * 60 * 24)
                if (diffDays < 0) {
                    alerts.add("Bakım Tarihi Geçti (${-diffDays} Gün Önce)" to Color(0xFFC0392B)) // red
                } else if (diffDays <= 30) {
                    alerts.add("Bakım Zamanı Yaklaştı ($diffDays Gün Kaldı)" to Color(0xFFD35400)) // orange
                }
            }
        } catch (e: Exception) {
            // catch parsing
        }
        return alerts
    }

    // Statistics counts
    val (totalVehicles, warningVehicles, criticalVehicles) = remember(AppDataStore.vehicles.size, AppDataStore.vehicles.map { it.currentKm }) {
        var warnCount = 0
        var critCount = 0
        AppDataStore.vehicles.forEach { v ->
            val alerts = getMaintenanceAlerts(v)
            if (alerts.any { it.second == Color(0xFFC0392B) }) {
                critCount++
            } else if (alerts.any { it.second == Color(0xFFD35400) }) {
                warnCount++
            }
        }
        Triple(AppDataStore.vehicles.size, warnCount, critCount)
    }

    // Filter vehicles
    val filteredVehicles = remember(searchQuery, AppDataStore.vehicles.size, AppDataStore.vehicles.map { it.currentKm }) {
        AppDataStore.vehicles.filter { v ->
            v.plate.contains(searchQuery, ignoreCase = true) || 
            v.brandModel.contains(searchQuery, ignoreCase = true) ||
            v.notes.contains(searchQuery, ignoreCase = true)
        }
    }

    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(16.dp),
        verticalArrangement = Arrangement.spacedBy(16.dp)
    ) {
        // --- 1. STATISTICS STATUS BANNER ---
        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.spacedBy(10.dp)
        ) {
            Card(
                modifier = Modifier.weight(1f),
                colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
                shape = RoundedCornerShape(12.dp),
                border = androidx.compose.foundation.BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
            ) {
                Column(
                    modifier = Modifier.padding(12.dp),
                    horizontalAlignment = Alignment.CenterHorizontally
                ) {
                    Icon(Icons.Filled.DirectionsCar, null, tint = MaterialTheme.colorScheme.primary)
                    Spacer(modifier = Modifier.height(4.dp))
                    Text("Filomuz", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                    Text("$totalVehicles Araç", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold)
                }
            }

            Card(
                modifier = Modifier.weight(1f),
                colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
                shape = RoundedCornerShape(12.dp),
                border = androidx.compose.foundation.BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
            ) {
                Column(
                    modifier = Modifier.padding(12.dp),
                    horizontalAlignment = Alignment.CenterHorizontally
                ) {
                    Icon(Icons.Filled.Warning, null, tint = Color(0xFFD35400))
                    Spacer(modifier = Modifier.height(4.dp))
                    Text("Yaklaşan", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                    Text("$warningVehicles Araç", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold, color = Color(0xFFD35400))
                }
            }

            Card(
                modifier = Modifier.weight(1f),
                colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
                shape = RoundedCornerShape(12.dp),
                border = androidx.compose.foundation.BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
            ) {
                Column(
                    modifier = Modifier.padding(12.dp),
                    horizontalAlignment = Alignment.CenterHorizontally
                ) {
                    Icon(Icons.Filled.Error, null, tint = Color(0xFFC0392B))
                    Spacer(modifier = Modifier.height(4.dp))
                    Text("Gecikenler", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                    Text("$criticalVehicles Araç", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold, color = Color(0xFFC0392B))
                }
            }
        }

        // --- 2. SEARCH AND ADD SECTION ---
        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.spacedBy(8.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            OutlinedTextField(
                value = searchQuery,
                onValueChange = { searchQuery = it },
                placeholder = { Text("Plaka veya araç modeli ara...") },
                leadingIcon = { Icon(Icons.Filled.Search, null) },
                singleLine = true,
                modifier = Modifier.weight(1f),
                colors = OutlinedTextFieldDefaults.colors(
                    unfocusedBorderColor = MaterialTheme.colorScheme.outlineVariant,
                    focusedBorderColor = MaterialTheme.colorScheme.primary
                ),
                shape = RoundedCornerShape(12.dp)
            )

            Button(
                onClick = { 
                    plateInput = ""
                    brandModelInput = ""
                    currentKmInput = ""
                    lastOilChangeInput = ""
                    nextOilChangeInput = ""
                    lastMaintenanceDateInput = SimpleDateFormat("dd.MM.yyyy", Locale.getDefault()).format(Date())
                    nextMaintenanceDateInput = ""
                    notesInput = ""
                    showAddVehicleDialog = true 
                },
                modifier = Modifier.height(52.dp),
                shape = RoundedCornerShape(12.dp)
            ) {
                Icon(Icons.Filled.Add, null)
                Spacer(modifier = Modifier.width(4.dp))
                Text("Yeni", fontWeight = FontWeight.Bold)
            }
        }

        // --- 3. LIST OF VEHICLES ---
        if (filteredVehicles.isEmpty()) {
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
                    Icon(Icons.Filled.TimeToLeave, null, modifier = Modifier.size(64.dp), tint = MaterialTheme.colorScheme.outline.copy(alpha = 0.5f))
                    Text("Kayıtlı şirket aracı bulunmamaktadır.", style = MaterialTheme.typography.bodyMedium, color = MaterialTheme.colorScheme.outline)
                }
            }
        } else {
            LazyColumn(
                modifier = Modifier
                    .fillMaxWidth()
                    .weight(1f),
                verticalArrangement = Arrangement.spacedBy(10.dp)
            ) {
                items(filteredVehicles) { vehicle ->
                    val alerts = getMaintenanceAlerts(vehicle)
                    val cardBorderColor = if (alerts.any { it.second == Color(0xFFC0392B) }) {
                        Color(0xFFC0392B).copy(alpha = 0.5f)
                    } else if (alerts.any { it.second == Color(0xFFD35400) }) {
                        Color(0xFFD35400).copy(alpha = 0.5f)
                    } else {
                        MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f)
                    }

                    Card(
                        modifier = Modifier.fillMaxWidth(),
                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
                        shape = RoundedCornerShape(14.dp),
                        border = androidx.compose.foundation.BorderStroke(2.dp, cardBorderColor)
                    ) {
                        Column(modifier = Modifier.padding(14.dp), verticalArrangement = Arrangement.spacedBy(8.dp)) {
                            // Plate and Brand banner
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Row(
                                    verticalAlignment = Alignment.CenterVertically,
                                    horizontalArrangement = Arrangement.spacedBy(10.dp)
                                ) {
                                    // Simulated plate card visual representation
                                    Box(
                                        modifier = Modifier
                                            .border(1.dp, Color.Blue, RoundedCornerShape(4.dp))
                                            .background(Color.White)
                                            .padding(horizontal = 8.dp, vertical = 2.dp)
                                    ) {
                                        Text(
                                            text = vehicle.plate,
                                            fontWeight = FontWeight.ExtraBold,
                                            style = MaterialTheme.typography.titleMedium,
                                            color = Color.Black
                                        )
                                    }

                                    Column {
                                        Text(vehicle.brandModel, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold)
                                        Text(
                                            text = String.format("%,d KM", vehicle.currentKm),
                                            style = MaterialTheme.typography.bodySmall,
                                            fontWeight = FontWeight.Bold,
                                            color = MaterialTheme.colorScheme.primary
                                        )
                                    }
                                }

                                IconButton(
                                    onClick = {
                                        vehicleToDelete = vehicle
                                    },
                                    modifier = Modifier.size(36.dp)
                                ) {
                                    Icon(Icons.Filled.Delete, "Sil", tint = MaterialTheme.colorScheme.error.copy(alpha = 0.7f), modifier = Modifier.size(18.dp))
                                }
                            }

                            Divider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.4f))

                            // Alert messages
                            if (alerts.isNotEmpty()) {
                                Column(verticalArrangement = Arrangement.spacedBy(4.dp)) {
                                    alerts.forEach { alert ->
                                        Row(
                                            verticalAlignment = Alignment.CenterVertically,
                                            horizontalArrangement = Arrangement.spacedBy(6.dp),
                                            modifier = Modifier
                                                .fillMaxWidth()
                                                .background(alert.second.copy(alpha = 0.1f), RoundedCornerShape(6.dp))
                                                .padding(horizontal = 8.dp, vertical = 6.dp)
                                        ) {
                                            Icon(Icons.Filled.Info, null, tint = alert.second, modifier = Modifier.size(16.dp))
                                            Text(
                                                text = alert.first,
                                                fontSize = 11.sp,
                                                fontWeight = FontWeight.Bold,
                                                color = alert.second
                                            )
                                        }
                                    }
                                }
                            } else {
                                Row(
                                    verticalAlignment = Alignment.CenterVertically,
                                    horizontalArrangement = Arrangement.spacedBy(6.dp),
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .background(Color(0xFF2E7D32).copy(alpha = 0.1f), RoundedCornerShape(6.dp))
                                        .padding(horizontal = 8.dp, vertical = 6.dp)
                                ) {
                                    Icon(Icons.Filled.CheckCircle, null, tint = Color(0xFF2E7D32), modifier = Modifier.size(16.dp))
                                    Text(
                                        text = "Tüm periyodik bakımlar ve KM yağ takibi güncel durumda.",
                                        fontSize = 11.sp,
                                        fontWeight = FontWeight.Bold,
                                        color = Color(0xFF2E7D32)
                                    )
                                }
                            }

                            // Info details
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Column {
                                    Text("Son Yağ Bakımı: ${vehicle.lastOilChangeKm} KM", style = MaterialTheme.typography.bodySmall, color = Color.Gray)
                                    Text("Sonraki Yağ Hedefi: ${vehicle.nextOilChangeKm} KM", style = MaterialTheme.typography.bodySmall, color = Color.Gray)
                                    Text("Gelecek Servis Tarihi: ${vehicle.nextMaintenanceDate}", style = MaterialTheme.typography.bodySmall, color = Color.Gray)
                                }
                                
                                TextButton(
                                    onClick = { selectedVehicleForDetails = vehicle },
                                    contentPadding = PaddingValues(0.dp)
                                ) {
                                    Text("Geçmişi Gör", fontSize = 12.sp, fontWeight = FontWeight.Bold)
                                    Icon(Icons.Filled.ArrowForwardIos, null, modifier = Modifier.size(12.dp))
                                }
                            }

                            // Short quick actions row
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.spacedBy(8.dp)
                            ) {
                                OutlinedButton(
                                    onClick = { 
                                        quickKmInput = vehicle.currentKm.toString()
                                        showUpdateKmDialog = vehicle 
                                    },
                                    modifier = Modifier.weight(1f),
                                    shape = RoundedCornerShape(8.dp)
                                ) {
                                    Icon(Icons.Filled.Speed, null, modifier = Modifier.size(16.dp))
                                    Spacer(modifier = Modifier.width(4.dp))
                                    Text("KM Güncelle", fontSize = 11.sp, fontWeight = FontWeight.Bold)
                                }

                                Button(
                                    onClick = { 
                                        maintCostInput = ""
                                        maintDescInput = ""
                                        nextOilTargetInput = (vehicle.currentKm + 10000).toString()
                                        
                                        // Next maint date targeted 1 year from now
                                        val cal = Calendar.getInstance()
                                        cal.add(Calendar.YEAR, 1)
                                        nextMaintDateTargetInput = SimpleDateFormat("dd.MM.yyyy", Locale.getDefault()).format(cal.time)
                                        
                                        showAddMaintenanceDialog = vehicle 
                                    },
                                    modifier = Modifier.weight(1.2f),
                                    shape = RoundedCornerShape(8.dp)
                                ) {
                                    Icon(Icons.Filled.Build, null, modifier = Modifier.size(16.dp))
                                    Spacer(modifier = Modifier.width(4.dp))
                                    Text("Bakım Kaydet", fontSize = 11.sp, fontWeight = FontWeight.Bold)
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    // --- 4. DETAILS AND MAINTENANCE HISTORY MODAL DIALOG ---
    if (selectedVehicleForDetails != null) {
        val detailVehicle = selectedVehicleForDetails!!
        
        // Deserialize its maintenance list
        val historyList = remember(detailVehicle.maintenanceHistoryJson) {
            val list = mutableListOf<VehicleMaintenance>()
            try {
                if (detailVehicle.maintenanceHistoryJson.isNotBlank()) {
                    val arr = JSONArray(detailVehicle.maintenanceHistoryJson)
                    for (i in 0 until arr.length()) {
                        val obj = arr.getJSONObject(i)
                        val pUri = if (obj.has("photoUri")) obj.getString("photoUri") else null
                        list.add(
                            VehicleMaintenance(
                                id = obj.getString("id"),
                                date = obj.getString("date"),
                                km = obj.getInt("km"),
                                description = obj.getString("description"),
                                cost = obj.getDouble("cost"),
                                photoUri = if (pUri == "null" || pUri.isNullOrEmpty()) null else pUri
                            )
                        )
                    }
                }
            } catch (e: Exception) {
                e.printStackTrace()
            }
            list
        }

        AlertDialog(
            onDismissRequest = { selectedVehicleForDetails = null },
            title = {
                Text("${detailVehicle.plate} - Bakım & Masraf Geçmişi", fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
            },
            text = {
                Column(
                    modifier = Modifier
                        .fillMaxWidth()
                        .heightIn(max = 400.dp)
                        .verticalScroll(rememberScrollState()),
                    verticalArrangement = Arrangement.spacedBy(10.dp)
                ) {
                    Text("Araç Notları:", fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodySmall)
                    Text(
                        text = if (detailVehicle.notes.isNotBlank()) detailVehicle.notes else "Eklenmiş bir not bulunmamaktadır.",
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                    
                    Text("Bakım Kayıt Günlüğü (${historyList.size}):", fontWeight = FontWeight.Bold, style = MaterialTheme.typography.titleSmall, color = MaterialTheme.colorScheme.primary)
                    
                    if (historyList.isEmpty()) {
                        Text("Kayıtlı herhangi bir servis/bakım faturası bulunamadı.", style = MaterialTheme.typography.bodySmall, color = Color.Gray)
                    } else {
                        historyList.forEach { m ->
                            Card(
                                modifier = Modifier.fillMaxWidth(),
                                colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.3f))
                            ) {
                                Column(modifier = Modifier.padding(10.dp), verticalArrangement = Arrangement.spacedBy(4.dp)) {
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween
                                    ) {
                                        Text(m.date, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodySmall)
                                        Text(
                                            text = String.format("%,.2f ₺", m.cost),
                                            fontWeight = FontWeight.ExtraBold,
                                            color = MaterialTheme.colorScheme.primary,
                                            style = MaterialTheme.typography.bodySmall
                                        )
                                    }
                                    Text("Kilometre: ${m.km} KM", style = MaterialTheme.typography.labelSmall, color = Color.Gray)
                                    
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween,
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        Text(m.description, style = MaterialTheme.typography.bodySmall, modifier = Modifier.weight(1f))
                                        if (m.photoUri != null) {
                                            Spacer(modifier = Modifier.width(6.dp))
                                            Box(
                                                modifier = Modifier
                                                    .size(36.dp)
                                                    .clip(RoundedCornerShape(6.dp))
                                                    .background(MaterialTheme.colorScheme.surfaceVariant)
                                                    .border(1.dp, MaterialTheme.colorScheme.outlineVariant, RoundedCornerShape(6.dp))
                                                    .clickable { fullScreenPhotoPath = m.photoUri },
                                                contentAlignment = Alignment.Center
                                            ) {
                                                coil.compose.AsyncImage(
                                                    model = m.photoUri,
                                                    contentDescription = "Onarım Belgesi",
                                                    modifier = Modifier.fillMaxSize(),
                                                    contentScale = androidx.compose.ui.layout.ContentScale.Crop
                                                )
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            },
            confirmButton = {
                TextButton(onClick = { selectedVehicleForDetails = null }) {
                    Text("Kapat")
                }
            }
        )
    }

    // --- 5. DIALOG FOR ADDING VEHICLE ---
    if (showAddVehicleDialog) {
        AlertDialog(
            onDismissRequest = { showAddVehicleDialog = false },
            title = {
                Text("Yeni Şirket Aracı Tanımla", fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
            },
            text = {
                Column(
                    modifier = Modifier
                        .fillMaxWidth()
                        .heightIn(max = 400.dp)
                        .verticalScroll(rememberScrollState()),
                    verticalArrangement = Arrangement.spacedBy(10.dp)
                ) {
                    OutlinedTextField(
                        value = plateInput,
                        onValueChange = { plateInput = it.uppercase() },
                        label = { Text("Plaka (örn. 34 ABC 123)") },
                        singleLine = true,
                        modifier = Modifier.fillMaxWidth()
                    )

                    OutlinedTextField(
                        value = brandModelInput,
                        onValueChange = { brandModelInput = it },
                        label = { Text("Marka / Model (örn. Fiat Egea 1.3 MJet)") },
                        singleLine = true,
                        modifier = Modifier.fillMaxWidth()
                    )

                    OutlinedTextField(
                        value = currentKmInput,
                        onValueChange = { currentKmInput = it },
                        label = { Text("Mevcut Kilometre (KM)") },
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                        singleLine = true,
                        modifier = Modifier.fillMaxWidth()
                    )

                    OutlinedTextField(
                        value = lastOilChangeInput,
                        onValueChange = { lastOilChangeInput = it },
                        label = { Text("Son Yağ Bakım Kilometresi") },
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                        singleLine = true,
                        modifier = Modifier.fillMaxWidth()
                    )

                    OutlinedTextField(
                        value = nextOilChangeInput,
                        onValueChange = { nextOilChangeInput = it },
                        label = { Text("Gelecek Yağ Bakım Kilometresi (KM)") },
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                        singleLine = true,
                        modifier = Modifier.fillMaxWidth()
                    )

                    OutlinedTextField(
                        value = lastMaintenanceDateInput,
                        onValueChange = { lastMaintenanceDateInput = it },
                        label = { Text("Son Muayene / Periyodik Bakım Tarihi") },
                        singleLine = true,
                        modifier = Modifier.fillMaxWidth()
                    )

                    OutlinedTextField(
                        value = nextMaintenanceDateInput,
                        onValueChange = { nextMaintenanceDateInput = it },
                        label = { Text("Gelecek Muayene / Bakım Tarihi (dd.MM.yyyy)") },
                        placeholder = { Text("örn. 15.11.2026") },
                        singleLine = true,
                        modifier = Modifier.fillMaxWidth()
                    )

                    OutlinedTextField(
                        value = notesInput,
                        onValueChange = { notesInput = it },
                        label = { Text("Araç Zimmet No / Özel Notlar") },
                        placeholder = { Text("Zimmetlenen satıcı veya kurye ismi...") },
                        singleLine = false,
                        maxLines = 2,
                        modifier = Modifier.fillMaxWidth()
                    )
                }
            },
            confirmButton = {
                Button(
                    onClick = {
                        val currentKmVal = currentKmInput.toIntOrNull()
                        val lastOilVal = lastOilChangeInput.toIntOrNull()
                        val nextOilVal = nextOilChangeInput.toIntOrNull()
                        
                        if (plateInput.isNotBlank() && brandModelInput.isNotBlank() && 
                            currentKmVal != null && lastOilVal != null && nextOilVal != null) {
                            
                            val nextMaintDate = if (nextMaintenanceDateInput.isBlank()) {
                                val cal = Calendar.getInstance()
                                cal.add(Calendar.YEAR, 1)
                                SimpleDateFormat("dd.MM.yyyy", Locale.getDefault()).format(cal.time)
                            } else nextMaintenanceDateInput.trim()

                            val newVehicle = Vehicle(
                                id = "VEH-" + (1000 + AppDataStore.vehicles.size),
                                plate = plateInput.trim(),
                                brandModel = brandModelInput.trim(),
                                currentKm = currentKmVal,
                                lastOilChangeKm = lastOilVal,
                                nextOilChangeKm = nextOilVal,
                                lastMaintenanceDate = lastMaintenanceDateInput.trim(),
                                nextMaintenanceDate = nextMaintDate,
                                notes = notesInput.trim(),
                                maintenanceHistoryJson = "[]"
                            )

                            AppDataStore.vehicles.add(newVehicle)
                            AppDataStore.persist(context)

                            Toast.makeText(context, "Şirket aracı başarıyla tescil edildi.", Toast.LENGTH_SHORT).show()
                            showAddVehicleDialog = false
                        } else {
                            Toast.makeText(context, "Lütfen gerekli plaka, marka ve KM bilgilerini doldurun.", Toast.LENGTH_SHORT).show()
                        }
                    }
                ) {
                    Text("Aracı Kaydet")
                }
            },
            dismissButton = {
                TextButton(onClick = { showAddVehicleDialog = false }) {
                    Text("İptal")
                }
            }
        )
    }

    // --- 6. DIALOG FOR QUICK KM ODOME UPDATE ---
    if (showUpdateKmDialog != null) {
        val targetV = showUpdateKmDialog!!
        AlertDialog(
            onDismissRequest = { showUpdateKmDialog = null },
            title = {
                Text("${targetV.plate} Kilometre Güncelle", fontWeight = FontWeight.Bold)
            },
            text = {
                Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                    Text("Mevcut Kilometre Değeri: ${targetV.currentKm} KM", style = MaterialTheme.typography.bodySmall)
                    OutlinedTextField(
                        value = quickKmInput,
                        onValueChange = { quickKmInput = it },
                        label = { Text("Yeni Kilometre Sayaç Değeri (KM)") },
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                        singleLine = true,
                        modifier = Modifier.fillMaxWidth()
                    )
                }
            },
            confirmButton = {
                Button(
                    onClick = {
                        val newKm = quickKmInput.toIntOrNull()
                        if (newKm != null && newKm >= targetV.currentKm) {
                            val idx = AppDataStore.vehicles.indexOf(targetV)
                            if (idx >= 0) {
                                AppDataStore.vehicles[idx] = targetV.copy(currentKm = newKm)
                                AppDataStore.persist(context)
                                Toast.makeText(context, "Kilometre güncellendi.", Toast.LENGTH_SHORT).show()
                            }
                            showUpdateKmDialog = null
                        } else {
                            Toast.makeText(context, "Girdiğiniz değer mevcut kilometreden az olamaz.", Toast.LENGTH_SHORT).show()
                        }
                    }
                ) {
                    Text("KM Değiştir")
                }
            },
            dismissButton = {
                TextButton(onClick = { showUpdateKmDialog = null }) {
                    Text("İptal")
                }
            }
        )
    }

    // --- 7. DIALOG FOR LOGGING MAINTENANCE AND BINDING WITH KASA ---
    if (showAddMaintenanceDialog != null) {
        val targetV = showAddMaintenanceDialog!!
        AlertDialog(
            onDismissRequest = { showAddMaintenanceDialog = null },
            title = {
                Text("${targetV.plate} Bakım / Onarım Kayıt İşlemi", fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
            },
            text = {
                Column(
                    modifier = Modifier
                        .fillMaxWidth()
                        .heightIn(max = 380.dp)
                        .verticalScroll(rememberScrollState()),
                    verticalArrangement = Arrangement.spacedBy(10.dp)
                ) {
                    OutlinedTextField(
                        value = maintCostInput,
                        onValueChange = { maintCostInput = it },
                        label = { Text("Bakım/Fatura Tutarı (₺)") },
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Decimal),
                        singleLine = true,
                        modifier = Modifier.fillMaxWidth()
                    )

                    OutlinedTextField(
                        value = maintDescInput,
                        onValueChange = { maintDescInput = it },
                        label = { Text("Yapılan İşlemler (Açıklama)") },
                        placeholder = { Text("Örn. Motor yağı değişti, hava polen filtreleri yenilendi, fren balatası takıldı.") },
                        singleLine = false,
                        maxLines = 3,
                        modifier = Modifier.fillMaxWidth()
                    )

                    OutlinedTextField(
                        value = nextOilTargetInput,
                        onValueChange = { nextOilTargetInput = it },
                        label = { Text("Yeni Gelecek Yağ Kilometre Hedefi (KM)") },
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                        singleLine = true,
                        modifier = Modifier.fillMaxWidth()
                    )

                    OutlinedTextField(
                        value = nextMaintDateTargetInput,
                        onValueChange = { nextMaintDateTargetInput = it },
                        label = { Text("Bir Sonraki Muayene/Servis Tarihi") },
                        placeholder = { Text("dd.MM.yyyy") },
                        singleLine = true,
                        modifier = Modifier.fillMaxWidth()
                    )

                    // Linked Kasa / Expense Integration Checkbox
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        verticalAlignment = Alignment.CenterVertically,
                        horizontalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        Checkbox(
                            checked = payFromKasaCheckbox,
                            onCheckedChange = { payFromKasaCheckbox = it }
                        )
                        Column {
                            Text("Kasadan Öde", fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodySmall)
                            Text("Bu onarım bedelini otomatik olarak şirket giderlerine işle ve kasadan düş.", fontSize = 10.sp, color = Color.Gray)
                        }
                    }

                    if (payFromKasaCheckbox) {
                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.spacedBy(8.dp)
                        ) {
                            Card(
                                modifier = Modifier
                                    .weight(1f)
                                    .clickable { maintKasaSelectionId = "CA-MAIN" },
                                colors = CardDefaults.cardColors(
                                    containerColor = if (maintKasaSelectionId == "CA-MAIN") MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surface
                                ),
                                border = androidx.compose.foundation.BorderStroke(
                                    width = 1.dp,
                                    color = if (maintKasaSelectionId == "CA-MAIN") MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.outlineVariant
                                )
                            ) {
                                Column(modifier = Modifier.padding(6.dp), horizontalAlignment = Alignment.CenterHorizontally) {
                                    Icon(Icons.Filled.Money, null, modifier = Modifier.size(16.dp), tint = if (maintKasaSelectionId == "CA-MAIN") MaterialTheme.colorScheme.primary else Color.Gray)
                                    Text("Merkez Kasa", fontSize = 10.sp, fontWeight = FontWeight.Bold)
                                }
                            }

                            Card(
                                modifier = Modifier
                                    .weight(1f)
                                    .clickable { maintKasaSelectionId = "CA-BANK" },
                                colors = CardDefaults.cardColors(
                                    containerColor = if (maintKasaSelectionId == "CA-BANK") MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surface
                                ),
                                border = androidx.compose.foundation.BorderStroke(
                                    width = 1.dp,
                                    color = if (maintKasaSelectionId == "CA-BANK") MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.outlineVariant
                                )
                            ) {
                                Column(modifier = Modifier.padding(6.dp), horizontalAlignment = Alignment.CenterHorizontally) {
                                    Icon(Icons.Filled.AccountBalance, null, modifier = Modifier.size(16.dp), tint = if (maintKasaSelectionId == "CA-BANK") MaterialTheme.colorScheme.primary else Color.Gray)
                                    Text("Banka Hesabı", fontSize = 10.sp, fontWeight = FontWeight.Bold)
                                }
                            }
                        }
                    }

                    // --- BAKIM / ONARIM FOTOĞRAF EKLEME ---
                    Spacer(modifier = Modifier.height(6.dp))
                    Text(
                        "Onarım / Fatura Görseli",
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

                    // Display preview if we have one
                    if (selectedMaintPhotoPath != null) {
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
                                model = selectedMaintPhotoPath,
                                contentDescription = "Onarım Fotoğrafı",
                                modifier = Modifier.fillMaxSize(),
                                contentScale = androidx.compose.ui.layout.ContentScale.Fit
                            )
                            IconButton(
                                onClick = { selectedMaintPhotoPath = null },
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
                        val costVal = maintCostInput.toDoubleOrNull()
                        val nextOilTargetVal = nextOilTargetInput.toIntOrNull()
                        
                        if (costVal != null && costVal >= 0 && nextOilTargetVal != null && maintDescInput.isNotBlank()) {
                            val idx = AppDataStore.vehicles.indexOf(targetV)
                            if (idx >= 0) {
                                val dateStr = SimpleDateFormat("dd.MM.yyyy", Locale.getDefault()).format(Date())
                                
                                // 1. Build new maintenance object
                                val list = mutableListOf<VehicleMaintenance>()
                                try {
                                    if (targetV.maintenanceHistoryJson.isNotBlank()) {
                                        val oldArr = JSONArray(targetV.maintenanceHistoryJson)
                                        for (i in 0 until oldArr.length()) {
                                            val obj = oldArr.getJSONObject(i)
                                            val pUri = if (obj.has("photoUri")) obj.getString("photoUri") else null
                                            list.add(
                                                VehicleMaintenance(
                                                    id = obj.getString("id"),
                                                    date = obj.getString("date"),
                                                    km = obj.getInt("km"),
                                                    description = obj.getString("description"),
                                                    cost = obj.getDouble("cost"),
                                                    photoUri = if (pUri == "null" || pUri.isNullOrEmpty()) null else pUri
                                                )
                                            )
                                        }
                                    }
                                } catch (e: Exception) { e.printStackTrace() }

                                val itemM = VehicleMaintenance(
                                    id = "VM-" + (2000 + list.size),
                                    date = dateStr,
                                    km = targetV.currentKm,
                                    description = maintDescInput.trim(),
                                    cost = costVal,
                                    photoUri = selectedMaintPhotoPath
                                )
                                list.add(0, itemM) // Add newest first

                                // Write array back
                                val newArr = JSONArray()
                                for (m in list) {
                                    val obj = JSONObject()
                                    obj.put("id", m.id)
                                    obj.put("date", m.date)
                                    obj.put("km", m.km)
                                    obj.put("description", m.description)
                                    obj.put("cost", m.cost)
                                    if (m.photoUri != null) {
                                        obj.put("photoUri", m.photoUri)
                                    }
                                    newArr.put(obj)
                                }

                                val nextMaintTarget = if (nextMaintDateTargetInput.isBlank()) {
                                    val cal = Calendar.getInstance()
                                    cal.add(Calendar.YEAR, 1)
                                    SimpleDateFormat("dd.MM.yyyy", Locale.getDefault()).format(cal.time)
                                } else nextMaintDateTargetInput.trim()

                                // 2. Update Vehicle fields (current KM oil changes etc.)
                                AppDataStore.vehicles[idx] = targetV.copy(
                                    lastOilChangeKm = targetV.currentKm,
                                    nextOilChangeKm = nextOilTargetVal,
                                    lastMaintenanceDate = dateStr,
                                    nextMaintenanceDate = nextMaintTarget,
                                    maintenanceHistoryJson = newArr.toString()
                                )

                                // 3. Optional linkage to safe
                                if (payFromKasaCheckbox && costVal > 0) {
                                    // Add to company expenses
                                    val expId = "EXP-" + (1000 + AppDataStore.expenses.size)
                                    val newExpVal = Expense(
                                        id = expId,
                                        date = dateStr + " " + SimpleDateFormat("HH:mm", Locale.getDefault()).format(Date()),
                                        category = "Bakım",
                                        amount = costVal,
                                        description = "(${targetV.plate}) " + maintDescInput.trim(),
                                        paymentSourceId = maintKasaSelectionId,
                                        photoUri = selectedMaintPhotoPath
                                    )
                                    AppDataStore.expenses.add(0, newExpVal)

                                    // Direct cash outflow log
                                    val logId = "K-" + (10000 + AppDataStore.kasaLogs.size)
                                    val safeLog = KasaLogItem(
                                        id = logId,
                                        date = dateStr + " " + SimpleDateFormat("HH:mm", Locale.getDefault()).format(Date()),
                                        type = "Tediye",
                                        customerOrSupplier = "Araç Bakım Gideri: " + targetV.plate,
                                        amount = costVal,
                                        paymentType = if (maintKasaSelectionId == "CA-MAIN") "Nakit" else "Banka",
                                        bankName = maintKasaSelectionId,
                                        desc = maintDescInput.trim()
                                    )
                                    AppDataStore.kasaLogs.add(safeLog)
                                }

                                AppDataStore.persist(context)
                                Toast.makeText(context, "Bakım kaydı başarıyla işlendi ve araca uygulandı.", Toast.LENGTH_SHORT).show()
                            }
                            showAddMaintenanceDialog = null
                            maintCostInput = ""
                            maintDescInput = ""
                            nextOilTargetInput = ""
                            nextMaintDateTargetInput = ""
                            selectedMaintPhotoPath = null
                        } else {
                            Toast.makeText(context, "Geçerli bir tutar, KM ve açıklama giriniz.", Toast.LENGTH_SHORT).show()
                        }
                    }
                ) {
                    Text("Kaydı Tamamla")
                }
            },
            dismissButton = {
                TextButton(onClick = { 
                    showAddMaintenanceDialog = null
                    selectedMaintPhotoPath = null
                }) {
                    Text("İptal")
                }
            }
        )
    }

    if (vehicleToDelete != null) {
        val targetV = vehicleToDelete!!
        AlertDialog(
            onDismissRequest = { vehicleToDelete = null },
            icon = { Icon(Icons.Filled.Warning, contentDescription = null, tint = MaterialTheme.colorScheme.error) },
            title = {
                Text("Aracı Sil", fontWeight = FontWeight.Bold)
            },
            text = {
                Text("${targetV.plate} plakalı ${targetV.brandModel} marka aracı sistemden kaldırmak istediğinize emin misiniz? Bu işlem geri alınamaz.")
            },
            confirmButton = {
                Button(
                    onClick = {
                        AppDataStore.vehicles.remove(targetV)
                        AppDataStore.persist(context)
                        Toast.makeText(context, "Araç sistemden kaldırıldı.", Toast.LENGTH_SHORT).show()
                        vehicleToDelete = null
                    },
                    colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.error)
                ) {
                    Text("Evet, Sil")
                }
            },
            dismissButton = {
                TextButton(onClick = { vehicleToDelete = null }) {
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
                        Text("Bakım / Onarım Belgesi", fontWeight = FontWeight.Bold, style = MaterialTheme.typography.titleMedium)
                        IconButton(onClick = { fullScreenPhotoPath = null }) {
                            Icon(Icons.Filled.Close, null)
                        }
                    }
                    coil.compose.AsyncImage(
                        model = fullScreenPhotoPath,
                        contentDescription = "Büyük Bakım Görseli",
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

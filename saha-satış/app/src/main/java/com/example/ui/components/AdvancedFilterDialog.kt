package com.example.ui.components

import androidx.compose.foundation.border
import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Close
import androidx.compose.material.icons.filled.Search
import androidx.compose.material.icons.filled.KeyboardArrowDown
import androidx.compose.material.icons.filled.KeyboardArrowUp
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.compose.ui.window.Dialog

@OptIn(ExperimentalMaterial3Api::class, ExperimentalLayoutApi::class)
@Composable
fun AdvancedFilterDialog(
    showDialog: Boolean,
    onDismiss: () -> Unit,
    brands: List<String>,
    categories: List<String>,
    ambalajs: List<String>,
    selectedBrands: Set<String>,
    onBrandsChange: (Set<String>) -> Unit,
    selectedCategories: Set<String>,
    onCategoriesChange: (Set<String>) -> Unit,
    selectedAmbalajs: Set<String>,
    onAmbalajsChange: (Set<String>) -> Unit,
    minPrice: String,
    onMinPriceChange: (String) -> Unit,
    maxPrice: String,
    onMaxPriceChange: (String) -> Unit,
    minStock: String,
    onMinStockChange: (String) -> Unit,
    maxStock: String,
    onMaxStockChange: (String) -> Unit,
    hideNoPhoto: Boolean,
    onHideNoPhotoChange: (Boolean) -> Unit,
    hideOutOfStock: Boolean,
    onHideOutOfStockChange: (Boolean) -> Unit,
    onReset: () -> Unit
) {
    if (showDialog) {
        Dialog(onDismissRequest = onDismiss) {
            Surface(
                modifier = Modifier
                    .fillMaxWidth()
                    .fillMaxHeight(0.9f),
                shape = RoundedCornerShape(24.dp),
                color = MaterialTheme.colorScheme.surface,
                tonalElevation = 6.dp
            ) {
                Column(
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(20.dp)
                ) {
                    // Header
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.SpaceBetween,
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Text(
                            text = "Gelişmiş Filtreler",
                            style = MaterialTheme.typography.titleLarge,
                            fontWeight = FontWeight.Bold,
                            color = MaterialTheme.colorScheme.primary
                        )
                        IconButton(onClick = onDismiss) {
                            Icon(Icons.Filled.Close, contentDescription = "Kapat")
                        }
                    }
                    
                    HorizontalDivider(modifier = Modifier.padding(vertical = 12.dp))
                    
                    // Scrollable content
                    Column(
                        modifier = Modifier
                            .weight(1f)
                            .verticalScroll(rememberScrollState()),
                        verticalArrangement = Arrangement.spacedBy(16.dp)
                    ) {
                        // Kategori (Multi-select with Search Dropdown)
                        MultiSelectSearchDropdown(
                            label = "Kategori",
                            allOptions = categories,
                            selectedOptions = selectedCategories,
                            onSelectionChange = onCategoriesChange,
                            placeholderText = "Kategori Seç..."
                        )

                        // Marka (Multi-select with Search Dropdown)
                        MultiSelectSearchDropdown(
                            label = "Marka",
                            allOptions = brands,
                            selectedOptions = selectedBrands,
                            onSelectionChange = onBrandsChange,
                            placeholderText = "Marka Seç..."
                        )

                        // Ambalaj (Multi-select)
                        Text(text = "Ambalaj / Paketleme", style = MaterialTheme.typography.titleSmall, fontWeight = FontWeight.SemiBold)
                        FlowRow(
                            horizontalArrangement = Arrangement.spacedBy(8.dp),
                            verticalArrangement = Arrangement.spacedBy(8.dp),
                            modifier = Modifier.fillMaxWidth()
                        ) {
                            ambalajs.forEach { amb ->
                                val isSelected = selectedAmbalajs.contains(amb)
                                FilterChip(
                                    selected = isSelected,
                                    onClick = {
                                        if (isSelected) {
                                            onAmbalajsChange(selectedAmbalajs - amb)
                                        } else {
                                            onAmbalajsChange(selectedAmbalajs + amb)
                                        }
                                    },
                                    label = { Text(amb, style = MaterialTheme.typography.bodySmall) }
                                )
                            }
                        }

                        // Fiyat Aralığı
                        Text(text = "Fiyat Aralığı (₺)", style = MaterialTheme.typography.titleSmall, fontWeight = FontWeight.SemiBold)
                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.spacedBy(12.dp)
                        ) {
                            OutlinedTextField(
                                value = minPrice,
                                onValueChange = onMinPriceChange,
                                placeholder = { Text("Min", style = MaterialTheme.typography.bodySmall) },
                                modifier = Modifier.weight(1f).height(52.dp),
                                textStyle = MaterialTheme.typography.bodySmall,
                                singleLine = true,
                                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Decimal),
                                shape = RoundedCornerShape(10.dp)
                            )
                            OutlinedTextField(
                                value = maxPrice,
                                onValueChange = onMaxPriceChange,
                                placeholder = { Text("Max", style = MaterialTheme.typography.bodySmall) },
                                modifier = Modifier.weight(1f).height(52.dp),
                                textStyle = MaterialTheme.typography.bodySmall,
                                singleLine = true,
                                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Decimal),
                                shape = RoundedCornerShape(10.dp)
                            )
                        }

                        // Stok Aralığı
                        Text(text = "Stok Aralığı", style = MaterialTheme.typography.titleSmall, fontWeight = FontWeight.SemiBold)
                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.spacedBy(12.dp)
                        ) {
                            OutlinedTextField(
                                value = minStock,
                                onValueChange = onMinStockChange,
                                placeholder = { Text("Min", style = MaterialTheme.typography.bodySmall) },
                                modifier = Modifier.weight(1f).height(52.dp),
                                textStyle = MaterialTheme.typography.bodySmall,
                                singleLine = true,
                                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                                shape = RoundedCornerShape(10.dp)
                            )
                            OutlinedTextField(
                                value = maxStock,
                                onValueChange = onMaxStockChange,
                                placeholder = { Text("Max", style = MaterialTheme.typography.bodySmall) },
                                modifier = Modifier.weight(1f).height(52.dp),
                                textStyle = MaterialTheme.typography.bodySmall,
                                singleLine = true,
                                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                                shape = RoundedCornerShape(10.dp)
                            )
                        }

                        // Switches section
                        HorizontalDivider(modifier = Modifier.padding(vertical = 8.dp))
                        
                        Row(
                            modifier = Modifier.fillMaxWidth().clickable { onHideNoPhotoChange(!hideNoPhoto) }.padding(vertical = 4.dp),
                            horizontalArrangement = Arrangement.SpaceBetween,
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Text("Fotoğrafı olmayan ürünleri gizle", style = MaterialTheme.typography.bodyMedium)
                            Switch(checked = hideNoPhoto, onCheckedChange = onHideNoPhotoChange)
                        }

                        Row(
                            modifier = Modifier.fillMaxWidth().clickable { onHideOutOfStockChange(!hideOutOfStock) }.padding(vertical = 4.dp),
                            horizontalArrangement = Arrangement.SpaceBetween,
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Text("Stokta olmayan ürünleri gizle", style = MaterialTheme.typography.bodyMedium)
                            Switch(checked = hideOutOfStock, onCheckedChange = onHideOutOfStockChange)
                        }
                    }

                    HorizontalDivider(modifier = Modifier.padding(vertical = 12.dp))

                    // Buttons
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.spacedBy(12.dp)
                    ) {
                        OutlinedButton(
                            onClick = {
                                onReset()
                                onDismiss()
                            },
                            modifier = Modifier.weight(1f),
                            shape = RoundedCornerShape(12.dp)
                        ) {
                            Text("Sıfırla")
                        }
                        Button(
                            onClick = onDismiss,
                            modifier = Modifier.weight(1f),
                            shape = RoundedCornerShape(12.dp)
                        ) {
                            Text("Filtreleri Uygula")
                        }
                    }
                }
            }
        }
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun MultiSelectSearchDropdown(
    label: String,
    allOptions: List<String>,
    selectedOptions: Set<String>,
    onSelectionChange: (Set<String>) -> Unit,
    placeholderText: String
) {
    var expanded by remember { mutableStateOf(false) }
    var searchVal by remember { mutableStateOf("") }
    
    Column(modifier = Modifier.fillMaxWidth()) {
        Text(text = label, style = MaterialTheme.typography.titleSmall, fontWeight = FontWeight.SemiBold)
        Spacer(modifier = Modifier.height(6.dp))
        
        val selectedText = if (selectedOptions.isEmpty()) {
            "Hepsi"
        } else {
            selectedOptions.joinToString(", ")
        }
        
        OutlinedCard(
            onClick = { expanded = !expanded },
            modifier = Modifier.fillMaxWidth(),
            shape = RoundedCornerShape(10.dp),
            colors = CardDefaults.outlinedCardColors(
                containerColor = MaterialTheme.colorScheme.surface
            )
        ) {
            Row(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(horizontal = 14.dp, vertical = 12.dp),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                Text(
                    text = selectedText,
                    style = MaterialTheme.typography.bodyMedium,
                    color = if (selectedOptions.isEmpty()) MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.7f) else MaterialTheme.colorScheme.onSurface,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis,
                    modifier = Modifier.weight(1f)
                )
                Icon(
                    imageVector = if (expanded) Icons.Filled.KeyboardArrowUp else Icons.Filled.KeyboardArrowDown,
                    contentDescription = null,
                    modifier = Modifier.size(20.dp),
                    tint = MaterialTheme.colorScheme.primary
                )
            }
        }
        
        if (expanded) {
            Spacer(modifier = Modifier.height(4.dp))
            Card(
                modifier = Modifier
                    .fillMaxWidth()
                    .border(1.dp, MaterialTheme.colorScheme.primary.copy(alpha = 0.3f), RoundedCornerShape(10.dp)),
                shape = RoundedCornerShape(10.dp),
                colors = CardDefaults.cardColors(
                    containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.15f)
                )
            ) {
                Column(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(8.dp)
                ) {
                    // Search Input
                    OutlinedTextField(
                        value = searchVal,
                        onValueChange = { searchVal = it },
                        placeholder = { Text("Arama...", fontSize = 11.sp) },
                        modifier = Modifier
                            .fillMaxWidth()
                            .height(48.dp),
                        textStyle = MaterialTheme.typography.bodySmall.copy(fontSize = 11.sp),
                        singleLine = true,
                        leadingIcon = { Icon(Icons.Filled.Search, null, modifier = Modifier.size(14.dp)) },
                        trailingIcon = {
                            if (searchVal.isNotEmpty()) {
                                IconButton(onClick = { searchVal = "" }) {
                                    Icon(Icons.Filled.Close, null, modifier = Modifier.size(14.dp))
                                }
                            }
                        },
                        shape = RoundedCornerShape(8.dp),
                        colors = OutlinedTextFieldDefaults.colors(
                            unfocusedContainerColor = MaterialTheme.colorScheme.surface,
                            focusedContainerColor = MaterialTheme.colorScheme.surface,
                            unfocusedBorderColor = MaterialTheme.colorScheme.outline.copy(alpha = 0.4f),
                            focusedBorderColor = MaterialTheme.colorScheme.primary
                        )
                    )
                    
                    Spacer(modifier = Modifier.height(8.dp))
                    
                    val filteredOptions = allOptions.filter { it.contains(searchVal, ignoreCase = true) }
                    
                    if (filteredOptions.isEmpty()) {
                        Box(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(12.dp),
                            contentAlignment = Alignment.Center
                        ) {
                            Text("Sonucu bulunamadı", style = MaterialTheme.typography.bodySmall, color = Color.Gray)
                        }
                    } else {
                        Column(
                            modifier = Modifier
                                .fillMaxWidth()
                                .heightIn(max = 200.dp)
                                .verticalScroll(rememberScrollState()),
                            verticalArrangement = Arrangement.spacedBy(2.dp)
                        ) {
                            filteredOptions.forEach { option ->
                                val isChecked = selectedOptions.contains(option)
                                java.lang.Boolean.valueOf(isChecked) // force a clean read just in case
                                Row(
                                    verticalAlignment = Alignment.CenterVertically,
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .clickable {
                                            if (isChecked) {
                                                onSelectionChange(selectedOptions - option)
                                            } else {
                                                onSelectionChange(selectedOptions + option)
                                            }
                                        }
                                        .padding(vertical = 4.dp, horizontal = 4.dp),
                                    horizontalArrangement = Arrangement.spacedBy(8.dp)
                                ) {
                                    Checkbox(
                                        checked = isChecked,
                                        onCheckedChange = { checked ->
                                            if (checked == true) {
                                                onSelectionChange(selectedOptions + option)
                                            } else {
                                                onSelectionChange(selectedOptions - option)
                                            }
                                        },
                                        modifier = Modifier.size(20.dp)
                                    )
                                    Text(
                                        text = option,
                                        style = MaterialTheme.typography.bodyMedium,
                                        color = MaterialTheme.colorScheme.onSurface,
                                        fontSize = 13.sp
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

import re

with open('app/src/main/java/com/example/ui/screens/CatalogScreen.kt', 'r') as f:
    content = f.read()

# Replace val filteredCollection = AppDataStore.products.filter { ... }
# With derived state.

search_pattern = r"(val filteredCollection = AppDataStore\.products\.filter \{.*?\}\.sortedWith\(.*?\))"

derived_code = """val filteredCollection by remember(selectedCategoryFilter, selectedPriceTier) {
                            derivedStateOf {
                                AppDataStore.products.filter { prod ->
                                    val matchesSearch = catalogSearchQuery.isBlank() ||
                                            prod.title.contains(catalogSearchQuery, ignoreCase = true) ||
                                            prod.code.contains(catalogSearchQuery, ignoreCase = true) ||
                                            prod.barcode.contains(catalogSearchQuery, ignoreCase = true) ||
                                            prod.barcodes.any { it.contains(catalogSearchQuery, ignoreCase = true) }
                                    
                                    val matchesCategory = if (AppDataStore.catalogFilterCategories.value.isNotEmpty()) {
                                        AppDataStore.catalogFilterCategories.value.contains(prod.category)
                                    } else {
                                        selectedCategoryFilter == "Tümü" || prod.category == selectedCategoryFilter
                                    }
                                    
                                    val matchesBrand = AppDataStore.catalogFilterBrands.value.isEmpty() ||
                                                        AppDataStore.catalogFilterBrands.value.contains(prod.brand ?: "Belirtilmemiş")
                                    
                                    val matchesAmbalaj = AppDataStore.catalogFilterAmbalajs.value.isEmpty() ||
                                                          AppDataStore.catalogFilterAmbalajs.value.contains(prod.inferredAmbalaj)
                                    
                                    val price = prod.getPriceForGroup(selectedPriceTier)
                                    val minPriceVal = AppDataStore.catalogFilterMinPrice.value.toDoubleOrNull()
                                    val maxPriceVal = AppDataStore.catalogFilterMaxPrice.value.toDoubleOrNull()
                                    val matchesPrice = (minPriceVal == null || price >= minPriceVal) &&
                                                        (maxPriceVal == null || price <= maxPriceVal)
                                    
                                    val totalStock = prod.stockByWarehouse.values.sum()
                                    val minStockVal = AppDataStore.catalogFilterMinStock.value.toIntOrNull()
                                    val maxStockVal = AppDataStore.catalogFilterMaxStock.value.toIntOrNull()
                                    val matchesStock = (minStockVal == null || totalStock >= minStockVal) &&
                                                        (maxStockVal == null || totalStock <= maxStockVal)
                                    
                                    val hasPhoto = !prod.imageUrl.isNullOrEmpty() || !prod.localImagePath.isNullOrEmpty()
                                    val matchesNoPhoto = !AppDataStore.catalogFilterHideNoPhoto.value || hasPhoto
                                    val matchesOutOfStock = !AppDataStore.catalogFilterHideOutOfStock.value || totalStock > 0
                                    
                                    matchesSearch && matchesCategory && matchesBrand && matchesAmbalaj && matchesPrice && matchesStock && matchesNoPhoto && matchesOutOfStock
                                }.sortedWith(
                                    run {
                                        val comparator = when (AppDataStore.catalogSelectedSortField.value) {
                                            "İsim" -> compareBy<ProductCatalog> { it.title }
                                            "Kod" -> compareBy { it.code }
                                            "Fiyat" -> compareBy { it.getPriceForGroup(selectedPriceTier) }
                                            "Marka" -> compareBy { it.brand ?: "" }
                                            "Stok" -> compareBy { it.stockByWarehouse.values.sum() }
                                            else -> compareBy { it.title }
                                        }
                                        if (AppDataStore.catalogSelectedSortAsc.value) comparator else comparator.reversed()
                                    }
                                )
                            }
                        }"""

content = re.sub(search_pattern, derived_code, content, flags=re.DOTALL)

# Add keys to LazyColumn items
content = content.replace("items(displayedProducts) { prod ->", "items(items = displayedProducts, key = { it.barcode }) { prod ->")

with open('app/src/main/java/com/example/ui/screens/CatalogScreen.kt', 'w') as f:
    f.write(content)

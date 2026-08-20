import re

with open('app/src/main/java/com/example/ui/screens/SalesScreen.kt', 'r') as f:
    content = f.read()

# Add derivedFilteredProducts at line 102
# Just after `var salesProductVisibleCount ... { mutableStateOf(100) }`

derived_code = """
    val derivedFilteredProducts by remember(selectedCategory) {
        derivedStateOf {
            val rawProducts = AppDataStore.products
            rawProducts.filter { p ->
                val matchesQuery = searchQuery.isEmpty() ||
                                   p.title.contains(searchQuery, ignoreCase = true) ||
                                    p.code.contains(searchQuery, ignoreCase = true) ||
                                    p.barcode.contains(searchQuery, ignoreCase = true) ||
                                   p.barcodes.any { it.contains(searchQuery, ignoreCase = true) }
                
                val matchesCat = if (AppDataStore.salesFilterCategories.value.isNotEmpty()) {
                    AppDataStore.salesFilterCategories.value.contains(p.category)
                } else {
                    selectedCategory == "Tümü" || p.category == selectedCategory
                }
                
                val matchesBrand = AppDataStore.salesFilterBrands.value.isEmpty() ||
                                    AppDataStore.salesFilterBrands.value.contains(p.brand ?: "Belirtilmemiş")
                
                val matchesAmbalaj = AppDataStore.salesFilterAmbalajs.value.isEmpty() ||
                                      AppDataStore.salesFilterAmbalajs.value.contains(p.inferredAmbalaj)
                
                val price = getProductPrice(p)
                val minPriceVal = AppDataStore.salesFilterMinPrice.value.toDoubleOrNull()
                val maxPriceVal = AppDataStore.salesFilterMaxPrice.value.toDoubleOrNull()
                val matchesPrice = (minPriceVal == null || price >= minPriceVal) &&
                                    (maxPriceVal == null || price <= maxPriceVal)
                
                val totalStock = p.stockByWarehouse.values.sum()
                val minStockVal = AppDataStore.salesFilterMinStock.value.toIntOrNull()
                val maxStockVal = AppDataStore.salesFilterMaxStock.value.toIntOrNull()
                val matchesStock = (minStockVal == null || totalStock >= minStockVal) &&
                                    (maxStockVal == null || totalStock <= maxStockVal)
                
                val hasPhoto = !p.imageUrl.isNullOrEmpty() || !p.localImagePath.isNullOrEmpty()
                val matchesNoPhoto = !AppDataStore.salesFilterHideNoPhoto.value || hasPhoto
                val matchesOutOfStock = !AppDataStore.salesFilterHideOutOfStock.value || totalStock > 0
                
                matchesQuery && matchesCat && matchesBrand && matchesAmbalaj && matchesPrice && matchesStock && matchesNoPhoto && matchesOutOfStock
            }.sortedWith(
                run {
                    val comparator = when (AppDataStore.salesSelectedSortField.value) {
                        "İsim" -> compareBy<ProductCatalog> { it.title }
                        "Kod" -> compareBy { it.code }
                        "Fiyat" -> compareBy { getProductPrice(it) }
                        "Marka" -> compareBy { it.brand ?: "" }
                        "Stok" -> compareBy { it.stockByWarehouse.values.sum() }
                        else -> compareBy { it.title }
                    }
                    if (AppDataStore.salesSelectedSortAsc.value) comparator else comparator.reversed()
                }
            )
        }
    }
"""

content = content.replace("    } { mutableStateOf(100) }", "    } { mutableStateOf(100) }\n" + derived_code)

# Now remove the old logic inside LazyColumn
old_logic = """                    // Dynamic Products Display Loop
                    val rawProducts = AppDataStore.products
                    val filteredProducts = rawProducts.filter { p ->
                        val matchesQuery = searchQuery.isEmpty() ||
                                           p.title.contains(searchQuery, ignoreCase = true) ||
                                            p.code.contains(searchQuery, ignoreCase = true) ||
                                            p.barcode.contains(searchQuery, ignoreCase = true) ||
                                           p.barcodes.any { it.contains(searchQuery, ignoreCase = true) }
                        
                        val matchesCat = if (AppDataStore.salesFilterCategories.value.isNotEmpty()) {
                            AppDataStore.salesFilterCategories.value.contains(p.category)
                        } else {
                            selectedCategory == "Tümü" || p.category == selectedCategory
                        }
                        
                        val matchesBrand = AppDataStore.salesFilterBrands.value.isEmpty() ||
                                            AppDataStore.salesFilterBrands.value.contains(p.brand ?: "Belirtilmemiş")
                        
                        val matchesAmbalaj = AppDataStore.salesFilterAmbalajs.value.isEmpty() ||
                                              AppDataStore.salesFilterAmbalajs.value.contains(p.inferredAmbalaj)
                        
                        val price = getProductPrice(p)
                        val minPriceVal = AppDataStore.salesFilterMinPrice.value.toDoubleOrNull()
                        val maxPriceVal = AppDataStore.salesFilterMaxPrice.value.toDoubleOrNull()
                        val matchesPrice = (minPriceVal == null || price >= minPriceVal) &&
                                            (maxPriceVal == null || price <= maxPriceVal)
                        
                        val totalStock = p.stockByWarehouse.values.sum()
                        val minStockVal = AppDataStore.salesFilterMinStock.value.toIntOrNull()
                        val maxStockVal = AppDataStore.salesFilterMaxStock.value.toIntOrNull()
                        val matchesStock = (minStockVal == null || totalStock >= minStockVal) &&
                                            (maxStockVal == null || totalStock <= maxStockVal)
                        
                        val hasPhoto = !p.imageUrl.isNullOrEmpty() || !p.localImagePath.isNullOrEmpty()
                        val matchesNoPhoto = !AppDataStore.salesFilterHideNoPhoto.value || hasPhoto
                        val matchesOutOfStock = !AppDataStore.salesFilterHideOutOfStock.value || totalStock > 0
                        
                        matchesQuery && matchesCat && matchesBrand && matchesAmbalaj && matchesPrice && matchesStock && matchesNoPhoto && matchesOutOfStock
                    }.sortedWith(
                        run {
                            val comparator = when (AppDataStore.salesSelectedSortField.value) {
                                "İsim" -> compareBy<ProductCatalog> { it.title }
                                "Kod" -> compareBy { it.code }
                                "Fiyat" -> compareBy { getProductPrice(it) }
                                "Marka" -> compareBy { it.brand ?: "" }
                                "Stok" -> compareBy { it.stockByWarehouse.values.sum() }
                                else -> compareBy { it.title }
                            }
                            if (AppDataStore.salesSelectedSortAsc.value) comparator else comparator.reversed()
                        }
                    )"""
                    
content = content.replace(old_logic, "                    // Using derived state for performance\n                    val filteredProducts = derivedFilteredProducts")

# Add key to itemsIndexed
# itemsIndexed(displayedProducts) { index, prod ->
content = content.replace("itemsIndexed(displayedProducts) { index, prod ->", "itemsIndexed(items = displayedProducts, key = { index, prod -> prod.barcode + \"_\" + index }) { index, prod ->")

# Also fix the Customers list items(filteredCustomers)
# There is a nested lazycolumn somewhere? Let's fix that.
# items(filteredCustomers) { cust -> -> key = { it.id }
content = content.replace("items(filteredCustomers) { cust ->", "items(items = filteredCustomers, key = { cust -> cust.id }) { cust ->")

with open('app/src/main/java/com/example/ui/screens/SalesScreen.kt', 'w') as f:
    f.write(content)


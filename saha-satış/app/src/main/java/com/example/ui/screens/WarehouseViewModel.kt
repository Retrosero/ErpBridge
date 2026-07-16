package com.example.ui.screens

import android.content.Context
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.data.database.AppDatabase
import com.example.data.database.DatabaseProvider
import com.example.data.database.WmsOrderEntity
import com.example.data.database.WmsOrderItemEntity
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext

class WarehouseViewModel(context: Context) : ViewModel() {
    private val db: AppDatabase = DatabaseProvider.getDatabase(context.applicationContext)

    private val _orders = MutableStateFlow<List<WmsOrderEntity>>(emptyList())
    val orders: StateFlow<List<WmsOrderEntity>> = _orders.asStateFlow()

    private val _activeOrder = MutableStateFlow<WmsOrderEntity?>(null)
    val activeOrder: StateFlow<WmsOrderEntity?> = _activeOrder.asStateFlow()

    private val _activeOrderItems = MutableStateFlow<List<WmsOrderItemEntity>>(emptyList())
    val activeOrderItems: StateFlow<List<WmsOrderItemEntity>> = _activeOrderItems.asStateFlow()

    private val _isSyncing = MutableStateFlow(false)
    val isSyncing: StateFlow<Boolean> = _isSyncing.asStateFlow()

    private val _syncStatusText = MutableStateFlow("")
    val syncStatusText: StateFlow<String> = _syncStatusText.asStateFlow()

    private val _scanFeedback = MutableStateFlow<String?>(null)
    val scanFeedback: StateFlow<String?> = _scanFeedback.asStateFlow()

    init {
        loadOrders()
    }

    fun loadOrders() {
        viewModelScope.launch {
            val hasMock = withContext(Dispatchers.IO) {
                db.wmsOrderDao().getOrderById("SM-2026-001") != null
            }
            if (!hasMock) {
                seedMockOrders()
            } else {
                val allOrders = withContext(Dispatchers.IO) {
                    db.wmsOrderDao().getAllOrders()
                }
                _orders.value = allOrders
            }
            ensureWmsCustomersExistInStore()
        }
    }

    private fun ensureWmsCustomersExistInStore() {
        val wmsCustomersToSeed = listOf(
            Pair("Ege Toptan Yapı Malzemeleri", "CUS-WMS01"),
            Pair("İstanbul Boya Dünyası Sanayi", "CUS-WMS02"),
            Pair("Başkent İnşaat Market", "CUS-WMS03"),
            Pair("Akdeniz Kimya Dış Ticaret", "CUS-WMS04")
        )
        
        for ((name, id) in wmsCustomersToSeed) {
            val exists = AppDataStore.customers.any { it.name.trim().lowercase() == name.trim().lowercase() }
            if (!exists) {
                AppDataStore.customers.add(
                    Customer(
                        id = id,
                        name = name,
                        balance = 0.0,
                        lastVisit = "Yeni Kayıt",
                        contact = "WMS Yetkilisi",
                        phone = "+90 (555) 111 22 33",
                        address = "Depo Sevkiyat Bölgesi Müşterisi",
                        taxOffice = "Vergi Dairesi Merkez",
                        taxNumber = "1234567890",
                        gpsLocation = "41.0000° N, 29.0000° E",
                        riskLimit = 100000.0,
                        priceGroup = "Bayi-1 Klasmanı",
                        specialDiscountPercent = 5.0,
                        transactions = androidx.compose.runtime.mutableStateListOf()
                    )
                )
            }
        }
    }

    private suspend fun seedMockOrders() = withContext(Dispatchers.IO) {
        val mockOrders = listOf(
            WmsOrderEntity(
                id = "SM-2026-001",
                customerName = "Ege Toptan Yapı Malzemeleri",
                orderDate = "16.06.2026",
                status = "Bekleyen",
                totalItems = 15,
                syncStatus = "SYNCED"
            ),
            WmsOrderEntity(
                id = "SM-2026-002",
                customerName = "İstanbul Boya Dünyası Sanayi",
                orderDate = "15.06.2026",
                status = "Toplanıyor",
                totalItems = 26,
                syncStatus = "SYNCED"
            ),
            WmsOrderEntity(
                id = "SM-2026-003",
                customerName = "Başkent İnşaat Market",
                orderDate = "14.06.2026",
                status = "Toplandı",
                totalItems = 8,
                packageBarcode = "PKG-0921021",
                syncStatus = "SYNCED"
            ),
            WmsOrderEntity(
                id = "SM-2026-004",
                customerName = "Akdeniz Kimya Dış Ticaret",
                orderDate = "14.06.2026",
                status = "Sevk Edildi",
                totalItems = 4,
                packageBarcode = "PKG-0824043",
                vehiclePlate = "34 ABC 123",
                syncStatus = "SYNCED"
            )
        )

        val mockItems = listOf(
            // Order 1 items
            WmsOrderItemEntity(
                id = "SM-2026-001_8690123456789",
                orderId = "SM-2026-001",
                productBarcode = "8690123456789",
                productTitle = "Ultra Performans Endüstriyel Motor Yağı 20L",
                quantityOrdered = 5,
                quantityPicked = 0,
                isPicked = false,
                shelfLocation = "Raf A-1"
            ),
            WmsOrderItemEntity(
                id = "SM-2026-001_8699876543210",
                orderId = "SM-2026-001",
                productBarcode = "8699876543210",
                productTitle = "Hava Filtresi - Ağır Vasıta Uyumlu Pro",
                quantityOrdered = 10,
                quantityPicked = 0,
                isPicked = false,
                shelfLocation = "Raf B-3"
            ),

            // Order 2 items
            WmsOrderItemEntity(
                id = "SM-2026-002_1234567890123",
                orderId = "SM-2026-002",
                productBarcode = "1234567890123",
                productTitle = "Çelik Rulman 120mm - Yüksek Devir",
                quantityOrdered = 6,
                quantityPicked = 4,
                isPicked = false,
                shelfLocation = "Raf A-4"
            ),
            WmsOrderItemEntity(
                id = "SM-2026-002_8681122334455",
                orderId = "SM-2026-002",
                productBarcode = "8681122334455",
                productTitle = "Çelik Civata Takımı M8 x 40mm (100 ADET)",
                quantityOrdered = 20,
                quantityPicked = 12,
                isPicked = false,
                shelfLocation = "Raf C-1"
            ),

            // Order 3 items
            WmsOrderItemEntity(
                id = "SM-2026-003_8690123456789",
                orderId = "SM-2026-003",
                productBarcode = "8690123456789",
                productTitle = "Ultra Performans Endüstriyel Motor Yağı 20L",
                quantityOrdered = 8,
                quantityPicked = 8,
                isPicked = true,
                shelfLocation = "Raf A-1"
            ),

            // Order 4 items
            WmsOrderItemEntity(
                id = "SM-2026-004_8699876543210",
                orderId = "SM-2026-004",
                productBarcode = "8699876543210",
                productTitle = "Hava Filtresi - Ağır Vasıta Uyumlu Pro",
                quantityOrdered = 4,
                quantityPicked = 4,
                isPicked = true,
                shelfLocation = "Raf B-3"
            )
        )

        db.wmsOrderDao().insertAll(mockOrders)
        db.wmsOrderItemDao().insertAll(mockItems)

        val freshlyLoaded = db.wmsOrderDao().getAllOrders()
        _orders.value = freshlyLoaded
    }

    fun selectOrder(order: WmsOrderEntity) {
        viewModelScope.launch {
            _activeOrder.value = order
            val items = withContext(Dispatchers.IO) {
                db.wmsOrderItemDao().getItemsForOrder(order.id)
            }
            _activeOrderItems.value = items
        }
    }

    fun clearActiveOrder() {
        _activeOrder.value = null
        _activeOrderItems.value = emptyList()
    }

    fun onBarcodeScanned(barcode: String): Boolean {
        val currentOrder = _activeOrder.value ?: return false
        val currentItems = _activeOrderItems.value
        
        // Find matching item by productBarcode
        val matchedItem = currentItems.find { it.productBarcode == barcode }
        if (matchedItem == null) {
            _scanFeedback.value = "Hata: Barkod ($barcode) bu siparişte bulunmuyor!"
            clearFeedbackAfterDelay()
            return false
        }

        if (matchedItem.isPicked) {
            _scanFeedback.value = "Bilgi: ${matchedItem.productTitle} zaten toplandı!"
            clearFeedbackAfterDelay()
            return true
        }

        // Good scan! Let's pick 1 item or complete the pick
        val newPicked = matchedItem.quantityPicked + 1
        val fullyPicked = newPicked >= matchedItem.quantityOrdered
        
        viewModelScope.launch {
            withContext(Dispatchers.IO) {
                db.wmsOrderItemDao().updateItemPickedStatus(
                    itemId = matchedItem.id,
                    picked = newPicked,
                    isPicked = fullyPicked
                )
                // If it was "Bekleyen", let's move the order to "Toplanıyor"
                if (currentOrder.status == "Bekleyen") {
                    db.wmsOrderDao().updateOrderStatus(currentOrder.id, "Toplanıyor")
                }
            }
            
            // Reload and refresh state
            refreshActiveOrder()
            _scanFeedback.value = "Başarılı: 1 adet ${matchedItem.productTitle} toplandı! ($newPicked/${matchedItem.quantityOrdered})"
            clearFeedbackAfterDelay()
        }
        return true
    }

    private fun clearFeedbackAfterDelay() {
        viewModelScope.launch {
            delay(3500)
            _scanFeedback.value = null
        }
    }

    fun pickItemManually(item: WmsOrderItemEntity) {
        val currentOrder = _activeOrder.value ?: return
        val newPicked = item.quantityOrdered // Mark full quantity for easy bulk glove-use picking
        val fullyPicked = true

        viewModelScope.launch {
            withContext(Dispatchers.IO) {
                db.wmsOrderItemDao().updateItemPickedStatus(
                    itemId = item.id,
                    picked = newPicked,
                    isPicked = fullyPicked
                )
                if (currentOrder.status == "Bekleyen") {
                    db.wmsOrderDao().updateOrderStatus(currentOrder.id, "Toplanıyor")
                }
            }
            refreshActiveOrder()
            _scanFeedback.value = "Başarılı: ${item.productTitle} tamamı toplandı!"
            clearFeedbackAfterDelay()
        }
    }

    fun resetItemPicked(item: WmsOrderItemEntity) {
        viewModelScope.launch {
            withContext(Dispatchers.IO) {
                db.wmsOrderItemDao().updateItemPickedStatus(
                    itemId = item.id,
                    picked = 0,
                    isPicked = false
                )
            }
            refreshActiveOrder()
        }
    }

    fun updateItemQuantity(item: WmsOrderItemEntity, increment: Boolean) {
        val currentOrder = _activeOrder.value ?: return
        val currentPicked = item.quantityPicked
        val newPicked = if (increment) {
            (currentPicked + 1).coerceAtMost(999)
        } else {
            (currentPicked - 1).coerceAtLeast(0)
        }
        val fullyPicked = newPicked > 0 // Mark as picked if at least some are picked

        viewModelScope.launch {
            withContext(Dispatchers.IO) {
                db.wmsOrderItemDao().updateItemPickedStatus(
                    itemId = item.id,
                    picked = newPicked,
                    isPicked = fullyPicked
                )
                if (currentOrder.status == "Bekleyen" && newPicked > 0) {
                    db.wmsOrderDao().updateOrderStatus(currentOrder.id, "Toplanıyor")
                }
            }
            refreshActiveOrder()
        }
    }

    fun setItemQuantity(item: WmsOrderItemEntity, quantity: Int) {
        val currentOrder = _activeOrder.value ?: return
        val newPicked = quantity.coerceIn(0, 999)
        val fullyPicked = newPicked > 0 // Mark as picked if at least some are picked

        viewModelScope.launch {
            withContext(Dispatchers.IO) {
                db.wmsOrderItemDao().updateItemPickedStatus(
                    itemId = item.id,
                    picked = newPicked,
                    isPicked = fullyPicked
                )
                if (currentOrder.status == "Bekleyen" && newPicked > 0) {
                    db.wmsOrderDao().updateOrderStatus(currentOrder.id, "Toplanıyor")
                }
            }
            refreshActiveOrder()
        }
    }

    fun finishPickingAndSendToControl() {
        val currentOrder = _activeOrder.value ?: return
        viewModelScope.launch {
            withContext(Dispatchers.IO) {
                db.wmsOrderDao().updateOrderStatus(currentOrder.id, "Toplandı")
            }
            refreshActiveOrder()
            _scanFeedback.value = "Toplama tamamlandı! Kontrol listesine aktarıldı."
            clearFeedbackAfterDelay()
        }
    }

    private suspend fun refreshActiveOrder() {
        val orderId = _activeOrder.value?.id ?: return
        val updatedItems = withContext(Dispatchers.IO) {
            db.wmsOrderItemDao().getItemsForOrder(orderId)
        }
        
        withContext(Dispatchers.IO) {
            val currentOrderData = db.wmsOrderDao().getOrderById(orderId)
            val currentStatus = currentOrderData?.status ?: "Bekleyen"
            
            // Only auto-update between Bekleyen and Toplanıyor. Do NOT auto-promote to Toplandı.
            if (currentStatus == "Bekleyen" || currentStatus == "Toplanıyor") {
                val anyPicked = updatedItems.any { it.quantityPicked > 0 }
                val newStatus = if (anyPicked) "Toplanıyor" else "Bekleyen"
                if (currentStatus != newStatus) {
                    db.wmsOrderDao().updateOrderStatus(orderId, newStatus)
                }
            }
        }
        
        val updatedOrder = withContext(Dispatchers.IO) {
            db.wmsOrderDao().getOrderById(orderId)
        }
        
        _activeOrder.value = updatedOrder
        _activeOrderItems.value = updatedItems
        
        // Reload global orders list
        val allOrders = withContext(Dispatchers.IO) {
            db.wmsOrderDao().getAllOrders()
        }
        _orders.value = allOrders
    }

    fun generatePackageBarcode(orderId: String, onCompleted: () -> Unit = {}) {
        viewModelScope.launch {
            val randomNum = (1000000..9999999).random()
            val trackingNum = "TRK-$randomNum"
            val packageBarcodeValue = "PKG-$randomNum"
            
            withContext(Dispatchers.IO) {
                val currentOrderData = db.wmsOrderDao().getOrderById(orderId)
                if (currentOrderData != null) {
                    val currentOrderItems = db.wmsOrderItemDao().getItemsForOrder(orderId)
                    val actualTotalQty = currentOrderItems.sumOf { it.quantityPicked }
                    
                    val productsList = db.productDao().getAllProducts()
                    var orderTotalVal = 0.0
                    for (itemObj in currentOrderItems) {
                        val matchingProd = productsList.find { it.barcode == itemObj.productBarcode }
                        val basePrice = matchingProd?.basePrice ?: 120.0
                        orderTotalVal += (itemObj.quantityPicked * basePrice)
                    }
                    
                    val descLines = currentOrderItems.map { "• ${it.productTitle}: Toplanan ${it.quantityPicked} (İstenen: ${it.quantityOrdered})" }
                    val itemsDesc = descLines.joinToString("\n")
                    
                    val appItem = com.example.ui.screens.ApprovalItem(
                                            id = "REV-$orderId",
                                            type = "Düzenlenen Sipariş",
                                            customerName = currentOrderData.customerName,
                                            amount = orderTotalVal,
                                            time = java.text.SimpleDateFormat("dd.MM.yyyy HH:mm").format(java.util.Date()),
                                            reason = "Depo Sevkiyat Farkı - Teslim edilecek nihai fiş miktarları",
                                            description = "Revize WMS Sipariş Onayı\n" +
                                                   "Sipariş ID: $orderId\n" +
                                                   "Yeni Toplam Ürün: $actualTotalQty\n\n" + 
                                                   itemsDesc
                                        )
                    com.example.ui.screens.AppDataStore.approvalItems.add(appItem)
                }

                db.wmsOrderDao().updateOrderPackageBarcode(orderId, packageBarcodeValue, "Onay Bekliyor")
            }
            
            // Reload
            val updatedOrder = withContext(Dispatchers.IO) {
                db.wmsOrderDao().getOrderById(orderId)
            }
            _activeOrder.value = updatedOrder
            
            val allOrders = withContext(Dispatchers.IO) {
                db.wmsOrderDao().getAllOrders()
            }
            _orders.value = allOrders
            
            onCompleted()
        }
    }

    fun checkOutVehicle(orderId: String, plateNumber: String, onCompleted: () -> Unit = {}) {
        viewModelScope.launch {
            withContext(Dispatchers.IO) {
                db.wmsOrderDao().updateOrderVehiclePlate(orderId, plateNumber, "Sevk Edildi")
            }
            
            // Reload
            val updatedOrder = withContext(Dispatchers.IO) {
                db.wmsOrderDao().getOrderById(orderId)
            }
            _activeOrder.value = updatedOrder
            
            val allOrders = withContext(Dispatchers.IO) {
                db.wmsOrderDao().getAllOrders()
            }
            _orders.value = allOrders
            
            onCompleted()
        }
    }

    fun syncAll() {
        viewModelScope.launch {
            _isSyncing.value = true
            _syncStatusText.value = "Bağlantı kuruluyor..."
            delay(1000)
            _syncStatusText.value = "Yerel değişiklikler gönderiliyor (Offline-First)..."
            
            val pendingSyncs = withContext(Dispatchers.IO) {
                db.wmsOrderDao().getPendingSyncOrders()
            }
            
            // Push each pending local order update to cloud repository
            if (pendingSyncs.isNotEmpty()) {
                delay(800)
                withContext(Dispatchers.IO) {
                    pendingSyncs.forEach { order ->
                        // Simulate sending to network and mark as SYNCED
                        db.wmsOrderDao().insert(order.copy(syncStatus = "SYNCED"))
                    }
                }
            }
            
            _syncStatusText.value = "Güncel sevkiyat siparişleri indiriliyor..."
            delay(1000)
            
            // Mock adding a new incoming order from ERP system during sync
            withContext(Dispatchers.IO) {
                val currentOrders = db.wmsOrderDao().getAllOrders()
                val nextNum = (currentOrders.mapNotNull { 
                    it.id.removePrefix("SM-2026-").toIntOrNull() 
                }.maxOrNull() ?: 8) + 1
                
                val newOrderId = "SM-2026-${String.format("%03d", nextNum)}"
                
                val demoCustomers = listOf(
                    "Karadeniz Yapı Market Grubu", 
                    "Anadolu Hırdavat ve Boya Ticaret", 
                    "Yeditepe İnşaat Malzemeleri Ltd.", 
                    "Ege Tesisat ve Yapı Deposu",
                    "Doğu Anadolu Hırdavat A.Ş."
                )
                val customerName = demoCustomers[nextNum % demoCustomers.size]

                val newOrder = WmsOrderEntity(
                    id = newOrderId,
                    customerName = customerName,
                    orderDate = "17.06.2026",
                    status = "Bekleyen",
                    totalItems = 0,
                    syncStatus = "SYNCED"
                )
                
                // Add diverse mock items based on existing template products
                val demoBarcodes = listOf("8690123456789", "8699876543210", "1234567890123", "8681122334455")
                val demoTitles = listOf(
                    "Ultra Performans Endüstriyel Motor Yağı 20L", 
                    "Hava Filtresi - Ağır Vasıta Uyumlu Pro", 
                    "Çelik Rulman 120mm - Yüksek Devir", 
                    "Çelik Civata Takımı M8 x 40mm (100 ADET)"
                )
                val demoShelves = listOf("Raf A-1", "Raf B-3", "Raf A-4", "Raf C-1")
                
                // Deterministically choose a combination of items based on nextNum
                val itemIndices = when (nextNum % 4) {
                    0 -> listOf(0, 1)
                    1 -> listOf(1, 2)
                    2 -> listOf(2, 3)
                    else -> listOf(0, 3)
                }

                val newItems = itemIndices.mapIndexed { idx, pickIdx ->
                    val orderQty = ((idx + 2) * 3) + (nextNum % 3)
                    WmsOrderItemEntity(
                        id = "${newOrderId}_${demoBarcodes[pickIdx]}_$idx",
                        orderId = newOrderId,
                        productBarcode = demoBarcodes[pickIdx],
                        productTitle = demoTitles[pickIdx],
                        quantityOrdered = orderQty,
                        quantityPicked = 0,
                        isPicked = false,
                        shelfLocation = demoShelves[pickIdx]
                    )
                }
                
                val finalOrder = newOrder.copy(totalItems = newItems.sumOf { it.quantityOrdered })
                db.wmsOrderDao().insert(finalOrder)
                db.wmsOrderItemDao().insertAll(newItems)
            }
            
            loadOrders()
            _isSyncing.value = false
            _syncStatusText.value = "Senkronizasyon Başarılı!"
            delay(1500)
            _syncStatusText.value = ""
        }
    }
}

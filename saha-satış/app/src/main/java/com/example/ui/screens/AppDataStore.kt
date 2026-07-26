package com.example.ui.screens

import androidx.compose.runtime.mutableStateListOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.getValue
import androidx.compose.runtime.setValue
import androidx.compose.ui.graphics.Color
import android.content.Context
import com.example.data.database.DatabaseProvider
import com.example.data.database.Converters
import com.example.data.database.CustomerEntity
import com.example.data.database.ProductEntity
import com.example.data.database.BankEntity
import com.example.data.database.KasaLogEntity
import com.example.data.database.SalesRecordEntity
import com.example.data.api.CariAdresDto
import com.example.data.api.CariBankaHesapDto
import com.example.data.api.BridgeBankaDto
import com.example.data.api.KasalarDto
import com.example.data.api.KasaYonetimDto
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import org.json.JSONArray
import org.json.JSONObject

// --- STOCK COUNT MODELS ---
data class CountedItem(
    val barcode: String,
    val productTitle: String,
    val productCode: String,
    val brand: String,
    val expectedStock: Int,
    val countedQty: Int,
    val aisle: String
)

data class StockCountSession(
    val id: String,
    val date: String,
    val status: String, // "PENDING", "COMPLETED", "CANCELLED"
    val user: String, // Sayımı yapan / kapatan kullanıcı
    val warehouse: String, // Sayımın yapıldığı depo
    val countedItems: List<CountedItem>
)

// --- CENTRAL SYSTEM MODELS ---

data class ProductCatalog(
    val barcode: String,
    val code: String,
    val title: String,
    val category: String,
    val desc: String,
    val basePrice: Double,
    val dealerPrice: Double,
    val wholesalePrice: Double,
    val kdvPercent: Int,
    val imageUrlColor: Color,
    val brand: String? = null,
    val stockByWarehouse: Map<String, Int>, // Warehouse Name -> Qty
    val boxQty: Int? = null, // koli adeti
    val packageQty: Int? = null, // paket adeti
    val imageUrl: String? = null,
    val localImagePath: String? = null,
    val aisle: String? = null,
    val customPrices: Map<String, Double> = emptyMap(),
    val barcodes: List<String> = emptyList(),
    val measurement: String? = null,
    val packaging: String? = null,
    val cartonQuantity: String? = null
)

fun ProductCatalog.getPriceForGroup(groupName: String): Double {
    val matchKey = customPrices.keys.find { key ->
        groupName.contains(key, ignoreCase = true)
    }
    if (matchKey != null) {
        val price = customPrices[matchKey] ?: 0.0
        if (price > 0.0) return price
    }
    return when {
        groupName.contains("Bayi", ignoreCase = true) -> dealerPrice
        groupName.contains("Toptan", ignoreCase = true) -> wholesalePrice
        else -> basePrice
    }
}

val ProductCatalog.inferredAmbalaj: String
    get() = when {
        title.contains("Koli", ignoreCase = true) || boxQty != null -> "Koli"
        title.contains("Paket", ignoreCase = true) || packageQty != null -> "Paket"
        title.contains("Kutu", ignoreCase = true) -> "Kutu"
        title.contains("Çuval", ignoreCase = true) -> "Çuval"
        else -> "Adet"
    }

data class Customer(
    val id: String,
    val name: String,
    var balance: Double, // Pozitif is Borçlu (Debit), Negatif is Alacaklı (Credit)
    val lastVisit: String,
    val contact: String,
    val phone: String,
    val address: String,
    val taxOffice: String,
    val taxNumber: String,
    val gpsLocation: String,
    val riskLimit: Double,
    val priceGroup: String,
    val specialDiscountPercent: Double,
    val transactions: MutableList<CustomerTx>
) {
    val calculatedBalance: Double
        get() = balance
}

data class CustomerTx(
    val id: String,
    val date: String,
    val type: String, // "SATIŞ", "TAHSİLAT", "TEDİYE"
    val amount: Double,
    val description: String,
    val isOffline: Boolean = false,
    val erpRef: String? = null,
    val recNo: String? = null,
    val cha_recno: Int? = null
)

data class Bank(
    val id: String,
    val name: String,
    val accountNo: String,
    val iban: String,
    var balance: Double = 0.0
)

data class CashAccount(
    val id: String,
    val name: String,
    val currency: String = "TRY",
    var balance: Double = 0.0
)

data class KasaLogItem(
    val id: String,
    val date: String,
    val type: String,          // "Tahsilat" (Debit), "Tediye" (Credit), "Satış" (Cash Sale), "İade" (Refund Purchase)
    val customerOrSupplier: String,
    val amount: Double,
    val paymentType: String,   // "Nakit", "Kredi Kartı", "EFT / Havale"
    val bankName: String?,     // Selected Bank Name if Kredi Kartı or EFT / Havale
    val desc: String
)

data class SalesRecord(
    val customerId: String,
    val productBarcode: String,
    val quantity: Int,
    val price: Double,
    val date: String
)

data class SuspendedSaleItem(
    val productBarcode: String,
    val quantity: Int,
    val price: Double
)

data class SuspendedSale(
    val id: String,
    val date: String,
    val customerId: String?,
    val customerName: String,
    val items: List<SuspendedSaleItem>,
    val note: String,
    val warehouseName: String,
    val totalAmount: Double
)

data class CartItem(
    val product: ProductCatalog,
    var quantity: Int,
    var lineDiscountPercent: Double = 0.0,
    var note: String = ""
)

data class ApprovalItem(
    val id: String,
    val type: String, // "Satış", "Tahsilat", "İade", "Alış", "Tediye"
    val customerName: String,
    val description: String,
    val amount: Double,
    val time: String,
    val reason: String,
    val paymentType: String? = null,
    val orderNote: String? = null
)

data class PurchaseCartItem(
    val code: String,
    val title: String,
    val qty: Int,
    val price: Double,
    val prevPrice: Double,
    val isRegistered: Boolean = false
)

data class SuspendedPurchase(
    val id: String,
    val date: String,
    val supplierId: String?,
    val supplierName: String,
    val items: List<PurchaseCartItem>,
    val warehouseName: String,
    val invoiceSerial: String,
    val invoiceSeq: String,
    val totalAmount: Double
)

data class Expense(
    val id: String,
    val date: String,
    val category: String, // Yemek, Kırtasiye, Kargo, Yol/Yakıt, Bakım, Diğer
    val amount: Double,
    val description: String,
    val paymentSourceId: String, // e.g. "CA-MAIN" or "CA-BANK"
    val photoUri: String? = null
)

data class VehicleMaintenance(
    val id: String,
    val date: String,
    val km: Int,
    val description: String,
    val cost: Double,
    val photoUri: String? = null
)

data class Vehicle(
    val id: String,
    val plate: String,
    val brandModel: String,
    val currentKm: Int,
    val lastOilChangeKm: Int,
    val nextOilChangeKm: Int,
    val lastMaintenanceDate: String,
    val nextMaintenanceDate: String,
    val notes: String,
    val maintenanceHistoryJson: String // Serialized List<VehicleMaintenance> JSON
)

object AppDataStore {
    private val dbScope = CoroutineScope(Dispatchers.IO)
    private var isInitialized = false

    val expenses = mutableStateListOf<Expense>()
    val vehicles = mutableStateListOf<Vehicle>()

    val defaultExpenses = listOf(
        Expense("EXP-1001", "17.06.2026 12:30", "Yemek", 350.00, "Ofis personel öğle yemeği gideri", "CA-MAIN"),
        Expense("EXP-1002", "16.06.2026 15:45", "Kargo", 125.50, "Müşteri numune gönderim kargo ücreti", "CA-MAIN"),
        Expense("EXP-1003", "14.06.2026 10:15", "Kırtasiye", 420.00, "Yazıcı kartuş toner ve A4 kağıt alımı", "CA-BANK")
    )

    val defaultVehicles = listOf(
        Vehicle(
            id = "VEH-1001",
            plate = "34 ABC 123",
            brandModel = "Fiat Egea 1.3 MJet S&S",
            currentKm = 88450,
            lastOilChangeKm = 80000,
            nextOilChangeKm = 90000, // Warning! Less than 2000km remains (1550km remains)
            lastMaintenanceDate = "15.11.2025",
            nextMaintenanceDate = "15.11.2026",
            notes = "Ahmet (Saha Satış Temsilcisi) Atanan Aracı",
            maintenanceHistoryJson = "[{\"id\":\"VM-1001\",\"date\":\"15.11.2025\",\"km\":80000,\"description\":\"10.000 KM periyodik bakımı ve motor yağı değişimi yapıldı.\",\"cost\":3200.0}]"
        ),
        Vehicle(
            id = "VEH-1002",
            plate = "06 DEF 456",
            brandModel = "Ford Transit Custom Courier",
            currentKm = 121500,
            lastOilChangeKm = 110000,
            nextOilChangeKm = 120000, // Critical! Overdue oil change by 1500km!
            lastMaintenanceDate = "10.02.2025",
            nextMaintenanceDate = "10.02.2026", // Critical! Next seasonal maintenance overdue!
            notes = "Merkez Depo Sevkiyat Aracı",
            maintenanceHistoryJson = "[{\"id\":\"VM-1002\",\"date\":\"10.02.2025\",\"km\":110000,\"description\":\"Ağır periyodik bakım kapsamında triger kayışı ve debriyaj balata seti komple yenilendi.\",\"cost\":14500.0}]"
        )
    )

    fun serializeExpenses(): String {
        val arr = JSONArray()
        for (e in expenses) {
            val obj = JSONObject()
            obj.put("id", e.id)
            obj.put("date", e.date)
            obj.put("category", e.category)
            obj.put("amount", e.amount)
            obj.put("description", e.description)
            obj.put("paymentSourceId", e.paymentSourceId)
            if (e.photoUri != null) {
                obj.put("photoUri", e.photoUri)
            }
            arr.put(obj)
        }
        return arr.toString()
    }

    fun deserializeExpenses(json: String): List<Expense> {
        val list = mutableListOf<Expense>()
        if (json.isBlank()) return list
        try {
            val arr = JSONArray(json)
            for (i in 0 until arr.length()) {
                val obj = arr.getJSONObject(i)
                val pUri = if (obj.has("photoUri")) obj.getString("photoUri") else null
                list.add(
                    Expense(
                        obj.getString("id"),
                        obj.getString("date"),
                        obj.getString("category"),
                        obj.getDouble("amount"),
                        obj.getString("description"),
                        obj.getString("paymentSourceId"),
                        if (pUri == "null" || pUri.isNullOrEmpty()) null else pUri
                    )
                )
            }
        } catch (e: Exception) {
            e.printStackTrace()
        }
        return list
    }

    fun serializeVehicles(): String {
        val arr = JSONArray()
        for (v in vehicles) {
            val obj = JSONObject()
            obj.put("id", v.id)
            obj.put("plate", v.plate)
            obj.put("brandModel", v.brandModel)
            obj.put("currentKm", v.currentKm)
            obj.put("lastOilChangeKm", v.lastOilChangeKm)
            obj.put("nextOilChangeKm", v.nextOilChangeKm)
            obj.put("lastMaintenanceDate", v.lastMaintenanceDate)
            obj.put("nextMaintenanceDate", v.nextMaintenanceDate)
            obj.put("notes", v.notes)
            obj.put("maintenanceHistoryJson", v.maintenanceHistoryJson)
            arr.put(obj)
        }
        return arr.toString()
    }

    fun deserializeVehicles(json: String): List<Vehicle> {
        val list = mutableListOf<Vehicle>()
        if (json.isBlank()) return list
        try {
            val arr = JSONArray(json)
            for (i in 0 until arr.length()) {
                val obj = arr.getJSONObject(i)
                list.add(
                    Vehicle(
                        obj.getString("id"),
                        obj.getString("plate"),
                        obj.getString("brandModel"),
                        obj.getInt("currentKm"),
                        obj.getInt("lastOilChangeKm"),
                        obj.getInt("nextOilChangeKm"),
                        obj.getString("lastMaintenanceDate"),
                        obj.getString("nextMaintenanceDate"),
                        obj.getString("notes"),
                        obj.getString("maintenanceHistoryJson")
                    )
                )
            }
        } catch (e: Exception) {
            e.printStackTrace()
        }
        return list
    }

    val stockCountSessions = mutableStateListOf<StockCountSession>()

    fun serializeStockCountSessions(): String {
        val array = JSONArray()
        for (session in stockCountSessions) {
            val sessionObj = JSONObject()
            sessionObj.put("id", session.id)
            sessionObj.put("date", session.date)
            sessionObj.put("status", session.status)
            sessionObj.put("user", session.user)
            sessionObj.put("warehouse", session.warehouse)
            
            val itemsArray = JSONArray()
            for (item in session.countedItems) {
                val itemObj = JSONObject()
                itemObj.put("barcode", item.barcode)
                itemObj.put("productTitle", item.productTitle)
                itemObj.put("productCode", item.productCode)
                itemObj.put("brand", item.brand)
                itemObj.put("expectedStock", item.expectedStock)
                itemObj.put("countedQty", item.countedQty)
                itemObj.put("aisle", item.aisle)
                itemsArray.put(itemObj)
            }
            sessionObj.put("countedItems", itemsArray)
            array.put(sessionObj)
        }
        return array.toString()
    }

    fun deserializeStockCountSessions(jsonStr: String): List<StockCountSession> {
        if (jsonStr.isBlank()) return emptyList()
        val list = mutableListOf<StockCountSession>()
        try {
            val array = JSONArray(jsonStr)
            for (i in 0 until array.length()) {
                val sessionObj = array.getJSONObject(i)
                val id = sessionObj.optString("id", "")
                val date = sessionObj.optString("date", "")
                val status = sessionObj.optString("status", "")
                val user = sessionObj.optString("user", "")
                val warehouse = sessionObj.optString("warehouse", "")
                
                val itemsArray = sessionObj.optJSONArray("countedItems")
                val items = mutableListOf<CountedItem>()
                if (itemsArray != null) {
                    for (j in 0 until itemsArray.length()) {
                        val itemObj = itemsArray.getJSONObject(j)
                        items.add(
                            CountedItem(
                                barcode = itemObj.optString("barcode", ""),
                                productTitle = itemObj.optString("productTitle", ""),
                                productCode = itemObj.optString("productCode", ""),
                                brand = itemObj.optString("brand", ""),
                                expectedStock = itemObj.optInt("expectedStock", 0),
                                countedQty = itemObj.optInt("countedQty", 0),
                                aisle = itemObj.optString("aisle", "")
                            )
                        )
                    }
                }
                list.add(
                    StockCountSession(id, date, status, user, warehouse, items)
                )
            }
        } catch (e: Exception) {
            e.printStackTrace()
        }
        return list
    }

    val activeCartItems = mutableStateListOf<CartItem>()
    val purchaseSelectedSupplier = mutableStateOf<Customer?>(null)
    val purchaseSelectedTab = mutableStateOf(0)
    var purchaseWarehouse by mutableStateOf("Ana Depo")
    val purchaseInvoiceSerial = mutableStateOf("AL")
    val purchaseInvoiceSeq = mutableStateOf("2026-00412")
    val purchaseSupplierSearchQuery = mutableStateOf("")

    // Product inputs
    val purchaseProductCodeInput = mutableStateOf("")
    val purchaseProductTitleInput = mutableStateOf("")
    val purchaseQuantityInput = mutableStateOf("10")
    val purchaseUnitPriceInput = mutableStateOf("")
    val purchaseIsRegisteredProduct = mutableStateOf(false)
    val purchasePreviousPurchasePrice = mutableStateOf(0.0)

    val purchaseShowBarcodeScanner = mutableStateOf(false)
    val purchaseCart = mutableStateListOf<PurchaseCartItem>()
    val suspendedPurchases = mutableStateListOf<SuspendedPurchase>()
    val catalogCartItems = mutableStateListOf<CartItem>()
    val activeSelectedCustomer = mutableStateOf<Customer?>(null)
    val activeSelectedWarehouse = mutableStateOf<String>("Ana Depo")
    val activeOrderNote = mutableStateOf("")
    val activeGeneralDiscountPercent = mutableStateOf(0f)

    val suspendedSales = mutableStateListOf<SuspendedSale>()
    var activeLoadSuspendedSale by mutableStateOf<SuspendedSale?>(null)

    var allowNegativeStock by mutableStateOf(false)

    val definitions = androidx.compose.runtime.mutableStateMapOf<String, List<String>>(
        "Banka" to listOf("Ziraat Bankası", "Garanti BBVA", "İş Bankası", "Akbank", "Halkbank", "Vakıfbank", "Yapı Kredi"),
        "KDV" to listOf("%0", "%1", "%8", "%10", "%18", "%20"),
        "Fiyat" to listOf("Perakende", "Bayi", "Toptan"),
        "Depo" to listOf("Ana Depo", "Araç Deposu", "Merkez Depo"),
        "Kategori" to listOf("Temel Gıda", "Süt Ürünleri", "Atıştırmalık", "İçecek", "Temizlik", "Kozmetik", "Diğer"),
        "Reyon" to listOf("Reyon 1", "Reyon 2", "Ön Kasa", "Soğuk Dolap"),
        "Ambalaj" to listOf("Adet", "Koli", "Paket", "Çuval", "Kutu"),
        "Marka" to listOf("Torku", "Ülker", "Eti", "Pınar", "Sütaş"),
        "Bölgeler" to listOf("Avrupa Yakası", "Anadolu Yakası", "Merkez", "Taşra"),
        "Birimler" to listOf("Adet", "Kg", "Gr", "Lt", "Paket", "Koli"),
        "Müşteri" to listOf("Kurumsal", "Bireysel", "Bakkal", "Market", "Toptancı")
    )

    var autoApproveAllTransactions by mutableStateOf(false)
    var sendToApprovalCenterDirectly by mutableStateOf(true)

    var visibleModules by mutableStateOf<Set<String>>(
        setOf(
            "sales", "suspended_sales", "operations/purchase", "operations/returns",
            "operations/collection", "operations/disbursement", "operations/cashbox",
            "operations/eod", "customers", "reports", "operations/stocks",
            "operations/counting", "operations/warehouses", "catalog", "operations/approvals", "more", "wms_warehouse",
            "operations/expenses", "operations/vehicles"
        )
    )

    var activeKpiList by mutableStateOf<List<String>>(
        listOf("ciro", "ziyaret")
    )

    var globalShowBarcodeScanner by mutableStateOf(false)
    var globalScannedBarcode by mutableStateOf<String?>(null)

    val wmsSelectedTab = mutableStateOf(0)
    val wmsShowScannerDialog = mutableStateOf(false)

    val salesSelectedTab = mutableStateOf(0)
    val salesShowImagesMode = mutableStateOf(true)
    val salesSearchQuery = mutableStateOf("")
    val salesSelectedCategory = mutableStateOf("Tümü")
    val salesSelectedSortOrder = mutableStateOf("Ürün Adı [A-Z]")
    val salesShowBarcodeScanner = mutableStateOf(false)
    val salesShowContinuousBarcodeScanner = mutableStateOf(false)
    val salesContinuousScannedBarcode = mutableStateOf<String?>(null)
    val salesShowFiltersDialog = mutableStateOf(false)

    // Advanced Sort & Filter states for Sales Screen
    val salesSelectedSortField = mutableStateOf("İsim") // "İsim", "Kod", "Fiyat", "Marka", "Stok"
    val salesSelectedSortAsc = mutableStateOf(true)
    val salesFilterBrands = mutableStateOf(setOf<String>())
    val salesFilterCategories = mutableStateOf(setOf<String>())
    val salesFilterAmbalajs = mutableStateOf(setOf<String>())
    val salesFilterMinPrice = mutableStateOf("")
    val salesFilterMaxPrice = mutableStateOf("")
    val salesFilterMinStock = mutableStateOf("")
    val salesFilterMaxStock = mutableStateOf("")
    val salesFilterHideNoPhoto = mutableStateOf(false)
    val salesFilterHideOutOfStock = mutableStateOf(false)

    // Customers Screen bottom bar unified states
    val customerSearchQuery = mutableStateOf("")
    val customerSelectedFilterTab = mutableStateOf("Tümü")
    val customerShowAddDialog = mutableStateOf(false)
    val customerShowEditDialog = mutableStateOf(false)
    val customerDetailActiveTab = mutableStateOf(0)

    // Catalog Screen bottom bar unified states
    val catalogSearchQuery = mutableStateOf("")
    val catalogSelectedCategory = mutableStateOf("Tümü")
    val catalogSelectedSortOrder = mutableStateOf("Ürün Adı [A-Z]")
    val catalogActiveTab = mutableStateOf(0)
    val catalogShowAddProductDialog = mutableStateOf(false)
    val catalogShowBarcodeScanner = mutableStateOf(false)
    val catalogShowCartDialog = mutableStateOf(false)
    val catalogSelectedViewMode = mutableStateOf("List")
    val catalogShowFiltersDialog = mutableStateOf(false)

    // Advanced Sort & Filter states for Catalog Screen
    val catalogSelectedSortField = mutableStateOf("İsim") // "İsim", "Kod", "Fiyat", "Marka", "Stok"
    val catalogSelectedSortAsc = mutableStateOf(true)
    val catalogFilterBrands = mutableStateOf(setOf<String>())
    val catalogFilterCategories = mutableStateOf(setOf<String>())
    val catalogFilterAmbalajs = mutableStateOf(setOf<String>())
    val catalogFilterMinPrice = mutableStateOf("")
    val catalogFilterMaxPrice = mutableStateOf("")
    val catalogFilterMinStock = mutableStateOf("")
    val catalogFilterMaxStock = mutableStateOf("")
    val catalogFilterHideNoPhoto = mutableStateOf(false)
    val catalogFilterHideOutOfStock = mutableStateOf(false)

    // Stocks Screen bottom bar unified states
    val stocksSearchQuery = mutableStateOf("")
    val stocksSelectedCategory = mutableStateOf("Hepsi")
    val stocksSelectedSortOrder = mutableStateOf("Ürün Adı [A-Z]")
    val stocksShowBarcodeScanner = mutableStateOf(false)
    val stocksShowAddProductDialog = mutableStateOf(false)
    val showStockDetailEditDialog = mutableStateOf(false)

    // Approvals Screen bottom bar unified states
    val approvalSearchQuery = mutableStateOf("")
    val approvalSelectedFilter = mutableStateOf("Tümü")
    val approvalSelectedDateFilter = mutableStateOf<String?>(null)
    val approvalIsSearchExpanded = mutableStateOf(false)
    val approvalStatusFilter = mutableStateOf("Bekleyen") // "Bekleyen", "Onaylanan", "Reddedilen"
    val approvedApprovalItems = androidx.compose.runtime.mutableStateListOf<ApprovalItem>()
    val rejectedApprovalItems = androidx.compose.runtime.mutableStateListOf<ApprovalItem>()
    val approvalOrderItemsMap = androidx.compose.runtime.mutableStateMapOf<String, List<CartItem>>()

    val approvalItems = androidx.compose.runtime.mutableStateListOf<ApprovalItem>(
        ApprovalItem("ST-2026-0034", "Satış", "Güven Gıda Ltd.", "12 Koli Ayçiçek Yağı Satışı", 16800.00, "08:50", "Müşteri risk limiti aşıldı; finans onayından sonra sevk edilecek.", "Cari Borç", "Sevkiyat acil, müşteri yarın sabah teslimat istiyor."),
        ApprovalItem("ST-2026-0038", "Satış", "Köroğlu Market", "Hediyelik Çikolata Siparişi", 4200.00, "11:15", "Tanımlanan iskonto (%15), izin verilen bayi sınırından (%10) daha yüksektir.", "Kredi Kartı", "Sipariş kartvizit ile paketlensin."),
        ApprovalItem("TS-2026-0012", "Tahsilat", "Yıldız Kozmetik", "Elden Senet Tahsilatı", 12000.00, "09:30", "Gecikmiş borç kapaması; muhasebe onayı beklenmektedir.", "Nakit"),
        ApprovalItem("TS-2026-0015", "Tahsilat", "Mert İletişim", "Çek Ciro Talebi", 45000.00, "14:22", "Çek vadesi 120 günü aşmaktadır (azami 90 gün limitli).", "Kredi Kartı / EFT"),
        ApprovalItem("IA-2026-0005", "İade", "Özgür Ticaret", "Hasarlı Ürün İade Talebi", 3450.00, "10:10", "Sezon dışı hasarlı ürün iade onayı beklenmektedir.", "Cari Borç", "Kırık ambalajlı teslim alınan mal iadesi."),
        ApprovalItem("AL-2026-0021", "Alış", "Toptancı Depo A.Ş.", "Depo Hammadde Alımı", 85000.00, "10:45", "Yüksek tutarlı alım emri; genel müdürlük teyidi gerekiyor.", "EFT / Havale"),
        ApprovalItem("TD-2026-0008", "Tediye", "Borusan Lojistik", "Saha Sevk Sevkiyat Bedeli", 7200.00, "13:05", "Saha avans kasasından tediye ödeme onayı bekleniyor.", "Nakit")
    )

    var bottomBarTabs by mutableStateOf<List<String>>(
        listOf("sales", "customers", "catalog", "reports")
    )

    var quickActionsOrder by mutableStateOf<List<String>>(
        listOf(
            "sales", "suspended_sales", "operations/purchase", "operations/returns",
            "operations/collection", "operations/disbursement", "operations/cashbox",
            "operations/eod", "customers", "reports", "operations/stocks",
            "operations/counting", "operations/warehouses", "catalog", "operations/approvals", "wms_warehouse",
            "operations/expenses", "operations/vehicles"
        )
    )

    var licenseKey by mutableStateOf("")
    var subscriptionPack by mutableStateOf("local")
    var companyId by mutableStateOf("")

    var moreSelectedTabIndex by mutableStateOf(0)

    fun isErpModeActive(context: Context): Boolean {
        val prefs = context.getApplicationContext().getSharedPreferences("erp_settings", Context.MODE_PRIVATE)
        return prefs.getBoolean("is_erp_active", true)
    }

    fun setSubscriptionPackSetting(context: Context, value: String) {
        subscriptionPack = value
        val prefs = context.getApplicationContext().getSharedPreferences("app_settings", Context.MODE_PRIVATE)
        prefs.edit().putString("subscription_pack", value).apply()
    }

    fun setCompanyIdSetting(context: Context, value: String) {
        companyId = value
        val prefs = context.getApplicationContext().getSharedPreferences("app_settings", Context.MODE_PRIVATE)
        prefs.edit().putString("company_id", value).apply()
    }

    fun setAllowNegativeStockSetting(context: Context, value: Boolean) {
        allowNegativeStock = value
        val prefs = context.getApplicationContext().getSharedPreferences("app_settings", Context.MODE_PRIVATE)
        prefs.edit().putBoolean("allow_negative_stock", value).apply()
    }

    fun setLicenseKeySetting(context: Context, value: String) {
        licenseKey = value
        val prefs = context.getApplicationContext().getSharedPreferences("app_settings", Context.MODE_PRIVATE)
        prefs.edit().putString("license_key", value).apply()
    }

    fun setVisibleModulesSetting(context: Context, value: Set<String>) {
        visibleModules = value
        val prefs = context.getApplicationContext().getSharedPreferences("app_settings", Context.MODE_PRIVATE)
        prefs.edit().putStringSet("visible_modules", value).apply()
    }

    fun setActiveKpiListSetting(context: Context, value: List<String>) {
        activeKpiList = value
        val prefs = context.getApplicationContext().getSharedPreferences("app_settings", Context.MODE_PRIVATE)
        prefs.edit().putString("active_kpi_list", value.joinToString(",")).apply()
    }

    fun setBottomBarTabsSetting(context: Context, value: List<String>) {
        bottomBarTabs = value
        val prefs = context.getApplicationContext().getSharedPreferences("app_settings", Context.MODE_PRIVATE)
        prefs.edit().putString("bottom_bar_tabs", value.joinToString(",")).apply()
    }

    fun setQuickActionsOrderSetting(context: Context, value: List<String>) {
        quickActionsOrder = value
        val prefs = context.getApplicationContext().getSharedPreferences("app_settings", Context.MODE_PRIVATE)
        prefs.edit().putString("quick_actions_order", value.joinToString(",")).apply()
    }

    // 1- Global Bank List
    val defaultBanks = listOf(
        Bank("B-001", "Garanti Ticari", "1234-5678-90", "TR90 1200 4500 0012 3456 7890 12", 250000.00),
        Bank("B-002", "Akbank Şirket", "9876-5432-10", "TR12 4500 6700 0098 7654 3210 98", 185000.00),
        Bank("B-003", "YapıKredi E-Ticaret", "1122-3344-55", "TR34 7600 2300 0011 2233 4455 66", 95000.00)
    )
    val banks = mutableStateListOf<Bank>()

    // 2- Global Kasa Ledger Logs (Kasa/Banka hareketleri)
    val defaultKasaLogs = listOf(
        KasaLogItem("K-1001", "08.06.2026 10:15", "Tahsilat", "Yıldırım Metal Döküm A.Ş.", 2500.0, "Kredi Kartı", "Garanti Ticari", "Bakiye tahsilatı"),
        KasaLogItem("K-1002", "08.06.2026 12:45", "Tediye", "Bursa Plastik Teknolojileri", 6000.0, "EFT / Havale", "Akbank Şirket", "Hammadde ödemesi"),
        KasaLogItem("K-1003", "08.06.2026 14:20", "Satış", "Akkurt Market Gıda Sanayi", 3120.0, "Nakit", null, "Saha satışı perakende nolu evrak kesimi"),
        KasaLogItem("K-1004", "08.06.2026 16:30", "İade", "Acme Corp Logistics Ltd.", 1200.0, "Nakit", null, "Ürün iade fişi nakit iade")
    )
    val kasaLogs = mutableStateListOf<KasaLogItem>()
    val cashAccounts = mutableStateListOf<CashAccount>(
        CashAccount("CA-MAIN", "Merkez Kasa", "TRY", 15000.0),
        CashAccount("CA-BANK", "Banka Hesabı (Akbank)", "TRY", 45000.0)
    )

    val cariAdresleri = mutableStateListOf<CariAdresDto>()
    val cariBankaHesaplari = mutableStateListOf<CariBankaHesapDto>()
    val bridgeBankalar = mutableStateListOf<BridgeBankaDto>()
    val bridgeKasalar = mutableStateListOf<KasalarDto>()
    val kasaYonetimList = mutableStateListOf<KasaYonetimDto>()

    // 3- Historically sold products to customers (For Sales filtering in returns)
    val defaultSalesHistory = listOf(
        // Acme Corp purchased: Motor Yağı and Hava Filtresi
        SalesRecord("CUS-10045", "8690123456789", 5, 2450.00, "05.06.2026"),
        SalesRecord("CUS-10045", "8699876543210", 10, 485.50, "02.06.2026"),
        
        // Global Petrol purchased: Rulman Seti and Motor Yağı
        SalesRecord("CUS-10082", "1234567890123", 2, 890.00, "15.05.2026"),
        SalesRecord("CUS-10082", "8690123456789", 3, 2450.00, "12.05.2026"),
        
        // Akkurt Market purchased: Civata Takımı
        SalesRecord("CUS-10115", "8681122334455", 15, 185.00, "07.06.2026")
    )
    val salesHistory = mutableStateListOf<SalesRecord>()

    // 4- Global Products List Shared across Catalog and Sales screens
    val defaultProducts = listOf(
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
            imageUrlColor = Color(0xFFFFDB58),
            stockByWarehouse = mapOf("Ana Depo" to 145, "Ankara Merkez" to 42, "Ege Bölge" to 12),
            boxQty = 4,
            packageQty = 1,
            imageUrl = "https://images.unsplash.com/photo-1619642751034-765dfdf7c58e?auto=format&fit=crop&q=80&w=600,https://images.unsplash.com/photo-1486006920555-c77dce18193b?auto=format&fit=crop&q=80&w=600,https://images.unsplash.com/photo-1518364538800-6bcb3f25da49?auto=format&fit=crop&q=80&w=600"
        ),
        ProductCatalog(
            barcode = "8699876543210",
            code = "FLT-AIR-901",
            title = "Hava Filtresi - Ağır Vasıta Uyumlu Pro",
            category = "Filtre Grupları",
            desc = "Yüksek mikron süzme kapasitelerine sahip, ağır hizmet kamyon filtresi.",
            basePrice = 485.50,
            dealerPrice = 410.00,
            wholesalePrice = 370.00,
            kdvPercent = 10,
            imageUrlColor = Color(0xFF0096FF),
            stockByWarehouse = mapOf("Ana Depo" to 89, "Ankara Merkez" to 110, "Ege Bölge" to 0),
            boxQty = 12,
            packageQty = 2,
            imageUrl = "https://images.unsplash.com/photo-1555215695-3004980ad54e?auto=format&fit=crop&q=80&w=600,https://images.unsplash.com/photo-1486006920555-c77dce18193b?auto=format&fit=crop&q=80&w=600,https://images.unsplash.com/photo-1504384308090-c894fdcc538d?auto=format&fit=crop&q=80&w=600"
        ),
        ProductCatalog(
            barcode = "1234567890123",
            code = "BRG-STL-120",
            title = "Çelik Rulman 120mm - Yüksek Devir",
            category = "Yedek Parça",
            desc = "CNC ve torna milleri için süper kaygan çift bilyalı mil yatağı rulmanı.",
            basePrice = 890.00,
            dealerPrice = 800.00,
            wholesalePrice = 720.00,
            kdvPercent = 20,
            imageUrlColor = Color(0xFF50C878),
            stockByWarehouse = mapOf("Ana Depo" to 0, "Ankara Merkez" to 5, "Ege Bölge" to 22),
            boxQty = 24,
            packageQty = 6,
            imageUrl = "https://images.unsplash.com/photo-1581092160607-ee22621dd758?auto=format&fit=crop&q=80&w=600,https://images.unsplash.com/photo-1537462715879-360eeb61a0bc?auto=format&fit=crop&q=80&w=600,https://images.unsplash.com/photo-1504198453319-5ce911bafcde?auto=format&fit=crop&q=80&w=600"
        ),
        ProductCatalog(
            barcode = "8681122334455",
            code = "SRF-CLV-M8",
            title = "Çelik Civata Takımı M8 x 40mm (100 ADET)",
            category = "Sarf Malzemeler",
            desc = "Sertleştirilmiş galvaniz kaplama endüstriyel civata.",
            basePrice = 185.00,
            dealerPrice = 160.00,
            wholesalePrice = 145.00,
            kdvPercent = 20,
            imageUrlColor = Color(0xFFEE4B2B),
            stockByWarehouse = mapOf("Ana Depo" to 320, "Ankara Merkez" to 80, "Ege Bölge" to 55),
            boxQty = 50,
            packageQty = 10,
            imageUrl = "https://images.unsplash.com/photo-1590372847146-2674026ec1dc?auto=format&fit=crop&q=80&w=600,https://images.unsplash.com/photo-1534224039826-c7a0dea0e66a?auto=format&fit=crop&q=80&w=600,https://images.unsplash.com/photo-1513258496099-48168024aec0?auto=format&fit=crop&q=80&w=600"
        )
    )
    val products = mutableStateListOf<ProductCatalog>()

    // 5- Global Customers List Shared across screens with Transaction sub-lists
    val defaultCustomers = listOf(
        Customer(
            id = "CUS-10045",
            name = "Acme Corp Logistics Ltd.",
            balance = 14250.00,
            lastVisit = "3 gün önce",
            contact = "Jane Doe",
            phone = "+90 (532) 123 45 67",
            address = "İkitelli Org. San. Bölgesi, Metal İşçileri Sitesi 12. Blok No: 4, Başakşehir / İstanbul",
            taxOffice = "İkitelli V.D.",
            taxNumber = "0023451234",
            gpsLocation = "41.0745° N, 28.7951° E",
            riskLimit = 50000.0,
            priceGroup = "Bayi-1 Klasmanı",
            specialDiscountPercent = 7.5,
            transactions = mutableStateListOf(
                CustomerTx("TX-1001", "05.06.2026", "SATIŞ", 8940.0, "Fatura No: FT-12002"),
                CustomerTx("TX-1002", "02.06.2026", "TAHSİLAT", 5000.0, "Nakit Tahsilat Makbuzu"),
                CustomerTx("TX-1003", "28.05.2026", "SATIŞ", 10310.0, "Fatura No: FT-11915")
            )
        ),
        Customer(
            id = "CUS-10082",
            name = "Global Petrol Kimya A.Ş.",
            balance = -2450.00,
            lastVisit = "45 gün önce",
            contact = "Mark Smith",
            phone = "+90 (212) 555 88 99",
            address = "Caddesi No: 89 Kat: 5, Maslak / İstanbul",
            taxOffice = "Maslak V.D.",
            taxNumber = "4432109876",
            gpsLocation = "41.1120° N, 29.0210° E",
            riskLimit = 150000.0,
            priceGroup = "Toptan Distribütör",
            specialDiscountPercent = 12.0,
            transactions = mutableStateListOf(
                CustomerTx("TX-2001", "15.05.2026", "TAHSİLAT", 10000.0, "Banka EFT - Finans"),
                CustomerTx("TX-2002", "12.05.2026", "TEDİYE", 7550.0, "Nakit İade Ödemesi")
            )
        ),
        Customer(
            id = "CUS-10115",
            name = "Akkurt Market Gıda Sanayi",
            balance = 850.50,
            lastVisit = "Dün",
            contact = "Süleyman Akkurt",
            phone = "+90 (544) 987 65 43",
            address = "Atatürk Cad. Çağlayan Mah. No: 12, Kağıthane / İstanbul",
            taxOffice = "Kağıthane V.D.",
            taxNumber = "8899776655",
            gpsLocation = "41.0820° N, 28.9734° E",
            riskLimit = 15000.0,
            priceGroup = "Perakende B Grubu",
            specialDiscountPercent = 3.0,
            transactions = mutableStateListOf(
                CustomerTx("TX-3001", "07.06.2026", "SATIŞ", 850.5, "Fatura No: FT-12056")
            )
        )
    )
    val customers = mutableStateListOf<Customer>()

    suspend fun clearAllDataSync(context: Context) {
        val db = DatabaseProvider.getDatabase(context)
        db.bankDao().deleteAll()
        db.kasaLogDao().deleteAll()
        db.salesRecordDao().deleteAll()
        db.productDao().deleteAll()
        db.customerDao().deleteAll()

        withContext(Dispatchers.Main) {
            banks.clear()
            kasaLogs.clear()
            salesHistory.clear()
            products.clear()
            customers.clear()
            activeCartItems.clear()
        }
    }

    suspend fun loadDemoDataSync(context: Context) {
        withContext(Dispatchers.Main) {
            banks.clear()
            banks.addAll(defaultBanks)
            kasaLogs.clear()
            kasaLogs.addAll(defaultKasaLogs)
            products.clear()
            products.addAll(defaultProducts)
            customers.clear()
            customers.addAll(defaultCustomers)
            salesHistory.clear()
            salesHistory.addAll(defaultSalesHistory)
        }
        persistSync(context)
    }

    suspend fun initializeSync(context: Context) {
        if (isInitialized) return
        withContext(Dispatchers.IO) {
            try {
                // Load settings
                val prefs = context.getApplicationContext().getSharedPreferences("app_settings", Context.MODE_PRIVATE)
                allowNegativeStock = prefs.getBoolean("allow_negative_stock", false)
                licenseKey = prefs.getString("license_key", "") ?: ""
                subscriptionPack = prefs.getString("subscription_pack", "local") ?: "local"
                companyId = prefs.getString("company_id", "") ?: ""

                val scJson = prefs.getString("stock_count_sessions", "") ?: ""
                val scList = deserializeStockCountSessions(scJson)
                
                val expensesStr = prefs.getString("expenses_json", "") ?: ""
                val vehiclesStr = prefs.getString("vehicles_json", "") ?: ""
                
                withContext(Dispatchers.Main) {
                    stockCountSessions.clear()
                    stockCountSessions.addAll(scList)
                    
                    expenses.clear()
                    if (expensesStr.isNotEmpty()) {
                        expenses.addAll(deserializeExpenses(expensesStr))
                    } else {
                        expenses.addAll(defaultExpenses)
                    }
                    
                    vehicles.clear()
                    if (vehiclesStr.isNotEmpty()) {
                        vehicles.addAll(deserializeVehicles(vehiclesStr))
                    } else {
                        vehicles.addAll(defaultVehicles)
                    }
                }

                // Initial alignment pull from cloud if company mode
                try {
                    com.example.data.CloudSyncManager.autoPullOnStartup(context)
                } catch (ce: Throwable) {
                    ce.printStackTrace()
                }

                // Load custom definitions
                definitions.keys.forEach { key ->
                    val saved = prefs.getString("def_${key}", null)
                    if (saved != null) {
                        definitions[key] = if (saved.isEmpty()) emptyList() else saved.split("|||")
                    }
                }

                val defaultModuleSet = setOf(
                    "sales", "suspended_sales", "operations/purchase", "operations/returns",
                    "operations/collection", "operations/disbursement", "operations/cashbox",
                    "operations/eod", "customers", "reports", "operations/stocks",
                    "operations/counting", "operations/warehouses", "catalog", "operations/approvals", "more", "wms_warehouse",
                    "operations/expenses", "operations/vehicles"
                )
                visibleModules = prefs.getStringSet("visible_modules", null) ?: defaultModuleSet

                val kpiString = prefs.getString("active_kpi_list", "ciro,ziyaret") ?: "ciro,ziyaret"
                activeKpiList = if (kpiString.isEmpty()) emptyList() else kpiString.split(",")

                val tabsString = prefs.getString("bottom_bar_tabs", "sales,customers,catalog,reports") ?: "sales,customers,catalog,reports"
                val splitTabs = tabsString.split(",")
                bottomBarTabs = if (splitTabs.size == 4) splitTabs else listOf("sales", "customers", "catalog", "reports")

                val orderString = prefs.getString("quick_actions_order", null)
                if (orderString != null) {
                    val splitOrder = orderString.split(",")
                    if (splitOrder.isNotEmpty()) {
                        quickActionsOrder = splitOrder
                    }
                }

                val db = DatabaseProvider.getDatabase(context)
                val existingCustomers = db.customerDao().getAllCustomers()
                val converter = Converters()
                val loggedInUser = db.userDao().getActiveUser()

                // Products and customers are synchronized independently.  Do not treat an
                // empty customer table as an empty local database; otherwise a successful
                // product-only synchronization is ignored on the next application launch.
                if (existingCustomers.isEmpty() && db.productDao().getAllProducts().isEmpty()) {
                    // Start of app without data
                    if (loggedInUser?.username == "admin") {
                        loadDemoDataSync(context)
                    }
                } else {
                    // Populate memory lists from persistent Room DB
                    val loadedBanks = db.bankDao().getAllBanks().map { Bank(it.id, it.name, it.accountNo, it.iban, it.balance) }
                    val loadedKasa = db.kasaLogDao().getAllKasaLogs().map { KasaLogItem(it.id, it.date, it.type, it.customerOrSupplier, it.amount, it.paymentType, it.bankName, it.desc) }
                    val loadedSales = db.salesRecordDao().getAllSalesRecords().map { SalesRecord(it.customerId, it.productBarcode, it.quantity, it.price, it.date) }
                    val loadedProducts = db.productDao().getAllProducts().map { prod ->
                        ProductCatalog(
                            barcode = prod.barcode,
                            code = prod.code,
                            title = prod.title,
                            category = prod.category,
                            desc = prod.desc,
                            basePrice = prod.basePrice,
                            dealerPrice = prod.dealerPrice,
                            wholesalePrice = prod.wholesalePrice,
                            kdvPercent = prod.kdvPercent,
                            imageUrlColor = Color(prod.colorValue.toULong()),
                            brand = prod.brand,
                            stockByWarehouse = converter.toWarehouseMap(prod.stockByWarehouseJson),
                            boxQty = prod.boxQty,
                            packageQty = prod.packageQty,
                            imageUrl = prod.imageUrl,
                            localImagePath = prod.localImagePath,
                            aisle = prod.aisle,
                            customPrices = converter.toCustomPricesMap(prod.customPricesJson ?: "{}"),
                            barcodes = converter.toBarcodeList(prod.barcodesJson)
                        )
                    }
                    val loadedCustomers = db.customerDao().getAllCustomers().map { cust ->
                        Customer(
                            id = cust.id,
                            name = cust.name,
                            balance = cust.balance,
                            lastVisit = cust.lastVisit,
                            contact = cust.contact,
                            phone = cust.phone,
                            address = cust.address,
                            taxOffice = cust.taxOffice,
                            taxNumber = cust.taxNumber,
                            gpsLocation = cust.gpsLocation,
                            riskLimit = cust.riskLimit,
                            priceGroup = cust.priceGroup,
                            specialDiscountPercent = cust.specialDiscountPercent,
                            transactions = mutableStateListOf<CustomerTx>().apply {
                                addAll(converter.toCustomerTxList(cust.transactionsJson))
                            }
                        )
                    }

                    withContext(Dispatchers.Main) {
                        banks.clear()
                        banks.addAll(loadedBanks)

                        kasaLogs.clear()
                        kasaLogs.addAll(loadedKasa)

                        salesHistory.clear()
                        salesHistory.addAll(loadedSales)

                        customers.clear()
                        customers.addAll(loadedCustomers)

                        // Start collecting products flow
                        dbScope.launch {
                            db.productDao().getAllProductsFlow().collect { prodEntities ->
                                val mapped = prodEntities.map { prod ->
                                    ProductCatalog(
                                        barcode = prod.barcode,
                                        code = prod.code,
                                        title = prod.title,
                                        category = prod.category,
                                        desc = prod.desc,
                                        basePrice = prod.basePrice,
                                        dealerPrice = prod.dealerPrice,
                                        wholesalePrice = prod.wholesalePrice,
                                        kdvPercent = prod.kdvPercent,
                                        imageUrlColor = Color(prod.colorValue.toULong()),
                                        brand = prod.brand,
                                        stockByWarehouse = converter.toWarehouseMap(prod.stockByWarehouseJson),
                                        boxQty = prod.boxQty,
                                        packageQty = prod.packageQty,
                                        imageUrl = prod.imageUrl,
                                        localImagePath = prod.localImagePath,
                                        aisle = prod.aisle,
                                        customPrices = converter.toCustomPricesMap(prod.customPricesJson ?: "{}"),
                                        barcodes = converter.toBarcodeList(prod.barcodesJson)
                                    )
                                }
                                withContext(Dispatchers.Main) {
                                    products.clear()
                                    products.addAll(mapped)
                                }
                            }
                        }

                        // Pre-populate product items for standard mock approvals
                        val oilProd = products.find { it.barcode == "8690123456789" }
                        val filterProd = products.find { it.barcode == "8699876543210" }
                        val bearingProd = products.find { it.barcode == "1234567890123" }

                        if (oilProd != null && filterProd != null) {
                            approvalOrderItemsMap["ST-2026-0034"] = listOf(
                                CartItem(
                                    product = oilProd,
                                    quantity = 6,
                                    lineDiscountPercent = 5.0,
                                    note = "Hediye promosyon eldiveni verilsin."
                                ),
                                CartItem(
                                    product = filterProd,
                                    quantity = 4,
                                    lineDiscountPercent = 0.0,
                                    note = "Ezilmeyecek şekilde paketlensin."
                                )
                            )
                        }

                        if (bearingProd != null) {
                            approvalOrderItemsMap["ST-2026-0038"] = listOf(
                                CartItem(
                                    product = bearingProd,
                                    quantity = 5,
                                    lineDiscountPercent = 15.0,
                                    note = "Müşteri özel iskonto talep etti."
                                )
                            )
                        }
                    }
                }
                isInitialized = true
            } catch (e: Throwable) {
                e.printStackTrace()
            }
        }
    }

    fun initialize(context: Context, onComplete: () -> Unit = {}) {
        dbScope.launch {
            try {
                initializeSync(context)
            } catch (t: Throwable) {
                t.printStackTrace()
            } finally {
                withContext(Dispatchers.Main) {
                    onComplete()
                }
            }
        }
    }

    fun persist(context: Context) {
        dbScope.launch {
            persistSync(context)
        }
    }

    private suspend fun persistSync(context: Context) {
        try {
            val db = DatabaseProvider.getDatabase(context)
            val converter = Converters()

            // Save custom definitions
            val prefs = context.getApplicationContext().getSharedPreferences("app_settings", Context.MODE_PRIVATE)
            val editor = prefs.edit()
            definitions.forEach { (key, list) ->
                editor.putString("def_${key}", list.joinToString("|||"))
            }

            // Save Stock Count Sessions
            val scJson = serializeStockCountSessions()
            editor.putString("stock_count_sessions", scJson)
            
            // Save Expenses & Vehicles to SharedPreferences
            editor.putString("expenses_json", serializeExpenses())
            editor.putString("vehicles_json", serializeVehicles())
            
            editor.apply()

            // 1. Save Banks
            val bankEntities = banks.map { BankEntity(it.id, it.name, it.accountNo, it.iban, it.balance) }
            db.bankDao().deleteAll()
            db.bankDao().insertAll(bankEntities)

            // 2. Save Kasa Logs
            val kasaEntities = kasaLogs.map { KasaLogEntity(it.id, it.date, it.type, it.customerOrSupplier, it.amount, it.paymentType, it.bankName, it.desc) }
            db.kasaLogDao().deleteAll()
            db.kasaLogDao().insertAll(kasaEntities)

            // 3. Save Sales History (using index offset for id to prevent batch insert key collisions)
            val salesEntities = salesHistory.mapIndexed { idx, it ->
                SalesRecordEntity(id = idx + 1, customerId = it.customerId, productBarcode = it.productBarcode, quantity = it.quantity, price = it.price, date = it.date)
            }
            db.salesRecordDao().deleteAll()
            db.salesRecordDao().insertAll(salesEntities)

            // 4. Save products.  ERP synchronization first updates the in-memory list and
            // then calls persist(); without this write, the product list disappeared after
            // a process restart even though synchronization reported success.
            val productEntities = products.map { product ->
                ProductEntity(
                    barcode = product.barcode,
                    code = product.code,
                    title = product.title,
                    category = product.category,
                    desc = product.desc,
                    basePrice = product.basePrice,
                    dealerPrice = product.dealerPrice,
                    wholesalePrice = product.wholesalePrice,
                    kdvPercent = product.kdvPercent,
                    colorValue = product.imageUrlColor.value.toLong(),
                    brand = product.brand,
                    stockByWarehouseJson = converter.fromWarehouseMap(product.stockByWarehouse),
                    boxQty = product.boxQty,
                    packageQty = product.packageQty,
                    imageUrl = product.imageUrl,
                    localImagePath = product.localImagePath,
                    aisle = product.aisle,
                    customPricesJson = converter.fromCustomPricesMap(product.customPrices),
                    barcodesJson = converter.fromBarcodeList(product.barcodes)
                )
            }
            db.productDao().insertAll(productEntities)

            // 5. Save Customers
            val customerEntities = customers.map { cust ->
                CustomerEntity(
                    id = cust.id,
                    name = cust.name,
                    balance = cust.balance,
                    lastVisit = cust.lastVisit,
                    contact = cust.contact,
                    phone = cust.phone,
                    address = cust.address,
                    taxOffice = cust.taxOffice,
                    taxNumber = cust.taxNumber,
                    gpsLocation = cust.gpsLocation,
                    riskLimit = cust.riskLimit,
                    priceGroup = cust.priceGroup,
                    specialDiscountPercent = cust.specialDiscountPercent,
                    transactionsJson = converter.fromCustomerTxList(cust.transactions)
                )
            }
            db.customerDao().deleteAll()
            db.customerDao().insertAll(customerEntities)

            // Trigger cloud background sync according to subscription tier
            try {
                com.example.data.CloudSyncManager.autoSyncOnSave(context)
                if (BridgeSyncHelper.isErpModeActive(context) && BridgeSyncHelper.isOnlineState.value) {
                    BridgeSyncHelper.triggerBackgroundSync(context)
                }
            } catch (se: Throwable) {
                se.printStackTrace()
            }

        } catch (e: Throwable) {
            e.printStackTrace()
        }
    }
}

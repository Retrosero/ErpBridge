package com.example.data.database

import androidx.room.Entity
import androidx.room.PrimaryKey

@Entity(tableName = "users")
@androidx.annotation.Keep
data class UserEntity(
    @PrimaryKey val username: String,
    val passwordHash: String,
    val fullName: String,
    val email: String,
    val isLoggedIn: Boolean = false
)

@Entity(tableName = "customers")
@androidx.annotation.Keep
data class CustomerEntity(
    @PrimaryKey val id: String,
    val name: String,
    val balance: Double,
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
    val transactionsJson: String // Serialized List<CustomerTx>
)

@Entity(tableName = "products")
@androidx.annotation.Keep
data class ProductEntity(
    @PrimaryKey val barcode: String,
    val code: String,
    val title: String,
    val category: String,
    val desc: String,
    val basePrice: Double,
    val dealerPrice: Double,
    val wholesalePrice: Double,
    val kdvPercent: Int,
    val colorValue: Long, // Color converted to Long (Color.value.toLong())
    val brand: String? = null,
    val stockByWarehouseJson: String, // Serialized Map<String, Int>
    val boxQty: Int? = null,
    val packageQty: Int? = null,
    val imageUrl: String? = null,
    val localImagePath: String? = null,
    val aisle: String? = null,
    val customPricesJson: String? = null,
    val barcodesJson: String? = null,
    val measurement: String? = null,
    val packaging: String? = null,
    val cartonQuantity: String? = null
)

@Entity(tableName = "banks")
@androidx.annotation.Keep
data class BankEntity(
    @PrimaryKey val id: String,
    val name: String,
    val accountNo: String,
    val iban: String,
    val balance: Double
)

@Entity(tableName = "kasa_logs")
@androidx.annotation.Keep
data class KasaLogEntity(
    @PrimaryKey val id: String,
    val date: String,
    val type: String,
    val customerOrSupplier: String,
    val amount: Double,
    val paymentType: String,
    val bankName: String?,
    val desc: String
)

@Entity(tableName = "sales_records")
@androidx.annotation.Keep
data class SalesRecordEntity(
    @PrimaryKey(autoGenerate = true) val id: Int = 0,
    val customerId: String,
    val productBarcode: String,
    val quantity: Int,
    val price: Double,
    val date: String
)

@Entity(tableName = "wms_orders")
@androidx.annotation.Keep
data class WmsOrderEntity(
    @PrimaryKey val id: String,
    val customerName: String,
    val orderDate: String,
    val status: String, // "Bekleyen", "Toplanıyor", "Toplandı", "Koli Hazırlandı", "Sevk Edildi"
    val totalItems: Int,
    val trackingNumber: String? = null,
    val packageBarcode: String? = null,
    val vehiclePlate: String? = null,
    val syncStatus: String = "SYNCED" // "LOCAL_PENDING", "SYNCED"
)

@Entity(tableName = "wms_order_items")
@androidx.annotation.Keep
data class WmsOrderItemEntity(
    @PrimaryKey val id: String, // "orderId_barcode"
    val orderId: String,
    val productBarcode: String,
    val productTitle: String,
    val quantityOrdered: Int,
    val quantityPicked: Int,
    val isPicked: Boolean = false,
    val shelfLocation: String? = null, // e.g. "Raf A-3"
    val sth_fat_recid_recno: Int? = null
)

@Entity(tableName = "customer_addresses", primaryKeys = ["customerCode", "addressNo"])
@androidx.annotation.Keep
data class CustomerAddressEntity(
    val customerCode: String,
    val addressNo: Int,
    val city: String?,
    val district: String?,
    val street: String?,
    val postalCode: String?,
    val latitude: Double?,
    val longitude: Double?,
    val salespersonCode: String?
)

@Entity(tableName = "customer_contacts")
@androidx.annotation.Keep
data class CustomerContactEntity(
    @PrimaryKey(autoGenerate = true) val id: Int = 0,
    val customerCode: String,
    val firstName: String?,
    val lastName: String?,
    val email: String?,
    val mobile: String?,
    val tcIdentityNo: String?,
    val taxNo: String?
)

@Entity(tableName = "barcodes")
@androidx.annotation.Keep
data class BarcodeEntity(
    @PrimaryKey val barcode: String,
    val stockCode: String,
    val partCode: String?,
    val lotNo: String?,
    val serialNo: String?,
    val unitPointer: Int
)

@Entity(tableName = "sales_conditions")
@androidx.annotation.Keep
data class SalesConditionEntity(
    @PrimaryKey(autoGenerate = true) val id: Int = 0,
    val stockCode: String?,
    val customerCode: String?,
    val warehouseNo: Int?,
    val paymentPlanNo: Int?,
    val startDate: String?,
    val endDate: String?,
    val grossPrice: Double?,
    val currency: String?,
    val discounts: List<Double>
)

@Entity(tableName = "cari_hareketleri")
@androidx.annotation.Keep
data class CariHareketEntity(
    @PrimaryKey val id: String,
    val customerCode: String,
    val date: String,
    val type: String,
    val amount: Double,
    val description: String,
    val erpRef: String? = null,
    val recNo: String? = null,
    val cha_recno: Int? = null
)

@Entity(tableName = "stock_movements")
@androidx.annotation.Keep
data class StockMovementEntity(
    @PrimaryKey(autoGenerate = true) val id: Long = 0,
    val stockCode: String,
    val date: String,
    val type: String,
    val qty: String,
    val detail: String,
    val user: String,
    val evrakNo: String = "",
    val cariKod: String? = null,
    val cariName: String? = null,
    val unitPrice: Double = 0.0,
    val totalAmount: Double = 0.0,
    val warehouse: String = ""
)

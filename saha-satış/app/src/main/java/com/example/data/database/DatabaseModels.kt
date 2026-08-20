package com.example.data.database

import androidx.room.Entity
import androidx.room.PrimaryKey

@Entity(tableName = "users")
data class UserEntity(
    @PrimaryKey val username: String,
    val passwordHash: String,
    val fullName: String,
    val email: String,
    val isLoggedIn: Boolean = false
)

@Entity(tableName = "customers")
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
    val measurement: String? = null,
    val packaging: String? = null,
    val cartonQuantity: String? = null,
    val customPricesJson: String? = null,
    val barcodesJson: String? = null,
    val transactionsJson: String? = null
)

@Entity(tableName = "banks")
data class BankEntity(
    @PrimaryKey val id: String,
    val name: String,
    val accountNo: String,
    val iban: String,
    val balance: Double
)

@Entity(tableName = "kasa_logs")
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
data class SalesRecordEntity(
    @PrimaryKey(autoGenerate = true) val id: Int = 0,
    val customerId: String,
    val productBarcode: String,
    val quantity: Int,
    val price: Double,
    val date: String
)

@Entity(tableName = "wms_orders")
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

@Entity(tableName = "cari_hesap_hareketleri")
data class CariHesapHareketEntity(
    @PrimaryKey val id: String,
    val cariKod: String,
    val tarih: String,
    val evrakTip: Int,
    val evrakNo: String,
    val tip: Int,
    val tutar: Double,
    val borcMu: Boolean,
    val aciklama: String
)

@Entity(tableName = "stok_hareketleri")
data class StokHareketEntity(
    @PrimaryKey val id: String,
    val stokKod: String,
    val tarih: String,
    val tip: Int,
    val evrakTip: Int,
    val evrakNo: String,
    val miktar: Double,
    val birimFiyat: Double,
    val tutar: Double,
    val cariKod: String,
    val depoNo: Int
)

@Entity(tableName = "bridge_bankalar")
data class BridgeBankaEntity(
    @PrimaryKey val id: String,
    val kod: String,
    val isim: String,
    val sube: String,
    val iban: String,
    val hesapNumarasi: String
)

@Entity(tableName = "cari_adresleri")
data class CariAdresEntity(
    @PrimaryKey val id: String,
    val cariKod: String,
    val adresNo: Int,
    val il: String,
    val ilce: String,
    val mahalle: String,
    val cadde: String,
    val sokak: String
)

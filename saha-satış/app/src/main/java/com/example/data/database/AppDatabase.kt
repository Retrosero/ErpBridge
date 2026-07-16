package com.example.data.database

import androidx.room.Dao
import androidx.room.Database
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import androidx.room.RoomDatabase
import androidx.room.TypeConverters

@Dao
interface UserDao {
    @Query("SELECT * FROM users WHERE username = :username LIMIT 1")
    suspend fun getUserByUsername(username: String): UserEntity?

    @Query("SELECT * FROM users")
    suspend fun getAllUsers(): List<UserEntity>

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertUser(user: UserEntity)

    @Query("SELECT * FROM users WHERE isLoggedIn = 1 LIMIT 1")
    suspend fun getActiveUser(): UserEntity?

    @Query("UPDATE users SET isLoggedIn = 0")
    suspend fun clearSessions()

    @Query("UPDATE users SET isLoggedIn = 1 WHERE username = :username")
    suspend fun markLoggedIn(username: String)
}

@Dao
interface CustomerDao {
    @Query("SELECT * FROM customers")
    suspend fun getAllCustomers(): List<CustomerEntity>
    
    @Query("SELECT * FROM customers")
    fun getAllCustomersFlow(): kotlinx.coroutines.flow.Flow<List<CustomerEntity>>

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertAll(customers: List<CustomerEntity>)

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insert(customer: CustomerEntity)

    @Query("DELETE FROM customers")
    suspend fun deleteAll()
}

@Dao
interface ProductDao {
    @Query("SELECT * FROM products")
    suspend fun getAllProducts(): List<ProductEntity>
    
    @Query("SELECT * FROM products")
    fun getAllProductsFlow(): kotlinx.coroutines.flow.Flow<List<ProductEntity>>

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertAll(products: List<ProductEntity>)

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insert(product: ProductEntity)

    @Query("DELETE FROM products")
    suspend fun deleteAll()
}

@Dao
interface BankDao {
    @Query("SELECT * FROM banks")
    suspend fun getAllBanks(): List<BankEntity>

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertAll(banks: List<BankEntity>)

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insert(bank: BankEntity)

    @Query("DELETE FROM banks")
    suspend fun deleteAll()
}

@Dao
interface KasaLogDao {
    @Query("SELECT * FROM kasa_logs")
    suspend fun getAllKasaLogs(): List<KasaLogEntity>

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertAll(kasaLogs: List<KasaLogEntity>)

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insert(kasaLog: KasaLogEntity)

    @Query("DELETE FROM kasa_logs")
    suspend fun deleteAll()
}

@Dao
interface SalesRecordDao {
    @Query("SELECT * FROM sales_records")
    suspend fun getAllSalesRecords(): List<SalesRecordEntity>

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertAll(salesRecords: List<SalesRecordEntity>)

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insert(salesRecord: SalesRecordEntity)

    @Query("DELETE FROM sales_records")
    suspend fun deleteAll()
}

@Dao
interface WmsOrderDao {
    @Query("SELECT * FROM wms_orders ORDER BY orderDate DESC")
    suspend fun getAllOrders(): List<WmsOrderEntity>

    @Query("SELECT * FROM wms_orders WHERE id = :orderId LIMIT 1")
    suspend fun getOrderById(orderId: String): WmsOrderEntity?

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertAll(orders: List<WmsOrderEntity>)

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insert(order: WmsOrderEntity)

    @Query("UPDATE wms_orders SET status = :status, syncStatus = :syncStatus WHERE id = :orderId")
    suspend fun updateOrderStatus(orderId: String, status: String, syncStatus: String = "LOCAL_PENDING")

    @Query("UPDATE wms_orders SET packageBarcode = :barcode, status = :status, syncStatus = :syncStatus WHERE id = :orderId")
    suspend fun updateOrderPackageBarcode(orderId: String, barcode: String, status: String, syncStatus: String = "LOCAL_PENDING")

    @Query("UPDATE wms_orders SET vehiclePlate = :plate, status = :status, syncStatus = :syncStatus WHERE id = :orderId")
    suspend fun updateOrderVehiclePlate(orderId: String, plate: String, status: String, syncStatus: String = "LOCAL_PENDING")

    @Query("SELECT * FROM wms_orders WHERE syncStatus = 'LOCAL_PENDING'")
    suspend fun getPendingSyncOrders(): List<WmsOrderEntity>

    @Query("DELETE FROM wms_orders")
    suspend fun deleteAll()
}

@Dao
interface WmsOrderItemDao {
    @Query("SELECT * FROM wms_order_items WHERE orderId = :orderId")
    suspend fun getItemsForOrder(orderId: String): List<WmsOrderItemEntity>

    @Query("SELECT * FROM wms_order_items WHERE sth_fat_recid_recno = :recNo")
    suspend fun getItemsByRecNo(recNo: Int): List<WmsOrderItemEntity>

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertAll(items: List<WmsOrderItemEntity>)

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insert(item: WmsOrderItemEntity)

    @Query("UPDATE wms_order_items SET quantityPicked = :picked, isPicked = :isPicked WHERE id = :itemId")
    suspend fun updateItemPickedStatus(itemId: String, picked: Int, isPicked: Boolean)

    @Query("DELETE FROM wms_order_items")
    suspend fun deleteAll()
}

@Database(
    entities = [
        UserEntity::class,
        CustomerEntity::class,
        ProductEntity::class,
        BankEntity::class,
        KasaLogEntity::class,
        SalesRecordEntity::class,
        WmsOrderEntity::class,
        WmsOrderItemEntity::class
    ],
    version = 7,
    exportSchema = false
)
@TypeConverters(Converters::class)
abstract class AppDatabase : RoomDatabase() {
    abstract fun userDao(): UserDao
    abstract fun customerDao(): CustomerDao
    abstract fun productDao(): ProductDao
    abstract fun bankDao(): BankDao
    abstract fun kasaLogDao(): KasaLogDao
    abstract fun salesRecordDao(): SalesRecordDao
    abstract fun wmsOrderDao(): WmsOrderDao
    abstract fun wmsOrderItemDao(): WmsOrderItemDao
}

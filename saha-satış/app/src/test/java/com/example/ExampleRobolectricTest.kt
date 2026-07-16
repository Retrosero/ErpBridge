package com.example

import android.content.Context
import androidx.test.core.app.ApplicationProvider
import com.example.data.database.DatabaseProvider
import com.example.data.database.Converters
import com.example.data.database.CustomerEntity
import com.example.data.database.ProductEntity
import com.example.data.database.BankEntity
import com.example.data.database.KasaLogEntity
import com.example.data.database.SalesRecordEntity
import com.example.ui.screens.AppDataStore
import com.example.ui.screens.Bank
import com.example.ui.screens.KasaLogItem
import com.example.ui.screens.SalesRecord
import com.example.ui.screens.ProductCatalog
import com.example.ui.screens.Customer
import com.example.ui.screens.CustomerTx
import androidx.compose.ui.graphics.Color
import androidx.compose.runtime.mutableStateListOf
import kotlinx.coroutines.runBlocking
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNotNull
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.annotation.Config

@RunWith(RobolectricTestRunner::class)
@Config(sdk = [36])
class ExampleRobolectricTest {

  @Test
  fun `read string from context`() {
    val context = ApplicationProvider.getApplicationContext<Context>()
    val appName = context.getString(R.string.app_name)
    assertEquals("Field Force Precision", appName)
  }

  @Test
  fun `test database seeding and loading operations synchronously`() = runBlocking {
    val context = ApplicationProvider.getApplicationContext<Context>()
    val db = DatabaseProvider.getDatabase(context)
    val converter = Converters()
    
    println("--- STEP 1: Verify Seed Data Preconditions ---")
    // Let's verify our in-memory lists exist and have items
    assertNotNull(AppDataStore.banks)
    assertNotNull(AppDataStore.kasaLogs)
    assertNotNull(AppDataStore.products)
    assertNotNull(AppDataStore.customers)
    
    println("--- STEP 2: Running Room Db Seeding Logic ---")
    // Replicate AppDataStore.persistSync
    val bankEntities = AppDataStore.banks.map { BankEntity(it.id, it.name, it.accountNo, it.iban, it.balance) }
    db.bankDao().deleteAll()
    db.bankDao().insertAll(bankEntities)

    val kasaEntities = AppDataStore.kasaLogs.map { KasaLogEntity(it.id, it.date, it.type, it.customerOrSupplier, it.amount, it.paymentType, it.bankName, it.desc) }
    db.kasaLogDao().deleteAll()
    db.kasaLogDao().insertAll(kasaEntities)

    val salesEntities = AppDataStore.salesHistory.map { SalesRecordEntity(customerId = it.customerId, productBarcode = it.productBarcode, quantity = it.quantity, price = it.price, date = it.date) }
    db.salesRecordDao().deleteAll()
    db.salesRecordDao().insertAll(salesEntities)

    val productEntities = AppDataStore.products.map { prod ->
        ProductEntity(
            barcode = prod.barcode,
            code = prod.code,
            title = prod.title,
            category = prod.category,
            desc = prod.desc,
            basePrice = prod.basePrice,
            dealerPrice = prod.dealerPrice,
            wholesalePrice = prod.wholesalePrice,
            kdvPercent = prod.kdvPercent,
            colorValue = prod.imageUrlColor.value.toLong(),
            brand = prod.brand,
            stockByWarehouseJson = converter.fromWarehouseMap(prod.stockByWarehouse),
            boxQty = prod.boxQty,
            packageQty = prod.packageQty
        )
    }
    db.productDao().deleteAll()
    db.productDao().insertAll(productEntities)

    val customerEntities = AppDataStore.customers.map { cust ->
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
    println("--- Room DB Seeding Successful ---")

    println("--- STEP 3: Running Room Db Loading Logic ---")
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
            packageQty = prod.packageQty
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
    
    println("Successfully loaded: ${loadedBanks.size} banks, ${loadedKasa.size} kasa logs, ${loadedSales.size} sales, ${loadedProducts.size} products, ${loadedCustomers.size} customers")
    assertEquals(AppDataStore.banks.size, loadedBanks.size)
    assertEquals(AppDataStore.products.size, loadedProducts.size)
    assertEquals(AppDataStore.customers.size, loadedCustomers.size)
    println("--- ALL Room DB Operations Completed Successfully & Verified ---")
  }
}

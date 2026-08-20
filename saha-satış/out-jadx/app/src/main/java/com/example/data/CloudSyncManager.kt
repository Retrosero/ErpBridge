package com.example.data

import android.content.Context
import android.util.Log
import androidx.compose.runtime.mutableStateListOf
import com.example.ui.screens.*
import com.google.firebase.firestore.FirebaseFirestore
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.tasks.await
import kotlinx.coroutines.withContext

object CloudSyncManager {

    private const val TAG = "CloudSyncManager"

    /**
     * Uploads the entire local state to Firestore at the specified root path.
     * Perfect for backing up single-user data or synchronizing initial company setup.
     */
    suspend fun uploadAllData(path: String): Boolean {
        val db = FirebaseManager.getFirestore() ?: return false
        return try {
            withContext(Dispatchers.IO) {
                // 1. Sync Customers
                val customersRef = db.collection("$path/customers")
                for (cust in AppDataStore.customers) {
                    val txListRaw = cust.transactions.map { tx ->
                        mapOf(
                            "id" to tx.id,
                            "date" to tx.date,
                            "type" to tx.type,
                            "amount" to tx.amount,
                            "description" to tx.description,
                            "isOffline" to tx.isOffline
                        )
                    }
                    val data = mapOf(
                        "id" to cust.id,
                        "name" to cust.name,
                        "balance" to cust.balance,
                        "lastVisit" to cust.lastVisit,
                        "contact" to cust.contact,
                        "phone" to cust.phone,
                        "address" to cust.address,
                        "taxOffice" to cust.taxOffice,
                        "taxNumber" to cust.taxNumber,
                        "gpsLocation" to cust.gpsLocation,
                        "riskLimit" to cust.riskLimit,
                        "priceGroup" to cust.priceGroup,
                        "specialDiscountPercent" to cust.specialDiscountPercent,
                        "transactions" to txListRaw
                    )
                    customersRef.document(cust.id).set(data).await()
                }

                // 2. Sync Products
                val productsRef = db.collection("$path/products")
                for (prod in AppDataStore.products) {
                    val data = mapOf(
                        "barcode" to prod.barcode,
                        "code" to prod.code,
                        "title" to prod.title,
                        "category" to prod.category,
                        "desc" to prod.desc,
                        "basePrice" to prod.basePrice,
                        "dealerPrice" to prod.dealerPrice,
                        "wholesalePrice" to prod.wholesalePrice,
                        "kdvPercent" to prod.kdvPercent,
                        "colorValue" to prod.imageUrlColor.value.toLong(),
                        "brand" to prod.brand,
                        "boxQty" to prod.boxQty,
                        "packageQty" to prod.packageQty,
                        "imageUrl" to prod.imageUrl,
                        "localImagePath" to prod.localImagePath,
                        "aisle" to prod.aisle,
                        "stockByWarehouse" to prod.stockByWarehouse,
                        "customPrices" to prod.customPrices
                    )
                    productsRef.document(prod.barcode).set(data).await()
                }

                // 3. Sync Banks
                val banksRef = db.collection("$path/banks")
                for (bank in AppDataStore.banks) {
                    val data = mapOf(
                        "id" to bank.id,
                        "name" to bank.name,
                        "accountNo" to bank.accountNo,
                        "iban" to bank.iban,
                        "balance" to bank.balance
                    )
                    banksRef.document(bank.id).set(data).await()
                }

                // 4. Sync Kasa Logs (Cashbox)
                val kasaRef = db.collection("$path/kasa_logs")
                for (log in AppDataStore.kasaLogs) {
                    val data = mapOf(
                        "id" to log.id,
                        "date" to log.date,
                        "type" to log.type,
                        "customerOrSupplier" to log.customerOrSupplier,
                        "amount" to log.amount,
                        "paymentType" to log.paymentType,
                        "bankName" to log.bankName,
                        "desc" to log.desc
                    )
                    kasaRef.document(log.id).set(data).await()
                }

                // 5. Sync Sales History
                val salesRef = db.collection("$path/sales_history")
                for ((index, sale) in AppDataStore.salesHistory.withIndex()) {
                    val docId = "sale_${index}_${sale.customerId}_${sale.productBarcode}"
                    val data = mapOf(
                        "customerId" to sale.customerId,
                        "productBarcode" to sale.productBarcode,
                        "quantity" to sale.quantity,
                        "price" to sale.price,
                        "date" to sale.date
                    )
                    salesRef.document(docId).set(data).await()
                }
            }
            Log.d(TAG, "All data uploaded successfully to cloud path: $path")
            true
        } catch (e: Exception) {
            Log.e(TAG, "Error uploading data to cloud: ${e.message}", e)
            false
        }
    }

    /**
     * Downloads the entire dataset from Firestore at the specified root path and replaces the local DB/state.
     * Essential for corporate real-time alignment and single-user backup restorations.
     */
    suspend fun downloadAndOverwriteData(context: Context, path: String): Boolean {
        val db = FirebaseManager.getFirestore() ?: return false
        return try {
            withContext(Dispatchers.IO) {
                // 1. Fetch Customers
                val customersSnap = db.collection("$path/customers").get().await()
                val loadedCustomers = customersSnap.documents.map { doc ->
                    val txListRaw = doc.get("transactions") as? List<Map<String, Any>> ?: emptyList()
                    val transactions = mutableStateListOf<CustomerTx>().apply {
                        addAll(txListRaw.map { tx ->
                            CustomerTx(
                                id = tx["id"] as? String ?: "",
                                date = tx["date"] as? String ?: "",
                                type = tx["type"] as? String ?: "",
                                amount = (tx["amount"] as? Double) ?: ((tx["amount"] as? Long)?.toDouble() ?: 0.0),
                                description = tx["description"] as? String ?: "",
                                isOffline = tx["isOffline"] as? Boolean ?: false
                            )
                        })
                    }

                    Customer(
                        id = doc.getString("id") ?: doc.id,
                        name = doc.getString("name") ?: "",
                        balance = doc.getDouble("balance") ?: 0.0,
                        lastVisit = doc.getString("lastVisit") ?: "",
                        contact = doc.getString("contact") ?: "",
                        phone = doc.getString("phone") ?: "",
                        address = doc.getString("address") ?: "",
                        taxOffice = doc.getString("taxOffice") ?: "",
                        taxNumber = doc.getString("taxNumber") ?: "",
                        gpsLocation = doc.getString("gpsLocation") ?: "",
                        riskLimit = doc.getDouble("riskLimit") ?: 50000.0,
                        priceGroup = doc.getString("priceGroup") ?: "Bireysel",
                        specialDiscountPercent = doc.getDouble("specialDiscountPercent") ?: 0.0,
                        transactions = transactions
                    )
                }

                // 2. Fetch Products
                val productsSnap = db.collection("$path/products").get().await()
                val loadedProducts = productsSnap.documents.map { doc ->
                    val colorVal = doc.getLong("colorValue") ?: 0xFF1976D2
                    ProductCatalog(
                        barcode = doc.getString("barcode") ?: doc.id,
                        code = doc.getString("code") ?: "",
                        title = doc.getString("title") ?: "",
                        category = doc.getString("category") ?: "Genel",
                        desc = doc.getString("desc") ?: "",
                        basePrice = doc.getDouble("basePrice") ?: 0.0,
                        dealerPrice = doc.getDouble("dealerPrice") ?: 0.0,
                        wholesalePrice = doc.getDouble("wholesalePrice") ?: 0.0,
                        kdvPercent = doc.getLong("kdvPercent")?.toInt() ?: 20,
                        imageUrlColor = androidx.compose.ui.graphics.Color(colorVal.toULong()),
                        brand = doc.getString("brand"),
                        stockByWarehouse = (doc.get("stockByWarehouse") as? Map<String, Any>)?.mapValues { (it.value as? Long)?.toInt() ?: 0 } ?: emptyMap(),
                        boxQty = doc.getLong("boxQty")?.toInt(),
                        packageQty = doc.getLong("packageQty")?.toInt(),
                        imageUrl = doc.getString("imageUrl"),
                        localImagePath = doc.getString("localImagePath"),
                        aisle = doc.getString("aisle"),
                        customPrices = (doc.get("customPrices") as? Map<String, Any>)?.mapValues { (it.value as? Double) ?: ((it.value as? Long)?.toDouble() ?: 0.0) } ?: emptyMap()
                    )
                }

                // 3. Fetch Banks
                val banksSnap = db.collection("$path/banks").get().await()
                val loadedBanks = banksSnap.documents.map { doc ->
                    Bank(
                        id = doc.getString("id") ?: doc.id,
                        name = doc.getString("name") ?: "",
                        accountNo = doc.getString("accountNo") ?: "",
                        iban = doc.getString("iban") ?: "",
                        balance = doc.getDouble("balance") ?: 0.0
                    )
                }

                // 4. Fetch Kasa Logs
                val kasaSnap = db.collection("$path/kasa_logs").get().await()
                val loadedKasa = kasaSnap.documents.map { doc ->
                    KasaLogItem(
                        id = doc.getString("id") ?: doc.id,
                        date = doc.getString("date") ?: "",
                        type = doc.getString("type") ?: "Bilgi",
                        customerOrSupplier = doc.getString("customerOrSupplier") ?: "",
                        amount = doc.getDouble("amount") ?: 0.0,
                        paymentType = doc.getString("paymentType") ?: "Nakit",
                        bankName = doc.getString("bankName"),
                        desc = doc.getString("desc") ?: ""
                    )
                }

                // 5. Fetch Sales History
                val salesSnap = db.collection("$path/sales_history").get().await()
                val loadedSales = salesSnap.documents.map { doc ->
                    SalesRecord(
                        customerId = doc.getString("customerId") ?: "",
                        productBarcode = doc.getString("productBarcode") ?: "",
                        quantity = doc.getLong("quantity")?.toInt() ?: 0,
                        price = doc.getDouble("price") ?: 0.0,
                        date = doc.getString("date") ?: ""
                    )
                }

                // Replace local state with fetched items
                withContext(Dispatchers.Main) {
                    if (loadedCustomers.isNotEmpty() || loadedProducts.isNotEmpty()) {
                        AppDataStore.customers.clear()
                        AppDataStore.customers.addAll(loadedCustomers)

                        AppDataStore.products.clear()
                        AppDataStore.products.addAll(loadedProducts)

                        AppDataStore.banks.clear()
                        AppDataStore.banks.addAll(loadedBanks)

                        AppDataStore.kasaLogs.clear()
                        AppDataStore.kasaLogs.addAll(loadedKasa)

                        AppDataStore.salesHistory.clear()
                        AppDataStore.salesHistory.addAll(loadedSales)
                    }
                }

                // Persist locally so it caches beautifully and acts offline-first
                AppDataStore.persist(context)
            }
            Log.d(TAG, "All data downloaded successfully and replaced state from cloud path: $path")
            true
        } catch (e: Exception) {
            Log.e(TAG, "Error downloading and overwriting state: ${e.message}", e)
            false
        }
    }

    /**
     * Automatic trigger called when data is saved. Syncs data in real-time or background
     * depending on the active subscription package.
     */
    suspend fun autoSyncOnSave(context: Context) {
        if (!FirebaseManager.isInitialized) return
        
        when (AppDataStore.subscriptionPack) {
            "backup" -> {
                val path = getPersonalBackupPath()
                if (path.isNotBlank()) {
                    Log.d(TAG, "Auto-Backup: Saving copy to cloud backup bucket: $path")
                    uploadAllData(path)
                }
            }
            "company" -> {
                val companyId = AppDataStore.companyId.trim().lowercase()
                if (companyId.isNotBlank() && companyId != "demo") {
                    val path = "companies/$companyId"
                    Log.d(TAG, "Auto-Sync: Updating corporate shared store: $path")
                    uploadAllData(path)
                }
            }
            else -> {
                // Standart Paket is offline/local only: Do nothing in the cloud!
                Log.d(TAG, "Standart Paket: Local Only - Data is completely offline.")
            }
        }
    }

    /**
     * Triggers complete data load/alignment when the application starts or package settings change.
     */
    suspend fun autoPullOnStartup(context: Context) {
        if (!FirebaseManager.isInitialized) return
        
        if (AppDataStore.subscriptionPack == "company") {
            val companyId = AppDataStore.companyId.trim().lowercase()
            if (companyId.isNotBlank() && companyId != "demo") {
                val path = "companies/$companyId"
                Log.d(TAG, "Startup Sync: Pulling down latest team data for company: $companyId")
                downloadAndOverwriteData(context, path)
            }
        }
    }

    fun getPersonalBackupPath(): String {
        val cleanLicense = AppDataStore.licenseKey.trim().uppercase()
        return if (cleanLicense.isNotBlank()) {
            "backups/$cleanLicense"
        } else {
            "backups/DEMO-BACKUP"
        }
    }
}

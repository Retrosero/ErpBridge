package com.example.sync

import android.content.Context
import androidx.test.core.app.ApplicationProvider
import androidx.test.ext.junit.runners.AndroidJUnit4
import com.example.data.api.*
import com.example.ui.screens.BridgeSyncHelper
import com.example.util.SyncManager
import com.example.util.SyncTask
import com.example.ui.screens.AppDataStore
import kotlinx.coroutines.test.runTest
import org.junit.After
import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Before
import org.junit.Test
import org.junit.runner.RunWith
import retrofit2.Response
import java.io.IOException
import java.lang.reflect.Proxy
import java.lang.reflect.InvocationHandler
import com.example.data.database.DatabaseProvider

@RunWith(AndroidJUnit4::class)
class PaginatedSyncTest {

    private lateinit var context: Context
    private var callCount = 0

    @Before
    fun setup() {
        context = ApplicationProvider.getApplicationContext()
        callCount = 0
        
        val handler = InvocationHandler { _, method, args ->
            if (method.name == "getCariHareketleri") {
                callCount++
                val req = args[0] as PullJobsRequest
                val page = req.page ?: 1
                val pageSize = req.pageSize ?: 500
                
                // Simulating 80,000 records, 500 per page = 160 pages.
                val totalRecords = 80000
                
                if (page > 160) {
                    return@InvocationHandler Response.success(CariHareketResponse(entity="cariHareketleri", cariKod="", page=page, pageSize=pageSize, total=totalRecords, since=null, items = emptyList()))
                }
                
                val items = List(pageSize) { idx ->
                    val globalIdx = (page - 1) * pageSize + idx
                    CariHareketiDto(
                        id = "TX-$globalIdx",
                        cariKod = "C1",
                        tarih = "2026-08-11",
                        tutar = 100.0,
                        aciklama = "Test $globalIdx",
                        erpRef = "", erp = "", evrakTip = 0, evrakNo = "", tip = 0, borcMu = true, updatedAt = ""
                    )
                }
                
                return@InvocationHandler Response.success(CariHareketResponse(entity="cariHareketleri", cariKod="", page=page, pageSize=pageSize, total=totalRecords, since=null, items = items))
            }
            if (method.name == "getFaturaHareket") {
                callCount++
                val req = args[0] as PullJobsRequest
                val page = req.page ?: 1
                val pageSize = req.pageSize ?: 500
                
                // Simulating 2,000 records for fast test
                val totalRecords = 2000
                
                if (page > 4) {
                    return@InvocationHandler Response.success(FaturaHareketResponse(
                        entity = "faturaHareket",
                        cariKod = "",
                        page = page,
                        pageSize = pageSize,
                        total = totalRecords,
                        since = null,
                        items = emptyList()
                    ))
                }
                
                val items = List(pageSize) { idx ->
                    val globalIdx = (page - 1) * pageSize + idx
                    FaturaHareketDto(
                        erpRef = "ERP-$globalIdx",
                        erp = "ERP",
                        cariKod = "C1",
                        evrakNo = "FT-$globalIdx",
                        tarih = "2026", evrakTip = 0, tip = 0, tutar = 50.0, updatedAt = "",
                        satirlar = listOf(FaturaSatirDto(erpRef = "", stokKod = "STK-1", miktar = 1.0, tutar = 50.0, tip = 1, cins = 1, tarih = "2026", girisMiktar = 0.0, cikisMiktar = 0.0, birimFiyat = 50.0, vergi = 0.0, girisDepoNo = 1, cikisDepoNo = 1, aciklama = "", updatedAt = ""))
                    )
                }
                
                return@InvocationHandler Response.success(FaturaHareketResponse(
                    entity = "faturaHareket",
                    cariKod = "",
                    page = page,
                    pageSize = pageSize,
                    total = totalRecords,
                    since = null,
                    items = items
                ))
            }
            null
        }
        
        val proxy = Proxy.newProxyInstance(
            FieldOpsApiService::class.java.classLoader,
            arrayOf(FieldOpsApiService::class.java),
            handler
        ) as FieldOpsApiService
        
        ApiClient.testingApiService = proxy
        
        AppDataStore.customers.clear()
        AppDataStore.customers.add(com.example.ui.screens.Customer(
            id = "C1", name = "Test Customer", balance = 0.0, lastVisit = "", contact = "", phone = "", address = "",
            taxOffice = "", taxNumber = "", gpsLocation = "", riskLimit = 0.0, priceGroup = "", specialDiscountPercent = 0.0,
            transactions = mutableListOf()
        ))
    }

    @After
    fun teardown() {
        ApiClient.testingApiService = null
        context.getSharedPreferences("erp_settings", Context.MODE_PRIVATE).edit().clear().apply()
    }

    @Test
    fun testPaginatedCariHareketleri_80k() = runTest {
        // Fast test for pagination logic
        BridgeSyncHelper.syncCariHareketleri(context, "http://test", "key", {}, {})
        
        // 160 pages + 1 empty page = 161 calls
        assertEquals(161, callCount)
        
        // Verify customer has 80,000 transactions (in memory simulation)
        val customer = AppDataStore.customers.find { it.id == "C1" }
        assertEquals(80000, customer?.transactions?.size)
        
        val db = DatabaseProvider.getDatabase(context)
        val allCustomersInDb = db.customerDao().getAllCustomers()
        // Wait, because we are using test DB or real context? 
        // In Robolectric it uses in-memory or actual test db.
        // The DB might not be synchronous for getAllCustomers but we check memory.
    }
    
    @Test
    fun testPaginatedFaturaHareket_2k() = runTest {
        BridgeSyncHelper.syncFaturaHareket(context, "http://test", "key", {}, {})
        
        // 4 pages + 1 empty page = 5 calls
        assertEquals(5, callCount)
    }
}

package com.example

import com.example.data.api.ApiClient
import kotlinx.coroutines.runBlocking
import org.junit.Test

class ExampleUnitTest {
    @Test
    fun addition_isCorrect() {
        org.junit.Assert.assertEquals(4, 2 + 2)
    }

    @Test
    fun checkUserBridgeUrl() = runBlocking {
        println("--- START CHECKING USER BRIDGE URL ---")
        val apiUrl = "https://ipaq-phrases-airplane-lanka.trycloudflare.com"
        
        // Try empty token, the default dev token, and other possible configurations
        val tokensToTry = listOf("", "dev-token-change-in-production", "bizim_token_8829911")
        
        for (token in tokensToTry) {
            println("\nTesting with token: '$token' ...")
            try {
                val apiService = ApiClient.getFieldOpsApiService(apiUrl, token)
                println("Connecting to $apiUrl/api/v1/sync/cari ...")
                val response = apiService.getCariler(pageSize = 100)
                
                println("Response Code: ${response.code()}")
                if (response.isSuccessful) {
                    val body = response.body()
                    if (body != null) {
                        println("CONNECTION SUCCESSFUL!")
                        println("Total items fetched in page: ${body.items.size}")
                        if (body.items.isNotEmpty()) {
                            println("First 10 customers:")
                            body.items.take(10).forEach { cari ->
                                println(" -> Kod: ${cari.actualCariKod}, Ünvan: ${cari.actualCariUnvan}, Ref: ${cari.erpRef}")
                            }
                        } else {
                            println("The customer (cari) list is empty.")
                        }
                    } else {
                        println("Response body is null!")
                    }
                    break
                } else {
                    println("Request failed. Code: ${response.code()}")
                    val errString = response.errorBody()?.string()
                    println("Error body: ${errString ?: "no error body"}")
                }
            } catch (e: Exception) {
                println("Exception for token '$token': ${e.message}")
                e.printStackTrace()
            }
        }
        println("--- END CHECKING USER BRIDGE URL ---")
    }

    @Test
    fun debugErpApiGokhanKaya() = runBlocking {
        println("--- START API DEBUG FOR GÖKHAN KAYA ---")
        val apiUrl = "https://d5e4-88-248-2-49.ngrok-free.app"
        val apiKey = "dev-token-change-in-production"
        
        try {
            val apiService = ApiClient.getFieldOpsApiService(apiUrl, apiKey)
            println("Fetching all customers (cariler)...")
            val carilerRes = apiService.getCariler(pageSize = 1000)
            if (!carilerRes.isSuccessful || carilerRes.body() == null) {
                println("Could not fetch cariler. Code: ${carilerRes.code()}, Body: ${carilerRes.errorBody()?.string()}")
                return@runBlocking
            }
            
            val customers = carilerRes.body()!!.items
            println("Total customers fetched: ${customers.size}")
            
            val matches = customers.filter { 
                it.unvan?.contains("Kaya", ignoreCase = true) == true || 
                it.unvan?.contains("Gökhan", ignoreCase = true) == true || 
                it.unvan?.contains("Gokhan", ignoreCase = true) == true 
            }
            println("All matching customers:")
            matches.forEach { println(" -> ID: ${it.id}, Kod: ${it.erpKod}, Ünvan: ${it.unvan}, Ref: ${it.erpRef}") }
            
            val gokhan = matches.find { 
                (it.unvan?.contains("Gökhan", ignoreCase = true) == true || it.unvan?.contains("Gokhan", ignoreCase = true) == true) &&
                it.unvan?.contains("Kaya", ignoreCase = true) == true 
            } ?: matches.firstOrNull()
            if (gokhan == null) {
                println("Could not find Gökhan Kaya or any close matches in customers list!")
                println("First 5 customers:")
                customers.take(5).forEach { println(" - ID: ${it.id}, Kod: ${it.erpKod}, Ünvan: ${it.unvan}") }
                return@runBlocking
            }
            
            println("FOUND TARGET CUSTOMER: ID: ${gokhan.id}, Kod: ${gokhan.erpKod}, Ünvan: ${gokhan.unvan}, Ref: ${gokhan.erpRef}")
            
            var txs = emptyList<com.example.data.api.CariHareketiDto>()
            val keysToTryForTx = listOfNotNull(gokhan.id, gokhan.erpKod, gokhan.erpRef)
            for (key in keysToTryForTx) {
                println("\nTrying to fetch transactions (cariHareket) for key: $key ...")
                val res = apiService.getCariHareket(cariKod = key, pageSize = 50)
                if (res.isSuccessful && res.body() != null) {
                    val items = res.body()!!.items
                    println("SUCCESS WITH cariHareket on key $key! Fetched size: ${items.size}")
                    if (items.isNotEmpty()) {
                        txs = items
                        break
                    }
                } else {
                    println("FAILED with cariHareket on key $key. Code: ${res.code()}")
                }
            }
            if (txs.isEmpty()) {
                for (key in keysToTryForTx) {
                    println("\nTrying to fetch transactions (cariHareketleri) for key: $key ...")
                    val res = apiService.getCariHareketleri(cariKod = key, pageSize = 50)
                    if (res.isSuccessful && res.body() != null) {
                        val items = res.body()!!.items
                        println("SUCCESS WITH cariHareketleri on key $key! Fetched size: ${items.size}")
                        if (items.isNotEmpty()) {
                            txs = items
                            break
                        }
                    } else {
                        println("FAILED with cariHareketleri on key $key. Code: ${res.code()}")
                    }
                }
            }
            
            if (txs.isEmpty()) {
                println("Could not fetch transactions with any key! Attempting empty/all-customer fetch...")
                val res = apiService.getCariHareketleri(cariKod = null, pageSize = 50)
                if (res.isSuccessful && res.body() != null) {
                    txs = res.body()!!.items.filter { it.cariKod == gokhan.id || it.cariKod == gokhan.erpKod || it.cariKod == gokhan.erpRef }
                    println("SUCCESS WITH general cariHareketleri query! Filtered matches for customer: ${txs.size}")
                }
            }
            
            val salesTxs = txs.filter { it.evrakTip == 29 || it.tip == 0 || it.borcMu == true }.take(5)
            println("\n--- DETECTED SALES INVOICES ---")
            salesTxs.forEach { tx ->
                println("Tx ID: ${tx.id}, EvrakNo: ${tx.evrakNo}, EvrakTip: ${tx.evrakTip}, Tip: ${tx.tip}, BorcMu: ${tx.borcMu}, Tutar: ${tx.tutar}, cha_recno: ${tx.cha_recno}, Aciklama: ${tx.aciklama}")
            }
            
            var invoices = emptyList<com.example.data.api.FaturaHareketDto>()
            for (key in keysToTryForTx) {
                println("\nFetching invoice detail/satirlar (faturaHareket) for key: $key ...")
                val fatRes = apiService.getFaturaHareket(cariKod = key, page = 1, pageSize = 50)
                if (fatRes.isSuccessful && fatRes.body() != null) {
                    val items = fatRes.body()!!.items
                    println("SUCCESS with faturaHareket on key $key! Invoices size: ${items.size}")
                    if (items.isNotEmpty()) {
                        invoices = items
                        break
                    }
                } else {
                    println("FAILED with faturaHareket on key $key. Code: ${fatRes.code()}")
                }
            }
            println("Total invoices fetched from faturaHareket: ${invoices.size}")
            
            invoices.forEach { f ->
                println("\nInvoice EvrakNo: ${f.evrakNo}, erpRef: ${f.erpRef}, Tarih: ${f.tarih}, Tutar: ${f.tutar}")
                f.satirlar?.forEach { s ->
                    println("  -> Line: StokKod: ${s.stokKod}, StokAd: ${s.stokAd ?: "N/A"}, Miktar: ${s.miktar}, Tutar: ${s.tutar}, sth_fat_recid_recno: ${s.sth_fat_recid_recno}")
                }
            }
            
            println("\n--- VERIFICATION MATCHING LOGIC ---")
            salesTxs.forEach { tx ->
                println("\nFinding details for Tx cha_recno=[${tx.cha_recno}], EvrakNo=[${tx.evrakNo}] using cha_recno matching:")
                var matched = false
                
                // Match 1: Using cha_recno
                if (tx.cha_recno != null) {
                    val matchingInvoice = invoices.find { f ->
                        f.satirlar?.any { it.sth_fat_recid_recno == tx.cha_recno } == true
                    }
                    if (matchingInvoice != null) {
                        println("  [MATCH FOUND BY cha_recno]: Invoice EvrakNo: ${matchingInvoice.evrakNo}")
                        val targetLines = matchingInvoice.satirlar?.filter { it.sth_fat_recid_recno == tx.cha_recno } ?: emptyList()
                        targetLines.forEach { s ->
                            println("    - ${s.stokAd} (Kod: ${s.stokKod}) x ${s.miktar} | sth_fat_recid_recno: ${s.sth_fat_recid_recno}")
                        }
                        matched = true
                    }
                }
                
                if (!matched) {
                    println("  [NO MATCH BY cha_recno] Trying fallback by EvrakNo/ERPRef...")
                    val matchingInvoice = invoices.find { f ->
                        val rawEvrak = f.evrakNo ?: ""
                        f.erpRef == tx.erpRef || rawEvrak == tx.evrakNo || tx.aciklama?.contains(rawEvrak) == true
                    }
                    if (matchingInvoice != null) {
                        println("  [MATCH FOUND BY FALLBACK]: Invoice EvrakNo: ${matchingInvoice.evrakNo}")
                        matchingInvoice.satirlar?.forEach { s ->
                            println("    - ${s.stokAd} (Kod: ${s.stokKod}) x ${s.miktar} | sth_fat_recid_recno: ${s.sth_fat_recid_recno}")
                        }
                    } else {
                        println("  [NO MATCH FOUND AT ALL] for this transaction!")
                    }
                }
            }
            
        } catch (e: Exception) {
            println("Exception inside debug test: ${e.message}")
            e.printStackTrace()
        }
    }
}


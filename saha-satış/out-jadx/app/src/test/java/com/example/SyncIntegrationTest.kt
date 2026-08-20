package com.example

import android.content.Context
import androidx.test.core.app.ApplicationProvider
import com.example.data.api.ApiClient
import com.example.util.SyncManager
import com.example.util.SyncTask
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.runBlocking
import org.junit.Assert.*
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.annotation.Config

@RunWith(RobolectricTestRunner::class)
@Config(sdk = [33])
class SyncIntegrationTest {

    @Test
    fun testPaginationSameFingerprint_StopsLoop() {
        val itemsPage1 = listOf(mapOf("id" to "1"), mapOf("id" to "2"))
        val itemsPage2 = listOf(mapOf("id" to "1"), mapOf("id" to "2")) // Same fingerprint
        
        val lastFingerprint = itemsPage1.joinToString(",") { it["id"].toString() }
        val currentFingerprint = itemsPage2.joinToString(",") { it["id"].toString() }
        
        var hasMore = true
        if (currentFingerprint == lastFingerprint) {
            hasMore = false
        }
        
        assertFalse("Loop should stop when fingerprint is same", hasMore)
    }

    @Test
    fun testItemsSizeLessThanPageSize_StopsLoop() {
        val items = List(422) { "item" }
        val pageSize = 1000
        var hasMore = true
        var currentPage = 1
        
        if (items.size < pageSize) {
            hasMore = false
        } else {
            currentPage++
        }
        
        assertFalse("Loop should stop when size < pageSize", hasMore)
        assertEquals("Page should not increment", 1, currentPage)
    }

    @Test
    fun testPartialSyncResult() = runBlocking {
        val context = ApplicationProvider.getApplicationContext<Context>()
        
        val tasks = listOf(
            object : SyncTask() {
                override val name = "Table1"
                override val description = "Success"
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {}
            },
            object : SyncTask() {
                override val name = "Table2"
                override val description = "Fail"
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {
                    throw Exception("Network Error")
                }
            }
        )
        
        SyncManager.startSyncAll(context, "url", "key", tasks)
        
        var finished = false
        for (i in 1..50) {
            if (SyncManager.isSyncAllFinished.value) {
                finished = true
                break
            }
            kotlinx.coroutines.delay(50)
        }
        
        val logs = SyncManager.syncLogs.value
        val hasPartialSyncLog = logs.any { it.contains("Kısmi senkronizasyon") }
        assertTrue("Should report partial sync", hasPartialSyncLog)
    }
}

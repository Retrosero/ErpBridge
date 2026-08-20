package com.example

import android.content.Context
import androidx.test.core.app.ApplicationProvider
import com.example.util.SyncManager
import com.example.util.SyncTask
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.runBlocking
import org.junit.Assert.assertTrue
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.annotation.Config

@RunWith(RobolectricTestRunner::class)
@Config(sdk = [33])
class FieldOpsSyncTest {

    @Test
    fun testPartialSyncResult() = runBlocking {
        val context = ApplicationProvider.getApplicationContext<Context>()
        
        val tasks = listOf(
            object : SyncTask() {
                override val name = "SuccessTask"
                override val description = "Success"
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {
                    log("Success")
                }
            },
            object : SyncTask() {
                override val name = "FailTask"
                override val description = "Fail"
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {
                    throw Exception("Network Error")
                }
            }
        )
        
        SyncManager.startSyncAll(context, "url", "key", tasks)
        
        // Wait for sync to finish
        var finished = false
        for (i in 1..50) {
            if (SyncManager.isSyncAllFinished.value) {
                finished = true
                break
            }
            kotlinx.coroutines.delay(100)
        }
        
        assertTrue("Sync should be finished", finished)
        
        val logs = SyncManager.syncLogs.value
        val hasPartialSyncLog = logs.any { it.contains("Kısmi senkronizasyon tamamlandı") }
        val hasFailedTableLog = logs.any { it.contains("FailTask (Network Error)") }
        
        assertTrue("Should report partial sync", hasPartialSyncLog)
        assertTrue("Should list the failed table", hasFailedTableLog)
    }
}

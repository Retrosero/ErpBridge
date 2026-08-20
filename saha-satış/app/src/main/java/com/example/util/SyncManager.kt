package com.example.util

import android.content.Context
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import com.example.ui.screens.BridgeSyncHelper

abstract class SyncTask {
    abstract val name: String
    abstract val description: String
    abstract suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit)
}

object SyncManager {
    private val scope = CoroutineScope(Dispatchers.IO + SupervisorJob())

    private val _isSyncing = MutableStateFlow(false)
    val isSyncing: StateFlow<Boolean> = _isSyncing.asStateFlow()

    private val _isSyncAllFinished = MutableStateFlow(false)
    val isSyncAllFinished: StateFlow<Boolean> = _isSyncAllFinished.asStateFlow()

    private val _currentSyncTaskIndex = MutableStateFlow(-1)
    val currentSyncTaskIndex: StateFlow<Int> = _currentSyncTaskIndex.asStateFlow()

    private val _currentSyncTaskName = MutableStateFlow("")
    val currentSyncTaskName: StateFlow<String> = _currentSyncTaskName.asStateFlow()

    private val _syncProgress = MutableStateFlow(0f)
    val syncProgress: StateFlow<Float> = _syncProgress.asStateFlow()
    private val _currentSyncStats = MutableStateFlow("")
    val currentSyncStats: StateFlow<String> = _currentSyncStats.asStateFlow()

    fun updateSyncStats(stats: String) {
        _currentSyncStats.value = stats
    }

    private val _syncLogs = MutableStateFlow<List<String>>(emptyList())
    val syncLogs: StateFlow<List<String>> = _syncLogs.asStateFlow()

    private val _syncTasks = MutableStateFlow<List<SyncTask>>(emptyList())
    val syncTasks: StateFlow<List<SyncTask>> = _syncTasks.asStateFlow()

    fun log(msg: String) {
        val currentLogs = _syncLogs.value.toMutableList()
        currentLogs.add(0, "[${java.text.SimpleDateFormat("HH:mm:ss").format(java.util.Date())}] $msg")
        _syncLogs.value = currentLogs
    }

    fun resetSyncState() {
        _isSyncing.value = false
        _isSyncAllFinished.value = false
        _syncLogs.value = emptyList()
        _currentSyncTaskIndex.value = -1
    }

    fun startSyncAll(context: Context, apiUrl: String, apiKey: String, tasks: List<SyncTask>) {
        if (_isSyncing.value) return
        
        _syncTasks.value = tasks
        scope.launch {
            _isSyncing.value = true
            _isSyncAllFinished.value = false
            _syncLogs.value = emptyList()
            log("Toplu entegrasyon başlatılıyor...")
            
            val successfulTasks = mutableListOf<String>()
            val failedTasks = mutableListOf<String>()
            
            for (idx in tasks.indices) {
                val task = tasks[idx]
                _currentSyncTaskIndex.value = idx
                _currentSyncTaskName.value = task.name
                _syncProgress.value = 0f
                
                log("• [${idx + 1}/${tasks.size}] ${task.name} senkronizasyonu başladı...")
                try {
                    task.execute(
                        ctx = context,
                        url = apiUrl,
                        key = apiKey,
                        log = { msg -> log("  -> $msg") },
                        progress = { p -> _syncProgress.value = p }
                    )
                    log("✅ ${task.name} başarıyla kopyalandı.")
                    successfulTasks.add(task.name)
                } catch (e: Exception) {
                    log("⚠️ ${task.name} aktarılırken hata: ${e.message}")
                    failedTasks.add("${task.name} (${e.message})")
                }
                delay(1000) // Small delay between tasks
            }
            
            _isSyncing.value = false
            _isSyncAllFinished.value = true
            _currentSyncTaskIndex.value = -1
            
            if (failedTasks.isEmpty()) {
                log("🎉 Toplu entegrasyon tamamlandı!")
            } else {
                log("⚠️ Kısmi senkronizasyon tamamlandı.")
                log("Başarısız tablolar:")
                failedTasks.forEach { log("- $it") }
            }
            
            kotlinx.coroutines.withContext(kotlinx.coroutines.Dispatchers.Main) {
                com.example.ui.screens.AppDataStore.persist(context)
            }
        }
    }
}

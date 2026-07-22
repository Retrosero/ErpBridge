package com.example.data

import kotlinx.coroutines.tasks.await

object LicenseManager {
    var maxProductsLimitCache: Int = 50

    suspend fun validateLicenseKeyOnline(key: String): Boolean {
        if (key == "DEMO-123") {
            maxProductsLimitCache = 50
            return true
        }

        val db = FirebaseManager.getFirestore()
        if (db == null) {
            maxProductsLimitCache = getFallbackProductLimit(key)
            return parseLicenseKey(key)
        }

        return try {
            val document = db.collection("licenses").document(key).get().await()
            if (document.exists() && document.getBoolean("isActive") == true) {
                maxProductsLimitCache = document.getLong("maxProducts")?.toInt() ?: getFallbackProductLimit(key)
                true
            } else {
                false
            }
        } catch (e: Exception) {
            e.printStackTrace()
            maxProductsLimitCache = getFallbackProductLimit(key)
            parseLicenseKey(key)
        }
    }

    fun parseLicenseKey(key: String): Boolean {
        return key.startsWith("TIER1") || key.startsWith("TIER2") || key.startsWith("TIER3") || key.startsWith("UNLIMITED")
    }

    private fun getFallbackProductLimit(key: String?): Int {
        if (key.isNullOrBlank()) return 50 // Default trial limit
        if (key.startsWith("TIER1")) return 100
        if (key.startsWith("TIER2")) return 500
        if (key.startsWith("TIER3")) return 1000
        if (key.startsWith("UNLIMITED")) return Int.MAX_VALUE
        return 50 // Invalid goes back to 50
    }

    fun getProductLimit(key: String?): Int {
        if (FirebaseManager.isInitialized) {
            return maxProductsLimitCache
        }
        return getFallbackProductLimit(key)
    }

    fun canAddMoreProducts(currentCount: Int, key: String?): Boolean {
        return currentCount < getProductLimit(key)
    }
}

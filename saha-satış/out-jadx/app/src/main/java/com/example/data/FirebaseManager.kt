package com.example.data

import android.content.Context
import android.util.Log
import com.example.BuildConfig
import com.google.firebase.FirebaseApp
import com.google.firebase.FirebaseOptions
import com.google.firebase.firestore.FirebaseFirestore

object FirebaseManager {
    var isInitialized = false

    fun initialize(context: Context) {
        if (isInitialized) return

        try {
            // First check if already initialized (e.g., via google-services.json)
            FirebaseApp.getInstance()
            isInitialized = true
            Log.d("FirebaseManager", "Firebase already initialized via google-services.json")
            return
        } catch (e: IllegalStateException) {
            // Not initialized yet
        }

        val projectId = BuildConfig.FIREBASE_PROJECT_ID
        val appId = BuildConfig.FIREBASE_APP_ID
        val apiKey = BuildConfig.FIREBASE_API_KEY

        val hasValidSecrets = projectId.isNotBlank() && !projectId.contains("_DEFAULT") &&
                appId.isNotBlank() && !appId.contains("_DEFAULT") &&
                apiKey.isNotBlank() && !apiKey.contains("_DEFAULT")

        if (hasValidSecrets) {
            try {
                val options = FirebaseOptions.Builder()
                    .setProjectId(projectId)
                    .setApplicationId(appId)
                    .setApiKey(apiKey)
                    .build()
                
                FirebaseApp.initializeApp(context, options)
                isInitialized = true
                Log.d("FirebaseManager", "Firebase initialized manually via Secrets")
            } catch (e: Exception) {
                Log.e("FirebaseManager", "Firebase manual initialization failed: ${e.message}", e)
            }
        } else {
            Log.i("FirebaseManager", "Firebase credentials not provided. Running in local mode. Configure them in AI Studio Secrets if cloud sync is required.")
        }
    }

    fun getFirestore(): FirebaseFirestore? {
        return if (isInitialized) {
            FirebaseFirestore.getInstance()
        } else {
            null
        }
    }

    // Example synchronization for Multi-Tenant Data Setup
    fun syncProductsToCloud(licenseKey: String, products: List<com.example.ui.screens.ProductCatalog>) {
        val db = getFirestore() ?: return
        if (licenseKey.isBlank() || licenseKey == "DEMO-123") return

        val batch = db.batch()
        val productsRef = db.collection("tenants").document(licenseKey).collection("products")
        
        for (product in products) {
            val docRef = productsRef.document(product.barcode)
            // Use Moshi/manual map for actual objects, here we use a simple map for demo
            val data = mapOf(
                "title" to product.title,
                "barcode" to product.barcode,
                "basePrice" to product.basePrice,
                "stockByWarehouse" to product.stockByWarehouse
            )
            batch.set(docRef, data)
        }
        
        batch.commit()
            .addOnSuccessListener { Log.d("FirebaseManager", "Products synced for tenant: $licenseKey") }
            .addOnFailureListener { e -> Log.e("FirebaseManager", "Error syncing products", e) }
    }
}

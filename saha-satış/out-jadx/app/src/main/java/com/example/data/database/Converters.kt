package com.example.data.database

import androidx.room.TypeConverter
import com.squareup.moshi.Moshi
import com.squareup.moshi.Types
import com.squareup.moshi.kotlin.reflect.KotlinJsonAdapterFactory
import com.example.ui.screens.CustomerTx

class Converters {
    private val moshi = Moshi.Builder()
        .addLast(KotlinJsonAdapterFactory())
        .build()

    @TypeConverter
    fun fromCustomerTxList(value: List<CustomerTx>?): String {
        if (value == null) return "[]"
        val type = Types.newParameterizedType(List::class.java, CustomerTx::class.java)
        val adapter = moshi.adapter<List<CustomerTx>>(type)
        return adapter.toJson(value)
    }

    @TypeConverter
    fun toCustomerTxList(value: String): List<CustomerTx> {
        val type = Types.newParameterizedType(List::class.java, CustomerTx::class.java)
        val adapter = moshi.adapter<List<CustomerTx>>(type)
        return adapter.fromJson(value) ?: emptyList()
    }

    @TypeConverter
    fun fromWarehouseMap(value: Map<String, Int>?): String {
        if (value == null) return "{}"
        val type = Types.newParameterizedType(Map::class.java, String::class.java, Int::class.javaObjectType)
        val adapter = moshi.adapter<Map<String, Int>>(type)
        return adapter.toJson(value)
    }

    @TypeConverter
    fun toWarehouseMap(value: String): Map<String, Int> {
        val type = Types.newParameterizedType(Map::class.java, String::class.java, Int::class.javaObjectType)
        val adapter = moshi.adapter<Map<String, Int>>(type)
        return adapter.fromJson(value) ?: emptyMap()
    }

    @TypeConverter
    fun fromCustomPricesMap(value: Map<String, Double>?): String {
        if (value == null) return "{}"
        val type = Types.newParameterizedType(Map::class.java, String::class.java, Double::class.javaObjectType)
        val adapter = moshi.adapter<Map<String, Double>>(type)
        return adapter.toJson(value)
    }

    @TypeConverter
    fun toCustomPricesMap(value: String): Map<String, Double> {
        val type = Types.newParameterizedType(Map::class.java, String::class.java, Double::class.javaObjectType)
        val adapter = moshi.adapter<Map<String, Double>>(type)
        return adapter.fromJson(value) ?: emptyMap()
    }

    @TypeConverter
    fun fromBarcodeList(value: List<String>?): String {
        if (value == null) return "[]"
        val type = Types.newParameterizedType(List::class.java, String::class.java)
        val adapter = moshi.adapter<List<String>>(type)
        return adapter.toJson(value)
    }

    @TypeConverter
    fun toBarcodeList(value: String?): List<String> {
        if (value.isNullOrBlank()) return emptyList()
        val type = Types.newParameterizedType(List::class.java, String::class.java)
        val adapter = moshi.adapter<List<String>>(type)
        return adapter.fromJson(value) ?: emptyList()
    }
}

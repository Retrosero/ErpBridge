import re

content = open("app/src/main/java/com/example/data/api/FieldOpsApiService.kt").read()

# Make FieldOpsSyncResponse handle `data` as well
new_sync_response = """
data class FieldOpsSyncResponse<T>(
    val success: Boolean? = null,
    val message: String? = null,
    val entity: String? = null,
    val page: Int? = null,
    val pageSize: Int? = null,
    val total: Int? = null,
    val since: String? = null,
    val watermark: String? = null,
    @com.squareup.moshi.Json(name = "items") val items: List<T>? = null,
    @com.squareup.moshi.Json(name = "data") val data: List<T>? = null
) {
    val actualItems: List<T> get() = items ?: data ?: emptyList()
}
"""

content = re.sub(r"data class FieldOpsSyncResponse<T>.*?val items: List<T>\n\)", new_sync_response.strip(), content, flags=re.DOTALL)

# Add @Json annotations to UrunDto
new_urun = """
data class UrunDto(
    @com.squareup.moshi.Json(name = "tenant_id") val tenant_id: String? = null,
    @com.squareup.moshi.Json(name = "api_key") val api_key: String? = null,
    @com.squareup.moshi.Json(name = "urun_kodu") val urun_kodu: String? = null,
    @com.squareup.moshi.Json(name = "urun_adi") val urun_adi: String? = null,
    @com.squareup.moshi.Json(name = "barkod") val barkod: String? = null,
    @com.squareup.moshi.Json(name = "satis_fiyati") val satis_fiyati: Double? = null,
    @com.squareup.moshi.Json(name = "kdv") val kdv: Double? = null,
    @com.squareup.moshi.Json(name = "stok") val stok: Int? = null,
    
    // Fallback/Legacy fields
    val id: String? = null,
    val erpRef: String? = null,
    val erpKod: String? = null,
    val urunKod: String? = null,
    val erp: String? = null,
    val ad: String? = null,
    val urunAd: String? = null,
    val urunTip: Int? = null,
    val birim: String? = null,
    val kdvOrani: Double? = null,
    val alisFiyat: Double? = null,
    val satisFiyat: Double? = null,
    val listeFiyati: Double? = null,
    val paraBirimi: String? = null,
    val kategori: String? = null,
    val marka: String? = null,
    val aktif: Boolean? = null,
    val updatedAt: String? = null,
    val createdAt: String? = null,
    val isDeleted: Boolean? = null,
    val miktar: Int? = null,
    val quantity: Int? = null,
    val stock: Int? = null,
    val miktarDepo: Map<String, Int>? = null,
    val stockByWarehouse: Map<String, Int>? = null,
    val bayiFiyati: Double? = null,
    val toptanFiyati: Double? = null,
    val customPrices: Map<String, Double>? = null
) {
    val actualUrunKod: String get() = urun_kodu ?: urunKod ?: erpKod ?: id ?: ""
    val actualUrunAd: String get() = urun_adi ?: urunAd ?: ad ?: "İsimsiz Ürün"
    val actualSatisFiyat: Double get() = satis_fiyati ?: satisFiyat ?: listeFiyati ?: 0.0
    val actualKdv: Double get() = kdv ?: kdvOrani ?: 20.0
    val actualBarkod: String get() = barkod ?: actualUrunKod
    val actualStok: Int get() = stok ?: miktar ?: quantity ?: stock ?: 0
}
"""
content = re.sub(r"data class UrunDto\(.*?\)\s*\{\s*val actualUrunKod:.*?val actualSatisFiyat:.*?\}", new_urun.strip(), content, flags=re.DOTALL)

open("app/src/main/java/com/example/data/api/FieldOpsApiService.kt", "w").write(content)

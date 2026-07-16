package com.example.data.api

import retrofit2.http.GET
import retrofit2.http.Path
import retrofit2.http.Query
import retrofit2.Response

interface GoappApiService {

    @GET("cari-hesaplar")
    suspend fun getCariHesaplar(
        @Query("search") search: String? = null,
        @Query("cari_tipi") cariTipi: String? = null
    ): Response<List<CariHesapNetwork>>

    @GET("satislar")
    suspend fun getSatislar(
        @Query("page") page: Int = 1,
        @Query("limit") limit: Int = 50,
        @Query("baslangic_tarihi") baslangicTarihi: String? = null,
        @Query("bitis_tarihi") bitisTarihi: String? = null,
        @Query("cari_hesap_id") cariHesapId: String? = null
    ): Response<SatisListResponse>

    @GET("tahsilatlar")
    suspend fun getTahsilatlar(
        @Query("cari_hesap_id") cariHesapId: String? = null,
        @Query("baslangic_tarihi") baslangicTarihi: String? = null,
        @Query("bitis_tarihi") bitisTarihi: String? = null
    ): Response<TahsilatListResponse>

    @GET("cari-hesap-hareketleri")
    suspend fun getCariHareketler(
        @Query("cari_hesap_id") cariHesapId: String? = null,
        @Query("start_date") startDate: String? = null,
        @Query("end_date") endDate: String? = null,
        @Query("limit") limit: Int = 50
    ): Response<CariHareketListResponse>
}

data class CariHesapNetwork(
    val id: String,
    val unvan: String?,
    val cari_kodu: String?,
    val bakiye: Double?,
    val yetkili_kisi: String?,
    val telefon: String?,
    val adres: String?,
    val vergi_dairesi: String?,
    val vergi_no: String?
)

data class SatisListResponse(
    val data: List<SatisNetwork>
)

data class SatisNetwork(
    val id: String,
    val belge_no: String?,
    val tarih: String?,
    val toplam_tutar: Double?,
    val cari_hesap_id: String?
)

data class TahsilatListResponse(
    val data: List<TahsilatNetwork>
)

data class TahsilatNetwork(
    val id: String,
    val islem_tarihi: String?,
    val tutar: Double?,
    val cari_hesap_id: String?,
    val aciklama: String?
)

data class CariHareketListResponse(
    val success: Boolean,
    val count: Int,
    val data: List<CariHareketNetwork>
)

data class CariHareketNetwork(
    val id: String,
    val islem_tarihi: String?,
    val aciklama: String?,
    val borc: Double?,
    val alacak: Double?,
    val bakiye: Double?
)

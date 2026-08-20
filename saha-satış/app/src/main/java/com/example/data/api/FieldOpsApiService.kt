package com.example.data.api

import retrofit2.Response
import retrofit2.http.*

interface FieldOpsApiService {
    @POST("api/v1/android/sync/stokHareket")
    suspend fun getStokHareket(@Body request: PullJobsRequest): Response<StokHareketResponse>

    @POST("api/v1/android/sync/stokHareketleri")
    suspend fun getStokHareketleri(@Body request: PullJobsRequest): Response<StokHareketResponse>

    @POST("api/v1/android/sync/cariHareket")
    suspend fun getCariHareket(@Body request: PullJobsRequest): Response<CariHareketResponse>

    @POST("api/v1/android/sync/barkodTanimi")
    suspend fun getBarkodTanimi(@Body request: PullJobsRequest): Response<BarkodTanimiResponseDto>

    @POST("api/v1/android/sync/cariAdresleri")
    suspend fun getCariAdresleri(@Body request: PullJobsRequest): Response<CariAdresResponseDto>

    @POST("api/v1/android/sync/cariBankaHesaplari")
    suspend fun getCariBankaHesaplari(@Body request: PullJobsRequest): Response<CariBankaHesapResponseDto>

    @POST("api/v1/android/sync/trigger")
    suspend fun triggerSync(@Body request: PullJobsRequest): Response<FieldOpsTriggerResponse>

    @POST("api/v1/android/sync/status")
    suspend fun getSyncStatus(@Body request: PullJobsRequest): Response<SyncStatusResponseDto>

    @POST("api/v1/android/sync/pushStatus")
    suspend fun getPushStatus(@Body request: PullJobsRequest): Response<FieldOpsPushStatusResponse>

    @POST("api/v1/android/sync/fiyatListesi")
    suspend fun getFiyatListesi(@Body request: PullJobsRequest): Response<okhttp3.ResponseBody>












    @POST("api/v1/android/sync/push")
    suspend fun push(@Body request: Map<String, Any?>): Response<FieldOpsPushResponse>








    @POST("api/v1/android/bootstrap")
    suspend fun bootstrap(@Body request: BootstrapRequest): Response<BootstrapResponse>

    @POST("api/v1/android/pull")
    suspend fun pullJobs(@Body request: PullJobsRequest): Response<PullJobsResponse>

    @POST("api/v1/android/push")
    suspend fun pushJobs(@Body request: PushJobsRequest): Response<PushJobsResponse>

    @POST("api/v1/android/sync/cari")
    suspend fun getCariler(@Body request: PullJobsRequest): Response<FieldOpsSyncResponse<CariDto>>

    @POST("api/v1/android/sync/urun")
    suspend fun getUrunler(@Body request: PullJobsRequest): Response<FieldOpsSyncResponse<UrunDto>>

    @POST("api/v1/android/sync/stokSeviye")
    suspend fun getStokSeviye(@Body request: PullJobsRequest): Response<StokSeviyeResponse>

    @POST("api/v1/android/sync/cariHareketleri")
    suspend fun getCariHareketleri(@Body request: PullJobsRequest): Response<CariHareketResponse>

    @POST("api/v1/android/sync/faturaHareket")
    suspend fun getFaturaHareket(@Body request: PullJobsRequest): Response<FaturaHareketResponse>

    @POST("api/v1/android/sync/stokSatisFiyatListeTanimlari")
    suspend fun getStokSatisFiyatListeTanimlari(@Body request: PullJobsRequest): Response<FieldOpsSyncResponse<StokSatisFiyatListeTanimlariDto>>

    @POST("api/v1/android/sync/stokSatisFiyatListeleri")
    suspend fun getStokSatisFiyatListeleri(@Body request: PullJobsRequest): Response<FieldOpsSyncResponse<StokSatisFiyatListeleriDto>>

    @POST("api/v1/android/sync/bankalar")
    suspend fun getBankalar(@Body request: PullJobsRequest): Response<BankalarResponseDto>

    @POST("api/v1/android/sync/kasalar")
    suspend fun getKasalar(@Body request: PullJobsRequest): Response<KasalarResponseDto>

    @POST("api/v1/android/sync/kasaYonetim")
    suspend fun getKasaYonetim(@Body request: PullJobsRequest): Response<KasaYonetimResponseDto>

    @POST("api/v1/android/sync/status")
    suspend fun getSyncStatusPost(@Body request: PullJobsRequest): Response<SyncStatusResponseDto>

    @POST("api/v1/android/license/status")
    suspend fun getLicenseStatus(@Body request: PullJobsRequest): Response<LicenseStatusDto>

    @POST("api/v1/android/sync/fiyatlar")
    suspend fun getFiyatlar(@Body request: PullJobsRequest): Response<FieldOpsSyncResponse<FiyatDto>>

    @POST("api/v1/android/sync/acikSiparisler")
    suspend fun getAcikSiparisler(@Body request: PullJobsRequest): Response<FieldOpsSyncResponse<AcikSiparisDto>>
}

// DTOs
data class FieldOpsSyncResponse<T>(
    val success: Boolean? = null,
    val message: String? = null,
    val entity: String? = null,
    val page: Int?? = null,
    val pageSize: Int?? = null,
    val total: Int?? = null,
    val since: String? = null,
    val watermark: String? = null,
    @com.squareup.moshi.Json(name = "items") val items: List<T>? = null,
    @com.squareup.moshi.Json(name = "data") val data: List<T>? = null
) {
    val actualItems: List<T> get() = items ?: data ?: emptyList()
}

data class CariHareketDto(
    @com.squareup.moshi.Json(name = "id") val id: String? = null,
    @com.squareup.moshi.Json(name = "date") val date: String? = null,
    @com.squareup.moshi.Json(name = "tarih") val tarih: String? = null,
    @com.squareup.moshi.Json(name = "cha_tarihi") val cha_tarihi: String? = null,
    @com.squareup.moshi.Json(name = "type") val type: String? = null, // SATIŞ, TAHSİLAT, TEDİYE
    @com.squareup.moshi.Json(name = "tip") val tip: Int? = null,
    @com.squareup.moshi.Json(name = "cha_tip") val cha_tip: Int? = null,
    @com.squareup.moshi.Json(name = "amount") val amount: Double? = null,
    @com.squareup.moshi.Json(name = "tutar") val tutar: Double? = null,
    @com.squareup.moshi.Json(name = "meblag") val meblag: Double? = null,
    @com.squareup.moshi.Json(name = "cha_meblag") val cha_meblag: Double? = null,
    @com.squareup.moshi.Json(name = "borc") val borc: Double? = null,
    @com.squareup.moshi.Json(name = "alacak") val alacak: Double? = null,
    @com.squareup.moshi.Json(name = "borcTutari") val borcTutari: Double? = null,
    @com.squareup.moshi.Json(name = "alacakTutari") val alacakTutari: Double? = null,
    @com.squareup.moshi.Json(name = "description") val description: String? = null,
    @com.squareup.moshi.Json(name = "aciklama") val aciklama: String? = null,
    @com.squareup.moshi.Json(name = "cha_aciklama") val cha_aciklama: String? = null,
    @com.squareup.moshi.Json(name = "evrakNo") val evrakNo: String? = null,
    @com.squareup.moshi.Json(name = "evrak_no") val evrak_no: String? = null,
    @com.squareup.moshi.Json(name = "cha_evrak_no") val cha_evrak_no: String? = null,
    @com.squareup.moshi.Json(name = "docNo") val docNo: String? = null
) {
    val actualDate: String get() = (date ?: tarih ?: cha_tarihi ?: "").trim()
    val actualAmount: Double get() = tutar ?: meblag ?: cha_meblag ?: amount ?: borc ?: alacak ?: borcTutari ?: alacakTutari ?: 0.0
    val actualDescription: String get() = (description ?: aciklama ?: cha_aciklama ?: "").trim()
    val actualType: String get() {
        if (!type.isNullOrBlank()) return type.uppercase()
        if (tip == 0 || cha_tip == 0 || (borc ?: 0.0) > 0 || (borcTutari ?: 0.0) > 0) return "SATIŞ"
        if (tip == 1 || cha_tip == 1 || (alacak ?: 0.0) > 0 || (alacakTutari ?: 0.0) > 0) return "TAHSİLAT"
        return "HAREKET"
    }
}

data class CariDto(
    // Central bootstrap records use customerCode as their stable identifier.
    // Keep id optional for compatibility with older FieldOps payloads.
    val id: String? = null,
    val customerCode: String? = null,
    val title1: String? = null,
    val erpRef: String?,
    val erpKod: String?,
    val cariKod: String? = null,
    val erp: String?,
    val unvan: String?,
    val cariUnvan: String? = null,
    val cariTip: Int? = null,
    val vergiNo: String?,
    val vergiDairesi: String?,
    val tcKimlikNo: String? = null,
    val adres: String?,
    val il: String? = null,
    val ilce: String? = null,
    val telefon: String?,
    val email: String?,
    val musteri: Boolean?,
    val updatedAt: String?,
    val createdAt: String?,
    val isDeleted: Boolean?,
    val paraBirimi: String? = null,
    // Additional/undocumented fields from bridge:
    val bakiye: Double? = null,
    val balance: Double? = null,
    val netBakiye: Double? = null,
    val borcBakiye: Double? = null,
    val alacakBakiye: Double? = null,
    val borc: Double? = null,
    val alacak: Double? = null,
    val hareketler: List<CariHareketDto>? = null,
    val transactions: List<CariHareketDto>? = null
) {
    val actualCariKod: String get() = customerCode ?: cariKod ?: erpKod ?: id ?: ""
    val actualCariUnvan: String get() = title1 ?: cariUnvan ?: unvan ?: "İsimsiz Cari"
    val actualBakiye: Double get() {
        if (bakiye != null) return bakiye
        if (netBakiye != null) return netBakiye
        if (balance != null) return balance
        val b = borcBakiye ?: borc ?: 0.0
        val a = alacakBakiye ?: alacak ?: 0.0
        if (b != 0.0 || a != 0.0) return b - a
        return 0.0
    }
}

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
    val stockCode: String? = null,
    val name: String? = null,
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
    @com.squareup.moshi.Json(name = "reyonKod") val reyonKod: String? = null,
    @com.squareup.moshi.Json(name = "olcu") val olcu: String? = null,
    @com.squareup.moshi.Json(name = "ambalaj") val ambalaj: String? = null,
    @com.squareup.moshi.Json(name = "koliAdet") val koliAdet: String? = null,
    @com.squareup.moshi.Json(name = "sto_yer_kod") val stoYerKod: String? = null,
    @com.squareup.moshi.Json(name = "sto_sektor_kodu") val stoSektorKodu: String? = null,
    @com.squareup.moshi.Json(name = "sto_ambalaj_kodu") val stoAmbalajKodu: String? = null,
    @com.squareup.moshi.Json(name = "sto_marka_kodu") val stoMarkaKodu: String? = null,
    @com.squareup.moshi.Json(name = "sto_kalkon_kodu") val stoKalkonKodu: String? = null,
    val customPrices: Map<String, Double>? = null
) {
    val actualUrunKod: String get() = urun_kodu ?: stockCode ?: urunKod ?: erpKod ?: id ?: ""
    val actualUrunAd: String get() = urun_adi ?: name ?: urunAd ?: ad ?: "İsimsiz Ürün"
    val actualSatisFiyat: Double get() = satis_fiyati ?: satisFiyat ?: listeFiyati ?: 0.0
    val actualKdv: Double get() = kdv ?: kdvOrani ?: 20.0
    val actualBarkod: String get() = barkod ?: actualUrunKod
    val actualStok: Int get() = stok ?: miktar ?: quantity ?: stock ?: 0
    val actualReyonKod: String? get() = listOf(reyonKod, stoYerKod).firstOrNull { !it.isNullOrBlank() }?.trim()
    val actualOlcu: String? get() = listOf(olcu, stoSektorKodu).firstOrNull { !it.isNullOrBlank() }?.trim()
    val actualAmbalaj: String? get() = listOf(ambalaj, stoAmbalajKodu).firstOrNull { !it.isNullOrBlank() }?.trim()
    val actualMarka: String? get() = listOf(marka, stoMarkaKodu).firstOrNull { !it.isNullOrBlank() }?.trim()
    val actualKoliAdet: String? get() = listOf(koliAdet, stoKalkonKodu).firstOrNull { !it.isNullOrBlank() }?.trim()
}

data class StokHareketResponse(
    val entity: String,
    val stokKod: String?,
    val page: Int??,
    val pageSize: Int??,
    val total: Int??,
    val since: String?,
    val items: List<StokHareketiDto>
)

data class StokHareketiDto(
    val id: String? = null,
    val erpRef: String?,
    val erp: String?,
    val stokKod: String?,
    val urunKod: String?,
    val tarih: String?,
    val tip: Int?,             // 0=giriş 1=çıkış 2=iade giriş 3=iade çıkış
    val cins: Int?,
    val evrakTip: Int?,
    val evrakNo: String?,
    val girisMiktar: Double?,
    val cikisMiktar: Double?,
    val miktar: Double?,       // negative for çıkış (legacy field)
    val birimFiyat: Double?,   // KDV hariç
    val tutar: Double?,
    val cariKod: String?,
    val girisDepoNo: Int?,
    val cikisDepoNo: Int?,
    val aciklama: String?,
    val updatedAt: String?,
    val faturaRecno: Int? = null
)

data class CariHareketResponse(
    val entity: String? = null,
    val cariKod: String? = null,
    val page: Int? = null,
    val pageSize: Int? = null,
    val total: Int? = null,
    val since: String? = null,
    @com.squareup.moshi.Json(name = "items") val itemsList: List<CariHareketiDto>? = null,
    @com.squareup.moshi.Json(name = "data") val dataList: List<CariHareketiDto>? = null
) {
    val items: List<CariHareketiDto> get() = itemsList ?: dataList ?: emptyList()
}

data class CariHareketiDto(
    @com.squareup.moshi.Json(name = "id") val id: String? = null,
    @com.squareup.moshi.Json(name = "erpRef") val erpRef: String? = null,
    @com.squareup.moshi.Json(name = "erp") val erp: String? = null,
    @com.squareup.moshi.Json(name = "cariKod") val cariKod: String? = null,
    @com.squareup.moshi.Json(name = "cari_kod") val cari_kod: String? = null,
    @com.squareup.moshi.Json(name = "cari_kodu") val cari_kodu: String? = null,
    @com.squareup.moshi.Json(name = "cha_kod") val cha_kod: String? = null,
    @com.squareup.moshi.Json(name = "cha_kodu") val cha_kodu: String? = null,
    @com.squareup.moshi.Json(name = "cha_cari_kodu") val cha_cari_kodu: String? = null,
    @com.squareup.moshi.Json(name = "customerCode") val customerCode: String? = null,
    @com.squareup.moshi.Json(name = "erpKod") val erpKod: String? = null,
    @com.squareup.moshi.Json(name = "tarih") val tarih: String? = null,
    @com.squareup.moshi.Json(name = "cha_tarihi") val cha_tarihi: String? = null,
    @com.squareup.moshi.Json(name = "cha_tarih") val cha_tarih: String? = null,
    @com.squareup.moshi.Json(name = "cha_belge_tarih") val cha_belge_tarih: String? = null,
    @com.squareup.moshi.Json(name = "belgeTarihi") val belgeTarihi: String? = null,
    @com.squareup.moshi.Json(name = "date") val date: String? = null,
    @com.squareup.moshi.Json(name = "evrakTip") val evrakTip: Int? = null,
    @com.squareup.moshi.Json(name = "cha_evrak_tip") val cha_evrak_tip: Int? = null,
    @com.squareup.moshi.Json(name = "evrak_tip") val evrak_tip: Int? = null,
    @com.squareup.moshi.Json(name = "evrakNo") val evrakNo: String? = null,
    @com.squareup.moshi.Json(name = "cha_evrak_no") val cha_evrak_no: String? = null,
    @com.squareup.moshi.Json(name = "evrak_no") val evrak_no: String? = null,
    @com.squareup.moshi.Json(name = "docNo") val docNo: String? = null,
    @com.squareup.moshi.Json(name = "cha_belge_no") val cha_belge_no: String? = null,
    @com.squareup.moshi.Json(name = "belgeNo") val belgeNo: String? = null,
    @com.squareup.moshi.Json(name = "cha_evrak_seri") val cha_evrak_seri: String? = null,
    @com.squareup.moshi.Json(name = "evrakSeri") val evrakSeri: String? = null,
    @com.squareup.moshi.Json(name = "cha_evrak_sira") val cha_evrak_sira: Int? = null,
    @com.squareup.moshi.Json(name = "evrakSira") val evrakSira: Int? = null,
    @com.squareup.moshi.Json(name = "tip") val tip: Int? = null,             // 0=borç 1=alacak
    @com.squareup.moshi.Json(name = "cha_tip") val cha_tip: Int? = null,
    @com.squareup.moshi.Json(name = "tutar") val tutar: Double? = null,
    @com.squareup.moshi.Json(name = "meblag") val meblag: Double? = null,
    @com.squareup.moshi.Json(name = "cha_meblag") val cha_meblag: Double? = null,
    @com.squareup.moshi.Json(name = "amount") val amount: Double? = null,
    @com.squareup.moshi.Json(name = "borc") val borc: Double? = null,
    @com.squareup.moshi.Json(name = "alacak") val alacak: Double? = null,
    @com.squareup.moshi.Json(name = "borcTutari") val borcTutari: Double? = null,
    @com.squareup.moshi.Json(name = "alacakTutari") val alacakTutari: Double? = null,
    @com.squareup.moshi.Json(name = "cha_borc_tutari") val cha_borc_tutari: Double? = null,
    @com.squareup.moshi.Json(name = "cha_alacak_tutari") val cha_alacak_tutari: Double? = null,
    @com.squareup.moshi.Json(name = "borcMu") val borcMu: Boolean? = null,      // tip==0 ise true
    @com.squareup.moshi.Json(name = "cha_borc_mu") val cha_borc_mu: Boolean? = null,
    @com.squareup.moshi.Json(name = "aciklama") val aciklama: String? = null,
    @com.squareup.moshi.Json(name = "cha_aciklama") val cha_aciklama: String? = null,
    @com.squareup.moshi.Json(name = "description") val description: String? = null,
    @com.squareup.moshi.Json(name = "vade") val vade: String? = null,
    @com.squareup.moshi.Json(name = "cha_vade") val cha_vade: String? = null,
    @com.squareup.moshi.Json(name = "vadeTarihi") val vadeTarihi: String? = null,
    @com.squareup.moshi.Json(name = "updatedAt") val updatedAt: String? = null,
    @com.squareup.moshi.Json(name = "cha_recno") val cha_recno: Int? = null,
    @com.squareup.moshi.Json(name = "cha_RECno") val cha_RECno: Int? = null,
    @com.squareup.moshi.Json(name = "chaRecNo") val chaRecNo: Int? = null,
    @com.squareup.moshi.Json(name = "recno") val recno: Int? = null
) {
    val realChaRecNo: Int? get() = recno ?: cha_recno ?: cha_RECno ?: chaRecNo
    val actualCariKod: String get() = (cariKod ?: cari_kod ?: cari_kodu ?: cha_kod ?: cha_kodu ?: cha_cari_kodu ?: customerCode ?: erpKod ?: "").trim()
    val actualTarih: String get() = (tarih ?: cha_tarihi ?: cha_tarih ?: date ?: cha_belge_tarih ?: belgeTarihi ?: "").trim()
    val actualEvrakTip: Int get() = evrakTip ?: cha_evrak_tip ?: evrak_tip ?: 0
    val actualEvrakNo: String get() {
        val direct = (evrakNo ?: cha_evrak_no ?: evrak_no ?: docNo ?: cha_belge_no ?: belgeNo ?: "").trim()
        if (direct.isNotEmpty()) return direct
        val seri = (cha_evrak_seri ?: evrakSeri ?: "").trim()
        val sira = (cha_evrak_sira ?: evrakSira ?: 0)
        return if (seri.isNotEmpty() || sira > 0) "$seri-$sira" else ""
    }
    val actualTutar: Double get() = tutar ?: meblag ?: cha_meblag ?: amount ?: borc ?: alacak ?: borcTutari ?: alacakTutari ?: cha_borc_tutari ?: cha_alacak_tutari ?: 0.0
    val actualAciklama: String get() = (aciklama ?: cha_aciklama ?: description ?: "").trim()
    val actualBorcMu: Boolean get() {
        if (borcMu != null) return borcMu
        if (cha_borc_mu != null) return cha_borc_mu
        if (tip != null) return tip == 0
        if (cha_tip != null) return cha_tip == 0
        val b = (borc ?: borcTutari ?: cha_borc_tutari ?: 0.0)
        val a = (alacak ?: alacakTutari ?: cha_alacak_tutari ?: 0.0)
        if (b > 0.0 && a == 0.0) return true
        if (a > 0.0 && b == 0.0) return false
        return true
    }
}

data class FieldOpsPushResponse(
    val requestId: String,
    val status: String,
    val isNew: Boolean,
    val queuedAt: String?
)

data class FieldOpsPushStatusResponse(
    val requestId: String,
    val status: String,
    val entity: String?,
    val erp: String?,
    val attemptCount: Int?,
    val lastErrorCode: String?,
    val lastErrorMessage: String?,
    val createdAt: String?,
    val updatedAt: String?,
    val completedAt: String?
)

data class FieldOpsTriggerResponse(
    val message: String?,
    val erp: String?,
    val entity: String?,
    val triggeredAt: String?
)

data class LicenseStatusDto(
    val state: String,
    val reason: String?,
    val lastCheckedAt: String?,
    val expiresAt: String?,
    val daysUntilExpiry: Int?,
    val daysRemaining: Int? = null,
    val enabledErps: List<String>?,
    val erpAllowed: List<String>? = null,
    val licensee: String? = null,
    val machineFingerprint: String? = null,
    val allowsSync: Boolean?
)

data class StokSatisFiyatListeTanimlariDto(
    val id: String? = null,
    val erpRef: String?,
    val listNo: Int?,                // sfiyat_listeno
    val aciklama: String?,            // sfiyat_aciklama
    val paraBirimi: String?,          // sfiyat_parabirimi
    val updatedAt: String?,
    val isDeleted: Boolean?
)

data class StokSatisFiyatListeleriDto(
    val id: String? = null,
    val erpRef: String?,
    val stokKod: String?,             // sfiyat_stokkod
    val listNo: Int?,                // sfiyat_listeno
    val fiyat: Double?,               // sfiyat_fiyat
    val doviz: String?,               // sfiyat_doviz
    val updatedAt: String?,
    val isDeleted: Boolean?
)

data class StokSeviyeResponse(
    val entity: String,
    val stokKod: String?,
    val page: Int??,
    val pageSize: Int??,
    val total: Int??,
    val items: List<StokSeviyeDto>
)

data class StokSeviyeDto(
    val erpRef: String?,
    val erp: String?,
    val stokKod: String? = null,
    val eldekiMiktar: Double? = null,
    val updatedAt: String?,
    val id: String? = null,
    val depoNo: Int? = null,
    val depoAd: String? = null,
    val miktar: Double? = null,
    val birim: String? = null
) {
    val actualMiktar: Double get() = miktar ?: eldekiMiktar ?: 0.0
}

data class FiyatListesiTanimiDto(
    val erpRef: String?,
    val erp: String?,
    val listeNo: Int,
    val aciklama: String?,
    val kdvDahil: Boolean?,
    val otvDahil: Boolean?,
    val oivDahil: Boolean?,
    val ilkTarih: String?,
    val sonTarih: String?,
    val updatedAt: String?
)

data class FiyatListesiSatirDto(
    val erpRef: String?,
    val erp: String?,
    val listeNo: Int,
    val stokKod: String? = null,
    val depoNo: Int?,
    val odemePlani: Int?,
    val fiyat: Double,
    val doviz: Int?,
    val iskontoKod: String?,
    val kampanyaKod: String?,
    val primYuzdesi: Double?,
    val updatedAt: String?,
    val aciklama: String? // Present in getFiyatListesi byStokKod (Mod 3) response
)

data class FaturaHareketResponse(
    val entity: String,
    val cariKod: String? = null,
    val page: Int??,
    val pageSize: Int??,
    val total: Int??,
    val since: String?,
    val items: List<FaturaHareketDto>
)

data class FaturaHareketDto(
    val erpRef: String?,
    val erp: String?,
    val cariKod: String?,
    val tarih: String?,
    val evrakTip: Int?,
    val evrakNo: String?,
    val tip: Int?,
    val tutar: Double?,
    val updatedAt: String?,
    val satirlar: List<FaturaSatirDto>?
)

data class FaturaSatirDto(
    val erpRef: String?,
    val stokKod: String?,
    val stokAd: String? = null,
    val tarih: String?,
    val tip: Int?,
    val cins: Int?,
    val girisMiktar: Double?,
    val cikisMiktar: Double?,
    val miktar: Double?,
    val birimFiyat: Double?,
    val tutar: Double?,
    val vergi: Double?,
    val girisDepoNo: Int?,
    val cikisDepoNo: Int?,
    val aciklama: String?,
    val updatedAt: String?,
    val sth_fat_recid_recno: Int? = null,
    val sth_fat_recid_RECno: Int? = null,
    val sth_fat_recid_recNo: Int? = null,
    val sthFatRecidRecno: Int? = null,
    val sth_fat_recid_recno_cir: Int? = null
) {
    val realSthFatRecidRecno: Int? get() = sth_fat_recid_recno ?: sth_fat_recid_RECno ?: sth_fat_recid_recNo ?: sthFatRecidRecno ?: sth_fat_recid_recno_cir
}

data class SyncStatusResponseDto(
    val erp: String?,
    val syncInProgress: Boolean? = null,
    val syncStartedAt: String? = null,
    val syncEntity: String? = null,
    val watermarks: List<WatermarkDto>? = null,
    val isRunning: Boolean? = null,
    val lastRunAt: String? = null,
    val lastRunEntity: String? = null,
    val progress: Int? = null,
    val queueDepth: Int? = null
)

data class WatermarkDto(
    val entity: String,
    val lastSyncAt: String?,
    val totalSynced: Int,
    val mode: String?
)

data class BarkodTanimiResponseDto(
    val entity: String,
    val mode: String?,
    val barkodKod: String?,
    val stokKod: String?,
    val page: Int??,
    val pageSize: Int??,
    val total: Int??,
    val items: List<BarkodTanimiDto>
)

data class BarkodTanimiDto(
    val erpRef: String?,
    val erp: String?,
    val barkod: String,
    val stokKod: String? = null,
    val partiKodu: String?,
    val lotNo: Int?,
    val serinoVeyaBaglantiKodu: String?,
    val barkodTipi: Int?,
    val icerigi: Int?,
    val birimGosterge: Int?,
    val master: Boolean?,
    val updatedAt: String?
)

data class CariAdresResponseDto(
    val entity: String,
    val mode: String?,
    val cariKod: String?,
    val page: Int??,
    val pageSize: Int??,
    val total: Int??,
    val items: List<CariAdresDto>
)

data class CariAdresDto(
    val erpRef: String?,
    val erp: String?,
    val cariKod: String? = null,
    val adresNo: Int,
    val yazdirilabilir: Boolean?,
    val cadde: String?,
    val mahalle: String?,
    val sokak: String?,
    val semt: String?,
    val ilce: String?,
    val il: String?,
    val ulke: String?,
    val postaKodu: String?,
    val telUlkeKodu: String?,
    val telBolgeKodu: String?,
    val telNo1: String?,
    val gpsEnlem: Double?,
    val gpsBoylam: Double?,
    val ziyaretPeriyodu: Int?,
    val ziyaretGunu: Int?,
    val eFaturaAlias: String?,
    val updatedAt: String?,
    val id: String? = null,
    val recno: Int? = null,
    val adresTip: Int? = null,
    val adresSatir1: String? = null,
    val adresSatir2: String? = null,
    val fax: String? = null,
    val email: String? = null,
    val telefon: String? = null
)

data class CariBankaHesapResponseDto(
    val entity: String,
    val mode: String?,
    val cariKod: String?,
    val page: Int??,
    val pageSize: Int??,
    val total: Int??,
    val items: List<CariBankaHesapDto>
)

data class CariBankaHesapDto(
    val erpRef: String?,
    val erp: String?,
    val cariKod: String? = null,
    val slot: Int,
    val tCMBKodu: String?,
    val tCMBSubeKodu: String?,
    val tCMBIlKodu: String?,
    val hesapNumarasi: String?,
    val swiftKodu: String?,
    val dovizCinsi: Int?,
    val musteriNo: String?,
    val updatedAt: String?,
    val id: String? = null,
    val recno: Int? = null,
    val bankaKod: String? = null,
    val subeKod: String? = null,
    val hesapNo: String? = null,
    val iban: String? = null,
    val hesapSahibi: String? = null,
    val paraBirimi: String? = null,
    val defaultMu: Boolean? = null
)

data class BankalarResponseDto(
    val entity: String,
    val page: Int??,
    val pageSize: Int??,
    val total: Int??,
    val items: List<BridgeBankaDto>
)

data class BridgeBankaDto(
    val erpRef: String?,
    val erp: String?,
    val kod: String,
    val isim: String,
    val sube: String?,
    val swiftKodu: String?,
    val iBANKodu: String?,
    val hesapNumarasi: String?,
    val tCMBKodu: String?,
    val tCMBSubeKodu: String?,
    val tCMBIlKodu: String?,
    val musteriNo: String?,
    val hesapTipi: Int?,
    val dovizCinsi: Int?,
    val cadde: String?,
    val mahalle: String?,
    val il: String?,
    val ulke: String?,
    val temsilci: String?,
    val temsilciEposta: String?,
    val updatedAt: String?,
    val id: String? = null,
    val recno: Int? = null,
    val bankaKod: String? = null,
    val bankaAd: String? = null,
    val subeAd: String? = null,
    val swiftKod: String? = null
)

data class KasalarResponseDto(
    val entity: String,
    val page: Int??,
    val pageSize: Int??,
    val total: Int??,
    val items: List<KasalarDto>
)

data class KasalarDto(
    val erpRef: String?,
    val erp: String?,
    val kod: String,
    val isim: String,
    val tip: Int?,
    val muhasebeKod: String?,
    val dovizCinsi: Int?,
    val bankaKodu: String?,
    val updatedAt: String?
)

data class KasaYonetimResponseDto(
    val entity: String,
    val total: Int??,
    val items: List<KasaYonetimDto>
)

data class KasaYonetimDto(
    val erpRef: String?,
    val erp: String?,
    val kasaKod: String? = null,
    val kasaAd: String? = null,
    val yonetim: String?,
    val muhasebeKod: String?,
    val updatedAt: String?
)

data class BootstrapRequest(
    val tenant_id: String,
    val api_key: String,
    val device_id: String,
    val agent_version: String
)

data class BootstrapResponse(
    // Kept for legacy settings screens. The current central endpoint signals
    // success with HTTP 200 and omits this old envelope field.
    val success: Boolean = true,
    val message: String? = null,
    val tenant_name: String? = null,
    val allowed_erps: List<String>? = null,
    val active_modules: List<String>? = null,
    val tenantId: String? = null,
    val sourceDatabase: String? = null,
    val pulledAtUtc: String? = null,
    val receivedAtUtc: String? = null
)

data class PullJobsRequest(
    val tenant_id: String,
    val api_key: String,
    val device_id: String,
    val agent_version: String,
    val entity: String? = null,
    val since: String? = null,
    val page: Int?? = 1,
    val pageSize: Int?? = 1000
)

data class PullJobsResponse(
    val success: Boolean,
    val message: String? = null,
    val entity: String? = null,
    val watermark: String? = null,
    val items: List<Map<String, Any?>>? = null
)

data class PushJobsRequest(
    val tenant_id: String,
    val api_key: String,
    val device_id: String,
    val agent_version: String,
    val entity: String,
    val payload: Map<String, Any?>
)

data class PushJobsResponse(
    val success: Boolean,
    val message: String? = null,
    val requestId: String? = null,
    val status: String? = null
)

data class FiyatDto(
    val id: String? = null,
    val urunKod: String? = null,
    val fiyat: Double? = null,
    val paraBirimi: String? = null,
    val fiyatListesiKod: String? = null
)

data class AcikSiparisDto(
    val id: String? = null,
    val cariKod: String? = null,
    val tarih: String? = null,
    val tutar: Double? = null,
    val paraBirimi: String? = null,
    val aciklama: String? = null
)

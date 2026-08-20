package com.example.util

import com.example.ui.screens.AppDataStore
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale
import java.util.TimeZone
import java.util.UUID

object MikroPayloadHelper {

    private val isoDateFormat: SimpleDateFormat
        get() = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ssXXX", Locale.getDefault()).apply {
            timeZone = TimeZone.getTimeZone("GMT+3")
        }

    /**
     * Helper to wrap a document payload in the common envelope format.
     */
    fun buildEnvelope(
        documentType: String,
        payload: Map<String, Any?>,
        tenantId: String = "tenant-001",
        deviceId: String = "android-cihaz-001",
        userCode: String = "plasiyer1",
        externalId: String = UUID.randomUUID().toString(),
        occurredAt: String = isoDateFormat.format(Date()),
        payloadVersion: Int = 1
    ): Map<String, Any?> {
        return mapOf(
            "tenantId" to tenantId,
            "deviceId" to deviceId,
            "userCode" to userCode,
            "documentType" to documentType,
            "externalId" to externalId,
            "occurredAt" to occurredAt,
            "payloadVersion" to payloadVersion,
            "payload" to payload
        )
    }

    /**
     * 1. Satış Faturası (sales_invoice)
     */
    fun buildSalesInvoice(
        cariKodu: String = "C001",
        evrakSeri: String = "F",
        tarih: String = isoDateFormat.format(Date()),
        depoKodu: String = "D01",
        temsilciKodu: String = "PL01",
        satirlar: List<Map<String, Any?>>? = null
    ): Map<String, Any?> {
        val lines = satirlar ?: listOf(
            mapOf(
                "stokKodu" to "STK001",
                "miktar" to 10.0,
                "birimFiyat" to 100.0,
                "kdvOrani" to 20.0,
                "birim" to "ADET",
                "aciklama" to "İlk satır"
            ),
            mapOf(
                "stokKodu" to "STK002",
                "miktar" to 5.0,
                "birimFiyat" to 200.0,
                "kdvOrani" to 20.0,
                "birim" to "ADET",
                "aciklama" to "İkinci satır"
            )
        )
        return mapOf(
            "cariKodu" to cariKodu,
            "evrakSeri" to evrakSeri,
            "tarih" to tarih,
            "depoKodu" to depoKodu,
            "temsilciKodu" to temsilciKodu,
            "satirlar" to lines
        )
    }

    /**
     * 2. Satış İrsaliyesi (sales_dispatch)
     */
    fun buildSalesDispatch(
        cariKodu: String = "C001",
        evrakSeri: String = "I",
        tarih: String = isoDateFormat.format(Date()),
        depoKodu: String = "D01",
        temsilciKodu: String = "PL01",
        satirlar: List<Map<String, Any?>>? = null
    ): Map<String, Any?> {
        val lines = satirlar ?: listOf(
            mapOf(
                "stokKodu" to "STK001",
                "miktar" to 10.0,
                "birim" to "ADET",
                "siparisSatirNo" to 1
            )
        )
        return mapOf(
            "cariKodu" to cariKodu,
            "evrakSeri" to evrakSeri,
            "tarih" to tarih,
            "depoKodu" to depoKodu,
            "temsilciKodu" to temsilciKodu,
            "satirlar" to lines
        )
    }

    /**
     * 3. Satış Siparişi (sales_order)
     */
    fun buildSalesOrder(
        cariKodu: String = "C001",
        evrakSeri: String = "S",
        tarih: String = isoDateFormat.format(Date()),
        depoKodu: String = "D01",
        temsilciKodu: String = "PL01",
        teslimTarihi: String = isoDateFormat.format(Date(System.currentTimeMillis() + 86400000 * 6)), // 6 days later
        aciklama: String = "Acil sipariş",
        satirlar: List<Map<String, Any?>>? = null
    ): Map<String, Any?> {
        val lines = satirlar ?: listOf(
            mapOf(
                "stokKodu" to "STK001",
                "miktar" to 100.0,
                "birimFiyat" to 50.0,
                "birim" to "ADET",
                "aciklama" to "İlk sipariş satırı"
            )
        )
        return mapOf(
            "cariKodu" to cariKodu,
            "evrakSeri" to evrakSeri,
            "tarih" to tarih,
            "depoKodu" to depoKodu,
            "temsilciKodu" to temsilciKodu,
            "teslimTarihi" to teslimTarihi,
            "aciklama" to aciklama,
            "satirlar" to lines
        )
    }

    /**
     * 4. Proforma Sipariş (proforma_order)
     */
    fun buildProformaOrder(
        cariKodu: String = "C001",
        evrakSeri: String = "P",
        tarih: String = isoDateFormat.format(Date()),
        depoKodu: String = "D01",
        temsilciKodu: String = "PL01",
        teslimTarihi: String = isoDateFormat.format(Date(System.currentTimeMillis() + 86400000 * 6)),
        aciklama: String = "Teklif",
        satirlar: List<Map<String, Any?>>? = null
    ): Map<String, Any?> {
        val lines = satirlar ?: listOf(
            mapOf(
                "stokKodu" to "STK001",
                "miktar" to 50.0,
                "birimFiyat" to 75.0,
                "birim" to "ADET",
                "aciklama" to "Teklif satırı"
            )
        )
        return mapOf(
            "cariKodu" to cariKodu,
            "evrakSeri" to evrakSeri,
            "tarih" to tarih,
            "depoKodu" to depoKodu,
            "temsilciKodu" to temsilciKodu,
            "teslimTarihi" to teslimTarihi,
            "aciklama" to aciklama,
            "satirlar" to lines
        )
    }

    /**
     * 5. Tahsilat (collection)
     */
    fun buildCollection(
        cariKodu: String = "C001",
        tutar: Double = 1000.00,
        doviz: String = "TRY",
        tarih: String = isoDateFormat.format(Date()),
        odemeTipi: String = "Nakit",
        kasaBankaKodu: String = "KASA01",
        evrakSeri: String = "T",
        temsilciKodu: String = "PL01",
        aciklama: String = "Peşin tahsilat"
    ): Map<String, Any?> {
        return mapOf(
            "cariKodu" to cariKodu,
            "tutar" to tutar,
            "doviz" to doviz,
            "tarih" to tarih,
            "odemeTipi" to odemeTipi,
            "kasaBankaKodu" to kasaBankaKodu,
            "evrakSeri" to evrakSeri,
            "temsilciKodu" to temsilciKodu,
            "aciklama" to aciklama
        )
    }

    /**
     * 6. Tediye (payment)
     */
    fun buildPayment(
        cariKodu: String = "C001",
        tutar: Double = 500.00,
        doviz: String = "TRY",
        tarih: String = isoDateFormat.format(Date()),
        odemeTipi: String = "Nakit",
        kasaBankaKodu: String = "BANK01",
        evrakSeri: String = "TD",
        temsilciKodu: String = "PL01",
        aciklama: String = "Peşin ödeme"
    ): Map<String, Any?> {
        return mapOf(
            "cariKodu" to cariKodu,
            "tutar" to tutar,
            "doviz" to doviz,
            "tarih" to tarih,
            "odemeTipi" to odemeTipi,
            "kasaBankaKodu" to kasaBankaKodu,
            "evrakSeri" to evrakSeri,
            "temsilciKodu" to temsilciKodu,
            "aciklama" to aciklama
        )
    }

    /**
     * 7. Alış Faturası (purchase_invoice)
     */
    fun buildPurchaseInvoice(
        cariKodu: String = "T001",
        tutar: Double = 2000.00,
        doviz: String = "TRY",
        tarih: String = isoDateFormat.format(Date()),
        iade: Boolean = false,
        evrakSeri: String = "A",
        temsilciKodu: String = "PL01",
        belgeNo: String = "TG20250625001",
        aciklama: String = "Alış faturası"
    ): Map<String, Any?> {
        return mapOf(
            "cariKodu" to cariKodu,
            "tutar" to tutar,
            "doviz" to doviz,
            "tarih" to tarih,
            "iade" to iade,
            "evrakSeri" to evrakSeri,
            "temsilciKodu" to temsilciKodu,
            "belgeNo" to belgeNo,
            "aciklama" to aciklama
        )
    }

    /**
     * 8. Satış İadesi (sales_return)
     */
    fun buildSalesReturn(
        cariKodu: String = "C001",
        tutar: Double = 500.00,
        doviz: String = "TRY",
        tarih: String = isoDateFormat.format(Date()),
        evrakSeri: String = "SI",
        temsilciKodu: String = "PL01",
        belgeNo: String = "IADE001",
        aciklama: String = "Müşteri iade"
    ): Map<String, Any?> {
        return mapOf(
            "cariKodu" to cariKodu,
            "tutar" to tutar,
            "doviz" to doviz,
            "tarih" to tarih,
            "evrakSeri" to evrakSeri,
            "temsilciKodu" to temsilciKodu,
            "belgeNo" to belgeNo,
            "aciklama" to aciklama
        )
    }

    /**
     * 9. Alış İrsaliyesi (purchase_dispatch)
     */
    fun buildPurchaseDispatch(
        cariKodu: String = "T001",
        evrakSeri: String = "G",
        tarih: String = isoDateFormat.format(Date()),
        depoKodu: String = "D01",
        temsilciKodu: String = "PL01",
        satirlar: List<Map<String, Any?>>? = null
    ): Map<String, Any?> {
        val lines = satirlar ?: listOf(
            mapOf(
                "stokKodu" to "STK001",
                "miktar" to 20.0,
                "birimFiyat" to 80.0,
                "kdvOrani" to 20.0,
                "birim" to "ADET",
                "aciklama" to "Alış irsaliyesi"
            )
        )
        return mapOf(
            "cariKodu" to cariKodu,
            "evrakSeri" to evrakSeri,
            "tarih" to tarih,
            "depoKodu" to depoKodu,
            "temsilciKodu" to temsilciKodu,
            "satirlar" to lines
        )
    }

    /**
     * 10. Depo Transferi (warehouse_transfer)
     */
    fun buildWarehouseTransfer(
        evrakSeri: String = "T",
        tarih: String = isoDateFormat.format(Date()),
        kaynakDepoKodu: String = "D01",
        hedefDepoKodu: String = "D02",
        temsilciKodu: String = "PL01",
        nakliyedurumu: Int = 0,
        satirlar: List<Map<String, Any?>>? = null
    ): Map<String, Any?> {
        val lines = satirlar ?: listOf(
            mapOf(
                "stokKodu" to "STK001",
                "miktar" to 50.0,
                "birimFiyat" to 100.0,
                "birim" to "ADET",
                "aciklama" to "Transfer"
            )
        )
        return mapOf(
            "evrakSeri" to evrakSeri,
            "tarih" to tarih,
            "kaynakDepoKodu" to kaynakDepoKodu,
            "hedefDepoKodu" to hedefDepoKodu,
            "temsilciKodu" to temsilciKodu,
            "nakliyedurumu" to nakliyedurumu,
            "satirlar" to lines
        )
    }

    /**
     * 11. Stok Sayımı (stock_count)
     */
    fun buildStockCount(
        evrakSeri: String = "S",
        tarih: String = isoDateFormat.format(Date()),
        depoKodu: String = "D01",
        temsilciKodu: String = "PL01",
        satirlar: List<Map<String, Any?>>? = null
    ): Map<String, Any?> {
        val lines = satirlar ?: listOf(
            mapOf(
                "stokKodu" to "STK001",
                "sayilanMiktar" to 98.0,
                "birim" to "ADET",
                "aciklama" to "Sayım farkı"
            )
        )
        return mapOf(
            "evrakSeri" to evrakSeri,
            "tarih" to tarih,
            "depoKodu" to depoKodu,
            "temsilciKodu" to temsilciKodu,
            "satirlar" to lines
        )
    }

    /**
     * 12. Yeni Cari (new_customer)
     */
    fun buildNewCustomer(
        cariKodu: String = "C999",
        unvan: String = "Örnek Müşteri Ltd. Şti.",
        vergiNo: String = "1234567890",
        vergiDairesi: String = "Kadıköy V.D.",
        temsilciKodu: String = "PL01",
        bolgeKodu: String = "B01",
        grupKodu: String = "G01",
        varsayilanDepoKodu: String = "D01",
        email: String = "info@ornek.com",
        cepTel: String = "05321234567",
        adresler: List<Map<String, Any?>>? = null
    ): Map<String, Any?> {
        val addressList = adresler ?: listOf(
            mapOf(
                "adres" to "Atatürk Cad. No:1 Kadıköy",
                "il" to "İstanbul",
                "ilce" to "Kadıköy",
                "ulke" to "TR",
                "telefon" to "02163331234",
                "eposta" to "sube1@ornek.com"
            )
        )
        return mapOf(
            "cariKodu" to cariKodu,
            "unvan" to unvan,
            "vergiNo" to vergiNo,
            "vergiDairesi" to vergiDairesi,
            "temsilciKodu" to temsilciKodu,
            "bolgeKodu" to bolgeKodu,
            "grupKodu" to grupKodu,
            "varsayilanDepoKodu" to varsayilanDepoKodu,
            "email" to email,
            "cepTel" to cepTel,
            "adresler" to addressList
        )
    }

    /**
     * 13. Tahsilat — Ödeme Emri (payment_order)
     */
    fun buildPaymentOrder(
        odemeTipi: String = "Havale",
        sahipCariKodu: String = "C001",
        neredeCariKodu: String = "BANK01",
        tutar: Double = 1000.00,
        doviz: String = "TRY",
        tarih: String = isoDateFormat.format(Date()),
        vade: String = isoDateFormat.format(Date(System.currentTimeMillis() + 86400000 * 5)),
        referansNo: String = "EFT20250625001",
        bankaNo: String = "TR1200064000000123456789",
        tcmbBankaKodu: String = "0064",
        tcmbSubeKodu: String = "00001",
        aciklama: String = "Havale"
    ): Map<String, Any?> {
        return mapOf(
            "odemeTipi" to odemeTipi,
            "sahipCariKodu" to sahipCariKodu,
            "neredeCariKodu" to neredeCariKodu,
            "tutar" to tutar,
            "doviz" to doviz,
            "tarih" to tarih,
            "vade" to vade,
            "referansNo" to referansNo,
            "bankaNo" to bankaNo,
            "tcmbBankaKodu" to tcmbBankaKodu,
            "tcmbSubeKodu" to tcmbSubeKodu,
            "aciklama" to aciklama
        )
    }

    /**
     * 14. Ziyaret (visit)
     */
    fun buildVisit(
        cariKodu: String = "C001",
        girisZamani: String = isoDateFormat.format(Date(System.currentTimeMillis() - 2700000)),
        cikisZamani: String = isoDateFormat.format(Date()),
        aciklama: String = "Ürün tanıtımı yapıldı",
        enlem: Double = 40.9876,
        boylam: Double = 29.1234
    ): Map<String, Any?> {
        return mapOf(
            "cariKodu" to cariKodu,
            "girisZamani" to girisZamani,
            "cikisZamani" to cikisZamani,
            "aciklama" to aciklama,
            "enlem" to enlem,
            "boylam" to boylam
        )
    }

    /**
     * 15. Gün Oturumu (day_session)
     */
    fun buildDaySession(
        tip: String = "open",
        zaman: String = isoDateFormat.format(Date()),
        enlem: Double = 40.9876,
        boylam: Double = 29.1234,
        not: String = "Gün başlangıcı, rotayı planla"
    ): Map<String, Any?> {
        return mapOf(
            "tip" to tip,
            "zaman" to zaman,
            "enlem" to enlem,
            "boylam" to boylam,
            "not" to not
        )
    }

    /**
     * 16a. Müşteri Notu (customer_note)
     */
    fun buildCustomerNote(
        cariKodu: String = "C001",
        not: String = "Müşteri yeni ürün talep etti.",
        tip: String = "genel"
    ): Map<String, Any?> {
        return mapOf(
            "cariKodu" to cariKodu,
            "not" to not,
            "tip" to tip
        )
    }

    /**
     * 16b. Müşteri Konum (customer_location)
     */
    fun buildCustomerLocation(
        cariKodu: String = "C001",
        enlem: Double = 40.9876,
        boylam: Double = 29.1234,
        adres: String = "Güncellenmiş adres",
        tip: String = "teslimat"
    ): Map<String, Any?> {
        return mapOf(
            "cariKodu" to cariKodu,
            "enlem" to enlem,
            "boylam" to boylam,
            "adres" to adres,
            "tip" to tip
        )
    }

    /**
     * Main builder function that generates a complete envelope mapping for any document type.
     */
    fun generatePayloadFor(
        docType: String,
        selectedCariKodu: String? = null,
        selectedStokKodu: String? = null,
        customAmount: Double? = null,
        customSeri: String? = null,
        customDepo: String? = null,
        customUser: String? = null,
        customTenant: String? = null
    ): Map<String, Any?> {
        val finalCari = selectedCariKodu ?: (AppDataStore.customers.firstOrNull()?.id ?: "C001")
        val finalStok = selectedStokKodu ?: (AppDataStore.products.firstOrNull()?.code ?: "STK001")
        val finalAmount = customAmount ?: 1500.00
        val finalSeri = customSeri ?: ""
        val finalDepo = customDepo ?: "D01"
        val finalUser = customUser ?: "plasiyer1"
        val finalTenant = customTenant ?: "tenant-001"

        val documentTypeEnum = when (docType.trim().lowercase()) {
            "sales_invoice", "satış faturası" -> "sales_invoice"
            "sales_dispatch", "satış irsaliyesi" -> "sales_dispatch"
            "sales_order", "satış siparişi" -> "sales_order"
            "proforma_order", "proforma sipariş" -> "proforma_order"
            "collection", "tahsilat" -> "collection"
            "payment", "tediye" -> "payment"
            "purchase_invoice", "alış faturası" -> "purchase_invoice"
            "sales_return", "satış iadesi" -> "sales_return"
            "purchase_dispatch", "alış irsaliyesi" -> "purchase_dispatch"
            "warehouse_transfer", "depo transferi" -> "warehouse_transfer"
            "stock_count", "stok sayımı" -> "stock_count"
            "new_customer", "yeni cari" -> "new_customer"
            "payment_order", "ödeme emri" -> "payment_order"
            "visit", "ziyaret" -> "visit"
            "day_session", "gün oturumu" -> "day_session"
            "customer_note", "müşteri notu" -> "customer_note"
            "customer_location", "müşteri konum" -> "customer_location"
            else -> "sales_invoice"
        }

        val innerPayload: Map<String, Any?> = when (documentTypeEnum) {
            "sales_invoice" -> buildSalesInvoice(
                cariKodu = finalCari,
                evrakSeri = finalSeri.ifEmpty { "F" },
                depoKodu = finalDepo,
                satirlar = listOf(
                    mapOf(
                        "stokKodu" to finalStok,
                        "miktar" to 5.0,
                        "birimFiyat" to (finalAmount / 5.0),
                        "kdvOrani" to 20.0,
                        "birim" to "ADET",
                        "aciklama" to "Saha Otomasyon Satış Faturası Kalemi"
                    )
                )
            )
            "sales_dispatch" -> buildSalesDispatch(
                cariKodu = finalCari,
                evrakSeri = finalSeri.ifEmpty { "I" },
                depoKodu = finalDepo,
                satirlar = listOf(
                    mapOf(
                        "stokKodu" to finalStok,
                        "miktar" to 5.0,
                        "birim" to "ADET",
                        "siparisSatirNo" to 1
                    )
                )
            )
            "sales_order" -> buildSalesOrder(
                cariKodu = finalCari,
                evrakSeri = finalSeri.ifEmpty { "S" },
                depoKodu = finalDepo,
                satirlar = listOf(
                    mapOf(
                        "stokKodu" to finalStok,
                        "miktar" to 10.0,
                        "birimFiyat" to (finalAmount / 10.0),
                        "birim" to "ADET",
                        "aciklama" to "Mobil Sipariş Girişi"
                    )
                )
            )
            "proforma_order" -> buildProformaOrder(
                cariKodu = finalCari,
                evrakSeri = finalSeri.ifEmpty { "P" },
                depoKodu = finalDepo,
                satirlar = listOf(
                    mapOf(
                        "stokKodu" to finalStok,
                        "miktar" to 2.0,
                        "birimFiyat" to (finalAmount / 2.0),
                        "birim" to "ADET",
                        "aciklama" to "Teklif Satış Kalemi"
                    )
                )
            )
            "collection" -> buildCollection(
                cariKodu = finalCari,
                tutar = finalAmount,
                evrakSeri = finalSeri.ifEmpty { "T" },
                odemeTipi = "Nakit",
                kasaBankaKodu = "KASA01"
            )
            "payment" -> buildPayment(
                cariKodu = finalCari,
                tutar = finalAmount,
                evrakSeri = finalSeri.ifEmpty { "TD" },
                odemeTipi = "Nakit",
                kasaBankaKodu = "BANK01"
            )
            "purchase_invoice" -> buildPurchaseInvoice(
                cariKodu = finalCari,
                tutar = finalAmount,
                evrakSeri = finalSeri.ifEmpty { "A" }
            )
            "sales_return" -> buildSalesReturn(
                cariKodu = finalCari,
                tutar = finalAmount,
                evrakSeri = finalSeri.ifEmpty { "SI" }
            )
            "purchase_dispatch" -> buildPurchaseDispatch(
                cariKodu = finalCari,
                evrakSeri = finalSeri.ifEmpty { "G" },
                depoKodu = finalDepo,
                satirlar = listOf(
                    mapOf(
                        "stokKodu" to finalStok,
                        "miktar" to 15.0,
                        "birimFiyat" to (finalAmount / 15.0),
                        "kdvOrani" to 20.0,
                        "birim" to "ADET",
                        "aciklama" to "Alış İade/Giriş İrsaliyesi"
                    )
                )
            )
            "warehouse_transfer" -> buildWarehouseTransfer(
                evrakSeri = finalSeri.ifEmpty { "T" },
                kaynakDepoKodu = finalDepo,
                hedefDepoKodu = if (finalDepo == "D01") "D02" else "D01",
                satirlar = listOf(
                    mapOf(
                        "stokKodu" to finalStok,
                        "miktar" to 25.0,
                        "birimFiyat" to 0.0,
                        "birim" to "ADET",
                        "aciklama" to "Araç Depolar Arası Sevk"
                    )
                )
            )
            "stock_count" -> buildStockCount(
                evrakSeri = finalSeri.ifEmpty { "S" },
                depoKodu = finalDepo,
                satirlar = listOf(
                    mapOf(
                        "stokKodu" to finalStok,
                        "sayilanMiktar" to 85.0,
                        "birim" to "ADET",
                        "aciklama" to "Ay Sonu Sayım Düzeltmesi"
                    )
                )
            )
            "new_customer" -> buildNewCustomer(
                cariKodu = "C" + (100 + (Math.random() * 900).toInt()),
                unvan = "Yeni Eklenen Cari Limitli Şti",
                vergiNo = "345" + (1000000 + (Math.random() * 9000000).toInt())
            )
            "payment_order" -> buildPaymentOrder(
                sahipCariKodu = finalCari,
                tutar = finalAmount
            )
            "visit" -> buildVisit(
                cariKodu = finalCari
            )
            "day_session" -> buildDaySession()
            "customer_note" -> buildCustomerNote(
                cariKodu = finalCari
            )
            "customer_location" -> buildCustomerLocation(
                cariKodu = finalCari
            )
            else -> buildSalesInvoice(cariKodu = finalCari)
        }

        return buildEnvelope(
            documentType = documentTypeEnum,
            payload = innerPayload,
            tenantId = finalTenant,
            userCode = finalUser
        )
    }
}

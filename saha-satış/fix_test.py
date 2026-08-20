import re

with open("app/src/test/java/com/example/sync/PaginatedSyncTest.kt", "r") as f:
    content = f.read()

# Replace CariHareketResponse and DTOs with correct parameters
content = content.replace(
    "Response.success(CariHareketResponse(items = emptyList(), total = totalRecords))",
    'Response.success(CariHareketResponse(entity="cariHareketleri", cariKod="", page=page, pageSize=pageSize, total=totalRecords, since=null, items = emptyList()))'
)

content = content.replace(
    "Response.success(CariHareketResponse(items = items, total = totalRecords))",
    'Response.success(CariHareketResponse(entity="cariHareketleri", cariKod="", page=page, pageSize=pageSize, total=totalRecords, since=null, items = items))'
)

content = content.replace(
    """                    CariHareketiDto(
                        id = "TX-$globalIdx",
                        cariKod = "C1",
                        tarih = "2026-08-11",
                        tutar = 100.0,
                        aciklama = "Test $globalIdx"
                    )""",
    """                    CariHareketiDto(
                        id = "TX-$globalIdx",
                        cariKod = "C1",
                        tarih = "2026-08-11",
                        tutar = 100.0,
                        aciklama = "Test $globalIdx",
                        erpRef = "", erp = "", evrakTip = 0, evrakNo = "", tip = 0, borcMu = true, updatedAt = ""
                    )"""
)

content = content.replace(
    """                    FaturaHareketDto(
                        erpRef = "ERP-$globalIdx",
                        erp = "ERP",
                        cariKod = "C1",
                        evrakNo = "FT-$globalIdx",
                        satirlar = listOf(FaturaSatirDto(stokKod = "STK-1", miktar = 1.0, tutar = 50.0, tip = 1, cins = 1, tarih = "2026", girisMiktar = 0.0, cikisMiktar = 0.0, birimFiyat = 50.0, vergi = 0.0, girisDepoNo = 1, cikisDepoNo = 1, aciklama = "", updatedAt = ""))
                    )""",
    """                    FaturaHareketDto(
                        erpRef = "ERP-$globalIdx",
                        erp = "ERP",
                        cariKod = "C1",
                        evrakNo = "FT-$globalIdx",
                        tarih = "2026", evrakTip = 0, tip = 0, tutar = 50.0, updatedAt = "",
                        satirlar = listOf(FaturaSatirDto(erpRef = "", stokKod = "STK-1", miktar = 1.0, tutar = 50.0, tip = 1, cins = 1, tarih = "2026", girisMiktar = 0.0, cikisMiktar = 0.0, birimFiyat = 50.0, vergi = 0.0, girisDepoNo = 1, cikisDepoNo = 1, aciklama = "", updatedAt = ""))
                    )"""
)

with open("app/src/test/java/com/example/sync/PaginatedSyncTest.kt", "w") as f:
    f.write(content)


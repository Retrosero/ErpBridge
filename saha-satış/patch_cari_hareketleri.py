import re

with open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "r") as f:
    content = f.read()

def patch_sync_cari(content):
    parts = content.split("fun syncCariHareketleri(")
    if len(parts) < 2: return content
    
    before = parts[0]
    after = parts[1]
    
    target = "lastFingerprint = currentFingerprint"
    
    insertion = """lastFingerprint = currentFingerprint
                        
                        val chEntities = items.map { dto ->
                            com.example.data.database.CariHesapHareketEntity(
                                id = dto.id ?: dto.erpRef ?: java.util.UUID.randomUUID().toString(),
                                cariKod = dto.cariKod ?: "",
                                tarih = dto.tarih ?: "",
                                evrakTip = dto.evrakTip ?: 0,
                                evrakNo = dto.evrakNo ?: "",
                                tip = dto.tip ?: 0,
                                tutar = dto.tutar ?: 0.0,
                                borcMu = dto.borcMu ?: false,
                                aciklama = dto.aciklama ?: ""
                            )
                        }
                        db.withTransaction {
                            chEntities.chunked(500).forEach { db.cariHesapHareketDao().insertAll(it) }
                        }
"""
    after = after.replace(target, insertion, 1)
    
    return before + "fun syncCariHareketleri(" + after

content = patch_sync_cari(content)

def patch_sync_stok(content):
    parts = content.split("fun syncStokHareketleri(")
    if len(parts) < 2: return content
    
    before = parts[0]
    after = parts[1]
    
    target = "val grouped = items.groupBy { it.stokKod ?: it.urunKod ?: \"\" }.filterKeys { it.isNotEmpty() }"
    
    insertion = """
                                        val shEntities = items.map { dto ->
                                            com.example.data.database.StokHareketEntity(
                                                id = dto.id ?: dto.erpRef ?: java.util.UUID.randomUUID().toString(),
                                                stokKod = dto.stokKod ?: dto.urunKod ?: "",
                                                tarih = dto.tarih ?: "",
                                                tip = dto.tip ?: 0,
                                                evrakTip = dto.evrakTip ?: 0,
                                                evrakNo = dto.evrakNo ?: "",
                                                miktar = dto.miktar ?: (if ((dto.girisMiktar ?: 0.0) > 0) dto.girisMiktar else dto.cikisMiktar) ?: 0.0,
                                                birimFiyat = dto.birimFiyat ?: 0.0,
                                                tutar = dto.tutar ?: 0.0,
                                                cariKod = dto.cariKod ?: "",
                                                depoNo = dto.cikisDepoNo ?: dto.girisDepoNo ?: 0
                                            )
                                        }
                                        val dbSyncStok = com.example.data.database.DatabaseProvider.getDatabase(context.applicationContext)
                                        dbSyncStok.withTransaction {
                                            shEntities.chunked(500).forEach { dbSyncStok.stokHareketDao().insertAll(it) }
                                        }

                                        val grouped = items.groupBy { it.stokKod ?: it.urunKod ?: "" }.filterKeys { it.isNotEmpty() }
"""
    after = after.replace(target, insertion, 1)
    
    return before + "fun syncStokHareketleri(" + after

content = patch_sync_stok(content)

with open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "w") as f:
    f.write(content)


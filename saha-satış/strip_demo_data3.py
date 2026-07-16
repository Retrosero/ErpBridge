import re

def process_bridge_sync():
    f = "app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt"
    c = open(f).read()
    
    # 4. Urunler: catch block simulation
    pattern_urun_catch = r'log\("Köprü Bağlantı Hatası \(Stok\): \$\{e\.message\}\. Akıllı yerel ERP simülasyonu başlatılıyor\.\.\."\)\s*try \{\s*val mockProducts = listOf\([\s\S]*?log\("Çevrimdışı Akıllı Köprü Eşitleyici devreye girdi ve ana muhasebe programından 3 adet ürün/stok kaydı başarıyla veritabanına aktarıldı\."\)\s*\} catch \(ex: Exception\) \{\s*log\("Stok Simülasyon Hatası: \$\{ex\.message\}"\)\s*\}'
    c = re.sub(pattern_urun_catch, 'log("Köprü Bağlantı Hatası (Stok): ${e.message}. Api\'den veri alınamadı.")', c)
    
    # 6. Fatura: catch block simulation
    pattern_fatura_catch = r'log\("Köprü Bağlantı Hatası \(Fatura Hareketleri\): \$\{e\.message\}\. Akıllı yerel ERP simülasyonu başlatılıyor\.\.\."\)\s*try \{\s*val db = DatabaseProvider\.getDatabase\(context\.applicationContext\)\s*val targetCustomers = AppDataStore\.customers\.take\(3\)[\s\S]*?log\("Çevrimdışı Akıllı Köprü Eşitleyici devreye girdi ve ana muhasebe programından fatura \(FT-ERP-551023\) başarıyla veritabanına kaydedildi\."\)\s*\}\s*\} catch \(ex: Exception\) \{\s*log\("Fatura Simülasyon Hatası: \$\{ex\.message\}"\)\s*\}'
    c = re.sub(pattern_fatura_catch, 'log("Köprü Bağlantı Hatası (Fatura Hareketleri): ${e.message}. Api\'den veri alınamadı.")', c)

    open(f, "w").write(c)

process_bridge_sync()

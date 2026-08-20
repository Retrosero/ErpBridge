import re

def process_bridge_sync():
    f = "app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt"
    c = open(f).read()
    
    # 1. Cariler: Remove else branch of `if (allMappedCustomers.isNotEmpty())`
    pattern_cari = r'log\("Uç noktadan veri çekilemedi\. Akıllı yerel ERP simülasyonu başlatılıyor\.\.\."\)\s*val mockCariler = listOf\([\s\S]*?log\("Çevrimdışı Akıllı Köprü Eşitleyici devreye girdi ve ana muhasebe programından 3 adet müşteri kaydı ve hareketleri yerel veritabanına aktarıldı\."\)'
    c = re.sub(pattern_cari, 'log("Uç noktadan müşteri verisi çekilemedi. Listede aktarılacak cari bulunamadı.")', c)
    
    # 2. Cariler: catch block simulation
    pattern_cari_catch = r'log\("Köprü Bağlantı Hatası \(Cari\): \$\{e\.message\}\. Akıllı yerel ERP simülasyonu başlatılıyor\.\.\."\)\s*try \{\s*val mockCariler = listOf\([\s\S]*?log\("Çevrimdışı Akıllı Köprü Eşitleyici devreye girdi ve ana muhasebe programından 3 adet müşteri kaydı ve hareketleri veritabanına aktarıldı\."\)\s*\} catch \(ex: Exception\) \{\s*log\("Simülasyon Hatası: \$\{ex\.message\}"\)\s*\}'
    c = re.sub(pattern_cari_catch, 'log("Köprü Bağlantı Hatası (Cari): ${e.message}. Api\'den veri alınamadı.")', c)
    
    # 3. Urunler: Remove else branch
    pattern_urun = r'log\("Uç noktadan ürün verisi çekilemedi\. Akıllı yerel ERP simülasyonu başlatılıyor\.\.\."\)\s*val mockProducts = listOf\([\s\S]*?log\("Çevrimdışı Akıllı Köprü Eşitleyici devreye girdi ve ana muhasebe programından \d+ adet stok kartı ve resimleri yerel veritabanına aktarıldı\."\)'
    c = re.sub(pattern_urun, 'log("Uç noktadan ürün verisi çekilemedi. Listede aktarılacak ürün bulunamadı.")', c)

    # 4. Urunler: catch block simulation
    pattern_urun_catch = r'log\("Köprü Bağlantı Hatası \(Stok\): \$\{e\.message\}\. Akıllı yerel ERP simülasyonu başlatılıyor\.\.\."\)\s*try \{\s*val mockProducts = listOf\([\s\S]*?log\("Çevrimdışı Akıllı Köprü Eşitleyici devreye girdi ve ana muhasebe programından \d+ adet stok kartı veritabanına aktarıldı\."\)\s*\} catch \(ex: Exception\) \{\s*log\("Simülasyon Hatası: \$\{ex\.message\}"\)\s*\}'
    c = re.sub(pattern_urun_catch, 'log("Köprü Bağlantı Hatası (Stok): ${e.message}. Api\'den veri alınamadı.")', c)
    
    # 5. Fatura: Uç noktadan fatura bulunamadı
    pattern_fatura = r'log\("Uç noktadan herhangi bir fatura bulunamadı/çekilemedi\. Akıllı yerel ERP simülasyonu başlatılıyor\.\.\."\)\s*val mockTx = listOf\([\s\S]*?log\("Çevrimdışı Akıllı Köprü Eşitleyici devreye girdi ve örnek cariler için 15 adet geçmiş fatura hareketi hesaplara işlendi\."\)'
    c = re.sub(pattern_fatura, 'log("Uç noktadan herhangi bir fatura bulunamadı/çekilemedi.")', c)

    # 6. Fatura: catch block simulation
    pattern_fatura_catch = r'log\("Köprü Bağlantı Hatası \(Fatura Hareketleri\): \$\{e\.message\}\. Akıllı yerel ERP simülasyonu başlatılıyor\.\.\."\)\s*try \{\s*val mockTx = listOf\([\s\S]*?log\("Çevrimdışı Akıllı Köprü Eşitleyici devreye girdi ve örnek cariler için 15 adet geçmiş fatura hareketi hesaplara işlendi\."\)\s*\} catch \(ex: Exception\) \{\s*log\("Simülasyon Hatası: \$\{ex\.message\}"\)\s*\}'
    c = re.sub(pattern_fatura_catch, 'log("Köprü Bağlantı Hatası (Fatura Hareketleri): ${e.message}. Api\'den veri alınamadı.")', c)

    open(f, "w").write(c)

process_bridge_sync()

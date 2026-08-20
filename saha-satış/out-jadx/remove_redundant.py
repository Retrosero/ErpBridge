import re

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r', encoding='utf-8') as f:
    content = f.read()

# For CariKartlar
content = re.sub(
    r'if \(allMappedCustomers\.isNotEmpty\(\)\) \{\s*withContext\(Dispatchers\.Main\) \{\s*for \(mapped in allMappedCustomers\) \{.*?\}\s*\}\s*AppDataStore\.persist\(context\)\s*log\("Başarılı! Toplam \$totalFetched adet cari kayıt FieldOps Bridge üzerinden başarıyla çekildi"\)\s*\}',
    'log("Başarılı! Toplam $totalFetched adet cari kayıt FieldOps Bridge üzerinden başarıyla çekildi")',
    content,
    flags=re.DOTALL
)

# For Stok Kartları
content = re.sub(
    r'if \(allMappedProducts\.isNotEmpty\(\)\) \{\s*withContext\(Dispatchers\.Main\) \{\s*for \(u in allMappedProducts\) \{.*?\}\s*\}\s*AppDataStore\.persist\(context\)\s*log\("Saha Gücü yerel stok kartları Room veritabanı başarıyla güncellendi\. Toplam \$totalFetched adet ürün/stok kaydı çekildi\."\)\s*\}',
    'log("Saha Gücü yerel stok kartları Room veritabanı başarıyla güncellendi. Toplam $totalFetched adet ürün/stok kaydı çekildi.")',
    content,
    flags=re.DOTALL
)

# For Cari Hareketleri
content = re.sub(
    r'if \(allTx\.isNotEmpty\(\)\) \{\s*withContext\(Dispatchers\.Main\) \{.*?\}\s*AppDataStore\.persist\(context\)\s*log\("Başarılı! Toplam \$totalFetched adet cari hareket çekildi\."\)\s*\}',
    'log("Başarılı! Toplam $totalFetched adet cari hareket çekildi.")',
    content,
    flags=re.DOTALL
)

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w', encoding='utf-8') as f:
    f.write(content)

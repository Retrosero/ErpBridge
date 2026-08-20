import re

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('log("Başarılı! Toplam $totalFetched adet cari kayıt FieldOps Bridge üzerinden başarıyla çekildi") else {', 'log("Başarılı! Toplam $totalFetched adet cari kayıt FieldOps Bridge üzerinden başarıyla çekildi")\n            if(totalFetched == 0) {')

content = content.replace('log("Saha Gücü yerel stok kartları Room veritabanı başarıyla güncellendi. Toplam $totalFetched adet ürün/stok kaydı çekildi.") else {', 'log("Saha Gücü yerel stok kartları Room veritabanı başarıyla güncellendi. Toplam $totalFetched adet ürün/stok kaydı çekildi.")\n            if(totalFetched == 0) {')

content = content.replace('log("Başarılı! Toplam $totalFetched adet cari hareket çekildi.") else {', 'log("Başarılı! Toplam $totalFetched adet cari hareket çekildi.")\n            if(totalFetched == 0) {')

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w', encoding='utf-8') as f:
    f.write(content)

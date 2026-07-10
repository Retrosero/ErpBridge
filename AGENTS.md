# AGENTS.md — ErpBridge Working Rules

Bu dosya, ErpBridge projesinde çalışan her agent (Mavis / team worker / gelecekteki
oturumlar) için **bağlayıcı** çalışma kurallarını içerir. Aşağıdaki kurallara
uyulması zorunludur.

## 1. Kaynak önceliği

Bir karar verirken bu sırayla bak:

1. `/workspace/.skills/erpbridge/SKILL.md` — bağlayıcı proje kuralları
2. `/workspace/ErpBridge/docs/` — mimari ve API sözleşmeleri
3. `/workspace/ErpBridge/README.md` — proje tanıtımı
4. Kullanıcının son mesajı

## 2. Çalışma kuralları (Hard rules)

- **Küçük adımlar.** Büyük bir değişikliği tek commit'te değil, gözden geçirilebilir
  küçük commit'lerde yap.
- **Build asla kırık bırakılmaz.** Her commit sonrası `dotnet build` temiz olmalı.
- **Test ekle.** Yeni interface veya davranış ekleniyorsa, yanına test yaz.
- **SQL parametrik.** Kullanıcı/payload verisi asla string concat ile SQL'e girmeyecek.
- **ERP yazma transaction içinde.** Header + tüm satırlar + açıklamalar tek transaction.
- **Idempotent yazım.** Aynı `externalId` ikinci kez gelirse Mikro'da evrak oluşturulmaz.
- **V15/V16 farkı adapter'da.** Core / UI / Service bu farkı bilmez.
- **Secret loglanmaz.** SQL şifresi, lisans anahtarı, API token loglara düz metin olarak
  düşmez; UI'da `PasswordBox` ile alınır.

## 3. Faz sırası

| Faz | Hedef | Bu dosya | Sahip |
|-----|-------|----------|-------|
| 1   | Solution + proje iskeleti, DI, Serilog, SQLite infra, WPF shell, Service shell, ERP abstraction, Mikro skeleton, Remote API skeleton, örnek test | Hazır | developer |
| 2   | SQLite migrations + config kaydetme/okuma + WPF ayar ekranı | Planlandı | developer |
| 3   | Mikro bağlantı testi + V15/V16 detector | Planlandı | developer |
| 4   | Merkezi API iskeleti (Web API + PostgreSQL) | Planlandı | developer |
| 5   | Mikro bootstrap okuma (cari/stok/fiyat/depo/kasa/banka/plasiyer) | Planlandı | developer |
| 6   | Satış siparişi yazma (SIPARISLER) | Planlandı | developer |
| 7   | Admin panel | Planlandı | developer |

Her faz sonunda `dotnet build` ve `dotnet test` temiz olmalı. Bir sonraki faza
geçmeden önce mevcut faz kapanır.

## 4. Çıktı disiplini

- Her kod değişikliği için: değişen dosyalar + yeni testler + kısa davranış açıklaması.
- Her faz kapanışında: `deliverable.md` (changed files, tests, build output).
- Hiçbir zaman log'a secret düz metin düşürme.

## 5. Takım çalışması (Mavis Team)

- Paralel track'ler sadece **bağımsız paketler** arasında yapılır (örn. Core, LocalStore,
  Service+UI, Mikro). Bir paket içinde suni alt görevlere bölme.
- Verifier read-only'dir. Doğrulama için build + test çalıştırır; dosya düzenlemez.
- Verifier FAIL verirse, eksik kapsamı (unit/integration/E2E) tam olarak listeler.
- Aynı paket üzerinde iki worker görevlendirilmez — çakışma olur.
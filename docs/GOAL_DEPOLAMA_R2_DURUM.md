# GOAL_DEPOLAMA_R2 — Durum

Plan: `GOAL_DEPOLAMA_R2.md`. İşaretler: ⬜ bekliyor · 🔄 sürüyor · ✅ bitti · ⏭️ atlandı · ⛔ tıkandı.

## Yetkiler (kullanıcı, 2026-10-01)
Dala push, PR, `main`'e birleştirme (CI yeşilken; Actions kredisi yoksa yerel `dotnet build` 0 uyarı + `dotnet test`
yeşilken), Coolify dağıtımı ve `/health/schema` kontrolü, Sipariş Cepte Play **internal** yüklemesi önceden onaylı.
Asla: `--force` push, CI kırmızıyken (kredi dışı sebeple) birleştirme, `main`'e doğrudan kod push'u, Play production,
R2 API anahtarı değerlerini görmek/girmek (kullanıcı girer).

| # | Görev | Durum | PR | Not |
|---|---|---|---|---|
| D0 | R2 kovaları + `img.appsgo.cloud` | ✅ | | Kovalar, alan adı ve Coolify `Storage__*` ayarları (anahtarları kullanıcı girdi); canlı günlükte eksik ayar uyarısı yok |
| S1 | `FileStore`, `stored_files`, `tenant_storage`, R2 istemcisi | ✅ | #237 | R2 istemcisi, `FileStore`, defter, satır kilitli kota; aynı kova adı reddedilir; kalıcı silme yarışı kapalı |
| S2 | SkiaSharp küçültme/WebP | ✅ | #237 | SkiaSharp `ImageProcessor` (1280/400, banner 1920×720/800×300 WebP) |
| S3 | Katalog + banner → R2 | ✅ | #238 | Katalog görseli ve banner `FileStore`'da; bytea okuma S10'a kadar ikili |
| S4 | Görev eki → R2 (özel kova) | ✅ | #238 | Görev eki özel kovada; GET sunucu akıtır |
| S5 | Gider/araç fişi | ✅ | #238 | `expense_attachments`, fiş uçları, panel `/fisler` |
| S6 | Ürün fotoğrafı | ✅ | #238 | `product_images`, `action.products.photo`, panel Stok > Fotoğraflar, katalogda yedek görsel |
| S7 | XML görsel eşitleyici (değişen/silinen izlenir) | ✅ | #239 | `XmlImageSync` + `SafeHttpFetcher` (DNS sabitleme, elle yönlendirme), `xml_images`, status/sync uçları, katalog yedeği ve manifest `xmlItems`; bozuk/boş/eşleşmeyen feed hiçbir şey silmez |
| S8 | Kota/kullanım + Admin | ✅ | #237 | `GET /api/v1/storage/usage`, Admin kota kartı (0 GB dahil), günlük sayaç hesabı |
| S9 | Temizlik + bakım işi | ✅ | #240 | Çöp öğeleri + geri alma (sınırlarla), temizlik grupları, kalıcı silme, günlük çöp boşaltma, karantina (T4), haftalık R2 mutabakatı, denetim kaydı; migration `DepolamaTemizlik` |
| S10 | Bytea → R2 göçü | 🔄 göç (düşürme ayrı PR) | #241 | `Storage/BlobMigration`: katalog/banner/görev blobları bayt bayt R2'ye (kota denetimsiz ama sayılır), SHA-256 ile doğrulanır, kayda bağlanır; işçinin dakikalık turu (500 dosya), Admin `GET/POST /admin/storage/migration` (`readyToDrop`). Bloblar yerinde; tabloları düşüren migration üretimde `readyToDrop` sonrası ayrı PR |
| P1 | Panel Depolama sayfası | ✅ | #240 | `/depolama` (kota çubuğu %80 sarı/%95 kırmızı, alan dağılımı, Alan aç, çöp kutusu, XML eşitleme); Kullanıcılar'da depolama özeti; katalog kota rengi 80/95 |
| P2–P3 | Panel fiş/ürün fotoğrafı + Admin kartı | ✅ | #237, #238, #240 | Fişler `/fisler`, Stok > Fotoğraflar, katalog göstergesi birleşik kotada (80/95), Admin firma mobil sayfasında depolama kartı ve kota alanı |
| A1–A4 | Telefon | ✅ | siparis_cepte#159, #162 | 1.5.299 Play internal (Room 50): fiş ve ürün fotoğrafı sunucuya, kota mesajı, yerel temizlik, XML görselleri sunucu kopyasından |
| K1 | Bilgi bankası | ✅ | her PR | ErpBridge kural 28, 37 ve veri sözlüğü; Siparis_Cepte kural 40, 57 ve veri sözlüğü |

## Karar günlüğü
| Tarih | Karar | Gerekçe |
|---|---|---|
| 2026-10-01 | R1–R6, T1–T11 (plan §2); varsayılan kota 5 GB | Kullanıcı plan onayı |
| 2026-10-02 | S9: "stoksuz ürünler" grubu **şu an** stoksuz olanları alır (plandaki "N gündür stoksuz" değil) | Sunucu stok geçmişi tutmuyor; panel seçimle temizler, geri alma 7 gün açık |
| 2026-10-02 | S9: iptal edilmiş görevin kapanış zamanı = son değişiklik (`UpdatedAtMs`) | Görevde ayrı iptal zamanı yok; en geç o an iptal edilmiştir |
| 2026-10-02 | S9: plandaki "silinmiş giderlerin fişleri" grubu eklenmedi | Fiş silinince zaten çöpe gider (öğesiyle); sunucuda "silinmiş gider belgesi" bilgisi yok. Silinmiş fiş satırları 30 gün sonra silinir |
| 2026-10-02 | S9: kaydı olmayan dosyalar (süpürme, 30 günlük silinmiş görev) çöpte görünür ama geri alınamaz | Geri konacak kayıt yok; geri alınsa ertesi gün yine süpürülürdü |
| 2026-10-02 | S10: göç kotayı denetlemez (`FileStore.PutMigratedAsync`) ama baytı sayar | Bayt zaten firmanındı; göç kota yüzünden yarım kalırsa bytea tabloları hiç düşemez. Kota aşılırsa yalnız yeni yüklemeler durur |
| 2026-10-02 | S10: bayt yeniden kodlanmaz, üst veri de silinmez | Doğrulama nesnenin SHA-256'sının blobunkine eşit olmasına dayanır; bu baytlar yüklenirken zaten denetlenmişti |
| 2026-10-02 | S10: başarısız satırlar şema değişikliği olmadan süreç belleğinde tutulur | Başarısız satır zaten "kalan" sayılır, `readyToDrop`'u engeller; yeniden başlatmada bir kez daha denenir. Kalıcı durum tablosu gerekmedi |
| 2026-10-02 | S10: silinmiş görev resimleri taşınmaz | Kimse okumaz; çöp/30 gün kuralı onları zaten siler; blob tabloyla gider |

# ErpBridge — Çalışma Kuralları (Claude için)

## 1. Önce bilgi bankasını oku — İSTİSNASIZ

Bu projede **herhangi bir değişiklik yapmadan, plan çıkarmadan veya soru
yanıtlamadan önce** ilgili knowledge_base dosyalarını oku:

- `ErpBridge/ErpBridge_knowledge_base/` — bu projeye özel (mimari, adaptörler,
  Mikro şeması, evrak kuralları, AI SQL kataloğu)
- `../knowledge_base/` — ekosistem geneli (3 uygulama birlikte)
- Mobil tarafı ilgilendiren bir iş varsa `../Siparis_Cepte/Siparis_Cepte_knowledge_base/`

`INDEX.md` ile başla, sonra konuyla ilgili 00–04 dosyalarını aç. Şemayı,
kolon adlarını veya mimariyi **asla tahmin etme** — bilgi bankasında yoksa
canlı DB'den veya `Fora_Mikro/` referans uygulamasından doğrula.

Bir değişiklik bilgi bankasındaki bir bilgiyi eskitiyorsa, **aynı turda**
ilgili knowledge_base dosyasını da güncelle.

## 2. Git / branch akışı

- `main` daima yeşil ve deploy edilebilir kalır — doğrudan `main`'e commit yok.
- Her iş için `main`'den yeni bir dal aç: `git switch main && git pull && git switch -c faz-<n>-<kısa-ad>` (örn. `faz-21-logo-katalog`).
- İş bitince: build + testler yeşil → commit → `git push -u origin <dal>` → GitHub'da Pull Request aç → gözden geçir → `main`'e squash-merge → dalı sil.
- Bir dal tek bir konuya odaklansın. Alakasız düzeltme çıkarsa ayrı dal/PR.
- Commit mesajları Türkçe, ne + neden. Sonuna:
  `Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>`
- Push ve PR açma "onay gerektiren" işlerdir — kullanıcıya sormadan yapma.
- **İstisna (2026-09-16):** `docs/GOAL_PANEL_GELISTIRMELERI.md` görevleri için dala push, PR açma,
  CI yeşil ve inceleme yorumları çözülmüşken `main`'e squash-merge ve Sipariş Cepte sürümünü Play
  **internal** kanalına yükleme **önceden onaylıdır** (kullanıcı `/goal` ile müdahalesiz çalıştırmak istedi).
  Yalnız o belgenin görevlerini kapsar. Hiçbir koşulda: `--force` push, CI kırmızıyken merge, `main`'e
  doğrudan push, Play production yayını.
- **İstisna (2026-09-17):** aynı yetkiler `docs/GOAL_LOG_MERKEZI.md` görevleri için de önceden onaylıdır
  (kullanıcı: "hepsi geçerli"). Yalnız o belgenin görevlerini kapsar; aynı yasaklar geçerlidir.
- **İstisna (2026-09-17):** aynı yetkiler `docs/GOAL_ERP_YAZIM.md` görevleri için de önceden onaylıdır
  (kullanıcı: "Evet, hepsi geçerli"); ek olarak yalnız yerel test kopyalarına **`MikroDB_V15_DEMO`** (asıl; Mikro'dan açılır) ve
  `MikroDB_V15_ERPBTEST` test evrakı yazılabilir, gerekirse aynı yedekten yeniden geri yüklenebilir. Aynı yasaklar + `MikroDB_V15_02` (canlı firma
  verisi) ve diğer müşteri Mikro DB'lerine yazmak yasak.
- **İstisna (2026-09-19):** aynı yetkiler `docs/GOAL_ERP_YAZIM_2.md` (tediye · alış faturası · yeni cari) görevleri için de
  önceden onaylıdır (kullanıcı: "Evet, aynısı geçerli"). Test yazımı yalnız **`MikroDB_V15_DEMO`**'ya. Aynı yasaklar
  geçerli; **`--force` push yasağı rebase sonrası `--force-with-lease` için de geçerlidir** (dalı güncellemek gerekirse
  `origin/main` dala merge edilir).
- **İstisna (2026-09-19):** aynı yetkiler `docs/GOAL_ERP_YAZIM_3.md` (alış faturası · gider · sayım) görevleri
  için de önceden onaylıdır (kullanıcı: "onay veriyorum"). Test yazımı yalnız **`MikroDB_V15_DEMO`**'ya.
  Aynı yasaklar geçerli.
- **İstisna (2026-09-22):** aynı yetkiler `docs/GOAL_PANEL_ERPLI.md` (ERP'li firmalarda panelden ürün/cari
  kartı düzenleme) görevleri için de önceden onaylıdır (kullanıcı: "Önceden onayla"). Test yazımı yalnız
  **`MikroDB_V15_DEMO`**/`MikroDB_V15_ERPBTEST`'e. Aynı yasaklar geçerli: `--force` push (ve rebase sonrası
  `--force-with-lease`) yasak, CI kırmızıyken merge yasak, `main`'e doğrudan push yasak, `MikroDB_V15_02`
  (canlı firma verisi) ve diğer müşteri Mikro DB'lerine yazmak yasak. **Bu goal 2026-09-22'de kullanıcı
  önceliğiyle durduruldu** — bkz. aşağıdaki `GOAL_PANEL_ERPSIZ.md` istisnası.
- **İstisna (2026-09-22):** aynı yetkiler `docs/GOAL_PANEL_ERPSIZ.md` (ERP'siz firmalarda panelden ürün/cari/
  satış/alış/iade/tahsilat/tediye girişi ve düzenleme) görevleri için de önceden onaylıdır (kullanıcı: "Önceden
  onayla"). Aynı yasaklar geçerli: `--force` push (ve rebase sonrası `--force-with-lease`) yasak, CI kırmızıyken
  merge yasak, `main`'e doğrudan push yasak. Bu goal Mikro'ya hiç yazmadığı (`NativeDocumentProcessor` +
  CentralApi PostgreSQL) için Mikro test DB kısıtı bu istisna için geçerli değil.
- **İstisna (2026-09-24):** aynı yetkiler `docs/GOAL_PANEL_DUZELTMELER.md` (çıkış · tekrar onaya al · stok hızı ·
  cari bakiyesi) görevleri için de önceden onaylıdır (kullanıcı: "Evet, önceden onaylı"). Test yazımı yalnız
  **`MikroDB_V15_DEMO`**/`MikroDB_V15_ERPBTEST`'e. Aynı yasaklar geçerli: `--force` push (ve rebase sonrası
  `--force-with-lease`) yasak, CI kırmızıyken merge yasak, `main`'e doğrudan push yasak, `MikroDB_V15_02` (canlı firma
  verisi) ve diğer müşteri Mikro DB'lerine yazmak yasak.

- **İstisna (2026-09-25):** aynı yetkiler `docs/GOAL_GOREVLER.md` (görevler + bildirim sistemi; sunucu ve
  Sipariş Cepte ayağı) görevleri için de önceden onaylıdır (kullanıcı: "yayına sen al"). `main`'e birleştirme
  Coolify dağıtımını tetikler; dağıtımdan sonra `/health/schema` kontrol edilir. Aynı yasaklar geçerli:
  `--force` push (ve `--force-with-lease`) yasak, CI kırmızıyken merge yasak, `main`'e doğrudan push yasak,
  Play production yasak. Görevler ERP'ye yazmaz; Mikro DB'lerine hiçbir yazım yoktur.

## 3. Test / build

- .NET: `dotnet build ErpBridge.sln -c Debug` (0 uyarı / 0 hata) + `dotnet test ErpBridge.sln`
- Canlı Mikro şema testi: `ERPBridge_RUN_INTEGRATION=1` (opsiyonel, `MikroDB_V15_02`/`V16_03`)
- `TreatWarningsAsErrors=true` — uyarı bırakma.

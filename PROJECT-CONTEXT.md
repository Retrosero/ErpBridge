# ErpBridge — Proje Durum Özeti (Token-Dostu)

> **Amaç:** Bu dosya, herhangi bir AI asistanın (ChatGPT / Gemini / MiniMax) projeyi **baştan
> okumadan** hızlıca bağlam kurabilmesi için yazıldı. Tüm dokümanları değil, sadece
> **bilmesi gereken minimum karar-yerlerini** içerir.
>
> **Kullanım:** Yeni bir AI ile başlarken bu dosyayı + ihtiyaç duyulan tek deliverable'ı
> (örn. sıradaki faz için `deliverable-faz3.md`) yapıştır. Proje ağacını, README'yi veya
> `docs/` altını okutma.

---

## 0. Genel Bakış (30 saniye)

- **Proje:** ErpBridge — Windows Agent (WPF + Service) ↔ merkezi API ↔ Mikro ERP (V15/V16).
- **Dil/Platform:** C# / .NET 8 (Windows hedefli), SQLite (yerel) + PostgreSQL (merkez).
- **Mimari:** Katmanlı — `Abstractions` → `Core` → adapter'lar (Mikro). Service/UI sadece
  interface'leri görür; V15/V16 farkı sadece `ErpBridge.Erp.Mikro` içinde kalır.
- **Test disiplini:** Her commit sonrası `dotnet build` ve `dotnet test` temiz olmalı.
  Şu an **286/286 test PASSED + 16 SKIPPED** (Faz 7 sonu — proje tamamlandı).
- **Çalışma modu:** Mavis Team (paralel track'ler → owner GATE → sıradaki faz). Her faz
  kendi `deliverable-fazN.md` dosyasıyla kapanır.

---

## 1. Bağlayıcı Kurallar (Hard Rules)

Bunlar **asla** çiğnenmez:

1. **V15/V16 farkı adapter'da kalır.** Core / Service / UI / RemoteApi bu farkı bilmez.
2. **SQL her zaman parametrik.** Kullanıcı/payload verisi string concat ile SQL'e giremez.
3. **ERP yazma transaction içinde.** Header + satırlar + açıklamalar + mapping = tek transaction.
4. **Idempotent yazım.** Aynı `externalId` ikinci kez gelirse Mikro'da evrak **oluşturulmaz**,
   önceki Recno/Guid dönülür. Mapping store: `(tenant_id, entity_type, document_type, external_id)` UNIQUE.
5. **Secret loglanmaz.** SQL şifresi, lisans anahtarı, API token. UI'da `PasswordBox` ile alınır;
   `ConnectionStringMasker` + `AgentConfigMasker` ile süzülür.
6. **Build asla kırık bırakılmaz.** Her commit sonrası `dotnet build` temiz.
7. **Yeni interface'e test yanında.** Interface ekleniyorsa test de eklenir.
8. **Cross-reference katman kuralı:**
   - `ErpBridge.Erp.Abstractions` → sadece `Shared`
   - `ErpBridge.Core` → `Shared` (+ `Abstractions`)
   - `ErpBridge.LocalStore` → `Shared`, `Core`
   - `ErpBridge.RemoteApi` → `Shared`, `Core`
   - `ErpBridge.Erp.Mikro` → `Shared`, `Abstractions` (Service/UI'a bağımlı değil)
   - `ErpBridge.Agent.Service` → `Core`, `LocalStore`, `RemoteApi`, `Erp.Mikro`
   - `ErpBridge.Agent.UI` → `Core`, `LocalStore`, `RemoteApi`, `Erp.Mikro`
   - `ErpBridge.Erp.Abstractions` ve `ErpBridge.Core` → **SqlClient/Dapper/Polly paketi YOK**
9. **`TreatWarningsAsErrors=true`** her projede.

---

## 2. Çözüm Yapısı (Hızlı Ağaç)

```
ErpBridge/
├── ErpBridge.sln                      # 13 src/test projesi
├── global.json                        # SDK rollForward: latestMajor
├── AGENTS.md                          # bağlayıcı kurallar (her agent okumalı)
├── docs/
│   ├── architecture.md                # katman yapısı, referans izinleri
│   ├── development-roadmap.md         # Faz 1-7+ yol haritası
│   ├── mikro-v15-v16-rules.md         # RECno vs Guid stratejisi
│   └── api-contracts.md               # Central API HTTP sözleşmesi
├── src/
│   ├── ErpBridge.Shared/              # Result, Error, Hash, ConnectionStringMasker, AgentSettingsValidation
│   ├── ErpBridge.Erp.Abstractions/    # IErpAdapter, ErpConnectionTestResult (Mikro yok)
│   ├── ErpBridge.Core/                # Domain modelleri, store interface'leri, AgentConfigMasker
│   ├── ErpBridge.LocalStore/          # SQLite migrations + 4 store + ProtectedConfig (AES/DPAPI)
│   ├── ErpBridge.RemoteApi/           # HttpRemoteApiClient + Polly v8 retry + Idempotency-Key header
│   ├── ErpBridge.Erp.Mikro/           # V15/V16 strategy, ConnectionTestOrchestrator, writer iskeleti
│   ├── ErpBridge.Agent.Service/       # BackgroundService: AgentWorker + HeartbeatWorker
│   └── ErpBridge.Agent.UI/            # WPF ayar paneli (PasswordBox, AsyncRelayCommand, badge)
└── tests/
    ├── ErpBridge.Core.Tests/          # 35 test
    ├── ErpBridge.LocalStore.Tests/    # 44 test
    ├── ErpBridge.Erp.Mikro.Tests/     # 64 test (+ 5 integration skip)
    ├── ErpBridge.Shared.Tests/        # 21 test
    └── ErpBridge.RemoteApi.Tests/     # 10 test (HttpMessageHandler mock)
    + tests/docker-compose.test.yml    # MSSQL 2019 (V15) + MSSQL 2022 (V16) integration
    + tests/Integration/...            # env-var gated integration fixture
```

---

## 3. Tamamlanan Fazlar

| Faz | İçerik | Deliverable | Test |
|-----|--------|-------------|------|
| 1 | Solution iskeleti, DI, Serilog, SQLite infra, WPF/Service shell, ERP abstraction, Mikro skeleton, RemoteApi skeleton, örnek test | `deliverable-faz1.md` | 86/86 |
| 2 | SQLite migrations + config kaydetme/okuma + WPF ayar ekranı + AES-GCM/DPAPI ile encrypt-at-rest + ConnectionStringMasker | `deliverable-faz2.md` | 153/153 (+ 1 skip) |
| 3 | Mikro bağlantı testi + V15/V16 detector + WPF badge + integration test altyapısı (docker MSSQL) | `deliverable-faz3.md` | 174/174 (+ 5 skip) |
| 4 | Merkezi API iskeleti (.NET 8 Web API + PostgreSQL, JWT, 6 endpoint) | `deliverable-faz4.md` | 209/209 (+ 5 skip) |
| 5 | Mikro bootstrap okuma (cari/stok/fiyat/depo/kasa/banka + SyncPackage payload tipleri) | `deliverable-faz5.md` | 226/226 (+ 5 skip) |
| 6 | Satış siparişi yazma (MikroSalesOrderWriter gerçek INSERT, V15 RECno / V16 Guid, transactional, AgentWorker → adapter → ack, 17 proje) | `deliverable-faz6.md` | 252/252 (+ 16 skip) |
| 7 | Admin panel (Blazor Server + CentralApi admin backend: AdminUser, login, 8 admin endpoint, RBAC, 11 panel test) | `deliverable-faz7.md` | **286/286 (+ 16 skip)** |

---

## 4. Proje Durumu

**Tüm 7 faz tamamlandı.** Proje teslim edilebilir.

### Faz 7 — Admin panel (Web) ✅ TAMAMLANDI

- Yeni proje: `src/ErpBridge.Admin/` (Blazor Server) + `tests/ErpBridge.Admin.Tests/`.
- 9 sayfa: Login, Logout, Dashboard, Tenants (+Create), Licenses, Agents, Jobs, Bootstrap.
- Typed HttpClient (`CentralApiClient`) — 11 method, 401 → token clear.
- TokenStore (in-memory singleton) + AdminAuthStateProvider (scope=admin claim).
- Cross-ref: Admin → sadece Shared. HTTP-only, CentralApi'ye ref yok.
- RBAC: AdminPolicy (scope=admin) + AgentPolicy (scope=agent) izole. scope mismatch → 403.

### Faz 8+ — Sonraki olası modüller (roadmap)

- Tahsilat (`CARI_HESAP_HAREKETLERI` + `ODEME_EMIRLERI`)
- İrsaliye (`STOK_HAREKETLERI`)
- Fatura (`CARI_HESAP_HAREKETLERI` + `STOK_HAREKETLERI`)
- Cari kart açma / Stok kart açma
- Logo / Paraşüt / Netsis adapter
- Admin refresh token flow
- Cross-DB atomicity reconciliation (Faz 6 sınırı)

### Faz 5 — Mikro bootstrap okuma (cari/stok/fiyat/depo/kasa/banka/plasiyer) ✅ TAMAMLANDI

- `IMikroAdapter.ReadBootstrapDataAsync` gerçek implementasyonu.
- `SyncPackage` → merkezi API'ye `PushBootstrapDataAsync`.
- V15/V16 dispatcher hâlâ Erp.Mikro içinde.

### Faz 6 — Satış siparişi yazma (`SIPARISLER`) ✅ TAMAMLANDI

- `MikroSalesOrderWriter` artık gerçek INSERT (tek transaction).
- V15: `SCOPE_IDENTITY()` ile RECno + `*_RECid_RECno` linkleri.
- V16: Guid insert + `*_uid` linki.
- Transaction içinde header + lines + mapping save.
- AgentWorker artık real ERP write ack gönderiyor (sales_order branch).
- Bilinen sınır: cross-DB atomicity yok (SQLite mapping + SQL Server tx ayrı); reconciliation ileride.

### Faz 7 — Admin panel ← **SIRADAKİ**

---

## 5. V15 vs V16 — Tek Sayfa Karar Tablosu

| Konu | V15 | V16 |
|------|-----|-----|
| Primary key | `RECno INT IDENTITY` | `Guid UNIQUEIDENTIFIER` |
| Tablo deseni | `TABLO`, `TABLO_RECid` (lookup) | `TABLO`, `TABLO_uid` (lookup) |
| Link kolonu | `IliskiliTablo_RECid_RECno` | `IliskiliTablo_uid` |
| Id strategy | `SCOPE_IDENTITY()` | `NEWID()` veya client-üretilmiş Guid |
| Detection | DB metadata probe (RECno kolonu var mı?) | DB metadata probe (Guid PK mı?) |
| Unknown | major.minor parse edilemedi → Unknown (lookup'tan recno alınamaz) | aynı |

Detay: `docs/mikro-v15-v16-rules.md`.

---

## 6. Idempotency Mapping Kuralı

`external_id` (remote tarafın gönderdiği dış kimlik) → `(tenant_id, entity_type, document_type, external_id)`
UNIQUE index. Tekrar gelirse INSERT yapılmaz, önceki dahili id (V15 RECno / V16 Guid) dönülür.

Mapping tablosu: `idempotency_mapping` (Core'un store interface'i, LocalStore'da SQLite implementasyonu).

**Faz 6 notu:** Mapping save Writer tarafından (SQL Server transaction commit'ten sonra) yapılıyor;
mapping save başarısız olursa Mikro tarafında evrak oluşur ama mapping kaydı yoktur → sonraki
retry idempotency hit bulamaz. Bu bilinen sınır; reconciliation track'i ileride.

---

## 7. Logging & Secret Politikası

- `ConnectionStringMasker` (Shared): `Password`/`Pwd`/`User ID`/`UID` anahtarlarını
  `***REDACTED***` ile değiştirir. Case-insensitive, multi-equals güvenli.
- `AgentConfigMasker` (Core): log için `AgentConfig` klonu, `is_secret=1` alanları `********`.
- `IProtectedConfigProvider`: secret alanlar SQLite'ta düz metin değil, encrypt-at-rest.
  - Windows: DPAPI (`DpapiProtectedConfigProvider`)
  - Linux/macOS: AES-256-GCM (`AesProtectedConfigProvider`)
- Key dosyası: `KeyStore` — Windows `Hidden` attribute, Linux/macOS `0600` izni.
- Serilog config ile production'da secret property'ler maskelenir; **asla** düz metin loglanmaz.

---

## 8. Test & Build Komutları

```bash
# Build (her zaman temiz olmalı)
dotnet build ErpBridge.sln -p:EnableWindowsTargeting=true

# Test (varsayılan: integration skip'li)
dotnet test ErpBridge.sln -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor

# Integration test (MSSQL docker ayağa kalktıktan sonra)
docker compose -f tests/docker-compose.test.yml up -d
ERPBridge_RUN_INTEGRATION=1 dotnet test ErpBridge.sln -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor
```

---

## 9. Yeni AI'a Minimum Yükleme Protokolü

1. Bu dosyayı (`PROJECT-CONTEXT.md`) yapıştır.
2. Üzerinde çalışılacak fazın `deliverable-fazN.md`'sini yapıştır.
3. Spesifik görev için ilgili source dosyalarını ek olarak yapıştır.

**Yapıştırma:**
- ❌ `docs/architecture.md` (zaten burada özetlendi)
- ❌ `docs/api-contracts.md` (Faz 4 sözleşmeyse onu da ekle, yoksa gereksiz)
- ❌ `docs/mikro-v15-v16-rules.md` (kurallar burada)
- ❌ `README.md` (proje tanıtımı, bu dosyada var)
- ❌ Tüm solution ağacı
- ✅ Spesifik track deliverable'ı (örn. `faz3-track2-deliverable.md`)
- ✅ Düzenlenecek / eklenecek source dosyalar

---

**Son güncelleme:** 2026-07-10 — TÜM 7 FAZ TAMAMLANDI (286/286 + 16 skip). Proje teslim edilebilir.

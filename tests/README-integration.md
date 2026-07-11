# Integration Tests

Bu dizindeki `docker-compose.test.yml`, ErpBridge integration testleri için üç
ayrı veritabanı konteyneri başlatır:

- **mssql-mikro16** (port 14330): SQL Server 2022 + V16 Guid kolonları
- **mssql-mikro15** (port 14331): SQL Server 2019 + V15 RECno kolonları
- **postgres-central** (port 54320): PostgreSQL 16 + CentralApi SaaS veritabanı

## Çalıştırma

```bash
cd tests

# Tüm servisleri başlat (MSSQL × 2 + PostgreSQL)
docker compose -f docker-compose.test.yml up -d

# Mikro V15/V16 integration testleri:
export ERPBridge_RUN_INTEGRATION=1
export ERPBridge_SQL_SERVER_16=localhost,14330
export ERPBridge_SQL_SERVER_15=localhost,14331
export ERPBridge_SQL_USER=sa
export ERPBridge_SQL_PASSWORD="ErpBridge_Test_2026!"

# CentralApi için PostgreSQL connection string:
export ConnectionStrings__CentralApi="Host=localhost;Port=54320;Database=erpbridge_central_test;Username=erpbridge;Password=ErpBridge_Test_2026!"

cd .. && dotnet test ErpBridge.sln -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor

# Kapat
cd tests && docker compose -f docker-compose.test.yml down
```

> **Not:** Eğer runtime sadece .NET 9 kuruluysa (örn. CI image), shell'e
> `DOTNET_ROLL_FORWARD=LatestMajor` ekleyin veya `global.json` SDK sürümünü
> 9.0.x'e çekin.

## PostgreSQL (Faz 4)

`docker compose -f docker-compose.test.yml up -d postgres-central` ile
CentralApi test veritabanı başlatılır. Port 54320 (host) → 5432 (container).

Connection string:
`Host=localhost;Port=54320;Database=erpbridge_central_test;Username=erpbridge;Password=ErpBridge_Test_2026!`

CentralApi'yi bu connection string ile çalıştırmak için:

```bash
export ConnectionStrings__CentralApi="Host=localhost;Port=54320;Database=erpbridge_central_test;Username=erpbridge;Password=ErpBridge_Test_2026!"
cd src/ErpBridge.CentralApi
dotnet run
```

Sonra `http://localhost:5080/swagger` üzerinden API'yi keşfedebilirsiniz.

### Şema ve seed

`central-init.sql` dosyası container ilk açılışında otomatik olarak
`/docker-entrypoint-initdb.d/` üzerinden çalıştırılır; iki örnek tenant
(`Test Tenant A` / `Test Tenant B`) ve iki örnek lisans
(`TEST-LICENSE-A` / `TEST-LICENSE-B`) seed'ler. Tablolar EF Core'un
`db.Database.EnsureCreated()` / migration akışı tarafından açılır; init
SQL yalnızca veri ekler.

> Faz 4'te migration henüz üretilmedi (Track 4'ün konusu). Bu yüzden
> CentralApi uygulamasının açılışı `db.Database.EnsureCreated()` ile
> boş tabloları yaratır; ardından `central-init.sql` INSERT'leri
> tenant + license satırlarını ekler.

### Connection pooling davranışı

PgSQL varsayılan connection pool'u 100 bağlantı sınırı ile çalışır.
Test ortamında bu sıkıntı yaratmaz; production'da
`Maximum Pool Size` parametresini orchestrator'a göre ayarlayın.

## Env var yoksa

Integration testler skip olur. Unit testler normal şekilde çalışır.

## Skip patterni

Integration testler `MikroIntegrationFixture.ShouldRun` ile başlar. Bu metot
`ERPBridge_RUN_INTEGRATION=1` ortam değişkeni set edilmediyse `false` döner;
test `return` ile sessizce çıkar. xUnit bunu **Passed** olarak raporlar
(skip değil). Bu sayede:

- CI matrix'inin "no-DB" kanalında hermetic unit test çalışır
- "with-DB" kanalında gerçek SQL Server fixture'a bağlanılır
- Default davranış (env yok) unit test patlaması değildir

CentralApi test'leri (`tests/ErpBridge.CentralApi.Tests`) ise
**hermetik** çalışır: gerçek PostgreSQL gerektirmez, in-memory EF Core
provider ile test in-process host üzerinden koşar.

## Şemalar

`mikro16-init.sql` ve `mikro15-init.sql` dosyaları, container ilk açılışında
Microsoft'un resmi `mssql` imajları tarafından
`/docker-entrypoint-initdb.d/` üzerinden otomatik çalıştırılır. V16
şemasında Guid kolonu vardır; V15 şemasında RECno kimlik şeması kullanılır.
VersionDetector bu farkı tespit eder.

`central-init.sql` ise `postgres:16-alpine` imajının aynı initdb
mekanizması üzerinden çalıştırılır; yalnızca seed INSERT'leri içerir
(tabloları CentralApi'nin kendisi yaratır).

## Şifreler

`MSSQL_SA_PASSWORD=ErpBridge_Test_2026!`,
`POSTGRES_PASSWORD=ErpBridge_Test_2026!` yalnızca fixture amaçlıdır;
container kapatıldığında veriler kaybolur (volume yok). Production
yapılandırmasında veya CI secret'ında bu şifreler **asla** yer almaz.

## Canlı Mikro sunucusuna karşı test (TULPAR)

`MikroBootstrapLiveIntegrationTests` aynı sorguları isteğe bağlı olarak
**gerçek bir Mikro V15 sunucusuna** karşı da koşturur. Hedef sunucu
`TulparLiveSettings` env-var'ları ile seçilir; bu env-var'lar set edilmediğinde
testler yukarıdaki docker-compose fixture'ını kullanır (fallback davranışı).

```bash
# TULPAR (gerçek müşteri sunucusu) için:
export ERPBridge_RUN_INTEGRATION=1
export ERPBridge_TULPAR_SERVER=TULPAR
export ERPBridge_TULPAR_DATABASE=MikroDB_V15_02
export ERPBridge_TULPAR_USER=mikro_sync_user
export ERPBridge_TULPAR_PASSWORD='...'        # production parola — loga düşmez
```

> **Güvenlik:** `ERPBridge_TULPAR_PASSWORD` **asla** test çıktısına veya log
> satırlarına yazılmaz; `TulparLiveSettings.Describe` parolayı
> `***REDACTED***` ile maskeler. Tüm hata mesajları `ConnectionStringMasker`
> üzerinden geçirilir.
>
> **Aktif sunucu:** `MikroDB_V15_02` ajan tarafından read-only erişim için
> kullanılır. Testler `SELECT`-only sorgular çalıştırır; INSERT/UPDATE/DDL
> yapmaz. TULPAR şifresi expired olduğunda test **bilinçli şekilde fail**
> olur (operator credential resetlemesini beklemek için).

### Çift-hedef stratejisi

`ResolveFixture()` önce TULPAR env-var'larına, sonra docker-V15 fixture'ına
bakarak hedef seçer. Bu sayede:

- Lokal geliştirme: `docker compose up -d` + `ERPBridge_RUN_INTEGRATION=1` → docker fixture
- Müşteri sahası: TULPAR env-var'larını set et → gerçek sunucu

Aynı test kodu iki hedefte de aynı şekilde koşar; fark yalnızca veri
büyüklüğüdür. Şifre yanlışsa test **SqlException** bekler; bağlantı
kurulamazsa hata mesajı masked log'a düşer.

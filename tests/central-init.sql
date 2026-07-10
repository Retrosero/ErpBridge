-- ErpBridge test ortamı için PostgreSQL seed verisi.
--
-- Tabloların kendisi CentralApi uygulamasının EF migrations katmanı
-- tarafından açılışta yaratılır; bu dosya yalnızca tenant + license
-- fixture satırlarını enjekte eder. Migration henüz eklenmediğinde
-- (Faz 4 sonuna kadar mümkün) bu dosya tablo CREATE'lerini de
-- içerebilir — şu an uygulama başlatılınca EnsureCreated() ile
-- boş tabloları yaratır, ardından bu INSERT'ler çalışır.
--
-- Kullanım:
--   docker compose -f docker-compose.test.yml up -d postgres-central
--   docker exec -it erpbridge-test-postgres psql -U erpbridge -d erpbridge_central_test -f /docker-entrypoint-initdb.d/central-init.sql
--
-- Şifreler test-only'dir; volume yok, container kapatılınca veri silinir.

BEGIN;

INSERT INTO tenants ("Id", "Name", "CreatedAtUtc", "IsActive") VALUES
    ('11111111-1111-1111-1111-111111111111', 'Test Tenant A', NOW() AT TIME ZONE 'UTC', TRUE),
    ('22222222-2222-2222-2222-222222222222', 'Test Tenant B', NOW() AT TIME ZONE 'UTC', TRUE)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO licenses ("Id", "TenantId", "LicenseKey", "IssuedAtUtc", "ExpiresAtUtc", "IsActive") VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '11111111-1111-1111-1111-111111111111',
     'TEST-LICENSE-A', NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC' + INTERVAL '1 year', TRUE),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', '22222222-2222-2222-2222-222222222222',
     'TEST-LICENSE-B', NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC' + INTERVAL '1 year', TRUE)
ON CONFLICT ("Id") DO NOTHING;

COMMIT;

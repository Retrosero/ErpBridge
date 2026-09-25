-- GOAL_PANEL_ERPSIZ E8b: 20.000 ürün / 5.000 cari / 200.000 hareket (100.000 cari + 100.000 stok satırı).
-- Yalnız yerel/test PostgreSQL'e (canlı veritabanına ASLA). :tenant psql değişkeni: tohumlanacak native firmanın Id'si.
-- Kullanım: psql -v tenant=<firma Id> -f scripts/perf/native-panel-seed.sql ; ölçüm: scripts/perf/native-panel-measure.py
\set ON_ERROR_STOP on
BEGIN;

SELECT COALESCE(MAX("UpdatedSeq"), 0) AS base FROM mobile_records WHERE "TenantId" = :'tenant' \gset

INSERT INTO mobile_records ("TenantId","Entity","RecordKey","StockKey","CustomerKey","PayloadJson","UpdatedSeq","IsDeleted","UpdatedAtUtc")
SELECT :'tenant', 'stocks', 'P' || lpad(n::text, 5, '0'), 'P' || lpad(n::text, 5, '0'), NULL,
       jsonb_build_object('stockCode', 'P' || lpad(n::text, 5, '0'), 'name', 'Ürün ' || n, 'urunAd', 'Ürün ' || n, 'birim', 'Adet',
                          'kdvOrani', 10, 'kategori', 'Grup ' || (n % 40), 'marka', 'Marka ' || (n % 25), 'updatedAt', now()),
       :base + n, false, now()
FROM generate_series(1, 20000) n;

INSERT INTO mobile_records ("TenantId","Entity","RecordKey","StockKey","CustomerKey","PayloadJson","UpdatedSeq","IsDeleted","UpdatedAtUtc")
SELECT :'tenant', 'inventory', 'P' || lpad(n::text, 5, '0') || '|1', 'P' || lpad(n::text, 5, '0'), NULL,
       jsonb_build_object('stockCode', 'P' || lpad(n::text, 5, '0'), 'warehouseNo', 1, 'quantity', 1000 - 5 * (n % 7)),
       :base + 20000 + n, false, now()
FROM generate_series(1, 20000) n;

INSERT INTO mobile_records ("TenantId","Entity","RecordKey","StockKey","CustomerKey","PayloadJson","UpdatedSeq","IsDeleted","UpdatedAtUtc")
SELECT :'tenant', 'prices', 'P' || lpad(n::text, 5, '0') || '|1', 'P' || lpad(n::text, 5, '0'), NULL,
       jsonb_build_object('stockCode', 'P' || lpad(n::text, 5, '0'), 'listNumber', 1, 'price', 10 + (n % 90)),
       :base + 40000 + n, false, now()
FROM generate_series(1, 20000) n;

INSERT INTO mobile_records ("TenantId","Entity","RecordKey","StockKey","CustomerKey","PayloadJson","UpdatedSeq","IsDeleted","UpdatedAtUtc")
SELECT :'tenant', 'customers', 'K' || lpad(n::text, 4, '0'), NULL, 'K' || lpad(n::text, 4, '0'),
       jsonb_build_object('customerCode', 'K' || lpad(n::text, 4, '0'), 'title1', 'Müşteri ' || n, 'balance', 0, 'currency', 'TRY', 'updatedAt', now()),
       :base + 60000 + n, false, now()
FROM generate_series(1, 5000) n;

-- 100.000 satış: her biri bir cari satırı ve bir stok satırı (aynı iş kimliği, NATIVE).
INSERT INTO mobile_records ("TenantId","Entity","RecordKey","StockKey","CustomerKey","PayloadJson","UpdatedSeq","IsDeleted","UpdatedAtUtc")
SELECT :'tenant', 'customerTransactions', 'perf-' || n || '|sale', NULL, NULL,
       jsonb_build_object('id', 'perf-' || n || '|sale', 'erp', 'NATIVE', 'cariKod', 'K' || lpad((1 + n % 5000)::text, 4, '0'),
                          'customerCode', 'K' || lpad((1 + n % 5000)::text, 4, '0'),
                          'tarih', to_char(timestamp '2025-01-01' + (n % 600) * interval '1 day', 'YYYY-MM-DD"T"12:00:00'),
                          'evrakNo', 'PS-' || n, 'type', 'Satış', 'tip', 0, 'borcMu', true, 'meblag', 100, 'amount', 100, 'updatedAt', now()),
       :base + 70000 + n, false, now()
FROM generate_series(1, 100000) n;

INSERT INTO mobile_records ("TenantId","Entity","RecordKey","StockKey","CustomerKey","PayloadJson","UpdatedSeq","IsDeleted","UpdatedAtUtc")
SELECT :'tenant', 'stockTransactions', 'perf-' || n || '|1', NULL, NULL,
       jsonb_build_object('id', 'perf-' || n || '|1', 'erp', 'NATIVE', 'stokKod', 'P' || lpad((1 + n % 20000)::text, 5, '0'),
                          'urunKod', 'P' || lpad((1 + n % 20000)::text, 5, '0'),
                          'tarih', to_char(timestamp '2025-01-01' + (n % 600) * interval '1 day', 'YYYY-MM-DD"T"12:00:00'),
                          'tip', 1, 'cins', 0, 'evrakNo', 'PS-' || n, 'cikisMiktar', 1, 'miktar', -1, 'birimFiyat', 100, 'tutar', 100,
                          'cariKod', 'K' || lpad((1 + n % 5000)::text, 4, '0'), 'cikisDepoNo', 1, 'updatedAt', now()),
       :base + 170000 + n, false, now()
FROM generate_series(1, 100000) n;

INSERT INTO native_stock_levels ("TenantId","StockCode","WarehouseNo","Quantity","UpdatedAtUtc")
SELECT :'tenant', 'P' || lpad(n::text, 5, '0'), 1, 1000 - 5 * (n % 7), now() FROM generate_series(1, 20000) n
ON CONFLICT DO NOTHING;

INSERT INTO tenant_sync_counter ("TenantId","LastSeq","TombstoneHorizonSeq") VALUES (:'tenant', :base + 270000, 0)
ON CONFLICT ("TenantId") DO UPDATE SET "LastSeq" = GREATEST(tenant_sync_counter."LastSeq", EXCLUDED."LastSeq");

COMMIT;

-- ErpBridge test ortamı için Mikro V16 şema iskeleti.
--
-- Bu script tam Mikro şeması DEĞİLDİR. Sadece ErpBridge'in test edebileceği
-- minimum yapıyı kurar. V16 Guid kimliği kullanır; primary key sip_Guid,
-- sth_Guid'dir. V15 RECno identity'si V16'da yok (cross-version uyumluluk
-- için identity kolonları default 0 ile kalır; writer V16 path'i bunları
-- kullanmaz).
--
-- Test-ONLY. Production veritabanlarında çalıştırılmaz.
--
-- Faz 6 kolon isimleri Mikro'nun gerçek söyleşimiyle (sip_/sth_ öneki,
-- evrakno_seri/sira, musteri_kod, satici_kod, depono, birim_pn, isk1..6)
-- birebir uyumlu — MikroSalesOrderWriter bu isimleri kullanır.

CREATE DATABASE MIKRO16_FAZ3;
GO

USE MIKRO16_FAZ3;
GO

-- V16'ya özgü Guid kimlik kolonu.
CREATE TABLE STOKLAR (
    sto_kod NVARCHAR(50) PRIMARY KEY,
    sto_isim NVARCHAR(255),
    sto_Guid UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    sto_lastup_date DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE CARI_HESAPLAR (
    cari_kod NVARCHAR(50) PRIMARY KEY,
    cari_unvan1 NVARCHAR(255),
    cari_Guid UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID()
);
GO

CREATE TABLE DEPOLAR (
    depo_no INT PRIMARY KEY,
    depo_adi NVARCHAR(100)
);
GO

-- Faz 6: Satış siparişi header (V16). Guid primary key — yazıcı Guid'i
-- INSERT'ten önce kendi üretir, parametre olarak geçer. V15 RECno
-- identity'si V16'da yok (writer V16 path'i RECno kullanmaz).
CREATE TABLE SIPARISLER (
    sip_Guid            UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    sip_firmano         INT              NOT NULL,
    sip_sube_no         INT              NOT NULL DEFAULT 0,
    sip_evrakno_seri    NVARCHAR(5),
    sip_evrakno_sira    INT,
    sip_tarih           DATETIME,
    sip_musteri_kod     NVARCHAR(50)     NOT NULL,
    sip_satici_kod      NVARCHAR(50),
    sip_depono          INT              NOT NULL,
    sip_doviz_cinsi     NVARCHAR(10),
    sip_kapat_fl        BIT              DEFAULT 0
);
GO

CREATE UNIQUE INDEX UX_SIPARISLER_FirmaSeriNumara
    ON SIPARISLER (sip_firmano, sip_evrakno_seri, sip_evrakno_sira);
GO

-- Faz 6: Satış siparişi satırları (V16). sth_sip_uid kolonu GuidStrategy
-- tarafından yeni header'ın sip_Guid değeriyle doldurulur. sth_RECno
-- identity V16'da yok; RECid link kolonları da yok.
CREATE TABLE STOK_HAREKETLERI (
    sth_Guid            UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    sth_firmano         INT              NOT NULL,
    sth_sube_no         INT              NOT NULL DEFAULT 0,
    sth_tarih           DATETIME,
    sth_evrakno_seri    NVARCHAR(5),
    sth_evrakno_sira    INT,
    sth_satirno         INT              NOT NULL,
    sth_stok_kod        NVARCHAR(50)     NOT NULL,
    sth_miktar          DECIMAL(18,4),
    sth_birim_pn        INT,
    sth_fiyat           DECIMAL(18,4),
    sth_kdv_pn          INT,
    sth_cikis_depo_no   INT,
    sth_tip             SMALLINT,
    sth_isk1            DECIMAL(8,4)     DEFAULT 0,
    sth_isk2            DECIMAL(8,4)     DEFAULT 0,
    sth_isk3            DECIMAL(8,4)     DEFAULT 0,
    sth_isk4            DECIMAL(8,4)     DEFAULT 0,
    sth_isk5            DECIMAL(8,4)     DEFAULT 0,
    sth_isk6            DECIMAL(8,4)     DEFAULT 0,
    sth_sip_uid         UNIQUEIDENTIFIER
);
GO

-- Test verisi
INSERT INTO STOKLAR (sto_kod, sto_isim) VALUES ('STK001', 'Test Stok 1');
-- Faz 6 F6.4: ek stok to enable multi-line integration tests for V16.
INSERT INTO STOKLAR (sto_kod, sto_isim) VALUES ('STK002', 'Test Stok 1 Line 2');
INSERT INTO CARI_HESAPLAR (cari_kod, cari_unvan1) VALUES ('120.01.0001', 'Test Cari');
INSERT INTO DEPOLAR (depo_no, depo_adi) VALUES (1, 'Ana Depo');
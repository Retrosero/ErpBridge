-- ErpBridge test ortamı için Mikro V15 şema iskeleti.
--
-- Bu script tam Mikro şeması DEĞİLDİR. Sadece ErpBridge'in test edebileceği
-- minimum yapıyı kurar. V15, RECno kimlik şemasını kullanır: identity sütunu
-- + RECid_DBCno / RECid_RECno link kolonları. Guid kolonu identity değil —
-- default NEWID() ile dolu, writer V15 path'inde bunu kullanmaz.
--
-- Test-ONLY. Production veritabanlarında çalıştırılmaz.
--
-- Faz 6 kolon isimleri Mikro'nun gerçek söyleşimiyle (sip_/sth_ öneki,
-- evrakno_seri/sira, musteri_kod, satici_kod, depono, birim_pn, isk1..6)
-- birebir uyumlu — MikroSalesOrderWriter bu isimleri kullanır.

CREATE DATABASE MIKRO15_FAZ3;
GO

USE MIKRO15_FAZ3;
GO

CREATE TABLE STOKLAR (
    sto_kod NVARCHAR(50) PRIMARY KEY,
    sto_isim NVARCHAR(255),
    sto_RECno INT IDENTITY(1,1),
    sto_RECid_DBCno INT DEFAULT 0,
    sto_RECid_RECno INT
);
GO

CREATE TABLE CARI_HESAPLAR (
    cari_kod NVARCHAR(50) PRIMARY KEY,
    cari_unvan1 NVARCHAR(255),
    cari_RECno INT IDENTITY(1,1),
    cari_RECid_DBCno INT DEFAULT 0,
    cari_RECid_RECno INT
);
GO

CREATE TABLE DEPOLAR (
    depo_no INT PRIMARY KEY,
    depo_adi NVARCHAR(100)
);
GO

-- Faz 6: Satış siparişi header (V15). RECno identity + RECid link kolonları.
-- Mikro writer sip_RECno'yu SELECT SCOPE_IDENTITY() ile alır, INSERT'te
-- belirtmez. sip_Guid V15'te identity değil ama kolon var (writer
-- kullanmıyor).
CREATE TABLE SIPARISLER (
    sip_RECno           INT IDENTITY(1,1) PRIMARY KEY,
    sip_RECid_DBCno     INT              DEFAULT 0,
    sip_RECid_RECno     INT,
    sip_Guid            UNIQUEIDENTIFIER DEFAULT NEWID(),  -- V15'te writer kullanmıyor; V16 cross-version şeması için mevcut
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

-- Faz 6: Satış siparişi satırları (V15). sth_sip_RECid_RECno kolonu
-- RecnoStrategy tarafından yeni header'ın sip_RECno değeriyle
-- doldurulur; sth_sip_RECid_DBCno writer tarafından set edilir.
-- sth_sip_uid V15'te writer kullanmıyor ama cross-version şeması için mevcut.
CREATE TABLE STOK_HAREKETLERI (
    sth_RECno           INT IDENTITY(1,1) PRIMARY KEY,
    sth_RECid_DBCno     INT              DEFAULT 0,
    sth_RECid_RECno     INT,
    sth_Guid            UNIQUEIDENTIFIER DEFAULT NEWID(),  -- V16 cross-version
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
    sth_sip_RECid_DBCno INT              DEFAULT 0,
    sth_sip_RECid_RECno INT,
    sth_sip_uid         UNIQUEIDENTIFIER                   -- V16 link; V15'te NULL
);
GO

INSERT INTO STOKLAR (sto_kod, sto_isim) VALUES ('STK001', 'Test Stok V15');
-- Faz 6 F6.4: ek stok to enable multi-line integration tests (writer
-- tests a payload with two distinct stock lines).
INSERT INTO STOKLAR (sto_kod, sto_isim) VALUES ('STK002', 'Test Stok V15 Line 2');
INSERT INTO CARI_HESAPLAR (cari_kod, cari_unvan1) VALUES ('120.01.0001', 'Test Cari V15');
INSERT INTO DEPOLAR (depo_no, depo_adi) VALUES (1, 'Ana Depo V15');

-- V15 için V16 Guid identity KULLANILMAZ — writer V15 path'inde
-- sip_RECno / sth_sip_RECid_RECno kullanır. sip_Guid / sth_Guid / sth_sip_uid
-- kolonları cross-version uyumluluk için mevcut (default NEWID() ile dolu,
-- writer bunları V15'te ignore eder). VersionDetector bu RECno identity
-- şemasını görerek V15 döner.
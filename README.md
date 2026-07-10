# ErpBridge

ErpBridge, müşterinin Windows bilgisayarında çalışan bir senkronizasyon ajanı ile merkezi
SaaS API arasında veri alışverişi yapan bir ERP bağlantı platformudur.

İlk desteklenen ERP: **Mikro ERP V15 ve V16**. Mimari adapter pattern ile kuruludur;
ileride Logo, Paraşüt, Netsis gibi ERP'ler eklenebilir.

## Temel akış

```
Android / Web Saha Satış Uygulaması
        ↓ (HTTPS)
Merkezi SaaS API
        ↓ (pending job kuyruğu)
Windows Sync Agent (müşteri bilgisayarı)
        ↓ (yerel ağ, Mikro SQL Server)
Mikro ERP
```

## Çözüm yapısı

```
ErpBridge/
├── src/
│   ├── ErpBridge.Agent.Service/         Windows Service (BackgroundService workers)
│   ├── ErpBridge.Agent.UI/              WPF ayar paneli
│   ├── ErpBridge.Core/                  Domain + orchestrators
│   ├── ErpBridge.LocalStore/            SQLite: mappings, queue, checkpoints, config
│   ├── ErpBridge.RemoteApi/             Merkezi API client
│   ├── ErpBridge.Erp.Abstractions/      Adapter arayüzleri (ERP bağımsız)
│   ├── ErpBridge.Erp.Mikro/             Mikro V15/V16 adapter
│   └── ErpBridge.Shared/                Ortak DTO / Result / sabitler
├── tests/
│   ├── ErpBridge.Core.Tests/
│   ├── ErpBridge.LocalStore.Tests/
│   └── ErpBridge.Erp.Mikro.Tests/
└── docs/
    ├── architecture.md
    ├── development-roadmap.md
    ├── mikro-v15-v16-rules.md
    └── api-contracts.md
```

## Çalıştırma

```bash
# Çözümü build et
dotnet build ErpBridge.sln

# Testleri çalıştır
dotnet test

# WPF ayar paneli (geliştirme)
dotnet run --project src/ErpBridge.Agent.UI

# Windows Service (yükleme sonra)
dotnet run --project src/ErpBridge.Agent.Service
```

## Lisans

Özel / internal. Tüm hakları saklıdır.

Detaylı kurallar için bkz. `AGENTS.md` ve `docs/`.
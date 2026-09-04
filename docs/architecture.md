# ErpBridge — Architecture

## Genel bakış

ErpBridge üç ana bileşenden oluşur:

1. **Windows Sync Agent** — müşterinin bilgisayarında çalışır. Merkezi API'den
   pending job çeker, Mikro SQL'e yazar, Mikro'dan bootstrap veri okuyup merkezi API'ye
   push eder. Tüm trafiği **outbound HTTPS**.
2. **Central SaaS API** — Android/web saha satış uygulaması ve agent arasındaki
   köprü. Tenant, lisans, heartbeat, pending job ve ack yönetir. PostgreSQL.
3. **Android / Web Saha Satış Uygulaması** — merkezi API'ye JSON payload gönderir,
   Mikro SQL'e **doğrudan bağlanmaz**.

> **Faz 10:** Agent artık `MikroConnectionSettings.CompanyNo` +
> `WarehouseNo` üzerinden çok-firmalı / çok-depolu Mikro kurulumlarını
> destekliyor. Operatör WPF'te Firma No / Şube No / Depo No girip
> kaydedebiliyor; her bootstrap okuması bu değerlerle filtreleniyor.

## Katmanlı yapı

```
+------------------+        outbound HTTPS        +-------------------+
| Android / Web UI |  ------------------------->  | Central API (.NET)|
+------------------+                              +-------------------+
                                                          ^
                                                          | outbound HTTPS
                                                          v
                                              +-----------------------+
                                              | Windows Sync Agent    |
                                              |  - Agent.Service      |
                                              |  - Agent.UI (WPF)     |
                                              +-----------------------+
                                                          | yerel ağ
                                                          v
                                              +-----------------------+
                                              | Mikro SQL Server      |
                                              |  (CARI / STOK / EVRAK)|
                                              +-----------------------+
```

## Agent iç mimarisi

```
ErpBridge.Agent.Service (BackgroundService worker)
  └─ AgentWorker (poll döngüsü)
       ├─ IRemoteApiClient       (merkezi API konuşması)
       ├─ ILocalQueueStore       (SQLite dayanıklı kuyruk)
       ├─ IMappingStore          (idempotency mappings)
       ├─ IAgentConfigStore      (yapılandırma)
       └─ IErpAdapter (factory)  (ERP bağımsız)
            └─ MikroAdapter      (ErpBridge.Erp.Mikro)
                 ├─ MikroVersionDetector
                 ├─ IMikroIdentityStrategy (RECno | Guid)
                 ├─ BootstrapReader
                 └─ SalesOrderWriter
```

## Bağımlılık kuralları

| Proje | Referans verebilir | Referans veremez |
|-------|-------------------|------------------|
| ErpBridge.Shared             | — | her şey |
| ErpBridge.Erp.Abstractions  | Shared | Mikro, Service, UI, LocalStore, RemoteApi |
| ErpBridge.Core              | Shared, Erp.Abstractions | Mikro, LocalStore, RemoteApi, Service, UI |
| ErpBridge.LocalStore        | Shared | Mikro, Erp.Abstractions |
| ErpBridge.RemoteApi         | Shared | Mikro, LocalStore, Core |
| ErpBridge.Erp.Mikro         | Shared, Erp.Abstractions | Service, UI, LocalStore, Core, RemoteApi |
| ErpBridge.Agent.Service     | Core, LocalStore, RemoteApi, Erp.Mikro, Erp.Abstractions, Shared | — |
| ErpBridge.Agent.UI          | Core, LocalStore, Shared | Erp.Mikro, RemoteApi |

**Kritik:** ErpBridge.Erp.Abstractions, **Mikro'ya bağımlı olamaz**. Bu kural ihlal
edilirse Core / RemoteApi / Service de Mikro'ya sızıntı yapar.

## Transaction ve idempotency

Tüm ERP yazma işlemleri tek SQL transaction içinde yapılır. Transaction commit olmadan
mapping kaydedilmez ve merkezi API'ye ack gönderilmez. Aynı `externalId` ile ikinci kez
gelen job, mapping tablosunda bulunursa sessizce atlanır ve idempotent ack gönderilir.

## Loglama

Serilog ile console + rolling file. Hassas alanlar (`SqlPassword`, `LicenseKey`,
`ApiToken`) için Destructuring veya Masking policy'si uygulanır.
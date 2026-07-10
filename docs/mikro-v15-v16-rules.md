# Mikro V15 vs V16 — Adapter Kuralları

Bu doküman, Mikro ERP'nin iki ana sürümü arasındaki kimlik/link alanı farklarını ve
ErpBridge adapter'ının bunları nasıl çözdüğünü anlatır. **Bu kurallar core'a sızmaz;
sadece `ErpBridge.Erp.Mikro` içinde uygulanır.**

## V15 — RECno / RECid pattern

Birçok tabloda birincil anahtar `RECno` (identity gibi davranan artan tamsayı). Çapraz
tablo bağlantıları için `*_RECid_DBCno` + `*_RECid_RECno` çifti kullanılır.

Tipik örnek — `STOK_HAREKETLERI` üzerinden sipariş bağlantısı:

| V15 alanı | Tip | Açıklama |
|-----------|-----|----------|
| `sth_RECno` | int | Stok hareketi kendi RECno'su |
| `sth_sip_RECid_DBCno` | int | Bağlı siparişin DB numarası |
| `sth_sip_RECid_RECno` | int | Bağlı siparişin RECno'su |

Insert sonrası `SCOPE_IDENTITY()` ile `sth_RECno` alınır ve aynı transaction içinde
ilgili link alanları update edilir.

## V16 — Guid / uid pattern

Birincil anahtar `Guid` (genelde `*_Guid` veya `*_uid`). Insert sırasında uygulama
`Guid.NewGuid()` üretir veya `NEWID()` kullanır. Çapraz tablo bağlantıları tek
`uid` alanı üzerinden kurulur.

Tipik örnek:

| V16 alanı | Tip | Açıklama |
|-----------|-----|----------|
| `sth_Guid` | uniqueidentifier | Stok hareketi kendi Guid'i |
| `sth_sip_uid` | uniqueidentifier | Bağlı siparişin Guid'i |

## Adapter stratejisi

`IMikroIdentityStrategy` adında iki implementasyon:

```csharp
public interface IMikroIdentityStrategy {
    string SelectScopeIdentitySql();
    string GenerateGuidSql();
    string ParameterPrefixForIdentity();
    // Sipariş header insertinden sonra link alanlarını set eder
    void ApplyLinkFields(IDbCommand cmd, string sipGuidOrRecno, bool isV16);
}

public sealed class RecnoStrategy : IMikroIdentityStrategy { /* V15 */ }
public sealed class GuidStrategy  : IMikroIdentityStrategy { /* V16 */ }
```

`MikroVersionDetector`, agent ilk bağlantı testinde versiyonu tespit eder ve adapter
bir kez `IMikroIdentityStrategy`'yi seçer. Tüm writer'lar bu strateji üzerinden
çalışır; yeni yazıcı eklendiğinde V15/V16 dallanması otomatik olarak çözülür.

## Tespit yöntemi

V15/V16 ayrımı için iki yaklaşım:

1. `SERVERPROPERTY('ProductVersion')` → örn. `15.x` veya `16.x`
2. `sys.tables` veya `INFORMATION_SCHEMA.COLUMNS` üzerinden bir bilinen V16
   kolonunun (`*_Guid` veya `*_uid`) varlığını sorgulamak

İlk yaklaşım daha hızlı; ikincisi daha sağlam. Adapter ilk bağlantıda **ikisini de**
dener ve çakışma durumunda V16'ya öncelik verir.

## Yazım kalıbı (her iki sürüm için ortak)

```csharp
using var tx = connection.BeginTransaction();
try {
    // 1) Header insert
    var sipRecnoOrGuid = identityStrategy.InsertSiparisHeader(tx, payload);

    // 2) Lines insert, link alanları header kimliğine bağlanır
    foreach (var line in payload.Lines) {
        identityStrategy.InsertSiparisLine(tx, payload, line, sipRecnoOrGuid);
    }

    // 3) Mapping kaydet
    await mappingStore.SaveAsync(new MappingRecord { ... }, ct);

    tx.Commit();
} catch {
    tx.Rollback();
    throw;
}
```

Bu kalıp **hiçbir zaman** string concat ile SQL üretmez; tüm alanlar `IDbCommand`
parametreleri olarak geçer.
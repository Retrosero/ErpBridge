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

## 3. Test / build

- .NET: `dotnet build ErpBridge.sln -c Debug` (0 uyarı / 0 hata) + `dotnet test ErpBridge.sln`
- Canlı Mikro şema testi: `ERPBridge_RUN_INTEGRATION=1` (opsiyonel, `MikroDB_V15_02`/`V16_03`)
- `TreatWarningsAsErrors=true` — uyarı bırakma.

# Panel ERP'li kart yönetimi — Durum

`docs/GOAL_PANEL_ERPLI.md` planının nerede olduğu. Son güncelleme: 2026-09-22.

## Tamamlananlar

| Faz | İş | PR |
|---|---|---|
| E0a | Plan onaylandı (kapsam ürün+cari, K6=ADMIN+MANAGER); `eb-panel-erpli` worktree açıldı; `CLAUDE.md`'ye istisna eklendi | (bu PR) |

## Sırada

E0b (yerel ERP'li test ortamı) → E1 (Mikro writer denetimi + kapı açma — en riskli görev).

## Seni Bekleyenler

- E1a'nın bulguları: `MikroCustomerCardWriter`/`MikroStockCardWriter` UPDATE'i destekliyor mu,
  yoksa yalnız INSERT mi? Desteklemiyorsa E1b'nin kapsamı büyür.
- E6b: gerçek Mikro'lu bir firmada canlı gözle kontrol.
- K5 (silme) ve fiş/evrak goal'ü (§6) ayrı kullanıcı onayı gerektirir.

# GOAL — Görevler (to-do) ve bildirim sistemi

> Başlangıç: 2026-09-25. Kullanıcı kararları: plan önerilerinin tamamı kabul ("Senin önerdiklerin"),
> sunucu yayınını Claude yapar, modül **tüm firmalara açık**. Durum: `GOAL_GOREVLER_DURUM.md`.
> Mobil taraf: `Siparis_Cepte/docs/GOAL_GOREVLER.md` bu belgeye bağlanır.

## 1. Kapsam ve kararlar

| # | Karar |
|---|---|
| K1 | Görevler **yalnız CentralApi PostgreSQL'de** tutulur; ERP'li ve ERP'siz firma aynı tablolar (`TenantId`). ERP'ye hiç yazılmaz, ajan görevleri görmez. |
| K2 | Push servisi yok (Firebase 2026-09-14'te kaldırıldı, geri gelmez). Uygulama açıkken uzun yoklama (`/tasks/events`) ile anında; kapalıyken telefonun 15 dk'lık işçisi yerel bildirim düşer. Hatırlatma telefonun kendi alarmıyla. |
| K3 | Atama: **yönetici (ADMIN veya MANAGER) herkese**, diğer kullanıcılar **yalnız kendine** atar. (Planda "aynı ekip" dendi; sistemde ekip kavramı olmadığı için kendine atama ile sınırlandı.) Takipçi herkes eklenebilir. |
| K4 | Görünürlük: ADMIN/MANAGER şirketin tüm görevlerini görür; diğerleri oluşturduğu, atandığı, takip ettiği veya alt görevine atandığı görevleri. |
| K5 | "İlgili yöneticiler" = görevi **oluşturan + takipçiler**. Tüm yöneticilere toplu bildirim gitmez. |
| K6 | Çok atananlı görev tektir; **herhangi bir atanan** bitirir. Pay dağıtımı alt görevle yapılır. |
| K7 | Alt görev tek seviye; kendi atananı ve bitiş tarihi olabilir. |
| K8 | Zamanlama üçü birden: ileri tarihli başlangıç (`startAtMs`, o ana kadar atanana bildirim gitmez), tekrarlayan seri (`task_series`, günlük/haftalık/aylık), hatırlatma (telefonda yerel). |
| K9 | Resim: görev başına en çok 10, her biri ≤ 2 MB (telefon ~1600 px / ~300 KB'a küçültür). **PostgreSQL'de** (`task_attachment_blobs`, bytea) — Coolify'da kalıcı disk tanımı gerekmez, yedeğe girer. Firma kotası 1 GB (`Tasks:TenantAttachmentQuotaBytes`). Silinen görevin resimleri 30 gün sonra temizlenir. İsteğe bağlı "tamamlarken fotoğraf zorunlu" (`requiresPhoto`). |
| K10 | Görev isteğe bağlı bir cariye bağlanır (`customerCode`, `customerName`); cari detayında görünür. |
| K11 | İlk sürüm telefonda; web paneli sonraki adım. Öncelik, yorum, takipçi ilk sürümde; etiket/toplu işlem sonra. |
| K12 | Saat dilimi: seri zamanları `Europe/Istanbul` yerel saatiyle tanımlanır; saklama unix ms (UTC). |

## 2. Veri modeli (CentralApi, EF Core, yeni tablolar)

Zamanlar `long` unix ms (SQLite testleri `DateTimeOffset` karşılaştıramaz). `UpdatedSeq`/`Seq`
**`tenant_sync_counter`'dan yazan işlem içinde** ayrılır (KB 00 kural 11).

- `tasks` — `Id` (Guid, **istemci üretir**), `TenantId`, `Title`(200), `Description`(4000), `Priority`
  (`LOW|NORMAL|HIGH|URGENT`), `Status` (`OPEN|DONE|CANCELLED`), `CreatedByUserId/Name`, `CreatedAtMs`,
  `UpdatedAtMs`, `StartAtMs?`, `DueAtMs?`, `CompletedAtMs?`, `CompletedByUserId/Name`, `RequiresPhoto`,
  `CustomerCode?`, `CustomerName?`, `SeriesId?`, `StartNotifiedAtMs?`, `DueSoonNotifiedAtMs?`,
  `OverdueNotifiedAtMs?`, `IsDeleted`, `DeletedAtMs?`, `UpdatedSeq`.
- `task_members` — (`TaskId`, `UserId`, `Role` = `ASSIGNEE|FOLLOWER`) + `UserName` anlık görüntü.
- `task_subtasks` — `Id` (Guid, istemci), `TaskId`, `Title`(300), `IsDone`, `DoneByUserId/Name`, `DoneAtMs?`,
  `AssigneeUserId?`, `AssigneeName?`, `DueAtMs?`, `SortOrder`, `IsDeleted`.
- `task_comments` — `Id` (Guid, istemci), `TaskId`, `AuthorUserId/Name`, `Text`(2000), `CreatedAtMs`, `IsDeleted`.
- `task_attachments` — `Id` (Guid, istemci), `TaskId`, `UploadedByUserId/Name`, `ContentType`, `SizeBytes`,
  `CreatedAtMs`, `IsDeleted`; baytlar ayrı `task_attachment_blobs` (`AttachmentId`, `Data`), liste baytı yüklemez.
- `task_events` — değişmez geçmiş (`Action`, `ActorUserId/Name`, `Detail`, `OccurredAtMs`).
- `task_series` — şablon (başlık, açıklama, öncelik, `RequiresPhoto`, cari, atanan/takipçi/alt görev JSON) +
  kural (`Frequency` `DAILY|WEEKLY|MONTHLY`, `Interval`, `Weekdays` bit maskesi Pzt=1…Paz=64, `MonthDay`,
  `TimeOfDayMinutes`, `DueAfterMinutes?`), `NextRunAtMs`, `EndsAtMs?`, `IsActive`, `UpdatedSeq`.
- `task_ops_applied` — (`TenantId`, `OpId`) tekil: işlem kimliği tekrar gelirse yeniden uygulanmaz.
- `user_notifications` — `Id`, `TenantId`, `UserId`, `Kind`, `Title`, `Body`, `TaskId?`, `CreatedAtMs`,
  `ReadAtMs?`, `Seq`.

## 3. Uçlar (`/api/v1/android/...`, `MobileUserPolicy`: telefon + panel)

| Uç | İş |
|---|---|
| `GET tasks?changedSinceSeq&take` | Görünür görevler (alt görev, üyeler, ek meta, yorum sayısı); `latestSeq`, `hasMore`. Silinenler `isDeleted=true` ile gelir. |
| `GET tasks/{id}` | Ayrıntı: + yorumlar, geçmiş. |
| `POST tasks/ops` | `{ ops: [{ opId, type, taskId, ... }] }` sırayla uygulanır; her biri `applied|duplicate|rejected` + hata kodu; güncel görevler döner. |
| `PUT/DELETE tasks/{taskId}/attachments/{attachmentId}` | Ham gövde `image/jpeg|png|webp`, ≤ 2 MB; `attachmentId` ile idempotent. |
| `GET tasks/{taskId}/attachments/{attachmentId}` | Baytlar (yetkili kullanıcı), değişmez önbellek başlığı. |
| `GET tasks/people` | Aktif kullanıcılar (`id`, `fullName`, `roles`) — atama seçici; herkes görür. |
| `GET tasks/series` | Görünür seriler. |
| `GET tasks/summary` | `latestTaskSeq`, `latestNotificationSeq`, `unreadCount`, `openAssignedCount`, `overdueCount`, `canManage`. |
| `GET tasks/events?wait&version` | Uzun yoklama (`TenantEventHub` konu `tasks`); değişince `{version}`, zaman aşımında 204. |
| `GET notifications?changedSinceSeq&take` | Kişinin bildirimleri. |
| `POST notifications/read` | `{ ids: [...] }` ya da `{ all: true }`. |

**İşlem türleri (`ops`):** `create_task`, `update_task`, `set_members`, `complete_task`, `reopen_task`,
`cancel_task`, `delete_task`, `add_subtask`, `update_subtask`, `toggle_subtask`, `delete_subtask`,
`add_comment`, `create_series`, `update_series`, `stop_series`.

**Yetki:** oluşturma herkes (K3 atama sınırıyla). Düzenleme/üyeler/alt görev ekle-sil/iptal/sil: oluşturan
veya yönetici. Atanan: alt görev işaretler, tamamlar/yeniden açar, yorum ve resim ekler. Takipçi: görür,
yorum yazar. Tamamlama `requiresPhoto` ise en az bir resim ister (`409 TASK_PHOTO_REQUIRED`).

**Bildirim türleri:** `TASK_ASSIGNED` (yeni atanan; `startAtMs` ilerideyse başlangıçta), `TASK_COMPLETED`
(oluşturan + takipçiler), `TASK_REOPENED` (atananlar), `TASK_COMMENTED` (oluşturan + atananlar + takipçiler),
`TASK_DUE_SOON` (bitişe 60 dk kala, atananlar), `TASK_OVERDUE` (bitiş geçti, atananlar + oluşturan). İşlemi
yapan kişiye kendi işlemi için bildirim gitmez. Zamanlayıcı (`TaskSchedulerWorker`, dakikada bir):
başlangıç, bitiş yaklaştı/geçti bildirimleri, seri örneklerinin oluşturulması, 30 günlük resim temizliği.

## 4. Görevler

### Sunucu (ErpBridge)
| # | Görev |
|---|---|
| S1 | Domain + `CentralApiDbContext` eşlemeleri + migration |
| S2 | `TaskService`: görünürlük, işlemlerin uygulanması, yetki, geçmiş, bildirim üretimi; `tasks`, `tasks/{id}`, `tasks/ops`, `tasks/people`, `tasks/summary` |
| S3 | Resim yükleme/indirme/silme + kota |
| S4 | Bildirim uçları + `tasks/events` uzun yoklaması |
| S5 | `TaskSchedulerWorker` (başlangıç, bitiş, gecikme, seri, temizlik) |
| S6 | Testler (SQLite ilişkisel) |
| S7 | Bilgi bankası, PR, `main` → Coolify otomatik dağıtım, `/health/schema` kontrolü |
| S8 | *(2026-09-25, kullanıcı isteği)* Cari ziyaretinde hatırlat: `VisitReminder`/`VisitReminderFromMs` (görev), `VisitReminder` (seri), migration `GorevZiyaretHatirlatma` |

### Telefon (Siparis_Cepte) — ayrıntı `Siparis_Cepte/docs/GOAL_GOREVLER.md`
| # | Görev |
|---|---|
| A1 | API istemcisi, Room v43 (önbellek + işlem kuyruğu), depo, eşitleme işçisi |
| A2 | Görevlerim / Atadıklarım listesi, detay, oluştur-düzenle (alt görev, tekrar, cari, takipçi, öncelik), yorum |
| A3 | Resim: kamera/galeri, küçültme, yükleme kuyruğu, yetkili gösterim |
| A4 | Bildirim merkezi (zil + rozet + liste), yerel bildirim, uzun yoklama, hatırlatma alarmı |
| A5 | Modül kaydı (ana sayfa, Diğer menüsü, cari detayı), bilgi bankası, Play internal |
| A6 | *(2026-09-25)* Cari ziyaretinde hatırlat: görev formunda seçenek (hemen / şu tarihten sonra), satış için cari açılınca hatırlatma penceresi, "sonra tekrar sor" (bir sonraki ziyarette yeniden) |

## 5. Yetkiler (kullanıcı onayı 2026-09-25)
Bu belgenin görevleri için dala push, PR, CI yeşilken `main`'e birleştirme (ErpBridge'de Coolify dağıtımı
dahil) ve Sipariş Cepte'nin Play **internal** yüklemesi onaylıdır. Yasaklar: `--force` push (ve
`--force-with-lease`), CI kırmızıyken birleştirme, `main`'e doğrudan push, Play production.

using System.Globalization;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Portal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ErpBridge.CentralApi.Targets;

/// <summary>
/// Sales targets (GOAL_HEDEF_RUT): reading them with their progress for the panel and the phone, and writing
/// them from the panel. Writes need <see cref="RolePermissions.CanManageTargets"/> and stay inside the writer's
/// <see cref="TeamScope"/> (K3, K4); every write is logged in <c>sales_target_events</c> (K12).
/// </summary>
public sealed class TargetService(TargetFactReader facts, IMemoryCache cache)
{
    public const int MaxItemsPerSave = 2000;
    public const int MaxItemCodeLength = 110;
    public const int MaxItemNameLength = 200;
    public const int MaxNoteLength = 500;
    public const decimal MaxValue = 1_000_000_000_000m;
    public const int MaxPickerItems = 50;

    public static long NowMs() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public static DateOnly Today() => PortalReports.BusinessDate(null, DateTimeOffset.UtcNow);

    // ---- reading -------------------------------------------------------------------------------

    /// <summary>
    /// The panel's board: every owner the caller sees — the company, teams and regions, people — with their
    /// summary and targets for the period. <paramref name="teamId"/> narrows it to one team or region.
    /// </summary>
    public async Task<TargetBoardResponse> BoardAsync(CentralApiDbContext db, Tenant tenant, TeamScope scope, TargetPeriod period, Guid? teamId, CancellationToken ct)
    {
        var (board, factSet) = await LoadBoardAsync(db, tenant, [period], ct);
        var people = await PeopleAsync(db, tenant.Id, ct);
        var directory = scope.Directory;

        var owners = new List<OwnerProgressDto>();
        HashSet<Guid>? onlyTeams = teamId is { } t && scope.SeesTeam(t) ? directory.Expand(t).ToHashSet() : null;
        if (scope.WholeCompany && onlyTeams is null)
            owners.Add(board.Build(TargetOwnerKinds.Company, Guid.Empty, "Firma geneli", period));
        foreach (var team in directory.Teams.Values
                     .Where(x => x.IsActive && scope.SeesTeam(x.Id) && (onlyTeams is null || onlyTeams.Contains(x.Id)))
                     .OrderBy(x => x.Kind == SalesTeamKinds.Region ? 0 : 1).ThenBy(x => x.Name, TurkishText))
            owners.Add(board.Build(TargetOwnerKinds.Team, team.Id, team.Name, period));
        foreach (var person in people
                     .Where(p => p.IsActive && scope.SeesUser(p.Id))
                     .Where(p => onlyTeams is null || (directory.TeamOfUser.TryGetValue(p.Id, out var of) && onlyTeams.Contains(of)))
                     .OrderBy(p => p.Name, TurkishText))
            owners.Add(board.Build(TargetOwnerKinds.User, person.Id, person.Name, period));

        var (total, elapsed, left, asOf) = board.WorkDaysOf(period);
        return new TargetBoardResponse
        {
            PeriodType = period.Type,
            PeriodKey = period.Key,
            Start = TargetPeriod.Format(period.Start),
            End = TargetPeriod.Format(period.End),
            AsOf = TargetPeriod.Format(asOf),
            WorkDaysTotal = total,
            WorkDaysElapsed = elapsed,
            WorkDaysLeft = left,
            DataSource = tenant.DataSource,
            Source = factSet.Source,
            Warnings = Warnings(factSet),
            CanManage = scope.CanManage,
            WholeCompany = scope.WholeCompany,
            Owners = [.. owners],
        };
    }

    /// <summary>The phone's own figures: the day, week and month holding <paramref name="date"/>.</summary>
    public async Task<MyTargetsResponse> MineAsync(CentralApiDbContext db, Tenant tenant, TeamScope scope, DateOnly date, CancellationToken ct)
    {
        var periods = TargetPeriodTypes.All.Select(type => TargetPeriod.Of(type, date)).ToList();
        var (board, factSet) = await LoadBoardAsync(db, tenant, periods, ct);
        var name = string.IsNullOrWhiteSpace(scope.User.FullName) ? scope.User.Username : scope.User.FullName;
        return new MyTargetsResponse
        {
            Date = TargetPeriod.Format(date),
            AsOfMs = NowMs(),
            DataSource = tenant.DataSource,
            Source = factSet.Source,
            Warnings = factSet.UsersWithoutSalesperson.Contains(scope.User.Id)
                ? [.. Warnings(factSet), "Plasiyer kodunuz eşlenmemiş; ERP satışlarınız size yazılamıyor. Yöneticinize bildirin."]
                : Warnings(factSet),
            CanViewTeam = scope.CanManage,
            Periods = periods.Select(period =>
            {
                var owner = board.Build(TargetOwnerKinds.User, scope.User.Id, name, period);
                var (total, _, left, _) = board.WorkDaysOf(period);
                return new PeriodProgressDto
                {
                    PeriodType = period.Type,
                    PeriodKey = period.Key,
                    Start = TargetPeriod.Format(period.Start),
                    End = TargetPeriod.Format(period.End),
                    WorkDaysTotal = total,
                    WorkDaysLeft = left,
                    Summary = owner.Summary,
                    Targets = owner.Targets,
                };
            }).ToArray(),
        };
    }

    private async Task<(TargetBoard Board, TargetFactSet Facts)> LoadBoardAsync(CentralApiDbContext db, Tenant tenant, IReadOnlyList<TargetPeriod> periods, CancellationToken ct)
    {
        var ranges = periods.Select(TargetBoard.FactRange).ToList();
        var from = ranges.Min(r => r.From);
        var to = ranges.Max(r => r.To);
        var factSet = await facts.ReadAsync(db, tenant, from, to, ct);
        var fromDay = TargetPeriod.Format(from);
        var toDay = TargetPeriod.Format(to);
        var targets = await db.SalesTargets.AsNoTracking()
            .Where(t => t.TenantId == tenant.Id && !t.IsDeleted
                        && string.Compare(t.PeriodStartDay, toDay) <= 0 && string.Compare(t.PeriodEndDay, fromDay) >= 0)
            .ToListAsync(ct);
        var editors = targets.Select(t => t.UpdatedByUserId).Distinct().ToList();
        var names = await db.MobileUsers.AsNoTracking()
            .Where(u => u.TenantId == tenant.Id && editors.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => string.IsNullOrWhiteSpace(u.FullName) ? u.Username : u.FullName, ct);
        var directory = await TeamDirectory.LoadAsync(db, tenant.Id, ct);
        return (new TargetBoard(factSet, directory, targets, names, await WorkDaysAsync(db, tenant.Id, ct), Today()), factSet);
    }

    private static string[] Warnings(TargetFactSet set) => set.Source == TargetSources.PhoneDocuments
        ? ["Ajan plasiyer kodunu henüz göndermiyor (ajan 1.3.0 öncesi); gerçekleşen, telefonlardan gelen belgelerden hesaplanıyor. Ofiste kesilen faturalar sayılmıyor."]
        : [];

    public sealed record Person(Guid Id, string Name, bool IsActive);

    private static async Task<List<Person>> PeopleAsync(CentralApiDbContext db, Guid tenantId, CancellationToken ct)
    {
        var users = await db.MobileUsers.AsNoTracking().Include(u => u.Roles)
            .Where(u => u.TenantId == tenantId && u.DeletedAtUtc == null)
            .ToListAsync(ct);
        // The sales force: whoever works on the phone, the warehouse-only staff aside.
        return users.Where(u => RolePermissions.CanUsePhone(u) && !RolePermissions.IsWarehouseOnlyOnPhone(u))
            .Select(u => new Person(u.Id, string.IsNullOrWhiteSpace(u.FullName) ? u.Username : u.FullName, u.IsActive))
            .ToList();
    }

    private static readonly StringComparer TurkishText = StringComparer.Create(CultureInfo.GetCultureInfo("tr-TR"), CompareOptions.IgnoreCase);

    // ---- writing ------------------------------------------------------------------------------

    private sealed record Valid(TargetPeriod Period, string Metric, string Measure, string ItemCode, string? ItemName, string OwnerKind, Guid OwnerId, decimal? Value, string? Note);

    /// <summary>
    /// Writes a batch all or nothing: any invalid item or any owner outside the caller's scope rejects the whole
    /// save with the item's index. A repeated <see cref="TargetsSaveRequest.OperationId"/> answers <c>duplicate</c>.
    /// </summary>
    public async Task<TargetsSaveResponse> SaveAsync(CentralApiDbContext db, Tenant tenant, TeamScope scope, TargetsSaveRequest request, CancellationToken ct)
    {
        if (request.OperationId == Guid.Empty)
            return Error(-1, "TARGET_OPERATION_REQUIRED", "Kayıt işlemi kimliği gerekli.");
        if (request.Items.Length > MaxItemsPerSave)
            return Error(-1, "TARGET_BATCH_TOO_LARGE", $"Bir seferde en çok {MaxItemsPerSave} hedef kaydedilebilir.");
        if (await db.SalesTargetOperations.AnyAsync(o => o.TenantId == tenant.Id && o.OperationId == request.OperationId, ct))
            return new TargetsSaveResponse { Duplicate = true };

        var users = await db.MobileUsers.AsNoTracking()
            .Where(u => u.TenantId == tenant.Id && u.DeletedAtUtc == null).Select(u => u.Id).ToListAsync(ct);
        var valid = new List<Valid>(request.Items.Length);
        var errors = new List<ItemError>();
        for (var i = 0; i < request.Items.Length; i++)
        {
            var (item, error) = Validate(request.Items[i], scope, users);
            if (error is not null) errors.Add(error with { Index = i });
            else valid.Add(item!);
        }
        if (errors.Count > 0) return new TargetsSaveResponse { Errors = [.. errors.Select(e => e.Dto)] };
        var duplicateKey = valid.GroupBy(Key).FirstOrDefault(g => g.Count() > 1);
        if (duplicateKey is not null)
            return Error(valid.IndexOf(duplicateKey.Skip(1).First()), "TARGET_DUPLICATE_ITEM", "Aynı hedef bu kayıtta iki kez var.");

        var response = new TargetsSaveResponse();
        var now = NowMs();
        var actorName = Clip(string.IsNullOrWhiteSpace(scope.User.FullName) ? scope.User.Username : scope.User.FullName, 120);
        var relational = db.Database.IsRelational();
        await using var transaction = relational ? await db.Database.BeginTransactionAsync(ct) : null;

        // The operation row first: a retry overlapping the first request waits on its primary key until the first
        // commits, then fails here and is answered as the duplicate it is (Codex, PR #214).
        db.SalesTargetOperations.Add(new SalesTargetOperation { TenantId = tenant.Id, OperationId = request.OperationId, UserId = scope.User.Id, AppliedAtMs = now });
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            if (transaction is not null) await transaction.RollbackAsync(ct);
            db.ChangeTracker.Clear();
            return new TargetsSaveResponse { Duplicate = true };
        }

        var keys = valid.Select(v => (v.Period.Type, v.Period.Key)).Distinct().ToList();
        var periodKeys = keys.Select(k => k.Key).ToList();
        var existing = (await db.SalesTargets
                .Where(t => t.TenantId == tenant.Id && periodKeys.Contains(t.PeriodKey))
                .ToListAsync(ct))
            .ToDictionary(t => (t.PeriodType, t.PeriodKey, t.Metric, t.Measure, t.ItemCode, t.OwnerKind, t.OwnerId));

        foreach (var item in valid)
        {
            existing.TryGetValue(Key(item), out var row);
            if (item.Value is null)
            {
                if (row is null || row.IsDeleted) { response.Unchanged++; continue; }
                Log(db, tenant.Id, row.Id, "DELETE", row.Value, null, scope.User.Id, actorName, now, request.OperationId);
                row.IsDeleted = true;
                row.UpdatedAtMs = now;
                row.UpdatedByUserId = scope.User.Id;
                response.Deleted++;
                continue;
            }
            if (row is { IsDeleted: false } && row.Value == item.Value && row.Note == item.Note)
            {
                response.Unchanged++;
                continue;
            }
            if (row is null)
            {
                row = new SalesTarget
                {
                    TenantId = tenant.Id,
                    PeriodType = item.Period.Type,
                    PeriodKey = item.Period.Key,
                    PeriodStartDay = TargetPeriod.Format(item.Period.Start),
                    PeriodEndDay = TargetPeriod.Format(item.Period.End),
                    Metric = item.Metric,
                    Measure = item.Measure,
                    ItemCode = item.ItemCode,
                    OwnerKind = item.OwnerKind,
                    OwnerId = item.OwnerId,
                    CreatedByUserId = scope.User.Id,
                    CreatedAtMs = now,
                };
                db.SalesTargets.Add(row);
                existing[Key(item)] = row;
            }
            Log(db, tenant.Id, row.Id, "SET", row.IsDeleted || row.CreatedAtMs == now ? null : row.Value, item.Value, scope.User.Id, actorName, now, request.OperationId);
            row.Value = item.Value.Value;
            row.Note = item.Note;
            row.ItemName = item.ItemName ?? row.ItemName;
            row.IsDeleted = false;
            row.UpdatedAtMs = now;
            row.UpdatedByUserId = scope.User.Id;
            response.Saved++;
        }

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Another save created one of these targets in the meantime (the natural key is unique).
            if (transaction is not null) await transaction.RollbackAsync(ct);
            db.ChangeTracker.Clear();
            return Error(-1, "TARGET_CONFLICT", "Aynı hedef şu anda başka biri tarafından da kaydedildi. Sayfayı yenileyip tekrar deneyin.");
        }
        if (transaction is not null) await transaction.CommitAsync(ct);
        return response;
    }

    private static (string, string, string, string, string, string, Guid) Key(Valid v) =>
        (v.Period.Type, v.Period.Key, v.Metric, v.Measure, v.ItemCode, v.OwnerKind, v.OwnerId);

    private static void Log(CentralApiDbContext db, Guid tenantId, Guid targetId, string action, decimal? oldValue, decimal? newValue,
        Guid actor, string actorName, long now, Guid operationId) =>
        db.SalesTargetEvents.Add(new SalesTargetEvent
        {
            TenantId = tenantId, TargetId = targetId, Action = action, OldValue = oldValue, NewValue = newValue,
            ActorUserId = actor, ActorName = actorName, OccurredAtMs = now, OperationId = operationId,
        });

    private sealed record ItemError(int Index, string Code, string Message)
    {
        public TargetErrorDto Dto => new() { Index = Index, ErrorCode = Code, Message = Message };
    }

    private static (Valid? Item, ItemError? Error) Validate(TargetInput input, TeamScope scope, IReadOnlyCollection<Guid> users)
    {
        static (Valid?, ItemError?) Bad(string code, string message) => (null, new ItemError(0, code, message));

        var type = input.PeriodType?.Trim().ToUpperInvariant();
        if (!TargetPeriod.TryParse(type, input.PeriodKey, out var period))
            return Bad("TARGET_INVALID_PERIOD", "Dönem okunamadı (gün yyyy-MM-dd, hafta yyyy-Www, ay yyyy-MM).");
        var metric = input.Metric?.Trim().ToUpperInvariant() ?? string.Empty;
        if (!TargetMetrics.All.Contains(metric)) return Bad("TARGET_INVALID_METRIC", "Bilinmeyen hedef türü.");
        var measures = TargetMetrics.MeasuresOf(metric);
        var measure = string.IsNullOrWhiteSpace(input.Measure) ? measures[0] : input.Measure.Trim().ToUpperInvariant();
        if (!measures.Contains(measure)) return Bad("TARGET_INVALID_MEASURE", "Bu hedef türü bu ölçüyle girilemez.");
        var itemCode = input.ItemCode?.Trim() ?? string.Empty;
        if (TargetMetrics.HasItem(metric) && itemCode.Length == 0) return Bad("TARGET_ITEM_REQUIRED", "Ürün, kategori veya marka seçilmeli.");
        if (!TargetMetrics.HasItem(metric)) itemCode = string.Empty;
        if (itemCode.Length > MaxItemCodeLength) return Bad("TARGET_INVALID_ITEM", $"Kod en çok {MaxItemCodeLength} karakter olabilir.");
        var itemName = string.IsNullOrWhiteSpace(input.ItemName) || itemCode.Length == 0 ? null : Clip(input.ItemName, MaxItemNameLength);

        var ownerKind = input.OwnerKind?.Trim().ToUpperInvariant() ?? string.Empty;
        if (!TargetOwnerKinds.All.Contains(ownerKind)) return Bad("TARGET_INVALID_OWNER", "Hedefin sahibi kişi, ekip ya da firma olmalı.");
        var ownerId = ownerKind == TargetOwnerKinds.Company ? Guid.Empty : input.OwnerId ?? Guid.Empty;
        if (ownerKind == TargetOwnerKinds.User && !users.Contains(ownerId)) return Bad("TARGET_INVALID_OWNER", "Kullanıcı bulunamadı.");
        if (ownerKind == TargetOwnerKinds.Team && !scope.Directory.Teams.ContainsKey(ownerId)) return Bad("TARGET_INVALID_OWNER", "Ekip bulunamadı.");
        if (!scope.MayWrite(ownerKind, ownerId))
            return Bad("TARGET_OUT_OF_SCOPE", ownerKind == TargetOwnerKinds.Company
                ? "Firma geneli hedefi yalnız tüm firmayı gören yönetici girebilir."
                : "Bu kişi ya da ekip sizin sorumluluğunuzda değil.");

        decimal? value = null;
        if (input.Value is { } raw)
        {
            if (raw < 0 || raw > MaxValue) return Bad("TARGET_INVALID_VALUE", "Hedef değeri 0 ile 1 trilyon arasında olmalı.");
            value = measure == TargetMeasures.Count ? Math.Round(raw, 0, MidpointRounding.AwayFromZero) : Math.Round(raw, 4, MidpointRounding.AwayFromZero);
        }
        var note = string.IsNullOrWhiteSpace(input.Note) ? null : input.Note.Trim();
        if (note is { Length: > MaxNoteLength }) return Bad("TARGET_INVALID_NOTE", $"Not en çok {MaxNoteLength} karakter olabilir.");
        return (new Valid(period, metric, measure, itemCode, itemName, ownerKind, ownerId, value, note), null);
    }

    private static TargetsSaveResponse Error(int index, string code, string message) =>
        new() { Errors = [new TargetErrorDto { Index = index, ErrorCode = code, Message = message }] };

    // ---- copy and distribute -------------------------------------------------------------------------

    /// <summary>
    /// The targets of one period carried to another of the same type, each changed by <see cref="TargetCopyRequest.Percent"/>;
    /// only owners the caller may write. Targets the new period already has are kept unless asked to overwrite.
    /// </summary>
    public async Task<(TargetPreviewResponse? Response, TargetErrorDto? Error)> CopyAsync(
        CentralApiDbContext db, Tenant tenant, TeamScope scope, TargetCopyRequest request, CancellationToken ct)
    {
        var type = request.PeriodType?.Trim().ToUpperInvariant();
        if (!TargetPeriod.TryParse(type, request.FromPeriodKey, out var from) || !TargetPeriod.TryParse(type, request.ToPeriodKey, out var to))
            return (null, Err("TARGET_INVALID_PERIOD", "Dönem okunamadı."));
        if (from.Key == to.Key) return (null, Err("TARGET_INVALID_PERIOD", "Kaynak ve hedef dönem aynı."));
        if (request.Percent is < -100 or > 1000) return (null, Err("TARGET_INVALID_VALUE", "Değişim yüzdesi -100 ile 1000 arasında olmalı."));
        var kinds = request.OwnerKinds is { Length: > 0 } list ? list.Select(k => k.Trim().ToUpperInvariant()).ToHashSet() : null;

        var source = await db.SalesTargets.AsNoTracking()
            .Where(t => t.TenantId == tenant.Id && !t.IsDeleted && t.PeriodType == from.Type && t.PeriodKey == from.Key)
            .ToListAsync(ct);
        var present = (await db.SalesTargets.AsNoTracking()
                .Where(t => t.TenantId == tenant.Id && !t.IsDeleted && t.PeriodType == to.Type && t.PeriodKey == to.Key)
                .Select(t => new { t.Metric, t.Measure, t.ItemCode, t.OwnerKind, t.OwnerId })
                .ToListAsync(ct))
            .Select(t => (t.Metric, t.Measure, t.ItemCode, t.OwnerKind, t.OwnerId)).ToHashSet();

        var factor = 1m + request.Percent / 100m;
        var items = source
            .Where(t => (kinds is null || kinds.Contains(t.OwnerKind)) && scope.MayWrite(t.OwnerKind, t.OwnerId))
            .Where(t => request.Overwrite || !present.Contains((t.Metric, t.Measure, t.ItemCode, t.OwnerKind, t.OwnerId)))
            .Select(t => new TargetInput
            {
                PeriodType = to.Type, PeriodKey = to.Key, Metric = t.Metric, Measure = t.Measure, ItemCode = t.ItemCode, ItemName = t.ItemName,
                OwnerKind = t.OwnerKind, OwnerId = t.OwnerKind == TargetOwnerKinds.Company ? null : t.OwnerId, Note = t.Note,
                Value = t.Measure == TargetMeasures.Count ? Math.Round(t.Value * factor, 0, MidpointRounding.AwayFromZero) : TargetBoard.Round(t.Value * factor),
            })
            .ToArray();

        var response = new TargetPreviewResponse { Items = items };
        if (request.Apply)
            response.Result = await SaveAsync(db, tenant, scope, new TargetsSaveRequest { OperationId = request.OperationId, Items = items }, ct);
        return (response, null);
    }

    /// <summary>
    /// A company or team value shared out one level down (K11): equally, or in proportion to what each did in the
    /// previous period (equally when nobody did anything). Only a preview; the panel saves it with <see cref="SaveAsync"/>.
    /// Rounding leftovers go to the largest share, so the parts add up to the value.
    /// </summary>
    public async Task<(TargetPreviewResponse? Response, TargetErrorDto? Error)> DistributeAsync(
        CentralApiDbContext db, Tenant tenant, TeamScope scope, TargetDistributeRequest request, CancellationToken ct)
    {
        var type = request.PeriodType?.Trim().ToUpperInvariant();
        if (!TargetPeriod.TryParse(type, request.PeriodKey, out var period)) return (null, Err("TARGET_INVALID_PERIOD", "Dönem okunamadı."));
        var metric = request.Metric?.Trim().ToUpperInvariant() ?? string.Empty;
        if (!TargetMetrics.All.Contains(metric)) return (null, Err("TARGET_INVALID_METRIC", "Bilinmeyen hedef türü."));
        var measure = string.IsNullOrWhiteSpace(request.Measure) ? TargetMetrics.MeasuresOf(metric)[0] : request.Measure.Trim().ToUpperInvariant();
        var itemCode = TargetMetrics.HasItem(metric) ? request.ItemCode?.Trim() ?? string.Empty : string.Empty;
        var sourceKind = request.SourceOwnerKind?.Trim().ToUpperInvariant();
        var sourceId = sourceKind == TargetOwnerKinds.Company ? Guid.Empty : request.SourceOwnerId ?? Guid.Empty;
        var level = request.TargetLevel?.Trim().ToUpperInvariant() ?? TargetOwnerKinds.User;
        var method = request.Method?.Trim().ToUpperInvariant() ?? "EQUAL";
        if (method is not ("EQUAL" or "LAST_PERIOD_SHARE")) return (null, Err("TARGET_INVALID_METHOD", "Dağıtım yöntemi EQUAL ya da LAST_PERIOD_SHARE olmalı."));
        if (level is not (TargetOwnerKinds.User or TargetOwnerKinds.Team)) return (null, Err("TARGET_INVALID_LEVEL", "Dağıtım kişilere ya da ekiplere yapılır."));
        var directory = scope.Directory;

        List<(string Kind, Guid Id)> children;
        if (sourceKind == TargetOwnerKinds.Company && scope.WholeCompany)
        {
            children = level == TargetOwnerKinds.Team
                ? directory.Teams.Values.Where(t => t.IsActive && t.ParentId is null).Select(t => (TargetOwnerKinds.Team, t.Id)).ToList()
                : (await PeopleAsync(db, tenant.Id, ct)).Where(p => p.IsActive).Select(p => (TargetOwnerKinds.User, p.Id)).ToList();
        }
        else if (sourceKind == TargetOwnerKinds.Team && scope.SeesTeam(sourceId) && directory.Teams.TryGetValue(sourceId, out var team))
        {
            children = level == TargetOwnerKinds.Team && team.Kind == SalesTeamKinds.Region
                ? directory.Teams.Values.Where(t => t.IsActive && t.ParentId == team.Id).Select(t => (TargetOwnerKinds.Team, t.Id)).ToList()
                : directory.MembersOf(team.Id).Select(u => (TargetOwnerKinds.User, u)).ToList();
        }
        else return (null, Err("TARGET_OUT_OF_SCOPE", "Bu kaynak sizin sorumluluğunuzda değil."));
        children = children.Where(c => scope.MayWrite(c.Kind, c.Id)).ToList();
        if (children.Count == 0) return (null, Err("TARGET_NO_CHILDREN", "Dağıtılacak kişi ya da ekip yok."));

        var value = request.Value;
        if (value is null)
        {
            var target = await db.SalesTargets.AsNoTracking().FirstOrDefaultAsync(t => t.TenantId == tenant.Id && !t.IsDeleted
                && t.PeriodType == period.Type && t.PeriodKey == period.Key && t.Metric == metric && t.Measure == measure
                && t.ItemCode == itemCode && t.OwnerKind == sourceKind && t.OwnerId == sourceId, ct);
            value = target?.Value;
        }
        if (value is not { } total || total < 0) return (null, Err("TARGET_INVALID_VALUE", "Dağıtılacak değer yok."));

        var weights = children.ToDictionary(c => c, _ => 1m);
        if (method == "LAST_PERIOD_SHARE")
        {
            var previous = period.Previous();
            var (board, _) = await LoadBoardAsync(db, tenant, [previous], ct);
            foreach (var child in children)
                weights[child] = Math.Max(0m, board.Measure(board.UsersOf(child.Kind, child.Id), previous.Start, previous.End, metric, measure, itemCode, pending: false));
            if (weights.Values.Sum() == 0m) foreach (var child in children) weights[child] = 1m;
        }

        var whole = measure == TargetMeasures.Count;
        var weightSum = weights.Values.Sum();
        var parts = children.ToDictionary(c => c, c => RoundShare(total * weights[c] / weightSum, whole));
        var largest = children.OrderByDescending(c => weights[c]).First();
        parts[largest] += total - parts.Values.Sum();

        return (new TargetPreviewResponse
        {
            Items = children.Select(c => new TargetInput
            {
                PeriodType = period.Type, PeriodKey = period.Key, Metric = metric, Measure = measure, ItemCode = itemCode,
                ItemName = request.ItemName, OwnerKind = c.Kind, OwnerId = c.Id, Value = parts[c],
            }).ToArray(),
        }, null);
    }

    private static decimal RoundShare(decimal value, bool whole) =>
        whole ? Math.Floor(value) : Math.Round(value, 2, MidpointRounding.ToZero);

    private static TargetErrorDto Err(string code, string message) => new() { Index = -1, ErrorCode = code, Message = message };

    // ---- item picker and settings ------------------------------------------------------------------

    /// <summary>Products, categories, sub-categories or brands for the picker, matching <paramref name="q"/>.</summary>
    public async Task<TargetItemsResponse> ItemsAsync(CentralApiDbContext db, Tenant tenant, string metric, string? q, CancellationToken ct)
    {
        var catalog = await PortalStockCatalog.LoadAsync(db, cache, tenant.Id, ct);
        var query = q?.Trim() ?? string.Empty;
        bool Hit(string code, string name) => query.Length == 0
            || code.Contains(query, StringComparison.CurrentCultureIgnoreCase) || name.Contains(query, StringComparison.CurrentCultureIgnoreCase);

        IEnumerable<TargetItemDto> items = metric switch
        {
            TargetMetrics.Product => catalog.Products
                .Where(p => Hit(p.Code, p.Name))
                .OrderBy(p => p.Name, TurkishText)
                .Select(p => new TargetItemDto { Code = p.Code, Name = p.Name, ProductCount = 1 }),
            TargetMetrics.Category => Groups(catalog.Products, p => p.MainGroup, catalog.MainGroupNames),
            TargetMetrics.SubCategory => Groups(catalog.Products, p => p.SubGroup is null ? null : $"{p.MainGroup}|{p.SubGroup}", null),
            TargetMetrics.Brand => Groups(catalog.Products, p => p.Brand, catalog.BrandNames),
            _ => [],
        };
        if (metric != TargetMetrics.Product) items = items.Where(i => Hit(i.Code, i.Name));
        var list = items.Take(MaxPickerItems + 1).ToList();
        return new TargetItemsResponse { Items = [.. list.Take(MaxPickerItems)], Truncated = list.Count > MaxPickerItems };
    }

    /// <summary>The codes products carry, named from the agent's lookups when it sends them (1.3.0), else by the code.</summary>
    private static IEnumerable<TargetItemDto> Groups(IEnumerable<PortalStockCatalog.Product> products, Func<PortalStockCatalog.Product, string?> code,
        IReadOnlyDictionary<string, string>? names) =>
        products.Select(p => code(p)).Where(c => !string.IsNullOrWhiteSpace(c))
            .GroupBy(c => c!, StringComparer.OrdinalIgnoreCase)
            .Select(g => new TargetItemDto { Code = g.Key, Name = names?.GetValueOrDefault(g.Key) ?? g.Key, ProductCount = g.Count() })
            .OrderBy(i => i.Name, TurkishText);

    public static async Task<int> WorkDaysAsync(CentralApiDbContext db, Guid tenantId, CancellationToken ct) =>
        await db.TargetSettings.AsNoTracking().Where(s => s.TenantId == tenantId).Select(s => (int?)s.WorkDays).FirstOrDefaultAsync(ct)
        ?? TargetSettings.DefaultWorkDays;

    public static async Task SetWorkDaysAsync(CentralApiDbContext db, Guid tenantId, int workDays, CancellationToken ct)
    {
        var row = await db.TargetSettings.FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);
        if (row is null) db.TargetSettings.Add(row = new TargetSettings { TenantId = tenantId });
        row.WorkDays = workDays;
        row.UpdatedAtMs = NowMs();
        await db.SaveChangesAsync(ct);
    }

    private static string Clip(string value, int max)
    {
        var trimmed = value.Trim();
        return trimmed.Length <= max ? trimmed : trimmed[..max];
    }
}

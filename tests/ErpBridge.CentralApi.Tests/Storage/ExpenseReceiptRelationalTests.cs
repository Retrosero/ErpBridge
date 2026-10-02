using System.Net;
using System.Net.Http.Headers;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Storage;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Storage;

/// <summary>
/// GOAL_DEPOLAMA_R2 S5: receipt photos of phone expense and vehicle maintenance documents go to the private bucket under
/// the document's phone id (before or after the document itself), count against the company quota, open to the uploader
/// and to who sees every receipt (admin, manager, accounting) — the phone path streams them, the storage path redirects —
/// and a deleted one goes to the trash. The panel lists them by day with their document and a short presigned address.
/// </summary>
public sealed class ExpenseReceiptRelationalTests : IClassFixture<StorageCentralApiFactory>
{
    private static readonly byte[] Jpeg = FileStoreRelationalTests.Jpeg(300);

    private readonly StorageCentralApiFactory _factory;

    public ExpenseReceiptRelationalTests(StorageCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task A_receipt_goes_to_the_private_bucket_under_its_document_and_a_retry_is_the_same_receipt()
    {
        var company = await StorageTestCompany.CreateAsync(_factory);
        var doc = "K-" + Guid.NewGuid();
        var id = Guid.NewGuid();

        var uploaded = await UploadAsync(company["ali"].Token, doc, id, Jpeg);
        uploaded.StatusCode.Should().Be(HttpStatusCode.OK, await uploaded.Content.ReadAsStringAsync());
        var dto = await uploaded.ReadAsJsonAsync<ExpenseAttachmentDto>();
        dto.Should().Match<ExpenseAttachmentDto>(d => d.Id == id && d.DocumentId == doc && d.Kind == "expense" && d.ContentType == "image/jpeg"
            && d.SizeBytes == Jpeg.Length && d.CreatedByUserId == company["ali"].Id && d.CreatedByName == "ali");
        (await UploadAsync(company["ali"].Token, doc, id, Jpeg)).StatusCode.Should().Be(HttpStatusCode.OK, "a retried upload is the same receipt");

        var file = await FileOfAsync(id);
        file.Should().Match<StoredFile>(f => f.Area == "expense" && f.Bucket == "private" && f.OwnerType == "expense" && f.OwnerKey == doc
            && f.Status == StoredFileStatuses.Active && f.CreatedByUserId == company["ali"].Id);
        _factory.Store.Bytes("private", file.ObjectKey).Should().Equal(Jpeg);
        (await ReadAsync(db => db.StoredFiles.CountAsync(f => f.OwnerKey == doc))).Should().Be(1, "the retry stored nothing new");
        (await ReadAsync(db => db.TenantStorage.AsNoTracking().SingleAsync(s => s.TenantId == company.TenantId))).UsedBytes.Should().Be(Jpeg.Length);

        var vehicle = Guid.NewGuid();
        (await UploadAsync(company["ali"].Token, doc, vehicle, Jpeg, "vehicle_maintenance")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await FileOfAsync(vehicle)).Area.Should().Be("vehicle");

        // The phone streams it back with the immutable cache header a task picture has.
        var download = await SendAsync(HttpMethod.Get, $"/api/v1/android/expenses/{doc}/attachments/{id}", company["ali"].Token);
        download.StatusCode.Should().Be(HttpStatusCode.OK);
        (await download.Content.ReadAsByteArrayAsync()).Should().Equal(Jpeg);
        download.Headers.CacheControl!.Private.Should().BeTrue();
        download.Headers.CacheControl.MaxAge.Should().Be(TimeSpan.FromDays(365));

        var list = await (await SendAsync(HttpMethod.Get, $"/api/v1/android/expenses/{doc}/attachments", company["ali"].Token)).ReadAsJsonAsync<ExpenseAttachmentListResponse>();
        list.Items.Select(i => i.Id).Should().Equal(id, vehicle);
    }

    [Fact]
    public async Task Only_the_uploader_and_who_sees_every_receipt_open_one()
    {
        var company = await StorageTestCompany.CreateAsync(_factory);
        var other = await StorageTestCompany.CreateAsync(_factory);
        var doc = "K-" + Guid.NewGuid();
        var id = Guid.NewGuid();
        (await UploadAsync(company["ali"].Token, doc, id, Jpeg)).StatusCode.Should().Be(HttpStatusCode.OK);
        var file = await FileOfAsync(id);

        foreach (var user in new[] { "ali", "mudur", "patron", "muhasebe" })
        {
            (await SendAsync(HttpMethod.Get, $"/api/v1/android/expenses/{doc}/attachments/{id}", company[user].Token)).StatusCode.Should().Be(HttpStatusCode.OK, user);
            var redirect = await SendAsync(HttpMethod.Get, $"/api/v1/storage/files/{file.Id}", company[user].Token);
            redirect.StatusCode.Should().Be(HttpStatusCode.Redirect, user);
            redirect.Headers.Location!.AbsoluteUri.Should().StartWith($"https://r2.test/private/{file.ObjectKey}?");
        }

        foreach (var (token, who) in new[] { (company["veli"].Token, "a colleague"), (other["patron"].Token, "another company") })
        {
            await ShouldFailAsync(await SendAsync(HttpMethod.Get, $"/api/v1/android/expenses/{doc}/attachments/{id}", token), HttpStatusCode.NotFound, "EXPENSE_ATTACHMENT_NOT_FOUND");
            (await SendAsync(HttpMethod.Get, $"/api/v1/storage/files/{file.Id}", token)).StatusCode.Should().Be(HttpStatusCode.NotFound, who);
            (await (await SendAsync(HttpMethod.Get, $"/api/v1/android/expenses/{doc}/attachments", token)).ReadAsJsonAsync<ExpenseAttachmentListResponse>())
                .Items.Should().BeEmpty(who);
            await ShouldFailAsync(await SendAsync(HttpMethod.Delete, $"/api/v1/android/expenses/{doc}/attachments/{id}", token), HttpStatusCode.NotFound, "EXPENSE_ATTACHMENT_NOT_FOUND");
        }

        // The accountant sees it but does not delete it; the uploader does, into the trash.
        await ShouldFailAsync(await SendAsync(HttpMethod.Delete, $"/api/v1/android/expenses/{doc}/attachments/{id}", company["muhasebe"].Token),
            HttpStatusCode.Forbidden, "EXPENSE_FORBIDDEN");
        (await SendAsync(HttpMethod.Delete, $"/api/v1/android/expenses/{doc}/attachments/{id}", company["ali"].Token)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await SendAsync(HttpMethod.Delete, $"/api/v1/android/expenses/{doc}/attachments/{id}", company["ali"].Token)).StatusCode.Should().Be(HttpStatusCode.NoContent, "deleting again changes nothing");
        (await ReadAsync(db => db.StoredFiles.AsNoTracking().SingleAsync(f => f.Id == file.Id))).Status.Should().Be(StoredFileStatuses.Trashed);
        await ShouldFailAsync(await SendAsync(HttpMethod.Get, $"/api/v1/android/expenses/{doc}/attachments/{id}", company["ali"].Token), HttpStatusCode.NotFound, "EXPENSE_ATTACHMENT_NOT_FOUND");
        (await UploadAsync(company["ali"].Token, doc, id, Jpeg)).StatusCode.Should().Be(HttpStatusCode.Conflict, "a deleted receipt's id is not reused");
    }

    [Fact]
    public async Task Someone_elses_document_takes_receipts_only_from_who_sees_every_receipt()
    {
        var company = await StorageTestCompany.CreateAsync(_factory);
        var doc = "K-" + Guid.NewGuid();
        await SeedAsync(db => db.Jobs.Add(new Job
        {
            TenantId = company.TenantId, ExternalId = doc, DocumentType = "expense", CreatedByUserId = company["ali"].Id,
            PayloadJson = """{"amount":125.5,"description":"Yakıt","counterparty":"Gider: Yakıt","occurredAt":"01.10.2026 10:15","expenseCardCode":"770"}""",
        }));

        await ShouldFailAsync(await UploadAsync(company["veli"].Token, doc, Guid.NewGuid(), Jpeg), HttpStatusCode.Forbidden, "EXPENSE_FORBIDDEN");
        (await UploadAsync(company["ali"].Token, doc, Guid.NewGuid(), Jpeg)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await UploadAsync(company["mudur"].Token, doc, Guid.NewGuid(), Jpeg)).StatusCode.Should().Be(HttpStatusCode.OK, "an office user adds the paper");

        // A document not on the server yet belongs to whoever sent its first receipt.
        var early = "K-" + Guid.NewGuid();
        (await UploadAsync(company["ali"].Token, early, Guid.NewGuid(), Jpeg)).StatusCode.Should().Be(HttpStatusCode.OK);
        await ShouldFailAsync(await UploadAsync(company["veli"].Token, early, Guid.NewGuid(), Jpeg), HttpStatusCode.Forbidden, "EXPENSE_FORBIDDEN");
    }

    [Fact]
    public async Task Uploads_are_checked_and_count_against_the_company_quota()
    {
        var company = await StorageTestCompany.CreateAsync(_factory);
        var token = company["ali"].Token;
        var doc = "K-" + Guid.NewGuid();

        await ShouldFailAsync(await UploadAsync(token, "bad id", Guid.NewGuid(), Jpeg), HttpStatusCode.BadRequest, "INVALID_DOCUMENT_ID");
        await ShouldFailAsync(await UploadAsync(token, doc, Guid.NewGuid(), Jpeg, "fuel"), HttpStatusCode.BadRequest, "INVALID_EXPENSE_KIND");
        await ShouldFailAsync(await UploadAsync(token, doc, Guid.NewGuid(), "<svg/>"u8.ToArray()), HttpStatusCode.UnsupportedMediaType, "INVALID_IMAGE");
        await ShouldFailAsync(await UploadAsync(token, doc, Guid.NewGuid(), FileStoreRelationalTests.Jpeg(2 * 1024 * 1024)), HttpStatusCode.RequestEntityTooLarge,
            "EXPENSE_ATTACHMENT_TOO_LARGE");
        for (var i = 0; i < 5; i++) (await UploadAsync(token, doc, Guid.NewGuid(), Jpeg)).StatusCode.Should().Be(HttpStatusCode.OK);
        await ShouldFailAsync(await UploadAsync(token, doc, Guid.NewGuid(), Jpeg), HttpStatusCode.Conflict, "EXPENSE_ATTACHMENT_LIMIT");

        await SeedAsync(db => db.TenantStorage.Single(s => s.TenantId == company.TenantId).QuotaBytes = 5L * Jpeg.Length + 10);
        var refused = await UploadAsync(token, "K-" + Guid.NewGuid(), Guid.NewGuid(), Jpeg);
        refused.StatusCode.Should().Be(HttpStatusCode.RequestEntityTooLarge);
        (await refused.ReadAsJsonAsync<StorageErrorResponse>()).Should().Match<StorageErrorResponse>(e =>
            e.ErrorCode == "STORAGE_QUOTA_EXCEEDED" && e.UsedBytes == 5L * Jpeg.Length && e.QuotaBytes == 5L * Jpeg.Length + 10);

        _factory.Store.FailNextPuts(1);
        await SeedAsync(db => db.TenantStorage.Single(s => s.TenantId == company.TenantId).QuotaBytes = null);
        await ShouldFailAsync(await UploadAsync(token, "K-" + Guid.NewGuid(), Guid.NewGuid(), Jpeg), HttpStatusCode.ServiceUnavailable, "STORAGE_UNAVAILABLE");
    }

    [Fact]
    public async Task Parallel_uploads_to_one_document_never_go_past_the_limit()
    {
        var company = await StorageTestCompany.CreateAsync(_factory);
        var token = company["ali"].Token;
        var doc = "K-" + Guid.NewGuid();
        for (var i = 0; i < 4; i++) (await UploadAsync(token, doc, Guid.NewGuid(), Jpeg)).StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await Task.WhenAll(Enumerable.Range(0, 6).Select(_ => Task.Run(() => UploadAsync(token, doc, Guid.NewGuid(), Jpeg))));

        results.Count(r => r.StatusCode == HttpStatusCode.OK).Should().Be(1);
        results.Where(r => r.StatusCode != HttpStatusCode.OK).Should().OnlyContain(r => r.StatusCode == HttpStatusCode.Conflict);
        (await ReadAsync(db => db.ExpenseAttachments.CountAsync(a => a.DocumentExternalId == doc))).Should().Be(5);
        (await ReadAsync(db => db.StoredFiles.CountAsync(f => f.OwnerKey == doc && f.Status == StoredFileStatuses.Active))).Should().Be(5,
            "a refused upload's file is purged");
    }

    [Fact]
    public async Task A_file_whose_record_is_gone_is_trashed_by_the_daily_sweep()
    {
        var company = await StorageTestCompany.CreateAsync(_factory);
        var doc = "K-" + Guid.NewGuid();
        var id = Guid.NewGuid();
        (await UploadAsync(company["ali"].Token, doc, id, Jpeg)).StatusCode.Should().Be(HttpStatusCode.OK);
        var kept = Guid.NewGuid();
        (await UploadAsync(company["ali"].Token, doc, kept, Jpeg)).StatusCode.Should().Be(HttpStatusCode.OK);
        var file = await FileOfAsync(id);
        // The record was deleted but trashing its file failed afterwards: the file is still active.
        await SeedAsync(db => db.ExpenseAttachments.Single(a => a.Id == id).IsDeleted = true);

        using (var scope = _factory.Services.CreateScope())
        {
            var maintenance = scope.ServiceProvider.GetRequiredService<StorageMaintenance>();
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            (await maintenance.TrashUnreferencedAsync(now, default)).Should().Be(0, "a fresh file may still be waiting for its record");
            (await maintenance.TrashUnreferencedAsync(now + (long)StorageMaintenance.UnreferencedGrace.TotalMilliseconds + 1000, default))
                .Should().BeGreaterThanOrEqualTo(1);
        }

        (await ReadAsync(db => db.StoredFiles.AsNoTracking().SingleAsync(f => f.Id == file.Id))).Status.Should().Be(StoredFileStatuses.Trashed);
        (await FileOfAsync(kept)).Status.Should().Be(StoredFileStatuses.Active, "a file a live record points at stays");
    }

    [Fact]
    public async Task The_panel_lists_the_receipts_with_their_document_and_a_short_address()
    {
        var company = await StorageTestCompany.CreateAsync(_factory);
        var doc = "K-" + Guid.NewGuid();
        await SeedAsync(db => db.Jobs.Add(new Job
        {
            TenantId = company.TenantId, ExternalId = doc, DocumentType = "expense", CreatedByUserId = company["ali"].Id,
            PayloadJson = """{"amount":125.5,"description":"Yakıt","counterparty":"Gider: Yakıt","occurredAt":"01.10.2026 10:15","expenseCardCode":"770"}""",
        }));
        var withDocument = Guid.NewGuid();
        var pending = Guid.NewGuid();
        var pendingDoc = "K-" + Guid.NewGuid();
        (await UploadAsync(company["ali"].Token, doc, withDocument, Jpeg)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await UploadAsync(company["veli"].Token, pendingDoc, pending, Jpeg, "vehicle_maintenance")).StatusCode.Should().Be(HttpStatusCode.OK);
        await SeedAsync(db => db.ExpenseAttachments.Single(a => a.Id == withDocument).CreatedAtMs -= 60_000);

        foreach (var user in new[] { "patron", "muhasebe" })
        {
            var response = await SendAsync(HttpMethod.Get, "/api/v1/portal/expense-receipts", company[user].Token);
            response.StatusCode.Should().Be(HttpStatusCode.OK, user);
            var items = (await response.ReadAsJsonAsync<PortalExpenseReceiptsResponse>()).Items;
            items.Select(i => i.Id).Should().Equal([pending, withDocument], "newest first");
            var first = items.Single(i => i.Id == withDocument);
            first.Should().Match<PortalExpenseReceiptDto>(i => i.DocumentId == doc && i.Kind == "expense" && i.CreatedByName == "ali"
                && i.Url!.StartsWith("https://r2.test/private/") && i.Url.Contains("X-Amz-Expires=300"));
            first.Document.Should().BeEquivalentTo(new ExpenseDocumentDto
            {
                Type = "expense", Status = "pending", Amount = 125.5m, Description = "Yakıt", Counterparty = "Gider: Yakıt",
                OccurredAt = "01.10.2026 10:15", ExpenseCardCode = "770",
            });
            items.Single(i => i.Id == pending).Document.Should().BeNull("the document has not reached the server");
        }

        var byDocument = await (await SendAsync(HttpMethod.Get, $"/api/v1/portal/expense-receipts?documentId={doc}", company["patron"].Token))
            .ReadAsJsonAsync<PortalExpenseReceiptsResponse>();
        byDocument.Items.Select(i => i.Id).Should().Equal(withDocument);
        var lastYear = DateTime.UtcNow.AddYears(-1).ToString("yyyy-MM-dd");
        (await (await SendAsync(HttpMethod.Get, $"/api/v1/portal/expense-receipts?from={lastYear}&to={lastYear}", company["patron"].Token))
            .ReadAsJsonAsync<PortalExpenseReceiptsResponse>()).Items.Should().BeEmpty();

        await ShouldFailAsync(await SendAsync(HttpMethod.Get, "/api/v1/portal/expense-receipts", company["ali"].Token), HttpStatusCode.Forbidden, "PORTAL_REQUIRES_MANAGER");

        // Opening a receipt asks for a fresh address.
        var fileId = (await FileOfAsync(withDocument)).Id;
        var link = await (await SendAsync(HttpMethod.Get, $"/api/v1/storage/files/{fileId}/link", company["muhasebe"].Token)).ReadAsJsonAsync<StoredFileLinkResponse>();
        link.Url.Should().StartWith("https://r2.test/private/");
        link.ExpiresAtMs.Should().BeGreaterThan(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        (await SendAsync(HttpMethod.Get, $"/api/v1/storage/files/{fileId}/link", company["veli"].Token)).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<StoredFile> FileOfAsync(Guid receiptId) => await ReadAsync(async db =>
    {
        var receipt = await db.ExpenseAttachments.AsNoTracking().SingleAsync(a => a.Id == receiptId);
        return await db.StoredFiles.AsNoTracking().SingleAsync(f => f.Id == receipt.StoredFileId);
    });

    private Task<HttpResponseMessage> UploadAsync(string token, string doc, Guid id, byte[] data, string? kind = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/android/expenses/{Uri.EscapeDataString(doc)}/attachments/{id}" + (kind is null ? "" : "?kind=" + kind))
        {
            Content = new ByteArrayContent(data),
        };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _factory.CreateClient().SendAsync(request);
    }

    private Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }).SendAsync(request);
    }

    private static async Task ShouldFailAsync(HttpResponseMessage response, HttpStatusCode status, string code)
    {
        response.StatusCode.Should().Be(status, await response.Content.ReadAsStringAsync());
        (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be(code);
    }

    private async Task<T> ReadAsync<T>(Func<CentralApiDbContext, Task<T>> read)
    {
        using var scope = _factory.Services.CreateScope();
        return await read(scope.ServiceProvider.GetRequiredService<CentralApiDbContext>());
    }

    private async Task SeedAsync(Action<CentralApiDbContext> change)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        change(db);
        await db.SaveChangesAsync();
    }
}

using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>Admin management for tenant-scoped Mikro company identities.</summary>
public static class AdminErpCompaniesEndpoints
{
    public static IEndpointRouteBuilder MapAdminErpCompaniesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/admin/erp-companies")
            .WithTags("Admin/ERP companies")
            .RequireAuthorization(Program.AdminPolicy)
            .RequireRateLimiting(Program.PerAdminRateLimitPolicy);

        group.MapGet("/", ListAsync).Produces<ErpCompanyDto[]>();
        group.MapPost("/", CreateAsync).Produces<ErpCompanyDto>(StatusCodes.Status201Created).Produces<ApiError>(StatusCodes.Status400BadRequest);
        group.MapPut("/{companyId:guid}/agents/{agentId:guid}", AssignAsync).Produces(StatusCodes.Status204NoContent).Produces<ApiError>(StatusCodes.Status404NotFound);
        return routes;
    }

    private static async Task<IResult> ListAsync([FromQuery] Guid? tenantId, CentralApiDbContext db, CancellationToken ct)
    {
        var query = db.ErpCompanies.AsNoTracking().OrderBy(x => x.Code).AsQueryable();
        if (tenantId is { } id && id != Guid.Empty) query = query.Where(x => x.TenantId == id);
        return JsonResults.Ok(await query.Select(x => ToDto(x)).ToArrayAsync(ct));
    }

    private static async Task<IResult> CreateAsync([FromBody] CreateErpCompanyRequest body, CentralApiDbContext db, CancellationToken ct)
    {
        if (body is null || body.TenantId == Guid.Empty || string.IsNullOrWhiteSpace(body.Code)
            || string.IsNullOrWhiteSpace(body.Name) || string.IsNullOrWhiteSpace(body.SourceDatabase)
            || body.CompanyNo <= 0 || body.BranchNo < 0 || body.WarehouseNo < 0)
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_ERP_COMPANY", Message = "tenantId, code, name, sourceDatabase and non-negative branch/warehouse values are required." });

        if (!await db.Tenants.AsNoTracking().AnyAsync(x => x.Id == body.TenantId, ct))
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "TENANT_NOT_FOUND", Message = "Tenant not found." });

        var company = new ErpCompany
        {
            TenantId = body.TenantId, Code = body.Code.Trim(), Name = body.Name.Trim(),
            SourceDatabase = body.SourceDatabase.Trim(), CompanyNo = body.CompanyNo,
            BranchNo = body.BranchNo, WarehouseNo = body.WarehouseNo,
        };
        db.ErpCompanies.Add(company);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException) { return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "ERP_COMPANY_EXISTS", Message = "Company code already exists for this tenant." }); }
        return JsonResults.Status(StatusCodes.Status201Created, ToDto(company));
    }

    private static async Task<IResult> AssignAsync(Guid companyId, Guid agentId, CentralApiDbContext db, CancellationToken ct)
    {
        var company = await db.ErpCompanies.AsNoTracking().FirstOrDefaultAsync(x => x.Id == companyId, ct);
        var agent = await db.Agents.AsNoTracking().FirstOrDefaultAsync(x => x.Id == agentId, ct);
        if (company is null || agent is null || company.TenantId != agent.TenantId)
            return JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = "AGENT_OR_COMPANY_NOT_FOUND", Message = "Agent and company must exist in the same tenant." });
        if (!await db.AgentCompanyAssignments.AnyAsync(x => x.AgentId == agentId && x.ErpCompanyId == companyId, ct))
        {
            db.AgentCompanyAssignments.Add(new AgentCompanyAssignment { AgentId = agentId, ErpCompanyId = companyId });
            await db.SaveChangesAsync(ct);
        }
        return Results.NoContent();
    }

    private static ErpCompanyDto ToDto(ErpCompany x) => new()
    {
        Id = x.Id, TenantId = x.TenantId, Code = x.Code, Name = x.Name, SourceDatabase = x.SourceDatabase,
        CompanyNo = x.CompanyNo, BranchNo = x.BranchNo, WarehouseNo = x.WarehouseNo, IsActive = x.IsActive,
    };
}

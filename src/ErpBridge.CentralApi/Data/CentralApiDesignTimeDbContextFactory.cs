using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ErpBridge.CentralApi.Data;

/// <summary>Design-time only context factory; migrations never need production configuration.</summary>
public sealed class CentralApiDesignTimeDbContextFactory : IDesignTimeDbContextFactory<CentralApiDbContext>
{
    public CentralApiDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CentralApiDbContext>()
            .UseNpgsql("Host=localhost;Database=erpbridge_design;Username=postgres;Password=design_only")
            .Options;
        return new CentralApiDbContext(options);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EcommerceAPI.Data;

/// <summary>
/// Design-time factory used by EF Core tools (dotnet ef migrations) to generate PostgreSQL-compatible migrations.
/// At runtime, Program.cs configures the provider based on DATABASE_URL.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Use a dummy PostgreSQL connection string for migration generation.
        // This ensures migrations are generated with Npgsql-compatible SQL.
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrEmpty(databaseUrl))
        {
            var uri = new Uri(databaseUrl);
            var userInfo = uri.UserInfo.Split(':');
            var npgsqlConn = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
            optionsBuilder.UseNpgsql(npgsqlConn);
        }
        else
        {
            // Dummy connection string just for migration code generation (never connects)
            optionsBuilder.UseNpgsql("Host=localhost;Database=ecommerce_design;Username=postgres;Password=postgres");
        }

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}

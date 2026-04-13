using System;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using SecurityReport.Infrastructure.Persistence;

namespace Tests.Integration
{
    public class SqlTestcontainerFixture : IAsyncDisposable
    {
        public MsSqlTestcontainer Container { get; }
        public string ConnectionString => Container.ConnectionString;

        public SqlTestcontainerFixture()
        {
            var builder = new TestcontainersBuilder<MsSqlTestcontainer>()
                .WithDatabase(new MsSqlTestcontainerConfiguration
                {
                    Password = "Your_strong!Passw0rd",
                })
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .WithCleanUp(true)
                .WithName("sr-test-sql");

            Container = builder.Build();
        }

        public async ValueTask StartAsync()
        {
            await Container.StartAsync();
            // Wait for SQL Server to be ready
            await Task.Delay(TimeSpan.FromSeconds(10));

            // Apply EF Core migrations programmatically using the Infrastructure DbContext
            var optionsBuilder = new DbContextOptionsBuilder<SecurityReportDbContext>();
            optionsBuilder.UseSqlServer(Container.ConnectionString, sql => sql.MigrationsAssembly("SecurityReport.Infrastructure"));

            using var context = new SecurityReportDbContext(optionsBuilder.Options);
            await context.Database.MigrateAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await Container.DisposeAsync();
        }
    }
}

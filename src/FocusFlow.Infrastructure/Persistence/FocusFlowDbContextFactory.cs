using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FocusFlow.Infrastructure.Persistence;

public sealed class FocusFlowDbContextFactory
    : IDesignTimeDbContextFactory<FocusFlowDbContext>
{
    public FocusFlowDbContext CreateDbContext(
        string[] args)
    {
        string databasePath =
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "focusflow.design.db");

        var optionsBuilder =
            new DbContextOptionsBuilder<
                FocusFlowDbContext>();

        optionsBuilder.UseSqlite(
            $"Data Source={databasePath};Foreign Keys=True");

        return new FocusFlowDbContext(
            optionsBuilder.Options);
    }
}
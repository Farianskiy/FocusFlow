using FocusFlow.Application.Abstractions.Persistence;
using FocusFlow.Domain.Presets;
using FocusFlow.Domain.Projects;
using FocusFlow.Domain.Sessions;
using FocusFlow.Domain.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FocusFlow.Infrastructure.Persistence;

public sealed class FocusFlowDbContext
    : DbContext,
      IUnitOfWork
{
    public FocusFlowDbContext(
        DbContextOptions<FocusFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<FocusTask> FocusTasks =>
        Set<FocusTask>();

    public DbSet<PomodoroPreset> PomodoroPresets =>
        Set<PomodoroPreset>();

    public DbSet<FocusSession> FocusSessions =>
        Set<FocusSession>();

    public DbSet<FocusProject> FocusProjects =>
        Set<FocusProject>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(FocusFlowDbContext).Assembly);
    }

    async Task IUnitOfWork.SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        await base.SaveChangesAsync(
            cancellationToken);
    }
}
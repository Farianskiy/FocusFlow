using FocusFlow.Domain.Projects;
using FocusFlow.Domain.Tasks;
using FocusFlow.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FocusFlow.Infrastructure.Persistence.Configurations;

internal sealed class FocusTaskConfiguration
    : IEntityTypeConfiguration<FocusTask>
{
    public void Configure(
        EntityTypeBuilder<FocusTask> builder)
    {
        builder.ToTable("focus_tasks");

        builder.HasKey(task => task.Id);

        builder
            .Property(task => task.Id)
            .ValueGeneratedNever();

        builder
            .Property(task => task.Title)
            .HasColumnName("title")
            .HasMaxLength(FocusTaskTitle.MaximumLength)
            .HasConversion(
                value => value.Value,
                value =>
                    DomainValueObjectFactory
                        .CreateTaskTitle(value))
            .IsRequired();

        builder
            .Property(task => task.Description)
            .HasColumnName("description")
            .HasMaxLength(
                FocusTaskDescription.MaximumLength)
            .HasConversion(
                value => value.Value,
                value =>
                    DomainValueObjectFactory
                        .CreateTaskDescription(value))
            .IsRequired();

        builder
            .Property(task => task.Estimate)
            .HasColumnName("estimated_pomodoros")
            .HasConversion(
                value => value.Value,
                value =>
                    DomainValueObjectFactory
                        .CreatePomodoroEstimate(value))
            .IsRequired();

        builder
            .Property(task => task.ProjectId)
            .HasColumnName("project_id");

        builder
            .Property(task => task.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder
            .Property(task => task.CreatedAtUtc)
            .HasColumnName("created_at_utc_ticks")
            .HasConversion(
                new DateTimeOffsetToUtcTicksConverter())
            .IsRequired();

        builder
            .Property(task => task.UpdatedAtUtc)
            .HasColumnName("updated_at_utc_ticks")
            .HasConversion(
                new DateTimeOffsetToUtcTicksConverter())
            .IsRequired();

        builder
            .Property(task => task.StartedAtUtc)
            .HasColumnName("started_at_utc_ticks")
            .HasConversion(
                new NullableDateTimeOffsetToUtcTicksConverter());

        builder
            .Property(task => task.CompletedAtUtc)
            .HasColumnName("completed_at_utc_ticks")
            .HasConversion(
                new NullableDateTimeOffsetToUtcTicksConverter());

        builder
            .Property(task => task.CancelledAtUtc)
            .HasColumnName("cancelled_at_utc_ticks")
            .HasConversion(
                new NullableDateTimeOffsetToUtcTicksConverter());

        PropertyBuilder<HashSet<Guid>> completedSessions =
            builder
                .Property<HashSet<Guid>>(
                    "_completedFocusSessionIds")
                .HasColumnName(
                    "completed_focus_session_ids_json")
                .HasConversion(
                    new GuidHashSetJsonConverter())
                .IsRequired();

        completedSessions.Metadata.SetValueComparer(
            new GuidHashSetValueComparer());

        completedSessions.UsePropertyAccessMode(
            PropertyAccessMode.Field);

        builder.Ignore(
            task => task.CompletedFocusSessionIds);

        builder.Ignore(
            task => task.CompletedPomodoroCount);

        builder.Ignore(
            task => task.RemainingEstimatedPomodoros);

        builder.Ignore(
            task => task.IsEstimateReached);

        builder.Ignore(
            task => task.ProgressPercentage);

        builder.Ignore(
            task => task.IsFinished);

        builder
            .HasOne<FocusProject>()
            .WithMany()
            .HasForeignKey(task => task.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(
            task => task.ProjectId);

        builder.HasIndex(
            task => task.Status);
    }
}
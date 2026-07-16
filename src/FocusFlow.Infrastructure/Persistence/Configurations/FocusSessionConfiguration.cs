using FocusFlow.Domain.Presets;
using FocusFlow.Domain.Sessions;
using FocusFlow.Domain.Tasks;
using FocusFlow.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FocusFlow.Infrastructure.Persistence.Configurations;

internal sealed class FocusSessionConfiguration
    : IEntityTypeConfiguration<FocusSession>
{
    public void Configure(
        EntityTypeBuilder<FocusSession> builder)
    {
        builder.ToTable("focus_sessions");

        builder.HasKey(session => session.Id);

        builder
            .Property(session => session.Id)
            .ValueGeneratedNever();

        builder
            .Property(session => session.TaskId)
            .HasColumnName("task_id");

        builder
            .Property(session => session.PresetId)
            .HasColumnName("preset_id");

        builder
            .Property(session => session.Type)
            .HasColumnName("type")
            .HasConversion<int>()
            .IsRequired();

        builder
            .Property(session => session.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder
            .Property(
                session => session.CompletionReason)
            .HasColumnName("completion_reason")
            .HasConversion<int>()
            .IsRequired();

        builder
            .Property(
                session => session.PlannedDuration)
            .HasColumnName(
                "planned_duration_ticks")
            .HasConversion(
                value => value.Value.Ticks,
                value =>
                    DomainValueObjectFactory
                        .CreateSessionDuration(value))
            .IsRequired();

        builder
            .Property(
                session => session.RemainingDuration)
            .HasColumnName(
                "remaining_duration_ticks")
            .HasConversion(
                new TimeSpanToTicksConverter())
            .IsRequired();

        builder
            .Property(session => session.ActualDuration)
            .HasColumnName("actual_duration_ticks")
            .HasConversion(
                new NullableTimeSpanToTicksConverter());

        builder
            .Property(session => session.StartedAtUtc)
            .HasColumnName("started_at_utc_ticks")
            .HasConversion(
                new DateTimeOffsetToUtcTicksConverter())
            .IsRequired();

        builder
            .Property(
                session => session.ExpectedEndAtUtc)
            .HasColumnName(
                "expected_end_at_utc_ticks")
            .HasConversion(
                new NullableDateTimeOffsetToUtcTicksConverter());

        builder
            .Property(session => session.PausedAtUtc)
            .HasColumnName("paused_at_utc_ticks")
            .HasConversion(
                new NullableDateTimeOffsetToUtcTicksConverter());

        builder
            .Property(session => session.CompletedAtUtc)
            .HasColumnName("completed_at_utc_ticks")
            .HasConversion(
                new NullableDateTimeOffsetToUtcTicksConverter());

        builder
            .Property(session => session.CancelledAtUtc)
            .HasColumnName("cancelled_at_utc_ticks")
            .HasConversion(
                new NullableDateTimeOffsetToUtcTicksConverter());

        builder
            .Property(session => session.CreatedAtUtc)
            .HasColumnName("created_at_utc_ticks")
            .HasConversion(
                new DateTimeOffsetToUtcTicksConverter())
            .IsRequired();

        builder
            .Property(session => session.UpdatedAtUtc)
            .HasColumnName("updated_at_utc_ticks")
            .HasConversion(
                new DateTimeOffsetToUtcTicksConverter())
            .IsRequired();

        builder.Ignore(
            session => session.IsActive);

        builder
            .HasOne<FocusTask>()
            .WithMany()
            .HasForeignKey(session => session.TaskId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<PomodoroPreset>()
            .WithMany()
            .HasForeignKey(session => session.PresetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(
            session => session.Status);

        builder.HasIndex(
            session => session.TaskId);

        builder.HasIndex(
            session => session.PresetId);
    }
}
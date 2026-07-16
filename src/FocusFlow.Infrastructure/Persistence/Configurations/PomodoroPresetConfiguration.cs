using FocusFlow.Domain.Presets;
using FocusFlow.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FocusFlow.Infrastructure.Persistence.Configurations;

internal sealed class PomodoroPresetConfiguration
    : IEntityTypeConfiguration<PomodoroPreset>
{
    public void Configure(
        EntityTypeBuilder<PomodoroPreset> builder)
    {
        builder.ToTable("pomodoro_presets");

        builder.HasKey(preset => preset.Id);

        builder
            .Property(preset => preset.Id)
            .ValueGeneratedNever();

        builder
            .Property(preset => preset.Name)
            .HasColumnName("name")
            .HasMaxLength(
                PomodoroPresetName.MaximumLength)
            .HasConversion(
                value => value.Value,
                value =>
                    DomainValueObjectFactory
                        .CreatePresetName(value))
            .IsRequired();

        builder
            .Property(preset => preset.CycleSettings)
            .HasColumnName("cycle_settings_json")
            .HasConversion(
                new PomodoroCycleSettingsJsonConverter())
            .IsRequired();

        builder
            .Property(preset => preset.Kind)
            .HasColumnName("kind")
            .HasConversion<int>()
            .IsRequired();

        builder
            .Property(preset => preset.AutoStartBreaks)
            .HasColumnName("auto_start_breaks")
            .IsRequired();

        builder
            .Property(
                preset =>
                    preset.AutoStartFocusSessions)
            .HasColumnName(
                "auto_start_focus_sessions")
            .IsRequired();

        builder
            .Property(preset => preset.CreatedAtUtc)
            .HasColumnName("created_at_utc_ticks")
            .HasConversion(
                new DateTimeOffsetToUtcTicksConverter())
            .IsRequired();

        builder
            .Property(preset => preset.UpdatedAtUtc)
            .HasColumnName("updated_at_utc_ticks")
            .HasConversion(
                new DateTimeOffsetToUtcTicksConverter())
            .IsRequired();

        builder.Ignore(
            preset => preset.IsSystem);

        builder.HasIndex(
            preset => preset.Kind);
    }
}
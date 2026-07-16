using FocusFlow.Domain.Projects;
using FocusFlow.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FocusFlow.Infrastructure.Persistence.Configurations;

internal sealed class FocusProjectConfiguration
    : IEntityTypeConfiguration<FocusProject>
{
    public void Configure(
        EntityTypeBuilder<FocusProject> builder)
    {
        builder.ToTable("focus_projects");

        builder.HasKey(project => project.Id);

        builder
            .Property(project => project.Id)
            .ValueGeneratedNever();

        builder
            .Property(project => project.Name)
            .HasColumnName("name")
            .HasMaxLength(
                FocusProjectName.MaximumLength)
            .HasConversion(
                value => value.Value,
                value =>
                    DomainValueObjectFactory
                        .CreateProjectName(value))
            .IsRequired();

        builder
            .Property(project => project.Description)
            .HasColumnName("description")
            .HasMaxLength(
                FocusProjectDescription.MaximumLength)
            .HasConversion(
                value => value.Value,
                value =>
                    DomainValueObjectFactory
                        .CreateProjectDescription(value))
            .IsRequired();

        builder
            .Property(project => project.Color)
            .HasColumnName("color")
            .HasMaxLength(7)
            .HasConversion(
                value => value.Value,
                value =>
                    DomainValueObjectFactory
                        .CreateProjectColor(value))
            .IsRequired();

        builder
            .Property(project => project.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder
            .Property(project => project.CreatedAtUtc)
            .HasColumnName("created_at_utc_ticks")
            .HasConversion(
                new DateTimeOffsetToUtcTicksConverter())
            .IsRequired();

        builder
            .Property(project => project.UpdatedAtUtc)
            .HasColumnName("updated_at_utc_ticks")
            .HasConversion(
                new DateTimeOffsetToUtcTicksConverter())
            .IsRequired();

        builder
            .Property(project => project.CompletedAtUtc)
            .HasColumnName("completed_at_utc_ticks")
            .HasConversion(
                new NullableDateTimeOffsetToUtcTicksConverter());

        builder
            .Property(project => project.ArchivedAtUtc)
            .HasColumnName("archived_at_utc_ticks")
            .HasConversion(
                new NullableDateTimeOffsetToUtcTicksConverter());

        builder.Ignore(
            project => project.CanAcceptTasks);

        builder.Ignore(
            project => project.IsCompleted);

        builder.Ignore(
            project => project.IsArchived);

        builder.HasIndex(
            project => project.Status);
    }
}
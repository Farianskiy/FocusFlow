using FluentAssertions;
using FocusFlow.Domain.Projects;

namespace FocusFlow.Domain.Tests.Projects;

public sealed class FocusProjectTests
{
    private static readonly DateTimeOffset CreatedAtUtc =
        new(
            2026,
            7,
            16,
            10,
            0,
            0,
            TimeSpan.Zero);

    [Fact]
    public void Create_WithValidData_ShouldCreateActiveProject()
    {
        // Act
        var result = FocusProject.Create(
            name: "Разработка FocusFlow",
            description: "Мобильное приложение Pomodoro.",
            color: "#6750A4",
            createdAt: CreatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        FocusProject project = result.Value;

        project.Id.Should().NotBe(Guid.Empty);

        project.Name.Value.Should().Be(
            "Разработка FocusFlow");

        project.Description.Value.Should().Be(
            "Мобильное приложение Pomodoro.");

        project.Color.Value.Should().Be(
            "#6750A4");

        project.Status.Should().Be(
            FocusProjectStatus.Active);

        project.CanAcceptTasks.Should().BeTrue();
        project.IsCompleted.Should().BeFalse();
        project.IsArchived.Should().BeFalse();

        project.CreatedAtUtc.Should().Be(
            CreatedAtUtc);

        project.UpdatedAtUtc.Should().Be(
            CreatedAtUtc);
    }

    [Fact]
    public void Create_ShouldNormalizeNameAndColor()
    {
        // Act
        var result = FocusProject.Create(
            name: "   Разработка    FocusFlow   ",
            description: null,
            color: "00ff88",
            createdAt: CreatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.Name.Value.Should().Be(
            "Разработка FocusFlow");

        result.Value.Color.Value.Should().Be(
            "#00FF88");

        result.Value.Description.IsEmpty
            .Should()
            .BeTrue();
    }

    [Fact]
    public void Create_WithoutColor_ShouldUseDefaultColor()
    {
        // Act
        var result = FocusProject.Create(
            name: "Проект",
            description: null,
            color: null,
            createdAt: CreatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.Color.Value.Should().Be(
            FocusProjectColor.DefaultValue);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldReturnFailure()
    {
        // Act
        var result = FocusProject.Create(
            name: "   ",
            description: null,
            color: null,
            createdAt: CreatedAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            FocusProjectErrors.NameIsRequired);
    }

    [Theory]
    [InlineData("#12345")]
    [InlineData("#1234567")]
    [InlineData("#GG0000")]
    [InlineData("red")]
    public void Create_WithInvalidColor_ShouldReturnFailure(
        string color)
    {
        // Act
        var result = FocusProject.Create(
            name: "Проект",
            description: null,
            color: color,
            createdAt: CreatedAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            FocusProjectErrors.ColorIsInvalid);
    }

    [Fact]
    public void Rename_ActiveProject_ShouldChangeName()
    {
        // Arrange
        FocusProject project = CreateProject();

        DateTimeOffset renamedAtUtc =
            CreatedAtUtc.AddMinutes(1);

        // Act
        var result = project.Rename(
            "Новое название",
            renamedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        project.Name.Value.Should().Be(
            "Новое название");

        project.UpdatedAtUtc.Should().Be(
            renamedAtUtc);
    }

    [Fact]
    public void ChangeDescription_ShouldUpdateDescription()
    {
        // Arrange
        FocusProject project = CreateProject();

        // Act
        var result = project.ChangeDescription(
            "Новое описание проекта.",
            CreatedAtUtc.AddMinutes(1));

        // Assert
        result.IsSuccess.Should().BeTrue();

        project.Description.Value.Should().Be(
            "Новое описание проекта.");
    }

    [Fact]
    public void ChangeColor_ShouldNormalizeAndUpdateColor()
    {
        // Arrange
        FocusProject project = CreateProject();

        // Act
        var result = project.ChangeColor(
            "ff5500",
            CreatedAtUtc.AddMinutes(1));

        // Assert
        result.IsSuccess.Should().BeTrue();

        project.Color.Value.Should().Be(
            "#FF5500");
    }

    [Fact]
    public void Complete_ActiveProject_ShouldCompleteProject()
    {
        // Arrange
        FocusProject project = CreateProject();

        DateTimeOffset completedAtUtc =
            CreatedAtUtc.AddHours(1);

        // Act
        var result = project.Complete(
            completedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        project.Status.Should().Be(
            FocusProjectStatus.Completed);

        project.CompletedAtUtc.Should().Be(
            completedAtUtc);

        project.CanAcceptTasks.Should().BeFalse();
        project.IsCompleted.Should().BeTrue();
        project.IsArchived.Should().BeFalse();
    }

    [Fact]
    public void Complete_CompletedProject_ShouldReturnFailure()
    {
        // Arrange
        FocusProject project = CreateProject();

        project.Complete(
            CreatedAtUtc.AddMinutes(1));

        // Act
        var result = project.Complete(
            CreatedAtUtc.AddMinutes(2));

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Code.Should().Be(
            "FocusProject.Complete.InvalidStatus");
    }

    [Fact]
    public void Reopen_CompletedProject_ShouldActivateProject()
    {
        // Arrange
        FocusProject project = CreateProject();

        project.Complete(
            CreatedAtUtc.AddMinutes(1));

        DateTimeOffset reopenedAtUtc =
            CreatedAtUtc.AddMinutes(2);

        // Act
        var result = project.Reopen(
            reopenedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        project.Status.Should().Be(
            FocusProjectStatus.Active);

        project.CompletedAtUtc.Should().BeNull();
        project.CanAcceptTasks.Should().BeTrue();

        project.UpdatedAtUtc.Should().Be(
            reopenedAtUtc);
    }

    [Fact]
    public void Archive_ActiveProject_ShouldReturnFailure()
    {
        // Arrange
        FocusProject project = CreateProject();

        // Act
        var result = project.Archive(
            CreatedAtUtc.AddMinutes(1));

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Code.Should().Be(
            "FocusProject.Archive.InvalidStatus");
    }

    [Fact]
    public void Archive_CompletedProject_ShouldArchiveProject()
    {
        // Arrange
        FocusProject project = CreateProject();

        project.Complete(
            CreatedAtUtc.AddMinutes(1));

        DateTimeOffset archivedAtUtc =
            CreatedAtUtc.AddMinutes(2);

        // Act
        var result = project.Archive(
            archivedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        project.Status.Should().Be(
            FocusProjectStatus.Archived);

        project.ArchivedAtUtc.Should().Be(
            archivedAtUtc);

        project.IsCompleted.Should().BeTrue();
        project.IsArchived.Should().BeTrue();
    }

    [Fact]
    public void Rename_ArchivedProject_ShouldReturnFailure()
    {
        // Arrange
        FocusProject project =
            CreateArchivedProject();

        // Act
        var result = project.Rename(
            "Новое название",
            CreatedAtUtc.AddMinutes(3));

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            FocusProjectErrors
                .ArchivedProjectCannotBeModified);
    }

    [Fact]
    public void Restore_ArchivedProject_ShouldReturnToCompleted()
    {
        // Arrange
        FocusProject project =
            CreateArchivedProject();

        DateTimeOffset restoredAtUtc =
            CreatedAtUtc.AddMinutes(3);

        // Act
        var result = project.Restore(
            restoredAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        project.Status.Should().Be(
            FocusProjectStatus.Completed);

        project.ArchivedAtUtc.Should().BeNull();

        project.CompletedAtUtc
            .Should()
            .NotBeNull();

        project.IsArchived.Should().BeFalse();

        project.UpdatedAtUtc.Should().Be(
            restoredAtUtc);
    }

    [Fact]
    public void Rename_WithEarlierTime_ShouldReturnFailure()
    {
        // Arrange
        FocusProject project = CreateProject();

        // Act
        var result = project.Rename(
            "Новое название",
            CreatedAtUtc.AddMinutes(-1));

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Code.Should().Be(
            "FocusProject.ModificationTime.Invalid");

        project.Name.Value.Should().Be(
            "Разработка FocusFlow");
    }

    private static FocusProject CreateProject()
    {
        var result = FocusProject.Create(
            name: "Разработка FocusFlow",
            description: "Мобильное приложение Pomodoro.",
            color: "#6750A4",
            createdAt: CreatedAtUtc);

        result.IsSuccess.Should().BeTrue();

        return result.Value;
    }

    private static FocusProject CreateArchivedProject()
    {
        FocusProject project = CreateProject();

        project.Complete(
            CreatedAtUtc.AddMinutes(1));

        project.Archive(
            CreatedAtUtc.AddMinutes(2));

        return project;
    }
}
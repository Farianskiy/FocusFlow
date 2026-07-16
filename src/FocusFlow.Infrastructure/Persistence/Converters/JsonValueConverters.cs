using System.Text.Json;
using FocusFlow.Domain.Presets;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FocusFlow.Infrastructure.Persistence.Converters;

internal sealed record PomodoroCycleSettingsStorageModel(
    long FocusDurationTicks,
    long ShortBreakDurationTicks,
    long LongBreakDurationTicks,
    int SessionsBeforeLongBreak);

internal sealed class PomodoroCycleSettingsJsonConverter
    : ValueConverter<PomodoroCycleSettings, string>
{
    public PomodoroCycleSettingsJsonConverter()
        : base(
            value => Serialize(value),
            value => Deserialize(value))
    {
    }

    private static string Serialize(
        PomodoroCycleSettings settings)
    {
        var storageModel =
            new PomodoroCycleSettingsStorageModel(
                settings.FocusDuration.Ticks,
                settings.ShortBreakDuration.Ticks,
                settings.LongBreakDuration.Ticks,
                settings.SessionsBeforeLongBreak);

        return JsonSerializer.Serialize(storageModel);
    }

    private static PomodoroCycleSettings Deserialize(
        string json)
    {
        PomodoroCycleSettingsStorageModel storageModel =
            JsonSerializer.Deserialize<
                PomodoroCycleSettingsStorageModel>(json)
            ?? throw new InvalidOperationException(
                "Не удалось прочитать настройки Pomodoro из базы данных.");

        return DomainValueObjectFactory.CreateCycleSettings(
            TimeSpan.FromTicks(
                storageModel.FocusDurationTicks),
            TimeSpan.FromTicks(
                storageModel.ShortBreakDurationTicks),
            TimeSpan.FromTicks(
                storageModel.LongBreakDurationTicks),
            storageModel.SessionsBeforeLongBreak);
    }
}

internal sealed class GuidHashSetJsonConverter
    : ValueConverter<HashSet<Guid>, string>
{
    public GuidHashSetJsonConverter()
        : base(
            value => Serialize(value),
            value => Deserialize(value))
    {
    }

    private static string Serialize(
        HashSet<Guid> values)
    {
        Guid[] orderedValues =
            values
                .OrderBy(value => value)
                .ToArray();

        return JsonSerializer.Serialize(orderedValues);
    }

    private static HashSet<Guid> Deserialize(
        string json)
    {
        Guid[] values =
            JsonSerializer.Deserialize<Guid[]>(json)
            ?? [];

        return new HashSet<Guid>(values);
    }
}

internal sealed class GuidHashSetValueComparer
    : ValueComparer<HashSet<Guid>>
{
    public GuidHashSetValueComparer()
        : base(
            (left, right) => AreEqual(left, right),
            value => CalculateHashCode(value),
            value => CreateSnapshot(value))
    {
    }

    private static bool AreEqual(
        HashSet<Guid>? left,
        HashSet<Guid>? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left == null || right == null)
            return false;

        return left.SetEquals(right);
    }

    private static int CalculateHashCode(
        HashSet<Guid> values)
    {
        int hashCode = 0;

        foreach (Guid value in values)
        {
            hashCode ^= value.GetHashCode();
        }

        return hashCode;
    }

    private static HashSet<Guid> CreateSnapshot(
        HashSet<Guid> values)
    {
        return new HashSet<Guid>(values);
    }
}
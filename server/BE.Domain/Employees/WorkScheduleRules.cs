namespace BE.Domain.Employees;

/// <summary>One weekday in a schedule as it is entered: both times set = working day, both null = day off.</summary>
public record ScheduleDay(DayOfWeek Day, TimeOnly? Start, TimeOnly? End)
{
    public bool IsDayOff => Start is null && End is null;
}

/// <summary>
/// Rules for entering work schedules (pure functions, unit tested).
/// A schedule always lists Monday–Friday; hours per day follow from the From–To times.
/// </summary>
public static class WorkScheduleRules
{
    public static readonly DayOfWeek[] WorkWeek =
        [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday];

    /// <summary>Returns the problems with a week, or an empty list when it is valid.</summary>
    public static IReadOnlyList<string> ValidateWeek(IReadOnlyList<ScheduleDay> days)
    {
        var errors = new List<string>();

        var duplicates = days.GroupBy(d => d.Day).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        foreach (var day in duplicates)
            errors.Add($"{day} is listed more than once.");

        foreach (var day in days.Where(d => !WorkWeek.Contains(d.Day)))
            errors.Add($"{day.Day} is not a working day – only Monday–Friday can be scheduled.");

        foreach (var day in WorkWeek.Where(wd => days.All(d => d.Day != wd)))
            errors.Add($"{day} is missing – give times or mark it as a day off.");

        foreach (var day in days)
        {
            if ((day.Start is null) != (day.End is null))
                errors.Add($"{day.Day}: give both From and To, or neither for a day off.");
            else if (day.Start is { } start && day.End is { } end && end <= start)
                errors.Add($"{day.Day}: To must be later than From.");
        }

        if (errors.Count == 0 && days.All(d => d.IsDayOff))
            errors.Add("A schedule needs at least one working day.");

        return errors;
    }

    /// <summary>
    /// A new schedule period may not rewrite history: it starts after the current period started,
    /// and within the employment.
    /// </summary>
    public static string? ValidateNewPeriodStart(DateOnly validFrom, DateOnly employmentStart, DateOnly? employmentEnd,
                                                 DateOnly? currentPeriodStart)
    {
        if (validFrom < employmentStart)
            return "A schedule cannot start before the employment starts.";
        if (employmentEnd is { } end && validFrom > end)
            return "A schedule cannot start after the employment has ended.";
        if (currentPeriodStart is { } current && validFrom <= current)
            return $"The new schedule must start after the current one (which started {current:yyyy-MM-dd}). History is never changed.";
        return null;
    }

    /// <summary>Converts the entered week to the domain WorkSchedule used by accrual (days off are left out).</summary>
    public static WorkSchedule ToWorkSchedule(DateOnly validFrom, DateOnly? validTo, IEnumerable<ScheduleDay> days) =>
        new(validFrom, validTo,
            days.Where(d => d is { Start: not null, End: not null })
                .Select(d => new WorkDay(d.Day, d.Start!.Value, d.End!.Value))
                .ToList());
}

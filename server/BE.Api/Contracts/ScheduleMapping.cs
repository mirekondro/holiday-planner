using System.Globalization;
using BE.Domain.Employees;
using BE.Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BE.Api.Contracts;

/// <summary>Converts schedules between the API shape ("HH:mm" strings), the domain and the database.</summary>
public static class ScheduleMapping
{
    private static DayOfWeek ToDayOfWeek(int weekday) => (DayOfWeek)weekday; // 1..5 = Monday..Friday in .NET too
    private static int ToWeekday(DayOfWeek day) => (int)day;

    /// <summary>Parses and validates the entered week. Problems are added to ModelState under <paramref name="key"/>.</summary>
    public static List<ScheduleDay>? ParseWeek(List<ScheduleDayDto> days, ModelStateDictionary modelState, string key)
    {
        var parsed = new List<ScheduleDay>();
        foreach (var day in days)
        {
            if (!TryParseTime(day.Start, out var start) || !TryParseTime(day.End, out var end))
            {
                modelState.AddModelError(key, $"Weekday {day.Weekday}: times must be HH:mm, e.g. 08:00.");
                continue;
            }
            parsed.Add(new ScheduleDay(ToDayOfWeek(day.Weekday), start, end));
        }

        foreach (var error in WorkScheduleRules.ValidateWeek(parsed))
            modelState.AddModelError(key, error);

        return modelState.IsValid ? parsed : null;
    }

    private static bool TryParseTime(string? value, out TimeOnly? time)
    {
        time = null;
        if (value is null) return true;
        if (!TimeOnly.TryParseExact(value, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            return false;
        time = parsed;
        return true;
    }

    public static List<WorkScheduleDayEntity> ToEntities(Guid scheduleId, IEnumerable<ScheduleDay> days) =>
        days.Select(d => new WorkScheduleDayEntity
        {
            ScheduleId = scheduleId,
            Weekday = ToWeekday(d.Day),
            StartTime = d.Start,
            EndTime = d.End,
        }).ToList();

    public static WorkSchedulePeriodDto ToDto(WorkScheduleEntity schedule, IEnumerable<WorkScheduleDayEntity> dayRows)
    {
        var days = dayRows
            .OrderBy(d => d.Weekday)
            .Select(d => new ScheduleDay(ToDayOfWeek(d.Weekday), d.StartTime, d.EndTime))
            .ToList();
        var weeklyHours = WorkScheduleRules.ToWorkSchedule(schedule.ValidFrom, schedule.ValidTo, days).WeeklyHours;

        return new WorkSchedulePeriodDto(
            schedule.Id,
            schedule.ValidFrom,
            schedule.ValidTo,
            Math.Round(weeklyHours, 2),
            days.Select(d => new ScheduleDayDto(ToWeekday(d.Day), d.Start?.ToString("HH:mm", CultureInfo.InvariantCulture), d.End?.ToString("HH:mm", CultureInfo.InvariantCulture))).ToList());
    }
}

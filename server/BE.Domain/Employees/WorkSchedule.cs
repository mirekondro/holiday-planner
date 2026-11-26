namespace BE.Domain.Employees;

// Jeden pracovní den v rozvrhu: od kdy do kdy zaměstnanec pracuje (např. 08:00–15:30).
public record WorkDay(DayOfWeek Day, TimeOnly Start, TimeOnly End)
{
    // Délka dne v hodinách, např. 08:00–15:30 = 7,5 h.
    public decimal Hours => (decimal)(End - Start).TotalMinutes / 60m;
}

// Týdenní rozvrh platný v určitém období. Zaměstnanec jich může mít v čase víc
// (plný úvazek → částečný → zase plný), proto má každý ValidFrom a ValidTo.
public record WorkSchedule(DateOnly ValidFrom, DateOnly? ValidTo, IReadOnlyList<WorkDay> Days)
{
    // Součet hodin za týden, plný úvazek = 37 h.
    public decimal WeeklyHours => Days.Sum(d => d.Hours);

    // Kolik hodin stojí volno v daný den v týdnu (víkend nebo den bez práce = 0).
    public decimal HoursOn(DayOfWeek day) => Days.Where(d => d.Day == day).Sum(d => d.Hours);

    // Platí tenhle rozvrh v daný den? ValidTo = null znamená „platí dodnes".
    public bool AppliesOn(DateOnly date) => date >= ValidFrom && (ValidTo is null || date <= ValidTo);

    // Výchozí rozvrh Business Esbjerg ze zadání: po–čt 7,5 h, pá 7 h = 37 h týdně.
    public static WorkSchedule FullTime(DateOnly validFrom, DateOnly? validTo = null) =>
        new(validFrom, validTo,
        [
            new WorkDay(DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(15, 30)),
            new WorkDay(DayOfWeek.Tuesday, new TimeOnly(8, 0), new TimeOnly(15, 30)),
            new WorkDay(DayOfWeek.Wednesday, new TimeOnly(8, 0), new TimeOnly(15, 30)),
            new WorkDay(DayOfWeek.Thursday, new TimeOnly(8, 0), new TimeOnly(15, 30)),
            new WorkDay(DayOfWeek.Friday, new TimeOnly(8, 0), new TimeOnly(15, 0))
        ]);
}

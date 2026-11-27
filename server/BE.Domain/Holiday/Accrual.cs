using BE.Domain.Employees;

namespace BE.Domain.Holiday;

// Výpočet, kolik hodin dovolené zaměstnanec nastřádal (připisování / accrual).
// Nic se neukládá – vždy se to spočítá znovu z rozvrhů a pravidel.
public static class Accrual
{
    // U neúplného měsíce (nástup nebo odchod v půlce) se počítá po dnech jako 1/30 měsíce.
    // Odpovídá zadání: 6. týden = 3,083 h / 30 ≈ 0,103 h za kalendářní den.
    private const decimal DaysInPartialMonth = 30m;

    // Kolik hodin je připsáno v jedné „obálce" (účet + rok získání) a dá se použít ke dni asOf.
    // Hodiny za měsíc se připíšou až po jeho skončení: za září jsou k dispozici od 1. 10.
    public static decimal Accrued(Employment employment, HolidayAccount account, int earningYear,
                                  DateOnly asOf, AccrualRules rules)
    {
        // Měsíční podíl pro plný úvazek: 185 / 12 ≈ 15,42 h, 37 / 12 ≈ 3,08 h.
        var fullTimeMonthlyHours = rules.HoursPerYear(account) / 12m;

        var earningEnd = HolidayYear.EarningEnd(account, earningYear);
        var monthStart = HolidayYear.EarningStart(account, earningYear);
        var total = 0m;

        while (monthStart <= earningEnd)
        {
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            // Měsíc, který ke dni asOf ještě neskončil, se nepřipisuje.
            if (monthEnd >= asOf)
                break;

            total += fullTimeMonthlyHours * MonthShare(employment, monthStart, monthEnd, rules.FullTimeWeeklyHours);
            monthStart = monthStart.AddMonths(1);
        }

        // Zaokrouhluje se až CELKOVÝ součet (kumulativně), ne každý měsíc zvlášť.
        // Díky tomu dá celý rok přesně 185,00 h a ne 12 × 15,42 = 185,04 h.
        return Math.Round(total, 2, MidpointRounding.AwayFromZero);
    }

    // Jakou část „měsíce plného úvazku" zaměstnanec v daném měsíci odpracoval.
    // Plný úvazek celý měsíc = 1, 20 h/týden celý měsíc = 20/37, nástup 16. 9. = 15/30 …
    private static decimal MonthShare(Employment employment, DateOnly monthStart, DateOnly monthEnd,
                                      decimal fullTimeWeeklyHours)
    {
        var daysInMonth = monthEnd.Day;
        var employedDays = 0;
        var weightSum = 0m;

        // Každý den v měsíci přispěje podle rozvrhu, který ten den platil.
        // Tím se správně spočítá i změna úvazku v půlce měsíce.
        for (var day = monthStart; day <= monthEnd; day = day.AddDays(1))
        {
            if (!employment.IsEmployedOn(day))
                continue;

            employedDays++;
            var schedule = employment.ScheduleOn(day);
            if (schedule is not null)
                weightSum += schedule.WeeklyHours / fullTimeWeeklyHours;
        }

        if (employedDays == 0)
            return 0m;

        // Celý měsíc zaměstnaný → dělí se skutečným počtem dní (výsledek je přesně 1 při stejném rozvrhu).
        // Jen část měsíce → dělí se 30 (pravidlo pro kalendářní dny ze zadání).
        var divisor = employedDays == daysInMonth ? daysInMonth : DaysInPartialMonth;
        return weightSum / divisor;
    }
}

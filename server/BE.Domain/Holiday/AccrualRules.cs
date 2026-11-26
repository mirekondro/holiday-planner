namespace BE.Domain.Holiday;

// Čísla, ze kterých se počítá připisování. Později je nahradí parametry s datem platnosti (issue #21).
// Počítáme z ročních hodin, ne z „2,08 dne": 2,08 × 12 = 24,96 dne, ale zadání říká 25 dní = 185 h.
public record AccrualRules(decimal FullTimeWeeklyHours, decimal Weeks1to5HoursPerYear, decimal Week6HoursPerYear)
{
    // Business Esbjerg: plný úvazek 37 h/týden, týdny 1–5 = 5 × 37 = 185 h/rok, 6. týden = 37 h/rok.
    public static AccrualRules BusinessEsbjerg { get; } = new(37m, 185m, 37m);

    public decimal HoursPerYear(HolidayAccount account) => account switch
    {
        HolidayAccount.Weeks1to5 => Weeks1to5HoursPerYear,
        HolidayAccount.Week6 => Week6HoursPerYear,
        _ => throw new ArgumentOutOfRangeException(nameof(account))
    };
}

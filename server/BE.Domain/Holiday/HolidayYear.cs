namespace BE.Domain.Holiday;

public static class HolidayYear
{
    // Do kterého roku získání patří dané datum?
    public static int EarningYearFor(HolidayAccount account, DateOnly date) => account switch
    {
        HolidayAccount.Weeks1to5 => date.Month >= 9 ? date.Year : date.Year - 1,
        HolidayAccount.Week6 => date.Year,
        _ => throw new ArgumentOutOfRangeException(nameof(account))
    };

    // První den, kdy se v daném roce hodiny získávají.
    public static DateOnly EarningStart(HolidayAccount account, int earningYear) => account switch
    {
        HolidayAccount.Weeks1to5 => new DateOnly(earningYear, 9, 1),
        HolidayAccount.Week6 => new DateOnly(earningYear, 1, 1),
        _ => throw new ArgumentOutOfRangeException(nameof(account))
    };

    // Poslední den, kdy se v daném roce hodiny získávají.
    public static DateOnly EarningEnd(HolidayAccount account, int earningYear) => account switch
    {
        HolidayAccount.Weeks1to5 => new DateOnly(earningYear + 1, 8, 31),
        HolidayAccount.Week6 => new DateOnly(earningYear, 12, 31),
        _ => throw new ArgumentOutOfRangeException(nameof(account))
    };

    // První den, kdy se hodiny z daného roku dají čerpat.
    public static DateOnly UsageStart(HolidayAccount account, int earningYear) => account switch
    {
        HolidayAccount.Weeks1to5 => new DateOnly(earningYear, 9, 1),
        HolidayAccount.Week6 => new DateOnly(earningYear + 1, 5, 1),
        _ => throw new ArgumentOutOfRangeException(nameof(account))
    };

    // Poslední den, kdy se hodiny z daného roku dají čerpat (datum expirace, které vidí zaměstnanec).
    public static DateOnly UsageEnd(HolidayAccount account, int earningYear) => account switch
    {
        HolidayAccount.Weeks1to5 => new DateOnly(earningYear + 1, 12, 31),
        HolidayAccount.Week6 => new DateOnly(earningYear + 2, 4, 30),
        _ => throw new ArgumentOutOfRangeException(nameof(account))
    };
}

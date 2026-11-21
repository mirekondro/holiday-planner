namespace BE.Domain.Holiday;

public static class Ledger
{
    public static decimal Sum(IEnumerable<LedgerEntry> entries, HolidayAccount account, int earningYear) =>
        entries
            .Where(e => e.Account == account && e.EarningYear == earningYear)
            .Sum(e => e.Hours);
}

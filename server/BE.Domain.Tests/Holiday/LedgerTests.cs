using BE.Domain.Holiday;

namespace BE.Domain.Tests.Holiday;

public class LedgerTests
{
    private static readonly Guid Employee = Guid.NewGuid();

    private static LedgerEntry Entry(HolidayAccount account, int earningYear, LedgerEntryType type, decimal hours) =>
        new(Employee, account, earningYear, new DateOnly(2027, 2, 2), type, hours);

    [Fact]
    public void Sum_OfNoEntries_IsZero()
    {
        var result = Ledger.Sum([], HolidayAccount.Weeks1to5, 2026);

        Assert.Equal(0m, result);
    }

    [Fact]
    public void Sum_UsageThenCancellation_IsZero()
    {
        var entries = new[]
        {
            Entry(HolidayAccount.Weeks1to5, 2026, LedgerEntryType.Usage, -37.00m),
            Entry(HolidayAccount.Weeks1to5, 2026, LedgerEntryType.CancellationReturn, 37.00m)
        };

        var result = Ledger.Sum(entries, HolidayAccount.Weeks1to5, 2026);

        Assert.Equal(0m, result);
    }

    [Fact]
    public void Sum_OnlyCountsTheRequestedAccount()
    {
        var entries = new[]
        {
            Entry(HolidayAccount.Weeks1to5, 2026, LedgerEntryType.Usage, -7.50m),
            Entry(HolidayAccount.Week6, 2026, LedgerEntryType.Usage, -7.00m)
        };

        Assert.Equal(-7.50m, Ledger.Sum(entries, HolidayAccount.Weeks1to5, 2026));
        Assert.Equal(-7.00m, Ledger.Sum(entries, HolidayAccount.Week6, 2026));
    }

    [Fact]
    public void Sum_OnlyCountsTheRequestedEarningYear()
    {
        var entries = new[]
        {
            Entry(HolidayAccount.Weeks1to5, 2025, LedgerEntryType.Usage, -15.00m),
            Entry(HolidayAccount.Weeks1to5, 2026, LedgerEntryType.Usage, -22.50m)
        };

        var result = Ledger.Sum(entries, HolidayAccount.Weeks1to5, 2026);

        Assert.Equal(-22.50m, result);
    }
}

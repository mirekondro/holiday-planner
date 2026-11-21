using BE.Domain.Holiday;

namespace BE.Domain.Tests.Holiday;

public class HolidayYearTests
{
    // Týdny 1–5: září začíná nový rok získání, leden–srpen ještě patří k předchozímu.
    [Theory]
    [InlineData(2026, 9, 1, 2026)]
    [InlineData(2026, 10, 15, 2026)]
    [InlineData(2026, 12, 31, 2026)]
    [InlineData(2027, 3, 1, 2026)]
    [InlineData(2027, 8, 31, 2026)]
    public void Weeks1to5_EarningYear(int year, int month, int day, int expected)
    {
        var date = new DateOnly(year, month, day);

        var result = HolidayYear.EarningYearFor(HolidayAccount.Weeks1to5, date);

        Assert.Equal(expected, result);
    }

    // 6. týden: rok získání je prostě kalendářní rok.
    [Theory]
    [InlineData(2026, 1, 1, 2026)]
    [InlineData(2026, 12, 31, 2026)]
    [InlineData(2027, 5, 1, 2027)]
    public void Week6_EarningYear(int year, int month, int day, int expected)
    {
        var date = new DateOnly(year, month, day);

        var result = HolidayYear.EarningYearFor(HolidayAccount.Week6, date);

        Assert.Equal(expected, result);
    }

    // Příklad ze zadání: týdny 1–5 získané od 1. 9. 2026 se dají čerpat do 31. 12. 2027 (16 měsíců).
    [Fact]
    public void Weeks1to5_EarningYear2026_IsEarnedAndUsedInTheRightPeriods()
    {
        const int earningYear = 2026;

        Assert.Equal(new DateOnly(2026, 9, 1), HolidayYear.EarningStart(HolidayAccount.Weeks1to5, earningYear));
        Assert.Equal(new DateOnly(2027, 8, 31), HolidayYear.EarningEnd(HolidayAccount.Weeks1to5, earningYear));
        Assert.Equal(new DateOnly(2026, 9, 1), HolidayYear.UsageStart(HolidayAccount.Weeks1to5, earningYear));
        Assert.Equal(new DateOnly(2027, 12, 31), HolidayYear.UsageEnd(HolidayAccount.Weeks1to5, earningYear));
    }

    // Příklad ze zadání: hodiny získané v roce 2026 se dají čerpat od 1. 5. 2027 do 30. 4. 2028.
    [Fact]
    public void Week6_EarningYear2026_IsEarnedAndUsedInTheRightPeriods()
    {
        const int earningYear = 2026;

        Assert.Equal(new DateOnly(2026, 1, 1), HolidayYear.EarningStart(HolidayAccount.Week6, earningYear));
        Assert.Equal(new DateOnly(2026, 12, 31), HolidayYear.EarningEnd(HolidayAccount.Week6, earningYear));
        Assert.Equal(new DateOnly(2027, 5, 1), HolidayYear.UsageStart(HolidayAccount.Week6, earningYear));
        Assert.Equal(new DateOnly(2028, 4, 30), HolidayYear.UsageEnd(HolidayAccount.Week6, earningYear));
    }
}

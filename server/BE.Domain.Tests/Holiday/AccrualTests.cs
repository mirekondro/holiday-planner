using BE.Domain.Employees;
using BE.Domain.Holiday;

namespace BE.Domain.Tests.Holiday;

public class AccrualTests
{
    private static readonly AccrualRules Rules = AccrualRules.BusinessEsbjerg;

    // Částečný úvazek 20 h/týden: po–čt 5 h, pá 0 h.
    private static WorkSchedule PartTime20(DateOnly validFrom, DateOnly? validTo = null) =>
        new(validFrom, validTo,
        [
            new WorkDay(DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(13, 0)),
            new WorkDay(DayOfWeek.Tuesday, new TimeOnly(8, 0), new TimeOnly(13, 0)),
            new WorkDay(DayOfWeek.Wednesday, new TimeOnly(8, 0), new TimeOnly(13, 0)),
            new WorkDay(DayOfWeek.Thursday, new TimeOnly(8, 0), new TimeOnly(13, 0))
        ]);

    // Anna: plný úvazek od 1. 9. 2026.
    private static Employment FullTimeFromSep2026() =>
        new(new DateOnly(2026, 9, 1), null, [WorkSchedule.FullTime(new DateOnly(2026, 9, 1))]);

    // Zadání: po–čt 7,5 h, pá 7 h = 37 h týdně.
    [Fact]
    public void FullTimeSchedule_Is37HoursPerWeek()
    {
        var schedule = WorkSchedule.FullTime(new DateOnly(2026, 9, 1));

        Assert.Equal(37m, schedule.WeeklyHours);
        Assert.Equal(7.5m, schedule.HoursOn(DayOfWeek.Tuesday));
        Assert.Equal(7m, schedule.HoursOn(DayOfWeek.Friday));
        Assert.Equal(0m, schedule.HoursOn(DayOfWeek.Saturday));
    }

    // Zadání: týdny 1–5 = 25 dní = 185 h ročně. Kumulativní zaokrouhlení → přesně 185,00, ne 185,04.
    [Fact]
    public void Weeks1to5_FullTime_WholeEarningYear_Is185Hours()
    {
        var result = Accrual.Accrued(FullTimeFromSep2026(), HolidayAccount.Weeks1to5, 2026,
                                     new DateOnly(2027, 9, 1), Rules);

        Assert.Equal(185.00m, result);
    }

    // Zadání: 6. týden = 12 × 3,083 = 37 h ročně.
    [Fact]
    public void Week6_FullTime_WholeCalendarYear_Is37Hours()
    {
        var employment = new Employment(new DateOnly(2026, 1, 1), null, [WorkSchedule.FullTime(new DateOnly(2026, 1, 1))]);

        var result = Accrual.Accrued(employment, HolidayAccount.Week6, 2026, new DateOnly(2027, 1, 1), Rules);

        Assert.Equal(37.00m, result);
    }

    // Hodiny za měsíc se připíšou až po jeho skončení („den získaný v říjnu se dá vzít v listopadu").
    [Theory]
    [InlineData(2026, 9, 30, 0.00)]
    [InlineData(2026, 10, 1, 15.42)]
    [InlineData(2026, 11, 1, 30.83)]
    [InlineData(2027, 1, 1, 61.67)]
    public void Weeks1to5_IsCreditedAfterEachMonthEnds(int year, int month, int day, double expected)
    {
        var result = Accrual.Accrued(FullTimeFromSep2026(), HolidayAccount.Weeks1to5, 2026,
                                     new DateOnly(year, month, day), Rules);

        Assert.Equal((decimal)expected, result);
    }

    // Částečný úvazek se počítá poměrně: 185 × 20/37 = 100 h.
    [Fact]
    public void Weeks1to5_PartTime20Hours_WholeYear_Is100Hours()
    {
        var employment = new Employment(new DateOnly(2026, 9, 1), null, [PartTime20(new DateOnly(2026, 9, 1))]);

        var result = Accrual.Accrued(employment, HolidayAccount.Weeks1to5, 2026, new DateOnly(2027, 9, 1), Rules);

        Assert.Equal(100.00m, result);
    }

    // Nástup 16. 9.: září má 15 odpracovaných dní → 15/30 × 15,4167 = 7,71 h.
    [Fact]
    public void Weeks1to5_StartMidMonth_CountsCalendarDays()
    {
        var employment = new Employment(new DateOnly(2026, 9, 16), null, [WorkSchedule.FullTime(new DateOnly(2026, 9, 16))]);

        var result = Accrual.Accrued(employment, HolidayAccount.Weeks1to5, 2026, new DateOnly(2026, 10, 1), Rules);

        Assert.Equal(7.71m, result);
    }

    // 6. týden při nástupu 16. 9.: září 15 × 0,103 h + říjen–prosinec 3 × 3,083 h = 10,79 h.
    [Fact]
    public void Week6_StartMidMonth_UsesAbout0Point103HoursPerDay()
    {
        var employment = new Employment(new DateOnly(2026, 9, 16), null, [WorkSchedule.FullTime(new DateOnly(2026, 9, 16))]);

        var result = Accrual.Accrued(employment, HolidayAccount.Week6, 2026, new DateOnly(2027, 1, 1), Rules);

        Assert.Equal(10.79m, result);
    }

    // Odchod 15. 3. 2027: září–únor celé (6 × 15,4167 = 92,5) + březen 15/30 (7,71) = 100,21 h, pak nic.
    [Fact]
    public void Weeks1to5_LeavingMidMonth_StopsAccrual()
    {
        var employment = new Employment(new DateOnly(2026, 9, 1), new DateOnly(2027, 3, 15),
                                        [WorkSchedule.FullTime(new DateOnly(2026, 9, 1))]);

        var result = Accrual.Accrued(employment, HolidayAccount.Weeks1to5, 2026, new DateOnly(2027, 9, 1), Rules);

        Assert.Equal(100.21m, result);
    }

    // Změna úvazku 1. 1. 2027: září–prosinec plný (4 × 15,4167) + leden–srpen 20 h (8 × 8,3333) = 128,33 h.
    [Fact]
    public void Weeks1to5_ScheduleChange_UsesTheScheduleValidInEachMonth()
    {
        var employment = new Employment(new DateOnly(2026, 9, 1), null,
        [
            WorkSchedule.FullTime(new DateOnly(2026, 9, 1), new DateOnly(2026, 12, 31)),
            PartTime20(new DateOnly(2027, 1, 1))
        ]);

        var result = Accrual.Accrued(employment, HolidayAccount.Weeks1to5, 2026, new DateOnly(2027, 9, 1), Rules);

        Assert.Equal(128.33m, result);
    }

    // Za rok, kdy zaměstnanec ještě nepracoval, se nic nepřipíše.
    [Fact]
    public void Weeks1to5_BeforeEmployment_IsZero()
    {
        var result = Accrual.Accrued(FullTimeFromSep2026(), HolidayAccount.Weeks1to5, 2025,
                                     new DateOnly(2026, 9, 1), Rules);

        Assert.Equal(0m, result);
    }
}

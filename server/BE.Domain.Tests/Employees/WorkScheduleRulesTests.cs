using BE.Domain.Employees;

namespace BE.Domain.Tests.Employees;

public class WorkScheduleRulesTests
{
    private static readonly TimeOnly Eight = new(8, 0);
    private static readonly DateOnly Start = new(2026, 1, 1);

    private static List<ScheduleDay> FullTimeWeek() =>
    [
        new(DayOfWeek.Monday, Eight, new TimeOnly(15, 30)),
        new(DayOfWeek.Tuesday, Eight, new TimeOnly(15, 30)),
        new(DayOfWeek.Wednesday, Eight, new TimeOnly(15, 30)),
        new(DayOfWeek.Thursday, Eight, new TimeOnly(15, 30)),
        new(DayOfWeek.Friday, Eight, new TimeOnly(15, 0)),
    ];

    [Fact]
    public void ValidateWeek_FullTimeWeek_IsValid() =>
        Assert.Empty(WorkScheduleRules.ValidateWeek(FullTimeWeek()));

    [Fact]
    public void ValidateWeek_PartTimeWithDaysOff_IsValid()
    {
        var week = FullTimeWeek();
        week[3] = new ScheduleDay(DayOfWeek.Thursday, null, null);
        week[4] = new ScheduleDay(DayOfWeek.Friday, null, null);

        Assert.Empty(WorkScheduleRules.ValidateWeek(week));
    }

    [Fact]
    public void ValidateWeek_MissingDay_IsRejected()
    {
        var week = FullTimeWeek();
        week.RemoveAt(4);

        Assert.Contains(WorkScheduleRules.ValidateWeek(week), e => e.Contains("Friday"));
    }

    [Fact]
    public void ValidateWeek_DuplicateDay_IsRejected()
    {
        var week = FullTimeWeek();
        week[4] = new ScheduleDay(DayOfWeek.Monday, Eight, new TimeOnly(12, 0));

        Assert.NotEmpty(WorkScheduleRules.ValidateWeek(week));
    }

    [Fact]
    public void ValidateWeek_Weekend_IsRejected()
    {
        var week = FullTimeWeek();
        week.Add(new ScheduleDay(DayOfWeek.Saturday, Eight, new TimeOnly(12, 0)));

        Assert.Contains(WorkScheduleRules.ValidateWeek(week), e => e.Contains("Saturday"));
    }

    [Fact]
    public void ValidateWeek_EndBeforeStart_IsRejected()
    {
        var week = FullTimeWeek();
        week[0] = new ScheduleDay(DayOfWeek.Monday, new TimeOnly(15, 0), Eight);

        Assert.Contains(WorkScheduleRules.ValidateWeek(week), e => e.Contains("Monday"));
    }

    [Fact]
    public void ValidateWeek_OnlyOneTime_IsRejected()
    {
        var week = FullTimeWeek();
        week[0] = new ScheduleDay(DayOfWeek.Monday, Eight, null);

        Assert.NotEmpty(WorkScheduleRules.ValidateWeek(week));
    }

    [Fact]
    public void ValidateWeek_AllDaysOff_IsRejected()
    {
        var week = WorkScheduleRules.WorkWeek.Select(d => new ScheduleDay(d, null, null)).ToList();

        Assert.Contains(WorkScheduleRules.ValidateWeek(week), e => e.Contains("at least one"));
    }

    [Fact]
    public void ToWorkSchedule_FullTimeWeek_Is37Hours()
    {
        var schedule = WorkScheduleRules.ToWorkSchedule(Start, null, FullTimeWeek());

        Assert.Equal(37m, schedule.WeeklyHours);
        Assert.Equal(7m, schedule.HoursOn(DayOfWeek.Friday));
    }

    [Fact]
    public void ToWorkSchedule_LeavesOutDaysOff()
    {
        var week = FullTimeWeek();
        week[4] = new ScheduleDay(DayOfWeek.Friday, null, null);

        var schedule = WorkScheduleRules.ToWorkSchedule(Start, null, week);

        Assert.Equal(30m, schedule.WeeklyHours);
        Assert.Equal(0m, schedule.HoursOn(DayOfWeek.Friday));
    }

    [Fact]
    public void ValidateNewPeriodStart_AfterCurrentPeriod_IsValid() =>
        Assert.Null(WorkScheduleRules.ValidateNewPeriodStart(new DateOnly(2026, 6, 1), Start, null, Start));

    [Fact]
    public void ValidateNewPeriodStart_SameDayAsCurrent_IsRejected() =>
        Assert.NotNull(WorkScheduleRules.ValidateNewPeriodStart(Start, Start, null, Start));

    [Fact]
    public void ValidateNewPeriodStart_BeforeEmployment_IsRejected() =>
        Assert.NotNull(WorkScheduleRules.ValidateNewPeriodStart(new DateOnly(2025, 12, 1), Start, null, null));

    [Fact]
    public void ValidateNewPeriodStart_AfterEmploymentEnded_IsRejected() =>
        Assert.NotNull(WorkScheduleRules.ValidateNewPeriodStart(new DateOnly(2026, 8, 1), Start, new DateOnly(2026, 7, 31), Start));
}

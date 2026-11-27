using System.Net;
using System.Net.Http.Json;

namespace BE.Api.Tests.Integration;

[Collection(ApiCollection.Name)]
public class EmployeesApiTests(ApiFactory api)
{
    private async Task<EmployeeJson> CreateEmployee(object? body = null)
    {
        var response = await api.As("SystemOwner").PostAsJsonAsync("/api/employees", body ?? TestData.NewEmployee());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<EmployeeJson>(TestData.Json))!;
    }

    [Fact]
    public async Task List_WithoutToken_Is401()
    {
        var response = await api.Anonymous().GetAsync("/api/employees");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_AsEmployee_Is403()
    {
        var response = await api.As("Employee", Guid.NewGuid()).PostAsJsonAsync("/api/employees", TestData.NewEmployee());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_AsAccountant_Is403()
    {
        var response = await api.As("Accountant").PostAsJsonAsync("/api/employees", TestData.NewEmployee());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_AsSystemOwner_SavesEmployeeAndFirstSchedule()
    {
        var email = TestData.UniqueEmail("anna");

        var created = await CreateEmployee(TestData.NewEmployee(email));

        Assert.Equal(email, created.Email);
        var fetched = await api.As("Accountant").GetFromJsonAsync<EmployeeJson>($"/api/employees/{created.Id}", TestData.Json);
        Assert.Equal("Hansen", fetched!.LastName);

        var schedules = await api.As("Accountant").GetFromJsonAsync<List<SchedulePeriodJson>>($"/api/employees/{created.Id}/schedules", TestData.Json);
        var first = Assert.Single(schedules!);
        Assert.Equal(new DateOnly(2026, 1, 1), first.ValidFrom);
        Assert.Null(first.ValidTo);
        Assert.Equal(37m, first.WeeklyHours);
    }

    [Fact]
    public async Task Create_WithNonOrganizationEmail_Is400()
    {
        var response = await api.As("SystemOwner").PostAsJsonAsync("/api/employees", TestData.NewEmployee("anna@gmail.com"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("businessesbjerg.com", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Create_WithInvalidSchedule_Is400()
    {
        var badWeek = TestData.FullTimeWeek();
        badWeek[0] = new ScheduleDayJson(1, "15:00", "08:00");

        var response = await api.As("SystemOwner").PostAsJsonAsync("/api/employees", TestData.NewEmployee(schedule: badWeek));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithoutSchedule_Is400()
    {
        var body = new { firstName = "No", lastName = "Schedule", email = TestData.UniqueEmail(), employmentStart = new DateOnly(2026, 1, 1) };

        var response = await api.As("SystemOwner").PostAsJsonAsync("/api/employees", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_DuplicateEmail_Is409()
    {
        var email = TestData.UniqueEmail("dup");
        await CreateEmployee(TestData.NewEmployee(email));

        var response = await api.As("SystemOwner").PostAsJsonAsync("/api/employees", TestData.NewEmployee(email.ToUpperInvariant()));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task AddSchedule_ClosesPreviousPeriodAndKeepsHistory()
    {
        var employee = await CreateEmployee();
        var body = new { validFrom = new DateOnly(2026, 6, 1), days = TestData.PartTimeWeek() };

        var response = await api.As("Accountant").PostAsJsonAsync($"/api/employees/{employee.Id}/schedules", body);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var periods = (await response.Content.ReadFromJsonAsync<List<SchedulePeriodJson>>(TestData.Json))!;
        Assert.Equal(2, periods.Count);
        Assert.Equal(new DateOnly(2026, 5, 31), periods[0].ValidTo); // full time until the day before
        Assert.Equal(37m, periods[0].WeeklyHours);
        Assert.Null(periods[1].ValidTo);
        Assert.Equal(20m, periods[1].WeeklyHours);
    }

    [Fact]
    public async Task AddSchedule_StartingBeforeCurrentPeriod_Is400()
    {
        var employee = await CreateEmployee();
        var body = new { validFrom = new DateOnly(2025, 12, 1), days = TestData.PartTimeWeek() };

        var response = await api.As("Accountant").PostAsJsonAsync($"/api/employees/{employee.Id}/schedules", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddSchedule_AsEmployee_Is403()
    {
        var employee = await CreateEmployee();
        var body = new { validFrom = new DateOnly(2026, 6, 1), days = TestData.PartTimeWeek() };

        // Even the employee themselves may not change their own schedule.
        var response = await api.As("Employee", employee.Id).PostAsJsonAsync($"/api/employees/{employee.Id}/schedules", body);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Schedules_EmployeeSeesOwnButNotColleagues()
    {
        var me = await CreateEmployee();
        var colleague = await CreateEmployee();
        var client = api.As("Employee", me.Id);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/employees/{me.Id}/schedules")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/api/employees/{colleague.Id}/schedules")).StatusCode);
    }

    [Fact]
    public async Task Deactivate_SetsEmploymentEnd()
    {
        var employee = await CreateEmployee();

        var response = await api.As("SystemOwner").PostAsJsonAsync($"/api/employees/{employee.Id}/deactivate",
            new { employmentEnd = new DateOnly(2026, 9, 30) });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = (await response.Content.ReadFromJsonAsync<EmployeeJson>(TestData.Json))!;
        Assert.Equal(new DateOnly(2026, 9, 30), updated.EmploymentEnd);
    }

    [Fact]
    public async Task Get_UnknownEmployee_Is404()
    {
        var response = await api.As("Accountant").GetAsync($"/api/employees/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

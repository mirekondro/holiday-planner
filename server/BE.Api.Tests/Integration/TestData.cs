using System.Text.Json;
using System.Text.Json.Serialization;

namespace BE.Api.Tests.Integration;

// Minimal copies of the API's JSON shapes, so the tests check the real wire format.
public record ScheduleDayJson(int Weekday, string? Start, string? End);
public record EmployeeJson(Guid Id, string FirstName, string LastName, string Email, DateOnly EmploymentStart, DateOnly? EmploymentEnd);
public record LoginUserJson(Guid Id, string Email, string Role, Guid? EmployeeId);
public record LoginJson(string Token, DateTimeOffset ExpiresAt, LoginUserJson User);
public record SchedulePeriodJson(Guid Id, DateOnly ValidFrom, DateOnly? ValidTo, decimal WeeklyHours, List<ScheduleDayJson> Days);

public static class TestData
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public static List<ScheduleDayJson> FullTimeWeek() =>
    [
        new(1, "08:00", "15:30"),
        new(2, "08:00", "15:30"),
        new(3, "08:00", "15:30"),
        new(4, "08:00", "15:30"),
        new(5, "08:00", "15:00"),
    ];

    /// <summary>20 hours: Monday–Wednesday 08:00–14:40, Thursday and Friday off.</summary>
    public static List<ScheduleDayJson> PartTimeWeek() =>
    [
        new(1, "08:00", "14:40"),
        new(2, "08:00", "14:40"),
        new(3, "08:00", "14:40"),
        new(4, null, null),
        new(5, null, null),
    ];

    public static string UniqueEmail(string name = "test") => $"{name}-{Guid.NewGuid():N}@businessesbjerg.com";

    public static object NewEmployee(string? email = null, List<ScheduleDayJson>? schedule = null) => new
    {
        firstName = "Anna",
        lastName = "Hansen",
        email = email ?? UniqueEmail("anna"),
        employmentStart = new DateOnly(2026, 1, 1),
        schedule = schedule ?? FullTimeWeek(),
    };
}

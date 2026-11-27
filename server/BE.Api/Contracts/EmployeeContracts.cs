using System.ComponentModel.DataAnnotations;

namespace BE.Api.Contracts;

/// <summary>One weekday. Weekday: 1 = Monday … 5 = Friday. Start/End as "HH:mm", both null = day off.</summary>
public record ScheduleDayDto(
    [Range(1, 5)] int Weekday,
    [RegularExpression(@"^\d{2}:\d{2}$")] string? Start,
    [RegularExpression(@"^\d{2}:\d{2}$")] string? End);

public record EmployeeDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    DateOnly EmploymentStart,
    DateOnly? EmploymentEnd);

public record WorkSchedulePeriodDto(
    Guid Id,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    decimal WeeklyHours,
    List<ScheduleDayDto> Days);

/// <summary>A new employee always comes with their first work schedule, valid from the employment start.</summary>
public record CreateEmployeeRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress, MaxLength(254)] string Email,
    [Required] DateOnly? EmploymentStart,
    [Required] List<ScheduleDayDto> Schedule);

public record UpdateEmployeeRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress, MaxLength(254)] string Email);

public record DeactivateEmployeeRequest([Required] DateOnly? EmploymentEnd);

public record AddWorkScheduleRequest(
    [Required] DateOnly? ValidFrom,
    [Required] List<ScheduleDayDto> Days);

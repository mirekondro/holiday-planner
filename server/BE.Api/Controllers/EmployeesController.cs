using BE.Api.Auth;
using BE.Api.Contracts;
using BE.Domain.Users;
using BE.Domain.Employees;
using BE.Infrastructure.Database;
using BE.Infrastructure.Entities;
using LinqToDB;
using LinqToDB.Async;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE.Api.Controllers;

/// <summary>
/// Employees and their work schedules.
/// Reading the employee list: any logged-in user (the shared holiday calendar needs it).
/// Creating / editing / deactivating: system owner only.
/// Schedules: read by the employee themselves, accountant and system owner; changed by accountant or system owner.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EmployeesController(AppDataConnection db) : ControllerBase
{
    [HttpGet]
    public async Task<List<EmployeeDto>> GetAll()
    {
        var employees = await db.Employees.OrderBy(e => e.LastName).ThenBy(e => e.FirstName).ToListAsync();
        return employees.Select(ToDto).ToList();
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<EmployeeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeDto>> Get(Guid id)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == id);
        if (employee is null) return NotFound();
        return ToDto(employee);
    }

    [HttpPost]
    [Authorize(Policy = Policies.SystemOwner)]
    [ProducesResponseType<EmployeeDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmployeeDto>> Create(CreateEmployeeRequest request)
    {
        ValidateEmail(request.Email);
        var week = ScheduleMapping.ParseWeek(request.Schedule, ModelState, nameof(request.Schedule));
        if (!ModelState.IsValid || week is null)
            return ValidationProblem(ModelState);

        var email = UserRules.NormalizeEmail(request.Email);
        if (await db.Employees.AnyAsync(e => e.Email == email))
            return Conflict(new ProblemDetails { Title = "An employee with this email already exists." });

        var employee = new EmployeeEntity
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            EmploymentStart = request.EmploymentStart!.Value, // [Required] already rejected null with 400
            EmploymentEnd = null,
        };
        var schedule = new WorkScheduleEntity
        {
            Id = Guid.NewGuid(),
            EmployeeId = employee.Id,
            ValidFrom = employee.EmploymentStart,
            ValidTo = null,
        };

        // Employee and first schedule are saved together or not at all.
        await using (var tx = await db.BeginTransactionAsync())
        {
            await db.InsertAsync(employee);
            await db.InsertAsync(schedule);
            foreach (var day in ScheduleMapping.ToEntities(schedule.Id, week))
                await db.InsertAsync(day);
            await tx.CommitAsync();
        }

        return CreatedAtAction(nameof(Get), new { id = employee.Id }, ToDto(employee));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.SystemOwner)]
    [ProducesResponseType<EmployeeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmployeeDto>> Update(Guid id, UpdateEmployeeRequest request)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == id);
        if (employee is null) return NotFound();

        ValidateEmail(request.Email);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var email = UserRules.NormalizeEmail(request.Email);
        if (await db.Employees.AnyAsync(e => e.Email == email && e.Id != id))
            return Conflict(new ProblemDetails { Title = "Another employee already uses this email." });

        employee.FirstName = request.FirstName.Trim();
        employee.LastName = request.LastName.Trim();
        employee.Email = email;
        // Employment start is deliberately not editable: accrued holiday depends on it.
        await db.UpdateAsync(employee);

        return ToDto(employee);
    }

    /// <summary>Ends the employment (resignation or dismissal). The employee and all history are kept.</summary>
    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = Policies.SystemOwner)]
    [ProducesResponseType<EmployeeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeDto>> Deactivate(Guid id, DeactivateEmployeeRequest request)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == id);
        if (employee is null) return NotFound();

        var end = request.EmploymentEnd!.Value;
        if (end < employee.EmploymentStart)
        {
            ModelState.AddModelError(nameof(request.EmploymentEnd), "Employment cannot end before it started.");
            return ValidationProblem(ModelState);
        }

        employee.EmploymentEnd = end;
        await db.UpdateAsync(employee);
        return ToDto(employee);
    }

    /// <summary>The full schedule history, oldest first.</summary>
    [HttpGet("{id:guid}/schedules")]
    [ProducesResponseType<List<WorkSchedulePeriodDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<WorkSchedulePeriodDto>>> GetSchedules(Guid id)
    {
        // Resource-based check: an employee may only see their own schedule.
        if (!CanSeeScheduleOf(id)) return Forbid();
        if (!await db.Employees.AnyAsync(e => e.Id == id)) return NotFound();

        return await LoadSchedules(id);
    }

    /// <summary>
    /// Starts a new schedule period (e.g. full time → part time). The current period is closed the day before,
    /// so periods never overlap and old periods are never changed.
    /// </summary>
    [HttpPost("{id:guid}/schedules")]
    [Authorize(Policy = Policies.ManageSchedules)]
    [ProducesResponseType<List<WorkSchedulePeriodDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<WorkSchedulePeriodDto>>> AddSchedule(Guid id, AddWorkScheduleRequest request)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == id);
        if (employee is null) return NotFound();

        var validFrom = request.ValidFrom!.Value;
        var current = await db.WorkSchedules
            .Where(s => s.EmployeeId == id)
            .OrderByDescending(s => s.ValidFrom)
            .FirstOrDefaultAsync();

        var periodError = WorkScheduleRules.ValidateNewPeriodStart(
            validFrom, employee.EmploymentStart, employee.EmploymentEnd, current?.ValidFrom);
        if (periodError is not null)
            ModelState.AddModelError(nameof(request.ValidFrom), periodError);

        var week = ScheduleMapping.ParseWeek(request.Days, ModelState, nameof(request.Days));
        if (!ModelState.IsValid || week is null)
            return ValidationProblem(ModelState);

        var schedule = new WorkScheduleEntity { Id = Guid.NewGuid(), EmployeeId = id, ValidFrom = validFrom, ValidTo = null };

        await using (var tx = await db.BeginTransactionAsync())
        {
            if (current is not null && (current.ValidTo is null || current.ValidTo >= validFrom))
            {
                var dayBefore = validFrom.AddDays(-1);
                await db.WorkSchedules.Where(s => s.Id == current.Id).Set(s => s.ValidTo, (DateOnly?)dayBefore).UpdateAsync();
            }
            await db.InsertAsync(schedule);
            foreach (var day in ScheduleMapping.ToEntities(schedule.Id, week))
                await db.InsertAsync(day);
            await tx.CommitAsync();
        }

        // Converting the remaining balance so the number of days stays the same is done by the ledger (separate issue).
        return Created($"/api/employees/{id}/schedules", await LoadSchedules(id));
    }

    private async Task<List<WorkSchedulePeriodDto>> LoadSchedules(Guid employeeId)
    {
        var schedules = await db.WorkSchedules.Where(s => s.EmployeeId == employeeId).OrderBy(s => s.ValidFrom).ToListAsync();
        var ids = schedules.Select(s => s.Id).ToList();
        var days = await db.WorkScheduleDays.Where(d => ids.Contains(d.ScheduleId)).ToListAsync();
        var daysBySchedule = days.ToLookup(d => d.ScheduleId);

        return schedules.Select(s => ScheduleMapping.ToDto(s, daysBySchedule[s.Id])).ToList();
    }

    private bool CanSeeScheduleOf(Guid employeeId) =>
        User.IsInRole(nameof(UserRole.Accountant))
        || User.IsInRole(nameof(UserRole.SystemOwner))
        || User.FindFirst(AuthClaims.EmployeeId)?.Value == employeeId.ToString();

    private void ValidateEmail(string email)
    {
        if (!UserRules.IsOrganizationEmail(email))
            ModelState.AddModelError("Email", $"Email must end with {UserRules.OrganizationDomain}.");
    }

    private static EmployeeDto ToDto(EmployeeEntity e) =>
        new(e.Id, e.FirstName, e.LastName, e.Email, e.EmploymentStart, e.EmploymentEnd);
}

using BE.Infrastructure.Database;
using BE.Infrastructure.Entities;
using LinqToDB.Async;
using Microsoft.AspNetCore.Mvc;

namespace BE.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController(AppDataConnection db) : ControllerBase
{
    [HttpGet]
    public async Task<List<EmployeeEntity>> GetAll() =>
        await db.Employees.ToListAsync();
}

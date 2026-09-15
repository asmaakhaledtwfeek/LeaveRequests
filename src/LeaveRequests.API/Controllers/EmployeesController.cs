using Microsoft.AspNetCore.Mvc;
using LeaveRequests.API.Services;

namespace LeaveRequests.API.Controllers;

[ApiController]
[Route("api/employees")]
public class EmployeesController : ControllerBase
{
    private readonly EmployeeService _employees;

    public EmployeesController(EmployeeService employees)
    {
        _employees = employees;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] int limit = 20,
        [FromQuery] int skip = 0)
    {
        var list = await _employees.GetListAsync(limit, skip, search);
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var emp = await _employees.GetByIdAsync(id);
        if (emp is null)
            return NotFound(new { error = $"Employee {id} not found." });

        return Ok(emp);
    }
}

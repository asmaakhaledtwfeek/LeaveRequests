using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LeaveRequests.API.DTOs;
using LeaveRequests.API.Services;

namespace LeaveRequests.API.Controllers;

[ApiController]
[Route("api/leave-requests")]
public class LeaveRequestsController : ControllerBase
{
    private readonly LeaveRequestService _service;

    public LeaveRequestsController(LeaveRequestService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] int? employeeId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 10;

        var result = await _service.GetPagedAsync(status, employeeId, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await _service.GetByIdAsync(id);
        if (dto is null)
            return NotFound(new { error = "Leave request not found." });

        return Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLeaveRequestDto input)
    {
        var (dto, error) = await _service.CreateAsync(input);

        if (error is not null)
        {
            if (error.Contains("not found"))
                return NotFound(new { error });
            return BadRequest(new { error });
        }

        return CreatedAtAction(nameof(GetById), new { id = dto!.Id }, dto);
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "HR")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto input)
    {
        var (dto, error, statusCode) = await _service.UpdateStatusAsync(id, input);

        return statusCode switch
        {
            404 => NotFound(new { error }),
            409 => Conflict(new { error }),
            _ => Ok(dto)
        };
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "HR")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, error) = await _service.DeleteAsync(id);

        if (!success)
        {
            if (error!.Contains("not found"))
                return NotFound(new { error });
            return BadRequest(new { error });
        }

        return NoContent();
    }
}

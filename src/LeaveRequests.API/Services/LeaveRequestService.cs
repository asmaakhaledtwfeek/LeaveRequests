using Microsoft.EntityFrameworkCore;
using LeaveRequests.API.Data;
using LeaveRequests.API.DTOs;
using LeaveRequests.API.Models;

namespace LeaveRequests.API.Services;

public class LeaveRequestService
{
    private readonly AppDbContext _db;
    private readonly EmployeeService _employees;

    public LeaveRequestService(AppDbContext db, EmployeeService employees)
    {
        _db = db;
        _employees = employees;
    }

    public async Task<PagedResult<LeaveRequestDto>> GetPagedAsync(
        string? status, int? employeeId, int page, int pageSize)
    {
        var query = _db.LeaveRequests.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.Status == status);

        if (employeeId.HasValue)
            query = query.Where(x => x.EmployeeId == employeeId.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var empIds = items.Select(x => x.EmployeeId).Distinct();
        var empMap = await _employees.GetByIdsAsync(empIds);

        var dtos = items.Select(r => MapToDto(r, empMap.GetValueOrDefault(r.EmployeeId))).ToList();

        return new PagedResult<LeaveRequestDto>
        {
            Items = dtos,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<LeaveRequestDto?> GetByIdAsync(int id)
    {
        var record = await _db.LeaveRequests.FindAsync(id);
        if (record is null) return null;

        var emp = await _employees.GetByIdAsync(record.EmployeeId);
        return MapToDto(record, emp);
    }

    public async Task<(LeaveRequestDto? dto, string? error)> CreateAsync(CreateLeaveRequestDto input)
    {
        if (input.EndDate < input.StartDate)
            return (null, "EndDate must be on or after StartDate.");

        var emp = await _employees.GetByIdAsync(input.EmployeeId);
        if (emp is null)
            return (null, $"Employee {input.EmployeeId} not found.");

        var request = new LeaveRequest
        {
            EmployeeId = input.EmployeeId,
            StartDate = input.StartDate.Date,
            EndDate = input.EndDate.Date,
            Type = input.Type,
            Status = LeaveStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _db.LeaveRequests.Add(request);
        await _db.SaveChangesAsync();

        return (MapToDto(request, emp), null);
    }

    public async Task<(LeaveRequestDto? dto, string? error, int statusCode)> UpdateStatusAsync(
        int id, UpdateStatusDto input)
    {
        var record = await _db.LeaveRequests.FindAsync(id);
        if (record is null)
            return (null, "Leave request not found.", 404);

        if (record.Status != LeaveStatus.Pending)
            return (null, $"Cannot transition from '{record.Status}'. Only Pending requests can be reviewed.", 409);

        record.Status = input.Status;
        record.ReviewerNote = input.ReviewerNote;
        await _db.SaveChangesAsync();

        var emp = await _employees.GetByIdAsync(record.EmployeeId);
        return (MapToDto(record, emp), null, 200);
    }

    public async Task<(bool success, string? error)> DeleteAsync(int id)
    {
        var record = await _db.LeaveRequests.FindAsync(id);
        if (record is null)
            return (false, "Leave request not found.");

        if (record.Status != LeaveStatus.Pending)
            return (false, $"Only Pending requests can be deleted. Current status: {record.Status}.");

        _db.LeaveRequests.Remove(record);
        await _db.SaveChangesAsync();
        return (true, null);
    }

    private static LeaveRequestDto MapToDto(LeaveRequest r, EmployeeDto? emp) => new()
    {
        Id = r.Id,
        EmployeeId = r.EmployeeId,
        EmployeeName = emp?.FullName ?? $"Employee #{r.EmployeeId}",
        Department = emp?.Company?.Department ?? string.Empty,
        StartDate = r.StartDate,
        EndDate = r.EndDate,
        Type = r.Type,
        Status = r.Status,
        CreatedAt = r.CreatedAt,
        ReviewerNote = r.ReviewerNote
    };
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using LeaveRequests.API.Data;
using LeaveRequests.API.DTOs;
using LeaveRequests.API.Models;
using LeaveRequests.API.Services;

namespace LeaveRequests.Tests;

public class LeaveRequestServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LeaveRequestService _service;
    private readonly FakeEmployeeService _fakeEmployees;

    public LeaveRequestServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _fakeEmployees = new FakeEmployeeService();
        _service = new LeaveRequestService(_db, _fakeEmployees);
    }

    public void Dispose() => _db.Dispose();

    private async Task<LeaveRequest> SeedPendingRequest(int employeeId = 1)
    {
        var r = new LeaveRequest
        {
            EmployeeId = employeeId,
            StartDate  = new DateTime(2025, 6, 1),
            EndDate    = new DateTime(2025, 6, 5),
            Type       = LeaveType.Vacation,
            Status     = LeaveStatus.Pending,
            CreatedAt  = DateTime.UtcNow
        };
        _db.LeaveRequests.Add(r);
        await _db.SaveChangesAsync();
        return r;
    }


    [Fact]
    public async Task ApproveRequest_WhenPending_SetsStatusApproved()
    {
        var r = await SeedPendingRequest();

        var (dto, error, code) = await _service.UpdateStatusAsync(
            r.Id, new UpdateStatusDto { Status = LeaveStatus.Approved });

        Assert.Null(error);
        Assert.Equal(200, code);
        Assert.Equal(LeaveStatus.Approved, dto!.Status);
    }


    [Fact]
    public async Task RejectRequest_WhenPending_SetsStatusRejected()
    {
        var r = await SeedPendingRequest();

        var (dto, error, code) = await _service.UpdateStatusAsync(
            r.Id, new UpdateStatusDto { Status = LeaveStatus.Rejected, ReviewerNote = "Too many absences." });

        Assert.Null(error);
        Assert.Equal(200, code);
        Assert.Equal(LeaveStatus.Rejected, dto!.Status);
        Assert.Equal("Too many absences.", dto.ReviewerNote);
    }


    [Fact]
    public async Task ApproveRequest_WhenAlreadyApproved_Returns409()
    {
        var r = await SeedPendingRequest();
        _db.Entry(r).State = EntityState.Detached;

        var approved = await _db.LeaveRequests.FindAsync(r.Id);
        approved!.Status = LeaveStatus.Approved;
        await _db.SaveChangesAsync();

        var (_, error, code) = await _service.UpdateStatusAsync(
            r.Id, new UpdateStatusDto { Status = LeaveStatus.Approved });

        Assert.Equal(409, code);
        Assert.NotNull(error);
    }


    [Fact]
    public async Task ApproveRequest_WhenRejected_Returns409()
    {
        var r = await SeedPendingRequest();
        r.Status = LeaveStatus.Rejected;
        await _db.SaveChangesAsync();

        var (_, error, code) = await _service.UpdateStatusAsync(
            r.Id, new UpdateStatusDto { Status = LeaveStatus.Approved });

        Assert.Equal(409, code);
        Assert.NotNull(error);
    }


    [Fact]
    public async Task UpdateStatus_NonExistentId_Returns404()
    {
        var (_, error, code) = await _service.UpdateStatusAsync(
            999, new UpdateStatusDto { Status = LeaveStatus.Approved });

        Assert.Equal(404, code);
        Assert.NotNull(error);
    }


    [Fact]
    public async Task Delete_WhenPending_Succeeds()
    {
        var r = await SeedPendingRequest();

        var (success, error) = await _service.DeleteAsync(r.Id);

        Assert.True(success);
        Assert.Null(error);
        Assert.Equal(0, await _db.LeaveRequests.CountAsync());
    }

    [Fact]
    public async Task Delete_WhenApproved_ReturnsBadRequest()
    {
        var r = await SeedPendingRequest();
        r.Status = LeaveStatus.Approved;
        await _db.SaveChangesAsync();

        var (success, error) = await _service.DeleteAsync(r.Id);

        Assert.False(success);
        Assert.NotNull(error);
    }


    [Fact]
    public async Task Create_WhenEndDateBeforeStartDate_ReturnsBadRequest()
    {
        var (dto, error) = await _service.CreateAsync(new CreateLeaveRequestDto
        {
            EmployeeId = 1,
            StartDate  = new DateTime(2025, 6, 10),
            EndDate    = new DateTime(2025, 6, 5),
            Type       = LeaveType.Vacation
        });

        Assert.Null(dto);
        Assert.NotNull(error);
        Assert.Contains("EndDate", error);
    }

    [Fact]
    public async Task Create_WhenSameDayRange_Succeeds()
    {
        var (dto, error) = await _service.CreateAsync(new CreateLeaveRequestDto
        {
            EmployeeId = 1,
            StartDate  = new DateTime(2025, 6, 5),
            EndDate    = new DateTime(2025, 6, 5),
            Type       = LeaveType.Sick
        });

        Assert.Null(error);
        Assert.NotNull(dto);
        Assert.Equal(LeaveStatus.Pending, dto!.Status);
    }

    [Fact]
    public async Task Create_WhenEmployeeNotFound_ReturnsError()
    {
        _fakeEmployees.ReturnNull = true;

        var (dto, error) = await _service.CreateAsync(new CreateLeaveRequestDto
        {
            EmployeeId = 999,
            StartDate  = new DateTime(2025, 7, 1),
            EndDate    = new DateTime(2025, 7, 3),
            Type       = LeaveType.Vacation
        });

        Assert.Null(dto);
        Assert.NotNull(error);
    }


    [Fact]
    public async Task GetPaged_FilterByStatus_ReturnsOnlyMatchingRecords()
    {
        await SeedPendingRequest(1);
        await SeedPendingRequest(2);

        var r3 = await SeedPendingRequest(3);
        r3.Status = LeaveStatus.Approved;
        await _db.SaveChangesAsync();

        var result = await _service.GetPagedAsync(LeaveStatus.Pending, null, 1, 10);

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, item => Assert.Equal(LeaveStatus.Pending, item.Status));
    }
}

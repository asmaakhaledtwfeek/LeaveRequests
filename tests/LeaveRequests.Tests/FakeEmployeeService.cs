using LeaveRequests.API.Services;

namespace LeaveRequests.Tests;

internal class FakeEmployeeService : EmployeeService
{
    public bool ReturnNull { get; set; }

    public FakeEmployeeService()
        : base(new HttpClient(), new Microsoft.Extensions.Caching.Memory.MemoryCache(
            new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions()))
    { }

    public override Task<EmployeeDto?> GetByIdAsync(int id)
    {
        if (ReturnNull) return Task.FromResult<EmployeeDto?>(null);

        return Task.FromResult<EmployeeDto?>(new EmployeeDto
        {
            Id        = id,
            FirstName = "Test",
            LastName  = $"Employee{id}",
            Company   = new CompanyInfo { Department = "Engineering", Title = "Dev" }
        });
    }

    public override Task<Dictionary<int, EmployeeDto>> GetByIdsAsync(IEnumerable<int> ids)
    {
        var dict = ids.ToDictionary(id => id, id => new EmployeeDto
        {
            Id        = id,
            FirstName = "Test",
            LastName  = $"Employee{id}",
            Company   = new CompanyInfo { Department = "Engineering", Title = "Dev" }
        });
        return Task.FromResult(dict);
    }
}

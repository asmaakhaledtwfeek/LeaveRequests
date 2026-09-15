using System.Text.Json.Serialization;

namespace LeaveRequests.API.Services;

public class EmployeeDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("company")]
    public CompanyInfo? Company { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}

public class CompanyInfo
{
    [JsonPropertyName("department")]
    public string Department { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
}

public class EmployeeListResult
{
    [JsonPropertyName("users")]
    public List<EmployeeDto> Users { get; set; } = new();

    [JsonPropertyName("total")]
    public int Total { get; set; }
}

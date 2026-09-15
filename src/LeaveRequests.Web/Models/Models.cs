using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HrPortal.Models;

public class LeaveRequestDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ReviewerNote { get; set; }
}

public class CreateLeaveRequestDto
{
    public int EmployeeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Type { get; set; } = string.Empty;
}

public class UpdateStatusDto
{
    public string Status { get; set; } = string.Empty;
    public string? ReviewerNote { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

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

public class CreateLeaveRequestModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Please select an employee.")]
    public int EmployeeId { get; set; }

    [Required(ErrorMessage = "Start date is required.")]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "End date is required.")]
    public DateTime EndDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Please select a leave type.")]
    public string Type { get; set; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;
using LeaveRequests.API.Models;

namespace LeaveRequests.API.DTOs;

public class CreateLeaveRequestDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "EmployeeId must be a positive integer.")]
    public int EmployeeId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    [RegularExpression("^(Vacation|Sick|Unpaid)$", ErrorMessage = "Type must be Vacation, Sick, or Unpaid.")]
    public string Type { get; set; } = string.Empty;
}

public class UpdateStatusDto
{
    [Required]
    [RegularExpression("^(Approved|Rejected)$", ErrorMessage = "Status must be Approved or Rejected.")]
    public string Status { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ReviewerNote { get; set; }
}

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

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

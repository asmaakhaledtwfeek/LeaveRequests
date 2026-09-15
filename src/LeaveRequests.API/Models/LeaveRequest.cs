namespace LeaveRequests.API.Models;

public class LeaveRequest
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = LeaveStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? ReviewerNote { get; set; }
}

public static class LeaveStatus
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";

    public static bool IsValid(string value) =>
        value is Pending or Approved or Rejected;
}

public static class LeaveType
{
    public const string Vacation = "Vacation";
    public const string Sick = "Sick";
    public const string Unpaid = "Unpaid";

    public static bool IsValid(string value) =>
        value is Vacation or Sick or Unpaid;
}

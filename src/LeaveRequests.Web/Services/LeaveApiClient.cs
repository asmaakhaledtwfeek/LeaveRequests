using System.Net.Http.Json;
using System.Text.Json;
using HrPortal.Models;

namespace HrPortal.Services;

public class ApiException : Exception
{
    public ApiException(string message) : base(message) { }
}

public class LeaveApiClient
{
    private readonly HttpClient _http;

    public LeaveApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<PagedResult<LeaveRequestDto>?> GetLeaveRequestsAsync(
        string? status, int? employeeId, int page, int pageSize)
    {
        var url = $"api/leave-requests?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(status)) url += $"&status={Uri.EscapeDataString(status)}";
        if (employeeId.HasValue) url += $"&employeeId={employeeId}";

        var response = await _http.GetAsync(url);
        await EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<PagedResult<LeaveRequestDto>>();
    }

    public async Task<LeaveRequestDto?> CreateLeaveRequestAsync(CreateLeaveRequestDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/leave-requests", dto);
        await EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<LeaveRequestDto>();
    }

    public async Task UpdateStatusAsync(int id, string status, string? note)
    {
        var response = await _http.PutAsJsonAsync($"api/leave-requests/{id}/status", new UpdateStatusDto
        {
            Status = status,
            ReviewerNote = note
        });
        await EnsureSuccess(response);
    }

    public async Task DeleteLeaveRequestAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/leave-requests/{id}");
        await EnsureSuccess(response);
    }

    public async Task<List<EmployeeDto>?> GetEmployeesAsync(string? search = null)
    {
        try
        {
            var url = "api/employees";
            if (!string.IsNullOrEmpty(search)) url += $"?search={Uri.EscapeDataString(search)}";
            var response = await _http.GetAsync(url);
            await EnsureSuccess(response);
            return await response.Content.ReadFromJsonAsync<List<EmployeeDto>>();
        }
        catch (HttpRequestException)
        {
            return new List<EmployeeDto>();
        }
    }

    private static async Task EnsureSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        string? message = null;
        try
        {
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            if (body.TryGetProperty("error", out var errProp))
                message = errProp.GetString();
        }
        catch { }

        throw new ApiException(message ?? $"Request failed ({(int)response.StatusCode}).");
    }
}

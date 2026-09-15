using Microsoft.Extensions.Caching.Memory;

namespace LeaveRequests.API.Services;

public class EmployeeService
{
    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private const string ListCacheKey = "employees_list";

    public EmployeeService(HttpClient http, IMemoryCache cache)
    {
        _http = http;
        _cache = cache;
    }

    public virtual async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        try
        {
            return await _http.GetFromJsonAsync<EmployeeDto>($"users/{id}");
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException)
        {
            return null;
        }
    }

    public async Task<List<EmployeeDto>> GetListAsync(int limit = 20, int skip = 0, string? search = null)
    {
        if (!string.IsNullOrEmpty(search))
        {
            try
            {
                var result = await _http.GetFromJsonAsync<EmployeeListResult>(
                    $"users/search?q={Uri.EscapeDataString(search)}&limit={limit}&skip={skip}"
                );
                return result?.Users ?? new List<EmployeeDto>();
            }
            catch
            {
                return new List<EmployeeDto>();
            }
        }

        if (_cache.TryGetValue(ListCacheKey, out List<EmployeeDto>? cached) && cached != null)
            return cached;

        try
        {
            var result = await _http.GetFromJsonAsync<EmployeeListResult>(
                $"users?limit=100&skip=0"
            );
            var list = result?.Users ?? new List<EmployeeDto>();

            _cache.Set(ListCacheKey, list, TimeSpan.FromMinutes(5));
            return list;
        }
        catch
        {
            return new List<EmployeeDto>();
        }
    }

    public virtual async Task<Dictionary<int, EmployeeDto>> GetByIdsAsync(IEnumerable<int> ids)
    {
        var all = await GetListAsync();
        var idSet = ids.ToHashSet();
        return all
            .Where(e => idSet.Contains(e.Id))
            .ToDictionary(e => e.Id);
    }
}
